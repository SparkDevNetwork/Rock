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
    /// List all the Interactions.
    /// </summary>
    [DisplayName( "Interaction List" )]
    [Category( "CRM" )]
    [Description( "List all the Interaction" )]

    [LinkedPage( "Interaction Detail Page", "Page reference to the interaction detail page. This will be included as a variable in the Lava.", false, order: 1 )]
    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, order: 2, defaultValue: @"
		<div class='panel panel-block'>
        <div class='panel-heading'>
			<h1 class='panel-title'><i class='fa fa-user'></i> Interactions</h1>
        </div>
		<div class='panel-body'>
		    <ul class='list-group margin-all-md'>
			{% for interaction in Interactions %}
			    {% if InteractionDetailPage != null and InteractionDetailPage != ''  %}
                <a href = '{{ InteractionDetailPage }}?interactionId={{ interaction.Id }}'>
                {% endif %}
				<li class='list-group-item margin-b-md' style='background-color: #edeae6;'>
                    <div class='row'>
                        <div class='col-md-6'>
                                <dl>
                               <dt>Date / Time</dt>
							   <dd>{{ interaction.InteractionDateTime }}<dd/>
                               </dl>
                               <dl>
                               <dt> Operation</dt>
							   <dd>{{ interaction.Operation }}<dd/>
                               </dl>
                          
                        </div>
                        <div class='col-md-6'>
                            <dl>
                               <dt>  Interaction</dt
                               <dd>{{ interaction.InteractionData }} <dd/>
                            </dl>
                            {% if interaction.PersonAlias != null and interaction.PersonAlias.Person.Name.FullName != '' %}
                                <dl>
                               <dt>  Person</dt
                               <dd>{{ interaction.PersonAlias.Person.FullName }} <dd/>
                            </dl>
                            {% endif %}
                        </div>
                    </div>
				</li>
				{% if InteractionDetailPage != null and InteractionDetailPage != ''  %}
				</a>
				{% endif %}
			{% endfor %}	
			</ul>
		</div>
	</div>" )]
    public partial class InteractionList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _componentId = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _componentId = PageParameter( "componentId" ).AsIntegerOrNull();

            if ( !_componentId.HasValue )
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
                if ( _componentId.HasValue )
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

            var interactionQry = interactionService.Queryable().AsNoTracking()
                                .Where( a => a.InteractionComponentId == _componentId.Value );
            var interactionComponent = interactionQry.Select( a => a.InteractionComponent ).First();
            // Parse the default template so that it does not need to be parsed multiple times
            var defaultTemplate = Template.Parse( GetAttributeValue( "DefaultTemplate" ) );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.AddOrIgnore( "Person", CurrentPerson );
            mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
            mergeFields.Add( "InteractionChannel", interactionComponent.Channel );
            mergeFields.Add( "InteractionComponent", interactionComponent );
            mergeFields.Add( "Interactions", interactionQry.ToList() );

            lContent.Text = interactionComponent.Channel.InteractionListTemplate.IsNotNullOrWhitespace() ?
                interactionComponent.Channel.InteractionListTemplate.ResolveMergeFields( mergeFields ) :
                defaultTemplate.Render( Hash.FromDictionary( mergeFields ) );

        }

        #endregion

    }
}