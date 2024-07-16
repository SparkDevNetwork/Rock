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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class AttributeMatrixItem
    {
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
            /*
             * 18-Dec-2019 BJW
             *
             * We want to log the history of attribute values within a person matrix. Most of this logging occurs from
             * the attribute value model. However, when a matrix item (row in the table) is deleted, the pre-save event
             * of the attribute values is not called. Therefore, the delete event needs to be logged here. Additionally,
             * when the matrix item is added, the history is much cleaner when added here so that all the values of the
             * row are consolidated to one history item.  Modified state is not possible to log here because the
             * matrix item is not actually modified when its attributes change.
             *
             * Task: https://app.asana.com/0/1120115219297347/1136643182208595/f
             */

            if ( state != EntityState.Deleted && state != EntityState.Added )
            {
                return;
            }

            // Get the AttributeMatrix associated with this AttributeMatrixItem.
            var rockContext = new RockContext();

            int? matrixId = AttributeMatrixId;
            if ( matrixId == default
                 && state == EntityState.Added )
            {
                matrixId = entry.OriginalValues[nameof( this.AttributeMatrixId )].ToStringSafe().AsIntegerOrNull();
            }

            var matrix = AttributeMatrix;
            if ( matrix == null && matrixId.HasValue )
            {
                var matrixService = new AttributeMatrixService( rockContext );
                matrix = matrixService.Queryable()
                    .AsNoTracking()
                    .FirstOrDefault( am => am.Id == matrixId );
            }
            if ( matrix == null )
            {
                return;
            }

            // Get the target entity for the AttributeMatrix.
            // The AttributeMatrix is linked to the target entity via a Guid reference stored in an Attribute of the target entity.
            var matrixGuidString = matrix.Guid.ToString();
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
            var attributeValueService = new AttributeValueService( rockContext );

            // The most efficient method of finding the Attribute Value that holds a reference to this AttributeMatrix
            // is by searching the ValueChecksum index. After locating the candidate records matching the checksum,
            // filter the matches to verify the actual value, just in case we have a checksum collision.
            // Finally, verify that the target entity is a Person - if not, history is ignored.
            var matrixAttributeValue = attributeValueService.Queryable()
                .AsNoTracking()
                .FirstOrDefault( av => av.ValueChecksum == SqlFunctions.Checksum( matrixGuidString )
                    && av.Value.Equals( matrixGuidString, System.StringComparison.OrdinalIgnoreCase )
                    && av.Attribute.EntityTypeId == personEntityTypeId );

            if ( matrixAttributeValue?.EntityId == null )
            {
                return;
            }

            // Get the Attribute associated with the matrix value, and then locate the specific target entity to which the AttributeMatrix is attached.
            var matrixAttributeCache = AttributeCache.Get( matrixAttributeValue.AttributeId );
            if ( matrixAttributeCache == null )
            {
                return;
            }

            // Evaluate the history changes.
            var historyChangeList = new History.HistoryChangeList();

            if ( AttributeValues == null || !AttributeValues.Any() )
            {
                this.LoadAttributes();
            }

            var isDelete = ( state == EntityState.Deleted );

            foreach ( var attributeValue in AttributeValues.Values )
            {
                var attributeCache = AttributeCache.Get( attributeValue.AttributeId );
                var formattedOldValue = isDelete ? GetHistoryFormattedValue( attributeValue.Value, attributeCache ) : string.Empty;
                var formattedNewValue = isDelete ? string.Empty : GetHistoryFormattedValue( attributeValue.Value, attributeCache );
                History.EvaluateChange( historyChangeList, attributeCache.Name, formattedOldValue, formattedNewValue, attributeCache.FieldType.Field.IsSensitive() );
            }

            if ( !historyChangeList.Any() )
            {
                historyChangeList.AddChange(
                    isDelete ? History.HistoryVerb.Delete : History.HistoryVerb.Add,
                    History.HistoryChangeType.Record,
                    $"{matrixAttributeCache.Name} Item" );
            }

            // Add the History items to the Person Demographic category.
            HistoryItems = HistoryService.GetChanges(
                typeof( Person ),
                SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                matrixAttributeValue.EntityId.GetValueOrDefault(),
                historyChangeList,
                matrixAttributeCache.Name,
                typeof( Attribute ),
                matrixAttributeCache.Id,
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

        #endregion History
    }
}
