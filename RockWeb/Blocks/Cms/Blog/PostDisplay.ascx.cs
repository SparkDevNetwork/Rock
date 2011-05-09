using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using System.Configuration;

using Rock.Models.Cms;
using Rock.Helpers;

namespace RockWeb.Blocks.Cms.Blog
{
    public partial class PostDisplay : Rock.Cms.CmsBlock
    {
        protected int postId = -1;
        protected BlogPost post = null;

        protected void Page_Init( object sender, EventArgs e )
        {
            // get block settings

            // get post id
            try
            {
                postId = Convert.ToInt32( PageParameter( "PostId" ) );
            }
            catch ( Exception ex )
            {
            }

            Rock.Services.Cms.BlogPostService postService = new Rock.Services.Cms.BlogPostService();
            post = postService.GetBlogPost( postId );

            lTitle.Text = post.Title;
            lContents.Text = post.Content;

            lPostDetails.Text = "Put details here";

            // check if comments are allowed
            if ( post.Blog.AllowComments )
            {
                LoadComments();
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

        private void LoadComments()
        {
            // display comments
            StringBuilder sbComments = new StringBuilder();

            var comments = post.BlogPostComments;

            if ( comments.Count > 0 )
                sbComments.Append( "<h1>Comments</h1>\n" );

            foreach ( BlogPostComment comment in comments )
            {
                sbComments.Append( "<article class=\"group\">\n" );
                sbComments.Append( "<header><img src=\"http://www.gravatar.com/avatar/" + HtmlHelper.CalculateMD5Hash( comment.EmailAddress ) + "?r=pg&d=identicon&s=50\" /><strong>" + comment.PersonName + " says:</strong> </h1>" + comment.CommentDate.Value.ToLongDateString() + " " + comment.CommentDate.Value.ToShortTimeString() + "</header>" );
                sbComments.Append( comment.Comment );

                sbComments.Append( "</article>\n" );
            }

            lPostComments.Text = sbComments.ToString();
        }

        protected void valRecaptcha_Validate( object sender, ServerValidateEventArgs e )
        {
            // ensure recaptcha is correct
            rcComment.Validate();
            if ( !rcComment.IsValid )
            {
                e.IsValid = false;

            }
        }

        protected void btnSubmitComment_Click( object sender, EventArgs e )
        {            
            if ( Page.IsValid )
            {
                // save comment
                Rock.Services.Cms.BlogPostCommentService commentService = new Rock.Services.Cms.BlogPostCommentService();
                BlogPostComment comment = new BlogPostComment();
                comment.Comment = txtComment.Text;
                comment.CommentDate = DateTime.Now;
                comment.EmailAddress = txtEmail.Text;
                comment.PostId = postId;
                comment.PersonName = txtName.Text;
                comment.PersonId = CurrentPersonId;

                commentService.AddBlogPostComment( comment );
                commentService.Save( comment, CurrentPersonId );

                // load comments
                post.BlogPostComments.Add( comment );
                LoadComments();

                // clear out comment fields
                txtName.Text = "";
                txtEmail.Text = "";
                txtComment.Text = "";
            }
        }

    }
}