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
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Provides helper methods and logic for working with entities in agent
    /// requests. This includes retrieving entities, updating properties,
    /// and managing bulk error responses.
    /// </summary>
    internal class AgentEntityHelper
    {
        #region Fields

        /// <summary>
        /// The database context to use for reading and writing to the database.
        /// </summary>
        private readonly RockContext _rockContext;

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
        /// Creates a new instance of the <see cref="AgentEntityHelper"/> class.
        /// </summary>
        /// <param name="rockContext">The database context to use for reading and writing to the database.</param>
        /// <param name="agentRequestContext">The context of the current agent request.</param>
        /// <param name="logger">The logger to use for logging errors and information.</param>
        public AgentEntityHelper( RockContext rockContext, AgentRequestContext agentRequestContext, ILogger logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _agentRequestContext = agentRequestContext ?? throw new ArgumentNullException( nameof( agentRequestContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Result Methods

        /// <summary>
        /// Gets the <see cref="RockToolResult"/> that contains all the errors and
        /// additional information encountered during processing. If no errors
        /// have been encountered then an exception will be thrown.
        /// </summary>
        /// <returns>A new instance of <see cref="RockToolResult"/>.</returns>
        public RockToolResult GetErrorResult()
        {
            if ( _errors.Count == 0 )
            {
                throw new Exception( "Unexpected call to GetErrorResult with no errors." );
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

            var service = Rock.Reflection.GetServiceForEntityType( typeof( TEntity ), _rockContext ) as Service<TEntity>
                ?? throw new Exception( $"Entity type ${typeof( TEntity ).FullName} does not have a support Service class." );

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
        public TEntity GetOptionalEntity<TEntity>( string parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
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
        public bool TryGetOptionalEntity<TEntity>( string parameter, out TEntity entity, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
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
        public TEntity GetRequiredEntity<TEntity>( string parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
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
        public bool TryGetRequiredEntity<TEntity>( string parameter, out TEntity entity, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
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
        public void SetAttributeValues( IHasAttributes entity, List<AttributeValueResult> attributeValues )
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

                    if ( !isInternal && !entity.Attributes[kvp.Key].IsPublic )
                    {
                        _errors.Add( $"The attribute '{kvp.Key}' is not available." );
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
                if ( !entity.Attributes[key].IsRequired )
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
        /// <returns></returns>
        public ICollection<AttributeResult> GetAvailableAttributes( IHasAttributes entity )
        {
            if ( entity == null || entity.Attributes == null )
            {
                return Array.Empty<AttributeResult>();
            }

            var isInternal = _agentRequestContext.AudienceType == AudienceType.Internal;

            return entity.Attributes.Values
                .Where( a => isInternal || a.IsPublic )
                .Select( a =>
                {
                    var attr = new AttributeResult
                    {
                        Key = a.Key,
                        Name = a.Name,
                        IsRequired = a.IsRequired,
                    };

                    return attr;
                } )
                .ToList();
        }

        #endregion

        #region Entity Update Methods

        /// <summary>
        /// Updates the specified property of an entity with a new value or
        /// clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, SetOrClear<TProperty?> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
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

            var propertyName = ExtractPropertyName( propertyExpression );
            var property = entity.GetType().GetProperty( propertyName )
                ?? throw new Exception( $"Property {propertyName} is not valid." );

            try
            {
                if ( parameter.ClearValue )
                {
                    property.SetValue( entity, null );
                }
                else
                {
                    property.SetValue( entity, parameter.Value );
                }
            }
            catch
            {
                AddError( $"The value of {parameterExpression} is not valid." );
            }
        }

        /// <summary>
        /// Updates the specified property of an entity with a new value or
        /// clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, SetOrClear<TProperty> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
            where TProperty : struct
        {
            UpdateProperty( entity, propertyExpression, new SetOrClear<TProperty?> { Value = parameter.Value, ClearValue = parameter.ClearValue }, parameterExpression );
        }

        /// <summary>
        /// Updates the specified property of an entity with a new value. If
        /// <paramref name="parameter"/> is <c>null</c> then no action will be
        /// taken. Any errors will be added to the error list automatically.
        /// Existing values are never cleared by this method.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, TProperty? parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
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

            UpdateProperty( entity, propertyExpression, new SetOrClear<TProperty?> { Value = parameter }, parameterExpression );
        }

        /// <summary>
        /// Updates the specified property of an entity with a new value. If
        /// <paramref name="parameter"/> is <c>null</c> then no action will be
        /// taken. Any errors will be added to the error list automatically.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <typeparam name="TProperty">The type of property in <paramref name="propertyExpression"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, TProperty? parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
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

            var propertyName = ExtractPropertyName( propertyExpression );
            var property = entity.GetType().GetProperty( propertyName )
                ?? throw new Exception( $"Property {propertyName} is not valid." );

            try
            {
                property.SetValue( entity, parameter.Value );
            }
            catch
            {
                AddError( $"The value of {parameterExpression} is not valid." );
            }
        }

        /// <summary>
        /// Updates the specified property of an entity with a new value or
        /// clears the existing value. If <paramref name="parameter"/> is
        /// <c>null</c> then no action will be taken. Any errors will be added
        /// to the error list automatically.
        /// </summary>
        /// <typeparam name="TEntity">The type of <see cref="IEntity"/> to be updated.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/> to be updated.</param>
        /// <param name="propertyExpression">The expression that identifies which property to update, such as <c>p =&gt; p.FirstName</c>.</param>
        /// <param name="parameter">The parameter that contains the value to be set or indicates the existing value should be cleared.</param>
        /// <param name="parameterExpression">The expression that describes what was passed to <paramref name="parameter"/>, this is used when generating error messages.</param>
        public void UpdateProperty<TEntity>( TEntity entity, Expression<Func<TEntity, string>> propertyExpression, SetOrClear<string> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
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

            var propertyName = ExtractPropertyName( propertyExpression );
            var property = entity.GetType().GetProperty( propertyName )
                ?? throw new Exception( $"Property {propertyName} is not valid." );

            try
            {
                if ( parameter.ClearValue )
                {
                    property.SetValue( entity, null );
                }
                else
                {
                    property.SetValue( entity, parameter.Value );
                }
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

            var navigationPropertyName = ExtractPropertyName( propertyExpression );
            var navigationProperty = entity.GetType().GetProperty( navigationPropertyName );
            var navigationIdProperty = entity.GetType().GetProperty( $"{navigationPropertyName}Id" );

            if ( navigationProperty == null || navigationIdProperty == null )
            {
                throw new Exception( $"Navigation property {navigationPropertyName} is not valid." );
            }

            if ( navigationIdProperty.PropertyType != typeof( int ) && navigationIdProperty.PropertyType != typeof( int? ) )
            {
                throw new Exception( $"Navigation Id property {navigationPropertyName}Id is not valid." );
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

            var navigationPropertyName = ExtractPropertyName( propertyExpression );
            var navigationProperty = entity.GetType().GetProperty( navigationPropertyName );
            var navigationIdProperty = entity.GetType().GetProperty( $"{navigationPropertyName}Id" );

            if ( navigationProperty == null || navigationIdProperty == null )
            {
                throw new Exception( $"Defined value property {navigationPropertyName} is not valid." );
            }

            if ( navigationIdProperty.PropertyType != typeof( int ) && navigationIdProperty.PropertyType != typeof( int? ) )
            {
                throw new Exception( $"Defined value Id property {navigationPropertyName}Id is not valid." );
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
        /// <returns>The name of the property accessed by the expression.</returns>
        private static string ExtractPropertyName( LambdaExpression propertyExpression )
        {
            // Extract the property name from the expression
            var memberExpression = propertyExpression.Body as MemberExpression;

            // If the property is a value type, it will be boxed, so handle UnaryExpression
            if ( memberExpression == null && propertyExpression.Body is UnaryExpression unaryExpression )
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if ( memberExpression == null )
            {
                throw new ArgumentException( "Expression must be a property accessor.", nameof( propertyExpression ) );
            }

            return memberExpression.Member.Name;
        }

        #endregion

        #region Save Methods

        /// <summary>
        /// Saves all changes made in this helper's <see cref="RockContext"/>.
        /// Additionally, any entities that had their attributes set via
        /// <see cref="SetAttributeValues(IHasAttributes, List{AttributeValueResult})"/>
        /// will have their attribute values saved as well. Any exceptions will
        /// be logged and automatically added to the error list.
        /// </summary>
        public void SaveChanges()
        {
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
        /// <see cref="SetAttributeValues(IHasAttributes, List{AttributeValueResult})"/>
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
            if ( HasErrors )
            {
                return;
            }

            SaveChanges();
        }

        #endregion
    }
}
