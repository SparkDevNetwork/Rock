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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rock.Core.Geography;
using Rock.Core.Geography.Classes;
using Rock.Data;
using Rock.Lava.Filters.Internal;
using Rock.Model;
using Rock.Net;
using Rock.Enums.Geography;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Data.Entity;
using Nest;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Group Finder",
        TagName = "groupfinder",
        Description = "Provides the group finder logic in a simple to use shortcode..",
        Documentation = DocumentationMetadata,
        Parameters = ParameterNamesMetadata,
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
    public class GroupFinderShortcode : LavaShortcodeBase, ILavaBlock
    {
        #region Constants

        /// <summary>
        /// The parameter names that are used in the shortcode.
        /// </summary>
        internal static class ParameterKeys
        {
            /// <summary>
            /// The group type ids to filter by.
            /// </summary>
            public const string GroupTypeIds = "grouptypeids";

            /// <summary>
            /// The maximum number of groups to return.
            /// </summary>
            public const string MaxResults = "maxresults";

            /// <summary>
            /// If true, only the closest location for each group will be returned. Otherwise, all locations for a group will be considered.
            /// </summary>
            public const string ReturnOnlyClosestLocationPerGroup = "returnonlyclosestlocationpergroup";

            /// <summary>
            /// The maximum distance to search for groups in meters.
            /// </summary>
            public const string MaxDistance = "maxdistance";

            /// <summary>
            /// The lat/long of the location to use for the search.
            /// </summary>
            public const string Origin = "origin";

            /// <summary>
            /// The lat/long of the location to use for the search.
            /// </summary>
            public const string TravelMode = "travelmode";

            /// <summary>
            /// The properties to eager load on the initial query.
            /// </summary>
            public const string Include = "include";

            /// <summary>
            /// Hides groups that don't have capcity for new members.
            /// </summary>
            public const string HideOvercapacityGroups = "hideovercapacitygroups";
        }

        /// <summary>
        /// The parameter names that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string ParameterNamesMetadata = ParameterKeys.GroupTypeIds
            + "," + ParameterKeys.MaxResults
            + "," + ParameterKeys.ReturnOnlyClosestLocationPerGroup
            + "," + ParameterKeys.MaxDistance
            + "," + ParameterKeys.Origin
            + "," + ParameterKeys.Include
            + "," + ParameterKeys.HideOvercapacityGroups
            + "," + ParameterKeys.TravelMode;

        /// <summary>
        /// The documentation for the shortcode that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string DocumentationMetadata = @"<p>
Rock's Group Finder block is an amazing tool to help people connect in community. Its simplicity comes with an opinionated user experience. This shortcode is for those who want more control over designing that experience. It handles the group filtering logic for you—and adds a bit of magic even the block can't replicate. Pair it with Rock's Lava Applications, and you've got the makings of a powerful search experience.
</p>

<p>
There are two experiences to choose from. One is a simple filtering option that lets you filter by attributes, campus, time of day, and day of week. The other builds on this by adding the ability to filter and sort groups based on proximity to an origin point.
</p>

<div class=""alert alert-info"">
	To power all of this magic, your Rock instance must have an active Google Maps API key with the Routes API enabled.
</div>

<h5>Selecting an Origin</h5>
<p>
Providing an origin is easy—just use the <code>origin</code> parameter in the shortcode. You can provide several formats, including:
</p>

<ul>
	<li><strong>Person Id</strong> – Provide an integer to use that person’s mapped location in Rock.</li>
	<li><strong>Latitude/Longitude</strong> – You can manually provide coordinates.</li>
	<li><strong>Postal Code</strong> – Use a value like <code>postalcode 85383</code> to filter and sort groups by the center of that ZIP code. We handle the geocoding for you.</li>
	<li><strong>Address</strong> – Enter an address, and we’ll geocode it as the origin.</li>
	<li><strong>Cross Streets</strong> – Provide cross streets (e.g., <code>Lake Pleasant & Happy Valley Peoria, AZ</code>), and we’ll use Google’s Routes API to geocode the location. This is great when individuals prefer not to enter their full address. While Google suggests using the exact city, we’ve found that using the metro area usually works fine.</li>
	<li><strong>City, State</strong> – For broader searches, use the city center as the origin.</li>
	<li><strong>Named Place</strong> – You can also use a named place like <code>Sky Harbor Airport</code> or <code>Chase Field, Phoenix, AZ</code>.</li>
</ul>

<h5>Travel Distance & Time</h5>
<p>
One common issue with group finders is that ""closest"" often means ""as the crow flies."" But geography and routes can make that inaccurate. To solve this, use the <code>travelmode</code> option. Set it to <code>drive</code>, <code>walk</code>, or <code>bicycle</code>, and we’ll return both travel distance and time 🤯.
</p>

<h5>Filtering</h5>
<p>
The shortcode supports a rich set of filters. All filters are combined with logical AND, so all criteria must match for a group to be shown.
</p>

<strong>Attributes</strong>
<p>
You can filter by group attributes. The filters work on raw attribute values, so keep that in mind when defining your filters. You can include as
many attribute filters as needed. To support filtering across groups with multiple group types, the filter won't be applied to any group that doesn't have an
attribute with the specified key.
</p>

<pre>{[ groupfinder grouptypeids:'25' ]}
	
    [[ filter type:'attribute' operator:'eq' key:'AllowsChildren' ]]true[[ endfilter ]]

    &lt;ul&gt;
    {% for result in MatchedGroups %}
        &lt;li&gt;&lt;strong&gt;{{ result.Group.Name }}&lt;/strong&gt;&lt;/li&gt;
    {% endfor %}
    &lt;/ul&gt;
{[ endgroupfinder ]}</pre>

<p>
In the example above, we filter groups by the attribute key <code>AllowsChildren</code>. Since it's a boolean, we use <code>true</code> with the <code>eq</code> (equals) operator. Supported operators include:
</p>

<ul>
	<li><code>eq</code> – Equals (default)</li>
	<li><code>ne</code> – Not equal</li>
	<li><code>sw</code> – Starts with</li>
	<li><code>ew</code> – Ends with</li>
	<li><code>con</code> – Contains</li>
	<li><code>in</code> – Checks if the value is in a multi-select attribute list (use commas for multiple values)</li>
</ul>

<strong>Campus(es)</strong>
<p>
You can filter by a comma-separated list of campus IDs. This filter doesn’t use an operator.
</p>

<pre>{[ groupfinder grouptypeids:'25' ]}
	
    [[ filter type:'campus' ]]12,16[[ endfilter ]]

    &lt;ul&gt;
    {% for result in MatchedGroups %}
        &lt;li&gt;&lt;strong&gt;{{ result.Group.Name }}&lt;/strong&gt;&lt;/li&gt;
    {% endfor %}
    &lt;/ul&gt;
{[ endgroupfinder ]}</pre>

<strong>Day of Week</strong>
<p>
If your group type supports scheduling, you can filter by day of week using names or numbers (Sunday = 0).
</p>

<pre>{[ groupfinder grouptypeids:'25' ]}
	
    [[ filter type:'dayofweek' ]]Monday,Wednesday[[ endfilter ]]

    &lt;ul&gt;
    {% for result in MatchedGroups %}
        &lt;li&gt;&lt;strong&gt;{{ result.Group.Name }}&lt;/strong&gt;&lt;/li&gt;
    {% endfor %}
    &lt;/ul&gt;
{[ endgroupfinder ]}</pre>

<strong>Time of Day</strong>
<p>
You can filter by time using multiple filters for ranges. Supported operators include <code>eq</code>, <code>ne</code>, <code>lt</code>, <code>lte</code>, <code>gt</code>, and <code>gte</code>. Default is <code>gte</code>. Time can be in <code>5:00 PM</code> or <code>17:00</code> format.
</p>

<pre>{[ groupfinder grouptypeids:'25' ]}
	
    [[ filter type:'timeofday' ]]5:00 PM[[ endfilter ]]
    [[ filter type:'timeofday' operator:'lte' ]]9:00 PM[[ endfilter ]]

    &lt;ul&gt;
    {% for result in MatchedGroups %}
        &lt;li&gt;&lt;strong&gt;{{ result.Group.Name }}&lt;/strong&gt;&lt;/li&gt;
    {% endfor %}
    &lt;/ul&gt;
{[ endgroupfinder ]}</pre>

<h5>Settings</h5>
<p>Below is a list of all shortcode parameters:</p>

<ul>
	<li><strong>grouptypeids</strong> – Comma-separated group type IDs to search.</li>
	<li><strong>origin</strong> – The point used for distance filtering and sorting.</li>
	<li><strong>maxresults</strong> (default: 10) – Max number of groups to return.</li>
	<li><strong>returnonlyclosestlocationpergroup</strong> (default: true) – If true, only the closest location for each group is returned.</li>
	<li><strong>maxdistance</strong> – Max distance from the origin, in meters.</li>
	<li><strong>travelmode</strong> – Adds travel distance/time. Options: <code>drive</code>, <code>walk</code>, <code>bicycle</code>.</li>
	<li><strong>include</strong> – Appended to the query to eager-load group properties.</li>
	<li><strong>hideovercapacitygroups</strong> (default: true) – Hides groups over capacity (group + role capacity).</li>
</ul>

<h5>Response Data</h5>
<p>Each matching group includes the following:</p>

<ul>
	<li><strong>Group</strong> – The matching group.</li>
	<li><strong>Location</strong> – The group's location.</li>
	<li><strong>StraightLineDistanceInMeters</strong> – Distance ""as the crow flies"" between origin and group.</li>
	<li><strong>TravelDistanceInMeters</strong> – Added if <code>travelmode</code> is provided.</li>
	<li><strong>TravelTimeInMinutes</strong> – Added if <code>travelmode</code> is provided.</li>
	<li><strong>TravelMode</strong> – The travel mode used.</li>
</ul>

<h5>Full Example</h5>
<p>Here’s a full example using several filters and options:</p>

<pre>{[ groupfinder origin:'{{ CurrentPerson.Id }}' grouptypeids:'25' travelmode:'walk' maxresults:'5' maxdistance:'20000' include:'Group.Schedule' ]}
    [[ filter type:'dayofweek' ]]Monday, Tuesday[[ endfilter ]]
    [[ filter type:'timeofday' ]]5:00 PM[[ endfilter ]]
    [[ filter type:'timeofday' operator:'lte' ]]9:00 PM[[ endfilter ]]
	
    [[ filter type:'attribute' operator:'eq' key:'AllowsChildren' ]]true[[ endfilter ]]
    [[ filter type:'attribute' operator:'in' key:'SupportOptions' ]]1,3[[ endfilter ]]

    &lt;ul&gt;
    {% for result in MatchedGroups %}
        &lt;li&gt;
            &lt;strong&gt;{{ result.Group.Name }}&lt;/strong&gt;&lt;br&gt;
            {{ result.Location.Latitude }}, {{ result.Location.Longitude }}&lt;br&gt;
            {{ result.Location.Street1 }}&lt;br&gt;
            Distance: {{ result.StraightLineDistanceInMeters }} meters – {{ result.StraightLineDistanceInMeters | MetersToMiles }} miles&lt;br&gt;
            Travel Time: {{ result.TravelTimeInMinutes }} minutes&lt;br&gt;
            Travel Distance: {{ result.TravelDistanceInMeters }} meters&lt;br&gt;
            Meeting Day: {{ result.Group.Schedule.WeeklyDayOfWeek }}&lt;br&gt;
            Meeting Time: {{ result.Group.Schedule.WeeklyTimeOfDay }}
        &lt;/li&gt;
    {% endfor %}
    &lt;/ul&gt;
{[ endgroupfinder ]}</pre>

<h5>Final Notes</h5>
<p>
You may have noticed that distance values are returned in meters. If you're more comfortable with the imperial system, you can use the <code>MetersToMiles</code> and <code>MilesToMeters</code> Lava filters to convert values.
</p>
";

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the type of Lava element for this shortcode.
        /// </summary>
        public override LavaShortcodeTypeSpecifier ElementType => LavaShortcodeTypeSpecifier.Block;

        #endregion

        #region Fields

        /// <summary>
        /// The markup that was passed after the shortcode name and before the closing ]}.
        /// </summary>
        private string _blockPropertiesMarkup = string.Empty;

        /// <summary>
        /// The markup that was inside the shortcode block.
        /// </summary>
        private string _internalMarkup = string.Empty;

        /// <summary>
        /// Sharable rock context
        /// </summary>
        private RockContext _rockContext;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _blockPropertiesMarkup = markup;

            // Get the internal Lava for the block. The last token will be the block's end tag.
            _internalMarkup = string.Join( string.Empty, tokens.Take( tokens.Count - 1 ) );

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var engine = context.GetService<ILavaEngine>();

            // Get Rock Context
            _rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );
            //var mergedMarkup = engine.RenderTemplate( _internalMarkup, LavaRenderParameters.WithContext( context ) );
            var childElementsAreValid = ExtractBlockChildElements( context, _internalMarkup, out var childElements, out var residualBlockContent );

            if ( !childElementsAreValid )
            {
                result.Write( "Child configuration settings are invalid." );
                return;
            }

            // Get the options for the shortcode
            var options = new Options
            {
                GroupTypeIds = settings[ParameterKeys.GroupTypeIds] ?? "",
                MaxResults = settings[ParameterKeys.MaxResults].AsIntegerOrNull() ?? 10,
                ReturnOnlyClosestLocationPerGroup = settings[ParameterKeys.ReturnOnlyClosestLocationPerGroup].AsBooleanOrNull() ?? true,
                MaxDistance = settings[ParameterKeys.MaxDistance].AsIntegerOrNull(),
                Origin = settings[ParameterKeys.Origin].ToString(),
                OriginPoint = GetOriginPoint( settings[ParameterKeys.Origin].ToString(), context ),
                TravelMode = settings[ParameterKeys.TravelMode].ToString().ConvertToEnumOrNull<TravelMode>(),
                Include = settings[ParameterKeys.Include] ?? "Group.Schedule",
                HideOvercapacityGroups = settings[ParameterKeys.HideOvercapacityGroups].AsBooleanOrNull() ?? true
            };

            // Create the initial queryable based on whether there is a origin provided.
            var groupQuery = GetGroupLocationQueryable( options );

            ApplyFilters( groupQuery, options, childElements );

            // Convert out origin point to a DbGeography for use in EF queries
            var sourcePoint = options.OriginPoint.ToDatabase();

            // Run query to get the results.
            var results = groupQuery.Select( g => new GroupProximityResult
            {
                StraightLineDistanceInMeters = g.Location.GeoPoint.Distance( sourcePoint ),
                Group = g.Group,
                Location = g.Location
            } ).ToList();

            // Append travel mode details
            if ( options.Origin.IsNotNullOrWhiteSpace() && options.TravelMode != null && results.Count > 0 )
            {
                try
                {
                    results = AppendTravelModeDetails( options.OriginPoint, options.TravelMode.Value, results );
                }
                catch ( Exception ex )
                {
                    var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                    result.Write( $"Error calculating travel distances: {message}" );
                    return;
                }
            }

            // Process the residual content and output results
            var mergeFields = context.GetMergeFields();
            mergeFields.AddOrReplace( "MatchedGroups", results );

            result.Write( residualBlockContent.ResolveMergeFields( mergeFields ) );
        }

        /// <summary>
        /// Creates an IQueryable for the search based on the existence of an origin point
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> GetGroupLocationQueryable( Options options )
        {
            if ( options.Origin.IsNotNullOrWhiteSpace() )
            {
                return new GroupService( _rockContext )
                    .GetNearestGroups( options.OriginPoint, options.GroupTypeIdList, options.MaxResults, options.ReturnOnlyClosestLocationPerGroup, options.MaxDistance )
                    .Include( options.Include );
            }
            else
            {
                return new GroupLocationService( _rockContext ).Queryable()
                    .Where( gl => options.GroupTypeIdList.Contains( gl.Group.GroupTypeId ) )
                    .Take( options.MaxResults );
            }
        }

        #region Filter Logic

        /// <summary>
        /// Applies the filters to the search
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilters( IQueryable<GroupLocation> groupQuery, Options options, List<ChildBlockElement> childElements )
        {
            ApplyFilterGroupOvercapacity( groupQuery, options );

            // Process each of the settings they provided in the child elements.
            foreach ( var setting in childElements )
            {
                if ( setting.Name == "filter" )
                {
                    var value = setting.Content;

                    switch ( setting.Parameters.GetValueOrNull( "type" ).ToString() )
                    {
                        // Campus(es)
                        case "campus":
                            {
                                ApplyFilterCampus( groupQuery, setting, options );
                                break;
                            }
                        // Attributes
                        case "attribute":
                            {
                                ApplyFilterAttributes( groupQuery, setting, options );
                                break;
                            }
                        // Day of week
                        case "dayofweek":
                            {
                                ApplyFilterDayOfWeek( groupQuery, setting, options );
                                break;
                            }
                        // Time of day
                        case "timeofday":
                            {
                                ApplyFilterTimeOfDay( groupQuery, setting, options );
                                break;
                            }
                    }
                }
            }

            return groupQuery;
        }

        /// <summary>
        /// Applies the campus filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterAttributes( IQueryable<GroupLocation> groupQuery, ChildBlockElement setting, Options options )
        {
            var key = setting.Parameters.GetValueOrNull( "key" ).ToString();
            var value = setting.Content;

            // No key specified, return unfiltered
            if ( key.IsNullOrWhiteSpace() )
            {
                return groupQuery; 
            }

            // Default the operator to 'eq' if not specified
            var filterOperator = setting.Parameters.GetValueOrNull( "operator" ) ?? "eq";

            switch ( filterOperator )
            {
                case "con":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value.Contains( value ) ) );
                    }
                case "sw":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value.StartsWith( value ) ) );
                    }
                case "ew":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value.EndsWith( value ) ) );
                    }
                case "in":
                    {
                        /*  Here we want to support an input value of 1,3 for an attribute with the key of "MultiValue'
                            matching to an attribute values of 1,2,3,4,5

                            To do this we'll create an expression tree for our query like:

                            { gl => 
                                !gl.Group.GroupAttributeValues.Any(a => a.Key == "MultiValue") ||
                                gl.Group.GroupAttributeValues.Any(a => 
                                    a.Key == "MultiValue" &&
                                    (
                                        a.Value == "1" ||
                                        a.Value.StartsWith("1,") ||
                                        a.Value.EndsWith(",1") ||
                                        a.Value.Contains(",1,") ||
                                        a.Value == "3" ||
                                        a.Value.StartsWith("3,") ||
                                        a.Value.EndsWith(",3") ||
                                        a.Value.Contains(",3,")
                                    )
                                )
                            )}

                            Note this will return true if the attribute key does not exist. This allows us to search across
                            multiple group types that might have differing attributes.
                        */

                        // Split the input
                        var valueList = value
                                .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                                .ToList();

                        var aParam = Expression.Parameter( typeof( QueryableAttributeValue ), "a" );
                        var keyProperty = Expression.Property( aParam, "Key" );
                        var keyCheck = Expression.Equal( keyProperty, Expression.Constant( key ) );

                        // Build the OR conditions for a.Value
                        var valueProperty = Expression.Property( aParam, "Value" );
                        Expression valueConditions = null;

                        // Create a where expression that checks for each value in the list.
                        foreach ( var valueItem in valueList )
                        {
                            var eq = Expression.Equal( valueProperty, Expression.Constant( valueItem ) );
                            var starts = Expression.Call( valueProperty, nameof( string.StartsWith ), null, Expression.Constant( valueItem + "," ) );
                            var ends = Expression.Call( valueProperty, nameof( string.EndsWith ), null, Expression.Constant( "," + valueItem ) );
                            var contains = Expression.Call( valueProperty, nameof( string.Contains ), null, Expression.Constant( "," + valueItem + "," ) );

                            var orBlock = Expression.OrElse(
                                Expression.OrElse( eq, starts ),
                                Expression.OrElse( ends, contains )
                            );

                            valueConditions = valueConditions == null ? orBlock : Expression.OrElse( valueConditions, orBlock );
                        }

                        // a => a.Key == "MultiValue"
                        var keyOnlyLambda = Expression.Lambda<Func<QueryableAttributeValue, bool>>( keyCheck, aParam );

                        // a => a.Key == "MultiValue" && valueConditions
                        var keyAndValueConditions = Expression.AndAlso( keyCheck, valueConditions );
                        var keyAndValueLambda = Expression.Lambda<Func<QueryableAttributeValue, bool>>( keyAndValueConditions, aParam );

                        // gl.Group.GroupAttributeValues
                        var glParam = Expression.Parameter( typeof( GroupLocation ), "gl" );
                        var groupProperty = Expression.Property( glParam, "Group" );
                        var gavProperty = Expression.Property( groupProperty, "GroupAttributeValues" );

                        // !gl.Group.GroupAttributeValues.Any(a => a.Key == "MultiValue")
                        var anyKeyOnlyCall = Expression.Call(
                            typeof( Enumerable ),
                            "Any",
                            new[] { typeof( QueryableAttributeValue ) },
                            gavProperty,
                            keyOnlyLambda
                        );
                        var notAnyKeyOnly = Expression.Not( anyKeyOnlyCall );

                        // gl.Group.GroupAttributeValues.Any(a => a.Key == "MultiValue" && valueConditions)
                        var anyKeyAndValueCall = Expression.Call(
                            typeof( Enumerable ),
                            "Any",
                            new[] { typeof( QueryableAttributeValue ) },
                            gavProperty,
                            keyAndValueLambda
                        );

                        // Combine: !Any(keyOnly) || Any(key && value)
                        var finalExpression = Expression.OrElse( notAnyKeyOnly, anyKeyAndValueCall );
                        var lambda = Expression.Lambda<Func<GroupLocation, bool>>( finalExpression, glParam );

                        // Apply to query
                        return groupQuery.Where( lambda );
                    }
                case "ne":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value != value ) );
                    }
                case "eq":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value == value ) );
                    }
                default:
                    {
                        throw new Exception( "Incorrect filter operator provided. Valid values are eq,ne,sw,ew,con,in." );
                    }
            }
        }

        /// <summary>
        /// Applies the campus filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterCampus( IQueryable<GroupLocation> groupQuery, ChildBlockElement setting, Options options )
        {
            var valueList = setting.Content
                    .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( v => int.Parse( v.Trim() ) )
                    .ToList();

            return groupQuery.Where( gl => gl.Group.CampusId == null || valueList.Contains( gl.Group.CampusId.Value ) );
        }

        /// <summary>
        /// Applies the day of week filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterDayOfWeek( IQueryable<GroupLocation> groupQuery, ChildBlockElement setting, Options options )
        {
            var daysOfWeek = setting.Content
                    .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( s => Enum.TryParse<DayOfWeek>( s.Trim(), true, out var d ) ? d : ( DayOfWeek? ) null )
                    .Where( d => d.HasValue )
                    .Select( d => d.Value )
                    .ToList();

            return groupQuery.Where( g => g.Group.Schedule.WeeklyDayOfWeek.HasValue && daysOfWeek.Contains( g.Group.Schedule.WeeklyDayOfWeek.Value ) );
        }

        /// <summary>
        /// Applies the time of day filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterTimeOfDay( IQueryable<GroupLocation> groupQuery, ChildBlockElement setting, Options options )
        {
            var time = DateTime.Parse( setting.Content );
            var timeSpan = time.TimeOfDay;

            // Default the operator to 'gte' if not specified
            var filterOperator = setting.Parameters.GetValueOrNull( "operator" ) ?? "gte";

            switch ( filterOperator )
            {
                case "lte":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value <= timeSpan );
                    }
                case "lt":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value < timeSpan );
                    }
                case "gt":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value > timeSpan );
                    }
                case "eq":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value == timeSpan );
                    }
                case "ne":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value != timeSpan );
                    }
                case "gte":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value >= timeSpan );
                    }
                default:
                    {
                        throw new Exception( "Incorrect filter operator provided. Valid values are eq,ne,lte,lt,gte,gt." );
                    }
            }
        }

        /// <summary>
        /// Applies the filters for group overcapacity
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterGroupOvercapacity( IQueryable<GroupLocation> groupQuery, Options options )
        {
            // Hide overcapacity groups
            // This hides the groups that are at or over capacity by doing two things:
            // 1) If the group has a GroupCapacity, check that we haven't met or exceeded that.
            // 2) When someone registers for a group on the front-end website, they automatically get added with the group's default
            //    GroupTypeRole. If that role exists and has a MaxCount, check that we haven't met or exceeded it yet.
            if ( options.HideOvercapacityGroups )
            {
                groupQuery = groupQuery.Where(
                    g => g.Group.GroupCapacity == null ||
                    g.Group.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count() < g.Group.GroupCapacity );

                groupQuery = groupQuery.Where( g =>
                     g.Group.GroupType == null ||
                     g.Group.GroupType.DefaultGroupRole == null ||
                     g.Group.GroupType.DefaultGroupRole.MaxCount == null ||
                     g.Group.Members.Where( m => m.GroupRoleId == g.Group.GroupType.DefaultGroupRole.Id ).Count() < g.Group.GroupType.DefaultGroupRole.MaxCount );
            }

            return groupQuery;
        }


        #endregion

        /// <summary>
        /// Appends the travel time and distances to the results.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="travelMode"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        private List<GroupProximityResult> AppendTravelModeDetails( GeographyPoint origin, TravelMode travelMode, List<GroupProximityResult> results )
        {
            // Get driving distances from location extensions
            var destinations = results
                .Where( r => r.Location?.Latitude != null && r.Location?.Longitude != null )
                .Select( r => new GeographyPoint { Latitude = r.Location.Latitude.Value, Longitude = r.Location.Longitude.Value } )
                .ToList();

            var travelDistances = Task.Run( () => GeographyHelpers.GetDrivingMatrixAsync( origin, destinations, travelMode ) ).Result;

            // Merge travel distances into group results
            foreach ( var travelDistance in travelDistances )
            {
                // Find matching group result
                var matches = results.Where( r => r.LocationPoint == travelDistance.DestinationPoint ).ToList();

                foreach ( var match in matches )
                {
                    match.TravelDistanceInMeters = travelDistance.DistanceInMeters;
                    match.TravelTimeInMinutes = travelDistance.TravelTimeInMinutes;
                    match.TravelMode = travelMode;
                }
            }

            return results.OrderBy( g => g.TravelDistanceInMeters ).ToList();
        }

        /// <summary>
        /// Gets the origin point from the settings. If the origin is not a lat/long, it will be geocoded.
        /// </summary>
        /// <param name="originString"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private GeographyPoint GetOriginPoint( string originString, ILavaRenderContext context )
        {
            originString = originString.Trim();

            // If blank then assume current person
            if ( originString.IsNullOrWhiteSpace() )
            {
                originString = GetCurrentPerson( context )?.Id.ToString();
            }

            // Check if it's an int, if so this will be a person id and we'll use their mapped address
            if ( Int32.TryParse( originString, out int personId ) )
            {
                var personLocation = new PersonService( _rockContext ).GetGeopoints( personId )?.FirstOrDefault();

                if ( personLocation == null )
                {
                    return null;
                }
                return GeographyPoint.FromDatabase( personLocation );
            }

            // Check if it's a lat/long if so return it
            if ( GeographyPoint.TryParse( originString, out var point ) )
            {
                return point;
            }

            // To search by postal code the user will append postalcode to the front. This prevents zip codes
            // from being confused with a person id. So we need to remove this so as not to confuse
            // the Google maps API.
            if ( originString.StartsWith( "postalcode" ) )
            {
                originString = originString.Substring( 3 ).Trim();
            }

            // Otherwise, run it through the geocoder
            return Task.Run( () => ( GeographyHelpers.Geocode( originString ) ) ).Result;
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Person GetCurrentPerson( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            if ( context.GetMergeField( "CurrentPerson" ) is Person currentPerson )
            {
                return currentPerson;
            }

            // Next check the RockRequestContext in the lava context.
            if ( context.GetInternalField( "RockRequestContext" ) is RockRequestContext currentRequest )
            {
                return currentRequest.CurrentPerson;
            }

            // Finally check the HttpContext.
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
            {
                return httpContext.Items["CurrentPerson"] as Person;
            }

            return null;
        }

        /// <summary>
        /// Gets the attributes and values from the markup. This ensures that
        /// all parameters exist in the settings.
        /// </summary>
        /// <param name="markup">The markup to be parsed.</param>
        /// <param name="context">The lava render context.</param>
        /// <returns>LavaElementAttributes.</returns>
        private static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Parse attributes string.
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            // Add default settings.
            settings.AddOrIgnore( settings[ParameterKeys.MaxResults], "" );

            return settings;
        }

        /// <summary>
        /// Extracts a set of child elements from the content of the block.
        /// Child elements are grouped by tag name, and each item in the collection has a set of properties
        /// corresponding to the child element tag attributes and a "content" property representing the inner content of the child element.
        /// </summary>
        /// <param name="context">The current lava render context.</param>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="childElements">The child parameters.</param>
        /// <param name="residualBlockContent">The block content that is left over after parsing.</param>
        /// <returns><c>true</c> if the child elements were valid, otherwise <c>false</c>.</returns>
        private bool ExtractBlockChildElements( ILavaRenderContext context, string blockContent, out List<ChildBlockElement> childElements, out string residualBlockContent )
        {
            childElements = new List<ChildBlockElement>();

            var startTagStartExpress = new Regex( @"\[\[\s*" );

            var isValid = true;
            var matchExists = true;
            while ( matchExists )
            {
                var match = startTagStartExpress.Match( blockContent );
                if ( match.Success )
                {
                    int startTagStartIndex = match.Index;

                    // get the name of the parameter
                    var parmNameMatch = new Regex( @"[\w-]*" ).Match( blockContent, startTagStartIndex + match.Length );
                    if ( parmNameMatch.Success )
                    {
                        var parmNameStartIndex = parmNameMatch.Index;
                        var parmNameEndIndex = parmNameStartIndex + parmNameMatch.Length;
                        var parmName = blockContent.Substring( parmNameStartIndex, parmNameMatch.Length );

                        // get end of the tag index
                        var startTagEndIndex = blockContent.IndexOf( "]]", parmNameStartIndex ) + 2;

                        // get the tags parameters
                        var tagParms = blockContent.Substring( parmNameEndIndex, startTagEndIndex - parmNameEndIndex ).Trim();

                        // get the closing tag location
                        var endTagMatchExpression = String.Format( @"\[\[\s*end{0}\s*\]\]", parmName );
                        var endTagMatch = new Regex( endTagMatchExpression ).Match( blockContent, startTagStartIndex );

                        if ( endTagMatch.Success )
                        {
                            var endTagStartIndex = endTagMatch.Index;
                            var endTagEndIndex = endTagStartIndex + endTagMatch.Length;

                            // get the parm content (the string between the two parm tags)
                            var parmContent = blockContent.Substring( startTagEndIndex, endTagStartIndex - startTagEndIndex ).Trim();

                            // Run Lava across the content
                            if ( parmContent.IsNotNullOrWhiteSpace() )
                            {
                                var engine = context.GetService<ILavaEngine>();
                                var renderParameters = new LavaRenderParameters { Context = context };
                                parmContent = engine.RenderTemplate( parmContent, renderParameters ).Text;
                            }

                            var childElement = new ChildBlockElement
                            {
                                Name = parmName,
                                Content = parmContent
                            };

                            // Regex pattern explanation:
                            //
                            //  \S*? Matches any non-whitespace characters (non-greedy) before the colon.
                            //  : Matches the colon character.
                            //  (['"]) Capturing group that matches either a single ' or double " quote. This group is captured as \2 for backreference.
                            //  (.*?): Non-greedy match of any character, capturing as few characters as needed.
                            //  \2: Backreference to the matched quote in (['"]), ensuring the string is closed with the same type of quote.
                            //
                            // This allows for network graph labels that include single quotes, and will match either:
                            //  label:'A/V Team'
                            //  label:"Pete's Group'
                            var parmItems = Regex.Matches( tagParms, @"(\S*?:(['""])(.*?)\2)" )
                                .Cast<Match>()
                                .Select( m => m.Value )
                                .ToList();

                            foreach ( var item in parmItems )
                            {
                                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                                if ( itemParts.Length > 1 )
                                {
                                    childElement.Parameters.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                                }
                            }

                            childElements.Add( childElement );

                            // pull this tag out of the block content
                            blockContent = blockContent.Remove( startTagStartIndex, endTagEndIndex - startTagStartIndex );
                        }
                        else
                        {
                            // there was no matching end tag, for safety sake we'd better bail out of loop
                            isValid = false;
                            matchExists = false;
                            blockContent = blockContent + "Warning: Missing field end tag." + parmName;
                        }
                    }
                    else
                    {
                        // there was no parm name on the tag, for safety sake we'd better bail out of loop
                        isValid = false;
                        matchExists = false;
                        blockContent += "Warning: Field definition does not have any parameters.";
                    }

                }
                else
                {
                    matchExists = false; // we're done here
                }
            }

            residualBlockContent = blockContent.Trim();

            return isValid;
        }
        #endregion

        #region Support Classes

        /// <summary>
        /// The options that can be passed into the network graph.
        /// </summary>
        private class Options
        {
            /// <summary>
            /// The group type id to use for the search.
            /// </summary>
            public string GroupTypeIds { get; set; }

            /// <summary>
            /// List of properties to eager load on the initial query.
            /// </summary>
            public string Include { get; set; }

            /// <summary>
            /// The group type ids in a list to use for the search.
            /// </summary>
            public List<int> GroupTypeIdList
            {
                get
                {
                    if ( GroupTypeIds.IsNullOrWhiteSpace() )
                    {
                        return new List<int>();
                    }
                    return GroupTypeIds.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsInteger() ).ToList();
                }
            }

            /// <summary>
            /// The maximum number of results to return.
            /// </summary>
            public int MaxResults { get; set; }

            /// <summary>
            /// The maximum distance to search for groups in meters.
            /// </summary>
            public int? MaxDistance { get; set; }

            /// <summary>
            /// If true, only the closest location for each group will be returned. Otherwise, all locations for a group will be considered.
            /// </summary>
            public bool ReturnOnlyClosestLocationPerGroup { get; set; }

            /// <summary>
            /// The origin to use for the search.
            /// </summary>
            public string Origin { get; set; }

            /// <summary>
            /// The origin point to use for the search.
            /// </summary>
            public GeographyPoint OriginPoint { get; set; }

            /// <summary>
            /// The travel mode to use for calculating travel mode distances and times.
            /// </summary>
            public TravelMode? TravelMode { get; set; }

            /// <summary>
            /// Determines whether to hide groups that are over their capacity.
            /// </summary>
            public bool HideOvercapacityGroups { get; set; }
        }

        private class ChildBlockElement
        {
            public string Name { get; set; }

            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

            public string Content { get; set; }
        }

        #endregion
    }
}
