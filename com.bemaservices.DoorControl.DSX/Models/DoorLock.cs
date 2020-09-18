using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using com.bemaservices.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.DoorControl.DSX.Models
{
    /// <summary>
    /// A Room Reservation
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_DoorLock" )]
    [DataContract]
    public class DoorLock : Rock.Data.Model<DoorLock>, Rock.Data.IRockEntity
    {
        #region Constants
        private const string ATTRIBUTE_KEY = "DoorOverrides";

        #endregion Contants

        #region Entity Properties

        [Required]
        [DataMember]
        public DateTime StartDateTime { get; set; }

        [Required]
        [DataMember]
        public DoorLockActions StartAction { get; set; }

        [Required]
        [DataMember]
        public DateTime EndDateTime { get; set; }

        [Required]
        [DataMember]
        public DoorLockActions EndAction { get; set; }

        [Required]
        [DataMember]
        public int OverrideGroup { get; set; }

        [DataMember]
        public string RoomName { get; set; }

        [DataMember]
        public int? ReservationId { get; set; }

        [Required]
        [DataMember]
        public int LocationId { get; set; }

        [DataMember]
        public bool? IsHvacOnly { get; set; }

        #endregion Entity Properties

        #region Virtual Properties
        [DataMember]
        public virtual Location Location { get; set; }

        [DataMember]
        public virtual Reservation Reservation { get; set; }

        #endregion Virtual Properties
    }
    public partial class DoorLockConfiguration : EntityTypeConfiguration<DoorLock>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoorLockConfiguration"/> class.
        /// </summary>
        public DoorLockConfiguration()
        {
            this.HasOptional( r => r.Reservation ).WithMany().HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "DoorLock" );
        }
    }
}