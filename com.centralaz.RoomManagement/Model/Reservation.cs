// <copyright>
// Copyright by the Central Christian Church
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Room Reservation
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_Reservation" )]
    [DataContract]
    public class Reservation : Rock.Data.Model<Reservation>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int ScheduleId { get; set; }

        [DataMember]
        public int? CampusId { get; set; }

        [DataMember]
        public int? ReservationMinistryId { get; set; }

        [DataMember]
        public ReservationApprovalState ApprovalState { get; set; }

        [DataMember]
        public int? RequesterAliasId { get; set; }

        [DataMember]
        public int? ApproverAliasId { get; set; }

        [DataMember]
        public int? SetupTime { get; set; }

        [DataMember]
        public int? CleanupTime { get; set; }

        [DataMember]
        public int? NumberAttending { get; set; }

        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public int? SetupPhotoId { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual Schedule Schedule { get; set; }

        [LavaInclude]
        public virtual Campus Campus { get; set; }

        [LavaInclude]
        public virtual ReservationMinistry ReservationMinistry { get; set; }

        [LavaInclude]
        public virtual PersonAlias RequesterAlias { get; set; }

        [LavaInclude]
        public virtual PersonAlias ApproverAlias { get; set; }

        [LavaInclude]
        public virtual ICollection<ReservationWorkflow> ReservationWorkflows
        {
            get { return _reservationWorkflows; }
            set { _reservationWorkflows = value; }
        }
        private ICollection<ReservationWorkflow> _reservationWorkflows;

        [LavaInclude]
        public virtual ICollection<ReservationResource> ReservationResources
        {
            get { return _reservationResources ?? ( _reservationResources = new Collection<ReservationResource>() ); }
            set { _reservationResources = value; }
        }
        private ICollection<ReservationResource> _reservationResources;

        [LavaInclude]
        public virtual ICollection<ReservationLocation> ReservationLocations
        {
            get { return _reservationLocations ?? ( _reservationLocations = new Collection<ReservationLocation>() ); }
            set { _reservationLocations = value; }
        }
        private ICollection<ReservationLocation> _reservationLocations;

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

        [DataMember]
        public virtual BinaryFile SetupPhoto { get; set; }

        #endregion

        #region Methods

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
                        .ToList();
                    {
                        // ensure the the datetime is DateTimeKind.Local since iCal returns DateTimeKind.UTC
                    }
                }

                return result;
            }
            else
            {
                return new List<ReservationDateTime>();
            }

        }

        public static string GetSetupPhotoUrl( Reservation reservation, int? maxWidth = null, int? maxHeight = null )
        {
            return GetSetupPhotoUrl( reservation.Id, reservation.SetupPhotoId, maxWidth, maxHeight );
        }

        public static string GetSetupPhotoUrl( int reservationId, int? maxWidth = null, int? maxHeight = null )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                Reservation reservation = new ReservationService( rockContext ).Get( reservationId );
                return GetSetupPhotoUrl( reservation, maxWidth, maxHeight );
            }
        }

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
        /// Creates a transaction to act a hook for workflow triggers before changes occur
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            if ( entry.State == System.Data.Entity.EntityState.Added || entry.State == System.Data.Entity.EntityState.Modified )
            {
                var transaction = new com.centralaz.RoomManagement.Transactions.ReservationChangeTransaction( entry );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion

    }

    #region Entity Configuration


    public partial class ReservationConfiguration : EntityTypeConfiguration<Reservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationConfiguration"/> class.
        /// </summary>
        public ReservationConfiguration()
        {
            this.HasRequired( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.ReservationMinistry ).WithMany().HasForeignKey( r => r.ReservationMinistryId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Schedule ).WithMany().HasForeignKey( r => r.ScheduleId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.RequesterAlias ).WithMany().HasForeignKey( r => r.RequesterAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.ApproverAlias ).WithMany().HasForeignKey( r => r.ApproverAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.SetupPhoto ).WithMany().HasForeignKey( p => p.SetupPhotoId ).WillCascadeOnDelete( false );

        }
    }

    #endregion

    #region Enumerations
    public enum ReservationApprovalState
    {
        Unapproved = 1,

        Approved = 2,

        Denied = 3,

        ChangesNeeded = 4,

        PendingReview = 5
    }

    #endregion

    #region Helper Classes
    public class ReservationDateTime
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }

    #endregion

}
