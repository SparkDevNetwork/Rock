// <copyright>
// Copyright 2013 by the Spark Development Network
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
    [DisplayName("Group Select")]
    [Category("Check-in")]
    [Description("Displays a list of groups that a person is configured to checkin to.")]
    public partial class GroupSelect : CheckInBlock
    {
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

                    CheckInPerson person = null;
                    List<CheckInGroupType> groupTypes = null;

                    person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .SelectMany( f => f.People.Where( p => p.Selected ) )
                        .FirstOrDefault();

                    if ( person != null )
                    {
                        groupTypes = person.GroupTypes.Where( t => t.Selected ).ToList();
                    }

                    if ( groupTypes == null || !groupTypes.Any() )
                    {
                        GoBack();
                    }

                    lTitle.Text = person.ToString();
                    lSubTitle.Text = groupTypes
                        .Where( t => t.GroupType != null )
                        .Select( t => t.GroupType.Name )
                        .ToList().AsDelimited( ", " );

                    var availGroups = groupTypes
                        .SelectMany( t => t.Groups.Where( g => !g.ExcludedByFilter) ).ToList();
                    if ( availGroups.Count == 1 )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            availGroups.FirstOrDefault().Selected = true;
                            ProcessSelection();
                        }
                    }
                    else
                    {
                        rSelection.DataSource = availGroups
                            .OrderBy( g => g.Group.Order )
                            .ToList();

                        rSelection.DataBind();
                    }
                }
            }
        }

        /// <summary>
        /// Clear any previously selected groups.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People )
                {
                    foreach ( var groupType in person.GroupTypes )
                    {
                        foreach ( var group in groupType.Groups )
                        {
                            group.Selected = false;
                        }
                    }
                }
            }
        }

        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var groupTypes = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                    .SelectMany( f => f.People.Where( p => p.Selected )
                        .SelectMany( p => p.GroupTypes.Where( t => t.Selected ) ) )
                            .ToList();

                if ( groupTypes != null && groupTypes.Any() )
                {
                    int id = Int32.Parse( e.CommandArgument.ToString() );
                    var group = groupTypes.SelectMany( t => t.Groups)
                        .Where( g => g.Group.Id == id ).FirstOrDefault();
                    if ( group != null )
                    {
                        group.Selected = true;
                        ProcessSelection();
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
                    .SelectMany( p => p.GroupTypes.Where( t => t.Selected )
                        .SelectMany( t => t.Groups.Where( g => g.Selected ) 
                            .SelectMany( g => g.Locations.Where( l => !l.ExcludedByFilter ) ) ) ) )
                .Count() <= 0,
                "<p>Sorry, based on your selection, there are currently not any available locations that can be checked into.</p>" );
        }
    }
}