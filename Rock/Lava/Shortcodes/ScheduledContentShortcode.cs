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
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rock.Model;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying content at scheduled times.
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Scheduled Content",
        TagName = "scheduledcontent",
        Description = "The scheduled content shortcode will show/hide a block of content based on a provided Rock schedule.",
        Documentation = DocumentationMetadata,
        Parameters = "scheduleid,showwhen,roleid",
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
    public class ScheduledContentShortcode : LavaShortcodeBase, ILavaBlock
    {
        string _markup = string.Empty;
        string _enabledSecurityCommands = string.Empty;
        StringBuilder _blockMarkup = new StringBuilder();

        // Keys
        const string SHOW_WHEN = "showwhen";
        const string SCHEDULE_ID = "scheduleid";
        const string ROLE_ID = "roleid";
        const string LOOK_AHEAD_DAYS = "lookaheaddays";
        const string SCHEDULE_CATEGORY_ID = "schedulecategoryid";
        const string AS_AT_DATE = "asatdate";

        internal const string DocumentationMetadata = @"<p>Rock's schedules are a powerful tool in determining when certain events occur. The scheduled content
Lava shortcode allows you to extend the power of schedules to show/hide content. Below is a description
of how it works.</p>

            <pre>{[ scheduledcontent scheduleid:'15' ]}
    {{ CurrentPerson.NickName }}, we're live!
 {[ endscheduledcontent ]}</pre>

         <p>Let's take a look at some of the parameters and options that are available.</p>

         <ul>
            <li><strong>scheduleid</strong> - The schedule id to use for determining if the content should be displayed. This can be a single schedule or a comma-separated list of schedules.</li>
            <li><strong>schedulecategoryid</strong> - The schedule category id to use for determining if the content should be displayed. All schedules in the category will be considered.</li>
            <li><strong>showwhen</strong> (live) - Determines when the content should be displayed. Valid values are 'live', 'notlive' and 'both'.</li>
            <li><strong>roleid</strong> - An optional parameter to limit the display to only people in a specified role (actually, any group id will work.)</li>
            <li><strong>lookaheaddays</strong> (30)- The number of days to look ahead to find the next occurrence.</li>
         </ul>

         <p>The 'scheduleid' and 'schedulecategoryid' settings are meant to be either or. If you provide a value for one you don't need to provide a value for the other.</p>

         <p>The following merge fields are available to the contents of your shortcode to help empower your user experience.</p>
         <ul>
            <li><strong>IsLive</strong> - Determines if the schedule is currently live. This is helpful when you set the 'showwhen' to 'both' as you can now display a different message by using this merge field in a simple if statement.</li>
            <li><strong>OccurrenceEndDateTime</strong> - When a schedule is live this field will provide the date/time when the schedule will no longer be active. This helps assist you in creating a countdown to finish counter.</li>
            <li><strong>NextOccurrenceDateTime</strong> - This is the date time of the next occurrence. This is provided to help you create a countdown to start counter. When a schedule is live this value will be the next active occurrence to help you craft messaging as to when you can see the next full occurrence.</li>
            <li><strong>Schedule</strong> - This is the schedule object related to the request. If the event is live it will be the active schedule otherwise it will be the upcoming schedule.</li>
         </ul>";

        /// <summary>
        /// Specifies the type of Liquid element for this shortcode.
        /// </summary>
        public override LavaShortcodeTypeSpecifier ElementType
        {
            get
            {
                return LavaShortcodeTypeSpecifier.Block;
            }
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.OnInitialize( tagName, markup, tokens );

            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.

            // We must take into consideration nested tags of the same type
            var startTag = $@"{{\[\s*{ this.SourceElementName }\s*\]}}";
            var endTag = $@"{{\[\s*end{ this.SourceElementName }\s*\]}}";

            var childTags = 0;

            Regex regExStart = new Regex( startTag );
            Regex regExEnd = new Regex( endTag );

            string token;
            while ( ( token = tokens.Shift() ) != null )
            {

                Match startTagMatch = regExStart.Match( token );
                if ( startTagMatch.Success )
                {
                    childTags++; // increment the child tag counter
                    _blockMarkup.Append( token );
                }
                else
                {
                    Match endTagMatch = regExEnd.Match( token );

                    if ( endTagMatch.Success )
                    {
                        if ( childTags > 0 )
                        {
                            childTags--; // decrement the child tag counter
                            _blockMarkup.Append( token );
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        _blockMarkup.Append( token );
                    }
                }
            }

            if ( childTags > 0 )
            {
                AssertMissingDelimitation();
            }
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            // Get enabled security commands
            _enabledSecurityCommands = context.GetEnabledCommands().JoinStrings( "," );

            using ( TextWriter writer = new StringWriter() )
            {
                bool filterProvided = false;

                base.OnRender( context, writer );

                var settings = LavaElementAttributes.NewFromMarkup( _markup, context );

                var lookAheadDays = settings.GetInteger( LOOK_AHEAD_DAYS, 30 );
                var scheduleCategoryId = settings.GetIntegerOrNull( SCHEDULE_CATEGORY_ID );
                var asAtDate = settings.GetDateTime( AS_AT_DATE, RockDateTime.Now );

                var scheduleIds = new List<int>();

                var requestedSchedules = settings.GetString( SCHEDULE_ID ).StringToIntList();

                var schedulesQry = new ScheduleService( rockContext ).Queryable().AsNoTracking()
                    .Where( s => s.IsActive == true );

                if ( requestedSchedules.Count() > 0 )
                {
                    schedulesQry = schedulesQry.Where( s => requestedSchedules.Contains( s.Id ) );
                    filterProvided = true;
                }

                if ( scheduleCategoryId.HasValue )
                {
                    schedulesQry = schedulesQry.Where( s => s.CategoryId == scheduleCategoryId.Value );
                    filterProvided = true;
                }

                // If neither a schedule id nor a schedule category id was provided stop
                if ( !filterProvided )
                {
                    return;
                }

                // Get the schedules are order them by the next start time
                var schedules = schedulesQry.ToList()
                                .Where( s => s.GetNextStartDateTime( asAtDate ) != null )
                                .OrderBy( s => s.GetNextStartDateTime( asAtDate ) );

                if ( schedules.Count() == 0 )
                {
                    return;
                }

                var nextSchedule = schedules.FirstOrDefault();

                var nextStartDateTime = nextSchedule.GetNextStartDateTime( asAtDate );
                var isLive = false;
                DateTime? occurrenceEndDateTime = null;

                // Determine if we're live
                if ( nextSchedule.WasScheduleActive( asAtDate ) )
                {
                    isLive = true;
                    var occurrences = nextSchedule.GetICalOccurrences( asAtDate, asAtDate.AddDays( lookAheadDays ) ).Take( 2 );
                    var activeOccurrence = occurrences.FirstOrDefault();
                    occurrenceEndDateTime = ( DateTime ) activeOccurrence.Period.EndTime.Value;

                    // Set the next occurrence to be the literal next occurrence (vs the current occurrence)
                    nextStartDateTime = null;
                    if ( occurrences.Count() > 1 )
                    {
                        nextStartDateTime = occurrences.Last().Period.EndTime.Value;
                    }
                }

                // Determine if the content should be shown for the current schedule status.
                // The default behavior is to show content when the schedule is Live.
                var showWhen = settings.GetString( SHOW_WHEN, "live" );
                if ( ( showWhen == "notlive" && isLive )
                    || ( showWhen == "live" && !isLive ) )
                {
                    return;
                }

                // Check role membership
                var roleId = settings.GetIntegerOrNull( ROLE_ID );

                if ( roleId.HasValue )
                {
                    var currentPerson = GetCurrentPerson( context );

                    var isInRole = new GroupMemberService( rockContext ).Queryable()
                                    .Where( m =>
                                                m.GroupId == roleId
                                                && m.PersonId == currentPerson.Id
                                            )
                                    .Any();

                    if ( !isInRole )
                    {
                        return;
                    }
                }

                var mergeFields = context.GetMergeFields();

                mergeFields.Add( "NextOccurrenceDateTime", nextStartDateTime );
                mergeFields.Add( "OccurrenceEndDateTime", occurrenceEndDateTime );
                mergeFields.Add( "Schedule", nextSchedule );
                mergeFields.Add( "IsLive", isLive );

                var engine = context.GetService<ILavaEngine>();

                var renderContext = engine.NewRenderContext( mergeFields, _enabledSecurityCommands.SplitDelimitedValues() );

                var results = engine.RenderTemplate( _blockMarkup.ToString(), LavaRenderParameters.WithContext( renderContext ) );

                result.Write( results.Text.Trim() );
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            var currentPerson = context.GetMergeField( "CurrentPerson", null ) as Person;

            if ( currentPerson == null )
            {
                var httpContext = System.Web.HttpContext.Current;
                if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
        }
    }
}