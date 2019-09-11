// <copyright>
// Copyright by the Spark Development Network
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Financial;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a payment gateway
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialGateway" )]
    [DataContract]
    public partial class FinancialGateway : Model<FinancialGateway>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the (internal) Name of the FinancialGateway. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the (internal) name of the FinancialGateway.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the FinancialGateway.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the FinancialGateway.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the gateway entity type identifier.
        /// </summary>
        /// <value>
        /// The gateway entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets the batch time offset (in ticks). By default online payments will be grouped into batches with a start time
        /// of 12:00:00 AM.  However if the payment gateway groups transactions into batches based on a different
        /// time, this offset can specified so that Rock will use the same time when creating batches for online
        /// transactions
        /// </summary>
        [DataMember]
        public virtual long BatchTimeOffsetTicks { get; set; }

        /// <summary>
        /// Null for daily batches (default). For weekly batches, this (as well as the
        /// BatchTimeOffsetTicks) indicates the day of the week that a new batch should
        /// begin.
        /// </summary>
        [DataMember]
        public virtual DayOfWeek? BatchDayOfWeek { get; set; }

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
        /// Gets or sets the type of the gateway entity.
        /// </summary>
        /// <value>
        /// The type of the gateway entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialGateway"/> class.
        /// </summary>
        public FinancialGateway()
        {
            IsActive = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the gateway component.
        /// </summary>
        /// <returns></returns>
        public virtual GatewayComponent GetGatewayComponent()
        {
            if ( EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( EntityTypeId.Value );
                if ( entityType != null )
                {
                    return GatewayContainer.GetComponent( entityType.Name );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the batch time offset.
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetBatchTimeOffset()
        {
            return new TimeSpan( BatchTimeOffsetTicks );
        }

        /// <summary>
        /// Sets the batch time offset.
        /// </summary>
        /// <param name="timespan">The timespan.</param>
        public void SetBatchTimeOffset( TimeSpan? timespan )
        {
            BatchTimeOffsetTicks = timespan.HasValue ? timespan.Value.Ticks : 0;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FinancialGateway.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FinancialGateway.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// FinancialGateway Configuration class.
    /// </summary>
    public partial class FinancialGatewayConfiguration : EntityTypeConfiguration<FinancialGateway>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialGatewayConfiguration"/> class.
        /// </summary>
        public FinancialGatewayConfiguration()
        {
            this.HasRequired( g => g.EntityType).WithMany().HasForeignKey( a => a.EntityTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}