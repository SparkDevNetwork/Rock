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
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Blocks;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Obsidian.UI
{
    /// <summary>
    /// Extension methods for <see cref="GridBuilder{T}"/>. This provides much of
    /// specific use functionality of the builder.
    /// </summary>
    public static class GridBuilderExtensions
    {
        /// <summary>
        /// Adds a new person field to the grid definition.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="name">The name of the field to be added.</param>
        /// <param name="valueExpression">The expression that provides the <see cref="Person"/> to use for the cell value.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public static GridBuilder<T> AddPersonField<T>( this GridBuilder<T> builder, string name, Func<T, Person> valueExpression )
        {
            return builder.AddField( name, row =>
            {
                var person = valueExpression( row );

                if ( person == null )
                {
                    return null;
                }

                return new PersonFieldBag
                {
                    NickName = person.NickName,
                    LastName = person.LastName,
                    PhotoUrl = person.PhotoUrl
                };
            } );
        }

        /// <summary>
        /// Adds a new date and time field to the grid definition.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="name">The name of the field to be added.</param>
        /// <param name="valueExpression">The expression that provides the <see cref=" DateTime"/> to use for the cell value.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public static GridBuilder<T> AddDateTimeField<T>( this GridBuilder<T> builder, string name, Func<T, DateTime?> valueExpression )
        {
            return builder.AddField( name, row => valueExpression( row )?.ToRockDateTimeOffset() );
        }

        /// <summary>
        /// Adds a new plain text field to the grid definition.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="name">The name of the field to be added.</param>
        /// <param name="valueExpression">The expression that provides the <see cref="string"/> to use for the cell value.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public static GridBuilder<T> AddTextField<T>( this GridBuilder<T> builder, string name, Func<T, string> valueExpression )
        {
            return builder.AddField( name, row => valueExpression( row ) );
        }

        /// <summary>
        /// Adds a set of attribute field to the grid definition.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="attributes">The attributes that should be added to the grid definition.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public static GridBuilder<T> AddAttributeFields<T>( this GridBuilder<T> builder, IEnumerable<AttributeCache> attributes )
            where T : IHasAttributes
        {
            if ( !typeof( IHasAttributes ).IsAssignableFrom( typeof( T ) ) )
            {
                throw new Exception( $"The type '{typeof( T ).FullName}' does not support attributes." );
            }

            return builder.AddAttributeFieldsFrom( item => ( IHasAttributes ) item, attributes );
        }

        /// <summary>
        /// Adds a set of attribute field to the grid definition.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="selector">The expression to call to get the <see cref="IHasAttributes"/> object from the item.</param>
        /// <param name="attributes">The attributes that should be added to the grid definition.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public static GridBuilder<T> AddAttributeFieldsFrom<T>( this GridBuilder<T> builder, Func<T, IHasAttributes> selector, IEnumerable<AttributeCache> attributes )
        {
            foreach ( var attribute in attributes )
            {
                var key = attribute.Key;
                var fieldKey = $"attr_{key}";

                builder.AddField( fieldKey, item =>
                {
                    var attributesItem = selector( item );

                    return attributesItem.GetAttributeCondensedHtmlValue( key );
                } );

                builder.AddDefinitionAction( definition =>
                {
                    var textFieldTypeGuid = SystemGuid.FieldType.TEXT.AsGuid();

                    definition.AttributeFields.Add( new AttributeFieldDefinitionBag
                    {
                        Name = fieldKey,
                        Title = attribute.Name,
                        FieldTypeGuid = attribute.FieldType?.Guid ?? textFieldTypeGuid
                    } );
                } );
            }

            return builder;
        }

        /// <summary>
        /// Adds all the standard features when displaying a grid as part of a block.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="block">The block that is displaying this grid.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public static GridBuilder<T> WithBlock<T>( this GridBuilder<T> builder, IRockBlockType block )
        {
            return WithBlock( builder, block, null );
        }

        /// <summary>
        /// Adds all the standard features when displaying a grid as part of a block.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="block">The block that is displaying this grid.</param>
        /// <param name="options">The options that describe optional configuration data.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public static GridBuilder<T> WithBlock<T>( this GridBuilder<T> builder, IRockBlockType block, GridBuilderGridOptions<T> options )
        {
            // Add all the action URLs for the current site.
            AddDefaultGridActionUrls( builder, block );

            // Add any custom columns that are defined in the block settings.
            AddCustomGridColumns( builder, block, options.LavaObject );

            // Add any custom actions that are defined in the block settings.
            AddCustomGridActions( builder, block );

            return builder;
        }

        /// <summary>
        /// Adds the default grid action urls relative to the specified block.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="block">The block that is displaying this grid.</param>
        private static void AddDefaultGridActionUrls<T>( GridBuilder<T> builder, IRockBlockType block )
        {
            builder.AddDefinitionAction( definition =>
            {
                string communicationUrl = GetCommunicationRoute( block );

                if ( communicationUrl.IsNotNullOrWhiteSpace() )
                {
                    definition.ActionUrls.AddOrIgnore( GridActionUrlKey.Communicate, communicationUrl );
                }

                if ( IsAuthorizedForRoute( block.RequestContext, "/PersonMerge/{EntitySetId}" ) )
                {
                    definition.ActionUrls.AddOrIgnore( GridActionUrlKey.MergePerson, "/PersonMerge/{EntitySetId}" );
                }

                if ( IsAuthorizedForRoute( block.RequestContext, "/BusinessMerge/{EntitySetId}" ) )
                {
                    definition.ActionUrls.AddOrIgnore( GridActionUrlKey.MergeBusiness, "/BusinessMerge/{EntitySetId}" );
                }

                if ( IsAuthorizedForRoute( block.RequestContext, "/BulkUpdate/{EntitySetId}" ) )
                {
                    definition.ActionUrls.AddOrIgnore( GridActionUrlKey.BulkUpdate, "/BulkUpdate/{EntitySetId}" );
                }

                if ( IsAuthorizedForRoute( block.RequestContext, "/LaunchWorkflows/{EntitySetId}" ) )
                {
                    definition.ActionUrls.AddOrIgnore( GridActionUrlKey.LaunchWorkflow, "/LaunchWorkflows/{EntitySetId}" );
                }

                if ( IsAuthorizedForRoute( block.RequestContext, "/MergeTemplate/{EntitySetId}" ) )
                {
                    definition.ActionUrls.AddOrIgnore( GridActionUrlKey.MergeTemplate, "/MergeTemplate/{EntitySetId}" );
                }
            } );
        }

        /// <summary>
        /// Adds the custom grid actions that are defined in the block settings.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="block">The block that is displaying this grid.</param>
        private static void AddCustomGridActions<T>( GridBuilder<T> builder, IRockBlockType block )
        {
            var customizedGrid = block.GetType().GetCustomAttribute<CustomizedGridAttribute>();

            if ( customizedGrid == null )
            {
                return;
            }

            var enableStickyHeaders = block.BlockCache.GetAttributeValue( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey ).AsBoolean();
            var enableLaunchWorkflow = block.BlockCache.GetAttributeValue( CustomGridOptionsConfig.EnableDefaultWorkflowLauncherAttributeKey ).AsBoolean();
            var customActions = block.BlockCache.GetAttributeValue( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey ).FromJsonOrNull<List<CustomActionConfig>>();

            builder.AddDefinitionAction( definition =>
            {
                definition.EnableStickyHeader = customizedGrid.IsStickyHeaderSupported && enableStickyHeaders;
                definition.EnableLaunchWorkflow = !customizedGrid.IsCustomActionsSupported || enableLaunchWorkflow;

                if ( customizedGrid.IsCustomActionsSupported && customActions != null && customActions.Any() )
                {
                    // Only initialize a new collection if we need to, so we
                    // don't wipe out any custom actions the block might have
                    // added itself.
                    if ( definition.CustomActions == null )
                    {
                        definition.CustomActions = new List<CustomActionBag>();
                    }

                    foreach ( var customAction in customActions )
                    {
                        if ( customAction.Route.IsNullOrWhiteSpace() )
                        {
                            continue;
                        }

                        if ( !IsAuthorizedForRoute( block.RequestContext, customAction.Route ) )
                        {
                            continue;
                        }

                        definition.CustomActions.Add( new CustomActionBag
                        {
                            Description = customAction.HelpText,
                            IconCssClass = customAction.IconCssClass,
                            Name = customAction.Name,
                            Route = customAction.Route
                        } );
                    }
                }
            } );
        }

        /// <summary>
        /// Adds the default grid columns that are defined in the block settings.
        /// </summary>
        /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
        /// <param name="builder">The <see cref="GridBuilder{T}"/> to add the field to.</param>
        /// <param name="block">The block that is displaying this grid.</param>
        /// <param name="lavaAccessor">The function that will be used to access the object sent to lava.</param>
        private static void AddCustomGridColumns<T>( GridBuilder<T> builder, IRockBlockType block, Func<T, object> lavaAccessor )
        {
            var customizedGrid = block.GetType().GetCustomAttribute<CustomizedGridAttribute>();

            if ( customizedGrid == null || !customizedGrid.IsCustomColumnsSupported )
            {
                return;
            }

            var additionalColumns = block.BlockCache.GetAttributeValue( CustomGridColumnsConfig.AttributeKey ).FromJsonOrNull<CustomGridColumnsConfig>();

            if ( additionalColumns == null || additionalColumns.ColumnsConfig.Count == 0 )
            {
                return;
            }

            for ( int i = 0; i < additionalColumns.ColumnsConfig.Count; i++ )
            {
                var column = additionalColumns.ColumnsConfig[i];

                builder.AddTextField( $"core.customColumn_${i}", row =>
                {
                    if ( lavaAccessor != null )
                    {
                        return GetCustomColumnText( lavaAccessor( row ), column.LavaTemplate, block.RequestContext );
                    }
                    else
                    {
                        return GetCustomColumnText( row, column.LavaTemplate, block.RequestContext );
                    }
                } );
            }

            builder.AddDefinitionAction( definition =>
            {
                definition.CustomColumns = additionalColumns.ColumnsConfig
                    .Select( ( cc, index ) => new CustomColumnDefinitionBag
                    {
                        HeaderText = cc.HeaderText,
                        HeaderClass = cc.HeaderClass,
                        ItemClass = cc.ItemClass,
                        FieldName = $"core.customColumn_${index}",
                        Anchor = cc.PositionOffsetType == CustomGridColumnsConfig.ColumnConfig.OffsetType.FirstColumn
                            ? Enums.Core.Grid.ColumnPositionAnchor.FirstColumn
                            : Enums.Core.Grid.ColumnPositionAnchor.LastColumn,
                        PositionOffset = cc.PositionOffset
                    } )
                    .ToList();
            } );
        }

        /// <summary>
        /// Gets the custom column text.
        /// </summary>
        /// <param name="row">The row to use as the merge field.</param>
        /// <param name="template">The lava template.</param>
        /// <param name="requestContext">The request context.</param>
        /// <returns>A string that contains the resolved text.</returns>
        private static string GetCustomColumnText( object row, string template, RockRequestContext requestContext )
        {
            var mergeFields = requestContext.GetCommonMergeFields();

            mergeFields.AddOrReplace( "Row", row );

            var text = template.ResolveMergeFields( mergeFields );

            // Resolve any dynamic url references.
            var appRoot = requestContext.ResolveRockUrl( "~/" );
            var themeRoot = requestContext.ResolveRockUrl( "~~/" );

            text = text.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            return text;
        }

        /// <summary>
        /// Gets the route to use when sending a new communication.
        /// </summary>
        /// <param name="block">The block instance building the grid.</param>
        /// <returns>A string that contains the route to use or <c>null</c>.</returns>
        private static string GetCommunicationRoute( IRockBlockType block )
        {
            SiteCache site;

            if ( block.BlockCache.Page != null )
            {
                site = SiteCache.Get( block.BlockCache.Page.SiteId );
            }
            else if ( block.BlockCache.Layout != null )
            {
                site = block.BlockCache.Layout.Site;
            }
            else
            {
                site = block.BlockCache.Site;
            }

            if ( site != null )
            {
                var pageRef = site.CommunicationPageReference;

                if ( pageRef.PageId > 0 )
                {
                    var communicationPage = PageCache.Get( pageRef.PageId );

                    if ( communicationPage.IsAuthorized( Security.Authorization.VIEW, block.RequestContext.CurrentPerson ) )
                    {
                        pageRef.Parameters.AddOrReplace( "CommunicationId", "{CommunicationId}" );
                        return pageRef.BuildUrl();
                    }
                }
            }
            else if ( IsAuthorizedForRoute( block.RequestContext, "/Communication/{CommunicationId}" ) )
            {
                return "/Communication/{CommunicationId}";
            }

            return null;
        }

        /// <summary>
        /// Resolves the rock URL and includes the original scheme and domain
        /// from the request.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="url">The URL to ben resolved.</param>
        /// <returns>A new string resolved to the proper domain.</returns>
        private static string ResolveRockUrlIncludeRoot( RockRequestContext context, string url )
        {
            var virtualPath = context.ResolveRockUrl( url );

            if ( context.RootUrlPath.IsNotNullOrWhiteSpace() )
            {
                return $"{context.RootUrlPath}{virtualPath}";
            }

            return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) + virtualPath.RemoveLeadingForwardslash();
        }

        /// <summary>
        /// Determines whether the person making the request has access to
        /// the page identified by the route.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="route">The route to be checked.</param>
        /// <returns><c>true</c> if the route was found and the requesting person is authorized; otherwise, <c>false</c>.</returns>
        private static bool IsAuthorizedForRoute( RockRequestContext context, string route )
        {
            try
            {
                // Replace any parameters in the route with fake values.
                route = new Regex( "{[^}]+}" ).Replace( route, "1" );

                // Resolve the route based on the current request.
                route = ResolveRockUrlIncludeRoot( context, route );

                // Try to parse the URL, if we can't then assume they can't
                // access the page.
                if ( !Uri.TryCreate( route, UriKind.Absolute, out var uri ) )
                {
                    return false;
                }

                // Find a page ref based on the uri.
                var pageRef = new Rock.Web.PageReference( uri, "/" );

                if ( pageRef.IsValid )
                {
                    // If a valid pageref was found, check the security of the page
                    var page = PageCache.Get( pageRef.PageId );

                    if ( page != null )
                    {
                        return page.IsAuthorized( Rock.Security.Authorization.VIEW, context.CurrentPerson );
                    }
                }
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( ex );
                // Log and move on...
            }

            return false;
        }
    }
}
