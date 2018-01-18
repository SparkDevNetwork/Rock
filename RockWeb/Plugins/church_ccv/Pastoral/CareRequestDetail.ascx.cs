using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using church.ccv.Pastoral.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Pastoral
{
    /// <summary>
    /// Block for users to create, edit, and view care requests.
    /// </summary>
    [DisplayName( "Care Request Detail" )]
    [Category( "Pastoral" )]
    [Description( "Block for users to create, edit, and view Care requests." )]
    [SecurityRoleField( "Worker Role", "The security role to draw workers from", true, church.ccv.Utility.SystemGuids.Group.GROUP_CARE_WORKERS )]
    [LinkedPage("Care Request Statement Page", "The page which summarises a care request for printing", false)]
    public partial class CareRequestDetail : RockBlock
    {
        #region Properties
        private List<int> DocumentsState { get; set; }
        #endregion

        #region ViewState and Dynamic Controls

        /// <summary>
        /// ViewState of CareResultInfos for Care Request
        /// </summary>
        /// <value>
        /// The state of the CareResultInfos for CareRequest.
        /// </value>
        public List<CareResultInfo> CareResultsState
        {
            get
            {
                List<CareResultInfo> result = ViewState["CareResultInfoState"] as List<CareResultInfo>;
                if ( result == null )
                {
                    result = new List<CareResultInfo>();
                }

                return result;
            }

            set
            {
                ViewState["CareResultInfoState"] = value;
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
            CareRequest careRequest = null;
            int careRequestId = PageParameter( "CareRequestId" ).AsInteger();
            if ( !careRequestId.Equals( 0 ) )
            {
                careRequest = new Service<CareRequest>( new RockContext() ).Get( careRequestId );
            }

            if ( careRequest == null )
            {
                careRequest = new CareRequest { Id = 0 };
            }

            if ( ViewState["CareResultInfoState"] == null )
            {
                List<CareResultInfo> brInfoList = new List<CareResultInfo>();
                foreach ( CareResult careResult in careRequest.CareResults )
                {
                    CareResultInfo careResultInfo = new CareResultInfo();
                    careResultInfo.ResultId = careResult.Id;
                    careResultInfo.Amount = careResult.Amount;
                    careResultInfo.TempGuid = careResult.Guid;
                    careResultInfo.ResultSummary = careResult.ResultSummary;
                    careResultInfo.ResultTypeValueId = careResult.ResultTypeValueId;
                    careResultInfo.ResultTypeName = careResult.ResultTypeValue.Value;
                    brInfoList.Add( careResultInfo );
                }

                CareResultsState = brInfoList;
            }

            dlDocuments.ItemDataBound += DlDocuments_ItemDataBound;
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
                cpCampus.Campuses = CampusCache.All();
                ShowDetail( PageParameter( "CareRequestId" ).AsInteger() );

                if( !string.IsNullOrEmpty( GetAttributeValue( "CareRequestStatementPage" ) ) )
                {
                    lbPrint.Visible = true;
                }

            }
            else
            {
                var rockContext = new RockContext();
                CareRequest item = new Service<CareRequest>(rockContext).Get( hfCareRequestId.ValueAsInt());
                if (item == null )
                {
                    item = new CareRequest();
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
            ShowDetail( PageParameter( "CareRequestId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the AddClick event of the gResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gResults_AddClick( object sender, EventArgs e )
        {
            ddlResultType.Items.Clear();
            ddlResultType.AutoPostBack = false;
            ddlResultType.Required = true;
            ddlResultType.BindToDefinedType( DefinedTypeCache.Read( new Guid( church.ccv.Utility.SystemGuids.DefinedType.CARE_RESULT_TYPE ) ), true );
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
            List<CareResultInfo> resultList = CareResultsState;
            var resultInfo = resultList.FirstOrDefault( r => r.TempGuid == infoGuid );
            if ( resultInfo != null )
            {
                ddlResultType.Items.Clear();
                ddlResultType.AutoPostBack = false;
                ddlResultType.Required = true;
                ddlResultType.BindToDefinedType( DefinedTypeCache.Read( new Guid( church.ccv.Utility.SystemGuids.DefinedType.CARE_RESULT_TYPE ) ), true );
                ddlResultType.SetValue( resultInfo.ResultTypeValueId );
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
            List<CareResultInfo> resultList = CareResultsState;
            var resultInfo = resultList.FirstOrDefault( r => r.TempGuid == infoGuid );
            if ( resultInfo != null )
            {
                resultList.Remove( resultInfo );
            }

            CareResultsState = resultList;
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
            int? resultType = ddlResultType.SelectedItem.Value.AsIntegerOrNull();
            List<CareResultInfo> careResultInfoViewStateList = CareResultsState;
            Guid? infoGuid = hfInfoGuid.Value.AsGuidOrNull();

            if ( infoGuid != null )
            {
                var resultInfo = careResultInfoViewStateList.FirstOrDefault( r => r.TempGuid == infoGuid );
                if ( resultInfo != null )
                {
                    resultInfo.Amount = dtbAmount.Text.AsDecimalOrNull();
                    resultInfo.ResultSummary = dtbResultSummary.Text;
                    if ( resultType != null )
                    {
                        resultInfo.ResultTypeValueId = resultType.Value;
                    }

                    resultInfo.ResultTypeName = ddlResultType.SelectedItem.Text;
                }
            }
            else
            {
                CareResultInfo careResultInfo = new CareResultInfo();

                careResultInfo.Amount = dtbAmount.Text.AsDecimalOrNull();

                careResultInfo.ResultSummary = dtbResultSummary.Text;
                if ( resultType != null )
                {
                    careResultInfo.ResultTypeValueId = resultType.Value;
                }

                careResultInfo.ResultTypeName = ddlResultType.SelectedItem.Text;
                careResultInfo.TempGuid = Guid.NewGuid();
                careResultInfoViewStateList.Add( careResultInfo );
            }

            CareResultsState = careResultInfoViewStateList;

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
                Service<CareRequest> careRequestService = new Service<CareRequest>( rockContext );
                Service<CareResult> careResultService = new Service<CareResult>( rockContext );

                CareRequest careRequest = null;
                int careRequestId = PageParameter( "CareRequestId" ).AsInteger();

                if ( !careRequestId.Equals( 0 ) )
                {
                    careRequest = careRequestService.Get( careRequestId );
                }

                if ( careRequest == null )
                {
                    careRequest = new CareRequest { Id = 0 };
                    careRequest.Type = CareRequest.Types.Care;
                }


                careRequest.FirstName = dtbFirstName.Text;
                careRequest.LastName = dtbLastName.Text;
                careRequest.Email = ebEmail.Text;
                careRequest.RequestText = dtbRequestText.Text;
                careRequest.ResultSummary = dtbSummary.Text;
                careRequest.CampusId = cpCampus.SelectedCampusId;
                
                if ( lapAddress.Location != null )
                {
                    careRequest.LocationId = lapAddress.Location.Id;
                }

                careRequest.RequestStatusValueId = ddlRequestStatus.SelectedValue.AsIntegerOrNull();
                careRequest.RequestedByPersonAliasId = ppPerson.PersonAliasId;
                careRequest.WorkerPersonAliasId = ddlWorker.SelectedValue.AsIntegerOrNull();
                careRequest.ConnectionStatusValueId = ddlConnectionStatus.SelectedValue.AsIntegerOrNull();

                if ( dpRequestDate.SelectedDate.HasValue )
                {
                    careRequest.RequestDateTime = dpRequestDate.SelectedDate.Value;
                }

                careRequest.HomePhoneNumber = pnbHomePhone.Number;
                careRequest.CellPhoneNumber = pnbCellPhone.Number;
                careRequest.WorkPhoneNumber = pnbWorkPhone.Number;

                List<CareResultInfo> resultListUI = CareResultsState;
                var resultListDB = careRequest.CareResults.ToList();

                // remove any Care Results that were removed in the UI
                foreach ( CareResult resultDB in resultListDB )
                {
                    if ( !resultListUI.Any( r => r.ResultId == resultDB.Id ) )
                    {
                        careRequest.CareResults.Remove( resultDB );
                        careResultService.Delete( resultDB );
                    }
                }

                // add any Care Results that were added in the UI
                foreach ( CareResultInfo resultUI in resultListUI )
                {
                    var resultDB = resultListDB.FirstOrDefault( r => r.Guid == resultUI.TempGuid );
                    if ( resultDB == null )
                    {
                        resultDB = new CareResult();
                        resultDB.CareRequestId = careRequest.Id;
                        resultDB.Guid = resultUI.TempGuid;
                        careRequest.CareResults.Add( resultDB );
                    }

                    resultDB.Amount = resultUI.Amount;
                    resultDB.ResultSummary = resultUI.ResultSummary;
                    resultDB.ResultTypeValueId = resultUI.ResultTypeValueId;
                }

                if ( careRequest.IsValid )
                {
                    if ( careRequest.Id.Equals( 0 ) )
                    {
                        careRequestService.Add( careRequest );
                    }

                    // get attributes
                    careRequest.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, careRequest );

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        careRequest.SaveAttributeValues( rockContext );
                    } );

                    // update related documents
                    var documentsService = new Service<CareDocument>( rockContext );

                    // delete any documents that were removed
                    var orphanedBinaryFileIds = new List<int>();
                    var documentsInDb = documentsService.Queryable().Where( b => b.CareRequestId == careRequest.Id ).ToList();

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
                            document = new CareDocument();
                            document.CareRequestId = careRequest.Id;
                            careRequest.Documents.Add( document );
                        }
                        document.BinaryFileId = binaryFileId;
                        document.Order = documentOrder;
                        documentOrder++;
                    }
                    rockContext.SaveChanges();

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
        protected void lbPrint_Click(object sender, EventArgs e)
        {
            var careRequestId = this.PageParameter("CareRequestId").AsIntegerOrNull();       
            if (careRequestId.HasValue && !careRequestId.Equals(0) && !string.IsNullOrEmpty(GetAttributeValue("CareRequestStatementPage")))
            {
                NavigateToLinkedPage("CareRequestStatementPage", new Dictionary<string, string> { { "CareRequestId", careRequestId.ToString() } });
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
                    if (!string.IsNullOrWhiteSpace(person.FirstName))
                    {
                        dtbFirstName.Text = person.FirstName;
                    }
                    else if ( !string.IsNullOrWhiteSpace( person.NickName ) )
                    {
                        dtbFirstName.Text = person.NickName;
                    }

                    //If both FirstName and NickName are blank, let them edit it manually
                    dtbFirstName.Enabled = string.IsNullOrWhiteSpace(dtbFirstName.Text);

                    dtbLastName.Text = person.LastName;
                    //If LastName is blank, let them edit it manually
                    dtbLastName.Enabled = string.IsNullOrWhiteSpace( dtbLastName.Text );

                    ddlConnectionStatus.SetValue( person.ConnectionStatusValueId );
                    ddlConnectionStatus.Enabled = false;

                    var homePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                    if ( homePhoneType != null )
                    {
                        var homePhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == homePhoneType.Id );
                        if ( homePhone != null )
                        {
                            pnbHomePhone.Text = homePhone.NumberFormatted;
                            pnbHomePhone.Enabled = false;
                        }
                    }

                    var mobilePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    if ( mobilePhoneType != null )
                    {
                        var mobileNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneType.Id );
                        if ( mobileNumber != null )
                        {
                            pnbCellPhone.Text = mobileNumber.NumberFormatted;
                            pnbCellPhone.Enabled = false;
                        }
                    }

                    var workPhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
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

                    // set the campus but not on page load (e will be null) unless from the person profile page (in which case CareRequestId in the query string will be 0)
                    int? requestId = Request["CareRequestId"].AsIntegerOrNull();
                    
                    if ( !cpCampus.SelectedCampusId.HasValue && ( e != null || (requestId.HasValue && requestId == 0 ) ) )
                    {
                        var personCampus = person.GetCampus();
                        cpCampus.SelectedCampusId = personCampus != null ? personCampus.Id : (int?)null;
                    }
                }
            }
            else
            {
                dtbFirstName.Enabled = true;
                dtbLastName.Enabled = true;
                ddlConnectionStatus.Enabled = true;
                pnbHomePhone.Enabled = true;
                pnbCellPhone.Enabled = true;
                pnbWorkPhone.Enabled = true;
                ebEmail.Enabled = true;
                lapAddress.Enabled = true;
            }
        }

        protected void fileUpDoc_FileUploaded( object sender, EventArgs e )
        {
            var fileUpDoc = (Rock.Web.UI.Controls.FileUploader)sender;

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
            var fileUpDoc = (Rock.Web.UI.Controls.FileUploader)sender;
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
            Guid binaryFileTypeGuid = church.ccv.Utility.SystemGuids.BinaryFiletype.CARE_REQUEST_DOCUMENTS.AsGuid();
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

            if ( ds.Count() < 6 )
            {
                ds.Add( 0 );
            }

            dlDocuments.DataSource = ds;
            dlDocuments.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="careRequestId">The care request identifier</param>
        public void ShowDetail( int careRequestId )
        {
            CareRequest careRequest = null;
            var rockContext = new RockContext();
            Service<CareRequest> careRequestService = new Service<CareRequest>( rockContext );
            if ( !careRequestId.Equals( 0 ) )
            {
                careRequest = careRequestService.Get( careRequestId );
                pdAuditDetails.SetEntity( careRequest, ResolveRockUrl( "~" ) );
            }

            if ( careRequest == null )
            {
                careRequest = new CareRequest { Id = 0 };
                careRequest.RequestDateTime = RockDateTime.Now;
                var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( rockContext ).Get( personId.Value );
                    if ( person != null )
                    {
                        careRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                        careRequest.RequestedByPersonAlias = person.PrimaryAlias;
                    }
                }
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            dtbFirstName.Text = careRequest.FirstName;
            dtbLastName.Text = careRequest.LastName;
            ebEmail.Text = careRequest.Email;
            dtbRequestText.Text = careRequest.RequestText;
            dtbSummary.Text = careRequest.ResultSummary;
            dpRequestDate.SelectedDate = careRequest.RequestDateTime;

            if ( careRequest.Campus != null )
            {
                cpCampus.SelectedCampusId = careRequest.CampusId;
            }
            else
            {
                cpCampus.SelectedIndex = 0;
            }

            if ( careRequest.RequestedByPersonAlias != null )
            {
                ppPerson.SetValue( careRequest.RequestedByPersonAlias.Person );
            }
            else
            {
                ppPerson.SetValue( null );
            }

            if ( careRequest.HomePhoneNumber != null )
            {
                pnbHomePhone.Text = careRequest.HomePhoneNumber;
            }

            if ( careRequest.CellPhoneNumber != null )
            {
                pnbCellPhone.Text = careRequest.CellPhoneNumber;
            }

            if ( careRequest.WorkPhoneNumber != null )
            {
                pnbWorkPhone.Text = careRequest.WorkPhoneNumber;
            }

            lapAddress.SetValue( careRequest.Location );

            LoadDropDowns( careRequest );

            if ( careRequest.RequestStatusValueId != null )
            {
                ddlRequestStatus.SetValue( careRequest.RequestStatusValueId );

                if ( careRequest.RequestStatusValue.Value == "Approved" )
                {
                    hlStatus.Text = "Approved";
                    hlStatus.LabelType = LabelType.Success;
                }

                if ( careRequest.RequestStatusValue.Value == "Denied" )
                {
                    hlStatus.Text = "Denied";
                    hlStatus.LabelType = LabelType.Danger;
                }

                if ( careRequest.RequestStatusValue.Value == "Pending" )
                {
                    hlStatus.Text = "Pending";
                    hlStatus.LabelType = LabelType.Warning;
                }
            }

            if ( careRequest.ConnectionStatusValueId != null )
            {
                ddlConnectionStatus.SetValue( careRequest.ConnectionStatusValueId );
            }

            ddlWorker.SetValue( careRequest.WorkerPersonAliasId );

            BindGridFromViewState();

            DocumentsState = careRequest.Documents.OrderBy( s => s.Order ).Select( s => s.BinaryFileId ).ToList();
            BindDocuments( true );

            careRequest.LoadAttributes();
            Rock.Attribute.Helper.AddEditControls( careRequest, phAttributes, true, BlockValidationGroup, 2 );

            // call the OnSelectPerson of the person picker which will update the UI based on the selected person
            ppPerson_SelectPerson( null, null );

            hfCareRequestId.Value = careRequest.Id.ToString();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGridFromViewState()
        {
            List<CareResultInfo> careResultInfoViewStateList = CareResultsState;
            gResults.DataSource = careResultInfoViewStateList;
            gResults.DataBind();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( CareRequest careRequest )
        {
            ddlConnectionStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ), true );

            Guid groupGuid = GetAttributeValue( "WorkerRole" ).AsGuid();
            var personList = new GroupMemberService( new RockContext() )
                .Queryable( "Person, Group" )
                .Where( gm => gm.Group.Guid == groupGuid )
                .Select( gm => gm.Person )
                .ToList();

            string WorkerPersonAliasValue = careRequest.WorkerPersonAliasId.ToString();
            if ( careRequest.WorkerPersonAlias != null && 
                careRequest.WorkerPersonAlias.Person != null &&
                !personList.Select( p => p.Id ).ToList().Contains( careRequest.WorkerPersonAlias.Person.Id ) )
            {
                personList.Add( careRequest.WorkerPersonAlias.Person );
            }

            ddlWorker.DataSource = personList.OrderBy( p => p.NickName ).ThenBy( p => p.LastName ).ToList();
            ddlWorker.DataTextField = "FullName";
            ddlWorker.DataValueField = "PrimaryAliasId";
            ddlWorker.DataBind();
            ddlWorker.Items.Insert( 0, new ListItem() );
        }

        #endregion

        #region CareResultInfo

        /// <summary>
        /// The class used to store CareResult info.
        /// </summary>
        [Serializable]
        public class CareResultInfo
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