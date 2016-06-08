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
        [HttpHead]
        [System.Web.Http.Route( "api/Podcast/Category/{categoryId}/{platform}/{version}" )]
        public HttpResponseMessage Category( int categoryId, string platform, int version, int numSeries = int.MaxValue, bool expandSeries = true )
        {
            // first, what platform are we handling?
            StringContent restContent = null;
            switch( platform.ToLower( ) )
            {
                case "mobile_app":
                {
                    restContent = Retrieve_MobileApp( version, numSeries, expandSeries );
                    break;
                }

                case "apple_tv":
                {
                    string response = string.Empty;

                    // if no category was specified, give them the root
                    if( categoryId == 0 )
                    {
                        response = Retrieve_RootWithCategories( version, numSeries, expandSeries );
                    }
                    else
                    {
                        // otherwise, give them their category with a flat list of series
                        response = Retrieve_FlatCategory( version, categoryId, numSeries, expandSeries );
                    }
                    restContent = new StringContent( response, Encoding.UTF8, "application/json" );
                    break;
                }

                case "roku":
                {
                    string response = string.Empty;

                    // if no category was specified, give them the root
                    if( categoryId == 0 )
                    {
                        response = Retrieve_RootWithCategories( version, numSeries, expandSeries );
                    }
                    else
                    {
                        // otherwise, give them their category with a flat list of series
                        response = Retrieve_FlatCategory( version, categoryId, numSeries, expandSeries );
                    }

                    restContent = new StringContent( response, Encoding.UTF8, "application/json" );
                    break;
                }

                case "itunes_video":
                {
                    restContent = Retrieve_iTunesRSS( version, true, numSeries );
                    break;
                }

                case "itunes_audio":
                {
                    restContent = Retrieve_iTunesRSS( version, false, numSeries );
                    break;
                }
            }

            return new HttpResponseMessage()
                {
                    Content = restContent
                };
        }

        [HttpGet]
        [HttpHead]
        [System.Web.Http.Route( "api/Podcast/Series/{seriesId}" )]
        public HttpResponseMessage Series( int seriesId, bool expandSeries = true )
        {
            // get the requested series
            PodcastUtil.PodcastSeries series = PodcastUtil.GetSeries( seriesId, expandSeries );
            
            if( series != null )
            {
                // serialize it into json
                StringContent restContent = new StringContent( JsonConvert.SerializeObject( series ), Encoding.UTF8, "application/json" );

                // return it.
                return new HttpResponseMessage()
                {
                    Content = restContent
                };
            }
            else
            {
                // wasn't found, so return NotFound
                return new HttpResponseMessage( ) { StatusCode = HttpStatusCode.NotFound };
            }
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

            var itemsQry = GetTreePodcastsByCategory( parentItemId );
            if ( itemsQry != null )
            {
                // do a ToList to load from database prior to ordering by name, just in case Name is a virtual property
                var itemsList = itemsQry.ToList();

                foreach ( var categorizedItem in itemsList.OrderByDescending( i => i.CreatedDateTime ) )
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
                        var childItems = GetTreePodcastsByCategory( parentId );
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

        string Retrieve_FlatCategory( int version, int categoryId, int numSeries, bool expandSeries )
        {
            // we will provide the "Root" that they ask for (defiend by categoryId),
            // and then all children as a FLAT LIST OF SERIES.
            //--Root (Weekend Series)(C)
            //----Game Plan (S)
            //----True (S)
            
            // Get the root category, with all its child series, and pass false so we take all child category series without
            // the categories.
            // Pass numSeries so we limit the amount requested.
            PodcastUtil.PodcastCategory rootCategory = PodcastUtil.GetPodcastsByCategory( categoryId, false, numSeries, expandSeries );
            
            return JsonConvert.SerializeObject( rootCategory );
        }

        string Retrieve_RootWithCategories( int version, int numSeries, bool expandSeries )
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
            PodcastUtil.PodcastCategory fullRootCategory = PodcastUtil.GetPodcastsByCategory( 0, true, numSeries, expandSeries );

            // now, we want to create a new root with only any immediate series as its children
            PodcastUtil.PodcastCategory rootCategory = new PodcastUtil.PodcastCategory( fullRootCategory.Name, fullRootCategory.Id );
            foreach( PodcastUtil.IPodcastNode node in fullRootCategory.Children )
            {
                if ( ( node as PodcastUtil.PodcastSeries ) != null )
                {
                    rootCategory.Children.Add( node );

                    // track the series we're adding
                    numSeries--;
                }
            }
            
            if( numSeries > 0 )
            {
                // now load the category they care about, so we can get its children
                RockContext rockContext = new RockContext( );
                var category = new CategoryService( rockContext ).Queryable( ).Where( c => c.Id == fullRootCategory.Id ).SingleOrDefault( );

                // finally, recursively load all child categories, but flatten it all into one list per child category
                foreach ( Category childCategory in category.ChildCategories )
                {
                    // make sure we haven't hit our series limit
                    if ( numSeries > 0 )
                    {
                        // get as many series as numSeries is set to
                        PodcastUtil.PodcastCategory podcasts = PodcastUtil.GetPodcastsByCategory( childCategory.Id, false, numSeries, expandSeries );

                        rootCategory.Children.Add( podcasts );

                        // subtract the number of series we just took (we know the Children are all series because we passed 'false'
                        // to keep hierarchy above)
                        numSeries -= podcasts.Children.Count( );
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            return JsonConvert.SerializeObject( rootCategory );
        }
        
        static IQueryable<ContentChannel> GetTreePodcastsByCategory( int categoryId )
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
        const int MaxNumPodcasts = 50; //Limit the actual number of messages returned to this.

        StringContent Retrieve_iTunesRSS( int version, bool wantVideo, int numSeries )
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
                    string mediaKindKey = wantVideo == true ? "HostedVideoUrl" : "HostedAudioUrl";
                    string mediaLengthKey = wantVideo == true ? "HostedVideoLength" : "HostedAudioLength";
                    string mediaRestUrl = wantVideo == true ? "itunes_video" : "itunes_audio";

                    // start with the root node and header info
                    string iTunesNamespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
                    string atomNamespace = "http://www.w3.org/2005/Atom";
                    string contentNamespace = "http://purl.org/rss/1.0/modules/content/";
                    string rssSource = publicApplicationRoot + string.Format( "api/Podcast/Retrieve/{0}/{1}/0", mediaRestUrl, version );

                    writer.WriteStartDocument( true );
                    writer.WriteStartElement( "rss" );
                    writer.WriteAttributeString( "xmlns", "itunes", null, iTunesNamespace );
                    writer.WriteAttributeString( "xmlns", "content", null, contentNamespace );
                    writer.WriteAttributeString( "xmlns", "atom", null, atomNamespace );
                    writer.WriteAttributeString( "version", "2.0" );

                    // start our RSS channel
                    writer.WriteStartElement( "channel" );

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
                    int numPodcastsAdded = 0;

                    // get the weekend series, and remove all categories inbetween (tho there shouldn't be any)
                    PodcastUtil.PodcastCategory rootCategory = PodcastUtil.GetPodcastsByCategory( PodcastUtil.WeekendVideos_CategoryId, false, numSeries );
                    
                    foreach( PodcastUtil.IPodcastNode podcastNode in rootCategory.Children )
                    {
                        // this is safe to cast to a series, because we ask for only Series by passing false to GetPodcastsByCategory                        
                        PodcastUtil.PodcastSeries series = podcastNode as PodcastUtil.PodcastSeries;

                        // don't inlude series that aren't active
                        bool activeValue = series.Attributes["Active"] == "True" ? true : false;
                        if( activeValue )
                        {
                            // Now each message of the series
                            foreach( PodcastUtil.PodcastMessage message in series.Messages )
                            {
                                // only include items whose start date has already begun and that have been approved
                                if( message.Date <= DateTime.Now && message.Approved == true )
                                {
                                    // there _must_ be a mediaURL and length in order for us to generate the podcast entry
                                    string mediaUrl = message.Attributes[ mediaKindKey ];
                                    string mediaLength = message.Attributes[ mediaLengthKey ];
                                    
                                    if( string.IsNullOrWhiteSpace( mediaUrl ) == false && string.IsNullOrWhiteSpace( mediaLength ) == false)
                                    {
                                        // before anything else, get the length of this content
                                        WebRequest webReq = HttpWebRequest.Create( mediaUrl );
                                        webReq.Method = "HEAD";
                                        try
                                        {
                                            using ( WebResponse webResponse = webReq.GetResponse() )
                                            {
                                                string contentLength = webResponse.Headers["Content-Length"];
                                                if( string.IsNullOrWhiteSpace( contentLength ) == false  )
                                                {
                                                    writer.WriteStartElement( "item" );

                                                    // Put required elements
                                                    writer.WriteStartElement( "title" );
                                                    writer.WriteValue( message.Name );
                                                    writer.WriteEndElement( );


                                                    // setup the summary and extra details. These will be the message's if it has them, and otherwise the series'.
                                                    string itemSummary = string.Empty;
                                                    string itemExtraDetails = string.Empty;

                                                    // Summary
                                                    // does the message have a summary?
                                                    if( string.IsNullOrWhiteSpace( message.Description ) == false )
                                                    {
                                                        itemSummary = message.Description;
                                                    }
                                                    else
                                                    {
                                                        itemSummary = series.Description;
                                                    }
                                                
                                                    // does the message have an extra details value?
                                                    string messageExtraDetails = message.Attributes[ "ExtraDetails" ];
                                                    if( string.IsNullOrWhiteSpace( messageExtraDetails ) == false )
                                                    {
                                                        itemExtraDetails += messageExtraDetails;
                                                    }
                                                    else
                                                    {
                                                        // then does the series?
                                                        string seriesExtraDetails = series.Attributes[ "ExtraDetails" ];
                                                        if( string.IsNullOrWhiteSpace( seriesExtraDetails ) == false )
                                                        {
                                                            itemExtraDetails += seriesExtraDetails;
                                                        }
                                                    }

                                                    writer.WriteStartElement( "description" );
                                                    writer.WriteValue( itemSummary );
                                                    writer.WriteEndElement( );

                                                    // if there WAS item extra details, format it properly
                                                    if( string.IsNullOrWhiteSpace( itemExtraDetails ) == false )
                                                    {
                                                        itemExtraDetails = "<![CDATA[" + itemSummary + itemExtraDetails + "]]>";

                                                        writer.WriteStartElement( "content", "encoded", contentNamespace );
                                                        writer.WriteValue( itemExtraDetails );
                                                        writer.WriteEndElement( );
                                                    }

                                                    writer.WriteStartElement( "itunes", "summary", iTunesNamespace );
                                                    writer.WriteValue( itemSummary );
                                                    writer.WriteEndElement( );

                                                    writer.WriteStartElement( "itunes", "subtitle", iTunesNamespace );
                                                    writer.WriteValue( iTunesRSS_Subtitle );
                                                    writer.WriteEndElement( );


                                                    writer.WriteStartElement( "pubDate" );
                                                    writer.WriteValue( message.Date.Value.ToString( "r" ) );
                                                    writer.WriteEndElement( );

                                                    writer.WriteStartElement( "itunes", "author", iTunesNamespace );
                                                    writer.WriteValue( iTunesRSS_Author );
                                                    writer.WriteEndElement( );

                                                    writer.WriteStartElement( "itunes", "image", iTunesNamespace );
                                                    writer.WriteAttributeString( "href", iTunesRSS_Image );
                                                    writer.WriteEndElement( );
                                        
                                                    writer.WriteStartElement( "enclosure" );
                                                        writer.WriteAttributeString( "url", mediaUrl );
                                                        writer.WriteAttributeString( "length", contentLength );
                                                        writer.WriteAttributeString( "type", mediaType );
                                                    writer.WriteEndElement( );

                                                    writer.WriteStartElement( "guid" );
                                                    writer.WriteValue( mediaUrl );
                                                    writer.WriteEndElement( );

                                                    writer.WriteStartElement( "itunes", "duration", iTunesNamespace );
                                                    writer.WriteValue( mediaLength );
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
                                        catch
                                        {
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
        StringContent Retrieve_MobileApp( int version, int numSeriesRequested, bool expandSeries )
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

                    // get the weekend series, and remove all categories inbetween (tho there shouldn't be any)
                    PodcastUtil.PodcastCategory rootCategory = PodcastUtil.GetPodcastsByCategory( PodcastUtil.WeekendVideos_CategoryId, false, numSeriesRequested, expandSeries );

                    int seriesAdded = 0;
                    
                    foreach( PodcastUtil.IPodcastNode podcastNode in rootCategory.Children )
                    {
                        // this is safe to cast to a series, because we ask for only Series by passing false to GetPodcastsByCategory                        
                        PodcastUtil.PodcastSeries series = podcastNode as PodcastUtil.PodcastSeries;

                        // write the "Series" start element
                        writer.WriteStartElement( "Series" );

                        // if it's not active, set it to "private"
                        bool activeValue = series.Attributes["Active"] == "True" ? true : false;
                        if( activeValue == false )
                        {
                            writer.WriteAttributeString( "Private", "true" );
                        }

                        // Put each needed XML element
                        writer.WriteStartElement( "SeriesName" );
                        writer.WriteValue( series.Name );
                        writer.WriteEndElement( );

                        writer.WriteStartElement( "Description" );
                        writer.WriteValue( series.Description );
                        writer.WriteEndElement( );

                        // parse and setup the date range for the series
                        string[] dateRanges = series.Attributes["DateRange"].Split( ',' );
                        string startDate = DateTime.Parse( dateRanges[ 0 ] ).ToShortDateString( );
                        string endDate = DateTime.Parse( dateRanges[ 1 ] ).ToShortDateString( );

                        writer.WriteStartElement( "DateRanges" );
                        writer.WriteValue( startDate + " - " + endDate );
                        writer.WriteEndElement( );

                        // The images will be Guids with the GetImage path prefixed (we'll also fix the resolution since that what the mobile app expects)
                        writer.WriteStartElement( "BillboardUrl" );
                        string billboardUrl = publicApplicationRoot + GetImageEndpoint + series.Attributes["Image_16_9"];
                        billboardUrl += "&width=750&height=422";
                        writer.WriteValue( billboardUrl );
                        writer.WriteEndElement( );

                        writer.WriteStartElement( "ThumbnailUrl" );
                        string thumbnailUrl = publicApplicationRoot + GetImageEndpoint + series.Attributes["Image_1_1"];
                        thumbnailUrl += "&width=140&height=140";
                        writer.WriteValue( thumbnailUrl );
                        writer.WriteEndElement( );

                
                        // Now generate each message of the series
                        foreach( PodcastUtil.PodcastMessage message in series.Messages )
                        {
                            writer.WriteStartElement( "Message" );

                            // if the message doesn't start yet, or hasn't been approved, set it to private.
                            if( message.Date > DateTime.Now || message.Approved == false )
                            {
                                writer.WriteAttributeString( "Private", "true" );
                            }

                            // Put required elements
                            writer.WriteStartElement( "Name" );
                            writer.WriteValue( message.Name );
                            writer.WriteEndElement( );

                            writer.WriteStartElement( "Speaker" );
                            writer.WriteValue( message.Attributes["Speaker"] );
                            writer.WriteEndElement( );

                            writer.WriteStartElement( "Date" );
                            writer.WriteValue( message.Date.Value.ToShortDateString( ) );
                            writer.WriteEndElement( );
                            
                            string noteUrlValue = message.Attributes["NoteUrl"];
                            if( string.IsNullOrWhiteSpace( noteUrlValue ) == false )
                            {
                                writer.WriteStartElement( "NoteUrl" );
                                writer.WriteValue( noteUrlValue );
                                writer.WriteEndElement();
                            }

                            // Watch/Share/Audio URLs are optional, so check that they exist
                            string watchUrlValue = message.Attributes["WatchUrl"];
                            if( string.IsNullOrWhiteSpace( watchUrlValue ) == false )
                            {
                                writer.WriteStartElement( "WatchUrl" );
                                writer.WriteValue( watchUrlValue );
                                writer.WriteEndElement();
                            }
                    
                            string shareUrlValue = message.Attributes["ShareUrl"];
                            if( string.IsNullOrWhiteSpace( shareUrlValue ) == false )
                            {
                                writer.WriteStartElement( "ShareUrl" );
                                writer.WriteValue( shareUrlValue );
                                writer.WriteEndElement();
                            }
                            
                            string audioUrlValue = message.Attributes["HostedAudioUrl"];
                            if( string.IsNullOrWhiteSpace( audioUrlValue ) == false )
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

                        // if we're beyond the number of series they wanted, stop.
                        seriesAdded++;
                        if( seriesAdded > numSeriesRequested )
                        {
                            break;
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
