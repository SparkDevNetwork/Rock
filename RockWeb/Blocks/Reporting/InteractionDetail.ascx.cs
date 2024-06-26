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
using System.ComponentModel;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Presents the details of a interaction using Lava
    /// </summary>
    [DisplayName( "Interaction Detail" )]
    [Category( "Reporting" )]
    [Description( "Presents the details of a interaction using Lava" )]

    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, false, order: 2, defaultValue: DEFAULT_LAVA_TEMPLATE )]
    [Rock.SystemGuid.BlockTypeGuid( "B6AD2D98-0DF3-4DFB-AE2B-A8CF6E21E5C0" )]
    public partial class InteractionDetail : Rock.Web.UI.RockBlock
    {
        #region Block Setting Strings
        protected const string DEFAULT_LAVA_TEMPLATE = @"<div class='panel panel-block'>
        <div class='panel-heading'>
	        <h1 class='panel-title'>
                <i class='fa fa-user'></i>
                Interaction Detail
            </h1>
        </div>
        <div class='panel-body'>
            <div class='row'>
                <div class='col-md-6'>
                    <dl>
                        <dt>Channel</dt><dd>{{ InteractionChannel.Name }}<dd/>
                        <dt>Date / Time</dt><dd>{{ Interaction.InteractionDateTime }}<dd/>
                        <dt>Operation</dt><dd>{{ Interaction.Operation }}<dd/>
                        
                        {% if InteractionEntityName != '' %}
                            <dt>Entity Name</dt><dd>{{ InteractionEntityName }}<dd/>
                        {% endif %}
                    </dl>
                </div>
                <div class='col-md-6'>
                    <dl>
                        <dt> Component</dt><dd>{{ InteractionComponent.Name }}<dd/>
                        {% if Interaction.PersonAlias.Person.FullName != '' %}
                            <dt>Person</dt><dd>{{ Interaction.PersonAlias.Person.FullName }}<dd/>
                        {% endif %}
                        
                        {% if Interaction.InteractionSummary and Interaction.InteractionSummary != '' %}
                            <dt>Interaction Summary</dt><dd>{{ Interaction.InteractionSummary }}<dd/>
                        {% endif %}
                        
                        {% if Interaction.InteractionData and Interaction.InteractionData != '' %}
                            <dt>Interaction Data</dt><dd>{{ Interaction.InteractionData }}<dd/>
                        {% endif %}
                    </dl>
                </div>
            </div>
        </div>
    </div>";
        #endregion

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
                ShowDetail( PageParameter( "InteractionId" ).AsInteger() );
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
            ShowDetail( PageParameter( "InteractionId" ).AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowDetail( int interactionId )
        {
            using ( var rockContext = new RockContext() )
            {
                var interaction = new InteractionService( rockContext ).Get( interactionId );

                IEntity interactionEntity = null;
                if ( interaction.EntityId.HasValue )
                {
                    interactionEntity = GetInteractionEntity( rockContext, interaction );
                }

                if ( interaction != null && ( UserCanEdit || interaction.IsAuthorized( Authorization.VIEW, CurrentPerson ) ) )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.TryAdd( "Person", CurrentPerson );
                    mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
                    mergeFields.Add( "InteractionChannel", interaction.InteractionComponent.InteractionChannel );
                    mergeFields.Add( "InteractionComponent", interaction.InteractionComponent );
                    mergeFields.Add( "InteractionEntity", interactionEntity );

                    if ( interactionEntity != null )
                    {
                        mergeFields.Add( "InteractionEntityName", interactionEntity.ToString() );
                    }
                    else
                    {
                        mergeFields.Add( "InteractionEntityName", string.Empty );
                    }
                    
                    mergeFields.Add( "Interaction", interaction );

                    lContent.Text = interaction.InteractionComponent.InteractionChannel.InteractionDetailTemplate.IsNotNullOrWhiteSpace() ?
                        interaction.InteractionComponent.InteractionChannel.InteractionDetailTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( "DefaultTemplate" ).ResolveMergeFields( mergeFields );
                }
            }
        }

        /// <summary>
        /// Gets the Component Entity
        /// </summary>
        /// <param name="rockContext">The db context.</param>
        /// <param name="interaction">The interaction .</param>
        private IEntity GetInteractionEntity( RockContext rockContext, Interaction interaction )
        {
            IEntity interactionEntity = null;
            var interactionEntityType = EntityTypeCache.Get( interaction.InteractionComponent.InteractionChannel.InteractionEntityTypeId.Value ).GetEntityType();
            IService serviceInstance = Reflection.GetServiceForEntityType( interactionEntityType, rockContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                interactionEntity = getMethod.Invoke( serviceInstance, new object[] { interaction.EntityId.Value } ) as Rock.Data.IEntity;
            }

            return interactionEntity;
        }

        #endregion

    }
}