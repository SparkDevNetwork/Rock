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
                    //var abilityLevelDtGuid = new Guid( "D8672146-C14F-41E8-A143-242361CECCD3" );
                    var abilityLevelDtGuid = new Guid( Rock.SystemGuid.DefinedType.PERSON_ABILITY_LEVEL );
                    
                    var family = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
                    if ( family != null )
                    {
                        var person = family.People.FirstOrDefault( p => p.Selected );
                        if ( person != null )
                        {
                            lPersonName.Text = person.ToString();
                            person.Person.LoadAttributes();
                            _personAbilityLevelGuid = person.Person.GetAttributeValue( "AbilityLevel" );

                            var abilityLevelDType = DefinedTypeCache.Read( abilityLevelDtGuid );

                            if ( abilityLevelDType != null )
                            {
                                // TODO -- remove ability levels that are lower (ordered) than the person's current
                                // ability level.
                                rSelection.DataSource = abilityLevelDType.DefinedValues.ToList();
                                rSelection.DataBind();
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
                                p.SetAttributeValue( "AbilityLevel", selectedAbilityLevelGuid );
                                Rock.Attribute.Helper.SaveAttributeValues( p, CurrentPersonId );
                            }
                        }

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

        /// <summary>
        /// Do nothing (such as unselecting something) but simply return to previous screen.
        /// </summary>
        private void GoBack()
        {
            SaveState();
            NavigateToPreviousPage();
        }

        private void ProcessSelection()
        {
            SaveState();
            NavigateToNextPage();
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