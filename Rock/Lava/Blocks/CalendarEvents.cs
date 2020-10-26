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

using DotLiquid;
using Rock.Model;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// A Lava Block that provides access to a filtered set of events from a specified calendar.
    /// Lava objects are created in the block context to provide access to the set of events matching the filter parmeters.
    /// The <c>EventItems</c> collection contains information about the Event instances.
    /// The <c>EventItemOccurrences</c> collection contains the actual occurrences of the event that match the filter.
    /// </summary>
    public class CalendarEvents : RockLavaBlockBase
    {
        /// <summary>
        /// The name of the element as it is used in the source document.
        /// </summary>
        public static readonly string TagSourceName = "calendarevents";

        private string _attributesMarkup;
        private bool _renderErrors = true;

        LavaElementAttributes _settings = new LavaElementAttributes();

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterTag<CalendarEvents>( TagSourceName );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            try
            {
                var dataSource = new EventOccurrencesLavaDataSource();

                _settings.ParseFromMarkup( _attributesMarkup, context );

                var events = dataSource.GetEventOccurrencesForCalendar( _settings );

                AddLavaMergeFieldsToContext( context, events );

                RenderAll( this.NodeList, context, result );
            }
            catch ( Exception ex )
            {
                var message = "Calendar Events not available. " + ex.Message;

                if ( _renderErrors )
                {
                    result.Write( message );
                }
                else
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        private void AddLavaMergeFieldsToContext( Context context, List<EventOccurrenceSummary> eventOccurrenceSummaries )
        {
            var eventSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .GroupBy( e => e.Name )
                .Select( e => e.ToList() )
                .ToList();

            eventOccurrenceSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Name )
                .ToList();

            context["EventItems"] = eventSummaries;
            context["EventItemOccurrences"] = eventOccurrenceSummaries;
        }
    }
}
