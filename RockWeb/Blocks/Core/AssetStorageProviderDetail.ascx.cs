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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Asset Storage Provider Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given asset storage provider." )]
    public partial class AssetStorageProviderDetail : RockBlock
    {
        /// <summary>
        /// Gets or sets the asset storage provider identifier.
        /// </summary>
        /// <value>
        /// The asset storage provider identifier.
        /// </value>
        public int AssetStorageProviderId
        {
            get
            {
                return ViewState["AssetStorageProviderId"] as int? ?? 0;
            }

            set
            {
                ViewState["AssetStorageProviderId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the asset storage provider entity type identifier.
        /// </summary>
        /// <value>
        /// The asset storage provider entity type identifier.
        /// </value>
        public int? AssetStorageProviderEntityTypeId
        {
            get
            {
                return ViewState["AssetStorageProviderEntityTypeId"] as int?;
            }

            set
            {
                ViewState["AssetStorageProviderEntityTypeId"] = value;
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var assetStorageProvider = new AssetStorageProvider { Id = AssetStorageProviderId, EntityTypeId = AssetStorageProviderEntityTypeId };
            BuildDynamicControls( assetStorageProvider, false );
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
                AssetStorageProviderId = PageParameter( "assetStorageProviderId" ).AsInteger();
                ShowDetail( AssetStorageProviderId );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                AssetStorageProvider assetStorageProvider = null;
                var assetStorageProviderService = new AssetStorageProviderService( rockContext );

                if ( AssetStorageProviderId != 0 )
                {
                    assetStorageProvider = assetStorageProviderService.Get( AssetStorageProviderId );
                }

                if ( assetStorageProvider == null )
                {
                    assetStorageProvider = new Rock.Model.AssetStorageProvider();
                    assetStorageProviderService.Add( assetStorageProvider );
                }

                assetStorageProvider.Name = tbName.Text;
                assetStorageProvider.IsActive = cbIsActive.Checked;
                assetStorageProvider.Description = tbDescription.Text;
                assetStorageProvider.EntityTypeId = cpAssetStorageType.SelectedEntityTypeId;

                rockContext.SaveChanges();

                assetStorageProvider.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, assetStorageProvider );
                assetStorageProvider.SaveAttributeValues( rockContext );
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
        /// Shows the detail.
        /// </summary>
        /// <param name="assetStorageProviderId">The asset storage provider identifier.</param>
        public void ShowDetail( int assetStorageProviderId )
        {
            if ( !IsUserAuthorized( Authorization.VIEW ) )
            {
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToView( AssetStorageProvider.FriendlyTypeName );
                return;
            }

            AssetStorageProvider assetStorageProvider = null;
            var rockContext = new RockContext();

            if ( assetStorageProviderId == 0 )
            {
                assetStorageProvider = new AssetStorageProvider();
                pdAuditDetails.Visible = false;
            }
            else
            {
                var assetStorageProviderService = new AssetStorageProviderService( rockContext );
                assetStorageProvider = assetStorageProviderService.Get( assetStorageProviderId );
                pdAuditDetails.SetEntity( assetStorageProvider, ResolveRockUrl( "~" ) );

                if ( assetStorageProvider == null )
                {
                    assetStorageProvider = new AssetStorageProvider();
                    pdAuditDetails.Visible = false;
                }
            }

            if ( assetStorageProvider.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( AssetStorageProvider.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = assetStorageProvider.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !assetStorageProvider.IsActive;

            tbName.Text = assetStorageProvider.Name;
            cbIsActive.Checked = assetStorageProvider.IsActive;
            tbDescription.Text = assetStorageProvider.Description;
            cpAssetStorageType.SetValue( assetStorageProvider.EntityType != null ? assetStorageProvider.EntityType.Guid.ToString().ToUpper() : string.Empty );

            BuildDynamicControls( assetStorageProvider, true );
        }

        /// <summary>
        /// Builds the dynamic controls.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildDynamicControls( AssetStorageProvider assetStorageProvider, bool setValues )
        {
            AssetStorageProviderEntityTypeId = assetStorageProvider.EntityTypeId;

            if ( assetStorageProvider.EntityTypeId.HasValue )
            {
                var assetStorageProviderComponentEntityType = EntityTypeCache.Get( assetStorageProvider.EntityTypeId.Value );
                var assetStorageProviderEntityType = EntityTypeCache.Get<Rock.Model.AssetStorageProvider>();

                if ( assetStorageProviderComponentEntityType != null && assetStorageProviderEntityType != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Helper.UpdateAttributes(
                            assetStorageProviderComponentEntityType.GetEntityType(),
                            assetStorageProviderEntityType.Id,
                            "EntityTypeId",
                            assetStorageProviderComponentEntityType.Id.ToString(),
                            rockContext );

                        assetStorageProvider.LoadAttributes( rockContext );
                    }
                }
            }

            phAttributes.Controls.Clear();
            Helper.AddEditControls( assetStorageProvider, phAttributes, setValues, BlockValidationGroup, new List<string> { "Active", "Order" }, false, 2 );

            foreach ( var tb in phAttributes.ControlsOfTypeRecursive<TextBox>() )
            {
                tb.AutoCompleteType = AutoCompleteType.Disabled;
                tb.Attributes["autocomplete"] = "off";
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpAssetStorageType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpAssetStorageType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var assetStorageProvider = new AssetStorageProvider { Id = AssetStorageProviderId, EntityTypeId = cpAssetStorageType.SelectedEntityTypeId };
            BuildDynamicControls( assetStorageProvider, true );
        }
    }
}