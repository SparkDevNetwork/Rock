﻿// <copyright>
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

using Rock.Data;
using Rock.UniversalSearch;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Calls <see cref="IRockIndexable.DeleteIndexedDocument(int)"/> for the specified <see cref="IEntity"/>
    /// </summary>
    public sealed class DeleteEntityTypeIndex : BusStartedTask<DeleteEntityTypeIndex.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var entityType = EntityTypeCache.Get( message.EntityTypeId );
            Type type = entityType.GetEntityType();

            if ( type != null )
            {
                object classInstance = Activator.CreateInstance( type, null );
                MethodInfo indexItemMethod = type.GetMethod( "DeleteIndexedDocument" );

                if ( classInstance != null && indexItemMethod != null )
                {
                    object[] parameters = { message.EntityId };
                    indexItemMethod.Invoke( classInstance, parameters );
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
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
        }
    }
}