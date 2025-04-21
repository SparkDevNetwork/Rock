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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options for the NormalizeNewValue API action of the AttributeMatrixEditor control.
    /// In this case, normalizing means we're taking the public edit values that we got from
    /// the "add" or "edit" forms of the matrix and converting them to the public value so
    /// they can be displayed correctly.
    /// </summary>
    public class AttributeMatrixEditorPublicItemBag
    {
        /// <summary>
        /// Unique identifier of the AttributeMatrixItem
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Order in which this item is displayed in a list of these items
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Only used on the client side as a temporary ID until the item is saved and gains a Guid.
        /// </summary>
        public int? Key { get; set; }

        /// <summary>
        /// A list of the attribute values (public edit values) of the AttributeMatrixItem. Key is the attribute name.
        /// </summary>
        public Dictionary<string, string> EditValues { get; set; }

        /// <summary>
        /// A list of the attribute values (public view values) of the AttributeMatrixItem. Key is the attribute name.
        /// </summary>
        public Dictionary<string, string> ViewValues { get; set; }
    }
}
