// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace chuch.ccv.Podcasts.Rest
{
    public partial class ContentChannelCategoriesController : Rock.Rest.ApiController<Category>
    {
        public ContentChannelCategoriesController() : base( new Rock.Model.CategoryService( new Rock.Data.RockContext() ) ) { } 

        const string ContentChannel_CategoryAttributeGuid = "DEA4ACCE-82F6-43E3-B381-6959FBF66E74";
        const int WeekendVideos_CategoryId = 451; 
        const string GetImageEndpoint = "GetImage.ashx?guid=";

        string PublicApplicationRoot { get; set; }
        
        [System.Web.Http.Route( "api/ContentChannelCategories/GetNotes/" )]
        public HttpResponseMessage GetNotes( )
        {
            using ( StringWriter stringWriter = new StringWriterWithEncoding(Encoding.UTF8) )
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\t";
                settings.NewLineOnAttributes = true;
                settings.Encoding = System.Text.Encoding.UTF8;
                
                using ( XmlWriter writer = XmlWriter.Create( stringWriter, settings) )
                {
                    // first, get the public application root
                    RockContext rockContext = new RockContext( );
                    var attribQuery = new AttributeService( rockContext ).Queryable( ).Where( a => a.Key == "PublicApplicationRoot" ).SingleOrDefault( );
                    PublicApplicationRoot = new AttributeValueService( rockContext ).Queryable( ).Where( av => av.AttributeId == attribQuery.Id ).SingleOrDefault( ).Value;


                    // start with the root node and header info
                    writer.WriteStartDocument( true );
                    writer.WriteStartElement( "NoteDB" );

                    //todo: decide how to manage this
                    /*writer.WriteStartElement( "HostDomain" );
                    writer.WriteString( PublicApplicationRoot );
                    writer.WriteEndElement( );*/
                                    
                    // write the series list, which is the heart of the XML
                    WriteSeriesList( writer );
                    
                    // close out the root node and document
                    writer.WriteEndElement( );
                    writer.WriteEndDocument( );

                    // dump to the stringWriter's stream
                    writer.Flush();
                }

                // return the XML
                return new HttpResponseMessage()
                {
                    Content = new StringContent( stringWriter.ToString(), Encoding.UTF8, "application/xml" )
                };
            }
        }
        
        private void WriteSeriesList( XmlWriter writer )
        {
            // begin the serisList section
            writer.WriteStartElement( "SeriesList" );

            // get all content channel types in the "Weekend Series" podcast
            IQueryable<ContentChannel> seriesContentChannels = GetCategorizedItems( WeekendVideos_CategoryId );
            IQueryable<AttributeValue> attribValueQuery = new AttributeValueService( new RockContext( ) ).Queryable( );
            
            foreach( ContentChannel series in seriesContentChannels )
            {
                writer.WriteStartElement( "Series" );

                // pull in all this series' attributes
                List<AttributeValue> contentChannelAttribValList = attribValueQuery.Where( av => av.EntityId == series.Id && 
                                                                                           av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" ).ToList( );

                // Put each needed XML element
                writer.WriteStartElement( "SeriesName" );
                writer.WriteValue( series.Name );
                writer.WriteEndElement( );

                writer.WriteStartElement( "Description" );
                writer.WriteValue( series.Description );
                writer.WriteEndElement( );

                writer.WriteStartElement( "DateRanges" );
                writer.WriteValue( contentChannelAttribValList.Where( av => av.AttributeKey == "DateRange" ).SingleOrDefault( ).Value );
                writer.WriteEndElement( );

                // The images will be Guids with the GetImage path prefixed
                writer.WriteStartElement( "BillboardUrl" );
                string billboardUrl = PublicApplicationRoot + GetImageEndpoint + contentChannelAttribValList.Where( av => av.AttributeKey == "BillboardImage" ).SingleOrDefault( ).Value;
                writer.WriteValue( billboardUrl );
                writer.WriteEndElement( );

                writer.WriteStartElement( "ThumbnailUrl" );
                string thumbnailUrl = PublicApplicationRoot + GetImageEndpoint + contentChannelAttribValList.Where( av => av.AttributeKey == "ThumbnailImage" ).SingleOrDefault( ).Value;
                writer.WriteValue( thumbnailUrl );
                writer.WriteEndElement( );

                
                // Now generate each message of the series
                foreach( ContentChannelItem message in series.Items )
                {
                    // get this message's attributes
                    List<AttributeValue> itemAttribValList = attribValueQuery.Where( av => av.EntityId == message.Id && 
                                                                                           av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" ).ToList( );

                    writer.WriteStartElement( "Message" );

                    // Put required elements
                    writer.WriteStartElement( "Name" );
                    writer.WriteValue( message.Title );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "Speaker" );
                    writer.WriteValue( itemAttribValList.Where( av => av.AttributeKey == "Speaker" ).SingleOrDefault( ).Value );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "Date" );
                    writer.WriteValue( message.StartDateTime.ToShortDateString( ) );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "NoteUrl" );
                    writer.WriteValue( itemAttribValList.Where( av => av.AttributeKey == "NoteUrl" ).SingleOrDefault( ).Value );
                    writer.WriteEndElement( );

                    // Watch/Share/Audio URLs are optional, so check that they exist
                    string watchUrlValue = itemAttribValList.Where( av => av.AttributeKey == "WatchUrl" ).SingleOrDefault( ).Value;
                    if( string.IsNullOrEmpty( watchUrlValue ) == false )
                    {
                        writer.WriteStartElement( "WatchUrl" );
                        writer.WriteValue( watchUrlValue );
                        writer.WriteEndElement();
                    }
                    
                    string shareUrlValue = itemAttribValList.Where( av => av.AttributeKey == "ShareUrl" ).SingleOrDefault( ).Value;
                    if( string.IsNullOrEmpty( shareUrlValue ) == false )
                    {
                        writer.WriteStartElement( "ShareUrl" );
                        writer.WriteValue( shareUrlValue );
                        writer.WriteEndElement();
                    }

                    string audioUrlValue = itemAttribValList.Where( av => av.AttributeKey == "AudioUrl" ).SingleOrDefault( ).Value;
                    if( string.IsNullOrEmpty( audioUrlValue ) == false )
                    {
                        writer.WriteStartElement( "AudioUrl" );
                        writer.WriteValue( audioUrlValue );
                        writer.WriteEndElement();
                    }

                    // close the message
                    writer.WriteEndElement( );
                }
                
                // close the series
                writer.WriteEndElement( );
            }

            // close the seriesList
            writer.WriteEndElement( );
        }
        
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ContentChannelCategories/GetChildren/{id}" )]
        public IQueryable<CategoryItem> GetChildren(
            int id,
            int rootCategoryId = 0,
            int entityTypeId = 0,
            string defaultIconCssClass = null )
        {
            Person currentPerson = GetPerson();
            defaultIconCssClass = defaultIconCssClass ?? "fa fa-list-ol";

            IQueryable<Category> qry = Get();

            if ( id == 0 )
            {
                if ( rootCategoryId != 0 )
                {
                    qry = qry.Where( a => a.ParentCategoryId == rootCategoryId );
                }
                else
                {
                    qry = qry.Where( a => a.ParentCategoryId == null );
                }
            }
            else
            {
                qry = qry.Where( a => a.ParentCategoryId == id );
            }

            var cachedEntityType = EntityTypeCache.Read( entityTypeId );
            if ( cachedEntityType != null )
            {
                qry = qry.Where( a => a.EntityTypeId == entityTypeId );
            }
            
            List<Category> categoryList = qry.OrderBy( c => c.Order ).ThenBy( c => c.Name ).ToList();
            List<CategoryItem> categoryItemList = new List<CategoryItem>();

            // Build the Category Nodes (Not the Category ITEMS, which will be Content Channel Items)
            foreach ( var category in categoryList )
            {
                if ( category.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    var categoryItem = new CategoryItem();
                    categoryItem.Id = category.Id.ToString();
                    categoryItem.Name = category.Name;
                    categoryItem.IsCategory = true;
                    categoryItem.IconCssClass = category.IconCssClass;
                    categoryItemList.Add( categoryItem );
                }
            }

            // this is where I should get the ACTUAL CONTENT CHANNEL ITEMS
            // if id is zero and we have a rootCategory, show the children of that rootCategory (but don't show the rootCategory)
            int parentItemId = id == 0 ? rootCategoryId : id;

            var itemsQry = GetCategorizedItems( parentItemId );
            if ( itemsQry != null )
            {
                // do a ToList to load from database prior to ordering by name, just in case Name is a virtual property
                var itemsList = itemsQry.ToList();

                foreach ( var categorizedItem in itemsList.OrderBy( i => i.Name ) )
                {
                    if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        var categoryItem = new CategoryItem();
                        categoryItem.Id = categorizedItem.Id.ToString();
                        categoryItem.Name = categorizedItem.Name;
                        categoryItem.IsCategory = false;
                        categoryItem.IconCssClass = categorizedItem.GetPropertyValue( "IconCssClass" ) as string ?? defaultIconCssClass;
                        categoryItem.IconSmallUrl = string.Empty;
                        categoryItemList.Add( categoryItem );
                    }
                }
            }

            // try to figure out which items have viewable children
            foreach ( var g in categoryItemList )
            {
                if ( g.IsCategory )
                {
                    int parentId = int.Parse( g.Id );

                    foreach ( var childCategory in Get().Where( c => c.ParentCategoryId == parentId ) )
                    {
                        if ( childCategory.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        {
                            g.HasChildren = true;
                            break;
                        }
                    }

                    if ( !g.HasChildren )
                    {
                        var childItems = GetCategorizedItems( parentId );
                        if ( childItems != null )
                        {
                            foreach ( var categorizedItem in childItems )
                            {
                                if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                                {
                                    g.HasChildren = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
            return categoryItemList.AsQueryable();
        }
        
        private IQueryable<ContentChannel> GetCategorizedItems( int categoryId )
        {
            // if there's a valid category ID, find content channel items
            if( categoryId != 0 )
            {
                RockContext rockContext = new RockContext( );

                // get the category that owns all the content channel items we care about
                var categoryList = new CategoryService( rockContext ).Queryable( ).Where( c => c.Id == categoryId ).SingleOrDefault( );
            
                // create a query that'll get all of the "Category" attributes for all the content channels
                var categoryAttribValList = new AttributeValueService( rockContext ).Queryable( ).Where( av => av.Attribute.Guid == new Guid( ContentChannel_CategoryAttributeGuid  ) );

                // now get all the content channels, with their parent category(s) attributes as a joined object
                ContentChannelService contentChannelService = new ContentChannelService( rockContext );
                var categoryContentChannelItems = contentChannelService.Queryable( ).Join( categoryAttribValList, 
                                                                                           cc => cc.Id, cav => cav.EntityId, ( cc, cav ) => new { ContentChannel = cc, CategoryAttribValue = cav } );

                // now only take content channel items whose category attribute (which is a list of category guids) includes the category defined by categoryId
                var finalList = categoryContentChannelItems.Where( cci => cci.CategoryAttribValue.Value.Contains( categoryList.Guid.ToString( ) ) ).Select( cci => cci.ContentChannel );


                return finalList;
            }

            return null;
        }
    }

    // Inherit StringWriter so we can set the encoding, which is protected
    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding (Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
    
    public class ContentChannelCategoryItem : Rock.Web.UI.Controls.TreeViewItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is category.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is category; otherwise, <c>false</c>.
        /// </value>
        public bool IsCategory { get; set; }
    }
}
