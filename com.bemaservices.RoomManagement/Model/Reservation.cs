// <copyright>
// Copyright by BEMA Software Services
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Room Reservation
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_Reservation" )]
    [DataContract]
    public class Reservation : Rock.Data.Model<Reservation>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation type identifier.
        /// </summary>
        /// <value>
        /// The reservation type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int ReservationTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [Required]
        [DataMember]
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence identifier.
        /// </summary>
        /// <value>
        /// The event item occurrence identifier.
        /// </value>
        [DataMember]
        public int? EventItemOccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the reservation ministry identifier.
        /// </summary>
        /// <value>
        /// The reservation ministry identifier.
        /// </value>
        [DataMember]
        public int? ReservationMinistryId { get; set; }

        /// <summary>
        /// Gets or sets the state of the approval.
        /// </summary>
        /// <value>
        /// The state of the approval.
        /// </value>
        [DataMember]
        public ReservationApprovalState ApprovalState { get; set; }

        /// <summary>
        /// Gets or sets the requester alias identifier.
        /// </summary>
        /// <value>
        /// The requester alias identifier.
        /// </value>
        [DataMember]
        public int? RequesterAliasId { get; set; }

        /// <summary>
        /// Gets or sets the approver alias identifier.
        /// </summary>
        /// <value>
        /// The approver alias identifier.
        /// </value>
        [DataMember]
        public int? ApproverAliasId { get; set; }

        /// <summary>
        /// Gets or sets the setup time.
        /// </summary>
        /// <value>
        /// The setup time.
        /// </value>
        [DataMember]
        public int? SetupTime { get; set; }

        /// <summary>
        /// Gets or sets the cleanup time.
        /// </summary>
        /// <value>
        /// The cleanup time.
        /// </value>
        [DataMember]
        public int? CleanupTime { get; set; }

        /// <summary>
        /// Gets or sets the number attending.
        /// </summary>
        /// <value>
        /// The number attending.
        /// </value>
        [DataMember]
        public int? NumberAttending { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        [MaxLength( 2500 )]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the setup photo identifier.
        /// </summary>
        /// <value>
        /// The setup photo identifier.
        /// </value>
        [DataMember]
        public int? SetupPhotoId { get; set; }

        /// <summary>
        /// Gets or sets the name of the event contact.
        /// </summary>
        /// <value>
        /// The name of the event contact.
        /// </value>
        [DataMember]
        public int? EventContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the event contact phone.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the phone number of the event contact person.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string EventContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the email address of the event contact.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the email of the event contact person.
        /// </value>
        [DataMember]
        [MaxLength( 400 )]
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
        public string EventContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the administrative contact.
        /// </summary>
        /// <value>
        /// The name of the administrative contact.
        /// </value>
        [DataMember]
        public int? AdministrativeContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact phone.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the phone number of the administrative contact person.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string AdministrativeContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the email address of the administrative contact.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the email of the administrative contact person.
        /// </value>
        [DataMember]
        [MaxLength( 400 )]
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
        public string AdministrativeContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the first occurrence date time.
        /// </summary>
        /// <value>
        /// The first occurrence date time.
        /// </value>
        [DataMember]
        public DateTime? FirstOccurrenceStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last occurrence date time.
        /// </summary>
        /// <value>
        /// The last occurrence date time.
        /// </value>
        [DataMember]
        public DateTime? LastOccurrenceEndDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>
        /// The type of the reservation.
        /// </value>
        [DataMember]
        public virtual ReservationType ReservationType { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [LavaInclude]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [LavaInclude]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence.
        /// </summary>
        /// <value>
        /// The event item occurrence.
        /// </value>
        [LavaInclude]
        public virtual EventItemOccurrence EventItemOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the reservation ministry.
        /// </summary>
        /// <value>
        /// The reservation ministry.
        /// </value>
        [LavaInclude]
        public virtual ReservationMinistry ReservationMinistry { get; set; }

        /// <summary>
        /// Gets or sets the requester alias.
        /// </summary>
        /// <value>
        /// The requester alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias RequesterAlias { get; set; }

        /// <summary>
        /// Gets or sets the approver alias.
        /// </summary>
        /// <value>
        /// The approver alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias ApproverAlias { get; set; }

        /// <summary>
        /// Gets or sets the reservation workflows.
        /// </summary>
        /// <value>
        /// The reservation workflows.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ReservationWorkflow> ReservationWorkflows
        {
            get { return _reservationWorkflows; }
            set { _reservationWorkflows = value; }
        }
        private ICollection<ReservationWorkflow> _reservationWorkflows;

        /// <summary>
        /// Gets or sets the reservation resources.
        /// </summary>
        /// <value>
        /// The reservation resources.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ReservationResource> ReservationResources
        {
            get { return _reservationResources ?? ( _reservationResources = new Collection<ReservationResource>() ); }
            set { _reservationResources = value; }
        }
        private ICollection<ReservationResource> _reservationResources;

        /// <summary>
        /// Gets or sets the reservation locations.
        /// </summary>
        /// <value>
        /// The reservation locations.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ReservationLocation> ReservationLocations
        {
            get { return _reservationLocations ?? ( _reservationLocations = new Collection<ReservationLocation>() ); }
            set { _reservationLocations = value; }
        }
        private ICollection<ReservationLocation> _reservationLocations;

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <value>
        /// The setup photo URL.
        /// </value>
        [LavaInclude]
        [NotMapped]
        public virtual string SetupPhotoUrl
        {
            get
            {
                return Reservation.GetSetupPhotoUrl( this );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets or sets the setup photo.
        /// </summary>
        /// <value>
        /// The setup photo.
        /// </value>
        [DataMember]
        public virtual BinaryFile SetupPhoto { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the event contact person.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the event contact person.
        /// </value>
        [DataMember]
        public virtual PersonAlias EventContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the administrative contact person.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the administrative contact person.
        /// </value>
        [DataMember]
        public virtual PersonAlias AdministrativeContactPersonAlias { get; set; }

        /// <summary>
        /// Gets the friendly reservation time.
        /// </summary>
        /// <value>
        /// The friendly reservation time.
        /// </value>
        [LavaInclude]
        [NotMapped]
        public virtual string FriendlyReservationTime
        {
            get
            {
                return GetFriendlyReservationScheduleText();
            }
            private set
            {

            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Rock.Security.ISecured ParentAuthority
        {
            get
            {
                return this.ReservationType != null ? this.ReservationType : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets a list of scheduled start datetimes between the two specified dates, sorted by datetime.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public virtual List<ReservationDateTime> GetReservationTimes( DateTime beginDateTime, DateTime endDateTime )
        {
            if ( Schedule != null )
            {
                var result = new List<ReservationDateTime>();

                DDay.iCal.Event calEvent = Schedule.GetCalenderEvent();
                if ( calEvent != null && calEvent.DTStart != null )
                {
                    var occurrences = ScheduleICalHelper.GetOccurrences( calEvent, beginDateTime, endDateTime );
                    result = occurrences
                        .Where( a =>
                            a.Period != null &&
                            a.Period.StartTime != null &&
                            a.Period.EndTime != null )
                        .Select( a => new ReservationDateTime
                        {
                            StartDateTime = DateTime.SpecifyKind( a.Period.StartTime.Value, DateTimeKind.Local ),
                            EndDateTime = DateTime.SpecifyKind( a.Period.EndTime.Value, DateTimeKind.Local )
                        } )
                        .OrderBy( a => a.StartDateTime )
                        .ToList();
                    {
                        // ensure the datetime is DateTimeKind.Local since iCal returns DateTimeKind.UTC
                    }
                }

                return result;
            }
            else
            {
                return new List<ReservationDateTime>();
            }

        }

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetSetupPhotoUrl( Reservation reservation, int? maxWidth = null, int? maxHeight = null )
        {
            return GetSetupPhotoUrl( reservation.Id, reservation.SetupPhotoId, maxWidth, maxHeight );
        }

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetSetupPhotoUrl( int reservationId, int? maxWidth = null, int? maxHeight = null )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                Reservation reservation = new ReservationService( rockContext ).Get( reservationId );
                return GetSetupPhotoUrl( reservation, maxWidth, maxHeight );
            }
        }

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="setupPhotoId">The setup photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetSetupPhotoUrl( int? reservationId, int? setupPhotoId, int? maxWidth = null, int? maxHeight = null )
        {
            string virtualPath = string.Empty;
            if ( setupPhotoId.HasValue )
            {
                string widthHeightParams = string.Empty;
                if ( maxWidth.HasValue )
                {
                    widthHeightParams += string.Format( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    widthHeightParams += string.Format( "&maxheight={0}", maxHeight.Value );
                }

                virtualPath = string.Format( "~/GetImage.ashx?id={0}" + widthHeightParams, setupPhotoId );
            }

            if ( System.Web.HttpContext.Current == null )
            {
                return virtualPath;
            }
            else
            {
                return VirtualPathUtility.ToAbsolute( virtualPath );
            }
        }

        /// <summary>
        /// Gets the setup photo image tag.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="altText">The alt text.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public static string GetSetupPhotoImageTag( Reservation reservation, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            if ( reservation != null )
            {
                return GetSetupPhotoImageTag( reservation.Id, reservation.SetupPhotoId, maxWidth, maxHeight, altText, className );
            }
            else
            {
                return GetSetupPhotoImageTag( null, null, maxWidth, maxHeight, altText, className );
            }

        }

        /// <summary>
        /// Gets the setup photo image tag.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="setupPhotoId">The setup photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="altText">The alt text.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public static string GetSetupPhotoImageTag( int? reservationId, int? setupPhotoId, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            var photoUrl = new StringBuilder();

            photoUrl.Append( VirtualPathUtility.ToAbsolute( "~/" ) );

            string styleString = string.Empty;

            string altString = string.IsNullOrWhiteSpace( altText ) ? string.Empty :
                string.Format( " alt='{0}'", altText );

            string classString = string.IsNullOrWhiteSpace( className ) ? string.Empty :
                string.Format( " class='{0}'", className );

            if ( setupPhotoId.HasValue )
            {
                photoUrl.AppendFormat( "GetImage.ashx?id={0}", setupPhotoId );
                if ( maxWidth.HasValue )
                {
                    photoUrl.AppendFormat( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    photoUrl.AppendFormat( "&maxheight={0}", maxHeight.Value );
                }

                return string.Format( "<img src='{0}'{1}{2}{3}/>", photoUrl.ToString(), styleString, altString, classString );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the friendly reservation schedule text.
        /// </summary>
        /// <returns></returns>
        public string GetFriendlyReservationScheduleText()
        {
            string result = "";
            if ( Schedule != null )
            {
                StringBuilder sb = new StringBuilder();
                sb.Append( Schedule.ToFriendlyScheduleText() );

                var calendarEvent = Schedule.GetCalendarEvent();
                if ( calendarEvent != null )
                {
                    if ( calendarEvent.Duration != null )
                    {
                        var duration = calendarEvent.Duration;
                        if ( duration.Hours > 0 )
                        {
                            if ( duration.Hours == 1 )
                            {
                                sb.AppendFormat( " for {0} hr", duration.Hours );
                            }
                            else
                            {
                                sb.AppendFormat( " for {0} hrs", duration.Hours );
                            }

                            if ( duration.Minutes > 0 )
                            {
                                sb.AppendFormat( " and {0} min", duration.Minutes );
                            }
                        }
                        else
                        {
                            if ( duration.Minutes > 0 )
                            {
                                sb.AppendFormat( " for {0} min", duration.Minutes );
                            }
                        }
                    }
                    if ( calendarEvent.RecurrenceRules.Any() )
                    {
                        if ( FirstOccurrenceStartDateTime.HasValue )
                        {
                            sb.AppendFormat( " from {0}", FirstOccurrenceStartDateTime.Value.ToShortDateString() );
                        }

                        if ( LastOccurrenceEndDateTime.HasValue )
                        {
                            sb.AppendFormat( " to {0}", LastOccurrenceEndDateTime.Value.ToShortDateString() );
                        }
                    }

                }

                result = sb.ToString();
            }

            return result;
        }

        /// <summary>
        /// Creates a transaction to act a hook for workflow triggers before changes occur
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            if ( entry.State == System.Data.Entity.EntityState.Added || entry.State == System.Data.Entity.EntityState.Modified )
            {
                var transaction = new com.bemaservices.RoomManagement.Transactions.ReservationChangeTransaction( entry );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion
    }

    #region Entity Configuration


    /// <summary>
    /// The EF configuration for the Reservation model
    /// </summary>
    public partial class ReservationConfiguration : EntityTypeConfiguration<Reservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationConfiguration"/> class.
        /// </summary>
        public ReservationConfiguration()
        {
            this.HasRequired( p => p.ReservationType ).WithMany( p => p.Reservations ).HasForeignKey( p => p.ReservationTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.EventItemOccurrence ).WithMany().HasForeignKey( r => r.EventItemOccurrenceId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.ReservationMinistry ).WithMany().HasForeignKey( r => r.ReservationMinistryId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Schedule ).WithMany().HasForeignKey( r => r.ScheduleId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.RequesterAlias ).WithMany().HasForeignKey( r => r.RequesterAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.ApproverAlias ).WithMany().HasForeignKey( r => r.ApproverAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.SetupPhoto ).WithMany().HasForeignKey( p => p.SetupPhotoId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.EventContactPersonAlias ).WithMany().HasForeignKey( p => p.EventContactPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.AdministrativeContactPersonAlias ).WithMany().HasForeignKey( p => p.AdministrativeContactPersonAliasId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "Reservation" );
        }
    }

    #endregion

    #region Enumerations
    /// <summary>
    /// The enumeration for the reservation approval state
    /// </summary>
    public enum ReservationApprovalState
    {
        /// <summary>
        /// The unapproved
        /// </summary>
        Unapproved = 1,

        /// <summary>
        /// The approved
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The denied
        /// </summary>
        Denied = 3,

        /// <summary>
        /// The changes needed
        /// </summary>
        ChangesNeeded = 4,

        /// <summary>
        /// The pending review
        /// </summary>
        PendingReview = 5
    }

    #endregion

    #region Helper Classes
    /// <summary>
    /// The view model for a Reservation DateTime
    /// </summary>
    public class ReservationDateTime
    {
        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public DateTime StartDateTime { get; set; }
        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        public DateTime EndDateTime { get; set; }
    }

    #endregion
}
