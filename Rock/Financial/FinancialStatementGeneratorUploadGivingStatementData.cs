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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Financial;

namespace Rock.Financial
{
    /// <summary>
    /// Request Body for api/FinancialGivingStatement/UploadGivingStatementDocument
    /// </summary>
    [RockClientInclude( "Request Body for api/FinancialGivingStatement/UploadGivingStatementDocument" )]
    public class FinancialStatementGeneratorUploadGivingStatementData
    {
        /// <summary>
        /// Gets or sets the financial statement individual save options.
        /// </summary>
        /// <value>
        /// The financial statement individual save options.
        /// </value>
        public FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions FinancialStatementIndividualSaveOptions { get; set; }

        /// <summary>
        /// The financial statement generator recipient
        /// </summary>
        public FinancialStatementGeneratorRecipient FinancialStatementGeneratorRecipient { get; set; }

        /// <summary>
        /// Gets or sets the PDF data.
        /// </summary>
        /// <value>
        /// The PDF data.
        /// </value>
        public byte[] PDFData { get; set; }
    }
}
