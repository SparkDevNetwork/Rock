using System;
using System.Collections.Generic;
using System.ComponentModel;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Mobile
{
    [DisplayName( "Mobile Layout Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile layout." )]
    public partial class MobileLayoutDetail : RockBlock
    {
        #region Base Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if (!IsPostBack)
            {
                ShowDetail( PageParameter( "LayoutId" ).AsInteger() );
            }
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
