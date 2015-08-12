// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

using DotLiquid;
using System.Runtime.Caching;

namespace RockWeb.Plugins.com_centralaz.Utility
{
    /// <summary>
    /// Creates an RSS feed of the items in the requested folder.  For use with RiseVision.
    /// </summary>
    [DisplayName( "RSS Folder Files" )]
    [Category( "Utility" )]
    [Description( "Creates an RSS feed of the items in the requested folder.  For use with RiseVision." )]
    [TextField( "Base Content Folder", "The base content folder (under the ~/Content folder) where the requested folders are found.", true, @"External Site\CampusAnnouncements" )]
    [MemoField( "RSS Template", "Don't change this unless you know what you're doing.", true, @"{% assign timezone = 'Now' | Date:'zzz' | Replace:':','' -%}
<?xml version='1.0' encoding='utf-8'?>
<rss version='2.0' xmlns:atom='http://www.w3.org/2005/Atom'>

<channel>
    <title>{{ Channel.Name }}</title>
    <link>{{ Channel.ChannelUrl }}</link>
    <description>{{ Channel.Description }}</description>
    <language>en-us</language>
    <ttl>{{ Channel.TimeToLive }}</ttl>
    <lastBuildDate>{{ 'Now' | Date:'ddd, dd MMM yyyy HH:mm:00' }} {{ timezone }}</lastBuildDate>
    <generator>RockWeb.Plugins.com_centralaz.Utility.RssFolderFiles</generator>
{% for item in Items -%}
    <item>
        <title>{{ item.Title }}</title>
        <guid>{{ Channel.ItemUrl }}/{{ item.Title }}</guid>
        <link>{{ item.Permalink }}</link>
        <enclosure type='{{ item.EnclosureContentType }}' url='{{ item.Permalink }}' length='0' />
        <pubDate>{{ item.StartDateTime | Date:'ddd, dd MMM yyyy HH:mm:00' }} {{ timezone }}</pubDate>
        <description></description>
         <media:group>
            <media:content url='{{ item.EnclosureUrl }}' type='{{ item.EnclosureContentType }}' medium='image' />
            <media:credit></media:credit>
            <media:description type='plain' />
            <media:keywords />
            <media:title type='plain'></media:title>
         </media:group>
    </item>
{% endfor -%}

</channel>
</rss>", "Advanced", 0 )]
    [IntegerField( "Time To Live", "The number of minutes the content is allowed to be cached by the consumer.", true, 2 )]
    public partial class RssFolderFiles : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        public static readonly string webRootContentFolder = "~/Content";

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string requestedFolder = PageParameter( "folder" );
                string debug = PageParameter( "debug" );

                // check for trickery
                if ( requestedFolder.Contains( '.' ) )
                {
                    return;
                }

                string physicalRootFolder = Request.MapPath( webRootContentFolder );
                string baseFolder = GetAttributeValue( "BaseContentFolder" );

                string relativeFolder = Path.Combine( webRootContentFolder, baseFolder, requestedFolder );
                string physicalContentFolderPath = Request.MapPath( relativeFolder );

                ObjectCache feedCache = RockMemoryCache.Default;

                string output;
                if ( feedCache[GetFeedCacheKey( physicalContentFolderPath )] != null )
                {
                    output = (string)feedCache[GetFeedCacheKey( physicalContentFolderPath )];
                }
                else
                {
                    output = GenRSS( relativeFolder, physicalContentFolderPath, requestedFolder );
                    if ( ! string.IsNullOrEmpty ( output ) )
                    {
                        int timeToLive = GetAttributeValue( "TimeToLive" ).AsInteger();
                        feedCache.Set( GetFeedCacheKey( physicalContentFolderPath ), output, DateTimeOffset.Now.AddMinutes( timeToLive ) );
                    }
                }

                // Don't send RSS feed if the person is a page editor.
                var currentPage = Rock.Web.Cache.PageCache.Read( RockPage.PageId );
                if ( ! string.IsNullOrEmpty( debug ) || currentPage.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson ) )
                {
                    lDebug.Text = HttpUtility.HtmlEncode( output );
                    return;
                }

                Response.ContentType = "application/rss+xml";
                Response.Write( output );
                Response.Flush();
                Response.End();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ClearCachedFeed( GetPhysicalFolderPath() );
        }

