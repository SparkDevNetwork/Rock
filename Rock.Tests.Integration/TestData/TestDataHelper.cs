using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData
{
    public static class TestDataHelper
    {
        private static List<PersonIdPersonAliasId> _PersonIdToAliasIdMap = null;

        /// <summary>
        /// Gets a list of the Id and AliasId of all Person records in the database
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public static List<PersonIdPersonAliasId> GetPersonIdWithAliasIdList()
        {
            if ( _PersonIdToAliasIdMap == null )
            {
                var aliasService = new PersonAliasService( new RockContext() );

                _PersonIdToAliasIdMap = aliasService.Queryable()
                    .Where( x => !x.Person.IsSystem )
                    .GroupBy( x => x.PersonId )
                    .Select( a => new PersonIdPersonAliasId
                    {
                        PersonId = a.Key,
                        PersonAliasId = a.FirstOrDefault().Id
                    } )
                    .ToList();
            }

            return _PersonIdToAliasIdMap;
        }

        /// <summary>
        /// A hardcoded Device Id of 2 (Main Campus Checkin)
        /// </summary>
        public const int KioskDeviceId = 2;

        public class PersonIdPersonAliasId
        {
            public int PersonId { get; set; }
            public int PersonAliasId { get; set; }
        }

        private static Random randomGenerator = new Random();
        public static DateTime GetRandomDateInRange( DateTime minDate, DateTime maxDate )
        {
            var range = ( maxDate - minDate ).Days;

            return minDate.AddDays( randomGenerator.Next( range ) );
        }

        public static Dictionary<int, DateTime> GetAnalyticsSourceDateTestData()
        {
            return new Dictionary<int, DateTime>
            {
                {20100131,  Convert.ToDateTime("1/31/2010")},
                {20101231,  Convert.ToDateTime("12/31/2010")},
                {20101201,  Convert.ToDateTime("12/1/2010")},
                {20100101,  Convert.ToDateTime("1/1/2010")},
                {20160229,  Convert.ToDateTime("02/29/2016")},
            };
        }

        public static DateTime GetAnalyticsSourceMinDateForYear( RockContext rockContext, int year )
        {
            return rockContext.Database.SqlQuery<DateTime>( $"SELECT MIN([Date]) FROM AnalyticsSourceDate WHERE CalendarYear = {year}" ).First();
        }

        public static DateTime GetAnalyticsSourceMaxDateForYear( RockContext rockContext, int year )
        {
            return rockContext.Database.SqlQuery<DateTime>( $"SELECT MAX([Date]) FROM AnalyticsSourceDate WHERE CalendarYear = {year}" ).First();
        }

        #region Attribute Helpers
        
        /// <summary>
        /// Sets the value of an existing <see cref="AttributeValue"/> and saves it to the database or creates a new database record if one doesn't already exist.
        /// </summary>
        /// <param name="attributeGuid">The parent <see cref="Rock.Model.Attribute"/> unique identifier.</param>
        /// <param name="entityId">The ID of the entity - if any - to which this <see cref="AttributeValue"/> belongs.</param>
        /// <param name="value">The value to be set.</param>
        /// <param name="previousValue">If a <see cref="AttributeValue"/> already exists in the database, it's current value will be returned, so you can set it back after the current tests complete.</param>
        /// <param name="newAttributeValueGuid">If a <see cref="AttributeValue"/> doesn't already exist in the database, the <see cref="Guid"/> of the newly-created record will be returned, so you can delete it after the current tests complete.</param>
        public static void SetAttributeValue( Guid attributeGuid, int? entityId, string value, out string previousValue, out Guid newAttributeValueGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                previousValue = null;
                newAttributeValueGuid = Guid.Empty;

                var attributeId = AttributeCache.GetId( attributeGuid );
                if ( !attributeId.HasValue )
                {
                    return;
                }

                var attributeValueService = new AttributeValueService( rockContext );

                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId.Value, entityId );

                if ( attributeValue == null )
                {
                    attributeValue = new AttributeValue
                    {
                        AttributeId = attributeId.Value,
                        EntityId = entityId,
                        Value = value
                    };

                    attributeValueService.Add( attributeValue );

                    // Remember this so we can delete this AttributeValue upon cleanup.
                    newAttributeValueGuid = attributeValue.Guid;
                }
                else
                {
                    // Remeber this so we can set it back upon cleanup.
                    previousValue = attributeValue.Value;
                    attributeValue.Value = value;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the attribute value.
        /// </summary>
        /// <param name="attributeValueGuid">The attribute value unique identifier.</param>
        public static void DeleteAttributeValue( Guid attributeValueGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.Get( attributeValueGuid );
                if ( attributeValue == null )
                {
                    return;
                }

                attributeValueService.Delete( attributeValue );
                rockContext.SaveChanges();
            }
        }

        #endregion
    }
}
