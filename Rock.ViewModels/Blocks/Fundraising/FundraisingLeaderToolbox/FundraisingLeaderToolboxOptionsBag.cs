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

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingLeaderToolbox
{
    /// <summary>
    /// The additional configuration options for the Fundraising Leader Toolbox block.
    /// </summary>
    public class FundraisingLeaderToolboxOptionsBag
    {
        /// <summary>
        /// Gets or sets the error message that prevents the block from being displayed.
        /// When set, the summary panel and grid are hidden and only this message is shown
        /// (for example, when no group was supplied, the group could not be found, or the
        /// current person is not a leader of the opportunity).
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the resolved Summary Lava Template HTML displayed at the top of the
        /// main panel (opportunity title, dates, location, and summary).
        /// </summary>
        public string SummaryHtml { get; set; }

        /// <summary>
        /// Gets or sets the URL of the opportunity photo displayed in the left sidebar.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the opportunity group, used as the grid export file name.
        /// </summary>
        public string GroupName { get; set; }
    }
}
