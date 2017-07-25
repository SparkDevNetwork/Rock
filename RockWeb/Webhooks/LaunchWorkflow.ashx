<%@ WebHandler Language="C#" Class="LaunchWorkflow" %>
// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Web;
using Rock.Data;
using Rock.Model;
using System.Linq;
using Rock;
using Rock.Web.Cache;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Xml;

/// <summary>
/// A webhook for launching a workflow. Does basic decoding of FORM data
/// and JSON data and provides basic HttpRequest information to the Workflow.
/// </summary>
public class LaunchWorkflow : IHttpHandler
{
    /// <summary>
    /// Process the incoming http request. This is the web handler entry point.
    /// </summary>
    /// <param name="context">The context that contains all information about this request.</param>
    public void ProcessRequest( HttpContext context )
    {
        try
        {
            // Convert the context values to a dictionary
            var mergeFields = RequestToDictionary( context );

            // Find the valid hooks for this request
            var hooks = GetHooksForRequest( mergeFields );
            if ( hooks.Any() )
            {
                foreach ( var hook in hooks )
                {
                    Guid guid = hook.GetAttributeValue( "WorkflowType" ).AsGuid();


                    WorkflowTypeCache workflowType = WorkflowTypeCache.Read( guid );
                    if ( workflowType != null )
                    {
                        Workflow workflow = Workflow.Activate( workflowType, context.Request.UserHostName );

                        // We found a workflow type and were able to instantiate a new one.
                        if ( workflow != null )
                        {
                            // Load in all the attributes that are defined in the workflow.
                            workflow.LoadAttributes();

                            mergeFields.AddOrReplace( "hook", hook );
                            PopulateWorkflowAttributes( workflow, hook, mergeFields );

                            // Execute the workflow.
                            using ( var rockContext = new RockContext() )
                            {
                                List<string> errorMessages;
                                new WorkflowService( rockContext ).Process( workflow, out errorMessages );

                                // We send a response (if one is available) wether the workflow has ended
                                // or not. This gives them a chance to send a "let me work on that for you"
                                // type response and then continue processing in the background.
                                SetWorkflowResponse( context, workflow );
                            }
                        }
                    }
                }

                context.Response.End();
                return;
            }

            // If we got here then something went wrong, probably we couldn't find a matching hook.
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 404;
            context.Response.Write( "Path not found." );
            context.Response.End();
        }
        catch ( Exception e )
        {
            WriteToLog( e.Message );
        }
    }

    /// <summary>
    /// These webhooks are not reusable and must only be used once.
    /// </summary>
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

    #region SuMethods

    /// <summary>
    /// Retrieve the DefinedValueCache for this request by matching the Method, Url
    /// and any other filters defined by subclasses.
    /// </summary>
    /// <param name="requestDict">The request dictionary.</param>
    /// <returns>
    /// A DefinedValue for the webhook request that was matched or null if one was not found.
    /// </returns>
    protected List<DefinedValueCache> GetHooksForRequest( Dictionary<string, object> requestDict )
    {
        var hooks = new List<DefinedValueCache>();

        var dt = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW.AsGuid() );
        if ( dt != null )
        {
            foreach ( DefinedValueCache hook in dt.DefinedValues.OrderBy( h => h.Order ) )
            {
                if ( hook.GetAttributeValue( "ProcessRequest" ).ResolveMergeFields( requestDict ).Trim().AsBoolean() )
                {
                    hooks.Add( hook );
                }
            }
        }

