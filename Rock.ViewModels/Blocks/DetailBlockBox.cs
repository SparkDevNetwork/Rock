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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks
{
    /// <summary>
    /// The information required to render a standard detail block.
    /// </summary>
    /// <typeparam name="TEntityBag">The type of the bag Entity.</typeparam>
    /// <typeparam name="TOptions">The type of the bag options.</typeparam>
    /// <seealso cref="Rock.ViewModels.Utility.IValidPropertiesBox" />
    public class DetailBlockBox<TEntityBag, TOptions> : BlockBox, IValidPropertiesBox, Internal.IDetailBlockBox
        where TOptions : new()
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public TEntityBag Entity { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        public TOptions Options { get; set; } = new TOptions();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value><c>true</c> if this instance is editable; otherwise, <c>false</c>.</value>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the valid properties.
        /// </summary>
        /// <value>The valid properties.</value>
        public List<string> ValidProperties { get; set; }

        /// <summary>
        /// Gets or sets the property names that are referenced by any attribute
        /// qualifiers which might require that attributes be refreshed when they
        /// are modified.
        /// </summary>
        public List<string> QualifiedAttributeProperties { get; set; }

        /// <summary>
        /// Gets or sets the name of the type of entity being viewed or edited.
        /// </summary>
        /// <value>The name of the type of entity.</value>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the type of entity being
        /// viewed or edited.
        /// </summary>
        /// <value>The entity type unique identifier.</value>
        public Guid? EntityTypeGuid { get; set; }
    }
}
