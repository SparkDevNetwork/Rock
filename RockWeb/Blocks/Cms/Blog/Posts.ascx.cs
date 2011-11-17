using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using Rock.Models.Cms;
using Rock.Helpers;

namespace RockWeb.Blocks.Cms.Blog
{
    public partial class Posts : Rock.Cms.CmsBlock
    {
        protected int skipCount = 0;
        protected int takeCount = 1;
        protected int currentPage = 1;
        protected int categoryId = 0;
        protected int tagId = 0;
        protected PageReference postDetailsPage = null;
        
        protected void Page_Init( object sender, EventArgs e )
        {
            // get current page
            if ( int.TryParse( PageParameter( "Page" ), out currentPage ) == false )
            {
                currentPage = 1;
            }

            // get post details page
            postDetailsPage = new PageReference( AttributeValue( "PostDetailPage" ) );

            // get number of posts to display per page
            takeCount = Convert.ToInt32( AttributeValue( "PostsPerPage" ) );

            // get category and tag id
            int.TryParse( PageParameter( "Category" ), out categoryId );
            int.TryParse( PageParameter( "Tag" ), out tagId );

            // calculate the number to skip
            skipCount = (takeCount * currentPage) - takeCount;

            DisplayList();
            this.AttributesUpdated += BlogPosts_AttributesUpdated;
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
                Rock.Services.Cms.BlogService blogService = new Rock.Services.Cms.BlogService();
                
                // try loading the blog object from the page cache
                Rock.Models.Cms.Blog blog = PageInstance.GetSharedItem( "blog" ) as Rock.Models.Cms.Blog;

                if ( blog == null )
                {
                    blog = blogService.Get( blogId );
                    PageInstance.SaveSharedItem( "blog", blog );
                }

                // add rss link in header if publish location is avail
                if ( blog.PublicFeedAddress != null && blog.PublicFeedAddress != string.Empty )
                {
                    System.Web.UI.HtmlControls.HtmlLink rssLink = new System.Web.UI.HtmlControls.HtmlLink();

                    rssLink.Attributes.Add( "type", "application/rss+xml" );
                    rssLink.Attributes.Add( "rel", "alternate" );
                    rssLink.Attributes.Add( "href", blog.PublicFeedAddress );
                    rssLink.Attributes.Add( "title", "RSS" );

                    PageInstance.AddHtmlLink( this.Page, rssLink );
                }

                lPosts.Text = "";

                // load posts
                //var posts = blog.BlogPosts.Select( p => new { p.Title, p.PublishDate, p.State, AuthorFirstName = p.Author.FirstName, AuthorLastName = p.Author.LastName, p.Content } ).Where( p => p.State == (int)BlogPost.PostStatus.Published && p.PublishDate < DateTime.Now ).OrderByDescending( p => p.PublishDate ).Skip( skipCount ).Take( takeCount + 1 ).ToList();
                IQueryable<BlogPost> qPosts = blog.BlogPosts.Where( p => p.State == (int)BlogPost.PostStatus.Published && p.PublishDate < DateTime.Now ).OrderByDescending( p => p.PublishDate ).Skip( skipCount ).Take( takeCount + 1 ).AsQueryable();

                // add category and tag filters if requested
                if ( categoryId != 0 )
                    qPosts = qPosts.Where( p => p.BlogCategorys.Any(c => c.Id == categoryId ));

                if ( tagId != 0 )
                    qPosts = qPosts.Where( p => p.BlogTags.Any( t => t.Id == tagId ) );
                
                // run query
                var posts = qPosts.ToList();

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
                        
                        if (postDetailsPage.IsValid)
                            sb.Append( "        <a href=\"" + PageInstance.BuildUrl( postDetailsPage, new Dictionary<string, string>() {{"PostId", post.Id.ToString()}} ) + "\"><h1>" + post.Title + "</h1></a>" );
                        else
                            sb.Append( "         <h1>" + post.Title + "</h1>" );

                        sb.Append( "         Posted " );

                        // determine categories
                        if ( post.BlogCategorys.Count > 0 )
                        {
                            sb.Append( "in <ul>" );

                            foreach ( BlogCategory category in post.BlogCategorys )
                            {
                                sb.Append( "<li><a href=\"" + PageInstance.BuildUrl( PageInstance.PageReference, new Dictionary<string, string>() { { "Category", category.Id.ToString() }, { "BlogId", blogId.ToString() } } ) + "\">" + category.Name + "</a></li?" );
                            }
                            sb.Append( "</ul> " );
                        }

                        sb.Append( "on " + publishDate.ToString("D") + " by " + post.Author.FirstName + " " + post.Author.LastName + "\n" );

                        // show tags if any
                        if ( post.BlogTags.Count > 0 )
                        {
                            sb.Append( "<div class=\"tags\">Tags:\n\t<ul>" );

                            foreach ( BlogTag tag in post.BlogTags )
                            {
                                sb.Append( "\t\t<li><a href=\"" + PageInstance.BuildUrl( PageInstance.PageReference, new Dictionary<string, string>() { { "Tag", tag.Id.ToString() }, { "BlogId", blogId.ToString() } } ) + "</a></li>\n" );
                            }

                            sb.Append( "\t</ul>\n</div>" );
                        }

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

                    if ( posts.Count() <= takeCount )
                        aOlder.Visible = false;
                    else
                        aOlder.Visible = true;

                   // set next prev button urls
                   aOlder.HRef = PageInstance.BuildUrl( PageInstance.PageReference, new Dictionary<string, string>() { { "Page", ( currentPage + 1 ).ToString() }, {"BlogId", blogId.ToString()} }, HttpContext.Current.Request.QueryString );
                   aNewer.HRef = PageInstance.BuildUrl( PageInstance.PageReference, new Dictionary<string, string>() { { "Page", ( currentPage - 1 ).ToString() }, { "BlogId", blogId.ToString() } }, HttpContext.Current.Request.QueryString );
                }
                catch ( NullReferenceException nrx )
                {
                    lPosts.Text = "<p class=\"block-warning\">The blog ID " + blogId.ToString() + " does not exist.</p>";
                }
            }
        }
    }
}