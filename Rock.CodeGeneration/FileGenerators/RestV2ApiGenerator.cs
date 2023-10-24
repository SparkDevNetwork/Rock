using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Utility;

namespace Rock.CodeGeneration.FileGenerators
{
    /// <summary>
    /// Provides generation functionality for the REST V2 API files.
    /// </summary>
    class RestV2ApiGenerator : Generator
    {
        /// <summary>
        /// Generates the content of the file that holds the standard REST
        /// API endpoints.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="endpoints">The endpoints to be generated.</param>
        /// <param name="existingContent">Content of the existing file, if any.</param>
        /// <returns>A string that contains the file content that should be written to disk.</returns>
        public string GenerateStandardFileContent( Type modelType, CodeGenerateRestEndpoint endpoints, string existingContent )
        {
            var usings = new List<string>();
            var usingMaps = new List<(string Name, string Source)>();
            var content = new StringBuilder();
            var hasGetItem = endpoints.HasFlag( CodeGenerateRestEndpoint.ReadItem );
            var hasPostItem = endpoints.HasFlag( CodeGenerateRestEndpoint.CreateItem );
            var hasPutItem = endpoints.HasFlag( CodeGenerateRestEndpoint.ReplaceItem );
            var hasPatchItem = endpoints.HasFlag( CodeGenerateRestEndpoint.UpdateItem );
            var hasDeleteItem = endpoints.HasFlag( CodeGenerateRestEndpoint.DeleteItem );
            var hasGetAttributeValues = endpoints.HasFlag( CodeGenerateRestEndpoint.ReadAttributeValues );
            var hasPatchAttributeValues = endpoints.HasFlag( CodeGenerateRestEndpoint.UpdateAttributeValues );
            var hasSearch = endpoints.HasFlag( CodeGenerateRestEndpoint.Search );

            usings.Add( "System.Collections.Generic" );
            usings.Add( "System.Net" );
            usings.Add( "Rock.Rest.Filters" );
            usings.Add( "Rock.Security" );
            usings.Add( "Rock.ViewModels.Core" );
            usings.Add( "Rock.ViewModels.Rest.Models" );

            usingMaps.Add( ("FromBodyAttribute", "System.Web.Http.FromBodyAttribute") );
            usingMaps.Add( ("IActionResult", "System.Web.Http.IHttpActionResult") );
            usingMaps.Add( ("RoutePrefixAttribute", "System.Web.Http.RoutePrefixAttribute") );
            usingMaps.Add( ("RouteAttribute", "System.Web.Http.RouteAttribute") );
            usingMaps.Add( ("HttpGetAttribute", "System.Web.Http.HttpGetAttribute") );
            usingMaps.Add( ("HttpPostAttribute", "System.Web.Http.HttpPostAttribute") );
            usingMaps.Add( ("HttpPutAttribute", "System.Web.Http.HttpPutAttribute") );
            usingMaps.Add( ("HttpPatchAttribute", "System.Web.Http.HttpPatchAttribute") );
            usingMaps.Add( ("HttpDeleteAttribute", "System.Web.Http.HttpDeleteAttribute") );

            // Emit all the mappings used for compatibility with webforms.
            content.AppendLine( "#if WEBFORMS" );
            foreach ( var (name, source) in usingMaps )
            {
                content.AppendLine( $"    using {name} = {source};" );
            }
            content.AppendLine( "#endif" );
            content.AppendLine();

            // Emit the class definition.
            var controllerGuid = GetExistingControllerGuid( existingContent, $"{modelType.Name.Pluralize()}Controller" ) ?? Guid.NewGuid();
            content.AppendLine( "    /// <summary>" );
            content.AppendLine( $"    /// Provides data API endpoints for {modelType.Name.Pluralize().SplitCase()}." );
            content.AppendLine( "    /// </summary>" );
            content.AppendLine( $"    [RoutePrefix( \"api/v2/models/{modelType.Name.Pluralize().ToLower()}\" )]" );
            content.AppendLine( "    [SecurityAction( \"UnrestrictedView\", \"Allows viewing entities regardless of per-entity security authorization.\" )]" );
            content.AppendLine( "    [SecurityAction( \"UnrestrictedEdit\", \"Allows editing entities regardless of per-entity security authorization.\" )]" );
            content.AppendLine( $"    [Rock.SystemGuid.RestControllerGuid( \"{controllerGuid}\" )]" );
            content.AppendLine( $"    public partial class {modelType.Name.Pluralize()}Controller : ApiControllerBase" );

            // No final new-line since each action starts with a new-line.
            content.Append( "    {" );

            if ( hasGetItem )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "GetItem" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GenerateGetItemAction( modelType, actionGuid ) );
            }

