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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Reporting.DataFilter;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    ///
    /// </summary>
    public class RockEntityBlock : LavaBlockBase, ILavaSecured
    {
        /// <summary>
        /// The type name of the target entity.
        /// </summary>
        public string EntityName { get; set; }

        string _markup = string.Empty;

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            //_entityName = tagName;
            _markup = markup;
            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <exception cref="System.Exception">Your Lava command must contain at least one valid filter. If you configured a filter it's possible that the property or attribute you provided does not exist.</exception>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // first ensure that entity commands are allowed in the context
            if ( !this.IsAuthorized( context, "rockentity" ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, "rockentity" ) );
                base.OnRender( context, result );
                return;
            }

            bool hasFilter = false;

            var modelName = string.Empty;

            // get a service for the entity based off it's friendly name
            if ( EntityName == "business" )
            {
                modelName = "Rock.Model.Person";
            }
            else
            {
                modelName = "Rock.Model." + EntityName;
            }

            // Check first to see if this is a core model. use the createIfNotFound = false option
            var entityTypeCache = EntityTypeCache.Get( modelName, false );

            if ( entityTypeCache == null )
            {
                var entityTypes = EntityTypeCache.All();

                // If not, look for first plug-in model that has same friendly name
                entityTypeCache = entityTypes
                    .Where( e =>
                        e.IsEntity &&
                        !e.Name.StartsWith( "Rock.Model" ) &&
                        e.FriendlyName != null &&
                        e.FriendlyName.RemoveSpaces().ToLower() == EntityName )
                    .OrderBy( e => e.Id )
                    .FirstOrDefault();

                // If still null check to see if this was a duplicate class and full class name was used as entity name
                if ( entityTypeCache == null )
                {
                    modelName = EntityName.Replace( '_', '.' );
                    entityTypeCache = entityTypes.Where( e => String.Equals( e.Name, modelName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
                }
            }

            if ( entityTypeCache != null )
            {
                Type entityType = entityTypeCache.GetEntityType();
                if ( entityType != null )
                {
                    // Get the appropriate database context for this entity type.
                    // Note that this may be different from the standard RockContext if the entity is sourced from a plug-in.
                    var dbContext = Reflection.GetDbContextForEntityType( entityType );

                    // Check if there is a RockContext in the Lava context, if so (and the entity is using a RockContext) we should use that one
                    if ( dbContext is RockContext )
                    {
                        dbContext = LavaHelper.GetRockContextFromLavaContext( context );
                    }

                    // Disable change-tracking for this data context to improve performance - objects supplied to a Lava context are read-only.
                    dbContext.Configuration.AutoDetectChangesEnabled = false;

                    // Create an instance of the entity's service
                    IService serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );

                    ParameterExpression paramExpression = Expression.Parameter( entityType, "x" );
                    Expression queryExpression = null; // the base expression we'll use to build our query from

                    // Parse markup
                    var settings = GetAttributesFromMarkup( _markup, context, this.EntityName );
                    var parms = settings.Attributes;

                    if ( parms.Any( p => p.Key == "id" ) )
                    {
                        string propertyName = "Id";

                        List<string> selectionParms = new List<string>();
                        selectionParms.Add( PropertyComparisonConversion( "==" ).ToString() );
                        selectionParms.Add( parms["id"].AsInteger().ToString() ); // Ensure this is an integer: https://github.com/SparkDevNetwork/Rock/issues/5230
                        selectionParms.Add( propertyName );

                        var entityProperty = entityType.GetProperty( propertyName );
                        queryExpression = ExpressionHelper.PropertyFilterExpression( selectionParms, paramExpression, propertyName, entityProperty.PropertyType );

                        hasFilter = true;
                    }
                    else
                    {
                        // where clause expression
                        if ( parms.Any( p => p.Key == "where" ) )
                        {
                            queryExpression = ParseWhere( parms["where"], entityType, serviceInstance, paramExpression, entityType, entityTypeCache );

                            if ( queryExpression != null )
                            {
                                hasFilter = true;
                            }
                        }

                        // DataView expression
                        if ( parms.Any( p => p.Key == "dataview" ) )
                        {
                            var dataViewId = parms["dataview"].AsIntegerOrNull();

                            if ( dataViewId.HasValue )
                            {
                                var dataViewExpression = GetDataViewExpression( dataViewId.Value, serviceInstance, paramExpression, entityTypeCache );

                                if ( queryExpression == null )
                                {
                                    queryExpression = dataViewExpression;
                                    hasFilter = true;
                                }
                                else
                                {
                                    queryExpression = Expression.AndAlso( queryExpression, dataViewExpression );
                                }
                            }
                        }

                        // process dynamic filter expressions (from the query string)
                        if ( parms.Any( p => p.Key == "dynamicparameters" ) )
                        {
                            var dynamicFilters = parms["dynamicparameters"].Split( ',' )
                                            .Select( x => x.Trim() )
                                            .Where( x => !string.IsNullOrWhiteSpace( x ) )
                                            .ToList();

                            foreach ( var dynamicFilter in dynamicFilters )
                            {
                                var dynamicFilterValue = HttpContext.Current.Request[dynamicFilter];
                                var dynamicFilterExpression = GetDynamicFilterExpression( dynamicFilter, dynamicFilterValue, entityType, serviceInstance, paramExpression );
                                if ( dynamicFilterExpression != null )
                                {
                                    if ( queryExpression == null )
                                    {
                                        queryExpression = dynamicFilterExpression;
                                        hasFilter = true;
                                    }
                                    else
                                    {
                                        queryExpression = Expression.AndAlso( queryExpression, dynamicFilterExpression );
                                    }
                                }
                            }
                        }
                    }

                    // Make the query from the expression.
                    /* [2020-10-08] DL
                     * "Get" is intentionally used here rather than "GetNoTracking" to allow lazy-loading of navigation properties from the Lava context.
                     * (Refer https://github.com/SparkDevNetwork/Rock/issues/4293)
                     */
                    MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( Rock.Web.UI.Controls.SortProperty ), typeof( int? ) } );

                    if ( getMethod != null )
                    {
                        // get a listing of ids and build it into the query expression
                        if ( parms.Any( p => p.Key == "ids" ) )
                        {
                            List<int> value = parms["ids"].ToString().Split( ',' ).Select( int.Parse ).ToList();
                            MemberExpression propertyExpression = Expression.Property( paramExpression, "Id" );
                            ConstantExpression constantExpression = Expression.Constant( value, typeof( List<int> ) );
                            Expression containsExpression = Expression.Call( constantExpression, typeof( List<int> ).GetMethod( "Contains", new Type[] { typeof( int ) } ), propertyExpression );
                            if ( queryExpression != null )
                            {
                                queryExpression = Expression.AndAlso( queryExpression, containsExpression );
                            }
                            else
                            {
                                queryExpression = containsExpression;
                            }

                            hasFilter = true;
                        }

                        var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, queryExpression, null, null } );
                        var queryResult = getResult as IQueryable<IEntity>;

                        // process entity specific filters
                        switch ( EntityName )
                        {
                            case "person":
                                {
                                    queryResult = PersonFilters( ( IQueryable<Person> ) queryResult, parms );
                                    break;
                                }
                            case "business":
                                {
                                    queryResult = BusinessFilters( ( IQueryable<Person> ) queryResult, parms );
                                    break;
                                }
                        }

                        // if there was a dynamic expression add it now
                        if ( parms.Any( p => p.Key == "expression" ) ) 
                        {
                            queryResult = queryResult.Where( parms["expression"] );
                            hasFilter = true;
                        }

                        var queryResultExpression = queryResult.Expression;

                        // add sort expressions
                        if ( parms.Any( p => p.Key == "sort" ) )
                        {
                            string orderByMethod = "OrderBy";

                            foreach ( var column in parms["sort"].Split( ',' ).Select( x => x.Trim() ).Where( x => !string.IsNullOrWhiteSpace( x ) ).ToList() )
                            {
                                string propertyName;
                                var direction = SortDirection.Ascending;

                                if ( column.EndsWith( " desc", StringComparison.OrdinalIgnoreCase ) )
                                {
                                    direction = SortDirection.Descending;
                                    propertyName = column.Left( column.Length - 5 );
                                }
                                else
                                {
                                    propertyName = column;
                                }

                                string methodName = direction == SortDirection.Descending ? orderByMethod + "Descending" : orderByMethod;

                                if ( entityType.GetProperty( propertyName ) != null )
                                {
                                    // sorting a entity property
                                    var memberExpression = Expression.Property( paramExpression, propertyName );
                                    LambdaExpression sortSelector = Expression.Lambda( memberExpression, paramExpression );
                                    queryResultExpression = Expression.Call( typeof( Queryable ), methodName, new Type[] { queryResult.ElementType, sortSelector.ReturnType }, queryResultExpression, sortSelector );
                                }
                                else
                                {
                                    // Sort by Attribute.
                                    // Get all of the Attributes for this EntityType that have a matching Key and apply the same sort for each of them.
                                    // This situation may occur, for example, where the target entity is a DefinedValue and the same Attribute Key exists for multiple Defined Types.
                                    var attributeIdListForAttributeKey = AttributeCache.GetByEntityType( entityTypeCache.Id )
                                                                .Where( a => a != null && a.Key == propertyName )
                                                                .Select( a => a.Id )
                                                                .ToList();

                                    if ( attributeIdListForAttributeKey.Any() )
                                    {
                                        // get AttributeValue queryable and parameter
                                        var rockContext = dbContext as RockContext;
                                        if ( rockContext == null  )
                                        {
                                            throw new Exception( $"The database context for type {entityTypeCache.FriendlyName} does not support RockContext attribute value queries." );
                                        }

                                        var attributeValues = new AttributeValueService( rockContext ).Queryable();
                                        foreach ( var attributeId in attributeIdListForAttributeKey )
                                        {
                                            methodName = ( direction == SortDirection.Descending ) ? orderByMethod + "Descending" : orderByMethod;

                                            var attributeValueParameter = Expression.Parameter( typeof( AttributeValue ), "v" );
                                            var idExpression = Expression.Property( paramExpression, "Id" );
                                            var attributeExpression = Attribute.Helper.GetAttributeValueExpression( attributeValues, attributeValueParameter, idExpression, attributeId );

                                            var sortSelector = Expression.Lambda( attributeExpression, paramExpression );
                                            queryResultExpression = Expression.Call( typeof( Queryable ), methodName, new Type[] { queryResult.ElementType, sortSelector.ReturnType }, queryResultExpression, sortSelector );

                                            orderByMethod = "ThenBy";
                                        }
                                    }
                                }

                                orderByMethod = "ThenBy";
                            }
                        }

                        // check to ensure we had some form of filter (otherwise we'll return all results in the table)
                        if ( !hasFilter )
                        {
                            throw new Exception( "Your Lava command must contain at least one valid filter. If you configured a filter it's possible that the property or attribute you provided does not exist." );
                        }

                        // Disable lazy loading if requested
                        if ( parms.Any( p => p.Key == "lazyloadenabled" ) )
                        {
                            if ( !parms["lazyloadenabled"].AsBoolean() )
                            {
                                dbContext.Configuration.LazyLoadingEnabled = false;
                            }
                        }

                        // Reassemble the queryable with the sort expressions
                        queryResult = queryResult.Provider.CreateQuery( queryResultExpression ) as IQueryable<IEntity>;

                        // Add included entities
                        if ( parms.Any( p => p.Key == "include" ) )
                        {
                            var includeList = parms["include"].Split( ',' )
                                                .Select( x => x.Trim() )
                                                .Where( x => !string.IsNullOrWhiteSpace( x ) );

                            foreach ( var includeItem in includeList )
                            {
                                queryResult = queryResult.Include( includeItem );
                            }
                        }

                        if ( parms.GetValueOrNull( "count" ).AsBoolean() )
                        {
                            int countResult = queryResult.Count();
                            context.SetMergeField( "count", countResult );
                        }
                        else
                        {
                            // Run security check on each result if enabled and entity is not a person (we do not check security on people)
                            if ( parms["securityenabled"].AsBoolean() && EntityName != "person" )
                            {
                                var items = queryResult.ToList();
                                var itemsSecured = new List<IEntity>();

                                Person person = GetCurrentPerson( context );

                                foreach ( IEntity item in items )
                                {
                                    ISecured itemSecured = item as ISecured;
                                    if ( itemSecured == null || itemSecured.IsAuthorized( Authorization.VIEW, person ) )
                                    {
                                        itemsSecured.Add( item );

                                        /*
	                                        8/13/2020 - JME 
	                                        It might seem logical to break out of the loop if there is limit parameter provided once the
                                            limit is reached. This though has two issues.

                                            FIRST
                                            Depending how it was implemented it can have the effect of breaking when an offset is
                                            provided. 
	                                            {% contentchannelitem where:'ContentChannelId == 1' limit:'3' %}
                                                    {% for item in contentchannelitemItems %}
                                                        {{ item.Id }} - {{ item.Title }}<br>
                                                    {% endfor %}
                                                {% endcontentchannelitem %}
                                            Returns 3 items (correct)

                                                {% contentchannelitem where:'ContentChannelId == 1' limit:'3' offset:'1' %}
                                                    {% for item in contentchannelitemItems %}
                                                        {{ item.Id }} - {{ item.Title }}<br>
                                                    {% endfor %}
                                                {% endcontentchannelitem %}
                                            Returns only 2 items (incorrect) - because of the offset

                                            SECOND
                                            If the limit is moved before the security check it's possible that the security checks
                                            will remove items and will therefore not give you the amount of items that you asked for.

                                            Unfortunately this has to be an inefficent process to ensure pagination works. I will also
                                            add a detailed note to the documentation to encourage people to disable security checks,
                                            especially when used with pagination, in the Lava docs.
                                        */
                                    }
                                }

                                queryResult = itemsSecured.AsQueryable();
                            }

                            // offset
                            if ( parms.Any( p => p.Key == "offset" ) )
                            {
                                queryResult = queryResult.Skip( parms["offset"].AsInteger() );
                            }

                            // limit, default to 1000
                            if ( parms.Any( p => p.Key == "limit" ) )
                            {
                                queryResult = queryResult.Take( parms["limit"].AsInteger() );
                            }
                            else
                            {
                                queryResult = queryResult.Take( 1000 );
                            }

                            // Process logic to be able to return an anonymous types and group bys. We have to abstract the return types as
                            // the select returns a type of List<dynamic> while the normal entity command returns List<IEntity>.
                            // Using a type of List<object> for both did not work (that would have eliminated the need for the
                            // firstItem and returnCount.
                            object returnValues = null;
                            object firstItem = null;
                            int returnCount = 0;

                            if ( parms.ContainsKey( "groupby" ) || parms.ContainsKey( "select" ) || parms.ContainsKey( "selectmany" ) )
                            {
                                /* 
                                   3/1/2021 - JME
                                   Ensure that lazy loading is enabled. If this is false it throws a null reference exception.
                                   I confirmed that the anonymous type is getting it's data from the single source SQL (no lazy loading).
                                   Not sure why this exception is happening. It looks to be within the ZZZ Project System.Linq.Dynamic.Core
                                   package. The important part is that the data is coming back in a single query.
                                */
                                dbContext.Configuration.LazyLoadingEnabled = true;


                                List<dynamic> results = null;

                                // Logic here is a groupby has to have a select, but a select doesn't need a groupby.
                                if ( parms.ContainsKey( "groupby" ) && parms.ContainsKey( "select" ) )
                                {
                                    results = queryResult.Cast( entityType )
                                                    .GroupBy( parms["groupby"] )
                                                    .Select( parms["select"] )
                                                    .ToDynamicList();
                                }
                                else
                                {
                                    if ( parms.ContainsKey( "select" ) )
                                    {
                                        results = queryResult.Cast( entityType )
                                                        .Select( parms["select"] )
                                                        .ToDynamicList();
                                    }
                                    else  // selectmany
                                    {
                                        results = queryResult.Cast( entityType )
                                                        .SelectMany( parms["selectmany"] )
                                                        .ToDynamicList();
                                    }
                                }

                                returnValues = results;
                                firstItem = results.FirstOrDefault();
                                returnCount = results.Count();
                            }
                            else
                            {
                                var results = queryResult.ToList();

                                // Pre-load attributes
                                var disableattributeprefetch = parms.GetValueOrDefault("disableattributeprefetch", "false").AsBoolean();
                                var attributeKeys = parms.GetValueOrDefault("prefetchattributes", string.Empty)
                                                        .Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries )
                                                        .ToList();

                                // Determine if we should prefetch attributes. By default we will unless they specifically say not to.
                                if ( !disableattributeprefetch )
                                {
                                    // If a filtered list of attributes keys are not provided load all attributes otherwise just load the ones for the keys provided.
                                    if ( attributeKeys.Count() == 0 )
                                    {
                                        results.Select( r => r as IHasAttributes ).Where( r => r != null ).ToList().LoadAttributes();
                                    }
                                    else
                                    {
                                        results.Select( r => r as IHasAttributes ).Where( r => r != null ).ToList().LoadFilteredAttributes( (RockContext)dbContext, a => attributeKeys.Contains( a.Key ) );
                                    }
                                }
                                
                                returnValues = results;
                                firstItem = results.FirstOrDefault();
                                returnCount = results.Count();
                            }

                            // Add the result to the current context.
                            context.SetMergeField( parms["iterator"], returnValues );

                            if ( returnCount == 1 )
                            {
                                // If there is only one item, set a singleton variable in addition to the result list.
                                context.SetMergeField( EntityName, firstItem, LavaContextRelativeScopeSpecifier.Current );
                            }
                        }
                    }
                }
            }
            else
            {
                result.Write( string.Format( "Could not find a model for {0}.", EntityName ) );
                base.OnRender( context, result );
            }

            base.OnRender( context, result );
        }

        #region Entity Specific Filters

        /// <summary>
        /// Special filters for the person entity.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        private IQueryable<IEntity> PersonFilters( IQueryable<Person> query, Dictionary<string, string> parms )
        {
            // limit to record type of person
            var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id;

            query = query.Where( p => p.RecordTypeValueId == personRecordTypeId );

            // filter out deceased records unless they specifically want them
            var includeDeceased = false;

            if ( parms.Any( p => p.Key == "includedeceased" ) )
            {
                includeDeceased = parms["includedeceased"].AsBoolean( false );
            }

            if ( !includeDeceased )
            {
                query = query.Where( p => p.IsDeceased == false );
            }

            return query;
        }

        /// <summary>
        /// Special filters for businesses.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        private IQueryable<IEntity> BusinessFilters( IQueryable<Person> query, Dictionary<string, string> parms )
        {
            // limit to record type of business
            var businessRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS ).Id;

            query = query.Where( p => p.RecordTypeValueId == businessRecordTypeId );
            return query;
        }

        #endregion

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup( ILavaEngine engine )
        {
            RegisterEntityCommands( engine );
        }

        /// <summary>
        /// Helper method to register the entity commands.
        /// </summary>
        public static void RegisterEntityCommands( ILavaEngine engine )
        {
            // If the database is not connected, we do not have access to entity definitions.
            // This can occur when the Lava engine is started without an attached database.
            if ( !RockApp.Current.IsDatabaseAvailable() )
            {
                return;
            }

            var entityTypes = EntityTypeCache.All();

            // register a business entity
           engine.RegisterBlock( "business", ( name ) => CreateEntityBlockInstance( name ) );

            // Register the core models, replacing existing blocks of the same name if necessary.
            foreach ( var entityType in entityTypes
                .Where( e =>
                    e.IsEntity &&
                    e.Name.StartsWith( "Rock.Model" ) &&
                    e.FriendlyName != null &&
                    e.FriendlyName != "" ) )
            {
                RegisterEntityCommand( engine, entityType, useQualifiedNameIfExists:true );
            }

            // Register plugin models, using fully-qualified namespace if necessary.
            foreach ( var entityType in entityTypes
                .Where( e =>
                    e.IsEntity &&
                    !e.Name.StartsWith( "Rock.Model" ) &&
                    e.FriendlyName != null &&
                    e.FriendlyName != "" )
                .OrderBy( e => e.Id ) )
            {
                RegisterEntityCommand( engine, entityType, useQualifiedNameIfExists:true );
            }

        }

        private static void RegisterEntityCommand( ILavaEngine engine, EntityTypeCache entityType, bool useQualifiedNameIfExists = false )
        {
            if ( entityType != null )
            {
                string entityName = entityType.FriendlyName.RemoveSpaces().ToLower();

                // if entity name is already registered, use the full class name with namespace
                if ( useQualifiedNameIfExists
                     && engine.GetRegisteredElements().ContainsKey( entityName ) )
                {
                    entityName = entityType.Name.Replace( '.', '_' );
                }

                engine.RegisterBlock( entityName, ( name ) => CreateEntityBlockInstance( name ) );
            }
        }

        /// <summary>
        /// Factory method to return a new block for the specified Entity.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        private static RockEntityBlock CreateEntityBlockInstance( string entityName )
        {
            // Return a block having a tag name corresponding to the entity name.
            return new RockEntityBlock() { SourceElementName = entityName, EntityName = entityName };
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( ILavaRenderContext context )
        {
            Person currentPerson = null;

            // First check for a person override value included in lava context
            currentPerson = context.GetMergeField( "CurrentPerson", null ) as Person;

            if ( currentPerson == null )
            {
                var httpContext = System.Web.HttpContext.Current;
                if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context, string entityName )
        {
            // Create default settings
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            if ( settings.Attributes.Count == 0 )
            {
                throw new Exception( "No parameters were found in your command. The syntax for a parameter is parmName:'' (note that you must use single quotes)." );
            }

            settings.AddOrIgnore( "iterator", string.Format( "{0}Items", entityName ) );
            settings.AddOrIgnore( "securityenabled", "true" );
            settings.AddOrIgnore( "cacheduration", "0" );

            var parms = settings.Attributes;

            // override any dynamic parameters
            List<string> dynamicFilters = new List<string>(); // will be used to process dynamic filters
            if ( parms.ContainsKey( "dynamicparameters" ) )
            {
                var dynamicParms = parms["dynamicparameters"];
                var dynamicParmList = dynamicParms.Split( ',' )
                                        .Select( x => x.Trim() )
                                        .Where( x => !string.IsNullOrWhiteSpace( x ) )
                                        .ToList();

                foreach ( var dynamicParm in dynamicParmList )
                {
                    if ( HttpContext.Current.Request[dynamicParm] != null )
                    {
                        var dynamicParmValue = HttpContext.Current.Request[dynamicParm].ToString();

                        switch ( dynamicParm )
                        {
                            case "id":
                            case "limit":
                            case "offset":
                            case "dataview":
                            case "expression":
                            case "sort":
                            case "iterator":
                            case "checksecurity":
                            case "includedeceased":
                            case "lazyloadenabled":
                            case "include":
                            case "select":
                            case "selectmany":
                            case "groupby":
                            case "securityenabled":
                            case "prefetchattributes":
                            case "disableattributeprefetch":
                                {
                                    parms.AddOrReplace( dynamicParm, dynamicParmValue );
                                    break;
                                }
                            default:
                                {
                                    dynamicFilters.Add( dynamicParm );
                                    break;
                                }
                        }
                    }
                }

                parms.AddOrReplace( "dynamicparameters", string.Join( ",", dynamicFilters ) );
            }

            return settings;
        }

        /// <summary>
        /// Gets the data view expression.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="service">The service.</param>
        /// <param name="parmExpression">The parm expression.</param>
        /// <param name="entityTypeCache">The entity type cache.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private Expression GetDataViewExpression( int dataViewId, IService service, ParameterExpression parmExpression, EntityTypeCache entityTypeCache )
        {
            if ( service.Context is RockContext )
            {
                var dataViewSource = new DataViewService( service.Context as RockContext ).Get( dataViewId );
                bool isCorrectDataType = dataViewSource.EntityTypeId == entityTypeCache.Id;

                if ( isCorrectDataType )
                {
                    var whereExpression = dataViewSource.GetExpression( service, parmExpression );
                    return whereExpression;
                }
                else
                {
                    throw new Exception( string.Format( "The DataView provided is not of type {0}.", entityTypeCache.FriendlyName ) );
                }
            }
            else
            {
                throw new Exception( string.Format( "The database context for type {0} does not support RockContext dataviews.", entityTypeCache.FriendlyName ) );
            }
        }

        /// <summary>
        /// Parses the where.
        /// </summary>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="type">The type.</param>
        /// <param name="service">The service.</param>
        /// <param name="parmExpression">The parm expression.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityTypeCache">The entity type cache.</param>
        /// <returns></returns>
        private Expression ParseWhere( string whereClause, Type type, IService service, ParameterExpression parmExpression, Type entityType, EntityTypeCache entityTypeCache )
        {
            Expression returnExpression = null;

            // find locations of and/or's
            var expressionComponents = Regex.Split( whereClause, @"(\|\||&&)" );

            var currentExpressionComparisonType = ExpressionComparisonType.And;

            foreach ( var component in expressionComponents )
            {
                if ( component == "||" )
                {
                    currentExpressionComparisonType = ExpressionComparisonType.Or;
                    continue;
                }

                if ( component == "&&" )
                {
                    currentExpressionComparisonType = ExpressionComparisonType.And;
                    continue;
                }

                // parse the part to get the expression
                var regexPattern = @"((?!_=|_!)[a-zA-Z0-9_]+)|(==|<=|>=|<|!=|\^=|\*=|\*!|_=|_!|>|\$=|#=)|("".*""|\d+)";
                var expressionParts = Regex.Matches( component, regexPattern )
               .Cast<Match>()
               .Select( m => m.Value )
               .ToList();

                if ( expressionParts.Count == 3 )
                {
                    var property = expressionParts[0];
                    var operatorType = expressionParts[1];
                    var value = expressionParts[2].Replace( "\"", "" );

                    // Check if the property is Id, if so ensure that it's an integer to prevent
                    // returning everything in the database.
                    // https://github.com/SparkDevNetwork/Rock/issues/5236
                    if ( property == "Id" )
                    {
                        value = value.AsInteger().ToString();
                    }

                    List<string> selectionParms = new List<string>();
                    selectionParms.Add( PropertyComparisonConversion( operatorType ).ToString() );
                    selectionParms.Add( value );
                    selectionParms.Add( property );

                    Expression expression = null;

                    if ( entityType.GetProperty( property ) != null )
                    {
                        var entityProperty = entityType.GetProperty( property );
                        expression = ExpressionHelper.PropertyFilterExpression( selectionParms, parmExpression, property, entityProperty.PropertyType );
                    }
                    else
                    {
                        AttributeCache filterAttribute = null;
                        Expression attributeWhereExpression = null;

                        var attributeKey = property;

                        // We would really love to further qualify this beyond the EntityType by including the
                        // EntityTypeQualifier and EntityTypeQualifierValue but we can't easily do that so, we
                        // will do that "Just in case..." code below (because this actually happened in our Spark
                        // environment.
                        // Also, there could be multiple attributes that have the same key (due to attribute qualifiers or just simply a duplicate key)
                        var entityAttributeListForAttributeKey = AttributeCache.GetByEntityType( entityTypeCache.Id )
                            .Where( a => a != null && a.Key == attributeKey ).ToList();

                        // Just in case this EntityType has multiple attributes with the same key, create a OR'd clause for each attribute that has this key
                        // NOTE: this could easily happen if doing an entity command against a DefinedValue, and the same attribute key is used in more than one defined type
                        foreach ( var attribute in entityAttributeListForAttributeKey )
                        {
                            filterAttribute = attribute;

                            var attributeEntityField = EntityHelper.GetEntityFieldForAttribute( filterAttribute, limitToFilterableAttributes:false );

                            Expression filterExpression;
                            if ( attributeEntityField == null )
                            {
                                // There is no Entity field matching this Attribute, so ignore the filter.
                                filterExpression = new NoAttributeFilterExpression();
                            }
                            else
                            {
                                filterExpression = ExpressionHelper.GetAttributeExpression( service, parmExpression, attributeEntityField, selectionParms );
                            }
                            
                            if ( filterExpression is NoAttributeFilterExpression )
                            {
                                // Ignore this filter because it would cause the Where expression to match everything.
                                continue;
                            }

                            if ( attributeWhereExpression == null )
                            {
                                attributeWhereExpression = filterExpression;
                            }
                            else
                            {
                                attributeWhereExpression = Expression.OrElse( attributeWhereExpression, filterExpression );
                            }
                        }

                        if ( attributeWhereExpression != null )
                        {
                            expression = attributeWhereExpression;
                        }
                    }

                    if ( returnExpression == null )
                    {
                        returnExpression = expression;
                    }
                    else
                    {
                        if ( expression == null )
                        {
                            // unable to match to property or attribute, so just return what we got
                            return returnExpression;
                        }

                        if ( currentExpressionComparisonType == ExpressionComparisonType.And )
                        {
                            returnExpression = Expression.AndAlso( returnExpression, expression );
                        }
                        else
                        {
                            returnExpression = Expression.OrElse( returnExpression, expression );
                        }
                    }

                }
                else
                {
                    // The Where clause is incomplete.
                    string errorDetail;
                    if ( expressionParts.Count == 2 )
                    {
                        errorDetail = "Missing or invalid value in Where expression.";
                    }
                    else
                    {
                        errorDetail = "Where expression is incomplete.";
                    }

                    errorDetail = $"{errorDetail} [Expression=\"{whereClause}\"]";
                    throw new Exception( "RockEntity block error. The Where expression is invalid.", new Exception( errorDetail ) );
                }
            }

            return returnExpression;
        }

        /// <summary>
        /// Gets the dynamic filter expression.
        /// </summary>
        /// <param name="dynamicFilter">The dynamic filter.</param>
        /// <param name="dynamicFilterValue">The dynamic filter value.</param>
        /// <param name="type">The type.</param>
        /// <param name="service">The service.</param>
        /// <param name="parmExpression">The parm expression.</param>
        /// <returns></returns>
        private Expression GetDynamicFilterExpression( string dynamicFilter, string dynamicFilterValue, Type type, IService service, ParameterExpression parmExpression )
        {
            if ( !string.IsNullOrWhiteSpace( dynamicFilter ) && !string.IsNullOrWhiteSpace( dynamicFilterValue ) )
            {
                var entityField = EntityHelper.FindFromFilterSelection( type, dynamicFilter );
                if ( entityField != null )
                {
                    var values = new List<string>();
                    string comparison = entityField.FieldType.Field.GetEqualToCompareValue();
                    if ( !string.IsNullOrWhiteSpace( comparison ) )
                    {
                        values.Add( comparison );
                    }
                    values.Add( dynamicFilterValue );

                    if ( entityField.FieldKind == FieldKind.Property )
                    {
                        return new PropertyFilter().GetPropertyExpression( service, parmExpression, entityField, values );
                    }
                    else
                    {
                        return new PropertyFilter().GetAttributeExpression( service, parmExpression, entityField, values );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Properties the comparison conversion.
        /// </summary>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns></returns>
        private int PropertyComparisonConversion( string comparisonOperator )
        {
            switch ( comparisonOperator )
            {
                case "==":
                    {
                        return 1;
                    }
                case "!=":
                    {
                        return 2;
                    }
                case "^=": // starts with
                    {
                        return 4;
                    }
                case "*=": // contains
                    {
                        return 8;
                    }
                case "*!": // does not contain
                    {
                        return 16;
                    }
                case "_=": // is blank
                    {
                        return 32;
                    }
                case "_!": // is not blank
                    {
                        return 64;
                    }
                case ">":
                    {
                        return 128;
                    }
                case ">=":
                    {
                        return 256;
                    }
                case "<":
                    {
                        return 512;
                    }
                case "<=":
                    {
                        return 1024;
                    }
                case "$=": // ends with
                    {
                        return 2048;
                    }
                case "#=": // regex
                    {
                        return 8192;
                    }
            }

            return -1;
        }

        /// <summary>
        /// An enum to specify and vs or comparisons
        /// </summary>
        enum ExpressionComparisonType { And, Or };

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "RockEntity";
            }
        }

        #endregion
    }
}
