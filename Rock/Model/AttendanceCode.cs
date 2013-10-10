//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the security code that is issued for an individual when they check in to an group occurrence/event. An AttendanceCode can cover
    /// multiple <see cref="Rock.Model.Attendance"/> incidents for an individual.
    /// </summary>
    [Table( "AttendanceCode" )]
    [DataContract]
    public partial class AttendanceCode : Entity<AttendanceCode>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the date and time that the Attendance Code was issued.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Attendance Code was issued.
        /// </value>
        [DataMember]
        public DateTime IssueDateTime { get; set; }

        /// <summary>
        /// Gets or sets the attendance/security code.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the attendance/security code.
        /// </value>
        [MaxLength(10)]
        [AlternateKey]
        [DataMember]
        public string Code { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.Attendance"/> entities that are associated with this AttendanceCode.
        /// </summary>
        /// <value>
        /// The set of <see cref="Rock.Model.Attendance"/> entities that are associated with this AttendanceCode.
        /// </value>
        [DataMember]
        public virtual ICollection<Attendance> Attendances
        {
            get { return _attendances ?? ( _attendances = new Collection<Attendance>() ); }
            set { _attendances = value; }
        }
        private ICollection<Attendance> _attendances;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Code that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Code that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Code;
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class AttendanceCodeConfiguration : EntityTypeConfiguration<AttendanceCode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceCodeConfiguration"/> class.
        /// </summary>
        public AttendanceCodeConfiguration()
        {
        }
    }

    #endregion

}
