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

using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// The additional configuration options for the Benevolence Request Detail block.
    /// </summary>
    public class BenevolenceRequestDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for the case worker role attribute.
        /// </summary>
        public List<ListItemBag> CaseWorkersByRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the country code attribute should be displayed.
        /// </summary>
        public bool DisplayCountryCodeAttribute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the government ID attribute should be displayed.
        /// </summary>
        public bool DisplayGovernmentIdAttribute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the middle name attribute should be displayed.
        /// </summary>
        public bool DisplayMiddleNameAttribute { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the benevolence request statement page attribute.
        /// </summary>
        public Guid BenevolenceRequestStatementPageAttribute { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the workflow detail page attribute.
        /// </summary>
        public Guid WorkflowDetailPageAttribute { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the workflow entry page attribute.
        /// </summary>
        public Guid WorkflowEntryPageAttribute { get; set; }

        /// <summary>
        /// Gets or sets the race option attribute value.
        /// </summary>
        public string RaceOptionAttribute { get; set; }

        /// <summary>
        /// Gets or sets the ethnicity option attribute value.
        /// </summary>
        public string EthnicityOptionAttribute { get; set; }

        /// <summary>
        /// Gets or sets the benevolence type.
        /// </summary>
        public List<ListItemBag> BenevolenceRequestTypes { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DefinedValue representing the Benevolence Request's status.
        /// </summary>
        public List<ListItemBag> RequestStatusValues { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DefinedValue representing the Requester's connection status.
        /// </summary>
        public List<ListItemBag> ConnectionStatusValues { get; set; }

        /// <summary>
        /// Gets or sets the collection of result type values.
        /// </summary>
        public List<ListItemBag> ResultTypeValues { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the binary file type associated with benevolence documents.
        /// </summary>
        public System.Guid BenevolenceDocumentBinaryFileTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether country codes are enabled.
        /// </summary>
        public bool CountryCodesEnabled { get; set; }
    }
}