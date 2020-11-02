﻿// <copyright>
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Ministry
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationMinistry" )]
    [DataContract]
    public class ReservationMinistry : Rock.Data.Model<ReservationMinistry>, Rock.Data.IRockEntity
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
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

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

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// The EF configuration class for the ReservationMinistry
    /// </summary>
    public partial class ReservationMinistryConfiguration : EntityTypeConfiguration<ReservationMinistry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationMinistryConfiguration"/> class.
        /// </summary>
        public ReservationMinistryConfiguration()
        {
            this.HasRequired( p => p.ReservationType ).WithMany( p => p.ReservationMinistries ).HasForeignKey( p => p.ReservationTypeId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationMinistry" );
        }
    }

    #endregion

}
