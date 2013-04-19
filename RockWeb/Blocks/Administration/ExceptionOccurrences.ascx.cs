using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    [DetailPage]
    public partial class ExceptionOccurrences : RockBlock
    {
        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            gExceptionOccurrences.GridRebind += gExceptionOccurrences_GridRebind;
            gExceptionOccurrences.RowSelected += gExceptionOccurrences_RowSelected;
            gExceptionOccurrences.DataKeyNames = new string[] { "Id" };
            hfExceptionLogID.Value = PageParameter( "ExceptionLogId" );
            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int exceptionId;
                if ( int.TryParse( hfExceptionLogID.Value, out exceptionId ) )
                {
                    ExceptionLogService exceptionService = new ExceptionLogService();
                    ExceptionLog exception = exceptionService.Get( exceptionId );
                    LoadExceptionSummmary( exception );
                    BindGrid( exception );
                }
            }
        }
        #endregion

        #region Grid Events
        protected void gExceptionOccurrences_GridRebind( object sender, EventArgs e )
        {
            int relatedExceptionID;

            if ( int.TryParse( hfExceptionLogID.Value, out relatedExceptionID ) )
            {
                ExceptionLogService exceptionService = new ExceptionLogService();
                var exception = exceptionService.Get( relatedExceptionID );
                BindGrid( exception );
            }
        }

        protected void gExceptionOccurrences_RowSelected( object sender, RowEventArgs e )
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Internal Methods
        private void BindGrid( ExceptionLog relatedException )
        {
            ExceptionLogService exceptionService = new ExceptionLogService();
            var exceptionOccurrences = exceptionService.Queryable()
                                        .Where( e => ( e.HasInnerException == null || e.HasInnerException == false ) )
                                        .Where( e => e.SiteId == relatedException.SiteId )
                                        .Where( e => e.PageId == relatedException.PageId )
                                        .Where( e => e.Description == relatedException.Description )
                                        .Select( e => new
                                                {
                                                    Id = e.Id,
                                                    ExceptionDateTime = e.ExceptionDateTime,
                                                    FullName = e.CreatedByPerson.FirstName + " " + e.CreatedByPerson.LastName,
                                                    Description = e.Description
                                                } );

            if ( gExceptionOccurrences.SortProperty != null )
            {
                gExceptionOccurrences.DataSource = exceptionOccurrences.Sort( gExceptionOccurrences.SortProperty ).ToList();
            }
            else
            {
                gExceptionOccurrences.DataSource = exceptionOccurrences.OrderByDescending( e => e.ExceptionDateTime ).ToList();
            }

            gExceptionOccurrences.DataBind();
        }
        
        private void LoadExceptionSummmary( ExceptionLog exception )
        {
            lblSite.Text = exception.Site.Name;
            lblPage.Text = exception.Page.Name;
            lblType.Text = exception.ExceptionType;
        }
        #endregion
    }
}