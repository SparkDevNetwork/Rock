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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// A DefinedType is a dictionary of consistent values for a particular thing in Rock. The individual items are refereed to as <see cref="Rock.Model.DefinedValue">DefinedValues</see> in Rock.  
    /// Several classic examples of DefinedTypes can be Shirt Sizes, a Country List, etc. Defined Values can be categorized, ordered and can be furthered specified by a <see cref="Rock.Model.FieldType"/>
    /// </summary>
    /// <remarks>
    /// Note: in some systems these are referred to as lookup values. The benefit of storing these values centrally is that it prevents us having to maintain <see cref="Rock.Model.EntityType">EntityTypes</see>
    /// for each defined value/lookup that you want to create.  In the case of attributes these can be created as the need arises without having to change the core base or add a plug-in just to 
    /// provide additional lookup data.
    /// </remarks>
    [Table( "DefinedType" )]
    [DataContract]
    public partial class DefinedType : Model<DefinedType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this DefinedType is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this DefinedType is part of the Rock core system/framework; otherwise this value is <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the FieldTypeId of the <see cref="Rock.Model.FieldType"/> that is used to set/select, and at times display the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that are associated with
        /// NOTE: Currently, Text is the only supported fieldType for DefinedTypes.
        /// this DefinedType. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the FieldTypeId of the <see cref="Rock.Model.FieldType"/> that is used for DefinedType.
        /// </value>
        [DataMember]
        public int? FieldTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the display order of this DefinedType.  The lower the number the higher the display priority.  This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the display order of this DefinedType.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }
        
        /// <summary>
        /// Gets or sets the Name of the DefinedType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the DefinedType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined description of the DefinedType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the DefinedType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the help text for the defined type.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the help text.
        /// </value>
        [DataMember]
        public string HelpText { get; set; }

        #endregion 

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to this DefinedType.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to this DefinedType.
        /// </value>
        [DataMember]
        public virtual ICollection<DefinedValue> DefinedValues { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FieldType"/> that is used to set/select, and at times display the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that are associated with
        /// this DefinedType. 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FieldType"/>.
        /// </value>
        [DataMember]
        public virtual FieldType FieldType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this DefinedType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this DefinedType.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Defined Type Configuration class.
    /// </summary>
    public partial class DefinedTypeConfiguration : EntityTypeConfiguration<DefinedType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedTypeConfiguration"/> class.
        /// </summary>
        public DefinedTypeConfiguration()
        {
            this.HasOptional( t => t.Category ).WithMany().HasForeignKey( t => t.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FieldType ).WithMany().HasForeignKey( t => t.FieldTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
