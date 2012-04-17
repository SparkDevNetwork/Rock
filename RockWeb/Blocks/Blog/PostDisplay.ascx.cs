//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.CMS;

namespace RockWeb.Blocks.Blog
{
    public partial class PostDisplay : Rock.Web.UI.Block
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
            catch 
            {
            }

            Rock.CMS.BlogPostRepository postRepository = new Rock.CMS.BlogPostRepository();
            post = postRepository.Get( postId );

            lTitle.Text = post.Title;
            lContents.Text = post.Content;

            // prepare post details
            StringBuilder pDetails = new StringBuilder();

            DateTime publishDate = (DateTime)post.PublishDate;
            pDetails.Append( "This entry was posted on " + publishDate.ToString( "D" ) + " at " + String.Format( "{0:t}", publishDate ) + " by " + post.Author.FirstName + " " + post.Author.LastName + "." );

            lPostDetails.Text = pDetails.ToString();

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
                sbComments.Append( "<header><img src=\"http://www.gravatar.com/avatar/" + Rock.Web.UI.Controls.HtmlHelper.CalculateMD5Hash( comment.EmailAddress ) + "?r=pg&d=identicon&s=50\" /><strong>" + comment.PersonName + " says:</strong> </h1>" + comment.CommentDate.Value.ToLongDateString() + " " + comment.CommentDate.Value.ToShortTimeString() + "</header>" );
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
                Rock.CMS.BlogPostCommentRepository commentRepository = new Rock.CMS.BlogPostCommentRepository();
                BlogPostComment comment = new BlogPostComment();
                comment.Comment = txtComment.Text;
                comment.CommentDate = DateTime.Now;
                comment.EmailAddress = txtEmail.Text;
                comment.PostId = postId;
                comment.PersonName = txtName.Text;
                comment.PersonId = CurrentPersonId;

                commentRepository.Add( comment, CurrentPersonId );
                commentRepository.Save( comment, CurrentPersonId );

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