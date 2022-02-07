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
using System.Linq;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class AttributeValue
    {
        /// <summary>
        /// Save hook implementation for <see cref="AttributeValue"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<AttributeValue>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                var attributeCache = AttributeCache.Get( Entity.AttributeId );

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
                        PreSaveBinaryFile( rockContext );
                    }

                    // Check to see if this attribute value is for a StructureContentEditorFieldType.
                    // If so then we need to detect any changes in the content blocks.
                    if ( field is Field.Types.StructureContentEditorFieldType )
                    {
                        PreSaveStructuredContent( rockContext );
                    }

                    // Save to the historical table if history is enabled
                    if ( attributeCache.EnableHistory )
                    {
                        SaveToHistoryTable( rockContext, attributeCache, true );
                    }
                }

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                var rockContext = this.RockContext;

                if ( Entity.PostSaveAttributeValueHistoryCurrent )
                {
                    var attributeValueHistoricalService = new AttributeValueHistoricalService( rockContext );
                    var attributeValueHistoricalPreviousCurrentRow = attributeValueHistoricalService.Queryable().Where( a => a.AttributeValueId == Entity.Id && a.CurrentRowIndicator == true ).FirstOrDefault();
                    var saveChangesDateTime = RockDateTime.Now;

                    if ( attributeValueHistoricalPreviousCurrentRow != null )
                    {
                        attributeValueHistoricalPreviousCurrentRow.CurrentRowIndicator = false;
                        attributeValueHistoricalPreviousCurrentRow.ExpireDateTime = saveChangesDateTime;
                    }

                    var attributeValueHistoricalCurrent = AttributeValueHistorical.CreateCurrentRowFromAttributeValue( Entity, saveChangesDateTime );

                    attributeValueHistoricalService.Add( attributeValueHistoricalCurrent );
                    rockContext.SaveChanges();
                }

                // If this a Person Attribute, Update the ModifiedDateTime on the Person that this AttributeValue is associated with.
                // For example, if the FavoriteColor attribute of Ted Decker is changed from Red to Blue, we'll update Ted's Person.ModifiedDateTime.
                if ( Entity.EntityId.HasValue && AttributeCache.Get( Entity.AttributeId )?.EntityTypeId == EntityTypeCache.Get<Rock.Model.Person>().Id )
                {
                    // since this could get called several times (one for each of changed Attributes on a person), do a direct SQL to minimize overhead
                    var currentDateTime = RockDateTime.Now;
                    int personId = Entity.EntityId.Value;
                    if ( Entity.ModifiedByPersonAliasId.HasValue )
                    {
                        rockContext.Database.ExecuteSqlCommand(
                            $"UPDATE [Person] SET [ModifiedDateTime] = @modifiedDateTime, [ModifiedByPersonAliasId] = @modifiedByPersonAliasId WHERE [Id] = @personId",
                            new System.Data.SqlClient.SqlParameter( "@modifiedDateTime", currentDateTime ),
                            new System.Data.SqlClient.SqlParameter( "@modifiedByPersonAliasId", Entity.ModifiedByPersonAliasId.Value ),
                            new System.Data.SqlClient.SqlParameter( "@personId", personId ) );
                    }
                    else
                    {
                        rockContext.Database.ExecuteSqlCommand(
                            $"UPDATE [Person] SET [ModifiedDateTime] = @modifiedDateTime, [ModifiedByPersonAliasId] = NULL WHERE [Id] = @personId",
                            new System.Data.SqlClient.SqlParameter( "@modifiedDateTime", currentDateTime ),
                            new System.Data.SqlClient.SqlParameter( "@personId", personId ) );
                    }
                }

                base.PostSave();
            }

            /// <summary>
            /// Delete the old binary file for UPDATE and DELETE and make sure new binary files are not temporary
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            private void PreSaveBinaryFile( RockContext rockContext )
            {
                Guid? newBinaryFileGuid = null;
                Guid? oldBinaryFileGuid = null;

                if ( State == EntityContextState.Added || State == EntityContextState.Modified )
                {
                    newBinaryFileGuid = Entity.Value.AsGuidOrNull();
                }

                if ( State == EntityContextState.Modified || State == EntityContextState.Deleted )
                {
                    oldBinaryFileGuid = Entry.OriginalValues[ nameof( Entity.Value )]?.ToString().AsGuidOrNull();
                }

                if ( oldBinaryFileGuid.HasValue )
                {
                    if ( !newBinaryFileGuid.HasValue || !newBinaryFileGuid.Value.Equals( oldBinaryFileGuid.Value ) )
                    {
                        var deleteBinaryFileAttributeMsg = new DeleteBinaryFileAttribute.Message()
                        {
                            BinaryFileGuid = oldBinaryFileGuid.Value
                        };

                        deleteBinaryFileAttributeMsg.Send();
                    }
                }

                if ( newBinaryFileGuid.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( newBinaryFileGuid.Value );
                    if ( binaryFile != null && binaryFile.IsTemporary )
                    {
                        binaryFile.IsTemporary = false;
                    }
                }
            }

            /// <summary>
            /// Processes the PreSave event when this value is for
            /// <see cref="Field.Types.StructureContentEditorFieldType"/>. Detect any
            /// changes to the internal content and apply them to the database as well.
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            private void PreSaveStructuredContent( RockContext rockContext )
            {
                string content = State == EntityContextState.Added || State == EntityContextState.Modified ? Entity.Value : string.Empty;
                string oldContent = State == EntityContextState.Modified ? Entry.OriginalValues[nameof( Entity.Value )] as string : string.Empty;

                var helper = new StructuredContentHelper( content );
                var changes = helper.DetectChanges( oldContent );

                helper.ApplyDatabaseChanges( changes, rockContext );
            }

            /// <summary>
            /// Saves to attribute historical value table.
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            /// <param name="attributeCache">The attribute cache.</param>
            /// <param name="saveToHistoryTable">if set to <c>true</c> [save to history table].</param>
            private void SaveToHistoryTable( RockContext rockContext, AttributeCache attributeCache, bool saveToHistoryTable )
            {
                Entity.PostSaveAttributeValueHistoryCurrent = false;

                var oldValue = GetHistoryOldValue();
                var newValue = GetHistoryNewValue();

                if ( oldValue == newValue || !attributeCache.EnableHistory )
                {
                    return;
                }

                var formattedOldValue = GetHistoryFormattedValue( oldValue, attributeCache );

                // value changed and attribute.EnableHistory = true, so flag PostSaveAttributeValueHistoryCurrent
                Entity.PostSaveAttributeValueHistoryCurrent = true;

                var attributeValueHistoricalService = new AttributeValueHistoricalService( rockContext as RockContext );

                if ( Entity.Id < 1 )
                {
                    return;
                }

                // this is an existing AttributeValue, so fetch the AttributeValue that is currently marked as CurrentRow for this attribute value (if it exists)
                bool hasAttributeValueHistoricalCurrentRow = attributeValueHistoricalService.Queryable().Where( a => a.AttributeValueId == Entity.Id && a.CurrentRowIndicator == true ).Any();

                if ( !hasAttributeValueHistoricalCurrentRow )
                {
                    // this is an existing AttributeValue but there isn't a CurrentRow AttributeValueHistorical for this AttributeValue yet, so create it off of the OriginalValues
                    AttributeValueHistorical attributeValueHistoricalPreviousCurrentRow = new AttributeValueHistorical
                    {
                        AttributeValueId = Entity.Id,
                        Value = oldValue,
                        ValueFormatted = formattedOldValue,
                        ValueAsNumeric = Entry.OriginalValues[nameof( Entity.ValueAsNumeric )] as decimal?,
                        ValueAsDateTime = Entry.OriginalValues[nameof( Entity.ValueAsDateTime )] as DateTime?,
                        ValueAsBoolean = Entry.OriginalValues[nameof( Entity.ValueAsBoolean )] as bool?,
                        ValueAsPersonId = Entry.OriginalValues[nameof( Entity.ValueAsPersonId )] as int?,
                        EffectiveDateTime = Entry.OriginalValues[nameof( Entity.ModifiedDateTime )] as DateTime? ?? RockDateTime.Now,
                        CurrentRowIndicator = true,
                        ExpireDateTime = HistoricalTracking.MaxExpireDateTime
                    };

                    attributeValueHistoricalService.Add( attributeValueHistoricalPreviousCurrentRow );
                }
            }

            /// <summary>
            /// Get the new value for a history record
            /// </summary>
            /// <returns></returns>
            private string GetHistoryNewValue()
            {
                switch ( State )
                {
                    case EntityContextState.Added:
                    case EntityContextState.Modified:
                        return Entity.Value;
                    case EntityContextState.Deleted:
                    default:
                        return string.Empty;
                }
            }

            /// <summary>
            /// Get the old value for a history record
            /// </summary>
            /// <returns></returns>
            private string GetHistoryOldValue()
            {
                switch ( State )
                {
                    case EntityContextState.Added:
                        return string.Empty;
                    case EntityContextState.Modified:
                    case EntityContextState.Deleted:
                    default:
                        return Entry.OriginalValues[nameof( AttributeValue.Value )].ToStringSafe();
                }
            }
        }
    }
}
