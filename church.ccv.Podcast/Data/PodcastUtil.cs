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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;

namespace church.ccv.Podcast
{
    public static class PodcastUtil
    {
        // This is rock's core podcast category and will never change.
        const int RootPodcast_CategoryId = 451;

        // This is Rock's Weekend Videos podcast category, and will also never change.
        // This is defined for external systems (like the block that displays our Weekend Series)
        public const int WeekendVideos_CategoryId = 452;

        const int ContentChannelTypeId_PodcastSeries = 8;
        
        public static PodcastCategory GetPodcastsByCategory( int categoryId, bool keepCategoryHierarchy = true, int maxSeries = int.MaxValue, bool expandSeries = true, bool includeEmptySeries = false )
        {
            // if they pass in 0, accept that as the Root
            if ( categoryId == 0 )
            {
                categoryId = RootPodcast_CategoryId;
            }

            RockContext rockContext = new RockContext( );

            // get the root category that's parent to all categories and podcasts they care about
            var category = new CategoryService( rockContext ).Queryable( ).Where( c => c.Id == categoryId ).SingleOrDefault( );
            
            // create a query that'll get all of the "Category" attributes for all the content channels
            var categoryAttribValList = new AttributeValueService( rockContext ).Queryable( ).Where( av => av.Attribute.Guid == new Guid( church.ccv.Utility.SystemGuids.Attribute.CONTENT_CHANNEL_CATEGORY_ATTRIBUTE  ) );

            // now get ALL content channels with their parent category(s) attributes as a joined object
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );
            var categoryContentChannelItems = contentChannelService.Queryable( ).Join( categoryAttribValList, 
                                                                                       cc => cc.Id, cav => cav.EntityId, ( cc, cav ) => new ContentChannelWithAttrib { ContentChannel = cc, AttribValue = cav } );
            
            // create our root podcast object
            PodcastCategory rootPodcast = new PodcastCategory( category.Name, category.Id );
                        
            // see if this category has any podcasts to add.
            Internal_GetPodcastsByCategory( category, rootPodcast, categoryContentChannelItems, maxSeries, keepCategoryHierarchy, expandSeries, includeEmptySeries );
    
            return rootPodcast;
        }

        static int Internal_GetPodcastsByCategory( Category category, PodcastCategory rootPodcast, IQueryable<ContentChannelWithAttrib> categoryContentChannelItems, int numSeriesToAdd, bool keepCategoryHierarchy, bool expandSeries, bool includeEmptySeries )
        {
            // Get all Content Channels that are immediate children of the provided category. Sort them by date, and then take 'numPodcastsToAdd', since 
            // this is recursive and we might not need anymore.
            var podcastsForCategory = categoryContentChannelItems.Where( cci => cci.AttribValue.Value.Contains( category.Guid.ToString( ) ) )
                                                                 .Select( cci => cci.ContentChannel )
                                                                 .OrderByDescending( cc => cc.CreatedDateTime )
                                                                 .Take( numSeriesToAdd );
            
            // Convert all the content channel items into PodcastSeries and add them as children.
            // (We're safe to do this because we KNOW we've only added PodcastSeries at this point)
            foreach( ContentChannel contentChannel in podcastsForCategory )
            {
                // if there are messages, or they want empty series, include it
                if( contentChannel.Items.Count( ) > 0 || includeEmptySeries == true )
                {
                    PodcastSeries podcastSeries = ContentChannelToPodcastSeries( contentChannel, expandSeries );
                    rootPodcast.Children.Add( podcastSeries );
                }
            }

            // store the number of podcasts added
            int seriesAdded = podcastsForCategory.Count( );
            
            // now recursively handle all child categories
            foreach ( Category childCategory in category.ChildCategories )
            {
                PodcastCategory podcastCategory = null;

                // if true, we'll maintain the category hierarchy.
                if ( keepCategoryHierarchy )
                {
                    // so create a new child category object
                    PodcastCategory childPodcastCategory = new PodcastCategory( childCategory.Name, childCategory.Id );

                    // add it to this root podcast
                    rootPodcast.Children.Add( childPodcastCategory );

                    // and set it as the next category to use
                    podcastCategory = childPodcastCategory;
                }
                else
                {
                    // false, so we should use the initial root category, so that all series / messages go into this category's
                    // Children list as a big flat list
                    podcastCategory = rootPodcast;
                }


                // if there are more podcasts to add
                if( seriesAdded < numSeriesToAdd )
                {
                    // recursively call this so it can add its children
                    seriesAdded += Internal_GetPodcastsByCategory( childCategory, podcastCategory, categoryContentChannelItems, numSeriesToAdd - seriesAdded, keepCategoryHierarchy, expandSeries, includeEmptySeries );
                }
            }

            return seriesAdded;
        }

