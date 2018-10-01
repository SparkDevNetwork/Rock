using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_newpointe.MyNewpointe
{
    /// <summary>
    /// Block for adding new families
    /// </summary>
    [DisplayName( "Edit Family" )]
    [Category( "NewPointe -> My NewPointe" )]
    [Description( "Allows a person to edit some details about their family." )]

    [CodeEditorField( "Unauthorized Message", "The message to show when a user doesn't have permission to edit a family or the family doesn't exist. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, "<div class='alert alert-warning'><b>Oops!</b> That family doesn't exist (or you don't have permission to edit it).</div>", "", 0 )]

    [CodeEditorField( "Success Message", "The message to show when changes have been successfuly saved. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, "<div class='alert alert-success'>Your information has been saved successfully.</div>", "", 1 )]
    [LinkedPage( "Success Redirect", "The Page to redirect to when changes have been successfuly saved. Overrides the Success Message.", false, "", "", 2 )]

    [LinkedPage( "Cancel Redirect", "The Page to redirect to when the user cancels the edit.", false, "", "", 3 )]
    public partial class EditFamily : Rock.Web.UI.RockBlock
    {

        static readonly Guid GUID_HOME = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
        static readonly Guid GUID_PREV = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS );
        static readonly Guid GUID_FAMILY = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

        static readonly DefinedValueCache PreviousLocationType = DefinedValueCache.Get( GUID_PREV );
        static readonly DefinedValueCache HomeLocationType = DefinedValueCache.Get( GUID_HOME );

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                loadFamily();
            }
        }

        protected bool checkPermissions( Group family )
        {
            return family.GroupType.Guid == GUID_FAMILY && family.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );
        }

        protected void showUnauthorized()
        {
            editPanel.Visible = false;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            string template = GetAttributeValue( "UnauthorizedMessage" );

            lMessageContent.Text = template.ResolveMergeFields( mergeFields );
        }

        protected void showSuccess( Group family )
        {
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "SuccessRedirect" ) ) )
            {
                NavigateToLinkedPage( "SuccessRedirect", new Dictionary<string, string>() { { "GroupId", family.Id.ToString() } } );
            }
            else
            {

                editPanel.Visible = false;

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                string template = GetAttributeValue( "SuccessMessage" );

                lMessageContent.Text = template.ResolveMergeFields( mergeFields );

            }
        }


        protected void cbHomeIsMailing_CheckedChanged( object sender, EventArgs e )
        {
            acHomeAddress.Visible = !cbHomeIsMailing.Checked;
        }

        private IEnumerable<GroupLocation> GetFamilyLocations( Group family )
        {
            return family.GroupLocations.Where( gl => gl.GroupLocationTypeValue != null && gl.GroupLocationTypeValue.Guid == GUID_HOME );
        }

        private GroupLocation GetFamilyMailingLocation( Group family )
        {
            return GetFamilyLocations( family ).FirstOrDefault( gl => gl.IsMailingLocation );
        }

        private GroupLocation GetFamilyHomeLocation( Group family )
        {
            return GetFamilyLocations( family ).FirstOrDefault( gl => gl.IsMappedLocation );
        }

        protected void loadFamily()
        {
            Group family = new GroupService( new RockContext() ).Get( PageParameter( "GroupId" ).AsInteger() );
            if ( family == null || !checkPermissions( family ) )
            {
                showUnauthorized();
                return;
            }
            editPanel.Visible = true;

            dtbFamilyName.Text = family.Name;

            if ( family.CampusId != null ) cpFamilyCampus.SelectedCampusId = family.CampusId;

            GroupLocation mailingGroupLocation = GetFamilyMailingLocation( family );
            if ( mailingGroupLocation != null ) acMailingAddress.SetValues( mailingGroupLocation.Location );

            GroupLocation homeGroupLocation = GetFamilyHomeLocation( family );
            if ( homeGroupLocation != null ) acHomeAddress.SetValues( homeGroupLocation.Location );

            bool homeSameAsMailing = mailingGroupLocation != null && homeGroupLocation != null && mailingGroupLocation.Id == homeGroupLocation.Id;

            acHomeAddress.Visible = !( cbHomeIsMailing.Checked = homeSameAsMailing );
        }

        protected Location GetLocationFromControl( LocationService locationService, AddressControl control, Group family )
        {
            if ( control.IsValid && !string.IsNullOrWhiteSpace( control.Street1 ) && !string.IsNullOrWhiteSpace( control.City ) && !string.IsNullOrWhiteSpace( control.State ) && !string.IsNullOrWhiteSpace( control.PostalCode ) )
            {
                return locationService.Get( control.Street1, control.Street2, control.City, control.State, control.PostalCode, control.Country, family, true );
            }
            return null;
        }

        protected List<GroupLocation> GetNewGroupLocations( RockContext rContext, Group family )
        {
            List<GroupLocation> locations = new List<GroupLocation>();

            LocationService locationService = new LocationService( rContext );

            Location MailingLocation = GetLocationFromControl( locationService, acMailingAddress, family );
            bool isHomeMailing = cbHomeIsMailing.Checked;
            Location HomeLocation = !isHomeMailing ? GetLocationFromControl( locationService, acHomeAddress, family ) : null;
            if ( HomeLocation != null && HomeLocation.Id == MailingLocation.Id)
            {
                isHomeMailing = true;
            }

            if ( MailingLocation != null )
            {
                locations.Add(
                    new GroupLocation()
                    {
                        GroupLocationTypeValueId = HomeLocationType.Id,
                        Location = MailingLocation,
                        IsMailingLocation = true,
                        IsMappedLocation = isHomeMailing
                    }
                );
            }

            if ( !isHomeMailing )
            {
                if ( HomeLocation != null)
                {
                    locations.Add(
                        new GroupLocation()
                        {
                            GroupLocationTypeValueId = HomeLocationType.Id,
                            Location = HomeLocation,
                            IsMailingLocation = false,
                            IsMappedLocation = true
                        }
                    );
                }
            }

            return locations;
        }

        protected void saveFamily( object sender, EventArgs e )
        {
            RockContext rContext = new RockContext();

            DefinedValueCache PreviousLocationType = DefinedValueCache.Get( GUID_PREV );
            DefinedValueCache HomeLocationType = DefinedValueCache.Get( GUID_HOME );

            Group family = new GroupService( rContext ).Get( PageParameter( "GroupId" ).AsInteger() );
            if ( family == null || !checkPermissions( family ) )
            {
                showUnauthorized();
                return;
            }


            family.Name = dtbFamilyName.Text;
            family.CampusId = cpFamilyCampus.SelectedCampusId;

            IEnumerable<GroupLocation> oldGroupLocations = GetFamilyLocations( family );
            List<GroupLocation> newGroupLocations = GetNewGroupLocations( rContext, family );

            // First, go through the old locations and either modify it to match a new one or mark it as previous
            foreach ( GroupLocation oldGroupLocation in oldGroupLocations )
            {
                // See if we can reuse it or if we have to replace it
                GroupLocation matchingNewLocation = newGroupLocations.FirstOrDefault( gl => gl.Location.Id == oldGroupLocation.Location.Id );
                if( matchingNewLocation != null )
                {
                    oldGroupLocation.IsMailingLocation = matchingNewLocation.IsMailingLocation;
                    oldGroupLocation.IsMappedLocation = matchingNewLocation.IsMappedLocation;
                    newGroupLocations.Remove( matchingNewLocation );
                }
                else
                {
                    oldGroupLocation.GroupLocationTypeValueId = PreviousLocationType.Id;
                    oldGroupLocation.IsMailingLocation = false;
                    oldGroupLocation.IsMappedLocation = false;
                }
            }

            // Add the ones we couldn't match
            foreach ( GroupLocation newGroupLocation in newGroupLocations )
            {
                family.GroupLocations.Add( newGroupLocation );
            }
            

            rContext.SaveChanges();
            showSuccess( family );

        }
    }
}