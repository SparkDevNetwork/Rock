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
    /// TODO
    /// </summary>
    public class MatrixFieldDataBag
    {
        /// <summary>
        /// TODO
        /// </summary>
        public Guid? AttributeMatrixGuid { get; set; } = null;

        /// <summary>
        /// TODO
        /// </summary>
        public List<AttributeMatrixEditorPublicItemBag> MatrixItems { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public Dictionary<string, string> DefaultAttributeValues { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public int? MinRows { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public int? MaxRows { get; set; }
    }
}
