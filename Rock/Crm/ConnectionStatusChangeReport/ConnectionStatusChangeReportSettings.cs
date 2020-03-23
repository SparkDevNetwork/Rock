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

namespace Rock.Crm.ConnectionStatusChangeReport
{
    /// <summary>
    /// The settings used to build a ConnectionStatusChangeReport.
    /// </summary>
    [Serializable]
    public class ConnectionStatusChangeReportSettings
    {
        private TimePeriod _DateRange = new TimePeriod();

        /// <summary>
        /// Gets or sets the reporting period.
        /// </summary>
        public TimePeriod ReportPeriod
        {
            get
            {
                return _DateRange;
            }
            set
            {
                _DateRange = value ?? new TimePeriod();
            }
        }

        /// <summary>
        /// Gets or sets the identifier of the Campus that is the subject of this report.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the number of days of membership required for a person to be included in the report.
        /// </summary>
        public int? FromConnectionStatusId { get; set; }

        /// <summary>
        /// Gets or sets the number of days of membership required for a person to be included in the report.
        /// </summary>
        public int? ToConnectionStatusId { get; set; }
    }
}