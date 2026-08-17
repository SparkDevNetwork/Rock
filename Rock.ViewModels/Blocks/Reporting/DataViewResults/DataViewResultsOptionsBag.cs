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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Reporting.DataViewResults
{
    /// <summary>
    /// The additional configuration options for the Data View Results block.
    /// </summary>
    public class DataViewResultsOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block has a valid, viewable
        /// data view in scope and should therefore render its results panel. When
        /// <c>false</c> the block renders nothing.
        /// </summary>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data view's entity type is
        /// Person. When <c>true</c> the grid exposes the person toolbar features
        /// (communicate, merge, bulk update, etc.).
        /// </summary>
        public bool IsPersonDataSet { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the data view's entity type. Drives
        /// the grid's entity-type-aware actions (launch workflow, merge template) for
        /// every entity type, not just Person.
        /// </summary>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the grid field that holds the person key for
        /// each row. Only set when <see cref="IsPersonDataSet"/> is <c>true</c>.
        /// </summary>
        public string PersonKeyField { get; set; }

        /// <summary>
        /// Gets or sets the grid field names that identify communication recipients.
        /// Only set when <see cref="IsPersonDataSet"/> is <c>true</c>.
        /// </summary>
        public List<string> CommunicationRecipientFields { get; set; }

        /// <summary>
        /// Gets or sets the title used for the grid export (the data view name).
        /// </summary>
        public string ExportTitle { get; set; }

        /// <summary>
        /// Gets or sets the singular term used for a row in the grid (the entity's
        /// friendly name, e.g. "Person"). Drives the grid's empty state and counts.
        /// </summary>
        public string ItemTerm { get; set; }
    }
}
