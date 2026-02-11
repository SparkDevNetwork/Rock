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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Provides helper methods and logic for working with entities in agent
    /// requests. This includes retrieving entities, updating properties,
    /// and managing bulk error responses.
    /// </summary>
    internal class AgentToolHelper
    {
        #region Constants

        /// <summary>
        /// The default page size to use when paginating results.
        /// </summary>
        private const int DefaultPageSize = 25;

        /// <summary>
        /// The maximum number of attempts that will be made to fill a page of
        /// results with cursor pagination. This is used to prevent an infinite
        /// loop in cases where there are no items the person has access to.
        /// </summary>
        private const int MaxCursorFillAttempts = 20;

        #endregion

        #region Fields

        /// <summary>
        /// The database context to use for reading and writing to the database.
        /// </summary>
        private readonly RockContext _rockContext;

        /// <summary>
        /// Indicates that the <see cref="_rockContext"/> is read-only and
        /// should not be used to save changes. This is set when the context
        /// comes from the <see cref="AgentRequestContext"/>.
        /// </summary>
        private readonly bool _isContextReadOnly;

        /// <summary>
        /// The context of the current agent request.
        /// </summary>
        private readonly AgentRequestContext _agentRequestContext;

        /// <summary>
        /// The logger to use for logging errors and information.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// A list of errors encountered during processing.
        /// </summary>
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// A list of instructions that will be included in the result.
        /// </summary>
        private readonly List<string> _instructions = new List<string>();

        /// <summary>
        /// A list of metadata key/value pairs to include in the result.
        /// </summary>
        private readonly List<KeyValuePair<string, object>> _metadata = new List<KeyValuePair<string, object>>();

        /// <summary>
        /// A list of entities that have had their attributes modified and need
        /// to be saved.
        /// </summary>
        private readonly List<IHasAttributes> _entitiesWithAttributesToSave = new List<IHasAttributes>();

        #endregion

        #region Properties

        /// <summary>
        /// Determines if this helper has encountered any errors while performing
        /// tasks.
        /// </summary>
        public bool HasErrors => _errors.Count > 0;

        /// <summary>
        /// Gets the <see cref="RockToolResult"/> that contains all the errors
        /// encountered during processing. Any information will also be
        /// included. This will throw an exception if no errors have been
        /// encountered.
        /// </summary>
        public RockToolResult ErrorResult => GetErrorResult();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="AgentToolHelper"/> class.
        /// </summary>
        /// <param name="rockContext">The database context to use for reading and writing to the database.</param>
        /// <param name="agentRequestContext">The context of the current agent request.</param>
        /// <param name="logger">The logger to use for logging errors and information.</param>
        public AgentToolHelper( RockContext rockContext, AgentRequestContext agentRequestContext, ILogger logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _agentRequestContext = agentRequestContext ?? throw new ArgumentNullException( nameof( agentRequestContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AgentToolHelper"/> class.
        /// Do not use this constructor if you plan to make changes to the
        /// database as the provided <see cref="RockContext"/> is considered
        /// read-only. Any calls to <see cref="SaveChanges()"/> will
        /// throw exceptions.
        /// </summary>
        /// <param name="agentRequestContext">The context of the current agent request.</param>
        /// <param name="logger">The logger to use for logging errors and information.</param>
        public AgentToolHelper( AgentRequestContext agentRequestContext, ILogger logger )
        {
            _agentRequestContext = agentRequestContext ?? throw new ArgumentNullException( nameof( agentRequestContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
            _rockContext = agentRequestContext.RockContext;
            _isContextReadOnly = true;
        }

        #endregion

        #region Result Methods

        /// <summary>
        /// Gets the <see cref="RockToolResult"/> that contains all the errors and
        /// additional information encountered during processing. If no errors
        /// have been encountered then an exception will be thrown.
        /// </summary>
        /// <returns>A new instance of <see cref="RockToolResult"/>.</returns>
        private RockToolResult GetErrorResult()
        {
            if ( _errors.Count == 0 )
            {
                throw new InvalidOperationException( "Unexpected call to GetErrorResult with no errors." );
            }

            var result = RockToolResult.Error( _errors );

            foreach ( var instruction in _instructions )
            {
                result.WithInstructions( instruction );
            }

            foreach ( var kvp in _metadata )
            {
                result.WithMetadata( kvp.Key, kvp.Value );
            }

            return result;
        }

        /// <summary>
        /// <para>
        /// Handles the common logic of constructing a paginated result from a
        /// set of paged items.
        /// </para>
        /// <para>
        /// Any errors that have been reported until now are ignored, so you must
        /// check for errors yourself before calling. This method will also
        /// include any instructions and metadata that have been added to the
        /// helper.
        /// </para>
        /// <para>
        /// The returned <see cref="RockToolResult"/> object will be configured
        /// to not have any history content.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of object to be paginated.</typeparam>
        /// <param name="pagedItems">The items to be included in the results.</param>
        /// <param name="sanitizeForSecurity">If <c>true</c> and <typeparamref name="T"/> is of type <see cref="EntityResultBase"/>, then each item will be sanitized by calling <see cref="EntityResultBase.Sanitize(AgentRequestContext)"/>.</param>
        /// <returns>A <see cref="RockToolResult"/> that contains the result data and any standard metadata.</returns>
        public RockToolResult GetPaginatedResult<T>( IReadOnlyCollection<T> pagedItems, bool sanitizeForSecurity = true )
        {
            RockToolResult result;

            if ( sanitizeForSecurity == true && typeof( EntityResultBase ).IsAssignableFrom( typeof( T ) ) )
            {
                foreach ( var item in pagedItems.Cast<EntityResultBase>() )
                {
                    item.Sanitize( _agentRequestContext );
                }
            }

            if ( !pagedItems.Any() )
            {
                result = RockToolResult.NoData();
            }
            else
            {
                result = RockToolResult.Success( pagedItems );
            }

            foreach ( var instruction in _instructions )
            {
                result.WithInstructions( instruction );
            }

            foreach ( var kvp in _metadata )
            {
                result.WithMetadata( kvp.Key, kvp.Value );
            }

            return result.WithoutHistoryContent();
        }

        #endregion

        #region Reporting Methods

        /// <summary>
        /// Adds a new error message to the list of errors. This can be used
        /// while performing custom processing to accumulate errors instead of
        /// returning a failure at the first error encountered.
        /// </summary>
        /// <param name="error">The text that describes the error.</param>
        public void AddError( string error )
        {
            _errors.Add( error );
        }

        /// <summary>
        /// Adds a new line of instructional text that will be included in the
        /// error result. This should be used to provide guidance to the agent
        /// on how to resolve the error.
        /// </summary>
        /// <param name="instruction">The text that describes how to proceed.</param>
        public void AddInstructions( string instruction )
        {
            _instructions.Add( instruction );
        }

        /// <summary>
        /// Adds a metadata entry with the specified key and value. These
        /// will be included in the error result returned to the agent.
        /// </summary>
        /// <param name="key">The key that identifies the metadata entry.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        public void AddMetadata( string key, object value )
        {
            _metadata.Add( new KeyValuePair<string, object>( key, value ) );
        }

        #endregion

        #region Pagination Methods

        /// <summary>
        /// Gets the items that make up the requested page for the specified
        /// query. Metadata describing the pagination details will be added to
        /// the helper. This method should be used when querying entities that
        /// need to be filtered based on the current person's permissions.
        /// </summary>
        /// <typeparam name="T">The type of object to be paginated.</typeparam>
        /// <param name="queryable">The queryable that represents the data to be paginated from the database.</param>
        /// <param name="paginator">The cursor paginator instance that will handle the core pagination logic.</param>
        /// <param name="cursor">The cursor that indicates the start of the page to retrieve or <c>null</c> to retrieve the first page.</param>
        /// <param name="pageSize">The size of each page. If <c>null</c> then a default page size will be applied.</param>
        /// <returns>A collection of items for the specified page.</returns>
        public IList<T> GetCursorPaginatedItems<T>( IQueryable<T> queryable, CursorPaginator<T> paginator, string cursor = null, int? pageSize = null )
            where T : class, IEntity
        {
            pageSize = pageSize ?? DefaultPageSize;

            var page = paginator.GetNextPage( queryable, cursor, pageSize.Value, true );

            AddMetadata( "nextCursor", page.NextCursor );
            AddMetadata( "pageSize", pageSize );
            AddMetadata( "returnedItemCount", page.Items.Count );
            AddMetadata( "hasMore", page.HasMore );

            return page.Items;
        }

        /// <summary>
        /// Gets the items that make up the requested page for the specified
        /// query. Metadata describing the pagination details will be added to
        /// the helper.
        /// </summary>
        /// <typeparam name="T">The type of object to be paginated.</typeparam>
        /// <param name="queryable">The queryable that represents the data to be paginated from the database.</param>
        /// <param name="pageNumber">The page number that was requested.</param>
        /// <param name="pageSize">The size of each page. If <c>null</c> then a default page size will be applied.</param>
        /// <returns>A collection of items for the specified page.</returns>
        public IList<T> GetPaginatedItems<T>( IQueryable<T> queryable, int pageNumber, int? pageSize = null )
        {
            pageSize = pageSize ?? DefaultPageSize;

            var pagedItems = queryable
                .Skip( ( pageNumber - 1 ) * pageSize.Value )
                // N+1 so we can compute hasMore later.
                .Take( pageSize.Value + 1 )
                .ToList();

            var hasMore = pagedItems.Count > pageSize;

            // Drop the lookahead row if we have it.
            while ( pagedItems.Count > pageSize )
            {
                pagedItems.RemoveAt( pagedItems.Count - 1 );
            }

            AddMetadata( "pageNumber", pageNumber );
            AddMetadata( "pageSize", pageSize );
            AddMetadata( "returnedItemCount", pagedItems.Count );
            AddMetadata( "hasMore", hasMore );

            return pagedItems;
        }

        /// <summary>
        /// Gets the items that make up the requested page for the specified
        /// set of items. Metadata describing the pagination details will be
        /// added to the helper.
        /// </summary>
        /// <typeparam name="T">The type of object to be paginated.</typeparam>
        /// <param name="items">The in-memory items to be paginated.</param>
        /// <param name="pageNumber">The page number that was requested.</param>
        /// <param name="pageSize">The size of each page. If <c>null</c> then a default page size will be applied.</param>
        /// <returns>A collection of items for the specified page.</returns>
        public IList<T> GetPaginatedItems<T>( IEnumerable<T> items, int pageNumber, int? pageSize = null )
        {
            return GetPaginatedItems( items.AsQueryable(), pageNumber, pageSize );
        }

        #endregion

        #region Entity Accessor Methods

        /// <summary>
        /// Attempts to get a single entity from the database. Any errors will
        /// automatically be added to the error list.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be loaded.</typeparam>
        /// <param name="parameter">The parameter that contains the encoded IdKey value.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <param name="isRequired"><c>true</c> if this entity is required. If it is not found or not provided then an error will be reported.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person.</param>
        /// <returns>An instance of <typeparamref name="TEntity"/> or <c>null</c> if an error occurred or it was not found.</returns>
        private TEntity GetEntity<TEntity>( string parameter, string parameterExpression, bool isRequired, bool checkSecurity )
            where TEntity : class, IEntity, new()
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter.IsNullOrWhiteSpace() )
            {
                if ( isRequired )
                {
                    _errors.Add( $"You must provide a {parameterExpression}." );
                }

                return null;
            }

            var service = ( Service<TEntity> ) Rock.Reflection.GetServiceForEntityType( typeof( TEntity ), _rockContext );

            var entity = service.Get( parameter, allowIntegerIdentifier: false );

            // If we got this far and the entity is null, then it wasn't found.
            // This is considered an error even if not required since they told
            // us to use something specific that doesn't exist.
            if ( entity == null )
            {
                _errors.Add( $"The {parameterExpression} is not valid." );

                return null;
            }

            // Perform the security check if requested and applicable.
            if ( checkSecurity && entity is ISecured securedEntity )
            {
                if ( !securedEntity.IsAuthorized( Authorization.VIEW, _agentRequestContext.RockRequestContext.CurrentPerson ) )
                {
                    _errors.Add( $"The {parameterExpression} is not valid." );

                    return null;
                }
            }

            // If this is a person, ensure they have a primary alias.
            if ( entity is Person person && !person.PrimaryAliasId.HasValue )
            {
                _errors.Add( $"The {parameterExpression} is not valid." );

                return null;
            }

            return entity;
        }

        /// <summary>
        /// Attempts to get a single entity from the database. Any errors will
        /// automatically be added to the error list. The entity is considered
        /// optional so no error will be generated if <paramref name="parameter"/>
        /// is not provided. If it is provided but not found then an error will
        /// be recorded.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be loaded.</typeparam>
        /// <param name="parameter">The parameter that contains the encoded IdKey value.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>An instance of <typeparamref name="TEntity"/> or <c>null</c> if an error occurred or it was not found.</returns>
        public TEntity GetOptionalEntity<TEntity>( string parameter, bool checkSecurity = true, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            return GetEntity<TEntity>( parameter, parameterExpression, isRequired: false, checkSecurity: checkSecurity );
        }

        /// <summary>
        /// Attempts to get a single entity from the database. Any errors will
        /// automatically be added to the error list. The entity is considered
        /// optional so no error will be generated if <paramref name="parameter"/>
        /// is not provided. If it is provided but not found then an error will
        /// be recorded.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be loaded.</typeparam>
        /// <param name="parameter">The parameter that contains the encoded IdKey value.</param>
        /// <param name="entity">On exit this will contain an instance of <typeparamref name="TEntity"/> or <c>null</c> if an error occurred or it was not found.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns><c>true</c> if an entity was found and loaded into <paramref name="entity"/>; otherwise <c>false</c> if an error occurred or <paramref name="parameter"/> was not specified.</returns>
        public bool TryGetOptionalEntity<TEntity>( string parameter, out TEntity entity, bool checkSecurity = true, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            entity = GetEntity<TEntity>( parameter, parameterExpression, isRequired: false, checkSecurity: checkSecurity );

            return entity != null;
        }

        /// <summary>
        /// Attempts to get a single entity from the database. Any errors will
        /// automatically be added to the error list. The entity is considered
        /// required and will generate an error if <paramref name="parameter"/>
        /// is not provided. If it is provided but not found then an error will
        /// be recorded.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be loaded.</typeparam>
        /// <param name="parameter">The parameter that contains the encoded IdKey value.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>An instance of <typeparamref name="TEntity"/> or <c>null</c> if an error occurred or it was not found.</returns>
        public TEntity GetRequiredEntity<TEntity>( string parameter, bool checkSecurity = true, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            return GetEntity<TEntity>( parameter, parameterExpression, isRequired: true, checkSecurity: checkSecurity );
        }

        /// <summary>
        /// Attempts to get a single entity from the database. Any errors will
        /// automatically be added to the error list. The entity is considered
        /// required and will generate an error if <paramref name="parameter"/>
        /// is not provided. If it is provided but not found then an error will
        /// be recorded.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be loaded.</typeparam>
        /// <param name="parameter">The parameter that contains the encoded IdKey value.</param>
        /// <param name="entity">On exit this will contain an instance of <typeparamref name="TEntity"/> or <c>null</c> if an error occurred or it was not found.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns><c>true</c> if an entity was found and loaded into <paramref name="entity"/>; otherwise <c>false</c> if an error occurred or <paramref name="parameter"/> was not specified.</returns>
        public bool TryGetRequiredEntity<TEntity>( string parameter, out TEntity entity, bool checkSecurity = true, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            entity = GetEntity<TEntity>( parameter, parameterExpression, isRequired: true, checkSecurity: checkSecurity );

            return entity != null;
        }

        #endregion

        #region Attribute Methods

        /// <summary>
        /// <para>
        /// Sets the attribute values of <paramref name="entity"/> based on
        /// the provides set. If any errors are encountered they will be added
        /// to the error list automatically. Attemping to erase a required
        /// attribute will generate an error.
        /// </para>
        /// <para>
        /// Any entities that have had their attributes modified by this method
        /// will be tracked. When <see cref="SaveChanges"/> is called each of
        /// these entities will have their attribute values saved as well.
        /// </para>
        /// <para>
        /// It is <strong>not</strong> recommended to call this method multiple
        /// times with different entities as it may result in the list of
        /// available attributes being overwritten. If you do need to call this
        /// with multiple entities, then you should check for errors after each
        /// call and return the error result if any errors were encountered.
        /// </para>
        /// </summary>
        /// <param name="entity">The entity whose attributes are to be set.</param>
        /// <param name="attributeValues">The values to be set.</param>
        /// <param name="enforceSecurity">Determines if security should be enforced or not when setting values.</param>
        public void SetAttributeValues( IHasAttributes entity, List<AttributeValueResult> attributeValues, bool enforceSecurity = true )
        {
            if ( entity == null )
            {
                return;
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( _rockContext );
            }

            var isInternal = _agentRequestContext.AudienceType == AudienceType.Internal;
            var previousErrorCount = _errors.Count;
            var hasChanged = false;

            // Try to set all provided attribute values.
            if ( attributeValues != null )
            {
                foreach ( var kvp in attributeValues )
                {
                    if ( !entity.Attributes.ContainsKey( kvp.Key ) )
                    {
                        _errors.Add( $"The attribute '{kvp.Key}' does not exist." );
                        continue;
                    }

                    var attribute = entity.Attributes[kvp.Key];

                    if ( !isInternal && !attribute.IsPublic )
                    {
                        _errors.Add( $"The attribute '{kvp.Key}' is not available." );
                        continue;
                    }

                    if ( enforceSecurity && !attribute.IsAuthorized( Authorization.EDIT, _agentRequestContext.RockRequestContext.CurrentPerson ) )
                    {
                        AddError( $"You do not have permission to edit the attribute '{kvp.Key}'." );
                        continue;
                    }

                    var value = kvp.Value ?? string.Empty;

                    // Only update the attribute if the value has changed. This
                    // saves us from later saving the attribute values if they
                    // never actually changed.
                    if ( entity.GetAttributeValue( kvp.Key ) != value )
                    {
                        entity.SetAttributeValue( kvp.Key, value );
                        hasChanged = true;
                    }
                }
            }

            if ( hasChanged )
            {
                _entitiesWithAttributesToSave.Add( entity );
            }

            // Check for any attribute values that are blank yet required.
            foreach ( var key in entity.Attributes.Keys )
            {
                var attribute = entity.Attributes[key];

                if ( !attribute.IsRequired )
                {
                    continue;
                }

                if ( enforceSecurity && !attribute.IsAuthorized( Authorization.EDIT, _agentRequestContext.RockRequestContext.CurrentPerson ) )
                {
                    continue;
                }

                if ( entity.GetAttributeValue( key ).IsNullOrWhiteSpace() )
                {
                    _errors.Add( $"The attribute '{key}' is required and cannot be empty." );
                }
            }

            // If we had any new errors, then add available attributes metadata.
            if ( _errors.Count != previousErrorCount )
            {
                AddInstructions( $"Check the list of availableAttributes to see what attributes are available." );
                AddMetadata( "availableAttributes", GetAvailableAttributes( entity ) );
            }
        }

        /// <summary>
        /// Gets the attributes that are available on the entity.
        /// </summary>
        /// <param name="entity">The entity whose attributes are to be retrieved.</param>
        /// <param name="enforceSecurity">Determines if security should be enforced or not when getting attributes.</param>
        /// <returns></returns>
        public ICollection<AttributeResult> GetAvailableAttributes( IHasAttributes entity, bool enforceSecurity = true )
        {
            if ( entity == null )
            {
                return Array.Empty<AttributeResult>();
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( _rockContext );
            }

            var isInternal = _agentRequestContext.AudienceType == AudienceType.Internal;

            return entity.Attributes.Values
                .Where( a => isInternal || a.IsPublic )
                .Where( a => !enforceSecurity || a.IsAuthorized( Authorization.VIEW, _agentRequestContext.RockRequestContext.CurrentPerson ) )
                .Select( a =>
                {
                    var attr = new AttributeResult
                    {
                        Key = a.Key,
                        Name = a.Name,
                        IsRequired = a.IsRequired,
                        IsReadOnly = enforceSecurity && !a.IsAuthorized( Authorization.EDIT, _agentRequestContext.RockRequestContext.CurrentPerson ),
                    };

                    if ( a.FieldType?.Field is Field.FieldType fieldType )
                    {
                        var hints = fieldType.GetFieldHints( a.ConfigurationValues );

                        attr.ValueFormat = hints.ValueFormat.ToStringOrDefault( null );
                    }

                    return attr;
                } )
                .ToList();
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Updates the specified property of an instance with a new value or
        /// clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </summary>
        /// <typeparam name="TInstance">The type of object to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="instance">The instance to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TInstance, TProperty>( TInstance instance, Expression<Func<TInstance, TProperty?>> propertyExpression, SetOrClear<TProperty?> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TProperty : struct
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter == null )
            {
                return;
            }

            var property = ExtractProperty( propertyExpression );

            try
            {
                if ( parameter.ClearValue )
                {
                    property.SetValue( instance, null );
                }
                else
                {
                    property.SetValue( instance, parameter.Value );
                }
            }
            catch
            {
                AddError( $"The value of {parameterExpression} is not valid." );
            }
        }

        /// <summary>
        /// Updates the specified property of an instance with a new value or
        /// clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </summary>
        /// <typeparam name="TInstance">The type of object to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="instance">The instance to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TInstance, TProperty>( TInstance instance, Expression<Func<TInstance, TProperty?>> propertyExpression, SetOrClear<TProperty> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TProperty : struct
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter == null )
            {
                return;
            }

            UpdateProperty( instance, propertyExpression, new SetOrClear<TProperty?> { Value = parameter.Value, ClearValue = parameter.ClearValue }, parameterExpression );
        }

        /// <summary>
        /// Updates the specified property of an instance with a new value. If
        /// <paramref name="parameter"/> is <c>null</c> then no action will be
        /// taken. Any errors will be added to the error list automatically.
        /// Existing values are never cleared by this method.
        /// </summary>
        /// <typeparam name="TInstance">The type of object to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="instance">The instance to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TInstance, TProperty>( TInstance instance, Expression<Func<TInstance, TProperty?>> propertyExpression, TProperty? parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TProperty : struct
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter == null )
            {
                return;
            }

            UpdateProperty( instance, propertyExpression, new SetOrClear<TProperty> { Value = parameter.Value }, parameterExpression );
        }

        /// <summary>
        /// Updates the specified property of an instance with a new value. If
        /// <paramref name="parameter"/> is <c>null</c> then no action will be
        /// taken. Any errors will be added to the error list automatically.
        /// </summary>
        /// <typeparam name="TInstance">The type of object to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="instance">The instance to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TInstance, TProperty>( TInstance instance, Expression<Func<TInstance, TProperty>> propertyExpression, TProperty? parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TProperty : struct
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter == null )
            {
                return;
            }

            var property = ExtractProperty( propertyExpression );

            try
            {
                property.SetValue( instance, parameter.Value );
            }
            catch
            {
                AddError( $"The value of {parameterExpression} is not valid." );
            }
        }

        /// <summary>
        /// Updates the specified property of an instance with a new value or
        /// clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </summary>
        /// <typeparam name="TInstance">The type of object to be updated.</typeparam>
        /// <param name="instance">The instance to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TInstance>( TInstance instance, Expression<Func<TInstance, string>> propertyExpression, SetOrClear<string> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter == null )
            {
                return;
            }

            var property = ExtractProperty( propertyExpression );

            try
            {
                if ( parameter.ClearValue )
                {
                    property.SetValue( instance, null );
                }
                else
                {
                    property.SetValue( instance, parameter.Value );
                }
            }
            catch
            {
                AddError( $"The value of {parameterExpression} is not valid." );
            }
        }

        /// <summary>
        /// Updates the specified property of an instance with a new value or
        /// clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> or whitespace then no action will be taken. Any errors
        /// will be added to the error list automatically.
        /// </summary>
        /// <typeparam name="TInstance">The type of object to be updated.</typeparam>
        /// <param name="instance">The instance to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TInstance>( TInstance instance, Expression<Func<TInstance, string>> propertyExpression, string parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter.IsNullOrWhiteSpace() )
            {
                return;
            }

            var property = ExtractProperty( propertyExpression );

            try
            {
                property.SetValue( instance, parameter );
            }
            catch
            {
                AddError( $"The value of {parameterExpression} is not valid." );
            }
        }

        /// <summary>
        /// <para>
        /// Updates the specified navigation property of an entity with a new
        /// value or clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </para>
        /// <para>
        /// This will update both the navigation property and the foreign key
        /// property.
        /// </para>
        /// <para>
        /// If the foreign key property is non-nullable then attempting to
        /// clear the value will generate an error.
        /// </para>
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.CreatedByPersonAlias</c>. This should always point to the navigation property.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person when loading the target entity.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateNavigationProperty<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, SetOrClear<string> parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
            where TProperty : class, IEntity, new()
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter == null )
            {
                return;
            }

            var navigationProperty = ExtractProperty( propertyExpression );
            var navigationIdProperty = entity.GetType().GetProperty( $"{navigationProperty.Name}Id" )
                ?? throw new Exception( $"Navigation property {navigationProperty.Name} is not valid." );

            if ( navigationIdProperty.PropertyType != typeof( int ) && navigationIdProperty.PropertyType != typeof( int? ) )
            {
                throw new Exception( $"Navigation Id property {navigationProperty.Name}Id is not valid." );
            }

            if ( parameter.ClearValue )
            {
                if ( navigationIdProperty.PropertyType == typeof( int ) )
                {
                    AddError( $"{parameterExpression} is required and can't be cleared." );
                    return;
                }

                navigationProperty.SetValue( entity, null );
                navigationIdProperty.SetValue( entity, null );
            }
            else if ( parameter.Value.IsNotNullOrWhiteSpace() )
            {
                if ( typeof( TProperty ) == typeof( PersonAlias ) )
                {
                    // We expect the AI to always pass us a Person identifier
                    // instead of a PersonAlias, so we need to translate.
                    if ( !TryGetRequiredEntity<Model.Person>( parameter.Value, out var target, checkSecurity: checkSecurity, parameterExpression: parameterExpression ) )
                    {
                        return;
                    }

                    navigationProperty.SetValue( entity, target.PrimaryAlias );
                    navigationIdProperty.SetValue( entity, target.PrimaryAliasId );
                }
                else
                {
                    if ( !TryGetRequiredEntity<TProperty>( parameter.Value, out var target, checkSecurity: checkSecurity, parameterExpression: parameterExpression ) )
                    {
                        return;
                    }

                    navigationProperty.SetValue( entity, target );
                    navigationIdProperty.SetValue( entity, target.Id );
                }
            }
        }

        /// <summary>
        /// <para>
        /// Updates the specified navigation property of an entity with a new
        /// value. If <paramref name="parameter"/> is <c>null</c> then no action
        /// will be taken. The current value will never be cleared. Any errors
        /// will be added to the error list automatically.
        /// </para>
        /// <para>
        /// This will update both the navigation property and the foreign key
        /// property.
        /// </para>
        /// <para>
        /// If the foreign key property is non-nullable then attempting to
        /// clear the value will generate an error.
        /// </para>
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.CreatedByPersonAlias</c>. This should always point to the navigation property.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person when loading the target entity.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateNavigationProperty<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, string parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
            where TProperty : class, IEntity, new()
        {
            UpdateNavigationProperty( entity, propertyExpression, new SetOrClear<string> { Value = parameter }, checkSecurity: checkSecurity, parameterExpression: parameterExpression );
        }

        /// <summary>
        /// <para>
        /// Updates the specified navigation property of an entity with a new
        /// value or clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </para>
        /// <para>
        /// This will update both the navigation property and the foreign key
        /// property. This has special handling for properties that are used as
        /// <see cref="DefinedValue"/> references. The property should be
        /// decorated with the <see cref="DefinedValueAttribute"/> to indicate
        /// which defined type is valid for the property. Attempting to set a
        /// value that does not belong to that defined type will generate an
        /// error.
        /// </para>
        /// <para>
        /// If the foreign key property is non-nullable then attempting to
        /// clear the value will generate an error.
        /// </para>
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.RecordSourceValue</c>. This should always point to the navigation property.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person when loading the target entity.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateDefinedValueProperty<TEntity>( TEntity entity, Expression<Func<TEntity, DefinedValue>> propertyExpression, SetOrClear<string> parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter == null )
            {
                return;
            }

            var navigationProperty = ExtractProperty( propertyExpression );
            var navigationIdProperty = entity.GetType().GetProperty( $"{navigationProperty.Name}Id" )
                ?? throw new Exception( $"Defined value property {navigationProperty.Name} is not valid." );

            if ( navigationIdProperty.PropertyType != typeof( int ) && navigationIdProperty.PropertyType != typeof( int? ) )
            {
                throw new Exception( $"Defined value Id property {navigationProperty.Name}Id is not valid." );
            }

            if ( parameter.ClearValue )
            {
                if ( navigationIdProperty.PropertyType == typeof( int ) )
                {
                    AddError( $"{parameterExpression} is required and can't be cleared." );
                    return;
                }

                navigationProperty.SetValue( entity, null );
                navigationIdProperty.SetValue( entity, null );
            }
            else if ( parameter.Value.IsNotNullOrWhiteSpace() )
            {
                if ( !TryGetRequiredEntity<DefinedValue>( parameter.Value, out var target, checkSecurity: checkSecurity, parameterExpression: parameterExpression ) )
                {
                    return;
                }

                var definedValueAttribute = navigationIdProperty.GetCustomAttribute<DefinedValueAttribute>();

                if ( definedValueAttribute != null && definedValueAttribute.DefinedTypeGuid.HasValue )
                {
                    var definedTypeCache = DefinedTypeCache.Get( definedValueAttribute.DefinedTypeGuid.Value, _rockContext );

                    if ( target.DefinedTypeId != definedTypeCache.Id )
                    {
                        AddError( $"The value of {parameterExpression} is not valid." );
                        return;
                    }
                }

                navigationProperty.SetValue( entity, target );
                navigationIdProperty.SetValue( entity, target.Id );
            }
        }

        /// <summary>
        /// <para>
        /// Updates the specified navigation property of an entity with a new
        /// value. An existing value will never be cleared by this method. If
        /// <paramref name="parameter"/> is <c>null</c> then no action will be
        /// taken. Any errors will be added to the error list automatically.
        /// </para>
        /// <para>
        /// This will update both the navigation property and the foreign key
        /// property. This has special handling for properties that are used as
        /// <see cref="DefinedValue"/> references. The property should be
        /// decorated with the <see cref="DefinedValueAttribute"/> to indicate
        /// which defined type is valid for the property. Attempting to set a
        /// value that does not belong to that defined type will generate an
        /// error.
        /// </para>
        /// <para>
        /// If the foreign key property is non-nullable then attempting to
        /// clear the value will generate an error.
        /// </para>
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.RecordSourceValue</c>. This should always point to the navigation property.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="checkSecurity"><c>true</c> if <see cref="Authorization.VIEW"/> access should be checked for the current person when loading the target entity.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateDefinedValueProperty<TEntity>( TEntity entity, Expression<Func<TEntity, DefinedValue>> propertyExpression, string parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
        {
            UpdateDefinedValueProperty( entity, propertyExpression, new SetOrClear<string> { Value = parameter }, checkSecurity: checkSecurity, parameterExpression: parameterExpression );
        }

        /// <summary>
        /// Extracts the property name from the expression. If the expression is
        /// not valid then an exception will be thrown.
        /// </summary>
        /// <param name="propertyExpression">The property expression containing a property accessor.</param>
        /// <returns>The reflected property accessed by the expression.</returns>
        [ExcludeFromCodeCoverage]
        private static PropertyInfo ExtractProperty( LambdaExpression propertyExpression )
        {
            // Extract the property name from the expression
            var memberExpression = propertyExpression.Body as MemberExpression;

            // If the property is a value type, it will be boxed, so handle UnaryExpression
            if ( memberExpression == null && propertyExpression.Body is UnaryExpression unaryExpression )
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if ( memberExpression?.Member is PropertyInfo property )
            {
                return property;
            }

            throw new ArgumentException( "Expression must be a property accessor.", nameof( propertyExpression ) );
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Handles filtering a queryable by an IdKey parameter. If the parameter
        /// is required then an error will be added if it is not provided. If the
        /// value provided is not valid then an error will be added. Any errors
        /// will cause the queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="isRequired">If <c>true</c> then an empty or missing <paramref name="parameter"/> value will be considered an error.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        private IQueryable<TSource> WhereIdKey<TSource>( IQueryable<TSource> queryable, Expression<Func<TSource, int?>> propertyExpression, string parameter, bool isRequired, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter.IsNullOrWhiteSpace() )
            {
                if ( isRequired )
                {
                    AddError( $"{parameterExpression} is required." );
                    return queryable.Where( a => false );
                }

                return queryable;
            }

            var id = IdHasher.Instance.GetId( parameter );

            if ( !id.HasValue )
            {
                AddError( $"The value of {parameterExpression} is not valid." );

                return queryable.Where( a => false );
            }

            var property = ExtractProperty( propertyExpression );
            var memberExpression = propertyExpression.Body;

            // If the property is a value type, it will be boxed, so handle UnaryExpression
            if ( memberExpression is UnaryExpression unaryExpression )
            {
                memberExpression = unaryExpression.Operand;
            }

            var valueExpression = Expression.Constant( id.Value, property.PropertyType );
            var body = Expression.Equal( memberExpression, valueExpression );
            var lambda = Expression.Lambda<Func<TSource, bool>>( body, propertyExpression.Parameters[0] );

            return queryable.Where( lambda );
        }

        /// <summary>
        /// Handles filtering a queryable by a parameter. If the parameter
        /// is required then an error will be added if it is not provided. Any
        /// errors will cause the queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <typeparam name="TProperty">The type of property being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="isRequired">If <c>true</c> then an empty or missing <paramref name="parameter"/> value will be considered an error.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        private IQueryable<TSource> WhereProperty<TSource, TProperty>( IQueryable<TSource> queryable, Expression<Func<TSource, TProperty?>> propertyExpression, TProperty? parameter, bool isRequired, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TProperty : struct
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( !parameter.HasValue )
            {
                if ( isRequired )
                {
                    AddError( $"{parameterExpression} is required." );
                    return queryable.Where( a => false );
                }

                return queryable;
            }

            var property = ExtractProperty( propertyExpression );
            var memberExpression = propertyExpression.Body;

            // If the property is a value type, it will be boxed, so handle UnaryExpression
            if ( memberExpression is UnaryExpression unaryExpression )
            {
                memberExpression = unaryExpression.Operand;
            }

            var valueExpression = Expression.Constant( parameter.Value, property.PropertyType );
            var body = Expression.Equal( memberExpression, valueExpression );
            var lambda = Expression.Lambda<Func<TSource, bool>>( body, propertyExpression.Parameters[0] );

            return queryable.Where( lambda );
        }

        /// <summary>
        /// Handles filtering a queryable by a parameter. If the parameter
        /// is required then an error will be added if it is not provided. Any
        /// errors will cause the queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="isRequired">If <c>true</c> then an empty or missing <paramref name="parameter"/> value will be considered an error.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        private IQueryable<TSource> WhereProperty<TSource>( IQueryable<TSource> queryable, Expression<Func<TSource, string>> propertyExpression, string parameter, bool isRequired, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            if ( parameterExpression.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( parameterExpression ), "The parameterExpression must be provided. It will be provided automatically if using C# 10, otherwise use 'nameof()' to get the name of the passed parameter." );
            }

            if ( parameter.IsNullOrWhiteSpace() )
            {
                if ( isRequired )
                {
                    AddError( $"{parameterExpression} is required." );
                    return queryable.Where( a => false );
                }

                return queryable;
            }

            var valueExpression = Expression.Constant( parameter, typeof( string ) );
            var body = Expression.Equal( propertyExpression.Body, valueExpression );
            var lambda = Expression.Lambda<Func<TSource, bool>>( body, propertyExpression.Parameters[0] );

            return queryable.Where( lambda );
        }

        /// <summary>
        /// Handles filtering a queryable by an IdKey parameter. If the parameter
        /// is blank then no filtering will be performed. If the value provided
        /// is not valid then an error will be added. Any errors will cause the
        /// queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        public IQueryable<TSource> WhereOptionalIdKey<TSource>( IQueryable<TSource> queryable, Expression<Func<TSource, int?>> propertyExpression, string parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            return WhereIdKey( queryable, propertyExpression, parameter, isRequired: false, parameterExpression: parameterExpression );
        }

        /// <summary>
        /// Handles filtering a queryable by an IdKey parameter. If the parameter
        /// is blank then an error will be added. If the value provided is not
        /// valid then an error will be added. Any errors will cause the
        /// queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        public IQueryable<TSource> WhereRequiredIdKey<TSource>( IQueryable<TSource> queryable, Expression<Func<TSource, int?>> propertyExpression, string parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            return WhereIdKey( queryable, propertyExpression, parameter, isRequired: true, parameterExpression: parameterExpression );
        }

        /// <summary>
        /// Handles filtering a queryable by a parameter. If the parameter
        /// is <c>null</c> then no filtering will be performed. Any errors will
        /// cause the queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <typeparam name="TProperty">The type of property being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        public IQueryable<TSource> WhereOptionalProperty<TSource, TProperty>( IQueryable<TSource> queryable, Expression<Func<TSource, TProperty?>> propertyExpression, TProperty? parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TProperty : struct
        {
            return WhereProperty( queryable, propertyExpression, parameter, isRequired: false, parameterExpression: parameterExpression );
        }

        /// <summary>
        /// Handles filtering a queryable by a parameter. If the parameter
        /// is blank then no filtering will be performed. Any errors will
        /// cause the queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        public IQueryable<TSource> WhereOptionalProperty<TSource>( IQueryable<TSource> queryable, Expression<Func<TSource, string>> propertyExpression, string parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            return WhereProperty( queryable, propertyExpression, parameter, isRequired: false, parameterExpression: parameterExpression );
        }

        /// <summary>
        /// Handles filtering a queryable by a parameter. If the parameter
        /// is <c>null</c> then an error will be added. Any errors will
        /// cause the queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <typeparam name="TProperty">The type of property being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        public IQueryable<TSource> WhereRequiredProperty<TSource, TProperty>( IQueryable<TSource> queryable, Expression<Func<TSource, TProperty?>> propertyExpression, TProperty? parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TProperty : struct
        {
            return WhereProperty( queryable, propertyExpression, parameter, isRequired: true, parameterExpression: parameterExpression );
        }

        /// <summary>
        /// Handles filtering a queryable by a parameter. If the parameter
        /// is empty then an error will be added. Any errors will cause the
        /// queryable to return no results.
        /// </summary>
        /// <typeparam name="TSource">The type of object being queried.</typeparam>
        /// <param name="queryable">The original queryable to chain with an additional where clause.</param>
        /// <param name="propertyExpression">The expression that maps to the property that will be filtered on.</param>
        /// <param name="parameter">The parameter that contains the value to be filtered against.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        /// <returns>A new <see cref="IQueryable{T}"/> that has the additional filter applied.</returns>
        public IQueryable<TSource> WhereRequiredProperty<TSource>( IQueryable<TSource> queryable, Expression<Func<TSource, string>> propertyExpression, string parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
        {
            return WhereProperty( queryable, propertyExpression, parameter, isRequired: true, parameterExpression: parameterExpression );
        }

        #endregion

        #region Save Methods

        /// <summary>
        /// Saves all changes made in this helper's <see cref="RockContext"/>.
        /// Additionally, any entities that had their attributes set via
        /// <see cref="SetAttributeValues(IHasAttributes, List{AttributeValueResult}, bool)"/>
        /// will have their attribute values saved as well. Any exceptions will
        /// be logged and automatically added to the error list.
        /// </summary>
        public void SaveChanges()
        {
            if ( _isContextReadOnly )
            {
                throw new InvalidOperationException( "The RockContext is read-only and changes cannot be saved. Use the constructor that takes a RockContext parameter." );
            }

            try
            {
                _rockContext.WrapTransaction( () =>
                {
                    _rockContext.SaveChanges();

                    foreach ( var entity in _entitiesWithAttributesToSave )
                    {
                        entity.SaveAttributeValues( _rockContext );
                    }
                } );

                _entitiesWithAttributesToSave.Clear();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while saving the changes." );

                AddError( "An error occurred while saving the changes." );
            }

        }

        /// <summary>
        /// <para>
        /// Saves all changes made in this helper's <see cref="RockContext"/>.
        /// Additionally, any entities that had their attributes set via
        /// <see cref="SetAttributeValues(IHasAttributes, List{AttributeValueResult}, bool)"/>
        /// will have their attribute values saved as well. Any exceptions will
        /// be logged and automatically added to the error list.
        /// </para>
        /// <para>
        /// This is a convenience method to help with saving. If there are
        /// already errors in the error list, then no changes will be saved
        /// to the database.
        /// </para>
        /// </summary>
        public void SaveChangesIfNoErrors()
        {
            // Throw early here so the developer knows if they are using
            // the wrong constructor. Otherwise, they might not get an error
            // if there were other errors.
            if ( _isContextReadOnly )
            {
                throw new InvalidOperationException( "The RockContext is read-only and changes cannot be saved. Use the constructor that takes a RockContext parameter." );
            }

            if ( HasErrors )
            {
                return;
            }

            SaveChanges();
        }

        #endregion
    }
}
