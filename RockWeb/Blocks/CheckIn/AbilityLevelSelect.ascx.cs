//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
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
                    var abilityLevelDtGuid = new Guid( "7BEEF4D4-0860-4913-9A3D-857634D1BF7C" );
                    
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
                                _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" );

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
                        _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" );

                        // Only save the ability level if it's changed
                        if ( _personAbilityLevelGuid != selectedAbilityLevelGuid )
                        {
                            // Need to load a fully hydrated person because the person.Person is only a clone.
                            Person p = new PersonService().Get( person.Person.Id );
                            if ( p != null )
                            {
                                p.LoadAttributes();
                                p.SetAttributeValue( "AbilityLevel", selectedAbilityLevelGuid.ToUpperInvariant() );
                                Rock.Attribute.Helper.SaveAttributeValues( p, CurrentPersonId );
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

            // Once we've hit the person's ability level -OR- if their level is not yet set, 
            // we stop lowlighting/disabling the buttons. 
            if ( _shouldLowlight && ( _personAbilityLevelGuid == "" || dvalue.Guid.ToString() == _personAbilityLevelGuid ) )
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

            if ( dvalue.Guid.ToString() == _personAbilityLevelGuid )
            {
                var linkButton = e.Item.FindControl( "lbSelect" ) as LinkButton;
                linkButton.Text = string.Format( "{0} {1}", "<i class='icon-check-sign'> </i>", linkButton.Text );
            }
        }
    }
}