using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using Rock.Models.Cms;
using Rock.Helpers;

namespace Rock.Web.Blocks.Cms
{
    public partial class BlogPosts : Rock.Cms.CmsBlock
    {
        protected int skipCount = 0;
        protected int takeCount = 1;
        protected int currentPage = 1;
        
        protected void Page_Init( object sender, EventArgs e )
        {
            // get current page
            if ( int.TryParse( PageParameter( "Page" ), out currentPage ) == false )
            {
                currentPage = 1;
            }

            // get number of posts to display per page
            takeCount = Convert.ToInt32( AttributeValue( "PostsPerPage" ) );

            // calculate the number to skip
            skipCount = (takeCount * currentPage) - takeCount;

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

            int blogId = -1;
            try
            {
                blogId = Convert.ToInt32( PageParameter( "BlogId" ) );
            }
            catch ( Exception ex )
            {
                lPosts.Text = "<p class=\"block-warning\">The ID of this blog could not be found in the address of this page</p>";
            }

            if ( blogId != -1 )
            {
                Rock.Services.Cms.BlogService blogService = new Services.Cms.BlogService();
                Rock.Models.Cms.Blog blog = blogService.GetBlog( blogId );

                lPosts.Text = "";

                // load posts
                //var posts = blog.BlogPosts.Select( p => new { p.Title, p.PublishDate, p.State, AuthorFirstName = p.Author.FirstName, AuthorLastName = p.Author.LastName, p.Content } ).Where( p => p.State == (int)BlogPost.PostStatus.Published && p.PublishDate < DateTime.Now ).OrderByDescending( p => p.PublishDate ).Skip( skipCount ).Take( takeCount + 1 ).ToList();
                var posts = blog.BlogPosts.Where( p => p.State == (int)BlogPost.PostStatus.Published && p.PublishDate < DateTime.Now ).OrderByDescending( p => p.PublishDate ).Skip( skipCount ).Take( takeCount + 1 ).ToList();

                // we load one exta post so that we can determine if there is a need to display the older posts button
                int postsToShow = posts.Count();
                if ( postsToShow > takeCount )
                    postsToShow = takeCount;

                try
                {
                    for ( int i = 0; i < postsToShow; i++ )
                    {
                        var post = posts[i];
                        DateTime publishDate = (DateTime)post.PublishDate;

                        StringBuilder sb = new StringBuilder();
                        sb.Append( "<article class=\"blog-post\">\n" );
                        sb.Append( "    <header>\n" );
                        sb.Append( "        <h1>" + post.Title + "</h1>" );
                        sb.Append( "         Posted " );

                        // determine categories
                        if ( post.Categorys.Count > 0 )
                        {
                            sb.Append( "in" );

                            bool firstCategory = true;
                            foreach ( BlogCategory category in post.Categorys )
                            {
                                if ( firstCategory )
                                {
                                    sb.Append( category.Name );
                                    firstCategory = false;
                                }
                                else
                                    sb.Append( "," + category.Name );
                            }
                        }

                        sb.Append( "on " + publishDate.ToString("D") + " by " + post.Author.FirstName + " " + post.Author.LastName + "\n" );
                        sb.Append( "     </header>\n" );
                        sb.Append( post.Content + "\n" );
                        sb.Append( "</article>\n" );
                        lPosts.Text += sb.ToString();
                    }

                    // set next prev buttons
                    if ( currentPage == 1 )
                        aNewer.Visible = false;
                    else
                        aNewer.Visible = true;

                    if ( posts.Count() < takeCount )
                        aOlder.Visible = false;
                    else
                        aOlder.Visible = true;

                   // set next prev button urls
                    aOlder.HRef = HtmlHelper.AppendQueryParameter( HttpContext.Current.Request.Url.PathAndQuery, "Page", (currentPage + 1).ToString() );
                    aNewer.HRef = HtmlHelper.AppendQueryParameter( HttpContext.Current.Request.Url.PathAndQuery, "Page", (currentPage - 1).ToString() );
                }
                catch ( NullReferenceException nrx )
                {
                    lPosts.Text = "<p class=\"block-warning\">The blog ID " + blogId.ToString() + " does not exist.</p>";
                }
            }
        }
    }
}