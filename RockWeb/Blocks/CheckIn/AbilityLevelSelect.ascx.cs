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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Ability Level Select")]
    [Category("Check-in")]
    [Description( "Check-in Ability Level Select block" )]

    #region Block Attributes

    [LinkedPage( "Previous Page (Family Check-in)",
        Key = "FamilyPreviousPage",
        Description = "The page to navigate back to if none of the people and schedules have been processed.",
        IsRequired = false,
        Order = 8 )]

    [TextField( "Caption",
        Key = AttributeKey.Caption,
        IsRequired = false,
        DefaultValue = "Select Ability Level",
        Category = "Text",
        Order = 9 )]

    [TextField( "No Option Title",
        Key = AttributeKey.NoOptionTitle,
        IsRequired = false,
        DefaultValue = "Sorry",
        Category = "Text",
        Order = 10 )]

    [TextField( "No Option Caption",
        Key = AttributeKey.NoOptionCaption,
        IsRequired = false,
        DefaultValue = "Sorry, there are currently not any available options to check into.",
        Category = "Text",
        Order = 11 )]

    [TextField( "Selection No Option",
        Key = AttributeKey.SelectionNoOption,
        Description = "Text displayed if there are not any options after selecting an ability level. Use {0} for person's name.",
        IsRequired = false,
        DefaultValue = "Sorry, based on your selection, there are currently not any available locations that {0} can check into.",
        Category = "Text",
        Order = 12 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "605389F5-5BC5-438F-8757-110328B0CED3" )]
    public partial class AbilityLevelSelect : CheckInBlockMultiPerson
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlockMultiPerson also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string FamilyPreviousPage = "FamilyPreviousPage";
            public const string Caption = "Caption";
            public const string NoOptionTitle = "NoOptionTitle";
            public const string NoOptionCaption = "NoOptionCaption";
            public const string SelectionNoOption = "SelectionNoOption";
            public const string MultiPersonLastPage = CheckInBlockMultiPerson.AttributeKey.MultiPersonLastPage;
        }

        private string _personAbilityLevelGuid;
        private bool _shouldLowlight = true;

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

            var person = CurrentCheckInState.CheckIn.CurrentPerson;
            if ( person == null )
            {
                return true;
            }

            if ( IsOverride || NoConfiguredAbilityLevels( person.GroupTypes ) )
            {
                if ( backingUp )
                {
                    GoBack( CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family );
                    return false;
                }
                else
                {
                    NavigateToNextPage( true );
                    return false;
                }
            }
            else
            {
                if ( backingUp )
                {
                    GoBack( CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family );
                    return false;
                }
                else
                {
                    // If an ability level has already been selected, just process the selection
                    if ( person.StateParameters.ContainsKey( "AbilityLevel" ) )
                    {
                        return !ProcessSelection();
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
                    ProcessPerson();
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Determines whether any of the selected person's GroupTypes or Groups have
        /// any AbilityLevel attributes defined.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if no AbilityLevel attributes are defined; otherwise, <c>false</c>.
        /// </returns>
        private bool NoConfiguredAbilityLevels( List<CheckInGroupType> groupTypes )
        {
            foreach ( var groupType in groupTypes )
            {
                var groupTypeAttributes = groupType.GroupType.GetAttributeValues( "AbilityLevel" );
                if ( groupTypeAttributes.Any() )
                {
                    // break out, we're done as soon as we find one!
                    return false;
                }

                foreach ( var group in groupType.Groups )
                {
                    var groupAttributes = group.Group.GetAttributeValues( "AbilityLevel" );
                    if ( groupAttributes.Any() )
                    {
                        // break out, we're done as soon as we find one!
                        return false;
                    }
                }
            }

            return true;
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
                    string selectedAbilityLevelGuid = e.CommandArgument.ToString();

                    //person.Person.LoadAttributes();
                    _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" ).ToUpper();

                    // Save the fact that user has already selected an ability level so they won't be asked again
                    person.StateParameters.AddOrReplace( "AbilityLevel", _personAbilityLevelGuid.ToString() );

                    // Only save the ability level if it's changed
                    if ( _personAbilityLevelGuid != selectedAbilityLevelGuid )
                    {
                        // Need to load a fully hydrated person because the person.Person is only a clone.
                        using ( var rockContext = new RockContext() )
                        {
                            Person p = new PersonService( rockContext ).Get( person.Person.Id );
                            if ( p != null )
                            {
                                p.LoadAttributes( rockContext );
                                p.SetAttributeValue( "AbilityLevel", selectedAbilityLevelGuid.ToUpperInvariant() );
                                p.SaveAttributeValues( rockContext );
                                person.Person.LoadAttributes( rockContext );
                            }
                        }
                    }

                    ProcessSelection();
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
            CancelCheckin();
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
        /// Handles the ItemDataBound event of the rSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rSelection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var dvalue = e.Item.DataItem as DefinedValueCache;
            var guid = dvalue.Guid.ToString().ToUpper();

            // Once we've hit the person's ability level -OR- if their level is not yet set, 
            // we stop lowlighting/disabling the buttons. 
            if ( _shouldLowlight && ( _personAbilityLevelGuid == "" || guid == _personAbilityLevelGuid ) )
            {
                _shouldLowlight = false;
            }

            // Otherwise... we dim out the button so it appears that it can't be selected.
            // But it is still selectable to deal with the small case when someone accidentally (?)
            // selected the wrong option.
            if ( _shouldLowlight )
            {
                var linkButton = e.Item.FindControl( "lbSelect" ) as LinkButton;
                linkButton.AddCssClass( "btn-dimmed" );
            }

            if ( guid == _personAbilityLevelGuid )
            {
                var linkButton = e.Item.FindControl( "lbSelect" ) as LinkButton;
                linkButton.Text = string.Format("{0} {1}", "<i class='fa fa-check-square'> </i>", linkButton.Text);
            }
        }

        /// <summary>
        /// Processes the person.
        /// </summary>
        protected void ProcessPerson()
        {
            var person = CurrentCheckInState.CheckIn.CurrentPerson;
            if ( person == null )
            {
                pnlNoOptions.Visible = true;
                divAbilityLevel.Visible = false;

                /*
                 * 2023-04-14 ETD
                 * Don't show the back button if prevent duplicate is enabled.
                 * This option will remove the existing schedule from the list and could result in the GroupType and Person being removed as well, causing NRE errors when loading another block.
                 * See issue 5369 for more details.
                 */
                lbBack.Visible = !CurrentCheckInState.CheckInType.PreventDuplicateCheckin;

                lNoOptionTitle.Text = GetAttributeValue( AttributeKey.NoOptionTitle );
                lNoOptionCaption.Text = GetAttributeValue( AttributeKey.NoOptionCaption );
            }
            else
            {
                lTitle.Text = GetTitleText(); 
                lCaption.Text = GetAttributeValue( AttributeKey.Caption );

                if ( IsOverride || NoConfiguredAbilityLevels( person.GroupTypes ) )
                {
                    if ( UserBackedUp )
                    {
                        GoBack( true );
                    }
                    else
                    {
                        NavigateToNextPage( true );
                    }
                }
                else
                {
                    // If an ability level has already been selected, just process the selection
                    if ( person.StateParameters.ContainsKey( "AbilityLevel" ) )
                    {
                        ProcessSelection();
                    }
                    else
                    {
                        person.Person.LoadAttributes();
                        _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" ).ToUpper();
                        person.StateParameters.Add( "AbilityLevel", _personAbilityLevelGuid );

                        if ( CurrentCheckInState.CheckInType.AbilityLevelDetermination == AbilityLevelDeterminationOptions.DoNotAsk ||
                            ( CurrentCheckInState.CheckInType.AbilityLevelDetermination == AbilityLevelDeterminationOptions.DoNotAskIfThereIsNoAbilityLevel && _personAbilityLevelGuid.IsNullOrWhiteSpace() ) )
                        {
                            if ( !ProcessSelection() )
                            {
                                NavigateToNextPage( true );
                            }
                        }
                        else
                        {
                            var abilityLevelDType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_ABILITY_LEVEL_TYPE.AsGuid() );
                            if ( abilityLevelDType != null )
                            {
                                rSelection.DataSource = abilityLevelDType.DefinedValues.ToList();
                                rSelection.DataBind();
                            }
                        }
                    }
                }
            }
        }

        private string GetTitleText()
        {
            var selectedArea = CurrentCheckInState.CheckIn.CurrentPerson.GroupTypes.Where( a => a.Selected ).FirstOrDefault()?.GroupType
                ?? CurrentCheckInState.CheckIn.CurrentPerson.GroupTypes.FirstOrDefault()?.GroupType;

            var mergeFields = new Dictionary<string, object>
            {
                { LavaMergeFieldName.Family, CurrentCheckInState.CheckIn.CurrentFamily.Group },
                { LavaMergeFieldName.Individual, CurrentCheckInState.CheckIn.CurrentPerson.Person },
                { LavaMergeFieldName.SelectedArea, selectedArea }
            };

            var abilityLevelSelectHeaderLavaTemplate = CurrentCheckInState.CheckInType.AbilityLevelSelectHeaderLavaTemplate ?? string.Empty;
            return abilityLevelSelectHeaderLavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Processes the selection.
        /// </summary>
        protected bool ProcessSelection()
        {
            if ( !ProcessSelection( 
                maWarning, 
                () => CurrentCheckInState.CheckIn.CurrentPerson.GroupTypes
                    .Where( t => !t.ExcludedByFilter ) 
                    .Count() <= 0,
                string.Format( "<p>{0}</p>", string.Format( GetAttributeValue( AttributeKey.SelectionNoOption ), CurrentCheckInState.CheckIn.CurrentPerson.Person.NickName ) ),
                true ) ) 
            {
                // Clear any filtered items so that user can select another option
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person == null )
                {
                    CancelCheckin();
                }
                else
                {
                    person.ClearFilteredExclusions();

                    if ( !NoConfiguredAbilityLevels( person.GroupTypes ) )
                    {
                        person.Person.LoadAttributes();
                        _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" ).ToUpper();

                        var abilityLevelDType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_ABILITY_LEVEL_TYPE.AsGuid() );
                        if ( abilityLevelDType != null )
                        {
                            rSelection.DataSource = abilityLevelDType.DefinedValues.ToList();
                            rSelection.DataBind();
                        }
                    }
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on previous page has a selection required before redirecting.</param>
        protected override void NavigateToPreviousPage( Dictionary<string, string> queryParams, bool validateSelectionRequired )
        {
            if ( CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family )
            {
                bool anythingProcessed = false;

                queryParams = CheckForOverride( queryParams );

                // First check for first unprocessed person
                var currentPerson = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( currentPerson != null )
                {
                    currentPerson.StateParameters.Remove( "AbilityLevel" );

                    var lastSchedule = currentPerson.PossibleSchedules.Where( p => p.Processed ).LastOrDefault();
                    if ( lastSchedule != null )
                    {
                        // Current person has a processed schedule, unmark that one and continue.
                        lastSchedule.Processed = false;
                        anythingProcessed = true;
                    }
                    else
                    {
                        // current person did not have any processed schedules, so find last processed person, and 
                        // mark them and their last schedule as not processed.
                        var family = CurrentCheckInState.CheckIn.CurrentFamily;
                        if ( family != null )
                        {
                            var lastPerson = family.People.Where( p => p.Processed ).LastOrDefault();
                            if ( lastPerson != null )
                            {
                                lastPerson.Processed = false;
                                lastSchedule = lastPerson.PossibleSchedules.Where( p => p.Processed ).LastOrDefault();
                                if ( lastSchedule != null )
                                {
                                    lastSchedule.Processed = false;
                                }

                                anythingProcessed = true;
                            }
                        }
                    }

                    SaveState();
                }

                if ( anythingProcessed )
                { 
                    if ( validateSelectionRequired )
                    {
                        var nextBlock = GetCheckInBlock( AttributeKey.MultiPersonLastPage );
                        if ( nextBlock != null && nextBlock.RequiresSelection( true ) )
                        {
                            NavigateToLinkedPage( AttributeKey.MultiPersonLastPage, queryParams );
                        }
                    }
                    else
                    {
                        NavigateToLinkedPage( AttributeKey.MultiPersonLastPage, queryParams );
                    }
                }
                else
                {
                    // If the current person did not have any processed schedules, then this would be the first person
                    // and we should navigate to previous page (person selection)
                    NavigateToLinkedPage( AttributeKey.FamilyPreviousPage, queryParams );
                }
            }
            else
            {
                base.NavigateToPreviousPage( queryParams, validateSelectionRequired );
            }
        }

    }
}