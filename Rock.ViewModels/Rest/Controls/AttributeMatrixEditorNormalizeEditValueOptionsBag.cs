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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options for the NormalizeNewValue API action of the AttributeMatrixEditor control.
    /// In this case, normalizing means we're taking the public edit values that we got from
    /// the "add" or "edit" forms of the matrix and converting them to the public value so
    /// they can be displayed correctly.
    /// </summary>
    public class AttributeMatrixEditorNormalizeEditValueOptionsBag
    {
        /// <summary>
        /// A list of the attribute values (public edit values) for the attributes of the
        /// new matrix item. Key is the attribute name.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Configuration information for the attributes that we're normalizing the value of.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }
    }
}
