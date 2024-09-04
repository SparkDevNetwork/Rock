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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI.Controls;
namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Registration List Lava" )]
    [Category( "Event" )]
    [Description( "List recent registrations using a Lava template." )]

    [CodeEditorField( "Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/RegistrationListSidebar.lava' %}", "", 2, "LavaTemplate" )]
    [IntegerField( "Max Results", "The maximum number of results to display.", false, 5, order: 3 )]
    [SlidingDateRangeField( "Date Range", "Date range to limit by.", false, "", enabledSlidingDateRangeTypes: "Previous, Last, Current, Next, Upcoming, DateRange", order: 7 )]
    [BooleanField( "Limit to registrations where money is still owed", "", true, "", 8, "LimitToOwed" )]
    [Rock.SystemGuid.BlockTypeGuid( "92E4BFE8-DF80-49D7-819D-417E579E282D" )]
    public partial class RegistrationListLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                LoadContent();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected void LoadContent()
        {
            RockContext rockContext = new RockContext();

            var registrationService = new RegistrationService( rockContext );
            var qryRegistrations = registrationService.Queryable();

            // only show Active registrations
            qryRegistrations = qryRegistrations
                .Where( a =>
                    a.RegistrationInstance.IsActive == true &&
                    !a.IsTemporary );

            // limit to the current person
            int currentPersonId = this.CurrentPersonId ?? 0;
            qryRegistrations = qryRegistrations.Where( a => a.PersonAlias.PersonId == currentPersonId );

            // bring into a list so we can filter on non-database columns
            var registrationList = qryRegistrations.ToList();

            List<Registration> hasDates = registrationList.Where( a => a.RegistrationInstance.Linkages.Any( x => x.EventItemOccurrenceId.HasValue && x.EventItemOccurrence.NextStartDateTime.HasValue ) ).ToList();
            List<Registration> noDates = registrationList.Where( a => !hasDates.Any( d => d.Id == a.Id ) ).OrderBy( x => x.RegistrationInstance.Name ).ToList();

            hasDates = hasDates
                .OrderBy( a => a.RegistrationInstance.Linkages
                     .Where( x => x.EventItemOccurrenceId.HasValue && x.EventItemOccurrence.NextStartDateTime.HasValue )
                     .OrderBy( b => b.EventItemOccurrence.NextStartDateTime )
                     .FirstOrDefault()
                     .EventItemOccurrence.NextStartDateTime )
                .ToList();

            // filter by date range
            var requestDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "DateRange" ) ?? "-1||" );
            if ( requestDateRange.Start.HasValue )
            {
                hasDates = hasDates
                    .Where( a => a.RegistrationInstance.Linkages
                        .Where( x => x.EventItemOccurrenceId.HasValue && x.EventItemOccurrence.NextStartDateTime.HasValue )
                        .OrderBy( b => b.EventItemOccurrence.NextStartDateTime )
                        .FirstOrDefault()
                        .EventItemOccurrence.NextStartDateTime >= requestDateRange.Start )
                    .ToList();
            }

            if ( requestDateRange.End.HasValue )
            {
                hasDates = hasDates
                    .Where( a => a.RegistrationInstance.Linkages
                        .Where( x => x.EventItemOccurrenceId.HasValue && x.EventItemOccurrence.NextStartDateTime.HasValue )
                        .OrderBy( b => b.EventItemOccurrence.NextStartDateTime )
                        .FirstOrDefault()
                        .EventItemOccurrence.NextStartDateTime < requestDateRange.End )
                    .ToList();
            }

            registrationList = hasDates;

            if ( this.GetAttributeValue( "LimitToOwed" ).AsBooleanOrNull() ?? true )
            {
                registrationList.AddRange( noDates );
                registrationList = registrationList.Where( a => a.BalanceDue != 0 ).ToList();
            }

            int? maxResults = GetAttributeValue( "MaxResults" ).AsIntegerOrNull();
            if ( maxResults.HasValue && maxResults > 0 )
            {
                registrationList = registrationList.Take( maxResults.Value ).ToList();
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Registrations", registrationList );

            string template = GetAttributeValue( "LavaTemplate" );
            lContent.Text = template.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}