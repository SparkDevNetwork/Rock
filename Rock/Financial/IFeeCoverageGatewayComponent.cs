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
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// A Gateway that has a configurable Fee Coverage that helps cover ACH/CreditCard Processing fees
    /// </summary>
    public interface IFeeCoverageGatewayComponent
    {
        /// <summary>
        /// Gets the credit card fee coverage percentage.
        /// Use this percentage to increase the amount if the person wants to cover the processing fee of a Credit Card transaction.
        /// </summary>
        /// <value>
        /// The credit card fee coverage percentage.
        /// </value>
        decimal? GetCreditCardFeeCoveragePercentage( FinancialGateway financialGateway );

        /// <summary>
        /// Gets the ach fee coverage amount.
        /// Use this to increase the amount if the person wants to cover the processing fee of an ACH transaction.
        /// </summary>
        /// <value>
        /// The ach fee coverage amount.
        /// </value>
        decimal? GetACHFeeCoverageAmount( FinancialGateway financialGateway );
    }
}