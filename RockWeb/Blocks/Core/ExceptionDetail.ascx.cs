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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Exception Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given exception." )]

    [Rock.SystemGuid.BlockTypeGuid( "B9E704E8-2097-491D-A216-8011012AA84E" )]
    public partial class ExceptionDetail : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "ExceptionId";
            public const string EntityGuid = "ExceptionGuid";
        }

        #endregion

        #region Common Code (EntityDetail)

        private RockContext _dataContext = null;

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetDataContext()
        {
            if ( _dataContext == null )
            {
                _dataContext = new RockContext();
            }

            return _dataContext;
        }

        #endregion

        #region Custom Methods (ExceptionDetail)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowContent();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Parse the query string value and build a list of values.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>unordered list of query string values</returns>
        private string BuildQueryStringList( string queryString )
        {
            string[] queryStringVariables = queryString.TrimStart( '?' ).Split( '&' );

            StringBuilder qsList = new StringBuilder();
            qsList.Append( "<ul type=\"disc\">" );

            foreach ( string query in queryStringVariables )
            {
                string[] queryStringValue = query.Split( "=".ToCharArray() );

                if ( queryStringValue.Length > 1 )
                {
                    qsList.AppendFormat( "<li>{0}: {1}</li>", queryStringValue[0].EncodeHtml(), queryStringValue[1].EncodeHtml() );
                }
                else
                {
                    qsList.AppendFormat( "<li>{0}</li>", queryStringValue[0].EncodeHtml() );
                }
            }

            qsList.Append( "</ul>" );

            return qsList.ToString();
        }

        /// <summary>
        /// Get the root-level exception of the specified exception.
        /// </summary>
        /// <param name="exceptionId">The id of an existing exception.</param>
        private ExceptionLog GetOutermostException( int exceptionId )
        {
            var dataContext = GetDataContext();

            var exceptionService = new ExceptionLogService( dataContext );

            var exception = exceptionService.GetNoTracking( exceptionId );

            return GetOutermostException( exception );
        }

        /// <summary>
        /// Get the root-level exception of the specified exception.
        /// </summary>
        /// <param name="exception">An existing exception.</param>
        private ExceptionLog GetOutermostException( ExceptionLog exception )
        {
            if ( exception == null )
            {
                return null;
            }

            if ( exception.ParentId == null )
            {
                return exception;
            }

            var dataContext = GetDataContext();

            var exceptionService = new ExceptionLogService( dataContext );

            ExceptionLog parentException;

            while ( exception != null )
            {
                parentException = exceptionService.Get( ( int ) exception.ParentId );

                // If the parent exception cannot be found, this is the base exception.
                if ( parentException == null )
                {
                    return exception;
                }

                if ( parentException.ParentId == null )
                {
                    return parentException;
                }

                exception = parentException;
            }

            return null;
        }

        /// <summary>
        /// Gets the related exception logs
        /// </summary>
        /// <param name="exception">The top-level exception.</param>
        /// <returns>List of Exception Detail Summary objects</returns>
        private List<ExceptionLog> GetExceptionLogs( ExceptionLog exception )
        {
            var exceptionList = new List<ExceptionLog>();

            var dataContext = GetDataContext();

            var exceptionService = new ExceptionLogService( dataContext );

            // load the base exception 
            exceptionList.Add( exception );

            // get the parentID
            int? parentId = exception.ParentId;

            // loop through exception hierarchy (parent, grandparent, etc)
            while ( parentId != null && parentId > 0 )
            {
                var parentException = exceptionService.Get( ( int ) parentId );

                if ( parentException != null )
                {
                    exceptionList.Add( parentException );
                }

                parentId = parentException.ParentId;
            }

            // get inner exceptions recursively
            List<ExceptionLog> childExceptionLogs = new List<ExceptionLog>();
            GetChildExceptionsRecursive( exceptionService, exception, ref childExceptionLogs );
            exceptionList.AddRange( childExceptionLogs);

            return exceptionList.Distinct().ToList();
        }

        /// <summary>
        /// Gets the child exceptions recursive.
        /// </summary>
        /// <param name="exceptionLogService">The exception log service.</param>
        /// <param name="exceptionLog">The exception log.</param>
        /// <param name="exceptionLogs">The exception logs.</param>
        private void GetChildExceptionsRecursive( ExceptionLogService exceptionLogService, ExceptionLog exceptionLog, ref List<ExceptionLog> exceptionLogs )
        {
            if ( exceptionLogs.Any( a => a.Id == exceptionLog.Id ) )
            {
                // prevent stackoverflow just in case
                return;
            }

            exceptionLogs.Add( exceptionLog );
            if ( exceptionLog.HasInnerException == true )
            {
                List<ExceptionLog> innerExceptions = exceptionLogService.GetByParentId( exceptionLog.Id ).ToList();
                foreach ( var innerException in innerExceptions )
                {
                    GetChildExceptionsRecursive( exceptionLogService, innerException, ref exceptionLogs );
                }
            }
        }

        /// <summary>
        /// Show the block content.
        /// </summary>
        private void ShowContent()
        {
            // show the detail
            int? exceptionId = PageParameter( PageParameterKey.EntityId ).AsIntegerOrNull();

            if ( !exceptionId.HasValue )
            {
                var exceptionGuid = PageParameter( PageParameterKey.EntityGuid ).AsGuidOrNull();

                if ( exceptionGuid.HasValue )
                {
                    var dataContext = GetDataContext();

                    exceptionId = new ExceptionLogService( dataContext ).Queryable().Where( a => a.Guid == exceptionGuid.Value ).Select( a => a.Id ).FirstOrDefault();
                }
            }

            ShowReadonlyDetail( exceptionId ?? 0 );
        }

        /// <summary>
        /// Shows the detail of the exception
        /// </summary>
        /// <param name="exceptionId">The exception identifier.</param>
        public void ShowReadonlyDetail( int exceptionId )
        {
            // Get the base-level exception.
            var baseException = GetOutermostException( exceptionId );

            if ( baseException == null )
            {
                pnlSummary.Visible = false;
                return;
            }

            var description = baseException.Description.Truncate( 100 );

            lPageTitle.Text = string.Format( "({0:g}) {1}", baseException.CreatedDateTime, description ).FormatAsHtmlTitle();

            var dl = new DescriptionList();

            dl.Add( "Exception Date", baseException.CreatedDateTime.HasValue ? string.Format( "{0:g}", baseException.CreatedDateTime.Value ) : string.Empty );
            dl.Add( "Description", baseException.Description.EncodeHtml().Truncate( 255, true ) );
            dl.Add( "Site", baseException.Site != null ? baseException.Site.Name : string.Empty );

            if ( baseException.Page != null || !string.IsNullOrWhiteSpace( baseException.PageUrl ) )
            {
                dl.Add( "Page", string.Format( "<a href=\"{1}\" target=\"_blank\">{0}</a>", baseException.Page != null ? baseException.Page.InternalName : baseException.PageUrl.EncodeHtml(), baseException.PageUrl.EncodeHtml() ) );
            }

            // If query string is not empty build query string list
            if ( !string.IsNullOrWhiteSpace( baseException.QueryString ) )
            {
                dl.Add( "Query String", BuildQueryStringList( baseException.QueryString ) );
            }

            if ( baseException.CreatedByPersonAlias != null && baseException.CreatedByPersonAlias.Person != null )
            {
                dl.Add( "User", baseException.CreatedByPersonAlias.Person.FullName );
            }

            lExceptionSummary.Text = dl.Html;

            lCookies.Text = baseException.Cookies;
            lServerVariables.Text = baseException.ServerVariables;

            btnShowCookies.Visible = !string.IsNullOrWhiteSpace( baseException.Cookies );
            btnShowVariables.Visible = !string.IsNullOrWhiteSpace( baseException.ServerVariables );

            // Make sure we have a root-level exception so we can show the entire hierarchy.
            var rootException = GetOutermostException( baseException );

            var logs = GetExceptionLogs( rootException );

            rptExceptionDetails.DataSource = logs.OrderBy( e => e.Id );

            rptExceptionDetails.DataBind();

            pnlSummary.Visible = true;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptExceptionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptExceptionDetails_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            ExceptionLog exceptionLog = e.Item.DataItem as ExceptionLog;
            if ( exceptionLog == null )
            {
                return;
            }

            Literal lStackTrace = e.Item.FindControl( "lStackTrace" ) as Literal;
            var stackTrace = exceptionLog.StackTrace;
            lStackTrace.Text = exceptionLog.StackTrace;

            var phStackTraceButton = e.Item.FindControl( "phStackTraceButton" ) as PlaceHolder;
            phStackTraceButton.Visible = exceptionLog.StackTrace.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Encode a field value as HTML.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected string EncodeHtml( object obj )
        {
            if ( obj != null )
            {
                return obj.ToString().EncodeHtml();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Show the block detail content.
        /// </summary>
        /// <param name="exceptionId"></param>
        public void ShowDetail( int exceptionId )
        {
            ShowReadonlyDetail( exceptionId );
        }

        #endregion
    }
}