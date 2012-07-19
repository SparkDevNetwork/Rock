
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
                        if ( post.BlogCategories.Count > 0 )
                        {
                            sb.Append( "in <ul>" );

                            foreach ( BlogCategory category in post.BlogCategories )
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
                catch ( NullReferenceException )
                {
                    lPosts.Text = "<p class=\"block-warning\">The blog ID " + blogId.ToString() + " does not exist.</p>";
                }
            }
        }
    }
}