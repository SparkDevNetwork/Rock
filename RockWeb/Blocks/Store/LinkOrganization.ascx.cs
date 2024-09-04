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
using Rock.Model;
using Rock.Store;

namespace RockWeb.Blocks.Store
{
    /// <summary>
    /// Lists packages that have been purchased in the Rock Store.
    /// </summary>
    [DisplayName( "Link Organization" )]
    [Category( "Store" )]
    [Description( "Links a Rock organization to the store." )]
    [Rock.SystemGuid.BlockTypeGuid( "41DFED6E-2ECD-4198-80C3-816B27241EB4" )]
    public partial class LinkOrganization : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties
        private string Password
        {
            get
            {
                var password = ViewState["password"].ToString();
                if ( password.IsNotNullOrWhiteSpace() )
                {
                    password = Rock.Security.Encryption.DecryptString( password );
                }

                return password;
            }
            set
            {
                ViewState["password"] = Rock.Security.Encryption.EncryptString( value );
            }
        }
        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected void btnRetrieveOrganization_Click( object sender, EventArgs e )
        {
            // get organizations
            string errorMessage = string.Empty;

            OrganizationService organizationService = new OrganizationService();
            Password = txtPassword.Text;
            var organizations = organizationService.GetOrganizations( txtUsername.Text, Password, out errorMessage ).ToList();
            if ( organizations.Count == 0 )
            {
                ProcessNoResults();
                return;
            }

            var organizationKey = StoreService.GetOrganizationKey();
            Organization selectedOrganization = null;
            if ( organizationKey.IsNotNullOrWhiteSpace() )
            {
                selectedOrganization = organizations.FirstOrDefault( o => o.Key == organizationKey );
            }
            else if ( organizations.Count == 1 )
            {
                selectedOrganization = organizations.FirstOrDefault();
            }

            if ( selectedOrganization == null )
            {
                ProcessMultipleOrganizations( organizations );
                return;
            }

            SetOrganization( selectedOrganization );
        }

        protected void rblOrganizations_SelectedIndexChanged( object sender, EventArgs e )
        {
            btnSelectOrganization.Enabled = true;
        }

        protected void btnSelectOrganization_Click( object sender, EventArgs e )
        {
            Organization organization = new Organization();
            organization.Key = rblOrganizations.SelectedValue;
            organization.Name = rblOrganizations.SelectedItem.Text;
            SetOrganization( organization );
        }

        protected void btnSelectOrganizationCancel_Click( object sender, EventArgs e )
        {
            pnlSelectOrganization.Visible = false;
            pnlAuthenicate.Visible = true;
        }

        protected void btnContinue_Click( object sender, EventArgs e )
        {
            var returnUrl = PageParameter( "ReturnUrl" );
            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                Response.Redirect( returnUrl );
            }
            else
            {
                Response.Redirect( "/RockShop" );
            }
        }

        #endregion

        #region Methods

        private void SetOrganization( Organization organization )
        {
            StoreService.SetOrganizationKey( organization.Key );

            pnlAuthenicate.Visible = false;
            pnlSelectOrganization.Visible = false;
            pnlAverageWeeklyAttendance.Visible = true;
            pnlComplete.Visible = false;
        }

        private void ProcessNoResults()
        {
            string errorMessage = string.Empty;

            // first check that the username/password they provided are correct
            bool canAuthicate = new StoreService().AuthenicateUser( txtUsername.Text, Password );

            if ( canAuthicate )
            {
                lMessages.Text = @"<div class='alert alert-warning margin-t-md'>It appears that no organizations have been configured for this account. You can 
                                set up an organization on the Rock RMS website. Simply log in and then select 'My Account' from the dropdown in the top right
                                corner or see the <a href='https://www.rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.</div>";
            }
            else
            {
                lMessages.Text = @"<div class='alert alert-warning margin-t-md'>The username/password provided did not match a user on the Rock RMS website. Be sure
                    you provide a valid account from this site. If you would like to create an account or retrieve your password please <a href='https://www.rockrms.com/Login'>
                    visit the Rock RMS website</a> or see the <a href='https://www.rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.</div>";
            }

        }

        private void ProcessMultipleOrganizations( List<Organization> organizations )
        {
            pnlAuthenicate.Visible = false;
            pnlSelectOrganization.Visible = true;
            rblOrganizations.DataSource = organizations;
            rblOrganizations.DataTextField = "Name";
            rblOrganizations.DataValueField = "Key";
            rblOrganizations.DataBind();
        }

        #endregion

        protected void btnSaveAttendance_Click( object sender, EventArgs e )
        {
            var averageWeeklyAttendance = nbAverageWeeklyAttendance.IntegerValue;

            if ( averageWeeklyAttendance == null )
            {
                // error message
                return;
            }

            var result = new OrganizationService().SetOrganizationSize( txtUsername.Text, Password, StoreService.GetOrganizationKey(), averageWeeklyAttendance.Value );
            if ( result.HasError )
            {
                // Show error
                return;
            }

            lCompleteMessage.Text = string.Format( "<div class='alert alert-success margin-t-md'><strong>Success!</strong> We were able to configure the store for use by {0}.</div>", result.Result.Name );

            pnlAuthenicate.Visible = false;
            pnlSelectOrganization.Visible = false;
            pnlAverageWeeklyAttendance.Visible = false;
            pnlComplete.Visible = true;

        }
    }
}