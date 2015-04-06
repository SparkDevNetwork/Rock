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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Data;
using Rock.Web.Cache;
using System.Text;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Adds an editable HTML fragment to the page.
    /// </summary>
    [DisplayName( "HTML Content" )]
    [Category( "CMS" )]
    [Description( "Adds an editable HTML fragment to the page." )]

    [SecurityAction( Authorization.EDIT, "The roles and/or users that can edit the HTML content.")]
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve HTML content." )]

    [BooleanField( "Use Code Editor", "Use the code editor instead of the WYSIWYG editor", true, "", 0 )]
    [TextField("Document Root Folder", "The folder to use as the root when browsing or uploading documents.", false, "~/Content", "", 1 )]
    [TextField( "Image Root Folder", "The folder to use as the root when browsing or uploading images.", false, "~/Content", "", 2 )]
    [BooleanField( "User Specific Folders", "Should the root folders be specific to current user?", false, "", 3 )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 3600, "", 4 )]
    [TextField( "Context Parameter", "Query string parameter to use for 'personalizing' content based on unique values.", false, "", "", 5 )]
    [TextField( "Context Name", "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", false, "", "", 6 )]
    [BooleanField( "Enable Versioning", "If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", false, "", 7, "SupportVersions" )]
    [BooleanField( "Require Approval", "Require that content be approved?", false, "", 8 )]
    [BooleanField( "Enable Debug", "Show lava merge fields.", false, "", 9 )]

    [ContextAware]
    public partial class HtmlContentDetail : RockBlockCustomSettings
    {

        #region Properties

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit HTML";
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += HtmlContentDetail_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlHtmlContent );

            gVersions.GridRebind += gVersions_GridRebind;
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
            else
            {
                nbApprovalRequired.Visible = false;
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
            bool supportsVersioning = GetAttributeValue( "SupportVersions" ).AsBoolean();
            bool requireApproval = GetAttributeValue( "RequireApproval" ).AsBoolean();
            if ( requireApproval && ! supportsVersioning )
            {
                SetAttributeValue( "SupportVersions", "true" );
                SaveAttributeValues();
            }

            HtmlContentService.FlushCachedContent( this.BlockId, EntityValue() );
            
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            bool supportVersioning = GetAttributeValue( "SupportVersions" ).AsBoolean();
            bool requireApproval = GetAttributeValue( "RequireApproval" ).AsBoolean();

            var rockContext = new RockContext();
            HtmlContentService htmlContentService = new HtmlContentService( rockContext );

            // get settings
            string entityValue = EntityValue();

            // get current content
            int version = hfVersion.ValueAsInt();
            HtmlContent htmlContent = htmlContentService.GetByBlockIdAndEntityValueAndVersion( this.BlockId, entityValue, version );

            // get the content depending on which mode we are in (codeeditor or ckeditor)
            string newContent = ceHtml.Visible ? ceHtml.Text : htmlEditor.Text;

            //// create a new record only in the following situations:
            ////   - this is the first time this htmlcontent block got content (new block and edited for the first time)
            ////   - the content was changed, versioning is enabled, and OverwriteVersion is not checked
            
            // if the existing content changed, and the overwrite option was not checked, create a new version
            if (htmlContent != null)
            {
                // Editing existing content. Check if content has changed
                if (htmlContent.Content != newContent)
                {
                    // The content has changed (different than database). Check if versioning is enabled
                    if (supportVersioning && !cbOverwriteVersion.Checked)
                    {
                        //// versioning is enabled, and they didn't choose to overwrite
                        //// set to null so that we'll create a new record
                        htmlContent = null;
                    }
                }
            }

            // if a record doesn't exist then create one
            if ( htmlContent == null )
            {
                htmlContent = new HtmlContent();
                htmlContent.BlockId = this.BlockId;
                htmlContent.EntityValue = entityValue;

                if ( supportVersioning )
                {
                    int? maxVersion = GetMaxVersionOfHtmlContent();

                    htmlContent.Version = maxVersion.HasValue ? maxVersion.Value + 1 : 1;
                }
                else
                {
                    htmlContent.Version = 1;
                }

                htmlContentService.Add( htmlContent );
            }

            htmlContent.StartDateTime = drpDateRange.LowerValue;
            htmlContent.ExpireDateTime = drpDateRange.UpperValue;
            bool currentUserCanApprove = IsUserAuthorized( "Approve" );

            if ( !requireApproval )
            {
                // if this block doesn't require Approval, mark it as approved
                htmlContent.IsApproved = true;
                htmlContent.ApprovedByPersonAliasId = CurrentPersonAliasId;
                htmlContent.ApprovedDateTime = RockDateTime.Now;
            }
            else
            {
                // since this content requires Approval, mark it as approved ONLY if the current user can approve
                // and they set the hfApprovalStatus flag to true.
                if ( currentUserCanApprove && hfApprovalStatus.Value.AsBoolean() )
                {
                    htmlContent.IsApproved = true;
                    htmlContent.ApprovedByPersonAliasId = CurrentPersonAliasId;
                    htmlContent.ApprovedDateTime = RockDateTime.Now;
                }
                else
                {
                    // if the content has changed
                    if ( htmlContent.Content != newContent )
                    {
                        nbApprovalRequired.Visible = true;
                        htmlContent.IsApproved = false;
                    }
                }
            }

            htmlContent.Content = newContent;

            rockContext.SaveChanges();

            // flush cache content 
            HtmlContentService.FlushCachedContent( htmlContent.BlockId, htmlContent.EntityValue );

            ShowView();
        }

        /// <summary>
        /// Handles the GridRebind event of the gVersions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gVersions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the SelectVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void SelectVersion_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            HtmlContent htmlContent = new HtmlContentService( new RockContext() ).Get( e.RowKeyId );
            pnlVersionGrid.Visible = false;
            pnlEdit.Visible = true;
            ShowEditDetail( htmlContent );
        }

        /// <summary>
        /// Handles the Click event of the lbShowVersionGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowVersionGrid_Click( object sender, EventArgs e )
        {
            // make sure grid goes back to first page
            gVersions.PageIndex = 0;
            BindGrid();
            pnlVersionGrid.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbReturnToEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbReturnToEdit_Click( object sender, EventArgs e )
        {
            pnlVersionGrid.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbApprove_Click( object sender, EventArgs e )
        {
            SetApprovalValues( true, CurrentPersonAlias );
        }

        /// <summary>
        /// Handles the Click event of the lbDeny control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeny_Click( object sender, EventArgs e )
        {
            SetApprovalValues( false, CurrentPersonAlias );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var htmlContentService = new HtmlContentService( new RockContext() );
            var content = htmlContentService.GetContent( this.BlockId, EntityValue() ).OrderByDescending( a => a.Version ).ThenByDescending( a => a.ModifiedDateTime ).ToList();

            var versions = content.Select( v =>
                new
                {
                    v.Id,
                    v.Version,
                    VersionText = "Version " + v.Version.ToString(),
                    ModifiedDateTime = "(" + v.ModifiedDateTime.ToElapsedString() + ")",
                    ModifiedByPerson = v.ModifiedByPersonAlias != null ? v.ModifiedByPersonAlias.Person : null,
                    Approved = v.IsApproved,
                    ApprovedByPerson = v.ApprovedByPersonAlias != null ? v.ApprovedByPersonAlias.Person : null,
                    v.StartDateTime,
                    v.ExpireDateTime
                } ).ToList();

            gVersions.EntityTypeId = EntityTypeCache.Read<HtmlContent>().Id;
            gVersions.DataSource = versions;
            
            gVersions.DataBind();
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ShowSettings()
        {
            // only enable viewstate for htmlEditor when needed (it is really big)
            pnlEdit.EnableViewState = true;

            pnlEdit.Visible = true;
            pnlVersionGrid.Visible = false;
            pnlEditModel.Visible = true;
            upnlHtmlContent.Update();
            mdEdit.Show();

            bool useCodeEditor = GetAttributeValue( "UseCodeEditor" ).AsBoolean();

            ceHtml.Visible = useCodeEditor;
            htmlEditor.Visible = !useCodeEditor;

            htmlEditor.Toolbar = HtmlEditor.ToolbarConfig.Full;

            // if the current user can't approve their own edits, set the approval to Not-Approved when they change something
            if ( !IsUserAuthorized( "Approve" ) )
            {
                string onchangeScriptFormat = @"
   $('#{0}').removeClass('label label-success label-danger').addClass('label label-danger');
   $('#{0}').text('Not-Approved');
   $('#{1}').val('false');
   $('#{2}').val('');
   $('#{3}').hide();";

                string onchangeScript = string.Format( onchangeScriptFormat, lblApprovalStatus.ClientID, hfApprovalStatus.ClientID, hfApprovalStatusPersonId.ClientID, lblApprovalStatusPerson.ClientID );

                htmlEditor.OnChangeScript = onchangeScript;
                ceHtml.OnChangeScript = onchangeScript;
            }

            htmlEditor.MergeFields.Clear();
            htmlEditor.MergeFields.Add( "GlobalAttribute" );
            htmlEditor.MergeFields.Add( "CurrentPerson^Rock.Model.Person|Current Person" );
            htmlEditor.MergeFields.Add( "Campuses" );
            htmlEditor.MergeFields.Add( "PageParameter" );
            htmlEditor.MergeFields.Add( "RockVersion" );
            htmlEditor.MergeFields.Add( "Date" );
            htmlEditor.MergeFields.Add( "Time" );
            htmlEditor.MergeFields.Add( "DayOfWeek" );

            ceHtml.MergeFields.Clear();
            ceHtml.MergeFields.Add( "GlobalAttribute" );
            ceHtml.MergeFields.Add( "CurrentPerson^Rock.Model.Person|Current Person" );
            ceHtml.MergeFields.Add( "Campuses" );
            ceHtml.MergeFields.Add( "RockVersion" );
            ceHtml.MergeFields.Add( "PageParameter" );
            ceHtml.MergeFields.Add( "Date" );
            ceHtml.MergeFields.Add( "Time" );
            ceHtml.MergeFields.Add( "DayOfWeek" );

            var contextObjects = new Dictionary<string, object>();
            foreach ( var contextEntityType in RockPage.GetContextEntityTypes() )
            {
                var contextEntity = RockPage.GetCurrentContext( contextEntityType );
                if ( contextEntity != null && contextEntity is Rock.Lava.ILiquidizable )
                {
                    var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );
                    if ( type != null )
                    {
                        string mergeField = string.Format( "Context.{0}^{1}|Current {0} (Context)|Context", type.Name, type.FullName );
                        htmlEditor.MergeFields.Add( mergeField );
                        ceHtml.MergeFields.Add( mergeField );
                    }
                }
            }

            string documentRoot = GetAttributeValue( "DocumentRootFolder" );
            string imageRoot = GetAttributeValue( "ImageRootFolder" );
            htmlEditor.UserSpecificRoot = GetAttributeValue( "UserSpecificFolders" ).AsBoolean();
            htmlEditor.DocumentFolderRoot = documentRoot;
            htmlEditor.ImageFolderRoot = imageRoot;

            bool supportsVersioning = GetAttributeValue( "SupportVersions" ).AsBoolean();
            bool requireApproval = GetAttributeValue( "RequireApproval" ).AsBoolean();

            lVersion.Visible = supportsVersioning;
            lbShowVersionGrid.Visible = supportsVersioning;
            cbOverwriteVersion.Visible = supportsVersioning;
            cbOverwriteVersion.Checked = false;

            // RequireApproval only applies if SupportsVersioning=True
            upnlApproval.Visible = supportsVersioning && requireApproval;
            lbApprove.Enabled = IsUserAuthorized( "Approve" );
            lbDeny.Enabled = IsUserAuthorized( "Approve" );

            string entityValue = EntityValue();
            HtmlContent htmlContent = new HtmlContentService( new RockContext() ).GetActiveContent( this.BlockId, entityValue );

            // set Height of editors
            if ( supportsVersioning && requireApproval )
            {
                ceHtml.EditorHeight = "280";
                htmlEditor.Height = 280;
            }
            else if ( supportsVersioning )
            {
                ceHtml.EditorHeight = "350";
                htmlEditor.Height = 350;
            }
            else
            {
                ceHtml.EditorHeight = "380";
                htmlEditor.Height = 380;
            }

            ShowEditDetail( htmlContent );
        }

        /// <summary>
        /// Shows the edit detail.
        /// </summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        private void ShowEditDetail( HtmlContent htmlContent )
        {
            if ( htmlContent == null )
            {
                htmlContent = new HtmlContent();
            }

            int? maxVersion = GetMaxVersionOfHtmlContent();

            hfVersion.Value = htmlContent.Version.ToString();
            if ( maxVersion.HasValue && maxVersion.Value != htmlContent.Version )
            {
                lVersion.Text = string.Format( "Version {0} <small>of {1}</small> | ", htmlContent.Version, maxVersion.Value );
            }
            else
            {
                lVersion.Text = string.Format( "Version {0} | ", htmlContent.Version );
            }

            SetApprovalValues( htmlContent.IsApproved, htmlContent.ApprovedByPersonAlias );

            drpDateRange.LowerValue = htmlContent.StartDateTime;
            drpDateRange.UpperValue = htmlContent.ExpireDateTime;
            htmlEditor.Text = htmlContent.Content;
            ceHtml.Text = htmlContent.Content;
        }

        /// <summary>
        /// Sets the approval values.
        /// </summary>
        /// <param name="approved">if set to <c>true</c> [approved].</param>
        /// <param name="person">The person.</param>
        private void SetApprovalValues( bool approved, PersonAlias personAlias )
        {
            string cssClass = string.Empty;

            if ( approved )
            {
                cssClass = "label label-success";
            }
            else
            {
                cssClass = "label label-danger";
            }

            lblApprovalStatus.Text = string.Format( "<span class='{0}'>{1}</span>", cssClass, approved ? "Approved" : "Not-Approved" );

            hfApprovalStatus.Value = approved.ToTrueFalse();

            if ( personAlias != null && personAlias.Person != null )
            { 
                lblApprovalStatusPerson.Visible = true;
                lblApprovalStatusPerson.Text = "by " + personAlias.Person.FullName;
                hfApprovalStatusPersonId.Value = personAlias.Person.Id.ToString();
            }
            else
            {
                lblApprovalStatusPerson.Visible = false;
            }
        }

        /// <summary>
        /// Gets the maximum version that this HtmlContent block 
        /// </summary>
        /// <returns></returns>
        private int? GetMaxVersionOfHtmlContent()
        {
            string entityValue = this.EntityValue();
            int? maxVersion = new HtmlContentService( new RockContext() ).Queryable()
                .Where( c => c.BlockId == this.BlockId && c.EntityValue == entityValue )
                .Select( c => (int?)c.Version ).Max();
            return maxVersion;
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        protected void ShowView()
        {
            mdEdit.Hide();
            pnlEditModel.Visible = false;
            upnlHtmlContent.Update();

            // prevent htmlEditor from using viewstate when not needed
            pnlEdit.EnableViewState = false;

            pnlEdit.Visible = false;
            pnlVersionGrid.Visible = false;
            string entityValue = EntityValue();
            string html = string.Empty;

            string cachedContent = HtmlContentService.GetCachedContent( this.BlockId, entityValue );

            // if content not cached load it from DB
            if ( cachedContent == null )
            {
                var rockContext = new RockContext();
                var htmlContentService = new HtmlContentService( rockContext );
                HtmlContent content = htmlContentService.GetActiveContent( this.BlockId, entityValue );

                if ( content != null )
                {
                    bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();

                    if ( content.Content.HasMergeFields() || enableDebug )
                    {
                        var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
                        if ( CurrentPerson != null )
                        {
                            // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                            mergeFields.Add( "Person", CurrentPerson );
                            mergeFields.Add( "CurrentPerson", CurrentPerson );
                        }

                        mergeFields.Add( "Campuses", CampusCache.All() );
                        mergeFields.Add( "PageParameter", PageParameters() );

                        var contextObjects = new Dictionary<string, object>();
                        foreach( var contextEntityType in RockPage.GetContextEntityTypes() )
                        {
                            var contextEntity = RockPage.GetCurrentContext( contextEntityType );
                            if (contextEntity != null && contextEntity is DotLiquid.ILiquidizable)
                            {
                                var type = Type.GetType(contextEntityType.AssemblyName ?? contextEntityType.Name);
                                if (type != null)
                                {
                                    contextObjects.Add( type.Name, contextEntity );
                                }
                            }

                        }

                        if ( contextObjects.Any() )
                        {
                            mergeFields.Add( "Context", contextObjects );
                        }

                        mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

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

        #endregion
    }
}