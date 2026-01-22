using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

// DISCUSSION:
//
// 1. AI would occasionally call the Add tool with extra arguments we don't handle in Add, but do in Update.
//    It would then think that value was set so it would never call update.
//    I added logic to the tool calling to check for unknown arguments and return an error.
//    This works well, but we might need to make this an opt in/out thing, for example Person Search it might be fairly common to get unknown arguments that we can safely ignore.
//
// 2. Turned indented output off when writing JSON responses to AI.
//    This has drastically reduced token counts, but there might have been a reason Microsoft specifically turned it on (it's normally off by default).
//
// 3. Our current attribute results might need additional data.
//    I had to create my own AttributeLookupResult to use when listing available attributes, it has things like Description and ValueFormat.
//    Without the ValueFormat, it didn't understand that an Integer field type called Priority could not accept "High" as a value. Now it translate "High" to a number.
//    But a scenario where I say "update Ted Decker's latest connection request to a high priority", it might query the existing request, see the current value (without that ValueFormat hint) and change the value to "high".
//
// 4. On that note, the ValueFormat has been working very well. I hard coded my Priority attribute to say "An integer between 0 and 10." and it can now translate words (like medium or high) to useful numbers.
//
// 5. Recommend having a per-entity tool to get available attributes.
//    In my testing, with the instructions I have in place, the AI only calls this tool when it needs to.
//    Meaning, when my request has data that it can't already perfectly map to the function arguments.
//    This causes the tool to be called first, but greatly reduced times it would map that stuff to 'comments'.
//
// 6. Recommend we don't include the available attributes after an Add or Update call.
//    UNLESS they provided an invalid attribute, or did not provide a required value.
//    From looking at the tool call flows, this is pointless data that is already has from item 5.
//
// 7. Look into changing "Attributes" everywhere to "AttributeValues" when it is holding actual values.
//    Probably add to system prompt information about the distincintion between the two.
//
// 8. Create documentation in Rockumentation book for the pattern of:
//    a. Creating an Add/Update tool.
//    b. Outline of how the pattern interacts with the LLM (tool call orders, etc.).
//    c. Outline of all tools required/recommended for "entity access".
//

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ConnectionSkill
    {
        #region Tool(s)

        [Description( "Gets the available attributes that can be set when adding or updating a connection request." )]
        [AgentPurpose( "This must be called when adding or updating connection requests if there is any extra data can not be directly mapped to a top-level parameter. Attributes must be used before comments." )]
        [AgentToolGuid( "c660989a-ba62-42f8-8eed-49c0bf7e8bf6" )]
        public RockToolResult GetConnectionRequestAvailableAttributes(
            string connectionRequestIdKey = null,
            string connectionOpportunityIdKey = null )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();
            var helper = new AddUpdateHelper( rockContext, AgentRequestContext );

            if ( connectionRequestIdKey.IsNotNullOrWhiteSpace() )
            {
                var connectionRequest = helper.GetRequiredEntity<ConnectionRequest>( connectionRequestIdKey, checkSecurity: true );

                if ( connectionRequest == null )
                {
                    return helper.ErrorResult;
                }

                connectionRequest.LoadAttributes( rockContext );

                return RockToolResult.Success( helper.GetAvailableAttributes( connectionRequest ) );
            }
            else if ( connectionOpportunityIdKey.IsNotNullOrWhiteSpace() )
            {
                var opportunity = helper.GetRequiredEntity<ConnectionOpportunity>( connectionOpportunityIdKey, checkSecurity: true );

                if ( opportunity == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( "Call the LookupConnectionTypesAndOpportunities function to determine available opportunities." );
                }

                var connectionRequest = new ConnectionRequest
                {
                    ConnectionOpportunityId = opportunity.Id,
                    ConnectionTypeId = opportunity.ConnectionTypeId,
                };

                connectionRequest.LoadAttributes( rockContext );

                return RockToolResult.Success( helper.GetAvailableAttributes( connectionRequest ) )
                    .WithInstructions( "Attributes are additional data that can be provided to the Add and Update functions. An attribute value is a key that specifies which attribute and a value which contains the text of the attribute value. Attributes that are required must be provided when adding connection requests, but may be optional when updating." );
            }
            else
            {
                return RockToolResult.Error( "Either requestIdKey or opportunityIdKey must be specified." );
            }
        }

        [Description( "Adds a connection request." )]
        [AgentToolGuid( "a300c848-7dd8-4cdf-ac12-1e6d73f22667" )]
        public RockToolResult AddConnectionRequest(
            [Description( "The required IdKey of the person the connection request is for." )]
            string personIdKey = null,

            [Description( "The required IdKey of the connection opportunity." )]
            string connectionOpportunityIdKey = null,

            ConnectionState? connectionState = null,
            string connectionStatusIdKey = null,
            string comments = null,
            string placementGroupIdKey = null,
            List<AttributeKeyAndValue> attributes = null )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();
            var helper = new AddUpdateHelper( rockContext, AgentRequestContext );

            // Get all the values we need to create the connection request.
            var person = helper.GetRequiredEntity<Model.Person>( personIdKey );
            var placementGroup = helper.GetOptionalEntity<Model.Group>( placementGroupIdKey );

            if ( !helper.TryGetRequiredEntity<ConnectionOpportunity>( connectionOpportunityIdKey, out var opportunity ) )
            {
                helper.AddInstructions( "Call the LookupConnectionTypesAndOpportunities function to determine available opportunities." );
            }

            var status = GetConnectionStatus( helper, connectionStatusIdKey, opportunity );

            // If we had any errors, then return an error result.
            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            // Create the new connection request.
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionRequest = rockContext.Set<ConnectionRequest>().Create();

            connectionRequest.PersonAliasId = person.PrimaryAliasId.Value;
            connectionRequest.ConnectionOpportunityId = opportunity.Id;
            connectionRequest.ConnectionState = connectionState ?? ConnectionState.Active;
            connectionRequest.ConnectionStatusId = status.Id;
            connectionRequest.Comments = comments;
            connectionRequest.AssignedGroupId = placementGroup?.Id;

            helper.TrySetAttributeValues( connectionRequest, attributes );

            // Check again for any errors.
            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            connectionRequestService.Add( connectionRequest );

            try
            {
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    if ( attributes != null )
                    {
                        connectionRequest.SaveAttributeValues( rockContext );
                    }
                } );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while saving a new connection request." );

                return RockToolResult.Error( "An error occurred while saving the connection request." );
            }

            var toolResult = RockToolResult
                .Success( GetResult( connectionRequest ) )
                .WithHistoryContent( new KeyNameResult
                {
                    Id = connectionRequest.Id
                } );

            //if ( attributes == null )
            //{
            //    toolResult = toolResult
            //        .WithMetadata( "availableAttributes", helper.GetAvailableAttributes( connectionRequest ) )
            //        .WithInstructions( "The connection request has been added. Check the list of availableAttributes to see if you have additional information to update connection request attribute values." );
            //}
            //else
            {
                toolResult = toolResult
                    .WithInstructions( "The connection request has been added." );
            }

            return toolResult;
        }

        [Description( "Updates a connection request." )]
        [AgentToolGuid( "8ee3913a-9bca-4971-a490-90abfc1690c3" )]
        public RockToolResult UpdateConnectionRequest(
            string connectionRequestIdKey,

            SetOrClear<string> connectorPersonIdKey = null,
            ConnectionState? connectionState = null,
            string connectionStatusIdKey = null,
            SetOrClear<string> comments = null,
            SetOrClear<string> placementGroupIdKey = null,
            List<AttributeKeyAndValue> attributes = null )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();
            var helper = new AddUpdateHelper( rockContext, AgentRequestContext );

            // Check the connection request parameter.
            if ( !helper.TryGetRequiredEntity<ConnectionRequest>( connectionRequestIdKey, out var connectionRequest ) )
            {
                return helper.ErrorResult;
            }

            // Process for the connector parameter.
            helper.TryUpdateNavigation( connectionRequest, cr => cr.ConnectorPersonAlias, connectorPersonIdKey );

            // Process the state parameter.
            if ( connectionState.HasValue )
            {
                connectionRequest.ConnectionState = connectionState.Value;
            }

            // Process the connection status parameter.
            if ( helper.TryGetOptionalEntity<ConnectionStatus>( connectionStatusIdKey, out var status ) )
            {
                if ( status.ConnectionTypeId == connectionRequest.ConnectionTypeId )
                {
                    connectionRequest.ConnectionStatus = status;
                    connectionRequest.ConnectionStatusId = status.Id;
                }
                else
                {
                    helper.AddError( $"The {nameof( connectionStatusIdKey )} is not valid." );
                    helper.AddInstructions( "Call the LookupConnectionTypesAndOpportunities function to determine available statuses that are valid for this connection request." );
                }
            }

            helper.TryUpdateProperty( connectionRequest, cr => cr.Comments, comments );
            helper.TryUpdateNavigation( connectionRequest, cr => cr.AssignedGroup, placementGroupIdKey, checkSecurity: true );
            helper.TryUpdateNavigation( connectionRequest, cr => cr.PersonAlias, connectorPersonIdKey );
            helper.TrySetAttributeValues( connectionRequest, attributes );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            try
            {
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    if ( attributes != null )
                    {
                        connectionRequest.SaveAttributeValues( rockContext );
                    }
                } );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while saving the connection request." );

                return RockToolResult.Error( "An error occurred while saving the connection request." );
            }

            return RockToolResult
                .Success( GetResult( connectionRequest ) )
                .WithHistoryContent( new KeyNameResult
                {
                    Id = connectionRequest.Id
                } )
                .WithInstructions( "The connection request has been updated." );
        }


        public RockToolResult UpdateConnectionRequestAlternate(
            string connectionRequestIdKey,

            SetOrClear<string> connectorPersonIdKey = null,
            ConnectionState? connectionState = null,
            string connectionStatusIdKey = null,
            SetOrClear<string> comments = null,
            SetOrClear<string> placementGroupIdKey = null,
            List<AttributeKeyAndValue> attributes = null )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();
            var helper = new AddUpdateHelper( rockContext, AgentRequestContext );

            // Check the connection request parameter.
            var connectionRequest = helper.GetRequiredEntity<ConnectionRequest>( connectionRequestIdKey );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            // Process for the connector parameter.
            helper.TryUpdateNavigation( connectionRequest, cr => cr.ConnectorPersonAlias, connectorPersonIdKey );

            // Process the state parameter.
            if ( connectionState.HasValue )
            {
                connectionRequest.ConnectionState = connectionState.Value;
            }

            // Process the connection status parameter.
            var status = helper.GetOptionalEntity<ConnectionStatus>( connectionStatusIdKey );

            if ( status != null )
            {
                if ( status.ConnectionTypeId == connectionRequest.ConnectionTypeId )
                {
                    connectionRequest.ConnectionStatus = status;
                    connectionRequest.ConnectionStatusId = status.Id;
                }
                else
                {
                    helper.AddError( $"The {nameof( connectionStatusIdKey )} is not valid." );
                    helper.AddInstructions( "Call the LookupConnectionTypesAndOpportunities function to determine available statuses that are valid for this connection request." );
                }
            }

            helper.TryUpdateProperty( connectionRequest, cr => cr.Comments, comments );
            helper.TryUpdateNavigation( connectionRequest, cr => cr.AssignedGroup, placementGroupIdKey, checkSecurity: true );
            helper.TryUpdateNavigation( connectionRequest, cr => cr.PersonAlias, connectorPersonIdKey );
            helper.TrySetAttributeValues( connectionRequest, attributes );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            try
            {
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    if ( attributes != null )
                    {
                        connectionRequest.SaveAttributeValues( rockContext );
                    }
                } );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while saving the connection request." );

                return RockToolResult.Error( "An error occurred while saving the connection request." );
            }

            var toolResult = RockToolResult
                .Success( new ConnectionRequestResult
                {
                    Id = connectionRequest.Id,
                } )
                .WithHistoryContent( connectionRequest.IdKey );

            if ( attributes != null )
            {
                toolResult = toolResult.WithInstructions( "The connection request has been updated." );
            }
            else
            {
                toolResult = toolResult
                    .WithMetadata( "availableAttributes", helper.GetAvailableAttributes( connectionRequest ) )
                    .WithInstructions( "The connection request has been updated. Check the list of availableAttributes to see if you have additional information to update connection request attribute values." );
            }

            return toolResult;
        }



        #endregion

        private static ConnectionStatus GetConnectionStatus( AddUpdateHelper helper, string statusIdKey, ConnectionOpportunity opportunity )
        {
            if ( statusIdKey.IsNotNullOrWhiteSpace() )
            {
                if ( !helper.TryGetRequiredEntity<ConnectionStatus>( statusIdKey, out var status ) )
                {
                    return null;
                }

                if ( opportunity != null && status.ConnectionTypeId != opportunity.ConnectionTypeId )
                {
                    helper.AddError( $"The {nameof( statusIdKey )} is not valid." );
                    helper.AddInstructions( "Call the LookupConnectionTypesAndOpportunities function to determine available statuses that match the specified opportunity." );

                    return null;
                }

                return status;
            }
            else if ( opportunity != null )
            {
                var status = opportunity.ConnectionType.ConnectionStatuses.FirstOrDefault();

                if ( status == null )
                {
                    helper.AddError( $"You must provide a {nameof( statusIdKey )}." );
                    helper.AddInstructions( "Call the LookupConnectionTypesAndOpportunities function to determine available statuses that match the specified opportunity." );

                    return null;
                }

                return status;
            }

            return null;
        }
    }

    internal class AddUpdateHelper
    {
        private readonly RockContext _rockContext;

        private readonly AgentRequestContext _agentRequestContext;

        private readonly List<string> _errors = [];

        private readonly List<string> _instructions = [];

        private readonly List<KeyValuePair<string, object>> _metadata = [];

        public bool HasErrors => _errors.Count > 0;

        public RockToolResult ErrorResult => GetErrorResult();

        public AddUpdateHelper( RockContext rockContext, AgentRequestContext agentRequestContext )
        {
            _rockContext = rockContext;
            _agentRequestContext = agentRequestContext;
        }

        private TEntity GetEntity<TEntity>( string parameter, string parameterExpression, bool isRequired, bool checkSecurity )
            where TEntity : class, IEntity, new()
        {
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

            if ( entity == null )
            {
                if ( isRequired )
                {
                    _errors.Add( $"The {parameterExpression} is not valid." );
                }

                return null;
            }

            if ( checkSecurity && entity is ISecured securedEntity )
            {
                if ( !securedEntity.IsAuthorized( Authorization.VIEW, _agentRequestContext.RockRequestContext.CurrentPerson ) )
                {
                    _errors.Add( $"The {parameterExpression} is not valid." );
                }
            }

            if ( entity is Model.Person person && isRequired && !person.PrimaryAliasId.HasValue )
            {
                _errors.Add( $"The {parameterExpression} is not valid." );

                return null;
            }

            return entity;
        }

        public RockToolResult GetErrorResult()
        {
            if ( _errors.Count == 0 )
            {
                throw new Exception( "Unexpected call to GetErrorResult with no errors." );
            }

            var result = RockToolResult.Error( _errors );

            if ( _instructions.Count > 0 )
            {
                result.WithInstructions( string.Join( ";", _instructions ) );
            }

            foreach ( var kvp in _metadata )
            {
                result.WithMetadata( kvp.Key, kvp.Value );
            }

            return result;
        }

        public void AddError( string error )
        {
            _errors.Add( error );
        }

        public void AddInstructions( string instruction )
        {
            _instructions.Add( instruction );
        }

        public void AddMetadata( string key, object value )
        {
            _metadata.Add( new KeyValuePair<string, object>( key, value ) );
        }

        public TEntity GetOptionalEntity<TEntity>( string parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            return GetEntity<TEntity>( parameter, parameterExpression, isRequired: false, checkSecurity: checkSecurity );
        }

        public bool TryGetOptionalEntity<TEntity>( string parameter, out TEntity entity, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            entity = GetEntity<TEntity>( parameter, parameterExpression, isRequired: false, checkSecurity: checkSecurity );

            return entity != null;
        }

        public TEntity GetRequiredEntity<TEntity>( string parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            return GetEntity<TEntity>( parameter, parameterExpression, isRequired: true, checkSecurity: checkSecurity );
        }

        public bool TryGetRequiredEntity<TEntity>( string parameter, out TEntity entity, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : class, IEntity, new()
        {
            entity = GetEntity<TEntity>( parameter, parameterExpression, isRequired: true, checkSecurity: checkSecurity );

            return entity != null;
        }

        public bool TrySetAttributeValues( IHasAttributes entity, List<AttributeKeyAndValue> attributeValues )
        {
            if ( entity == null )
            {
                return true;
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( _rockContext );
            }

            var previousErrorCount = _errors.Count;

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

                    entity.SetAttributeValue( kvp.Key, kvp.Value ?? string.Empty );
                }
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

            if ( _errors.Count != previousErrorCount )
            {
                var entityType = Web.Cache.EntityTypeCache.Get( entity.GetType(), false, _rockContext );
                var typeName = entityType?.FriendlyName.ToLower();

                if ( typeName.IsNullOrWhiteSpace() )
                {
                    var type = entity.GetType();

                    if ( type.IsDynamicProxyType() )
                    {
                        type = type.BaseType;
                    }

                    typeName = type.Name.SplitCase().ToLower();
                }

                AddInstructions( $"Check the list of availableAttributes to see if you have additional information to update {typeName} attribute values." );
                AddMetadata( "availableAttributes", GetAvailableAttributes( entity ) );

                return false;
            }

            return true;
        }

        public ICollection<AttributeLookupResult> GetAvailableAttributes( IHasAttributes entity )
        {
            if ( entity == null || entity.Attributes == null )
            {
                return Array.Empty<AttributeLookupResult>();
            }

            return entity.Attributes.Values
                .Select( a =>
                {
                    var attr = new AttributeLookupResult
                    {
                        Key = a.Key,
                        IsRequired = a.IsRequired,
                        Description = a.Description,
                    };

                    if ( attr.Key == "Priority" )
                    {
                        attr.ValueFormat = "An integer between 0 and 10.";
                    }

                    return attr;
                } )
                .ToList();
        }

        public bool TryUpdateProperty<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, SetOrClear<TProperty> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
            where TProperty : struct
        {
            if ( parameter == null )
            {
                return true;
            }

            var propertyName = ExtractPropertyName( propertyExpression );
            var property =  entity.GetType().GetProperty( propertyName )
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

            return true;
        }

        public bool TryUpdateProperty<TEntity>( TEntity entity, Expression<Func<TEntity, string>> propertyExpression, SetOrClear<string> parameter, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
        {
            if ( parameter == null )
            {
                return true;
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
                else if ( parameter.Value.IsNotNullOrWhiteSpace() )
                {
                    property.SetValue( entity, parameter.Value );
                }
            }
            catch
            {
                AddError( $"The value of {parameterExpression} is not valid." );
            }

            return true;
        }

        public bool TryUpdateNavigation<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, SetOrClear<string> parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
            where TProperty : class, IEntity, new()
        {
            if ( parameter == null )
            {
                return true;
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
                    throw new Exception( $"Navigation property {navigationPropertyName} is required and can't be cleared." );
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
                        return false;
                    }

                    navigationProperty.SetValue( entity, target.PrimaryAlias );
                    navigationIdProperty.SetValue( entity, target.PrimaryAliasId );
                }
                else
                {
                    if ( !TryGetRequiredEntity<TProperty>( parameter.Value, out var target, checkSecurity: checkSecurity, parameterExpression: parameterExpression ) )
                    {
                        return false;
                    }

                    navigationProperty.SetValue( entity, target );
                    navigationIdProperty.SetValue( entity, target.Id );
                }
            }

            return true;
        }

        public bool TryUpdateNavigation<TEntity, TProperty>( TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, string parameter, bool checkSecurity = false, [CallerArgumentExpression( nameof( parameter ) )] string parameterExpression = null )
            where TEntity : IEntity
            where TProperty : class, IEntity, new()
        {
            return TryUpdateNavigation( entity, propertyExpression, new SetOrClear<string> { Value = parameter }, checkSecurity: checkSecurity, parameterExpression: parameterExpression );
        }

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
                throw new ArgumentException( "Expression must be a property access.", nameof( propertyExpression ) );
            }

            return memberExpression.Member.Name;
        }
    }
}

#if !NET6_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    using System;

    [global::System.AttributeUsage( AttributeTargets.Parameter, AllowMultiple = false, Inherited = false )]
    internal sealed class CallerArgumentExpressionAttribute : global::System.Attribute
    {
        public CallerArgumentExpressionAttribute( string parameterName )
        {
            ParameterName = parameterName;
        }



        public string ParameterName { get; }
    }
}
#endif