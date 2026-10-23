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

using Rock.Model;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// A single field collected by a registrant form.
    /// </summary>
    public class RegistrationTemplateFormFieldBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the field. Visibility rules of other
        /// fields reference this identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display order of the field within its form.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets where the value of the field comes from.
        /// </summary>
        public RegistrationFieldSource FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the person field that is collected. Only meaningful when the
        /// field source is a person field.
        /// </summary>
        public RegistrationPersonFieldType PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets the person or group member attribute that is collected. The value
        /// is the unique identifier of the attribute. Only meaningful for the person
        /// attribute and group member attribute field sources.
        /// </summary>
        public ListItemBag Attribute { get; set; }

        /// <summary>
        /// Gets or sets the registrant attribute definition that is collected. Only
        /// meaningful for the registrant attribute field source.
        /// </summary>
        public PublicEditableAttributeBag RegistrantAttribute { get; set; }

        /// <summary>
        /// Gets or sets the display name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the field type name of the attribute backing the field. Empty
        /// for person fields.
        /// </summary>
        public string FieldTypeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is only visible internally.
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value entered for the first
        /// registrant is used for every additional registrant.
        /// </summary>
        public bool IsSharedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person's current value is displayed
        /// when they register.
        /// </summary>
        public bool ShowCurrentValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value is required when registering.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value is displayed on the list of registrants.
        /// </summary>
        public bool IsGridField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is shown to a person registering on the wait list.
        /// </summary>
        public bool ShowOnWaitlist { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field cannot be edited when the
        /// person already has a value.
        /// </summary>
        public bool IsLockedIfValuesExist { get; set; }

        /// <summary>
        /// Gets or sets the HTML rendered before the field.
        /// </summary>
        public string PreText { get; set; }

        /// <summary>
        /// Gets or sets the HTML rendered after the field.
        /// </summary>
        public string PostText { get; set; }

        /// <summary>
        /// Gets or sets the rules that determine when the field is visible. Each rule's
        /// attribute unique identifier is the unique identifier of the form field being
        /// compared against.
        /// </summary>
        public FieldFilterGroupBag VisibilityRules { get; set; }
    }
}
