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
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// A job that updates the attribute value persisted values.
    /// </summary>
    [DisplayName( "Update Persisted Attribute Values" )]
    [Description( "A job that updates the attribute value persisted values." )]

    #region Job Attributes

    [IntegerField( "Rebuild Percentage",
        Description = "The percentage of all attribute values to rebuild even if they aren't dirty. Dirty values are always rebuilt.",
        DefaultIntegerValue = 25,
        IsRequired = true,
        Key = AttributeKey.RebuildPercentage,
        Order = 0 )]

    [IntegerField( "Command Timeout",
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Attribute Values, this could take several minutes or more.",
        IsRequired = true,
        DefaultIntegerValue = 5 * 60,
        Key = AttributeKey.CommandTimeout,
        Order = 1 )]

    #endregion

    [DisallowConcurrentExecution]
    public class UpdatePersistedAttributeValues : IJob
    {
        #region Keys

        /// <summary>
        /// Keys to use for job Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string RebuildPercentage = "RebuildPercentage";

            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Keys to use for system settings.
        /// </summary>
        private static class SystemSettingKey
        {
            public const string LastProcessedAttributeId = "update-persisted-attribute-values-last-attribute-id";
        }

        #endregion

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersistedAttributeValues()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;
            var rebuildPercentage = dataMap.GetString( AttributeKey.RebuildPercentage ).AsInteger();
            var commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsInteger();
            var updatedCount = 0;

            CreateIndex( commandTimeout );

            updatedCount += ForceRebuildAttributesAndValues( rebuildPercentage, commandTimeout, context, out var forcedRebuildErrorMessages );

            updatedCount += UpdateVolatileAttributesAndValues( commandTimeout, context, out var volatileErrorMessages );

            updatedCount += UpdateDirtyAttributesAndValues( commandTimeout, context );

            var errorMessages = new List<string>();
            errorMessages.AddRange( forcedRebuildErrorMessages );
            errorMessages.AddRange( volatileErrorMessages );

            var message = $"Updated {updatedCount:N0} persisted values.";

            if ( forcedRebuildErrorMessages.Any() )
            {
                message += $"\nEncounted errors:\n{string.Join( "\n", forcedRebuildErrorMessages )}";
            }

            context.Result = message;
        }

        /// <summary>
        /// Creates the index if it has not already been created.
        /// </summary>
        /// <param name="commandTimeout">The command timeout value in seconds.</param>
        private void CreateIndex( int commandTimeout )
        {
            var migrationHelper = new MigrationHelper( new JobMigration( commandTimeout ) );

            migrationHelper.CreateIndexIfNotExists( "AttributeValue", new[] { nameof( AttributeValue.ValueChecksum ) }, Array.Empty<string>() );
        }

        /// <summary>
        /// Forcefully rebuild a chunk of the attributes and values. This provides
        /// the ability to periodically sync persisted values even if we think
        /// they are correct. So if something changes that was missed in normal
        /// update logic this will eventually pick it up.
        /// </summary>
        /// <param name="rebuildPercentage">The percentage (0-100) of the attributes to rebuild.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="context">The job context to use when updating the current status.</param>
        /// <param name="errorMessages">On return will contain a list of errors that were encountered while processing.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int ForceRebuildAttributesAndValues( int rebuildPercentage, int commandTimeout, IJobExecutionContext context, out List<string> errorMessages )
        {
            int? lastAttributeId = null;
            var updatedCount = 0;

            var attributeIds = GetAttributeIdsToForceRebuild( rebuildPercentage, commandTimeout );

            errorMessages = new List<string>();

            for ( var attributeIndex = 0; attributeIndex < attributeIds.Count; attributeIndex++ )
            {
                var attributeId = attributeIds[attributeIndex];

                try
                {
                    context.UpdateLastStatusMessage( $"Rebuilding attribute {attributeIndex + 1:N0} of {attributeIds.Count:N0}." );
                    updatedCount += ForceRebuildAttributeAndValues( attributeId, commandTimeout );
                    lastAttributeId = attributeId;
                }
                catch ( Exception ex )
                {
                    errorMessages.Add( $"Error updating attribute #{attributeId}: {ex.Message}" );
                    ExceptionLogService.LogException( ex );
                }
            }

            Rock.Web.SystemSettings.SetValue( SystemSettingKey.LastProcessedAttributeId, lastAttributeId.ToStringSafe() );

            return updatedCount;
        }

        /// <summary>
        /// Forcefully rebuild a single attribute and all its values.
        /// </summary>
        /// <param name="attributeId">The attribute identifier to be updated.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <returns>The number of attribute values that were updated, plus one for the attribute.</returns>
        private int ForceRebuildAttributeAndValues( int attributeId, int commandTimeout )
        {
            int updatedCount = 0;
            var configurationValues = new Dictionary<string, string>();
            Rock.Field.IFieldType field;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                // First rebuild the attribute's default value.
                var attribute = new AttributeService( rockContext ).Queryable()
                    .Include( a => a.AttributeQualifiers )
                    .Single( a => a.Id == attributeId );

                if ( attribute == null )
                {
                    return 0;
                }

                field = FieldTypeCache.Get( attribute.FieldTypeId ).Field;

                if ( field == null )
                {
                    return 0;
                }

                Helper.UpdateAttributeDefaultPersistedValues( attribute );
                Helper.UpdateAttributeEntityReferences( attribute, rockContext );

                rockContext.SaveChanges();

                updatedCount++;

                // Now we need to tackle all the attribute values. Get the configuration
                // of the attribute in the database.
                foreach ( var qualifier in attribute.AttributeQualifiers )
                {
                    configurationValues.AddOrReplace( qualifier.Key, qualifier.Value );
                }

                // Get all the distinctvalues and then process them in batches.
                var distinctValues = new AttributeValueService( rockContext )
                    .Queryable()
                    .Where( av => av.AttributeId == attribute.Id )
                    .Select( av => av.Value )
                    .Distinct()
                    .ToList();

                foreach ( var value in distinctValues )
                {
                    Field.PersistedValues persistedValues;

                    if ( field.IsPersistedValueSupported( configurationValues ) )
                    {
                        persistedValues = field.GetPersistedValues( value, configurationValues );
                    }
                    else
                    {
                        var placeholderValue = field.GetPersistedValuePlaceholder( configurationValues );

                        persistedValues = new Field.PersistedValues
                        {
                            TextValue = placeholderValue,
                            CondensedTextValue = placeholderValue,
                            HtmlValue = placeholderValue,
                            CondensedHtmlValue = placeholderValue
                        };
                    }

                    updatedCount += Helper.BulkUpdateAttributeValuePersistedValues( attribute.Id, value, persistedValues, rockContext );
                }
            }

            // Check if this field type references other entities.
            if ( !( field is Rock.Field.IEntityReferenceFieldType referencedField ) )
            {
                return updatedCount;
            }

            Dictionary<string, List<int>> attributeValueList;

            // Get a list of all the attribute value identifiers that we
            // need to update the references for.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                attributeValueList = new AttributeValueService( rockContext )
                    .Queryable()
                    .Where( av => av.AttributeId == attributeId )
                    .Select( av => new
                    {
                        av.Id,
                        av.Value
                    } )
                    .ToList()
                    .GroupBy( av => av.Value ?? string.Empty )
                    .ToDictionary( grp => grp.Key, grp => grp.Select( av => av.Id ).ToList() );
            }

            // Now we need to update all the referenced entities.
            foreach ( var kvp in attributeValueList )
            {
                var value = kvp.Key;
                var referencedEntities = referencedField.GetReferencedEntities( value, configurationValues ) ?? new List<Field.ReferencedEntity>();
                var attributeValueIds = kvp.Value;

                // Update the referenced entities 1,000 at a time. This seems to be
                // a rough sweet spot in performance. More than 1,000 doesn't really
                // gain us much and means any errors fail out a larger set.
                while ( attributeValueIds.Any() )
                {
                    var valueIds = attributeValueIds.Take( 1_000 ).ToList();
                    attributeValueIds = attributeValueIds.Skip( 1_000 ).ToList();

                    var referenceDictionary = valueIds.ToDictionary( valueId => valueId, valueId => referencedEntities );

                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = commandTimeout;

                        Helper.BulkUpdateAttributeValueEntityReferences( referenceDictionary, rockContext );
                    }
                }
            }

            return updatedCount;
        }

        /// <summary>
        /// Get the attribute identifiers to use during a forceful rebuild.
        /// </summary>
        /// <param name="rebuildPercentage">The percentage (0-100) of the attributes to rebuild.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <returns>A list of attribute identifiers that should be rebuilt.</returns>
        private List<int> GetAttributeIdsToForceRebuild( int rebuildPercentage, int commandTimeout )
        {
            // If they specify less than 1 then don't do any forced rebuilds.
            if ( rebuildPercentage < 1 )
            {
                return new List<int>();
            }

            var lastProcessedAttributeId = Rock.Web.SystemSettings.GetValue( SystemSettingKey.LastProcessedAttributeId ).AsIntegerOrNull();

            if ( lastProcessedAttributeId <= 1 )
            {
                lastProcessedAttributeId = null;
            }

            // Rebuild percentage must be between 1 and 100.
            rebuildPercentage = Math.Min( 100, rebuildPercentage );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var attributeService = new AttributeService( rockContext );
                var attributeCount = attributeService.Queryable().Count();
                var numberToRebuild = ( int ) Math.Ceiling( attributeCount * ( rebuildPercentage / 100.0 ) );

                var qry = attributeService.Queryable();

                if ( lastProcessedAttributeId.HasValue )
                {
                    qry = qry.Where( a => a.Id < lastProcessedAttributeId.Value );
                }

                var attributeIds = qry.OrderByDescending( a => a.Id )
                    .Take( numberToRebuild )
                    .Select( a => a.Id )
                    .ToList();

                // If we didn't get any, then start over from the top without
                // the where clause.
                if ( !attributeIds.Any() )
                {
                    attributeIds = attributeService.Queryable()
                        .OrderByDescending( a => a.Id )
                        .Take( numberToRebuild )
                        .Select( a => a.Id )
                        .ToList();
                }

                return attributeIds;
            }
        }

        /// <summary>
        /// Updates any attributes and values that are considered volatile. This
        /// allows attribute values that have a high probability of being changed
        /// by outside influence to get their values back in sync.
        /// </summary>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="context">The job context to use when updating the current status.</param>
        /// <param name="errorMessages">On return will contain a list of errors that were encountered while processing.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int UpdateVolatileAttributesAndValues( int commandTimeout, IJobExecutionContext context, out List<string> errorMessages )
        {
            var updatedCount = 0;

            var attributeIds = AttributeCache.All()
                .Where( a => a.FieldType.Field != null && a.FieldType.Field.IsPersistedValueVolatile( a.ConfigurationValues ) )
                .Select( a => a.Id )
                .ToList();

            errorMessages = new List<string>();

            for ( var attributeIndex = 0; attributeIndex < attributeIds.Count; attributeIndex++ )
            {
                var attributeId = attributeIds[attributeIndex];

                try
                {
                    context.UpdateLastStatusMessage( $"Rebuilding volatile attribute {attributeIndex + 1:N0} of {attributeIds.Count:N0}." );
                    updatedCount += ForceRebuildAttributeAndValues( attributeId, commandTimeout );
                }
                catch ( Exception ex )
                {
                    errorMessages.Add( $"Error updating attribute #{attributeId}: {ex.Message}" );
                    ExceptionLogService.LogException( ex );
                }
            }

            return updatedCount;
        }

        /// <summary>
        /// Updates all attributes and values that are currently marked dirty.
        /// </summary>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="context">The job context to use when updating the current status.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int UpdateDirtyAttributesAndValues( int commandTimeout, IJobExecutionContext context )
        {
            var updatedCount = 0;
            var attributeIds = GetDirtyAttributeIds( commandTimeout );

            context.UpdateLastStatusMessage( "Updating dirty attributes." );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var attributes = new AttributeService( rockContext )
                    .Queryable()
                    .Where( a => attributeIds.Contains( a.Id ) )
                    .ToList();

                foreach ( var attribute in attributes )
                {
                    Helper.UpdateAttributeDefaultPersistedValues( attribute );
                    Helper.UpdateAttributeEntityReferences( attribute, rockContext );

                    updatedCount++;
                }

                rockContext.SaveChanges();
            }

            updatedCount += UpdateDirtyAttributeValues( commandTimeout, context );

            return updatedCount;
        }

        /// <summary>
        /// Updates all the dirty attribute values that are currently marked dirty.
        /// This is automatically called by <see cref="UpdateDirtyAttributesAndValues(int, IJobExecutionContext)"/>.
        /// </summary>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="context">The job context to use when updating the current status.</param>
        /// <returns>The number of attribute values that were updated.</returns>
        private int UpdateDirtyAttributeValues( int commandTimeout, IJobExecutionContext context )
        {
            var updatedCount = 0;
            var attributeIndex = 0;
            var dirtyDictionary = GetDirtyAttributeValues( commandTimeout );

            foreach ( var kvpDirty in dirtyDictionary )
            {
                attributeIndex++;
                var attributeId = kvpDirty.Key;
                var attributeValues = kvpDirty.Value;
                var attributeCache = AttributeCache.Get( attributeId );
                var field = attributeCache.FieldType.Field;

                context.UpdateLastStatusMessage( $"Updating dirty values for attribute {attributeIndex:N0} of {dirtyDictionary.Count:N0}." );

                // Make sure this isn't a bad field type.
                if ( field == null )
                {
                    continue;
                }

                var valueGroups = attributeValues.GroupBy( v => v.Value );

                foreach ( var valueGroup in valueGroups )
                {
                    var value = valueGroup.Key;
                    var attributeValueIds = valueGroup.Select( grp => grp.Id ).ToList();
                    Field.PersistedValues persistedValues;

                    if ( field.IsPersistedValueSupported( attributeCache.ConfigurationValues ) )
                    {
                        persistedValues = field.GetPersistedValues( value, attributeCache.ConfigurationValues );
                    }
                    else
                    {
                        var placeholderValue = field.GetPersistedValuePlaceholder( attributeCache.ConfigurationValues );

                        persistedValues = new Field.PersistedValues
                        {
                            TextValue = placeholderValue,
                            CondensedTextValue = placeholderValue,
                            HtmlValue = placeholderValue,
                            CondensedHtmlValue = placeholderValue
                        };
                    }

                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = commandTimeout;

                        updatedCount += Helper.BulkUpdateAttributeValuePersistedValues( attributeId, attributeValueIds, persistedValues, rockContext );

                        if ( attributeCache.IsReferencedEntityFieldType )
                        {
                            UpdateDirtyAttributeValueReferences( attributeId, value, attributeValueIds, rockContext );
                        }
                    }
                }
            }

            return updatedCount;
        }

        /// <summary>
        /// Updates the referenced entities for a set of attribute values.
        /// </summary>
        /// <param name="attributeId">The identifier of the attribute these values belong to, used to get field information.</param>
        /// <param name="value">The raw database value that these attribute values all share in common.</param>
        /// <param name="attributeValueIds">The identifiers of the values that match the value.</param>
        /// <param name="rockContext">The database context to operate in.</param>
        private void UpdateDirtyAttributeValueReferences( int attributeId, string value, List<int> attributeValueIds, RockContext rockContext )
        {
            var attributeCache = AttributeCache.Get( attributeId );
            var field = ( Field.IEntityReferenceFieldType ) AttributeCache.Get( attributeId ).FieldType.Field;
            var referencedEntities = field.GetReferencedEntities( value, attributeCache.ConfigurationValues ) ?? new List<Field.ReferencedEntity>();

            while ( attributeValueIds.Any() )
            {
                var valueIds = attributeValueIds.Take( 1_000 ).ToList();
                attributeValueIds = attributeValueIds.Skip( 1_000 ).ToList();

                var referenceDictionary = attributeValueIds.ToDictionary( vid => vid, _ => referencedEntities );

                Helper.BulkUpdateAttributeValueEntityReferences( referenceDictionary, rockContext );
            }
        }

        /// <summary>
        /// Gets a list of all the dirty attribute values in the system.
        /// </summary>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <returns>A dictionary whose key identifies the attribute identifier and whose value is a list of the attribute value identifiers.</returns>
        private Dictionary<int, List<(int Id, string Value)>> GetDirtyAttributeValues( int commandTimeout )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                return rockContext.Set<AttributeValue>()
                    .Where( av => av.IsPersistedValueDirty )
                    .Select( av => new
                    {
                        av.AttributeId,
                        av.Id,
                        av.Value
                    } )
                    .ToList()
                    .GroupBy( av => av.AttributeId, av => (av.Id, av.Value) )
                    .ToDictionary( grp => grp.Key, grp => grp.ToList() );
            }
        }

        /// <summary>
        /// Gets the attribute identifies that are currently marked as dirty.
        /// </summary>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <returns>A list of attribute identifiers.</returns>
        private List<int> GetDirtyAttributeIds( int commandTimeout )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                return rockContext.Set<Rock.Model.Attribute>()
                    .Where( a => a.IsDefaultPersistedValueDirty )
                    .Select( a => a.Id )
                    .ToList();
            }
        }
    }
}
