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
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsDimPersonHistorical is SQL View based on AnalyticsSourcePersonHistorical
    /// and represents the historic and current records from AnalyticsSourcePersonHistorical
    /// </summary>
    [Table( "AnalyticsDimPersonHistorical" )]
    [DataContract]
    public class AnalyticsDimPersonHistorical : AnalyticsDimPersonBase<AnalyticsDimPersonHistorical>
    {
        // intentionally blank. See AnalyticsDimPersonBase, etc for the fields
    }

    /// <summary>
    /// *Another* Abstract Layer since AnalyticDimPersonHistorical and AnalyticsDimPersonCurrent share all the same fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Rock.Model.AnalyticsSourcePersonBase{T}" />
    public abstract class AnalyticsDimPersonBase<T> : AnalyticsSourcePersonBase<T>
        where T : AnalyticsDimPersonBase<T>, new()
    {
        #region Denormalized Lookup Values

        /// <summary>
        /// Gets or sets the marital status.
        /// </summary>
        /// <value>
        /// The marital status.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a Marital Status from MaritalStatusValueId
        [HideFromReporting]
        public string MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a Connection Status from ConnectionStatusValueId
        [HideFromReporting]
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the review reason.
        /// </summary>
        /// <value>
        /// The review reason.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a ReviewReason from ReviewReasonValueId
        [HideFromReporting]
        public string ReviewReason { get; set; }

        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        /// <value>
        /// The record status.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a RecordStatus from RecordStatusValueId
        [HideFromReporting]
        public string RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the record status reason.
        /// </summary>
        /// <value>
        /// The record status reason.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a RecordStatusReason from RecordStatusReasonValueId
        [HideFromReporting]
        public string RecordStatusReason { get; set; }

        /// <summary>
        /// Gets or sets the record type value.
        /// </summary>
        /// <value>
        /// The record type value.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a RecordType from RecordTypeValueId
        [HideFromReporting]
        public string RecordType { get; set; }

        /// <summary>
        /// Gets or sets the suffix value.
        /// </summary>
        /// <value>
        /// The suffix value.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a Suffix from SuffixValueId
        [HideFromReporting]
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the title value.
        /// </summary>
        /// <value>
        /// The title value.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already have a Title from TitleValueId
        [HideFromReporting]
        public string Title { get; set; }

        #endregion

        #region Enums as Text

        /// <summary>
        /// Gets or sets the gender text.
        /// </summary>
        /// <value>
        /// The gender text.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already show Text value of the Gender enum
        [HideFromReporting]
        public string GenderText { get; set; }

        /// <summary>
        /// Gets or sets the email preference text.
        /// </summary>
        /// <value>
        /// The email preference text.
        /// </value>
        [DataMember]
        // Hide from Reporting because Reporting will already show the Text value of EmailPreferenceEnum
        [HideFromReporting]
        public string EmailPreferenceText { get; set; }

        /// <summary>
        /// Gets or sets the primary family key which relates to the AnalyticsDimFamily tables/views
        /// </summary>
        /// <value>
        /// The primary family key.
        /// </value>
        [DataMember]
        public int? PrimaryFamilyKey { get; set; }

        #endregion
    }
}
