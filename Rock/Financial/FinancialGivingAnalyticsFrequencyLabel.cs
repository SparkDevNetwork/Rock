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
using System.ComponentModel;

namespace Rock.Financial
{
    /// <summary>
    /// Frequency Labels:
    /// <list>
    /// <item>
    ///      Weekly = Avg days between 4.5 - 8.5; Std Dev &lt; 7;
    /// </item>
    /// <item>
    ///      Bi-Weekly = Avg days between 9 - 17; Std Dev &lt; 10;
    /// </item>
    /// <item>   
    ///      Monthly = Avg days between 25 - 35; Std Dev &lt; 10;
    /// </item>
    /// <item>
    ///      Quarterly = Avg days between 80 - 110; Std Dev &lt; 15;
    /// </item>
    /// <item>
    ///      Erratic = Freq Avg / 2 &lt; Std Dev
    /// </item>
    /// <item>
    ///      Undetermined = Everything else
    ///      </item>
    /// </list>
    /// </summary>
    public enum FinancialGivingAnalyticsFrequencyLabel
    {
        /// <summary>
        /// Weekly Avg days between 4.5 - 8.5; Std Dev &lt; 7;
        /// </summary>
        [Description( "Weekly" )]
        Weekly = 1,

        /// <summary>
        /// Bi-Weekly: Avg days between 9 - 17; Std Dev &lt; 10;
        /// </summary>
        [Description( "Bi-Weekly" )]
        BiWeekly = 2,

        /// <summary>
        /// Monthly: Avg days between 25 - 35; Std Dev &lt; 10;
        /// </summary>
        [Description( "Monthly" )]
        Monthly = 3,

        /// <summary>
        /// Quarterly - Avg days between 80 - 110; Std Dev &lt; 15;
        /// </summary>
        [Description( "Quarterly" )]
        Quarterly = 4,

        /// <summary>
        /// Erratic: Freq Avg / 2 &lt; Std Dev
        /// </summary>
        [Description( "Erratic" )]
        Erratic = 5,

        /// <summary>
        ///  Undetermined: Everything else
        /// </summary>
        [Description( "Undetermined" )]
        Undetermined = 6
    }
}
