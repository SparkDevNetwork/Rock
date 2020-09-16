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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Model;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Workflow Trigger
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_ReservationLocationType" )]
    [DataContract]
    public class ReservationLocationType : Rock.Data.Model<ReservationLocationType>, Rock.Data.IRockEntity
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
        /// Gets or sets the workflow type identifier.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int LocationTypeValueId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [DataMember]
        public virtual DefinedValue LocationTypeValue { get; set; }

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
    /// The EF configuration for the ReservationWorkflowTrigger
    /// </summary>
    public partial class ReservationLocationTypeConfiguration : EntityTypeConfiguration<ReservationLocationType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationLocationTypeConfiguration"/> class.
        /// </summary>
        public ReservationLocationTypeConfiguration()
        {
            this.HasRequired( p => p.LocationTypeValue ).WithMany().HasForeignKey( p => p.LocationTypeValueId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.ReservationType ).WithMany( p => p.ReservationLocationTypes ).HasForeignKey( p => p.ReservationTypeId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationLocationType" );
        }
    }
}

#endregion
