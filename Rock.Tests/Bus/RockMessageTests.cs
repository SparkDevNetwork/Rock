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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Tests.Bus
{
    [TestClass]
    public class RockMessageTests
    {
        private const string MarkerValue = "message-body-marker-6f1c";

        [TestMethod]
        public void GetLogString_IncludesQueueAndMessageType()
        {
            var message = new TestEventMessage { Marker = MarkerValue };

            var logString = RockMessage.GetLogString( message );

            StringAssert.Contains( logString, "rock-giving-event-queue" );
            StringAssert.Contains( logString, typeof( TestEventMessage ).FullName );
        }

        [TestMethod]
        public void GetLogString_DoesNotIncludeMessageBody()
        {
            var message = new TestEventMessage { Marker = MarkerValue };

            var logString = RockMessage.GetLogString( message );

            Assert.IsFalse( logString.Contains( MarkerValue ), "The log string should not contain the message body." );
        }

        public class TestEventMessage : IEventMessage<GivingEventQueue>
        {
            public string SenderNodeName { get; set; }

            public string Marker { get; set; }
        }
    }
}
