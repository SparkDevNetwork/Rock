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
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

using DotLiquid;

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
    /// <seealso cref="DotLiquid.Block" />
    public class RockEntity : RockLavaBlockBase
    {
        string _entityName = string.Empty;
        string _markup = string.Empty;

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _entityName = tagName;
            _markup = markup;
            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <exception cref="System.Exception">Your Lava command must contain at least one valid filter. If you configured a filter it's possible that the property or attribute you provided does not exist.</exception>
        public override void Render( Context context, TextWriter result )
        {
            // first ensure that entity commands are allowed in the context
            if ( ! this.IsAuthorized(context) )
            {
                result.Write( string.Format( RockLavaBlockBase.NotAuthorizedMessage, this.Name ) );
                base.Render( context, result );
                return;
            }

            bool hasFilter = false;

            // get a service for the entity based off it's friendly name
            var entityTypes = EntityTypeCache.All();

            var model = string.Empty;

            if (_entityName == "business" )
            {
                model = "Rock.Model.Person";
            }
            else
            {
                model = "Rock.Model." + _entityName;
            }

            // Check first to see if this is a core model
            var entityTypeCache = entityTypes.Where( e => String.Equals( e.Name, model, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();

            // If not, look for first plug-in model that has same friendly name
            if ( entityTypeCache == null )
            {
                entityTypeCache = entityTypes
                    .Where( e =>
                        e.IsEntity &&
                        !e.Name.StartsWith( "Rock.Model" ) &&
                        e.FriendlyName != null &&
                        e.FriendlyName.RemoveSpaces().ToLower() == _entityName )
                    .OrderBy( e => e.Id )
                    .FirstOrDefault();
            }

            // If still null check to see if this was a duplicate class and full class name was used as entity name
            if ( entityTypeCache == null )
            {
                model = _entityName.Replace( '_', '.' );
                entityTypeCache = entityTypes.Where( e => String.Equals( e.Name, model, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
            }

            if ( entityTypeCache != null )
            {
                Type entityType = entityTypeCache.GetEntityType();
                if ( entityType != null )
                {
                    // Get the database context
                    var dbContext = Reflection.GetDbContextForEntityType( entityType );

                    // create an instance of the entity's service
                    Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );

                    ParameterExpression paramExpression = Expression.Parameter( entityType, "x" );
                    Expression queryExpression = null; // the base expression we'll use to build our query from

                    // parse markup
                    var parms = ParseMarkup( _markup, context );

                    if ( parms.Any( p => p.Key == "id" ) )
                    {
                        string propertyName = "Id";

                        List<string> selectionParms = new List<string>();
                        selectionParms.Add( PropertyComparisonConversion( "==" ).ToString() );
                        selectionParms.Add( parms["id"].ToString() );
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

                    // make the query from the expression
                    MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( Rock.Web.UI.Controls.SortProperty ), typeof( int? ) } );
                    if ( getMethod != null )
                    {
                        var queryResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, queryExpression, null, null } ) as IQueryable<IEntity>;

                        // process entity specific filters
                        switch ( _entityName )
                        {
                            case "person":
                                {
                                    queryResult = PersonFilters( (IQueryable<Person>)queryResult, parms );
                                    break;
                                }
                            case "business":
                                {
                                    queryResult = BusinessFilters( (IQueryable<Person>)queryResult, parms );
                                    break;
                                }
                        }

                        // if there was a dynamic expression add it now
                        if ( parms.Any( p => p.Key == "expression" ) )
                        {
                            queryResult = queryResult.Where( parms["expression"] );
                            hasFilter = true;
                        }

                        // get a listing of ids
                        if ( parms.Any( p => p.Key == "ids" ) )
                        {
                            var value = parms["ids"].ToString().Split( ',' ).Select( int.Parse ).ToList();
                            queryResult = queryResult.Where( x => value.Contains(x.Id ) );
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
                                    // sorting on an attribute

                                    // get attribute id
                                    int? attributeId = null;
                                    foreach ( var id in AttributeCache.GetByEntity( entityTypeCache.Id ).SelectMany( a => a.AttributeIds ) )
                                    {
                                        var attribute = AttributeCache.Get( id );
                                        if ( attribute.Key == propertyName )
                                        {
                                            attributeId = id;
                                            break;
                                        }
                                    }

                                    if ( attributeId.HasValue )
                                    {
                                        // get AttributeValue queryable and parameter
                                        if ( dbContext is RockContext)
                                        {
                                            var attributeValues = new AttributeValueService( dbContext as RockContext ).Queryable();
                                            ParameterExpression attributeValueParameter = Expression.Parameter( typeof( AttributeValue ), "v" );
                                            MemberExpression idExpression = Expression.Property( paramExpression, "Id" );
                                            var attributeExpression = Attribute.Helper.GetAttributeValueExpression( attributeValues, attributeValueParameter, idExpression, attributeId.Value );

                                            LambdaExpression sortSelector = Expression.Lambda( attributeExpression, paramExpression );
                                            queryResultExpression = Expression.Call( typeof( Queryable ), methodName, new Type[] { queryResult.ElementType, sortSelector.ReturnType }, queryResultExpression, sortSelector );
                                        }
                                        else
                                        {
                                            throw new Exception( string.Format( "The database context for type {0} does not support RockContext attribute value queries.", entityTypeCache.FriendlyName ) );
                                        }
                                    }
                                }

                                orderByMethod = "ThenBy";
                            }
                        }

                        // reassemble the queryable with the sort expressions
                        queryResult = queryResult.Provider.CreateQuery( queryResultExpression ) as IQueryable<IEntity>;

                        if ( parms.GetValueOrNull( "count" ).AsBoolean() )
                        {
                            int countResult = queryResult.Count();
                            context.Scopes.Last()["count"] = countResult;
                        }
                        else
                        {
                            // run security check on each result
                            var items = queryResult.ToList();
                            var itemsSecured = new List<IEntity>();

                            Person person = GetCurrentPerson( context );

                            foreach ( IEntity item in items )
                            {
                                ISecured itemSecured = item as ISecured;
                                if ( itemSecured == null || itemSecured.IsAuthorized( Authorization.VIEW, person ) )
                                {
                                    itemsSecured.Add( item );
                                }
                            }

                            queryResult = itemsSecured.AsQueryable();

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

                            // check to ensure we had some form of filter (otherwise we'll return all results in the table)
                            if ( !hasFilter )
                            {
                                throw new Exception( "Your Lava command must contain at least one valid filter. If you configured a filter it's possible that the property or attribute you provided does not exist." );
                            }

                            var resultList = queryResult.ToList();

                            // if there is only one item to return set an alternative non-array based variable
                            if ( resultList.Count == 1 )
                            {
                                context.Scopes.Last()[_entityName] = resultList.FirstOrDefault();
                            }

                            context.Scopes.Last()[parms["iterator"]] = resultList;
                        }
                    }
                }
            }
            else
            {
                result.Write( string.Format( "Could not find a model for {0}.", _entityName ) );
                base.Render( context, result );
            }

            base.Render( context, result );
        }

        #region Entity Specific Filters

        /// <summary>
        /// Special filters for the person entity.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        private IQueryable<IEntity> PersonFilters(IQueryable<Person> query, Dictionary<string,string> parms)
        {
            // limit to record type of person
            var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id;

            query = query.Where( p => p.RecordTypeValueId == personRecordTypeId );

            // filter out deceased records unless they specifically want them
            var includeDeceased = false;

            if (parms.Any( p => p.Key == "includedeceased" ) )
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
        public override void OnStartup()
        {
            RegisterEntityCommands();
        }

        /// <summary>
        /// Helper method to register the entity commands.
        /// </summary>
        public static void RegisterEntityCommands()
        {
            var entityTypes = EntityTypeCache.All();

            // register a business entity
            Template.RegisterTag<Rock.Lava.Blocks.RockEntity>( "business" );

            // Register the core models first
            foreach ( var entityType in entityTypes
                .Where( e =>
                    e.IsEntity &&
                    e.Name.StartsWith( "Rock.Model" ) &&
                    e.FriendlyName != null &&
                    e.FriendlyName != "" ) )
            {
                RegisterEntityCommand( entityType );
            }

            // Now register plugin models
            foreach ( var entityType in entityTypes
                .Where( e =>
                    e.IsEntity &&
                    !e.Name.StartsWith( "Rock.Model" ) &&
                    e.FriendlyName != null &&
                    e.FriendlyName != "" )
                .OrderBy( e => e.Id ) )
            {
                RegisterEntityCommand( entityType );
            }

        }

        private static void RegisterEntityCommand( EntityTypeCache entityType )
        {
            if ( entityType != null )
            {
                string entityName = entityType.FriendlyName.RemoveSpaces().ToLower();

                // if entity name is already registered, use the full class name with namespace
                Type tagType = Template.GetTagType( entityName );
                if ( tagType != null )
                {
                    entityName = entityType.Name.Replace( '.', '_' );
                }

                Template.RegisterTag<Rock.Lava.Blocks.RockEntity>( entityName );
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( DotLiquid.Context context )
        {
            Person currentPerson = null;

            // First check for a person override value included in lava context
            if ( context.Scopes != null )
            {
                foreach ( var scopeHash in context.Scopes )
                {
                    if ( scopeHash.ContainsKey( "CurrentPerson" ) )
                    {
                        currentPerson = scopeHash["CurrentPerson"] as Person;
                    }
                }
            }

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

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup, Context context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = new Dictionary<string, object>();

            // get variables defined in the lava source
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            // get merge fields loaded by the block or container
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }
            var resolvedMarkup = markup.ResolveMergeFields( internalMergeFields );

            var parms = new Dictionary<string, string>();
            parms.Add( "iterator", string.Format( "{0}Items", _entityName ) );

            var markupItems = Regex.Matches( resolvedMarkup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            if ( markupItems.Count == 0 )
            {
                throw new Exception( "No parameters were found in your command. The syntax for a parameter is parmName:'' (note that you must use single quotes)." );
            }

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }

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


            return parms;
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
                    List<string> errorMessages = new List<string>();
                    var whereExpression = dataViewSource.GetExpression( service, parmExpression, out errorMessages );
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
                string regexPattern = @"([a-zA-Z]+)|(==|<=|>=|<|!=|\^=|\*=|\*!|_=|_!|>|\$=|#=)|("".*""|\d+)";
                var expressionParts = Regex.Matches( component, regexPattern )
               .Cast<Match>()
               .Select( m => m.Value )
               .ToList();

                if ( expressionParts.Count == 3 )
                {
                    var property = expressionParts[0];
                    var operatorType = expressionParts[1];
                    var value = expressionParts[2].Replace( "\"", "" );

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

                        // We would really love to further qualify this beyond the EntityType by including the
                        // EntityTypeQualifier and EntityTypeQualifierValue but we can't easily do that so, we
                        // will do that "Just in case..." code below (because this actually happened in our Spark
                        // environment.
                        foreach ( var id in AttributeCache.GetByEntity( entityTypeCache.Id ).SelectMany( a => a.AttributeIds ) )
                        {
                            var attribute = AttributeCache.Get( id );

                            // Just in case this EntityType has multiple attributes with the same key, create a OR'd clause for each attribute that has this key
                            // NOTE: this could easily happen if doing an entity command against a DefinedValue, and the same attribute key is used in more than one defined type
                            if ( attribute.Key == property )
                            {
                                filterAttribute = attribute;
                                var attributeEntityField = EntityHelper.GetEntityFieldForAttribute( filterAttribute );

                                if ( attributeWhereExpression == null )
                                {
                                    attributeWhereExpression = ExpressionHelper.GetAttributeExpression( service, parmExpression, attributeEntityField, selectionParms );
                                }
                                else
                                {
                                    attributeWhereExpression = Expression.OrElse( attributeWhereExpression, ExpressionHelper.GetAttributeExpression( service, parmExpression, attributeEntityField, selectionParms ) );
                                }
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
                    // error in parsing expression
                    throw new Exception( "Error in Where expression" );
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
    }
}
