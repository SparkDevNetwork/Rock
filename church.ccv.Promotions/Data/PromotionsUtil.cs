using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using church.ccv.Promotions.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace church.ccv.Promotions.Data
{
    public static class PromotionsUtil
    {
        public static bool IsContentChannelMultiCampus( int contentChannelId )
        {
            RockContext rockContext = new RockContext( );
                        
            // if it has the multiCampusKey, then it supports multi-campus.
            var multiCampusAttributeValue = new AttributeService( rockContext ).Queryable( )
                .Where( a => a.EntityTypeQualifierValue == contentChannelId.ToString( ) && 
                            a.FieldType.Guid == new Guid( Rock.SystemGuid.FieldType.CAMPUSES ) ).SingleOrDefault( );

            if( multiCampusAttributeValue != null )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a promotion occurrence and associated content channel.
        /// </summary>
        /// <param name="contentChannelId"></param>
        /// <param name="contentChannelTypeId"></param>
        /// <param name="startDate"></param>
        /// <param name="approvedByPersonAliasId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="contentChannelItemCampusGuid"></param>
        /// <param name="campusId"></param>
        /// <param name="promoRequestId"></param>
        public static void CreatePromotionOccurrence( int contentChannelId, 
                                                      int contentChannelTypeId, 
                                                      DateTime startDate, 
                                                      int? approvedByPersonAliasId, 
                                                      string title, 
                                                      string content, 
                                                      string contentChannelItemCampusGuid, 
                                                      string campusGuids,
                                                      int? promoRequestId )
        {
            PromotionsContext promoContext = new PromotionsContext( );
                        
            // get the event item linked to the request
            RockContext rockContext = new RockContext( );
            
            // build the content channel item for this event
            ContentChannelItemService contentService = new ContentChannelItemService( rockContext );
            ContentChannelItem contentItem = new ContentChannelItem( );
            contentItem.ContentChannelId = contentChannelId;
            contentItem.ContentChannelTypeId = contentChannelTypeId;
            contentItem.StartDateTime = startDate;
            contentItem.ApprovedDateTime = DateTime.Now;
            contentItem.ApprovedByPersonAliasId = approvedByPersonAliasId;
            contentItem.ExpireDateTime = null;
            contentItem.Title = title;
            contentItem.Content = content;
            contentService.Add( contentItem );
            rockContext.SaveChanges( );

            // now set the campus, (if the event has one and this content channel type supports one)
            contentItem.LoadAttributes( rockContext );
            
            // this will either be the key for a single campus attribute, or the multi-campus attribute. It's
            // the caller's responsibility to figure that out.
            var attribute = contentItem.Attributes.Where( a => a.Value.FieldType.Guid == new Guid( contentChannelItemCampusGuid ) ).Select( a => a.Value ).FirstOrDefault( );
            if( attribute != null )
            {
                contentItem.SetAttributeValue( attribute.Key, campusGuids );
                
                contentItem.SaveAttributeValues( rockContext );
            }

            rockContext.SaveChanges();
            
            // finally, create the promotion occurrence item
            PromotionOccurrence promoOccurrence = new PromotionOccurrence( );
            promoOccurrence.ContentChannelItemId = contentItem.Id;
            promoOccurrence.PromotionRequestId = promoRequestId;
            promoContext.PromotionOccurrence.Add( promoOccurrence );
            
            // save changes to database
            promoContext.SaveChanges( );
        }
    }
}
