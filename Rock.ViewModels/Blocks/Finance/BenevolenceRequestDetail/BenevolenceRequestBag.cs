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
    /// The item details for the Benevolence Request Detail block.
    /// </summary>
    public class BenevolenceRequestBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the person who initiated the request.
        /// </summary>
        public PersonBag Requester { get; set; }

        /// <summary>
        /// Gets or sets the caseworker assigned to the current case.
        /// </summary>
        public PersonBag CaseWorker { get; set; }

        /// <summary>
        /// Gets or sets the benevolence type identifier.
        /// </summary>
        /// <value>
        /// The benevolence type identifier.
        /// </value>
        public int BenevolenceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value representing the status of the Benevolence Request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the status of the Benevolence Request.
        /// </value>
        public int? RequestStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the date that this benevolence request was entered.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this benevolence request was entered.
        /// </value>
        public DateTime RequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets the text/content of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the text/content of the request.
        /// </value>
        public string RequestText { get; set; }

        /// <summary>
        /// Gets or sets the summary of the request result.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the summary of the request result.
        /// </value>
        public string ResultSummary { get; set; }

        /// <summary>
        /// Gets or sets the provided next steps.
        /// </summary>
        /// <value>
        /// The provided next steps.
        /// </value>
        public string ProvidedNextSteps { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public CampusBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the collection of benevolence results.
        /// </summary>
        public List<BenevolenceResultBag> Results { get; set; }

        /// <summary>
        /// Gets or sets the collection of documents associated with the request,
        /// represented as a list of <see cref="ListItemBag"/> objects.
        /// </summary>
        public List<BenevolenceDocumentBag> RequestDocuments { get; set; }

        /// <summary>
        /// Gets or sets the collection of workflow options available for this request.
        /// </summary>
        public List<BenevolenceRequestWorkflowBag> Workflows { get; set; }
    }
}
