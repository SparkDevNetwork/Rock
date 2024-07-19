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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Reporting.DynamicData
{
    /// <summary>
    /// A bag that contains the grid results for the dynamic data block.
    /// <para>
    /// This will contain the grid definition and information about enabled grid features, but will not
    /// contain grid data rows; that data should instead be loaded on demand by the Obsidian grid client.
    /// </para>
    /// </summary>
    public class GridResultsBag
    {
        /// <summary>
        /// Gets or sets the grid definition.
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the grid's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the field that will be used to uniquely identify each row.
        /// <remarks>
        /// This may be a simple field name (i.e. "id") or a dot-separated path to the
        /// key value within a particular field's object (i.e. "person.idKey").
        /// </remarks>
        /// </summary>
        public string KeyField { get; set; }

        /// <summary>
        /// Gets or sets whether to show the checkbox select column on the grid as the first column.
        /// </summary>
        public bool ShowCheckboxSelectionColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to disable paging on the grid.
        /// </summary>
        public bool DisablePaging { get; set; }

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
        /// Gets or sets whether the query returns a list of people.
        /// </summary>
        public bool IsPersonReport { get; set; }

        /// <summary>
        /// Gets or sets the field that will contain the person key.
        /// <remarks>
        /// This may be a simple field name (i.e. "id") or a dot-separated path to the
        /// key value within a particular field's object (i.e. "person.idKey").
        /// </remarks>
        /// </summary>
        public string PersonKeyField { get; set; }

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
        /// Gets or sets the column name(s) that should be available to use as merge fields for the communication.
        /// </summary>
        public List<string> CommunicationMergeFields { get; set; }

        /// <summary>
        /// Gets or sets the fields that contain a person key to use as the recipient for a communication.
        /// <remarks>
        /// These may be simple field names (i.e. "id") or dot-separated paths to the
        /// key values within each particular field's object (i.e. "person.idKey").
        /// </remarks>
        /// </summary>
        public List<string> CommunicationRecipientFields { get; set; }

        /// <summary>
        /// Gets or sets the Lava template to be rendered above the grid.
        /// </summary>
        public string GridHeaderHtml { get; set; }

        /// <summary>
        /// Gets or set the Lava template to be rendered below the grid.
        /// </summary>
        public string GridFooterHtml { get; set; }
    }
}
