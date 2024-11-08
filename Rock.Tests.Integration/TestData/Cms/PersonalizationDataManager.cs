// <copyright>
// Copyright by the Spark Development Network
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
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Personalization;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Crm.Personalization
{
    /// <summary>
    /// Provides actions to manage data for the Personalization features of Rock.
    /// </summary>
    public class PersonalizationDataManager
    {
        private static Lazy<PersonalizationDataManager> _dataManager = new Lazy<PersonalizationDataManager>();
        public static PersonalizationDataManager Instance => _dataManager.Value;

        #region Test Data

        public static class Constants
        {
            public const string SegmentAllMenGuid = "A8B006AF-1531-42B5-AD9A-570F97C8EFC1";
            public const string SegmentAllWomenGuid = "30D30C13-27B9-46FF-8F5B-2EAAB1E69855";
            public const string SegmentAttenderGuid = "9F43E363-8BC1-43C2-A22E-32C92901E52C";
            public const string SegmentMarriedGuid = "BE44836F-97BC-4B20-886F-2C4E921A968B";
            public const string SegmentSingleGuid = "307DB9FD-951A-4D80-8BB0-1398A365CA87";
            public const string SegmentSmallGroupGuid = "F189099D-B1B7-4067-BD28-D354110AAB1D";
            public const string SegmentHasGivenGuid = "354C8281-C4B6-44E2-8BB3-4E5E58A656A5";
            public const string SegmentTestInactiveGuid = "E5DBC839-1E16-4430-81A6-9EDEF9422B4F";

            public const string SegmentKeyMarried = "MARRIED";
            public const string SegmentKeySingle = "SINGLE";

            public const string FilterMobileDeviceGuid = "F4F31B2A-F525-4E52-8A6E-ECA1E1EBD75B";
            public const string FilterQueryParameterInactiveGuid = "45AA8BAF-78F2-4220-93C7-0ADC7CB8CADD";
            public const string FilterQueryParameter1Guid = "674BD50E-DA57-495F-B2EC-B16BC34BE2FA";
            public const string FilterQueryParameter2Guid = "806079BE-768E-4707-9C86-47A73DB7A1C7";
            public const string FilterDesktopDeviceGuid = "58DEE912-6EF2-43C8-BE89-452A830BB7EF";
        }

        /// <summary>
        /// Add test data for Personalization features.
        /// </summary>
        public void AddDataForTestPersonalization()
        {
            LogHelper.Log( $"Personalization Data: adding sample data..." );

            ConfigurePersonalizationSegments();
            ConfigurePersonalizationRequestFilters();

            ConfigurePersonalizationForSite();
            ConfigurePersonalizationForPeople();
            ConfigurePersonalizationForContentChannels();

            LogHelper.Log( $"Personalization Data: sample data added." );
        }

        private void ConfigurePersonalizationSegments()
        {
            var rockContext = new RockContext();
            var personalizationService = new PersonalizationSegmentService( rockContext );

            // Segment: All Men
            var segmentAllMen = personalizationService.Get( Constants.SegmentAllMenGuid.AsGuid() );
            if ( segmentAllMen == null )
            {
                segmentAllMen = new PersonalizationSegment();
                personalizationService.Add( segmentAllMen );
            }

            segmentAllMen.SegmentKey = "ALL_MEN";
            segmentAllMen.Name = "All Men";
            segmentAllMen.Guid = Constants.SegmentAllMenGuid.AsGuid();

            // Segment: All Women
            var segmentAllWomen = personalizationService.Get( Constants.SegmentAllWomenGuid.AsGuid() );
            if ( segmentAllWomen == null )
            {
                segmentAllWomen = new PersonalizationSegment();
                personalizationService.Add( segmentAllWomen );
            }

            segmentAllWomen.SegmentKey = "ALL_WOMEN";
            segmentAllWomen.Name = "All Women";
            segmentAllWomen.Guid = Constants.SegmentAllWomenGuid.AsGuid();

            // Segment: Married
            var segmentMarried = personalizationService.Get( Constants.SegmentMarriedGuid.AsGuid() );
            if ( segmentMarried == null )
            {
                segmentMarried = new PersonalizationSegment();
                personalizationService.Add( segmentMarried );
            }

            segmentMarried.SegmentKey = Constants.SegmentKeyMarried;
            segmentMarried.Name = "Married";
            segmentMarried.Guid = Constants.SegmentMarriedGuid.AsGuid();

            // Segment: Single
            var segmentSingle = personalizationService.Get( Constants.SegmentSingleGuid.AsGuid() );
            if ( segmentSingle == null )
            {
                segmentSingle = new PersonalizationSegment();
                personalizationService.Add( segmentSingle );
            }

            segmentSingle.SegmentKey = Constants.SegmentKeySingle;
            segmentSingle.Name = "Single";
            segmentSingle.Guid = Constants.SegmentSingleGuid.AsGuid();

            // Segment: Attender
            var segmentAttender = personalizationService.Get( Constants.SegmentAttenderGuid.AsGuid() );
            if ( segmentAttender == null )
            {
                segmentAttender = new PersonalizationSegment();
                personalizationService.Add( segmentAttender );
            }

            segmentAttender.SegmentKey = "ATTENDER";
            segmentAttender.Name = "Attender";
            segmentAttender.Guid = Constants.SegmentAttenderGuid.AsGuid();

            // Segment: In Small Group
            var segmentSmallGroup = personalizationService.Get( Constants.SegmentSmallGroupGuid.AsGuid() );
            if ( segmentSmallGroup == null )
            {
                segmentSmallGroup = new PersonalizationSegment();
                personalizationService.Add( segmentSmallGroup );
            }

            segmentSmallGroup.SegmentKey = "IN_SMALL_GROUP";
            segmentSmallGroup.Name = "Small Group";
            segmentSmallGroup.Guid = Constants.SegmentSmallGroupGuid.AsGuid();
            rockContext.SaveChanges();

            // Segment: Has Given
            var segmentHasGiven = personalizationService.Get( Constants.SegmentHasGivenGuid.AsGuid() );
            if ( segmentHasGiven == null )
            {
                segmentHasGiven = new PersonalizationSegment();
                personalizationService.Add( segmentHasGiven );
            }

            segmentHasGiven.SegmentKey = "HAS_GIVEN";
            segmentHasGiven.Name = "Has Given";
            segmentHasGiven.Guid = Constants.SegmentHasGivenGuid.AsGuid();
            rockContext.SaveChanges();

            // Segment: Inactive
            var segmentTestInactive = personalizationService.Get( Constants.SegmentTestInactiveGuid.AsGuid() );
            if ( segmentTestInactive == null )
            {
                segmentTestInactive = new PersonalizationSegment();
                personalizationService.Add( segmentTestInactive );
            }

            segmentTestInactive.SegmentKey = "SEGMENT_INACTIVE";
            segmentTestInactive.Name = "Inactive Segment";
            segmentTestInactive.Guid = Constants.SegmentTestInactiveGuid.AsGuid();
            segmentTestInactive.IsActive = false;
            rockContext.SaveChanges();
        }

        private void ConfigurePersonalizationRequestFilters()
        {
            var rockContext = new RockContext();
            var filterService = new RequestFilterService( rockContext );

            // Request Filter: Desktop Device.
            var filterDesktop = filterService.Get( Constants.FilterDesktopDeviceGuid.AsGuid() );
            if ( filterDesktop == null )
            {
                filterDesktop = new RequestFilter();
                filterService.Add( filterDesktop );
            }

            filterDesktop.RequestFilterKey = "DESKTOP";
            filterDesktop.Name = "Desktop Device";
            filterDesktop.Guid = Constants.FilterDesktopDeviceGuid.AsGuid();

            // Request Filter: Mobile Device.
            var filterMobile = filterService.Get( Constants.FilterMobileDeviceGuid.AsGuid() );
            if ( filterMobile == null )
            {
                filterMobile = new RequestFilter();
                filterService.Add( filterMobile );
            }

            filterMobile.RequestFilterKey = "MOBILE";
            filterMobile.Name = "Mobile Device";
            filterMobile.Guid = Constants.FilterMobileDeviceGuid.AsGuid();

            rockContext.SaveChanges();

            // Request Filter: QueryStringParameter1.
            AddOrUpdateRequestFilterForQueryString( rockContext, Constants.FilterQueryParameter1Guid, "QUERY_1", "parameter1", "true" );

            // Request Filter: QueryStringParameter2.
            AddOrUpdateRequestFilterForQueryString( rockContext, Constants.FilterQueryParameter2Guid, "QUERY_2", "parameter2", "true" );

            // Request Filter: QueryStringParameter0, Inactive.
            var inactiveFilter = AddOrUpdateRequestFilterForQueryString( rockContext, Constants.FilterQueryParameterInactiveGuid, "REQUEST_INACTIVE", "inactive", " true" );
            inactiveFilter.IsActive = false;

            rockContext.SaveChanges();
        }

        private void ConfigurePersonalizationForContentChannels()
        {
            SetContentChannelPersonalizationEnabled( "Messages", true );

            // Add Personalization Segments to items in Content Channel "Messages".
            SetContentChannelItemPersonalizationSegments( "Of Myths and Money", new List<string> { Constants.SegmentAttenderGuid, Constants.SegmentHasGivenGuid } );
            SetContentChannelItemPersonalizationSegments( "Of Faith and Firsts", new List<string> { Constants.SegmentAttenderGuid, Constants.SegmentHasGivenGuid } );

            SetContentChannelItemPersonalizationSegments( "Are You Dealing With Insecurity?", new List<string> { Constants.SegmentSingleGuid } );
            SetContentChannelItemPersonalizationSegments( "Hallelujah!", new List<string> { Constants.SegmentSingleGuid } );

            SetContentChannelItemPersonalizationSegments( "The Secret That Could Cost You Your Marriage", new List<string> { Constants.SegmentMarriedGuid } );
            SetContentChannelItemPersonalizationSegments( "How To Make Your Marriage Better Today", new List<string> { Constants.SegmentMarriedGuid } );

            SetContentChannelItemPersonalizationSegments( "Extended Family", new List<string> { Constants.SegmentMarriedGuid, Constants.SegmentSmallGroupGuid } );
            SetContentChannelItemPersonalizationSegments( "Immediate Family", new List<string> { Constants.SegmentMarriedGuid } );

            SetContentChannelItemPersonalizationSegments( "Momentum at Home", new List<string> { Constants.SegmentSingleGuid, Constants.SegmentSmallGroupGuid } );
            SetContentChannelItemPersonalizationSegments( "Momentum at Work", new List<string> { Constants.SegmentSingleGuid } );
        }

        private void ConfigurePersonalizationForPeople()
        {
            var rockContext = new RockContext();

            // Ted Decker: Male, Married, Attender, Has Given, Small Group.
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentAllMenGuid, TestGuids.TestPeople.TedDecker );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentMarriedGuid, TestGuids.TestPeople.TedDecker );

            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentAttenderGuid, TestGuids.TestPeople.TedDecker );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentHasGivenGuid, TestGuids.TestPeople.TedDecker );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentSmallGroupGuid, TestGuids.TestPeople.TedDecker );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentTestInactiveGuid, TestGuids.TestPeople.TedDecker );

            // Bill Marble
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentAllMenGuid, TestGuids.TestPeople.BillMarble );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentMarriedGuid, TestGuids.TestPeople.BillMarble );


            // Mariah Jackson: Female, Single, Attender, Has Given.
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentAllWomenGuid, TestGuids.TestPeople.MariahJackson );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentSingleGuid, TestGuids.TestPeople.MariahJackson );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentAttenderGuid, TestGuids.TestPeople.MariahJackson );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, Constants.SegmentHasGivenGuid, TestGuids.TestPeople.MariahJackson );

            rockContext.SaveChanges();
        }

        private RequestFilter AddOrUpdateRequestFilterForQueryString( RockContext rockContext, string guid, string filterKey, string parameterName, string parameterValue )
        {
            var filterService = new RequestFilterService( rockContext );

            var newFilter = filterService.Get( guid.AsGuid() );
            if ( newFilter == null )
            {
                newFilter = new RequestFilter();
                filterService.Add( newFilter );
            }

            newFilter.RequestFilterKey = filterKey;
            newFilter.Name = filterKey;
            newFilter.Guid = guid.AsGuid();

            var filterConfig = new PersonalizationRequestFilterConfiguration();
            filterConfig.QueryStringRequestFilterExpressionType = FilterExpressionType.Filter;
            var queryStringFilter = new QueryStringRequestFilter()
            {
                Key = parameterName,
                ComparisonType = ComparisonType.EqualTo,
                ComparisonValue = parameterValue,
            };
            filterConfig.QueryStringRequestFilters = new List<QueryStringRequestFilter> { queryStringFilter };

            newFilter.FilterConfiguration = filterConfig;

            return newFilter;
        }

        private PersonAliasPersonalization AddOrUpdatePersonalizationSegmentForPerson( RockContext rockContext, string segmentGuid, string personGuid )
        {
            var pap = new PersonAliasPersonalization();
            pap.PersonalizationType = PersonalizationType.Segment;

            var personService = new PersonService( rockContext );
            var person = personService.GetByGuids( new List<Guid> { personGuid.AsGuid() } ).FirstOrDefault();
            pap.PersonAliasId = person.PrimaryAliasId.Value;

            var personalizationService = new PersonalizationSegmentService( rockContext );
            var segment = personalizationService.Get( segmentGuid.AsGuid() );
            pap.PersonalizationEntityId = segment.Id;

            var exists = rockContext.Set<PersonAliasPersonalization>()
                .Any( x => x.PersonAliasId == person.PrimaryAliasId.Value
                 && x.PersonalizationEntityId == segment.Id
                 && x.PersonalizationType == PersonalizationType.Segment );

            if ( !exists )
            {
                rockContext.Set<PersonAliasPersonalization>().Add( pap );
            }

            return pap;
        }

        private void ConfigurePersonalizationForSite()
        {
            var rockContext = new RockContext();

            var siteManager = new SiteService( rockContext );
            var site = siteManager.GetByIdentifierOrThrow( SystemGuid.Site.EXTERNAL_SITE );

            site.EnablePersonalization = true;
            site.EnableVisitorTracking = true;

            rockContext.SaveChanges();
        }

        private void SetContentChannelPersonalizationEnabled( string contentChannelIdentifier, bool isEnabled )
        {
            var rockContext = new RockContext();

            var siteManager = new ContentChannelService( rockContext );
            var contentChannel = siteManager.GetByIdentifierOrThrow( contentChannelIdentifier );

            contentChannel.EnablePersonalization = isEnabled;

            rockContext.SaveChanges();
        }

        private void SetContentChannelItemPersonalizationSegments( string contentChannelItemIdentifier, List<string> segmentIdentifiers )
        {
            var rockContext = new RockContext();

            var itemService = new ContentChannelItemService( rockContext );
            var contentItem = itemService.GetByIdentifierOrThrow( contentChannelItemIdentifier, "Title" );

            if ( !contentItem.ContentChannel.EnablePersonalization )
            {
                throw new Exception( "Personalization is not enabled for this Content Channel" );
            }

            var entityTypeId = EntityTypeCache.Get<Rock.Model.ContentChannelItem>().Id;
            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );

            var segmentIdList = new List<int>();
            foreach (var segmentIdentifier in segmentIdentifiers )
            {
                var segment = personalizationSegmentService.GetByIdentifierOrThrow( segmentIdentifier );
                segmentIdList.Add( segment.Id );
            }

            personalizationSegmentService.UpdatePersonalizedEntityForSegments( entityTypeId, contentItem.Id, segmentIdList );

            rockContext.SaveChanges();
        }

        #endregion
    }
}
