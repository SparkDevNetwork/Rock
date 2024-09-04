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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The data used by the Obsidian Matrix Field Type to be able to edit an AttributeMatrix
    /// </summary>
    public class MatrixFieldDataBag
    {
        /// <summary>
        /// Unique identifier of the currently saved AttributeMatrix
        /// </summary>
        public Guid? AttributeMatrixGuid { get; set; } = null;

        /// <summary>
        /// List of representations of AttributeMatrixItems in this AttributeMatrix
        /// </summary>
        public List<AttributeMatrixEditorPublicItemBag> MatrixItems { get; set; }

        /// <summary>
        /// Public configuration data for the attributes of the matrix items
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Default values for each of the different values
        /// </summary>
        public Dictionary<string, string> DefaultAttributeValues { get; set; }

        /// <summary>
        /// Minimum number of matrix items needed to be valid
        /// </summary>
        public int? MinRows { get; set; }

        /// <summary>
        /// Maximum number of matrix items allowed to be valid
        /// </summary>
        public int? MaxRows { get; set; }
    }
}
