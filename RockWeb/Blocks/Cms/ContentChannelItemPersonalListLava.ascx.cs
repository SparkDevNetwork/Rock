// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    /// Displays a list of active users of a website.
    /// </summary>
    [DisplayName( "Content Channel Item Personal List Lava" )]
    [Category( "CMS" )]
    [Description( "Displays a list of content items for the person using a Lava template." )]

    [ContentChannelField("Content Channel", "The content channel to filter on. If blank all content items for the user will be displayed.", false, order:0)]
    [IntegerField( "Max Items", "The maximum number of items to display (default 10)", false, 10, order: 1 )]
    [LinkedPage( "Detail Page", "Page reference to the detail page. This will be included as a variable in the Lava.", false, order: 2 )]
    [CodeEditorField("Lava Template", "The Lava template to use.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, order: 3)]
    [BooleanField( "Enable Debug", "Show merge data to help you see what's available to you.", order: 4 )]
    public partial class ContentChannelItemPersonalListLava : Rock.Web.UI.RockBlock
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

            DisplayItems();
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

            Guid? contentChannelGuid = GetAttributeValue( "ContentChannel").AsGuidOrNull();
            ContentChannel contentChannel = null;

            ContentChannelItemService itemService = new ContentChannelItemService( rockContext );
            var items = itemService.Queryable().AsNoTracking().Where(c => c.CreatedByPersonAlias != null && c.CreatedByPersonAlias.PersonId == CurrentPersonId);

            if ( contentChannelGuid.HasValue )
            {
                items = items.Where( c => c.ContentChannel.Guid == contentChannelGuid.Value );

                contentChannel = new ContentChannelService( rockContext ).Get( contentChannelGuid.Value );
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "DetailPage", LinkedPageUrl( "DetailPage", null ) );
            mergeFields.Add( "ContentChannel", contentChannel );    
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "Items", items.Take(GetAttributeValue( "MaxItems" ).AsInteger()).ToList() );
            
            string template = GetAttributeValue( "LavaTemplate" );

            lContent.Text = template.ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
        }

        #endregion
    }
}