//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Cms;

namespace RockWeb.Blocks.Cms
{
    [Rock.Security.AdditionalActions( new string[] { "Approve" } )]
    [Rock.Attribute.Property( 0, "Pre-Text", "PreText", "", "HTML text to render before the blocks main content.", false, "" )]
    [Rock.Attribute.Property( 1, "Post-Text", "PostText", "", "HTML text to render after the blocks main content.", false, "" )]
    [Rock.Attribute.Property( 2, "Cache Duration", "CacheDuration", "", "Number of seconds to cache the content.", false, "0", "Rock", "Rock.Field.Types.Integer" )]
    [Rock.Attribute.Property( 3, "Context Parameter", "ContextParameter", "", "Query string parameter to use for 'personalizing' content based on unique values.", false, "" )]
    [Rock.Attribute.Property( 4, "Context Name", "ContextName", "", "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", false, "" )]
    [Rock.Attribute.Property( 5, "Support Versions", "Advanced", "Support content versioning?", false, "False", "Rock", "Rock.Field.Types.Boolean" )]
    [Rock.Attribute.Property( 6, "Require Approval", "Advanced", "Require that content be approved?", false, "False", "Rock", "Rock.Field.Types.Boolean" )]

    public partial class HtmlContent : Rock.Web.UI.Block
    {
        #region Private Global Variables

        bool _supportVersioning = false;
        bool _requireApproval = false;

        #endregion

        #region Events

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            CurrentPage.AddScriptLink( this.Page, "~/scripts/ckeditor/ckeditor.js" );
            CurrentPage.AddScriptLink( this.Page, "~/scripts/ckeditor/adapters/jquery.js" );
            CurrentPage.AddScriptLink( this.Page, "~/Scripts/Rock/htmlContentOptions.js" );
            CurrentPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.core.min.js" );
            CurrentPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.fx.min.js" );
            CurrentPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.popup.min.js" );
            CurrentPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.calendar.min.js" );
            CurrentPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.datepicker.min.js" );

            CurrentPage.AddCSSLink( this.Page, "~/CSS/Kendo/kendo.common.min.css" );
            CurrentPage.AddCSSLink( this.Page, "~/CSS/Kendo/kendo.rock.min.css" );

            _supportVersioning = bool.Parse( AttributeValue( "SupportVersions" ) ?? "false" );
            _requireApproval = bool.Parse( AttributeValue( "RequireApproval" ) ?? "false" );

            mpeContent.OnOkScript = string.Format("saveHtmlContent_{0}();", CurrentBlock.Id);

            rGrid.DataKeyNames = new string[] { "id" };
            rGrid.ShowActionRow = false;

            this.AttributesUpdated += HtmlContent_AttributesUpdated;

            ShowView();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            hfAction.Value = string.Empty;
        }

        protected void lbEdit_Click( object sender, EventArgs e )
        {
            HtmlContentService service = new HtmlContentService();
            Rock.Cms.HtmlContent content = service.GetActiveContent( CurrentBlock.Id, EntityValue() );
            if ( content == null )
                content = new Rock.Cms.HtmlContent();

            if ( _supportVersioning )
            {
                phCurrentVersion.Visible = true;
                pnlVersioningHeader.Visible = true;
                cbOverwriteVersion.Visible = true;

                hfVersion.Value = content.Version.ToString();
                lVersion.Text = content.Version.ToString();
                tbStartDate.Text = content.StartDateTime.HasValue ? content.StartDateTime.Value.ToShortDateString() : string.Empty;
                tbExpireDate.Text = content.ExpireDateTime.HasValue ? content.ExpireDateTime.Value.ToShortDateString() : string.Empty;

                if ( _requireApproval )
                {
                    cbApprove.Checked = content.IsApproved;
                    cbApprove.Enabled = IsUserAuthorized( "Approve" );
                    cbApprove.Visible = true;
                }
                else
                    cbApprove.Visible = false;
            }
            else
            {
                phCurrentVersion.Visible = false;
                pnlVersioningHeader.Visible = false;
                cbOverwriteVersion.Visible = false;
            }

            txtHtmlContentEditor.Text = content.Content;

            BindGrid();

            hfAction.Value = "Edit";
        }

        protected override void OnPreRender( EventArgs e )
        {
            aClose.Attributes["onclick"] = string.Format(
                "$find('{0}').hide();return false;", mpeContent.BehaviorID );

            base.OnPreRender( e );
        }

