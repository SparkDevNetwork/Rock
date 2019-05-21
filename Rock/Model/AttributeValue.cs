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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a value of an <see cref="Rock.Model.Attribute"/>. 
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeValue" )]
    [DataContract]
    [JsonConverter( typeof( Rock.Utility.AttributeValueJsonConverter ) )]
    public partial class AttributeValue : Model<AttributeValue>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Creates the non persisted attribute value.
        /// Warning: This should NOT be used to create an AttributeValue that is stored in the database.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        static internal AttributeValue CreateNonPersistedAttributeValue( string value )
        {
            var attributeValue = new AttributeValue()
            {
                Value = value,
                ValueAsBoolean = value.AsBooleanOrNull(),
                ValueAsDateTime = value.AsDateTime(),
                ValueAsNumeric = value.AsDecimalOrNull()
            };

            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext())
                {
                    attributeValue.ValueAsPersonId = new PersonAliasService( rockContext).Queryable().Where( a => a.Guid.Equals( guid.Value ) ).Select( a => a.PersonId).FirstOrDefault();
                }
            }

            return attributeValue;
        }

        /// <summary>
        /// Gets or sets a flag indicating if this AttributeValue is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if is part of the Rock core system/framework.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [LavaIgnore]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeValue provides a value for.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeValue provides a value for.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity instance that uses this AttributeValue. An <see cref="Rock.Model.Attribute"/> is a configuration setting, so each 
        /// instance of the Entity that uses the same Attribute can have a different value.  For instance a <see cref="Rock.Model.BlockType"/> has a declared attribute, and that attribute can be configured 
        /// with a different value on each <see cref="Rock.Model.Block"/> that implements the <see cref="Rock.Model.BlockType"/>. This value will either be 0 or null for global attributes or attributes that have a 
        /// constant across all instances of an EntityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that identifies the Id of the entity instance that uses this AttributeValue.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the value.
        /// </value>
        [DataMember]
        public string Value
        {
            get
            {
                return _value ?? string.Empty;
            }

            set
            {
                _value = value;
            }
        }

        private string _value = string.Empty;

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the Value as a double (Computed Column)
        /// </summary>
        /// <value>
        /// </value>
        /* Computed Column Spec:
        CASE 
        WHEN len([value]) < (100)
            AND isnumeric([value]) = (1)
            AND NOT [value] LIKE '%[^0-9.]%'
            AND NOT [value] LIKE '%[.]%'
            THEN CONVERT([numeric](38, 10), [value])
        END         
         */
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaIgnore]
        public decimal? ValueAsNumeric { get; set; }

        /// <summary>
        /// Gets the Value as a DateTime (maintained by SQL Trigger on AttributeValue)
        /// </summary>
        /// <remarks>
        /// see tgrAttributeValue_InsertUpdate                                                                                    
        /// </remarks>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaIgnore]
        public DateTime? ValueAsDateTime { get; internal set; }

        /// <summary>
        /// Gets the value as boolean (computed column)
        /// </summary>
        /// <value>
        /// The value as boolean.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaIgnore]
        public bool? ValueAsBoolean { get; internal set; }

        /// <summary>
        /// Gets a person alias guid value as a PersonId (ComputedColumn).
        /// </summary>
        /// <remarks>
        /// Computed Column Spec:
        /// case 
        ///     when [Value] like '________-____-____-____-____________' 
        ///         then [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid]([Value]) 
        ///     else null 
        /// end
        /// </remarks>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaIgnore]
        public int? ValueAsPersonId { get; private set; }

        /// <summary>
        /// Gets the <see cref="Rock.Model.FieldType"/> that represents the type of value that is being represented by the AttributeValue, and provides a UI for the user to set the value.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FieldType"/> that is represented by this Attribute Value.
        /// </value>
        [NotMapped]
        private Rock.Field.IFieldType FieldType
        {
            get
            {
                Rock.Field.IFieldType result = null;
                Rock.Web.Cache.AttributeCache attribute = Rock.Web.Cache.AttributeCache.Get( this.AttributeId );
                if ( attribute != null )
                {
                    if ( attribute.FieldType != null )
                    {
                        result = attribute.FieldType.Field;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Attribute"/> that uses this AttributeValue.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Attribute"/> that uses this value.
        /// </value>
        [DataMember]
        [LavaIgnore]
        public virtual Attribute Attribute { get; set; }

        /// <summary>
        /// Gets or sets the a list of previous values that this attribute value had (If Attribute.EnableHistory is enabled)
        /// </summary>
        /// <value>
        /// The attribute values historical.
        /// </value>
        [DataMember]
        [LavaIgnore]
        public virtual ICollection<AttributeValueHistorical> AttributeValuesHistorical { get; set; } = new Collection<AttributeValueHistorical>();

        /// <summary>
        /// Gets the value formatted.
        /// </summary>
        /// <value>
        /// The value formatted.
        /// </value>
        [LavaInclude]
        public virtual string ValueFormatted
        {
            get
            {
                var attribute = AttributeCache.Get( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.FieldType.Field.FormatValue( null, attribute.EntityTypeId, this.EntityId, Value, attribute.QualifierValues, false );
                }

                return Value;
            }
        }

        /// <summary>
        /// Gets the name of the attribute 
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The name of the attribute.
        /// </value>
        [LavaInclude]
        public virtual string AttributeName
        {
            get
            {
                var attribute = AttributeCache.Get( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.Name;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The attribute key.
        /// </value>
        [LavaInclude]
        public virtual string AttributeKey
        {
            get
            {
                var attribute = AttributeCache.Get( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.Key;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether attribute is grid column.
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// <c>true</c> if [attribute is grid column]; otherwise, <c>false</c>.
        /// </value>
        [LavaInclude]
        public virtual bool AttributeIsGridColumn
        {
            get
            {
                var attribute = AttributeCache.Get( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.IsGridColumn;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets or sets the history changes to be saved in PostSaveChanges
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        private History.HistoryChangeList HistoryChanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new AttributeValueHistory with CurrentRowIndicator needs to be saved in PostSaveChanges
        /// </summary>
        /// <value>
        ///   <c>true</c> if [post save attribute value history]; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        private bool PostSaveAttributeValueHistoryCurrent { get; set; } = false;

        /// <summary>
        /// Gets or sets the type of the history entity.
        /// </summary>
        /// <value>
        /// The type of the history entity.
        /// </value>
        [NotMapped]
        private int? HistoryEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the history entity identifier.
        /// </summary>
        /// <value>
        /// The history entity identifier.
        /// </value>
        [NotMapped]
        private int? HistoryEntityId { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var attributeCache = AttributeCache.Get( this.AttributeId );
            if ( attributeCache != null )
            {
                // Check to see if this attribute value if for a File, Image, or BackgroundCheck field type.
                // If so then delete the old binary file for UPDATE and DELETE and make sure new binary files are not temporary
                // This path needs to happen for FieldTypes that actually upload/create a file but not for FieldTypes that consist of already existing files.
                // This attribute value should not effect a file that anything could be using.
                // The Label field type is a list of existing labels so should not be included, but the image field type uploads a new file so we do want it included.
                // Don't use BinaryFileFieldType as that type of attribute's file can be used by more than one attribute
                var field = attributeCache.FieldType.Field;
                if ( field != null && (
                    field is Field.Types.FileFieldType ||
                    field is Field.Types.ImageFieldType ||
                    field is Field.Types.BackgroundCheckFieldType ) )
                {
                    PreSaveBinaryFile( dbContext, entry );
                }

                // Check to see if this attribute is for a person or group, and if so, save to history table
                bool saveToHistoryTable = attributeCache.EntityTypeId.HasValue &&
                        ( attributeCache.EntityTypeId.Value == EntityTypeCache.Get( typeof( Person ) ).Id
                          || attributeCache.EntityTypeId.Value == EntityTypeCache.Get( typeof( Group ) ).Id );

                if ( saveToHistoryTable || attributeCache.EnableHistory )
                {
                    SaveToHistoryTable( dbContext, entry, attributeCache, saveToHistoryTable );
                }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            int? historyEntityId = ( HistoryEntityId.HasValue && HistoryEntityId.Value > 0 ) ? HistoryEntityId.Value : this.EntityId;
            var rockContext = dbContext as RockContext;
            if ( HistoryChanges != null && HistoryChanges.Any() && HistoryEntityTypeId.HasValue && historyEntityId.HasValue )
            {
                if ( HistoryEntityTypeId.Value == EntityTypeCache.Get( typeof( Person ) ).Id )
                {
                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), historyEntityId.Value, HistoryChanges, string.Empty, typeof( Attribute ), AttributeId, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );
                }
                else
                {
                    HistoryService.SaveChanges( rockContext, typeof( Group ), Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(), historyEntityId.Value, HistoryChanges, string.Empty, typeof( Attribute ), AttributeId, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );
                }
            }

            if ( this.PostSaveAttributeValueHistoryCurrent )
            {
                var attributeValueHistoricalService = new AttributeValueHistoricalService( rockContext );
                var attributeValueHistoricalPreviousCurrentRow = attributeValueHistoricalService.Queryable().Where( a => a.AttributeValueId == this.Id && a.CurrentRowIndicator == true ).FirstOrDefault();
                var saveChangesDateTime = RockDateTime.Now;

                if ( attributeValueHistoricalPreviousCurrentRow != null )
                {
                    attributeValueHistoricalPreviousCurrentRow.CurrentRowIndicator = false;
                    attributeValueHistoricalPreviousCurrentRow.ExpireDateTime = saveChangesDateTime;
                }

                var attributeValueHistoricalCurrent = AttributeValueHistorical.CreateCurrentRowFromAttributeValue( this, saveChangesDateTime );

                attributeValueHistoricalService.Add( attributeValueHistoricalCurrent );
                rockContext.SaveChanges();
            }

            // If this a Person Attribute, Update the ModifiedDateTime on the Person that this AttributeValue is associated with
            if ( this.EntityId.HasValue && AttributeCache.Get( this.AttributeId )?.EntityTypeId == EntityTypeCache.Get<Rock.Model.Person>().Id )
            {
                var currentDateTime = RockDateTime.Now;
                int personId = this.EntityId.Value;
                var qryPersonsToUpdate = new PersonService( rockContext ).Queryable( true, true ).Where( a => a.Id == personId );
                rockContext.BulkUpdate( qryPersonsToUpdate, p => new Person { ModifiedDateTime = currentDateTime, ModifiedByPersonAliasId = this.ModifiedByPersonAliasId } );
            }

            base.PostSaveChanges( dbContext );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Value;
        }

        #endregion  Public Methods

        #region Protected Methods

        /// <summary>
        /// Delete the old binary file for UPDATE and DELETE and make sure new binary files are not temporary
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        protected void PreSaveBinaryFile( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            Guid? newBinaryFileGuid = null;
            Guid? oldBinaryFileGuid = null;

            if ( entry.State == EntityState.Added || entry.State == EntityState.Modified )
            {
                newBinaryFileGuid = Value.AsGuidOrNull();
            }

            if ( entry.State == EntityState.Modified || entry.State == EntityState.Deleted )
            {
                oldBinaryFileGuid = entry.OriginalValues["Value"]?.ToString().AsGuidOrNull();
            }

            if ( oldBinaryFileGuid.HasValue )
            {
                if ( !newBinaryFileGuid.HasValue || !newBinaryFileGuid.Value.Equals( oldBinaryFileGuid.Value ) )
                {
                    var transaction = new Rock.Transactions.DeleteAttributeBinaryFile( oldBinaryFileGuid.Value );
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                }
            }

            if ( newBinaryFileGuid.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( ( RockContext ) dbContext );
                var binaryFile = binaryFileService.Get( newBinaryFileGuid.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }
        }

        /// <summary>
        /// Saves to history table.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="attributeCache">The attribute cache.</param>
        /// <param name="saveToHistoryTable">if set to <c>true</c> [save to history table].</param>
        protected void SaveToHistoryTable( Rock.Data.DbContext dbContext, DbEntityEntry entry, AttributeCache attributeCache, bool saveToHistoryTable )
        {
            string oldValue = string.Empty;
            string newValue = string.Empty;

            HistoryEntityTypeId = attributeCache.EntityTypeId.Value;
            HistoryEntityId = EntityId;

            switch ( entry.State )
            {
                case EntityState.Added:
                    newValue = Value;
                    break;

                case EntityState.Modified:
                    oldValue = entry.OriginalValues["Value"].ToStringSafe();
                    newValue = Value;
                    break;

                case EntityState.Deleted:
                    HistoryEntityId = entry.OriginalValues["EntityId"].ToStringSafe().AsIntegerOrNull();
                    oldValue = entry.OriginalValues["Value"].ToStringSafe();
                    return;
            }

            this.PostSaveAttributeValueHistoryCurrent = false;

            if ( oldValue == newValue )
            {
                return;
            }

            var formattedOldValue = oldValue.IsNotNullOrWhiteSpace() ? attributeCache.FieldType.Field.FormatValue( null, oldValue, attributeCache.QualifierValues, true ) : string.Empty;
            var formattedNewValue = newValue.IsNotNullOrWhiteSpace() ? attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, true ) : string.Empty;

            if ( saveToHistoryTable )
            {
                HistoryChanges = new History.HistoryChangeList();
                History.EvaluateChange( HistoryChanges, attributeCache.Name, formattedOldValue, formattedNewValue, attributeCache.FieldType.Field.IsSensitive() );
            }

            if ( !attributeCache.EnableHistory )
            {
                return;
            }

            // value changed and attribute.EnableHistory = true, so flag PostSaveAttributeValueHistoryCurrent
            this.PostSaveAttributeValueHistoryCurrent = true;

            var attributeValueHistoricalService = new AttributeValueHistoricalService( dbContext as RockContext );

            if ( this.Id < 1 )
            {
                return;
            }

            // this is an existing AttributeValue, so fetch the AttributeValue that is currently marked as CurrentRow for this attribute value (if it exists)
            bool hasAttributeValueHistoricalCurrentRow = attributeValueHistoricalService.Queryable().Where( a => a.AttributeValueId == this.Id && a.CurrentRowIndicator == true ).Any();

            if ( !hasAttributeValueHistoricalCurrentRow )
            {
                // this is an existing AttributeValue but there isn't a CurrentRow AttributeValueHistorical for this AttributeValue yet, so create it off of the OriginalValues
                AttributeValueHistorical attributeValueHistoricalPreviousCurrentRow = new AttributeValueHistorical
                {
                    AttributeValueId = this.Id,
                    Value = oldValue,
                    ValueFormatted = formattedOldValue,
                    ValueAsNumeric = entry.OriginalValues["ValueAsNumeric"] as decimal?,
                    ValueAsDateTime = entry.OriginalValues["ValueAsDateTime"] as DateTime?,
                    ValueAsBoolean = entry.OriginalValues["ValueAsBoolean"] as bool?,
                    ValueAsPersonId = entry.OriginalValues["ValueAsPersonId"] as int?,
                    EffectiveDateTime = entry.OriginalValues["ModifiedDateTime"] as DateTime? ?? RockDateTime.Now,
                    CurrentRowIndicator = true,
                    ExpireDateTime = HistoricalTracking.MaxExpireDateTime
                };

                attributeValueHistoricalService.Add( attributeValueHistoricalPreviousCurrentRow );
            }
        }

        #endregion Protected Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            // no cache entity associated with AttributeValue
            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            AttributeCache cacheAttribute = AttributeCache.Get( this.AttributeId, dbContext as RockContext );

            if ( cacheAttribute == null )
            {
                return;
            }

            if ( this.EntityId.HasValue && cacheAttribute.EntityTypeId.HasValue )
            {
                EntityTypeCache entityType = EntityTypeCache.Get( cacheAttribute.EntityTypeId.Value, dbContext as RockContext );

                if ( entityType?.HasEntityCache() == true )
                {
                    entityType.FlushCachedItem( this.EntityId.Value );
                }
                else if ( cacheAttribute.EntityTypeId == EntityTypeCache.GetId<Rock.Model.Device>())
                {
                    Rock.CheckIn.KioskDevice.FlushItem( this.EntityId.Value );
                }
            }

            if ( ( !cacheAttribute.EntityTypeId.HasValue || cacheAttribute.EntityTypeId.Value == 0 ) && string.IsNullOrEmpty( cacheAttribute.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( cacheAttribute.EntityTypeQualifierValue ) )
            {
                // Update GlobalAttributes if one of the values changed
                GlobalAttributesCache.Remove();
            }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Attribute Value Configuration class.
    /// </summary>
    public partial class AttributeValueConfiguration : EntityTypeConfiguration<AttributeValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueConfiguration"/> class.
        /// </summary>
        public AttributeValueConfiguration()
        {
            this.HasRequired( p => p.Attribute ).WithMany().HasForeignKey( p => p.AttributeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
