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
using System.Web.Http;
using System.Xml;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;
using church.ccv.Podcast;
using Newtonsoft.Json;

namespace chuch.ccv.Podcast.Rest
{
    public partial class PodcastController : Rock.Rest.ApiController<Category>
    {
        public PodcastController() : base( new Rock.Model.CategoryService( new Rock.Data.RockContext() ) ) { } 
        
        const string GetImageEndpoint = "GetImage.ashx?guid=";
                
        [HttpGet]
        [System.Web.Http.Route( "api/Podcast/Retrieve/{platform}/{version}/{category}" )]
        public HttpResponseMessage Retrieve( string platform, int version, int category )
        {
            // first, what platform are we handling?
            StringContent restContent = null;
            switch( platform )
            {
                case "mobile_app":
                {
                    restContent = Retrieve_MobileApp( version );
                    break;
                }

                case "apple_tv":
                {
                    string response = string.Empty;

                    // if no category was specified, give them the root
                    if( category == 0 )
                    {
                        response = Retrieve_RootWithCategories( version );
                    }
                    else
                    {
                        // otherwise, give them their category with a flat list of series
                        response = Retrieve_FlatCategory( version, category );
                    }
                    restContent = new StringContent( response, Encoding.UTF8, "application/json" );
                    break;
                }

                case "roku":
                {
                    string response = string.Empty;

                    // if no category was specified, give them the root
                    if( category == 0 )
                    {
                        response = Retrieve_RootWithCategories( version );
                    }
                    else
                    {
                        // otherwise, give them their category with a flat list of series
                        response = Retrieve_FlatCategory( version, category );
                    }

                    restContent = new StringContent( response, Encoding.UTF8, "application/json" );
                    break;
                }
            }

            return new HttpResponseMessage()
                {
                    Content = restContent
                };
        }

        string Retrieve_FlatCategory( int version, int categoryId )
        {
            // we will provide the "Root" that they ask for (defiend by categoryId),
            // and then all children as a FLAT LIST OF SERIES.
            //--Root (Weekend Series)(C)
            //----Game Plan (S)
            //----True (S)
            
            // First, get the root category, fully, and with its child categories.
            PodcastUtil.PodcastCategory rootCategory = PodcastUtil.GetPodcastsByCategory( categoryId, false );
            
            return JsonConvert.SerializeObject( rootCategory );
        }

        string Retrieve_RootWithCategories( int version )
        {
            // we will provide the "Root" that they ask for (defiend by categoryId),
            // and then the child series and immediate child categories. The child categories will contain all their
            // children as a FLAT SERIES LIST.
            //--Root (C)
            //----Weekend Series (C)
            //--------Game Plan (S)
            //--------True (S)
            //----Neighborhoood Videos (C)
            //--------Walk Thru John (S) [Note, this might actually be in a child category of Neighborhood Videos, but we flatten those.

            // First, get the root category, fully, and with its child categories.
            PodcastUtil.PodcastCategory fullRootCategory = PodcastUtil.GetPodcastsByCategory( 0, true );

            // now, we want to create a new root with only any immediate series as its children
            PodcastUtil.PodcastCategory rootCategory = new PodcastUtil.PodcastCategory( fullRootCategory.Name, fullRootCategory.Id );
            foreach( PodcastUtil.IPodcastNode node in fullRootCategory.Children )
            {
                if ( ( node as PodcastUtil.PodcastSeries ) != null )
                {
                    rootCategory.Children.Add( node );
                }
            }
            
            // now load the category they care about, so we can get its children
            RockContext rockContext = new RockContext( );
            var category = new CategoryService( rockContext ).Queryable( ).Where( c => c.Id == fullRootCategory.Id ).SingleOrDefault( );

            // finally, recursively load all child categories, but flatten it all into one list per child category
            foreach( Category childCategory in category.ChildCategories )
            {
                PodcastUtil.PodcastCategory podcasts = PodcastUtil.GetPodcastsByCategory( childCategory.Id, false );
                
                rootCategory.Children.Add( podcasts );
            }
            
            return JsonConvert.SerializeObject( rootCategory );
        }
        
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Podcast/GetChildren/{id}" )]
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

            var itemsQry = GetPodcastsByCategory( parentItemId );
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
                        var childItems = GetPodcastsByCategory( parentId );
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

