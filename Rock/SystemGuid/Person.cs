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
namespace Rock.SystemGuid
{
    /// <summary>
    /// 
    /// </summary>
    public static class Person
    {
        /// <summary>
        /// The Guid of the 'Giver Anonymous' person that ships with core
        /// </summary>
        public const string GIVER_ANONYMOUS = "802235DC-3CA5-94B0-4326-AACE71180F48";

        /// <summary>
        /// The Guid of the 'Anonymous Visitor' person that ships with core. This is used by Visitor Tracking.
        /// </summary>
        public const string ANONYMOUS_VISITOR = "7EBC167B-512D-4683-9D80-98B6BB02E1B9";

        /// <summary>
        /// The guid of the 'System Sender' person that ships with core. This is used as the 'from' person when sending
        /// automated communications.
        /// </summary>
        public const string SYSTEM_SENDER = "817E7C25-6CB1-4ED0-B224-FF3C4DA0B716";

        /// <summary>
        /// The Guid that stands for chat itself as the author of a message, shown as 'Rock Chat'.
        /// </summary>
        /// <remarks>
        /// <para>
        /// No Person row backs this. It is the sender of the messages a conversation generates about itself, such as
        /// someone joining or a channel being renamed, and it needs an identity only because every message names one.
        /// </para>
        /// <para>
        /// It is the same value in every installation on purpose. A per-installation value would make each church's
        /// system author a different person to every client, so no client could recognise one without being told, and
        /// nothing could be styled or filtered by it.
        /// </para>
        /// </remarks>
        public const string CHAT_SYSTEM_AUTHOR = "A7C0DE00-0000-4000-8000-000000000001";
    }
}
