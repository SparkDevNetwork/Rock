using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Observability;

namespace Rock.Tests.UnitTests.Rock.Observability
{
    [TestClass]
    public class ObservabilityHelperTests
    {
        /// <summary>
        /// Verifies that an activity can not have more than 9,999 descendants.
        /// Some analytics databases limit the logic to 10,000 activities in
        /// a single task.
        /// </summary>
        [TestMethod]
        public void RootActivityDoesNotExceedCap()
        {
            // The listener is removed from the activity source when it is disposed.
            using ( var listener = new ActivityListener() )
            {
                var childActivityCount = 0;
                var source = RockActivitySource.ActivitySource;
                var expectedCount = ObservabilityHelper.SpanCountLimit - 1;

                listener.ActivityStarted = activity =>
                {
                    if ( activity.Parent != null )
                    {
                        childActivityCount++;
                    }
                };
                listener.ShouldListenTo = src => src == source;
                listener.SampleUsingParentId = ( ref ActivityCreationOptions<string> _ ) => ActivitySamplingResult.AllData;
                listener.Sample = ( ref ActivityCreationOptions<ActivityContext> _ ) => ActivitySamplingResult.AllData;

                ActivitySource.AddActivityListener( listener );

                using ( var rootActivity = ObservabilityHelper.StartActivity( "Root Activity" ) )
                {
                    Assert.IsNotNull( rootActivity, "Root activity was null, configuration failed." );

                    for ( int c = 0; c < 15; c++ )
                    {
                        using ( var childActivity = ObservabilityHelper.StartActivity( $"Child Activity {c}" ) )
                        {
                            for ( int gc = 0; gc < 1_000; gc++ )
                            {
                                using ( var grandChildActivity = ObservabilityHelper.StartActivity( $"Grand Child Activity {c}.{gc}" ) )
                                {
                                }
                            }
                        }
                    }

                    Assert.AreEqual( expectedCount, childActivityCount, "Incorrect child activity count." );
                    Assert.AreEqual( 15_015, rootActivity.GetTagItem( "rock.descendant_count" ), "Incorrect descendant count." );
                }
            }
        }
    }
}
