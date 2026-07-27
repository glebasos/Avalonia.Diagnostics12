using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;
using Scope = Avalonia.Diagnostics.UnitTests.DevToolsScope;

namespace Avalonia.Diagnostics.UnitTests
{
    public class TreePageViewModelTests
    {
        [AvaloniaFact]
        public void FindNode_Locates_A_Descendant()
        {
            var button = new Button();
            using var scope = Scope.Create(new Panel { Children = { button } });
            using var page = CreatePage(scope);

            Assert.Same(button, page.FindNode(button)?.Visual);
        }

        [AvaloniaFact]
        public void FindNode_Returns_Null_For_A_Control_Outside_The_Tree()
        {
            using var scope = Scope.Create(new Button());
            using var page = CreatePage(scope);

            Assert.Null(page.FindNode(new Button()));
        }

        [AvaloniaFact]
        public void SelectControl_Selects_The_Matching_Node()
        {
            var button = new Button();
            using var scope = Scope.Create(new Panel { Children = { button } });
            using var page = CreatePage(scope);

            page.SelectControl(button);

            Assert.Same(button, page.SelectedNode?.Visual);
        }

        [AvaloniaFact]
        public void SelectControl_Walks_Up_To_The_Nearest_Control_That_Has_A_Node()
        {
            // Template children are not in the logical tree, so selecting one has to fall back to
            // its nearest visual ancestor that is - otherwise clicking a templated part in the app
            // would select nothing.
            var button = new Button { Content = "Hi" };
            using var scope = Scope.Create(button);
            using var page = CreatePage(scope);

            var templateChild = button.GetVisualChildren().OfType<Control>().First();
            Assert.Null(page.FindNode(templateChild));

            page.SelectControl(templateChild);

            Assert.Same(button, page.SelectedNode?.Visual);
        }

        [AvaloniaFact]
        public void SelectControl_Expands_The_Ancestor_Nodes()
        {
            var button = new Button();
            var panel = new Panel { Children = { button } };
            using var scope = Scope.Create(panel);
            using var page = CreatePage(scope);

            page.SelectControl(button);

            var root = page.Nodes.Single();
            Assert.True(root.IsExpanded);
            Assert.True(page.FindNode(panel)!.IsExpanded);
        }

        [AvaloniaFact]
        public void Selecting_A_Node_Creates_Its_Details_And_Clearing_It_Removes_Them()
        {
            var button = new Button();
            using var scope = Scope.Create(button);
            using var page = CreatePage(scope);

            page.SelectControl(button);
            Assert.NotNull(page.Details);
            Assert.NotNull(page.Details!.PropertiesView);

            page.SelectedNode = null;
            Assert.Null(page.Details);
        }

        [AvaloniaFact]
        public void The_Properties_Filter_Refreshes_The_Details_View()
        {
            var button = new Button();
            using var scope = Scope.Create(button);
            using var page = CreatePage(scope);
            page.SelectControl(button);

            page.PropertiesFilter.FilterString = "Background";

            var names = page.Details!.PropertiesView!.Cast<PropertyViewModel>().Select(p => p.Name);
            Assert.NotEmpty(names);
            Assert.All(names, n => Assert.Contains("background", n.ToLowerInvariant()));
        }

        [AvaloniaFact]
        public void ExpandRecursively_And_CollapseChildren_Walk_The_Whole_Subtree()
        {
            var button = new Button();
            var panel = new Panel { Children = { button } };
            using var scope = Scope.Create(panel);
            using var page = CreatePage(scope);

            page.SelectControl(panel);
            page.ExpandRecursively();
            Assert.True(page.FindNode(button)!.IsExpanded);

            page.CollapseChildren();
            Assert.False(page.FindNode(button)!.IsExpanded);
            Assert.False(page.FindNode(panel)!.IsExpanded);
        }

        [AvaloniaFact]
        public void CopySelector_Emits_A_Selector_For_The_Selected_Control()
        {
            var button = new Button { Name = "Ok" };
            button.Classes.Add("primary");
            using var scope = Scope.Create(button);
            using var page = CreatePage(scope);
            page.SelectControl(button);

            string? selector = null;
            page.ClipboardCopyRequested += (_, s) => selector = s;
            page.CopySelector();

            Assert.NotNull(selector);
            Assert.Contains("|Button", selector);
            Assert.Contains("#Ok", selector);
            Assert.Contains(".primary", selector);
        }

        [AvaloniaFact]
        public void CopySelector_Does_Nothing_Without_A_Selection()
        {
            using var scope = Scope.Create(new Button());
            using var page = CreatePage(scope);

            var raised = false;
            page.ClipboardCopyRequested += (_, _) => raised = true;
            page.CopySelector();

            Assert.False(raised);
        }

        [AvaloniaFact]
        public void CopySelectorFromTemplateParent_Chains_Through_The_Template_Parents()
        {
            var button = new Button { Content = "Hi" };
            using var scope = Scope.Create(button);
            using var page = CreatePage(scope);

            var templateChild = button.GetVisualChildren().OfType<Control>().First();
            page.SelectedNode = new VisualTreeNode(templateChild, null);

            string? selector = null;
            page.ClipboardCopyRequested += (_, s) => selector = s;
            page.CopySelectorFromTemplateParent();

            Assert.NotNull(selector);
            Assert.Contains(" /template/ ", selector);
            Assert.StartsWith("{", selector);
        }

        private static TreePageViewModel CreatePage(Scope scope) =>
            new(scope.Model, LogicalTreeNode.Create(scope.Window), new HashSet<string>());
    }
}
