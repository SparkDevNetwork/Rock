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
using System.ComponentModel;
using System.ComponentModel.Composition;

using RestSharp;
using RestSharp.Authenticators;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;


namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends Web request
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Makes web requests to the enpoint of your choice." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Web Request" )]

    [CustomDropdownListField( "Method", "HTTP method to use when making requests.", "GET,POST,PUT,DELETE,PATCH", true, "GET", "", 0 )]
    [TextField( "URL", "Sets the BaseUrl property for requests made by this client instance  <span class='tip tip-lava'></span>", true, key: "Url", order: 1 )]
    [KeyValueListField( "Parameters", "The parameters to send with request. <span class='tip tip-lava'></span>", false, "", "Parameters", "", order: 2 )]
    [KeyValueListField( "Headers", "The key value pairs to add in the http header. <span class='tip tip-lava'></span>", false, "", "Headers", "", order: 3 )]
    [TextField( "Basic Auth UserName", "The user name for basic http authentication.", false, "", "", 4 )]
    [TextField( "Basic Auth Password", "The password for basic http authentication.", false, "", "", 5, isPassword: true )]
    [EnumField( "Request Content Type", "", typeof( RequestContentType ), true, "0", order: 6 )]
    [EnumField( "Response Content Type", "", typeof( ResponseContentType ), true, "0", order: 7 )]
    [CodeEditorField( "Body", "The body to send with the request. <span class='tip tip-lava'></span>", Web.UI.Controls.CodeEditorMode.Lava, Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 8 )]
    [WorkflowAttribute( "Response Attribute", "An attribute to set to the response from web request.", false, "", "", 9 )]
    public class WebRequest : ActionComponent
    {
        private const string METHOD = "Method";
        private const string URL = "Url";
        private const string PARAMETERS = "Parameters";
        private const string HEADERS = "Headers";
        private const string BASIC_AUTH_USERNAME = "BasicAuthUserName";
        private const string BASIC_AUTH_PASSWORD = "BasicAuthPassword";
        private const string BODY = "Body";
        private const string REQUEST_CONTENT_TYPE = "RequestContentType";
        private const string RESPONSE_CONTENT_TYPE = "ResponseContentType";
        private const string RESPONSE_ATTRIBUTE = "ResponseAttribute";

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var mergeFields = GetMergeFields( action );

            string method = GetAttributeValue( action, METHOD );
            string url = GetAttributeValue( action, URL ).ResolveMergeFields( mergeFields );

            var parametersValue = GetAttributeValue( action, PARAMETERS );
            var parameterList = new Field.Types.KeyValueListFieldType().GetValuesFromString( null, parametersValue, null, false );
            var parameters = new Dictionary<string, object>();
            foreach ( var p in parameterList )
            {
                var value = p.Value != null ? p.Value.ToString().ResolveMergeFields( mergeFields ) : null;

                parameters.AddOrReplace( p.Key, value );
            }

            var headersValue = GetAttributeValue( action, HEADERS );
            var headerList = new Field.Types.KeyValueListFieldType().GetValuesFromString( null, headersValue, null, false );
            var headers = new Dictionary<string, object>();
            foreach ( var p in headerList )
            {
                var value = p.Value != null ? p.Value.ToString().ResolveMergeFields( mergeFields ) : null;

                headers.AddOrReplace( p.Key, value );
            }

            string basicAuthUserName = GetAttributeValue( action, BASIC_AUTH_USERNAME );
            string basicAuthPassword = GetAttributeValue( action, BASIC_AUTH_PASSWORD );


            string body = GetAttributeValue( action, BODY ).ResolveMergeFields( mergeFields );

            var requestContentType = this.GetAttributeValue( action, REQUEST_CONTENT_TYPE ).ConvertToEnum<RequestContentType>( RequestContentType.JSON );
            var responseContentType = this.GetAttributeValue( action, RESPONSE_CONTENT_TYPE ).ConvertToEnum<ResponseContentType>( ResponseContentType.JSON );

            if ( !string.IsNullOrWhiteSpace( url ) )
            {
                var client = new RestClient( url );

                var request = new RestRequest( method.ToUpper().ConvertToEnum<Method>( Method.GET ) );
                client.Timeout = 12000;

                // handle basic auth
                if ( !string.IsNullOrEmpty( basicAuthUserName ) && !string.IsNullOrEmpty( basicAuthPassword ) )
                {
                    client.Authenticator = new HttpBasicAuthenticator( basicAuthUserName, basicAuthPassword );
                }

                foreach ( var parameter in parameters )
                {
                    request.AddParameter( parameter.Key, parameter.Value );
                }

                // add headers
                foreach ( var header in headers )
                {
                    request.AddHeader( header.Key, header.Value.ToString() );
                }

                if ( !string.IsNullOrWhiteSpace( body ) )
                {
                    if ( requestContentType == RequestContentType.JSON )
                    {
                        request.RequestFormat = DataFormat.Json;
                    }
                    else
                    {
                        request.RequestFormat = DataFormat.Xml;
                    }
                    request.AddParameter( requestContentType.ToString(), body, ParameterType.RequestBody );
                }

                switch ( responseContentType )
                {
                    case ResponseContentType.JSON:
                        {
                            request.AddHeader( "Accept", "application/json" );
                        }
                        break;
                    case ResponseContentType.XML:
                        {
                            request.AddHeader( "Accept", "application/xml" );
                        }
                        break;
                }


                IRestResponse response = client.Execute( request );
                var responseString = response.Content;

                var attribute = SetWorkflowAttributeValue( action, RESPONSE_ATTRIBUTE, responseString );
                if ( attribute != null )
                {
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, responseString ) );
                }

            }
            return true;
        }
    }

    #region Helper Classes

    /// <summary>
    /// Request Content Type
    /// </summary>
    public enum RequestContentType
    {
        /// <summary>
        /// JSON
        /// </summary>
        JSON = 0,

        /// <summary>
        /// XML
        /// </summary>
        XML = 1,
    }

    /// <summary>
    /// Response Content Type
    /// </summary>
    public enum ResponseContentType
    {
        /// <summary>
        /// JSON
        /// </summary>
        JSON = 0,

        /// <summary>
        /// XML
        /// </summary>
        XML = 1,

        /// <summary>
        /// HTML
        /// </summary>
        HTML = 2
    }

    #endregion
}
