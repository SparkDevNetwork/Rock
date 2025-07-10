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

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// Defines a single section that will wrap a number of fields on a
    /// workflow entry form.
    /// </summary>
    public class EntryFormSectionBag
    {
        /// <summary>
        /// The identifier of this section.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The title to display at the top of the section header.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The text to display beneath the title in the section header.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines if the separator bar should be visible between the
        /// section header and content.
        /// </summary>
        public bool IsHeadingSeparatorVisible { get; set; }

        /// <summary>
        /// The CSS class to apply to the div element that contains the section.
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// The main rule that describes if this field should be visible or not
        /// depending on the values of other fields.
        /// </summary>
        public FieldFilterGroupBag VisibilityRule { get; set; }
    }
}
