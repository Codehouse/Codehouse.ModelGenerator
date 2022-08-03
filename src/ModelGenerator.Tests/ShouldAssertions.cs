using ModelGenerator.Framework.Progress;
using Shouldly;

namespace ModelGenerator.Tests
{
    public static class ShouldAssertions
    {
        public static void ShouldContainFail<T>(this RagBuilder<T> tracker)
        {
            tracker.GetFails().ShouldNotBeEmpty("Tracker should contain fails but did not.");
        }
        public static void ShouldContainFail<T>(this ScopedRagBuilder<T> tracker)
        {
            tracker.HasFails.ShouldBeTrue("Tracker should contain fails but did not.");
        }
        public static void ShouldContainWarning<T>(this RagBuilder<T> tracker)
        {
            tracker.GetWarns().ShouldNotBeEmpty("Tracker should contain warnings but did not.");
        }
        public static void ShouldContainWarning<T>(this ScopedRagBuilder<T> tracker)
        {
            tracker.HasWarns.ShouldBeTrue("Tracker should contain warnings but did not.");
        }
    }
}