﻿// <copyright>
// Copyright by BEMA Information Technologies
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
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_bemaservices_HrManagement_PtoTier" )]
    [DataContract]
    public class PtoTier : Rock.Data.Model<PtoTier>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [Required]
        [DataMember]
        public bool IsActive { get; set; }

        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        [MaxLength( 100 )]
        public string Color { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection containing the <see cref="com.bemaservices.HrManagement.Model.PtoBracket">PtoBrackets</see> who are associated with the PtoTier.
        /// </summary>
        /// <value>
        /// A collection of <see cref="com.bemaservices.HrManagement.Model.PtoBracket">PtoBrackets</see> who are associated with the PtoTier.
        /// </value>
        [LavaInclude]
        public virtual ICollection<PtoBracket> PtoBrackets
        {
            get { return _ptoBrackets ?? ( _ptoBrackets = new Collection<PtoBracket>() ); }
            set { _ptoBrackets = value; }
        }

        private ICollection<PtoBracket> _ptoBrackets;
        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class PtoTierConfiguration : EntityTypeConfiguration<PtoTier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoTierConfiguration"/> class.
        /// </summary>
        public PtoTierConfiguration()
        {
            // IMPORTANT!!
            this.HasEntitySetName( "PtoTier" );
        }
    }

    #endregion
}