        public static DateTime? LatestModifiedDateTime( )
        {
            //JHM 6-21-2016: cc.ModifiedDateTime< DateTime.Now is a hack to fix an issue when porting from Arena. We used the Series StartDateTime as the Created/Modified DateTime, which
            // causes a ModifiedDateTime in the future. This can go away after 7-3-2016.

            // this will gather the series, messages, and all attribute values, and see what the date/time of the most recent change was
            RockContext rockContext = new RockContext( );

            DateTime? latestDate = null;

            // get all podcast series sorted by ModifiedDateTime
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );
            var podcastSeries = contentChannelService.Queryable( ).Where( cc => cc.ContentChannelTypeId == ContentChannelTypeId_PodcastSeries && cc.ModifiedDateTime< DateTime.Now ).OrderByDescending( cc => cc.ModifiedDateTime );
            if( podcastSeries.Count( ) > 0 )
            {
                var seriesIds = podcastSeries.Select( ps => ps.Id );

                // take the latest modified date/time and assume that's the latest change
                latestDate = podcastSeries.FirstOrDefault( ).ModifiedDateTime;

                // get all messages for all the podcast series, sorted by ModifiedDateTime
                ContentChannelItemService contentChannelItemService = new ContentChannelItemService( rockContext );
                var podcastMessages = contentChannelItemService.Queryable( ).Where( cci => seriesIds.Contains( cci.ContentChannelId ) && cci.ModifiedDateTime< DateTime.Now ).OrderByDescending( cci => cci.ModifiedDateTime );
                if ( podcastMessages.Count() > 0 )
                {
                    var messageIds = podcastMessages.Select( pm => pm.Id );

                    // if there's a more recent change in the messages, take that date/time
                    if ( latestDate < podcastMessages.FirstOrDefault().ModifiedDateTime )
                    {
                        latestDate = podcastMessages.FirstOrDefault().ModifiedDateTime;
                    }

                    // NOW, get all attrib values for messages AND series, sorted by date
                    var attribValList = new AttributeValueService( rockContext ).Queryable().Where( av => av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" && av.ModifiedDateTime < DateTime.Now &&
                                                                                                   ( seriesIds.Contains( av.EntityId.Value ) || messageIds.Contains( av.EntityId.Value ) ) ).OrderByDescending( av => av.ModifiedDateTime );

                    if ( attribValList.Count() > 0 )
                    {
                        // and see if there's anything newer
                        if ( latestDate < attribValList.FirstOrDefault().ModifiedDateTime )
                        {
                            latestDate = attribValList.FirstOrDefault().ModifiedDateTime;
                        }
                    }
                }
            }

