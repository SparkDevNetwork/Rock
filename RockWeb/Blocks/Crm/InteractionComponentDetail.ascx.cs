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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Presents the details of a interaction component using Lava
    /// </summary>
    [DisplayName( "Interaction Component Detail" )]
    [Category( "CRM" )]
    [Description( "Presents the details of a interaction channel using Lava" )]

    [CodeEditorField( "Default Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
         <div class='row'>
                        <div class='col-md-6'>
                                <dl>
                               <dt>Name</dt>
                               <dd>{{ InteractionComponent.Name }}<dd/>
                               </dl>
                        </div>
                        <div class='col-md-6'>
                        {% if InteractionComponentEntity != '' %}
                            <dl>
                               <dt>Entity Name</dt>
                               <dd>{{ InteractionComponentEntity }}<dd/>
                            </dl>
                        {% endif %}
                        </div>
                    </div>
", "", 0 )]

    public partial class InteractionComponentDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", InteractionChannel.FriendlyTypeName );

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
                int? componentId = PageParameter( "componentId" ).AsIntegerOrNull();
                if ( !componentId.HasValue )
                {
                    lTitle.Text = "Interaction Component";

                    nbWarningMessage.Title = "Missing Channel Information";
                    nbWarningMessage.Text = "<p>Make sure you have navigated to this page from Channel Listing page.</p>";
                    nbWarningMessage.Visible = true;

                    pnlViewDetails.Visible = false;

                    return;
                }

                ShowDetail( componentId.Value );
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
            var channelId = hfComponentId.Value.AsInteger();
            ShowDetail( channelId );
        }


        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            InteractionComponentService interactionComponentService = new InteractionComponentService( new RockContext() );
            InteractionComponent interactionComponent = interactionComponentService.Get( hfComponentId.ValueAsInt() );
            ShowEditDetails( interactionComponent );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            InteractionChannelService interactionChannelService = new InteractionChannelService( rockContext );
            InteractionChannel interactionChannel = interactionChannelService.Get( hfComponentId.Value.AsInteger() );

            if ( interactionChannel != null )
            {
                if ( !interactionChannel.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this account.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !interactionChannelService.CanDelete( interactionChannel, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                interactionChannelService.Delete( interactionChannel );

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            InteractionComponent interactionComponent;
            var rockContext = new RockContext();

            var interactionComponentService = new InteractionComponentService( rockContext );

            interactionComponent = interactionComponentService.Get( hfComponentId.Value.AsInteger() );

            interactionComponent.Name = tbName.Text;

            if ( epEntityPicker.Visible )
            {
                interactionComponent.EntityId = epEntityPicker.EntityId;
            }

            interactionComponent.ModifiedDateTime = RockDateTime.Now;
            interactionComponent.ModifiedByPersonAliasId = CurrentPersonAliasId;

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["componentId"] = interactionComponent.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams["ComponentId"] = hfComponentId.Value;
            NavigateToPage( RockPage.Guid, qryParams );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="component">The interaction component.</param>
        private void ShowEditDetails( InteractionComponent component )
        {
            lTitle.Text = component.Name.FormatAsHtmlTitle();
            SetEditMode( true );

            tbName.Text = component.Name;
            if ( component.Channel.ComponentEntityType.SingleValueFieldTypeId.HasValue )
            {
                epEntityPicker.Visible = true;
                epEntityPicker.EntityTypeId = component.Channel.ComponentEntityTypeId;
                epEntityPicker.Label = component.Channel.ComponentEntityType.FriendlyName;
                epEntityPicker.EntityId = component.EntityId;
            }
            else
            {
                epEntityPicker.Visible = false;
            }
        }

        /// <summary>
        /// Shows the detail with lava.
        /// </summary>
        public void ShowDetail( int componentId )
        {
            bool viewAllowed = false;
            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            RockContext rockContext = new RockContext();
            InteractionComponentService interactionComponentService = new InteractionComponentService( rockContext );
            var interactionComponent = interactionComponentService.Queryable().Where( a => a.Id == componentId ).FirstOrDefault();

            IEntity componentEntity = null;
            if ( interactionComponent.EntityId.HasValue )
            {
                componentEntity = GetComponentEntity( rockContext, interactionComponent );
            }

            viewAllowed = editAllowed || interactionComponent.IsAuthorized( Authorization.VIEW, CurrentPerson );
            editAllowed = editAllowed || interactionComponent.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = viewAllowed;
            if ( !editAllowed )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
            }
            SetEditMode( false );

            hfComponentId.SetValue( interactionComponent.Id );
            lTitle.Text = interactionComponent.Name.FormatAsHtmlTitle();

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.AddOrIgnore( "Person", CurrentPerson );
            mergeFields.Add( "InteractionChannel", interactionComponent.Channel );
            mergeFields.Add( "InteractionComponent", interactionComponent );
            mergeFields.Add( "InteractionComponentEntity", componentEntity.ToString() );

            string template = GetAttributeValue( "DefaultTemplate" );
            if ( !string.IsNullOrEmpty( interactionComponent.Channel.ComponentDetailTemplate ) )
            {
                template = interactionComponent.Channel.ComponentDetailTemplate;
            }
            lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
        }

        /// <summary>
        /// Gets the Component Entity
        /// </summary>
        /// <param name="rockContext">The db context.</param>
        /// <param name="component">The interaction component.</param>
        private IEntity GetComponentEntity( RockContext rockContext, InteractionComponent interactionComponent )
        {
            IEntity componentEntity = null;
            var componentEntityType = EntityTypeCache.Read( interactionComponent.Channel.ComponentEntityTypeId.Value ).GetEntityType();
            IService serviceInstance = Reflection.GetServiceForEntityType( componentEntityType, rockContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                componentEntity = getMethod.Invoke( serviceInstance, new object[] { interactionComponent.EntityId.Value } ) as Rock.Data.IEntity;
            }

            return componentEntity;
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

        #endregion

    }
}