//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the emailTemplates that are available for a specific entity
    /// </summary>
    public partial class EmailTemplates : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            BindFilter();

            if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                gEmailTemplates.DataKeyNames = new string[] { "id" };
                gEmailTemplates.Actions.ShowAdd = true;
                gEmailTemplates.Actions.AddClick += gEmailTemplates_AddClick;
                gEmailTemplates.GridRebind += gEmailTemplates_GridRebind;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                }
            }
            else
            {
                gEmailTemplates.Visible = false;
                nbMessage.Text = WarningMessage.NotAuthorizedToEdit( EmailTemplate.FriendlyTypeName );
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );
            BindGrid();
        }


        /// <summary>
        /// Handles the AddClick event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_AddClick( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)gEmailTemplates.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Delete event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_Delete( object sender, RowEventArgs e )
        {
            EmailTemplateService emailTemplateService = new EmailTemplateService();
            EmailTemplate emailTemplate = emailTemplateService.Get( (int)gEmailTemplates.DataKeys[e.RowIndex]["id"] );
            if ( emailTemplate != null )
            {
                emailTemplateService.Delete( emailTemplate, CurrentPersonId );
                emailTemplateService.Save( emailTemplate, CurrentPersonId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlList.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            EmailTemplateService emailTemplateService = new EmailTemplateService();
            EmailTemplate emailTemplate;

            int emailTemplateId = int.Parse(hfEmailTemplateId.Value);

            if ( emailTemplateId == 0 )
            {
                emailTemplate = new EmailTemplate();
                emailTemplateService.Add( emailTemplate, CurrentPersonId );
            }
            else
            {
                emailTemplate = emailTemplateService.Get( emailTemplateId );
            }

            emailTemplate.Category = tbCategory.Text;
            emailTemplate.Title = tbTitle.Text;
            emailTemplate.From = tbFrom.Text;
            emailTemplate.To = tbTo.Text;
            emailTemplate.Cc = tbCc.Text;
            emailTemplate.Bcc = tbBcc.Text;
            emailTemplate.Subject = tbSubject.Text;
            emailTemplate.Body = tbBody.Text;

            if ( !emailTemplate.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            emailTemplateService.Save( emailTemplate, CurrentPersonId );
            BindFilter();
            BindGrid();
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( new ListItem(All.Text, All.Id.ToString()) );

            EmailTemplateService emailTemplateService = new EmailTemplateService();
            var items = emailTemplateService.Queryable().
                Where( a => a.Category.Trim() != "" && a.Category != null ).
                OrderBy( a => a.Category ).
                Select( a => a.Category.Trim() ).
                Distinct().ToList();

            foreach ( var item in items )
            {
                ListItem li = new ListItem( item );
                li.Selected = ( !Page.IsPostBack && rFilter.GetUserPreference( "Category" ) == item );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            EmailTemplateService emailTemplateService = new EmailTemplateService();
            SortProperty sortProperty = gEmailTemplates.SortProperty;

            var emailTemplates = emailTemplateService.Queryable();

            if ( ddlCategoryFilter.SelectedValue != All.Id.ToString() )
            {
                emailTemplates = emailTemplates.Where( a => a.Category.Trim() == ddlCategoryFilter.SelectedValue );
            }

            if ( sortProperty != null )
            {
                gEmailTemplates.DataSource = emailTemplates.Sort( sortProperty ).ToList();
            }
            else
            {
                gEmailTemplates.DataSource = emailTemplates.OrderBy( a => a.Category ).ThenBy( a => a.Title ).ToList();
            }

            gEmailTemplates.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="emailTemplateId">The email template id.</param>
        protected void ShowEdit( int emailTemplateId )
        {
            pnlList.Visible = false;
            pnlDetails.Visible = true;

            EmailTemplateService emailTemplateService = new EmailTemplateService();
            EmailTemplate emailTemplate = emailTemplateService.Get( emailTemplateId );

            if ( emailTemplate != null )
            {
                lActionTitle.Text = ActionTitle.Edit( EmailTemplate.FriendlyTypeName );
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
                lActionTitle.Text = ActionTitle.Add( EmailTemplate.FriendlyTypeName );
                hfEmailTemplateId.Value = 0.ToString();

                tbCategory.Text = string.Empty;
                tbTitle.Text = string.Empty;
                tbFrom.Text = string.Empty;
                tbTo.Text = string.Empty;
                tbCc.Text = string.Empty;
                tbBcc.Text = string.Empty;
                tbSubject.Text = string.Empty;
                tbBody.Text = string.Empty;
            }
        }

        #endregion
    }
}