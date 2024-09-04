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

namespace Rock.ViewModels.Blocks.Reporting.DynamicData
{
    /// <summary>
    /// The settings that will be edited in the custom settings panel for the dynamic data block.
    /// </summary>
    public class DynamicDataCustomSettingsBag
    {
        #region Shared Settings (shared between grid and Lava results formatting display modes).

        /// <summary>
        /// Gets or sets whether to enable the updating of the parent page's name and description.
        /// </summary>
        public bool EnablePageUpdate { get; set; }

        /// <summary>
        /// Gets or sets the page name.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Gets or sets the page description.
        /// </summary>
        public string PageDescription { get; set; }

        /// <summary>
        /// Gets or sets the SQL query or stored procedure name to execute.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets whether the query represents the name of a stored procedure to execute.
        /// </summary>
        public bool IsStoredProcedure { get; set; }

        /// <summary>
        /// Gets or sets the parameters required by the query or stored procedure.
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Gets or sets the amount of time in seconds to allow the query to run before timing out.
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets how the results should be displayed.
        /// </summary>
        public string ResultsDisplayMode { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of field names that need to be decrypted before displaying their value.
        /// </summary>
        public string EncryptedFields { get; set; }

        /// <summary>
        /// Gets or sets optional Lava for setting the page title.
        /// </summary>
        public string PageTitleLava { get; set; }

        #endregion Shared Settings (shared between grid and Lava results formatting display modes).

        #region Results Formatting - Grid Settings

        /// <summary>
        /// Gets or sets configuration objects describing how each column of the grid
        /// results should be displayed, as well as rules for filtering, exporting, Etc.
        /// </summary>
        public List<ColumnConfigurationBag> ColumnConfigurations { get; set; }

        /// <summary>
        /// Gets or sets whether to show the checkbox select column on the grid as the first column.
        /// </summary>
        public bool ShowCheckboxSelectionColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to disable paging on the grid.
        /// </summary>
        public bool DisablePaging { get; set; }

        /// <summary>
        /// Gets or sets whether the query returns a list of people.
        /// </summary>
        public bool IsPersonReport { get; set; }

        /// <summary>
        /// Gets or sets whether the header on the grid will be sticky at the top of the page.
        /// </summary>
        public bool EnableStickyHeader { get; set; }

        /// <summary>
        /// Gets or sets whether to show 'Export to Excel' and 'Export to CSV' buttons in the grid.
        /// </summary>
        public bool EnableExport { get; set; }

        /// <summary>
        /// Gets or sets whether to show 'Merge Template' button in the grid.
        /// </summary>
        public bool EnableMergeTemplate { get; set; }

        /// <summary>
        /// Gets or sets whether to show 'Communicate' button in the grid.
        /// </summary>
        public bool EnableCommunications { get; set; }

        /// <summary>
        /// Gets or sets whether to show 'Merge Person Records' button in the grid.
        /// </summary>
        public bool EnablePersonMerge { get; set; }

        /// <summary>
        /// Gets or sets whether to show 'Bulk Update' button in the grid.
        /// </summary>
        public bool EnableBulkUpdate { get; set; }

        /// <summary>
        /// Get or sets whether to show 'Launch Workflow' button in the grid.
        /// </summary>
        public bool EnableLaunchWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the grid's title.
        /// </summary>
        public string GridTitle { get; set; }

        /// <summary>
        /// Gets or sets the URL to redirect individual to when they click on a row in the grid.
        /// </summary>
        public string SelectionUrl { get; set; }

        /// <summary>
        /// Gets or sets the column name(s) that should be available to use as merge fields for the communication.
        /// </summary>
        public string CommunicationMergeFields { get; set; }

        /// <summary>
        /// Gets or sets the column name(s) that contain a person ID field to use as the recipient for a communication.
        /// </summary>
        public string CommunicationRecipientFields { get; set; }

        /// <summary>
        /// Gets or sets the Lava template to be rendered above the grid.
        /// </summary>
        public string GridHeaderContent { get; set; }

        /// <summary>
        /// Gets or set the Lava template to be rendered below the grid.
        /// </summary>
        public string GridFooterContent { get; set; }

        #endregion Results Formatting - Grid Settings

        #region Results Formatting - Lava Settings

        /// <summary>
        /// Gets or sets the formatting to apply to the returned results.
        /// </summary>
        public string LavaTemplate { get; set; }

        #endregion Results Formatting - Lava Settings
    }
}
