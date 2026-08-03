using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Layout;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;
using Scope = Avalonia.Diagnostics.UnitTests.DevToolsScope;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// The details pane: the property grid, the applied value frames and the pseudo-class list.
    /// </summary>
    public class ControlDetailsViewModelTests
    {
        [AvaloniaFact]
        public void Lists_Both_Avalonia_And_Clr_Properties()
        {
            using var fixture = Fixture.Create(new Button());

            var groups = fixture.Details.PropertiesView!
                .Cast<PropertyViewModel>()
                .Select(p => p.Group)
                .Distinct()
                .ToList();

            Assert.Contains("Properties", groups);
            Assert.Contains("CLR Properties", groups);
            Assert.Contains(fixture.Properties, p => p is AvaloniaPropertyViewModel);
            Assert.Contains(fixture.Properties, p => p is ClrPropertyViewModel);
        }

        [AvaloniaFact]
        public void Attached_Properties_Are_Named_And_Grouped_Separately()
        {
            var button = new Button();
            Grid.SetRow(button, 3);

            using var fixture = Fixture.Create(button);

            var row = Assert.Single(
                fixture.Properties.OfType<AvaloniaPropertyViewModel>(),
                p => p.Name == "[Grid.Row]");

            Assert.Equal(3, row.Value);
            Assert.Equal("Attached Properties", row.Group);
            Assert.True(row.IsAttached);
        }

        [AvaloniaFact]
        public void An_Unset_Property_Is_Reported_With_Unset_Priority()
        {
            using var fixture = Fixture.Create(new Border());

            var background = fixture.Avalonia(nameof(Border.Background));

            Assert.Equal("Unset", background.Priority);
            Assert.Null(background.Value);
            // Note the row still lands in the ordinary "Properties" group: the view model's "Unset"
            // group is only reached when reading the diagnostic throws.
            Assert.Equal("Properties", background.Group);
        }

        [AvaloniaFact]
        public void A_Locally_Set_Property_Reports_Its_Value_And_Priority()
        {
            using var fixture = Fixture.Create(new Border { Background = Brushes.Red });

            var background = fixture.Avalonia(nameof(Border.Background));

            Assert.Same(Brushes.Red, background.Value);
            Assert.Equal("LocalValue", background.Priority);
            Assert.Equal("Properties", background.Group);
        }

        [AvaloniaFact]
        public void Editing_A_Property_Value_Writes_Back_To_The_Control()
        {
            var border = new Border();
            using var fixture = Fixture.Create(border);

            fixture.Avalonia(nameof(Border.Background)).Value = Brushes.Blue;

            Assert.Same(Brushes.Blue, border.Background);
        }

        [AvaloniaFact]
        public void A_Property_Change_On_The_Control_Updates_The_Row()
        {
            var border = new Border();
            using var fixture = Fixture.Create(border);
            var background = fixture.Avalonia(nameof(Border.Background));

            border.Background = Brushes.Green;

            Assert.Same(Brushes.Green, background.Value);
        }

        [AvaloniaFact]
        public void Read_Only_Properties_Are_Flagged()
        {
            using var fixture = Fixture.Create(new Border());

            Assert.True(fixture.Avalonia(nameof(Visual.Bounds)).IsReadonly);
            Assert.False(fixture.Avalonia(nameof(Border.Background)).IsReadonly);
        }

        [AvaloniaFact]
        public void Pinning_A_Property_Moves_It_To_The_Pinned_Group_And_Is_Remembered()
        {
            var pinned = new HashSet<string>();
            using var fixture = Fixture.Create(new Border(), pinned);
            var background = fixture.Avalonia(nameof(Border.Background));

            fixture.Details.TogglePinnedProperty(background);

            Assert.True(background.IsPinned);
            Assert.Equal("Pinned", background.Group);
            Assert.Contains(background.FullName, pinned);

            fixture.Details.TogglePinnedProperty(background);

            Assert.False(background.IsPinned);
            Assert.NotEqual("Pinned", background.Group);
            Assert.Empty(pinned);
        }

        [AvaloniaFact]
        public void A_Pin_Survives_Reselecting_The_Same_Control()
        {
            var pinned = new HashSet<string>();
            var border = new Border();
            using var fixture = Fixture.Create(border, pinned);
            fixture.Details.TogglePinnedProperty(fixture.Avalonia(nameof(Border.Background)));

            fixture.Details.UpdatePropertiesView(showImplementedInterfaces: true);

            Assert.True(fixture.Avalonia(nameof(Border.Background)).IsPinned);
        }

        [AvaloniaFact]
        public void Interface_Properties_Are_Only_Listed_When_Asked_For()
        {
            using var fixture = Fixture.Create(new Border());

            fixture.Details.UpdatePropertiesView(showImplementedInterfaces: false);
            var without = fixture.Properties.Count(p => p.Name.Contains('.') && p is ClrPropertyViewModel);

            fixture.Details.UpdatePropertiesView(showImplementedInterfaces: true);
            var with = fixture.Properties.Count(p => p.Name.Contains('.') && p is ClrPropertyViewModel);

            Assert.True(with > without);
        }

        [AvaloniaFact]
        public void SelectProperty_Selects_The_Matching_Row()
        {
            using var fixture = Fixture.Create(new Border());

            fixture.Details.SelectProperty(Border.BackgroundProperty);

            var selected = Assert.IsType<AvaloniaPropertyViewModel>(fixture.Details.SelectedProperty);
            Assert.Equal(Border.BackgroundProperty, selected.Property);
        }

        [AvaloniaFact]
        public void Navigating_Into_A_Property_Pushes_And_Pops_The_Entity_Stack()
        {
            var border = new Border { Background = Brushes.Red };
            using var fixture = Fixture.Create(border);

            Assert.False(fixture.Details.CanNavigateToParentProperty);

            fixture.Details.SelectProperty(Border.BackgroundProperty);
            fixture.Details.NavigateToSelectedProperty();

            Assert.Same(Brushes.Red, fixture.Details.SelectedEntity);
            Assert.True(fixture.Details.CanNavigateToParentProperty);

            fixture.Details.NavigateToParentProperty();

            Assert.Same(border, fixture.Details.SelectedEntity);
            Assert.False(fixture.Details.CanNavigateToParentProperty);
        }

        [AvaloniaFact]
        public void Value_Types_And_Strings_Are_Not_Navigable()
        {
            var border = new Border { Margin = new Thickness(4) };
            using var fixture = Fixture.Create(border);

            fixture.Details.SelectProperty(Layoutable.MarginProperty);
            fixture.Details.NavigateToSelectedProperty();

            Assert.Same(border, fixture.Details.SelectedEntity);
            Assert.False(fixture.Details.CanNavigateToParentProperty);
        }

        [AvaloniaFact]
        public void Pseudo_Classes_Declared_By_The_Control_Are_Listed_And_Tracked()
        {
            var button = new Button();
            using var fixture = Fixture.Create(button);

            var pointerOver = Assert.Single(fixture.Details.PseudoClasses, p => p.Name == ":pointerover");
            Assert.False(pointerOver.IsActive);

            ((IPseudoClasses)button.Classes).Add(":pointerover");
            pointerOver.Update();

            Assert.True(pointerOver.IsActive);
        }

        [AvaloniaFact]
        public void Applied_Frames_Are_Reported_With_A_Status_Line()
        {
            var button = new Button { Content = "Hi" };
            using var fixture = Fixture.Create(button, showInWindow: true);

            Assert.NotEmpty(fixture.Details.AppliedFrames);
            Assert.StartsWith("Value Frames (", fixture.Details.FramesStatus);
        }

        [AvaloniaFact]
        public void The_Setters_Filter_Hides_Non_Matching_Frames()
        {
            var button = new Button { Content = "Hi" };
            using var fixture = Fixture.Create(button, showInWindow: true);

            fixture.Page.SettersFilter.FilterString = "ThisMatchesNothing";
            fixture.Details.UpdateStyleFilters();

            Assert.NotEmpty(fixture.Details.AppliedFrames);
            Assert.All(fixture.Details.AppliedFrames, f => Assert.False(f.IsVisible));

            fixture.Page.SettersFilter.FilterString = string.Empty;
            fixture.Details.UpdateStyleFilters();

            Assert.Contains(fixture.Details.AppliedFrames, f => f.IsVisible);
        }

        [AvaloniaFact]
        public void Layout_Is_Only_Created_For_Visuals()
        {
            using var visual = Fixture.Create(new Border());
            Assert.NotNull(visual.Details.Layout);
        }

        [AvaloniaFact]
        public void Refreshing_Picks_Up_Clr_Properties_That_Never_Notify()
        {
            // IsEffectivelyVisible is a plain CLR property whose change event is internal, so nothing
            // pushes it into the grid: it stays stale until the pane is refreshed by hand.
            var border = new Border();
            var parent = new StackPanel { IsVisible = false, Children = { border } };
            using var fixture = Fixture.Create(border, showInWindow: false, host: parent);

            var effectivelyVisible = fixture.Clr(nameof(Visual.IsEffectivelyVisible));
            Assert.Equal(false, effectivelyVisible.Value);

            parent.IsVisible = true;
            TestWindow.RunLayout();

            Assert.Equal(false, effectivelyVisible.Value);

            fixture.Details.RefreshProperties();

            Assert.Equal(true, effectivelyVisible.Value);
        }

        [AvaloniaFact]
        public void Refreshing_Works_For_A_Non_Visual_Target()
        {
            // Inlines are StyledElements but not Visuals, so they reach the details pane through the
            // logical tree and have no Layout. Refreshing one must still re-read it rather than
            // trip over the missing visual.
            var run = new Run("hi");
            var text = new TextBlock();
            text.Inlines!.Add(run);

            using var fixture = Fixture.Create(run);
            Assert.Null(fixture.Details.Layout);

            var foreground = fixture.Avalonia("[TextElement.Foreground]");
            Assert.Equal(Brushes.Black, foreground.Value);

            run.Foreground = Brushes.Red;
            fixture.Details.RefreshProperties();

            Assert.Same(Brushes.Red, foreground.Value);
        }

        private sealed class Fixture : System.IDisposable
        {
            private readonly Scope _scope;

            private Fixture(Scope scope, TreePageViewModel page, ControlDetailsViewModel details)
            {
                _scope = scope;
                Page = page;
                Details = details;
            }

            public TreePageViewModel Page { get; }

            public ControlDetailsViewModel Details { get; }

            public IReadOnlyList<PropertyViewModel> Properties =>
                Details.PropertiesView!.SourceCollection.Cast<PropertyViewModel>().ToList();

            public AvaloniaPropertyViewModel Avalonia(string name) =>
                Properties.OfType<AvaloniaPropertyViewModel>().Single(p => p.Name == name);

            public ClrPropertyViewModel Clr(string name) =>
                Properties.OfType<ClrPropertyViewModel>().First(p => p.Name == name);

            /// <param name="host">
            /// Shown in the window instead of <paramref name="target"/>, for targets that need a
            /// parent (or that are not <see cref="Control"/>s at all, such as inlines).
            /// </param>
            public static Fixture Create(
                AvaloniaObject target,
                ISet<string>? pinned = null,
                bool showInWindow = false,
                Control? host = null)
            {
                var scope = host is not null ? Scope.Create(host)
                    : showInWindow ? Scope.Create(target as Control)
                    : Scope.Create();

                pinned ??= new HashSet<string>();
                var page = new TreePageViewModel(scope.Model, LogicalTreeNode.Create(target), pinned);
                var details = new ControlDetailsViewModel(page, target, pinned);
                details.UpdatePropertiesView(showImplementedInterfaces: true);
                details.UpdateStyleFilters();

                return new Fixture(scope, page, details);
            }

            public void Dispose()
            {
                Details.Dispose();
                Page.Dispose();
                _scope.Dispose();
            }
        }
    }
}
