// <copyright>
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_bemaservices_HrManagement_PtoBracket" )]
    [DataContract]
    public class PtoBracket : Rock.Data.Model<PtoBracket>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [Required]
        [DataMember]
        public int PtoTierId { get; set; }

        [Required]
        [DataMember]
        public bool IsActive { get; set; }

        [Required]
        [DataMember]
        public int MinimumYear { get; set; }

        [DataMember]
        public int? MaximumYear { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual PtoTier PtoTier { get; set; }

        [LavaInclude]
        public virtual string Name { get { return ToString(); } }

        [LavaInclude]
        public virtual ICollection<PtoBracketType> PtoBracketTypes
        {
            get { return _ptoBracketTypes ?? ( _ptoBracketTypes = new Collection<PtoBracketType>() ); }
            set { _ptoBracketTypes = value; }
        }

        private ICollection<PtoBracketType> _ptoBracketTypes;

        #endregion

        public override string ToString()
        {

            if ( this.MaximumYear.HasValue )
            {
                return this.MinimumYear.ToString() + " to " + this.MaximumYear.Value.ToString() + " years";
            }
            else
            {
                return this.MinimumYear.ToString() + "+ years";
            }

        }
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class PtoBracketConfiguration : EntityTypeConfiguration<PtoBracket>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoBracketConfiguration"/> class.
        /// </summary>
        public PtoBracketConfiguration()
        {
            this.HasRequired( r => r.PtoTier ).WithMany( r => r.PtoBrackets ).HasForeignKey( r => r.PtoTierId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "PtoBracket" );
        }
    }

    #endregion
}
