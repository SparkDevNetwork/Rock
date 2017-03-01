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
    public class ReservationMinistryService : Service<ReservationMinistry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationMinistryService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationMinistryService( RockContext context ) : base( context ) { }
    }

    public static partial class ReservationMinistryExtensionMethods
    {
        public static ReservationMinistry Clone( this ReservationMinistry source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as ReservationMinistry;
            }
            else
            {
                var target = new ReservationMinistry();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        public static void CopyPropertiesFrom( this ReservationMinistry target, ReservationMinistry source )
        {
            target.Id = source.Id;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.Name = source.Name;
            target.Description = source.Description;
            target.Order = source.Order;
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
