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


    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this content channel item block.", false, category: "CustomSetting" )]

    [ContentChannelField( "Content Channel", "Limits content channel items to a specific channel, or leave blank to leave unrestricted.", false, "", category: "CustomSetting" )]
    [TextField( "Content Channel Query Parameter", @"
Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is. 
The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid

", required: false, category: "CustomSetting" )]


    [CodeEditorField( "Lava Template", "The template to use when formatting the content channel item.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, category: "CustomSetting", defaultValue: @"
<h1>{{ Item.Title }}</h1>
{{ Item.Content }}" )]

    [IntegerField( "Output Cache Duration", "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", required: false, key: "OutputCacheDuration", category: "CustomSetting" )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the channel name or content item.", category: "CustomSetting" )]

    [InteractionChannelField( "Interaction Channel", "The Interaction Channel to log interactions to. Leave blank to not log interactions.", category: "CustomSetting", required: false )]
    [TextField( "Interaction Operation", "", defaultValue: "View", category: "Interactions" )]
    [BooleanField( "Write Interaction Only If Individual Logged In", "Set to true to only write interactions for logged in users, or set to false to write interactions for both logged in and anonymous users.", category: "CustomSetting" )]

    [WorkflowTypeField( "Workflow Type", "The workflow type to launch when the content is viewed.", category: "CustomSetting" )]
    [BooleanField( "Launch Workflow Only If Individual Logged In", "Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users.", category: "CustomSetting" )]
    [BooleanField( "Launch Workflow Once Per Person", "This setting keeps the workflow from launching on subsequent views by the same person. For this to remain accurate the workflows of this type should not be deleted.", category: "CustomSetting" )]

    [TextField( "Meta Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Type", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Title Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Image Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Title Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Title Description", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Title Image", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Title Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Card", required: false, category: "CustomSetting" )]
    public partial class ContentChannelViewDetail : RockBlockCustomSettings
    {
        #region Settings

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlSettings.Visible = true;
            ddlContentChannel.Items.Clear();
            ddlContentChannel.Items.Add( new ListItem() );
            foreach ( var contentChannel in CacheContentChannel.All().OrderBy( a => a.Name ) )
            {
                ddlContentChannel.Items.Add( new ListItem( contentChannel.Name, contentChannel.Guid.ToString() ) );
            }

            ddlContentChannel.SetValue( this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull() );

            tbContentChannelQueryParameter.Text = this.GetAttributeValue( "ContentChannelQueryParameter" );
            ceLavaTemplate.Text = this.GetAttributeValue( "LavaTemplate" );
            rbOutputCacheDuration.Text = this.GetAttributeValue( "OutputCacheDuration" );
            cbSetPageTitle.Checked = this.GetAttributeValue( "SetPageTitle" ).AsBoolean();

            ddlInteractionChannel.Items.Clear();
            ddlInteractionChannel.Items.Add( new ListItem() );
            foreach ( var interactionChannel in CacheInteractionChannel.All().OrderBy( a => a.Name ) )
            {
                ddlInteractionChannel.Items.Add( new ListItem( interactionChannel.Name, interactionChannel.Guid.ToString() ) );
            }


            tbInteractionOperation.Text = this.GetAttributeValue( "InteractionOperation" );
            cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = this.GetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn" ).AsBoolean();

            //ddlMetaDescriptionAttribute


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

        protected void ddlContentChannel_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        protected void ddlInteractionChannel_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        protected void wtpWorkflowType_SelectItem( object sender, EventArgs e )
        {

        }
    }
}