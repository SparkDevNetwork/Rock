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
using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsSourceFamilyHistorical is a real table, and AnalyticsDimFamilyHistorical and AnalyticsDimFamilyCurrent are VIEWs off of AnalyticsSourceFamilyHistorical, so they share lots of columns
    /// </summary>
    [RockDomain( "Reporting" )]
    public abstract class AnalyticsSourceFamilyBase<T> : Entity<T>
        where T : AnalyticsSourceFamilyBase<T>, new()
    {
        #region Entity Properties Specific to Analytics

        /// <summary>
        /// Gets or sets the FamilyId (which is really a GroupId)  (the live [Group] record of GroupTypeFamily that represents the family)
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int FamilyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current row indicator].
        /// This will be True if this represents the same values as the current Rock.Model.Group record for this Family
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current row indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CurrentRowIndicator { get; set; }

        /// <summary>
        /// Gets or sets the effective date.
        /// This is the starting date that the Family record had the values reflected in this record
        /// </summary>
        /// <value>
        /// The effective date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expire date.
        /// This is the last date that the Family record had the values reflected in this record
        /// For example, if a Family's LastName changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
        /// If this is most current record, the ExpireDate will be '9999-01-01'
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// NOTE:  This always has a (hard-coded) value of 1. It is stored in the table to assist with analytics calculations.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; } = 1;

        #endregion Entity Properties Specific to Analytics

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name (the value from Group.Name )
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the family title from ufnCrm_GetFamilyTitle
        /// </summary>
        /// <value>
        /// The family title.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string FamilyTitle { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier (based on the Family's Address/Location record's CampusId)
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// The ConnectionStatusValue.Name of the "Most Connected Family Member", which is defined as topmost ConnectionStatus of the family members (by DefinedValue.Order)
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the family has at least one family member with a RecordStatus of Active
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is family active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsFamilyActive { get; set; }

        /// <summary>
        /// Gets or sets the number of Adults in the family (GroupTypeRole.Adult)
        /// </summary>
        /// <value>
        /// The adult count.
        /// </value>
        [DataMember]
        public int AdultCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Children in the family (GroupTypeRole.Child)
        /// </summary>
        /// <value>
        /// The child count.
        /// </value>
        [DataMember]
        public int ChildCount { get; set; }

        /// <summary>
        /// Gets or sets the head of household person key which is defined as oldest Male Adult, or if there isn't a Male Adult, the oldest Female Adult
        /// </summary>
        /// <value>
        /// The head of household person key.
        /// </value>
        [DataMember]
        public int? HeadOfHouseholdPersonKey { get; set; }

        /// <summary>
        /// True if any of the family members's IsEra is True (as defined by the core_CurrentlyAnEra Person Attribute)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is era; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsEra { get; set; }

        /// <summary>
        /// Gets or sets the mailing address location identifier.
        /// </summary>
        /// <value>
        /// The mailing address location identifier.
        /// </value>
        [DataMember]
        public int? MailingAddressLocationId { get; set; }

        /// <summary>
        /// Gets or sets the mapped address location identifier.
        /// </summary>
        /// <value>
        /// The mapped address location identifier.
        /// </value>
        [DataMember]
        public int? MappedAddressLocationId { get; set; }

        #endregion Entity Properties
    }
}
