// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace RockWeb
{
    public partial class Error : System.Web.UI.Page
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            lLogoSvg.Text = System.IO.File.ReadAllText( HttpContext.Current.Request.MapPath( "~/Assets/Images/rock-logo-sm.svg" ) );

            string errorType = Request["type"];
            if ( string.IsNullOrWhiteSpace( errorType ) )
            {
                errorType = "exception";
            }

            if ( errorType.Equals( "security", StringComparison.OrdinalIgnoreCase ) )
            {
                ShowSecurityError();
            }
            else
            {
                ShowException();
            }
        }

        private void ShowSecurityError()
        {
            pnlSecurity.Visible = true;
        }

        private void ShowException()
        {
            pnlException.Visible = true;

            // get error level
            int errorLevel = 0;

            if ( Request["error"] != null )
            {
                errorLevel = Int32.Parse( Request["error"].ToString() );
            }
            else
            {
                if ( Request.RequestContext.HttpContext.Items["error"] != null )
                {
                    errorLevel = Int32.Parse( Request.RequestContext.HttpContext.Items["error"].ToString() );
                }
            }


            switch ( errorLevel )
            {
                case 1:
                    // check to see if the user is an admin, if so allow them to view the error details
                    var userLogin = Rock.Model.UserLoginService.GetCurrentUser();

                    try
                    {
                        GroupService service = new GroupService( new RockContext() );
                        Group adminGroup = service.GetByGuid( new Guid( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS ) );

                        if ( userLogin != null && adminGroup.Members.Where( m => m.PersonId == userLogin.PersonId ).Count() > 0 )
                        {
                            // get exception from Session
                            if ( Session["Exception"] != null )
                            {
                                // is an admin
                                lErrorInfo.Text = "<h3>Exception Log:</h3>";

                                ProcessException( (Exception)Session["Exception"], " " );
                            }
                        }
                    }
                    catch { }

                    break;

                case 66: // massive errors from global.asax.cs or routehandler

                    if ( Session["Exception"] != null )
                    {
                        lErrorInfo.Text = "<h3>Exception Log:</h3>";
                        ProcessException( (Exception)Session["Exception"], " " );
                    }
                    else
                    {
                        if ( Request.RequestContext.HttpContext.Items["Exception"] != null )
                        {
                            lErrorInfo.Text = "<h3>Exception Log:</h3>";
                            ProcessException( (Exception)Request.RequestContext.HttpContext.Items["Exception"], " " );
                        }
                    }
                    break;
            }

            // clear session object
            Session.Remove( "Exception" );

        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="exLevel">The ex level.</param>
        private void ProcessException( Exception ex, string exLevel )
        {
            lErrorInfo.Text += "<div class=\"alert alert-danger\">";
            lErrorInfo.Text += "<h4>" + exLevel + ex.GetType().Name + " in " + ex.Source + "</h3>";
            lErrorInfo.Text += "<p><strong>Message</strong><br>" + ex.Message + "</p>";
            lErrorInfo.Text += "<p><strong>Stack Trace</strong><br><pre>" + ex.StackTrace + "</pre></p>";
            lErrorInfo.Text += "</div>";

            // check for inner exception
            if ( ex.InnerException != null )
            {
                //lErrorInfo.Text += "<p /><p />";
                ProcessException( ex.InnerException, "-" + exLevel );
            }
        }
    }
}