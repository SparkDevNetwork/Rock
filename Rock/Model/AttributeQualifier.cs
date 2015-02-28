// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a attribute qualifier that limits or qualifies the values that can be accepted as <see cref="Rock.Model.AttributeValue">AttributeValues</see>.
    /// </summary>
    /// <remarks>
    /// Examples this can be a <see cref="Rock.Model.DefinedValue"/>, SQL query, or a list of options.
    /// </remarks>
    [Table( "AttributeQualifier" )]
    [DataContract]
    public partial class AttributeQualifier : Entity<AttributeQualifier>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if the AttributeQualifer is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the AttributeQualifer is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeQualifier limits the values of.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the AttributeId of the <see cref="Rock.Model.Attribute"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index("IX_AttributeIdKey", 0, IsUnique=true)]
        public int AttributeId { get; set; }
        
        /// <summary>
        /// Gets or sets the Key value that represents the type of qualifier that is being used.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the type of qualifier that is being used.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [Index( "IX_AttributeIdKey", 1, IsUnique = true )]
        public string Key { get; set; }
        
        /// <summary>
        /// Gets or sets the value of the AttributeQualifier
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the value of the AttributeQualifier.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Attribute"/> that uses this AttributeQualifier.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Attribute"/> that uses this AttributeQualifier.
        /// </value>
        public virtual Attribute Attribute { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Key;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Attribute Qualifier Configuration class.
    /// </summary>
    public partial class AttributeQualifierConfiguration : EntityTypeConfiguration<AttributeQualifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeQualifierConfiguration"/> class.
        /// </summary>
        public AttributeQualifierConfiguration()
        {
            this.HasRequired( p => p.Attribute ).WithMany( p => p.AttributeQualifiers ).HasForeignKey( p => p.AttributeId ).WillCascadeOnDelete(true);
        }
    }

    #endregion
}
