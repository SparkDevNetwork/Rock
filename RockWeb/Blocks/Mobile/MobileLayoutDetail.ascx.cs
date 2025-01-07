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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Mobile
{
    [DisplayName( "Mobile Layout Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile layout." )]
    [Rock.SystemGuid.BlockTypeGuid( "74B6C64A-9617-4745-9928-ABAC7948A95D" )]
    public partial class MobileLayoutDetail : RockBlock
    {
        #region Base Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if (!IsPostBack)
            {
                ShowDetail( PageParameter( "LayoutId" ).AsInteger() );
            }

            base.OnLoad( e );
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
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? layoutId = PageParameter( pageReference, "LayoutId" ).AsIntegerOrNull();
            if ( layoutId != null )
            {
                var layout = new LayoutService( new RockContext() ).Get( layoutId.Value );

                if ( layout != null )
                {
                    breadCrumbs.Add( new BreadCrumb( layout.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Layout", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="layoutId">The layout identifier.</param>
        private void ShowDetail( int layoutId )
        {
            var layout = new LayoutService( new RockContext() ).Get( layoutId ) ?? new Layout();

            //
            // Ensure the layout exists.
            //
            if ( layout == null )
            {
                nbError.Text = "The layout was not found.";
                pnlDetails.Visible = false;

                return;
            }

            //
            // Ensure the user has edit access.
            //
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( typeof( Layout ).GetFriendlyTypeName() );
                pnlDetails.Visible = false;

                return;
            }

            //
            // Ensure the layout is part of a mobile site.
            //
            if ( layout.Site != null && layout.Site.SiteType != SiteType.Mobile )
            {
                nbError.Text = "This block can only edit mobile layouts.";
                pnlDetails.Visible = false;

                return;
            }

            tbName.Text = layout.Name;
            tbDescription.Text = layout.Description;
            cePhoneLayout.Text = layout.LayoutMobilePhone;
            ceTabletLayout.Text = layout.LayoutMobileTablet;

            pnlDetails.Visible = true;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var layoutService = new LayoutService( rockContext );

            var layout = layoutService.Get( PageParameter( "LayoutId" ).AsInteger() );
            if ( layout == null )
            {
                layout = new Layout
                {
                    SiteId = PageParameter( "SiteId" ).AsInteger()
                };
                layoutService.Add( layout );
            }

            layout.Name = tbName.Text;
            layout.FileName = tbName.Text + ".xaml";
            layout.Description = tbDescription.Text;
            layout.LayoutMobilePhone = cePhoneLayout.Text;
            layout.LayoutMobileTablet = ceTabletLayout.Text;

            rockContext.SaveChanges();

            NavigateToParentPage( new Dictionary<string, string>
            {
                { "SiteId", PageParameter( "SiteId" ) },
                { "Tab", "Layouts" }
            } );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage( new Dictionary<string, string>
            {
                { "SiteId", PageParameter( "SiteId" ) },
                { "Tab", "Layouts" }
            } );
        }

        #endregion
    }
}