using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.Blocks.Cms
{
    public partial class BlogPosts : Rock.Cms.CmsBlock
    {
        protected void Page_Init( object sender, EventArgs e )
        {
            DisplayList();
            this.AttributesUpdated += new Rock.Cms.AttributesUpdatedEventHandler( BlogPosts_AttributesUpdated );
            //this.AddAttributeUpdateTrigger( upPosts );
        }

        void BlogPosts_AttributesUpdated( object sender, EventArgs e )
        {
            DisplayList();
        }

        protected void Page_Load( object sender, EventArgs e )
        {
        }

        private void DisplayList()
        {
            int blogId = Convert.ToInt32( PageParameter( "BlogId" ) );

            Rock.Services.Cms.BlogService blogService = new Services.Cms.BlogService();
            Rock.Models.Cms.Blog blog = blogService.GetBlog( blogId );

            int maxPosts = Convert.ToInt32( AttributeValue( "Max Posts" ) );

            blPosts.Items.Clear();
            foreach ( Rock.Models.Cms.BlogPost post in blog.BlogPosts.OrderByDescending(p => p.PublishDate).Take(maxPosts) )
                blPosts.Items.Add( new ListItem( post.Content ) );
        }
    }
}