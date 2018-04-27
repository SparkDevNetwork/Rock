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
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display a specific content channel item.
    /// </summary>
    [DisplayName( "Content Channel View Detail" )]
    [Category( "CMS" )]
    [Description( "Block to display a specific content channel item." )]

    // Block Properties
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this content channel item block.", false, order: 0 )]

    // Custom Settings
    [ContentChannelTypeField( "Content Channel Type", "The content channel type that the content channel item ", false, "", "CustomSetting" )]
    [CodeEditorField( "Lava Template", "The template to use when formatting the content channel item.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, category: "CustomSetting", defaultValue: @"
<h1>{{ Item.Title }}</h1>
{{ Item.Content }}" )]

    [IntegerField( "Output Cache Duration", "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", false, 0, "CustomSetting", 0, "OutputCacheDuration" )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the channel name or content item.", false, "CustomSetting" )]

    [TextField( "Meta Description Attribute", "Attribute to use for storing the description attribute.", false, "", "CustomSetting" )]
    //[TextField( "Meta Image Attribute", "Attribute to use for storing the image attribute.", false, "", "CustomSetting" )]

    [InteractionChannelField( "Interaction Channel", "", category: "CustomSetting" )]
    [TextField( "Interaction Operation", "", category: "CustomSetting", defaultValue: "View" )]
    public partial class ContentChannelViewDetail : RockBlockCustomSettings
    {

        #region Settings

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlSettings.Visible = true;

            // TODO

            mdSettings.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSettings_SaveClick( object sender, EventArgs e )
        {
            // TODO

            mdSettings.Hide();
            pnlSettings.Visible = false;

            // reload the page to make sure we have a clean load
            NavigateToCurrentPageReference();
        }

        #endregion Settings
    }
}