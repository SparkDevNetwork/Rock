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
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Presents the details of a interaction component using Lava
    /// </summary>
    [DisplayName( "Interaction Component Detail" )]
    [Category( "Reporting" )]
    [Description( "Presents the details of a interaction channel using Lava" )]

    [CodeEditorField( "Default Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"
    <div class='row'>
        <div class='col-md-6'>
            <dl><dt>Name</dt><dd>{{ InteractionComponent.Name }}<dd/></dl>
        </div>
        {% if InteractionComponentEntity != '' %}
            <div class='col-md-6'>
                <dl>
                    <dt>Entity Name</dt><dd>{{ InteractionComponentEntity }}<dd/>
                </dl>
            </div>
        {% endif %}
    </div>
", "", 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "926261B2-CF4C-4B1F-A384-CD83696CFBC2" )]
    public partial class InteractionComponentDetail : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

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
                ShowDetail( 0 );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var componentService = new InteractionComponentService( rockContext );
                var componentName = componentService.GetSelect( PageParameter( "ComponentId" ).AsInteger(), c => c.Name );

                var breadCrumbs = new List<BreadCrumb>();
                breadCrumbs.Add( new BreadCrumb( componentName != null ? componentName : "Component", pageReference ) );
                return breadCrumbs;
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
            ShowDetail( 0 );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail with lava.
        /// </summary>
        public void ShowDetail( int componentId )
        {
            var rockContext = new RockContext();
            var componentService = new InteractionComponentService( rockContext );
            var component = componentService.Get( PageParameter( "ComponentId" ).AsInteger() );

            if ( component != null )
            {
                pnlDetails.Visible = UserCanEdit || component.IsAuthorized( Authorization.VIEW, CurrentPerson );

                IEntity componentEntity = null;
                if ( component.InteractionChannel.ComponentEntityTypeId.HasValue )
                {
                    componentEntity = GetComponentEntity( rockContext, component );
                }

                lTitle.Text = component.Name.FormatAsHtmlTitle();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "InteractionChannel", component.InteractionChannel );
                mergeFields.Add( "InteractionComponent", component );

                if ( componentEntity != null )
                {
                    mergeFields.Add( "InteractionComponentEntityName", componentEntity.ToString() );
                }
                else
                {
                    mergeFields.Add( "InteractionComponentEntityName", string.Empty );
                }
                
                mergeFields.Add( "InteractionComponentEntity", componentEntity);

                string template = component.InteractionChannel.ComponentDetailTemplate.IsNotNullOrWhiteSpace() ?
                    component.InteractionChannel.ComponentDetailTemplate :
                    GetAttributeValue( "DefaultTemplate" );

                lContent.Text = template.ResolveMergeFields( mergeFields );
            }
            else
            {
                pnlDetails.Visible = true;

                lTitle.Text = "Interaction Component";

                nbWarningMessage.Title = "Missing Component Information";
                nbWarningMessage.Text = "<p>Make sure you have navigated to this page correctly.</p>";
                nbWarningMessage.Visible = true;

                return;

            }
        }

        /// <summary>
        /// Gets the Component Entity
        /// </summary>
        /// <param name="rockContext">The db context.</param>
        /// <param name="component">The interaction component.</param>
        private IEntity GetComponentEntity( RockContext rockContext, InteractionComponent interactionComponent )
        {
            IEntity componentEntity = null;

            try
            {
                var componentEntityType = EntityTypeCache.Get( interactionComponent.InteractionChannel.ComponentEntityTypeId.Value ).GetEntityType();
                IService serviceInstance = Reflection.GetServiceForEntityType( componentEntityType, rockContext );
                if ( serviceInstance != null )
                {
                    System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                    componentEntity = getMethod.Invoke( serviceInstance, new object[] { interactionComponent.EntityId.Value } ) as Rock.Data.IEntity;
                }
            }
            catch
            {
                // If we can't get the entity type, just return null
            }

            return componentEntity;
        }


        #endregion

    }
}