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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class AttributeValue
    {
        #region Properties

        /// <summary>
        /// Gets the Value as a decimal (Computed on Save).
        /// </summary>
        /// <value>
        /// </value>
        [DataMember]
        [LavaHidden]
        public decimal? ValueAsNumeric
        {
            get
            {
                // since this will get called on every save, don't spend time attempting to convert a large string to a decimal
                // SQL Server type is decimal(18,2) so 18 digits max with 2 being the fractional. Including the possibility of 4 commas
                // and a decimal point to get a max string length of 24 that can be turned into the SQL number type.
                if ( this.Value.IsNull() || this.Value.Length > 24 )
                {
                    _valueAsNumeric = null;
                    return _valueAsNumeric;
                }

                _valueAsNumeric = this.Value.AsDecimalOrNull();

                // If this is true then we are probably dealing with a comma delimited list and not a number.
                // In either case it won't save to the DB and needs to be handled.  Don't do the
                // rounding trick since nonnumeric attribute values should be null here.
                if ( _valueAsNumeric != null && _valueAsNumeric > ( decimal ) 9999999999999999.99 )
                {
                    _valueAsNumeric = null;
                }

                return _valueAsNumeric;
            }

            set
            {
                _valueAsNumeric = value;
            }
        }

        private decimal? _valueAsNumeric;

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
        /// Gets the value formatted.
        /// </summary>
        /// <value>
        /// The value formatted.
        /// </value>
        [LavaVisible]
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
        [LavaVisible]
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
        [LavaVisible]
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
        [LavaVisible]
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
        /// Gets or sets a value indicating whether a new AttributeValueHistory with CurrentRowIndicator needs to be saved in <see cref="SaveHook.PostSave()"/>
        /// </summary>
        /// <value>
        ///   <c>true</c> if [post save attribute value history]; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        private bool PostSaveAttributeValueHistoryCurrent { get; set; } = false;

        #endregion

        #region Methods

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
                using ( var rockContext = new RockContext() )
                {
                    attributeValue.ValueAsPersonId = new PersonAliasService( rockContext ).Queryable().Where( a => a.Guid.Equals( guid.Value ) ).Select( a => a.PersonId ).FirstOrDefault();
                }
            }

            return attributeValue;
        }

        #endregion

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
                else if ( cacheAttribute.EntityTypeId == EntityTypeCache.GetId<Rock.Model.Device>() )
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

        #endregion ICacheable

        #region History

        /// <summary>
        /// This method is called in the
        /// <see cref="M:Rock.Data.Model`1.PreSaveChanges(Rock.Data.DbContext,System.Data.Entity.Infrastructure.DbEntityEntry,System.Data.Entity.EntityState)" />
        /// method. Use it to populate <see cref="P:Rock.Data.Model`1.HistoryItems" /> if needed.
        /// These history items are queued to be written into the database post save (so that they
        /// are only written if the save actually occurs).
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        protected override void BuildHistoryItems( Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            var attributeCache = AttributeCache.Get( AttributeId );

            if ( attributeCache?.EntityTypeId == null )
            {
                return;
            }

            var entityTypeId = attributeCache.EntityTypeId.Value;
            var entityId = EntityId;
            if ( !entityId.HasValue && ( entry.State == EntityState.Modified || entry.State == EntityState.Deleted ) )
            {
                entityId = entry.OriginalValues[nameof( this.EntityId )].ToStringSafe().AsIntegerOrNull();
            }

            var caption = attributeCache.Name;

            // Check to see if this attribute is for a person or group, and if so, save to history table
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;

            var entityTypesToSaveToHistoryTable = new List<int> {
                personEntityTypeId,
                EntityTypeCache.Get( typeof( Group ) ).Id
            };

            var saveToHistoryTable = entityTypesToSaveToHistoryTable.Contains( entityTypeId );

            // If the value is not directly linked to a person or group, it still may be linked through an attribute matrix.
            // Matrix attribute changes are only logged here for modify. Add and delete are handled in the AttributeMatrixItem.
            if ( !saveToHistoryTable && state == EntityState.Modified && IsLikelyWithinMatrix() )
            {
                var rootMatrixAttributeValue = GetRootMatrixAttributeValue();

                if ( rootMatrixAttributeValue == null )
                {
                    return;
                }

                var rootMatrixAttributeCache = AttributeCache.Get( rootMatrixAttributeValue.AttributeId );

                if ( rootMatrixAttributeCache == null || !rootMatrixAttributeCache.EntityTypeId.HasValue )
                {
                    return;
                }

                saveToHistoryTable = entityTypesToSaveToHistoryTable.Contains( rootMatrixAttributeCache.EntityTypeId.Value );

                if ( saveToHistoryTable )
                {
                    // Use the values from the root matrix attribute since this is the attribute that ties the
                    // values to a person or group and are thus more meaningful
                    entityTypeId = rootMatrixAttributeCache.EntityTypeId.Value;
                    entityId = rootMatrixAttributeValue.EntityId;
                    caption = rootMatrixAttributeCache.Name;
                }
            }

            if ( !saveToHistoryTable || !entityId.HasValue )
            {
                return;
            }

            // We have determined to write to the History table. Now determine what changed.
            var oldValue = GetHistoryOldValue( entry );
            var newValue = GetHistoryNewValue( state );

            if ( oldValue == newValue )
            {
                return;
            }

            // Evaluate the history change
            var formattedOldValue = GetHistoryFormattedValue( oldValue, attributeCache );
            var formattedNewValue = GetHistoryFormattedValue( newValue, attributeCache );
            var historyChangeList = new History.HistoryChangeList();
            History.EvaluateChange( historyChangeList, attributeCache.Name, formattedOldValue, formattedNewValue, attributeCache.FieldType.Field.IsSensitive() );

            if ( !historyChangeList.Any() )
            {
                return;
            }

            var categoryGuid = entityTypeId == personEntityTypeId ?
                SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid() :
                SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid();

            HistoryItems = HistoryService.GetChanges(
                entityTypeId == personEntityTypeId ? typeof( Person ) : typeof( Group ),
                categoryGuid,
                entityId.Value,
                historyChangeList,
                caption,
                typeof( Attribute ),
                AttributeId,
                dbContext.GetCurrentPersonAlias()?.Id,
                dbContext.SourceOfChange );
        }

        /// <summary>
        /// Gets a formatted old or new value for history recording purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="attributeCache"></param>
        /// <returns></returns>
        private static string GetHistoryFormattedValue( string value, AttributeCache attributeCache )
        {
            return value.IsNotNullOrWhiteSpace() ?
                attributeCache.FieldType.Field.FormatValue( null, value, attributeCache.QualifierValues, true ) :
                string.Empty;
        }

        /// <summary>
        /// Determines whether this attribute value is part of an attribute matrix.
        /// This is determined by checking if the attribute's entity type is
        /// <see cref="AttributeMatrixItem" />. This does not guarantee that the value
        /// is part of a matrix of values, but it is a good indicator to check before
        /// doing more expensive queries like <see cref="GetRootMatrixAttributeValue" />
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is within matrix]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLikelyWithinMatrix()
        {
            var attributeCache = AttributeCache.Get( AttributeId );
            var entityTypeId = EntityTypeCache.GetId( typeof( AttributeMatrixItem ) );
            return attributeCache?.EntityTypeId != null && attributeCache.EntityTypeId == entityTypeId;
        }

        /// <summary>
        /// Gets the root matrix attribute value that ties the matrix values to a person or other entity.
        /// </summary>
        /// <returns></returns>
        private AttributeValue GetRootMatrixAttributeValue()
        {
            var rockContext = new RockContext();
            var attributeMatrixService = new AttributeMatrixService( rockContext );
            var attributeService = new AttributeService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );

            var matrixGuidQuery = attributeMatrixService.Queryable().AsNoTracking().Where( am =>
                am.AttributeMatrixItems.Any( ami => ami.Id == EntityId ) )
                .Select( am => am.Guid.ToString() );

            var matrixFieldType = FieldTypeCache.Get( SystemGuid.FieldType.MATRIX );
            var attributeIdQuery = attributeService.Queryable().AsNoTracking().Where( a =>
                a.FieldTypeId == matrixFieldType.Id )
                .Select( a => a.Id );

            var attributeValue = attributeValueService.Queryable().AsNoTracking().FirstOrDefault( av =>
                 attributeIdQuery.Contains( av.AttributeId ) && matrixGuidQuery.Contains( av.Value ) );

            return attributeValue;
        }

        /// <summary>
        /// Get the new value for a history record
        /// </summary>
        /// <returns></returns>
        private string GetHistoryNewValue( EntityState state )
        {
            switch ( state )
            {
                case EntityState.Added:
                case EntityState.Modified:
                    return Value;
                case EntityState.Deleted:
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Get the old value for a history record
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private static string GetHistoryOldValue( DbEntityEntry entry )
        {
            switch ( entry.State )
            {
                case EntityState.Added:
                    return string.Empty;
                case EntityState.Modified:
                case EntityState.Deleted:
                default:
                    return entry.OriginalValues[nameof( Value )].ToStringSafe();
            }
        }

        #endregion History
    }
}