            // now return the latestDate, which is the most recent change to ANYTHING related to the podcast system
            return latestDate;
        }

        static PodcastSeries ContentChannelToPodcastSeries( ContentChannel contentChannel, bool expandSeries )
        {
            // Given a content channel in the database, this will convert it into our Podcast model.

            RockContext rockContext = new RockContext( );
            IQueryable<AttributeValue> attribValQuery = new AttributeValueService( rockContext ).Queryable( );

            // get the list of attributes for this content channel
            List<AttributeValue> contentChannelAttribValList = attribValQuery.Where( av => av.EntityId == contentChannel.Id && 
                                                                                           av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" )
                                                                             .ToList( );

            // setup the series
            PodcastSeries series = new PodcastSeries( );
            series.Id = contentChannel.Id;
            series.Name = contentChannel.Name;
            series.Description = contentChannel.Description;
                
            // add all the attributes
            series.Attributes = new Dictionary<string, string>( );
            foreach( AttributeValue attribValue in contentChannelAttribValList )
            {
                series.Attributes.Add( attribValue.AttributeKey, attribValue.Value );
            }

            // use the created dateTime as the series' date.
            series.Date = contentChannel.CreatedDateTime;
    
            // only add messages if 'expandSeries' is set to true. This allows requesters to opt-out and
            // only get a list of series, which is faster.
            series.Messages = new List<PodcastMessage>( );
            if ( expandSeries )
            {
                // sort the messages by date
                var orderedContentChannelItems = contentChannel.Items.OrderByDescending( i => i.CreatedDateTime );

                foreach( ContentChannelItem contentChannelItem in orderedContentChannelItems )
                {
                    // convert each contentChannelItem into a PodcastMessage, and add it to our list
                    PodcastMessage message = ContentChannelItemToPodcastMessage( contentChannelItem );
                    series.Messages.Add( message );
                }
            }

            return series;
        }

        static PodcastMessage ContentChannelItemToPodcastMessage( ContentChannelItem contentChannelItem )
        {
            // Given a content channel item, convert it into a PodcastMessage and return it

            RockContext rockContext = new RockContext( );
            IQueryable<AttributeValue> attribValQuery = new AttributeValueService( rockContext ).Queryable( );

            // get this message's attributes
            List<AttributeValue> itemAttribValList = attribValQuery.Where( av => av.EntityId == contentChannelItem.Id && 
                                                                                 av.Attribute.EntityTypeQualifierColumn == "ContentChannelTypeId" )
                                                                    .ToList( );

            PodcastMessage message = new PodcastMessage( );
            message.Id = contentChannelItem.Id;
            message.Name = contentChannelItem.Title;
            message.Description = contentChannelItem.Content;
            message.Date = contentChannelItem.StartDateTime;
            message.Approved = contentChannelItem.Status == ContentChannelItemStatus.Approved ? true : false;

            // add all the attributes
            message.Attributes = new Dictionary<string, string>( );
            foreach( AttributeValue attribValue in itemAttribValList )
            {
                message.Attributes.Add( attribValue.AttributeKey, attribValue.Value );
            }

            return message;
        }

        public static PodcastCategory PodcastsAsModel( int podcastCategory, bool keepHierarchy = false, int numSeries = int.MaxValue )
        {
            return GetPodcastsByCategory( podcastCategory, keepHierarchy, numSeries );
        }

        public static PodcastSeries GetSeries( int seriesId, bool expandSeries )
        {
            // try to get the content channel that represents this series
            RockContext rockContext = new RockContext( );
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            ContentChannel seriesContentChannel = contentChannelService.Queryable( ).Where( cc => cc.Id == seriesId ).SingleOrDefault( );
            if( seriesContentChannel != null )
            {
                // convert it to a PodcastSeries and return it
                PodcastSeries series = ContentChannelToPodcastSeries( seriesContentChannel, expandSeries );
                return series;
            }
            
            // couldn't find it? return null
            return null;
        }

        public static PodcastMessage GetMessage( int messageId )
        {
            // try to get the content channel item that represents this message
            RockContext rockContext = new RockContext( );
            ContentChannelItemService contentChannelItemService = new ContentChannelItemService( rockContext );

            ContentChannelItem messageContentChannelItem = contentChannelItemService.Queryable( ).Where( cc => cc.Id == messageId ).SingleOrDefault( );
            if( messageContentChannelItem != null )
            {
                // convert it to a PodcastMessage and return it
                PodcastMessage message = ContentChannelItemToPodcastMessage( messageContentChannelItem );
                return message;
            }
            
            // couldn't find it? return null
            return null;
        }
        
        // Helper class for storing a Content Channel with an associated Attribute Value
        public class ContentChannelWithAttrib
        {
            public ContentChannel ContentChannel { get; set; }
            public AttributeValue AttribValue { get; set; }
        }

        // Interface so that PodcastCategories can have either Series or Categories as children.
        public interface IPodcastNode : Rock.Lava.ILiquidizable
        {
            DateTime? Date { get; set; }
        }

        public class PodcastCategory : IPodcastNode
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<IPodcastNode> Children { get; set; }
            public DateTime? Date { get; set; }


            public PodcastCategory( string name, int id )
            {
                Name = name;
                Id = id;
                Children = new List<IPodcastNode>( );
            }



            // Liquid Methods
            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public List<string> AvailableKeys
            {
                get
                {
                    var availableKeys = new List<string> { "Id", "Name", "Children" };
                                            
                    foreach ( IPodcastNode child in Children )
                    {
                        availableKeys.AddRange( child.AvailableKeys );
                    }

                    return availableKeys;
                }
            }
            
            public object ToLiquid()
            {
                return this;
            }

            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public object this[object key]
            {
               get
                {
                   switch( key.ToStringSafe() )
                   {
                       case "Id": return Id;
                       case "Name": return Name;
                       case "Children": return Children;
                   }

                    return null;
                }
            }
            
            public bool ContainsKey( object key )
            {
                var additionalKeys = new List<string> { "Id", "Name", "Children" };
                if ( additionalKeys.Contains( key.ToStringSafe() ) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            //
        }

        public class PodcastSeries : IPodcastNode
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime? Date { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
            public List<PodcastMessage> Messages { get; set; }

            // Liquid Methods
            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public List<string> AvailableKeys
            {
                get
                {
                    var availableKeys = new List<string> { "Id", "Name", "Description", "Date", "Attributes", "Messages" };

                    foreach ( IPodcastNode child in Messages )
                    {
                        availableKeys.AddRange( child.AvailableKeys );
                    }

                    return availableKeys;
                }
            }
            
            public object ToLiquid()
            {
                return this;
            }

            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public object this[object key]
            {
               get
                {
                   switch( key.ToStringSafe() )
                   {
                       case "Id": return Id;
                       case "Name": return Name;
                       case "Description": return Description;
                       case "Date": return Date;
                       case "Attributes": return Attributes;
                       case "Messages": return Messages;
                   }

                    return null;
                }
            }
            
            public bool ContainsKey( object key )
            {
                var additionalKeys = new List<string> { "Id", "Name", "Description", "Date", "Attributes", "Messages" };
                if ( additionalKeys.Contains( key.ToStringSafe() ) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            //
        }

        public class PodcastMessage : IPodcastNode
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime? Date { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
            public bool Approved { get; set; }

            // Liquid Methods
            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public List<string> AvailableKeys
            {
                get
                {
                    var availableKeys = new List<string> { "Id", "Name", "Description", "Date", "Attributes", "Approved" };
                    
                    return availableKeys;
                }
            }
            
            public object ToLiquid()
            {
                return this;
            }

            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public object this[object key]
            {
               get
                {
                   switch( key.ToStringSafe() )
                   {
                       case "Id": return Id;
                       case "Name": return Name;
                       case "Description": return Description;
                       case "Date": return Date;
                       case "Attributes": return Attributes;
                       case "Approved": return Approved;
                   }

                    return null;
                }
            }
            
            public bool ContainsKey( object key )
            {
                var additionalKeys = new List<string> { "Id", "Name", "Description", "Date", "Attributes", "Approved" };
                if ( additionalKeys.Contains( key.ToStringSafe() ) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            //
        }
    }
}
