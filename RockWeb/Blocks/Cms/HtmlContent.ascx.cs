//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;

using Rock.CMS;

namespace RockWeb.Blocks.Cms
{
    [Rock.Security.AdditionalActions( new string[] { "Approve" } )]
    [Rock.Attribute.Property( 0, "Pre-Text", "PreText", "", "HTML text to render before the blocks main content.", false, "" )]
    [Rock.Attribute.Property( 1, "Post-Text", "PostText", "", "HTML text to render after the blocks main content.", false, "" )]
    [Rock.Attribute.Property( 2, "Cache Duration", "CacheDuration", "", "Number of seconds to cache the content.", false, "0", "Rock", "Rock.FieldTypes.Integer")]
    [Rock.Attribute.Property( 3, "Entity Aware Key", "EntityAwareKey", "", "Query string key to use to 'personalize' the content to a particular entity.", false, "" )]
    public partial class HtmlContent : Rock.Web.UI.Block
    {
        public override List<Control> GetConfigurationControls( bool canConfig, bool canEdit)
        {
            List<Control> configControls = new List<Control>();

            // add edit icon to config controls if user has edit permission
            if ( canConfig || canEdit )
            {
                System.Web.UI.HtmlControls.HtmlGenericControl aAttributes = new System.Web.UI.HtmlControls.HtmlGenericControl( "a" );
                aAttributes.Attributes.Add( "class", "edit icon-button" );
                aAttributes.Attributes.Add( "href", "#" );
                aAttributes.Attributes.Add( "title", "Edit HTML" );
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
                    Rock.CMS.HtmlContent content = new HtmlContentService().GetActiveContentByBlockKey( BlockInstance.Id, entityValue );

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

            this.AttributesUpdated += HtmlContent_AttributesUpdated;
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
            if ( UserAuthorized( "Edit" ) || UserAuthorized( "Configure" ) )
            {
                // get settings
                string entityKey = AttributeValue( "EntityAwareKey" );
                string entityValue = PageParameter( entityKey );
                int cacheDuration = Int32.Parse( AttributeValue( "CacheDuration" ) );

                // get current  content
                HtmlContentService service = new HtmlContentService();
                Rock.CMS.HtmlContent content = service.GetActiveContentByBlockKey( BlockInstance.Id, entityValue );

                // if a record doesn't exist then  create one
                if ( content == null )
                {
                    content = new Rock.CMS.HtmlContent();
                    content.BlockId = BlockInstance.Id;
                    content.EntityValue = entityValue;
                    content.Approved = true;
                    content.ApprovedByPersonId = CurrentPersonId;
                    service.Add( content, CurrentPersonId );
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