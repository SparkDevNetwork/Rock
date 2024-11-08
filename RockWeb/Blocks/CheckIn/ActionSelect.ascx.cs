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

    [LinkedPage( "Next Page (Family Check-in)",
        Key = AttributeKey.FamilyNextPage,
        IsRequired = false,
        Order = 5 )]

    [LinkedPage( "Check Out Page",
        Key = AttributeKey.CheckOutPage,
        IsRequired = false,
        Order = 6 )]

    [TextField( "Caption",
        Key = AttributeKey.Caption,
        IsRequired = false,
        DefaultValue = "Select Action",
        Category = "Text",
        Order = 7 )]

    [Rock.SystemGuid.BlockTypeGuid( "66DDB050-8F60-4DF3-9AED-5CE283E22350" )]
    public partial class ActionSelect : CheckInBlock
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlock also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string Caption = "Caption";
            public const string CheckOutPage = "CheckOutPage";
            public const string FamilyNextPage = "FamilyNextPage";
            public const string NextPage = CheckInBlock.AttributeKey.NextPage;
        }

        protected override void OnLoad( EventArgs e )
        {
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
                                lTitle.Text = GetTitleText();
                                lCaption.Text = GetAttributeValue( AttributeKey.Caption );

                                lbCheckIn.Visible = family.People.Count > 0;
                            }
                        }
                    }
                }
            }

            base.OnLoad( e );
        }

        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                family.Action = CheckinAction.CheckIn;
            }
        }

        private string GetTitleText()
        {
            var mergeFields = new Dictionary<string, object>
            {
                { LavaMergeFieldName.Family, CurrentCheckInState.CheckIn.CurrentFamily.Group }
            };

            var actionSelectHeaderLavaTemplate = CurrentCheckInState.CheckInType.ActionSelectHeaderLavaTemplate ?? string.Empty;
            return actionSelectHeaderLavaTemplate.ResolveMergeFields( mergeFields );
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
                NavigateToLinkedPage( AttributeKey.CheckOutPage );
            }
            else
            {
                string pageAttributeKey = AttributeKey.NextPage;
                if ( CurrentCheckInType != null &&
                    CurrentCheckInType.TypeOfCheckin == TypeOfCheckin.Family &&
                    !string.IsNullOrWhiteSpace( LinkedPageUrl( AttributeKey.FamilyNextPage ) ) )
                {
                    pageAttributeKey = AttributeKey.FamilyNextPage;
                }

                queryParams = CheckForOverride( queryParams );
                NavigateToLinkedPage( pageAttributeKey, queryParams );
            }
        }

    }
}