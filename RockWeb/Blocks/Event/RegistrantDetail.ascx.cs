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
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays interface for editing the registration attribute values and fees for a given registrant.
    /// </summary>
    [DisplayName( "Registrant Detail" )]
    [Category( "Event" )]
    [Description( "Displays interface for editing the registration attribute values and fees for a given registrant." )]

    public partial class RegistrantDetail : RockBlock
    {

        #region Properties

        private RegistrationTemplate TemplateState { get; set; }
        private RegistrantInfo RegistrantState { get; set; }
        private int RegistrationInstanceId { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["Template"] as string;
            if ( !string.IsNullOrWhiteSpace( json ) )
            {
                TemplateState = JsonConvert.DeserializeObject<RegistrationTemplate>( json );
            }

            json = ViewState["Registrant"] as string;
            if ( !string.IsNullOrWhiteSpace( json ) )
            {
                RegistrantState = JsonConvert.DeserializeObject<RegistrantInfo>( json );
            }

            RegistrationInstanceId = ViewState["RegistrationInstanceId"] as int? ?? 0;

            BuildControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlRegistrantDetail );
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
                LoadState();
                BuildControls( true );
            }
            else
            {
                ParseControls();
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
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["Template"] = JsonConvert.SerializeObject( TemplateState, Formatting.None, jsonSetting );
            ViewState["Registrant"] = JsonConvert.SerializeObject( RegistrantState, Formatting.None, jsonSetting );
            ViewState["RegistrationInstanceId"] = RegistrationInstanceId;
            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( RegistrantState != null )
            {
                RockContext rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var registrantService = new RegistrationRegistrantService( rockContext );
                var registrantFeeService = new RegistrationRegistrantFeeService( rockContext );
                var registrationTemplateFeeService = new RegistrationTemplateFeeService( rockContext );
                RegistrationRegistrant registrant = null;
                if ( RegistrantState.Id > 0 )
                {
                    registrant = registrantService.Get( RegistrantState.Id );
                }

                bool newRegistrant = false;
                var registrantChanges = new List<string>();

                if ( registrant == null )
                {
                    newRegistrant = true;
                    registrant = new RegistrationRegistrant();
                    registrant.RegistrationId = RegistrantState.RegistrationId;
                    registrantService.Add( registrant );
                    registrantChanges.Add( "Created Registrant" );
                }

                if ( !registrant.PersonAliasId.Equals( ppPerson.PersonAliasId ) )
                {
                    string prevPerson = ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null ) ?
                        registrant.PersonAlias.Person.FullName : string.Empty;
                    string newPerson = ppPerson.PersonName;
                    History.EvaluateChange( registrantChanges, "Person", prevPerson, newPerson );
                }
                int? personId = ppPerson.PersonId.Value;
                registrant.PersonAliasId = ppPerson.PersonAliasId.Value;

                // Get the name of registrant for history
                string registrantName = "Unknown";
                if ( ppPerson.PersonId.HasValue )
                {
                    var person = personService.Get( ppPerson.PersonId.Value );
                    if ( person != null )
                    {
                        registrantName = person.FullName;
                    }
                }

                // set their status (wait list / registrant)
                registrant.OnWaitList = !tglWaitList.Checked;

                History.EvaluateChange( registrantChanges, "Cost", registrant.Cost, cbCost.Text.AsDecimal() );
                registrant.Cost = cbCost.Text.AsDecimal();

                History.EvaluateChange( registrantChanges, "Discount Applies", registrant.DiscountApplies, cbDiscountApplies.Checked );
                registrant.DiscountApplies = cbDiscountApplies.Checked;

                if ( !Page.IsValid )
                {
                    return;
                }

                // Remove/delete any registrant fees that are no longer in UI with quantity 
                foreach ( var dbFee in registrant.Fees.ToList() )
                {
                    if ( !RegistrantState.FeeValues.Keys.Contains( dbFee.RegistrationTemplateFeeId ) ||
                        RegistrantState.FeeValues[dbFee.RegistrationTemplateFeeId] == null ||
                        !RegistrantState.FeeValues[dbFee.RegistrationTemplateFeeId]
                            .Any( f =>
                                f.Option == dbFee.Option &&
                                f.Quantity > 0 ) )
                    {
                        registrantChanges.Add( string.Format( "Removed '{0}' Fee (Quantity:{1:N0}, Cost:{2:C2}, Option:{3}",
                            dbFee.RegistrationTemplateFee.Name, dbFee.Quantity, dbFee.Cost, dbFee.Option ) );

                        registrant.Fees.Remove( dbFee );
                        registrantFeeService.Delete( dbFee );
                    }
                }

                // Add/Update any of the fees from UI
                foreach ( var uiFee in RegistrantState.FeeValues.Where( f => f.Value != null ) )
                {
                    foreach ( var uiFeeOption in uiFee.Value )
                    {
                        var dbFee = registrant.Fees
                            .Where( f =>
                                f.RegistrationTemplateFeeId == uiFee.Key &&
                                f.Option == uiFeeOption.Option )
                            .FirstOrDefault();

                        if ( dbFee == null )
                        {
                            dbFee = new RegistrationRegistrantFee();
                            dbFee.RegistrationTemplateFeeId = uiFee.Key;
                            dbFee.Option = uiFeeOption.Option;
                            registrant.Fees.Add( dbFee );
                        }

                        var templateFee = dbFee.RegistrationTemplateFee;
                        if ( templateFee == null )
                        {
                            templateFee = registrationTemplateFeeService.Get( uiFee.Key );
                        }

                        string feeName = templateFee != null ? templateFee.Name : "Fee";
                        if ( !string.IsNullOrWhiteSpace( uiFeeOption.Option ) )
                        {
                            feeName = string.Format( "{0} ({1})", feeName, uiFeeOption.Option );
                        }

                        if ( dbFee.Id <= 0 )
                        {
                            registrantChanges.Add( feeName + " Fee Added" );
                        }

                        History.EvaluateChange( registrantChanges, feeName + " Quantity", dbFee.Quantity, uiFeeOption.Quantity );
                        dbFee.Quantity = uiFeeOption.Quantity;

                        History.EvaluateChange( registrantChanges, feeName + " Cost", dbFee.Cost, uiFeeOption.Cost );
                        dbFee.Cost = uiFeeOption.Cost;
                    }
                }

                if ( TemplateState.RequiredSignatureDocumentTemplate != null )
                {
                    var person = new PersonService( rockContext ).Get( personId.Value );

                    var documentService = new SignatureDocumentService( rockContext );
                    var binaryFileService = new BinaryFileService( rockContext );
                    SignatureDocument document = null;

                    int? signatureDocumentId = hfSignedDocumentId.Value.AsIntegerOrNull();
                    int? binaryFileId = fuSignedDocument.BinaryFileId;
                    if ( signatureDocumentId.HasValue )
                    {
                        document = documentService.Get( signatureDocumentId.Value );
                    }

                    if ( document == null && binaryFileId.HasValue )
                    {
                        var instance = new RegistrationInstanceService( rockContext ).Get( RegistrationInstanceId );

                        document = new SignatureDocument();
                        document.SignatureDocumentTemplateId = TemplateState.RequiredSignatureDocumentTemplate.Id;
                        document.AppliesToPersonAliasId = registrant.PersonAliasId.Value;
                        document.AssignedToPersonAliasId = registrant.PersonAliasId.Value;
                        document.Name = string.Format( "{0}_{1}",
                            ( instance != null ? instance.Name : TemplateState.Name ),
                            ( person != null ? person.FullName.RemoveSpecialCharacters() : string.Empty ) );
                        document.Status = SignatureDocumentStatus.Signed;
                        document.LastStatusDate = RockDateTime.Now;
                        documentService.Add( document );
                    }

                    if ( document != null )
                    {
                        int? origBinaryFileId = document.BinaryFileId;
                        document.BinaryFileId = binaryFileId;

                        if ( origBinaryFileId.HasValue && origBinaryFileId.Value != document.BinaryFileId )
                        {
                            // if a new the binaryFile was uploaded, mark the old one as Temporary so that it gets cleaned up
                            var oldBinaryFile = binaryFileService.Get( origBinaryFileId.Value );
                            if ( oldBinaryFile != null && !oldBinaryFile.IsTemporary )
                            {
                                oldBinaryFile.IsTemporary = true;
                            }
                        }

                        // ensure the IsTemporary is set to false on binaryFile associated with this document
                        if ( document.BinaryFileId.HasValue )
                        {
                            var binaryFile = binaryFileService.Get( document.BinaryFileId.Value );
                            if ( binaryFile != null && binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }
                        }
                    }
                }

                if ( !registrant.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    registrant.LoadAttributes();
                    foreach ( var field in TemplateState.Forms
                        .SelectMany( f => f.Fields
                            .Where( t =>
                                t.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                                t.AttributeId.HasValue ) ) )
                    {
                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        if ( attribute != null )
                        {
                            string originalValue = registrant.GetAttributeValue( attribute.Key );
                            var fieldValue = RegistrantState.FieldValues
                                .Where( f => f.Key == field.Id )
                                .Select( f => f.Value.FieldValue )
                                .FirstOrDefault();
                            string newValue = fieldValue != null ? fieldValue.ToString() : string.Empty;

                            if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                            {
                                string formattedOriginalValue = string.Empty;
                                if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                {
                                    formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                }

                                string formattedNewValue = string.Empty;
                                if ( !string.IsNullOrWhiteSpace( newValue ) )
                                {
                                    formattedNewValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                }

                                History.EvaluateChange( registrantChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                            }

                            if ( fieldValue != null )
                            {
                                registrant.SetAttributeValue( attribute.Key, fieldValue.ToString() );
                            }
                        }
                    }

                    registrant.SaveAttributeValues( rockContext );
                } );

                if ( newRegistrant && TemplateState.GroupTypeId.HasValue && ppPerson.PersonId.HasValue )
                {
                    using ( var newRockContext = new RockContext() )
                    {
                        var reloadedRegistrant = new RegistrationRegistrantService( newRockContext ).Get( registrant.Id );
                        if ( reloadedRegistrant != null &&
                            reloadedRegistrant.Registration != null &&
                            reloadedRegistrant.Registration.Group != null &&
                            reloadedRegistrant.Registration.Group.GroupTypeId == TemplateState.GroupTypeId.Value )
                        {
                            int? groupRoleId = TemplateState.GroupMemberRoleId.HasValue ?
                                TemplateState.GroupMemberRoleId.Value :
                                reloadedRegistrant.Registration.Group.GroupType.DefaultGroupRoleId;
                            if ( groupRoleId.HasValue )
                            {
                                var groupMemberService = new GroupMemberService( newRockContext );
                                var groupMember = groupMemberService
                                    .Queryable().AsNoTracking()
                                    .Where( m =>
                                        m.GroupId == reloadedRegistrant.Registration.Group.Id &&
                                        m.PersonId == reloadedRegistrant.PersonId &&
                                        m.GroupRoleId == groupRoleId.Value )
                                    .FirstOrDefault();
                                if ( groupMember == null )
                                {
                                    groupMember = new GroupMember();
                                    groupMemberService.Add( groupMember );
                                    groupMember.GroupId = reloadedRegistrant.Registration.Group.Id;
                                    groupMember.PersonId = ppPerson.PersonId.Value;
                                    groupMember.GroupRoleId = groupRoleId.Value;
                                    groupMember.GroupMemberStatus = TemplateState.GroupMemberStatus;

                                    newRockContext.SaveChanges();

                                    registrantChanges.Add( string.Format( "Registrant added to {0} group", reloadedRegistrant.Registration.Group.Name ) );
                                }
                                else
                                {
                                    registrantChanges.Add( string.Format( "Registrant group member reference updated to existing person in {0} group", reloadedRegistrant.Registration.Group.Name ) );
                                }

                                reloadedRegistrant.GroupMemberId = groupMember.Id;
                                newRockContext.SaveChanges();
                            }
                        }

                        // Record this to the Person's History...
                        if ( reloadedRegistrant != null && reloadedRegistrant.Registration != null )
                        { 
                            var changes = new List<string> { "Registered for" };
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_REGISTRATION.AsGuid(),
                                ppPerson.PersonId.Value,
                                changes,
                                reloadedRegistrant.Registration.RegistrationInstance.Name,
                                typeof( Registration ),
                                reloadedRegistrant.Registration.Id,
                                true,
                                CurrentPersonAliasId );
                        }
                    }
                }

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registrant.RegistrationId,
                    registrantChanges,
                    "Registrant: " + registrantName,
                    null, null );
            }
            
            NavigateToRegistration();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToRegistration();
        }

        protected void lbWizardTemplate_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Read( RockPage.PageId );
            if ( pageCache != null && 
                pageCache.ParentPage != null && 
                pageCache.ParentPage.ParentPage != null &&
                pageCache.ParentPage.ParentPage.ParentPage != null )
            {
                qryParams.Add( "RegistrationTemplateId", TemplateState != null ? TemplateState.Id.ToString() : "0" );
                NavigateToPage( pageCache.ParentPage.ParentPage.ParentPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbWizardInstance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbWizardInstance_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Read( RockPage.PageId );
            if ( pageCache != null &&
                pageCache.ParentPage != null &&
                pageCache.ParentPage.ParentPage != null )
            {
                qryParams.Add( "RegistrationInstanceId", RegistrationInstanceId.ToString() );
                NavigateToPage( pageCache.ParentPage.ParentPage.Guid, qryParams );
            }
        }

        protected void lbWizardRegistration_Click( object sender, EventArgs e )
        {
            NavigateToRegistration();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RegistrantState = null;
            LoadState();
            BuildControls( true );
        }

        #endregion

        #region Methods

        private void LoadState()
        {
            int? registrantId = PageParameter( "RegistrantId" ).AsIntegerOrNull();
            int? registrationId = PageParameter( "RegistrationId" ).AsIntegerOrNull();

            if ( RegistrantState == null )
            {
                var rockContext = new RockContext();
                RegistrationRegistrant registrant = null;

                if ( registrantId.HasValue && registrantId.Value != 0 )
                {
                    registrant = new RegistrationRegistrantService( rockContext )
                        .Queryable( "Registration.RegistrationInstance.RegistrationTemplate.Forms.Fields,Registration.RegistrationInstance.RegistrationTemplate.Fees,PersonAlias.Person,Fees" ).AsNoTracking()
                        .Where( r => r.Id == registrantId.Value )
                        .FirstOrDefault();

                    if ( registrant != null &&
                        registrant.Registration != null &&
                        registrant.Registration.RegistrationInstance != null &&
                        registrant.Registration.RegistrationInstance.RegistrationTemplate != null )
                    {
                        RegistrantState = new RegistrantInfo( registrant, rockContext );
                        TemplateState = registrant.Registration.RegistrationInstance.RegistrationTemplate; 
                        
                        RegistrationInstanceId = registrant.Registration.RegistrationInstanceId;

                        lWizardTemplateName.Text = registrant.Registration.RegistrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = registrant.Registration.RegistrationInstance.Name;
                        lWizardRegistrationName.Text = registrant.Registration.ToString();
                        lWizardRegistrantName.Text = registrant.ToString();

                        tglWaitList.Checked = !registrant.OnWaitList;
                    }
                }

                if ( TemplateState == null && registrationId.HasValue && registrationId.Value != 0 )
                {
                    var registration = new RegistrationService( rockContext )
                        .Queryable( "RegistrationInstance.RegistrationTemplate.Forms.Fields,RegistrationInstance.RegistrationTemplate.Fees" ).AsNoTracking()
                        .Where( r => r.Id == registrationId.Value )
                        .FirstOrDefault();

                    if ( registration != null &&
                        registration.RegistrationInstance != null &&
                        registration.RegistrationInstance.RegistrationTemplate != null )
                    {
                        TemplateState = registration.RegistrationInstance.RegistrationTemplate;
                        
                        RegistrationInstanceId = registration.RegistrationInstanceId;

                        lWizardTemplateName.Text = registration.RegistrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = registration.RegistrationInstance.Name;
                        lWizardRegistrationName.Text = registration.ToString();
                        lWizardRegistrantName.Text = "New Registrant";
                    }
                }

                if ( TemplateState != null )
                {
                    tglWaitList.Visible = TemplateState.WaitListEnabled;
                }

                if ( TemplateState != null && RegistrantState == null )
                {
                    RegistrantState = new RegistrantInfo();
                    RegistrantState.RegistrationId = registrationId ?? 0;
                    if ( TemplateState.SetCostOnInstance.HasValue && TemplateState.SetCostOnInstance.Value )
                    {
                        var instance = new RegistrationInstanceService( rockContext ).Get( RegistrationInstanceId );
                        if ( instance != null )
                        {
                            RegistrantState.Cost = instance.Cost ?? 0.0m;
                        }
                    }
                    else
                    {
                        RegistrantState.Cost = TemplateState.Cost;
                    }
                }

                if ( registrant != null && registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                {
                    ppPerson.SetValue( registrant.PersonAlias.Person );
                    if ( TemplateState != null && TemplateState.RequiredSignatureDocumentTemplate != null )
                    {
                        fuSignedDocument.Label = TemplateState.RequiredSignatureDocumentTemplate.Name;
                        if ( TemplateState.RequiredSignatureDocumentTemplate.BinaryFileType != null )
                        {
                            fuSignedDocument.BinaryFileTypeGuid = TemplateState.RequiredSignatureDocumentTemplate.BinaryFileType.Guid;
                        }

                        var signatureDocument = new SignatureDocumentService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.SignatureDocumentTemplateId == TemplateState.RequiredSignatureDocumentTemplateId.Value &&
                                d.AppliesToPersonAlias != null &&
                                d.AppliesToPersonAlias.PersonId == registrant.PersonAlias.PersonId &&
                                d.LastStatusDate.HasValue &&
                                d.Status == SignatureDocumentStatus.Signed &&
                                d.BinaryFile != null )
                            .OrderByDescending( d => d.LastStatusDate.Value )
                            .FirstOrDefault();

                        if ( signatureDocument != null )
                        {
                            hfSignedDocumentId.Value = signatureDocument.Id.ToString();
                            fuSignedDocument.BinaryFileId = signatureDocument.BinaryFileId;
                        }

                        fuSignedDocument.Visible = true;
                    }
                    else
                    {
                        fuSignedDocument.Visible = false;
                    }
                }
                else
                {
                    ppPerson.SetValue( null );
                }

                if ( RegistrantState != null )
                {
                    cbCost.Text = RegistrantState.Cost.ToString( "N2" );
                    cbDiscountApplies.Checked = RegistrantState.DiscountApplies;
                }
            }
        }

        private void NavigateToRegistration()
        {
            if ( RegistrantState != null  )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "RegistrationId", RegistrantState.RegistrationId.ToString() );
                NavigateToParentPage( qryParams );
            }
        }

        #region Build Controls

        private void BuildControls( bool setValues )
        {
            if ( RegistrantState != null && TemplateState != null )
            {
                BuildFields( setValues );
                BuildFees( setValues );
            }
        }

        private void BuildFields( bool setValues )
        {
            phFields.Controls.Clear();

            if ( TemplateState.Forms != null )
            {
                foreach ( var form in TemplateState.Forms.OrderBy( f => f.Order ) )
                {
                    if ( form.Fields != null )
                    {
                        foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                        {
                            if ( field.FieldSource == RegistrationFieldSource.RegistrationAttribute )
                            {
                                object fieldValue = null;
                                if ( RegistrantState.FieldValues.ContainsKey( field.Id ) )
                                {
                                    fieldValue = RegistrantState.FieldValues[field.Id].FieldValue;
                                }

                                if ( field.AttributeId.HasValue )
                                {
                                    var attribute = AttributeCache.Read( field.AttributeId.Value );
                                    string value = string.Empty;
                                    if ( setValues && fieldValue != null )
                                    {
                                        value = fieldValue.ToString();
                                    }
                                    attribute.AddControl( phFields.Controls, value, BlockValidationGroup, setValues, true, field.IsRequired, null, string.Empty );
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BuildFees( bool setValues )
        {
            phFees.Controls.Clear();

            if ( TemplateState.Fees != null && TemplateState.Fees.Any() )
            {
                divFees.Visible = true;

                foreach ( var fee in TemplateState.Fees.OrderBy( f => f.Order ) )
                {
                    var feeValues = new List<FeeInfo>();
                    if ( RegistrantState.FeeValues.ContainsKey( fee.Id ) )
                    {
                        feeValues = RegistrantState.FeeValues[fee.Id];
                    }

                    if ( fee.FeeType == RegistrationFeeType.Single )
                    {
                        string label = fee.Name;
                        var cost = fee.CostValue.AsDecimalOrNull();
                        if ( cost.HasValue && cost.Value != 0.0M )
                        {
                            label = string.Format( "{0} ({1})", fee.Name, cost.Value.FormatAsCurrency() );
                        }

                        if ( fee.AllowMultiple )
                        {
                            // Single Option, Multi Quantity
                            var numUpDown = new NumberUpDown();
                            numUpDown.ID = "fee_" + fee.Id.ToString();
                            numUpDown.Label = label;
                            numUpDown.Minimum = 0;
                            phFees.Controls.Add( numUpDown );

                            if ( setValues && feeValues != null && feeValues.Any() )
                            {
                                numUpDown.Value = feeValues.First().Quantity;
                            }
                        }
                        else
                        {
                            // Single Option, Single Quantity
                            var cb = new RockCheckBox();
                            cb.ID = "fee_" + fee.Id.ToString();
                            cb.Label = label;
                            cb.SelectedIconCssClass = "fa fa-check-square-o fa-lg";
                            cb.UnSelectedIconCssClass = "fa fa-square-o fa-lg";
                            phFees.Controls.Add( cb );

                            if ( setValues && feeValues != null && feeValues.Any() )
                            {
                                cb.Checked = feeValues.First().Quantity > 0;
                            }
                        }
                    }
                    else
                    {
                        // Parse the options to get name and cost for each
                        var options = new Dictionary<string, string>();
                        string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                        foreach ( string nameValue in nameValues )
                        {
                            string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                            if ( nameAndValue.Length == 1 )
                            {
                                options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                            }
                            if ( nameAndValue.Length == 2 )
                            {
                                options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1})", nameAndValue[0], nameAndValue[1].AsDecimal().FormatAsCurrency() ) );
                            }
                        }

                        if ( fee.AllowMultiple )
                        {
                            HtmlGenericControl feeAllowMultiple = new HtmlGenericControl( "div" );
                            phFees.Controls.Add( feeAllowMultiple );

                            feeAllowMultiple.AddCssClass( "feetype-allowmultiples" );

                            Label titleLabel = new Label();
                            feeAllowMultiple.Controls.Add( titleLabel );
                            titleLabel.CssClass = "control-label";
                            titleLabel.Text = fee.Name;

                            foreach ( var optionKeyVal in options )
                            {
                                var numUpDown = new NumberUpDown();
                                numUpDown.ID = string.Format( "fee_{0}_{1}", fee.Id, optionKeyVal.Key );
                                numUpDown.Label = string.Format( "{0}", optionKeyVal.Value );
                                numUpDown.Minimum = 0;
                                numUpDown.CssClass = "fee-allowmultiple";
                                feeAllowMultiple.Controls.Add( numUpDown );

                                if ( setValues && feeValues != null && feeValues.Any() )
                                {
                                    numUpDown.Value = feeValues
                                        .Where( f => f.Option == optionKeyVal.Key )
                                        .Select( f => f.Quantity )
                                        .FirstOrDefault();
                                }
                            }
                        }
                        else
                        {
                            // Multi Option, Single Quantity
                            var ddl = new RockDropDownList();
                            ddl.ID = "fee_" + fee.Id.ToString();
                            ddl.AddCssClass( "input-width-md" );
                            ddl.Label = fee.Name;
                            ddl.DataValueField = "Key";
                            ddl.DataTextField = "Value";
                            ddl.DataSource = options;
                            ddl.DataBind();
                            ddl.Items.Insert( 0, "" );
                            phFees.Controls.Add( ddl );

                            if ( setValues && feeValues != null && feeValues.Any() )
                            {
                                ddl.SetValue( feeValues
                                    .Where( f => f.Quantity > 0 )
                                    .Select( f => f.Option )
                                    .FirstOrDefault() );
                            }
                        }
                    }
                }
            }
            else
            {
                divFees.Visible = false;
            }
        }

        #endregion

        #region Parse Controls

        private void ParseControls ()
        {
            if ( RegistrantState != null && TemplateState != null )
            {
                ParseFields();
                ParseFees();
            }
        }

        private void ParseFields()
        {
            if ( TemplateState.Forms != null )
            {
                foreach ( var form in TemplateState.Forms.OrderBy( f => f.Order ) )
                {
                    if ( form.Fields != null )
                    {
                        foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                        {
                            if ( field.FieldSource == RegistrationFieldSource.RegistrationAttribute )
                            {
                                object value = null;

                                if ( field.AttributeId.HasValue )
                                {
                                    var attribute = AttributeCache.Read( field.AttributeId.Value );
                                    string fieldId = "attribute_field_" + attribute.Id.ToString();

                                    Control control = phFields.FindControl( fieldId );
                                    if ( control != null )
                                    {
                                        value = attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues );
                                    }
                                }

                                if ( value != null )
                                {
                                    RegistrantState.FieldValues.AddOrReplace( field.Id, new FieldValueObject( field, value ) );
                                }
                                else
                                {
                                    RegistrantState.FieldValues.Remove( field.Id );
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ParseFees()
        {
            if ( TemplateState.Fees != null )
            {
                foreach ( var fee in TemplateState.Fees.OrderBy( f => f.Order ) )
                {
                    List<FeeInfo> feeValues = ParseFee( fee );
                    if ( fee != null )
                    {
                        RegistrantState.FeeValues.AddOrReplace( fee.Id, feeValues );
                    }
                    else
                    {
                        RegistrantState.FeeValues.Remove( fee.Id );
                    }
                }
            }
        }

        private List<FeeInfo> ParseFee( RegistrationTemplateFee fee )
        {
            string fieldId = string.Format( "fee_{0}", fee.Id );

            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = phFees.FindControl( fieldId ) as NumberUpDown;
                    if ( numUpDown != null && numUpDown.Value > 0 )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, numUpDown.Value, fee.CostValue.AsDecimal() ) };
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = phFees.FindControl( fieldId ) as RockCheckBox;
                    if ( cb != null && cb.Checked )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, 1, fee.CostValue.AsDecimal() ) };
                    }
                }
            }
            else
            {
                // Parse the options to get name and cost for each
                var options = new Dictionary<string, string>();
                var optionCosts = new Dictionary<string, decimal>();

                string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameAndValue.Length == 1 )
                    {
                        options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                        optionCosts.AddOrIgnore( nameAndValue[0], 0.0m );
                    }
                    if ( nameAndValue.Length == 2 )
                    {
                        options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1})", nameAndValue[0], nameAndValue[1].AsDecimal().FormatAsCurrency() ) );
                        optionCosts.AddOrIgnore( nameAndValue[0], nameAndValue[1].AsDecimal() );
                    }
                }

                if ( fee.AllowMultiple )
                {
                    // Multi Option, Multi Quantity
                    var result = new List<FeeInfo>();

                    foreach ( var optionKeyVal in options )
                    {
                        string optionFieldId = string.Format( "{0}_{1}", fieldId, optionKeyVal.Key );
                        var numUpDown = phFees.FindControl( optionFieldId ) as NumberUpDown;
                        if ( numUpDown != null && numUpDown.Value > 0 )
                        {
                            result.Add( new FeeInfo( optionKeyVal.Key, numUpDown.Value, optionCosts[optionKeyVal.Key] ) );
                        }
                    }

                    if ( result.Any() )
                    {
                        return result;
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = phFees.FindControl( fieldId ) as RockDropDownList;
                    if ( ddl != null && ddl.SelectedValue != "" )
                    {
                        return new List<FeeInfo> { new FeeInfo( ddl.SelectedValue, 1, optionCosts[ddl.SelectedValue] ) };
                    }
                }
            }

            return null;
        }

        #endregion

        #endregion

    }
}