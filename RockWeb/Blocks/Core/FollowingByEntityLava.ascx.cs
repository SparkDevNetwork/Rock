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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using Rock.Security;
using System.Reflection;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Takes a entity type and displays a person's following items for that entity using a Lava template.
    /// </summary>
    [DisplayName( "Following By Entity" )]
    [Category( "Core" )]
    [Description( "Takes a entity type and displays a person's following items for that entity using a Lava template." )]
    [EntityTypeField("Entity Type", "The type of entity to show following for.", order: 0)]
    [TextField("Link URL", "The address to use for the link. The text '[Id]' will be replaced with the Id of the entity '[Guid]' will be replaced with the Guid.", false, "/samplepage/[Id]", "", 1, "LinkUrl")]
    [CodeEditorField("Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"<div class=""panel panel-block""> 
    <div class=""panel-heading"">
       <h4 class=""panel-title"">Followed {{ EntityType | Pluralize }}</h4>
    </div>
    <div class=""panel-body"">

        <ul>
        {% for item in FollowingItems %}
            {% if LinkUrl != '' %}
                <li><a href=""{{ LinkUrl | Replace:'[Id]',item.Id }}"">{{ item.Name }}</a></li>
            {% else %}
                <li>{{ item.Name }}</li>
            {% endif %}
        {% endfor %}

        {% if HasMore %}
            <li><i class='fa fa-fw''></i> <small>(showing top {{ Quantity }})</small></li>
        {% endif %}

        </ul>
        
    </div>
</div>", "", 2, "LavaTemplate" )]
    [IntegerField("Max Results", "The maximum number of results to display.", true, 100, order: 4)]
    public partial class FollowingByEntityLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
                LoadContent();
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
            LoadContent();
        }

        #endregion

        #region Methods

        protected void LoadContent()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            
            var entityType = EntityTypeCache.Read(GetAttributeValue("EntityType").AsGuid());

            if ( entityType != null )
            {

                RockContext rockContext = new RockContext();

                int personId = this.CurrentPersonId.Value;

                var followingService = new FollowingService( rockContext );
                IQueryable<IEntity> qryFollowedItems = followingService.GetFollowedItems( entityType.Id, personId );

                int quantity = GetAttributeValue( "MaxResults" ).AsInteger();
                var items = qryFollowedItems.Take(quantity + 1).ToList();

                bool hasMore = (quantity < items.Count);

                mergeFields.Add( "FollowingItems", items.Take( quantity ) );
                mergeFields.Add( "HasMore", hasMore );
                mergeFields.Add( "EntityType", entityType.FriendlyName );
                mergeFields.Add( "LinkUrl", GetAttributeValue( "LinkUrl" ) );
                mergeFields.Add( "Quantity", quantity );

                string template = GetAttributeValue( "LavaTemplate" );
                lContent.Text = template.ResolveMergeFields( mergeFields );

            }
            else
            {
                lContent.Text = string.Format( "<div class='alert alert-warning'>Please configure an entity in the block settings." );

            }
        }

        #endregion
    }
}