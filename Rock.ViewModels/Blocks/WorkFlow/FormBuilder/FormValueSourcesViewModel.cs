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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Represents the sources of truth for various pickers and lists of entities
    /// that will be used by the JavaScript code.
    /// </summary>
    public class FormValueSourcesViewModel
    {
        /// <summary>
        /// The list of campus topic options that are available to pick from.
        /// </summary>
        public List<ListItemBag> CampusTopicOptions { get; set; }

        /// <summary>
        /// The list of campus type options that are available to pick from.
        /// </summary>
        public List<ListItemBag> CampusTypeOptions { get; set; }

        /// <summary>
        /// The list of campus status options that are available to pick from.
        /// </summary>
        public List<ListItemBag> CampusStatusOptions { get; set; }

        /// <summary>
        /// The list of record status options that are available to pick from.
        /// </summary>
        public List<ListItemBag> RecordStatusOptions { get; set; }

        /// <summary>
        /// The list of record source options that are available to pick from.
        /// </summary>
        public List<ListItemBag> RecordSourceOptions { get; set; }

        /// <summary>
        /// The list of connection status options that are available to pick from.
        /// </summary>
        public List<ListItemBag> ConnectionStatusOptions { get; set; }

        /// <summary>
        /// The list of address type options that are available to pick from.
        /// </summary>
        public List<ListItemBag> AddressTypeOptions { get; set; }

        /// <summary>
        /// The list of e-mail template options that are available to pick from.
        /// </summary>
        public List<ListItemBag> EmailTemplateOptions { get; set; }

        /// <summary>
        /// The list of section type options that are available to pick from.
        /// </summary>
        public List<ListItemBag> SectionTypeOptions { get; set; }

        /// <summary>
        /// The list of field types that are available to pick from.
        /// </summary>
        public List<FormFieldTypeViewModel> FieldTypes { get; set; }

        /// <summary>
        /// The form templates that are available to pick from.
        /// </summary>
        public List<FormTemplateListItemViewModel> FormTemplateOptions { get; set; }

        /// <summary>
        /// Gets or sets the default type of the section to use when new sections
        /// are added to the form.
        /// </summary>
        public Guid? DefaultSectionType { get; set; }

        /// <summary>
        /// Gets or sets the list of pages that are available to pick from.
        /// </summary>
        public List<FormBuilderDetailLinkToFormBag> LinkToFormOptions { get; set; }
    }
}
