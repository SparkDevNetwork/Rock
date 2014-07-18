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
    public partial class AbilityLevelSelect : CheckInBlock
    {
        private string _personAbilityLevelGuid;
        private bool _shouldLowlight = true;

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    var abilityLevelDtGuid = new Guid( Rock.SystemGuid.DefinedType.PERSON_ABILITY_LEVEL_TYPE );
                    
                    var family = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
                    if ( family != null )
                    {
                        var person = family.People.FirstOrDefault( p => p.Selected );
                        if ( person != null )
                        {
                            // If no AbilityLevel attributes on Groups or GroupTypes, skip to next screen.
                            if ( HasNoAbilityLevelAttribsOnGroupTypesOrGroups() )
                            {
                                if ( UserBackedUp )
                                {
                                    GoBack();
                                }
                                else
                                {
                                    ProcessSelection( maWarning );
                                }
                            }
                            else
                            {
                                lPersonName.Text = person.ToString();
                                person.Person.LoadAttributes();
                                _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" ).ToUpper();

                                var abilityLevelDType = DefinedTypeCache.Read( abilityLevelDtGuid );

                                if ( abilityLevelDType != null )
                                {
                                    rSelection.DataSource = abilityLevelDType.DefinedValues.ToList();
                                    rSelection.DataBind();
                                }
                            }
                        }
                    }
                    else
                    {
                        GoBack();
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether any of the selected person's GroupTypes or Groups have
        /// any AbilityLevel attributes defined.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if no AbilityLevel attributes are defined; otherwise, <c>false</c>.
        /// </returns>
        private bool HasNoAbilityLevelAttribsOnGroupTypesOrGroups()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People.Where( p => p.Selected ) )
                {
                    foreach ( var groupType in person.GroupTypes )
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
                }
            }

            return true;
        }

        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                    if ( person != null )
                    {
                        string selectedAbilityLevelGuid = e.CommandArgument.ToString();
                        //person.Person.LoadAttributes();
                        _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" ).ToUpper();

                        // Only save the ability level if it's changed
                        if ( _personAbilityLevelGuid != selectedAbilityLevelGuid )
                        {
                            // Need to load a fully hydrated person because the person.Person is only a clone.
                            Person p = new PersonService( new RockContext() ).Get( person.Person.Id );
                            if ( p != null )
                            {
                                p.LoadAttributes();
                                p.SetAttributeValue( "AbilityLevel", selectedAbilityLevelGuid.ToUpperInvariant() );
                                p.SaveAttributeValues();
                            }
                        }

                        ProcessSelection( maWarning );
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

        protected void rSelection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var dvalue = e.Item.DataItem as CachedModel<DefinedValue>;
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
    }
}