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

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// All the settings related to a single section on the form.
    /// </summary>
    public class FormSectionViewModel
    {
        /// <summary>
        /// The unique identifier of this section.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The title that will be displayed above this section.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The additional descriptive text that will be displayed under
        /// the title.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines if the heading separator will be visible when the form
        /// is displayed.
        /// </summary>
        public bool ShowHeadingSeparator { get; set; }

        /// <summary>
        /// The unique identifier of the type that controls how the section
        /// is rendered.
        /// </summary>
        public Guid? Type { get; set; }

        /// <summary>
        /// The list of fields that are contained within this section.
        /// </summary>
        public List<FormFieldViewModel> Fields { get; set; }
    }
}
