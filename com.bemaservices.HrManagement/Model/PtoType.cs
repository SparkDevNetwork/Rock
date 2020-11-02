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
    [Table( "_com_bemaservices_HrManagement_PtoType" )]
    [DataContract]
    public class PtoType : Rock.Data.Model<PtoType>, Rock.Data.IRockEntity
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
        public bool IsNegativeTimeBalanceAllowed { get; set; }

        [DataMember]
        [MaxLength( 100 )]
        public string Color { get; set; }

        [DataMember]
        public int? WorkflowTypeId { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual WorkflowType WorkflowType { get; set; }

        #endregion

        #region mothods

        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class PtoTypeConfiguration : EntityTypeConfiguration<PtoType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoTypeConfiguration"/> class.
        /// </summary>
        public PtoTypeConfiguration()
        {
            this.HasOptional( r => r.WorkflowType ).WithMany().HasForeignKey( r => r.WorkflowTypeId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "PtoType" );
        }
    }

    #endregion
}
