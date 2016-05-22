<%@ WebHandler Language="C#" Class="RockWeb.Webhooks.ProtectMyMinistry" %>
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace RockWeb.Webhooks
{
    /// <summary>
    /// Handles the background check results sent from Protect My Ministry
    /// </summary>
    public class ProtectMyMinistry : IHttpHandler
    {
        public void ProcessRequest( HttpContext context )
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/plain";

            if ( request.HttpMethod != "POST" )
            {
                response.Write( "Invalid request type." );
                return;
            }

            if ( request.Form["REQUEST"] != null )
            {
                try
                {
                    var rockContext = new Rock.Data.RockContext();

                    XDocument xResult = null;
                    string orderId = string.Empty;

                    xResult = XDocument.Parse( HttpUtility.UrlDecode( request.Form["REQUEST"] ) );

                    // Get the orderid from the XML
                    orderId = ( from o in xResult.Descendants( "OrderDetail" ) select (string)o.Attribute( "OrderId" ) ).FirstOrDefault() ?? "OrderIdUnknown";

                    if ( !string.IsNullOrEmpty( orderId ) && orderId != "OrderIdUnknown" )
                    {
                        // Find and update the associated workflow
                        var workflowService = new WorkflowService( rockContext );
                        var workflow = new WorkflowService( rockContext ).Get( orderId.AsInteger() );
                        if ( workflow != null && workflow.IsActive )
                        {
                            workflow.LoadAttributes();
                            
                            Rock.Security.BackgroundCheck.ProtectMyMinistry.SaveResults( xResult, workflow, rockContext );
                            
                            rockContext.WrapTransaction( () =>
                            {
                                rockContext.SaveChanges();
                                workflow.SaveAttributeValues( rockContext );
                                foreach ( var activity in workflow.Activities )
                                {
                                    activity.SaveAttributeValues( rockContext );
                                }
                            } );

                        }
                    }

                    try
                    {
                        // Return the success XML to PMM
                        XDocument xdocResult = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ),
                            new XElement( "OrderXML",
                                new XElement( "Success", "TRUE" ) ) );

                        response.StatusCode = 200;
                        response.ContentType = "text/xml";
                        response.AddHeader( "Content-Type", "text/xml" );
                        xdocResult.Save( response.OutputStream );
                    }
                    catch { }
                }
                catch ( SystemException ex )
                {
                    ExceptionLogService.LogException( ex, context );
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        
    }
}