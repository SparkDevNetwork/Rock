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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;
using DDay.iCal;
namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_ReservationLocation" )]
    [DataContract]
    public class ReservationLocation : Rock.Data.Model<ReservationLocation>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [DataMember]
        public int ReservationId { get; set; }

        [DataMember]
        public int LocationId { get; set; }

        [DataMember]
        public bool IsApproved { get; set; }

        #endregion

        #region Virtual Properties

        public virtual Reservation Reservation { get; set; }

        public virtual Location Location { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class ReservationLocationConfiguration : EntityTypeConfiguration<ReservationLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationLocationConfiguration"/> class.
        /// </summary>
        public ReservationLocationConfiguration()
        {
            this.HasRequired( r => r.Reservation ).WithMany( r => r.ReservationLocations ).HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
