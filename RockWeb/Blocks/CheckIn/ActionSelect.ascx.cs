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
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Action Select")]
    [Category("Check-in")]
    [Description("Displays option for family to Check In or Check Out.")]

    [LinkedPage( "Next Page (Family Check-in)", "", false, "", "", 5, "FamilyNextPage" )]
    [LinkedPage( "Check Out Page", "", false, "", "", 6, "CheckOutPage" )]

    [TextField( "Title", "Title to display. Use {0} for family name", false, "{0}", "Text", 7 )]
    [TextField( "Caption", "", false, "Select Action", "Text", 8 )]

    public partial class ActionSelect : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-actionselect-bg" );
            }

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
                    else
                    {
                        if ( UserBackedUp && ( !CurrentCheckInState.AllowCheckout || !family.CheckOutPeople.Any() ) )
                        {
                            GoBack();
                        }
                        else
                        {
                            if ( !CurrentCheckInState.AllowCheckout || !family.CheckOutPeople.Any() )
                            {
                                ProcessSelection( CheckinAction.CheckIn );
                            }
                            else
                            {

                                lTitle.Text = string.Format( GetAttributeValue( "Title" ), family.ToString() );
                                lCaption.Text = GetAttributeValue( "Caption" );

                                lbCheckIn.Visible = family.People.Count > 0;
                            }
                        }
                    }
                }
            }
        }

        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                family.Action = CheckinAction.CheckIn;
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

        protected void lbCheckOut_Click( object sender, EventArgs e )
        {
            ProcessSelection( CheckinAction.CheckOut );
        }

        protected void lbCheckIn_Click( object sender, EventArgs e )
        {
            ProcessSelection( CheckinAction.CheckIn );
        }

        private void ProcessSelection( CheckinAction action )
        {
            var family = CurrentCheckInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                family.Action = action;
                ProcessSelection( maWarning, false );
            }
        }

        protected override void NavigateToNextPage( Dictionary<string, string> queryParams, bool validateSelectionRequired )
        {
            if ( CurrentCheckInState.CheckIn.CurrentFamily.Action == CheckinAction.CheckOut )
            {
                NavigateToLinkedPage( "CheckOutPage" );
            }
            else
            {
                string pageAttributeKey = "NextPage";
                if ( CurrentCheckInType != null &&
                    CurrentCheckInType.TypeOfCheckin == TypeOfCheckin.Family &&
                    !string.IsNullOrWhiteSpace( LinkedPageUrl( "FamilyNextPage" ) ) )
                {
                    pageAttributeKey = "FamilyNextPage";
                }

                queryParams = CheckForOverride( queryParams );
                NavigateToLinkedPage( pageAttributeKey, queryParams );
            }
        }

    }
}