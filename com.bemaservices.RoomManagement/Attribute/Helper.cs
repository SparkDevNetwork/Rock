// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using com.bemaservices.RoomManagement.Model;

using Rock;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Attribute
{
    /// <summary>
    /// Static Helper class for the Room Management plugin
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Loads the reservation location attributes.
        /// </summary>
        /// <param name="reservationLocation">The reservation location.</param>
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

        /// <summary>
        /// Loads the reservation resource attributes.
        /// </summary>
        /// <param name="reservationResource">The reservation resource.</param>
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
