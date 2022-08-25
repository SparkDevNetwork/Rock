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

using System.Collections.Generic;

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Class EntityBagBase.
    /// Implements the <see cref="Rock.ViewModels.Utility.IViewModelWithAttributes" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.IViewModelWithAttributes" />
    public abstract class EntityBagBase : IViewModelWithAttributes
    {
        /// <summary>
        /// Gets or sets the identifier key of this entity.
        /// </summary>
        /// <value>
        /// The identifier key of this entity.
        /// </value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [TypeScriptType( "Record<string, PublicAttribute> | null", "import { PublicAttribute } from './publicAttribute';" )]
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
