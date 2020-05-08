// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

/*
 * BEMA Modified Core Block ( v9.2.1)
 * Version Number based off of RockVersion.RockHotFixVersion.BemaFeatureVersion
 * 
 * Additional Features:
 * - FE1) Added Ability to launch a workflow when a benevolence request is saved
 * - FE2) Added Ability to set how many files can be attached to the request
 * - UI1) Added Ability to customize the label on Case Worker controls
 * - UI2) Added Ability to customize the  label on Government Id controls
 * - UI3) Added Ability to show all statuses as labels
 * - UI4) The "Assigned" Label Type can be set
 * - UI5) The "Initiated" Label Type can be set
 */

namespace RockWeb.Plugins.com_bemaservices.Finance
{
    /// <summary>
    /// Block for users to create, edit, and view benevolence requests.
    /// </summary>
    [DisplayName( "Benevolence Request Detail" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Block for users to create, edit, and view benevolence requests." )]
    [SecurityRoleField( "Case Worker Role", "The security role to draw case workers from", false, "", "", 0 )]
    [LinkedPage( "Benevolence Request Statement Page", "The page which summarizes a benevolence request for printing", true )]

    /* BEMA.FE1.Start */
    [WorkflowTypeField(
            "Workflow",
            key: BemaAttributeKey.Workflow,
            description: "Workflow to be triggered when the page is saved",
            allowMultiple: true,
            required: false,
            defaultWorkflowTypeGuid: "",
            Category = "BEMA Additional Features" )]
    /* BEMA.FE1.End */

    /* BEMA.FE2.Start */
    [IntegerField(
        "Maximum Number of Attachments",
        Key = BemaAttributeKey.MaximumAttachmentNumber,
        Description = "The maximum number of attachments allowed.",
        IsRequired = true,
        DefaultValue = "6",
        Category = "BEMA Additional Features" )]
    // UMC Value = "8"
    /* BEMA.FE2.End */

    /* BEMA.UI1.Start */
    [TextField(
        "Case Worker Label",
        Key = BemaAttributeKey.CaseWorkerLabel,
        Description = "The Label on the Case Worker controls.",
        IsRequired = true,
        DefaultValue = "Case Worker",
        Category = "BEMA Additional Features" )]
    // UMC Value = "Case Manager"
    /* BEMA.UI1.End */

    /* BEMA.UI2.Start */
    [TextField(
        "Government Id Label",
        Key = BemaAttributeKey.GovernmentIdLabel,
        Description = "The Label on the Government Id controls.",
        IsRequired = true,
        DefaultValue = "Government ID",
        Category = "BEMA Additional Features" )]
    // UMC Value = "Primary Language"
    /* BEMA.UI2.End */

    /* BEMA.UI3.Start */
    [BooleanField(
        "Are all statuses shown as labels?",
        Key = BemaAttributeKey.AreAllStatusesShownAsLabels,
        DefaultValue = "False",
        Category = "BEMA Additional Features" )]
    // UMC Value = true
    /* BEMA.UI3.End */

    /* BEMA.UI4.Start */
    [EnumField(
        "Assigned Label Type",
        description: "What Label Type should be used for the Initiated Label?",
        enumSourceType: typeof( LabelType ),
        DefaultValue = "3", //Info
        Key = BemaAttributeKey.AssignedLabelType,
        Category = "BEMA Additional Features"
        )]
    /* BEMA.UI4.End */

    /* BEMA.UI5.Start */
    [EnumField(
        "Initiated Label Type",
        description: "What Label Type should be used for the Initiated Label?",
        enumSourceType: typeof( LabelType ),
        DefaultValue = "4", //Warning
        Key = BemaAttributeKey.InitiatedLabelType,
        Category = "BEMA Additional Features"
        )]
    /* BEMA.UI5.End */

    public partial class BenevolenceRequestDetail : Rock.Web.UI.RockBlock
    {
        /* BEMA.Start */
        #region BEMA Attribute Keys
        private static class BemaAttributeKey
        {
            public const string Workflow = "Workflow";
            public const string CaseWorkerLabel = "CaseWorkerLabel";
            public const string GovernmentIdLabel = "GovernmentIdLabel";
            public const string AreAllStatusesShownAsLabels = "AreAllStatusesShownAsLabels";
            public const string AssignedLabelType = "AssignedLabelType";
            public const string InitiatedLabelType = "InitiatedLabelType";
            public const string MaximumAttachmentNumber = "MaximumAttachmentNumber";
        }

        #endregion
        /* BEMA.End */

        #region Fields 

        private Guid? _caseWorkerGroupGuid = null;

        #endregion

        #region Properties

        private List<int> DocumentsState { get; set; }

        #endregion

        #region ViewState and Dynamic Controls

        /// <summary>
        /// ViewState of BenevolenceResultInfos for BenevolenceRequest
        /// </summary>
        /// <value>
        /// The state of the BenevolenceResultInfos for BenevolenceRequest.
        /// </value>
        public List<BenevolenceResultInfo> BenevolenceResultsState
        {
            get
            {
                List<BenevolenceResultInfo> result = ViewState["BenevolenceResultInfoState"] as List<BenevolenceResultInfo>;
                if ( result == null )
                {
                    result = new List<BenevolenceResultInfo>();
                }

                return result;
            }

            set
            {
                ViewState["BenevolenceResultInfoState"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            gResults.DataKeyNames = new string[] { "TempGuid" };
            gResults.Actions.AddClick += gResults_AddClick;
            gResults.Actions.ShowAdd = true;
            gResults.IsDeleteEnabled = true;

            // Gets any existing results and places them into the ViewState
            BenevolenceRequest benevolenceRequest = null;
            int benevolenceRequestId = PageParameter( "BenevolenceRequestId" ).AsInteger();
            if ( !benevolenceRequestId.Equals( 0 ) )
            {
                benevolenceRequest = new BenevolenceRequestService( new RockContext() ).Get( benevolenceRequestId );
            }

            if ( benevolenceRequest == null )
            {
                benevolenceRequest = new BenevolenceRequest { Id = 0 };
            }

            if ( ViewState["BenevolenceResultInfoState"] == null )
            {
                List<BenevolenceResultInfo> brInfoList = new List<BenevolenceResultInfo>();
                foreach ( BenevolenceResult benevolenceResult in benevolenceRequest.BenevolenceResults )
                {
                    BenevolenceResultInfo benevolenceResultInfo = new BenevolenceResultInfo();
                    benevolenceResultInfo.ResultId = benevolenceResult.Id;
                    benevolenceResultInfo.Amount = benevolenceResult.Amount;
                    benevolenceResultInfo.TempGuid = benevolenceResult.Guid;
                    benevolenceResultInfo.ResultSummary = benevolenceResult.ResultSummary;
                    benevolenceResultInfo.ResultTypeValueId = benevolenceResult.ResultTypeValueId;
                    benevolenceResultInfo.ResultTypeName = benevolenceResult.ResultTypeValue.Value;
                    brInfoList.Add( benevolenceResultInfo );
                }

                BenevolenceResultsState = brInfoList;
            }

            dlDocuments.ItemDataBound += DlDocuments_ItemDataBound;

            _caseWorkerGroupGuid = GetAttributeValue( "CaseWorkerRole" ).AsGuidOrNull();

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            /* BEMA.UI1.Start */
            ppCaseWorker.Label = GetAttributeValue( BemaAttributeKey.CaseWorkerLabel );
            ddlCaseWorker.Label = GetAttributeValue( BemaAttributeKey.CaseWorkerLabel );
            /* BEMA.UI1.End */

            /* BEMA.UI2.Start */
            dtbGovernmentId.Label = GetAttributeValue( BemaAttributeKey.GovernmentIdLabel );
            /* BEMA.UI2.End */

            if ( !Page.IsPostBack )
            {
                cpCampus.Campuses = CampusCache.All();
                ShowDetail( PageParameter( "BenevolenceRequestId" ).AsInteger() );
            }
            else
            {
                var rockContext = new RockContext();
                BenevolenceRequest item = new BenevolenceRequestService( rockContext ).Get( hfBenevolenceRequestId.ValueAsInt() );
                if ( item == null )
                {
                    item = new BenevolenceRequest();
                }
                item.LoadAttributes();

                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( item, phAttributes, false, BlockValidationGroup, 2 );

                confirmExit.Enabled = true;
            }
        }


        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            DocumentsState = ViewState["DocumentsState"] as List<int>;
            if ( DocumentsState == null )
            {
                DocumentsState = new List<int>();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["DocumentsState"] = DocumentsState;

            return base.SaveViewState();
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "BenevolenceRequestId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the AddClick event of the gResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gResults_AddClick( object sender, EventArgs e )
        {
            dvpResultType.Items.Clear();
            dvpResultType.AutoPostBack = false;
            dvpResultType.Required = true;
            dvpResultType.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ).Id;
            dtbResultSummary.Text = string.Empty;
            dtbAmount.Text = string.Empty;

            mdAddResult.Show();
        }

        /// <summary>
        /// Handles the RowSelected event of the gResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gResults_RowSelected( object sender, RowEventArgs e )
        {
            Guid? infoGuid = e.RowKeyValue as Guid?;
            List<BenevolenceResultInfo> resultList = BenevolenceResultsState;
            var resultInfo = resultList.FirstOrDefault( r => r.TempGuid == infoGuid );
            if ( resultInfo != null )
            {
                dvpResultType.Items.Clear();
                dvpResultType.AutoPostBack = false;
                dvpResultType.Required = true;
                dvpResultType.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ).Id;
                dvpResultType.SetValue( resultInfo.ResultTypeValueId );
                dtbResultSummary.Text = resultInfo.ResultSummary;
                dtbAmount.Text = resultInfo.Amount.ToString();
                hfInfoGuid.Value = e.RowKeyValue.ToString();
                mdAddResult.Show();
            }
        }

        /// <summary>
        /// Handles the DeleteClick event of the gResult control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gResults_DeleteClick( object sender, RowEventArgs e )
        {
            Guid? infoGuid = e.RowKeyValue as Guid?;
            List<BenevolenceResultInfo> resultList = BenevolenceResultsState;
            var resultInfo = resultList.FirstOrDefault( r => r.TempGuid == infoGuid );
            if ( resultInfo != null )
            {
                resultList.Remove( resultInfo );
            }

            BenevolenceResultsState = resultList;
            BindGridFromViewState();
        }

        /// <summary>
        /// Handles the AddClick event of the mdAddResult control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnAddResults_Click( object sender, EventArgs e )
        {
            int? resultType = dvpResultType.SelectedItem.Value.AsIntegerOrNull();
            List<BenevolenceResultInfo> benevolenceResultInfoViewStateList = BenevolenceResultsState;
            Guid? infoGuid = hfInfoGuid.Value.AsGuidOrNull();

            if ( infoGuid != null )
            {
                var resultInfo = benevolenceResultInfoViewStateList.FirstOrDefault( r => r.TempGuid == infoGuid );
                if ( resultInfo != null )
                {
                    resultInfo.Amount = dtbAmount.Text.AsDecimalOrNull();
                    resultInfo.ResultSummary = dtbResultSummary.Text;
                    if ( resultType != null )
                    {
                        resultInfo.ResultTypeValueId = resultType.Value;
                    }

                    resultInfo.ResultTypeName = dvpResultType.SelectedItem.Text;
                }
            }
            else
            {
                BenevolenceResultInfo benevolenceResultInfo = new BenevolenceResultInfo();

                benevolenceResultInfo.Amount = dtbAmount.Text.AsDecimalOrNull();

                benevolenceResultInfo.ResultSummary = dtbResultSummary.Text;
                if ( resultType != null )
                {
                    benevolenceResultInfo.ResultTypeValueId = resultType.Value;
                }

                benevolenceResultInfo.ResultTypeName = dvpResultType.SelectedItem.Text;
                benevolenceResultInfo.TempGuid = Guid.NewGuid();
                benevolenceResultInfoViewStateList.Add( benevolenceResultInfo );
            }

            BenevolenceResultsState = benevolenceResultInfoViewStateList;

            mdAddResult.Hide();
            pnlView.Visible = true;
            BindGridFromViewState();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();
                BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );
                BenevolenceResultService benevolenceResultService = new BenevolenceResultService( rockContext );

                BenevolenceRequest benevolenceRequest = null;
                int benevolenceRequestId = PageParameter( "BenevolenceRequestId" ).AsInteger();

                if ( !benevolenceRequestId.Equals( 0 ) )
                {
                    benevolenceRequest = benevolenceRequestService.Get( benevolenceRequestId );
                }

                if ( benevolenceRequest == null )
                {
                    benevolenceRequest = new BenevolenceRequest { Id = 0 };
                }

                benevolenceRequest.FirstName = dtbFirstName.Text;
                benevolenceRequest.LastName = dtbLastName.Text;
                benevolenceRequest.Email = ebEmail.Text;
                benevolenceRequest.RequestText = dtbRequestText.Text;
                benevolenceRequest.ResultSummary = dtbSummary.Text;
                benevolenceRequest.CampusId = cpCampus.SelectedCampusId;
                benevolenceRequest.ProvidedNextSteps = dtbProvidedNextSteps.Text;
                benevolenceRequest.GovernmentId = dtbGovernmentId.Text;

                if ( lapAddress.Location != null )
                {
                    benevolenceRequest.LocationId = lapAddress.Location.Id;
                }

                benevolenceRequest.RequestedByPersonAliasId = ppPerson.PersonAliasId;

                if ( _caseWorkerGroupGuid.HasValue )
                {
                    benevolenceRequest.CaseWorkerPersonAliasId = ddlCaseWorker.SelectedValue.AsIntegerOrNull();
                }
                else
                {
                    benevolenceRequest.CaseWorkerPersonAliasId = ppCaseWorker.PersonAliasId;
                }

                /* BEMA.FE1.Start */
                int? statusChangeId = benevolenceRequest.RequestStatusValueId;
                /* BEMA.FE1.End */

                benevolenceRequest.RequestStatusValueId = dvpRequestStatus.SelectedValue.AsIntegerOrNull();
                benevolenceRequest.ConnectionStatusValueId = dvpConnectionStatus.SelectedValue.AsIntegerOrNull();

                if ( dpRequestDate.SelectedDate.HasValue )
                {
                    benevolenceRequest.RequestDateTime = dpRequestDate.SelectedDate.Value;
                }

                benevolenceRequest.HomePhoneNumber = pnbHomePhone.Number;
                benevolenceRequest.CellPhoneNumber = pnbCellPhone.Number;
                benevolenceRequest.WorkPhoneNumber = pnbWorkPhone.Number;

                List<BenevolenceResultInfo> resultListUI = BenevolenceResultsState;
                var resultListDB = benevolenceRequest.BenevolenceResults.ToList();

                // remove any Benevolence Results that were removed in the UI
                foreach ( BenevolenceResult resultDB in resultListDB )
                {
                    if ( !resultListUI.Any( r => r.ResultId == resultDB.Id ) )
                    {
                        benevolenceRequest.BenevolenceResults.Remove( resultDB );
                        benevolenceResultService.Delete( resultDB );
                    }
                }

                // add any Benevolence Results that were added in the UI
                foreach ( BenevolenceResultInfo resultUI in resultListUI )
                {
                    var resultDB = resultListDB.FirstOrDefault( r => r.Guid == resultUI.TempGuid );
                    if ( resultDB == null )
                    {
                        resultDB = new BenevolenceResult();
                        resultDB.BenevolenceRequestId = benevolenceRequest.Id;
                        resultDB.Guid = resultUI.TempGuid;
                        benevolenceRequest.BenevolenceResults.Add( resultDB );
                    }

                    resultDB.Amount = resultUI.Amount;
                    resultDB.ResultSummary = resultUI.ResultSummary;
                    resultDB.ResultTypeValueId = resultUI.ResultTypeValueId;
                }

                if ( benevolenceRequest.IsValid )
                {
                    if ( benevolenceRequest.Id.Equals( 0 ) )
                    {
                        benevolenceRequestService.Add( benevolenceRequest );
                    }

                    // get attributes
                    benevolenceRequest.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, benevolenceRequest );

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        benevolenceRequest.SaveAttributeValues( rockContext );
                    } );

                    // update related documents
                    var documentsService = new BenevolenceRequestDocumentService( rockContext );

                    // delete any images that were removed
                    var orphanedBinaryFileIds = new List<int>();
                    var documentsInDb = documentsService.Queryable().Where( b => b.BenevolenceRequestId == benevolenceRequest.Id ).ToList();

                    foreach ( var document in documentsInDb.Where( i => !DocumentsState.Contains( i.BinaryFileId ) ) )
                    {
                        orphanedBinaryFileIds.Add( document.BinaryFileId );
                        documentsService.Delete( document );
                    }

                    // save documents
                    int documentOrder = 0;
                    foreach ( var binaryFileId in DocumentsState )
                    {
                        // Add or Update the activity type
                        var document = documentsInDb.FirstOrDefault( i => i.BinaryFileId == binaryFileId );
                        if ( document == null )
                        {
                            document = new BenevolenceRequestDocument();
                            document.BenevolenceRequestId = benevolenceRequest.Id;
                            benevolenceRequest.Documents.Add( document );
                        }
                        document.BinaryFileId = binaryFileId;
                        document.Order = documentOrder;
                        documentOrder++;
                    }
                    rockContext.SaveChanges();

                    /* BEMA.FE1.Start */

                    string statusChange = "";

                    if ( statusChangeId != dvpRequestStatus.SelectedValue.AsIntegerOrNull() )
                    {
                        statusChange = dvpRequestStatus.SelectedItem.ToString();
                    }
                    else
                    {
                        statusChange = "";
                    }

                    if ( statusChange != "" )
                    {
                        Guid? workflowTypeGuid = GetAttributeValue( BemaAttributeKey.Workflow ).AsGuidOrNull();
                        if ( workflowTypeGuid.HasValue )
                        {
                            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );

                            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                            {
                                var workflow = Rock.Model.Workflow.Activate( workflowType, "Benevolence - " + benevolenceRequest.Id.ToString() );
                                workflow.SetAttributeValue( "BenevolenceRequest", benevolenceRequest.Guid );

                                if ( benevolenceRequest.CaseWorkerPersonAlias != null )
                                    workflow.SetAttributeValue( "CaseManager", benevolenceRequest.CaseWorkerPersonAlias.Guid );

                                workflow.SetAttributeValue( "RequestId", benevolenceRequestId );
                                workflow.SetAttributeValue( "StatusChange", statusChange );
                                workflow.SetAttributeValue( "RequestorPerson", CurrentPerson.PrimaryAlias.Guid );


                                List<string> workflowErrors;
                                new WorkflowService( rockContext ).Process( workflow, out workflowErrors );
                            }
                        }

                        rockContext.SaveChanges();
                    }
                    /* BEMA.FE1.End */

                    // redirect back to parent
                    var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
                    var qryParams = new Dictionary<string, string>();
                    if ( personId.HasValue )
                    {
                        qryParams.Add( "PersonId", personId.ToString() );
                    }

                    NavigateToParentPage( qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
            var qryParams = new Dictionary<string, string>();
            if ( personId.HasValue )
            {
                qryParams.Add( "PersonId", personId.ToString() );
            }

            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbPrint control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrint_Click( object sender, EventArgs e )
        {
            var benevolenceRequestId = this.PageParameter( "BenevolenceRequestId" ).AsIntegerOrNull();
            if ( benevolenceRequestId.HasValue && !benevolenceRequestId.Equals( 0 ) && !string.IsNullOrEmpty( GetAttributeValue( "BenevolenceRequestStatementPage" ) ) )
            {
                NavigateToLinkedPage( "BenevolenceRequestStatementPage", new Dictionary<string, string> { { "BenevolenceRequestId", benevolenceRequestId.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppPerson.PersonId != null )
            {
                Person person = new PersonService( new RockContext() ).Get( ppPerson.PersonId.Value );
                if ( person != null )
                {
                    // Make sure that the FirstName box gets either FirstName or NickName of person. 
                    if ( !string.IsNullOrWhiteSpace( person.FirstName ) )
                    {
                        dtbFirstName.Text = person.FirstName;
                    }
                    else if ( !string.IsNullOrWhiteSpace( person.NickName ) )
                    {
                        dtbFirstName.Text = person.NickName;
                    }

                    //If both FirstName and NickName are blank, let them edit it manually
                    dtbFirstName.Enabled = string.IsNullOrWhiteSpace( dtbFirstName.Text );

                    dtbLastName.Text = person.LastName;
                    //If both LastName is blank, let them edit it manually
                    dtbLastName.Enabled = string.IsNullOrWhiteSpace( dtbLastName.Text );
                    ;

                    dvpConnectionStatus.SetValue( person.ConnectionStatusValueId );
                    dvpConnectionStatus.Enabled = false;

                    var homePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                    if ( homePhoneType != null )
                    {
                        var homePhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == homePhoneType.Id );
                        if ( homePhone != null )
                        {
                            pnbHomePhone.Text = homePhone.NumberFormatted;
                            pnbHomePhone.Enabled = false;
                        }
                    }

                    var mobilePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    if ( mobilePhoneType != null )
                    {
                        var mobileNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneType.Id );
                        if ( mobileNumber != null )
                        {
                            pnbCellPhone.Text = mobileNumber.NumberFormatted;
                            pnbCellPhone.Enabled = false;
                        }
                    }

                    var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                    if ( workPhoneType != null )
                    {
                        var workPhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                        if ( workPhone != null )
                        {
                            pnbWorkPhone.Text = workPhone.NumberFormatted;
                            pnbWorkPhone.Enabled = false;
                        }
                    }

                    ebEmail.Text = person.Email;
                    ebEmail.Enabled = false;

                    lapAddress.SetValue( person.GetHomeLocation() );
                    lapAddress.Enabled = false;

                    // set the campus but not on page load (e will be null) unless from the person profile page (in which case BenevolenceRequestId in the query string will be 0)
                    int? requestId = PageParameter( "BenevolenceRequestId" ).AsIntegerOrNull();

                    if ( !cpCampus.SelectedCampusId.HasValue && ( e != null || ( requestId.HasValue && requestId == 0 ) ) )
                    {
                        var personCampus = person.GetCampus();
                        cpCampus.SelectedCampusId = personCampus != null ? personCampus.Id : ( int? ) null;
                    }
                }
            }
            else
            {
                dtbFirstName.Enabled = true;
                dtbLastName.Enabled = true;
                dvpConnectionStatus.Enabled = true;
                pnbHomePhone.Enabled = true;
                pnbCellPhone.Enabled = true;
                pnbWorkPhone.Enabled = true;
                ebEmail.Enabled = true;
                lapAddress.Enabled = true;
            }
        }

        protected void fileUpDoc_FileUploaded( object sender, EventArgs e )
        {
            var fileUpDoc = ( Rock.Web.UI.Controls.FileUploader ) sender;

            if ( fileUpDoc.BinaryFileId.HasValue )
            {
                DocumentsState.Add( fileUpDoc.BinaryFileId.Value );
                BindDocuments( true );
            }
        }

        /// <summary>
        /// Handles the FileRemoved event of the fileUpDoc control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fileUpDoc_FileRemoved( object sender, FileUploaderEventArgs e )
        {
            var fileUpDoc = ( Rock.Web.UI.Controls.FileUploader ) sender;
            if ( e.BinaryFileId.HasValue )
            {
                DocumentsState.Remove( e.BinaryFileId.Value );
                BindDocuments( true );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the DlDocuments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataListItemEventArgs"/> instance containing the event data.</param>
        private void DlDocuments_ItemDataBound( object sender, DataListItemEventArgs e )
        {
            Guid binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS.AsGuid();
            var fileupDoc = e.Item.FindControl( "fileupDoc" ) as Rock.Web.UI.Controls.FileUploader;
            if ( fileupDoc != null )
            {
                fileupDoc.BinaryFileTypeGuid = binaryFileTypeGuid;
            }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Binds the documents.
        /// </summary>
        /// <param name="canEdit">if set to <c>true</c> [can edit].</param>
        private void BindDocuments( bool canEdit )
        {
            var ds = DocumentsState.ToList();

            /* BEMA.FE2.Start */
            if ( ds.Count() < GetAttributeValue( BemaAttributeKey.MaximumAttachmentNumber ).AsInteger() )
            /* BEMA.FE2.End */
            {
                ds.Add( 0 );
            }

            dlDocuments.DataSource = ds;
            dlDocuments.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="benevolenceRequestId">The benevolence request identifier</param>
        public void ShowDetail( int benevolenceRequestId )
        {
            BenevolenceRequest benevolenceRequest = null;
            var rockContext = new RockContext();
            BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );
            if ( !benevolenceRequestId.Equals( 0 ) )
            {
                benevolenceRequest = benevolenceRequestService.Get( benevolenceRequestId );
                pdAuditDetails.SetEntity( benevolenceRequest, ResolveRockUrl( "~" ) );
            }

            if ( benevolenceRequest == null )
            {
                benevolenceRequest = new BenevolenceRequest { Id = 0 };
                benevolenceRequest.RequestDateTime = RockDateTime.Now;
                var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( rockContext ).Get( personId.Value );
                    if ( person != null )
                    {
                        benevolenceRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                        benevolenceRequest.RequestedByPersonAlias = person.PrimaryAlias;
                    }
                }
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            dtbFirstName.Text = benevolenceRequest.FirstName;
            dtbLastName.Text = benevolenceRequest.LastName;
            dtbGovernmentId.Text = benevolenceRequest.GovernmentId;
            ebEmail.Text = benevolenceRequest.Email;
            dtbRequestText.Text = benevolenceRequest.RequestText;
            dtbSummary.Text = benevolenceRequest.ResultSummary;
            dtbProvidedNextSteps.Text = benevolenceRequest.ProvidedNextSteps;
            dpRequestDate.SelectedDate = benevolenceRequest.RequestDateTime;

            if ( benevolenceRequest.Campus != null )
            {
                cpCampus.SelectedCampusId = benevolenceRequest.CampusId;
            }
            else
            {
                cpCampus.SelectedIndex = 0;
            }

            if ( benevolenceRequest.RequestedByPersonAlias != null )
            {
                ppPerson.SetValue( benevolenceRequest.RequestedByPersonAlias.Person );
            }
            else
            {
                ppPerson.SetValue( null );
            }

            if ( benevolenceRequest.HomePhoneNumber != null )
            {
                pnbHomePhone.Text = benevolenceRequest.HomePhoneNumber;
            }

            if ( benevolenceRequest.CellPhoneNumber != null )
            {
                pnbCellPhone.Text = benevolenceRequest.CellPhoneNumber;
            }

            if ( benevolenceRequest.WorkPhoneNumber != null )
            {
                pnbWorkPhone.Text = benevolenceRequest.WorkPhoneNumber;
            }

            lapAddress.SetValue( benevolenceRequest.Location );

            LoadDropDowns( benevolenceRequest );

            if ( benevolenceRequest.RequestStatusValueId != null )
            {
                dvpRequestStatus.SetValue( benevolenceRequest.RequestStatusValueId );

                if ( benevolenceRequest.RequestStatusValue.Value == "Approved" )
                {
                    hlStatus.Text = "Approved";
                    hlStatus.LabelType = LabelType.Success;
                }

                if ( benevolenceRequest.RequestStatusValue.Value == "Denied" )
                {
                    hlStatus.Text = "Denied";
                    hlStatus.LabelType = LabelType.Danger;
                }

                /* BEMA.UI3.Start */
                var showAllStatuses = GetAttributeValue( BemaAttributeKey.AreAllStatusesShownAsLabels ).AsBoolean();
                if ( showAllStatuses )
                {
                    hlStatus.Text = benevolenceRequest.RequestStatusValue.Value;
                }
                /* BEMA.UI3.End */

                /* BEMA.UI4.Start */
                if ( benevolenceRequest.RequestStatusValue.Value == "Assigned" )
                {
                    hlStatus.LabelType = GetAttributeValue( BemaAttributeKey.AssignedLabelType ).ConvertToEnum<LabelType>();
                }
                /* BEMA.UI4.End */

                /* BEMA.UI5.Start */
                if ( benevolenceRequest.RequestStatusValue.Value == "Initiated" )
                {
                    hlStatus.LabelType = GetAttributeValue( BemaAttributeKey.InitiatedLabelType ).ConvertToEnum<LabelType>();
                }
                /* BEMA.UI5.End */
            }

            if ( benevolenceRequest.ConnectionStatusValueId != null )
            {
                dvpConnectionStatus.SetValue( benevolenceRequest.ConnectionStatusValueId );
            }

            if ( _caseWorkerGroupGuid.HasValue )
            {
                ddlCaseWorker.SetValue( benevolenceRequest.CaseWorkerPersonAliasId );
            }
            else
            {
                if ( benevolenceRequest.CaseWorkerPersonAlias != null )
                {
                    ppCaseWorker.SetValue( benevolenceRequest.CaseWorkerPersonAlias.Person );
                }
                else
                {
                    ppCaseWorker.SetValue( null );
                }
            }

            BindGridFromViewState();

            DocumentsState = benevolenceRequest.Documents.OrderBy( s => s.Order ).Select( s => s.BinaryFileId ).ToList();
            BindDocuments( true );

            benevolenceRequest.LoadAttributes();
            Rock.Attribute.Helper.AddEditControls( benevolenceRequest, phAttributes, true, BlockValidationGroup, 2 );

            // call the OnSelectPerson of the person picker which will update the UI based on the selected person
            ppPerson_SelectPerson( null, null );

            hfBenevolenceRequestId.Value = benevolenceRequest.Id.ToString();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGridFromViewState()
        {
            List<BenevolenceResultInfo> benevolenceResultInfoViewStateList = BenevolenceResultsState;
            gResults.DataSource = benevolenceResultInfoViewStateList;
            gResults.DataBind();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( BenevolenceRequest benevolenceRequest )
        {
            dvpRequestStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS ) ).Id;
            dvpConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;

            if ( _caseWorkerGroupGuid.HasValue )
            {
                var personList = new GroupMemberService( new RockContext() )
                    .Queryable( "Person, Group" )
                    .Where( gm => gm.Group.Guid == _caseWorkerGroupGuid.Value )
                    .Select( gm => gm.Person )
                    .ToList();

                string caseWorkerPersonAliasValue = benevolenceRequest.CaseWorkerPersonAliasId.ToString();
                if ( benevolenceRequest.CaseWorkerPersonAlias != null &&
                    benevolenceRequest.CaseWorkerPersonAlias.Person != null &&
                    !personList.Select( p => p.Id ).ToList().Contains( benevolenceRequest.CaseWorkerPersonAlias.Person.Id ) )
                {
                    personList.Add( benevolenceRequest.CaseWorkerPersonAlias.Person );
                }

                ddlCaseWorker.DataSource = personList.OrderBy( p => p.NickName ).ThenBy( p => p.LastName ).ToList();
                ddlCaseWorker.DataTextField = "FullName";
                ddlCaseWorker.DataValueField = "PrimaryAliasId";
                ddlCaseWorker.DataBind();
                ddlCaseWorker.Items.Insert( 0, new ListItem() );

                ppCaseWorker.Visible = false;
                ddlCaseWorker.Visible = true;
            }
            else
            {
                ppCaseWorker.Visible = true;
                ddlCaseWorker.Visible = false;
            }
        }

        #endregion

        #region BenevolenceResultInfo

        /// <summary>
        /// The class used to store BenevolenceResult info.
        /// </summary>
        [Serializable]
        public class BenevolenceResultInfo
        {
            [DataMember]
            public int? ResultId { get; set; }

            [DataMember]
            public int ResultTypeValueId { get; set; }

            [DataMember]
            public string ResultTypeName { get; set; }

            [DataMember]
            public decimal? Amount { get; set; }

            [DataMember]
            public Guid TempGuid { get; set; }

            [DataMember]
            public string ResultSummary { get; set; }
        }

        #endregion


    }
}