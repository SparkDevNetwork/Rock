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

using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// Custom attribute used to decorate Rock Blocks that require context.  If entity type is not 
    /// included in the attribute, a block property will automatically be added for user to set 
    /// the entity type when block is placed on a page
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ContextAwareAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType { get; set; }
        
        /// <summary>
        /// Gets the default name of the parameter.
        /// </summary>
        /// <value>
        /// The default name of the parameter.
        /// </value>
        public string DefaultParameterName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this ContextAware attribute is configurable in BlockProperties
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is configurable; otherwise, <c>false</c>.
        /// </value>
        public bool IsConfigurable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareAttribute" /> class.
        /// </summary>
        public ContextAwareAttribute()
        {
            IsConfigurable = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareAttribute" /> class.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        public ContextAwareAttribute( Type entityType )
        {
            EntityType = EntityTypeCache.Get( entityType );
            DefaultParameterName = entityType.Name + "Id";
        }
    }
}