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
using System.Globalization;
using System.Net;

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
            switch( platform.ToLower( ) )
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

                case "itunes_video":
                {
                    restContent = Retrieve_iTunesRSS( version, true );
                    break;
                }

                case "itunes_audio":
                {
                    restContent = Retrieve_iTunesRSS( version, false );
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
                if ( category.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
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
                    if ( categorizedItem != null && categorizedItem.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
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
                        if ( childCategory.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
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
                                if ( categorizedItem != null && categorizedItem.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
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

        // Ok, ideally this part would be data driven, but this is just for CCV, so who cares.
        const string iTunesRSS_VideoTitle = "CCV Video Messages (Christ's Church of the Valley)";
        const string iTunesRSS_AudioTitle = "CCV Audio Messages (Christ's Church of the Valley)";
        const string iTunesRSS_Copyright = "{0} Christ's Church of the Valley";
        const string iTunesRSS_Description = "At CCV it is our mission to WIN people to Jesus Christ, TRAIN believers to become disciples, and SEND disciples out to impact the world. The contemporary worship services are designed to encourage and inspire you with relevant music and messages.";
        const string iTunesRSS_Subtitle = "CCV";
        const string iTunesRSS_Author = "Christ's Church of the Valley";
        const string iTunesRSS_Summary = "CCV (Christ's Church of the Valley) is a nondenominational church with multiple locations in the Phoenix area.";
        const string iTunesRSS_Image = "http://media.ccvonline.com/images/itunes/messages.png";
        const string iTunesRSS_OwnerName = "Christ's Church of the Valley";
        const string iTunesRSS_OwnerEmail = "communications@ccv.church";
        const string iTunesRSS_Keywords = "CCV,Christs,Church,valley,Don,Wilson";
        const int MaxNumPodcasts = 50;

        StringContent Retrieve_iTunesRSS( int version, bool wantVideo )
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

                    // since we're using the same function for video or audio, setup the values that differ.
                    string rssTitle = wantVideo == true ? iTunesRSS_VideoTitle : iTunesRSS_AudioTitle;
                    string mediaType = wantVideo == true ? "video/mp4" : "audio/mpeg";
                    string mediaKindKey = wantVideo == true ? "HostedVideoUrl" : "AudioUrl";
                    string mediaLengthKey = wantVideo == true ? "HostedVideoLength" : "AudioLength";
                    string mediaFilesizeKey = wantVideo == true ? "HostedVideoFilesize" : "AudioFilesize";
                    string mediaUrl = wantVideo == true ? "itunes_video" : "itunes_audio";

                    // start with the root node and header info
                    string iTunesNamespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
                    string atomNamespace = "http://www.w3.org/2005/Atom";
                    string rssSource = publicApplicationRoot + string.Format( "api/Podcast/Retrieve/{0}/{1}/0", mediaUrl, version );

                    writer.WriteStartDocument( true );
                    writer.WriteStartElement( "rss" );
                    writer.WriteAttributeString( "xmlns", "itunes", null, iTunesNamespace );
                    writer.WriteAttributeString( "version", "2.0" );

                    // start our RSS channel
                    writer.WriteStartElement( "channel" );
                    writer.WriteAttributeString( "xmlns", "atom", null, atomNamespace );

                    writer.WriteStartElement( "atom", "link", atomNamespace );
                    writer.WriteAttributeString( "href", rssSource );
                    writer.WriteAttributeString( "rel", "self" );
                    writer.WriteAttributeString( "type", "application/rss+xml" );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "title" );
                    writer.WriteValue( rssTitle );
                    writer.WriteEndElement( );
                    
                    writer.WriteStartElement( "link" );
                    writer.WriteValue( rssSource );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "language" );
                    writer.WriteValue( "en-us" );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "copyright" );
                    writer.WriteValue( string.Format( iTunesRSS_Copyright, DateTime.Now.Year.ToString( ) ) );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "description" );
                    writer.WriteValue( iTunesRSS_Description );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "itunes", "subtitle", iTunesNamespace );
                    writer.WriteValue( iTunesRSS_Subtitle );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "itunes", "author", iTunesNamespace );
                    writer.WriteValue( iTunesRSS_Author );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "itunes", "summary", iTunesNamespace );
                    writer.WriteValue( iTunesRSS_Summary );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "itunes", "image", iTunesNamespace );
                    writer.WriteAttributeString( "href", iTunesRSS_Image );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "itunes", "owner", iTunesNamespace );
                        writer.WriteStartElement( "itunes", "name", iTunesNamespace );
                        writer.WriteValue( iTunesRSS_OwnerName );
                        writer.WriteEndElement( );

                        writer.WriteStartElement( "itunes", "email", iTunesNamespace );
                        writer.WriteValue( iTunesRSS_OwnerEmail );
                        writer.WriteEndElement( );
                    writer.WriteEndElement( );


                    writer.WriteStartElement( "itunes", "explicit", iTunesNamespace );
                    writer.WriteValue( "no" );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "itunes", "keywords", iTunesNamespace );
                    writer.WriteValue( iTunesRSS_Keywords );
                    writer.WriteEndElement( );

                    writer.WriteStartElement( "itunes", "category", iTunesNamespace );
                    writer.WriteAttributeString( "text", "Religion & Spirituality" );

                        writer.WriteStartElement( "itunes", "category", iTunesNamespace );
                        writer.WriteAttributeString( "text", "Christianity" );    
                        writer.WriteEndElement( );

                    writer.WriteEndElement( );

                    // get all content channel types in the "Weekend Series" podcast
                    IQueryable<ContentChannel> seriesContentChannels = GetPodcastsByCategory( PodcastUtil.WeekendVideos_CategoryId );
                    IQueryable<AttributeValue> attribValueQuery = new AttributeValueService( new RockContext( ) ).Queryable( );

                    int numPodcastsAdded = 0;
            
                    foreach( ContentChannel series in seriesContentChannels )
                    {
                        // pull in all this series' attributes
                        List<AttributeValue> contentChannelAttribValList = attribValueQuery.Where( av => av.EntityId == series.Id && 
                                                                                                    av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" ).ToList( );

                        // don't inlude series that aren't active
                        bool activeValue = contentChannelAttribValList.Where( av => av.AttributeKey == "Active" ).SingleOrDefault( ).Value == "True" ? true : false;
                        if( activeValue )
                        {
                            // Now each message of the series
                            foreach( ContentChannelItem message in series.Items )
                            {
                                // only include items whose start date has already begun and that have been approved
                                if( message.StartDateTime < DateTime.Now && message.Status == ContentChannelItemStatus.Approved )
                                {
                                    // get this message's attributes
                                    List<AttributeValue> itemAttribValList = attribValueQuery.Where( av => av.EntityId == message.Id && 
                                                                                                           av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" ).ToList( );

                                    // there _must_ be a mediaURL and length in order for us to generate the podcast entry
                                    AttributeValue mediaUrlAV = itemAttribValList.Where( av => av.AttributeKey == mediaKindKey ).SingleOrDefault( );
                                    AttributeValue mediaLengthAV = itemAttribValList.Where( av => av.AttributeKey == mediaLengthKey ).SingleOrDefault( );
                                    
                                    if( mediaUrlAV != null && string.IsNullOrWhiteSpace( mediaUrlAV.Value ) == false && 
                                        mediaLengthAV != null && string.IsNullOrWhiteSpace( mediaLengthAV.Value) == false)
                                    {
                                        // before anything else, get the length of this content
                                        WebRequest webReq = HttpWebRequest.Create( mediaUrlAV.Value );
                                        webReq.Method = "HEAD";
                                        using ( WebResponse webResponse = webReq.GetResponse() )
                                        {
                                            string contentLength = webResponse.Headers["Content-Length"];
                                            if( string.IsNullOrWhiteSpace( contentLength ) == false  )
                                            {
                                                writer.WriteStartElement( "item" );

                                                // Put required elements
                                                writer.WriteStartElement( "title" );
                                                writer.WriteValue( message.Title );
                                                writer.WriteEndElement( );

                                                writer.WriteStartElement( "pubDate" );
                                                writer.WriteValue( message.StartDateTime.ToString( "r" ) );
                                                writer.WriteEndElement( );

                                                writer.WriteStartElement( "itunes", "author", iTunesNamespace );
                                                writer.WriteValue( iTunesRSS_Author );
                                                writer.WriteEndElement( );

                                                writer.WriteStartElement( "itunes", "image", iTunesNamespace );
                                                writer.WriteAttributeString( "href", iTunesRSS_Image );
                                                writer.WriteEndElement( );

                                                writer.WriteStartElement( "itunes", "summary", iTunesNamespace );
                                                writer.WriteValue( series.Description );
                                                writer.WriteEndElement( );

                                                writer.WriteStartElement( "itunes", "subtitle", iTunesNamespace );
                                                writer.WriteValue( iTunesRSS_Subtitle );
                                                writer.WriteEndElement( );
                                        
                                                writer.WriteStartElement( "enclosure" );
                                                    writer.WriteAttributeString( "url", mediaUrlAV.Value );
                                                    writer.WriteAttributeString( "length", contentLength );
                                                    writer.WriteAttributeString( "type", mediaType );
                                                writer.WriteEndElement( );

                                                writer.WriteStartElement( "guid" );
                                                writer.WriteValue( mediaUrlAV.Value );
                                                writer.WriteEndElement( );

                                                writer.WriteStartElement( "itunes", "duration", iTunesNamespace );
                                                writer.WriteValue( mediaLengthAV.Value );
                                                writer.WriteEndElement( );
                                        
                                                writer.WriteStartElement( "itunes", "keywords", iTunesNamespace );
                                                writer.WriteValue( iTunesRSS_Keywords );
                                                writer.WriteEndElement( );

                                                // close the message
                                                writer.WriteEndElement();

                                                // now see if we should stop iterating because we hit our limit
                                                numPodcastsAdded++;
                                            }
                                        }
                                    } 
                                } 

                                // fall out of messages if we hit our limit
                                if( numPodcastsAdded >= MaxNumPodcasts )
                                {
                                    break;
                                }
                            } // End Message Loop
                        }

                        // fall out of series as well
                        if( numPodcastsAdded >= MaxNumPodcasts )
                        {
                            break;
                        }

                    } //End Series Loop

                    // close the channel
                    writer.WriteEndElement( );

                    
                    // close out the root node 'rss'
                    writer.WriteEndElement( );
                    writer.WriteEndDocument( );

                    // dump to the stringWriter's stream
                    writer.Flush();
                }

                // return the XML
                return new StringContent( stringWriter.ToString(), Encoding.UTF8, "application/xml" );
            }
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

                            // The images will be Guids with the GetImage path prefixed (we'll also fix the resolution since that what the mobile app expects)
                            writer.WriteStartElement( "BillboardUrl" );
                            string billboardUrl = publicApplicationRoot + GetImageEndpoint + contentChannelAttribValList.Where( av => av.AttributeKey == "16_9_Image" ).SingleOrDefault( ).Value;
                            billboardUrl += "&width=750&height=422";
                            writer.WriteValue( billboardUrl );
                            writer.WriteEndElement( );

                            writer.WriteStartElement( "ThumbnailUrl" );
                            string thumbnailUrl = publicApplicationRoot + GetImageEndpoint + contentChannelAttribValList.Where( av => av.AttributeKey == "1_1_Image" ).SingleOrDefault( ).Value;
                            thumbnailUrl += "&width=140&height=140";
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
                            }

                            // close the series
                            writer.WriteEndElement();
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
