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

using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel
{
    public abstract class ViewModelBase : IViewModelWithAttributes
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>
        /// The Id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Guid.
        /// </summary>
        /// <value>
        /// The Guid.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [TypeScriptType( "Record<string, ClientAttributeValue> | null", "import { ClientAttributeValue } from './clientAttributeValue';" )]
        public Dictionary<string, PublicAttributeValueViewModel> Attributes { get; set; }
    }
}
