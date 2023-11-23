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

using System.Threading;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Updates the data in <see cref="Rock.Model.PersonAliasPersonalization"/> table based on the specified segment's criteria.
    /// </summary>
    public sealed class UpdatePersonAliasPersonalizationData : BusStartedTask<UpdatePersonAliasPersonalizationData.Message>
    {
        /// <inheritdoc/>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = 600;

                var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
                var personalizationSegment = personalizationSegmentService.Get( message.PersonalizationSegmentId );

                if ( personalizationSegment != null )
                {
                    personalizationSegmentService.UpdatePersonAliasPersonalizationData( PersonalizationSegmentCache.Get( message.PersonalizationSegmentId ) );

                    personalizationSegment.IsDirty = false;

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// The message containing the personalization segment to be calculated
        /// </summary>
        /// <seealso cref="Rock.Tasks.BusStartedTaskMessage" />
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the personalization segment identifier.
            /// </summary>
            /// <value>
            /// The personalization segment identifier.
            /// </value>
            public int PersonalizationSegmentId { get; set; }
        }
    }
}
