//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInAttendance EF Model.
    /// </summary>
    [Table( "Attendance" )]
    [DataContract( IsReference = true )]
    public partial class Attendance : Model<Attendance>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        /// <value>
        /// The location id.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the shedule id.
        /// </summary>
        /// <value>
        /// The shedule id.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the attendance code id.
        /// </summary>
        /// <value>
        /// The attendance code id.
        /// </value>
        [DataMember]
        public int? AttendanceCodeId { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value id.
        /// </summary>
        /// <value>
        /// The qualifier value id.
        /// </value>
        [DataMember]
        public int? QualifierValueId { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        [DataMember]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        [DataMember]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [did attend].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [did attend]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool DidAttend 
        {
            get { return _didAttend; }
            set { _didAttend = value; }
        }
        private bool _didAttend = true;

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public string Note { get; set; }
        
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the attendance code.
        /// </summary>
        /// <value>
        /// The attendance code.
        /// </value>
        [DataMember]
        public virtual AttendanceCode AttendanceCode { get; set; }

        /// <summary>
        /// Gets or sets the qualifier.
        /// </summary>
        /// <value>
        /// The qualifier.
        /// </value>
        [DataMember]
        public virtual DefinedValue Qualifier { get; set; }

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
            StringBuilder sb = new StringBuilder();

            sb.Append(Person != null ? Person.ToString() + " " : "");
            sb.Append(DidAttend ? "attended " : "did not attend ");
            sb.Append(Group != null ? Group.ToString()+ " " : "");
            sb.AppendFormat("on {0} at {1} ", StartDateTime.ToShortDateString(), StartDateTime.ToShortTimeString());
            if (EndDateTime.HasValue)
            {
                sb.AppendFormat("until {0} at {1} ", EndDateTime.Value.ToShortDateString(), EndDateTime.Value.ToShortTimeString());
            }
            sb.Append(Location != null ? "in " + Location.ToString() : "");

            return sb.ToString().Trim();

        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class AttendanceConfiguration : EntityTypeConfiguration<Attendance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceConfiguration"/> class.
        /// </summary>
        public AttendanceConfiguration()
        {
            this.HasOptional( a => a.Location).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Person ).WithMany().HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Qualifier ).WithMany().HasForeignKey( p => p.QualifierValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AttendanceCode ).WithMany( c => c.Attendances ).HasForeignKey( a => a.AttendanceCodeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
