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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type or category of <see cref="Rock.Model.Note">Notes</see> in Rock, and configures the type of entities that notes of this type apply to other settings
    /// specific to the type of note.
    /// </summary>
    [Table( "NoteType" )]
    [DataContract]
    public partial class NoteType : Model<NoteType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating that this NoteType is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this NoteType is part of the core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EntityType"/> that this NoteType is used for.  A NoteType can only be associated with a single <see cref="Rock.Model.EntityType"/> and will 
        /// only contain notes for entities of this type. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EntityType"/>
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the qualifier column/property on the <see cref="Rock.Model.EntityType"/> that this NoteType applies to. If this is not 
        /// provided, the note type can be used on all entities of the provided <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the qualifier column that this NoteType applies to.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value in the qualifier column that this note type applies to.  For instance this note type and related notes will only be applicable to entity 
        /// if the value in the EntityTypeQualiferColumn matches this value. This property should not be populated without also populating the EntityTypeQualifierColumn property.
        /// </summary>
        /// <value>
        /// Entity Type Qualifier Value.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the Name of the NoteType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the NoteType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is user selectable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user selectable]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UserSelectable { get; set; }

        /// <summary>
        /// Gets or sets an optional CSS class to include for the note
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the name of an icon CSS class. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of an icon CSS class
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the entities that <see cref="Rock.Model.Note">Notes</see> of this NoteType 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that this NoteType is associated with.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration    

    /// <summary>
    /// Note Type Configuration class.
    /// </summary>
    public partial class NoteTypeConfiguration : EntityTypeConfiguration<NoteType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoteTypeConfiguration"/> class.
        /// </summary>
        public NoteTypeConfiguration()
        {
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
