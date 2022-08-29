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
using Rock.Enums.Controls;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetDataViews API action of
    /// the DataViewPicker control.
    /// </summary>
    public class MetricCategoryPickerGetChildrenOptionsBag
    {
        /// <summary>
        /// Gets or sets the parent unique identifier whose children are to
        /// be retrieved. If null then the root items are being requested.
        /// </summary>
        /// <value>The parent unique identifier.</value>
        public Guid? ParentGuid { get; set; }

        /// <summary>
        /// Gets or sets the default icon CSS class to use for items that do not
        /// specify their own IconCssClass value.
        /// </summary>
        /// <value>The default icon CSS class.</value>
        public string DefaultIconCssClass { get; set; } = "fa fa-bar-chart-o";

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}

