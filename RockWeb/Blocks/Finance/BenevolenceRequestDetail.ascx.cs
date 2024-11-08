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
// </copyright>hlEditStatus
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Encodings.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block for users to create, edit, and view benevolence requests.
    /// </summary>
    [DisplayName( "Benevolence Request Detail" )]
    [Category( "Finance" )]
    [Description( "Block for users to view and edit benevolence requests." )]

    #region Block Attributes
    [SecurityRoleField( "Case Worker Role",
        Description = "The security role to draw case workers from",
        IsRequired = false,
        Key = AttributeKey.CaseWorkerRole,
        Order = 1 )]

    [BooleanField(
        "Display Country Code",
        Key = AttributeKey.DisplayCountryCode,
        Description = "When enabled prepends the country code to all phone numbers.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField( "Display Government Id",
        Key = AttributeKey.DisplayGovernmentId,
        Description = "Display the government identifier.",
        DefaultBooleanValue = true,
        Order = 3 )]

    [BooleanField( "Display Middle Name",
        Key = AttributeKey.DisplayMiddleName,
        Description = "Display the middle name of the person.",
        DefaultBooleanValue = false,
        Order = 4 )]

    [LinkedPage( "Benevolence Request Statement Page",
        Description = "The page which summarizes a benevolence request for printing",
        IsRequired = true,
        Key = AttributeKey.BenevolenceRequestStatementPage,
        Order = 5 )]

    [LinkedPage(
        "Workflow Detail Page",
        Description = "Page used to display details about a workflow.",
        Order = 6,
        Key = AttributeKey.WorkflowDetailPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Order = 7,
        Key = AttributeKey.WorkflowEntryPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY )]

    [CustomDropdownListField(
        "Race",
        Key = AttributeKey.RaceOption,
        Description = "Allow race to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = "Individual",
        Order = 8 )]

    [CustomDropdownListField(
        "Ethnicity",
        Key = AttributeKey.EthnicityOption,
        Description = "Allow Ethnicity to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = "Individual",
        Order = 9 )]
    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "34275D0E-BC7E-4A9C-913E-623D086159A1" )]
    public partial class BenevolenceRequestDetailView : RockBlock
    {
        #region ViewState Keys
        private static class ViewStateKey
        {
            public const string DocumentsState = "DocumentsState";
            public const string NoteTypeId = "NoteTypeId";
        }
        #endregion ViewState

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Badges = "Badges";
            public const string CaseWorkerRole = "CaseWorkerRole";
            public const string DisplayCountryCode = "DisplayCountryCode";
            public const string DisplayMiddleName = "DisplayMiddleName";
            public const string DisplayGovernmentId = "DisplayGovernmentId";
            public const string EnableCallOrigination = "EnableCallOrigination";
            public const string BenevolenceRequestStatementPage = "BenevolenceRequestStatementPage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string RaceOption = "RaceOption";
            public const string EthnicityOption = "EthnicityOption";
        }

        #endregion Attribute Keys

        #region List Source
        private static class ListSource
        {
            public const string HIDE_OPTIONAL_REQUIRED = "Hide,Optional,Required";
        }

        #endregion

        #region Page PageParameterKeys
        private static class PageParameterKey
        {
            public const string BenevolenceRequestId = "BenevolenceRequestId";
            public const string Mode = "Mode";
            public const string NamelessPersonId = "NamelessPersonId";
            public const string PersonGuid = "Person";
            public const string PersonId = "PersonId";
        }
        #endregion Page PageParameterKeys

        #region Fields
        private BenevolenceResult _mocBenevolenceResult;
        private Guid? _caseWorkRoleGuid;
        private List<int> _documentsState;
        private Person _requester;
        private Person _assignedTo;
        private int _benevolenceRequestId;
        private bool _isNewRecord;
        private bool _isExistingRecord;
        #endregion Fields

        #region Properties        
        /// <summary>
        /// Gets the mock benevolence result.
        /// </summary>
        /// <value>The mock benevolence result.</value>
        private BenevolenceResult MockBenevolenceResult
        {
            get
            {
                if ( _mocBenevolenceResult == null )
                {
                    _mocBenevolenceResult = new BenevolenceResult();
                    _mocBenevolenceResult.LoadAttributes();
                }

                return _mocBenevolenceResult;
            }
        }
        #endregion Properties

        #region Base Control Methods        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            SetPageParameters();
            _caseWorkRoleGuid = GetAttributeValue( AttributeKey.CaseWorkerRole ).AsGuidOrNull();

            rptViewRequestWorkflows.ItemCommand += rptRequestWorkflows_ItemCommand;

            InitializeViewResultsSummary();

            if ( _isNewRecord )
            {
                SetEditMode();
            }
            else
            {
                SetViewMode();
            }

            dlEditDocuments.ItemDataBound += dlDocuments_ItemDataBound;
            BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            SetPageParameters();
            _caseWorkRoleGuid = GetAttributeValue( AttributeKey.CaseWorkerRole ).AsGuidOrNull();
            LoadEditDetails();
            LoadViewDetails();
            ConfigureRaceAndEthnicityControls();
            base.OnLoad( e );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _documentsState = ViewState[ViewStateKey.DocumentsState] as List<int>;
            if ( _documentsState == null )
            {
                _documentsState = new List<int>();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.</returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.DocumentsState] = _documentsState;

            return base.SaveViewState();
        }
        #endregion

        #region Edit Events
        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadEditDetails();
            ConfigureRaceAndEthnicityControls();
        }

        /// <summary>
        /// Handles the Click event of the lbEditSave control.
        /// </summary>
        protected void lbEditSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();
                BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );
                BenevolenceResultService benevolenceResultService = new BenevolenceResultService( rockContext );

                BenevolenceRequest benevolenceRequest = null;
                int benevolenceRequestId = PageParameter( PageParameterKey.BenevolenceRequestId ).AsInteger();

                if ( _isExistingRecord )
                {
                    benevolenceRequest = benevolenceRequestService.Get( benevolenceRequestId );
                }

                if ( benevolenceRequest == null )
                {
                    benevolenceRequest = new BenevolenceRequest { Id = 0 };
                }

                benevolenceRequest.FirstName = dtbEditFirstName.Text;
                benevolenceRequest.LastName = dtbEditLastName.Text;
                benevolenceRequest.Email = ebEditEmail.Text;
                benevolenceRequest.RequestText = dtbEditRequestText.Text;
                benevolenceRequest.ResultSummary = dtbEditSummary.Text;
                benevolenceRequest.CampusId = cpEditCampus.SelectedCampusId;
                benevolenceRequest.ProvidedNextSteps = dtbEditProvidedNextSteps.Text;
                benevolenceRequest.GovernmentId = dtbEditGovernmentId.Text;

                if ( lapEditAddress.Location != null )
                {
                    benevolenceRequest.LocationId = lapEditAddress.Location.Id;
                }

                benevolenceRequest.RequestedByPersonAliasId = ppEditPerson.PersonAliasId;

                if ( _caseWorkRoleGuid.HasValue )
                {
                    benevolenceRequest.CaseWorkerPersonAliasId = ddlEditCaseWorker.SelectedValue.AsIntegerOrNull();
                }
                else
                {
                    benevolenceRequest.CaseWorkerPersonAliasId = ppEditCaseWorker.PersonAliasId;
                }

                benevolenceRequest.RequestStatusValueId = dvpEditRequestStatus.SelectedValue.AsIntegerOrNull();
                benevolenceRequest.ConnectionStatusValueId = dvpEditConnectionStatus.SelectedValue.AsIntegerOrNull();

                if ( dpEditRequestDate.SelectedDate.HasValue )
                {
                    benevolenceRequest.RequestDateTime = dpEditRequestDate.SelectedDate.Value;
                }

                var benevolenceTypeId = ddlEditRequestType.SelectedValue.AsIntegerOrNull().GetValueOrDefault( 0 );
                if ( benevolenceTypeId > 0 )
                {
                    benevolenceRequest.BenevolenceTypeId = benevolenceTypeId;
                }

                benevolenceRequest.HomePhoneNumber = pnbEditHomePhone.Number;
                benevolenceRequest.CellPhoneNumber = pnbEditCellPhone.Number;
                benevolenceRequest.WorkPhoneNumber = pnbEditWorkPhone.Number;

                if ( benevolenceRequest.IsValid )
                {
                    if ( _isNewRecord )
                    {
                        benevolenceRequestService.Add( benevolenceRequest );
                    }


                    // load the attributes of the BenevolenceRequestType
                    benevolenceRequest.LoadAttributes( rockContext );
                    avcAttributes.GetEditValues( benevolenceRequest );

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        benevolenceRequest.SaveAttributeValues( rockContext );
                        benevolenceRequest.BenevolenceResults.ToList().ForEach( r => r.SaveAttributeValues( rockContext ) );

                        _benevolenceRequestId = benevolenceRequest.Id;
                    } );

                    // update related documents
                    var documentsService = new BenevolenceRequestDocumentService( rockContext );
                    var binaryFileService = new BinaryFileService( rockContext );

                    // delete any images that were removed
                    var orphanedBinaryFileIds = new List<int>();
                    var documentsInDb = documentsService.Queryable().Where( b => b.BenevolenceRequestId == benevolenceRequest.Id ).ToList();

                    foreach ( var document in documentsInDb.Where( i => !_documentsState.Contains( i.BinaryFileId ) ) )
                    {
                        orphanedBinaryFileIds.Add( document.BinaryFileId );
                        documentsService.Delete( document );
                    }

                    // save documents
                    int documentOrder = 0;
                    foreach ( var binaryFileId in _documentsState )
                    {
                        // Add or Update the activity type
                        var document = documentsInDb.FirstOrDefault( i => i.BinaryFileId == binaryFileId );
                        if ( document == null )
                        {
                            document = new BenevolenceRequestDocument
                            {
                                BenevolenceRequestId = benevolenceRequest.Id
                            };
                            benevolenceRequest.Documents.Add( document );
                        }

                        document.BinaryFileId = binaryFileId;
                        document.Order = documentOrder;
                        documentOrder++;
                    }

                    // Make sure updated binary files are not temporary
                    foreach ( var binaryFile in binaryFileService.Queryable().Where( f => _documentsState.Contains( f.Id ) ) )
                    {
                        binaryFile.IsTemporary = false;
                    }

                    // Delete any orphaned images
                    foreach ( var binaryFile in binaryFileService.Queryable().Where( f => orphanedBinaryFileIds.Contains( f.Id ) ) )
                    {
                        binaryFile.IsTemporary = true;
                    }

                    rockContext.SaveChanges();

                    if ( _isNewRecord )
                    {
                        var queryParams = new Dictionary<string, string>
                        {
                            { "BenevolenceRequestId", this._benevolenceRequestId.ToString() }
                        };

                        NavigateToCurrentPage( queryParams );
                    }
                    else
                    {
                        LoadEditDetails( true );
                        LoadViewDetails();
                        SetViewMode();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEditCreatePerson control.
        /// </summary>
        protected void lbEditCreatePerson_Click( object sender, EventArgs e )
        {
            var firstName = dtbEditFirstName.Text.Trim();
            var lastName = dtbEditLastName.Text.Trim();
            var emailAddress = ebEditEmail.Text.Trim();

            var homePhone = pnbEditHomePhone.Text.Trim();
            var mobilePhone = pnbEditCellPhone.Text.Trim();
            var workPhone = pnbEditWorkPhone.Text.Trim();

            var rockContext = new RockContext();

            var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, emailAddress, mobilePhone );
            var personService = new PersonService( rockContext );

            var persons = personService.FindPersons( personQuery, true );

            Person person = persons?.FirstOrDefault();
            if ( person == null )
            {
                person = new Person { FirstName = firstName, LastName = lastName, Email = emailAddress, RaceValueId = rpRace.SelectedValueAsId(), EthnicityValueId = epEthnicity.SelectedValueAsId() };

                // set the person's connection status using the form fields
                if ( person.ConnectionStatusValueId == null || !person.ConnectionStatusValueId.HasValue )
                {
                    person.ConnectionStatusValueId = dvpEditConnectionStatus.SelectedValue.AsIntegerOrNull();
                }

                // set the person's record status to active.
                if ( person.RecordStatusValueId == null || !person.RecordStatusValueId.HasValue )
                {
                    var newRecordStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
                    if ( newRecordStatus != null )
                    {
                        person.RecordStatusValueId = newRecordStatus.Id;
                    }
                }

                var group = PersonService.SaveNewPerson( person, rockContext );

                SavePhoneNumbers( person.Id, homePhone, mobilePhone, workPhone, rockContext );

                if ( group != null )
                {
                    SaveHomeAddress( rockContext, lapEditAddress.Location, group );
                }

                ppEditPerson.SetValue( person );
            }

            if ( person != null )
            {
                ppEditPerson.SetValue( person );

                var enabledFlag = false;

                dtbEditFirstName.Enabled = enabledFlag;
                dtbEditLastName.Enabled = enabledFlag;
                dvpEditConnectionStatus.Enabled = enabledFlag;
                pnbEditHomePhone.Enabled = enabledFlag;
                pnbEditCellPhone.Enabled = enabledFlag;
                pnbEditWorkPhone.Enabled = enabledFlag;
                ebEditEmail.Enabled = enabledFlag;
                lapEditAddress.Enabled = enabledFlag;
                lbEditCreatePerson.Visible = enabledFlag;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEditCancel control.
        /// </summary>
        protected void lbEditCancel_Click( object sender, EventArgs e )
        {
            if ( _isNewRecord )
            {
                NavigateToParentPage();
            }
            else
            {
                SetViewMode();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbViewPrint control.
        /// </summary>
        protected void lbViewPrint_Click( object sender, EventArgs e )
        {
            if ( _isExistingRecord && !string.IsNullOrEmpty( GetAttributeValue( AttributeKey.BenevolenceRequestStatementPage ) ) )
            {
                NavigateToLinkedPage( AttributeKey.BenevolenceRequestStatementPage, new Dictionary<string, string> { { PageParameterKey.BenevolenceRequestId, _benevolenceRequestId.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppEditPerson control.
        /// </summary>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppEditPerson.PersonId != null )
            {
                Person person = new PersonService( new RockContext() ).Get( ppEditPerson.PersonId.Value );
                if ( person != null )
                {
                    lbEditCreatePerson.Visible = false;

                    // Make sure that the FirstName box gets either FirstName or NickName of person. 
                    if ( !string.IsNullOrWhiteSpace( person.FirstName ) )
                    {
                        dtbEditFirstName.Text = person.FirstName;
                    }
                    else if ( !string.IsNullOrWhiteSpace( person.NickName ) )
                    {
                        dtbEditFirstName.Text = person.NickName;
                    }

                    // If both FirstName and NickName are blank, let them edit it manually
                    dtbEditFirstName.Enabled = string.IsNullOrWhiteSpace( dtbEditFirstName.Text );

                    dtbEditLastName.Text = person.LastName;

                    // If both LastName is blank, let them edit it manually
                    dtbEditLastName.Enabled = string.IsNullOrWhiteSpace( dtbEditLastName.Text );

                    dvpEditConnectionStatus.SetValue( person.ConnectionStatusValueId );
                    dvpEditConnectionStatus.Enabled = false;

                    rpRace.SetValue( person.RaceValueId );
                    rpRace.Enabled = false;

                    epEthnicity.SetValue( person.EthnicityValueId );
                    epEthnicity.Enabled = false;

                    var homePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                    if ( homePhoneType != null )
                    {
                        var homePhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == homePhoneType.Id );
                        if ( homePhone != null )
                        {
                            pnbEditHomePhone.Text = homePhone.NumberFormatted;
                            pnbEditHomePhone.Enabled = false;
                        }
                    }

                    var mobilePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    if ( mobilePhoneType != null )
                    {
                        var mobileNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneType.Id );
                        if ( mobileNumber != null )
                        {
                            pnbEditCellPhone.Text = mobileNumber.NumberFormatted;
                            pnbEditCellPhone.Enabled = false;
                        }
                    }

                    var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                    if ( workPhoneType != null )
                    {
                        var workPhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                        if ( workPhone != null )
                        {
                            pnbEditWorkPhone.Text = workPhone.NumberFormatted;
                            pnbEditWorkPhone.Enabled = false;
                        }
                    }

                    ebEditEmail.Text = person.Email;
                    ebEditEmail.Enabled = false;

                    lapEditAddress.SetValue( person.GetHomeLocation() );
                    lapEditAddress.Enabled = false;

                    // set the campus but not on page load (e will be null) unless from the person profile page (in which case BenevolenceRequestId in the query string will be 0)
                    int? requestId = PageParameter( PageParameterKey.BenevolenceRequestId ).AsIntegerOrNull();

                    if ( !cpEditCampus.SelectedCampusId.HasValue && ( e != null || ( requestId.HasValue && requestId == 0 ) ) )
                    {
                        var personCampus = person.GetCampus();
                        cpEditCampus.SelectedCampusId = personCampus != null ? personCampus.Id : ( int? ) null;
                    }
                }
            }
            else
            {
                dtbEditFirstName.Enabled = true;
                dtbEditLastName.Enabled = true;
                dvpEditConnectionStatus.Enabled = true;
                pnbEditHomePhone.Enabled = true;
                pnbEditCellPhone.Enabled = true;
                pnbEditWorkPhone.Enabled = true;
                ebEditEmail.Enabled = true;
                lapEditAddress.Enabled = true;
                lbEditCreatePerson.Visible = true;
                rpRace.Enabled = true;
                epEthnicity.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fuEditDoc control.
        /// </summary>
        protected void fuEditDoc_FileUploaded( object sender, EventArgs e )
        {
            var fuEditDoc = ( Rock.Web.UI.Controls.FileUploader ) sender;

            if ( fuEditDoc.BinaryFileId.HasValue )
            {
                _documentsState.Add( fuEditDoc.BinaryFileId.Value );
                BindUploadDocuments();
            }
        }

        /// <summary>
        /// Handles the FileRemoved event of the fuEditDoc control.
        /// </summary>
        protected void fuEditDoc_FileRemoved( object sender, FileUploaderEventArgs e )
        {
            var fuEditDoc = ( Rock.Web.UI.Controls.FileUploader ) sender;
            if ( e.BinaryFileId.HasValue )
            {
                _documentsState.Remove( e.BinaryFileId.Value );
                BindUploadDocuments();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the dlDocuments control.
        /// </summary>
        private void dlDocuments_ItemDataBound( object sender, DataListItemEventArgs e )
        {
            Guid binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS.AsGuid();
            var fuEditDoc = e.Item.FindControl( "fuEditDoc" ) as Rock.Web.UI.Controls.FileUploader;
            if ( fuEditDoc != null )
            {
                fuEditDoc.BinaryFileTypeGuid = binaryFileTypeGuid;
            }
        }
        #endregion Edit Events

        #region View Events

        /// <summary>
        /// Handles the ItemCommand event of the rptRequestWorkflows control.
        /// </summary>
        private void rptRequestWorkflows_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "LaunchWorkflow" )
            {
                using ( var rockContext = new RockContext() )
                {
                    var benevolenceWorkflow = new BenevolenceWorkflowService( rockContext ).Get( e.CommandArgument.ToString().AsInteger() );
                    var benvolenceRequest = GetBenevolenceRequest();
                    if ( benvolenceRequest != null && benevolenceWorkflow != null && benevolenceWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        LaunchWorkflow( rockContext, benvolenceRequest, benevolenceWorkflow );
                    }
                }
            }
        }

        /// <summary>
        /// Add a script to the client load event for the current page that will also open a new page for the workflow entry form.
        /// </summary>
        private void RegisterWorkflowDetailPageScript( int workflowTypeId, Guid workflowGuid, string message = null )
        {
            var qryParam = new Dictionary<string, string>
                {
                    { "WorkflowTypeId", workflowTypeId.ToString() },
                    { "WorkflowGuid", workflowGuid.ToString() }
                };

            var url = LinkedPageUrl( AttributeKey.WorkflowEntryPage, qryParam );

            // When the script is executed, it is also removed from the client load event to ensure that it is only run once.
            string script;

            if ( string.IsNullOrEmpty( message ) )
            {
                // Open the workflow detail page.
                script = $@"
                           <script language='javascript' type='text/javascript'> 
                             Sys.Application.add_load(openWorkflowEntryPage);
                               function openWorkflowEntryPage() {{
                                  Sys.Application.remove_load( openWorkflowEntryPage );
                                  window.open('{url}');
                                  }}
                           </script>";
            }
            else
            {
                // Show a modal message dialog, and open the workflow detail page when the dialog is closed.
                message = message.SanitizeHtml( false ).Replace( "'", "&#39;" );
                script = $@"
                           <script language='javascript' type='text/javascript'> 
                             Sys.Application.add_load(openWorkflowEntryPage);
                               function openWorkflowEntryPage() {{
                                  Sys.Application.remove_load( openWorkflowEntryPage );
                                  bootbox.alert({{ message:'{message}',callback: function() {{ window.open('{url}'); }}}});
                                  }}
                          </script>";
            }

            ScriptManager.RegisterStartupScript( upViewWorkflows,
                upViewWorkflows.GetType(),
                "openWorkflowScript",
               script,
                false );

        }

        /// <summary>
        /// Handles the Click event of the lbViewProfile control.
        /// </summary>
        protected void lbViewProfile_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.PersonId, _requester.Id.ToString() }
            };

            if ( _requester.IsNameless() )
            {
                NavigateToPage( new Guid( Rock.SystemGuid.Page.EDIT_PERSON ), queryParams );
            }
            else
            {
                NavigateToPage( new Guid( Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES ), queryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbViewEdit control.
        /// </summary>
        protected void lbViewEdit_Click( object sender, EventArgs e )
        {
            SetEditMode();
        }

        /// <summary>
        /// Handles the Click event of the lbViewCancel control.
        /// </summary>
        protected void lbViewCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptBenevolenceDocuments control.
        /// </summary>
        private void rptBenevolenceDocuments_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var uploadLink = e.Item.FindControl( "lnkViewUploadedFile" ) as HyperLink;

            var benevolenceDocumentService = new BenevolenceRequestDocumentService( new RockContext() );
            var benevolenceRequestDocument = e.Item.DataItem as BenevolenceRequestDocument;
            if ( benevolenceRequestDocument?.BinaryFile != null )
            {
                var getFileUrl = FileUrlHelper.GetFileUrl( benevolenceRequestDocument.BinaryFile.Guid );

                uploadLink.NavigateUrl = getFileUrl;
                uploadLink.Text = benevolenceRequestDocument.BinaryFile.FileName;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gViewResults control.
        /// </summary>
        protected void gViewResults_RowSelected( object sender, RowEventArgs e )
        {
            Guid? infoGuid = e.RowKeyValue as Guid?;
            var benvolenceResultService = new BenevolenceResultService( new RockContext() );
            var benevolenceResult = benvolenceResultService.Get( infoGuid.GetValueOrDefault( Guid.Empty ) );
            if ( benevolenceResult != null )
            {
                dvpResultType.Items.Clear();
                dvpResultType.AutoPostBack = false;
                dvpResultType.Required = true;
                dvpResultType.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ).Id;
                dvpResultType.SetValue( benevolenceResult.ResultTypeValueId );
                dtbResultSummary.Text = benevolenceResult.ResultSummary;
                dtbAmount.Value = benevolenceResult.Amount;
                hfInfoGuid.Value = e.RowKeyValue.ToString();

                phViewResultAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( benevolenceResult, phViewResultAttributes, true, valViewResultsSummary.ValidationGroup, 2 );

                mdViewAddResult.SaveButtonText = "Save";
                mdViewAddResult.Show();
            }
        }

        /// <summary>
        /// Handles the DeleteClick event of the gViewResults control.
        /// </summary>
        private void gViewResults_DeleteClick( object sender, RowEventArgs e )
        {
            Guid? infoGuid = e.RowKeyValue as Guid?;
            var rockContext = new RockContext();
            var benevolenceResultService = new BenevolenceResultService( rockContext );
            var benevolenceResult = benevolenceResultService.Get( infoGuid.GetValueOrDefault( Guid.Empty ) );
            if ( benevolenceResult != null )
            {
                benevolenceResultService.Delete( benevolenceResult );
                rockContext.SaveChanges();
            }

            BindResultsGrid();
        }

        /// <summary>
        /// Handles the PlusIconClick event of the gViewResults control.
        /// </summary>
        private void gViewResults_PlusIconClick( object sender, EventArgs e )
        {
            dvpResultType.Items.Clear();
            dvpResultType.AutoPostBack = false;
            dvpResultType.Required = true;
            dvpResultType.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ).Id;
            dtbResultSummary.Text = string.Empty;
            dtbAmount.Value = null;
            hfInfoGuid.Value = Guid.NewGuid().ToString();

            phViewResultAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( MockBenevolenceResult, phViewResultAttributes, true, valViewResultsSummary.ValidationGroup, 2 );

            mdViewAddResult.SaveButtonText = "Save";
            mdViewAddResult.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the btnAddResult control.
        /// </summary>
        protected void btnAddResult_SaveClick( object sender, EventArgs e )
        {
            int? resultType = dvpResultType.SelectedItem.Value.AsIntegerOrNull();

            Guid? infoGuid = hfInfoGuid.Value.AsGuidOrNull();
            var rockContext = new RockContext();
            var benevolenceRequest = GetBenevolenceRequest( rockContext );
            var benevolenceResult = benevolenceRequest.BenevolenceResults.FirstOrDefault( v => v.Guid == infoGuid.GetValueOrDefault( Guid.Empty ) );

            if ( benevolenceResult != null )
            {
                benevolenceResult.Amount = dtbAmount.Value;
                benevolenceResult.ResultSummary = dtbResultSummary.Text;
                if ( resultType != null )
                {
                    benevolenceResult.ResultTypeValueId = resultType.Value;
                }
            }
            else
            {
                var benevolenceResultInfo = new BenevolenceResult
                {
                    // We need the attributes and values so that we can populate them later
                    Attributes = MockBenevolenceResult.Attributes,
                    AttributeValues = MockBenevolenceResult.AttributeValues,

                    Amount = dtbAmount.Value,

                    ResultSummary = dtbResultSummary.Text
                };
                if ( resultType != null )
                {
                    benevolenceResultInfo.ResultTypeValueId = resultType.Value;
                }

                benevolenceResultInfo.Guid = Guid.NewGuid();
                benevolenceRequest.BenevolenceResults.Add( benevolenceResultInfo );
            }

            rockContext.SaveChanges();
            BindResultsGrid();

            mdViewAddResult.Hide();
            hfInfoGuid.Value = null;
        }
        #endregion View Events

        #region Edit Methods

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        private void SetEditMode()
        {
            this.HideSecondaryBlocks( true );
            pnlViewDetail.Visible = false;
            pnlEditDetail.Visible = true;
        }

        /// <summary>
        /// Sets the view mode.
        /// </summary>
        private void SetViewMode()
        {
            pnlViewDetail.Visible = true;
            pnlEditDetail.Visible = false;
        }

        private void ConfigureRaceAndEthnicityControls()
        {
            rpRace.Visible = GetAttributeValue( AttributeKey.RaceOption ) != "Hide";
            rpRace.Required = GetAttributeValue( AttributeKey.RaceOption ) == "Required";

            epEthnicity.Visible = GetAttributeValue( AttributeKey.EthnicityOption ) != "Hide";
            epEthnicity.Required = GetAttributeValue( AttributeKey.EthnicityOption ) == "Required";
        }

        /// <summary>
        /// Loads the edit details.
        /// </summary>
        /// <param name="reload">if set to <c>true</c> [reload].</param>
        private void LoadEditDetails( bool reload = false )
        {
            if ( !Page.IsPostBack || reload )
            {
                cpEditCampus.Campuses = CampusCache.All();

                BenevolenceRequest benevolenceRequest = null;
                if ( _isExistingRecord )
                {
                    benevolenceRequest = GetBenevolenceRequest();
                    pdEditAuditDetails.SetEntity( benevolenceRequest, ResolveRockUrl( "~" ) );
                }

                if ( benevolenceRequest == null )
                {
                    benevolenceRequest = new BenevolenceRequest { Id = 0 };
                    benevolenceRequest.RequestDateTime = RockDateTime.Now;
                    var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( new RockContext() ).Get( personId.Value );
                        if ( person != null )
                        {
                            benevolenceRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                            benevolenceRequest.RequestedByPersonAlias = person.PrimaryAlias;
                        }
                    }

                    // hide the panel drawer that show created and last modified dates
                    pdEditAuditDetails.Visible = false;
                }

                _requester = benevolenceRequest?.RequestedByPersonAlias?.Person;
                _assignedTo = benevolenceRequest?.CaseWorkerPersonAlias?.Person;

                dtbEditFirstName.Text = benevolenceRequest.FirstName;
                dtbEditLastName.Text = benevolenceRequest.LastName;
                dtbEditGovernmentId.Text = benevolenceRequest.GovernmentId;
                ebEditEmail.Text = benevolenceRequest.Email;
                dtbEditRequestText.Text = benevolenceRequest.RequestText;
                dtbEditSummary.Text = benevolenceRequest.ResultSummary;
                dtbEditProvidedNextSteps.Text = benevolenceRequest.ProvidedNextSteps;
                dpEditRequestDate.SelectedDate = benevolenceRequest.RequestDateTime;

                if ( benevolenceRequest.Campus != null )
                {
                    cpEditCampus.SelectedCampusId = benevolenceRequest.CampusId;
                }
                else
                {
                    cpEditCampus.SelectedIndex = 0;
                }

                if ( benevolenceRequest.RequestedByPersonAlias != null )
                {
                    ppEditPerson.SetValue( benevolenceRequest.RequestedByPersonAlias.Person );
                }
                else
                {
                    ppEditPerson.SetValue( null );
                }

                if ( benevolenceRequest.HomePhoneNumber != null )
                {
                    pnbEditHomePhone.Text = benevolenceRequest.HomePhoneNumber;
                }

                if ( benevolenceRequest.CellPhoneNumber != null )
                {
                    pnbEditCellPhone.Text = benevolenceRequest.CellPhoneNumber;
                }

                if ( benevolenceRequest.WorkPhoneNumber != null )
                {
                    pnbEditWorkPhone.Text = benevolenceRequest.WorkPhoneNumber;
                }

                lapEditAddress.SetValue( benevolenceRequest.Location );

                LoadDropDowns( benevolenceRequest );

                if ( benevolenceRequest.RequestStatusValueId != null )
                {
                    dvpEditRequestStatus.SetValue( benevolenceRequest.RequestStatusValueId );

                    if ( benevolenceRequest.RequestStatusValue.Value == "Approved" )
                    {
                        hlEditStatus.Text = "Approved";
                        hlEditStatus.LabelType = LabelType.Success;
                    }

                    if ( benevolenceRequest.RequestStatusValue.Value == "Denied" )
                    {
                        hlEditStatus.Text = "Denied";
                        hlEditStatus.LabelType = LabelType.Danger;
                    }
                }

                if ( benevolenceRequest.ConnectionStatusValueId != null )
                {
                    dvpEditConnectionStatus.SetValue( benevolenceRequest.ConnectionStatusValueId );
                }

                if ( _caseWorkRoleGuid.HasValue )
                {
                    ddlEditCaseWorker.SetValue( benevolenceRequest.CaseWorkerPersonAliasId );
                }
                else
                {
                    if ( benevolenceRequest.CaseWorkerPersonAlias != null )
                    {
                        ppEditCaseWorker.SetValue( benevolenceRequest.CaseWorkerPersonAlias.Person );
                    }
                    else
                    {
                        ppEditCaseWorker.SetValue( null );
                    }
                }

                _documentsState = benevolenceRequest.Documents.OrderBy( s => s.Order ).Select( s => s.BinaryFileId ).ToList();
                BindUploadDocuments();

                avcAttributes.AddEditControls( benevolenceRequest, Rock.Security.Authorization.EDIT, CurrentPerson );

                // call the OnSelectPerson of the person picker which will update the UI based on the selected person
                ppPerson_SelectPerson( null, null );
            }
            else
            {
                var benevolenceRequest = GetBenevolenceRequest();
                benevolenceRequest.BenevolenceTypeId = ddlEditRequestType.SelectedValue.ToIntSafe();
                confirmEditExit.Enabled = true;
            }
        }

        /// <summary>
        /// Binds the upload documents.
        /// </summary>
        private void BindUploadDocuments()
        {
            var benevolenceTypeId = ddlEditRequestType.SelectedValue.ToIntSafe();
            if ( benevolenceTypeId == 0 )
            {
                return;
            }

            var benevolenceType = new BenevolenceTypeService( new RockContext() ).Get( benevolenceTypeId );
            var maxDocuments = benevolenceType.AdditionalSettingsJson?.FromJsonOrNull<BenevolenceType.AdditionalSettings>().MaximumNumberOfDocuments ?? 6;

            var ds = _documentsState.ToList();
            if ( ds.Count() < maxDocuments )
            {
                ds.Add( 0 );
            }

            dlEditDocuments.DataSource = ds;
            dlEditDocuments.DataBind();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        /// <param name="benevolenceRequest">The benevolence request.</param>
        private void LoadDropDowns( BenevolenceRequest benevolenceRequest )
        {
            dvpEditRequestStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS ) ).Id;
            dvpEditConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;

            var rockContext = new RockContext();

            if ( _caseWorkRoleGuid.HasValue )
            {
                var personList = new GroupMemberService( rockContext )
                    .Queryable( "Person, Group" )
                    .Where( gm => gm.Group.Guid == _caseWorkRoleGuid.Value )
                    .Select( gm => gm.Person )
                    .ToList();

                if ( benevolenceRequest.CaseWorkerPersonAlias != null &&
                    benevolenceRequest.CaseWorkerPersonAlias.Person != null &&
                    !personList.Select( p => p.Id ).ToList().Contains( benevolenceRequest.CaseWorkerPersonAlias.Person.Id ) )
                {
                    personList.Add( benevolenceRequest.CaseWorkerPersonAlias.Person );
                }

                ddlEditCaseWorker.DataSource = personList.OrderBy( p => p.NickName ).ThenBy( p => p.LastName ).ToList();
                ddlEditCaseWorker.DataTextField = "FullName";
                ddlEditCaseWorker.DataValueField = "PrimaryAliasId";
                ddlEditCaseWorker.DataBind();
                ddlEditCaseWorker.Items.Insert( 0, new ListItem() );

                ppEditCaseWorker.Visible = false;
                ddlEditCaseWorker.Visible = true;
            }
            else
            {
                ppEditCaseWorker.Visible = true;
                ddlEditCaseWorker.Visible = false;
            }

            var benevolenceTypeList = new BenevolenceTypeService( rockContext )
                .Queryable()
                .OrderBy( p => p.Name )
                .ToList();

            // Load Benevolence Types and set the value from the Benevolence Request
            ddlEditRequestType.DataSource = benevolenceTypeList;
            ddlEditRequestType.DataTextField = "Name";
            ddlEditRequestType.DataValueField = "Id";
            ddlEditRequestType.SelectedValue = benevolenceRequest?.BenevolenceType?.Id.ToString();
            ddlEditRequestType.DataBind();
            ddlEditRequestType.Items.Insert( 0, new ListItem() );

            ddlEditRequestType.Enabled = _isNewRecord;

            if ( _isNewRecord && benevolenceTypeList.Count == 1 )
            {
                ddlEditRequestType.SelectedValue = benevolenceTypeList.FirstOrDefault().Id.ToString();
            }
        }

        /// <summary>
        /// Saves the phone numbers.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="homePhoneNumber">The home phone number.</param>
        /// <param name="mobilePhoneNumber">The mobile phone number.</param>
        /// <param name="workPhoneNumber">The work phone number.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SavePhoneNumbers( int personId, string homePhoneNumber, string mobilePhoneNumber, string workPhoneNumber, RockContext rockContext )
        {
            var savable = false;

            var phoneNumberService = new PhoneNumberService( rockContext );

            string mobilePhone = PhoneNumber.CleanNumber( mobilePhoneNumber );
            var mobilePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            if ( mobilePhoneType != null )
            {
                var phoneNumber = phoneNumberService.Queryable()
                    .Where( n =>
                        n.PersonId == personId &&
                        n.NumberTypeValueId.HasValue &&
                        n.NumberTypeValueId.Value == mobilePhoneType.Id )
                    .FirstOrDefault();

                if ( phoneNumber == null )
                {
                    if ( mobilePhone.IsNotNullOrWhiteSpace() )
                    {
                        phoneNumber = new PhoneNumber();
                        phoneNumberService.Add( phoneNumber );

                        phoneNumber.PersonId = personId;
                        phoneNumber.NumberTypeValueId = mobilePhoneType.Id;
                        phoneNumber.Number = mobilePhone;

                        savable = true;
                    }
                }
            }

            string homePhone = PhoneNumber.CleanNumber( homePhoneNumber );
            var homePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );

            if ( homePhoneType != null )
            {
                var phoneNumber = phoneNumberService.Queryable()
                    .Where( n =>
                        n.PersonId == personId &&
                        n.NumberTypeValueId.HasValue &&
                        n.NumberTypeValueId.Value == homePhoneType.Id )
                    .FirstOrDefault();

                if ( phoneNumber == null )
                {
                    if ( homePhone.IsNotNullOrWhiteSpace() )
                    {
                        phoneNumber = new PhoneNumber();
                        phoneNumberService.Add( phoneNumber );

                        phoneNumber.PersonId = personId;
                        phoneNumber.NumberTypeValueId = homePhoneType.Id;
                        phoneNumber.Number = homePhone;

                        savable = true;
                    }
                }
            }

            string workPhone = PhoneNumber.CleanNumber( workPhoneNumber );
            var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );

            if ( workPhoneType != null )
            {
                var phoneNumber = phoneNumberService.Queryable()
                    .Where( n =>
                        n.PersonId == personId &&
                        n.NumberTypeValueId.HasValue &&
                        n.NumberTypeValueId.Value == workPhoneType.Id )
                    .FirstOrDefault();

                if ( phoneNumber == null )
                {
                    if ( workPhone.IsNotNullOrWhiteSpace() )
                    {
                        phoneNumber = new PhoneNumber();
                        phoneNumberService.Add( phoneNumber );

                        phoneNumber.PersonId = personId;
                        phoneNumber.NumberTypeValueId = workPhoneType.Id;
                        phoneNumber.Number = workPhone;

                        savable = true;
                    }
                }
            }

            if ( savable )
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Saves the home address.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="location">The location.</param>
        /// <param name="family">The family.</param>
        private void SaveHomeAddress( RockContext rockContext, Location location, Group family )
        {
            // Save the family address
            var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            if ( homeLocationType != null && location != null )
            {
                if ( location.Street1.IsNotNullOrWhiteSpace() && location.City.IsNotNullOrWhiteSpace() )
                {
                    location = new LocationService( rockContext ).Get(
                        location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country, family, true );
                }
                else
                {
                    location = null;
                }

                // Check to see if family has an existing home address
                var groupLocation = family.GroupLocations
                    .FirstOrDefault( l =>
                        l.GroupLocationTypeValueId.HasValue &&
                        l.GroupLocationTypeValueId.Value == homeLocationType.Id );

                if ( location != null )
                {
                    if ( groupLocation == null || groupLocation.LocationId != location.Id )
                    {
                        // If family does not currently have a home address or it is different than the one entered, add a new address (move old address to prev)
                        GroupService.AddNewGroupAddress( rockContext, family, homeLocationType.Guid.ToString(), location, true, string.Empty, true, true );
                    }
                }

                rockContext.SaveChanges();
            }
        }

        #endregion Edit Methods

        #region View Methods

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="unlisted">if set to <c>true</c> [unlisted].</param>
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">The number.</param>
        /// <param name="phoneNumberTypeId">The phone number type identifier.</param>
        /// <param name="smsEnabled">if set to <c>true</c> [SMS enabled].</param>
        /// <returns>System.String.</returns>
        protected string FormatPhoneNumber( bool unlisted, object countryCode, object number, int phoneNumberTypeId, bool smsEnabled = false )
        {
            var originationEnabled = GetAttributeValue( AttributeKey.EnableCallOrigination ).AsBoolean();

            string formattedNumber = "Unlisted";

            string cc = countryCode as string ?? string.Empty;
            string n = number as string ?? string.Empty;

            if ( !unlisted )
            {
                if ( GetAttributeValue( AttributeKey.DisplayCountryCode ).AsBoolean() )
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n, true );
                }
                else
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n );
                }
            }

            var phoneType = DefinedValueCache.Get( phoneNumberTypeId );
            if ( phoneType != null )
            {
                string phoneMarkup = formattedNumber;

                if ( originationEnabled )
                {
                    var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( CurrentPerson );

                    if ( pbxComponent != null )
                    {
                        var jsScript = string.Format( "javascript: Rock.controls.pbx.originate('{0}', '{1}', '{2}','{3}','{4}');", CurrentPerson.Guid, number.ToString(), CurrentPerson.FullName, _requester.FullName, formattedNumber );
                        phoneMarkup = string.Format( "<a class='originate-call js-originate-call' href=\"{0}\">{1}</a>", jsScript, formattedNumber );
                    }
                    else if ( RockPage.IsMobileRequest )
                    {
                        // if the page is being loaded locally then add the tel:// link
                        phoneMarkup = string.Format( "<a href=\"tel://{0}\">{1}</a>", n, formattedNumber );
                    }
                }

                formattedNumber = string.Format( "{0} <small>{1}</small>", phoneMarkup, phoneType.Value );
            }

            return formattedNumber;
        }

        /// <summary>
        /// Sets the name of the person.
        /// </summary>
        private void DisplayPersonName()
        {
            var benevolenceRequest = GetBenevolenceRequest();

            // Check if this record represents a Business.
            bool isBusiness = false;

            // Get the Display Name.
            string nameText;

            if ( _requester == null )
            {
                nameText = $"<span class='first-word nickname'>{benevolenceRequest.FirstName}</span> <span class='lastname'>{ benevolenceRequest.LastName}</span>";
                lName.Text = nameText;
                return;
            }

            if ( _requester?.RecordTypeValueId != null && _requester.RecordTypeValueId.HasValue )
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

                isBusiness = _requester.RecordTypeValueId.Value == recordTypeValueIdBusiness;
            }

            if ( isBusiness )
            {
                nameText = _requester.LastName;
            }
            else
            {
                if ( GetAttributeValue( AttributeKey.DisplayMiddleName ).AsBoolean() && !string.IsNullOrWhiteSpace( _requester.MiddleName ) )
                {
                    nameText = $"<span class='first-word nickname'>{ _requester.NickName}</span> <span class='middlename'>{_requester.MiddleName}</span> <span class='lastname'>{ _requester.LastName}</span>";
                }
                else
                {
                    nameText = $"<span class='first-word nickname'>{_requester.NickName}</span> <span class='lastname'>{ _requester.LastName}</span>";
                }

                // Prefix with Title if they have a Title with IsFormal=True
                if ( _requester.TitleValueId.HasValue )
                {
                    var personTitleValue = DefinedValueCache.Get( _requester.TitleValueId.Value );
                    if ( personTitleValue != null && personTitleValue.GetAttributeValue( "IsFormal" ).AsBoolean() )
                    {
                        nameText = $"<span class='title'>{personTitleValue.Value + nameText}</span>";
                    }
                }

                // Add First Name if different from NickName.
                if ( _requester.NickName != _requester.FirstName )
                {
                    if ( !string.IsNullOrWhiteSpace( _requester.FirstName ) )
                    {
                        nameText += $" <span class='firstname'>({_requester.FirstName})</span>";
                    }
                }

                // Add Suffix.
                if ( _requester.SuffixValueId.HasValue )
                {
                    var suffix = DefinedValueCache.Get( _requester.SuffixValueId.Value );
                    if ( suffix != null )
                    {
                        nameText += " " + suffix.Value;
                    }
                }

                // Add Previous Names. 
                using ( var rockContext = new RockContext() )
                {
                    var previousNames = _requester.GetPreviousNames( rockContext ).Select( a => a.LastName );

                    if ( previousNames.Any() )
                    {
                        nameText += $"{Environment.NewLine}<span class='previous-names'>(Previous Names: {previousNames.ToList().AsDelimited( ", " )})</span>";
                    }
                }
            }

            lName.Text = nameText;
        }

        /// <summary>
        /// Shows the workflow details.
        /// </summary>
        private void ShowWorkflowDetails()
        {
            var benevolenceRequest = GetBenevolenceRequest();
            if ( benevolenceRequest != null )
            {
                var benevolenceWorkflows = benevolenceRequest.BenevolenceType.BenevolenceWorkflows.Union( benevolenceRequest.BenevolenceType.BenevolenceWorkflows );
                var manualWorkflows = benevolenceWorkflows
                    .Where( w =>
                        w.TriggerType == BenevolenceWorkflowTriggerType.Manual && w.WorkflowType != null )
                    .OrderBy( w => w.WorkflowType.Name )
                    .Distinct();

                var authorizedWorkflows = new List<BenevolenceWorkflow>();
                foreach ( var manualWorkflow in manualWorkflows )
                {
                    if ( ( manualWorkflow.WorkflowType.IsActive ?? true ) && manualWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        authorizedWorkflows.Add( manualWorkflow );
                    }
                }

                upViewWorkflows.Visible = authorizedWorkflows.Any();

                if ( authorizedWorkflows.Any() )
                {
                    rptViewRequestWorkflows.DataSource = authorizedWorkflows.ToList();
                    rptViewRequestWorkflows.DataBind();
                }
            }
        }

        /// <summary>
        /// Shows the user profile details.
        /// </summary>
        private void ShowUserProfileDetails()
        {
            var benevolenceRequest = GetBenevolenceRequest();
            _requester = benevolenceRequest?.RequestedByPersonAlias?.Person;
            _assignedTo = benevolenceRequest?.CaseWorkerPersonAlias?.Person;

            hlViewBenevolenceType.Text = $"{benevolenceRequest?.BenevolenceType?.Name}";
            hlViewBenevolenceType.LabelType = LabelType.Type;

            var campus = benevolenceRequest?.Campus;

            hlViewCampus.LabelType = LabelType.Campus;
            hlViewCampus.Text = ( campus != null ? campus.Name : string.Empty );

            switch ( benevolenceRequest?.RequestStatusValue?.Value.ToUpper() )
            {
                case "APPROVED":
                    hlViewStatus.LabelType = LabelType.Success;
                    break;
                case "PENDING":
                    hlViewStatus.LabelType = LabelType.Default;
                    break;
                case "DENIED":
                    hlViewStatus.LabelType = LabelType.Danger;
                    break;
            }

            hlViewStatus.Text = $"{benevolenceRequest?.RequestStatusValue?.Value}";

            DisplayPersonName();

            // Setup Image
            if ( _assignedTo != null )
            {
                imgViewAssignedTo.ImageUrl = Person.GetPersonPhotoUrl( _assignedTo );
            }
            else
            {
                imgViewAssignedTo.ImageUrl = "/Assets/Images/person-no-photo-unknown.svg";
            }

            if ( _requester == null )
            {
                imgViewRequestor.ImageUrl = "/Assets/Images/person-no-photo-unknown.svg";
                lViewNotLinkedProfile.Visible = true;
                lbViewProfile.Visible = false;
                lViewNotLinkedProfile.Text = "<small class='text-muted'> Record Not Linked</small>";

                if ( benevolenceRequest.HomePhoneNumber.IsNotNullOrWhiteSpace() || benevolenceRequest.WorkPhoneNumber.IsNotNullOrWhiteSpace() || benevolenceRequest.CellPhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    var phoneNumbers = new List<PhoneNumber>();
                    var definedTypeService = new DefinedTypeService( new RockContext() );
                    var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );

                    if ( benevolenceRequest.HomePhoneNumber.IsNotNullOrWhiteSpace() )
                    {
                        var phoneTypeValue = phoneNumberTypes.DefinedValues.Where( v => v.Guid == new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).FirstOrDefault();
                        phoneNumbers.Add( new PhoneNumber { Number = benevolenceRequest.HomePhoneNumber, IsUnlisted = false, CountryCode = PhoneNumber.DefaultCountryCode(), NumberTypeValueId = phoneTypeValue.Id, IsMessagingEnabled = false } );
                    }

                    if ( benevolenceRequest.WorkPhoneNumber.IsNotNullOrWhiteSpace() )
                    {
                        var phoneTypeValue = phoneNumberTypes.DefinedValues.Where( v => v.Guid == new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).FirstOrDefault();
                        phoneNumbers.Add( new PhoneNumber { Number = benevolenceRequest.WorkPhoneNumber, IsUnlisted = false, CountryCode = PhoneNumber.DefaultCountryCode(), NumberTypeValueId = phoneTypeValue.Id, IsMessagingEnabled = false } );
                    }

                    if ( benevolenceRequest.CellPhoneNumber.IsNotNullOrWhiteSpace() )
                    {
                        var phoneTypeValue = phoneNumberTypes.DefinedValues.Where( v => v.Guid == new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).FirstOrDefault();
                        phoneNumbers.Add( new PhoneNumber { Number = benevolenceRequest.CellPhoneNumber, IsUnlisted = false, CountryCode = PhoneNumber.DefaultCountryCode(), NumberTypeValueId = phoneTypeValue.Id, IsMessagingEnabled = false } );
                    }

                    if ( phoneNumberTypes.DefinedValues.Any() )
                    {
                        var phoneNumberTypeIds = phoneNumberTypes.DefinedValues.Select( a => a.Id ).ToList();
                        var phoneNumbersOrdered = phoneNumbers.OrderBy( a => phoneNumberTypeIds.IndexOf( a.NumberTypeValueId.Value ) );
                        phoneNumbers = phoneNumbers?.ToList();
                    }

                    rptViewPhones.DataSource = phoneNumbers;
                    rptViewPhones.DataBind();
                }

                lViewEmail.Text = $"<a href='mailto:{benevolenceRequest?.Email}?subject=Benevolence Request - {UrlEncoder.Default.Encode( benevolenceRequest.BenevolenceType.Name )}'>{benevolenceRequest?.Email}</a>";
                lViewAddress.Text = benevolenceRequest.Location?.FormattedHtmlAddress;
            }
            else
            {
                imgViewRequestor.ImageUrl = Person.GetPersonPhotoUrl( _requester );
                lViewNotLinkedProfile.Visible = false;
                lbViewProfile.Visible = true;
                lbViewProfile.Text = "<small> View Profile</small>";
                if ( _requester.PhoneNumbers != null )
                {
                    var phoneNumbers = _requester.PhoneNumbers.AsEnumerable();
                    var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
                    if ( phoneNumberTypes.DefinedValues.Any() )
                    {
                        var phoneNumberTypeIds = phoneNumberTypes.DefinedValues.Select( a => a.Id ).ToList();
                        phoneNumbers = phoneNumbers.OrderBy( a => phoneNumberTypeIds.IndexOf( a.NumberTypeValueId.Value ) );
                    }

                    rptViewPhones.DataSource = phoneNumbers;
                    rptViewPhones.DataBind();
                }

                // Get the connection status badge
                var badgesEntity = new PersonService( new RockContext() ).Get( _requester.Guid );
                var badge = BadgeCache.Get( new Guid( "66972BFF-42CD-49AB-9A7A-E1B9DECA4EBF" ) );
                if ( badge != null )
                {
                    blViewStatus.BadgeTypes.Clear();
                    blViewStatus.BadgeTypes.Add( badge );
                    blViewStatus.Entity = badgesEntity;
                }

                Rock.Web.PageReference communicationPageReference;
                var pageService = new PageService( new RockContext() );
                var newCommunicationPage = pageService.Get( new Guid( Rock.SystemGuid.Page.NEW_COMMUNICATION ) );
                if ( newCommunicationPage != null )
                {
                    communicationPageReference = new Rock.Web.PageReference( newCommunicationPage.Id );
                    lViewEmail.Text = _requester.GetEmailTag( ResolveRockUrl( "/" ), communicationPageReference );
                }

                lViewAddress.Text = _requester.GetHomeLocation( new RockContext() )?.FormattedHtmlAddress;
            }

            lViewAssignedTo.Text = _assignedTo?.FullName ?? "Not assigned";
            lViewRequestDate.Text = $"{benevolenceRequest.RequestDateTime.ToShortDateString()} <small>({RockDateTime.Now.Subtract( benevolenceRequest.RequestDateTime ).Days} days)</small>";

            if ( ( benevolenceRequest?.GovernmentId?.IsNotNullOrWhiteSpace() ).GetValueOrDefault( false ) && GetAttributeValue( AttributeKey.DisplayGovernmentId ).AsBoolean() )
            {
                lViewGovernmentId.Visible = true;
                lViewGovernmentId.Text = $"{benevolenceRequest?.GovernmentId}";
            }
        }

        /// <summary>
        /// Shows the lava details.
        /// </summary>
        private void ShowLavaDetails()
        {
            var benevolenceRequest = GetBenevolenceRequest();
            if ( lViewBenevolenceTypeLava != null && benevolenceRequest != null )
            {
                var benevolenceTypeTemplate = benevolenceRequest.BenevolenceType.RequestLavaTemplate;
                if ( benevolenceTypeTemplate.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions() );
                    mergeFields.Add( "BenevolenceRequest", benevolenceRequest );
                    lViewBenevolenceTypeLava.Text = benevolenceTypeTemplate.ResolveMergeFields( mergeFields );
                }
            }
        }

        /// <summary>
        /// Shows the request details.
        /// </summary>
        private void ShowRequestDetails()
        {
            var benevolenceRequest = GetBenevolenceRequest();
            benevolenceRequest.LoadAttributes();

            divViewAttributes.Visible = ( benevolenceRequest?.Attributes?.Any() ).GetValueOrDefault( false );

            lViewBenevolenceTypeDescription.Text = $"{benevolenceRequest?.RequestText}";

            avcViewBenevolenceTypeAttributes.AddDisplayControls( benevolenceRequest, Rock.Security.Authorization.VIEW, CurrentPerson );

            var documentList = benevolenceRequest?.Documents.ToList();

            rptViewBenevolenceDocuments.ItemDataBound += rptBenevolenceDocuments_ItemDataBound;
            rptViewBenevolenceDocuments.DataSource = documentList;
            rptViewBenevolenceDocuments.DataBind();

            divViewRelatedDocs.Visible = documentList.Any();

            ConfigureRaceAndEthnicityControls();
        }

        /// <summary>
        /// Initializes the view results summary.
        /// </summary>
        private void InitializeViewResultsSummary()
        {
            var benevolenceRequest = GetBenevolenceRequest();
            var showResults = ( benevolenceRequest?.BenevolenceType?.ShowFinancialResults ).GetValueOrDefault( false );
            pnlResults.Visible = showResults;
            gViewResults.DataKeyNames = new string[] { "Guid" };
            gViewResults.Actions.AddClick += gViewResults_PlusIconClick;
            gViewResults.Actions.ShowAdd = true;
            gViewResults.IsDeleteEnabled = true;

            benevolenceRequest?.BenevolenceResults?.LoadAttributes();

            AddResultsColumns();
        }

        /// <summary>
        /// Shows the request summary.
        /// </summary>
        private void ShowRequestSummary()
        {
            var benevolenceRequest = GetBenevolenceRequest();

            lViewSummaryResults.Text = $"<small><p>{benevolenceRequest.ResultSummary}</p></small>";
            lViewSummaryNextSteps.Text = $"<small><p>{benevolenceRequest.ProvidedNextSteps}</p></small>";

            BindResultsGrid();
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="benevolenceRequest">The benevolence request.</param>
        /// <param name="benevolenceWorkflow">The benevolence workflow.</param>
        private void LaunchWorkflow( RockContext rockContext, BenevolenceRequest benevolenceRequest, BenevolenceWorkflow benevolenceWorkflow )
        {
            if ( benevolenceRequest != null && benevolenceWorkflow != null )
            {
                var workflowType = benevolenceWorkflow.WorkflowTypeCache;
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, benevolenceWorkflow.WorkflowType.WorkTerm, rockContext );
                    if ( workflow != null )
                    {
                        List<string> workflowErrors;

                        var workflowService = new Rock.Model.WorkflowService( rockContext );

                        if ( workflowService.Process( workflow, benevolenceRequest, out workflowErrors ) )
                        {

                            var message = $"A '{workflowType.Name}' workflow has been started.";
                            RegisterWorkflowDetailPageScript( workflowType.Id, workflow.Guid, message );

                        }
                        else
                        {
                            mdViewWorkflowLaunched.Show( "Workflow Processing Error(s):<ul><li>" + workflowErrors?.AsDelimited( "</li><li>" ) + "</li></ul>", ModalAlertType.Information );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Binds the results grid.
        /// </summary>
        private void BindResultsGrid()
        {
            var benevolenceRequest = GetBenevolenceRequest();
            gViewResults.DataSource = benevolenceRequest.BenevolenceResults?.ToList();
            gViewResults.DataBind();
        }

        /// <summary>
        /// Adds columns to the results grid 
        /// </summary>
        private void AddResultsColumns()
        {
            var attributes = MockBenevolenceResult.Attributes.Select( a => a.Value ).Where( a => a.IsGridColumn ).ToList();

            foreach ( var attribute in attributes )
            {
                bool columnExists = gViewResults.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField
                    {
                        DataField = attribute.Key,
                        AttributeId = attribute.Id,
                        HeaderText = attribute.Name
                    };

                    var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    gViewResults.Columns.Add( boundField );
                }
            }

            // Add delete column
            var deleteField = new DeleteField();
            gViewResults.Columns.Add( deleteField );
            deleteField.Click += gViewResults_DeleteClick;
        }

        /// <summary>
        /// Gets the benevolence request.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>BenevolenceRequest.</returns>
        private BenevolenceRequest GetBenevolenceRequest( RockContext rockContext = null )
        {
            var benevolenceRequest = new BenevolenceRequestService( rockContext ?? new RockContext() ).Get( _benevolenceRequestId );
            return benevolenceRequest ?? new BenevolenceRequest { Id = 0 };
        }

        /// <summary>
        /// Loads the view details.
        /// </summary>
        private void LoadViewDetails()
        {
            if ( _isNewRecord )
            {
                return;
            }

            ShowUserProfileDetails();
            ShowWorkflowDetails();
            ShowLavaDetails();
            ShowRequestDetails();
            ShowRequestSummary();
        }

        #endregion View Methods

        #region Shared Methods        

        /// <summary>
        /// Sets the page parameters.
        /// </summary>
        private void SetPageParameters()
        {
            _benevolenceRequestId = PageParameter( PageParameterKey.BenevolenceRequestId ).AsInteger();
            _isNewRecord = _benevolenceRequestId == 0;
            _isExistingRecord = _benevolenceRequestId > 0;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Class AdditionalSettings.
        /// </summary>
        public class AdditionalSettings
        {
            /// <summary>
            /// Gets or sets the maximum number of documents.
            /// </summary>
            /// <value>The maximum number of documents.</value>
            public int MaximumNumberOfDocuments { get; set; }
        }

        #endregion

        protected void ddlEditRequestType_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindUploadDocuments();
        }
    }
}