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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Model;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationLocation" )]
    [DataContract]
    public class ReservationLocation : Rock.Data.Model<ReservationLocation>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation identifier.
        /// </summary>
        /// <value>
        /// The reservation identifier.
        /// </value>
        [DataMember]
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the location layout identifier.
        /// </summary>
        /// <value>
        /// The location layout identifier.
        /// </value>
        [DataMember]
        public int? LocationLayoutId { get; set; }

        /// <summary>
        /// Gets or sets the state of the approval.
        /// </summary>
        /// <value>
        /// The state of the approval.
        /// </value>
        [DataMember]
        public ReservationLocationApprovalState ApprovalState { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the reservation.
        /// </summary>
        /// <value>
        /// The reservation.
        /// </value>
        public virtual Reservation Reservation { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [LavaInclude]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the location layout.
        /// </summary>
        /// <value>
        /// The location layout.
        /// </value>
        [LavaInclude]
        public virtual LocationLayout LocationLayout { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( ReservationLocation source )
        {
            this.Id = source.Id;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.ReservationId = source.ReservationId;
            this.LocationId = source.LocationId;
            this.LocationLayoutId = source.LocationLayoutId;
            this.ApprovalState = source.ApprovalState;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class ReservationLocationConfiguration : EntityTypeConfiguration<ReservationLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationLocationConfiguration"/> class.
        /// </summary>
        public ReservationLocationConfiguration()
        {
            this.HasRequired( r => r.Reservation ).WithMany( r => r.ReservationLocations ).HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.LocationLayout ).WithMany().HasForeignKey( r => r.LocationLayoutId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationLocation" );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// An enum that represents when a Job notification status should be sent.
    /// </summary>
    public enum ReservationLocationApprovalState
    {
        /// <summary>
        /// Notifications should be sent when a job completes with any notification status.
        /// </summary>
        Unapproved = 1,

        /// <summary>
        /// Notification should be sent when the job has completed successfully.
        /// </summary>
        /// 
        Approved = 2,

        /// <summary>
        /// Notification should be sent when the job has completed with an error status.
        /// </summary>
        Denied = 3
    }

    #endregion
}
