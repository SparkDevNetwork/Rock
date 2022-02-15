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

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Identifies a single attribute that was created outside the form and
    /// is available for use by the form.
    /// </summary>
    public class FormOtherAttributeViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the attribute.
        /// </summary>
        /// <value>The unique identifier of the attribute.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the field type used by the
        /// attribute.
        /// </summary>
        /// <value>The unique identifier of the field type.</value>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <value>The name of the attribute.</value>
        public string Name { get; set; }
    }
}
