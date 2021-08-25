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
using Rock.Follow;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// FollowingEventType Logic
    /// </summary>
    public partial class FollowingEventType
    {
        #region Public Methods

        /// <summary>
        /// Gets the event component.
        /// </summary>
        /// <returns></returns>
        public virtual EventComponent GetEventComponent()
        {
            if ( EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( EntityTypeId.Value );
                if ( entityType != null )
                {
                    return EventContainer.GetComponent( entityType.Name );
                }
            }

            return null;
        }

        #endregion Public Methods
    }
}