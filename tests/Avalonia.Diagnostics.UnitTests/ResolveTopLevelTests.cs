using Avalonia.Controls;
using Avalonia.Diagnostics.Views;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;
using DevToolsWindow = Avalonia.Diagnostics.Views.MainWindow;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// Regression cover for the Avalonia 11 -> 12 port. In Avalonia 12 an input root's
    /// <c>RootElement</c> is a <see cref="TopLevelHost"/> that sits <em>above</em> the TopLevel in
    /// the visual tree, so <see cref="TopLevel.GetTopLevel"/> - which walks up - returns null for
    /// it. Every DevTools hotkey silently stopped working because the handler bailed at that guard.
    /// <see cref="DevToolsWindow.ResolveTopLevel(Visual)"/> searches downward instead.
    /// </summary>
    public class ResolveTopLevelTests
    {
        [AvaloniaFact]
        public void GetTopLevel_Does_Not_Work_On_The_Host_Visual()
        {
            var window = TestWindow.Show(new Button());
            var host = window.GetVisualParent();

            // The premise of the helper. If this ever starts returning the window, the downward
            // search is no longer needed - but until then, do not reintroduce GetTopLevel here.
            Assert.NotNull(host);
            Assert.Null(TopLevel.GetTopLevel(host!));

            window.Close();
        }

        [AvaloniaFact]
        public void Resolves_The_TopLevel_From_Its_Host_Visual()
        {
            var window = TestWindow.Show(new Button());
            var host = window.GetVisualParent();

            Assert.Same(window, DevToolsWindow.ResolveTopLevel(host!));

            window.Close();
        }

        [AvaloniaFact]
        public void A_TopLevel_Resolves_To_Itself()
        {
            var window = TestWindow.Show(new Button());

            Assert.Same(window, DevToolsWindow.ResolveTopLevel(window));

            window.Close();
        }

        [AvaloniaFact]
        public void Returns_Null_When_There_Is_No_TopLevel_Below()
        {
            var orphan = new Panel { Children = { new Button() } };

            Assert.Null(DevToolsWindow.ResolveTopLevel(orphan));
        }
    }
}
