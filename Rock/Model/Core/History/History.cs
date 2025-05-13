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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a history that is entered in Rock and is associated with a specific entity. For example, a history could be entered on a person, GroupMember, a device, etc or for a specific subset of an entity type.
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "History" )]
    [DataContract]
    [CodeGenerateRest( Enums.CodeGenerateRestEndpoint.ReadOnly )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.HISTORY )]
    public partial class History : Model<History>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this history is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this history is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Category"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Category"/>
        /// </value>
        [Required]
        [DataMember]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EntityType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EntityType"/>
        /// </value>
        [Required]
        [DataMember]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this history is related to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this history is related to.
        /// </value>
        [Required]
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the verb which is a structured (for querying) field to describe what the action is (ADD, DELETE, UPDATE, VIEW, WATCHED,  etc).
        /// <see cref="History.HistoryVerb"/> constants for common verbs
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the verb of the History.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets the caption
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the caption of the History.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the related entity type identifier.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        ///
        [DataMember]
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the related data.
        /// </summary>
        /// <value>
        /// The related data.
        /// </value>
        [DataMember]
        public string RelatedData { get; set; }

        /// <summary>
        /// Gets or sets the ChangeType which is a structured (for querying) field to describe what type of data was changed (Record, Property, Attribute, Location, Schedule, etc)
        /// <see cref="History.HistoryChangeType"/> constants for common change types
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the name of the value depending on ChangeType: ChangeTypeName.Property => Property Friendly Name, ChangeType.Attribute => Attribute Name, ChangeType.Record => the ToString of the record
        /// </summary>
        /// <value>
        /// The name of the value.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string ValueName { get; set; }

        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        /// <value>
        /// The new value.
        /// </value>
        [DataMember]
        public string NewValue { get; set; }

        /// <summary>
        /// Creates new rawvalue.
        /// </summary>
        /// <value>
        /// The new raw value.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string NewRawValue { get; set; }

        /// <summary>
        /// Gets or sets the old value.
        /// </summary>
        /// <value>
        /// The old value.
        /// </value>
        [DataMember]
        public string OldValue { get; set; }

        /// <summary>
        /// Gets or sets the old raw value.
        /// </summary>
        /// <value>
        /// The old raw value.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string OldRawValue { get; set; }

        /// <summary>
        /// Gets or sets whether the NewValue and/or OldValue is null because the value is sensitive data that shouldn't be logged
        /// If "IsSensitive" doesn't apply to this, it can be left null
        /// </summary>
        /// <value>
        /// IsSensitive.
        /// </value>
        [DataMember]
        public bool? IsSensitive { get; set; }

        /// <summary>
        /// Optional: Gets or sets name of the tool or process that changed the value
        /// </summary>
        /// <value>
        /// The source of change.
        /// </value>
        [DataMember]
        public string SourceOfChange { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the entity type this history is associated with
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of this history.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the type of the related entity.
        /// </summary>
        /// <value>
        /// The type of the related entity.
        /// </value>
        [DataMember]
        public virtual EntityType RelatedEntityType { get; set; }

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
            return this.SummaryHtml;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// History Configuration class.
    /// </summary>
    public partial class HistoryConfiguration : EntityTypeConfiguration<History>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryConfiguration"/> class.
        /// </summary>
        public HistoryConfiguration()
        {
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Category ).WithMany().HasForeignKey( p => p.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RelatedEntityType ).WithMany().HasForeignKey( p => p.RelatedEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}