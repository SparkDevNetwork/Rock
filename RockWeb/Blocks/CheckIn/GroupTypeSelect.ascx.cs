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
    [DisplayName( "Group Type Select" )]
    [Category( "Check-in" )]
    [Description( "Displays a list of group types the person is configured to checkin to." )]

    #region Block Attributes

    [BooleanField( "Select All and Skip",
        Key = AttributeKey.SelectAll,
        Description = "Select this option if end-user should never see screen to select group types, all group types will automatically be selected and all the groups in all types will be available.",
        DefaultBooleanValue = false,
        Order = 8 )]
    
    [TextField( "Caption",
        Key = AttributeKey.Caption,
        IsRequired = false,
        DefaultValue = "Select Area",
        Category = "Text",
        Order = 9 )]

    [TextField( "No Option Message",
        Key = AttributeKey.NoOptionMessage,
        Description = "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.",
        IsRequired = false,
        DefaultValue = "Sorry, there are currently not any available areas that {0} can check into at {1}.",
        Category = "Text",
        Order = 10 )]

    [TextField( "No Option After Select Message",
        Key = AttributeKey.NoOptionAfterSelectMessage,
        Description = "Message to display when there are not any options available after group type is selected. Use {0} for person's name",
        IsRequired = false,
        DefaultValue = "Sorry, based on your selection, there are currently not any available times that {0} can check into.",
        Category = "Text",
        Order = 11 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "7E20E97E-63F2-413D-9C2C-16FF34023F70" )]
    public partial class GroupTypeSelect : CheckInBlockMultiPerson
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlockMultiPerson also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string SelectAll = "SelectAll";
            public const string Caption = "Caption";
            public const string NoOptionMessage = "NoOptionMessage";
            public const string NoOptionAfterSelectMessage = "NoOptionAfterSelectMessage";
        }

        /// <summary>
        /// Determines if the block requires that a selection be made. This is used to determine if user should
        /// be redirected to this block or not.
        /// </summary>
        /// <param name="backingUp">if set to <c>true</c> [backing up].</param>
        /// <returns></returns>
        public override bool RequiresSelection( bool backingUp )
        {
            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
                return false;
            }
            else
            {
                ClearSelection();

                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person == null )
                {
                    GoBack( true );
                    return false;
                }

                var schedule = person.CurrentSchedule;
                var availGroupTypes = person.GetAvailableGroupTypes( schedule );
                if ( availGroupTypes.Any() )
                {
                    if ( availGroupTypes.Count == 1 )
                    {
                        if ( backingUp )
                        {
                            GoBack( true );
                            return false;
                        }
                        else
                        {
                            var groupType = availGroupTypes.First();
                            if ( schedule == null )
                            {
                                groupType.Selected = true;
                            }
                            else
                            {
                                groupType.SelectedForSchedule.Add( schedule.Schedule.Id );
                            }

                            return !ProcessSelection( person, schedule );
                        }
                    }
                    else
                    {
                        bool SelectAll = GetAttributeValue( AttributeKey.SelectAll ).AsBoolean( false );
                        if ( SelectAll )
                        {
                            if ( backingUp )
                            {
                                GoBack( true );
                                return false;
                            }
                            else
                            {
                                availGroupTypes.ForEach( t => t.Selected = true );
                                return !ProcessSelection( person, schedule );
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if ( backingUp )
                    {
                        GoBack( true );
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

                    var person = CurrentCheckInState.CheckIn.CurrentPerson;
                    if ( person == null )
                    {
                        GoBack();
                    }

                    lTitle.Text = GetTitleText();
                    lCaption.Text = GetAttributeValue( AttributeKey.Caption );

                    var schedule = person.CurrentSchedule;

                    var availGroupTypes = person.GetAvailableGroupTypes( schedule );
                    if ( availGroupTypes.Any() )
                    {
                        if ( availGroupTypes.Count == 1 )
                        {
                            if ( UserBackedUp )
                            {
                                GoBack();
                            }
                            else
                            {
                                var groupType = availGroupTypes.First();
                                if ( schedule == null )
                                {
                                    groupType.Selected = true;
                                }
                                else
                                {
                                    groupType.SelectedForSchedule.Add( schedule.Schedule.Id );
                                }

                                ProcessSelection( person, schedule );
                            }
                        }
                        else
                        {
                            bool SelectAll = GetAttributeValue( AttributeKey.SelectAll ).AsBoolean( false );
                            if ( SelectAll )
                            {
                                if ( UserBackedUp )
                                {
                                    GoBack();
                                }
                                else
                                {
                                    availGroupTypes.ForEach( t => t.Selected = true );
                                    availGroupTypes.ForEach( t => t.SelectedForSchedule.Add( schedule.Schedule.Id ) );
                                    ProcessSelection( person, schedule );
                                }
                            }
                            else
                            {
                                rSelection.DataSource = availGroupTypes
                                    .OrderBy( g => g.GroupType.Order )
                                    .ToList();

                                rSelection.DataBind();
                            }
                        }
                    }
                    else
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            pnlNoOptions.Visible = true;
                            rSelection.Visible = false;
                            lNoOptions.Text = string.Format( GetAttributeValue( AttributeKey.NoOptionMessage ),
                                person.Person.NickName,
                                person.CurrentSchedule != null ? person.CurrentSchedule.ToString() : "this time" );
                        }
                    }
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Clear any previously group types people.
        /// </summary>
        private void ClearSelection()
        {
            var person = CurrentCheckInState.CheckIn.CurrentPerson;
            if ( person != null )
            {
                var schedule = person.CurrentSchedule;
                foreach ( var groupType in person.SelectedGroupTypes( schedule ) )
                {
                    groupType.Selected = false;
                    groupType.SelectedForSchedule = schedule != null ?
                        groupType.SelectedForSchedule.Where( s => s != schedule.Schedule.Id ).ToList() :
                        new List<int>();
                }
            }
        }

        private string GetTitleText()
        {
            var mergeFields = new Dictionary<string, object>
            {
                { LavaMergeFieldName.Family, CurrentCheckInState.CheckIn.CurrentFamily?.Group },
                { LavaMergeFieldName.Individual, CurrentCheckInState.CheckIn.CurrentPerson?.Person },
                { LavaMergeFieldName.SelectedSchedule, CurrentCheckInState.CheckIn.CurrentPerson.CurrentSchedule?.Schedule }
            };

            var personSelectHeaderLavaTemplate = CurrentCheckInState.CheckInType.GroupTypeSelectHeaderLavaTemplate ?? string.Empty;
            return personSelectHeaderLavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rSelection control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person != null )
                {
                    int id = Int32.Parse( e.CommandArgument.ToString() );
                    var groupType = person.GroupTypes.Where( g => g.GroupType.Id == id ).FirstOrDefault();
                    if ( groupType != null )
                    {
                        var schedule = person.CurrentSchedule;
                        if ( schedule == null )
                        {
                            groupType.Selected = true;
                        }
                        else
                        {
                            groupType.SelectedForSchedule.Add( schedule.Schedule.Id );
                        }

                        ProcessSelection( person, schedule );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNoOptionOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNoOptionOk_Click( object sender, EventArgs e )
        {
            ProcessNoOption();
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack( true );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        /// <summary>
        /// Processes the selection.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        protected bool ProcessSelection( CheckInPerson person, CheckInSchedule schedule )
        {
            if ( person != null )
            {
                string msg = string.Format( GetAttributeValue( AttributeKey.NoOptionAfterSelectMessage ), person.Person.NickName );
                if ( !ProcessSelection(
                    maWarning,
                    () => person.SelectedGroupTypes( schedule )
                        .SelectMany( t => t.Groups.Where( g => !g.ExcludedByFilter ) )
                        .Count() <= 0,
                    string.Format( "<p>{0}</p>", msg ),
                    true ) )
                {
                    ClearSelection();
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

    }
}