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
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
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

                //sets icon and text for cookies and server variables checkboxes
                cbShowCookies.Text = "<i class=\"icon-laptop\"> </i> Show Cookies";
                cbShowServerVariables.Text = "<i class=\"icon-hdd\"></i> Show Server Variables";

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
        /// Builds table cells for the Exception Detail table and adds them to the table
        /// </summary>
        /// <param name="detailSummaries">List of Excpetion Detial Summary objects</param>
        private void BuildExceptionDetailTable( List<ExceptionDetailSummary> detailSummaries )
        {
            StringBuilder script = new StringBuilder();
            foreach ( var summary in detailSummaries )
            {
                string exceptionPageUrl = string.Format( "/page/{0}?ExceptionId={1}", CurrentPage.Id, summary.ExceptionId );
                TableRow detailRow = new TableRow();
          
                detailRow.ID = string.Format( "tdRowExceptionDetail_{0}", summary.ExceptionId );

                TableCell exceptionTypeCell = new TableCell();
                exceptionTypeCell.ID = string.Format( "tcExceptionType_{0}", summary.ExceptionId );
                exceptionTypeCell.Text = summary.ExceptionType;
                detailRow.Cells.Add( exceptionTypeCell );

                TableCell exceptionSourceCell = new TableCell();
                exceptionSourceCell.ID = string.Format( "tcExceptionSource_{0}", summary.ExceptionId );
                exceptionSourceCell.Text = summary.ExceptionSource;
                detailRow.Cells.Add( exceptionSourceCell );

                TableCell exceptionDescriptionCell = new TableCell();
                exceptionDescriptionCell.ID = string.Format( "tcExceptionDetail_{0}", summary.ExceptionId );
                exceptionDescriptionCell.Text = summary.ExceptionDescription;
                detailRow.Cells.Add( exceptionDescriptionCell );

                TableCell exceptionStackTraceToggleCell = new TableCell();
                exceptionStackTraceToggleCell.ID = string.Format( "tcExceptionStackTraceToggle_{0}", summary.ExceptionId );

                LinkButton lbExceptionStackTrace = new LinkButton();
                lbExceptionStackTrace.ID = string.Format( "lbExceptionStackTraceToggle_{0}", summary.ExceptionId );
                lbExceptionStackTrace.Attributes.Add( "onClick", string.Format("toggleStackTrace({0});return false;", summary.ExceptionId) );
                var iToggle = new HtmlGenericControl( "i" );
                iToggle.AddCssClass( "icon-file-alt" );
                lbExceptionStackTrace.Controls.Add( iToggle );

                var spanTitle = new HtmlGenericContainer( "span" );
                spanTitle.ID = string.Format( "spanExceptionStackTrace_{0}", summary.ExceptionId );
                spanTitle.InnerText = " Show Stack Trace";
                lbExceptionStackTrace.Controls.Add( spanTitle );


                lbExceptionStackTrace.AddCssClass( "btn" );
                exceptionStackTraceToggleCell.Controls.Add( lbExceptionStackTrace );

                exceptionStackTraceToggleCell.HorizontalAlign = HorizontalAlign.Center;
                detailRow.Cells.Add( exceptionStackTraceToggleCell );

                tblExceptionDetails.Rows.Add( detailRow );

                TableRow stackTraceRow = new TableRow();
                stackTraceRow.CssClass = "exceptionDetail-stackTrace-hide";
                stackTraceRow.ID = string.Format( "tdRowExceptionStackTrace_{0}", summary.ExceptionId );

                TableCell exceptionStackTraceCell = new TableCell();
                exceptionStackTraceCell.ID = string.Format( "tdExceptionStackTrace_{0}", summary.ExceptionId );
                exceptionStackTraceCell.ColumnSpan = 4;
                exceptionStackTraceCell.Text = summary.StackTrace;
                exceptionStackTraceCell.HorizontalAlign = HorizontalAlign.Left;

                stackTraceRow.Cells.Add( exceptionStackTraceCell );

                tblExceptionDetails.Rows.Add( stackTraceRow );

                script.Append( "$(\"[id*=" + exceptionSourceCell.ID + "]\").click(function () { redirectToPage(\"" + exceptionPageUrl + "\"); });" );
                script.Append( "$(\"[id*=" + exceptionSourceCell.ID +"]\").click(function () { redirectToPage(\"" + exceptionPageUrl + "\"); });" );
                script.Append( "$(\"[id*=" + exceptionDescriptionCell.ID + "]\").click(function () { redirectToPage(\"" + exceptionPageUrl + "\"); });" );
                script.Append( "$(\"[id*=" + exceptionStackTraceCell.ID + "]\").click(function () { redirectToPage(\"" + exceptionPageUrl + "\"); });" );
            }

            if ( !String.IsNullOrWhiteSpace( script.ToString() ) )
            {
                ScriptManager.RegisterStartupScript( upExcpetionDetail, upExcpetionDetail.GetType(), "ExceptionRedirects" + DateTime.Now.Ticks, script.ToString(), true );
            }
        }

        /// <summary>
        /// Disect the query string value and build unordered list
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
        /// <returns>List of Exception Detial Summary objects</returns>
        private List<ExceptionDetailSummary> GetExceptionLogs( ExceptionLog baseException )
        {
            List<ExceptionDetailSummary> summaryList = new List<ExceptionDetailSummary>();
            ExceptionLogService svc = new ExceptionLogService();

            //load the base exception 
            summaryList.Add( new ExceptionDetailSummary() 
                {   ExceptionId = baseException.Id, 
                    ExceptionSource = baseException.Source, 
                    ExceptionType = baseException.ExceptionType, 
                    ExceptionDescription = baseException.Description, 
                    StackTrace = baseException.StackTrace 
                } );
            
            //get the parentID
            int? parentId = baseException.ParentId;

            //loop through exception hierachy (parent, grandparent, etc)
            while ( parentId != null && parentId > 0 )
            {
                var exception = svc.Get( (int)parentId );

                if ( exception != null )
                {
                    summaryList.Add( new ExceptionDetailSummary()
                        {
                            ExceptionId = exception.Id,
                            ExceptionSource = exception.Source,
                            ExceptionType = exception.ExceptionType,
                            ExceptionDescription = exception.Description,
                            StackTrace = exception.StackTrace
                        }
                    );
                }

                parentId = exception.ParentId;
            }

            //get inner excpetions
            if ( baseException.HasInnerException != null &&  (bool)baseException.HasInnerException )
            {
                foreach ( ExceptionLog exception in svc.GetByParentId(baseException.Id) )
                {
                    summaryList.Add( new ExceptionDetailSummary()
                        {
                            ExceptionId = exception.Id,
                            ExceptionSource = exception.Source,
                            ExceptionType = exception.ExceptionType,
                            ExceptionDescription = exception.Description,
                            StackTrace = exception.StackTrace
                        }
                        );
                }
            }

            return summaryList;
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

            lblSite.Text = baseException.Site != null ? baseException.Site.Name : string.Empty;
            lblPage.Text = baseException.Page != null ? baseException.Site.Name : string.Empty;

            hlPageLink.NavigateUrl = string.Format( "~{0}", baseException.PageUrl );

            if ( baseException.CreatedByPersonId == null || baseException.CreatedByPersonId == 0 )
            {
                //user null show Anonymous
                lblUser.Text = "Anonymous";
            }
            else
            {
                lblUser.Text = baseException.CreatedByPerson.FullName;
            }

            litCookies.Text = baseException.Cookies;
            litServerVariables.Text = baseException.ServerVariables;

            //If query string is not empty buld query string list and show div
            if (!String.IsNullOrWhiteSpace(baseException.QueryString))
            {
                litQueryStringList.Text = BuildQueryStringList(baseException.QueryString);
                divQueryString.Visible = true;
            }
            else
            {
                divQueryString.Visible = false;
            }

            //check to see if Show Cookies attribute is true
            if( Convert.ToBoolean( GetAttributeValue( "ShowCookies" ) ) )
            {
                //if so check checkbox and register script to show cookies div
                cbShowCookies.Checked = true;
                ScriptManager.RegisterStartupScript( upExcpetionDetail, upExcpetionDetail.GetType(), "ShowCookiesOnLoad" + DateTime.Now.Ticks, "$(\"#pnlCookies\").css(\"display\", \"inherit\");", true );
            }
            else
            {
                cbShowCookies.Checked = false;
            }

            //check to see if show server variables attribute is checked
            if ( Convert.ToBoolean( GetAttributeValue( "ShowServerVariables" ) ) )
            {
                cbShowServerVariables.Checked = true;
                ScriptManager.RegisterStartupScript( upExcpetionDetail, upExcpetionDetail.GetType(), "ShowServerVariablesOnLoad" + DateTime.Now.Ticks, "$(\"#pnlServerVariables\").css(\"display\", \"inherit\");", true );
            }
            else
            {
                cbShowServerVariables.Checked = false;
            }

            //get related exceptions
            List<ExceptionDetailSummary> exceptionDetailSummaryList = GetExceptionLogs( baseException );

            //build the exception detail table
            BuildExceptionDetailTable( exceptionDetailSummaryList );

            pnlSummary.Visible = true;
        }
        #endregion
    }

    public class ExceptionDetailSummary
    {
        public int ExceptionId { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionSource { get; set; }
        public string ExceptionDescription { get; set; }
        public string StackTrace { get; set; }

    }
}