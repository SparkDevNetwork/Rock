using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using Rock.CMS;
using Rock.Helpers;

namespace RockWeb.Blocks.Blog
{
    public partial class Categories : Rock.Web.UI.Block
    {
        protected void Page_Init( object sender, EventArgs e )
        {
            // get block settings
            string heading = AttributeValue( "Heading" );

            // create string to hold output
            StringBuilder output = new StringBuilder();
            
            // get blog id to load
            int blogId = -1;
            try
            {
                blogId = Convert.ToInt32( PageParameter( "BlogId" ) );
            }
            catch
            {
                lCategories.Text = "<p class=\"block-warning\">The ID of this blog could not be found in the address of this page</p>";
            }

            if ( blogId != -1 )
            {
                Rock.CMS.BlogService blogService = new Rock.CMS.BlogService();

                // try loading the blog object from the page cache
                Rock.CMS.Blog blog = PageInstance.GetSharedItem( "blog" ) as Rock.CMS.Blog;

                if ( blog == null )
                {
                    blog = blogService.Get( blogId );
                    PageInstance.SaveSharedItem( "blog", blog );
                }

                if ( heading != string.Empty )
                    output.Append( "<h1>" + heading + "</h1>\n\n" );

                // print categories as an un-ordered list
                output.Append( "<ul>" );

                foreach ( Rock.CMS.BlogCategory category in blog.BlogCategories.OrderBy( c => c.Name ) )
                {
                    output.Append( "<li><a href=\"" + HttpContext.Current.Request.Url.LocalPath + "?Category=" + category.Id.ToString() + "\">" + category.Name + "</a></li>" );
                }

                output.Append( "</ul>" );

                lCategories.Text = output.ToString();
            }
        }

    }
}