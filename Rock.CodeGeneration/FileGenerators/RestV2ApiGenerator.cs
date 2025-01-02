using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Rock.Utility;

namespace Rock.CodeGeneration.FileGenerators
{
    /// <summary>
    /// Provides generation functionality for the REST V2 API files.
    /// </summary>
    class RestV2ApiGenerator : Generator
    {
        /// <summary>
        /// The C# namespace to use when generating the file.
        /// </summary>
        public string NamespaceName { get; set; } = "Rock.Rest.v2.Models";

        /// <summary>
        /// The prefix to use when generating the route for each controller.
        /// This must not end with a trailing slash.
        /// </summary>
        public string ControllerRoutePrefix { get; set; } = "api/v2/models";

        /// <summary>
        /// Generates the content of the file that holds the standard REST
        /// API endpoints.
        /// </summary>
        /// <param name="modelTypeName">Name of the model C# class.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="endpoints">The endpoints to be generated.</param>
        /// <returns>A string that contains the file content that should be written to disk.</returns>
        public string GenerateStandardFileContent( string modelTypeName, string modelTypeFullName, CodeGenerateRestEndpoint endpoints )
        {
            var modelControllerNamespaceGuid = new Guid( "d9b3e947-5c19-4145-a356-712851d544de" );
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

            usings.Add( "Microsoft.AspNetCore.Mvc" );
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
            var routePrefix = $"{ControllerRoutePrefix}/{modelTypeName.Pluralize().ToLower()}";
            var controllerGuid = NewV5Guid( modelControllerNamespaceGuid, routePrefix );
            content.AppendLine( "    /// <summary>" );
            content.AppendLine( $"    /// Provides data API endpoints for {modelTypeName.Pluralize().SplitCase()}." );
            content.AppendLine( "    /// </summary>" );
            content.AppendLine( $"    [RoutePrefix( \"{routePrefix}\" )]" );
            content.AppendLine( "    [SecurityAction( Security.Authorization.UNRESTRICTED_VIEW, \"Allows viewing entities regardless of per-entity security authorization.\" )]" );
            content.AppendLine( "    [SecurityAction( Security.Authorization.UNRESTRICTED_EDIT, \"Allows editing entities regardless of per-entity security authorization.\" )]" );
            content.AppendLine( $"    [Rock.SystemGuid.RestControllerGuid( \"{controllerGuid}\" )]" );
            content.AppendLine( $"    public partial class {modelTypeName.Pluralize()}Controller : ApiControllerBase" );

            // No final new-line since each action starts with a new-line.
            content.Append( "    {" );

            if ( hasGetItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "GetItem" );

                content.AppendLine();
                content.Append( GenerateGetItemAction( modelTypeFullName, actionGuid ) );
            }

            if ( hasPostItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PostItem" );

                content.AppendLine();
                content.Append( GeneratePostItemAction( modelTypeFullName, actionGuid ) );
            }

            if ( hasPutItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PutItem" );

                content.AppendLine();
                content.Append( GeneratePutItemAction( modelTypeFullName, actionGuid ) );
            }

            if ( hasPatchItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PatchItem" );

                content.AppendLine();
                content.Append( GeneratePatchItemAction( modelTypeFullName, actionGuid ) );
            }

            if ( hasDeleteItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "DeleteItem" );

                content.AppendLine();
                content.Append( GenerateDeleteItemAction( modelTypeFullName, actionGuid ) );
            }

            if ( hasGetAttributeValues )
            {
                var actionGuid = NewV5Guid( controllerGuid, "GetAttributeValues" );

                content.AppendLine();
                content.Append( GenerateGetAttributeValuesAction( modelTypeFullName, actionGuid ) );
            }

            if ( hasPatchAttributeValues )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PatchAttributeValues" );

