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

using Rock.Enums.CheckIn.Labels;
using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.LabelDesigner
{
    /// <summary>
    /// The configuration options for the label designer block.
    /// </summary>
    public class LabelDesignerOptionsBag
    {
        /// <summary>
        /// The identifier for the check-in label that is being designed.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Determines if the label is a system label. If so it can not be
        /// saved, but they can look around and run preview tests.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// The data that describes the check-in label that is being designed.
        /// </summary>
        public LabelDetailBag Label { get; set; }

        /// <summary>
        /// The name of the label being designed.
        /// </summary>
        public string LabelName { get; set; }

        /// <summary>
        /// The type of label being designed.
        /// </summary>
        public LabelType LabelType { get; set; }

        /// <summary>
        /// The list of data sources that are available to pick from when
        /// editing a dynamic text field.
        /// </summary>
        public List<DataSourceBag> DataSources { get; set; }

        /// <summary>
        /// The filter sources that are available to pick from when editing
        /// a field filter or the label filter.
        /// </summary>
        public List<FieldFilterSourceBag> FilterSources { get; set; }

        /// <summary>
        /// The list of icons that are available to pick from when editing an
        /// icon field.
        /// </summary>
        public List<IconItemBag> Icons { get; set; }

        /// <summary>
        /// The URL to return to when the cancel button is clicked.
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}
