using System;
using System.Collections.Generic;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// <see cref="TypeExtesnions.GetTypeName"/> produces the type name shown in the property grid's
    /// Type column, so its output is user-visible.
    /// </summary>
    public class TypeExtensionsTests
    {
        [Fact]
        public void Non_Generic_Type_Uses_The_Short_Name()
        {
            Assert.Equal("Int32", typeof(int).GetTypeName());
            Assert.Equal("Thickness", typeof(Thickness).GetTypeName());
        }

        [Fact]
        public void Nullable_Value_Type_Gets_A_Question_Mark()
        {
            Assert.Equal("Int32?", typeof(int?).GetTypeName());
        }

        [Fact]
        public void Nullable_Is_Preferred_Over_The_Generic_Rendering()
        {
            // Nullable<T> is itself generic, so the ordering of the two branches is what stops this
            // from coming out as "Nullable<Double>".
            Assert.Equal("Double?", typeof(double?).GetTypeName());
        }

        [Fact]
        public void Generic_Type_Loses_Its_Arity_Suffix_And_Lists_Its_Arguments()
        {
            Assert.Equal("List<String>", typeof(List<string>).GetTypeName());
            Assert.Equal("Dictionary<String,Int32>", typeof(Dictionary<string, int>).GetTypeName());
        }

        [Fact]
        public void Generic_Arguments_Are_Formatted_Recursively()
        {
            Assert.Equal(
                "Dictionary<String,List<Int32?>>",
                typeof(Dictionary<string, List<int?>>).GetTypeName());
        }

        [Fact]
        public void Open_Generic_Definition_Keeps_Its_Parameter_Names()
        {
            Assert.Equal("List<T>", typeof(List<>).GetTypeName());
        }

        [Fact]
        public void Repeated_Calls_Return_The_Cached_Instance()
        {
            // The results are cached in a ConditionalWeakTable; a second call must hit it rather
            // than throw on the Add of a duplicate key.
            var first = typeof(Dictionary<string, int>).GetTypeName();
            var second = typeof(Dictionary<string, int>).GetTypeName();

            Assert.Same(first, second);
        }
    }
}
