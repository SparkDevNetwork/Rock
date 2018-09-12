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
    [DisplayName( "Asset Storage System Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given asset storage system." )]
    public partial class AssetStorageSystemDetail : RockBlock, IDetailBlock
    {
        /// <summary>
        /// Gets or sets the asset storage system identifier.
        /// </summary>
        /// <value>
        /// The asset storage system identifier.
        /// </value>
        public int AssetStorageSystemId
        {
            get
            {
                return ViewState["AssetStorageSystemId"] as int? ?? 0;
            }

            set
            {
                ViewState["AssetStorageSystemId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the asset storage system entity type identifier.
        /// </summary>
        /// <value>
        /// The asset storage system entity type identifier.
        /// </value>
        public int? AssetStorageSystemEntityTypeId
        {
            get
            {
                return ViewState["AssetStorageSystemEntityTypeId"] as int?;
            }

            set
            {
                ViewState["AssetStorageSystemEntityTypeId"] = value;
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var assetStorageSystem = new AssetStorageSystem { Id = AssetStorageSystemId, EntityTypeId = AssetStorageSystemEntityTypeId };
            BuildDynamicControls( assetStorageSystem, false );
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
                AssetStorageSystemId = PageParameter( "assetStorageSystemId" ).AsInteger();
                ShowDetail( AssetStorageSystemId );
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
                AssetStorageSystem assetStorageSystem = null;
                var assetStorageSystemService = new AssetStorageSystemService( rockContext );

                if ( AssetStorageSystemId != 0 )
                {
                    assetStorageSystem = assetStorageSystemService.Get( AssetStorageSystemId );
                }

                if ( assetStorageSystem == null )
                {
                    assetStorageSystem = new Rock.Model.AssetStorageSystem();
                    assetStorageSystemService.Add( assetStorageSystem );
                }

                assetStorageSystem.Name = tbName.Text;
                assetStorageSystem.IsActive = cbIsActive.Checked;
                assetStorageSystem.Description = tbDescription.Text;
                assetStorageSystem.EntityTypeId = cpAssetStorageType.SelectedEntityTypeId;

                rockContext.SaveChanges();

                assetStorageSystem.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, assetStorageSystem );
                assetStorageSystem.SaveAttributeValues( rockContext );
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
        /// <param name="assetStoragesystemId">The asset storagesystem identifier.</param>
        public void ShowDetail( int assetStoragesystemId )
        {
            if ( !IsUserAuthorized( Authorization.VIEW ) )
            {
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToView( AssetStorageSystem.FriendlyTypeName );
                return;
            }

            AssetStorageSystem assetStorageSystem = null;
            var rockContext = new RockContext();

            if ( assetStoragesystemId == 0 )
            {
                assetStorageSystem = new AssetStorageSystem();
                pdAuditDetails.Visible = false;
            }
            else
            {
                var assetStorageSystemService = new AssetStorageSystemService( rockContext );
                assetStorageSystem = assetStorageSystemService.Get( assetStoragesystemId );
                pdAuditDetails.SetEntity( assetStorageSystem, ResolveRockUrl( "~" ) );

                if ( assetStorageSystem == null )
                {
                    assetStorageSystem = new AssetStorageSystem();
                    pdAuditDetails.Visible = false;
                }
            }

            if ( assetStorageSystem.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( AssetStorageSystem.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = assetStorageSystem.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !assetStorageSystem.IsActive;

            tbName.Text = assetStorageSystem.Name;
            cbIsActive.Checked = assetStorageSystem.IsActive;
            tbDescription.Text = assetStorageSystem.Description;
            cpAssetStorageType.SetValue( assetStorageSystem.EntityType != null ? assetStorageSystem.EntityType.Guid.ToString().ToUpper() : string.Empty );

            BuildDynamicControls( assetStorageSystem, true );
        }

        /// <summary>
        /// Builds the dynamic controls.
        /// </summary>
        /// <param name="assetStorageSystem">The asset storage system.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildDynamicControls( AssetStorageSystem assetStorageSystem, bool setValues )
        {
            AssetStorageSystemEntityTypeId = assetStorageSystem.EntityTypeId;

            if ( assetStorageSystem.EntityTypeId.HasValue )
            {
                var assetStorageSystemComponentEntityType = EntityTypeCache.Get( assetStorageSystem.EntityTypeId.Value );
                var assetStorageSystemEntityType = EntityTypeCache.Get<Rock.Model.AssetStorageSystem>();

                if ( assetStorageSystemComponentEntityType != null && assetStorageSystemEntityType != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Helper.UpdateAttributes(
                            assetStorageSystemComponentEntityType.GetEntityType(),
                            assetStorageSystemEntityType.Id,
                            "EntityTypeId",
                            assetStorageSystemComponentEntityType.Id.ToString(),
                            rockContext );

                        assetStorageSystem.LoadAttributes( rockContext );
                    }
                }
            }

            phAttributes.Controls.Clear();
            Helper.AddEditControls( assetStorageSystem, phAttributes, setValues, BlockValidationGroup, new List<string> { "Active", "Order" }, false, 2 );

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
            var assetStorageSystem = new AssetStorageSystem { Id = AssetStorageSystemId, EntityTypeId = cpAssetStorageType.SelectedEntityTypeId };
            BuildDynamicControls( assetStorageSystem, true );
        }
    }
}