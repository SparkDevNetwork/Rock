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

using System.ComponentModel.DataAnnotations.Schema;
using Rock.Financial;

namespace Rock.Model
{
    public partial class FinancialStatementTemplate
    {
        /// <summary>
        /// Gets or sets the report setting.
        /// </summary>
        /// <value>
        /// The report setting.
        /// </value>
        [NotMapped]
        public virtual FinancialStatementTemplateReportSettings ReportSettings { get; set; } = new FinancialStatementTemplateReportSettings();

        /// <summary>
        /// Gets or sets the footer settings.
        /// </summary>
        /// <value>
        /// The footer settings.
        /// </value>
        [NotMapped]
        public virtual FinancialStatementTemplateHeaderFooterSettings FooterSettings { get; set; } = new FinancialStatementTemplateHeaderFooterSettings();
    }
}
