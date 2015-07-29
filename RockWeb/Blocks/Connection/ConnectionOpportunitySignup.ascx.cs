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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Opportunity Signup" )]
    [Category( "Connection" )]
    [Description( "Block used to sign up for a connection opportunity." )]
    [BooleanField( "Display Home Phone", "Whether to display home phone", true )]
    [BooleanField( "Display Mobile Phone", "Whether to display mobile phone", true )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the response message.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}", "", 2 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the opportunity name.", true )]
    [BooleanField( "Enable Campus Context", "If the page has a campus context it's value will be used as a filter", true )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2" )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49" )]
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

            if ( !Page.IsPostBack )
            {
                string opportunityId = PageParameter( "OpportunityId" );
                if ( !string.IsNullOrWhiteSpace( opportunityId ) )
                {
                    ShowDetail( opportunityId.AsInteger() );
                }
                else
                {
                    pnlSignup.Visible = false;
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? opportunityId = PageParameter( pageReference, "OpportunityId" ).AsIntegerOrNull();
            if ( opportunityId != null )
            {
                ConnectionOpportunity connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( opportunityId.Value );
                breadCrumbs.Add( new BreadCrumb( string.Format( "{0} Signup", connectionOpportunity.Name ), pageReference ) );
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnConnect_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var isValid = true;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine( "The following fields are required:" );
                sb.AppendLine( "<ul>" );
                var connectionRequest = new ConnectionRequest();
                var person = new Person();

                // Find the person and create a new one if needed
                if ( CurrentPerson == null )
                {
                    if ( String.IsNullOrWhiteSpace( tbFirstName.Text ) )
                    {
                        sb.AppendLine( "<li>First Name</li>" );
                        isValid = false;
                    }
                    else
                    {
                        person.FirstName = tbFirstName.Text.Trim();
                    }

                    if ( String.IsNullOrWhiteSpace( tbLastName.Text ) )
                    {
                        sb.AppendLine( "<li>Last Name</li>" );
                        isValid = false;
                    }
                    else
                    {
                        person.LastName = tbLastName.Text.Trim();
                    }

                    if ( String.IsNullOrWhiteSpace( tbEmail.Text ) )
                    {
                        sb.AppendLine( "<li>Email</li>" );
                        isValid = false;
                    }
                    else
                    {
                        person.Email = tbEmail.Text.Trim();
                    }

                    if ( GetAttributeValue( "DisplayHomePhone" ).AsBoolean() || GetAttributeValue( "DisplayMobilePhone" ).AsBoolean() )
                    {
                        if ( String.IsNullOrWhiteSpace( pnHome.Number ) && String.IsNullOrWhiteSpace( pnMobile.Number ) )
                        {
                            sb.AppendLine( "<li>Phone Number</li>" );
                            isValid = false;
                        }
                        else
                        {
                            string homeNumber = PhoneNumber.CleanNumber( pnHome.Number );
                            if ( _homePhone != null && !string.IsNullOrWhiteSpace( homeNumber ) )
                            {
                                var homePhoneNumber = new PhoneNumber();
                                homePhoneNumber.NumberTypeValueId = _homePhone.Id;
                                homePhoneNumber.Number = homeNumber;
                                homePhoneNumber.CountryCode = PhoneNumber.CleanNumber( pnHome.CountryCode );
                                person.PhoneNumbers.Add( homePhoneNumber );
                            }

                            string cellNumber = PhoneNumber.CleanNumber( pnMobile.Number );
                            if ( _cellPhone != null && !string.IsNullOrWhiteSpace( cellNumber ) )
                            {
                                var cellPhoneNumber = new PhoneNumber();
                                cellPhoneNumber.NumberTypeValueId = _cellPhone.Id;
                                cellPhoneNumber.Number = cellNumber;
                                cellPhoneNumber.CountryCode = PhoneNumber.CleanNumber( pnMobile.CountryCode );
                                person.PhoneNumbers.Add( cellPhoneNumber );
                            }
                        }
                    }

                    if ( !isValid )
                    {
                        nbInvalidMessage.Text = sb.ToString();
                        nbInvalidMessage.Visible = true;
                        return;
                    }
                    else
                    {
                        var personMatch = personService.GetByMatch( person.FirstName, person.LastName, person.Email ).FirstOrDefault();
                        if ( personMatch != null )
                        {
                            person = personMatch;
                        }
                        else
                        {
                            DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                            DefinedValueCache dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

                            person.IsEmailActive = true;
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

                            person.Gender = Gender.Unknown;

                            PersonService.SaveNewPerson( person, rockContext, null, false );
                        }
                    }
                }
                else
                {
                    var changes = new List<string>();

                    person = personService.Get( CurrentPersonId.Value );

                    if ( GetAttributeValue( "DisplayHomePhone" ).AsBoolean() || GetAttributeValue( "DisplayMobilePhone" ).AsBoolean() )
                    {
                        if ( String.IsNullOrWhiteSpace( pnHome.Number ) && String.IsNullOrWhiteSpace( pnMobile.Number ) )
                        {
                            isValid = false;
                        }
                        else
                        {
                            string homeNumber = PhoneNumber.CleanNumber( pnHome.Number );
                            if ( _homePhone != null && !string.IsNullOrWhiteSpace( homeNumber ) )
                            {
                                var homePhone = new PhoneNumber();
                                homePhone.CountryCode = PhoneNumber.CleanNumber( pnHome.CountryCode );
                                homePhone.Number = PhoneNumber.CleanNumber( pnHome.Number );
                                SavePhone( homePhone, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), changes );
                            }

                            string cellNumber = PhoneNumber.CleanNumber( pnMobile.Number );
                            if ( _cellPhone != null && !string.IsNullOrWhiteSpace( cellNumber ) )
                            {
                                var cellPhone = new PhoneNumber();
                                cellPhone.CountryCode = PhoneNumber.CleanNumber( pnMobile.CountryCode );
                                cellPhone.Number = PhoneNumber.CleanNumber( pnMobile.Number );
                                SavePhone( cellPhone, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), changes );
                            }
                        }
                    }

                    if ( !isValid )
                    {
                        nbInvalidMessage.Text = sb.ToString();
                        nbInvalidMessage.Visible = true;
                        return;
                    }
                    else
                    {
                        if ( rockContext.SaveChanges() > 0 )
                        {
                            if ( changes.Any() )
                            {
                                HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( Person ),
                                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                    person.Id,
                                    changes );
                            }
                        }
                    }
                }

                // Now that we have a person, we can create the connection request
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( PageParameter( "OpportunityId" ).AsInteger() );
                connectionRequest.PersonAlias = person.PrimaryAlias;
                connectionRequest.Comments = tbComments.Text.Trim();
                connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                connectionRequest.ConnectionState = ConnectionState.Active;
                connectionRequest.ConnectionStatusId = new ConnectionStatusService( rockContext ).Queryable().FirstOrDefault( s => s.IsDefault == true && s.ConnectionTypeId == connectionOpportunity.ConnectionTypeId ).Id;
                connectionRequest.CampusId = ddlCampus.SelectedValueAsId().Value;

                if ( !connectionRequest.IsValid )
                {
                    nbInvalidMessage.Text = "Error submitting request.";
                    nbInvalidMessage.Visible = true;
                    return;
                }

                connectionRequestService.Add( connectionRequest );
                rockContext.SaveChanges();

                // Controls will show warnings
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( PageParameter( "OpportunityId" ).AsInteger() ) );
                mergeFields.Add( "CurrentPerson", CurrentPerson );

                lResponseMessage.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
                lResponseMessage.Visible = true;

                pnlSignup.Visible = false;
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

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            string opportunityId = PageParameter( "OpportunityId" );

            if ( !string.IsNullOrWhiteSpace( opportunityId ) )
            {
                ShowDetail( opportunityId.AsInteger() );
            }
            else
            {
                pnlSignup.Visible = false;
            }
        }

        #endregion

        #region Internal Methods

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
                    return;
                }

                lIcon.Text = string.Format( "<i class='{0}' ></i>", opportunity.IconCssClass );
                lTitle.Text = opportunity.Name.FormatAsHtmlTitle();

                if ( !GetAttributeValue( "DisplayHomePhone" ).AsBoolean() )
                {
                    pnHome.Visible = false;
                }

                if ( !GetAttributeValue( "DisplayMobilePhone" ).AsBoolean() )
                {
                    pnMobile.Visible = false;
                }

                if ( CurrentPerson != null )
                {
                    tbFirstName.Text = CurrentPerson.FirstName.EncodeHtml();
                    tbLastName.Text = CurrentPerson.LastName.EncodeHtml();
                    tbEmail.Text = CurrentPerson.Email.EncodeHtml();

                    if ( _homePhone != null )
                    {
                        var homePhoneNumber = CurrentPerson.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                        if ( homePhoneNumber != null )
                        {
                            pnHome.Number = homePhoneNumber.NumberFormatted;
                            pnHome.CountryCode = homePhoneNumber.CountryCode;
                        }
                    }

                    if ( _cellPhone != null )
                    {
                        var cellPhoneNumber = CurrentPerson.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                        if ( cellPhoneNumber != null )
                        {
                            pnMobile.Number = cellPhoneNumber.NumberFormatted;
                            pnMobile.CountryCode = cellPhoneNumber.CountryCode;
                        }
                    }
                }

                ddlCampus.Items.Clear();
                foreach ( var campus in CampusCache.All() )
                {
                    ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString().ToUpper() ) );
                }

                if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                {
                    var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );
                    var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                    if ( contextCampus != null )
                    {
                        ddlCampus.SetValue( contextCampus.Id.ToString() );
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

                if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() )
                {
                    string pageTitle = string.Format( "{0} Signup", opportunity.PublicName );
                    RockPage.PageTitle = pageTitle;
                    RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                    RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                }
            }
        }

        /// <summary>
        /// Saves the phone.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="person">The person.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SavePhone( PhoneNumber newPhone, Person person, Guid phoneTypeGuid, List<string> changes )
        {
            if ( newPhone != null )
            {
                var numberType = DefinedValueCache.Read( phoneTypeGuid );
                if ( numberType != null )
                {
                    var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );
                    string oldPhoneNumber = string.Empty;

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

                    if ( phone == null || newPhone.NumberFormattedWithCountryCode != phone.NumberFormattedWithCountryCode )
                    {
                        phone.CountryCode = PhoneNumber.CleanNumber( newPhone.CountryCode );
                        phone.Number = PhoneNumber.CleanNumber( newPhone.Number );

                        History.EvaluateChange(
                            changes,
                            string.Format( "{0} Phone", numberType.Value ),
                            oldPhoneNumber,
                            newPhone.NumberFormattedWithCountryCode );
                    }

                }
            }
        }
        #endregion

    }
}