                content.AppendLine();
                content.Append( GeneratePatchAttributeValuesAction( modelTypeFullName, actionGuid ) );
            }

            if ( hasSearch )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PostSearch" );

                content.AppendLine();
                content.Append( GeneratePostSearchAction( modelTypeFullName, actionGuid ) );

                actionGuid = NewV5Guid( controllerGuid, "GetSearchByKey" );

                content.AppendLine();
                content.Append( GenerateGetSearchByKeyAction( modelTypeFullName, actionGuid ) );

                actionGuid = NewV5Guid( controllerGuid, "PostSearchByKey" );

                content.AppendLine();
                content.Append( GeneratePostSearchByKeyAction( modelTypeFullName, actionGuid ) );
            }

            content.AppendLine( "    }" );

            return GenerateCSharpFile( usings, NamespaceName, content.ToString() );
        }

        /// <summary>
        /// Generates the content of the GetItem action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateGetItemAction( string modelTypeFullName, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Gets a single item from the database.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>The requested item.</returns>
        [HttpGet]
        [Authenticate]
        [Secured( Security.Authorization.VIEW )]
        [ExcludeSecurityActions( Security.Authorization.EDIT, Security.Authorization.UNRESTRICTED_EDIT )]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( {modelTypeFullName} ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult GetItem( string id )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Get( id );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PostItem action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePostItemAction( string modelTypeFullName, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        /// <param name=""value"">The item to be created.</param>
        /// <returns>An object that contains the new identifier values.</returns>
        [HttpPost]
        [Authenticate]
        [Secured( Security.Authorization.EDIT )]
        [ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.UNRESTRICTED_VIEW )]
        [Route( """" )]
        [ProducesResponseType( HttpStatusCode.Created, Type = typeof( CreatedAtResponseBag ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PostItem( [FromBody] {modelTypeFullName} value )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Create( value );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PutItem action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePutItemAction( string modelTypeFullName, Guid actionGuid )
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
        [Secured( Security.Authorization.EDIT )]
        [ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.UNRESTRICTED_VIEW )]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PutItem( string id, [FromBody] {modelTypeFullName} value )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Update( id, value );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PatchItem action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePatchItemAction( string modelTypeFullName, Guid actionGuid )
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
        [Secured( Security.Authorization.EDIT )]
        [ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.UNRESTRICTED_VIEW )]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PatchItem( string id, [FromBody] Dictionary<string, object> values )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Patch( id, values );
        }}
";
        }

        /// <summary>
        /// Generates the content of the DeleteItem action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateDeleteItemAction( string modelTypeFullName, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Deletes a single item from the database.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An empty response.</returns>
        [HttpDelete]
        [Authenticate]
        [Secured( Security.Authorization.EDIT )]
        [ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.UNRESTRICTED_VIEW )]
        [Route( ""{{id}}"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult DeleteItem( string id )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Delete( id );
        }}
";
        }

        /// <summary>
        /// Generates the content of the GetAttributeValues action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateGetAttributeValuesAction( string modelTypeFullName, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Gets all the attribute values for the specified item.
        /// </summary>
        /// <param name=""id"">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An array of objects that represent all the attribute values.</returns>
        [HttpGet]
        [Authenticate]
        [Secured( Security.Authorization.VIEW )]
        [ExcludeSecurityActions( Security.Authorization.EDIT, Security.Authorization.UNRESTRICTED_EDIT )]
        [Route( ""{{id}}/attributevalues"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Dictionary<string, ModelAttributeValueBag> ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult GetAttributeValues( string id )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).GetAttributeValues( id );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PatchAttributeValues action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePatchAttributeValuesAction( string modelTypeFullName, Guid actionGuid )
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
        [Secured( Security.Authorization.EDIT )]
        [ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.UNRESTRICTED_VIEW )]
        [Route( ""{{id}}/attributevalues"" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PatchAttributeValues( string id, [FromBody] Dictionary<string, string> values )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).PatchAttributeValues( id, values );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PostSearch action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePostSearchAction( string modelTypeFullName, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a search of items using the specified user query.
        /// </summary>
        /// <param name=""query"">Query options to be applied.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpPost]
        [Authenticate]
        [Secured( Security.Authorization.VIEW )]
        [ExcludeSecurityActions( Security.Authorization.EDIT, Security.Authorization.UNRESTRICTED_EDIT )]
        [Route( ""search"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PostSearch( [FromBody] EntitySearchQueryBag query )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Search( query );
        }}
";
        }

        /// <summary>
        /// Generates the content of the GetSearch action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GenerateGetSearchByKeyAction( string modelTypeFullName, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a search of items using the specified system query.
        /// </summary>
        /// <param name=""searchKey"">The key that identifies the entity search query to execute.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpGet]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.EDIT, Security.Authorization.UNRESTRICTED_VIEW, Security.Authorization.UNRESTRICTED_EDIT )]
        [Route( ""search/{{searchKey}}"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult GetSearchByKey( string searchKey )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Search( searchKey, null );
        }}
";
        }

        /// <summary>
        /// Generates the content of the PostSearch action.
        /// </summary>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <returns>A string that contains the content for the action.</returns>
        private static string GeneratePostSearchByKeyAction( string modelTypeFullName, Guid actionGuid )
        {
            return $@"        /// <summary>
        /// Performs a search of items using the specified system query.
        /// </summary>
        /// <param name=""query"">Additional query refinement options to be applied.</param>
        /// <param name=""searchKey"">The key that identifies the entity search query to execute.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpPost]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.EDIT, Security.Authorization.UNRESTRICTED_VIEW, Security.Authorization.UNRESTRICTED_EDIT )]
        [Route( ""search/{{searchKey}}"" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( ""{actionGuid}"" )]
        public IActionResult PostSearchByKey( [FromBody] EntitySearchQueryBag query, string searchKey )
        {{
            return new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this ).Search( searchKey, query );
        }}
";
        }

        /// <summary>
        /// Generates a version 5 UUID based on the namespace and name. These
        /// are essentially deterministic UUIDs so that we always get the same
        /// return value given the same <paramref name="namespaceGuid"/> and
        /// <paramref name="name"/>. This way different runs of the generator
        /// will always use the same Guid values.
        /// </summary>
        /// <remarks>
        /// Taken from MIT licensed: https://github.com/microsoft/PowerToys/blob/a720dd537c25d076b4395756a097309b50a04bc6/src/modules/launcher/Plugins/Community.PowerToys.Run.Plugin.ValueGenerator/Generators/GUID/GUIDGenerator.cs
        /// </remarks>
        /// <param name="namespaceGuid">The <see cref="Guid"/> that acts as the namespace.</param>
        /// <param name="name">The unique name within <paramref name="namespaceGuid"/>.</param>
        /// <returns>A new guid that represents <paramref name="name"/>.</returns>
        private static Guid NewV5Guid( Guid namespaceGuid, string name )
        {
            byte[] namespaceBytes = namespaceGuid.ToByteArray();
            byte[] networkEndianNamespaceBytes = namespaceBytes;

            // Convert time_low, time_mid and time_hi_and_version to network order
            int time_low = IPAddress.HostToNetworkOrder( BitConverter.ToInt32( networkEndianNamespaceBytes, 0 ) );
            short time_mid = IPAddress.HostToNetworkOrder( BitConverter.ToInt16( networkEndianNamespaceBytes, 4 ) );
            short time_hi_and_version = IPAddress.HostToNetworkOrder( BitConverter.ToInt16( networkEndianNamespaceBytes, 6 ) );

            Buffer.BlockCopy( BitConverter.GetBytes( time_low ), 0, networkEndianNamespaceBytes, 0, 4 );
            Buffer.BlockCopy( BitConverter.GetBytes( time_mid ), 0, networkEndianNamespaceBytes, 4, 2 );
            Buffer.BlockCopy( BitConverter.GetBytes( time_hi_and_version ), 0, networkEndianNamespaceBytes, 6, 2 );

            byte[] nameBytes = Encoding.ASCII.GetBytes( name );

            byte[] namespaceAndNameBytes = new byte[networkEndianNamespaceBytes.Length + nameBytes.Length];
            Buffer.BlockCopy( networkEndianNamespaceBytes, 0, namespaceAndNameBytes, 0, namespaceBytes.Length );
            Buffer.BlockCopy( nameBytes, 0, namespaceAndNameBytes, networkEndianNamespaceBytes.Length, nameBytes.Length );

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
            var hash = SHA1.Create().ComputeHash( namespaceAndNameBytes );
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms

            byte[] result = new byte[16];

            // Copy first 16-bytes of the hash into our Uuid result
            Buffer.BlockCopy( hash, 0, result, 0, 16 );

            // Convert put time_low, time_mid and time_hi_and_version back to host order
            time_low = IPAddress.NetworkToHostOrder( BitConverter.ToInt32( result, 0 ) );
            time_mid = IPAddress.NetworkToHostOrder( BitConverter.ToInt16( result, 4 ) );
            time_hi_and_version = IPAddress.NetworkToHostOrder( BitConverter.ToInt16( result, 6 ) );

            // Set version 'version' in time_hi_and_version field according to https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.3
            time_hi_and_version &= 0x0FFF;
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            time_hi_and_version = ( short ) ( time_hi_and_version | ( 5 << 12 ) );
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

            Buffer.BlockCopy( BitConverter.GetBytes( time_low ), 0, result, 0, 4 );
            Buffer.BlockCopy( BitConverter.GetBytes( time_mid ), 0, result, 4, 2 );
            Buffer.BlockCopy( BitConverter.GetBytes( time_hi_and_version ), 0, result, 6, 2 );

            // Set upper two bits to "10"
            result[8] &= 0x3F;
            result[8] |= 0x80;

            return new Guid( result );
        }
    }
}
