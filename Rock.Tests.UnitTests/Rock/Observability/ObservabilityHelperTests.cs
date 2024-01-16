// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
                    Assert.AreEqual( 15_015, rootActivity.GetTagItem( "rock-descendant-count" ), "Incorrect descendant count." );
                }
            }
        }
    }
}
