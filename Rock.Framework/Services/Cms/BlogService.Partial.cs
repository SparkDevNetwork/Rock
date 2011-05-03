using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

using System.Xml;

using Rock.Models.Cms;

namespace Rock.Services.Cms
{
    public partial class BlogService: IFeed
    {
        public string ReturnFeed( int key, int count, string format, out string errorMessage, out string contentType )
        {
            errorMessage = string.Empty;
            contentType = "application/rss+xml";

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;

            StringBuilder builder = new StringBuilder();

            using ( XmlWriter writer = XmlWriter.Create( builder, settings ) )
            {
                // get blog
                Blog blog = this.GetBlog( key );

                if ( blog == null )
                {
                    errorMessage = "The blog with the id of " + key.ToString() + " could not be found.";
                }
                else
                {
                    writer.WriteStartDocument();

                    // write rss Tags
                    writer.WriteStartElement( "rss" );
                    writer.WriteAttributeString( "version", "2.0" );

                    writer.WriteStartElement( "channel" );
                    writer.WriteElementString( "title", blog.Name );
                    writer.WriteElementString( "link", blog.PublicPublishingPoint );
                    writer.WriteElementString( "description", blog.Description );
                    writer.WriteElementString( "copyright", blog.CopyrightStatement );

                    // get posts
                    var blogPosts = blog.BlogPosts.Take( count );

                    foreach ( BlogPost post in blogPosts )
                    {
                        writer.WriteStartElement( "item" );
                        writer.WriteElementString( "title", post.Title );
                        writer.WriteElementString( "description", post.Content );
                        writer.WriteElementString( "link", "" );
                        writer.WriteElementString( "pubDate", post.PublishDate.ToString() );
                        writer.WriteEndElement();
                    }

                    //  close up tags
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                writer.Flush();
                writer.Close();

            }
            return builder.ToString();
        }
    }
}
