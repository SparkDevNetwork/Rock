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
using System.Text;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInAttendanceCodenEF Model.
    /// </summary>
    [Table( "AttendanceCode" )]
    [DataContract( IsReference = true )]
    public partial class AttendanceCode : Entity<AttendanceCode>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the issue date time.
        /// </summary>
        /// <value>
        /// The issue date time.
        /// </value>
        [DataMember]
        public DateTime IssueDateTime { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The security code.
        /// </value>
        [MaxLength(10)]
        [AlternateKey]
        [DataMember]
        public string Code { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
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
