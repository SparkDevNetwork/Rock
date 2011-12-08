//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Web;

using Rock.CMS;

namespace RockWeb
{
    /// <summary>
    /// Summary description for feed
    /// </summary>
    public class feed : IHttpHandler
    {
        protected string format = string.Empty;
        protected int count = 0;
        protected int key = 0;
        protected string type = string.Empty;

        public void ProcessRequest( HttpContext context )
        {
            // get feed format
            if (context.Request["format"] != null)
                format = context.Request["format"].ToString();
            else
                format = "rss";

            // get return count (default 20)
            if ( int.TryParse( context.Request["count"], out count ) == false )
            {
                count = 20;
            }

            // get object type
            if ( context.Request["type"] != null )
                type = context.Request["type"].ToString();

            // get object key
            int.TryParse( context.Request["key"], out key );

            context.Response.Clear();

            if ( type != string.Empty )
            {
                string errorMessage = string.Empty;
                string contentType = string.Empty;

                BlogService blogService = new BlogService();
                string feedXml = blogService.ReturnFeed(key, count, format, out errorMessage, out contentType);

                if ( errorMessage == string.Empty )
                {
                    context.Response.ContentType = contentType;
                    context.Response.Write( feedXml );
                }
                else
                {
                    context.Response.ContentType = "text/html";
                    context.Response.Write( errorMessage );
                }

                context.Response.End();
            }
            else
            {
                
                context.Response.ContentType = "text/html";

                context.Response.Write( "No Feed Type Provided." );

                context.Response.End();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}