            if ( hasPostItem )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "PostItem" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GeneratePostItemAction( modelType, actionGuid ) );
            }

            if ( hasPutItem )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "PutItem" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GeneratePutItemAction( modelType, actionGuid ) );
            }

            if ( hasPatchItem )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "PatchItem" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GeneratePatchItemAction( modelType, actionGuid ) );
            }

            if ( hasDeleteItem )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "DeleteItem" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GenerateDeleteItemAction( modelType, actionGuid ) );
            }

            if ( hasGetAttributeValues )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "GetAttributeValues" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GenerateGetAttributeValuesAction( modelType, actionGuid ) );
            }

            if ( hasPatchAttributeValues )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "PatchAttributeValues" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GeneratePatchAttributeValuesAction( modelType, actionGuid ) );
            }

            if ( hasSearch )
            {
                var actionGuid = GetExistingActionGuid( existingContent, "GetSearch" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GenerateGetSearchAction( modelType, actionGuid ) );

                actionGuid = GetExistingActionGuid( existingContent, "PostSearch" ) ?? Guid.NewGuid();

                content.AppendLine();
                content.Append( GeneratePostSearchAction( modelType, actionGuid ) );
            }

            content.AppendLine( "    }" );

            return GenerateCSharpFile( usings, "Rock.Rest.v2.Models", content.ToString() );
        }

        /// <summary>
        /// Generates the content of the GetItem action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateGetItemAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Gets a single item from the database.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>The requested item.</returns>
        [HttpGet]
        [Authenticate]
        [Secured]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( {modelType.FullName} ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult GetItem( string id )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).Get( id );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PostItem action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePostItemAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        /// <param name=""value"">The item to be created.</param>
        /// <returns>An object that contains the new identifier values.</returns>
        [HttpPost]
        [Authenticate]
        [Secured]
        [Route( """" )]
        [ProducesResponseType( HttpStatusCode.Created, Type = typeof( CreatedAtResponseBag ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PostItem( [FromBody] {modelType.FullName} value )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).Create( value );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PutItem action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePutItemAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a full update of the item. All property values must be
        /// specified.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name=""value"">The item that represents all the new values.</param>
        /// <returns>An empty response.</returns>
        [HttpPut]
        [Authenticate]
        [Secured]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PutItem( string id, [FromBody] {modelType.FullName} value )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).Update( id, value );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PatchItem action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePatchItemAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a partial update of the item. Only specified property keys
        /// will be updated.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name=""values"">An object that identifies the properties and values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Authenticate]
        [Secured]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PatchItem( string id, [FromBody] Dictionary<string, object> values )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).Patch( id, values );
        }}
";
        }

        /// <summary>
        /// Generates the content of the DeleteItem action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateDeleteItemAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Deletes a single item from the database.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An empty response.</returns>
        [HttpDelete]
        [Authenticate]
        [Secured]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult DeleteItem( string id )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).Delete( id );
        }}
";
        }

        /// <summary>
        /// Generates the content of the GetAttributeValues action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateGetAttributeValuesAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Gets all the attribute values for the specified item.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An array of objects that represent all the attribute values.</returns>
        [HttpGet]
        [Authenticate]
        [Secured]
        [Route( ""{{id}}/attributevalues"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Dictionary<string, ModelAttributeValueBag> ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult GetAttributeValues( string id )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).GetAttributeValues( id );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PatchAttributeValues action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePatchAttributeValuesAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a partial update of attribute values for the item. Only
        /// attributes specified will be updated.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name=""values"">An object that identifies the attribute keys and raw values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Authenticate]
        [Secured]
        [Route( ""{{id}}/attributevalues"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PatchAttributeValues( string id, [FromBody] Dictionary<string, string> values )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).PatchAttributeValues( id, values );
        }}
";
        }

        /// <summary>
        /// Generates the content of the GetSearch action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateGetSearchAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a search of items using the specified system query.
        /// </summary>
        /// <param name=""searchKey"">The key that identifies the entity search query to execute.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpGet]
        [Authenticate]
        [Secured]
        [Route( ""search/{{searchKey}}"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult GetSearch( string searchKey )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).Search( searchKey, null );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PostSearch action.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePostSearchAction( Type modelType, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a search of items using the specified system query.
        /// </summary>
        /// <param name=""query"">Additional query refinement options to be applied.</param>
        /// <param name=""searchKey"">The key that identifies the entity search query to execute.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpPost]
        [Authenticate]
        [Secured]
        [Route( ""search/{{searchKey}}"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PostSearch( [FromBody] EntitySearchQueryBag query, string searchKey )
        {{
            return new RestApiHelper<{modelType.FullName}, {modelType.FullName}Service>( this ).Search( searchKey, query );
        }}
";
        }

        /// <summary>
        /// Gets the existing controller unique identifier from the content.
        /// </summary>
        /// <param name="content">The content to be scanned.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>A <see cref="Guid"/> if the controller was found, otherwise <c>null</c>.</returns>
        private static Guid? GetExistingControllerGuid( string content, string controllerName )
        {
            if ( content.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var match = Regex.Match( content, $"RestControllerGuid\\(\\s*\"([a-zA-Z0-9-]+)\"\\s* \\)]\\s*public partial class {controllerName}\\s*:" );

            return match.Success ? match.Groups[1].Value.AsGuidOrNull() : null;
        }

        /// <summary>
        /// Gets the existing action unique identifier from the content.
        /// </summary>
        /// <param name="content">The content to be scanned.</param>
        /// <param name="controllerName">Name of the action.</param>
        /// <returns>A <see cref="Guid"/> if the action was found, otherwise <c>null</c>.</returns>
        private static Guid? GetExistingActionGuid( string content, string actionName )
        {
            if ( content.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var match = Regex.Match( content, $"RestActionGuid\\(\\s*\"([a-zA-Z0-9-]+)\"\\s* \\)]\\s*public IActionResult {actionName}\\(" );

            return match.Success ? match.Groups[1].Value.AsGuidOrNull() : null;
        }
    }
}
