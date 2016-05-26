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
        public const int WeekendVideos_CategoryId = 452;

        public static PodcastCategory GetPodcastsByCategory( int categoryId, bool keepCategoryHierarchy = true )
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
                                                                                       cc => cc.Id, cav => cav.EntityId, ( cc, cav ) => new ContentChannelWithAttribs { ContentChannel = cc, CategoryAttribValue = cav } );
            
            // create our root podcast object
            PodcastCategory rootPodcast = new PodcastCategory( category.Name, category.Id );
                        
            // see if this category has any podcasts to add.
            Internal_GetPodcastsByCategory( category, rootPodcast, categoryContentChannelItems, keepCategoryHierarchy );
    
            return rootPodcast;
        }

        static void Internal_GetPodcastsByCategory( Category category, PodcastCategory rootPodcast, IQueryable<ContentChannelWithAttribs> categoryContentChannelItems, bool keepCategoryHierarchy = true )
        {
            // Get all Content Channels that are immediate children of the provided category.
            var podcastsForCategory = categoryContentChannelItems.Where( cci => cci.CategoryAttribValue.Value.Contains( category.Guid.ToString( ) ) ).Select( cci => cci.ContentChannel );

            // Convert all the content channel items into PodcastSeries and add them as children.
            foreach( ContentChannel contentChannel in podcastsForCategory )
            {
                PodcastSeries podcastSeries = ContentChannelToPodcastSeries( contentChannel );

                rootPodcast.Children.Add( podcastSeries );
            }
            
            // now sort all series based on their first message's date.
            // (We're safe to do this because we KNOW we've only added PodcastSeries at this point)
            rootPodcast.Children.Sort( delegate( IPodcastNode a, IPodcastNode b )
            {
                if( a.Date > b.Date )
                {
                    return -1;
                }
                return 1;
            });

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

                // now recursively call this so it can add its children
                Internal_GetPodcastsByCategory( childCategory, podcastCategory, categoryContentChannelItems, keepCategoryHierarchy );
            }
        }

        static PodcastSeries ContentChannelToPodcastSeries( ContentChannel contentChannel )
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
                
            series.Messages = new List<PodcastMessage>( );
            foreach( ContentChannelItem contentChannelItem in contentChannel.Items )
            {
                // convert each contentChannelItem into a PodcastMessage, and add it to our list
                PodcastMessage message = ContentChannelItemToPodcastMessage( contentChannelItem );
                series.Messages.Add( message );
            }

            // if there are messages, sort them by date
            if( series.Messages.Count > 0 )
            {
                // sort the messages by date
                series.Messages.Sort( delegate( PodcastMessage a, PodcastMessage b )
                {
                    if( a.Date > b.Date )
                    {
                        return -1;
                    }
                    return 1;
                });

                // then set the series' date to the first message's (earliest)
                series.Date = series.Messages[0].Date;
            }
            else
            {
                series.Date = null;
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

        public static PodcastCategory PodcastsAsModel( int podcastCategory )
        {
            return GetPodcastsByCategory( podcastCategory );
        }

        public static PodcastSeries GetSeries( int seriesId )
        {
            // try to get the content channel that represents this series
            RockContext rockContext = new RockContext( );
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            ContentChannel seriesContentChannel = contentChannelService.Queryable( ).Where( cc => cc.Id == seriesId ).SingleOrDefault( );
            if( seriesContentChannel != null )
            {
                // convert it to a PodcastSeries and return it
                PodcastSeries series = ContentChannelToPodcastSeries( seriesContentChannel );
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
        
        // Helper class for storing a Content Channel with its Category Attribute Value
        public class ContentChannelWithAttribs
        {
            public ContentChannel ContentChannel { get; set; }
            public AttributeValue CategoryAttribValue { get; set; }
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
