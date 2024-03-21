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

using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Identifies a single form field that has been placed on the form.
    /// </summary>
    public class FormFieldViewModel
    {
        /// <summary>
        /// The unique identifier for this form field.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The unique identifier of the field type used to identify this field.
        /// </summary>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// The unique identifier of the field type used to render the edit
        /// control of this field if it is a universal type.
        /// </summary>
        public Guid? UniversalFieldTypeGuid { get; set; }

        /// <summary>
        /// The display name of this field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The descriptive help text that will be rendered along with the name.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The unique key used to identify this field in Lava operations.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The width of this field in display columns. This should be a value
        /// between 1 and 12 inclusive.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Determines if this field will be considered required in order to
        /// submit the form.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Determines if the label (name) should be hidden when this field is
        /// displayed.
        /// </summary>
        public bool IsHideLabel { get; set; }

        /// <summary>
        /// Determines if this field will be included in the results grid
        /// displayed to staff when examining the submissions.
        /// </summary>
        public bool IsShowOnGrid { get; set; }

        /// <summary>
        /// The configuration values that have been set for this field.
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; set; }

        /// <summary>
        /// The configuration values for the field's edit mode.
        /// </summary>
        public Dictionary<string, string> EditConfigurationValues { get; set; }

        /// <summary>
        /// The rule that controls when this field is visible.
        /// </summary>
        public FieldFilterGroupBag VisibilityRule { get; set; }

        /// <summary>
        /// The default value that will be used when the field is initially
        /// displayed.
        /// </summary>
        public string DefaultValue { get; set; }
    }
}
