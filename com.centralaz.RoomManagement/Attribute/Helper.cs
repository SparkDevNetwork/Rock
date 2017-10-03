using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using com.centralaz.RoomManagement.Model;

using Rock;
using Rock.Data;

namespace com.centralaz.RoomManagement.Attribute
{
    /// <summary>
    /// Static Helper class for the Room Management plugin
    /// </summary>
    public static class Helper
    {
        public static void LoadReservationLocationAttributes( this ReservationLocation reservationLocation )
        {
            var rockContext = new RockContext();
            var questionService = new QuestionService( rockContext );
            var questionAttributeIds = questionService.Queryable().Where( q => q.LocationId == reservationLocation.LocationId ).Select( q => q.AttributeId ).ToList();

            var starterReservationLocation = new ReservationLocation();
            starterReservationLocation.CopyPropertiesFrom( reservationLocation );

            Rock.Attribute.Helper.LoadAttributes( starterReservationLocation );

            var attributeList = starterReservationLocation.Attributes.Where( kvp => questionAttributeIds.Contains( kvp.Value.Id ) ).ToList();
            reservationLocation.Attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();
            attributeList.ForEach( a => reservationLocation.Attributes.AddOrIgnore( a.Key, a.Value ) );

            var attributeValueList = starterReservationLocation.AttributeValues.Where( kvp => questionAttributeIds.Contains( kvp.Value.AttributeId ) ).ToList();
            reservationLocation.AttributeValues = new Dictionary<string, Rock.Web.Cache.AttributeValueCache>();
            attributeValueList.ForEach( a => reservationLocation.AttributeValues.AddOrIgnore( a.Key, a.Value ) );
        }

        public static void LoadReservationResourceAttributes( this ReservationResource reservationResource )
        {
            var rockContext = new RockContext();
            var questionService = new QuestionService( rockContext );
            var questionAttributeIds = questionService.Queryable().Where( q => q.ResourceId == reservationResource.ResourceId ).Select( q => q.AttributeId ).ToList();

            var starterReservationResource = new ReservationResource();
            starterReservationResource.CopyPropertiesFrom( reservationResource );

            Rock.Attribute.Helper.LoadAttributes( starterReservationResource );

            var attributeList = starterReservationResource.Attributes.Where( kvp => questionAttributeIds.Contains( kvp.Value.Id ) ).ToList();
            reservationResource.Attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();
            attributeList.ForEach( a => reservationResource.Attributes.AddOrIgnore( a.Key, a.Value ) );

            var attributeValueList = starterReservationResource.AttributeValues.Where( kvp => questionAttributeIds.Contains( kvp.Value.AttributeId ) ).ToList();
            reservationResource.AttributeValues = new Dictionary<string, Rock.Web.Cache.AttributeValueCache>();
            attributeValueList.ForEach( a => reservationResource.AttributeValues.AddOrIgnore( a.Key, a.Value ) );
        }
    }

}
