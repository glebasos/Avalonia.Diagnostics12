using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Diagnostics.ViewModels;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// <see cref="FilterViewModel"/> backs the filter boxes above the property grid and the setter
    /// list. It is plain logic - no UI thread required.
    /// </summary>
    public class FilterViewModelTests
    {
        [Fact]
        public void Empty_Filter_Matches_Everything()
        {
            var vm = new FilterViewModel();

            Assert.True(vm.Filter("Background"));
            Assert.True(vm.Filter(string.Empty));
        }

        [Fact]
        public void Plain_Filter_Is_A_Case_Insensitive_Substring_Match()
        {
            var vm = new FilterViewModel { FilterString = "back" };

            Assert.True(vm.Filter("Background"));
            Assert.True(vm.Filter("BACKGROUND"));
            Assert.False(vm.Filter("Foreground"));
        }

        [Fact]
        public void Filter_String_Is_Trimmed()
        {
            var vm = new FilterViewModel { FilterString = "  Background  " };

            Assert.True(vm.Filter("Background"));
        }

        [Fact]
        public void Regex_Metacharacters_Are_Escaped_When_Regex_Mode_Is_Off()
        {
            // "." must match a literal dot, not any character - otherwise filtering for an
            // attached property like "Grid.Row" would match unrelated names.
            var vm = new FilterViewModel { FilterString = "Grid.Row" };

            Assert.True(vm.Filter("[Grid.Row]"));
            Assert.False(vm.Filter("GridXRow"));
        }

        [Fact]
        public void Regex_Mode_Treats_The_Filter_As_A_Pattern()
        {
            var vm = new FilterViewModel { UseRegexFilter = true, FilterString = "^(Fore|Back)ground$" };

            Assert.True(vm.Filter("Background"));
            Assert.True(vm.Filter("Foreground"));
            Assert.False(vm.Filter("BackgroundSizing"));
        }

        [Fact]
        public void Case_Sensitive_Mode_Stops_Matching_On_Case()
        {
            var vm = new FilterViewModel { UseCaseSensitiveFilter = true, FilterString = "back" };

            Assert.False(vm.Filter("Background"));
            Assert.True(vm.Filter("outback"));
        }

        [Fact]
        public void Whole_Word_Mode_Requires_Word_Boundaries()
        {
            var vm = new FilterViewModel { UseWholeWordFilter = true, FilterString = "Row" };

            Assert.True(vm.Filter("[Grid.Row]"));
            Assert.False(vm.Filter("RowDefinitions"));
        }

        [Fact]
        public void Whole_Word_And_Regex_Modes_Compose()
        {
            // The whole-word wrapper has to group the pattern, or the alternation would bind to the
            // boundaries instead: \bFore|Background\b rather than \b(?:Fore|Background)\b.
            var vm = new FilterViewModel
            {
                UseRegexFilter = true,
                UseWholeWordFilter = true,
                FilterString = "Fore|Background"
            };

            Assert.True(vm.Filter("Background Sizing"));
            Assert.False(vm.Filter("Foreground"));
        }

        [Fact]
        public void Toggling_An_Option_Reapplies_The_Existing_Filter_String()
        {
            var vm = new FilterViewModel { FilterString = "Grid.Row" };
            Assert.False(vm.Filter("GridXRow"));

            vm.UseRegexFilter = true;

            Assert.True(vm.Filter("GridXRow"));
        }

        [Fact]
        public void Changing_The_Filter_Raises_RefreshFilter()
        {
            var vm = new FilterViewModel();
            var raised = 0;
            vm.RefreshFilter += (_, _) => raised++;

            vm.FilterString = "a";
            vm.UseRegexFilter = true;
            vm.UseCaseSensitiveFilter = true;
            vm.UseWholeWordFilter = true;

            Assert.Equal(4, raised);
        }

        [Fact]
        public void Setting_The_Same_Value_Does_Not_Raise_RefreshFilter()
        {
            var vm = new FilterViewModel { FilterString = "a" };
            var raised = 0;
            vm.RefreshFilter += (_, _) => raised++;

            vm.FilterString = "a";

            Assert.Equal(0, raised);
        }

        [Fact]
        public void An_Invalid_Regex_Is_Reported_Through_INotifyDataErrorInfo()
        {
            var vm = new FilterViewModel { UseRegexFilter = true };
            var errorsChangedFor = new List<string?>();
            vm.ErrorsChanged += (_, e) => errorsChangedFor.Add(e.PropertyName);

            vm.FilterString = "(unclosed";

            Assert.True(vm.HasErrors);
            Assert.Equal(new[] { nameof(FilterViewModel.FilterString) }, errorsChangedFor);
            Assert.NotEmpty(vm.GetErrors(nameof(FilterViewModel.FilterString)).Cast<object>());
        }

        [Fact]
        public void An_Invalid_Regex_Leaves_The_Previous_Filter_In_Place()
        {
            var vm = new FilterViewModel { UseRegexFilter = true, FilterString = "Background" };

            vm.FilterString = "(unclosed";

            // The compiled regex is only replaced on success, so the last good pattern keeps
            // filtering rather than the grid flipping to "match everything".
            Assert.True(vm.Filter("Background"));
            Assert.False(vm.Filter("Foreground"));
        }

        [Fact]
        public void Fixing_An_Invalid_Regex_Clears_The_Error()
        {
            var vm = new FilterViewModel { UseRegexFilter = true, FilterString = "(unclosed" };
            Assert.True(vm.HasErrors);

            vm.FilterString = "(closed)";

            Assert.False(vm.HasErrors);
            Assert.Empty(vm.GetErrors(nameof(FilterViewModel.FilterString)).Cast<object>());
        }

        [Fact]
        public void GetErrors_Returns_Nothing_For_Other_Properties()
        {
            var vm = new FilterViewModel { UseRegexFilter = true, FilterString = "(unclosed" };

            Assert.Empty(vm.GetErrors("SomethingElse").Cast<object>());
            Assert.Empty(vm.GetErrors(null).Cast<object>());
        }

        [Fact]
        public void Property_Changes_Are_Notified()
        {
            var vm = new FilterViewModel();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.FilterString = "a";
            vm.UseRegexFilter = true;

            Assert.Equal(
                new[] { nameof(FilterViewModel.FilterString), nameof(FilterViewModel.UseRegexFilter) },
                changed);
        }
    }
}
