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

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// The primary view model that contains all the runtime information needed
    /// by the FormBuilder block.
    /// </summary>
    public class FormBuilderDetailViewModel
    {
        /// <summary>
        /// The URL to redirect the individual to when the Submissions tab is
        /// clicked.
        /// </summary>
        public string SubmissionsPageUrl { get; set; }

        /// <summary>
        /// The URL to redirect the individual to when the Analytics tab is
        /// clicked.
        /// </summary>
        public string AnalyticsPageUrl { get; set; }

        /// <summary>
        /// The source of information for various pickers and controls.
        /// </summary>
        public FormValueSourcesViewModel Sources { get; set; }

        /// <summary>
        /// The unique identifier of the form being edited.
        /// </summary>
        public Guid FormGuid { get; set; }

        /// <summary>
        /// The details about the form that is to be edited.
        /// </summary>
        public FormSettingsViewModel Form { get; set; }

        /// <summary>
        /// Gets or sets the other attributes that are available to the workflow
        /// form while running. These include special attributes as well as
        /// user-defined attributes.
        /// </summary>
        public List<FormOtherAttributeViewModel> OtherAttributes { get; set; }
    }
}