        return hooks;
    }

    /// <summary>
    /// Populates any defined Workflow attributes specified to this webhook type.
    /// Subclasses may call the base method to have the Request attribute set.
    /// </summary>
    /// <param name="workflow">The workflow whose attributes need to be set.</param>
    /// <param name="hook">The DefinedValue of the currently executing webhook.</param>
    /// <param name="mergeFields">The merge fields.</param>
    protected void PopulateWorkflowAttributes( Workflow workflow, DefinedValueCache hook, Dictionary<string, object> mergeFields )
    {
        // Set workflow attributes
        string workflowAttributes = hook.GetAttributeValue( "WorkflowAttributes" );
        string[] attributes = workflowAttributes.Split( '|' );
        foreach ( string attribute in attributes )
        {
            if ( attribute.Contains( '^' ) )
            {
                string[] settings = attribute.Split( '^' );
                workflow.SetAttributeValue( settings[0], settings[1].ResolveMergeFields( mergeFields ) );
            }
        }

        // set workflow name
        string nameTemplate = hook.GetAttributeValue( "WorkflowNameTemplate" ).ResolveMergeFields( mergeFields );
        if ( nameTemplate.IsNotNullOrWhitespace() )
        {
            workflow.Name = nameTemplate.ResolveMergeFields( mergeFields );
        }
    }

    /// <summary>
    /// Convert the request into a generic JSON object that can provide information
    /// to the workflow. If a subclass does needs to customize this data they can
    /// call the base method and then modify the content before returning it.
    /// </summary>
    /// <param name="hook">The DefinedValue of the currently executing webhook.</param>
    /// <returns></returns>
    protected Dictionary<string, object> RequestToDictionary( HttpContext httpContext )
    {
        var dictionary = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

        // Set the standard values to be used.
        dictionary.Add( "Url", "/" + string.Join( "", httpContext.Request.Url.Segments.SkipWhile( s => !s.EndsWith( ".ashx", StringComparison.InvariantCultureIgnoreCase ) && !s.EndsWith( ".ashx/", StringComparison.InvariantCultureIgnoreCase ) ).Skip( 1 ).ToArray() ) );
        dictionary.Add( "RawUrl", httpContext.Request.Url.AbsoluteUri );
        dictionary.Add( "Method", httpContext.Request.HttpMethod );
        dictionary.Add( "QueryString", httpContext.Request.QueryString.Cast<string>().ToDictionary( q => q, q => httpContext.Request.QueryString[q] ) );
        dictionary.Add( "RemoteAddress", httpContext.Request.UserHostAddress );
        dictionary.Add( "RemoteName", httpContext.Request.UserHostName );
        dictionary.Add( "ServerName", httpContext.Request.Url.Host );
        dictionary.Add( "ContentType", httpContext.Request.ContentType );

        // Add in the raw body content.
        using ( StreamReader reader = new StreamReader( httpContext.Request.InputStream, Encoding.UTF8 ) )
        {
            dictionary.Add( "RawBody", reader.ReadToEnd() );
        }

        // Parse the body content if it is JSON or standard Form data.
        if ( httpContext.Request.ContentType == "application/json" )
        {
            try
            {
                dictionary.Add( "Body", Newtonsoft.Json.JsonConvert.DeserializeObject( (string)dictionary["RawBody"] ) );
            }
            catch
            {
            }
        }
        else if ( httpContext.Request.ContentType == "application/x-www-form-urlencoded" )
        {
            try
            {
                dictionary.Add( "Body", httpContext.Request.Form.Cast<string>().ToDictionary( q => q, q => httpContext.Request.Form[q] ) );
            }
            catch
            {
            }
        }
        else if ( httpContext.Request.ContentType == "application/xml" )
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml( (string)dictionary["RawBody"] );
                string jsonText = JsonConvert.SerializeXmlNode( doc );
                dictionary.Add( "Body", Newtonsoft.Json.JsonConvert.DeserializeObject( ( jsonText ) ) );
            }
            catch
            {
            }
        }

        // Add the headers
        var headers = httpContext.Request.Headers.Cast<string>()
            .Where( h => !h.Equals( "Authorization", StringComparison.InvariantCultureIgnoreCase ) )
            .Where( h => !h.Equals( "Cookie", StringComparison.InvariantCultureIgnoreCase ) )
            .ToDictionary( h => h, h => httpContext.Request.Headers[h] );
        dictionary.Add( "Headers", headers );

        // Add the cookies
        dictionary.Add( "Cookies", httpContext.Request.Cookies.Cast<string>().ToDictionary( q => q, q => httpContext.Request.Cookies[q].Value ) );

        return dictionary;
    }

    /// <summary>
    /// Sets an optional response to the current request.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="workflow">The workflow that can the response data.</param>
    protected void SetWorkflowResponse( HttpContext httpContext, Workflow workflow )
    {
        string response = workflow.GetAttributeValue( "WebhookResponse" );
        string contentType = workflow.GetAttributeValue( "WebhookResponseContentType" );

        if ( response.IsNotNullOrWhitespace() )
        {
            httpContext.Response.Write( response );
            httpContext.Response.ContentType = contentType.IsNotNullOrWhitespace() ? contentType : "text/plain";
        }
    }

    /// <summary>
    /// Log a message to the WebhookToWorkflow.txt file. The message is prefixed by
    /// the date and the class name.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    protected void WriteToLog( string message )
    {
        string logFile = HttpContext.Current.Server.MapPath( "~/App_Data/Logs/LaunchWorkflow.txt" );

        // Write to the log, but if an ioexception occurs wait a couple seconds and then try again (up to 3 times).
        var maxRetry = 3;
        for ( int retry = 0; retry < maxRetry; retry++ )
        {
            try
            {
                using ( FileStream fs = new FileStream( logFile, FileMode.Append, FileAccess.Write ) )
                {
                    using ( StreamWriter sw = new StreamWriter( fs ) )
                    {
                        sw.WriteLine( string.Format( "{0} [{2}] - {1}", RockDateTime.Now.ToString(), message, GetType().Name ) );
                        break;
                    }
                }
            }
            catch ( IOException )
            {
                if ( retry < maxRetry - 1 )
                {
                    System.Threading.Thread.Sleep( 2000 );
                }
            }
        }

    }

    #endregion

}