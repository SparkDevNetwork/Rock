using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using church.ccv.Datamart.Data;
using Rock.Data;

namespace church.ccv.Datamart.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Datamart_ERA" )]
    [DataContract]
    public partial class DatamartERA : Rock.Data.Entity<DatamartERA>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>
        /// The family identifier.
        /// </value>
        [DataMember]
        public int FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the weekend date.
        /// </summary>
        /// <value>
        /// The weekend date.
        /// </value>
        [DataMember]
        public DateTime WeekendDate { get; set; }

        /// <summary>
        /// Gets or sets the times attended last16 weeks.
        /// </summary>
        /// <value>
        /// The times attended last16 weeks.
        /// </value>
        [DataMember]
        public int? TimesAttendedLast16Weeks { get; set; }

        /// <summary>
        /// Gets or sets the first attended.
        /// </summary>
        /// <value>
        /// The first attended.
        /// </value>
        [DataMember]
        public DateTime? FirstAttended { get; set; }

        /// <summary>
        /// Gets or sets the last attended.
        /// </summary>
        /// <value>
        /// The last attended.
        /// </value>
        [DataMember]
        public DateTime? LastAttended { get; set; }

        /// <summary>
        /// Gets or sets the times gave last6 weeks.
        /// </summary>
        /// <value>
        /// The times gave last6 weeks.
        /// </value>
        [DataMember]
        public int? TimesGaveLast6Weeks { get; set; }

        /// <summary>
        /// Gets or sets the times gave last year.
        /// </summary>
        /// <value>
        /// The times gave last year.
        /// </value>
        [DataMember]
        public int? TimesGaveLastYear { get; set; }

        /// <summary>
        /// Gets or sets the times gave total.
        /// </summary>
        /// <value>
        /// The times gave total.
        /// </value>
        [DataMember]
        public int? TimesGaveTotal { get; set; }

        /// <summary>
        /// Gets or sets the last gave.
        /// </summary>
        /// <value>
        /// The last gave.
        /// </value>
        [DataMember]
        public DateTime? LastGave { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [regular attendee].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [regular attendee]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RegularAttendee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [regular attendee c].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [regular attendee c]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RegularAttendeeC { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [regular attendee g].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [regular attendee g]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RegularAttendeeG { get; set; }

        #endregion
    }
}
