using System.Linq;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Input;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    public class HotKeyPageViewModelTests
    {
        [Fact]
        public void Descriptions_Are_Empty_Until_Options_Are_Supplied()
        {
            Assert.Null(new HotKeyPageViewModel().HotKeyDescriptions);
        }

        [Fact]
        public void Every_Configured_Gesture_Is_Listed()
        {
            var vm = new HotKeyPageViewModel();

            vm.SetOptions(new DevToolsOptions());

            var descriptions = vm.HotKeyDescriptions!;
            Assert.Equal(6, descriptions.Count);
            Assert.Contains(descriptions, d => d.Gesture == "F12" && d.BriefDescription == "Launch DevTools");
            Assert.Contains(descriptions, d => d.Gesture == "Alt+S");
            Assert.Contains(descriptions, d => d.Gesture == "Alt+D");
            Assert.Contains(descriptions, d => d.Gesture == "Ctrl+Alt+F");
            Assert.Contains(descriptions, d => d.Gesture == "F8");
        }

        [Fact]
        public void A_Modifier_Only_Gesture_Does_Not_Render_Its_None_Key()
        {
            // InspectHoveredControl is Ctrl+Shift with no key, which KeyGesture renders as
            // "Ctrl+Shift+None".
            var vm = new HotKeyPageViewModel();

            vm.SetOptions(new DevToolsOptions());

            var inspect = vm.HotKeyDescriptions!
                .Single(d => d.BriefDescription == "Inspect Control Under Pointer");

            Assert.Equal("Ctrl+Shift", inspect.Gesture);
        }

        [Fact]
        public void Custom_Gestures_Are_Reflected()
        {
            var vm = new HotKeyPageViewModel();

            vm.SetOptions(new DevToolsOptions
            {
                Gesture = new KeyGesture(Key.F9, KeyModifiers.Control),
                HotKeys = new HotKeyConfiguration { ScreenshotSelectedControl = new KeyGesture(Key.P, KeyModifiers.Alt) }
            });

            var descriptions = vm.HotKeyDescriptions!;
            Assert.Contains(descriptions, d => d.Gesture == "Ctrl+F9" && d.BriefDescription == "Launch DevTools");
            Assert.Contains(descriptions, d => d.Gesture == "Alt+P");
        }

        [Fact]
        public void Every_Entry_Carries_A_Detailed_Description()
        {
            var vm = new HotKeyPageViewModel();

            vm.SetOptions(new DevToolsOptions());

            Assert.All(vm.HotKeyDescriptions!, d => Assert.False(string.IsNullOrWhiteSpace(d.DetailedDescription)));
        }
    }
}
