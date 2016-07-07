// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Person Select (Family Check-in)")]
    [Category("Check-in")]
    [Description("Lists people who match the selected family and provides option of selecting multiple.")]
    public partial class MultiPersonSelect : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = string.Format( @"
        function GetPersonSelection() {{
            var ids = '';
            $('div.checkin-person-list').find('i.fa-check-square').each( function() {{
                ids += $(this).closest('a').attr('person-id') + ',';
            }});
            if (ids == '') {{
                bootbox.alert('Please select at least one person');
                return false;
            }}
            else
            {{
                $('#{0}').button('loading')
                $('#{1}').val(ids);
                return true;
            }}
        }}

        $('a.btn-checkin-select').click( function() {{
            //$(this).toggleClass('btn-dimmed');
            $(this).find('i').toggleClass('fa-check-square').toggleClass('fa-square-o');
        }});

", lbSelect.ClientID, hfPeople.ClientID );
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "SelectPerson", script, true );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
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

                    var family = CurrentCheckInState.CheckIn.CurrentFamily;
                    if ( family == null )
                    {
                        GoBack();
                    }

                    lFamilyName.Text = family.ToString();

                    rSelection.DataSource = family.People
                        .OrderByDescending( p => p.FamilyMember )
                        .ThenBy( p => p.Person.BirthYear )
                        .ThenBy( p => p.Person.BirthMonth )
                        .ThenBy( p => p.Person.BirthDay )
                        .ToList();

                    rSelection.DataBind();

                }
            }
        }

        /// <summary>
        /// Clear any previously selected people.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People )
                {
                    person.ClearFilteredExclusions();
                    person.PossibleSchedules = new List<CheckInSchedule>();
                    person.Selected = false;
                    person.Processed = false;
                }
            }
        }

        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var selectedPersonIds = hfPeople.Value.SplitDelimitedValues().AsIntegerList();

                var family = CurrentCheckInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    foreach ( var person in family.People )
                    {
                        person.Selected = selectedPersonIds.Contains( person.Person.Id );
                        person.PreSelected = person.Selected;
                    }

                    ProcessSelection( maWarning );
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

        protected void ProcessSelection()
        {
            ProcessSelection( 
                maWarning, 
                () => CurrentCheckInState.CheckIn.CurrentFamily.GetPeople( true )
                    .SelectMany( p => p.GroupTypes.Where( t => !t.ExcludedByFilter ) ) 
                    .Count() <= 0,
                "<p>Sorry, based on your selection, there are currently not any available locations that can be checked into.</p>" );
        }

        protected string GetCheckboxClass( bool selected )
        {
            return selected ? "fa fa-check-square fa-3x" : "fa fa-square-o fa-3x";
        }

        protected string GetPersonImageTag( object dataitem )
        {
            var person = dataitem as Person;
            if ( person != null )
            {
                return Person.GetPersonPhotoUrl( person, 200, 200 );
            }
            return string.Empty;
        }

    }
}