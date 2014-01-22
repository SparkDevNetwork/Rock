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

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "HTML Content" )]
    [Category( "CMS" )]
    [Description( "Adds an editable HTML fragment to the page." )]
    [AdditionalActions( new string[] { "Approve" } )]
    [BooleanField( "Use Code Editor", "Use the code editor instead of the WYSIWYG editor", false, "", 0 )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 0, "", 3 )]
    [TextField( "Context Parameter", "Query string parameter to use for 'personalizing' content based on unique values.", false, "", "", 4 )]
    [TextField( "Context Name", "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", false, "", "", 5 )]
    [BooleanField( "Require Approval", "Require that content be approved?", false, "", 6 )]
    [BooleanField( "Support Versions", "Support content versioning?", false, "", 7 )]
    public partial class HtmlContentDetail : RockBlock
    {
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

            if ( !this.IsPostBack )
            {
                ShowView();
            }
        }

        /// <summary>
        /// Adds icons to the configuration area of a block instance.  Can be overridden to
        /// add additionsl icons
        /// </summary>
        /// <param name="canConfig"></param>
        /// <param name="canEdit"></param>
        /// <returns></returns>
        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            // add edit icon to config controls if user has edit permission
            if ( canConfig || canEdit )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = "Edit HTML";
                lbEdit.Click += lbEdit_Click;
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add( "class", "fa fa-pencil-square-o" );

                // will toggle the block config so they are no longer showing
                lbEdit.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
            }

            configControls.AddRange( base.GetAdministrateControls( canConfig, canEdit ) );

            return configControls;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            // only enable viewstate for htmlEditor when needed (it is really big)
            pnlEdit.EnableViewState = true;

            pnlEdit.Visible = true;
            pnlVersionGrid.Visible = false;
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
            HtmlContent htmlContent = new HtmlContentService().GetActiveContent( this.BlockId, entityValue );

            ShowEditDetail( htmlContent );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the HtmlContentDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HtmlContentDetail_BlockUpdated( object sender, EventArgs e )
        {
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
            HtmlContentService htmlContentService = new HtmlContentService();

            // get settings
            string entityValue = EntityValue();

            // get current content
            int version = hfVersion.ValueAsInt();
            HtmlContent htmlContent = htmlContentService.GetByBlockIdAndEntityValueAndVersion( this.BlockId, entityValue, version );

            // if the existing content changed, and the overwrite option was not checked, create a new version
            string newContent = ceHtml.Visible ? ceHtml.Text : htmlEditor.Text;

            if ( htmlContent != null && supportVersioning && htmlContent.Content != newContent && !cbOverwriteVersion.Checked )
            {
                htmlContent = null;
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

                htmlContentService.Add( htmlContent, CurrentPersonId );
            }

            htmlContent.StartDateTime = drpDateRange.LowerValue;
            htmlContent.ExpireDateTime = drpDateRange.UpperValue;

            if ( !requireApproval || IsUserAuthorized( "Approve" ) )
            {
                htmlContent.IsApproved = !requireApproval || hfApprovalStatus.Value.AsBoolean();
                if ( htmlContent.IsApproved )
                {
                    int personId = hfApprovalStatusPersonId.ValueAsInt();
                    if ( personId > 0 )
                    {
                        htmlContent.ApprovedByPersonId = personId;
                        htmlContent.ApprovedDateTime = DateTime.Now;
                    }
                }
            }

            htmlContent.Content = newContent;

            if ( htmlContentService.Save( htmlContent, CurrentPersonId ) )
            {
                // flush cache content 
                this.FlushCacheItem( entityValue );

                // force the updatepanel for the view to update since we have two update panels and we only want the View to update when it is edited and Saved
                upnlHtmlContent.Update();
                ShowView();
            }
            else
            {
                // TODO: service.ErrorMessages;
            }
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
            HtmlContent htmlContent = new HtmlContentService().Get( e.RowKeyId );
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
            SetApprovalValues( true, CurrentPerson );
        }

        /// <summary>
        /// Handles the Click event of the lbDeny control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeny_Click( object sender, EventArgs e )
        {
            SetApprovalValues( false, CurrentPerson );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var htmlContentService = new HtmlContentService();
            var content = htmlContentService.GetContent( this.BlockId, EntityValue() );

            var versions = content.Select( v =>
                new
                {
                    v.Id,
                    v.Version,
                    VersionText = "Version " + v.Version.ToString(),
                    ModifiedDateTime = "(" + v.ModifiedDateTime.ToElapsedString() + ")",
                    ModifiedByPerson = v.ModifiedByPersonAlias != null ? v.ModifiedByPersonAlias.Person : null,
                    Approved = v.IsApproved,
                    ApprovedByPerson = v.ApprovedByPerson,
                    v.StartDateTime,
                    v.ExpireDateTime
                } ).ToList();

            gVersions.DataSource = versions;
            gVersions.GridRebind += gVersions_GridRebind;
            gVersions.DataBind();
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

            SetApprovalValues( htmlContent.IsApproved, htmlContent.ApprovedByPerson );

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
        private void SetApprovalValues( bool approved, Person person )
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
            lblApprovalStatusPerson.Visible = person != null;
            if ( person != null )
            {
                lblApprovalStatusPerson.Text = "by " + person.FullName;
                hfApprovalStatusPersonId.Value = person.Id.ToString();
            }
        }

        /// <summary>
        /// Gets the maximum version that this HtmlContent block 
        /// </summary>
        /// <returns></returns>
        private int? GetMaxVersionOfHtmlContent()
        {
            string entityValue = this.EntityValue();
            int? maxVersion = new HtmlContentService().Queryable()
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

            // prevent htmlEditor from using viewstate when not needed
            pnlEdit.EnableViewState = false;

            pnlEdit.Visible = false;
            pnlVersionGrid.Visible = false;
            string entityValue = EntityValue();
            string html = string.Empty;

            string cachedContent = GetCacheItem( entityValue ) as string;

            // if content not cached load it from DB
            if ( cachedContent == null )
            {
                HtmlContent content = new HtmlContentService().GetActiveContent( this.BlockId, entityValue );

                if ( content != null )
                {
                    html = content.Content.ResolveMergeFields( Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson ) );
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
                int cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger() ?? 0;
                if ( cacheDuration > 0 )
                {
                    AddCacheItem( entityValue, html, cacheDuration );
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