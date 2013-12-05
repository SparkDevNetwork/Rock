//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [LinkedPage("Detail Page")] 
    [BooleanField( "Show Cookies", "Show cookie information when block loads.", false )]
    [BooleanField( "Show Server Variables", "Show server variables when block loads.", false )]
    public partial class ExceptionDetail : RockBlock, IDetailBlock
    {

        #region Control Methods
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int exceptionId = 0;

                // set page title
                lPageTitle.Text = ("Exception Overview").FormatAsHtmlTitle();

                //sets icon and text for cookies and server variables check boxes
                chkShowCookies.Text = "<i class=\"fa fa-laptop\"> </i> Show Cookies";
                chkShowServerVariables.Text = "<i class=\"fa fa-hdd-o\"></i> Show Server Variables";

                //if ExceptionId is not null and is an integer
                if ( !String.IsNullOrWhiteSpace( PageParameter( "ExceptionId" ) ) && int.TryParse( PageParameter( "ExceptionId" ), out exceptionId ) )
                {
                    //show the detail
                    ShowDetail( "ExceptionId", exceptionId );
                }
                else
                {
                    pnlSummary.Visible = false;
                }
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Dissect the query string value and build unordered list
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>unordered list of query string values</returns>
        private string BuildQueryStringList( string queryString )
        {
            string[] queryStringVariables = queryString.Split( "&".ToCharArray() );

            StringBuilder qsList = new StringBuilder();
            qsList.Append("<ul type=\"disc\">");

            foreach ( string query in queryStringVariables )
            {
                string[] queryStringValue = query.Split( "=".ToCharArray() );

                if (queryStringValue.Length > 1)
                {
                    qsList.AppendFormat( "<li>{0}: {1}</li>", queryStringValue[0], queryStringValue[1] );
                }
                else
                {
                    qsList.AppendFormat( "<li>{0}</li>", queryStringValue[0] );
                }
            }

            qsList.Append( "</ul>" );

            return qsList.ToString();
        }

        /// <summary>
        /// Gets the related exception logs
        /// </summary>
        /// <param name="baseException">The base exception.</param>
        /// <returns>List of Exception Detail Summary objects</returns>
        private List<ExceptionLog> GetExceptionLogs( ExceptionLog baseException )
        {
            List<ExceptionLog> exceptionList = new List<ExceptionLog>();
            ExceptionLogService svc = new ExceptionLogService();

            //load the base exception 
            exceptionList.Add( baseException );
            
            //get the parentID
            int? parentId = baseException.ParentId;

            //loop through exception hierarchy (parent, grandparent, etc)
            while ( parentId != null && parentId > 0 )
            {
                var exception = svc.Get( (int)parentId );

                if ( exception != null )
                {
                    exceptionList.Add( exception );
                }

                parentId = exception.ParentId;
            }

            //get inner exceptions
            if ( baseException.HasInnerException != null &&  (bool)baseException.HasInnerException )
            {
                exceptionList.AddRange( svc.GetByParentId( baseException.Id ) );
            }

            return exceptionList;
        }

        protected string GetExceptionDetailUrl( int exceptionId )
        {
            return string.Format( "/page/{0}?ExceptionId={1}", RockPage.PageId, exceptionId );
        }

        /// <summary>
        /// Shows the detail of the exception
        /// </summary>
        /// <param name="itemKey">Item Key (should be ExceptionId in this instance).</param>
        /// <param name="itemKeyValue">Item key value (should be ExceptionId).</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            //if item key is not ExceptionId return
            if ( !itemKey.Equals( "ExceptionId" ) )
            {
                return;
            }

            //Get exception
            var baseException = new ExceptionLogService().Get( itemKeyValue );

            //set fields
            if ( baseException == null )
            {
                return;
            }

            DescriptionList dl = new DescriptionList();

            dl.Add( "Site", baseException.Site != null ? baseException.Site.Name : String.Empty, true );
            dl.Add( "Page", baseException.Page != null ? string.Format( "{0} <a href=\"{1}\" class=\"btn btn-mini\" target=\"_blank\"><i class=\"fa fa-arrow-right\"></i></a>", baseException.Page.Name, baseException.PageUrl ) : String.Empty, true );

            //If query string is not empty build query string list
            if ( !String.IsNullOrWhiteSpace( baseException.QueryString ) )
            {
                dl.Add( "Query String", BuildQueryStringList( baseException.QueryString ) );
            }

            dl.StartSecondColumn();


            dl.Add( "User", baseException.CreatedByPersonId != null ? baseException.CreatedByPerson.FullName : "Anonymous" );
            dl.Add( "Exception Date", string.Format( "{0:g}", baseException.ExceptionDateTime ) );
            lExceptionSummary.Text = dl.Html;

            lCookies.Text = baseException.Cookies;
            lServerVariables.Text = baseException.ServerVariables;

            //check to see if Show Cookies attribute is true
            if( Convert.ToBoolean( GetAttributeValue( "ShowCookies" ) ) )
            {
                //if so check check box and register script to show cookies div
                chkShowCookies.Checked = true;
                ScriptManager.RegisterStartupScript( upExcpetionDetail, upExcpetionDetail.GetType(), "ShowCookiesOnLoad" + DateTime.Now.Ticks, "$(\"#divCookies\").css(\"display\", \"inherit\");", true );
            }
            else
            {
                chkShowCookies.Checked = false;
            }

            //check to see if show server variables attribute is checked
            if ( Convert.ToBoolean( GetAttributeValue( "ShowServerVariables" ) ) )
            {
                chkShowServerVariables.Checked = true;
                ScriptManager.RegisterStartupScript( upExcpetionDetail, upExcpetionDetail.GetType(), "ShowServerVariablesOnLoad" + DateTime.Now.Ticks, "$(\"#divServerVariables\").css(\"display\", \"inherit\");", true );
            }
            else
            {
                chkShowServerVariables.Checked = false;
            }

            rptExcpetionDetails.DataSource = GetExceptionLogs( baseException ).OrderBy( e => e.Id );
            rptExcpetionDetails.DataBind();

            pnlSummary.Visible = true;
        }
        #endregion
    }

}