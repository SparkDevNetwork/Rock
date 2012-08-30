//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Crm;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the emailTemplates that are available for a specific entity
    /// </summary>
    public partial class EmailTemplates : Rock.Web.UI.Block
    {
        #region Fields

        private bool _canConfigure = false;
        private Rock.Crm.EmailTemplateService _emailTemplateService = new Rock.Crm.EmailTemplateService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                _canConfigure = PageInstance.IsAuthorized( "Configure", CurrentPerson );

                BindFilter();

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.IsAddEnabled = true;
                    rGrid.Actions.AddClick += rGrid_AddClick;
                    rGrid.GridRebind += rGrid_GridRebind;

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this template?');
                }});
        }});
    ", rGrid.ClientID );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );
                }
                else
                {
                    DisplayError( "You are not authorized to configure this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
                BindGrid();

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.Crm.EmailTemplate emailTemplate = _emailTemplateService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( emailTemplate != null )
            {
                _emailTemplateService.Delete( emailTemplate, CurrentPersonId );
                _emailTemplateService.Save( emailTemplate, CurrentPersonId );
            }

            BindGrid();
        }

        protected void rGrid_AddClick( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            phList.Visible = true;
            pnlDetails.Visible = false;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.Crm.EmailTemplate emailTemplate;

            int emailTemplateId = 0;
            if ( !Int32.TryParse( hfEmailTemplateId.Value, out emailTemplateId ) )
                emailTemplateId = 0;

            if (emailTemplateId == 0)
            {
                emailTemplate = new EmailTemplate();
                _emailTemplateService.Add(emailTemplate, CurrentPersonId);
            }
            else
                emailTemplate = _emailTemplateService.Get(emailTemplateId);

            emailTemplate.Category = tbCategory.Text;
            emailTemplate.Title = tbTitle.Text;
            emailTemplate.From = tbFrom.Text;
            emailTemplate.To = tbTo.Text;
            emailTemplate.Cc = tbCc.Text;
            emailTemplate.Bcc = tbBcc.Text;
            emailTemplate.Subject = tbSubject.Text;
            emailTemplate.Body = tbBody.Text;

            _emailTemplateService.Save(emailTemplate, CurrentPersonId);

            BindFilter();
            BindGrid();

            pnlDetails.Visible = false;
            phList.Visible = true;
        }


        #endregion

        #region Internal Methods

        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( "[All]" );

            EmailTemplateService emailTemplateService = new EmailTemplateService();
            var items = emailTemplateService.Queryable().
                Where( a => a.Category.Trim() != "" && a.Category != null ).
                OrderBy( a => a.Category ).
                Select( a => a.Category.Trim() ).
                Distinct().ToList();

            foreach ( var item in items )
                ddlCategoryFilter.Items.Add( item );
        }

        private void BindGrid()
        {
            var emailTemplates = _emailTemplateService.Queryable();

            if ( ddlCategoryFilter.SelectedValue != "[All]" )
                emailTemplates = emailTemplates.
                    Where( a => a.Category.Trim() == ddlCategoryFilter.SelectedValue );

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
                emailTemplates = emailTemplates.Sort( sortProperty );
            else
                emailTemplates = emailTemplates.
                    OrderBy( a => a.Category ).
                    ThenBy( a => a.Title );

            rGrid.DataSource = emailTemplates.ToList();
            rGrid.DataBind();
        }

        protected void ShowEdit( int emailTemplateId )
        {
            Rock.Crm.EmailTemplate emailTemplate = _emailTemplateService.Get( emailTemplateId );

            if ( emailTemplate != null )
            {
                lAction.Text = "Edit";
                hfEmailTemplateId.Value = emailTemplate.Id.ToString();

                tbCategory.Text = emailTemplate.Category;
                tbTitle.Text = emailTemplate.Title;
                tbFrom.Text = emailTemplate.From;
                tbTo.Text = emailTemplate.To;
                tbCc.Text = emailTemplate.Cc;
                tbBcc.Text = emailTemplate.Bcc;
                tbSubject.Text = emailTemplate.Subject;
                tbBody.Text = emailTemplate.Body;
            }
            else
            {
                lAction.Text = "Add";
                tbCategory.Text = string.Empty;
                tbTitle.Text = string.Empty;
                tbFrom.Text = string.Empty;
                tbTo.Text = string.Empty;
                tbCc.Text = string.Empty;
                tbBcc.Text = string.Empty;
                tbSubject.Text = string.Empty;
                tbBody.Text = string.Empty;

            }

            phList.Visible = false;
            pnlDetails.Visible = true;
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            phList.Visible = false;
            pnlDetails.Visible = false;
        }

        #endregion


    }
}