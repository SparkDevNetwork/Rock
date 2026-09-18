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
using System.Linq;
using System.Net.Sockets;
using System.Net;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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

    [RockLoggingCategory]
    public class UpdatePersistedAttributeValues : RockJob
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

        #region Fields

        /// <summary>
        /// The number of attributes to load and rebuild per database context during
        /// the force and volatile phases. Batching amortizes the per-attribute load
        /// over one query; a smaller batch keeps the blast radius small if a batch
        /// has to fall back to isolated per-attribute processing.
        /// </summary>
        private const int RebuildBatchSize = 100;

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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var rebuildPercentage = GetAttributeValue( AttributeKey.RebuildPercentage ).AsInteger();
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsInteger();
            var updatedCount = 0;

            // Update the last status message at most every 2.5 seconds.
            var statusMessage = new ThrottleLogger( 2500, msg => UpdateLastStatusMessage( msg ) );

            CreateIndex( commandTimeout );

            var sw = System.Diagnostics.Stopwatch.StartNew();
            updatedCount += ForceRebuildAttributesAndValues( rebuildPercentage, commandTimeout, statusMessage, out var forcedRebuildErrorMessages );
            LogTimedMessage( $"Force Rebuild Attributes and Values.", sw.Elapsed.TotalMilliseconds );

            sw.Restart();
            updatedCount += UpdateVolatileAttributesAndValues( commandTimeout, statusMessage, out var volatileErrorMessages );
            LogTimedMessage( $"Force Volatile Attributes and Values.", sw.Elapsed.TotalMilliseconds );

            sw.Restart();
            updatedCount += UpdateDirtyAttributesAndValues( commandTimeout, statusMessage, out var dirtyErrorMessages );
            LogTimedMessage( $"Force Dirty Attributes and Values.", sw.Elapsed.TotalMilliseconds );

            var errorMessages = new List<string>();
            errorMessages.AddRange( forcedRebuildErrorMessages );
            errorMessages.AddRange( volatileErrorMessages );
            errorMessages.AddRange( dirtyErrorMessages );

            var message = $"Updated {updatedCount:N0} persisted values.";

            message += $"\nServer: {GetLocalIPAddress()}";

            if ( errorMessages.Any() )
            {
                message += $"\nEncounted errors:\n{string.Join( "\n", errorMessages )}";
            }

            this.Result = message;
        }

        /// <summary>
        /// Creates the index if it has not already been created.
        /// </summary>
        /// <param name="commandTimeout">The command timeout value in seconds.</param>
        private void CreateIndex( int commandTimeout )
        {
            var migrationHelper = new MigrationHelper( new JobMigration( commandTimeout ) );

            /*
                9/17/26 - CLAUDE

                The persisted-value UPDATE queries filter on both ValueChecksum
                and AttributeId. With a single-column index on ValueChecksum the
                optimizer kept flip-flopping between an index seek and a full
                clustered scan (CHECKSUM collides, so rows-per-checksum varies
                wildly), which produced unstable query plans. Widening the index
                to key on ValueChecksum + AttributeId gives a stable, selective
                seek. We drop the old single-column index and create the wider
                one under its convention-correct name (IX_ValueChecksum_AttributeId)
                rather than modifying the existing index in place, so the index
                name continues to reflect the columns it covers.

                Reason: Stabilize the plan for persisted attribute value updates.
            */
            migrationHelper.DropIndexIfExists( "AttributeValue", "IX_ValueChecksum" );

            migrationHelper.CreateIndexIfNotExists(
                "AttributeValue",
                new[] { nameof( AttributeValue.ValueChecksum ), nameof( AttributeValue.AttributeId ) },
                new[] { nameof( AttributeValue.IsPersistedValueDirty ) } );
        }

        /// <summary>
        /// Forcefully rebuild a chunk of the attributes and values. This provides
        /// the ability to periodically sync persisted values even if we think
        /// they are correct. So if something changes that was missed in normal
        /// update logic this will eventually pick it up.
        /// </summary>
        /// <param name="rebuildPercentage">The percentage (0-100) of the attributes to rebuild.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="statusMessage">The object to use when updating the current status.</param>
        /// <param name="errorMessages">On return will contain a list of errors that were encountered while processing.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int ForceRebuildAttributesAndValues( int rebuildPercentage, int commandTimeout, ThrottleLogger statusMessage, out List<string> errorMessages )
        {
            var updatedCount = 0;

            var attributeIds = GetAttributeIdsToForceRebuild( rebuildPercentage, commandTimeout );

            errorMessages = new List<string>();

            for ( var batchStart = 0; batchStart < attributeIds.Count; batchStart += RebuildBatchSize )
            {
                var batchIds = attributeIds.Skip( batchStart ).Take( RebuildBatchSize ).ToList();
                var batchEnd = Math.Min( batchStart + RebuildBatchSize, attributeIds.Count );

                statusMessage.Write( $"Rebuilding attributes {batchStart + 1:N0}-{batchEnd:N0} of {attributeIds.Count:N0}." );

                updatedCount += ProcessForceRebuildBatch( batchIds, commandTimeout, errorMessages );
            }

            // The attributes are processed in descending Id order, so the last entry
            // is the lowest Id we reached this run. Remember it so the next run
            // continues below it (see GetAttributeIdsToForceRebuild).
            var lastAttributeId = attributeIds.Any() ? ( int? ) attributeIds.Last() : null;

            Rock.Web.SystemSettings.SetValue( SystemSettingKey.LastProcessedAttributeId, lastAttributeId.ToStringSafe() );

            return updatedCount;
        }

        /// <summary>
        /// Forcefully rebuild a single attribute and all its values in its own
        /// database context. This is the isolated fallback used by
        /// <see cref="ProcessForceRebuildBatch"/> when a batch cannot be processed
        /// as a unit.
        /// </summary>
        /// <param name="attributeId">The attribute identifier to be updated.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <returns>The number of attribute values that were updated, plus one for the attribute.</returns>
        private int ForceRebuildAttributeAndValues( int attributeId, int commandTimeout )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( commandTimeout );

                var attribute = new AttributeService( rockContext ).Queryable()
                    .Include( a => a.AttributeQualifiers )
                    .Single( a => a.Id == attributeId );

                if ( attribute == null )
                {
                    return 0;
                }

                var field = FieldTypeCache.Get( attribute.FieldTypeId )?.Field;

                if ( field == null )
                {
                    return 0;
                }

                var updatedCount = ForceRebuildLoadedAttributeAndValues( attribute, field, rockContext, commandTimeout );

                rockContext.SaveChanges();

                return updatedCount;
            }
        }

        /// <summary>
        /// Processes a batch of attributes in a single database context, loading them
        /// with one query and saving all their default-value and reference changes in
        /// one call. If the batch cannot be processed or saved as a unit, it is retried
        /// one attribute at a time in isolated contexts so a single failing attribute
        /// cannot discard the work for the rest of the batch.
        /// </summary>
        /// <param name="attributeIds">The identifiers of the attributes in this batch.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="errorMessages">On return, appended with any per-attribute errors from the fallback path.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int ProcessForceRebuildBatch( List<int> attributeIds, int commandTimeout, List<string> errorMessages )
        {
            try
            {
                var updatedCount = 0;

                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.SetCommandTimeout( commandTimeout );

                    // Load the whole batch (with qualifiers) in a single query rather
                    // than one query per attribute.
                    var attributes = new AttributeService( rockContext ).Queryable()
                        .Include( a => a.AttributeQualifiers )
                        .Where( a => attributeIds.Contains( a.Id ) )
                        .ToList()
                        .ToDictionary( a => a.Id );

                    // Process in the original (descending Id) order.
                    foreach ( var attributeId in attributeIds )
                    {
                        if ( !attributes.TryGetValue( attributeId, out var attribute ) )
                        {
                            continue;
                        }

                        var field = FieldTypeCache.Get( attribute.FieldTypeId )?.Field;

                        if ( field == null )
                        {
                            continue;
                        }

                        updatedCount += ForceRebuildLoadedAttributeAndValues( attribute, field, rockContext, commandTimeout );
                    }

                    // Persist every attribute's default-value and reference changes
                    // for the batch in a single round trip.
                    rockContext.SaveChanges();
                }

                return updatedCount;
            }
            catch ( Exception ex )
            {
                /*
                    9/18/26 - CLAUDE

                    A batch shares one context, so a single bad attribute (or a
                    failed batch SaveChanges) would otherwise lose the work for the
                    entire batch. Fall back to processing each attribute in its own
                    context so the failure is isolated and logged per attribute. The
                    attribute-value updates run via immediate SQL, so any that already
                    ran during the failed batch attempt are simply re-run here; they
                    are idempotent.

                    Reason: Keep per-attribute failure isolation while batching the load.
                */
                ExceptionLogService.LogException( ex );

                return ProcessForceRebuildIndividually( attributeIds, commandTimeout, errorMessages );
            }
        }

        /// <summary>
        /// Processes each attribute in its own database context, collecting errors per
        /// attribute. This is the isolated fallback for <see cref="ProcessForceRebuildBatch"/>.
        /// </summary>
        /// <param name="attributeIds">The identifiers of the attributes to process.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="errorMessages">On return, appended with any per-attribute errors.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int ProcessForceRebuildIndividually( List<int> attributeIds, int commandTimeout, List<string> errorMessages )
        {
            var updatedCount = 0;

            foreach ( var attributeId in attributeIds )
            {
                try
                {
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
        /// Rebuilds the default value, entity references, and all attribute values for a
        /// single already-loaded attribute. The caller is responsible for calling
        /// <see cref="Rock.Data.DbContext.SaveChanges()"/> on <paramref name="rockContext"/>
        /// so that a batch of attributes can be saved together in one round trip.
        /// </summary>
        /// <param name="attribute">The attribute to rebuild; must be tracked by <paramref name="rockContext"/> with its qualifiers loaded.</param>
        /// <param name="field">The field type for the attribute.</param>
        /// <param name="rockContext">The database context that <paramref name="attribute"/> is tracked by.</param>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <returns>The number of attribute values that were updated, plus one for the attribute.</returns>
        private int ForceRebuildLoadedAttributeAndValues( Rock.Model.Attribute attribute, Rock.Field.IFieldType field, RockContext rockContext, int commandTimeout )
        {
            var updatedCount = 0;
            var configurationValues = new Dictionary<string, string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Rebuild the attribute's default value and references. These changes stay
            // tracked (unsaved) so the caller can save the whole batch at once.
            Helper.UpdateAttributeDefaultPersistedValues( attribute );
            Helper.UpdateAttributeEntityReferences( attribute, rockContext );

            updatedCount++;

            // Get the configuration of the attribute from its qualifiers.
            foreach ( var qualifier in attribute.AttributeQualifiers )
            {
                configurationValues.AddOrReplace( qualifier.Key, qualifier.Value );
            }

            // Get all the distinct values and then process them.
            var distinctValues = new AttributeValueService( rockContext )
                .Queryable()
                .Where( av => av.AttributeId == attribute.Id )
                .Select( av => av.Value )
                .Distinct()
                .ToList();

            if ( field.IsPersistedValueSupported( configurationValues ) )
            {
                var cache = new Dictionary<string, object>();

                foreach ( var value in distinctValues )
                {
                    var persistedValues = Helper.GetPersistedValuesOrPlaceholder( field, value, configurationValues, cache );

                    updatedCount += Helper.BulkUpdateAttributeValueComputedAndPersistedValues( attribute.Id, value, persistedValues, rockContext );
                }
            }
            else
            {
                var placeholderValues = Helper.GetPersistedValuePlaceholderOrDefault( field, configurationValues );

                foreach ( var value in distinctValues )
                {
                    updatedCount += Helper.BulkUpdateAttributeValueComputedAndPersistedValues( attribute.Id, value, placeholderValues, rockContext );
                }
            }

            LogTimedMessage( $"Force rebuild of attribute #{attribute.Id}.", sw.Elapsed.TotalMilliseconds );

            // Check if this field type references other entities.
            if ( !( field is Rock.Field.IEntityReferenceFieldType referencedField ) )
            {
                return updatedCount;
            }

            Dictionary<string, List<int>> attributeValueList;
            sw.Restart();

            // Get a list of all the attribute value identifiers that we
            // need to update the references for.
            using ( var referenceContext = new RockContext() )
            {
                referenceContext.Database.SetCommandTimeout( commandTimeout );

                attributeValueList = new AttributeValueService( referenceContext )
                    .Queryable()
                    .Where( av => av.AttributeId == attribute.Id )
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

                    using ( var referenceContext = new RockContext() )
                    {
                        referenceContext.Database.SetCommandTimeout( commandTimeout );

                        Helper.BulkUpdateAttributeValueEntityReferences( referenceDictionary, referenceContext );
                    }
                }
            }

            LogTimedMessage( $"Rebuild of entity references for attribute #{attribute.Id}.", sw.Elapsed.TotalMilliseconds );

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
                rockContext.Database.SetCommandTimeout( commandTimeout );

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
        /// <param name="statusMessage">The object to use when updating the current status.</param>
        /// <param name="errorMessages">On return will contain a list of errors that were encountered while processing.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int UpdateVolatileAttributesAndValues( int commandTimeout, ThrottleLogger statusMessage, out List<string> errorMessages )
        {
            var updatedCount = 0;

            var attributeIds = GetVolatileAttributeIds();

            errorMessages = new List<string>();

            for ( var batchStart = 0; batchStart < attributeIds.Count; batchStart += RebuildBatchSize )
            {
                var batchIds = attributeIds.Skip( batchStart ).Take( RebuildBatchSize ).ToList();
                var batchEnd = Math.Min( batchStart + RebuildBatchSize, attributeIds.Count );

                statusMessage.Write( $"Rebuilding volatile attributes {batchStart + 1:N0}-{batchEnd:N0} of {attributeIds.Count:N0}." );

                updatedCount += ProcessForceRebuildBatch( batchIds, commandTimeout, errorMessages );
            }

            return updatedCount;
        }

        /// <summary>
        /// Updates all attributes and values that are currently marked dirty.
        /// </summary>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="statusMessage">The object to use when updating the current status.</param>
        /// <param name="errorMessages">On return will contain a list of errors that were encountered while processing.</param>
        /// <returns>The number of attributes and values that were updated.</returns>
        private int UpdateDirtyAttributesAndValues( int commandTimeout, ThrottleLogger statusMessage, out List<string> errorMessages )
        {
            var updatedCount = 0;
            var attributeIds = GetDirtyAttributeIds( commandTimeout );

            errorMessages = new List<string>();
            statusMessage.Write( "Updating dirty attributes.", true );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( commandTimeout );

                var attributes = new AttributeService( rockContext )
                    .Queryable()
                    .Where( a => attributeIds.Contains( a.Id ) )
                    .ToList();

                foreach ( var attribute in attributes )
                {
                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();

                        Helper.UpdateAttributeDefaultPersistedValues( attribute );
                        Helper.UpdateAttributeEntityReferences( attribute, rockContext );

                        LogTimedMessage( $"Rebuild of dirty attribute #{attribute.Id}.", sw.Elapsed.TotalMilliseconds );

                        updatedCount++;
                    }
                    catch ( Exception ex )
                    {
                        errorMessages.Add( $"Error updating dirty attribute #{attribute.Id}: {ex.Message}" );
                        ExceptionLogService.LogException( ex );
                    }
                }

                rockContext.SaveChanges();
            }

            updatedCount += UpdateDirtyAttributeValues( commandTimeout, statusMessage, out var valueErrorMessages );

            errorMessages.AddRange( valueErrorMessages );

            return updatedCount;
        }

        /// <summary>
        /// Updates all the dirty attribute values that are currently marked dirty.
        /// This is automatically called by <see cref="UpdateDirtyAttributesAndValues(int, ThrottleLogger, out List{string})"/>.
        /// </summary>
        /// <param name="commandTimeout">The timeout to use for a single command against the database.</param>
        /// <param name="statusMessage">The object to use when updating the current status.</param>
        /// <param name="errorMessages">On return will contain a list of errors that were encountered while processing.</param>
        /// <returns>The number of attribute values that were updated.</returns>
        private int UpdateDirtyAttributeValues( int commandTimeout, ThrottleLogger statusMessage, out List<string> errorMessages )
        {
            var updatedCount = 0;
            var attributeIndex = 0;
            var dirtyDictionary = GetDirtyAttributeValues( commandTimeout );

            errorMessages = new List<string>();

            // Safety loop counter to quit after 1,000 iterations.  GetDirtyAttributeValues() can return up to 100,000
            // attribute values per iteration, so 1,000 iterations will be up to 100,000,000 total attribute value records.
            int loopCount = 0;
            while ( dirtyDictionary.Count > 0 && loopCount <= 1000 )
            {
                loopCount++;

                foreach ( var kvpDirty in dirtyDictionary )
                {
                    attributeIndex++;
                    var attributeId = kvpDirty.Key;
                    var attributeValues = kvpDirty.Value;
                    var attributeCache = AttributeCache.Get( attributeId );
                    var field = attributeCache.FieldType.Field;
                    var cache = new Dictionary<string, object>();

                    // Make sure this isn't a bad field type.
                    if ( field == null )
                    {
                        continue;
                    }

                    statusMessage.Write( $"Updating dirty values for attribute {attributeIndex:N0} of {dirtyDictionary.Count:N0}." );

                    Field.PersistedValues placeholderValues = null;
                    var persistedValueSupported = field.IsPersistedValueSupported( attributeCache.ConfigurationValues );

                    try
                    {
                        var valueGroups = attributeValues.GroupBy( v => v.Value );

                        foreach ( var valueGroup in valueGroups )
                        {
                            var sw = System.Diagnostics.Stopwatch.StartNew();
                            var value = valueGroup.Key;
                            var attributeValueIds = valueGroup.Select( grp => grp.Id ).ToList();
                            Field.PersistedValues persistedValues;

                            if ( persistedValueSupported )
                            {
                                persistedValues = Helper.GetPersistedValuesOrPlaceholder( field, value, attributeCache.ConfigurationValues, cache );
                            }
                            else
                            {
                                if ( placeholderValues == null )
                                {
                                    placeholderValues = Helper.GetPersistedValuePlaceholderOrDefault( field, attributeCache.ConfigurationValues );
                                }

                                persistedValues = placeholderValues;
                            }

                            using ( var rockContext = new RockContext() )
                            {
                                rockContext.Database.SetCommandTimeout( commandTimeout );

                                updatedCount += Helper.BulkUpdateAttributeValueComputedAndPersistedValues( attributeId, attributeValueIds, value, persistedValues, true, rockContext );

                                LogTimedMessage( $"Rebuild of {attributeValueIds.Count:N0} dirty values for attribute #{attributeId}.", sw.Elapsed.TotalMilliseconds );

                                if ( attributeCache.IsReferencedEntityFieldType )
                                {
                                    sw.Restart();
                                    UpdateDirtyAttributeValueReferences( attributeId, value, attributeValueIds, rockContext );
                                    LogTimedMessage( $"Rebuild of entity references on {attributeValueIds.Count:N0} values for attribute #{attributeId}.", sw.Elapsed.TotalMilliseconds );
                                }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        errorMessages.Add( $"Error updating dirty attribute values for attribute #{attributeId}: {ex.Message}" );
                        ExceptionLogService.LogException( ex );
                    }
                }

                dirtyDictionary = GetDirtyAttributeValues( commandTimeout );
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

                var referenceDictionary = valueIds.ToDictionary( vid => vid, _ => referencedEntities );

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
                rockContext.Database.SetCommandTimeout( commandTimeout );

                return rockContext.Set<AttributeValue>()
                    .Where( av => av.IsPersistedValueDirty )
                    .Select( av => new
                    {
                        av.AttributeId,
                        av.Id,
                        av.Value
                    } )
                    .OrderBy( av => av.AttributeId )
                    .Take( 100_000 ) // Limit this query to 100,000 records at a time to avoid running out of memory.
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
                rockContext.Database.SetCommandTimeout( commandTimeout );

                return rockContext.Set<Rock.Model.Attribute>()
                    .Where( a => a.IsDefaultPersistedValueDirty )
                    .Select( a => a.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all the attribute identifiers that are marked as volatile.
        /// </summary>
        /// <returns>A list of attribute identifiers that have been determined to be volatile.</returns>
        private List<int> GetVolatileAttributeIds()
        {
            var attributeIds = new List<int>( 1000 );

            foreach ( var attribute in AttributeCache.All() )
            {
                try
                {
                    if ( attribute.FieldType.Field == null )
                    {
                        continue;
                    }

                    if ( attribute.FieldType.Field.IsPersistedValueVolatile( attribute.ConfigurationValues ) )
                    {
                        attributeIds.Add( attribute.Id );
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            return attributeIds;
        }

        /// <summary>
        /// Logs a message to either the information channel or debug channel
        /// depending on how long it took.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="milliseconds">The duration of the operation.</param>
        private void LogTimedMessage( string message, double milliseconds )
        {
            if ( milliseconds > 1_000 )
            {
                Logger.LogInformation( $"[{milliseconds:N0}ms] {message}" );
            }
            else
            {
                Logger.LogDebug( $"[{milliseconds:N0}ms] {message}" );
            }
        }

        /// <summary>
        /// Gets the local IP address as best can be determined.
        /// </summary>
        /// <returns>The IP address for this server.</returns>
        private static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry( Dns.GetHostName() );

                foreach ( var ip in host.AddressList )
                {
                    if ( ip.AddressFamily == AddressFamily.InterNetwork && !ip.IsIPv6LinkLocal && !ip.IsIPv6Multicast && !ip.IsIPv6SiteLocal )
                    {
                        return ip.ToString();
                    }
                }
            }
            catch
            {
                // Intentionally ignored.
            }

            return "127.0.0.1";
        }

        /// <summary>
        /// Quick and dirty class to throttle logging. Whenever we update
        /// the status message it writes to the database. We don't need that
        /// so just update every so often?
        /// </summary>
        private class ThrottleLogger
        {
            private readonly int _milliseconds;

            private readonly Action<string> _logAction;

            private readonly System.Diagnostics.Stopwatch _stopwatch;

            public ThrottleLogger( int milliseconds, Action<string> logAction )
            {
                _milliseconds = milliseconds;
                _logAction = logAction;
                _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }

            public void Write( string message, bool force = false )
            {
                if ( force || _stopwatch.ElapsedMilliseconds > _milliseconds )
                {
                    _logAction( message );

                    _stopwatch.Restart();
                }
            }
        }
    }
}
