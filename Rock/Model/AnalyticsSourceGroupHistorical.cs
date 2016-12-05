using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the source record for the AnalyticsDimGroupHistorical and AnalyticsDimGroupCurrent views
    /// </summary> 
    [Table( "AnalyticsSourceGroupHistorical" )]
    [DataContract]
    [HideFromReporting]
    public class AnalyticsSourceGroupHistorical : AnalyticsSourceGroupBase<AnalyticsSourceGroupHistorical>
    {
        // intentionally blank
    }

    /// <summary>
    /// AnalyticsSourceGroupHistorical is a real table, and AnalyticsDimGroupHistorical and AnalyticsDimGroupCurrent are VIEWs off of AnalyticsSourceGroupHistorical, so they share lots of columns
    /// </summary>
    public abstract class AnalyticsSourceGroupBase<T> : Entity<T>
        where T : AnalyticsSourceGroupBase<T>, new()
    {
        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the group identifier. (In Rock, a Group is stored as Group with a GroupType of Group)
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current row indicator].
        /// This will be True if this represents the same values as the current Rock.Model.Group record for this Group
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current row indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CurrentRowIndicator { get; set; }

        /// <summary>
        /// Gets or sets the effective date.
        /// This is the starting date that the group/Group record had the values reflected in this record
        /// </summary>
        /// <value>
        /// The effective date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expire date.
        /// This is the last date that the group record had the values reflected in this record
        /// For example, if a Group's name or campus changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
        /// If this is most current record, the ExpireDate will be '9999-01-01'
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime ExpireDate { get; set; }

        #endregion

        #region Selected Entity Properties from Rock.Model.Group

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 450 )]
        [DataMember]
        public string Name { get; set; }

        #endregion
    }
}
