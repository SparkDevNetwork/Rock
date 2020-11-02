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
using System;

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_bemaservices_HrManagement_PtoRequest" )]
    [DataContract]
    public class PtoRequest : Rock.Data.Model<PtoRequest>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [Required]
        [DataMember]
        public DateTime RequestDate { get; set; }

        [Required]
        [DataMember]
        public decimal Hours { get; set; }

        [DataMember]
        public int PtoAllocationId { get; set; }

        [DataMember]
        public int? ApproverPersonAliasId { get; set; }

        [Required]
        [DataMember]
        public PtoRequestApprovalState PtoRequestApprovalState { get; set; }

        [Required]
        [DataMember]
        public string Reason { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual PersonAlias ApproverPersonAlias { get; set; }

        [LavaInclude]
        public virtual PtoAllocation PtoAllocation { get; set; }

        [DataMember]
        [NotMapped]
        public virtual string Name
        {
            get
            {
                // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so
                // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
                return this.PtoAllocation.PersonAlias.Person.FullName + "(" + this.RequestDate.ToString( "M/d/yyyy" ) + ")";
            }

            private set
            {
                // intentionally blank
            }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class PtoRequestConfiguration : EntityTypeConfiguration<PtoRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoBracketTypeConfiguration"/> class.
        /// </summary>
        public PtoRequestConfiguration()
        {
            this.HasRequired( r => r.ApproverPersonAlias ).WithMany().HasForeignKey( r => r.ApproverPersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.PtoAllocation ).WithMany( r => r.PtoRequests ).HasForeignKey( r => r.PtoAllocationId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "PtoRequest" );
        }
    }

    #endregion

    #region Enumerations
    public enum PtoRequestApprovalState
    {
        Pending = 0,
        
        Approved = 1,
        
        Denied = 2,
        
        Cancelled = 3
    }

    #endregion
}
