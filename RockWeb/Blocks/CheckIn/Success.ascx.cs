//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Success block" )]
    public partial class Success : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                GoToWelcomePage();
            }
            else
            {
                foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    lbAnother.Visible = family.People.Count > 1;

                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            foreach ( var location in groupType.Locations.Where( l => l.Selected ) )
                            {
                                foreach ( var group in location.Groups.Where( g => g.Selected ) )
                                {
                                    foreach ( var schedule in group.Schedules.Where( s => s.Selected ) )
                                    {
                                        var li = new HtmlGenericControl("li");
                                        li.InnerText = string.Format("Person: {0}; Group: {1}; Location: {2}; When: {3}",
                                            person.ToString(), group.ToString(), location.ToString(), schedule.ToString());
                                        phResults.Controls.Add(li);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void lbDone_Click( object sender, EventArgs e )
        {
            GoToWelcomePage();
        }

        protected void lbAnother_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        person.Selected = false;

                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            groupType.Selected = false;
                            groupType.Locations = new List<CheckInLocation>();
                        }
                    }
                }

                SaveState();
                GoToPersonSelectPage();

            }
            else
            {
                GoToWelcomePage();
            }
        }

    }
}