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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotLiquid;
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
    /// Presents the details of a interaction using Lava
    /// </summary>
    [DisplayName( "Interaction Detail" )]
    [Category( "CRM" )]
    [Description( "Presents the details of a interaction using Lava" )]

    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, order: 2, defaultValue: @"
		<div class='panel panel-block'>
        <div class='panel-heading'>
			<h1 class='panel-title'><i class='fa fa-user'></i> Interaction Detail</h1>
        </div>
		<div class='panel-body'>
                    <div class='row'>
                        <div class='col-md-6'>
                            <dl>
                               <dt> Channel</dt
                               <dd>{{ InteractionChannel.Name }} <dd/>
                            </dl>
                                <dl>
                               <dt>Date / Time</dt>
							   <dd>{{ Interaction.InteractionDateTime }}<dd/>
                               </dl>
                               <dl>
                               <dt> Operation</dt>
							   <dd>{{ Interaction.Operation }}<dd/>
                               </dl>
                          
                        </div>
                        <div class='col-md-6'>
                            <dl>
                               <dt>  Component</dt
                               <dd>{{ InteractionComponent.Name }} <dd/>
                            </dl>
                            {% if Interaction.PersonAlias.Person.FullName != '' %}
                                <dl>
                               <dt>  Person</dt
                               <dd>{{ Interaction.PersonAlias.Person.FullName }} <dd/>
                            </dl>
                            {% endif %}
                            <dl>
                               <dt>  Interaction</dt
                               <dd>{{ Interaction.InteractionData }} <dd/>
                            </dl>
                        </div>
                    </div>
		</div>
	</div>" )]
    public partial class InteractionDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _interactionId = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _interactionId = PageParameter( "interactionId" ).AsIntegerOrNull();

            if ( !_interactionId.HasValue )
            {
                upnlContent.Visible = false;
            }
            else
            {
                upnlContent.Visible = true;
            }
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
                if ( _interactionId.HasValue )
                {
                    ShowList();
                }
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
            ShowList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList()
        {

            var rockContext = new RockContext();
            InteractionService interactionService = new InteractionService( rockContext );

            var interaction = interactionService.Get( _interactionId.Value );

            // Parse the default template so that it does not need to be parsed multiple times
            var defaultTemplate = Template.Parse( GetAttributeValue( "DefaultTemplate" ) );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.AddOrIgnore( "Person", CurrentPerson );
            mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
            mergeFields.Add( "InteractionChannel", interaction.InteractionComponent.Channel );
            mergeFields.Add( "InteractionComponent", interaction.InteractionComponent );
            mergeFields.Add( "Interaction", interaction );

            lContent.Text = interaction.InteractionComponent.Channel.InteractionDetailTemplate.IsNotNullOrWhitespace() ?
                interaction.InteractionComponent.Channel.InteractionDetailTemplate.ResolveMergeFields( mergeFields ) :
                defaultTemplate.Render( Hash.FromDictionary( mergeFields ) );

        }

        #endregion

    }
}