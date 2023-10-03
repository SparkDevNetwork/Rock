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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Person Select")]
    [Category("Check-in")]
    [Description("Lists people who match the selected family to pick to check-in or check-out.")]

    [TextField( "Caption",
        Key = AttributeKey.Caption,
        Description = "",
        IsRequired = false,
        DefaultValue = "Select Person",
        Category = "Text",
        Order = 8 )]

    [TextField( "No Option Message",
        Key = AttributeKey.NoOptionMessage,
        Description = "The option to display when there are not any people that match. Use {0} for the current action ('into' or 'out of').",
        IsRequired = false,
        DefaultValue = "Sorry, there are currently not any available areas that the selected person can check {0}.",
        Category = "Text",
        Order = 9 )]

    [Rock.SystemGuid.BlockTypeGuid( "34B48E0F-5E37-425E-9588-E612ED34DB03" )]
    public partial class PersonSelect : CheckInBlock
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlock also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string Caption = "Caption";
            public const string NoOptionMessage = "NoOptionMessage";
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

                    var family = CurrentCheckInState.CheckIn.CurrentFamily;
                    if ( family == null )
                    {
                        GoBack();
                        return;
                    }

                    lbEditFamily.Visible = CurrentCheckInState.Kiosk.RegistrationModeEnabled;
                    lTitle.Text = GetTitleText();
                    lCaption.Text = GetAttributeValue( AttributeKey.Caption );

                    if ( family.People.Count == 1 && !CurrentCheckInState.Kiosk.RegistrationModeEnabled )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            family.People.FirstOrDefault().Selected = true;
                            ProcessSelection();
                        }
                    }
                    else
                    {
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
                    person.Selected = false;
                }
            }
        }

        private string GetTitleText()
        {
            var mergeFields = new Dictionary<string, object>
            {
                { LavaMergeFieldName.Family, CurrentCheckInState.CheckIn.CurrentFamily.Group }
            };

            var personSelectHeaderLavaTemplate = CurrentCheckInState.CheckInType.PersonSelectHeaderLavaTemplate ?? string.Empty;
            return personSelectHeaderLavaTemplate.ResolveMergeFields( mergeFields );
        }

        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                int id = Int32.Parse( e.CommandArgument.ToString() );
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                    .SelectMany( f => f.People.Where( p => p.Person.Id == id ) )
                    .FirstOrDefault();

                if ( person != null )
                {
                    person.Selected = true;
                    ProcessSelection();
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

        protected void rSelection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var person = e.Item.DataItem as CheckInPerson;
            string buttonText = person.Person.FullName;
            var linkButton = e.Item.FindControl( "lbSelect" ) as LinkButton;

            if ( ! string.IsNullOrEmpty( person.SecurityCode ) )
            {
                linkButton.AddCssClass( "btn-dimmed" );
            }

            var personSelectLavaTemplate = CurrentCheckInState.CheckInType.PersonSelectAdditionalInfoLavaTemplate;
            if ( personSelectLavaTemplate.IsNotNullOrWhiteSpace() )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions() );
                mergeFields.Add( "Person", person );
                buttonText += string.Format( "<br /><span class='text-light'>{0}</span>", personSelectLavaTemplate.ResolveMergeFields( mergeFields ) );
            }

            linkButton.Text = buttonText;
        }

        protected void ProcessSelection()
        {
            string noOption = string.Format( GetAttributeValue( AttributeKey.NoOptionMessage ), CurrentCheckInState.CheckIn.CurrentFamily.Action == CheckinAction.CheckIn ? "into" : "out of" );
            string msg = string.Format( "<p>{0}</p>", noOption );
            ProcessSelection( 
                maWarning, 
                () => CurrentCheckInState.CheckIn.CurrentFamily.GetPeople( true )
                    .SelectMany( p => p.GroupTypes.Where( t => !t.ExcludedByFilter ) )
                    .Count() <= 0, msg );
        }

        /// <summary>
        /// Handles the Click event of the lbEditFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditFamily_Click( object sender, EventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                return;
            }

            var editFamilyBlock = this.RockPage.ControlsOfTypeRecursive<CheckInEditFamilyBlock>().FirstOrDefault();
            if ( editFamilyBlock != null && CurrentCheckInState.CheckIn.CurrentFamily != null )
            {
                editFamilyBlock.ShowEditFamily( CurrentCheckInState.CheckIn.CurrentFamily );
            }
        }
    }
}