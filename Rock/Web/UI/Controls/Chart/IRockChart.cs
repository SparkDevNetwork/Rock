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
using Rock.Attribute;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Represents a chart that provides a graphical representation of Rock data.
    /// </summary>
    [RockInternal( "1.14" )]
    public interface IRockChart
    {
        #region Events

        /// <summary>
        /// Occurs when the chart is clicked.
        /// </summary>
        event EventHandler<ChartClickArgs> ChartClick;

        #endregion

        /// <summary>
        /// Gets or sets a flag indicating if the chart is visible.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the ratio of height to width is maintained when the chart is resized.
        /// </summary>
        bool MaintainAspectRatio { get; set; }
    }
}
