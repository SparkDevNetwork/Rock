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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_bemaservices_HrManagement_PtoAllocation" )]
    [DataContract]
    public class PtoAllocation : Rock.Data.Model<PtoAllocation>, Rock.Data.IRockEntity
    {

        #region Entity Properties
        
        [Required]
        [DataMember]
        public int PtoTypeId { get; set; }

        [Required]
        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public decimal Hours { get; set; }

        [DataMember]
        public PtoAccrualSchedule PtoAccrualSchedule { get; set; }

        [Required]
        [DataMember]
        public PtoAllocationSourceType PtoAllocationSourceType { get; set; }

        [DataMember]
        public DateTime? LastProcessedDate { get; set; }

        [Required]
        [DataMember]
        public PtoAllocationStatus PtoAllocationStatus { get; set; }

        [Required]
        [DataMember]
        public int PersonAliasId { get; set; }

        [DataMember]
        public string Note { get; set; }

        #endregion

        #region methods

        public override string ToString()
        {
            return PtoType.Name + " " + this.StartDate.ToString( "M/yyyy") + ( this.EndDate.HasValue ? " - " + this.EndDate.Value.ToString("M/yyyy") : string.Empty ); 
        }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual PtoType PtoType { get; set; }

        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }
        
        [LavaInclude]
        public virtual ICollection<PtoRequest> PtoRequests
        {
            get { return _ptoRequests ?? ( _ptoRequests = new Collection<PtoRequest>() ); }
            set { _ptoRequests = value; }
        }

        private ICollection<PtoRequest> _ptoRequests;
        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class PtoAllocationConfiguration : EntityTypeConfiguration<PtoAllocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoAllocationConfiguration"/> class.
        /// </summary>
        public PtoAllocationConfiguration()
        {
            this.HasRequired( r => r.PtoType ).WithMany().HasForeignKey( r => r.PtoTypeId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "PtoAllocation" );
        }
    }

    #endregion

    #region Enumerations
    public enum PtoAccrualSchedule
    {
        None = 0,
        Yearly = 1,
        Quarterly = 2,
        Monthly = 3,
        Weekly = 4
    }

    public enum PtoAllocationSourceType
    {
        Automatic = 1,
        Manual = 2,
        Request = 3
    }

    public enum PtoAllocationStatus
    {
        Inactive = 0,
        Active = 1,
        Pending = 2,
        Denied = 3
    }
    #endregion
}
