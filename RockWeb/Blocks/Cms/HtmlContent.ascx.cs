using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Models.Cms;
using Rock.Services.Cms;

namespace RockWeb.Blocks.Cms
{
    public partial class HtmlContent : Rock.Cms.CmsBlock
    {
        public override List<Control> GetConfigurationControls( bool canConfig, bool canEdit)
        {
            List<Control> configControls = new List<Control>();

            // add edit icon to config controls if user has edit permission
            if ( canEdit )
            {
                System.Web.UI.HtmlControls.HtmlGenericControl aAttributes = new System.Web.UI.HtmlControls.HtmlGenericControl( "a" );
                aAttributes.Attributes.Add( "class", "edit icon-button" );
                aAttributes.Attributes.Add( "href", "#" );
                aAttributes.Attributes.Add( "title", "Edit HTML" );
                aAttributes.InnerText = "Edit";
                configControls.Add( aAttributes );
            }

            configControls.AddRange( base.GetConfigurationControls(canConfig, canEdit) );

            return configControls;
        }

        protected void Page_Init( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                // register scripts
                PageInstance.AddScriptLink( this.Page, "~/scripts/ckeditor/ckeditor.js" );
                PageInstance.AddScriptLink( this.Page, "~/scripts/ckeditor/adapters/jquery.js" );

                string blockContent = "";

                // get settings
                string entityKey = AttributeValue( "EntityAwareKey" );
                string entityValue = PageParameter( entityKey );
                int cacheDuration = Int32.Parse ( AttributeValue( "CacheDuration" ) );

                // get blocks cached content
                string cachedContent = null;

                cachedContent = GetCacheItem( entityValue ) as string;

                // if content not cached load it from DB
                if ( cachedContent == null )
                {
                    Rock.Models.Cms.HtmlContent content = new HtmlContentService().GetActiveContentByBlockKey( BlockInstance.Id, entityValue );

                    if ( content != null )
                    {
                        blockContent = content.Content;

                        // cache content
                        if ( cacheDuration > 0 )
                            AddCacheItem( entityValue, content.Content, cacheDuration );
                    }
                }
                else
                    blockContent = cachedContent;
               
                // add content to the editor
                txtHtmlContentEditor.Text = blockContent;

                // add content to the content window
                lPreText.Text = AttributeValue( "PreText" );
                lHtmlContent.Text = blockContent;
                lPostText.Text = AttributeValue( "PostText" );

            }
        }

        protected override void  OnInit(EventArgs e)
        {
 	        base.OnInit(e);

            this.AttributesUpdated += new Rock.Cms.AttributesUpdatedEventHandler( HtmlContent_AttributesUpdated );
            this.AddAttributeUpdateTrigger( pnlContent );
        }

        void HtmlContent_AttributesUpdated( object sender, EventArgs e )
        {
            lPreText.Text = AttributeValue( "PreText" );
            lPostText.Text = AttributeValue( "PostText" );
            pnlContent.Update();
        }

        protected void btnSaveContent_Click( object sender, EventArgs e )
        {
            if ( UserAuthorized( "Edit" ) )
            {
                // get settings
                string entityKey = AttributeValue( "EntityAwareKey" );
                string entityValue = PageParameter( entityKey );
                int cacheDuration = Int32.Parse( AttributeValue( "CacheDuration" ) );
                
                // get current  content
                HtmlContentService service = new HtmlContentService();
                Rock.Models.Cms.HtmlContent content = service.GetActiveContentByBlockKey( BlockInstance.Id, entityValue );

                // if a record doesn't exist then  create one
                if ( content == null )
                {
                    content = new Rock.Models.Cms.HtmlContent();
                    content.BlockId = BlockInstance.Id;
                    content.EntityValue = entityValue;
                    content.Approved = true;
                    content.ApprovedByPersonId = CurrentPersonId;
                    service.AddHtmlContent( content );
                }

                content.Content = txtHtmlContentEditor.Text;

                service.Save( content, CurrentPersonId );

                // refresh cache content if desired
                this.AddCacheItem( entityValue, txtHtmlContentEditor.Text, cacheDuration );

                // set page content to new content
                lHtmlContent.Text = txtHtmlContentEditor.Text;
            }
        }
    }
}

/*
CKEditor Notes:
 * 
 * Toolbar Options: http://docs.cksource.com/CKEditor_3.x/Developers_Guide/Toolbar
 * Config Settings: http://docs.cksource.com/ckeditor_api/index.html

*/