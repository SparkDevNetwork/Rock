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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Reporting;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.DataViewResults;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Shows the results of a data view in a grid whose columns are generated at
    /// runtime from the data view's entity type.
    /// </summary>
    [DisplayName( "Data View Results" )]
    [Category( "Reporting" )]
    [Description( "Shows the details of the given data view." )]
    [IconCssClass( "ti ti-table" )]
    [SupportedSiteTypes( SiteType.Web )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]

    [BooleanField(
        "Enable Counting Data View Statistics",
        Key = AttributeKey.EnableCountingDataViewStatistics,
        Description = "Set this to false to prevent this block from counting data view statistics.",
        DefaultBooleanValue = true,
        Order = 1 )]

    [CustomizedGrid]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]

    [Rock.SystemGuid.EntityTypeGuid( "80345DE5-1E67-4CB0-99D1-12113EA0215C" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "0C3ACA71-5594-46DA-816A-026187298D4C" )]
    [Rock.SystemGuid.BlockTypeGuid( "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866" )]
    public class DataViewResults : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The attribute keys for the block settings.
        /// </summary>
        private static class AttributeKey
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
            public const string EnableCountingDataViewStatistics = "EnableCountingDataViewStatistics";
        }

        /// <summary>
        /// The page parameter keys for the block.
        /// </summary>
        private static class PageParameterKey
        {
            public const string DataViewId = "DataViewId";
        }

        /// <summary>
        /// The column type identifiers understood by the Obsidian grid's dynamic
        /// column components.
        /// </summary>
        private static class ColumnType
        {
            public const string Boolean = "boolean";
            public const string Currency = "currency";
            public const string Date = "date";
            public const string Number = "number";
            public const string Text = "text";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The database timeout, in seconds, used when the block setting is not configured.
        /// </summary>
        private const int DefaultDatabaseTimeoutSeconds = 180;

        /// <summary>
        /// The grid field name that holds each row's key (the entity IdKey). It is populated for
        /// every row to back the row key and person features, but is never shown as a column.
        /// </summary>
        private const string KeyFieldName = "idKey";

        /// <summary>
        /// The visible priority that keeps a dynamic column visible at every breakpoint.
        /// </summary>
        private const string VisiblePriorityExtraSmall = "xs";

        private const string BoundFieldTypeAttributeName = "BoundFieldTypeAttribute";
        private const string CurrencyFieldControlName = "CurrencyField";

        /// <summary>
        /// Framework audit and foreign-key property names that are excluded from the
        /// <em>fallback</em> preview columns in <see cref="GetPreviewProperties"/>.
        /// <para>
        /// When an entity declares one or more <see cref="PreviewableAttribute"/>
        /// properties, only those are shown and this set is never consulted. But when
        /// an entity has no <see cref="PreviewableAttribute"/> properties at all, the
        /// fallback path surfaces every serializable, reporting-visible property. Without
        /// this exclusion, these framework columns (Guid, Foreign*, audit dates) would
        /// leak into the results preview even though they carry no meaningful value for
        /// the user.
        /// </para>
        /// </summary>
        private static readonly HashSet<string> SystemPropertyNames = new HashSet<string>
        {
            nameof( IEntity.Guid ),
            nameof( IEntity.ForeignId ),
            nameof( IEntity.ForeignGuid ),
            nameof( IEntity.ForeignKey ),
            nameof( IModel.CreatedDateTime ),
            nameof( IModel.ModifiedDateTime )
        };

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<DataViewResultsOptionsBag>();

            var dataView = GetDataView();
            var entityType = EntityTypeCache.Get( dataView?.EntityTypeId ?? 0 )?.GetEntityType();

            // Render nothing unless there is a viewable data view with a resolvable entity type.
            if ( entityType == null || !dataView.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return box;
            }

            box.Options = GetBoxOptions( dataView );
            box.GridDefinition = GetGridBuilder( entityType ).BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the additional configuration options for the block.
        /// </summary>
        /// <param name="dataView">The data view whose results are shown.</param>
        /// <returns>The configured options bag.</returns>
        private DataViewResultsOptionsBag GetBoxOptions( DataViewCache dataView )
        {
            var entityTypeCache = EntityTypeCache.Get( dataView.EntityTypeId.Value );
            var isPersonDataSet = dataView.EntityTypeId == EntityTypeCache.GetId<Person>();

            return new DataViewResultsOptionsBag
            {
                IsBlockVisible = true,
                IsPersonDataSet = isPersonDataSet,
                EntityTypeGuid = entityTypeCache.Guid,
                PersonKeyField = isPersonDataSet ? KeyFieldName : null,
                CommunicationRecipientFields = isPersonDataSet ? new List<string> { KeyFieldName } : null,
                ExportTitle = dataView.Name,
                ItemTerm = entityTypeCache.FriendlyName
            };
        }

        /// <summary>
        /// Resolves the data view referenced by the page parameter, or <c>null</c>
        /// when none is in scope.
        /// </summary>
        /// <returns>The cached data view, or <c>null</c>.</returns>
        private DataViewCache GetDataView()
        {
            var dataViewKey = PageParameter( PageParameterKey.DataViewId );
            if ( dataViewKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return DataViewCache.Get( dataViewKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Builds the grid for the supplied entity type, adding a hidden key field
        /// plus one dynamic column per preview property.
        /// </summary>
        /// <param name="entityType">The entity type the data view returns.</param>
        /// <returns>The configured grid builder.</returns>
        private GridBuilder<IEntity> GetGridBuilder( Type entityType )
        {
            var builder = new GridBuilder<IEntity>()
                .WithBlock( this )
                .AddField( KeyFieldName, entity => entity.IdKey );

            // Track added field names so colliding properties (e.g. a base property hidden by
            // "new") cannot add a duplicate field, which GridBuilder.AddField rejects by throwing.
            var addedFieldNames = new HashSet<string> { KeyFieldName };

            foreach ( var property in GetPreviewProperties( entityType ) )
            {
                // Resolve each property's metadata once and capture it in the value
                // extractor so the per-row callback does not repeat the reflection lookup.
                var propertyInfo = property;
                var fieldName = propertyInfo.Name.ToCamelCase();

                // Skip any property whose field name is already taken to avoid a duplicate field.
                if ( !addedFieldNames.Add( fieldName ) )
                {
                    continue;
                }

                var title = propertyInfo.Name.SplitCase();
                var columnType = GetColumnType( propertyInfo );
                var underlyingType = Nullable.GetUnderlyingType( propertyInfo.PropertyType ) ?? propertyInfo.PropertyType;

                builder.AddField( fieldName, entity =>
                {
                    var value = propertyInfo.GetValue( entity );
                    if ( value == null )
                    {
                        return null;
                    }

                    if ( underlyingType.IsEnum )
                    {
                        return ( ( Enum ) value ).ConvertToString();
                    }

                    if ( columnType == ColumnType.Date )
                    {
                        // The date column parses an offset-aware ISO 8601 value, so normalize DateTime to a Rock DateTimeOffset.
                        return value is DateTime dateTimeValue ? ( object ) dateTimeValue.ToRockDateTimeOffset() : value;
                    }

                    if ( value is IEnumerable enumerableValue && !( value is string ) )
                    {
                        return enumerableValue.Cast<object>()
                            .Where( item => item != null )
                            .Select( item => item.ToString() )
                            .ToList()
                            .AsDelimited( ", " );
                    }

                    return value;
                } );

                builder.AddDefinitionAction( definition =>
                {
                    definition.DynamicFields.Add( new DynamicFieldDefinitionBag
                    {
                        Name = fieldName,
                        Title = title,
                        ColumnType = columnType,
                        EnableFiltering = true,
                        VisiblePriority = VisiblePriorityExtraSmall
                    } );
                } );
            }

            // Guarantee at least one visible column. A grid with no visible columns renders
            // no rows even when the query returns data, so fall back to showing the Id.
            if ( addedFieldNames.Count == 1 )
            {
                builder.AddField( "id", entity => entity.Id );
                builder.AddDefinitionAction( definition =>
                {
                    definition.DynamicFields.Add( new DynamicFieldDefinitionBag
                    {
                        Name = "id",
                        Title = "Id",
                        ColumnType = ColumnType.Number,
                        EnableFiltering = true,
                        VisiblePriority = VisiblePriorityExtraSmall
                    } );
                } );
            }

            return builder;
        }

        /// <summary>
        /// Gets the entity properties to show as preview columns. Prefers properties
        /// decorated with <see cref="PreviewableAttribute"/>; when none exist, falls
        /// back to serializable properties that are not hidden from reporting.
        /// </summary>
        /// <param name="entityType">The entity type to inspect.</param>
        /// <returns>The ordered list of properties to expose as columns.</returns>
        private static List<PropertyInfo> GetPreviewProperties( Type entityType )
        {
            // Exclude IdKey alongside Id: it is already the hidden key field, has no display
            // value, and would otherwise collide with the reserved "idKey" field.
            var consideredProperties = entityType.GetProperties()
                .Where( p => p.Name != nameof( IEntity.Id )
                    && p.Name != nameof( IEntity.IdKey )
                    && p.GetIndexParameters().Length == 0 )
                .Where( p =>
                {
                    if ( p.GetCustomAttribute<PreviewableAttribute>() != null )
                    {
                        return true;
                    }

                    var getMethod = p.GetGetMethod();
                    return getMethod != null && ( !getMethod.IsVirtual || getMethod.IsFinal );
                } )
                .ToList();

            var previewableProperties = consideredProperties
                .Where( p => p.GetCustomAttribute<PreviewableAttribute>() != null )
                .ToList();

            if ( previewableProperties.Any() )
            {
                return previewableProperties;
            }

            // Fallback: serializable, reporting-visible properties, minus the framework
            // audit and foreign-key columns that are noise in a results preview.
            return consideredProperties
                .Where( p => p.GetCustomAttribute<DataMemberAttribute>() != null
                    && p.GetCustomAttribute<HideFromReportingAttribute>() == null
                    && !SystemPropertyNames.Contains( p.Name ) )
                .ToList();
        }

        /// <summary>
        /// Maps a property to the grid's dynamic column type. Honors an explicit
        /// <c>[BoundFieldType]</c> before falling back to the property's CLR type.
        /// </summary>
        /// <param name="propertyInfo">The property to map.</param>
        /// <returns>The grid column type identifier.</returns>
        private static string GetColumnType( PropertyInfo propertyInfo )
        {
            // Financial Amount fields render as currency via [BoundFieldType( typeof( CurrencyField ) )].
            if ( IsCurrencyField( propertyInfo ) )
            {
                return ColumnType.Currency;
            }

            var type = Nullable.GetUnderlyingType( propertyInfo.PropertyType ) ?? propertyInfo.PropertyType;

            if ( type == typeof( bool ) )
            {
                return ColumnType.Boolean;
            }

            if ( type == typeof( DateTime ) || type == typeof( DateTimeOffset ) )
            {
                return ColumnType.Date;
            }

            if ( type.IsEnum )
            {
                return ColumnType.Text;
            }

            if ( type == typeof( int ) || type == typeof( long ) || type == typeof( short ) || type == typeof( byte )
                || type == typeof( decimal ) || type == typeof( double ) || type == typeof( float ) )
            {
                return ColumnType.Number;
            }

            return ColumnType.Text;
        }

        /// <summary>
        /// Determines whether a property opts into currency rendering via
        /// <c>[BoundFieldType( typeof( CurrencyField ) )]</c> (the financial Amount fields). The
        /// attribute metadata is matched by type name so this block never references the
        /// System.Web-coupled attribute or control types (and never constructs the attribute).
        /// </summary>
        /// <param name="propertyInfo">The property to inspect.</param>
        /// <returns><c>true</c> when the property is tagged to render as currency.</returns>
        private static bool IsCurrencyField( PropertyInfo propertyInfo )
        {
            return propertyInfo.GetCustomAttributesData()
                .Any( attributeData => attributeData.AttributeType.Name == BoundFieldTypeAttributeName
                    && attributeData.ConstructorArguments.Any( argument => ( argument.Value as Type )?.Name == CurrencyFieldControlName ) );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Runs the data view query and returns the materialized grid rows.
        /// </summary>
        /// <returns>The grid data, or a friendly error message when the query fails.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            var dataView = GetDataView();
            if ( dataView?.EntityTypeId == null )
            {
                return ActionOk( new GridDataBag() );
            }

            var entityType = EntityTypeCache.Get( dataView.EntityTypeId.Value )?.GetEntityType();
            if ( entityType == null )
            {
                return ActionOk( new GridDataBag() );
            }

            if ( !dataView.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionOk( new GridDataBag() );
            }

            var enableCountingStatistics = GetAttributeValue( AttributeKey.EnableCountingDataViewStatistics ).AsBooleanOrNull() ?? true;
            var databaseTimeoutSeconds = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? DefaultDatabaseTimeoutSeconds;

            var queryOptions = new GetQueryableOptions
            {
                DatabaseTimeoutSeconds = databaseTimeoutSeconds,
                DataViewFilterOverrides = new DataViewFilterOverrides
                {
                    ShouldUpdateStatics = enableCountingStatistics
                }
            };

            try
            {
                var items = dataView.GetQuery( queryOptions ).AsNoTracking().ToList();

                return ActionOk( GetGridBuilder( entityType ).Build( items ) );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                if ( sqlTimeoutException != null )
                {
                    return ActionBadRequest( "This data view did not complete in a timely manner. You can try again or adjust the timeout setting of this block." );
                }

                if ( ex is RockDataViewFilterExpressionException rockDataViewFilterExpressionException )
                {
                    return ActionBadRequest( rockDataViewFilterExpressionException.GetFriendlyMessage( dataView ) );
                }

                return ActionBadRequest( "There was a problem loading the data view results." );
            }
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid. This
        /// backs the bulk update, person merge, merge template, and launch workflow
        /// grid actions.
        /// </summary>
        /// <param name="entitySet">The entity set bag describing the selected rows.</param>
        /// <returns>An action result that contains the identifier of the entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        /// <summary>
        /// Creates a communication for the subset of selected rows in the grid. This
        /// backs the communicate grid action available when the data view returns people.
        /// </summary>
        /// <param name="communication">The communication bag describing the recipients.</param>
        /// <returns>An action result that contains the identifier of the communication.</returns>
        [BlockAction]
        public BlockActionResult CreateGridCommunication( GridCommunicationBag communication )
        {
            if ( communication == null )
            {
                return ActionBadRequest( "No communication data was provided." );
            }

            var rockCommunication = GridHelper.CreateCommunication( communication, RequestContext );

            if ( rockCommunication == null )
            {
                return ActionBadRequest( "Grid has no recipients." );
            }

            return ActionOk( rockCommunication.Id.ToString() );
        }

        #endregion Block Actions
    }
}
