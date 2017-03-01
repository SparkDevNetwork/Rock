// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Data;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationStatusService : Service<ReservationStatus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationStatusService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationStatusService( RockContext context ) : base( context ) { }      
    }

    public static partial class ReservationStatusExtensionMethods
    {
        public static ReservationStatus Clone( this ReservationStatus source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as ReservationStatus;
            }
            else
            {
                var target = new ReservationStatus();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        public static void CopyPropertiesFrom( this ReservationStatus target, ReservationStatus source )
        {
            target.Id = source.Id;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.Name = source.Name;
            target.Description = source.Description;
            target.IsCritical = source.IsCritical;
            target.IsDefault = source.IsDefault;
            target.IsActive = source.IsActive;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
        }
    }
}
