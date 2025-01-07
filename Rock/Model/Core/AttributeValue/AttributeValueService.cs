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

using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service for <see cref="Rock.Model.AttributeValue"/> entity objects.
    /// </summary>
    public partial class AttributeValueService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by <see cref="Rock.Model.Attribute"/>.
        /// </summary>
        /// <param name="attributeId">A <see cref="System.Int32" /> that represents the AttributeId of the <see cref="Rock.Model.Attribute"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by the specified <see cref="Rock.Model.Attribute"/>.</returns>
        public IQueryable<AttributeValue> GetByAttributeId( int attributeId )
        {
            return Queryable().Where( t => t.AttributeId == attributeId );
        }

        /// <summary>
        /// Gets an Attribute Value by Attribute Id And Entity Id
        /// </summary>
        /// <param name="attributeId">Attribute Id.</param>
        /// <param name="entityId">Entity Id.</param>
        /// <returns></returns>
        public AttributeValue GetByAttributeIdAndEntityId( int attributeId, int? entityId )
        {
            return Queryable()
                .Where( t =>
                    t.AttributeId == attributeId &&
                    ( 
                        ( !t.EntityId.HasValue && !entityId.HasValue ) || 
                        ( t.EntityId.HasValue && entityId.HasValue && t.EntityId.Value == entityId.Value )
                    ) )
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by EntityId.
        /// </summary>
        /// <param name="entityId">A <see cref="System.Int32"/> representing the EntityId to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by EntityId.</returns>
        public IQueryable<AttributeValue> GetByEntityId( int? entityId )
        {
            return Queryable().Where( t => ( t.EntityId == entityId || ( entityId == null && t.EntityId == null ) ) );
        }

        /// <summary>
        /// Returns an IQueryable of <see cref="Rock.Model.AttributeValue">AttributeValues</see>
        /// by EntityTypeId, EntityTypeQualifierColumn and EntityTypeQualifierValue.
        /// </summary>
        /// <param name="entityTypeId">The Id of the <see cref="EntityType"/> for <see cref="Attribute"/> to search for.</param>
        /// <param name="qualifierColumn">The value of the EntityTypeQualifierColumn to search for (e.g. 'BlockTypeId' or 'EntityTypeId').</param>
        /// <param name="qualifierValue">The value of the EntityTypeQualifierValue to search for (e.g. 'BlockTypeId' or 'EntityTypeId').</param>
        /// <returns>An IQueryable of <see cref="Rock.Model.AttributeValue">AttributeValues</see> for the specified parameters.</returns>
        public IQueryable<AttributeValue> GetByEntityTypeQualified( int entityTypeId, string qualifierColumn, string qualifierValue )
        {
            return Queryable().Where( t =>
                t.Attribute.EntityTypeId == entityTypeId
                && t.Attribute.EntityTypeQualifierColumn == qualifierColumn
                && t.Attribute.EntityTypeQualifierValue == qualifierValue );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.AttributeValue"/> for a <see cref="Rock.Model.Attribute"/> by Key.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the Global <see cref="Rock.Model.Attribute">Attribute's</see> key value.</param>
        /// <returns>The <see cref="Rock.Model.AttributeValue" /> of the global <see cref="Rock.Model.Attribute"/>.</returns>
        public AttributeValue GetGlobalAttributeValue( string key )
        {
            return Queryable()
                .Where( v =>
                    v.Attribute.Key == key &&
                    !v.Attribute.EntityTypeId.HasValue &&
                    ( v.Attribute.EntityTypeQualifierColumn == null || v.Attribute.EntityTypeQualifierColumn == string.Empty ) &&
                    !v.EntityId.HasValue )
                .FirstOrDefault();
        }

        #region Static Methods

        /// <summary>
        /// Updates all AttributeValues to move date values from the [Value] field to the [ValueAsDateTime] field.  Temporarily adjusts the
        /// RockContext.Database.CommandTimeout property to ensure that the command completes without a timeout.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        /// <param name="commandTimeout">The CommandTimeout property to set (default 120).</param>
        /// <returns>The number of records affected.</returns>
        public static int UpdateAllValueAsDateTimeFromTextValue( RockContext rockContext = null, int commandTimeout = 120 )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var updateSql = @"
                -- Update the [ValueAsDateTime] field for every row that has a date value in the [Value] field, but has a NULL value in the [ValueAsDateTime] field.
                UPDATE	[AttributeValue]
                SET		[AttributeValue].[ValueAsDateTime] =
	                CASE WHEN [Value] LIKE '____-__-__T%__:__:%' THEN
		                ISNULL(TRY_CAST(TRY_CAST(LEFT([Value],19) AS DATETIMEOFFSET) AS DATETIME) , TRY_CAST([Value] AS DATETIME))
	                ELSE
		                TRY_CAST([Value] AS DATETIME)
	                END
                FROM	[AttributeValue]
                WHERE	[ValueAsDateTime] IS NULL
	                AND	[Value] IS NOT NULL
	                AND [Value] <> ''
	                AND	LEN([Value]) < 50
	                AND	ISNUMERIC([Value]) = 0
	                AND ([Value] LIKE '____-__-__T%__:__:%' OR TRY_CAST([Value] AS DATETIME) IS NOT NULL);
            ";

            // Store current CommandTimeout setting and change it to 120 seconds.
            var currentTimeoutSetting = rockContext.Database.CommandTimeout;
            rockContext.Database.CommandTimeout = commandTimeout;

            // Execute SQL command.
            var recordsAffected = rockContext.Database.ExecuteSqlCommand( updateSql );

            // Return CommandTimeout to previous setting.
            rockContext.Database.CommandTimeout = currentTimeoutSetting;

            return recordsAffected;
        }

        #endregion Static Methods
    }
}
