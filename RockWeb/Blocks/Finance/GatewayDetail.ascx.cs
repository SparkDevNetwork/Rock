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
using System.Linq;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Gateway Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given financial gateway." )]
    public partial class GatewayDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        public int GatewayId
        {
            get { return ViewState["GatewayId"] as int? ?? 0; }
            set { ViewState["GatewayId"] = value; }
        }

        public int? GatewayEntityTypeId
        {
            get { return ViewState["GatewayEntityTypeId"] as int?; }
            set { ViewState["GatewayEntityTypeId"] = value; }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var gateway = new FinancialGateway { Id = GatewayId, EntityTypeId = GatewayEntityTypeId };
            BuildDynamicControls( gateway, false );
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
                ShowDetail( PageParameter( "gatewayId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                FinancialGateway gateway = null;

                var gatewayService = new Rock.Model.FinancialGatewayService( rockContext );

                if ( GatewayId != 0 )
                {
                    gateway = gatewayService.Get( GatewayId );
                }

                if ( gateway == null )
                {
                    gateway = new Rock.Model.FinancialGateway();
                    gatewayService.Add( gateway );
                }

                gateway.Name = tbName.Text;
                gateway.IsActive = cbIsActive.Checked;
                gateway.Description = tbDescription.Text;
                gateway.EntityTypeId = cpGatewayType.SelectedEntityTypeId;
                gateway.SetBatchTimeOffset( tpBatchTimeOffset.SelectedTime );

                rockContext.SaveChanges();

                gateway.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, gateway );
                gateway.SaveAttributeValues( rockContext );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpGatewayType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpGatewayType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var gateway = new FinancialGateway { Id = GatewayId, EntityTypeId = cpGatewayType.SelectedEntityTypeId };
            BuildDynamicControls( gateway, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="gatewayId">The gateway identifier.</param>
        public void ShowDetail( int gatewayId )
        {
            FinancialGateway gateway = null;

            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            if ( !gatewayId.Equals( 0 ) )
            {
                gateway = new FinancialGatewayService( new RockContext() ).Get( gatewayId );
                editAllowed = editAllowed || gateway.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            if ( gateway == null )
            {
                gateway = new FinancialGateway { Id = 0, IsActive = true };
            }

            GatewayId = gateway.Id;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialGateway.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( gateway );
            }
            else
            {
                ShowEditDetails( gateway );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        private void ShowEditDetails( FinancialGateway gateway )
        {
            if ( gateway.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( FinancialGateway.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = gateway.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !gateway.IsActive;

            SetEditMode( true );

            tbName.Text = gateway.Name;
            cbIsActive.Checked = gateway.IsActive;
            tbDescription.Text = gateway.Description;
            cpGatewayType.SetValue( gateway.EntityType != null ? gateway.EntityType.Guid.ToString().ToUpper() : string.Empty );
            tpBatchTimeOffset.SelectedTime = gateway.GetBatchTimeOffset();

            BuildDynamicControls( gateway, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        private void ShowReadonlyDetails( FinancialGateway gateway )
        {
            SetEditMode( false );

            lActionTitle.Text = gateway.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !gateway.IsActive;
            lGatewayDescription.Text = gateway.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( gateway.EntityType != null )
            {
                descriptionList.Add( "Gateway Type", gateway.EntityType.Name );
            }

            var timeSpan = gateway.GetBatchTimeOffset();
            if ( timeSpan.Ticks > 0 )
            {
                descriptionList.Add( "Batch Time Offset", timeSpan.ToString() );
            }

            lblMainDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        private void BuildDynamicControls( FinancialGateway gateway, bool SetValues )
        {
            GatewayEntityTypeId = gateway.EntityTypeId;
            if ( gateway.EntityTypeId.HasValue )
            {
                var GatewayComponentEntityType = EntityTypeCache.Read( gateway.EntityTypeId.Value );
                var GatewayEntityType = EntityTypeCache.Read( "Rock.Model.FinancialGateway " );
                if ( GatewayComponentEntityType != null && GatewayEntityType != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Rock.Attribute.Helper.UpdateAttributes( GatewayComponentEntityType.GetEntityType(), GatewayEntityType.Id, "EntityTypeId", GatewayComponentEntityType.Id.ToString(), rockContext );
                        gateway.LoadAttributes( rockContext );
                    }
                }
            }

            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( gateway, phAttributes, SetValues, BlockValidationGroup, new List<string> { "Active", "Order" } );
        }

        #endregion
    }
}