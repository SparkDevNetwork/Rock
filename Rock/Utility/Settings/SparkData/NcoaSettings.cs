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

using Rock.Data;

namespace Rock.Utility.Settings.SparkData
{
    /// <summary>
    /// Settings for NCOA
    /// </summary>
    public class NcoaSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NcoaSettings"/> class.
        /// </summary>
        public NcoaSettings()
        {
            IsEnabled = false;
            RecurringEnabled = true;
            FileName = string.Empty;
            CurrentReportKey = string.Empty;
            CurrentReportStatus = string.Empty;
            IsAcceptedTerms = false;
            IsAckPrice = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the person full name.
        /// </summary>
        /// <value>
        /// The person full name.
        /// </value>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Gets or sets the last NCOA run date.
        /// </summary>
        /// <value>
        /// The last NCOA run date.
        /// </value>
        public DateTime? LastRunDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [recurring enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [recurring enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool RecurringEnabled { get; set; }

        /// <summary>
        /// Gets or sets the recurrence interval.
        /// </summary>
        /// <value>
        /// The recurrence interval.
        /// </value>
        public int RecurrenceInterval { get; set; } = 95;

        /// <summary>
        /// Gets or sets the name of the file on the NCOA server.
        /// </summary>
        /// <value>
        /// The name of the NCOA file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the current NCOA report key.
        /// </summary>
        /// <value>
        /// The current NCOA report key.
        /// </value>
        public string CurrentReportKey { get; set; }

        /// <summary>
        /// Gets or sets the current NCOA report export key.
        /// </summary>
        /// <value>
        /// The current NCOA report export key.
        /// </value>
        public string CurrentReportExportKey { get; set; }

        /// <summary>
        /// Gets or sets the current report status.
        /// </summary>
        /// <value>
        /// The current report status.
        /// </value>
        public string CurrentReportStatus { get; set; }

        /// <summary>
        /// Gets or sets the person data view Id.
        /// </summary>
        /// <value>
        /// The person data view Id.
        /// </value>
        public int? PersonDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the current upload count to NCOA.
        /// </summary>
        /// <value>
        /// The current upload count to NCOA.
        /// </value>
        public int? CurrentUploadCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is accepted terms.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is accepted terms; otherwise, <c>false</c>.
        /// </value>
        public bool IsAcceptedTerms { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is acknowledging price.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is acknowledging price; otherwise, <c>false</c>.
        /// </value>
        public bool IsAckPrice { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Inactive Record Reason <see cref="Rock.Model.DefinedValue"/> to use when inactivating people due to moving beyond the configured number of miles.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Inactive Record Reason <see cref="Rock.Model.DefinedValue"/> to use when inactivating people due to moving beyond the configured number of miles.
        /// </value>
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON )]
        public int? InactiveRecordReasonId { get; set; }

        /// <summary>
        /// Returns true if the NCOA Settings are valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            return InactiveRecordReasonId.HasValue &&
                InactiveRecordReasonId.Value != 0 &&
                PersonDataViewId.HasValue &&
                PersonDataViewId.Value != 0;
        }
    }
}
