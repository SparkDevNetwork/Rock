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

namespace Rock.ViewModels.Blocks.Internal
{
    /// <summary>
    /// This defines the structure of the detail block box for the properties
    /// that do not require any generic type definition. It is used by the
    /// framework and should not be used by plugins as its structure may
    /// change without warning.
    /// </summary>
    public interface IDetailBlockBox : IBlockBox
    {
        /// <summary>
        /// Gets or sets the property names that are referenced by any attribute
        /// qualifiers which might require that attributes be refreshed when they
        /// are modified.
        /// </summary>
        /// <value>An array of strings.</value>
        List<string> QualifiedAttributeProperties { get; set; }

        /// <summary>
        /// Gets or sets the name of the type of entity being viewed or edited.
        /// </summary>
        /// <value>The name of the type of entity.</value>
        string EntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the type of entity being
        /// viewed or edited.
        /// </summary>
        /// <value>The entity type unique identifier.</value>
        Guid? EntityTypeGuid { get; set; }
    }
}
