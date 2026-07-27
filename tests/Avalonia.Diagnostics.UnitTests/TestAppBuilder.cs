using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Themes.Simple;

[assembly: AvaloniaTestApplication(typeof(Avalonia.Diagnostics.UnitTests.TestAppBuilder))]
// Swaps in the xunit v3 test framework that marshals [AvaloniaFact]/[AvaloniaTheory] bodies onto
// the headless UI thread.
[assembly: AvaloniaTestFramework]

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// Headless application used by every <c>[AvaloniaTest]</c> in this assembly. The Simple theme is
    /// included because that is what the DevTools window itself styles against, so any test that
    /// builds a real DevTools view gets the same templates the shipped tool does.
    /// </summary>
    public sealed class TestApp : Application
    {
        public override void Initialize() => Styles.Add(new SimpleTheme());
    }

    public static class TestAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder
            .Configure<TestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
