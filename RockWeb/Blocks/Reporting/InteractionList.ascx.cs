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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// List all the Interactions.
    /// </summary>
    [DisplayName( "Interaction List" )]
    [Category( "Reporting" )]
    [Description( "List all the Interaction" )]

    [LinkedPage( "Interaction Detail Page", "Page reference to the interaction detail page. This will be included as a variable in the Lava.", false, order: 1 )]
    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, false, order: 2, defaultValue: @"
    <div class='panel panel-block'>
        <div class='panel-heading'>
	        <h1 class='panel-title'>
                <i class='fa fa-user'></i>
                Interactions
            </h1>
        </div>
        <div class='panel-body'>
	        <ul class='list-group margin-all-md'>
	        {% for interaction in Interactions %}
		        {% if InteractionDetailPage != null and InteractionDetailPage != ''  %}
                    <a href='{{ InteractionDetailPage }}?InteractionId={{ interaction.Id }}'>
                {% endif %}
		        <li class='list-group-item margin-b-md' style='background-color: #edeae6;'>
                    <div class='row'>
                        <div class='col-md-6'>
                            <dl>
                                <dt>Date / Time</dt><dd>{{ interaction.InteractionDateTime }}<dd/>
                                <dt>Operation</dt><dd>{{ interaction.Operation }}<dd/>
                            </dl>
                        </div>
                        <div class='col-md-6'>
                            <dl>
                                <dt>Interaction</dt><dd>{{ interaction.InteractionData }}<dd/>
                                {% if interaction.PersonAlias != null and interaction.PersonAlias.Person.Name.FullName != '' %}
                                    <dt>Person</dt><dd>{{ interaction.PersonAlias.Person.FullName }}<dd/>
                                {% endif %}
                            </dl>
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
                ShowList( PageParameter( "componentId" ).AsInteger() );
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
            ShowList( PageParameter( "componentId" ).AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList( int componentId )
        {
            using ( var rockContext = new RockContext() )
            {
                var component = new InteractionComponentService( rockContext ).Get( componentId );
                if ( component != null && ( UserCanEdit || component.IsAuthorized( Authorization.VIEW, CurrentPerson ) ) )
                {
                    var interactions = new InteractionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.InteractionComponentId == componentId )
                        .ToList();

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.AddOrIgnore( "Person", CurrentPerson );
                    mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
                    mergeFields.Add( "InteractionChannel", component.Channel );
                    mergeFields.Add( "InteractionComponent", component );
                    mergeFields.Add( "Interactions", interactions.ToList() );

                    lContent.Text = component.Channel.InteractionListTemplate.IsNotNullOrWhitespace() ?
                        component.Channel.InteractionListTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( "DefaultTemplate" ).ResolveMergeFields( mergeFields );
                }
            }
        }

        #endregion

    }
}