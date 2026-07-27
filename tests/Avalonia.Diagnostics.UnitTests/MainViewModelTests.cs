using Avalonia.Controls;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;
using Scope = Avalonia.Diagnostics.UnitTests.DevToolsScope;

namespace Avalonia.Diagnostics.UnitTests
{
    public class MainViewModelTests
    {
        [AvaloniaFact]
        public void Starts_On_The_Logical_Tree()
        {
            using var scope = Scope.Create();

            Assert.Equal(0, scope.Model.SelectedTab);
            var tree = Assert.IsType<TreePageViewModel>(scope.Model.Content);
            Assert.Same(scope.Window, Assert.Single(tree.Nodes).Visual);
        }

        [AvaloniaFact]
        public void SelectedTab_Switches_The_Content_Page()
        {
            using var scope = Scope.Create();
            var logical = scope.Model.Content;

            scope.Model.SelectedTab = 1;
            var visual = Assert.IsType<TreePageViewModel>(scope.Model.Content);
            Assert.NotSame(logical, visual);

            scope.Model.SelectedTab = 2;
            Assert.IsType<EventsPageViewModel>(scope.Model.Content);

            scope.Model.SelectedTab = 3;
            Assert.IsType<HotKeyPageViewModel>(scope.Model.Content);

            scope.Model.SelectedTab = 0;
            Assert.Same(logical, scope.Model.Content);
        }

        [AvaloniaFact]
        public void An_Unknown_Tab_Index_Falls_Back_To_The_Logical_Tree()
        {
            using var scope = Scope.Create();
            var logical = scope.Model.Content;

            scope.Model.SelectedTab = 99;

            Assert.Same(logical, scope.Model.Content);
        }

        [AvaloniaFact]
        public void ShowHotKeys_Selects_The_HotKeys_Tab()
        {
            using var scope = Scope.Create();

            scope.Model.ShowHotKeys();

            Assert.Equal(3, scope.Model.SelectedTab);
            Assert.IsType<HotKeyPageViewModel>(scope.Model.Content);
        }

        [AvaloniaFact]
        public void SelectControl_Selects_The_Node_On_The_Current_Page()
        {
            var button = new Button();
            using var scope = Scope.Create(button);

            scope.Model.SelectControl(button);

            var tree = Assert.IsType<TreePageViewModel>(scope.Model.Content);
            Assert.Same(button, tree.SelectedNode?.Visual);
        }

        [AvaloniaFact]
        public void RequestTreeNavigateTo_Switches_Tab_And_Selects()
        {
            var button = new Button();
            using var scope = Scope.Create(button);

            scope.Model.RequestTreeNavigateTo(button, isVisualTree: true);

            Assert.Equal(1, scope.Model.SelectedTab);
            var tree = Assert.IsType<TreePageViewModel>(scope.Model.Content);
            Assert.Same(button, tree.SelectedNode?.Visual);
        }

        [AvaloniaFact]
        public void CanShot_Requires_A_Selected_Node_That_Is_In_A_Visual_Tree()
        {
            var button = new Button();
            using var scope = Scope.Create(button);

            Assert.False(scope.Model.CanShot(null));

            scope.Model.SelectControl(button);

            Assert.True(scope.Model.CanShot(null));
        }

        [AvaloniaFact]
        public void SetOptions_Applies_The_Supplied_Options()
        {
            using var scope = Scope.Create();
            var brush = Brushes.Magenta;

            scope.Model.SetOptions(new DevToolsOptions
            {
                ShowImplementedInterfaces = false,
                StartupScreenIndex = 2,
                FocusHighlighterBrush = brush,
                LaunchView = DevToolsViewKind.VisualTree
            });

            Assert.False(scope.Model.ShowImplementedInterfaces);
            Assert.Equal(2, scope.Model.StartupScreenIndex);
            Assert.Same(brush, scope.Model.FocusHighlighter);
            Assert.Equal((int)DevToolsViewKind.VisualTree, scope.Model.SelectedTab);
        }

        [AvaloniaFact]
        public void Toggles_Flip_Their_Flag()
        {
            using var scope = Scope.Create();

            var interfaces = scope.Model.ShowImplementedInterfaces;
            scope.Model.ToggleShowImplementedInterfaces(new object());
            Assert.Equal(!interfaces, scope.Model.ShowImplementedInterfaces);

            var propertyType = scope.Model.ShowDetailsPropertyType;
            scope.Model.ToggleShowDetailsPropertyType(new object());
            Assert.Equal(!propertyType, scope.Model.ShowDetailsPropertyType);

            var marginPadding = scope.Model.ShouldVisualizeMarginPadding;
            scope.Model.ToggleVisualizeMarginPadding();
            Assert.Equal(!marginPadding, scope.Model.ShouldVisualizeMarginPadding);
        }

        [AvaloniaFact]
        public void SelectFocusHighlighter_Ignores_Non_Brush_Parameters()
        {
            using var scope = Scope.Create();

            scope.Model.SelectFocusHighlighter(Brushes.Lime);
            Assert.Same(Brushes.Lime, scope.Model.FocusHighlighter);

            scope.Model.SelectFocusHighlighter("not a brush");
            Assert.Null(scope.Model.FocusHighlighter);
        }

        [AvaloniaFact]
        public void EnableSnapshotStyles_Reaches_The_Selected_Nodes_Details()
        {
            var button = new Button();
            using var scope = Scope.Create(button);
            scope.Model.SelectControl(button);

            scope.Model.EnableSnapshotStyles(true);

            var tree = Assert.IsType<TreePageViewModel>(scope.Model.Content);
            Assert.True(tree.Details!.SnapshotFrames);
        }

        [AvaloniaFact]
        public void Dispose_Clears_The_Renderer_Debug_Overlays()
        {
            var scope = Scope.Create();
            scope.Model.ShowFpsOverlay = true;
            Assert.True(scope.Model.ShowFpsOverlay);

            scope.Model.Dispose();

            Assert.False(scope.Model.ShowFpsOverlay);
            scope.Window.Close();
        }

    }
}
