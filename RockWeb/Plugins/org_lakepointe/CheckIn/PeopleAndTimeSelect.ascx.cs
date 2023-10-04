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

// This is forked from the core TimeSelect block. The principle difference is that the core block
// finds all schedules that anyone in the "family" can potentially check into--as one list of schedules.
// This version keeps separate lists for each family member so we can let them select who will be
// checking in for which hours instead of assuming all family members will be checking into something
// at each hour. The core approach sometimes checked adults into volunteer roles because they were
// checking their kids into children's programming.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Plugins.org_lakepointe.Checkin
{
    [DisplayName("People And Time Select")]
    [Category("LPC > Check-in")]
    [Description("Displays a list of people and times to check in for.")]

    [TextField( "Caption",
        Key = AttributeKey.Caption,
        IsRequired = false,
        DefaultValue = "Select Time(s)",
        Category = "Text",
        Order = 5 )]

    [TextField( "No Check-in Options Message",
        Key = AttributeKey.NoCheckinOptionsMessage,
        Description = "Message to display when there are not any schedule times after a person is selected. Use {0} for person's name",
        IsRequired = false,
        DefaultValue = "Sorry, there are currently not any available times that {0} can check into.",
        Category = "Text",
        Order = 6 )]

    // [Rock.SystemGuid.BlockTypeGuid( "CA184D55-0B17-46E4-94CA-649F9A1BD741" )]
    public partial class PeopleAndTimeSelect : CheckInBlock
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlock also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string Caption = "Caption";
            public const string NoCheckinOptionsMessage = "NoCheckinOptionsMessage";
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    ClearSelection();

                    var personSchedules = new List<CheckInSchedule>();
                    var distinctSchedules = new List<CheckInSchedule>();
                    if ( CurrentCheckInType != null && CurrentCheckInType.TypeOfCheckin == TypeOfCheckin.Family )
                    {
                        CheckInFamily family = CurrentCheckInState.CheckIn.CurrentFamily;
                        if ( family != null )
                        {
                            var schedules = family.GetPeople( false ).SelectMany( p => p.PossibleSchedules ).OrderBy(s => s.StartTime).ToList();
                            if ( !schedules.Any() )
                            {
                                CheckInPerson person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).SelectMany( f => f.People ).FirstOrDefault();
                                string msg = $"<p>{string.Format( GetAttributeValue( AttributeKey.NoCheckinOptionsMessage ), person?.Person.FullName ?? string.Empty )}</p>" ;
                                ProcessSelection( maWarning, () => schedules.Count > 0, msg, true );
                            }

                            foreach ( var schedule in schedules )
                            {
                                personSchedules.Add( schedule );
                                if ( !distinctSchedules.Any( s => s.Schedule.Id == schedule.Schedule.Id ) )
                                {
                                    distinctSchedules.Add( schedule );
                                }
                            }
                        }
                        else
                        {
                            GoBack();
                        }

                        lTitle.Text = GetTitleText();
                        lbSelect.Text = "Next";
                        lbSelect.Attributes.Add( "data-loading-text", "Loading..." );
                    }
                    else // LPC: This paragraph has not been reviewed or updated post-fork as we're focused on Family Check-in (above)
                    { 
                        CheckInPerson person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).SelectMany( f => f.People ).FirstOrDefault();
                        CheckInGroup group = null;
                        CheckInLocation location = null;

                        if ( person != null )
                        {
                            group = person.GroupTypes.Where( t => t.Selected ).SelectMany( t => t.Groups.Where( g => g.Selected ) ).FirstOrDefault();

                            if ( group != null )
                            {
                                location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
                            }
                        }

                        if ( location == null )
                        {
                            GoBack();
                        }

                        lTitle.Text = GetTitleText();
                        lbSelect.Text = "Check In";
                        lbSelect.Attributes.Add( "data-loading-text", "Printing..." );

                        personSchedules = location.Schedules.Where( s => !s.ExcludedByFilter ).ToList();
                        distinctSchedules = personSchedules;
                    }

                    lCaption.Text = GetAttributeValue( AttributeKey.Caption );

                    // ::: Are there still cases where it makes sense to skip this screen?
                    //if ( distinctSchedules.Count == 1 )
                    //{
                    //    personSchedules.ForEach( s => s.Selected = true );
                    //    ProcessSelection( maWarning );
                    //}
                    //else
                    {
                        var scheduleOptions = new List<PersonScheduleOptions>();
                        foreach ( var checkinPerson in CurrentCheckInState.CheckIn.CurrentFamily.GetPeople( false ) )
                        {
                            var personScheduleOptions = new PersonScheduleOptions( checkinPerson.Person.FullName, checkinPerson.Person.Id, distinctSchedules.Count );
                            foreach ( var schedule in checkinPerson.PossibleSchedules )
                            {
                                personScheduleOptions.Schedules[ distinctSchedules.IndexOf( distinctSchedules.Where( s => s.Schedule.Id == schedule.Schedule.Id ).FirstOrDefault() ) ] = schedule;
                            }
                            scheduleOptions.Add( personScheduleOptions );
                        }

                        if ( scheduleOptions.Count > 0 )
                        {
                            string script = string.Format( @"
        <script>
            function GetTimeSelection() {{
                var ids = '';
                $('div.checkin-timelist button.active').each( function() {{
                    ids += $(this).attr('schedule-id') + '|' + $(this).attr('person-id') + ',';
                }});
                if (ids == '') {{
                    bootbox.alert('Please select at least one time');
                    return false;
                }}
                else
                {{
                    $('#{0}').button('loading')
                    $('#{1}').val(ids);
                    return true;
                }}
            }}
        </script>
    ", lbSelect.ClientID, hfTimes.ClientID );
                            Page.ClientScript.RegisterClientScriptBlock( this.GetType(), "SelectTime", script );

                            gSelection.DataSource = scheduleOptions.Select( s => new ViewModelPersonScheduleOptions() { PersonScheduleOptions = s } );
                            gSelection.DataBind();

                            if ( gSelection.Rows.Count > 0 )
                            {
                                //var headers = scheduleOptions.Select( s => s.Schedules.Where( ss => ss != null ).FirstOrDefault() ).Select( sss => sss.Schedule.Name ).ToList(); //?.Schedule.Name ?? "Other" ).ToList();
                                //var headers = scheduleOptions.Select( s => s.Schedules.Where( ss => ss != null ).FirstOrDefault()?.Schedule.Name ?? "Other" ).ToList();
                                for ( int i = 0; i < 4; i++ )
                                {
                                    // +1 here is because first column is the person's name
                                    if ( scheduleOptions[0].Schedules.Length > i )
                                    {
                                        gSelection.HeaderRow.Cells[i + 1].Text = scheduleOptions.Where( s => s.Schedules[i] != null ).FirstOrDefault()?.Schedules[i].Schedule.Name ?? "Other";
                                        gSelection.Columns[i + 1].ShowHeader = true;
                                        gSelection.Columns[i + 1].Visible = true;
                                    }
                                    else
                                    {
                                        gSelection.Columns[i + 1].Visible = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears any previously selected schedules.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People )
                {
                    foreach ( var schedule in person.PossibleSchedules )
                    {
                        schedule.Selected = false;
                        schedule.Processed = false;
                    }

                    foreach ( var groupType in person.GroupTypes )
                    {
                        foreach ( var group in groupType.Groups )
                        {
                            foreach ( var location in group.Locations )
                            {
                                foreach ( var schedule in location.Schedules )
                                {
                                    schedule.Selected = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GetTitleText()
        {
            // The checkinPerson, selectedGroup, and selectedLocation are only needed for individual checkins, so no use running the queries if this is a mutli person checkin.
            var checkinPerson = CurrentCheckInType.TypeOfCheckin == TypeOfCheckin.Individual
                ? CurrentCheckInState.CheckIn.Families
                    .Where( f => f.Selected )
                    .SelectMany( f => f.People.Where( p => p.Selected ) )
                    .FirstOrDefault()
                : null;

            var selectedGroup = checkinPerson?.GroupTypes
                .Where( t => t.Selected )
                .SelectMany( t => t.Groups.Where( g => g.Selected ) )
                .FirstOrDefault();

            var selectedLocation = selectedGroup?.Locations.Where( l => l.Selected ).FirstOrDefault()?.Location;

            var selectedIndividuals = CurrentCheckInState.CheckIn.CurrentFamily.People.Where( p => p.Selected == true ).Select( p => p.Person );
            
            var mergeFields = new Dictionary<string, object>
            {
                { LavaMergeFieldName.Family, CurrentCheckInState.CheckIn.CurrentFamily.Group },
                { LavaMergeFieldName.SelectedIndividuals, selectedIndividuals },
                { LavaMergeFieldName.CheckinType, CurrentCheckInType.TypeOfCheckin },
                { LavaMergeFieldName.SelectedGroup, selectedGroup?.Group },
                { LavaMergeFieldName.SelectedLocation, selectedLocation },
            };

            var timeSelectHeaderLavaTemplate = CurrentCheckInState.CheckInType.TimeSelectHeaderLavaTemplate ?? string.Empty;
            return timeSelectHeaderLavaTemplate.ResolveMergeFields( mergeFields );
        }

        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                // Select people
                var family = CurrentCheckInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var selectedPersonIds = hfTimes.Value.Split( ',' ).Select( x => x.Split( '|' ).Last().AsInteger() ).Distinct();
                    foreach ( var person in family.People )
                    {
                        person.Selected = selectedPersonIds.Contains( person.Person.Id );
                        person.PersonSelectedSchedules = person.Selected ? new List<CheckInSchedule>() : null;
                    }
                }

                // Select schedules
                var something = false;
                foreach (var selection in hfTimes.Value.Split( ',' ) )
                {
                    if ( selection == "" )
                    {
                        continue;
                    }

                    var parts = selection.Split( '|' );
                    var scheduleId = parts[0].AsInteger();
                    var personId = parts[1].AsInteger();
                    var schedule = family.GetPeople( false )
                        .Where( p => p.Person.Id == personId ).FirstOrDefault()
                        .PossibleSchedules
                        .Where( s => s.Schedule.Id == scheduleId ).FirstOrDefault();
                    schedule.Selected = true;

                    var person = family.GetPeople( false )
                        .Where( p => p.Person.Id == personId ).FirstOrDefault();
                    person.PersonSelectedSchedules.Add( schedule );

                    something = true;
                }

                if ( something )
                {
                    ProcessSelection( maWarning, true );
                }
            }
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        public class PersonScheduleOptions
        {
            public PersonScheduleOptions( string name, int personId, int capacity )
            {
                Name = name;
                PersonId = personId;
                Schedules = new CheckInSchedule[capacity];
            }

            public string Name { get; set; }
            public int PersonId { get; set; }
            public CheckInSchedule[] Schedules { get; private set; }
        }

        public class ViewModelPersonScheduleOptions
        {
            public PersonScheduleOptions PersonScheduleOptions { get; set; }

            public string Name => PersonScheduleOptions.Name;
            public int PersonId => PersonScheduleOptions.PersonId;

            public CheckInSchedule Schedule0 => PersonScheduleOptions.Schedules.Length < 1 ? null : PersonScheduleOptions.Schedules[0];
            public CheckInSchedule Schedule1 => PersonScheduleOptions.Schedules.Length < 2 ? null : PersonScheduleOptions.Schedules[1];
            public CheckInSchedule Schedule2 => PersonScheduleOptions.Schedules.Length < 3 ? null : PersonScheduleOptions.Schedules[2];
            public CheckInSchedule Schedule3 => PersonScheduleOptions.Schedules.Length < 4 ? null : PersonScheduleOptions.Schedules[3];
        }
    }
}