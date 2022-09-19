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
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotLiquid;

using Rock.Data;
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
        Documentation = ScheduledContentShortcode.DocumentationMetadata,
        Parameters = "scheduleid,showwhen,roleid",
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
    public class ScheduledContent : RockLavaShortcodeBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;
        string _enabledSecurityCommands = string.Empty;
        StringBuilder _blockMarkup = new StringBuilder();
        string _tagName = "scheduledcontent";

        // Keys
        const string SHOW_WHEN = "showwhen";
        const string SCHEDULE_ID = "scheduleid";
        const string ROLE_ID = "roleid";
        const string LOOK_AHEAD_DAYS = "lookaheaddays";
        const string SCHEDULE_CATEGORY_ID = "schedulecategoryid";
        const string AS_AT_DATE = "asatdate";

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterShortcode<ScheduledContent>( _tagName );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Parses the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        protected override void Parse( List<string> tokens )
        {
            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.

            // We must take into consideration nested tags of the same type

            var endTagFound = false;

            var startTag = $@"{{\[\s*{ _tagName }\s*\]}}";
            var endTag = $@"{{\[\s*end{ _tagName }\s*\]}}";

            var childTags = 0;

            Regex regExStart = new Regex( startTag );
            Regex regExEnd = new Regex( endTag );

            NodeList = NodeList ?? new List<object>();
            NodeList.Clear();

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
                        if (childTags > 0 )
                        {
                            childTags--; // decrement the child tag counter
                            _blockMarkup.Append( token );
                        }
                        else
                        {
                            endTagFound = true;
                            break;
                        }
                    }
                    else
                    {
                        _blockMarkup.Append( token );
                    }
                }
            }

            if ( !endTagFound )
            {
                AssertMissingDelimitation();
            }
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            var rockContext = new RockContext();

            // Get enabled security commands
            if ( context.Registers.ContainsKey( "EnabledCommands" ) )
            {
                _enabledSecurityCommands = context.Registers["EnabledCommands"].ToString();
            }

            using ( TextWriter writer = new StringWriter() )
            {
                bool filterProvided = false;

                base.Render( context, writer );

                var parms = ParseMarkup( _markup, context );
                var lookAheadDays = parms[ LOOK_AHEAD_DAYS ].AsInteger();
                var scheduleCategoryId = parms[ SCHEDULE_CATEGORY_ID ].AsIntegerOrNull();
                var asAtDate = parms[AS_AT_DATE].AsDateTime();

                var now = asAtDate ?? RockDateTime.Now;

                var scheduleIds = new List<int>();

                var requestedSchedules = parms[SCHEDULE_ID].StringToIntList();

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
                                .Where( s => s.GetNextStartDateTime( now ) != null )
                                .OrderBy( s => s.GetNextStartDateTime( now ) );

                if ( schedules.Count() == 0 )
                {
                    return;
                }

                var nextSchedule = schedules.FirstOrDefault();

                var nextStartDateTime = nextSchedule.GetNextStartDateTime( now );
                var isLive = false;
                DateTime? occurrenceEndDateTime = null;

                // Determine if we're live
                if ( nextSchedule.WasScheduleActive( now ) )
                {
                    isLive = true;
                    var occurrences = nextSchedule.GetICalOccurrences( now, now.AddDays( lookAheadDays ) ).Take(2);
                    var activeOccurrence = occurrences.FirstOrDefault();
                    occurrenceEndDateTime = (DateTime) activeOccurrence.Period.EndTime.Value;

                    // Set the next occurrence to be the literal next occurrence (vs the current occurrence)
                    nextStartDateTime = null;
                    if ( occurrences.Count() > 1 )
                    {
                        nextStartDateTime = occurrences.Last().Period.EndTime.Value;
                    }
                }

                // Determine when not to show the content
                if ( ( parms[ SHOW_WHEN ] == "notlive" && isLive )
                    || ( parms[ SHOW_WHEN ] == "live" && !isLive ) )
                {
                    return;
                }

                // Check role membership
                var roleId = parms[ ROLE_ID ].AsIntegerOrNull();

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

                var mergeFields = LoadBlockMergeFields( context );
                mergeFields.Add( "NextOccurrenceDateTime", nextStartDateTime );
                mergeFields.Add( "OccurrenceEndDateTime", occurrenceEndDateTime );
                mergeFields.Add( "Schedule", nextSchedule );
                mergeFields.Add( "IsLive", isLive );

                var results = _blockMarkup.ToString().ResolveMergeFields( mergeFields, _enabledSecurityCommands );
                result.Write( results.Trim() );
                base.Render( context, result );
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( Context context )
        {
            Person currentPerson = null;

            // First check for a person override value included in lava context
            if ( context.Scopes != null )
            {
                foreach ( var scopeHash in context.Scopes )
                {
                    if ( scopeHash.ContainsKey( "CurrentPerson" ) )
                    {
                        currentPerson = scopeHash["CurrentPerson"] as Person;
                    }
                }
            }

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

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup, Context context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = new Dictionary<string, object>();

            // get variables defined in the lava source
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            // get merge fields loaded by the block or container
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }
            var resolvedMarkup = markup.ResolveMergeFields( internalMergeFields );

            var parms = new Dictionary<string, string>();
            parms.Add( SCHEDULE_ID, "" );
            parms.Add( ROLE_ID, "" );
            parms.Add( SHOW_WHEN, "live" );
            parms.Add( LOOK_AHEAD_DAYS, "30" );
            parms.Add( SCHEDULE_CATEGORY_ID, "" );
            parms.Add( AS_AT_DATE, "" );

            var markupItems = Regex.Matches( resolvedMarkup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }
            return parms;
        }


        /// <summary>
        /// Loads the block merge fields.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, object> LoadBlockMergeFields( Context context )
        {
            var _internalMergeFields = new Dictionary<string, object>();

            // Get merge fields loaded by the block or container
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    _internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            return _internalMergeFields;
        }
    }
}