using System;
using Avalonia.Controls;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Threading;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// A shown headless window plus the <see cref="MainViewModel"/> rooted at it. The window has to
    /// be shown for the view model's input-root plumbing to have anything to attach to, and it is
    /// what most of these tests use as "the application being inspected".
    /// </summary>
    internal sealed class DevToolsScope : IDisposable
    {
        private DevToolsScope(Window window, MainViewModel model)
        {
            Window = window;
            Model = model;
        }

        public Window Window { get; }

        public MainViewModel Model { get; }

        public static DevToolsScope Create(Control? content = null)
        {
            var window = TestWindow.Show(content);

            return new DevToolsScope(window, new MainViewModel(window));
        }

        public void Dispose()
        {
            Model.Dispose();
            Window.Close();
        }
    }

    internal static class TestWindow
    {
        /// <summary>Shows a headless window and runs a layout pass over it.</summary>
        public static Window Show(Control? content = null)
        {
            var window = new Window { Width = 400, Height = 300, Content = content };
            window.Show();
            RunLayout();

            return window;
        }

        /// <summary>
        /// Drains the dispatcher down to layout priority. Avalonia's LayoutManager is internal, so
        /// this is how a test gets templates applied and bounds assigned.
        /// </summary>
        public static void RunLayout() => Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
    }
}
