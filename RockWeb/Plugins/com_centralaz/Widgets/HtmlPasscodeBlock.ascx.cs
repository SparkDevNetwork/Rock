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
using System.Web.UI.WebControls;
using System.IO;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    
    [DisplayName( "Html Passcode Block" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Allows a user to enter a password to view security protected content." )]

    [SecurityAction( Authorization.EDIT, "The roles and/or users that can edit the HTML content." )]
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve HTML content." )]

    [TextField( "Passcode", "The password that is used to view content", true, "", "CustomSetting" )]
    [IntegerField( "Block Id", "The Id of the block whose content we're viewing", true, category: "CustomSetting" )]

    public partial class HtmlPasscodeBlock : RockBlockCustomSettings
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int? blockId = GetAttributeValue( "BlockId" ).AsIntegerOrNull();
            if ( !blockId.HasValue || String.IsNullOrWhiteSpace( GetAttributeValue( "Passcode" ) ) )
            {
                nbConfiguration.Text = "This block is not yet configured";
                nbConfiguration.Visible = true;
            }

            this.BlockUpdated += HtmlContentDetail_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlHtmlContent );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !this.IsPostBack )
            {
                ShowView();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the HtmlContentDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HtmlContentDetail_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the lbOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOk_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "Passcode", tbPasscodeAttribute.Text );
            SetAttributeValue( "BlockId", ddlBlock.SelectedValue );
            SaveAttributeValues();

            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnEnterPasscode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEnterPasscode_Click( object sender, EventArgs e )
        {
            if ( tbPasscode.Text == GetAttributeValue( "Passcode" ) )
            {
                int? blockId = GetAttributeValue( "BlockId" ).AsIntegerOrNull();
                if ( blockId.HasValue )
                {
                    ShowContent( blockId.Value );
                }
            }
            else
            {
                lHtmlContent.Text = @"
<div class='alert alert-warning'>
    <p>Sorry, that's not the correct passcode.</p>
</div>";
            }
            upnlHtmlContent.Update();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ShowSettings()
        {
            tbPasscodeAttribute.Text = GetAttributeValue( "Passcode" );
            ddlBlock.Items.Clear();
            foreach ( var block in this.RockPage.RockBlocks )
            {
                var rockBlock = new BlockService( new RockContext() ).Get( block.BlockId );
                if ( rockBlock.BlockType.Guid == Rock.SystemGuid.BlockType.HTML_CONTENT.AsGuid() )
                {
                    ddlBlock.Items.Add( new ListItem( rockBlock.Name, rockBlock.Id.ToString() ) );
                }
            }
            string currentBlockIdAttribute = GetAttributeValue( "BlockId" ) ?? string.Empty;
            foreach ( ListItem item in ddlBlock.Items )
            {
                item.Selected = ( item.Value == currentBlockIdAttribute );
            }
            pnlEdit.Visible = true;
            upnlHtmlContent.Update();
            mdEdit.Show();
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        protected void ShowView()
        {
            mdEdit.Hide();
            pnlEdit.Visible = false;
            upnlHtmlContent.Update();

            pnlEdit.Visible = false;

        }

        /// <summary>
        /// Shows the content.
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        private void ShowContent( int blockId )
        {

            string entityValue = EntityValue();
            string html = string.Empty;

            string cachedContent = HtmlContentService.GetCachedContent( blockId, entityValue );

            // if content not cached load it from DB
            if ( cachedContent == null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var htmlContentService = new HtmlContentService( rockContext );
                    HtmlContent content = htmlContentService.GetActiveContent( blockId, entityValue );

                    if ( content != null )
                    {
                        bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();

                        if ( content.Content.HasMergeFields() || enableDebug )
                        {
                            var contextObjects = new Dictionary<string, object>();
                            foreach ( var contextEntityType in RockPage.GetContextEntityTypes() )
                            {
                                var contextEntity = RockPage.GetCurrentContext( contextEntityType );
                                if ( contextEntity != null && contextEntity is DotLiquid.ILiquidizable )
                                {
                                    var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );
                                    if ( type != null )
                                    {
                                        contextObjects.Add( type.Name, contextEntity );
                                    }
                                }
                            }

                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                            mergeFields.Add( "CurrentPage", GetPageProperties() );
                            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                            mergeFields.Add( "CurrentPersonCanEdit", IsUserAuthorized( Authorization.EDIT ) );
                            mergeFields.Add( "CurrentPersonCanAdministrate", IsUserAuthorized( Authorization.ADMINISTRATE ) );

                            if ( contextObjects.Any() )
                            {
                                mergeFields.Add( "Context", contextObjects );
                            }

                            html = content.Content.ResolveMergeFields( mergeFields );

                            // show merge fields if enable debug true
                            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
                            {
                                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                                mergeFields.Remove( "Person" );
                                html += mergeFields.lavaDebugInfo();
                            }
                        }
                        else
                        {
                            html = content.Content;
                        }
                    }
                    else
                    {
                        html = string.Empty;
                    }
                }

                // Resolve any dynamic url references
                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                html = html.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                // cache content
                int cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                if ( cacheDuration > 0 )
                {
                    HtmlContentService.AddCachedContent( this.BlockId, entityValue, html, cacheDuration );
                }
            }
            else
            {
                html = cachedContent;
            }

            // add content to the content window
            lHtmlContent.Text = html;
        }

        /// <summary>
        /// Entities the value.
        /// </summary>
        /// <returns></returns>
        private string EntityValue()
        {
            string entityValue = string.Empty;

            string contextParameter = GetAttributeValue( "ContextParameter" );
            if ( !string.IsNullOrEmpty( contextParameter ) )
            {
                entityValue = string.Format( "{0}={1}", contextParameter, PageParameter( contextParameter ) ?? string.Empty );
            }

            string contextName = GetAttributeValue( "ContextName" );
            if ( !string.IsNullOrEmpty( contextName ) )
            {
                entityValue += "&ContextName=" + contextName;
            }

            return entityValue;
        }

        /// <summary>
        /// Gets the page properties.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetPageProperties()
        {
            Dictionary<string, object> pageProperties = new Dictionary<string, object>();
            pageProperties.Add( "Id", this.RockPage.PageId.ToString() );
            pageProperties.Add( "BrowserTitle", this.RockPage.BrowserTitle );
            pageProperties.Add( "PageTitle", this.RockPage.PageTitle );
            pageProperties.Add( "Site", this.RockPage.Site.Name );
            pageProperties.Add( "SiteId", this.RockPage.Site.Id.ToString() );
            pageProperties.Add( "LayoutId", this.RockPage.Layout.Id.ToString() );
            pageProperties.Add( "Layout", this.RockPage.Layout.Name );
            pageProperties.Add( "SiteTheme", this.RockPage.Site.Theme );
            pageProperties.Add( "PageIcon", this.RockPage.PageIcon );
            pageProperties.Add( "Description", this.RockPage.MetaDescription );
            return pageProperties;
        }
        #endregion
    }
}