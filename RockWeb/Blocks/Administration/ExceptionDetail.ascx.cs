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
    [DetailPage] 
    [BooleanField( "Show Cookies", "Show cookie information when block loads.", false )]
    [BooleanField( "Show Server Variables", "Show server variables when block loads.", false )]
    public partial class ExceptionDetail : RockBlock, IDetailBlock
    {

        #region Control Methods
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int exceptionId = 0;
                cbShowCookies.Text = "<i class=\"icon-laptop\"> </i> Show Cookies";
                cbShowServerVariables.Text = "<i class=\"icon-hdd\"></i> Show Server Variables";
                if ( !String.IsNullOrWhiteSpace( PageParameter( "ExceptionId" ) ) && int.TryParse( PageParameter( "ExceptionId" ), out exceptionId ) )
                {
                    ShowDetail( "ExceptionId", exceptionId );
                }
                else
                {
                    pnlSummary.Visible = false;
                }
            }
            else
            {
                if ( ViewState["ExceptionDetails"] != null )
                {

                    List<ExceptionDetailSummary> exceptionDetailSummary = (List<ExceptionDetailSummary>)Newtonsoft.Json.JsonConvert.DeserializeObject( ViewState["ExceptionDetails"].ToString(), typeof( List<ExceptionDetailSummary> ) );
                    BuildExceptionDetailTable( exceptionDetailSummary );
                }
            }
        }
        #endregion

        #region Internal Methods

        private void BuildExceptionDetailTable( List<ExceptionDetailSummary> detailSummaries )
        {
            foreach ( var summary in detailSummaries )
            {
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
                exceptionStackTraceCell.ColumnSpan = 4;
                exceptionStackTraceCell.Text = summary.StackTrace;
                exceptionStackTraceCell.HorizontalAlign = HorizontalAlign.Left;

                stackTraceRow.Cells.Add( exceptionStackTraceCell );

                tblExceptionDetails.Rows.Add( stackTraceRow );

            }
        }

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

        private List<ExceptionDetailSummary> GetExceptionLogs( ExceptionLog baseException )
        {
            List<ExceptionDetailSummary> summaryList = new List<ExceptionDetailSummary>();
            ExceptionLogService svc = new ExceptionLogService();
            summaryList.Add( new ExceptionDetailSummary() 
                {   ExceptionId = baseException.Id, 
                    ExceptionSource = baseException.Source, 
                    ExceptionType = baseException.ExceptionType, 
                    ExceptionDescription = baseException.Description, 
                    StackTrace = baseException.StackTrace 
                } );

            int? parentId = baseException.ParentId;
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

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "ExceptionId" ) )
            {
                return;
            }

            var baseException = new ExceptionLogService().Get( itemKeyValue );

            hfExceptionID.Value = baseException.Id.ToString();
            lblSite.Text = baseException.Site.Name;
            lblPage.Text = baseException.Page.Name;

            if ( baseException.CreatedByPersonId == null || baseException.CreatedByPersonId == 0 )
            {
                lblUser.Text = "Anonymous";
            }
            else
            {
                lblUser.Text = baseException.CreatedByPerson.FullName;
            }

            litCookies.Text = baseException.Cookies;
            litServerVariables.Text = baseException.ServerVariables;

            if (!String.IsNullOrWhiteSpace(baseException.QueryString))
            {
                litQueryStringList.Text = BuildQueryStringList(baseException.QueryString);
                divQueryString.Visible = true;
            }
            else
            {
                divQueryString.Visible = false;
            }

            if( Convert.ToBoolean( GetAttributeValue( "ShowCookies" ) ) )
            {
                cbShowCookies.Checked = true;
            }
            else
            {
                cbShowCookies.Checked = false;
            }

            if ( Convert.ToBoolean( GetAttributeValue( "ShowServerVariables" ) ) )
            {
                cbShowServerVariables.Checked = true;
            }
            else
            {
                cbShowServerVariables.Checked = false;
            }

            List<ExceptionDetailSummary> exceptionDetailSummaryList = GetExceptionLogs( baseException );

            ViewState["ExceptionDetails"] = exceptionDetailSummaryList.ToJson();

            BuildExceptionDetailTable( exceptionDetailSummaryList );
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