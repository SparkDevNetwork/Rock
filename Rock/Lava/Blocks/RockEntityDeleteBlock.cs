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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Rock;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava.Blocks.Internal;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// A lava tag that deletes an entity from the database.
    /// </summary>
    public class RockEntityDeleteBlock : LavaBlockBase, ILavaSecured
    {
        #region Fields

        /// <summary>
        /// The type name of the target entity.
        /// </summary>
        private string _entityName;

        /// <summary>
        /// The markup for the block describing the properties being passed
        /// to the block logic.
        /// </summary>
        private string _blockPropertiesMarkup = string.Empty;

        /// <summary>
        /// The inner markup between the start and end tags of the block.
        /// </summary>
        private string _blockContent = string.Empty;

        /// <summary>
        /// The result of the delete entity operation. This will be set by
        /// various methods as the block processes.
        /// </summary>
        private readonly DeleteEntityResult _result = new DeleteEntityResult();

        /// <summary>
        /// The type of entity being deleted.
        /// </summary>
        private Type _entityType = null;

        #endregion

        #region Base Method Overrides

        /// <inheritdoc />
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _blockPropertiesMarkup = markup;

            // Get the internal Lava for the block. The last token will be the block's end tag.
            _blockContent = String.Join( "", tokens.Take( tokens.Count - 1 ) );

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <inheritdoc />
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // Check that we're not in a db transaction where one db execution has already failed. In this
            // case there is no reason for us to process as we'll just be rolled back and it's likely we
            // won't have the data we need to run this update.
            if ( context.GetInternalField( "rock_dbtransaction" ) is DbTransactionResult transactionResult )
            {
                if ( !transactionResult.Success )
                {
                    return;
                }
            }

            // Set RockContext
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            // Enable change-tracking for this data context as entity commands would have disabled this. There is
            // consideration of changing the entity commands to not do this in the future.
#if REVIEW_WEBFORMS
            rockContext.Configuration.AutoDetectChangesEnabled = true;
#endif

            // First ensure that entity commands are allowed in the context
            if ( !this.IsAuthorized( context, RequiredPermissionKey ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, RequiredPermissionKey ) );
                base.OnRender( context, result );
                return;
            }

            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );
            var parms = settings.Attributes;

            ExtractBlockChildElements( context, _blockContent, out _, out var residualMarkup );

            if ( parms["id"] == null || parms["id"].AsIntegerOrNull() == 0 )
            {
                _result.Success = false;
                _result.ErrorMessage = "No id was provided for the entity to delete.";
                base.OnRender( context, result );
                return;
            }

            // Get the service for the provided entity.
            var service = GetEntityService( rockContext );

            // Get the entity to delete
            var entity = GetEntity( parms["id"], service );

            var deleted = false;

            if ( _entityName == "person" || _entityName == "business" )
            {
                _result.Success = false;
                _result.ErrorMessage = "The entity type 'Person' and 'Business' are not supported for deletion.";
            }
            else if ( entity == null )
            {
                _result.Success = false;
                _result.ErrorMessage = $"Could not find the entity of type {_entityName} with the id of '{parms["id"]}'";
            }
            else
            {
                deleted = DeleteEntity( context, entity, IsSecurityEnabled( parms ), service, rockContext );
            }

            // Mark that the database item was not saved to the Lava context so any transactions know to rollback
            if ( _result.Success == false || !deleted )
            {
                // Append current validation messages to Lava context
                if ( context.GetInternalField( "rock_dbtransaction" ) is DbTransactionResult transactionResultUpdate )
                {
                    transactionResultUpdate.Success = false;
                    transactionResultUpdate.ValidationErrors.AddRange( _result.ValidationErrors );
                    transactionResultUpdate.ErrorMessage += $" {_result.ErrorMessage}";
                }
            }

            // Process send validation to client
            if ( parms["clientvalidationenabled"] == "true" )
            {
                if ( _result.Success == false )
                {
                    var validationResult = $@"{{""showValidationSummary"":""{Base64Encode( _result.ValidationErrors.ToJson() )}""}}";

#if REVIEW_WEBFORMS
                    HttpContext.Current?.Response.Headers.Add( "HX-Trigger", validationResult );
                    HttpContext.Current?.Response.Headers.Add( "HX-Reswap", "none" );
#else
                    throw new NotImplementedException();
#endif
                    throw new LavaInterruptException();
                }
            }

            // Create the object that we'll return back to the Lava template
            var returnObject = new RockDynamic();
            returnObject["Success"] = _result.Success;
            returnObject["ErrorMessage"] = _result.ErrorMessage;
            returnObject["ValidationErrors"] = _result.ValidationErrors;

            context.SetMergeField( parms["return"], returnObject );

            // Render the final results
            var engine = context?.GetService<ILavaEngine>();
            var renderParameters = new LavaRenderParameters { Context = context };
            var results = engine.RenderTemplate( residualMarkup, renderParameters );

            result.Write( results.Text );
            base.OnRender( context, result );
        }

        /// <inheritdoc />
        public override void OnStartup( ILavaEngine engine )
        {
            RegisterEntityDeleteCommands( engine );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logic to determine if security is enabled for modifying the entity.
        /// </summary>
        /// <param name="parms">The parameters that were passed to the block command.</param>
        /// <returns><c>true</c> if security should be enabled when processing changes.</returns>
        private bool IsSecurityEnabled( Dictionary<string, string> parms )
        {
            // People can not be secured in Rock (and a business is a person)
            if ( _entityName.ToLower() == "person" || _entityName.ToLower() == "business" )
            {
                return false;
            }

            return parms["securityenabled"].AsBoolean();
        }

        /// <summary>
        /// Extracts a set of child elements from the content of the block.
        /// Child elements are grouped by tag name, and each item in the collection has a set of properties
        /// corresponding to the child element tag attributes and a "content" property representing the inner content of the child element.
        /// </summary>
        /// <param name="context">The rendering context currently handling rendering this block.</param>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="childParameters">The child parameters.</param>
        /// <param name="residualBlockContent">The inner content of the block after the child tags have been extracted.</param>
        /// <returns><c>true</c> if the inner content was valid; otherwise <c>false</c>.</returns>
        private bool ExtractBlockChildElements( ILavaRenderContext context, string blockContent, out Dictionary<string, object> childParameters, out string residualBlockContent )
        {
            childParameters = new Dictionary<string, object>();

            var startTagStartExpress = new Regex( @"\[\[\s*" );

            var isValid = true;
            var matchExists = true;
            while ( matchExists )
            {
                var match = startTagStartExpress.Match( blockContent );
                if ( match.Success )
                {
                    int startTagStartIndex = match.Index;

                    // get the name of the parameter
                    var parmNameMatch = new Regex( @"[\w-]*" ).Match( blockContent, startTagStartIndex + match.Length );
                    if ( parmNameMatch.Success )
                    {
                        var parmNameStartIndex = parmNameMatch.Index;
                        var parmNameEndIndex = parmNameStartIndex + parmNameMatch.Length;
                        var parmName = blockContent.Substring( parmNameStartIndex, parmNameMatch.Length );

                        // get end of the tag index
                        var startTagEndIndex = blockContent.IndexOf( "]]", parmNameStartIndex ) + 2;

                        // get the tags parameters
                        var tagParms = blockContent.Substring( parmNameEndIndex, startTagEndIndex - parmNameEndIndex ).Trim();

                        // get the closing tag location
                        var endTagMatchExpression = String.Format( @"\[\[\s*end{0}\s*\]\]", parmName );
                        var endTagMatch = new Regex( endTagMatchExpression ).Match( blockContent, startTagStartIndex );

                        if ( endTagMatch.Success )
                        {
                            var endTagStartIndex = endTagMatch.Index;
                            var endTagEndIndex = endTagStartIndex + endTagMatch.Length;

                            // get the parm content (the string between the two parm tags)
                            var parmContent = blockContent.Substring( startTagEndIndex, endTagStartIndex - startTagEndIndex ).Trim();

                            // Run Lava across the content
                            if ( parmContent.IsNotNullOrWhiteSpace() )
                            {
                                var engine = context?.GetService<ILavaEngine>();
                                var renderParameters = new LavaRenderParameters { Context = context };
                                parmContent = engine.RenderTemplate( parmContent, renderParameters ).Text;
                            }

                            // create dynamic object from parms
                            var dynamicParm = new Dictionary<string, object>
                            {
                                { "content", parmContent }
                            };

                            var parmItems = Regex.Matches( tagParms, @"(\S*?:'[^']+')" )
                                .Cast<Match>()
                                .Select( m => m.Value )
                                .ToList();

                            foreach ( var item in parmItems )
                            {
                                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                                if ( itemParts.Length > 1 )
                                {
                                    dynamicParm.Add( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                                }
                            }

                            // add new parm to a collection of parms
                            var propertyCollectionName = parmName.Pluralize();

                            if ( childParameters.ContainsKey( propertyCollectionName ) )
                            {
                                var parmList = ( List<object> ) childParameters[propertyCollectionName];
                                parmList.Add( dynamicParm );
                            }
                            else
                            {
                                var parmList = new List<object>
                                {
                                    dynamicParm
                                };
                                childParameters.Add( propertyCollectionName, parmList );
                            }

                            // pull this tag out of the block content
                            blockContent = blockContent.Remove( startTagStartIndex, endTagEndIndex - startTagStartIndex );
                        }
                        else
                        {
                            // there was no matching end tag, for safety sake we'd better bail out of loop
                            isValid = false;
                            matchExists = false;
                            blockContent = blockContent + "Warning: Missing field end tag." + parmName;
                        }
                    }
                    else
                    {
                        // there was no parm name on the tag, for safety sake we'd better bail out of loop
                        isValid = false;
                        matchExists = false;
                        blockContent += "Warning: Field definition does not have any parameters.";
                    }

                }
                else
                {
                    matchExists = false; // we're done here
                }
            }

            residualBlockContent = blockContent.Trim();

            return isValid;
        }

        /// <summary>
        /// Performs a base64 encoding fo the text and then makes it safe for
        /// use in a query string parameter by replacing the "+" and "/" characters.
        /// </summary>
        /// <param name="text">The text to be encoded as base64 data.</param>
        /// <returns>A string that represents the text as a base64 string.</returns>
        private string Base64Encode( string text )
        {
            return Convert.ToBase64String( Encoding.UTF8.GetBytes( text ) ).TrimEnd( '=' ).Replace( '+', '-' )
                .Replace( '/', '_' );
        }

        /// <summary>
        /// Deletes the entity from the database.
        /// </summary>
        /// <param name="context">The Lava render context that will provide additional values.</param>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="isSecurityEnabled">Whether or not security is enabled.</param>
        /// <param name="service">The service to use for deletion.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns><c>true</c> if the entity was deleted; otherwise <c>false</c>.</returns>
        private bool DeleteEntity( ILavaRenderContext context, IEntity entity, bool isSecurityEnabled, IService service, RockContext rockContext )
        {
            // Ensure we have security to this entity
            if ( isSecurityEnabled )
            {
                var currentPerson = GetCurrentPerson( context );

                if ( !( entity is ISecured itemSecured ) || !itemSecured.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    _result.Success = false;
                    _result.ErrorMessage = "You do not have rights to modify this entity.";
                    return false;
                }
            }

            try
            {
                var errorMessage = string.Empty;

                // Use reflection to check if there is a Boolean property of IsSystem, and the
                // value is true, do not allow the delete.
                var isSystemProperty = entity.GetType().GetProperty( "IsSystem" );
                if ( isSystemProperty != null )
                {
                    var isSystemValue = isSystemProperty.GetValue( entity ) as bool?;

                    if ( isSystemValue.HasValue && isSystemValue.Value )
                    {
                        _result.Success = false;
                        _result.ErrorMessage = "This entity is a system entity and can not be deleted.";
                        return false;
                    }
                }

                // Use reflection to check if the service has a CanDelete method.
                // If so, call the method to ensure the entity is valid
                // for deletion.
                // Note: Some services, like NoteService, have a CanDelete method
                // that takes additional parameters. This code does not handle that.
                var canDeleteMethod = service.GetType().GetMethod( "CanDelete" );

                // Check to ensure our canDeleteMethod matches our supported
                // method signature (bool CanDelete( IEntity entity, out string errorMessage ))
                if ( canDeleteMethod != null )
                {
                    var canDeleteParameters = canDeleteMethod.GetParameters();
                    if ( canDeleteParameters.Length != 2 || canDeleteParameters[0].ParameterType != typeof( IEntity ) || canDeleteParameters[1].ParameterType != typeof( string ).MakeByRefType() )
                    {
                        canDeleteMethod = null;
                    }
                }

                if ( canDeleteMethod != null )
                {
                    // Prepare parameters: entity and the out parameter (initially set to null)
                    object[] parameters = { entity, null };

                    // Invoke the method
                    var canDeleteResult = canDeleteMethod.Invoke( service, parameters ) as bool?;

                    // The out parameter is the error message.
                    errorMessage = parameters[1] as string;

                    // Check the result and handle the error message
                    if ( canDeleteResult.HasValue && !canDeleteResult.Value )
                    {
                        _result.Success = false;
                        _result.ErrorMessage = errorMessage;
                        return false;
                    }
                }

                // Use reflection to check if the service has a Delete method. Some services have multiple delete methods so we
                // must be specific which one we want by providing the entity type.
                var deleteMethod = service.GetType().GetMethod( "Delete", new[] { entity.GetType() } );
                if ( deleteMethod != null )
                {
                    // Prepare parameters: entity
                    object[] parameters = { entity };

                    // Invoke the method
                    var deleted = deleteMethod.Invoke( service, parameters );
                    if ( deleted is bool deletedValue && deletedValue == false )
                    {
                        _result.Success = false;
                        _result.ErrorMessage = "The entity could not be deleted.";
                        return false;
                    }
                }
            }
            catch ( Exception ex )
            {
                _result.Success = false;

                if ( ex.Message.StartsWith( "Entity Validation Error" ) )
                {
                    _result.ValidationErrors = ParseValidationErrors( ex.Message );
                    _result.ErrorMessage = "A validation error occurred.";
                }

                // Check for foreign key or other errors. These will have a inner exception
                if ( ex.InnerException != null )
                {
                    _result.ErrorMessage = GetBaseExceptionMessage( ex.InnerException );
                }
                else
                {
                    _result.ErrorMessage = ex.Message;
                }

                return false;
            }

            if ( _result.Success )
            {
                rockContext.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the exception message at the bottom of the exception chain.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>The inner most exception.</returns>
        private string GetBaseExceptionMessage( Exception ex )
        {
            while ( ex.InnerException != null )
            {
                ex = ex.InnerException;
            }

            return ex.Message;
        }

        /// <summary>
        /// Parses any validation messages from the exception message.
        /// </summary>
        /// <param name="validationExceptionMessage">The validation exception message to be parsed.</param>
        /// <returns>A list of validation errors found in the message.</returns>
        private List<ValidationError> ParseValidationErrors( string validationExceptionMessage )
        {
            // Validation meta data replace pattern
            string pattern = @"\[.*?\]";

            var validationMessages = new List<ValidationError>();
            validationExceptionMessage = validationExceptionMessage.ReplaceFirstOccurrence( "Entity Validation Error: ", "" );

            var messages = validationExceptionMessage.Split( ';' ).ToList();
            // Sample Validation Message: [Person/Modified/Id=15/Property=NickName] The field NickName must be a string or array type with a maximum length of '50'.
            foreach ( var message in messages )
            {
                validationMessages.Add( new ValidationError( Regex.Replace( message, pattern, "" ) ) );
            }

            return validationMessages;
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The render context used to obtain the current person from.</param>
        /// <returns>The <see cref="Person"/> that represents the current person, or <c>null</c>.</returns>
        private static Person GetCurrentPerson( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            var currentPerson = context.GetMergeField( "CurrentPerson", null ) as Person;

            if ( currentPerson == null )
            {
#if REVIEW_WEBFORMS
                var httpContext = HttpContext.Current;
                if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
#endif
            }

            return currentPerson;
        }

        /// <summary>
        /// Gets (or creates) and entity from the provided id.
        /// </summary>
        /// <param name="id">The identifier as an integer, unique identifier or IdKey.</param>
        /// <param name="service">The service that will be used to load the entity.</param>
        /// <returns>An instance of the entity or <c>null</c> if it was not found or had an invalid identifier.</returns>
        private IEntity GetEntity( string id, IService service )
        {
            // Try id as an integer
            var idAsInt = id.AsIntegerOrNull();
            if ( idAsInt.HasValue && idAsInt.Value > 0 )
            {
                return GetEntityFromIdAsInt( idAsInt.Value, service );
            }

            // Try id as a guid
            var idAsGuid = id.AsGuidOrNull();

            if ( idAsGuid.HasValue )
            {
                return GetEntityFromIdAsGuid( idAsGuid.Value, service );
            }

            // Try id as an IdKey 
            idAsInt = IdHasher.Instance.GetId( id );

            if ( idAsInt.HasValue )
            {
                return GetEntityFromIdAsInt( idAsInt.Value, service );
            }

            return null;
        }

        /// <summary>
        /// Gets the entity from the id as an guid.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to be loaded.</param>
        /// <param name="service">The service that will be used to load the entity.</param>
        /// <returns>An instance of the entity or <c>null</c> if it was not found.</returns>
        private IEntity GetEntityFromIdAsGuid( Guid id, IService service )
        {
            MethodInfo getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );

            if ( getMethod == null )
            {
                return null;
            }

            var getResult = getMethod.Invoke( service, new object[] { id } );
            return getResult as IEntity;
        }

        /// <summary>
        /// Gets the entity from the id as an integer.
        /// </summary>
        /// <param name="id">the integer identifier of the entity to be loaded.</param>
        /// <param name="service">The service that will be used to load the entity.</param>
        /// <returns>An instance of the entity or <c>null</c> if it was not found.</returns>
        private IEntity GetEntityFromIdAsInt( int id, IService service )
        {
            MethodInfo getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );

            if ( getMethod == null )
            {
                return null;
            }

            var getResult = getMethod.Invoke( service, new object[] { id } );
            return getResult as IEntity;
        }

        /// <summary>
        /// Gets a list of parameter attributes from the block command.
        /// </summary>
        /// <param name="markup">The markup that describes the command parameters.</param>
        /// <param name="context">The rendering context used to parse the parameters.</param>
        /// <returns>An instance of <see cref="LavaElementAttributes"/> that contains the parameters.</returns>
        private static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Create default settings
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            if ( settings.Attributes.Count == 0 )
            {
                throw new Exception( "No parameters were found in your entity delete command. The syntax for a parameter is parmName:'' (note that you must use single quotes)." );
            }

            settings.AddOrIgnore( "securityenabled", "true" );
            settings.AddOrIgnore( "clientvalidationenabled", "false" );
            settings.AddOrIgnore( "return", "DeleteResult" );

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
#if REVIEW_WEBFORMS
                    if ( HttpContext.Current?.Request[dynamicParm] != null )
                    {
                        var dynamicParmValue = HttpContext.Current.Request[dynamicParm].ToString();

                        switch ( dynamicParm )
                        {
                            case "id":
                            case "securityenabled":
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
#else
                    throw new NotImplementedException();
#endif
                }
            }

            return settings;
        }

        /// <summary>
        /// Returns a service based on the entity type being modified.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>The <see cref="IService"/> that will be used to access the entity.</returns>
        private IService GetEntityService( RockContext rockContext )
        {
            var entityTypeCache = GetEntityType();

            if ( entityTypeCache == null )
            {
                _result.Success = false;
                _result.ErrorMessage = $"Could not find related entity for the type '{_entityName}'.";
                return null;
            }

            _entityType = entityTypeCache.GetEntityType();
            if ( _entityType == null )
            {
                _result.Success = false;
                _result.ErrorMessage = $"Could not find c# type for the entity '{_entityName}'.";
                return null;
            }

            // Create an instance of the entity's service
            return Reflection.GetServiceForEntityType( _entityType, rockContext );
        }

        /// <summary>
        /// Get's the entity type from the model name.
        /// </summary>
        /// <returns>An instance of <see cref="EntityTypeCache"/> that represents the entity type specified in the command.</returns>
        private EntityTypeCache GetEntityType()
        {
            var modelName = string.Empty;

            // Get a service for the entity based off it's friendly name
            if ( _entityName == "business" )
            {
                modelName = "Rock.Model.Person";
            }
            else
            {
                modelName = "Rock.Model." + _entityName;
            }

            // Check first to see if this is a core model. use the createIfNotFound = false option
            var entityTypeCache = EntityTypeCache.Get( modelName, false );

            // If not, look for plug-in model
            if ( entityTypeCache == null )
            {
                var entityTypes = EntityTypeCache.All();

                // Look for first plug-in model that has same friendly name
                entityTypeCache = entityTypes
                    .Where( e =>
                        e.IsEntity &&
                        !e.Name.StartsWith( "Rock.Model" ) &&
                        e.FriendlyName != null &&
                        e.FriendlyName.RemoveSpaces().ToLower() == _entityName.RemoveSpaces().ToLower() )
                    .OrderBy( e => e.Id )
                    .FirstOrDefault();

                // If still null check to see if this was a duplicate class and full class name was used as entity name
                if ( entityTypeCache == null )
                {
                    modelName = _entityName.Replace( '_', '.' );
                    entityTypeCache = entityTypes.Where( e => String.Equals( e.Name, modelName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
                }
            }

            return entityTypeCache;
        }

        /// <summary>
        /// Helper method to register the entity commands.
        /// </summary>
        public static void RegisterEntityDeleteCommands( ILavaEngine engine )
        {
            // If the database is not connected, we do not have access to entity definitions.
            // This can occur when the Lava engine is started without an attached database.
            if ( RockApp.Current == null || !RockApp.Current.GetDatabaseConfiguration().IsDatabaseAvailable )
            {
                return;
            }

            var entityTypes = EntityTypeCache.All();

            // Register the core models, replacing existing blocks of the same name if necessary.
            foreach ( var entityType in entityTypes
                .Where( e =>
                    e.IsEntity &&
                    e.Name.StartsWith( "Rock.Model" ) &&
                    e.FriendlyName != null &&
                    e.FriendlyName != "" ) )
            {
                RegisterEntityCommand( engine, entityType );
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
                RegisterEntityCommand( engine, entityType );
            }

        }

        /// <summary>
        /// Registers a block for the specified entity type.
        /// </summary>
        /// <param name="engine">The Lava engine to register the blocks for.</param>
        /// <param name="entityType">The entity type to register the command for.</param>
        private static void RegisterEntityCommand( ILavaEngine engine, EntityTypeCache entityType )
        {
            if ( entityType != null )
            {
                string blockName = $"delete{entityType.FriendlyName.RemoveSpaces().ToLower()}";
                string entityName = entityType.FriendlyName.RemoveSpaces();

                // if entity name is already registered, use the full class name with namespace
                if ( engine.GetRegisteredElements().ContainsKey( blockName ) )
                {
                    blockName = entityType.Name.Replace( '.', '_' );
                }

                engine.RegisterBlock( blockName, ( name ) => CreateEntityBlockInstance( entityName ) );
            }
        }

        /// <summary>
        /// Factory method to return a new block for the specified Entity.
        /// </summary>
        /// <param name="entityName">The name of the entity.</param>
        /// <returns>An new instance that will handle the block request.</returns>
        private static RockEntityDeleteBlock CreateEntityBlockInstance( string entityName )
        {
            // Return a block having a tag name corresponding to the entity name.
            return new RockEntityDeleteBlock() { SourceElementName = entityName, _entityName = entityName };
        }

        #endregion

        #region ILavaSecured Implementation

        /// <inheritdoc />
        public string RequiredPermissionKey => "RockEntityDelete";

        #endregion
    }
}
