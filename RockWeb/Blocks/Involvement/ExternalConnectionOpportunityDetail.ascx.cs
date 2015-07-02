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
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Involvement
{
    [DisplayName( "External Connection Opportunity Detail" )]
    [Category( "Involvement" )]
    [Description( "Displays the details of the given opportunity for the external website." )]
    [BooleanField( "Display Home Phone", "Whether to display home phone", true )]
    [BooleanField( "Display Mobile Phone", "Whether to display mobile phone", true )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of opportunities.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunityResponseMessage.lava' %}", "", 2 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2" )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49" )]
    public partial class ExternalConnectionOpportunityDetail : RockBlock, IDetailBlock
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
                    pnlDetails.Visible = false;
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
                breadCrumbs.Add( new BreadCrumb( connectionOpportunity.Name, pageReference ) );
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

            ShowDialog( "MemberWorkflowTriggers", true );
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
                pnlDetails.Visible = false;
            }
        }

        protected void dlgConnectionRequest_SaveClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var isValid = true;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "The following fields are required:" );
            sb.AppendLine( "<ul>" );
            var connectionRequest = new ConnectionRequest();
            var person = new Person();

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
                person = personService.Get( CurrentPersonId.Value );
            }

            var connectionRequestService = new ConnectionRequestService( rockContext );
            connectionRequest.PersonAlias = person.PrimaryAlias;
            connectionRequest.Comments = tbNote.Text.Trim();
            connectionRequest.ConnectionOpportunityId = PageParameter( "OpportunityId" ).AsInteger();
            connectionRequest.ConnectionState = ConnectionState.Active;
            connectionRequest.ConnectionStatusId = new ConnectionStatusService( rockContext ).Queryable().FirstOrDefault( s => s.IsDefault == true ).Id;
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

            HideDialog();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( PageParameter( "OpportunityId" ).AsInteger() ) );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lResponseMessage.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
            lResponseMessage.Visible = true;
            btnConnect.Enabled = false;
        }


        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        public void ShowDetail( int opportunityId )
        {
            var rockContext = new RockContext();
            var opportunity = new ConnectionOpportunityService( rockContext ).Get( opportunityId );
            if ( opportunity == null )
            {
                return;
            }

            lIcon.Text = string.Format( "<i class='{0}' ></i>", opportunity.IconCssClass );
            lTitle.Text = opportunity.Name.FormatAsHtmlTitle();

            lDescription.Text = opportunity.Description;

            opportunity.LoadAttributes();
            var attributes = opportunity.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            // Display Attribute Values that have the "Display as Grid Column" flag enabled.
            var attributeCategories = Helper.GetAttributeCategories( attributes );

            var excludedAttributes = attributes.Where( x => !x.IsGridColumn ).Select( x => x.Name ).ToList();

            Rock.Attribute.Helper.AddDisplayControls( opportunity, attributeCategories, phAttributes, excludedAttributes, false );

        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "MEMBERWORKFLOWTRIGGERS":
                    dlgConnectionRequest.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "MEMBERWORKFLOWTRIGGERS":
                    dlgConnectionRequest.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

    }
}