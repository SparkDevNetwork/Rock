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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for managing the emailTemplates that are available for a specific entity
    /// </summary>
    [DisplayName( "Email Templates" )]
    [Category( "Communication" )]
    [Description( "Allows the administration of email templates." )]
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

            if ( RockPage.IsAuthorized( "Administrate", CurrentPerson ) )
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

            if ( RockPage.IsAuthorized( "Administrate", CurrentPerson ) )
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
                emailTemplateService.Delete( emailTemplate, CurrentPersonAlias );
                emailTemplateService.Save( emailTemplate, CurrentPersonAlias );
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
                emailTemplateService.Add( emailTemplate, CurrentPersonAlias );
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

            emailTemplateService.Save( emailTemplate, CurrentPersonAlias );
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
            var globalAttributes = GlobalAttributesCache.Read();
            string globalFrom = globalAttributes.GetValue( "OrganizationEmail" );
            tbFrom.Help = string.Format( "If a From value is not entered the 'Organization Email' Global Attribute value of '{0}' will be used when this template is sent.", globalFrom );

            pnlList.Visible = false;
            pnlDetails.Visible = true;

            EmailTemplateService emailTemplateService = new EmailTemplateService();
            EmailTemplate emailTemplate = emailTemplateService.Get( emailTemplateId );

            if ( emailTemplate != null )
            {
                lActionTitle.Text = ActionTitle.Edit( EmailTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
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
                lActionTitle.Text = ActionTitle.Add( EmailTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
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