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
using HtmlAgilityPack;
using System.Web;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Adds an editable HTML fragment to the page.
    /// </summary>
    [DisplayName( "HTML Content" )]
    [Category( "CMS" )]
    [Description( "Adds an editable HTML fragment to the page." )]

    #region Block Attributes

    [SecurityAction(
        Authorization.EDIT,
        "The roles and/or users that can edit the HTML content." )]

    [SecurityAction(
        Authorization.APPROVE,
        "The roles and/or users that have access to approve HTML content." )]

    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EnabledLavaCommands )]

    [BooleanField(
        "Start in Code Editor mode",
        Description = "Start the editor in code editor mode instead of WYSIWYG editor mode.",
        DefaultBooleanValue = true,
        Key = AttributeKey.UseCodeEditor,
        Order = 1 )]

    [TextField(
        "Document Root Folder",
        Description = "The folder to use as the root when browsing or uploading documents.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Order = 2,
        Key = AttributeKey.DocumentRootFolder )]

    [TextField(
        "Image Root Folder",
        Description = "The folder to use as the root when browsing or uploading images.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Order = 3,
        Key = AttributeKey.ImageRootFolder )]
    [BooleanField(
        "User Specific Folders",
        Description = "Should the root folders be specific to current user?",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.UserSpecificFolders )]
    [IntegerField(
        "Cache Duration",
        Description = "Number of seconds to cache the content.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 5,
        Key = AttributeKey.CacheDuration )]
    [TextField(
        "Context Parameter",
        Description = "Query string parameter to use for 'personalizing' content based on unique values.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.ContextParameter )]

    [TextField(
        "Context Name",
        Description = "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share HTML values.",
        IsRequired = false,
        Order = 7,
        Key = AttributeKey.ContextName )]
    [BooleanField(
        "Enable Versioning",
        Description = "If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.",
        DefaultBooleanValue = false,
        Order = 8,
        Key = AttributeKey.SupportVersions )]
    [BooleanField(
        "Require Approval",
        Description = "Require that content be approved?",
        DefaultBooleanValue = false,
        Order = 9 )]
    [CustomCheckboxListField(
        "Cache Tags",
        Description = "Cached tags are used to link cached content so that it can be expired as a group",
        ListSource = CACHE_TAG_LIST,
        IsRequired = false,
        Key = AttributeKey.CacheTags,
        Order = 10 )]
    // Disable QuickEdit for v7
    //[CustomDropdownListField( "Quick Edit", "Allow quick editing of HTML contents.", "AIREDIT^In Place Editing,DBLCLICK^Double-Click For Edit Dialog", false, "", "", 11, "QuickEdit")]

    [BooleanField(
        "Is Secondary Block",
        Description = "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.",
        DefaultBooleanValue = false,
        Order = 11,
        Key = AttributeKey.IsSecondaryBlock )]

    [ContextAware]
    #endregion Block Attributes
    public partial class HtmlContentDetail : RockBlockCustomSettings, ISecondaryBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string UseCodeEditor = "UseCodeEditor";
            public const string DocumentRootFolder = "DocumentRootFolder";
            public const string ImageRootFolder = "ImageRootFolder";
            public const string UserSpecificFolders = "UserSpecificFolders";
            public const string CacheDuration = "CacheDuration";
            public const string ContextParameter = "ContextParameter";
            public const string ContextName = "ContextName";
            public const string SupportVersions = "SupportVersions";
            public const string RequireApproval = "RequireApproval";
            public const string CacheTags = "CacheTags";
            public const string IsSecondaryBlock = "IsSecondaryBlock";
        }

        #endregion Attribute Keys

        #region Properties

        /// <summary>
        /// Supplies the CustomCheckboxListField "CacheTags" with a list of tags.
        /// </summary>
        private const string CACHE_TAG_LIST = @"
            SELECT CAST([DefinedValue].[Value] AS VARCHAR) AS [Value], [DefinedValue].[Value] AS [Text]
            FROM[DefinedType]
            JOIN[DefinedValue] ON[DefinedType].[Id] = [DefinedValue].[DefinedTypeId]
            WHERE[DefinedType].[Guid] = 'BDF73089-9154-40C1-90E4-74518E9937DC'";

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
            this.AddConfigurationUpdateTrigger( upnlHtmlContentEdit );

            // Disable QuickEdit for v7
            //RegisterScript();

            gVersions.GridRebind += gVersions_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            /*
             * 2020-06-10 - JH
             *
             * In some areas of Rock, we force a full postback (page reload), as opposed to the default
             * partial postback that occurs from within an UpdatePanel. See the 'Cms/EmailForm' Block
             * for an example of forcing a full postback, by way of the 'PostBackTrigger' control:
             *
             * https://github.com/SparkDevNetwork/Rock/blob/b0239d87882d6986afb32bc5a5353dcbfc3edee3/RockWeb/Blocks/Cms/EmailForm.ascx#L87
             * 
             * When this happens, any 'HtmlContentDetail' Blocks on the page lose their content,
             * because the 'this.IsPostBack' check below only returns true for partial postbacks.
             * The fix is to detect if the current postback represents a full postback, and reload
             * the HTML content in this case.
             * 
             * '!ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack'
             * 
             * Reason: Issue #4237
             * https://github.com/SparkDevNetwork/Rock/issues/4237
             */
            if ( !this.IsPostBack || !ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                ShowView();
            }
            else
            {
                nbApprovalRequired.Visible = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        // Disable QuickEdit for v7
        //protected override void OnPreRender( EventArgs e )
        //{
        //    if ( GetAttributeValue( "QuickEdit" ) == "AIREDIT" )
        //    {
        //        hfEntityValue.Value = EntityValue();
        //    }

        //    base.OnPreRender( e );
        //}

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the HtmlContentDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HtmlContentDetail_BlockUpdated( object sender, EventArgs e )
        {
            bool supportsVersioning = GetAttributeValue( AttributeKey.SupportVersions ).AsBoolean();
            bool requireApproval = GetAttributeValue( AttributeKey.RequireApproval ).AsBoolean();
            if ( requireApproval && !supportsVersioning )
            {
                SetAttributeValue( AttributeKey.SupportVersions, "true" );
                SaveAttributeValues();
            }

            HtmlContentService.FlushCachedContent( this.BlockId, EntityValue() );

            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the lbQuickEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        // Disable QuickEdit for v7
        //protected void lbQuickEdit_Click( object sender, EventArgs e )
        //{
        //    ShowSettings();
        //}

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            bool supportVersioning = GetAttributeValue( AttributeKey.SupportVersions ).AsBoolean();
            bool requireApproval = GetAttributeValue( AttributeKey.RequireApproval ).AsBoolean();

            var rockContext = new RockContext();
            HtmlContentService htmlContentService = new HtmlContentService( rockContext );

            // get settings
            string entityValue = EntityValue();

            // get current content
            int version = hfVersion.ValueAsInt();
            HtmlContent htmlContent = htmlContentService.GetByBlockIdAndEntityValueAndVersion( this.BlockId, entityValue, version );

            string newContent = htmlEditor.Text;

            // check if the new content is valid
            // NOTE: This is a limited check that will only warn of invalid HTML the first
            // time a user clicks the save button. Any errors encountered on the second runthrough
            // are assumed to be intentional.
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml( newContent );

            if ( doc.ParseErrors.Count() > 0 && !nbInvalidHtml.Visible )
            {
                var reasons = doc.ParseErrors.Select( r => r.Reason ).ToList();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine( "Warning: The HTML has the following errors:<ul>" );
                foreach ( var reason in reasons )
                {
                    sb.AppendLine( String.Format( "<li>{0}</li>", reason.EncodeHtml() ) );
                }
                sb.AppendLine( "</ul> <br/> If you wish to save anyway, click the save button again." );
                nbInvalidHtml.Text = sb.ToString();
                nbInvalidHtml.Visible = true;
                return;
            }

            //// create a new record only in the following situations:
            ////   - this is the first time this htmlcontent block got content (new block and edited for the first time)
            ////   - the content was changed, versioning is enabled, and OverwriteVersion is not checked

            // if the existing content changed, and the overwrite option was not checked, create a new version
            if ( htmlContent != null )
            {
                // Editing existing content. Check if content has changed
                if ( htmlContent.Content != newContent )
                {
                    // The content has changed (different than database). Check if versioning is enabled
                    if ( supportVersioning && !cbOverwriteVersion.Checked )
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

        // Disable QuickEdit for v7
        //        private void RegisterScript()
        //        {
        //            if ( UserCanEdit )
        //            {
        //                string script = "";
        //                if ( GetAttributeValue( "QuickEdit" ) == "DBLCLICK" )
        //                {
        //                    script = string.Format( @"
        //    Sys.Application.add_load( function () {{
        //        $('#{0} > div.html-content-view').dblclick(function (e) {{
        //            {1};
        //        }});
        //    }});
        //", upnlHtmlContent.ClientID, this.Page.ClientScript.GetPostBackEventReference( lbQuickEdit, "" ) );
        //                }

        //                if ( GetAttributeValue( "QuickEdit" ) == "AIREDIT" )
        //                {
        //                    RockPage.AddScriptLink( Page, "~/Scripts/summernote/summernote.min.js", true );

        //                    script = string.Format( @"
        //    Sys.Application.add_load( function () {{
        //        $('#{0} > div.html-content-view').summernote( {{
        //            airMode: true,
        //            callbacks: {{
        //                onChange: function( contents, $editable ) {{
        //                    var htmlContents = {{
        //                        EntityValue: $('#{2}').val(),
        //                        Content: contents
        //                    }};
        //                    $.post( Rock.settings.get('baseUrl') + 'api/HtmlContents/UpdateContents/{1}', htmlContents, null, 'application/json' );
        //                }}
        //            }}
        //        }});
        //    }});
        //", upnlHtmlContent.ClientID, this.BlockId, hfEntityValue.ClientID );
        //                }

        //                if ( !string.IsNullOrWhiteSpace( script ) )
        //                {
        //                    ScriptManager.RegisterStartupScript( lbQuickEdit, lbQuickEdit.GetType(), string.Format( "html-content-block-{0}", this.BlockId ), script, true );
        //                }
        //            }
        //        }

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

            gVersions.EntityTypeId = EntityTypeCache.Get<HtmlContent>().Id;
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
            upnlHtmlContentEdit.Update();
            mdEdit.Show();

            bool useCodeEditor = GetAttributeValue( AttributeKey.UseCodeEditor ).AsBoolean();

            htmlEditor.StartInCodeEditorMode = useCodeEditor;

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

                htmlEditor.CallbackOnKeyupScript = onchangeScript;
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
                    }
                }
            }

            string documentRoot = GetAttributeValue( AttributeKey.DocumentRootFolder );
            string imageRoot = GetAttributeValue( AttributeKey.ImageRootFolder );
            htmlEditor.UserSpecificRoot = GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBoolean();
            htmlEditor.DocumentFolderRoot = documentRoot;
            htmlEditor.ImageFolderRoot = imageRoot;

            bool supportsVersioning = GetAttributeValue( AttributeKey.SupportVersions ).AsBoolean();
            bool requireApproval = GetAttributeValue( AttributeKey.RequireApproval ).AsBoolean();

            lVersion.Visible = supportsVersioning;
            lbShowVersionGrid.Visible = supportsVersioning;
            cbOverwriteVersion.Visible = supportsVersioning;
            cbOverwriteVersion.Checked = false;

            // RequireApproval only applies if SupportsVersioning=True
            upnlApproval.Visible = supportsVersioning && requireApproval;
            lbApprove.Enabled = IsUserAuthorized( "Approve" );
            lbDeny.Enabled = IsUserAuthorized( "Approve" );

            string entityValue = EntityValue();
            HtmlContent htmlContent = new HtmlContentService( new RockContext() ).GetLatestVersion( this.BlockId, entityValue );

            // set Height of editors
            if ( supportsVersioning && requireApproval )
            {
                htmlEditor.Height = 280;
            }
            else if ( supportsVersioning )
            {
                htmlEditor.Height = 350;
            }
            else
            {
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
                .Select( c => ( int? ) c.Version ).Max();
            return maxVersion;
        }


        /// <summary>
        /// Shows the view.
        /// </summary>
        protected void ShowView()
        {
            mdEdit.Hide();
            pnlEditModel.Visible = false;
            upnlHtmlContentEdit.Update();
            upnlHtmlContentView.Update();

            // prevent htmlEditor from using viewstate when not needed
            pnlEdit.EnableViewState = false;

            pnlEdit.Visible = false;
            pnlVersionGrid.Visible = false;

            // If we are rendering in configuration-only mode, hide the block runtime content.
            if ( this.ConfigurationRenderModeIsEnabled )
            {
                upnlHtmlContentView.Visible = false;
                return;
            }

            string entityValue = EntityValue();
            string html = string.Empty;

            int cacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsInteger();
            string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
            string cachedContent = null;

            // only load from the cache if a cacheDuration was specified
            if ( cacheDuration > 0 )
            {
                cachedContent = HtmlContentService.GetCachedContent( this.BlockId, entityValue );
            }

            // if content not cached load it from DB
            if ( cachedContent == null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var htmlContentService = new HtmlContentService( rockContext );
                    var contentHtml = htmlContentService.GetActiveContentHtml( this.BlockId, entityValue );

                    if ( contentHtml != null )
                    {
                        if ( contentHtml.HasMergeFields() )
                        {
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                            mergeFields.Add( "CurrentPage", this.PageCache );

                            if ( CurrentPerson != null )
                            {
                                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                                mergeFields.AddOrIgnore( "Person", CurrentPerson );
                            }

                            mergeFields.Add( "CurrentBrowser", this.RockPage.BrowserClient );

                            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                            mergeFields.Add( "CurrentPersonCanEdit", IsUserAuthorized( Authorization.EDIT ) );
                            mergeFields.Add( "CurrentPersonCanAdministrate", IsUserAuthorized( Authorization.ADMINISTRATE ) );

                            html = contentHtml.ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
                        }
                        else
                        {
                            html = contentHtml;
                        }
                    }
                    else
                    {
                        html = string.Empty;
                    }
                }

                // Resolve any dynamic URL references
                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                html = html.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                // cache content
                if ( cacheDuration > 0 )
                {
                    HtmlContentService.AddCachedContent( this.BlockId, entityValue, html, cacheDuration, cacheTags );
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

            string contextParameter = GetAttributeValue( AttributeKey.ContextParameter );
            if ( !string.IsNullOrEmpty( contextParameter ) )
            {
                /*
                    6/15/2020 - JME
                    Updated the logic to get the Context Parameter. Before this would simply use the
                    query string. Updated it to also consider the context variable if one exists and
                    the query string did not contain the configured context paramater. This was added
                    to allow having different content for each context object. The specific use case
                    is when used in conjection with the campus context switcher. This change will allow
                    having separate content per campus (without any Lava case statements).  
                */
                var entityId = PageParameter( contextParameter );

                // If no page parameter then check for context value
                if ( entityId.IsNullOrWhiteSpace() && this.ContextEntity() != null )
                {
                    entityId = this.ContextEntity().Id.ToString();
                }
                entityValue = string.Format( "{0}={1}", contextParameter, entityId ?? string.Empty );
            }

            string contextName = GetAttributeValue( AttributeKey.ContextName );
            if ( !string.IsNullOrEmpty( contextName ) )
            {
                entityValue += "&ContextName=" + contextName;
            }

            return entityValue;
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            if ( this.GetAttributeValue( AttributeKey.IsSecondaryBlock ).AsBooleanOrNull() ?? false )
            {
                if ( lHtmlContent.Visible != visible )
                {
                    lHtmlContent.Visible = visible;

                    // upnlHtmlContent has UpdateMode=Conditional so tell it to update if Visible changed
                    upnlHtmlContentView.Update();
                }
            }
        }

        #endregion
    }
}