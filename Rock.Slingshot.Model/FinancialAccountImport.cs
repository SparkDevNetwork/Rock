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
namespace Rock.Slingshot.Model
{
    public class FinancialAccountImport
    {
        /// <summary>
        /// Gets or sets the financial account foreign identifier.
        /// </summary>
        /// <value>
        /// The financial account foreign identifier.
        /// </value>
        public int FinancialAccountForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tax deductible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tax deductible; otherwise, <c>false</c>.
        /// </value>
        public bool IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the parent financial account foreign identifier.
        /// </summary>
        /// <value>
        /// The parent financial account foreign identifier.
        /// </value>
        public int? ParentFinancialAccountForeignId { get; set; }
    }
}
