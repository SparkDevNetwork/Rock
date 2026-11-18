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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Data;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.ViewModels.Event;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Checks which real-time channels <see cref="AttendanceService"/> sends
    /// attendance deleted notices to.
    /// </summary>
    [TestClass]
    public class AttendanceServiceTests
    {
        #region Fields

        private Engine _previousEngine;

        private List<string> _sentChannels;

        #endregion

        #region Setup

        /// <summary>
        /// Swaps in an engine that records the channels each notice is sent to.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _sentChannels = new List<string>();
            _previousEngine = RealTimeHelper.Engine;

            RealTimeHelper.Initialize( new RecordingEngine( _sentChannels ) );
        }

        /// <summary>
        /// Puts back the engine that was in place before the test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            RealTimeHelper.Initialize( _previousEngine );
        }

        #endregion

        #region Tests

        /// <summary>
        /// A deletion notice must go only to the channels of the record's own
        /// group and location.
        /// </summary>
        [TestMethod]
        public async Task SendAttendanceDeletedMessagesAsync_WithGroupAndLocation_SendsOnlyToThoseChannels()
        {
            var groupGuid = Guid.NewGuid();
            var locationGuid = Guid.NewGuid();
            var item = CreateDeletedState();
            var bag = new AttendanceUpdatedMessageBag
            {
                AttendanceGuid = item.Guid,
                GroupGuid = groupGuid,
                LocationGuid = locationGuid
            };

            await AttendanceService.SendAttendanceDeletedMessagesAsync( new[] { item }, new List<AttendanceUpdatedMessageBag> { bag } );

            CollectionAssert.AreEquivalent( new[]
            {
                EntityUpdatedTopic.GetAttendanceChannelForGroup( groupGuid ),
                EntityUpdatedTopic.GetAttendanceChannelForLocation( locationGuid )
            }, _sentChannels );
        }

        /// <summary>
        /// A deletion notice with no group and no location has nobody to go
        /// to, so it must not be sent.
        /// </summary>
        [TestMethod]
        public async Task SendAttendanceDeletedMessagesAsync_WithoutGroupOrLocation_SendsNothing()
        {
            var item = CreateDeletedState();
            var bag = new AttendanceUpdatedMessageBag
            {
                AttendanceGuid = item.Guid
            };

            await AttendanceService.SendAttendanceDeletedMessagesAsync( new[] { item }, new List<AttendanceUpdatedMessageBag> { bag } );

            Assert.AreEqual( 0, _sentChannels.Count );
        }

        /// <summary>
        /// A deleted record whose message bag could not be built must not be
        /// sent.
        /// </summary>
        [TestMethod]
        public async Task SendAttendanceDeletedMessagesAsync_WithoutBag_SendsNothing()
        {
            var item = CreateDeletedState();

            await AttendanceService.SendAttendanceDeletedMessagesAsync( new[] { item }, new List<AttendanceUpdatedMessageBag>() );

            Assert.AreEqual( 0, _sentChannels.Count );
        }

        #endregion

        #region Support Methods

        private static AttendanceService.AttendanceUpdatedState CreateDeletedState()
        {
            var attendance = new Attendance
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                OccurrenceId = 1
            };

            return new AttendanceService.AttendanceUpdatedState( attendance, 1, EntityContextState.Deleted );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// An engine that knows only the entity updated topic and records the
        /// channel names that messages are sent to.
        /// </summary>
        private class RecordingEngine : Engine
        {
            public RecordingEngine( List<string> sentChannels )
            {
                var configuration = new RecordingTopicConfiguration( typeof( EntityUpdatedTopic ) );
                var topics = new List<TopicConfiguration> { configuration };
                var registeredTopicsField = typeof( Engine ).GetField( "_registeredTopics", BindingFlags.NonPublic | BindingFlags.Instance );

                registeredTopicsField.SetValue( this, new Lazy<List<TopicConfiguration>>( () => topics ) );

                var client = new Mock<IEntityUpdated>();
                client.Setup( c => c.AttendanceDeleted( It.IsAny<Guid>(), It.IsAny<AttendanceUpdatedMessageBag>() ) )
                    .Returns( Task.CompletedTask );

                var clients = new Mock<ITopicClients<IEntityUpdated>>();
                clients.Setup( c => c.Channel( It.IsAny<string>() ) )
                    .Callback<string>( channelName => sentChannels.Add( channelName ) )
                    .Returns( client.Object );
                clients.Setup( c => c.Channels( It.IsAny<IReadOnlyList<string>>() ) )
                    .Callback<IReadOnlyList<string>>( channelNames => sentChannels.AddRange( channelNames ) )
                    .Returns( client.Object );

                configuration.TopicContext.Clients = clients.Object;
            }

            protected override TopicConfiguration GetTopicConfiguration( Type topicType )
            {
                return new RecordingTopicConfiguration( topicType );
            }

            protected override void ConfigureTopicInstance( object realTimeHub, TopicConfiguration topicConfiguration, ITopicInternal topicInstance )
            {
            }

            public override Task SendMessageAsync( object proxy, string topicIdentifier, string messageName, object[] parameters, CancellationToken cancellationToken )
            {
                return Task.CompletedTask;
            }

            protected override Task RemoveFromTopicChannelsAsync( object realTimeHub, string connectionIdentifier, string topicIdentifier, IEnumerable<string> channelNames )
            {
                return Task.CompletedTask;
            }
        }

        private class RecordingTopicConfiguration : TopicConfiguration
        {
            public RecordingTopicConfiguration( Type topicType )
                : base( topicType )
            {
            }
        }

        #endregion
    }
}
