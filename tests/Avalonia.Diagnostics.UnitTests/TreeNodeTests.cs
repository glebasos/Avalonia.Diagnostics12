using System.Linq;
using Avalonia.Controls;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// The Logical and Visual tree pages are built out of these nodes; their children collections
    /// are lazy and stay subscribed to the underlying tree.
    /// </summary>
    public class TreeNodeTests
    {
        [AvaloniaFact]
        public void Logical_Tree_Mirrors_The_Logical_Children()
        {
            var button = new Button();
            var panel = new Panel { Children = { button } };

            using var node = LogicalTreeNode.Create(panel).Single();

            Assert.Same(panel, node.Visual);
            Assert.Same(button, Assert.Single(node.Children).Visual);
        }

        [AvaloniaFact]
        public void Logical_Tree_Tracks_Children_Added_After_The_Node_Was_Created()
        {
            var panel = new Panel();
            using var node = LogicalTreeNode.Create(panel).Single();

            // Force the lazy collection to initialize (and subscribe) before mutating.
            Assert.Empty(node.Children);

            var button = new Button();
            panel.Children.Add(button);

            Assert.Same(button, Assert.Single(node.Children).Visual);

            panel.Children.Remove(button);

            Assert.Empty(node.Children);
        }

        [AvaloniaFact]
        public void Visual_Tree_Mirrors_The_Visual_Children()
        {
            var button = new Button();
            var border = new Border { Child = button };

            using var node = VisualTreeNode.Create(border).Single();

            Assert.Same(border, node.Visual);
            Assert.Same(button, Assert.Single(node.Children).Visual);
        }

        [AvaloniaFact]
        public void Create_Returns_Nothing_For_A_Non_AvaloniaObject()
        {
            Assert.Empty(LogicalTreeNode.Create("not a control"));
            Assert.Empty(VisualTreeNode.Create("not a control"));
        }

        [AvaloniaFact]
        public void Node_Exposes_The_Type_And_Element_Name()
        {
            using var node = LogicalTreeNode.Create(new Button { Name = "OkButton" }).Single();

            Assert.Equal("Button", node.Type);
            Assert.Equal("OkButton", node.ElementName);
        }

        [AvaloniaFact]
        public void Classes_Are_Rendered_In_Parentheses_And_Kept_Up_To_Date()
        {
            var button = new Button();
            using var node = LogicalTreeNode.Create(button).Single();

            Assert.Equal(string.Empty, node.Classes);

            button.Classes.Add("primary");
            Assert.Equal("(primary)", node.Classes);

            button.Classes.Add("large");
            Assert.Equal("(primary large)", node.Classes);

            button.Classes.Clear();
            Assert.Equal(string.Empty, node.Classes);
        }

        [AvaloniaFact]
        public void A_TopLevel_Node_Is_Rendered_In_Bold()
        {
            var window = new Window();
            using var windowNode = LogicalTreeNode.Create(window).Single();
            using var buttonNode = LogicalTreeNode.Create(new Button()).Single();

            Assert.Equal(FontWeight.Bold, windowNode.FontWeight);
            Assert.Equal(FontWeight.Normal, buttonNode.FontWeight);
        }

        [AvaloniaFact]
        public void Templated_Children_Are_Flagged_As_In_Template()
        {
            var button = new Button { Content = "Hi" };
            var window = TestWindow.Show(button);

            using var node = VisualTreeNode.Create(button).Single();
            var templateChild = Assert.IsType<VisualTreeNode>(node.Children.FirstOrDefault());

            Assert.False(node.IsInTemplate);
            Assert.True(templateChild.IsInTemplate);

            window.Close();
        }

        [AvaloniaFact]
        public void Empty_Collection_Is_Usable()
        {
            // TreeNodeCollection.Empty is handed out for objects that are neither logical nor
            // visual, so it has to behave like a real (empty) collection.
            Assert.Empty(TreeNodeCollection.Empty);
        }
    }
}
