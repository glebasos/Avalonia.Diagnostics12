using System;
using Xunit;
using Slot = Avalonia.Diagnostics.DevTools.SerialDisposableSlot;

namespace Avalonia.Diagnostics.UnitTests
{
    /// <summary>
    /// Fork-local replacement for Avalonia 11's internal <c>SerialDisposableValue</c>, which was
    /// removed in Avalonia 12. It holds the subscription for the currently-open DevTools window, so
    /// getting the disposal semantics wrong leaks a window per hotkey press.
    /// </summary>
    public class SerialDisposableSlotTests
    {
        [Fact]
        public void Assigning_A_New_Value_Disposes_The_Previous_One()
        {
            var first = new Tracker();
            var second = new Tracker();
            using var slot = new Slot { Disposable = first };

            slot.Disposable = second;

            Assert.True(first.Disposed);
            Assert.False(second.Disposed);
            Assert.Same(second, slot.Disposable);
        }

        [Fact]
        public void Disposing_The_Slot_Disposes_The_Current_Value()
        {
            var tracker = new Tracker();
            var slot = new Slot { Disposable = tracker };

            slot.Dispose();

            Assert.True(tracker.Disposed);
            Assert.Null(slot.Disposable);
        }

        [Fact]
        public void Assigning_After_Disposal_Disposes_The_New_Value_Immediately()
        {
            var slot = new Slot();
            slot.Dispose();

            var tracker = new Tracker();
            slot.Disposable = tracker;

            Assert.True(tracker.Disposed);
            Assert.Null(slot.Disposable);
        }

        [Fact]
        public void Disposing_Twice_Does_Not_Dispose_The_Value_Twice()
        {
            var tracker = new Tracker();
            var slot = new Slot { Disposable = tracker };

            slot.Dispose();
            slot.Dispose();

            Assert.Equal(1, tracker.DisposeCount);
        }

        [Fact]
        public void A_Null_Value_Is_Accepted()
        {
            var tracker = new Tracker();
            using var slot = new Slot { Disposable = tracker };

            slot.Disposable = null;

            Assert.True(tracker.Disposed);
            Assert.Null(slot.Disposable);
        }

        private sealed class Tracker : IDisposable
        {
            public int DisposeCount { get; private set; }

            public bool Disposed => DisposeCount > 0;

            public void Dispose() => DisposeCount++;
        }
    }
}
