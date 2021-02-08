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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.ViewModel;

namespace Rock.Model
{
    /// <summary>
    /// Represents the source record for the AnalyticDimCampus view in Rock.
    /// NOTE: Rock.Jobs.ProcessBIAnalytics dynamically adds additional columns to this table for any Attribute that is marked for Analytics
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceCampus" )]
    [DataContract]
    [HideFromReporting]
    [ViewModelExclude]
    public class AnalyticsSourceCampus : AnalyticsSourceCampusBase<AnalyticsSourceCampus>
    {
        // intentionally blank
    }

    /// <summary>
    /// AnalyticSourceCampus is a real table, and AnalyticsFactCampus is a VIEW off of AnalyticSourceCampus, so they share lots of columns
    /// </summary>
    [RockDomain( "Reporting" )]
    public abstract class AnalyticsSourceCampusBase<T> : Entity<T>
        where T : AnalyticsSourceCampusBase<T>, new()
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the name of the Campus. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Campus name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [Index( IsUnique = true )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets an optional short code identifier for the campus.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> value that represents a short code identifier for a campus. If the campus does not have a ShortCode
        /// this value is null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that is associated with this campus. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the Id of the (physical) location of the campus. If none exists, this value is null.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the campus.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the campus phone number.
        /// </value>
        [DataMember]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that is the leader of the campus.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the Id of the person who leads the campus.
        /// </value>
        [DataMember]
        public int? LeaderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the service times (Stored as a delimeted list)
        /// </summary>
        /// <value>
        /// The service times.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string ServiceTimes { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the count.
        /// NOTE: this always has a hardcoded value of 1. It is stored in the table because it is supposed to help do certain types of things in analytics
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; }

        #endregion
    }
}
