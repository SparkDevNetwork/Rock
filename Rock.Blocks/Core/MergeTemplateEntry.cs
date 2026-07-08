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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Newtonsoft.Json.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.MergeTemplateEntry;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Merges an entity set with a pre-defined template to produce an output document (Word, HTML, etc.).
    /// </summary>
    [DisplayName( "Merge Template Entry" )]
    [Category( "Core" )]
    [Description( "Used for merging data into output documents, such as Word, Html, using a pre-defined template." )]
    [IconCssClass( "ti ti-files" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Database Timeout",
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultValue = "180",
        Order = 1,
        Key = AttributeKey.DatabaseTimeout )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "D2C255E9-8043-41E1-A905-B473512AE2C6" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "147B5FEF-1CC2-49E5-B1E0-1466244986C0" )]
    [Rock.SystemGuid.BlockTypeGuid( "8C6280DA-9BB4-47C8-96BA-3878B8B85466" )]
    public class MergeTemplateEntry : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DatabaseTimeout = "DatabaseTimeout";
        }

        private static class PageParameterKey
        {
            public const string Set = "Set";
        }

        #endregion Keys
            
        #region Constants

        /// <summary>
        /// The number of data rows shown in the preview.
        /// </summary>
        private const int PreviewRowCount = 15;

        /// <summary>
        /// The column type identifiers understood by the Obsidian grid's dynamic column components.
        /// </summary>
        private static class ColumnType
        {
            public const string Boolean = "boolean";
            public const string Currency = "currency";
            public const string Date = "date";
            public const string Number = "number";
            public const string Text = "text";
        }

        #endregion Constants

        #region Block Overrides

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<MergeTemplateEntryBag, MergeTemplateEntryOptionsBag>();

            box.Options.MergeTemplateOwnership = Rock.Enums.Controls.MergeTemplateOwnership.PersonalAndGlobal;
            box.Bag = GetInitializationBag();

            return box;
        }

        #endregion Block Overrides

        #region Methods

        /// <summary>
        /// Builds the initial state for the block based on the requested entity set.
        /// </summary>
        /// <returns>The state bag describing what the block should render.</returns>
        private MergeTemplateEntryBag GetInitializationBag()
        {
            var entitySetKey = PageParameter( PageParameterKey.Set );

            // With no entity set there is nothing to merge, so the entry panel stays hidden.
            if ( entitySetKey.IsNullOrWhiteSpace() )
            {
                return new MergeTemplateEntryBag { IsEntryPanelVisible = false };
            }

            var entitySet = GetEntitySet();
            if ( entitySet == null )
            {
                return new MergeTemplateEntryBag
                {
                    IsEntryPanelVisible = false,
                    WarningMessage = "Merge Records not found"
                };
            }

            return new MergeTemplateEntryBag
            {
                IsEntryPanelVisible = true,
                RecordCount = GetRecordCount( entitySet.Id ),
                IsCombineFamilyMembersVisible = GetIsCombineFamilyMembersVisible( entitySet )
            };
        }

        /// <summary>
        /// Resolves the entity set identified by the <c>Set</c> page parameter.
        /// </summary>
        /// <returns>The entity set, or <c>null</c> when one was not provided or could not be found.</returns>
        private EntitySet GetEntitySet()
        {
            var entitySetKey = PageParameter( PageParameterKey.Set );
            if ( entitySetKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new EntitySetService( RockContext ).Get( entitySetKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the number of items in the entity set.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <returns>The number of items that will be merged.</returns>
        private int GetRecordCount( int entitySetId )
        {
            return new EntitySetItemService( RockContext )
                .Queryable()
                .Count( a => a.EntitySetId == entitySetId );
        }

        /// <summary>
        /// Determines whether the "Combine Family Members" option applies to this entity set.
        /// It is only relevant for Person and Group Member sets.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns><c>true</c> if the option should be shown; otherwise <c>false</c>.</returns>
        private bool GetIsCombineFamilyMembersVisible( EntitySet entitySet )
        {
            if ( !entitySet.EntityTypeId.HasValue )
            {
                return false;
            }

            var entityTypeId = entitySet.EntityTypeId.Value;

            return entityTypeId == EntityTypeCache.GetId<Person>()
                || entityTypeId == EntityTypeCache.GetId<GroupMember>();
        }

        /// <summary>
        /// Applies the configured database timeout (when positive) to the block's context so that
        /// large merges do not time out prematurely.
        /// </summary>
        private void ApplyDatabaseTimeout()
        {
            var timeoutSeconds = GetAttributeValue( AttributeKey.DatabaseTimeout ).AsIntegerOrNull();
            if ( timeoutSeconds.HasValue && timeoutSeconds.Value > 0 )
            {
                RockContext.Database.CommandTimeout = timeoutSeconds.Value;
            }
        }

        /// <summary>
        /// Combines the common Lava merge fields with the global merge objects produced by the data source.
        /// </summary>
        /// <param name="globalDataSourceFields">The global merge objects from the data source.</param>
        /// <returns>The merged set of global Lava fields.</returns>
        private Dictionary<string, object> GetGlobalMergeFields( Dictionary<string, object> globalDataSourceFields )
        {
            var globalMergeFields = RequestContext.GetCommonMergeFields();

            foreach ( var kv in globalDataSourceFields )
            {
                globalMergeFields.TryAdd( kv.Key, kv.Value );
            }

            return globalMergeFields;
        }

        /// <summary>
        /// Logs any exceptions that were collected while creating the merge document.
        /// </summary>
        /// <param name="mergeTemplateType">The merge template type that produced the document.</param>
        /// <param name="mergeTemplate">The merge template.</param>
        private void LogMergeTemplateExceptions( MergeTemplateType mergeTemplateType, MergeTemplate mergeTemplate )
        {
            if ( mergeTemplateType.Exceptions == null || !mergeTemplateType.Exceptions.Any() )
            {
                return;
            }

            if ( mergeTemplateType.Exceptions.Count == 1 )
            {
                ExceptionLogService.LogException( mergeTemplateType.Exceptions[0] );
            }
            else if ( mergeTemplateType.Exceptions.Count > 50 )
            {
                ExceptionLogService.LogException( new AggregateException( $"Exceptions merging template {mergeTemplate.Name}. See InnerExceptions for top 50.", mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
            }
            else
            {
                ExceptionLogService.LogException( new AggregateException( $"Exceptions merging template {mergeTemplate.Name}. See InnerExceptions", mergeTemplateType.Exceptions.ToList() ) );
            }
        }

        /// <summary>
        /// Builds a small preview of the rows for an entity-based set as a typed grid
        /// (the columns and the rows the grid will render).
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The preview grid definition and data.</returns>
        private MergeTemplateEntryDataPreviewBag GetEntityDataPreview( EntitySet entitySet )
        {
            var entityType = EntityTypeCache.Get( entitySet.EntityTypeId.Value )?.GetEntityType();
            if ( entityType == null )
            {
                // Return an empty (but fully-initialized) grid so the preview simply shows "No Results".
                var emptyBuilder = new GridBuilder<IEntity>();
                return new MergeTemplateEntryDataPreviewBag
                {
                    GridDefinition = emptyBuilder.BuildDefinition(),
                    GridData = emptyBuilder.Build( Enumerable.Empty<IEntity>() )
                };
            }

            var builder = GetEntityPreviewGridBuilder( entityType );

            // Not AsNoTracking: a property marked [Previewable] may be a navigation property
            // that needs to lazy-load, matching the entities the legacy grid bound.
            var entities = new EntitySetService( RockContext )
                .GetEntityQuery( entitySet.Id )
                .Take( PreviewRowCount )
                .ToList();

            return new MergeTemplateEntryDataPreviewBag
            {
                GridDefinition = builder.BuildDefinition(),
                GridData = builder.Build( entities )
            };
        }

        /// <summary>
        /// Builds the grid for an entity-based preview, adding one typed column per preview property.
        /// </summary>
        /// <param name="entityType">The entity type being previewed.</param>
        /// <returns>The configured grid builder.</returns>
        private GridBuilder<IEntity> GetEntityPreviewGridBuilder( Type entityType )
        {
            var builder = new GridBuilder<IEntity>();

            // Track added field names so a colliding property (e.g. a base property hidden by
            // "new") cannot add a duplicate field, which GridBuilder.AddField rejects by throwing.
            var addedFieldNames = new HashSet<string>();

            foreach ( var property in GetPreviewProperties( entityType ) )
            {
                // Capture the property and column type so the per-row callback does not repeat the lookup.
                var propertyInfo = property;
                var fieldName = propertyInfo.Name.ToCamelCase();

                if ( !addedFieldNames.Add( fieldName ) )
                {
                    continue;
                }

                var title = propertyInfo.Name.SplitCase();
                var columnType = GetColumnType( propertyInfo );

                builder.AddField( fieldName, entity => FormatPreviewValue( propertyInfo.GetValue( entity ), columnType ) );
                builder.AddDefinitionAction( definition => definition.DynamicFields.Add( CreateDynamicField( fieldName, title, columnType ) ) );
            }

            return builder;
        }

        /// <summary>
        /// Builds a small preview of the rows for a set that only contains additional (non-entity)
        /// merge values, as a typed grid.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <returns>The preview grid definition and data.</returns>
        private MergeTemplateEntryDataPreviewBag GetNonEntityDataPreview( int entitySetId )
        {
            var items = new EntitySetItemService( RockContext )
                .GetByEntitySetId( entitySetId, true )
                .Take( PreviewRowCount )
                .ToList()
                .Select( a => a.AdditionalMergeValuesJson.FromJsonOrNull<Dictionary<string, object>>() )
                .Where( d => d != null )
                .ToList();

            if ( !items.Any() )
            {
                // Return an empty (but fully-initialized) grid so the preview simply shows "No Results".
                var emptyBuilder = new GridBuilder<Dictionary<string, object>>();
                return new MergeTemplateEntryDataPreviewBag
                {
                    GridDefinition = emptyBuilder.BuildDefinition(),
                    GridData = emptyBuilder.Build( items )
                };
            }

            var builder = GetNonEntityPreviewGridBuilder( items.First() );

            return new MergeTemplateEntryDataPreviewBag
            {
                GridDefinition = builder.BuildDefinition(),
                GridData = builder.Build( items )
            };
        }

        /// <summary>
        /// Builds the grid for a non-entity preview. The first row defines the columns (matching the
        /// legacy preview), and each column's type is inferred from that row's value.
        /// </summary>
        /// <param name="firstRow">The first row of merge values, used to define the columns.</param>
        /// <returns>The configured grid builder.</returns>
        private GridBuilder<Dictionary<string, object>> GetNonEntityPreviewGridBuilder( Dictionary<string, object> firstRow )
        {
            var builder = new GridBuilder<Dictionary<string, object>>();

            foreach ( var pair in firstRow )
            {
                // Capture the key so the per-row callback reads the correct merge value.
                var key = pair.Key;
                if ( key.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var title = key.SplitCase();
                var columnType = GetColumnTypeForValue( pair.Value );

                builder.AddField( key, row => FormatPreviewValue( row.TryGetValue( key, out var value ) ? value : null, columnType ) );
                builder.AddDefinitionAction( definition => definition.DynamicFields.Add( CreateDynamicField( key, title, columnType ) ) );
            }

            return builder;
        }

        /// <summary>
        /// Selects the properties of an entity type that should be shown in the preview. A type that
        /// declares <see cref="PreviewableAttribute"/> properties curates its own preview; otherwise
        /// all reportable scalar properties are used.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The properties to show, in declaration order.</returns>
        private static List<PropertyInfo> GetPreviewProperties( Type entityType )
        {
            var previewableProperties = new List<PropertyInfo>();
            var reportableProperties = new List<PropertyInfo>();

            foreach ( var property in entityType.GetProperties() )
            {
                if ( property.Name == "Id" )
                {
                    continue;
                }

                var getMethod = property.GetGetMethod();
                var isPreviewable = property.GetCustomAttribute<PreviewableAttribute>() != null;

                // Skip lazy-loaded navigation properties (virtual getters) unless explicitly marked previewable.
                var isScalarProperty = getMethod != null && ( !getMethod.IsVirtual || getMethod.IsFinal );
                if ( !isScalarProperty && !isPreviewable )
                {
                    continue;
                }

                if ( isPreviewable )
                {
                    previewableProperties.Add( property );
                }
                else if ( property.GetCustomAttribute<DataMemberAttribute>() != null
                    && property.GetCustomAttribute<HideFromReportingAttribute>() == null )
                {
                    reportableProperties.Add( property );
                }
            }

            return previewableProperties.Count > 0 ? previewableProperties : reportableProperties;
        }

        /// <summary>
        /// Formats a single preview cell value for the grid based on its column type. The grid's
        /// typed column components handle the final display (date formatting, the boolean glyph,
        /// currency formatting); other values are reduced to their string form.
        /// </summary>
        /// <param name="value">The raw value.</param>
        /// <param name="columnType">The grid column type the value will be rendered as.</param>
        /// <returns>The value to place in the grid row.</returns>
        private static object FormatPreviewValue( object value, string columnType )
        {
            if ( value == null )
            {
                return null;
            }

            // Enums render as their split-cased description rather than the raw identifier.
            if ( value is Enum enumValue )
            {
                return enumValue.ConvertToString();
            }

            // The date column parses an offset-aware ISO 8601 value, so normalize a DateTime to a Rock DateTimeOffset.
            if ( columnType == ColumnType.Date )
            {
                return value is DateTime dateTimeValue ? ( object ) dateTimeValue.ToRockDateTimeOffset() : value;
            }

            // The boolean and currency columns format the raw typed value themselves.
            if ( columnType == ColumnType.Boolean || columnType == ColumnType.Currency )
            {
                return value;
            }

            // Collections render as a comma-delimited list of their items.
            if ( value is IEnumerable enumerableValue && !( value is string ) )
            {
                return enumerableValue.Cast<object>()
                    .Where( item => item != null )
                    .Select( item => item.ToString() )
                    .ToList()
                    .AsDelimited( ", " );
            }

            // Everything else is reduced to its string form. Numbers use the number column type (so the
            // grid sorts them numerically) but are emitted as a pre-formatted string and rendered through
            // a text cell, so an Id shows as "12345" rather than "12,345", matching the legacy grid.
            return value.ToString();
        }

        /// <summary>
        /// Maps an entity property to the grid's dynamic column type, honoring an explicit
        /// currency <c>[BoundFieldType]</c> before falling back to the property's CLR type.
        /// </summary>
        /// <param name="propertyInfo">The property to map.</param>
        /// <returns>The grid column type identifier.</returns>
        private static string GetColumnType( PropertyInfo propertyInfo )
        {
            // Financial Amount fields opt into currency rendering via [BoundFieldType( typeof( CurrencyField ) )].
            if ( IsCurrencyField( propertyInfo ) )
            {
                return ColumnType.Currency;
            }

            var type = Nullable.GetUnderlyingType( propertyInfo.PropertyType ) ?? propertyInfo.PropertyType;

            if ( type == typeof( bool ) )
            {
                return ColumnType.Boolean;
            }

            // Only DateTime maps to the date-only column, matching the legacy grid's DateField
            // (which was used for DateTime/DateTime? only; other types, including DateTimeOffset,
            // fell through to a plain text column).
            if ( type == typeof( DateTime ) )
            {
                return ColumnType.Date;
            }

            // Enums render as their split-cased description (see FormatPreviewValue), so they use the
            // text column rather than the numeric column their underlying integral type would imply.
            if ( type.IsEnum )
            {
                return ColumnType.Text;
            }

            // Plain numbers use the number column so the grid sorts and filters them numerically. They
            // are still rendered through a text cell (see the .obs columnComponents mapping) so they
            // display without thousands separators, matching the legacy grid.
            if ( type == typeof( int ) || type == typeof( long ) || type == typeof( short ) || type == typeof( byte )
                || type == typeof( decimal ) || type == typeof( double ) || type == typeof( float ) )
            {
                return ColumnType.Number;
            }

            // Strings and everything else render as text.
            return ColumnType.Text;
        }

        /// <summary>
        /// Infers the grid's dynamic column type from a (non-entity) merge value.
        /// </summary>
        /// <param name="value">The first row's value for the column.</param>
        /// <returns>The grid column type identifier.</returns>
        private static string GetColumnTypeForValue( object value )
        {
            if ( value is bool )
            {
                return ColumnType.Boolean;
            }

            if ( value is DateTime )
            {
                return ColumnType.Date;
            }

            // A JSON value deserializes to long/double (and occasionally int/decimal); route these to the
            // number column so they sort and filter numerically while still rendering as a plain string.
            if ( value is int || value is long || value is short || value is byte
                || value is decimal || value is double || value is float )
            {
                return ColumnType.Number;
            }

            return ColumnType.Text;
        }

        /// <summary>
        /// Determines whether a property opts into currency rendering via
        /// <c>[BoundFieldType( typeof( CurrencyField ) )]</c>. The attribute is matched by name so this
        /// block never references the System.Web-coupled attribute or control types.
        /// </summary>
        /// <param name="propertyInfo">The property to inspect.</param>
        /// <returns><c>true</c> when the property is tagged to render as currency.</returns>
        private static bool IsCurrencyField( PropertyInfo propertyInfo )
        {
            return propertyInfo.GetCustomAttributesData()
                .Any( attributeData => attributeData.AttributeType.Name == "BoundFieldTypeAttribute"
                    && attributeData.ConstructorArguments.Any( argument => ( argument.Value as Type )?.Name == "CurrencyField" ) );
        }

        /// <summary>
        /// Creates a dynamic field definition for a preview column.
        /// </summary>
        /// <param name="fieldName">The grid field name (also the row object key).</param>
        /// <param name="title">The column header text.</param>
        /// <param name="columnType">The grid column type used to render the column.</param>
        /// <returns>The configured dynamic field definition.</returns>
        private static DynamicFieldDefinitionBag CreateDynamicField( string fieldName, string title, string columnType )
        {
            return new DynamicFieldDefinitionBag
            {
                Name = fieldName,
                Title = title,
                ColumnType = columnType,

                // Keep every preview column visible at all breakpoints.
                VisiblePriority = "xs"
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Generates the merge document and returns the URL the browser should use to download it.
        /// </summary>
        /// <param name="bag">The merge request details.</param>
        /// <returns>The download URL, or an error message describing why the merge failed.</returns>
        [BlockAction]
        public BlockActionResult Merge( MergeTemplateEntryMergeRequestBag bag )
        {
            var entitySet = GetEntitySet();
            if ( entitySet == null )
            {
                return ActionBadRequest( "Merge Records not found" );
            }

            ApplyDatabaseTimeout();

            var mergeTemplate = bag.MergeTemplateGuid.HasValue
                ? new MergeTemplateService( RockContext ).Get( bag.MergeTemplateGuid.Value )
                : null;
            if ( mergeTemplate == null )
            {
                return ActionOk( new MergeTemplateEntryMergeResponseBag { ErrorMessage = "Unable to get merge template", IsErrorDanger = true } );
            }

            var mergeTemplateType = mergeTemplate.GetMergeTemplateType();
            if ( mergeTemplateType == null )
            {
                return ActionOk( new MergeTemplateEntryMergeResponseBag { ErrorMessage = "Unable to get merge template type", IsErrorDanger = true } );
            }

            try
            {
                var dataSourceResult = new MergeDataSourceBuilder()
                    .GetMergeObjectsFromEntitySet( RockContext, entitySet.Id, bag.IsCombineFamilyMembers );
                if ( dataSourceResult.Error != null )
                {
                    throw dataSourceResult.Error;
                }

                var globalMergeFields = GetGlobalMergeFields( dataSourceResult.GlobalMergeObjects );

                var outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate,
                    dataSourceResult.DetailMergeObjects.Values.ToList(),
                    globalMergeFields );

                LogMergeTemplateExceptions( mergeTemplateType, mergeTemplate );

                return ActionOk( new MergeTemplateEntryMergeResponseBag
                {
                    DownloadUrl = $"{FileUrlHelper.GetFileUrl( outputBinaryFileDoc.Id )}&attachment=true"
                } );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                var errorMessage = ex is FormatException
                    ? "Error loading the merge template. Please verify that the merge template file is valid."
                    : "An error occurred while merging";

                return ActionOk( new MergeTemplateEntryMergeResponseBag
                {
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                } );
            }
        }

        /// <summary>
        /// Gets a small preview of the data rows that will be merged.
        /// </summary>
        /// <returns>The preview columns and rows.</returns>
        [BlockAction]
        public BlockActionResult GetDataPreview()
        {
            var entitySet = GetEntitySet();
            if ( entitySet == null )
            {
                return ActionBadRequest( "Merge Records not found" );
            }

            ApplyDatabaseTimeout();

            var preview = entitySet.EntityTypeId.HasValue
                ? GetEntityDataPreview( entitySet )
                : GetNonEntityDataPreview( entitySet.Id );

            return ActionOk( preview );
        }

        /// <summary>
        /// Gets the Lava merge fields help describing the data available to the selected template.
        /// </summary>
        /// <param name="bag">The merge request details (template and combine-family option).</param>
        /// <returns>The rendered Lava merge fields help.</returns>
        [BlockAction]
        public BlockActionResult GetMergeFieldsHelp( MergeTemplateEntryMergeRequestBag bag )
        {
            var entitySet = GetEntitySet();
            if ( entitySet == null )
            {
                return ActionBadRequest( "Merge Records not found" );
            }

            ApplyDatabaseTimeout();

            var dataSourceResult = new MergeDataSourceBuilder()
                .GetMergeObjectsFromEntitySet( RockContext, entitySet.Id, bag.IsCombineFamilyMembers, fetchCount: 1 );

            if ( dataSourceResult.Error != null )
            {
                ExceptionLogService.LogException( dataSourceResult.Error );
                return ActionBadRequest( "Unable to build the merge fields help." );
            }

            var detailMergeFields = dataSourceResult.DetailMergeObjects.Values.ToList();
            var globalMergeFields = GetGlobalMergeFields( dataSourceResult.GlobalMergeObjects );

            var mergeTemplate = bag.MergeTemplateGuid.HasValue
                ? new MergeTemplateService( RockContext ).Get( bag.MergeTemplateGuid.Value )
                : null;
            var mergeTemplateType = mergeTemplate?.GetMergeTemplateType();

            var lavaDebugHtml = mergeTemplateType != null
                ? mergeTemplateType.GetLavaDebugInfo( detailMergeFields, globalMergeFields )
                : MergeTemplateType.GetDefaultLavaDebugInfo( detailMergeFields, globalMergeFields );

            return ActionOk( new MergeTemplateEntryMergeFieldsHelpBag { LavaDebugHtml = lavaDebugHtml } );
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// Builds the detail and global merge objects for an entity set so they can be combined with a merge template.
        /// </summary>
        private class MergeDataSourceBuilder
        {
            /// <summary>
            /// Gets the merge objects for the specified entity set.
            /// </summary>
            /// <param name="rockContext">The context to query with. It must remain alive while the merge document is produced so navigation properties can lazy-load.</param>
            /// <param name="entitySetId">The entity set identifier.</param>
            /// <param name="combineFamilyMembers">When <c>true</c>, family members are combined into a single row.</param>
            /// <param name="fetchCount">When set, limits the number of rows produced.</param>
            /// <returns>The detail merge objects, the global merge objects, and any error that occurred.</returns>
            public GetMergeObjectsResult GetMergeObjectsFromEntitySet( RockContext rockContext, int entitySetId, bool combineFamilyMembers = false, int? fetchCount = null )
            {
                var personService = new PersonService( rockContext );
                var entitySetService = new EntitySetService( rockContext );
                var entitySet = entitySetService.Get( entitySetId );

                var result = new GetMergeObjectsResult();
                var mergeObjectsDictionary = new Dictionary<int, object>();
                var globalObjectDictionary = new Dictionary<string, object>();

                // First add any IEntity items the set contains.
                if ( entitySet?.EntityTypeId != null )
                {
                    var qryEntity = entitySetService.GetEntityQuery( entitySetId );
                    if ( fetchCount.HasValue )
                    {
                        qryEntity = qryEntity.Take( fetchCount.Value );
                    }

                    var entityTypeCache = EntityTypeCache.Get( entitySet.EntityTypeId.Value );
                    var isPersonEntityType = entityTypeCache != null && entityTypeCache.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid();
                    var isGroupMemberEntityType = entityTypeCache != null && entityTypeCache.Guid == Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid();

                    // Expose the parent groups to the global merge objects.
                    if ( isGroupMemberEntityType )
                    {
                        var groups = qryEntity.OfType<GroupMember>().Select( a => a.Group ).DistinctBy( gm => gm.Id ).ToList();

                        globalObjectDictionary.AddOrReplace( "Groups", groups );

                        // Add the first entry as a singleton reference for convenience.
                        globalObjectDictionary.AddOrReplace( "Group", groups.FirstOrDefault() );
                    }

                    if ( ( isGroupMemberEntityType || isPersonEntityType ) && combineFamilyMembers )
                    {
                        IQueryable<IEntity> qryPersons;
                        if ( isGroupMemberEntityType )
                        {
                            qryPersons = qryEntity.OfType<GroupMember>().Select( a => a.Person );
                        }
                        else
                        {
                            qryPersons = qryEntity;
                        }

                        var familyGroupType = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

                        // Keep this as an unexecuted query so EF generates a subquery. Materializing it could
                        // overflow the batch-size limit when used to filter a large set of persons.
                        var qryPersonIds = qryPersons.Select( a => a.Id );

                        if ( isGroupMemberEntityType )
                        {
                            qryPersons = qryPersons.Distinct();
                        }

                        var qryFamilyGroupMembers = new GroupMemberService( rockContext ).Queryable( "GroupRole,Person" ).AsNoTracking()
                            .Where( a => a.Group.GroupType.Guid == familyGroupType )
                            .Where( a => qryPersonIds.Contains( a.PersonId ) );

                        var qryCombined = qryFamilyGroupMembers.Join(
                            qryPersons,
                            m => m.PersonId,
                            p => p.Id,
                            ( m, p ) => new { GroupMember = m, Person = p } )
                            .GroupBy( a => a.GroupMember.GroupId )
                            .Select( x => new
                            {
                                GroupId = x.Key,
                                // Order people to match the ordering used in the Group Members block.
                                Persons =
                                        // Adult Male
                                        x.Where( xx => xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                        xx.GroupMember.Person.Gender == Gender.Male ).OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person )
                                        // Adult Female
                                        .Concat( x.Where( xx => xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                        xx.GroupMember.Person.Gender != Gender.Male ).OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person ) )
                                        // Non-adults
                                        .Concat( x.Where( xx => !xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                                        .OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person ) )
                            } );

                        // Pre-fetch the first group member per person once so the loop below does not query per family.
                        // Left tracked (not AsNoTracking) so the group member's navigation properties can still lazy-load when used by Lava.
                        Dictionary<int, GroupMember> groupMemberByPersonId = null;
                        if ( isGroupMemberEntityType )
                        {
                            groupMemberByPersonId = qryEntity.OfType<GroupMember>().ToList()
                                .GroupBy( gm => gm.PersonId )
                                .ToDictionary( g => g.Key, g => g.First() );
                        }

                        foreach ( var combinedFamilyItem in qryCombined )
                        {
                            var personIds = combinedFamilyItem.Persons.Select( a => a.Id ).Distinct().ToArray();

                            var primaryGroupPerson = combinedFamilyItem.Persons.FirstOrDefault() as Person;
                            if ( primaryGroupPerson == null )
                            {
                                continue;
                            }

                            // If the primary person is already in the merge list, use the first family member that isn't.
                            if ( mergeObjectsDictionary.ContainsKey( primaryGroupPerson.Id ) )
                            {
                                foreach ( var person in combinedFamilyItem.Persons )
                                {
                                    if ( !mergeObjectsDictionary.ContainsKey( person.Id ) )
                                    {
                                        primaryGroupPerson = person as Person;
                                        break;
                                    }
                                }
                            }

                            // For a Group Member set, attach the primary person's group member fields.
                            if ( isGroupMemberEntityType && groupMemberByPersonId != null )
                            {
                                primaryGroupPerson.AdditionalLavaFields = primaryGroupPerson.AdditionalLavaFields ?? new Dictionary<string, object>();
                                if ( groupMemberByPersonId.TryGetValue( primaryGroupPerson.Id, out var groupMember ) )
                                {
                                    primaryGroupPerson.AdditionalLavaFields.TryAdd( "GroupMember", groupMember );
                                }
                            }

                            object mergeObject;
                            if ( combinedFamilyItem.Persons.Count() > 1 )
                            {
                                var combinedPerson = primaryGroupPerson.ToJson().FromJsonOrNull<MergeTemplateCombinedPerson>();

                                combinedPerson.FullName = Person.CalculateFamilySalutation( primaryGroupPerson, new Person.CalculateFamilySalutationArgs( true ) { LimitToPersonIds = personIds, RockContext = rockContext } );

                                var firstNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).FirstName ).ToList();
                                var nickNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).NickName ).ToList();

                                combinedPerson.FirstName = firstNameList.AsDelimited( ", ", " & " );
                                combinedPerson.NickName = nickNameList.AsDelimited( ", ", " & " );
                                combinedPerson.LastName = primaryGroupPerson.LastName;
                                combinedPerson.SuffixValueId = null;
                                combinedPerson.SuffixValue = null;
                                mergeObject = combinedPerson;
                            }
                            else
                            {
                                mergeObject = primaryGroupPerson;
                            }

                            mergeObjectsDictionary.TryAdd( primaryGroupPerson.Id, mergeObject );
                        }

                        // Restore the original selection order.
                        var orderedPersonIdList = qryPersonIds.ToList();
                        mergeObjectsDictionary = mergeObjectsDictionary.OrderBy( a => orderedPersonIdList.IndexOf( a.Key ) ).ToDictionary( x => x.Key, y => y.Value );
                    }
                    else if ( isGroupMemberEntityType )
                    {
                        var attachedPersonIds = new HashSet<int>();

                        foreach ( var groupMember in qryEntity.AsNoTracking().OfType<GroupMember>() )
                        {
                            var person = groupMember.Person;

                            // Attach the person so navigation properties can lazy-load if the template needs them.
                            if ( attachedPersonIds.Add( person.Id ) )
                            {
                                personService.Attach( person );
                            }

                            person.AdditionalLavaFields = new Dictionary<string, object>
                            {
                                { "GroupMember", groupMember }
                            };

                            mergeObjectsDictionary.TryAdd( groupMember.PersonId, person );
                        }
                    }
                    else
                    {
                        foreach ( var item in qryEntity.AsNoTracking() )
                        {
                            mergeObjectsDictionary.TryAdd( item.Id, item );
                        }
                    }
                }

                // Add the additional (non-entity) merge values whether or not the set contained IEntity items.
                var emptyJson = new[] { string.Empty, "{}" };
                var entitySetItemMergeValuesQry = new EntitySetItemService( rockContext ).GetByEntitySetId( entitySetId, true )
                    .Where( a => !emptyJson.Contains( a.AdditionalMergeValuesJson ) );

                if ( fetchCount.HasValue )
                {
                    entitySetItemMergeValuesQry = entitySetItemMergeValuesQry.Take( fetchCount.Value );
                }

                // The entity id to use for non-entity objects.
                var nonEntityId = 1;

                foreach ( var additionalMergeValuesItem in entitySetItemMergeValuesQry.AsNoTracking() )
                {
                    object mergeObject;
                    var entityId = additionalMergeValuesItem.EntityId > 0
                        ? additionalMergeValuesItem.EntityId
                        : nonEntityId++;

                    if ( mergeObjectsDictionary.ContainsKey( entityId ) )
                    {
                        mergeObject = mergeObjectsDictionary[entityId];
                    }
                    else
                    {
                        // If the set already holds real entities, don't add stray non-entity items.
                        if ( entitySet.EntityTypeId.HasValue )
                        {
                            continue;
                        }

                        mergeObject = new Dictionary<string, object>();
                        mergeObjectsDictionary.TryAdd( entityId, mergeObject );
                    }

                    foreach ( var additionalMergeValue in additionalMergeValuesItem.AdditionalMergeValues )
                    {
                        if ( mergeObject is IEntity mergeEntity )
                        {
                            mergeEntity.AdditionalLavaFields = mergeEntity.AdditionalLavaFields ?? new Dictionary<string, object>();
                            var mergeValueObject = additionalMergeValue.Value;

                            // Convert a JArray into an ExpandoObject (or list of them) so Lava can work with it.
                            if ( mergeValueObject is JArray )
                            {
                                var jsonOfObject = mergeValueObject.ToJson();
                                try
                                {
                                    mergeValueObject = jsonOfObject.FromJsonDynamicOrNull();
                                }
                                catch ( Exception ex )
                                {
                                    result.Error = new Exception( "MergeTemplateEntry couldn't do a FromJSON", ex );
                                }
                            }

                            mergeEntity.AdditionalLavaFields.TryAdd( additionalMergeValue.Key, mergeValueObject );
                        }
                        else if ( mergeObject is IDictionary<string, object> nonEntityObject )
                        {
                            nonEntityObject.TryAdd( additionalMergeValue.Key, additionalMergeValue.Value );
                        }
                        else
                        {
                            result.Error = new Exception( $"Unexpected MergeObject Type: {mergeObject}" );
                            return result;
                        }
                    }
                }

                result.GlobalMergeObjects = globalObjectDictionary;

                var detailObjects = fetchCount.HasValue
                    ? mergeObjectsDictionary.Take( fetchCount.Value )
                    : mergeObjectsDictionary;

                result.DetailMergeObjects = detailObjects.ToDictionary( k => k.Key.ToString(), v => v.Value );

                return result;
            }

            /// <summary>
            /// The result of building merge objects from an entity set.
            /// </summary>
            public class GetMergeObjectsResult
            {
                /// <summary>
                /// Gets or sets the global merge objects (for example, the parent groups).
                /// </summary>
                public Dictionary<string, object> GlobalMergeObjects { get; set; }

                /// <summary>
                /// Gets or sets the per-row detail merge objects, keyed by their identifier.
                /// </summary>
                public Dictionary<string, object> DetailMergeObjects { get; set; }

                /// <summary>
                /// Gets or sets an error that occurred while building the merge objects, if any.
                /// </summary>
                public Exception Error { get; set; }
            }

            /// <summary>
            /// A <see cref="Person"/> whose <see cref="FullName"/> can be set, used to represent a
            /// combined family (for example, "Ted &amp; Cindy Decker").
            /// </summary>
            private class MergeTemplateCombinedPerson : Person
            {
                /// <summary>
                /// Gets or sets the family title to use in place of the derived full name.
                /// </summary>
                [DataMember]
                public new string FullName { get; set; }
            }
        }

        #endregion Support Classes
    }
}
