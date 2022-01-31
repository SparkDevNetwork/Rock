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
using System.Runtime.Serialization;
using Rock.Lava;

namespace Rock.Crm.ConnectionStatusChangeReport
{
    /// <summary>
    /// A Lava data object that holds summary information about Group Membership for a given time period.
    /// </summary>
    [Serializable]
    [LavaType]
    [DotLiquid.LiquidType]
    public class ConnectionStatusChangeReportData
    {
        /// <summary>
        /// Gets or sets the settings for this report.
        /// </summary>
        [DataMember]
        public ConnectionStatusChangeReportSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the start date of the summary period.
        /// </summary>
        [DataMember]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the summary period.
        /// </summary>
        [DataMember]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The connection status change events contained in the report.
        /// </summary>
        [DataMember]
        public List<ConnectionStatusChangeEventInfo> ChangeEvents { get; set; }

        /// <summary>
        /// Gets a description of the reporting period.
        /// </summary>
        /// <returns></returns>
        public string GetPeriodDescription()
        {
            return string.Format("{0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}", this.StartDate, this.EndDate );
        }

        /// <summary>
        /// Gets a description of the report settings.
        /// </summary>
        /// <returns></returns>
        public string GetSettingsDescription()
        {
            return Settings.ToString();
        }
    }
}