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

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder.FormTemplateDetail
{
    /// <summary>
    /// Configuration data for the Form Template Detail block.
    /// </summary>
    public class FormTemplateDetailConfiguration
    {
        /// <summary>
        /// Gets or sets the source of information for various pickers and controls.
        /// </summary>
        /// <value>The source of information for various pickers and controls.</value>
        public ValueSourcesViewModel Sources { get; set; }

        /// <summary>
        /// Gets or sets the URL of the parent page.
        /// </summary>
        /// <value>The URL of the parent page.</value>
        public string ParentUrl { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the individual should be
        /// allowed to enter edit mode.
        /// </summary>
        /// <value>A value that indicates if the template can be edited. </value>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the template being edited.
        /// </summary>
        /// <value>The unique identifier of the template being edited.</value>
        public Guid? TemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the details about the template being viewed.
        /// </summary>
        /// <value>The details abou tthe template being viewed.</value>
        public object Template { get; set; }
    }
}
