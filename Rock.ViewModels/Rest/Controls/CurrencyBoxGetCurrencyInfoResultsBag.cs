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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The results returned by the GetCurrencyInfo API action of the CurrencyBox control.
    /// </summary>
    public class CurrencyBoxGetCurrencyInfoResultsBag
    {
        /// <summary>
        /// The symbol used to represent the currency
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The number of decimal places used with the currency
        /// </summary>
        public int DecimalPlaces { get; set; }
    }
}
