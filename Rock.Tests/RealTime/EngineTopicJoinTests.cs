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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.RealTime;

namespace Rock.Tests.RealTime
{
    /// <summary>
    /// This suite checks that <see cref="Engine"/> only counts a connection
    /// as joined to a topic after the topic's OnConnectedAsync check has
    /// passed, and that joins and leaves on one connection do not interleave.
    /// </summary>
    [TestClass]
    public class EngineTopicJoinTests
    {
        #region Constants

        /// <summary>
        /// The longest any single await in these tests may take before the
        /// test fails, so a deadlock fails the test instead of hanging it.
        /// </summary>
        private const int AwaitTimeoutMilliseconds = 5000;

        /// <summary>
        /// The timeout for each test method.
        /// </summary>
        private const int TestTimeoutMilliseconds = 30000;

        /// <summary>
        /// How long to wait before checking that a call is still blocked.
        /// </summary>
        private const int BlockedCheckDelayMilliseconds = 200;

        /// <summary>
        /// The channel the test topic adds the connection to while joining.
        /// </summary>
        private const string TestChannelName = "test-channel";

        #endregion

        #region Tests

        /// <summary>
        /// Two concurrent joins on one connection whose check fails must both
        /// fail and leave the connection not joined.
        /// </summary>
        [TestMethod]
        [Timeout( TestTimeoutMilliseconds )]
        public async Task ConnectToTopic_ConcurrentFailingJoins_BothFailAndConnectionIsNotJoined()
        {
            var engine = new TestEngine();
            var hub = new TestHub( engine, "connection-1" );

            var firstJoin = engine.ConnectToTopic( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier );
            var secondJoin = engine.ConnectToTopic( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier );

            await Task.Delay( BlockedCheckDelayMilliseconds );

            Assert.IsFalse( secondJoin.IsCompleted, "The second join completed while the first join's check was still running." );
            Assert.AreEqual( 1, hub.Gate.ConnectedCallCount );

            hub.Gate.Fail();

            await AssertThrowsAsync<RealTimeException>( firstJoin );
            await AssertThrowsAsync<RealTimeException>( secondJoin );

            Assert.IsFalse( IsConnected( engine, hub.ConnectionIdentifier ) );
            Assert.AreEqual( 2, hub.Gate.ConnectedCallCount );
            CollectionAssert.Contains( engine.RemovedChannels.ToList(), TestChannelName );
        }

        /// <summary>
        /// Two concurrent joins on one connection whose check passes must both
        /// succeed, run the check once and count two joins.
        /// </summary>
        [TestMethod]
        [Timeout( TestTimeoutMilliseconds )]
        public async Task ConnectToTopic_ConcurrentSucceedingJoins_BothSucceedAndCountTwice()
        {
            var engine = new TestEngine();
            var hub = new TestHub( engine, "connection-1" );

            var firstJoin = engine.ConnectToTopic( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier );
            var secondJoin = engine.ConnectToTopic( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier );

            hub.Gate.Succeed();

            Assert.IsTrue( await WithTimeout( firstJoin ) );
            Assert.IsTrue( await WithTimeout( secondJoin ) );
            Assert.AreEqual( 1, hub.Gate.ConnectedCallCount );
            Assert.AreEqual( 2, GetJoinCount( engine, hub.ConnectionIdentifier ) );

            Assert.IsTrue( await WithTimeout( engine.DisconnectFromTopicAsync( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier ) ) );
            Assert.IsTrue( IsConnected( engine, hub.ConnectionIdentifier ) );

            Assert.IsTrue( await WithTimeout( engine.DisconnectFromTopicAsync( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier ) ) );
            Assert.IsFalse( IsConnected( engine, hub.ConnectionIdentifier ) );
            Assert.AreEqual( 1, hub.Gate.DisconnectedCallCount );
        }

