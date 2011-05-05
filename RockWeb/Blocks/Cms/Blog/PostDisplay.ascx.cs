using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using Rock.Models.Cms;
using Rock.Helpers;

namespace Rock.Web.Blocks.Cms.Blog
{
    public partial class PostDisplay : Rock.Cms.CmsBlock
    {
        protected void Page_Init( object sender, EventArgs e )
        {
            // get block settings
            
            // get post id
            int postId = -1;
            try
            {
                postId = Convert.ToInt32( PageParameter( "PostId" ) );
            }
            catch ( Exception ex )
            {
            }
            
            Rock.Services.Cms.BlogPostService postService = new Services.Cms.BlogPostService();
            BlogPost post = postService.GetBlogPost( postId );

            lTitle.Text = post.Title;
            lContents.Text = post.Content;

            lPostDetails.Text = "Put details here";

            // check if comments are allowed
            if ( post.Blog.AllowComments )
            {
                lPostComments.Text = "Put comments here";
            }
            else
                pnlAddComment.Visible = false;

            // add rss link in header if publish location is avail
            if ( post.Blog.PublicFeedAddress != null && post.Blog.PublicFeedAddress != string.Empty )
            {
                System.Web.UI.HtmlControls.HtmlLink rssLink = new System.Web.UI.HtmlControls.HtmlLink();

                rssLink.Attributes.Add( "type", "application/rss+xml" );
                rssLink.Attributes.Add( "rel", "alternate" );
                rssLink.Attributes.Add( "href", post.Blog.PublicFeedAddress );
                rssLink.Attributes.Add( "title", "RSS" );

                PageInstance.AddHtmlLink( this.Page, rssLink );
            }
        }

    }
}