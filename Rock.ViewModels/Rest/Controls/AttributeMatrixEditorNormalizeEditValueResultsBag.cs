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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The results for the NormalizeNewValue API action of the AttributeMatrixEditor control.
    /// It contains the newly retrieved viewing values along with the edit values that were sent.
    /// </summary>
    public class AttributeMatrixEditorNormalizeEditValueResultsBag
    {
        /// <summary>
        /// A list of the attribute values (public viewing values) for the attributes of the
        /// new matrix item. Key is the attribute name.
        /// </summary>
        public Dictionary<string, string> ViewValues { get; set; }

        /// <summary>
        /// A list of the attribute values (public edit values) for the attributes of the
        /// new matrix item. Key is the attribute name.
        /// </summary>
        public Dictionary<string, string> EditValues { get; set; }
    }
}
