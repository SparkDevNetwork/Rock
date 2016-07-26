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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Data;
using System.Data.Entity;
using System.Text;
using Rock.Security;
using Rock.Reporting.Dashboard;


namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Exception List Block
    /// </summary>
    [DisplayName( "Exception Occurrences" )]
    [Category( "Core" )]
    [Description( "Lists all exception occurrences." )]

    [LinkedPage( "Detail Page" )]
    public partial class ExceptionOccurrences : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Initializes the control/Rock Block
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //Set properties and events for Exception Occurrences
            gExceptionOccurrences.DataKeyNames = new string[] { "Id" };
            gExceptionOccurrences.GridRebind += gExceptionOccurrences_GridRebind;
            gExceptionOccurrences.RowSelected += gExceptionOccurrences_RowSelected;
            gExceptionOccurrences.RowItemText = "Exception";
        }

        /// <summary>
        /// Loads the control.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? exceptionId = PageParameter( "ExceptionId" ).AsIntegerOrNull();
                if ( exceptionId.HasValue )
                {
                    ShowDetail( exceptionId.Value );
                }
                else
                {
                    // NavigateToParentPage();
                }
            }
        }

        #endregion

        #region Exception Occurrence Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gExceptionOccurrences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gExceptionOccurrences_GridRebind( object sender, EventArgs e )
        {
            int exceptionID = 0;
            if ( int.TryParse( hfBaseExceptionID.Value, out exceptionID ) )
            {
                ExceptionLogService exceptionService = new ExceptionLogService( new RockContext() );
                ExceptionLog baseException = exceptionService.Get( exceptionID );

                BindExceptionOccurrenceGrid( baseException );
            }

        }

        protected void gExceptionOccurrences_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ExceptionId", e.RowKeyId );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the exception occurrence grid.
        /// </summary>
        /// <param name="baseException">Exception to base the occurrence grid off of.</param>
        private void BindExceptionOccurrenceGrid( ExceptionLog baseException )
        {
            ExceptionLogService exceptionService = new ExceptionLogService( new RockContext() );
            string url = String.Format( "{0}?ExceptionId=", LinkedPageUrl( "DetailPage" ) );

            var query = exceptionService.Queryable()
                .Where( e => e.HasInnerException == null || e.HasInnerException == false )
                .Where( e => e.Description.Substring( 0, 28 ) == baseException.Description.Substring( 0, 28 ) )
                .Select( e => new
                {
                    Id = e.Id,
                    CreatedDateTime = e.CreatedDateTime,
                    PageName = e.Page.InternalName ?? e.PageUrl,
                    FullName = ( e.CreatedByPersonAlias != null &&
                            e.CreatedByPersonAlias.Person != null ) ?
                            e.CreatedByPersonAlias.Person.LastName + ", " + e.CreatedByPersonAlias.Person.NickName : "",
                    Description = "<a href='" + url + e.Id + "'>" + e.Description + "</a>"
                } ).OrderBy( e => e.CreatedDateTime );

            if ( gExceptionOccurrences.SortProperty == null )
            {
                query = query.OrderByDescending( e => e.CreatedDateTime );
            }
            else
            {
                query = query.Sort( gExceptionOccurrences.SortProperty );
            }

            gExceptionOccurrences.EntityTypeId = EntityTypeCache.Read<ExceptionLog>().Id;
            gExceptionOccurrences.SetLinqDataSource( query );
            gExceptionOccurrences.DataBind();
        }

        /// <summary>
        /// Loads the exception occurrences panel
        /// </summary>
        /// <param name="exceptionId">The Id of the base exception for the grid</param>
        private void ShowDetail( int exceptionId )
        {
            //get the base exception
            ExceptionLogService exceptionService = new ExceptionLogService( new RockContext() );
            ExceptionLog exception = exceptionService.Get( exceptionId );

            //set the summary fields for the base exception
            if ( exception != null )
            {
                if ( Page.IsPostBack && Page.IsAsync )
                {
                    this.AddHistory( "ExceptionId", exceptionId.ToString(), string.Format( "Exception Occurrences {0}", exception.Description ) );
                }
                hfBaseExceptionID.Value = exceptionId.ToString();

                var descriptionList = new Rock.Web.DescriptionList();

                // set detail title with formating
                lDetailTitle.Text = String.Format( "Occurrences of {0}", exception.ExceptionType ).FormatAsHtmlTitle();

                if ( !string.IsNullOrEmpty( exception.ExceptionType ) )
                {
                    descriptionList.Add( "Type", exception.ExceptionType );
                }

                if ( exception.Site != null )
                {
                    descriptionList.Add( "Site", exception.Site.Name );
                }

                if ( exception.Page != null )
                {
                    descriptionList.Add( "Page", exception.Page.InternalName );
                }

                lblMainDetails.Text = descriptionList.Html;

                //Load the occurrences for the selected exception
                BindExceptionOccurrenceGrid( exception );
            }
        }

        #endregion
    }

}