        static IQueryable<ContentChannel> GetPodcastsByCategory( int categoryId )
        {
            // if there's a valid category ID, find content channel items
            if( categoryId != 0 )
            {
                RockContext rockContext = new RockContext( );

                // get the category that owns all the content channel items we care about
                var categoryList = new CategoryService( rockContext ).Queryable( ).Where( c => c.Id == categoryId ).SingleOrDefault( );
            
                // create a query that'll get all of the "Category" attributes for all the content channels
                var categoryAttribValList = new AttributeValueService( rockContext ).Queryable( ).Where( av => av.Attribute.Guid == new Guid( church.ccv.Utility.SystemGuids.Attribute.CONTENT_CHANNEL_CATEGORY_ATTRIBUTE ) );

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

        // eventually it'd be better to replace the mobile app endpoint with one that uses json like the rest. Until then..
        StringContent Retrieve_MobileApp( int version )
        {
            using ( StringWriter stringWriter = new StringWriterWithEncoding(Encoding.UTF8) )
            {
                string publicApplicationRoot = string.Empty;

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
                    publicApplicationRoot = new AttributeValueService( rockContext ).Queryable( ).Where( av => av.AttributeId == attribQuery.Id ).SingleOrDefault( ).Value;


                    // start with the root node and header info
                    writer.WriteStartDocument( true );
                    writer.WriteStartElement( "NoteDB" );
                                    
                    // write the series list, which is the heart of the XML
                    // begin the serisList section
                    writer.WriteStartElement( "SeriesList" );

                    // get all content channel types in the "Weekend Series" podcast
                    IQueryable<ContentChannel> seriesContentChannels = GetPodcastsByCategory( PodcastUtil.WeekendVideos_CategoryId );
                    IQueryable<AttributeValue> attribValueQuery = new AttributeValueService( new RockContext( ) ).Queryable( );
            
                    foreach( ContentChannel series in seriesContentChannels )
                    {
                        // pull in all this series' attributes
                        List<AttributeValue> contentChannelAttribValList = attribValueQuery.Where( av => av.EntityId == series.Id && 
                                                                                                    av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" ).ToList( );

                        // don't inlude series that aren't active
                        bool activeValue = contentChannelAttribValList.Where( av => av.AttributeKey == "Active" ).SingleOrDefault( ).Value == "True" ? true : false;
                        if( activeValue )
                        {
                            // write the "Series" start element
                            writer.WriteStartElement( "Series" );

                            // Put each needed XML element
                            writer.WriteStartElement( "SeriesName" );
                            writer.WriteValue( series.Name );
                            writer.WriteEndElement( );

                            writer.WriteStartElement( "Description" );
                            writer.WriteValue( series.Description );
                            writer.WriteEndElement( );

                            // parse and setup the date range for the series
                            string[] dateRanges = contentChannelAttribValList.Where( av => av.AttributeKey == "DateRange" ).SingleOrDefault( ).Value.Split( ',' );
                            string startDate = DateTime.Parse( dateRanges[ 0 ] ).ToShortDateString( );
                            string endDate = DateTime.Parse( dateRanges[ 1 ] ).ToShortDateString( );

                            writer.WriteStartElement( "DateRanges" );
                            writer.WriteValue( startDate + " - " + endDate );
                            writer.WriteEndElement( );

                            // The images will be Guids with the GetImage path prefixed
                            writer.WriteStartElement( "BillboardUrl" );
                            string billboardUrl = publicApplicationRoot + GetImageEndpoint + contentChannelAttribValList.Where( av => av.AttributeKey == "BillboardImage" ).SingleOrDefault( ).Value;
                            writer.WriteValue( billboardUrl );
                            writer.WriteEndElement( );

                            writer.WriteStartElement( "ThumbnailUrl" );
                            string thumbnailUrl = publicApplicationRoot + GetImageEndpoint + contentChannelAttribValList.Where( av => av.AttributeKey == "ThumbnailImage" ).SingleOrDefault( ).Value;
                            writer.WriteValue( thumbnailUrl );
                            writer.WriteEndElement( );

                
                            // Now generate each message of the series
                            foreach( ContentChannelItem message in series.Items )
                            {
                                // only include items whose start date has already begun and that have been approved
                                if( message.StartDateTime < DateTime.Now && message.Status == ContentChannelItemStatus.Approved )
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
                                    writer.WriteEndElement();
                                }

                                // close the series
                                writer.WriteEndElement();
                            }
                        }
                    }

                    // close the seriesList
                    writer.WriteEndElement( );

                    
                    // close out the root node and document
                    writer.WriteEndElement( );
                    writer.WriteEndDocument( );

                    // dump to the stringWriter's stream
                    writer.Flush();
                }

                // return the XML
                return new StringContent( stringWriter.ToString(), Encoding.UTF8, "application/xml" );
            }
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
}
