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

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_bemaservices_HrManagement_PtoBracketType" )]
    [DataContract]
    public class PtoBracketType : Rock.Data.Model<PtoBracketType>, Rock.Data.IRockEntity
    {

        #region Entity Properties
        
        [Required]
        [DataMember]
        public int PtoBracketId { get; set; }
        
        [Required]
        [DataMember]
        public int PtoTypeId { get; set; }

        [Required]
        [DataMember]
        public bool IsActive { get; set; }

        [Required]
        [DataMember]
        public int DefaultHours { get; set; }
        
        
        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual PtoBracket PtoBracket { get; set; }

        [LavaInclude]
        public virtual PtoType PtoType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class PtoBracketTypeConfiguration : EntityTypeConfiguration<PtoBracketType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoBracketTypeConfiguration"/> class.
        /// </summary>
        public PtoBracketTypeConfiguration()
        {
            this.HasRequired( r => r.PtoBracket ).WithMany( r => r.PtoBracketTypes ).HasForeignKey( r => r.PtoBracketId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.PtoType ).WithMany().HasForeignKey( r => r.PtoTypeId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "PtoBracketType" );
        }
    }

    #endregion
}
