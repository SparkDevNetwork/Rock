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

using System.Collections.Generic;

using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInLabelDetail
{
    /// <summary>
    /// The configuration options for the Check-in Label Detail block.
    /// </summary>
    public class CheckInLabelDetailOptionsBag
    {
        /// <summary>
        /// The filter sources available when editing an attendance label.
        /// </summary>
        public List<FieldFilterSourceBag> AttendanceLabelFilterSources { get; set; }

        /// <summary>
        /// The filter sources available when editing a family label.
        /// </summary>
        public List<FieldFilterSourceBag> FamilyLabelFilterSources { get; set; }

        /// <summary>
        /// The filter sources available when editing a person label.
        /// </summary>
        public List<FieldFilterSourceBag> PersonLabelFilterSources { get; set; }

        /// <summary>
        /// The filter sources available when editing a checkout label.
        /// </summary>
        public List<FieldFilterSourceBag> CheckoutLabelFilterSources { get; set; }

        /// <summary>
        /// The filter sources available when editing a person location label.
        /// </summary>
        public List<FieldFilterSourceBag> PersonLocationLabelFilterSources { get; set; }
    }
}
