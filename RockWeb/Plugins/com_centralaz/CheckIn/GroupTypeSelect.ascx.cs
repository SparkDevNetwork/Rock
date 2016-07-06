// <copyright>
// Copyright by Central Christian Church
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
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    [DisplayName("Group Type Select")]
    [Category( "com_centralaz > Check-in" )]
    [Description("Displays a list of group types the person is configured to checkin to.")]
    [Obsolete( "Deprecated.  We'll be moving back to most of Rock's core check-in blocks with the exception of Admin.ascx and Success.ascx" )]
    public partial class GroupTypeSelect : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/CheckIn/Scripts/checkin-core.js" );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    ClearSelection();
                    SetSelectedPeopleInHiddenList();
                    SetupSelectionScreenForNextPerson();
                }
            }
        }

        /// <summary>
        /// Set each selected person into the list of people to be processed.
        /// This is a queue that will drain as we process each person.
        /// </summary>
        private void SetSelectedPeopleInHiddenList()
        {
            var ids = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .SelectMany( f => f.People.Where( p => p.Selected ) )
                        .Select( p=>p.Person.Id.ToString() ).ToArray();

            hfPeopleToProcess.Value = string.Join( ",", ids );
        }

        /// <summary>
        /// Builds the selection screen for the next person who needs it
        /// and returns how many people remain to be processed.
        /// When no more remain it will call ProcessSelection()
        /// </summary>
        private void SetupSelectionScreenForNextPerson()
        {
            // if there are people to process, then process them
            if ( !string.IsNullOrEmpty( hfPeopleToProcess.Value ) )
            {
                Queue<string> ids = new Queue<string>( hfPeopleToProcess.Value.SplitDelimitedValues() );

                // Process each person in the stack until there are no more.
                while ( ids.Count > 0 )
                {
                    int personId = ids.Dequeue().AsInteger();
                    hfPeopleToProcess.Value = string.Join( ",", ids );
                    hfPerson.Value = personId.ToString();

                    var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .SelectMany( f => f.People.Where( p => p.Selected && p.Person.Id == personId ) )
                        .FirstOrDefault();

                    if ( person == null )
                    {
                        GoBack();
                    }

                    // Find available GroupTypes, if only one select it and move to the next person.
                    var availGroupTypes = person.GroupTypes.Where( t => !t.ExcludedByFilter ).ToList();
                    if ( availGroupTypes.Count == 1 )
                    {
                        // ??
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            availGroupTypes.FirstOrDefault().Selected = true;
                        }
                    }
                    else
                    {
                        // Since there are more than one, we need to ask the user which one they want
                        lPersonName.Text = person.Person.FullName;
                        rSelection.DataSource = availGroupTypes;
                        rSelection.DataBind();
                        return;
                    }
                }
            }

            // No more people, then continue to next step
            ProcessSelection();
        }

        /// <summary>
        /// Clear any previously group types people.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People )
                {
                    foreach ( var groupType in person.GroupTypes )
                    {
                        groupType.Selected = false;
                    }
                }
            }
        }

        /// <summary>
        /// Process save the user selected GroupType for the person
        /// being processed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var personId = hfPerson.ValueAsInt();
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                    .SelectMany( f => f.People.Where( p => p.Selected && p.Person.Id == personId ) )
                    .FirstOrDefault();

                if ( person != null )
                {
                    int id = Int32.Parse( e.CommandArgument.ToString() );
                    var groupType = person.GroupTypes.Where( g => g.GroupType.Id == id ).FirstOrDefault();
                    if ( groupType != null )
                    {
                        groupType.Selected = true;
                        SetupSelectionScreenForNextPerson();
                    }
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
            ProcessSelection( maWarning, () => CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                .SelectMany( f => f.People.Where( p => p.Selected )
                    .SelectMany( p => p.GroupTypes.Where( t => t.Selected)
                        .SelectMany( t => t.Groups.Where( g => !g.ExcludedByFilter ) ) ) )
                .Count() <= 0,
                "<ul><li>Sorry, based on your selection, there are currently not any available locations that can be checked into.</li></ul>" );
        }
    }
}