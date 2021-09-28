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
    /// Response from api/FinancialGivingStatement/UploadGivingStatementDocument
    /// </summary>
    [RockClientInclude( "Result response from api/FinancialGivingStatement/UploadGivingStatementDocument" )]
    public class FinancialStatementGeneratorUploadGivingStatementResult
    {

        /// <summary>
        /// Gets or sets the number of individuals that received the document based on the 
        /// <see cref="FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions.FinancialStatementIndividualSaveOptionsSaveFor"/> option
        /// </summary>
        /// <value>
        /// The number of individuals.
        /// </value>
        public int NumberOfIndividuals { get; set; }
    }
}