        protected void lbClearCache_Click( object sender, EventArgs e )
        {
            ClearCachedFeed( GetPhysicalFolderPath() );
        }

        #endregion

        #region Methods

        public string GetPhysicalFolderPath()
        {
            string requestedFolder = PageParameter( "folder" );

            string physicalRootFolder = Request.MapPath( webRootContentFolder );
            string baseFolder = GetAttributeValue( "BaseContentFolder" );

            string relativeFolder = Path.Combine( webRootContentFolder, baseFolder, requestedFolder );
            string physicalContentFolderPath = Request.MapPath( relativeFolder );

            return physicalContentFolderPath;
        }

        public static void ClearCachedFeed( string feedUrl )
        {
            ObjectCache cache = RockMemoryCache.Default;
            string cacheKey = GetFeedCacheKey( feedUrl );

            cache.Remove( cacheKey );
        }

        private static string GetFeedCacheKey( string feedUrl )
        {
            return string.Format( "com_centralaz.Utility:RssFolderFeed:{0}", feedUrl );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private string GenRSS( string webPath, string path, string folderName )
        {
            if ( ! Directory.Exists( path ) )
            {
                return string.Empty;
            }

            int timeToLive = GetAttributeValue( "TimeToLive" ).AsInteger();
            string rssTemplate = GetAttributeValue( "RSSTemplate" );

            var files = Directory.EnumerateFiles( path, "*.*", SearchOption.AllDirectories )
            .Where( s => s.EndsWith( ".jpg" ) || s.EndsWith( ".png" ) );

            RssChannel channel = new RssChannel();
            channel.ChannelUrl = Request.Url.AbsoluteUri;
            channel.ItemUrl = this.ResolveUrl( webPath );

            channel.Name = folderName;
            channel.Description = string.Format( "Published images for {0} as an RSS feed for use with RiseVision digital signage.", folderName );
            channel.TimeToLive = timeToLive;
            var mergeFields = new Dictionary<string, object>();

            Dictionary<string, object> requestObjects = new Dictionary<string, object>();
            requestObjects.Add( "Scheme", Request.Url.Scheme );
            requestObjects.Add( "Host", Request.Url.Host );
            requestObjects.Add( "Authority", Request.Url.Authority );
            requestObjects.Add( "LocalPath", Request.Url.LocalPath );
            requestObjects.Add( "AbsoluteUri", Request.Url.AbsoluteUri );
            requestObjects.Add( "AbsolutePath", Request.Url.AbsolutePath );
            requestObjects.Add( "Port", Request.Url.Port );
            requestObjects.Add( "Query", Request.Url.Query );
            requestObjects.Add( "OriginalString", Request.Url.OriginalString );

            mergeFields.Add( "Request", requestObjects );
            mergeFields.Add( "Channel", channel );

            List<RssChannelItem> items = new List<RssChannelItem>();
            foreach ( var file in files )
            {
                var fileName = Path.GetFileName( file );
                RssChannelItem item = new RssChannelItem();
                item.Title = fileName;
                item.StartDateTime = File.GetCreationTime( file );
                item.EnclosureUrl = Request.Url.GetLeftPart( UriPartial.Authority ) + 
                    this.ResolveUrl( webPath ) + "/" + fileName;
                item.Permalink = item.EnclosureUrl;
                switch ( Path.GetExtension( file ).ToLowerInvariant() )
                {
                    case ".png":
                        item.EnclosureContentType = "image/png";
                        break;
                    case ".jpg":
                    default:
                        item.EnclosureContentType = "image/jpeg";
                        break;
                }
                channel.RssItems.Add( item );
            }

            mergeFields.Add( "Items", channel.RssItems );
            return rssTemplate.ResolveMergeFields( mergeFields );
        }

        #endregion
}

    public class RssChannel : ContentChannel
    {
        public ICollection<RssChannelItem> RssItems { get; set; }
        public RssChannel()
        {
            RssItems = new List<RssChannelItem>();
        }
    }

    public class RssChannelItem : ContentChannelItem
    {
        [DataMember]
        public string EnclosureUrl { get; set; }
        [DataMember]
        public string EnclosureLength { get; set; }
        [DataMember]
        public string EnclosureContentType { get; set; }

        public RssChannelItem() { }
    }

}