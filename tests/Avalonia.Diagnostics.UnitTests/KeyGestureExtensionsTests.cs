using Avalonia.Input;
using Avalonia.Input.Raw;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// This is the match that opens DevTools from a raw key event (DevTools.Attach on an
    /// Application), so a regression here means F12 silently stops working.
    /// </summary>
    public class KeyGestureExtensionsTests
    {
        [Fact]
        public void Matches_An_Identical_Key_And_Modifier_Set()
        {
            Assert.True(new KeyGesture(Key.F12).Matches(Key.F12, RawInputModifiers.None));
        }

        [Fact]
        public void Does_Not_Match_A_Different_Key()
        {
            Assert.False(new KeyGesture(Key.F12).Matches(Key.F11, RawInputModifiers.None));
        }

        [Fact]
        public void Modifiers_Must_Match_Exactly()
        {
            var gesture = new KeyGesture(Key.F, KeyModifiers.Control | KeyModifiers.Alt);

            Assert.True(gesture.Matches(Key.F, RawInputModifiers.Control | RawInputModifiers.Alt));
            Assert.False(gesture.Matches(Key.F, RawInputModifiers.Control));
            Assert.False(gesture.Matches(
                Key.F,
                RawInputModifiers.Control | RawInputModifiers.Alt | RawInputModifiers.Shift));
        }

        [Fact]
        public void An_Unmodified_Gesture_Does_Not_Match_A_Modified_Press()
        {
            Assert.False(new KeyGesture(Key.F12).Matches(Key.F12, RawInputModifiers.Control));
        }

        [Fact]
        public void Pointer_Buttons_Are_Masked_Out_Of_The_Modifiers()
        {
            // Raw modifiers carry mouse button state as well; holding a mouse button down must not
            // stop the gesture from matching.
            Assert.True(new KeyGesture(Key.F12).Matches(
                Key.F12,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.RightMouseButton));
        }

        [Theory]
        [InlineData(Key.Add, Key.OemPlus)]
        [InlineData(Key.Subtract, Key.OemMinus)]
        [InlineData(Key.Decimal, Key.OemPeriod)]
        public void Numpad_Operation_Keys_Match_Their_Main_Keyboard_Equivalent(Key numpad, Key main)
        {
            Assert.True(new KeyGesture(main).Matches(numpad, RawInputModifiers.None));
            Assert.True(new KeyGesture(numpad).Matches(main, RawInputModifiers.None));
            Assert.True(new KeyGesture(numpad).Matches(numpad, RawInputModifiers.None));
        }

        [Fact]
        public void Unrelated_Numpad_Keys_Are_Left_Alone()
        {
            Assert.False(new KeyGesture(Key.Multiply).Matches(Key.Divide, RawInputModifiers.None));
        }
    }
}
