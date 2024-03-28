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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a row of matrix values in an <see cref="AttributeMatrix"/>.
    /// Individual cell values are stored as a collection of <see cref="AttributeValue"/> instances associated with this entity.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeMatrixItem" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "3C9D5021-0484-4846-AEF6-B6216D26C3C8")]
    public partial class AttributeMatrixItem : Model<AttributeMatrixItem>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the attribute matrix identifier.
        /// </summary>
        /// <value>
        /// The attribute matrix identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        public int AttributeMatrixId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets the attribute matrix template identifier (Need this so that Attributes can be qualified on AttributeMatrix's AttributeMatrixTempleId)
        /// </summary>
        /// <value>
        /// The attribute matrix template identifier.
        /// </value>
        public virtual int AttributeMatrixTemplateId
        {
            get
            {
                // Need to check for a null in case the AttributeMatrix obj didn't get lazy loaded as is the case with the REST API.
                if ( this.AttributeMatrix == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        return new AttributeMatrixService( rockContext ).GetNoTracking( this.AttributeMatrixId ).AttributeMatrixTemplateId;
                    }
                }

                return this.AttributeMatrix.AttributeMatrixTemplateId;
            }
        }

        /// <summary>
        /// Gets or sets the attribute matrix.
        /// </summary>
        /// <value>
        /// The attribute matrix.
        /// </value>
        [DataMember]
        public virtual AttributeMatrix AttributeMatrix { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AttributeMatrixItemConfiguration : EntityTypeConfiguration<AttributeMatrixItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMatrixItemConfiguration"/> class.
        /// </summary>
        public AttributeMatrixItemConfiguration()
        {
            this.HasRequired( p => p.AttributeMatrix ).WithMany( p => p.AttributeMatrixItems ).HasForeignKey( p => p.AttributeMatrixId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
