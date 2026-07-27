using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    public class VisualTreeDebugTests
    {
        [AvaloniaFact]
        public void Prints_The_Root_Type_And_Its_Descendants()
        {
            var child = new Button();
            var root = new Panel { Children = { new Border { Child = child } } };

            var text = VisualTreeDebug.PrintVisualTree(root);

            Assert.Contains("Panel", text);
            Assert.Contains("+- Border", text);
            Assert.Contains("+- Button", text);
        }

        [AvaloniaFact]
        public void Indents_By_Depth()
        {
            var root = new Panel { Children = { new Border { Child = new Button() } } };

            var lines = VisualTreeDebug.PrintVisualTree(root).Split('\n');

            var border = System.Array.Find(lines, l => l.Contains("+- Border"))!;
            var button = System.Array.Find(lines, l => l.Contains("+- Button"))!;

            // Four spaces per level, plus the single space in front of the " +- " marker.
            Assert.Equal(1, border.IndexOf('+'));
            Assert.Equal(5, button.IndexOf('+'));
        }

        /// <summary>
        /// Characterization test, not an endorsement. The header line appends
        /// <c>control.Classes.ToString()</c>, and <c>Classes</c> (an AvaloniaList) does not override
        /// ToString - so the dump prints the collection's type name and the actual style classes
        /// never appear. Inherited from upstream. If that is ever fixed, this test is the one that
        /// will notice.
        /// </summary>
        [AvaloniaFact]
        public void Style_Classes_Are_Not_Actually_Printed()
        {
            var root = new Border();
            root.Classes.Add("highlighted");

            var text = VisualTreeDebug.PrintVisualTree(root);

            Assert.Contains("Avalonia.Controls.Classes", text);
            Assert.DoesNotContain("highlighted", text);
        }

        [AvaloniaFact]
        public void Prints_Set_Properties_With_Their_Binding_Priority()
        {
            var root = new Border { Background = Brushes.Red };

            var text = VisualTreeDebug.PrintVisualTree(root);

            Assert.Contains("Background = ", text);
            Assert.Contains("[LocalValue]", text);
        }

        [AvaloniaFact]
        public void Does_Not_Print_Unset_Properties()
        {
            // The whole point of the dump is to show what was actually set; every registered
            // property would drown that out.
            var text = VisualTreeDebug.PrintVisualTree(new Border());

            Assert.DoesNotContain("Background = ", text);
        }
    }
}
