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
    /// Identifies a single field type that can be used when designing the form.
    /// </summary>
    public class FormFieldTypeViewModel
    {
        /// <summary>
        /// The unique identifier of the field type.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The text that represents the display name of the field type.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The CSS classes that are used to provide an iconic representation
        /// of this field type.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Determines if this field type is considered common and should be
        /// made readily accessible.
        /// </summary>
        public bool IsCommon { get; set; }
    }
}
