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
using System.ComponentModel;
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
using Rock.Data;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Exception Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given exception." )]

    [LinkedPage("Detail Page")] 
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
                //show the detail
                ShowDetail( PageParameter( "ExceptionId" ).AsInteger() );
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
            ExceptionLogService svc = new ExceptionLogService( new RockContext() );

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
        /// <param name="exceptionId">The exception identifier.</param>
        public void ShowDetail( int exceptionId )
        {
            ExceptionLog baseException = null;

            if ( exceptionId != 0 )
            {
                baseException = new ExceptionLogService( new RockContext() ).Get( exceptionId );
            }

            //set fields
            if ( baseException == null )
            {
                pnlSummary.Visible = false;
                return;
            }

            // set page title
            lPageTitle.Text = String.Format("Exception Overview").FormatAsHtmlTitle();

            DescriptionList dl = new DescriptionList();

            dl.Add( "Site", baseException.Site != null ? baseException.Site.Name : String.Empty, true );
            if ( baseException.Page != null || !string.IsNullOrWhiteSpace(baseException.PageUrl) )
            {
                dl.Add( "Page", string.Format( "{0} <a href=\"{1}\" class=\"btn btn-link btn-xs\" target=\"_blank\">Visit Page</a>", baseException.Page != null ? baseException.Page.InternalName : baseException.PageUrl, baseException.PageUrl ) );
            }

            //If query string is not empty build query string list
            if ( !String.IsNullOrWhiteSpace( baseException.QueryString ) )
            {
                dl.Add( "Query String", BuildQueryStringList( baseException.QueryString ) );
            }

            if (baseException.CreatedByPersonAlias != null &&  baseException.CreatedByPersonAlias.Person != null)
            {
                dl.Add( "User", baseException.CreatedByPersonAlias.Person.FullName );
            }

            if ( baseException.CreatedDateTime.HasValue )
            {
                dl.Add( "Exception Date", string.Format( "{0:g}", baseException.CreatedDateTime.Value ) );
            }

            lExceptionSummary.Text = dl.Html;

            lCookies.Text = baseException.Cookies;
            lServerVariables.Text = baseException.ServerVariables;

            rptExcpetionDetails.DataSource = GetExceptionLogs( baseException ).OrderBy( e => e.Id );
            rptExcpetionDetails.DataBind();

            pnlSummary.Visible = true;
        }
        #endregion
    }

}