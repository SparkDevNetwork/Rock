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
    /// A Room Reservation Type
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationType" )]
    [DataContract]
    public class ReservationType : Rock.Data.Model<ReservationType>, Rock.Data.IRockEntity
    {

        #region Entity Properties

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
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the final approval group identifier.
        /// </summary>
        /// <value>
        /// The final approval group identifier.
        /// </value>
        [DataMember]
        public int? FinalApprovalGroupId { get; set; }

        /// <summary>
        /// Gets or sets the super admin group identifier.
        /// </summary>
        /// <value>
        /// The super admin group identifier.
        /// </value>
        [DataMember]
        public int? SuperAdminGroupId { get; set; }

        /// <summary>
        /// Gets or sets the notification email identifier.
        /// </summary>
        /// <value>
        /// The notification email identifier.
        /// </value>
        [DataMember]
        public int? NotificationEmailId { get; set; }

        /// <summary>
        /// Gets or sets the default setup time.
        /// </summary>
        /// <value>
        /// The default setup time.
        /// </value>
        [DataMember]
        public int? DefaultSetupTime { get; set; }

        /// <summary>
        /// Gets or sets the default cleanup time.
        /// </summary>
        /// <value>
        /// The default cleanup time.
        /// </value>
        [DataMember]
        public int? DefaultCleanupTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is communication history saved.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is communication history saved; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCommunicationHistorySaved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is number attending required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is number attending required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsNumberAttendingRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is contact details required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is contact details required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsContactDetailsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is setup time required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is setup time required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSetupTimeRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reservation booked on approval.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is reservation booked on approval; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsReservationBookedOnApproval { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the final approval group.
        /// </summary>
        /// <value>
        /// The final approval group.
        /// </value>
        [LavaInclude]
        public virtual Group FinalApprovalGroup { get; set; }

        /// <summary>
        /// Gets or sets the super admin group.
        /// </summary>
        /// <value>
        /// The super admin group.
        /// </value>
        [LavaInclude]
        public virtual Group SuperAdminGroup { get; set; }

        /// <summary>
        /// Gets or sets the notification email.
        /// </summary>
        /// <value>
        /// The notification email.
        /// </value>
        [LavaInclude]
        public virtual SystemEmail NotificationEmail { get; set; }

        /// <summary>
        /// Gets or sets the reservations.
        /// </summary>
        /// <value>
        /// The reservations.
        /// </value>
        [LavaInclude]
        public virtual ICollection<Reservation> Reservations
        {
            get { return _reservations ?? ( _reservations = new Collection<Reservation>() ); }
            set { _reservations = value; }
        }
        private ICollection<Reservation> _reservations;

        /// <summary>
        /// Gets or sets the reservation ministries.
        /// </summary>
        /// <value>
        /// The reservation ministries.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ReservationMinistry> ReservationMinistries
        {
            get { return _reservationMinistries ?? ( _reservationMinistries = new Collection<ReservationMinistry>() ); }
            set { _reservationMinistries = value; }
        }
        private ICollection<ReservationMinistry> _reservationMinistries;

        /// <summary>
        /// Gets or sets the reservation workflow triggers.
        /// </summary>
        /// <value>
        /// The reservation workflow triggers.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ReservationWorkflowTrigger> ReservationWorkflowTriggers
        {
            get { return _reservationWorkflowTriggers ?? ( _reservationWorkflowTriggers = new Collection<ReservationWorkflowTrigger>() ); }
            set { _reservationWorkflowTriggers = value; }
        }
        private ICollection<ReservationWorkflowTrigger> _reservationWorkflowTriggers;


        /// <summary>
        /// Gets or sets the reservation location types.
        /// </summary>
        /// <value>
        /// The reservation location types.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ReservationLocationType> ReservationLocationTypes
        {
            get { return _reservationLocationTypes ?? ( _reservationLocationTypes = new Collection<ReservationLocationType>() ); }
            set { _reservationLocationTypes = value; }
        }
        private ICollection<ReservationLocationType> _reservationLocationTypes;

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        [NotMapped]
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.DELETE, "Additional roles and/or users that have access to delete reservations outside of the traditional approval process." );
                return supportedActions;
            }
        }

        #endregion
    }

    #region Entity Configuration


    /// <summary>
    /// EF configuration class for the ReservationType model
    /// </summary>
    public partial class ReservationTypeConfiguration : EntityTypeConfiguration<ReservationType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationTypeConfiguration"/> class.
        /// </summary>
        public ReservationTypeConfiguration()
        {
            this.HasOptional( r => r.FinalApprovalGroup ).WithMany().HasForeignKey( r => r.FinalApprovalGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.SuperAdminGroup ).WithMany().HasForeignKey( r => r.SuperAdminGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.NotificationEmail ).WithMany().HasForeignKey( r => r.NotificationEmailId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationType" );
        }
    }

    #endregion

}
