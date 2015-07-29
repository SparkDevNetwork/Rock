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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Connection Opportunity Signup" )]
    [Category( "Connection" )]
    [Description( "Block used to sign up for a connection opportunity." )]

    [BooleanField( "Display Home Phone", "Whether to display home phone", true, "", 0 )]
    [BooleanField( "Display Mobile Phone", "Whether to display mobile phone", true, "", 1 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the response message.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}", "", 2 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]
    [BooleanField( "Enable Campus Context", "If the page has a campus context it's value will be used as a filter", true, "", 4 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 5 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 6 )]
    public partial class ConnectionOpportunitySignup : RockBlock, IDetailBlock
    {
        #region Fields

        DefinedValueCache _homePhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
        DefinedValueCache _cellPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlOpportunityDetail );
        }
 
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbErrorMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "OpportunityId" ).AsInteger() );
            }
        }


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "OpportunityId" ).AsInteger() );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnConnect_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var opportunityService = new ConnectionOpportunityService( rockContext );
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var personService = new PersonService( rockContext );


                // Get the opportunity and default status
                int opportunityId = PageParameter( "OpportunityId" ).AsInteger();
                var opportunity = opportunityService
                    .Queryable()
                    .Where( o => o.Id == opportunityId )
                    .FirstOrDefault();

                int defaultStatusId = opportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .Select( s => s.Id )
                    .FirstOrDefault();

                // If opportunity is valid and has a default status
                if ( opportunity != null && defaultStatusId > 0 )
                {
                    Person person = null;

                    string firstName = tbFirstName.Text.Trim();
                    string lastName = tbLastName.Text.Trim();
                    string email = tbEmail.Text.Trim();
                    int? campudId = cpCampus.SelectedCampusId;

                    if ( CurrentPerson != null &&
                        CurrentPerson.LastName.Equals( lastName, StringComparison.OrdinalIgnoreCase ) &&
                        CurrentPerson.NickName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) &&
                        CurrentPerson.Email.Equals( email, StringComparison.OrdinalIgnoreCase ) )
                    {
                        // If the name and email entered are the same as current person (wasn't changed), use the current person
                        person = personService.Get( CurrentPerson.Id );
                    }

                    else
                    {
                        // Try to find matching person
                        var personMatches = personService.GetByMatch( firstName, lastName, email );
                        if ( personMatches.Count() == 1 )
                        {
                            // If one person with same name and email address exists, use that person
                            person = personMatches.First();
                        }
                    }

                    // If person was not found, create a new one
                    if ( person == null )
                    {
                        // If a match was not found, create a new person
                        var dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                        var dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

                        person = new Person();
                        person.FirstName = firstName;
                        person.LastName = lastName;
                        person.IsEmailActive = true;
                        person.Email = email;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        if ( dvcConnectionStatus != null )
                        {
                            person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                        }
                        if ( dvcRecordStatus != null )
                        {
                            person.RecordStatusValueId = dvcRecordStatus.Id;
                        }

                        PersonService.SaveNewPerson( person, rockContext, campudId, false );
                        person = personService.Get( person.Id );
                    }

                    // If there is a valid person with a primary alias, continue
                    if ( person != null && person.PrimaryAliasId.HasValue )
                    {
                        var changes = new List<string>();
                        
                        if ( pnHome.Visible )
                        {
                            SavePhone( pnHome, person, _homePhone.Guid, changes );
                        }

                        if ( pnMobile.Visible )
                        {
                            SavePhone( pnMobile, person, _cellPhone.Guid, changes );
                        }

                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                person.Id,
                                changes );
                        }

                        // Now that we have a person, we can create the connection request
                        var connectionRequest = new ConnectionRequest();
                        connectionRequest.PersonAliasId = person.PrimaryAliasId.Value;
                        connectionRequest.Comments = tbComments.Text.Trim();
                        connectionRequest.ConnectionOpportunityId = opportunity.Id;
                        connectionRequest.ConnectionState = ConnectionState.Active;
                        connectionRequest.ConnectionStatusId = defaultStatusId;
                        connectionRequest.CampusId = campudId;

                        if ( !connectionRequest.IsValid )
                        {
                            // Controls will show warnings
                            return;
                        }

                        connectionRequestService.Add( connectionRequest );
                        rockContext.SaveChanges();

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( PageParameter( "OpportunityId" ).AsInteger() ) );
                        mergeFields.Add( "CurrentPerson", CurrentPerson );
                        mergeFields.Add( "Person", person );

                        lResponseMessage.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
                        lResponseMessage.Visible = true;

                        pnlSignup.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        public void ShowDetail( int opportunityId )
        {
            using ( var rockContext = new RockContext() )
            {
                var opportunity = new ConnectionOpportunityService( rockContext ).Get( opportunityId );
                if ( opportunity == null )
                {
                    pnlSignup.Visible = false;
                    ShowError( "Incorrect Opportunity Type", "The requested opportunity does not exist." );
                    return;
                }

                pnlSignup.Visible = true;

                if ( !string.IsNullOrWhiteSpace( opportunity.IconCssClass ) )
                {
                    lIcon.Text = string.Format( "<i class='{0}' ></i>", opportunity.IconCssClass );
                }

                lTitle.Text = opportunity.Name.FormatAsHtmlTitle();

                pnHome.Visible = GetAttributeValue( "DisplayHomePhone" ).AsBoolean();
                pnMobile.Visible = GetAttributeValue( "DisplayMobilePhone" ).AsBoolean();

                if ( CurrentPerson != null )
                {
                    tbFirstName.Text = CurrentPerson.FirstName.EncodeHtml();
                    tbLastName.Text = CurrentPerson.LastName.EncodeHtml();
                    tbEmail.Text = CurrentPerson.Email.EncodeHtml();

                    if ( pnHome.Visible && _homePhone != null )
                    {
                        var homePhoneNumber = CurrentPerson.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                        if ( homePhoneNumber != null )
                        {
                            pnHome.Number = homePhoneNumber.NumberFormatted;
                            pnHome.CountryCode = homePhoneNumber.CountryCode;
                        }
                    }

                    if ( pnMobile.Visible && _cellPhone != null )
                    {
                        var cellPhoneNumber = CurrentPerson.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                        if ( cellPhoneNumber != null )
                        {
                            pnMobile.Number = cellPhoneNumber.NumberFormatted;
                            pnMobile.CountryCode = cellPhoneNumber.CountryCode;
                        }
                    }
                }

                var campuses = CampusCache.All();
                cpCampus.Campuses = campuses;
                cpCampus.Visible = campuses.Count > 1;

                if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                {
                    var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );
                    var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                    if ( contextCampus != null )
                    {
                        cpCampus.SelectedCampusId = contextCampus.Id;
                    }
                }

                // show debug info
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( PageParameter( "OpportunityId" ).AsInteger() ) );
                mergeFields.Add( "CurrentPerson", CurrentPerson );
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
        }

        private void SavePhone( PhoneNumberBox phoneNumberBox, Person person, Guid phoneTypeGuid, List<string> changes )
        {
            var numberType = DefinedValueCache.Read( phoneTypeGuid );
            if ( numberType != null )
            {
                var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );
                string oldPhoneNumber = phone != null ? phone.NumberFormattedWithCountryCode : string.Empty;
                string newPhoneNumber = PhoneNumber.CleanNumber( phoneNumberBox.Number );

                if ( newPhoneNumber != string.Empty )
                {
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        person.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberType.Id;
                    }
                    else
                    {
                        oldPhoneNumber = phone.NumberFormattedWithCountryCode;
                    }
                    phone.CountryCode = PhoneNumber.CleanNumber( phoneNumberBox.CountryCode );
                    phone.Number = newPhoneNumber;

                    History.EvaluateChange(
                        changes,
                        string.Format( "{0} Phone", numberType.Value ),
                        oldPhoneNumber,
                        phone.NumberFormattedWithCountryCode );
                }

            }
        }

        private void ShowError( string title, string message )
        {
            nbErrorMessage.Title = title;
            nbErrorMessage.Text = string.Format( "<p>{0}</p>", message );
            nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbErrorMessage.Visible = true;
        }

        #endregion

    }
}