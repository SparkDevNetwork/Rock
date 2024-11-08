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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a table of values, where each cell of the table is an AttributeValue.
    /// that can be associated with a target Entity.
    /// The columns of the table are defined by an <see cref="AttributeMatrixTemplate"/>.
    /// The rows of the table are represented by AttributeMatrixItems, and each populated cell in the table is stored
    /// as an <see cref="AttributeValue"/> associated with the <see cref="AttributeMatrixItem"/> corresponding to the row.
    /// </summary>
    /// <remarks>
    /// An AttributeMatrix does not have a default association with any specific Entity Type.
    /// The matrix is linked to a target Entity Type by defining an Attribute on the target, configured as:
    /// * [Field Type] = "Matrix"
    /// * [Attribute Matrix Template] = an <see cref="AttributeMatrixTemplate"/> that defines the columns of the matrix.
    /// This configuration allows new AttributeMatrix instances to be created and associated with instances of the target Entity Type.
    /// A specific target entity is linked to a specific AttributeMatrix instance by storing the Guid identifier
    /// of the AttributeMatrix in the matrix AttributeValue associated with the target entity.
    /// For example:
    /// 1. A Matrix Attribute "Matrix1" exists for the Person Entity; it has a Matrix Type of "Template1".
    /// 2. Two rows of values are added to "Matrix1" for Ted Decker.
    /// 3. When these rows are saved:
    ///    (a) A new AttributeMatrix is created, with a new AttributeMatrixItem for each row of values.
    ///    (b) The Guid identifier of the new AttributeMatrix is stored in the AttributeValue "Matrix1" associated with Ted Decker.
    ///    (c) Individual cell values are stored as AttributeValues linked to the AttributeMatrixItem of the corresponding row.
    /// </remarks>
    [RockDomain( "Core" )]
    [Table( "AttributeMatrix" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "028228F0-B1D9-4DE5-9E6A-F898C34DDAB8")]
    public partial class AttributeMatrix : Model<AttributeMatrix>
    {
        #region Entity Properties

        /// <summary>
        /// The Id of the AttributeMatrixTemplate
        /// </summary>
        /// <value>
        /// The template identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        public int AttributeMatrixTemplateId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the attribute matrix template.
        /// </summary>
        /// <value>
        /// The attribute matrix template.
        /// </value>
        [DataMember]
        public virtual AttributeMatrixTemplate AttributeMatrixTemplate { get; set; }

        /// <summary>
        /// Gets or sets the attribute matrix items.
        /// </summary>
        /// <value>
        /// The attribute matrix items.
        /// </value>
        [DataMember]
        public virtual ICollection<AttributeMatrixItem> AttributeMatrixItems { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AttributeMatrixConfiguration : EntityTypeConfiguration<AttributeMatrix>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMatrixConfiguration"/> class.
        /// </summary>
        public AttributeMatrixConfiguration()
        {
            this.HasRequired( p => p.AttributeMatrixTemplate ).WithMany( p => p.AttributeMatrices ).HasForeignKey( p => p.AttributeMatrixTemplateId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
