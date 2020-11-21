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
using System.Linq;
using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// Custom attribute used to decorate Rock Blocks that require context.  If entity type is not 
    /// included in the attribute, a block property will automatically be added for user to set 
    /// the entity type when block is placed on a page
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
    public class ContextAwareAttribute : System.Attribute
    {
        /// <summary>
        /// The contexts
        /// </summary>
        public List<Context> Contexts { get; } = new List<Context>();

        /// <summary>
        /// Gets or sets a value indicating whether this ContextAware attribute is configurable in BlockProperties
        /// </summary>
        public bool IsConfigurable
        {
            get => _isConfigurable && !Contexts.Any();
            set => _isConfigurable = value;
        }
        private bool _isConfigurable = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareAttribute" /> class.
        /// </summary>
        public ContextAwareAttribute()
        {
            IsConfigurable = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareAttribute"/> class.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        public ContextAwareAttribute( Type entityType ) : this( new[] { entityType } )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareAttribute"/> class.
        /// </summary>
        /// <param name="entityTypes">The entity types.</param>
        public ContextAwareAttribute( params Type[] entityTypes )
        {
            foreach ( var entityType in entityTypes )
            {
                if ( entityType != null )
                {
                    Contexts.Add( new Context( entityType ) );
                }
            }
        }

        /// <summary>
        /// A context of an entity and default parameter
        /// </summary>
        public class Context
        {
            private Type _entityType;

            /// <summary>
            /// Create a context
            /// </summary>
            /// <param name="entityType"></param>
            public Context( Type entityType )
            {
                _entityType = entityType;
            }

            /// <summary>
            /// Gets or sets the type of the entity.
            /// </summary>
            /// <value>
            /// The type of the entity.
            /// </value>
            public EntityTypeCache EntityType
            {
                get
                {
                    if ( _entityType == null)
                    {
                        return null;
                    }
                    
                    return EntityTypeCache.Get( _entityType );
                }
            }

            /// <summary>
            /// Gets the default name of the parameter.
            /// </summary>
            /// <value>
            /// The default name of the parameter.
            /// </value>
            public string DefaultParameterName => $"{_entityType.Name}Id";
        }
    }
}