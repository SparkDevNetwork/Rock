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
using Rock.ViewModels.CheckIn.Labels;
using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.LabelDesigner
{
    /// <summary>
    /// Represents a single check-in label that will be edited in the designer
    /// block.
    /// </summary>
    public class LabelDetailBag
    {
        /// <summary>
        /// The label and field details of a designed label.
        /// </summary>
        public DesignedLabelBag LabelData { get; set; }

        /// <summary>
        /// The ruleset that will be used to determine if this label should be
        /// printed out during check-in.
        /// </summary>
        public FieldFilterGroupBag ConditionalVisibility { get; set; }
    }
}