        /// <summary>
        /// A failed join must release the join lock so a later join on the
        /// same connection can still run.
        /// </summary>
        [TestMethod]
        [Timeout( TestTimeoutMilliseconds )]
        public async Task ConnectToTopic_AfterFailedJoin_LaterJoinCompletes()
        {
            var engine = new TestEngine();
            var failingHub = new TestHub( engine, "connection-1" );

            var failedJoin = engine.ConnectToTopic( failingHub, TestEngine.TopicIdentifier, failingHub.ConnectionIdentifier );
            failingHub.Gate.Fail();

            await AssertThrowsAsync<RealTimeException>( failedJoin );

            var succeedingHub = new TestHub( engine, "connection-1" );
            succeedingHub.Gate.Succeed();

            Assert.IsTrue( await WithTimeout( engine.ConnectToTopic( succeedingHub, TestEngine.TopicIdentifier, succeedingHub.ConnectionIdentifier ) ) );
            Assert.IsTrue( IsConnected( engine, succeedingHub.ConnectionIdentifier ) );
        }

        /// <summary>
        /// A leave on a connection must wait for a join of the same topic on
        /// that connection to finish its check.
        /// </summary>
        [TestMethod]
        [Timeout( TestTimeoutMilliseconds )]
        public async Task DisconnectFromTopicAsync_WhileJoinIsRunning_WaitsForJoin()
        {
            var engine = new TestEngine();
            var hub = new TestHub( engine, "connection-1" );

            var join = engine.ConnectToTopic( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier );
            var leave = engine.DisconnectFromTopicAsync( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier );

            await Task.Delay( BlockedCheckDelayMilliseconds );

            Assert.IsFalse( leave.IsCompleted, "The leave completed while the join's check was still running." );

            hub.Gate.Succeed();

            Assert.IsTrue( await WithTimeout( join ) );
            Assert.IsTrue( await WithTimeout( leave ) );
            Assert.AreEqual( 0, GetJoinCount( engine, hub.ConnectionIdentifier ) );
            Assert.IsFalse( IsConnected( engine, hub.ConnectionIdentifier ) );
        }

        /// <summary>
        /// A client disconnect must not wait for a join that is still running
        /// its check, and the join must still finish cleanly afterwards.
        /// </summary>
        [TestMethod]
        [Timeout( TestTimeoutMilliseconds )]
        public async Task ClientDisconnectedAsync_WhileJoinIsRunning_DoesNotWait()
        {
            var engine = new TestEngine();
            var hub = new TestHub( engine, "connection-1" );

            await WithTimeout( engine.ClientConnectedAsync( hub, hub.ConnectionIdentifier ) );

            var join = engine.ConnectToTopic( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier );

            await WithTimeout( engine.ClientDisconnectedAsync( hub, hub.ConnectionIdentifier ) );

            hub.Gate.Succeed();

            Assert.IsTrue( await WithTimeout( join ) );
            Assert.IsFalse( engine.HasConnectionState<EngineConnectionState>( hub.ConnectionIdentifier ) );
        }

        /// <summary>
        /// A join held on one connection must not delay joins on other
        /// connections.
        /// </summary>
        [TestMethod]
        [Timeout( TestTimeoutMilliseconds )]
        public async Task ConnectToTopic_HeldJoinOnOneConnection_DoesNotBlockOtherConnections()
        {
            var engine = new TestEngine();
            var heldHub = new TestHub( engine, "connection-held" );

            var heldJoin = engine.ConnectToTopic( heldHub, TestEngine.TopicIdentifier, heldHub.ConnectionIdentifier );

            var otherHubs = Enumerable.Range( 0, 500 )
                .Select( i => new TestHub( engine, $"connection-{i}" ) )
                .ToList();

            foreach ( var otherHub in otherHubs )
            {
                otherHub.Gate.Succeed();
            }

            var otherJoins = otherHubs
                .Select( h => Task.Run( () => engine.ConnectToTopic( h, TestEngine.TopicIdentifier, h.ConnectionIdentifier ) ) )
                .ToList();

            var results = await WithTimeout( Task.WhenAll( otherJoins ) );

            Assert.IsFalse( heldJoin.IsCompleted );
            Assert.IsTrue( results.All( r => r ) );

            foreach ( var otherHub in otherHubs )
            {
                Assert.IsTrue( IsConnected( engine, otherHub.ConnectionIdentifier ) );
            }

            heldHub.Gate.Succeed();

            Assert.IsTrue( await WithTimeout( heldJoin ) );
        }

