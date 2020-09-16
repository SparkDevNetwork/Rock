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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;
using com.centralaz.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Web.Cache;
namespace com.centralaz.RoomManagement.Web.Cache
{
    /// <summary>
    /// Information about a reservation ministry that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class ReservationMinistryCache : ModelCache<ReservationMinistryCache, ReservationMinistry>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the reservation type identifier.
        /// </summary>
        /// <value>
        /// The reservation type identifier.
        /// </value>
        [DataMember]
        public int ReservationTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
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
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>
        /// The type of the reservation.
        /// </value>
        public virtual ReservationType ReservationType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var reservationMinistry = entity as ReservationMinistry;
            if ( reservationMinistry == null ) return;

            this.Description = reservationMinistry.Description;
            this.IsActive = reservationMinistry.IsActive;
            this.Name = reservationMinistry.Name;
            this.ReservationType = reservationMinistry.ReservationType;
            this.ReservationTypeId = reservationMinistry.ReservationTypeId;
            this.Order = reservationMinistry.Order;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }
}