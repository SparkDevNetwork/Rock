//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

public partial class error : System.Web.UI.Page
{
    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void Page_Load( object sender, EventArgs e )
    {
        logoImg.Src = ResolveUrl( "~/Assets/Images/rock-logo.svg" );
        
        // get error level
        int errorLevel = 0;

        if ( Request["error"] != null )
            errorLevel = Int32.Parse( Request["error"].ToString() );

        if ( errorLevel == 1 )
        {
            // check to see if the user is an admin, if so allow them to view the error details
            var userLogin = Rock.Model.UserLoginService.GetCurrentUser();

            GroupService service = new GroupService();
            Group adminGroup = service.GetByGuid( new Guid( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS ) );

            if ( userLogin != null && adminGroup.Members.Where( m => m.PersonId == userLogin.PersonId ).Count() > 0 )
            {
                // is an admin
                lErrorInfo.Text = "<h3>Exception Log:</h3>";

                // get exception from Session
                if ( Session["Exception"] != null )
                {
                    ProcessException( (Exception)Session["Exception"], " " );
                }
            }
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