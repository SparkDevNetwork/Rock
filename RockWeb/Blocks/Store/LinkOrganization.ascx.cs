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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Utility;

namespace RockWeb.Blocks.Store
{
    /// <summary>
    /// Lists packages that have been purchased in the Rock Store.
    /// </summary>
    [DisplayName( "Link Organization" )]
    [Category( "Store" )]
    [Description( "Links a Rock organization to the store." )]
    public partial class LinkOrganization : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

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

            if ( !Page.IsPostBack )
            {
                
            }
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
            List<Organization> organizations = organizationService.GetOrganizations(txtUsername.Text, txtPassword.Text, out errorMessage).ToList();

            switch ( organizations.Count )
            {
                case 1:
                    SetOrganization(organizations.First());
                    break;
                case 0:
                    ProcessNoResults();
                    break;
                default:
                    ProcessMultipleOrganizations(organizations);
                    break;
            }
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
            if ( PageParameter( "ReturnUrl" ) != string.Empty )
            {
                Response.Redirect( PageParameter( "ReturnUrl" ) );
            }
            else
            {
                Response.Redirect( Server.MapPath("~/Store") );
            }
        }

        #endregion

        #region Methods

        private void SetOrganization(Organization organization)
        {
            GlobalAttributesCache globalCache = GlobalAttributesCache.Read();
            globalCache.SetValue( "StoreOrganizationKey", organization.Key, true );
            pnlAuthenicate.Visible = false;
            pnlSelectOrganization.Visible = false;
            pnlComplete.Visible = true;

            lCompleteMessage.Text = string.Format( "<div class='alert alert-success margin-t-md'><strong>Success!</strong> We were able to configure the store for use by {0}.</div>", organization.Name );
        }

        private void ProcessNoResults()
        {
            string errorMessage = string.Empty;
            
            // first check that the username/password they provided are correct
            bool canAuthicate = new StoreService().AuthenicateUser( txtUsername.Text, txtPassword.Text);

            if ( canAuthicate )
            {
                lMessages.Text = @"<div class='alert alert-warning margin-t-md'>It appears that no organizations have been configured for this account. You can 
                                set up an organization on the Rock RMS website. Simply login and then select 'My Account' from the dropdown in the top right
                                corner.</div>";
            }
            else
            {
                lMessages.Text = @"<div class='alert alert-warning margin-t-md'>The username/password provided did not match a user on the Rock RMS website. Be sure
                    you provide a valid account from this site. If you would like to create an account or retrieve your password please <a href='https://www.rockrms.com/Rock/Login'>
                    vistit the Rock RMS website</a>.</div>";
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
    }
}