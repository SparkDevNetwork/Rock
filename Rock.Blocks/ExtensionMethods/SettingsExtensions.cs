﻿// <copyright>
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
using Rock.Enums.Controls;
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Extension methods to assist with getting and setting values between <see cref="AttributeValue"/> or
    /// <see cref="PersonPreference"/> records and their associated UI controls.
    /// </summary>
    internal static class SettingsExtensions
    {
        #region Generic Converters: Not Entity-Specific

        /// <summary>
        /// Returns the <see cref="ListItemBag.Value"/>, if defined, or <see langword="null"/> if not.
        /// </summary>
        /// <param name="listItemBag">The <see cref="ListItemBag"/> whose value should be returned.</param>
        /// <returns>The <see cref="ListItemBag.Value"/>, if defined, or <see langword="null"/> if not.</returns>
        public static string ToValueString( this ListItemBag listItemBag )
        {
            return listItemBag?.Value;
        }

        /// <summary>
        /// Serializes the <see cref="ListItemBag.Value"/>s as a comma-delimited string.
        /// </summary>
        /// <param name="listItemBags">The <see cref="ListItemBag"/> list whose values should be serialized.</param>
        /// <param name="shouldIncludeUndefinedValues">Whether <see langword="null"/>, empty string or whitespace <see cref="ListItemBag.Value"/>s should be included in the serialized return value.</param>
        /// <returns>The comma-delimited string of <see cref="ListItemBag.Value"/>s.</returns>
        public static string ToCommaDelimitedValuesString( this List<ListItemBag> listItemBags, bool shouldIncludeUndefinedValues = false )
        {
            var values = new List<string>();

            if ( listItemBags?.Any() == true )
            {
                foreach ( var listItemBag in listItemBags )
                {
                    if ( string.IsNullOrWhiteSpace( listItemBag?.Value ) && !shouldIncludeUndefinedValues )
                    {
                        continue;
                    }

                    values.Add( listItemBag.Value );
                }
            }

            return values.AsDelimited( "," );
        }

        /// <summary>
        /// Deserializes the <see cref="IEntity.Guid"/> string into a <see cref="ListItemBag"/>, using the specified delegate to source its corresponding <see cref="IEntityCache"/> instance.
        /// </summary>
        /// <param name="entityGuidString">The <see cref="IEntity.Guid"/> string.</param>
        /// <param name="getEntityCacheFunc">A delegate to source the corresponding <see cref="IEntityCache"/> instance using the <see cref="IEntity.Guid"/> string.</param>
        /// <returns>A <see cref="ListItemBag"/> representing the <see cref="IEntityCache"/>, with the <see cref="IEntityCache.Guid"/> string mapped to <see cref="ListItemBag.Value"/> and its corresponding <see cref="IEntityCache.ToString()"/> result mapped to <see cref="ListItemBag.Text"/>.</returns>
        public static ListItemBag ToListItemBagUsingCache( this string entityGuidString, Func<string, IEntityCache> getEntityCacheFunc )
        {
            if ( string.IsNullOrWhiteSpace( entityGuidString ) || getEntityCacheFunc == null )
            {
                return null;
            }

            var entityCache = getEntityCacheFunc( entityGuidString );
            if ( entityCache == null )
            {
                return null;
            }

            return entityCache.ToListItemBag();
        }

        /// <summary>
        /// Deserializes the comma-delimited <see cref="IEntity.Guid"/> strings into <see cref="ListItemBag"/>s, using the specified delegate to source corresponding <see cref="IEntityCache"/> instances.
        /// </summary>
        /// <param name="entityGuidStrings">The comma-delimited <see cref="IEntity.Guid"/> strings.</param>
        /// <param name="getEntityCacheFunc">A delegate to source corresponding <see cref="IEntityCache"/> instances using each <see cref="IEntity.Guid"/> string.</param>
        /// <returns>A <see cref="ListItemBag"/> list of <see cref="IEntityCache"/> representations, with each <see cref="IEntityCache.Guid"/> string mapped to <see cref="ListItemBag.Value"/> and its corresponding <see cref="IEntityCache.ToString()"/> result mapped to <see cref="ListItemBag.Text"/>.</returns>
        public static List<ListItemBag> ToListItemBagListUsingCache( this string entityGuidStrings, Func<string, IEntityCache> getEntityCacheFunc )
        {
            var listItemBags = new List<ListItemBag>();

            foreach ( var entityGuidString in ( entityGuidStrings ?? string.Empty ).SplitDelimitedValues( "," ).Where( a => !string.IsNullOrWhiteSpace( a ) ) )
            {
                var listItemBag = entityGuidString.ToListItemBagUsingCache( getEntityCacheFunc );
                if ( listItemBag != null )
                {
                    listItemBags.Add( listItemBag );
                }
            }

            return listItemBags;
        }

        /// <summary>
        /// Attempts to match each comma-delimited string value to an available option, returning a <see cref="ListItemBag"/> for each matched value.
        /// </summary>
        /// <param name="values">The comma-delimited string values for which to create <see cref="ListItemBag"/>s.</param>
        /// <param name="availableOptions">The available options to which each value should be matched.</param>
        /// <returns>A <see cref="ListItemBag"/> list of values and their corresponding friendly text from the matching available options.</returns>
        public static List<ListItemBag> ToListItemBagListUsingAvailableOptions( this string delimitedValues, List<ListItemBag> availableOptions )
        {
            var listItemBags = new List<ListItemBag>();

            foreach ( var stringValue in ( delimitedValues ?? string.Empty )
                .SplitDelimitedValues( "," )
                .Where( v => !string.IsNullOrWhiteSpace( v ) ) )
            {
                var option = availableOptions?.FirstOrDefault( ao => ao.Value == stringValue );
                if ( option == null )
                {
                    continue;
                }

                listItemBags.Add( new ListItemBag
                {
                    Value = stringValue,
                    Text = option?.Text
                } );
            }

            return listItemBags;
        }

        #endregion Generic Converters: Not Entity-Specific

        #region Convenience Converters: Entity-Specific Usages of Generic Converters

        #region Campuses

        /// <summary>
        /// Deserializes the campus guid string into a <see cref="ListItemBag"/> so it can be populated in the associated UI control.
        /// <para>
        /// The <see cref="CampusCache"/> is used to populate the <see cref="ListItemBag.Text"/> for the deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="campusGuidString">The campus guid string.</param>
        /// <returns>A <see cref="ListItemBag"/> representing the campus.</returns>
        public static ListItemBag CampusGuidToListItemBag( this string campusGuidString )
        {
            return campusGuidString.ToListItemBagUsingCache( CampusCache.Get );
        }

        /// <summary>
        /// Deserializes the comma-delimited campus guid strings into <see cref="ListItemBag"/>s so they can be populated in the associated UI control.
        /// <para>
        /// The <see cref="CampusCache"/> is used to populate the <see cref="ListItemBag.Text"/> for each deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="campusGuidStrings">The comma-delimited campus guid strings.</param>
        /// <returns>A <see cref="ListItemBag"/> list representing the campuses.</returns>
        public static List<ListItemBag> CampusGuidsToListItemBagList( this string campusGuidStrings )
        {
            return campusGuidStrings.ToListItemBagListUsingCache( CampusCache.Get );
        }

        #endregion Campuses

        #region Categories

        /// <summary>
        /// Deserializes the category guid string into a <see cref="ListItemBag"/> so it can be populated in the associated UI control.
        /// <para>
        /// The <see cref="CategoryCache"/> is used to populate the <see cref="ListItemBag.Text"/> for the deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="categoryGuidString">The category guid string.</param>
        /// <returns>A <see cref="ListItemBag"/> representing the category.</returns>
        public static ListItemBag CategoryGuidToListItemBag( this string categoryGuidString )
        {
            return categoryGuidString.ToListItemBagUsingCache( CategoryCache.Get );
        }

        /// <summary>
        /// Deserializes the comma-delimited category guid strings into <see cref="ListItemBag"/>s so they can be populated in the associated UI control.
        /// <para>
        /// The <see cref="CategoryCache"/> is used to populate the <see cref="ListItemBag.Text"/> for each deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="categoryGuidStrings">The comma-delimited category guid strings.</param>
        /// <returns>A <see cref="ListItemBag"/> list representing the categories.</returns>
        public static List<ListItemBag> CategoryGuidsToListItemBagList( this string categoryGuidStrings )
        {
            return categoryGuidStrings.ToListItemBagListUsingCache( CategoryCache.Get );
        }

        #endregion Categories

        #region Defined Values

        /// <summary>
        /// Deserializes the defined value guid string into a <see cref="ListItemBag"/> so it can be populated in the associated UI control.
        /// <para>
        /// The <see cref="DefinedValueCache"/> is used to populate the <see cref="ListItemBag.Text"/> for the deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="definedValueGuidString">The defined value guid string.</param>
        /// <returns>A <see cref="ListItemBag"/> representing the defined value.</returns>
        public static ListItemBag DefinedValueGuidToListItemBag( this string definedValueGuidString )
        {
            return definedValueGuidString.ToListItemBagUsingCache( DefinedValueCache.Get );
        }

        /// <summary>
        /// Deserializes the comma-delimited defined value guid strings into <see cref="ListItemBag"/>s so they can be populated in the associated UI control.
        /// <para>
        /// The <see cref="DefinedValueCache"/> is used to populate the <see cref="ListItemBag.Text"/> for each deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="definedValueGuidStrings">The comma-delimited defined value guid strings.</param>
        /// <returns>A <see cref="ListItemBag"/> list representing the defined values.</returns>
        public static List<ListItemBag> DefinedValueGuidsToListItemBagList( this string definedValueGuidStrings )
        {
            return definedValueGuidStrings.ToListItemBagListUsingCache( DefinedValueCache.Get );
        }

        #endregion Defined Values

        #region Group Types

        /// <summary>
        /// Deserializes the group type guid string into a <see cref="ListItemBag"/> so it can be populated in the associated UI control.
        /// <para>
        /// The <see cref="GroupTypeCache"/> is used to populate the <see cref="ListItemBag.Text"/> for the deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="groupTypeGuidStrings">The group type guid string.</param>
        /// <returns>The <see cref="ListItemBag"/> representing the group type.</returns>
        public static ListItemBag GroupTypeGuidToListItemBag( this string groupTypeGuidString )
        {
            return groupTypeGuidString.ToListItemBagUsingCache( GroupTypeCache.Get );
        }

        /// <summary>
        /// Deserializes the comma-delimited group type guid strings into <see cref="ListItemBag"/>s so they can be populated in the associated UI control.
        /// <para>
        /// The <see cref="GroupTypeCache"/> is used to populate the <see cref="ListItemBag.Text"/> for each deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="groupTypeGuidStrings">The comma-delimited group type guid strings.</param>
        /// <returns>The <see cref="ListItemBag"/> list representing the group types.</returns>
        public static List<ListItemBag> GroupTypeGuidsToListItemBagList( this string groupTypeGuidStrings )
        {
            return groupTypeGuidStrings.ToListItemBagListUsingCache( GroupTypeCache.Get );
        }

        #endregion Group Types

        #region Named Schedules

        /// <summary>
        /// Deserializes the named schedule guid string into a <see cref="ListItemBag"/> so it can be populated in the associated UI control.
        /// <para>
        /// The <see cref="NamedScheduleCache"/> is used to populate the <see cref="ListItemBag.Text"/> for the deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="namedScheduleGuidString">The named schedule guid string.</param>
        /// <returns>The <see cref="ListItemBag"/> representing the named schedule.</returns>
        public static ListItemBag NamedScheduleGuidToListItemBag( this string namedScheduleGuidString )
        {
            return namedScheduleGuidString.ToListItemBagUsingCache( NamedScheduleCache.Get );
        }

        /// <summary>
        /// Deserializes the comma-delimited named schedule guid strings into <see cref="ListItemBag"/>s so they can be populated in the associated UI control.
        /// <para>
        /// The <see cref="NamedScheduleCache"/> is used to populate the <see cref="ListItemBag.Text"/> for each deserialized guid value.
        /// </para>
        /// </summary>
        /// <param name="namedScheduleGuidStrings">The comma-delimited named schedule guid strings.</param>
        /// <returns>The <see cref="ListItemBag"/> list representing the named schedules.</returns>
        public static List<ListItemBag> NamedScheduleGuidsToListItemBagList( this string namedScheduleGuidStrings )
        {
            return namedScheduleGuidStrings.ToListItemBagListUsingCache( NamedScheduleCache.Get );
        }

        #endregion Named Schedules

        #endregion Convenience Converters: Entity-Specific Usages of Generic Converters

        #region One-Off Converters with No Commonality

        #region PageRouteValueBag

        /// <summary>
        /// Serializes the <see cref="PageRouteValueBag"/> as comma-delimited guid strings: the first representing the <see cref="Page"/>; the second (if defined) representing the <see cref="PageRoute"/>.
        /// </summary>
        /// <param name="pageReference">The <see cref="PageRouteValueBag"/> representing the page reference.</param>
        /// <returns>The serialized page reference.</returns>
        public static string ToCommaDelimitedPageRouteValues( this PageRouteValueBag pageReference )
        {
            if ( pageReference == null )
            {
                return null;
            }

            return new List<ListItemBag>
            {
                pageReference?.Page,
                pageReference?.Route
            }.ToCommaDelimitedValuesString();
        }

        /// <summary>
        /// Deserializes the comma-delimited <see cref="Page"/> and (if defined) <see cref="PageRoute"/> guid strings, so they can be preselected in the page picker.
        /// <para>
        /// The <see cref="PageCache"/> is used to populate the <see cref="ListItemBag.Text"/> for the <see cref="PageRouteValueBag.Page"/> and (if defined) <see cref="PageRouteValueBag.Route"/>.
        /// </para>
        /// </summary>
        /// <param name="pageReferenceGuidStrings">The serialized page reference.</param>
        /// <returns>The <see cref="PageRouteValueBag"/> representing the page reference.</returns>
        public static PageRouteValueBag ToPageRouteValueBag( this string pageReferenceGuidStrings )
        {
            var guidStrings = ( pageReferenceGuidStrings ?? string.Empty ).SplitDelimitedValues( "," );
            if ( !guidStrings.Any() )
            {
                return null;
            }

            // The first guid represents the page; the second (if defined) represents the route.
            var pageGuid = guidStrings.First();
            var pageCache = PageCache.Get( pageGuid );
            if ( pageCache == null )
            {
                return null;
            }

            var page = new ListItemBag
            {
                Value = pageGuid,
                Text = pageCache.InternalName
            };

            ListItemBag route = null;
            if ( guidStrings.Length == 2 )
            {
                var routeGuid = guidStrings.Last();
                var pageRouteInfo = pageCache.PageRoutes?.FirstOrDefault( r => r.Guid.Equals( new Guid( routeGuid ) ) );
                if ( pageRouteInfo != null )
                {
                    route = new ListItemBag
                    {
                        Value = routeGuid,
                        Text = pageRouteInfo.Route
                    };
                }
            }

            return new PageRouteValueBag
            {
                Page = page,
                Route = route
            };
        }

        #endregion

        #endregion One-Off Converters with No Commonality

        #region Validators

        #region SlidingDateRangeBag

        /// <summary>
        /// Deserializes the delimited sliding date range string into a <see cref="SlidingDateRangeBag"/>, if possible.
        /// </summary>
        /// <param name="delimitedSlidingDateRange">The delimited sliding date range string.</param>
        /// <returns>A <see cref="SlidingDateRangeBag"/> or <see langword="null"/> if unable to parse.</returns>
        /// <remarks>
        /// <para>
        /// No validation will be performed on the deserialized <see cref="SlidingDateRangeBag"/>. Use the
        /// <see cref="Validate(SlidingDateRangeBag, SlidingDateRangeBag)"/> method for this purpose.
        /// </para>
        /// <para>
        /// The expected string format is "RangeType|TimeValue|TimeUnit|LowerDate|UpperDate".
        /// </para>
        /// </remarks>
        public static SlidingDateRangeBag ToSlidingDateRangeBagOrNull( this string delimitedSlidingDateRange )
        {
            if ( delimitedSlidingDateRange.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var slidingDateRangeParts = delimitedSlidingDateRange.SplitDelimitedValues( "|", StringSplitOptions.None );
            if ( slidingDateRangeParts.Length != 5 )
            {
                return null;
            }

            var rangeType = slidingDateRangeParts[0].ConvertToEnumOrNull<SlidingDateRangeType>();
            if ( !rangeType.HasValue )
            {
                return null;
            }

            return new SlidingDateRangeBag
            {
                RangeType = rangeType.Value,
                TimeValue = slidingDateRangeParts[1].AsIntegerOrNull(),
                TimeUnit = slidingDateRangeParts[2].ConvertToEnumOrNull<TimeUnitType>(),
                LowerDate = slidingDateRangeParts[3].AsDateTime(),
                UpperDate = slidingDateRangeParts[4].AsDateTime()
            };
        }

        /// <summary>
        /// Serializes the <see cref="SlidingDateRangeBag"/> as a delimited sliding date range string.
        /// </summary>
        /// <param name="slidingDateRangeBag">The <see cref="SlidingDateRangeBag"/> to serialize.</param>
        /// <returns>The delimited sliding date range string in the format "RangeType|TimeValue|TimeUnit|LowerDate|UpperDate"
        /// or <see langword="null"/> if unable to serialize.</returns>
        public static string ToDelimitedSlidingDateRangeOrNull( this SlidingDateRangeBag slidingDateRangeBag )
        {
            if ( slidingDateRangeBag == null )
            {
                return null;
            }

            var rangeType = slidingDateRangeBag.RangeType.ToString();
            var timeValue = slidingDateRangeBag.TimeValue.ToString();
            var timeUnit = slidingDateRangeBag.TimeUnit.ToString();
            var lowerDate = slidingDateRangeBag.LowerDate.ToString();
            var upperDate = slidingDateRangeBag.UpperDate.ToString();

            return $"{rangeType}|{timeValue}|{timeUnit}|{lowerDate}|{upperDate}";
        }

        /// <summary>
        /// Gets the <see cref="DateRange"/> represented within the <see cref="SlidingDateRangeBag"/>.
        /// </summary>
        /// <param name="slidingDateRangeBag">The <see cref="SlidingDateRangeBag"/> for which to get the <see cref="DateRange"/>.</param>
        /// <returns>The <see cref="DateRange"/> represented within the <see cref="SlidingDateRangeBag"/>, or <see langword="null"/>.</returns>
        public static DateRange ToActualDateRange( this SlidingDateRangeBag slidingDateRangeBag )
        {
            return RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( slidingDateRangeBag.ToDelimitedSlidingDateRangeOrNull() );
        }

        /// <summary>
        /// Ensures this <see cref="SlidingDateRangeBag"/> passes basic validation checks.
        /// </summary>
        /// <param name="toValidate">The <see cref="SlidingDateRangeBag"/> to validate.</param>
        /// <param name="defaultIfInvalid">The default <see cref="SlidingDateRangeBag"/> to use if invalid.</param>
        /// <returns>An object containing the validated <see cref="SlidingDateRangeBag"/> (or default if invalid) along
        /// with the actual <see cref="DateRange"/> represented within.</returns>
        public static ValidatedDateRange Validate( this SlidingDateRangeBag toValidate, SlidingDateRangeBag defaultIfInvalid )
        {
            // Validate the sliding date range bag as follows:
            //
            //  1) If no sliding date range bag provided, return the default.
            //  2) If range type is specific date range (with selected start and end dates):
            //      a) If end date < start date, set end date = start date.
            //      b) If only a start date is selected, set end date = start date.
            //      c) If only an end date is selected, set start date = end date.
            //      d) If neither start nor end date are selected, return the default.
            //  3) Allow any other valid selections (knowing they might represent an excessively-large range).

            if ( toValidate == null )
            {
                return new ValidatedDateRange
                {
                    SlidingDateRangeBag = defaultIfInvalid,
                    ActualDateRange = defaultIfInvalid.ToActualDateRange()
                };
            }

            if ( toValidate.RangeType == SlidingDateRangeType.DateRange )
            {
                var lowerDate = toValidate.LowerDate;
                var upperDate = toValidate.UpperDate;

                if ( lowerDate.HasValue && upperDate.HasValue )
                {
                    if ( upperDate < lowerDate )
                    {
                        // Set end date = start date.
                        toValidate.UpperDate = lowerDate;
                    }
                }
                else if ( lowerDate.HasValue )
                {
                    // Set end date = start date.
                    toValidate.UpperDate = lowerDate;
                }
                else if ( upperDate.HasValue )
                {
                    // Set start date = end date.
                    toValidate.LowerDate = upperDate;
                }
                else
                {
                    // Return the default.
                    return new ValidatedDateRange
                    {
                        SlidingDateRangeBag = defaultIfInvalid,
                        ActualDateRange = defaultIfInvalid.ToActualDateRange()
                    };
                }
            }

            var actualDateRange = toValidate.ToActualDateRange();
            if ( actualDateRange?.Start == null || actualDateRange?.End == null )
            {
                // If for some reason we don't have valid start and end dates at this point, return the default.
                return new ValidatedDateRange
                {
                    SlidingDateRangeBag = defaultIfInvalid,
                    ActualDateRange = defaultIfInvalid.ToActualDateRange()
                };
            }

            // Otherwise, return the provided, validated sliding date range bag + actual date range.
            return new ValidatedDateRange
            {
                SlidingDateRangeBag = toValidate,
                ActualDateRange = actualDateRange
            };
        }

        /// <summary>
        /// A POCO to represent a validated <see cref="Rock.ViewModels.Controls.SlidingDateRangeBag"/> along with the
        /// actual <see cref="DateRange"/> represented within.
        /// </summary>
        public class ValidatedDateRange
        {
            /// <summary>
            /// The sliding date range bag.
            /// </summary>
            public SlidingDateRangeBag SlidingDateRangeBag { get; set; }

            /// <summary>
            /// The actual date range represented within the sliding date range bag.
            /// </summary>
            public DateRange ActualDateRange { get; set; }
        }

        #endregion SlidingDateRangeBag

        #endregion Validators
    }
}