        /// <summary>
        /// Many concurrent joins of one topic on one connection must all
        /// succeed, run the check once and count every join.
        /// </summary>
        [TestMethod]
        [Timeout( TestTimeoutMilliseconds )]
        public async Task ConnectToTopic_ManyConcurrentJoinsOnOneConnection_AllCounted()
        {
            var engine = new TestEngine();
            var hub = new TestHub( engine, "connection-1" );

            hub.Gate.Succeed();

            var joins = Enumerable.Range( 0, 50 )
                .Select( _ => Task.Run( () => engine.ConnectToTopic( hub, TestEngine.TopicIdentifier, hub.ConnectionIdentifier ) ) )
                .ToList();

            var results = await WithTimeout( Task.WhenAll( joins ) );

            Assert.IsTrue( results.All( r => r ) );
            Assert.AreEqual( 1, hub.Gate.ConnectedCallCount );
            Assert.AreEqual( 50, GetJoinCount( engine, hub.ConnectionIdentifier ) );
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Waits for the task, failing the test if it does not finish in time.
        /// </summary>
        private static async Task WithTimeout( Task task )
        {
            if ( await Task.WhenAny( task, Task.Delay( AwaitTimeoutMilliseconds ) ) != task )
            {
                Assert.Fail( "The operation did not complete in time." );
            }

            await task;
        }

        /// <summary>
        /// Waits for the task, failing the test if it does not finish in time.
        /// </summary>
        private static async Task<T> WithTimeout<T>( Task<T> task )
        {
            await WithTimeout( ( Task ) task );

            return await task;
        }

        /// <summary>
        /// Checks that the task fails with the expected exception in time.
        /// </summary>
        private static async Task AssertThrowsAsync<TException>( Task task )
            where TException : Exception
        {
            try
            {
                await WithTimeout( task );
            }
            catch ( TException )
            {
                return;
            }

            Assert.Fail( $"Expected {typeof( TException ).Name} to be thrown." );
        }

        /// <summary>
        /// Determines whether the connection counts as joined to the test topic.
        /// </summary>
        private static bool IsConnected( TestEngine engine, string connectionIdentifier )
        {
            return engine.GetConnectionState<EngineConnectionState>( connectionIdentifier )
                .ConnectedTopics
                .IsConnected( TestEngine.TopicIdentifier );
        }

        /// <summary>
        /// Gets the number of joins the connection has for the test topic.
        /// </summary>
        private static int GetJoinCount( TestEngine engine, string connectionIdentifier )
        {
            var topicChannels = engine.GetConnectionState<EngineConnectionState>( connectionIdentifier ).ConnectedTopics;
            var countersField = typeof( ConcurrentTopicChannels ).GetField( "_counters", BindingFlags.NonPublic | BindingFlags.Instance );
            var counters = ( Dictionary<string, int> ) countersField.GetValue( topicChannels );

            return counters.TryGetValue( TestEngine.TopicIdentifier, out var count ) ? count : 0;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Controls when and how the test topic's join check finishes.
        /// </summary>
        private class TopicGate
        {
            private readonly TaskCompletionSource<bool> _result = new TaskCompletionSource<bool>( TaskCreationOptions.RunContinuationsAsynchronously );

            private int _connectedCallCount;

            private int _disconnectedCallCount;

            public int ConnectedCallCount => Volatile.Read( ref _connectedCallCount );

            public int DisconnectedCallCount => Volatile.Read( ref _disconnectedCallCount );

            public Task<bool> Result => _result.Task;

            public void Succeed() => _result.TrySetResult( true );

            public void Fail() => _result.TrySetResult( false );

            public void RecordConnected() => Interlocked.Increment( ref _connectedCallCount );

            public void RecordDisconnected() => Interlocked.Increment( ref _disconnectedCallCount );
        }

        /// <summary>
        /// Stands in for the real hub object. Carries the connection and the
        /// gate the test topic should use.
        /// </summary>
        private class TestHub
        {
            public TestHub( TestEngine engine, string connectionIdentifier )
            {
                Engine = engine;
                ConnectionIdentifier = connectionIdentifier;
            }

            public TestEngine Engine { get; }

            public string ConnectionIdentifier { get; }

            public TopicGate Gate { get; } = new TopicGate();
        }

        /// <summary>
        /// The client interface for the test topic.
        /// </summary>
        private interface IGatedTestTopicClient
        {
        }

        /// <summary>
        /// A topic whose join check waits on a <see cref="TopicGate"/>.
        /// </summary>
        [RealTimeTopic]
        private class GatedTestTopic : Topic<IGatedTestTopicClient>
        {
            public TestHub Hub { get; set; }

            public override async Task OnConnectedAsync()
            {
                Hub.Gate.RecordConnected();

                // Simulate the topic adding the connection to a channel
                // before its check finishes.
                Hub.Engine.ClientAddedToChannel( Hub.ConnectionIdentifier, TestEngine.TopicIdentifier, TestChannelName );

                if ( !await Hub.Gate.Result )
                {
                    throw new RealTimeException( "Not authorized." );
                }
            }

            public override Task OnDisconnectedAsync()
            {
                Hub.Gate.RecordDisconnected();

                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// The configuration for the test topic.
        /// </summary>
        private class TestTopicConfiguration : TopicConfiguration
        {
            public TestTopicConfiguration( Type topicType )
                : base( topicType )
            {
            }
        }

        /// <summary>
        /// An engine that only knows the test topic and records the channels
        /// it is asked to remove connections from.
        /// </summary>
        private class TestEngine : Engine
        {
            public static readonly string TopicIdentifier = typeof( GatedTestTopic ).FullName;

            public TestEngine()
            {
                // Register only the test topic instead of scanning assemblies.
                var topics = new List<TopicConfiguration> { new TestTopicConfiguration( typeof( GatedTestTopic ) ) };
                var registeredTopicsField = typeof( Engine ).GetField( "_registeredTopics", BindingFlags.NonPublic | BindingFlags.Instance );

                registeredTopicsField.SetValue( this, new Lazy<List<TopicConfiguration>>( () => topics ) );
            }

            public ConcurrentQueue<string> RemovedChannels { get; } = new ConcurrentQueue<string>();

            protected override TopicConfiguration GetTopicConfiguration( Type topicType )
            {
                return new TestTopicConfiguration( topicType );
            }

            protected override void ConfigureTopicInstance( object realTimeHub, TopicConfiguration topicConfiguration, ITopicInternal topicInstance )
            {
                ( ( GatedTestTopic ) topicInstance ).Hub = ( TestHub ) realTimeHub;
            }

            public override Task SendMessageAsync( object proxy, string topicIdentifier, string messageName, object[] parameters, CancellationToken cancellationToken )
            {
                return Task.CompletedTask;
            }

            protected override Task RemoveFromTopicChannelsAsync( object realTimeHub, string connectionIdentifier, string topicIdentifier, IEnumerable<string> channelNames )
            {
                foreach ( var channelName in channelNames )
                {
                    RemovedChannels.Enqueue( channelName );
                }

                return Task.CompletedTask;
            }
        }

        #endregion
    }
}
