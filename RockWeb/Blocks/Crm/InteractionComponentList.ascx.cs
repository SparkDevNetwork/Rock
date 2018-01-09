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
    /// List all the Interaction Component.
    /// </summary>
    [DisplayName( "Interaction Component List" )]
    [Category( "CRM" )]
    [Description( "List all the Interaction Component" )]

    [LinkedPage( "Component Detail Page", "Page reference to the component detail page. This will be included as a variable in the Lava.", false, order: 1 )]
    [LinkedPage( "Interaction Detail Page", "Page reference to the interaction detail page. This will be included as a variable in the Lava.", false, order: 1 )]
    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, order: 2, defaultValue: @"
	<div class='panel panel-block'>
        <div class='panel-heading'>
			<h1 class='panel-title'><i class='fa fa-th'></i> Components</h1>
        </div>
		<div class='panel-body'>
		    <ul class='list-group margin-all-md'>
			{% for component in InteractionComponents %}
				<li class='list-group-item margin-b-md' style='background-color: #edeae6;'>
                    <div class='row'>
                        <div class='col-md-6'>
                                <dl>
                               <dt>Name</dt>
							   {% if ComponentDetailPage != null and ComponentDetailPage != ''  %}
                               <dd><a href = '{{ ComponentDetailPage }}?ComponentId={{ component.Id }}' > Started {{ component.Name }}</a><dd/>
							   {% else %}
							   <dd>{{ component.Name }}<dd/>
							   {% endif %}
                               </dl>
                          
                        </div>
                        <div class='col-md-6'>
                            {% if InteractionChannel.Name != '' %}
                                <dl>
                               <dt>Channel Name</dt
                               <dd>{{ InteractionChannel.Name }} <dd/>
                            </dl>
                            {% endif %}
                        </div>
                    </div>
				</li>
			{% endfor %}	
			</ul>
		</div>
	</div>" )]
    public partial class InteractionComponentList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _channelId = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _channelId = PageParameter( "channelId" ).AsIntegerOrNull();

            if ( !_channelId.HasValue )
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
                if ( _channelId.HasValue )
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
            InteractionComponentService interactionComponentService = new InteractionComponentService( rockContext );

            var interactionChannel = new InteractionChannelService( rockContext ).Get( _channelId.Value );

            var interactionComponentQry = interactionComponentService.Queryable().AsNoTracking()
                                .Where( a => a.ChannelId == _channelId.Value );

            // Parse the default template so that it does not need to be parsed multiple times
            var defaultTemplate = Template.Parse( GetAttributeValue( "DefaultTemplate" ) );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.AddOrIgnore( "Person", CurrentPerson );
            mergeFields.Add( "ComponentDetailPage", LinkedPageRoute( "ComponentDetailPage" ) );
            mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
            mergeFields.Add( "InteractionChannel", interactionChannel );
            mergeFields.Add( "InteractionComponents", interactionComponentQry.ToList() );

            lContent.Text = interactionChannel.ComponentListTemplate.IsNotNullOrWhitespace() ?
                interactionChannel.ComponentListTemplate.ResolveMergeFields( mergeFields ) :
                defaultTemplate.Render( Hash.FromDictionary( mergeFields ) );

        }

        #endregion

    }
}