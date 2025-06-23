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

using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// Defines a single field to display on a workflow entry form.
    /// </summary>
    public class EntryFormFieldBag
    {
        /// <summary>
        /// Gets or sets the attribute associated with this field.
        /// </summary>
        /// <value>
        /// The attribute associated with this field.
        /// </value>
        public PublicAttributeBag Attribute { get; set; }

        /// <summary>
        /// Determines if this field is required in order for the form to be
        /// submitted.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Determines if this field's value should be displayed without any
        /// label above.
        /// </summary>
        public bool IsLabelHidden { get; set; }

        /// <summary>
        /// The identifier of the section to display this field in.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// The size of the column (1-12) to use when displaying this field.
        /// </summary>
        public int? ColumnSize { get; set; }

        /// <summary>
        /// Contains any HTML that should be rendered before the field.
        /// </summary>
        public string PreHtml { get; set; }

        /// <summary>
        /// Contains any HTML that should be rendered after the field.
        /// </summary>
        public string PostHtml { get; set; }

        /// <summary>
        /// The main rule that describes if this field should be visible or not
        /// depending on the values of other fields.
        /// </summary>
        public FieldFilterGroupBag VisibilityRule { get; set; }
    }
}
