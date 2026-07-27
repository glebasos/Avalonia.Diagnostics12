using Avalonia.Controls;
using Avalonia.Diagnostics.Controls;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Diagnostics.Views;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// The editor control the details grid puts in the Value column. Which control it picks, and
    /// whether it is writable, is decided from the property's type.
    /// </summary>
    public class PropertyValueEditorViewTests
    {
        [AvaloniaFact]
        public void A_Bool_Property_Gets_A_CheckBox()
        {
            Assert.IsType<CheckBox>(Editor(new Border(), Visual.IsVisibleProperty).Content);
        }

        [AvaloniaFact]
        public void An_Integer_Property_Gets_A_NumericUpDown()
        {
            Assert.IsType<NumericUpDown>(Editor(new Border(), Grid.RowProperty).Content);
        }

        /// <summary>
        /// Characterization test. <c>IsValidNumeric</c> accepts the integer type codes plus Single,
        /// but not Double or Decimal - so Width, Height, Opacity and friends fall through to the
        /// text box rather than getting a spinner. Inherited from upstream, not a port regression.
        /// </summary>
        [AvaloniaFact]
        public void A_Double_Property_Falls_Through_To_A_Text_Box()
        {
            Assert.IsType<CommitTextBox>(Editor(new Border(), Layoutable.WidthProperty).Content);
        }

        [AvaloniaFact]
        public void An_Enum_Property_Gets_A_ComboBox()
        {
            var combo = Assert.IsType<ComboBox>(
                Editor(new Border(), Layoutable.HorizontalAlignmentProperty).Content);

            Assert.NotNull(combo.ItemsSource);
        }

        [AvaloniaFact]
        public void A_Brush_Property_Gets_A_BrushEditor()
        {
            Assert.IsType<BrushEditor>(Editor(new Border(), Border.BackgroundProperty).Content);
        }

        [AvaloniaFact]
        public void Anything_Else_Gets_A_CommitTextBox()
        {
            Assert.IsType<CommitTextBox>(Editor(new Border(), Layoutable.MarginProperty).Content);
        }

        [AvaloniaFact]
        public void A_Read_Only_Property_Produces_A_Read_Only_Editor()
        {
            var textBox = Assert.IsType<CommitTextBox>(
                Editor(new Border(), Visual.BoundsProperty).Content);

            Assert.True(textBox.IsReadOnly);
        }

        [AvaloniaFact]
        public void A_Command_Property_Is_Read_Only()
        {
            // The ICommand rule in StringConversionHelper, seen from the control that consumes it.
            var textBox = Assert.IsType<CommitTextBox>(
                Editor(new Button(), Button.CommandProperty).Content);

            Assert.True(textBox.IsReadOnly);
        }

        [AvaloniaFact]
        public void Typing_An_Unparseable_Value_Flags_The_Row()
        {
            var textBox = Assert.IsType<CommitTextBox>(
                Editor(new Border(), Layoutable.MarginProperty).Content);

            textBox.Text = "not a thickness";

            Assert.True(DataValidationErrors.GetHasErrors(textBox));
        }

        [AvaloniaFact]
        public void Typing_A_Parseable_Value_Clears_The_Flag()
        {
            var textBox = Assert.IsType<CommitTextBox>(
                Editor(new Border(), Layoutable.MarginProperty).Content);

            textBox.Text = "not a thickness";
            textBox.Text = "1,2,3,4";

            Assert.False(DataValidationErrors.GetHasErrors(textBox));
        }

        /// <summary>
        /// Regression cover for the CommittedText guard. The row's text comes from
        /// <c>ToString(value)</c>, and plenty of types do not render as something their own converter
        /// can parse back - <see cref="TransformOperations"/> has no ToString override, so the row
        /// displays "Avalonia.Media.Transformation.TransformOperations". Validating that would paint
        /// a pristine, never-touched row red. Text still equal to CommittedText is by definition not
        /// a user edit.
        /// </summary>
        [AvaloniaFact]
        public void A_Value_That_Does_Not_Round_Trip_Does_Not_Flag_An_Untouched_Row()
        {
            var border = new Border
            {
                RenderTransform = TransformOperations.Parse("translate(10px, 10px)")
            };

            var textBox = Assert.IsType<CommitTextBox>(
                Editor(border, Visual.RenderTransformProperty).Content);

            Assert.Equal(textBox.CommittedText, textBox.Text);
            Assert.False(DataValidationErrors.GetHasErrors(textBox));
        }

        [AvaloniaFact]
        public void Restoring_The_Committed_Text_Clears_An_Error()
        {
            var textBox = Assert.IsType<CommitTextBox>(
                Editor(new Border { Margin = new Thickness(1) }, Layoutable.MarginProperty).Content);
            var committed = textBox.CommittedText;

            textBox.Text = "not a thickness";
            Assert.True(DataValidationErrors.GetHasErrors(textBox));

            textBox.Text = committed;

            Assert.False(DataValidationErrors.GetHasErrors(textBox));
        }

        private static PropertyValueEditorView Editor(AvaloniaObject target, AvaloniaProperty property) =>
            new() { DataContext = new AvaloniaPropertyViewModel(target, property) };
    }
}