        void HtmlContent_AttributesUpdated( object sender, EventArgs e )
        {
            lPreText.Text = AttributeValue( "PreText" );
            lPostText.Text = AttributeValue( "PostText" );
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( IsUserAuthorized( "Edit" ) || IsUserAuthorized( "Configure" ) )
            {
                Rock.Cms.HtmlContent content = null;
                HtmlContentService service = new HtmlContentService();

                // get settings
                string entityValue = EntityValue();

                // get current  content
                int version = 0;
                if ( !Int32.TryParse( hfVersion.Value, out version ) )
                    version = 0;
                content = service.GetByBlockIdAndEntityValueAndVersion( CurrentBlock.Id, entityValue, version );

                // if the existing content changed, and the overwrite option was not checked, create a new version
                if ( content != null &&
                    _supportVersioning &&
                    content.Content != txtHtmlContentEditor.Text &&
                    !cbOverwriteVersion.Checked )
                    content = null;

                // if a record doesn't exist then  create one
                if ( content == null )
                {
                    content = new Rock.Cms.HtmlContent();
                    content.BlockId = CurrentBlock.Id;
                    content.EntityValue = entityValue;

                    if ( _supportVersioning )
                    {
                        int? maxVersion = service.Queryable().
                            Where( c => c.BlockId == CurrentBlock.Id &&
                                c.EntityValue == entityValue ).
                            Select( c => ( int? )c.Version ).Max();

                        content.Version = maxVersion.HasValue ? maxVersion.Value + 1 : 1;
                    }
                    else
                        content.Version = 0;

                    service.Add( content, CurrentPersonId );
                }

                if ( _supportVersioning )
                {
                    DateTime startDate;
                    if ( DateTime.TryParse( tbStartDate.Text, out startDate ) )
                        content.StartDateTime = startDate;
                    else
                        content.StartDateTime = null;

                    DateTime expireDate;
                    if ( DateTime.TryParse( tbExpireDate.Text, out expireDate ) )
                        content.ExpireDateTime = expireDate;
                    else
                        content.ExpireDateTime = null;
                }
                else
                {
                    content.StartDateTime = null;
                    content.ExpireDateTime = null;
                }

                if ( !_requireApproval || IsUserAuthorized( "Approve" ) )
                {
                    content.IsApproved = !_requireApproval || cbApprove.Checked;
                    if ( content.IsApproved )
                    {
                        content.ApprovedByPersonId = CurrentPersonId;
                        content.ApprovedDateTime = DateTime.Now;
                    }
                }

                content.Content = txtHtmlContentEditor.Text;

                service.Save( content, CurrentPersonId );

                // flush cache content 
                this.FlushCacheItem( entityValue );

            }

            ShowView();
        }

        #endregion

        #region Methods

        public override List<Control> GetConfigurationControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            // add edit icon to config controls if user has edit permission
            if ( canConfig || canEdit )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = "Edit HTML";
                lbEdit.Click += new EventHandler( lbEdit_Click );
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                iEdit.Attributes.Add( "class", "icon-edit" );

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
            }

            configControls.AddRange( base.GetConfigurationControls( canConfig, canEdit ) );

            return configControls;
        }

        private void ShowView()
        {
            string entityValue = EntityValue();
            string html = "";

            string cachedContent = GetCacheItem( entityValue ) as string;

            // if content not cached load it from DB
            if ( cachedContent == null )
            {
                Rock.Cms.HtmlContent content = new HtmlContentService().GetActiveContent( CurrentBlock.Id, entityValue );

                if ( content != null )
                    html = content.Content;
                else
                    html = string.Empty;

                // cache content
                int cacheDuration = 0;
                if ( Int32.TryParse( AttributeValue( "CacheDuration" ), out cacheDuration ) && cacheDuration > 0 )
                    AddCacheItem( entityValue, html, cacheDuration );
            }
            else
                html = cachedContent;

            // add content to the content window
            lPreText.Text = AttributeValue( "PreText" );
            lHtmlContent.Text = html;
            lPostText.Text = AttributeValue( "PostText" );
        }

        private void BindGrid()
        {
            var HtmlService = new HtmlContentService();
            var content = HtmlService.GetContent( CurrentBlock.Id, EntityValue() );

			var personService = new Rock.Crm.PersonService();
			var modifiedPersons = new Dictionary<int, string>();
			foreach ( var personId in content.Where( c => c.ModifiedByPersonId.HasValue ).Select( c => c.ModifiedByPersonId ).Distinct() )
			{
				var modifiedPerson = personService.Get( personId.Value, true );
				modifiedPersons.Add( personId.Value, modifiedPerson != null ? modifiedPerson.FullName : string.Empty );
			}

			var versions = content.
                Select( v => new
                {
                    v.Id,
                    v.Version,
                    v.Content,
                    ModifiedDateTime = v.ModifiedDateTime.ToElapsedString(),
					ModifiedByPerson = v.ModifiedByPersonId.HasValue ? modifiedPersons[v.ModifiedByPersonId.Value] : string.Empty,
                    Approved = v.IsApproved,
                    ApprovedByPerson = v.ApprovedByPerson != null ? v.ApprovedByPerson.FullName : "",
                    v.StartDateTime,
                    v.ExpireDateTime
                } ).ToList();

            rGrid.DataSource = versions;
            rGrid.DataBind();
        }

        private string EntityValue()
        {
            string entityValue = string.Empty;

            string contextParameter = AttributeValue( "ContextParameter" );
            if ( !string.IsNullOrEmpty( contextParameter ) )
                entityValue = string.Format( "{0}={1}", contextParameter, PageParameter( contextParameter ) ?? string.Empty );

            string contextParameterValue = PageParameter( contextParameter );
            if ( !string.IsNullOrEmpty( contextParameterValue ) )
                entityValue += "&ContextName=" + contextParameterValue;

            return entityValue;
        }

        #endregion
    }
}

