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

using Rock.Enums.CheckIn.Labels;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInLabelDetail
{
    /// <summary>
    /// A bag that contains the required information to render a check in label.
    /// </summary>
    public class CheckInLabelBag : EntityBagBase
    {
        /// <summary>
        /// The filter rules that dictate when this label will be printed.
        /// </summary>
        public FieldFilterGroupBag ConditionalPrintCriteria { get; set; }

        /// <summary>
        /// The text that describes the purpose of the label and what kind of
        /// information it shows.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// <para>
        /// A flag indicating if the label is active. An in-active label will
        /// still be shown in the list of existing labels to be printed, but
        /// will not be available when adding a new label to be printed to a
        /// group.
        /// </para>
        /// <para>
        /// In-active labels will not be printed.
        /// </para>
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// A flag indicating if this label is part of the Rock core
        /// system/framework. System labels cannot be edited or deleted.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// The format that the label's content is stored in. This determines
        /// what UI is displayed for editing the label as well as how the label
        /// is printed.
        /// </summary>
        public LabelFormat LabelFormat { get; set; }

        /// <summary>
        /// The label content to be edited. This is only valid if LabelFormat
        /// is ZPL.
        /// </summary>
        public string LabelContent { get; set; }

        /// <summary>
        /// The size of the label specified in inches in the format "WxH".
        /// </summary>
        public string LabelSize { get; set; }

        /// <summary>
        /// The type of label. Label types are used to determine what kind of data
        /// is available to the label and also how many instances of the label are
        /// generated.
        /// </summary>
        public LabelType LabelType { get; set; }

        /// <summary>
        /// The name of the check-in label that will be displayed in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The base-64 encoded image data that represents a preview of the
        /// label.
        /// </summary>
        public string PreviewImage { get; set; }
    }
}
