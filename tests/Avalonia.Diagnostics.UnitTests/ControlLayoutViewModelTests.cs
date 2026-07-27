using Avalonia.Controls;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// The Layout Explorer panel. Edits made in it are pushed straight back onto the control, and
    /// changes made by the app have to flow the other way without echoing back.
    /// </summary>
    public class ControlLayoutViewModelTests
    {
        [AvaloniaFact]
        public void Reads_The_Initial_Values_Off_The_Control()
        {
            var border = new Border
            {
                Margin = new Thickness(1),
                Padding = new Thickness(2),
                BorderThickness = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            var vm = new ControlLayoutViewModel(border);

            Assert.Equal(new Thickness(1), vm.MarginThickness);
            Assert.Equal(new Thickness(2), vm.PaddingThickness);
            Assert.Equal(new Thickness(3), vm.BorderThickness);
            Assert.Equal(HorizontalAlignment.Right, vm.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Bottom, vm.VerticalAlignment);
        }

        [AvaloniaFact]
        public void Reading_The_Initial_Values_Does_Not_Write_Them_Back()
        {
            // The constructor sets the view model's own properties, which would otherwise round-trip
            // into SetValue and turn every inherited/styled value into a local one.
            var border = new Border();

            _ = new ControlLayoutViewModel(border);

            Assert.False(border.IsSet(Layoutable.MarginProperty));
            Assert.False(border.IsSet(Decorator.PaddingProperty));
        }

        [AvaloniaFact]
        public void Padding_And_Border_Are_Only_Offered_Where_The_Control_Has_Them()
        {
            var withBoth = new ControlLayoutViewModel(new Border());
            var withNeither = new ControlLayoutViewModel(new global::Avalonia.Controls.Shapes.Rectangle());

            Assert.True(withBoth.HasPadding);
            Assert.True(withBoth.HasBorder);
            Assert.False(withNeither.HasPadding);
            Assert.False(withNeither.HasBorder);
        }

        [AvaloniaFact]
        public void Editing_A_Thickness_Writes_It_To_The_Control()
        {
            var border = new Border();
            var vm = new ControlLayoutViewModel(border);

            vm.MarginThickness = new Thickness(5);
            vm.PaddingThickness = new Thickness(6);
            vm.BorderThickness = new Thickness(7);

            Assert.Equal(new Thickness(5), border.Margin);
            Assert.Equal(new Thickness(6), border.Padding);
            Assert.Equal(new Thickness(7), border.BorderThickness);
        }

        [AvaloniaFact]
        public void Editing_An_Alignment_Writes_It_To_The_Control()
        {
            var border = new Border();
            var vm = new ControlLayoutViewModel(border);

            vm.HorizontalAlignment = HorizontalAlignment.Center;
            vm.VerticalAlignment = VerticalAlignment.Top;

            Assert.Equal(HorizontalAlignment.Center, border.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Top, border.VerticalAlignment);
        }

        [AvaloniaFact]
        public void A_Change_On_The_Control_Is_Pulled_Into_The_View_Model()
        {
            var border = new Border();
            var vm = new ControlLayoutViewModel(border);

            border.PropertyChanged += vm.ControlPropertyChanged;
            border.Margin = new Thickness(8);
            border.BorderThickness = new Thickness(9);

            Assert.Equal(new Thickness(8), vm.MarginThickness);
            Assert.Equal(new Thickness(9), vm.BorderThickness);
        }

        [AvaloniaFact]
        public void Size_Constraints_Are_Rendered_Only_When_Set()
        {
            Assert.Null(new ControlLayoutViewModel(new Border()).WidthConstraint);

            var constrained = new Border { MinWidth = 10, MaxWidth = 100, MaxHeight = 50 };
            var vm = new ControlLayoutViewModel(constrained);

            Assert.Contains("Min: 10", vm.WidthConstraint);
            Assert.Contains("Max: 100", vm.WidthConstraint);
            Assert.DoesNotContain("Min:", vm.HeightConstraint);
            Assert.Contains("Max: 50", vm.HeightConstraint);
        }

        [AvaloniaFact]
        public void Size_Constraints_Are_Recomputed_When_They_Change()
        {
            var border = new Border();
            var vm = new ControlLayoutViewModel(border);
            border.PropertyChanged += vm.ControlPropertyChanged;

            border.MinWidth = 42;

            Assert.Contains("Min: 42", vm.WidthConstraint);
        }

        [AvaloniaFact]
        public void Size_Follows_The_Controls_Bounds()
        {
            var border = new Border { Width = 123.456, Height = 20 };
            var window = TestWindow.Show(border);

            var vm = new ControlLayoutViewModel(border);

            // Layout rounding decides the actual bounds; what this pins is that the panel reports
            // the laid-out size, rounded to two decimals rather than echoing the requested Width.
            Assert.Equal(System.Math.Round(border.Bounds.Width, 2), vm.Width);
            Assert.Equal(20, vm.Height);
            Assert.True(vm.Width > 0);

            window.Close();
        }
    }
}
