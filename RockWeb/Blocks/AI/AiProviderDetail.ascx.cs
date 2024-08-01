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
using Rock.AI.Provider;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.AI
{
    [DisplayName( "AI Provider Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of an AI Provider." )]

    [Rock.SystemGuid.BlockTypeGuid( "3CE25D9D-0F91-49BE-9616-FF4664A26F96" )]
    public partial class AiProviderDetail : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "ProviderId";
        }

        #endregion

        #region Constants

        private static string ViewStateEntityId = "EntityId";
        private static string ViewStateEntityTypeId = "EntityTypeId";

        #endregion

        #region Control Methods

        public int ProviderId
        {
            get { return ViewState[ViewStateEntityId] as int? ?? 0; }
            set { ViewState[ViewStateEntityId] = value; }
        }

        public int? ProviderComponentEntityTypeId
        {
            get { return ViewState[ViewStateEntityTypeId] as int?; }
            set { ViewState[ViewStateEntityTypeId] = value; }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var provider = new AIProvider { Id = ProviderId, ProviderComponentEntityTypeId = this.ProviderComponentEntityTypeId };
            BuildDynamicControls( provider, false );
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
                ShowDetail( PageParameter( PageParameterKey.EntityId ).AsInteger() );
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
                AIProvider provider = null;

                var service = new AIProviderService( rockContext );

                if ( ProviderId != 0 )
                {
                    provider = service.Get( ProviderId );
                }

                if ( provider == null )
                {
                    provider = new AIProvider();
                    service.Add( provider );
                }

                provider.Name = tbName.Text;
                provider.IsActive = cbIsActive.Checked;
                provider.Description = tbDescription.Text;
                provider.ProviderComponentEntityTypeId = cpProviderType.SelectedEntityTypeId;
                 
                rockContext.SaveChanges();

                provider.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, provider );
                provider.SaveAttributeValues( rockContext );
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
        /// Handles the SelectedIndexChanged event of the cpProviderType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpProviderType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var provider = new AIProvider { Id = ProviderId, ProviderComponentEntityTypeId = cpProviderType.SelectedEntityTypeId };
            BuildDynamicControls( provider, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name=providerId>The provider identifier.</param>
        public void ShowDetail( int providerId )
        {
            AIProvider provider = null;

            bool editAllowed = IsUserAuthorized( Authorization.EDIT );
            

            if ( providerId != 0 )
            {
                var rockContext = new RockContext();
                var providerService = new AIProviderService( rockContext );
                provider = providerService.Get( providerId );
                editAllowed = editAllowed || provider.IsAuthorized( Authorization.EDIT, CurrentPerson );
                pdAuditDetails.SetEntity( provider, ResolveRockUrl( "~" ) );
            }

            if ( provider == null )
            {
                provider = new AIProvider { Id = 0, IsActive = true };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            this.ProviderId = provider.Id;

            var readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            if ( provider.IsSystem )
            {
                nbEditModeMessage.Text = EditModeMessage.System( AIProvider.FriendlyTypeName );

                tbName.ReadOnly = true;
                tbDescription.ReadOnly = true;
                cpProviderType.Enabled = false;
            }

            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( AIProvider.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( provider );
            }
            else
            {
                ShowEditDetails( provider );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="provider"></param>
        private void ShowEditDetails( AIProvider provider )
        {
            if ( provider.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( AIProvider.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = provider.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !provider.IsActive;

            SetEditMode( true );

            tbName.Text = provider.Name;
            cbIsActive.Checked = provider.IsActive;
            tbDescription.Text = provider.Description;
            cpProviderType.SetValue( provider.ProviderComponentEntityType != null ? provider.ProviderComponentEntityType.Guid.ToString().ToUpper() : string.Empty );

            BuildDynamicControls( provider, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="provider"></param>
        private void ShowReadonlyDetails( AIProvider provider )
        {
            SetEditMode( false );

            lActionTitle.Text = provider.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !provider.IsActive;
            lEntityDescription.Text = provider.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( provider.ProviderComponentEntityType != null )
            {
                descriptionList.Add( "AI Provider Type", provider.ProviderComponentEntityType.Name );
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

        private void BuildDynamicControls( AIProvider provider, bool SetValues )
        {
            this.ProviderComponentEntityTypeId = provider.ProviderComponentEntityTypeId;

            if ( provider.ProviderComponentEntityTypeId.HasValue )
            {
                var componentEntityType = EntityTypeCache.Get( provider.ProviderComponentEntityTypeId.Value );
                var entityType = EntityTypeCache.Get<AIProvider>();
                if ( componentEntityType != null && entityType != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Rock.Attribute.Helper.UpdateAttributes( componentEntityType.GetEntityType(), entityType.Id, nameof( provider.ProviderComponentEntityTypeId ), componentEntityType.Id.ToString(), rockContext );
                        provider.LoadAttributes( rockContext );
                    }

                    try
                    {
                        var selectedComponentType = Rock.Reflection.FindType( typeof( AIProviderComponent ), componentEntityType.Name );
                        if ( tbName.Text.IsNullOrWhiteSpace() )
                        {
                            tbName.Text = Rock.Reflection.GetDisplayName( selectedComponentType );
                        }

                        if ( tbDescription.Text.IsNullOrWhiteSpace() )
                        {
                            tbDescription.Text = Rock.Reflection.GetDescription( selectedComponentType );
                        }
                    }
                    catch
                    {
                        // ignore if there is a problem getting the name or description from the selected type
                    }
                }
            }

            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( provider, phAttributes, SetValues, BlockValidationGroup, new List<string> { "Active", "Order" } );
            foreach ( var tb in phAttributes.ControlsOfTypeRecursive<TextBox>() )
            {
                tb.AutoCompleteType = AutoCompleteType.Disabled;
                tb.Attributes["autocomplete"] = "off";

                if ( tb is IRockControl )
                {
                    if ( ( tb as IRockControl ).Label.IndexOf( "password", StringComparison.OrdinalIgnoreCase ) >= 0 )
                    {
                        tb.Attributes["autocomplete"] = "new-password";
                    }
                }
            }
        }

        #endregion
    }
}