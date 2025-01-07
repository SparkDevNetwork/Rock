using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Rock.CodeGeneration.Utility;
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
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        /// <returns>A string that contains the file content that should be written to disk.</returns>
        public string GenerateStandardFileContent( string modelTypeName, string modelTypeFullName, CodeGenerateRestEndpoint endpoints, bool disableEntitySecurity )
        {
            var modelControllerNamespaceGuid = new Guid( "d9b3e947-5c19-4145-a356-712851d544de" );
            var usings = new List<string>();
            var usingMaps = new List<(string Name, string Source)>();
            var codeBuilder = new IndentedStringBuilder();
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
            codeBuilder.AppendLine( "#if WEBFORMS" );
            foreach ( var (name, source) in usingMaps )
            {
                codeBuilder.AppendLine( $"    using {name} = {source};" );
            }
            codeBuilder.AppendLine( "#endif" );
            codeBuilder.AppendLine();

            // Emit the class definition.
            var routePrefix = $"{ControllerRoutePrefix}/{modelTypeName.Pluralize().ToLower()}";
            var controllerGuid = NewV5Guid( modelControllerNamespaceGuid, routePrefix );
            codeBuilder.Indent();
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( $"/// Provides data API endpoints for {modelTypeName.Pluralize().SplitCase()}." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( $"[RoutePrefix( \"{routePrefix}\" )]" );

            if ( !disableEntitySecurity )
            {
                codeBuilder.AppendLine( "[SecurityAction( Security.Authorization.EXECUTE_READ, \"Allows execution of API endpoints in the context of reading data.\" )]" );
                codeBuilder.AppendLine( "[SecurityAction( Security.Authorization.EXECUTE_WRITE, \"Allows execution of API endpoints in the context of writing data.\" )]" );
            }

            codeBuilder.AppendLine( "[SecurityAction( Security.Authorization.EXECUTE_UNRESTRICTED_READ, \"Allows execution of API endpoints in the context of reading data without performing per-entity security checks.\" )]" );
            codeBuilder.AppendLine( "[SecurityAction( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE, \"Allows execution of API endpoints in the context of writing data without performing per-entity security checks.\" )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.VIEW, Security.Authorization.EDIT )]" );

            codeBuilder.AppendLine( $"[Rock.SystemGuid.RestControllerGuid( \"{controllerGuid}\" )]" );
            codeBuilder.AppendLine( $"public partial class {modelTypeName.Pluralize()}Controller : ApiControllerBase" );
            codeBuilder.AppendLine( "{" );

            var isFirstAction = true;

            void AppendLineIfNotFirstAction()
            {
                if ( isFirstAction )
                {
                    isFirstAction = false;
                }
                else
                {
                    codeBuilder.AppendLine();
                }
            }

            if ( hasGetItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "GetItem" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GenerateGetItemAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            if ( hasPostItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PostItem" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GeneratePostItemAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            if ( hasPutItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PutItem" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GeneratePutItemAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            if ( hasPatchItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PatchItem" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GeneratePatchItemAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            if ( hasDeleteItem )
            {
                var actionGuid = NewV5Guid( controllerGuid, "DeleteItem" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GenerateDeleteItemAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            if ( hasGetAttributeValues )
            {
                var actionGuid = NewV5Guid( controllerGuid, "GetAttributeValues" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GenerateGetAttributeValuesAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            if ( hasPatchAttributeValues )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PatchAttributeValues" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GeneratePatchAttributeValuesAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            if ( hasSearch )
            {
                var actionGuid = NewV5Guid( controllerGuid, "PostSearch" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GeneratePostSearchAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );

                actionGuid = NewV5Guid( controllerGuid, "GetSearchByKey" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GenerateGetSearchByKeyAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );

                actionGuid = NewV5Guid( controllerGuid, "PostSearchByKey" );

                AppendLineIfNotFirstAction();
                codeBuilder.Indent( () => GeneratePostSearchByKeyAction( codeBuilder, modelTypeFullName, actionGuid, disableEntitySecurity ) );
            }

            codeBuilder.AppendLine( "}" );

            return GenerateCSharpFile( usings, NamespaceName, codeBuilder.ToString() );
        }

        /// <summary>
        /// Generates the content of the GetItem action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GenerateGetItemAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            var securityAction = disableEntitySecurity ? "EXECUTE_UNRESTRICTED_READ" : "EXECUTE_READ";

            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Gets a single item from the database." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"id\">The identifier as either an Id, Guid or IdKey value.</param>" );
            codeBuilder.AppendLine( "/// <returns>The requested item.</returns>" );
            codeBuilder.AppendLine( "[HttpGet]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( $"[Secured( Security.Authorization.{securityAction} )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]" );
            codeBuilder.AppendLine( "[Route( \"{id}\" )]" );
            codeBuilder.AppendLine( $"[ProducesResponseType( HttpStatusCode.OK, Type = typeof( {modelTypeFullName} ) )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult GetItem( string id )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( "return helper.Get( id );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the PostItem action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GeneratePostItemAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Creates a new item in the database." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"value\">The item to be created.</param>" );
            codeBuilder.AppendLine( "/// <returns>An object that contains the new identifier values.</returns>" );
            codeBuilder.AppendLine( "[HttpPost]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_WRITE )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]" );
            codeBuilder.AppendLine( "[Route( \"\" )]");
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Created, Type = typeof( CreatedAtResponseBag ) )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( $"public IActionResult PostItem( [FromBody] {modelTypeFullName} value )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.Create( value );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the PutItem action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GeneratePutItemAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Performs a full update of the item. All property values must be" );
            codeBuilder.AppendLine( "/// specified." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"id\">The identifier as either an Id, Guid or IdKey value.</param>" );
            codeBuilder.AppendLine( "/// <param name=\"value\">The item that represents all the new values.</param>" );
            codeBuilder.AppendLine( "/// <returns>An empty response.</returns>" );
            codeBuilder.AppendLine( "[HttpPut]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_WRITE )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]" );
            codeBuilder.AppendLine( "[Route( \"{id}\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NoContent )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( $"public IActionResult PutItem( string id, [FromBody] {modelTypeFullName} value )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.Update( id, value );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the PatchItem action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GeneratePatchItemAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Performs a partial update of the item. Only specified property keys" );
            codeBuilder.AppendLine( "/// will be updated." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"id\">The identifier as either an Id, Guid or IdKey value.</param>" );
            codeBuilder.AppendLine( "/// <param name=\"values\">An object that identifies the properties and values to be updated.</param>" );
            codeBuilder.AppendLine( "/// <returns>An empty response.</returns>" );
            codeBuilder.AppendLine( "[HttpPatch]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_WRITE )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]" );
            codeBuilder.AppendLine( "[Route( \"{id}\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NoContent )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult PatchItem( string id, [FromBody] Dictionary<string, object> values )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.Patch( id, values );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the DeleteItem action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GenerateDeleteItemAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Deletes a single item from the database." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"id\">The identifier as either an Id, Guid or IdKey value.</param>" );
            codeBuilder.AppendLine( "/// <returns>An empty response.</returns>" );
            codeBuilder.AppendLine( "[HttpDelete]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_WRITE )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]" );
            codeBuilder.AppendLine( "[Route( \"{id}\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NoContent )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult DeleteItem( string id )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.Delete( id );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the GetAttributeValues action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GenerateGetAttributeValuesAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Gets all the attribute values for the specified item." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"id\">The identifier as either an Id, Guid or IdKey value.</param>" );
            codeBuilder.AppendLine( "/// <returns>An array of objects that represent all the attribute values.</returns>" );
            codeBuilder.AppendLine( "[HttpGet]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_READ )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]" );
            codeBuilder.AppendLine( "[Route( \"{id}/attributevalues\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.OK, Type = typeof( Dictionary<string, ModelAttributeValueBag> ) )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult GetAttributeValues( string id )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.GetAttributeValues( id );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the PatchAttributeValues action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GeneratePatchAttributeValuesAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Performs a partial update of attribute values for the item. Only" );
            codeBuilder.AppendLine( "/// attributes specified will be updated." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"id\">The identifier as either an Id, Guid or IdKey value.</param>" );
            codeBuilder.AppendLine( "/// <param name=\"values\">An object that identifies the attribute keys and raw values to be updated.</param>" );
            codeBuilder.AppendLine( "/// <returns>An empty response.</returns>" );
            codeBuilder.AppendLine( "[HttpPatch]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_WRITE )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]" );
            codeBuilder.AppendLine( "[Route( \"{id}/attributevalues\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NoContent )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult PatchAttributeValues( string id, [FromBody] Dictionary<string, string> values )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.PatchAttributeValues( id, values );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the PostSearch action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GeneratePostSearchAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Performs a search of items using the specified user query." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"query\">Query options to be applied.</param>" );
            codeBuilder.AppendLine( "/// <returns>An array of objects returned by the query.</returns>" );
            codeBuilder.AppendLine( "[HttpPost]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_UNRESTRICTED_READ )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]" );
            codeBuilder.AppendLine( "[Route( \"search\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult PostSearch( [FromBody] EntitySearchQueryBag query )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.Search( query );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the GetSearch action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GenerateGetSearchByKeyAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Performs a search of items using the specified system query." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"searchKey\">The key that identifies the entity search query to execute.</param>" );
            codeBuilder.AppendLine( "/// <returns>An array of objects returned by the query.</returns>" );
            codeBuilder.AppendLine( "[HttpGet]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_READ )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]" );
            codeBuilder.AppendLine( "[Route( \"search/{searchKey}\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult GetSearchByKey( string searchKey )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.Search( searchKey, null );" );
            } );
            codeBuilder.AppendLine( "}" );
        }

        /// <summary>
        /// Generates the content of the PostSearch action.
        /// </summary>
        /// <param name="codeBuilder">The builder to write the method declaration to.</param>
        /// <param name="modelTypeFullName">The full namespace and name of the C# class.</param>
        /// <param name="actionGuid">The unique identifier to use for the action.</param>
        /// <param name="disableEntitySecurity">If <c>true</c> then entity security will not be used for these endpoints.</param>
        private static void GeneratePostSearchByKeyAction( IndentedStringBuilder codeBuilder, string modelTypeFullName, Guid actionGuid, bool disableEntitySecurity )
        {
            codeBuilder.AppendLine( "/// <summary>" );
            codeBuilder.AppendLine( "/// Performs a search of items using the specified system query." );
            codeBuilder.AppendLine( "/// </summary>" );
            codeBuilder.AppendLine( "/// <param name=\"query\">Additional query refinement options to be applied.</param>" );
            codeBuilder.AppendLine( "/// <param name=\"searchKey\">The key that identifies the entity search query to execute.</param>" );
            codeBuilder.AppendLine( "/// <returns>An array of objects returned by the query.</returns>" );
            codeBuilder.AppendLine( "[HttpPost]" );
            codeBuilder.AppendLine( "[Authenticate]" );
            codeBuilder.AppendLine( "[Secured( Security.Authorization.EXECUTE_READ )]" );
            codeBuilder.AppendLine( "[ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]" );
            codeBuilder.AppendLine( "[Route( \"search/{searchKey}\" )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.BadRequest )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.NotFound )]" );
            codeBuilder.AppendLine( "[ProducesResponseType( HttpStatusCode.Unauthorized )]" );
            codeBuilder.AppendLine( $"[SystemGuid.RestActionGuid( \"{actionGuid}\" )]" );
            codeBuilder.AppendLine( "public IActionResult PostSearchByKey( string searchKey, [FromBody] EntitySearchQueryBag query )" );
            codeBuilder.AppendLine( "{" );
            codeBuilder.Indent( () =>
            {
                codeBuilder.AppendLine( $"var helper = new RestApiHelper<{modelTypeFullName}, {modelTypeFullName}Service>( this );" );
                codeBuilder.AppendLine();

                if ( disableEntitySecurity )
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = true;" );
                }
                else
                {
                    codeBuilder.AppendLine( "helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );" );
                }

                codeBuilder.AppendLine();
                codeBuilder.AppendLine( $"return helper.Search( searchKey, query );" );
            } );
            codeBuilder.AppendLine( "}" );
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
