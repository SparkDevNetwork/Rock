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

namespace Rock.Apps.StatementGenerator.Client
{
    /// <summary>
    /// Pledge Settings related to the Statement Generator
    /// </summary>
    public partial class FinancialStatementTemplatePledgeSettings
    {
        /// <summary />
        public List<int> AccountIds { get; set; }

        /// <summary />
        public bool IncludeGiftsToChildAccounts { get; set; }

        /// <summary />
        public bool IncludeNonCashGifts { get; set; }

        /// <summary>
        /// Copies the base properties from a source FinancialStatementTemplatePledgeSettings object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( FinancialStatementTemplatePledgeSettings source )
        {
            this.AccountIds = source.AccountIds;
            this.IncludeGiftsToChildAccounts = source.IncludeGiftsToChildAccounts;
            this.IncludeNonCashGifts = source.IncludeNonCashGifts;

        }
    }
}
