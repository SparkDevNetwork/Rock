//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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

namespace RockWeb.Blocks.Cms
{
    [AdditionalActions( new string[] { "Approve" } )]
    [CodeEditorField( "Pre-Text", "HTML text to render before the blocks main content.", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 0, "PreText" )]
    [CodeEditorField( "Post-Text", "HTML text to render after the blocks main content.", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 1, "PostText" )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 0, "", 3 )]
    [TextField( "Context Parameter", "Query string parameter to use for 'personalizing' content based on unique values.", false, "", "", 4 )]
    [TextField( "Context Name", "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", false, "", "", 5 )]
    [BooleanField( "Require Approval", "Require that content be approved?", false, "", 6 )]
    [BooleanField( "Support Versions", "Support content versioning?", false, "", 7 )]
    public partial class HtmlContentDetail : RockBlock
    {
        #region Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += HtmlContentDetail_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPanel );

            if ( !this.IsPostBack )
            {
                ShowView();
            }
        }

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

            edtHtml.Toolbar = "RockCustomConfigFull";
            edtHtml.MergeFields.Clear();
            edtHtml.MergeFields.Add( "GlobalAttribute" );

            bool supportsVersioning = GetAttributeValue( "SupportVersions" ).AsBoolean();
            bool requireApproval = GetAttributeValue( "RequireApproval" ).AsBoolean();

            lVersion.Visible = supportsVersioning;
            btnShowVersionGrid.Visible = supportsVersioning;
            cbOverwriteVersion.Visible = supportsVersioning;
            cbOverwriteVersion.Checked = false;

            // RequireApproval only applies if SupportsVersioning=True
            upApproval.Visible = supportsVersioning && requireApproval;
            lbApprove.Enabled = IsUserAuthorized( "Approve" );
            lbDeny.Enabled = IsUserAuthorized( "Approve" );

            string entityValue = EntityValue();
            HtmlContent htmlContent = new HtmlContentService().GetActiveContent( this.BlockId, entityValue );

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

            SetApprovalValues( htmlContent.IsApproved, htmlContent.ApprovedByPerson );

            pDateRange.LowerValue = htmlContent.StartDateTime;
            pDateRange.UpperValue = htmlContent.ExpireDateTime;
            edtHtml.Text = htmlContent.Content;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the HtmlContentDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HtmlContentDetail_BlockUpdated( object sender, EventArgs e )
        {
            lPreText.Text = GetAttributeValue( "PreText" );
            lPostText.Text = GetAttributeValue( "PostText" );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
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
            if ( htmlContent != null && supportVersioning && htmlContent.Content != edtHtml.Text && !cbOverwriteVersion.Checked )
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

            htmlContent.StartDateTime = pDateRange.LowerValue;
            htmlContent.ExpireDateTime = pDateRange.UpperValue;

            if ( !requireApproval || IsUserAuthorized( "Approve" ) )
            {
                htmlContent.IsApproved = !requireApproval || hfApprovalStatus.Value.AsBoolean();
                if ( htmlContent.IsApproved )
                {
                    htmlContent.ApprovedByPersonId = hfApprovalStatusPersonId.ValueAsInt();
                    htmlContent.ApprovedDateTime = DateTime.Now;
                }
            }

            htmlContent.LastModifiedPersonId = this.CurrentPersonId;
            htmlContent.LastModifiedDateTime = DateTime.Now;
            htmlContent.Content = edtHtml.Text;

            if ( htmlContentService.Save( htmlContent, CurrentPersonId ) )
            {
                // flush cache content 
                this.FlushCacheItem( entityValue );
                ShowView();
            }
            else
            {
                // TODO: service.ErrorMessages;
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

        #region View methods

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
                    html = content.Content.ResolveMergeFields( GetGlobalMergeFields() );
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
            lPreText.Text = GetAttributeValue( "PreText" );
            lHtmlContent.Text = html;
            lPostText.Text = GetAttributeValue( "PostText" );
        }

        #endregion

        #region Version History

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
                    ModifiedDateTime = v.LastModifiedDateTime.ToElapsedString(),
                    ModifiedByPerson = v.LastModifiedPerson,
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
        /// Handles the GridRebind event of the gVersions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gVersions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
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
        /// Gets the global merge fields.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetGlobalMergeFields()
        {
            var configValues = new Dictionary<string, object>();

            var globalAttributeValues = new Dictionary<string, object>();
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            foreach ( var attributeCache in globalAttributes.Attributes.OrderBy( a => a.Key ) )
            {
                if ( attributeCache.IsAuthorized( "View", null ) )
                {
                    string value = attributeCache.FieldType.Field.FormatValue( this, globalAttributes.AttributeValues[attributeCache.Key].Value, attributeCache.QualifierValues, false );
                    globalAttributeValues.Add( attributeCache.Key, value );
                }
            }

            configValues.Add( "GlobalAttribute", globalAttributeValues );

            return configValues;
        }

        #endregion

        #region Edit

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ShowView();
        }

        #endregion

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
        /// Handles the Click event of the btnShowVersionGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowVersionGrid_Click( object sender, EventArgs e )
        {
            BindGrid();
            pnlVersionGrid.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnReturnToEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReturnToEdit_Click( object sender, EventArgs e )
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
                cssClass = "label HtmlContentApprovalStatus label-success";
            }
            else
            {
                cssClass = "label HtmlContentApprovalStatus label-danger";
            }

            lApprovalStatus.Text = String.Format( "<span class='{0}'>{1}</span>", cssClass, approved ? "Approved" : "Not-Approved" );

            hfApprovalStatus.Value = approved.ToTrueFalse();
            lblApprovalStatusPerson.Visible = person != null;
            if ( person != null )
            {
                lblApprovalStatusPerson.Text = "by " + person.FullName;
                hfApprovalStatusPersonId.Value = person.Id.ToString();
            }
        }

    }
}