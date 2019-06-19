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
    /// 
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeMatrixItem" )]
    [DataContract]
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the attribute matrix template identifier (Need this so that Attributes can be qualified on AttributeMatrix's AttributeMatrixTempleId)
        /// </summary>
        /// <value>
        /// The attribute matrix template identifier.
        /// </value>
        public virtual int AttributeMatrixTemplateId => AttributeMatrix.AttributeMatrixTemplateId;

        /// <summary>
        /// Gets or sets the attribute matrix.
        /// </summary>
        /// <value>
        /// The attribute matrix.
        /// </value>
        [DataMember]
        public virtual AttributeMatrix AttributeMatrix { get; set; }

        #endregion
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

    #endregion
}
