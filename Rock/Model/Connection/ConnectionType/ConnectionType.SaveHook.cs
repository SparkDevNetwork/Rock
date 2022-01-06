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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public partial class ConnectionType
    {
        /// <summary>
        /// Save hook implementation for <see cref="ConnectionType"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<ConnectionType>
        {
            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                if ( this.State == EntityContextState.Deleted )
                {
                    var qualifierValue = Entity.Id.ToString();
                    var rockContext = ( RockContext ) this.RockContext;
                    var attributeService = new AttributeService( rockContext );
                    var existingAttributes = attributeService.GetByEntityTypeId( new ConnectionRequest().TypeId, true )
                        .AsQueryable()
                        .Where( a =>
                           a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                           a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                       .OrderBy( a => a.Order )
                       .ThenBy( a => a.Name )
                       .ToList();

                    foreach ( var attr in existingAttributes )
                    {
                        attributeService.Delete( attr );
                        rockContext.SaveChanges();
                    }
                }

                base.PostSave();
            }
        }
    }
}
