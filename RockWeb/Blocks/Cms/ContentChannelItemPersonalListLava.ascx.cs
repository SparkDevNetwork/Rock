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
using System.Text;
using System.Web;
using System.Data.Entity;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Displays a list of content items for the person using a Lava template
    /// </summary>
    [DisplayName( "Content Channel Item Personal List Lava" )]
    [Category( "CMS" )]
    [Description( "Displays a list of content items for the person using a Lava template." )]

    #region Block Attributes

    [ContentChannelField(
        "Content Channel",
        Description = "The content channel to filter on. If blank all content items for the user will be displayed.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.ContentChannel )]
    [IntegerField(
        "Max Items",
        Description = "The maximum number of items to display (default 10)",
        IsRequired = false,
        DefaultIntegerValue = 10,
        Order = 1,
        Key = AttributeKey.MaxItems )]
    [LinkedPage(
        "Detail Page",
        Description = "Page reference to the detail page. This will be included as a variable in the Lava.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.DetailPage )]
    [CodeEditorField(
        "Lava Template",
        Description = "The Lava template to use.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 300,
        Order = 3,
        DefaultValue = LavaTemplateDefaultValue,
        Key = AttributeKey.LavaTemplate )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "13E4D4B5-0929-4ED6-9E59-05A6D511FA06" )]
    public partial class ContentChannelItemPersonalListLava : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ContentChannel = "ContentChannel";
            public const string MaxItems = "MaxItems";
            public const string DetailPage = "DetailPage";
            public const string LavaTemplate = "LavaTemplate";
        }

        #endregion Attribute Keys

        #region constants

        protected const string LavaTemplateDefaultValue = @"{% assign itemCount = Items | Size %}

{% if itemCount > 0 %}
    <div class='panel panel-default'> 
        <div class='panel-heading'>
           <h5 class='panel-title'>Content Channel Items for {{ CurrentPerson.FullName }}</h5>
        </div>
        <div class='panel-body'>
            <ul>
                {% for item in Items %}
                <li>
                    {% if DetailPage != '' %}
                        <a href = '{{ DetailPage }}?contentItemId={{ item.Id }}' >{{ item.Title }}</a>
                    {% else %}
                        {{ item.Title }}
                    {% endif %}
                    
                    {% case item.Status %}
                        {% when 'PendingApproval' %}
                            <span class='label label-warning'>Pending</span>
                        {% when 'Approved' %}
                            <span class='label label-success'>Approved</span>
                        {% when 'Denied' %}
                            <span class='label label-danger'>Denied</span>
                    {% endcase %}
                </li>
                {% endfor %}
            <ul>
        </div>
    </div>
{% endif %}";

        #endregion Constants
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
            DisplayItems();

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the active users control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayItems();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the active users.
        /// </summary>
        private void DisplayItems()
        {
            RockContext rockContext = new RockContext();

            Guid? contentChannelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            ContentChannel contentChannel = null;

            ContentChannelItemService itemService = new ContentChannelItemService( rockContext );
            var items = itemService.Queryable().AsNoTracking().Where(c => c.CreatedByPersonAlias != null && c.CreatedByPersonAlias.PersonId == CurrentPersonId);

            if ( contentChannelGuid.HasValue )
            {
                items = items.Where( c => c.ContentChannel.Guid == contentChannelGuid.Value );

                contentChannel = new ContentChannelService( rockContext ).Get( contentChannelGuid.Value );
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "DetailPage", LinkedPageRoute( AttributeKey.DetailPage ) );
            mergeFields.Add( "ContentChannel", contentChannel );    
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "Items", items.Take(GetAttributeValue( AttributeKey.MaxItems ).AsInteger()).ToList() );
            
            string template = GetAttributeValue( AttributeKey.LavaTemplate );

            lContent.Text = template.ResolveMergeFields( mergeFields );

        }

        #endregion
    }
}