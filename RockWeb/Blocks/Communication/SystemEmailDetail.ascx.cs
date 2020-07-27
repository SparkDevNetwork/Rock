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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for editing a system email
    /// </summary>
#pragma warning disable CS0618
    [Obsolete( "Use SystemCommunicationDetail instead." )]
#pragma warning restore CS0618
    [RockObsolete( "1.10" )]

    [DisplayName( "System Email Detail (Obsolete. Use SystemCommunicationDetail instead)" )]
    [Category( "Communication" )]
    [Description( "Allows the administration of a system email." )]
    public partial class SystemEmailDetail : RockBlock
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
                ShowEdit(PageParameter("EmailId").AsInteger());
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();
            
            string pageTitle = "New System Email";

            int? emailId = PageParameter( "EmailId" ).AsIntegerOrNull();
            if ( emailId.HasValue )
            {
                SystemEmail email = new SystemEmailService( new RockContext() ).Get( emailId.Value );
                if ( email != null )
                {
                    pageTitle = email.Title;
                    breadCrumbs.Add( new BreadCrumb( email.Title, pageReference ) );
                }
            }

            RockPage.Title = pageTitle;

            return breadCrumbs;
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
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            SystemEmailService emailTemplateService = new SystemEmailService( rockContext );
            SystemEmail emailTemplate;

            int emailTemplateId = int.Parse(hfEmailTemplateId.Value);

            if ( emailTemplateId == 0 )
            {
                emailTemplate = new SystemEmail();
                emailTemplateService.Add( emailTemplate );
            }
            else
            {
                emailTemplate = emailTemplateService.Get( emailTemplateId );
            }

            emailTemplate.CategoryId = cpCategory.SelectedValueAsInt();
            emailTemplate.Title = tbTitle.Text;
            emailTemplate.FromName = tbFromName.Text;
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

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="emailTemplateId">The email template id.</param>
        protected void ShowEdit( int emailTemplateId )
        {
            var globalAttributes = GlobalAttributesCache.Get();
            
            string globalFromName = globalAttributes.GetValue( "OrganizationName" );
            tbFromName.Help = string.Format( "If a From Name value is not entered the 'Organization Name' Global Attribute value of '{0}' will be used when this template is sent. <small><span class='tip tip-lava'></span></small>", globalFromName );

            string globalFrom = globalAttributes.GetValue( "OrganizationEmail" );
            tbFrom.Help = string.Format( "If a From Address value is not entered the 'Organization Email' Global Attribute value of '{0}' will be used when this template is sent. <small><span class='tip tip-lava'></span></small>", globalFrom );

            tbTo.Help = "You can specify multiple email addresses by separating them with commas or semi-colons.";

            SystemEmailService emailTemplateService = new SystemEmailService( new RockContext() );
            SystemEmail emailTemplate = emailTemplateService.Get( emailTemplateId );

            if ( emailTemplate != null )
            {
                pdAuditDetails.Visible = true;
                pdAuditDetails.SetEntity( emailTemplate, ResolveRockUrl( "~" ) );

                lActionTitle.Text = ActionTitle.Edit( SystemEmail.FriendlyTypeName ).FormatAsHtmlTitle();
                hfEmailTemplateId.Value = emailTemplate.Id.ToString();

                cpCategory.SetValue( emailTemplate.CategoryId );
                tbTitle.Text = emailTemplate.Title;
                tbFromName.Text = emailTemplate.FromName;
                tbFrom.Text = emailTemplate.From;
                tbTo.Text = emailTemplate.To;
                tbCc.Text = emailTemplate.Cc;
                tbBcc.Text = emailTemplate.Bcc;
                tbSubject.Text = emailTemplate.Subject;
                tbBody.Text = emailTemplate.Body;
            }
            else
            {
                pdAuditDetails.Visible = false;
                lActionTitle.Text = ActionTitle.Add( SystemEmail.FriendlyTypeName ).FormatAsHtmlTitle();
                hfEmailTemplateId.Value = 0.ToString();

                cpCategory.SetValue( (int?)null );
                tbTitle.Text = string.Empty;
                tbFromName.Text = string.Empty;
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