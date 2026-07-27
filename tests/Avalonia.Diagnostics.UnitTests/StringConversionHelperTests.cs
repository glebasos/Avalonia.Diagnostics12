using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia.Media;
using Xunit;
using Helper = Avalonia.Diagnostics.Views.PropertyValueEditorView.StringConversionHelper;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// Covers the rule that decides whether a property row in the details grid is an editable text
    /// box or a read-only one.
    /// </summary>
    public class StringConversionHelperTests
    {
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(Thickness))]
        [InlineData(typeof(CornerRadius))]
        public void Types_With_A_String_Converter_Are_Editable(Type type)
        {
            Assert.True(Helper.CanConvertFromString(type));
        }

        [Fact]
        public void Types_With_Only_A_Parse_Method_Are_Editable()
        {
            // No TypeConverter, but a public static Parse(string, IFormatProvider) - the helper's
            // reflection fallback has to find it.
            Assert.True(Helper.CanConvertFromString(typeof(DateTime)));
        }

        [Fact]
        public void Object_Has_No_Meaningful_Conversion()
        {
            Assert.False(Helper.CanConvertFromString(typeof(object)));
        }

        [Fact]
        public void ICommand_Is_Never_Editable()
        {
            Assert.False(Helper.CanConvertFromString(typeof(ICommand)));
        }

        /// <summary>
        /// The real-world bug: <c>System.Windows.Input.ICommand</c> carries a
        /// <c>[TypeConverter]</c> pointing at WPF's <c>CommandConverter</c>, which reports
        /// <c>CanConvertFrom(string) == true</c> but throws for anything that is not a WPF
        /// RoutedCommand name - so the Command row became a writable text box that immediately
        /// flagged its own contents as invalid. The attribute only resolves when
        /// PresentationFramework happens to be loaded, which a plain test host does not do, so this
        /// test stands in a converter with the same shape: a command type whose TypeConverter
        /// *claims* it can be built from a string. The assertion is that the ICommand check wins
        /// before TypeDescriptor is ever consulted.
        /// </summary>
        [Fact]
        public void A_Command_Wins_Over_A_TypeConverter_That_Claims_String_Support()
        {
            // Guard: without the ICommand rule TypeDescriptor would answer true here.
            Assert.True(TypeDescriptor.GetConverter(typeof(ConvertibleCommand)).CanConvertFrom(typeof(string)));

            Assert.False(Helper.CanConvertFromString(typeof(ConvertibleCommand)));
        }

        [Fact]
        public void Types_Deriving_From_A_Command_Are_Also_Read_Only()
        {
            Assert.False(Helper.CanConvertFromString(typeof(DerivedCommand)));
        }

        [Fact]
        public void ToString_Uses_The_Type_Converter_With_The_Invariant_Culture()
        {
            Assert.Equal("1,2,3,4", Helper.ToString(new Thickness(1, 2, 3, 4)));
        }

        [Fact]
        public void ToString_Falls_Back_To_Object_ToString_For_Collections()
        {
            // CollectionConverter only ever renders "(Collection)", which tells the user nothing.
            var value = new System.Collections.Generic.List<int> { 1, 2, 3 };

            Assert.Equal(value.ToString(), Helper.ToString(value));
        }

        [Fact]
        public void FromString_Round_Trips_Through_The_Type_Converter()
        {
            Assert.Equal(new Thickness(1, 2, 3, 4), Helper.FromString("1,2,3,4", typeof(Thickness)));
        }

        [Fact]
        public void FromString_Uses_The_Parse_Fallback_When_There_Is_No_Converter()
        {
            var expected = new DateTime(2024, 5, 6);

            Assert.Equal(expected, Helper.FromString("2024-05-06", typeof(DateTime)));
        }

        [Fact]
        public void FromString_Throws_On_Unparseable_Input()
        {
            // The editor relies on this throwing to paint the row's validation error.
            Assert.ThrowsAny<Exception>(() => Helper.FromString("not a thickness", typeof(Thickness)));
        }

        [TypeConverter(typeof(FakeCommandConverter))]
        private class ConvertibleCommand : ICommand
        {
#pragma warning disable CS0067 // never raised; the type only exists to be inspected
            public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) { }
        }

        private sealed class DerivedCommand : ConvertibleCommand;

        /// <summary>Stands in for WPF's CommandConverter: claims strings, delivers nothing.</summary>
        private sealed class FakeCommandConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
                sourceType == typeof(string);

            public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
                throw new NotSupportedException();
        }
    }
}
