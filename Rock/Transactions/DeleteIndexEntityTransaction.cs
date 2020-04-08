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
using System.Reflection;

using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a person is viewed.
    /// </summary>
    public class DeleteIndexEntityTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the viewer person id.
        /// </summary>
        /// <value>
        /// The viewer person id.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int EntityId { get; set; }
        
        /// <summary>
        /// Execute method to delete the indexed document.
        /// </summary>
        public void Execute()
        {
            var entityType = EntityTypeCache.Get( EntityTypeId );
            Type type = entityType.GetEntityType();

            if ( type != null )
            {
                object classInstance = Activator.CreateInstance( type, null );
                MethodInfo indexItemMethod = type.GetMethod( "DeleteIndexedDocument" );

                if ( classInstance != null && indexItemMethod != null )
                {
                    object[] parameters = { EntityId };
                    indexItemMethod.Invoke( classInstance, parameters );
                }
            }
        }
    }
}