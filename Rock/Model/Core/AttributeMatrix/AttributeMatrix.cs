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
    /// 
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeMatrix" )]
    [DataContract]
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
