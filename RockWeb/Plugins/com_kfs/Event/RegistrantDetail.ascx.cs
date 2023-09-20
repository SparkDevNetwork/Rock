// KFS Registrant Detail

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_kfs.Event
{
    /// <summary>
    /// Displays interface for editing the registration attribute values and fees for a given registrant.
    /// </summary>
    [DisplayName("Advanced Registrant Detail")]
    [Category("KFS > Advanced Event Registration")]
    [Description("Displays interface for editing the registration attribute values and fees for a given registrant.")]
    [LinkedPage("Add Family Link", "Select the page where a new family can be added. If specified, a link will be shown which will open in a new window when clicked", false, "6a11a13d-05ab-4982-a4c2-67a8b1950c74,af36e4c2-78c6-4737-a983-e7a78137ddc7", "", 2)]
    [SecurityAction("AddFamilies", "The roles and/or users that can add new families to the system.")]
    public partial class RegistrantDetail : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the TemplateState
        /// </summary>
        /// <value>
        /// The state of the template.
        /// </value>
        private RegistrationTemplate RegistrationTemplate
        {
            get
            {
                if ( _registrationTemplate == null )
                {
                    _registrationTemplate = new RegistrationTemplateService( new RockContext() )
                        .Queryable().Where( a => a.Id == this.RegistrationTemplateId )
                        .Include( a => a.FinancialGateway )
                        .Include( a => a.Discounts )
                        .Include( a => a.Fees )
                        .Include( a => a.Forms )
                        .FirstOrDefault();
                }

                return _registrationTemplate;
            }
        }

        private RegistrationTemplate _registrationTemplate = null;

        /// <summary>
        /// Gets or sets the RegistrantSate
        /// </summary>
        /// <value>
        /// The state of the registrant.
        /// </value>
        private RegistrantInfo RegistrantState { get; set; }

        /// <summary>
        /// The Registrant's group assignment
        /// </summary>
        /// <value>
        /// The group that the registrant is in
        /// </value>
        private Group RegistrationGroup { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Registrant was placed in their group
        /// </summary>
        /// <value>
        /// A flag indicating that the registrant was placed in the group
        /// </value>
        private bool RegistrantPlacedInGroup { get; set; } // all other registration fields stored as FieldValue objects

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        private int RegistrationInstanceId
        {
            get
            {
                return ViewState["RegistrationInstanceId"] as int? ?? 0;
            }

            set
            {
                ViewState["RegistrationInstanceId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        private int RegistrationTemplateId
        {
            get
            {
                return ViewState["RegistrationTemplateId"] as int? ?? 0;
            }

            set
            {
                ViewState["RegistrationTemplateId"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            //var json = ViewState["Template"] as string;
            //if (!string.IsNullOrWhiteSpace(json))
            //{
            //    TemplateState = JsonConvert.DeserializeObject<RegistrationTemplate>(json);
            //}

            var json = ViewState["Registrant"] as string;
            if (!string.IsNullOrWhiteSpace(json))
            {
                RegistrantState = JsonConvert.DeserializeObject<RegistrantInfo>(json);
            }

            json = ViewState["RegistrationGroup"] as string;
            if (!string.IsNullOrWhiteSpace(json))
            {
                RegistrationGroup = JsonConvert.DeserializeObject<Group>(json);
            }

            //RegistrationInstanceId = ViewState["RegistrationInstanceId"] as int? ?? 0;

            BuildControls(false);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlRegistrantDetail);

            bool canAddFamilies = UserCanAdministrate || IsUserAuthorized("AddFamilies");
            string addFamilyUrl = this.LinkedPageUrl("AddFamilyLink");
            rcwAddNewFamily.Visible = (!string.IsNullOrWhiteSpace(addFamilyUrl) && canAddFamilies);
            if (rcwAddNewFamily.Visible)
            {
                // force the link to open a new scrollable,resizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
                hlAddNewFamily.Attributes["onclick"] = string.Format("javascript: window.open('{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;", addFamilyUrl);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                LoadState();
                BuildControls(true);
            }
            else
            {
                ParseControls();
            }

            RegisterClientScript();
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

            //ViewState["Template"] = JsonConvert.SerializeObject(TemplateState, Formatting.None, jsonSetting);
            ViewState["Registrant"] = JsonConvert.SerializeObject(RegistrantState, Formatting.None, jsonSetting);
            ViewState["RegistrationGroup"] = JsonConvert.SerializeObject(RegistrationGroup, Formatting.None, jsonSetting);
            //ViewState["RegistrationInstanceId"] = RegistrationInstanceId;
            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (RegistrantState != null)
            {
                RockContext rockContext = new RockContext();
                var personService = new PersonService(rockContext);
                var registrantService = new RegistrationRegistrantService(rockContext);
                var registrantFeeService = new RegistrationRegistrantFeeService(rockContext);
                var registrationTemplateFeeService = new RegistrationTemplateFeeService(rockContext);
                var registrationTemplateFeeItemService = new RegistrationTemplateFeeItemService(rockContext);
                RegistrationRegistrant registrant = null;
                if (RegistrantState.Id > 0)
                {
                    registrant = registrantService.Get(RegistrantState.Id);
                }

                var previousRegistrantPersonIds = registrantService.Queryable().Where(a => a.RegistrationId == RegistrantState.RegistrationId)
                                .Where(r => r.PersonAlias != null)
                                .Select(r => r.PersonAlias.PersonId)
                                .ToList();

                bool newRegistrant = false;
                var registrantChanges = new History.HistoryChangeList();
                var personChanges = new History.HistoryChangeList();

                if (registrant == null)
                {
                    newRegistrant = true;
                    registrant = new RegistrationRegistrant();
                    registrant.RegistrationId = RegistrantState.RegistrationId;
                    registrantService.Add(registrant);
                    registrantChanges.AddChange(History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registrant");
                }

                if (!registrant.PersonAliasId.Equals(ppPerson.PersonAliasId))
                {
                    string prevPerson = (registrant.PersonAlias != null && registrant.PersonAlias.Person != null) ?
                        registrant.PersonAlias.Person.FullName : string.Empty;
                    string newPerson = ppPerson.PersonName;
                    newRegistrant = true;
                    History.EvaluateChange(registrantChanges, "Person", prevPerson, newPerson);
                }
                int? personId = ppPerson.PersonId.Value;
                registrant.PersonAliasId = ppPerson.PersonAliasId.Value;

                // Get the name of registrant for history
                string registrantName = "Unknown";
                var person = new Person();
                if (ppPerson.PersonId.HasValue)
                {
                    person = personService.Get(ppPerson.PersonId.Value);
                    if (person != null)
                    {
                        registrantName = person.FullName;
                    }
                }

                // update group membership
                if (RegistrationGroup != null)
                {
                    rockContext.WrapTransaction(() =>
                   {
                       var groupMemberService = new GroupMemberService(rockContext);
                       var groupMember = groupMemberService.Queryable()
                           .FirstOrDefault(m =>
                               m.GroupId == RegistrationGroup.Id &&
                               m.PersonId == person.Id &&
                               m.GroupRoleId == RegistrationTemplate.GroupMemberRoleId.Value);
                       if (groupMember == null && RegistrantPlacedInGroup)
                       {
                            // add the group member
                            groupMember = new GroupMember();
                           groupMember.GroupId = RegistrationGroup.Id;
                           groupMember.PersonId = person.Id;
                           groupMember.GroupRoleId = RegistrationTemplate.GroupMemberRoleId.Value;
                           groupMember.GroupMemberStatus = RegistrationTemplate.GroupMemberStatus;
                           groupMemberService.Add(groupMember);
                           rockContext.SaveChanges();
                           registrant.GroupMember = groupMember;
                           registrant.GroupMemberId = groupMember.Id;
                           registrantChanges.AddChange(History.HistoryVerb.AddedToGroup, History.HistoryChangeType.Record, RegistrationGroup.Name);
                       }
                       else if (groupMember != null && !RegistrantPlacedInGroup)
                       {
                            // remove the group member
                            groupMemberService.Delete(groupMember);
                           rockContext.SaveChanges();
                           registrant.GroupMember = null;
                           registrantChanges.AddChange(History.HistoryVerb.RemovedFromGroup, History.HistoryChangeType.Record, RegistrationGroup.Name);
                       }
                   });
                }

                // set their status (wait list / registrant)
                registrant.OnWaitList = !tglWaitList.Checked;

                History.EvaluateChange(registrantChanges, "Cost", registrant.Cost, cbCost.Text.AsDecimal());
                registrant.Cost = cbCost.Text.AsDecimal();

                History.EvaluateChange(registrantChanges, "Discount Applies", registrant.DiscountApplies, cbDiscountApplies.Checked);
                registrant.DiscountApplies = cbDiscountApplies.Checked;

                if (!Page.IsValid)
                {
                    return;
                }

                // Remove/delete any registrant fees that are no longer in UI with quantity
                foreach (var dbFee in registrant.Fees.ToList())
                {
                    if (!RegistrantState.FeeValues.Keys.Contains(dbFee.RegistrationTemplateFeeId) ||
                        RegistrantState.FeeValues[dbFee.RegistrationTemplateFeeId] == null ||
                        !RegistrantState.FeeValues[dbFee.RegistrationTemplateFeeId]
                            .Any(f =>
                               f.RegistrationTemplateFeeItemId == dbFee.RegistrationTemplateFeeItemId &&
                               f.Quantity > 0))
                    {
                        var feeOldValue = string.Format("'{0}' Fee (Quantity:{1:N0}, Cost:{2:C2}, Option:{3}",
                            dbFee.RegistrationTemplateFee.Name, dbFee.Quantity, dbFee.Cost, dbFee.Option);

                        registrantChanges.AddChange(History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Fee").SetOldValue(feeOldValue);
                        registrant.Fees.Remove(dbFee);
                        registrantFeeService.Delete(dbFee);
                    }
                }

                // Add/Update any of the fees from UI
                foreach (var uiFee in RegistrantState.FeeValues.Where(f => f.Value != null))
                {
                    foreach (var uiFeeOption in uiFee.Value)
                    {
                        var dbFee = registrant.Fees
                            .Where(f =>
                               f.RegistrationTemplateFeeId == uiFee.Key &&
                               f.RegistrationTemplateFeeItemId == uiFeeOption.RegistrationTemplateFeeItemId)
                            .FirstOrDefault();

                        if (dbFee == null)
                        {
                            dbFee = new RegistrationRegistrantFee();
                            dbFee.RegistrationTemplateFeeId = uiFee.Key;
                            var registrationTemplateFeeItem = registrationTemplateFeeItemService.GetNoTracking(uiFeeOption.RegistrationTemplateFeeItemId.HasValue ? uiFeeOption.RegistrationTemplateFeeItemId.Value : 0);
                            if (registrationTemplateFeeItem != null)
                            {
                                dbFee.Option = registrationTemplateFeeItem.Name;
                            }

                            dbFee.RegistrationTemplateFeeItemId = uiFeeOption.RegistrationTemplateFeeItemId;
                            registrant.Fees.Add(dbFee);
                        }

                        var templateFee = dbFee.RegistrationTemplateFee;
                        if (templateFee == null)
                        {
                            templateFee = registrationTemplateFeeService.Get(uiFee.Key);
                        }

                        string feeName = templateFee != null ? templateFee.Name : "Fee";
                        if (!string.IsNullOrWhiteSpace(uiFeeOption.FeeLabel))
                        {
                            feeName = string.Format("{0} ({1})", feeName, uiFeeOption.FeeLabel);
                        }

                        if (dbFee.Id <= 0)
                        {
                            registrantChanges.AddChange(History.HistoryVerb.Add, History.HistoryChangeType.Record, "Fee").SetNewValue(feeName);
                        }

                        History.EvaluateChange(registrantChanges, feeName + " Quantity", dbFee.Quantity, uiFeeOption.Quantity);
                        dbFee.Quantity = uiFeeOption.Quantity;

                        History.EvaluateChange(registrantChanges, feeName + " Cost", dbFee.Cost, uiFeeOption.Cost);
                        dbFee.Cost = uiFeeOption.Cost;
                    }
                }

                if (RegistrationTemplate.RequiredSignatureDocumentTemplate != null)
                {
                    var documentService = new SignatureDocumentService(rockContext);
                    var binaryFileService = new BinaryFileService(rockContext);
                    SignatureDocument document = null;

                    int? signatureDocumentId = hfSignedDocumentId.Value.AsIntegerOrNull();
                    int? binaryFileId = fuSignedDocument.BinaryFileId;
                    if (signatureDocumentId.HasValue)
                    {
                        document = documentService.Get(signatureDocumentId.Value);
                    }

                    if (document == null && binaryFileId.HasValue)
                    {
                        var instance = new RegistrationInstanceService(rockContext).Get(RegistrationInstanceId);

                        document = new SignatureDocument();
                        document.SignatureDocumentTemplateId = RegistrationTemplate.RequiredSignatureDocumentTemplate.Id;
                        document.AppliesToPersonAliasId = registrant.PersonAliasId.Value;
                        document.AssignedToPersonAliasId = registrant.PersonAliasId.Value;
                        document.Name = string.Format("{0}_{1}",
                            (instance != null ? instance.Name : RegistrationTemplate.Name),
                            (person != null ? person.FullName.RemoveSpecialCharacters() : string.Empty));
                        document.Status = SignatureDocumentStatus.Signed;
                        document.LastStatusDate = RockDateTime.Now;
                        documentService.Add(document);
                    }

                    if (document != null)
                    {
                        int? origBinaryFileId = document.BinaryFileId;
                        document.BinaryFileId = binaryFileId;

                        if (origBinaryFileId.HasValue && origBinaryFileId.Value != document.BinaryFileId)
                        {
                            // if a new the binaryFile was uploaded, mark the old one as Temporary so that it gets cleaned up
                            var oldBinaryFile = binaryFileService.Get(origBinaryFileId.Value);
                            if (oldBinaryFile != null && !oldBinaryFile.IsTemporary)
                            {
                                oldBinaryFile.IsTemporary = true;
                            }
                        }

                        // ensure the IsTemporary is set to false on binaryFile associated with this document
                        if (document.BinaryFileId.HasValue)
                        {
                            var binaryFile = binaryFileService.Get(document.BinaryFileId.Value);
                            if (binaryFile != null && binaryFile.IsTemporary)
                            {
                                binaryFile.IsTemporary = false;
                            }
                        }
                    }
                }

                if (!registrant.IsValid)
                {
                    // Controls will render the error messages
                    return;
                }

                var familyGroupType = GroupTypeCache.Get(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY);
                var adultRoleId = familyGroupType.Roles
                .Where(r => r.Guid.Equals(Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()))
                .Select(r => r.Id)
                .FirstOrDefault();
                var childRoleId = familyGroupType.Roles
                    .Where(r => r.Guid.Equals(Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid()))
                    .Select(r => r.Id)
                    .FirstOrDefault();
                var multipleFamilyGroupIds = new Dictionary<Guid, int>();
                var dvcConnectionStatus = DefinedValueCache.Get(GetAttributeValue("ConnectionStatus").AsGuid());
                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction(() =>
               {
                   rockContext.SaveChanges();

                   int? campusId = null;
                   Location location = null;

                    // Set any of the template's person fields
                    foreach (var field in RegistrationTemplate.Forms
                   .SelectMany(f => f.Fields
                       .Where(t => t.FieldSource == RegistrationFieldSource.PersonField)))
                   {
                       var fieldValue = RegistrantState.FieldValues
                           .Where(f => f.Key == field.Id)
                           .Select(f => f.Value.FieldValue)
                           .FirstOrDefault();

                       if (fieldValue != null)
                       {
                           switch (field.PersonFieldType)
                           {
                               case RegistrationPersonFieldType.Campus:
                                   {
                                       if (fieldValue != null)
                                       {
                                           campusId = fieldValue.ToString().AsIntegerOrNull();
                                       }
                                       break;
                                   }

                               case RegistrationPersonFieldType.MiddleName:
                                   string middleName = fieldValue.ToString().Trim();
                                   History.EvaluateChange(personChanges, "Middle Name", person.MiddleName, middleName);
                                   person.MiddleName = middleName;
                                   break;

                               case RegistrationPersonFieldType.Address:
                                   {
                                       location = fieldValue as Location;
                                       break;
                                   }

                               case RegistrationPersonFieldType.Birthdate:
                                   {
                                       var birthMonth = person.BirthMonth;
                                       var birthDay = person.BirthDay;
                                       var birthYear = person.BirthYear;

                                       person.SetBirthDate(fieldValue as DateTime?);

                                       History.EvaluateChange(personChanges, "Birth Month", birthMonth, person.BirthMonth);
                                       History.EvaluateChange(personChanges, "Birth Day", birthDay, person.BirthDay);
                                       History.EvaluateChange(personChanges, "Birth Year", birthYear, person.BirthYear);

                                       break;
                                   }

                               case RegistrationPersonFieldType.Grade:
                                   {
                                       var newGraduationYear = fieldValue.ToString().AsIntegerOrNull();
                                       History.EvaluateChange(personChanges, "Graduation Year", person.GraduationYear, newGraduationYear);
                                       person.GraduationYear = newGraduationYear;

                                       break;
                                   }

                               case RegistrationPersonFieldType.Gender:
                                   {
                                       var newGender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                                       History.EvaluateChange(personChanges, "Gender", person.Gender, newGender);
                                       person.Gender = newGender;
                                       break;
                                   }

                               case RegistrationPersonFieldType.MaritalStatus:
                                   {
                                       if (fieldValue != null)
                                       {
                                           int? newMaritalStatusId = fieldValue.ToString().AsIntegerOrNull();
                                           History.EvaluateChange(personChanges, "Marital Status", DefinedValueCache.GetName(person.MaritalStatusValueId), DefinedValueCache.GetName(newMaritalStatusId));
                                           person.MaritalStatusValueId = newMaritalStatusId;
                                       }
                                       break;
                                   }

                               case RegistrationPersonFieldType.AnniversaryDate:
                                   var oldAnniversaryDate = person.AnniversaryDate;
                                   person.AnniversaryDate = fieldValue.ToString().AsDateTime();
                                   History.EvaluateChange(personChanges, "Anniversary Date", oldAnniversaryDate, person.AnniversaryDate);
                                   break;

                               case RegistrationPersonFieldType.MobilePhone:
                                   {
                                       SavePhone(fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), personChanges);
                                       break;
                                   }

                               case RegistrationPersonFieldType.HomePhone:
                                   {
                                       SavePhone(fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), personChanges);
                                       break;
                                   }

                               case RegistrationPersonFieldType.WorkPhone:
                                   {
                                       SavePhone(fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(), personChanges);
                                       break;
                                   }

                               case RegistrationPersonFieldType.Email:
                                   {
                                       var newEmail = fieldValue.ToString() ?? string.Empty;
                                       History.EvaluateChange(personChanges, "Email", person.Email, newEmail);
                                       person.Email = newEmail;
                                       break;
                                   }

                               case RegistrationPersonFieldType.ConnectionStatus:
                                   var newConnectionStatusId = fieldValue.ToString().AsIntegerOrNull() ?? dvcConnectionStatus.Id;
                                   History.EvaluateChange(personChanges, "Connection Status", DefinedValueCache.GetName(person.ConnectionStatusValueId), DefinedValueCache.GetName(newConnectionStatusId));
                                   person.ConnectionStatusValueId = newConnectionStatusId;
                                   break;
                           }
                       }
                   }

                   var family = person.GetFamily();
                   int? singleFamilyId = null;
                   singleFamilyId = family.Id;

                    // Save the person ( and family if needed )
                    SavePerson(rockContext, person, family.Guid, campusId, location, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId);

                    // Load the person's attributes
                    person.LoadAttributes();

                    // Set any of the template's person fields
                    foreach (var field in RegistrationTemplate.Forms
                       .SelectMany(f => f.Fields
                       .Where(t =>
                          t.FieldSource == RegistrationFieldSource.PersonAttribute &&
                          t.AttributeId.HasValue)))
                   {
                        // Find the registrant's value
                        var fieldValue = RegistrantState.FieldValues
                       .Where(f => f.Key == field.Id)
                       .Select(f => f.Value.FieldValue)
                       .FirstOrDefault();

                       if (fieldValue != null)
                       {
                           var attribute = AttributeCache.Get(field.AttributeId.Value);
                           if (attribute != null)
                           {
                               string originalValue = person.GetAttributeValue(attribute.Key);
                               string newValue = fieldValue.ToString();
                               person.SetAttributeValue(attribute.Key, fieldValue.ToString());

                                // DateTime values must be stored in ISO8601 format as http://www.rockrms.com/Rock/Developer/BookContent/16/16#datetimeformatting
                                if (attribute.FieldType.Guid.Equals(Rock.SystemGuid.FieldType.DATE.AsGuid()) ||
                               attribute.FieldType.Guid.Equals(Rock.SystemGuid.FieldType.DATE_TIME.AsGuid()))
                               {
                                   DateTime aDateTime;
                                   if (DateTime.TryParse(newValue, out aDateTime))
                                   {
                                       newValue = aDateTime.ToString("o");
                                   }
                               }

                               if ((originalValue ?? string.Empty).Trim() != (newValue ?? string.Empty).Trim())
                               {
                                   string formattedOriginalValue = string.Empty;
                                   if (!string.IsNullOrWhiteSpace(originalValue))
                                   {
                                       formattedOriginalValue = attribute.FieldType.Field.FormatValue(null, originalValue, attribute.QualifierValues, false);
                                   }

                                   string formattedNewValue = string.Empty;
                                   if (!string.IsNullOrWhiteSpace(newValue))
                                   {
                                       formattedNewValue = attribute.FieldType.Field.FormatValue(null, newValue, attribute.QualifierValues, false);
                                   }

                                   Rock.Attribute.Helper.SaveAttributeValue(person, attribute, newValue, rockContext);
                                   History.EvaluateChange(personChanges, attribute.Name, formattedOriginalValue, formattedNewValue);
                               }
                           }
                       }
                   }

                    //
                    // this causes duplicate phone number types to be added
                    // leaving old phone numbers and adding new phone numbers
                    //
                    //personChanges.ForEach( c => registrantChanges.Add( c ) );

                    rockContext.SaveChanges();

                    // Set any of the template's registrant attributes
                    registrant.LoadAttributes();
                   foreach (var field in RegistrationTemplate.Forms
                       .SelectMany(f => f.Fields
                          .Where(t =>
                             t.FieldSource == RegistrationFieldSource.RegistrantAttribute &&
                             t.AttributeId.HasValue)))
                   {
                       var attribute = AttributeCache.Get(field.AttributeId.Value);
                       if (attribute != null)
                       {
                           string originalValue = registrant.GetAttributeValue(attribute.Key);
                           var fieldValue = RegistrantState.FieldValues
                               .Where(f => f.Key == field.Id)
                               .Select(f => f.Value.FieldValue)
                               .FirstOrDefault();
                           string newValue = fieldValue != null ? fieldValue.ToString() : string.Empty;

                           if ((originalValue ?? string.Empty).Trim() != (newValue ?? string.Empty).Trim())
                           {
                               string formattedOriginalValue = string.Empty;
                               if (!string.IsNullOrWhiteSpace(originalValue))
                               {
                                   formattedOriginalValue = attribute.FieldType.Field.FormatValue(null, originalValue, attribute.QualifierValues, false);
                               }

                               string formattedNewValue = string.Empty;
                               if (!string.IsNullOrWhiteSpace(newValue))
                               {
                                   formattedNewValue = attribute.FieldType.Field.FormatValue(null, newValue, attribute.QualifierValues, false);
                               }

                               History.EvaluateChange(registrantChanges, attribute.Name, formattedOriginalValue, formattedNewValue);
                           }

                           if (fieldValue != null)
                           {
                               registrant.SetAttributeValue(attribute.Key, fieldValue.ToString());
                           }
                       }
                   }

                   registrant.SaveAttributeValues(rockContext);

                    // Set any of the template's group member attributes

                    if (registrant.GroupMember != null)
                   {
                       registrant.GroupMember.LoadAttributes();
                       foreach (var field in RegistrationTemplate.Forms
                           .SelectMany(f => f.Fields
                               .Where(t =>
                                  t.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                                  t.AttributeId.HasValue)))
                       {

                           var attribute = AttributeCache.Get(field.AttributeId.Value);
                           if (attribute != null)
                           {
                               string originalValue = registrant.GroupMember.GetAttributeValue(attribute.Key);
                               var fieldValue = RegistrantState.FieldValues
                                   .Where(f => f.Key == field.Id)
                                   .Select(f => f.Value.FieldValue)
                                   .FirstOrDefault();

                               string newValue = fieldValue != null ? fieldValue.ToString() : string.Empty;

                               if ((originalValue ?? string.Empty).Trim() != (newValue ?? string.Empty).Trim())
                               {

                               }

                           }
                       }
                   }
               });

                if (newRegistrant && RegistrationTemplate.GroupTypeId.HasValue && ppPerson.PersonId.HasValue)
                {
                    using (var newRockContext = new RockContext())
                    {
                        var reloadedRegistrant = new RegistrationRegistrantService(newRockContext).Get(registrant.Id);
                        if (reloadedRegistrant != null &&
                            reloadedRegistrant.Registration != null &&
                            reloadedRegistrant.Registration.Group != null &&
                            reloadedRegistrant.Registration.Group.GroupTypeId == RegistrationTemplate.GroupTypeId.Value)
                        {
                            int? groupRoleId = RegistrationTemplate.GroupMemberRoleId.HasValue ?
                                RegistrationTemplate.GroupMemberRoleId.Value :
                                reloadedRegistrant.Registration.Group.GroupType.DefaultGroupRoleId;
                            if (groupRoleId.HasValue)
                            {
                                var groupMemberService = new GroupMemberService(newRockContext);
                                var groupMember = groupMemberService
                                    .Queryable().AsNoTracking()
                                    .Where(m =>
                                       m.GroupId == reloadedRegistrant.Registration.Group.Id &&
                                       m.PersonId == reloadedRegistrant.PersonId &&
                                       m.GroupRoleId == groupRoleId.Value)
                                    .FirstOrDefault();
                                if (groupMember == null)
                                {
                                    groupMember = new GroupMember();
                                    groupMemberService.Add(groupMember);
                                    groupMember.GroupId = reloadedRegistrant.Registration.Group.Id;
                                    groupMember.PersonId = ppPerson.PersonId.Value;
                                    groupMember.GroupRoleId = groupRoleId.Value;
                                    groupMember.GroupMemberStatus = RegistrationTemplate.GroupMemberStatus;

                                    newRockContext.SaveChanges();

                                    registrantChanges.AddChange(History.HistoryVerb.Add, History.HistoryChangeType.Record, string.Format("Registrant to {0} group", reloadedRegistrant.Registration.Group.Name));
                                }
                                else
                                {
                                    registrantChanges.AddChange(History.HistoryVerb.Modify, History.HistoryChangeType.Record, string.Format("Registrant to existing person in {0} group", reloadedRegistrant.Registration.Group.Name));
                                }

                                if (reloadedRegistrant.GroupMemberId.HasValue && reloadedRegistrant.GroupMemberId.Value != groupMember.Id)
                                {
                                    groupMemberService.Delete(reloadedRegistrant.GroupMember);
                                    newRockContext.SaveChanges();
                                    registrantChanges.AddChange(History.HistoryVerb.Delete, History.HistoryChangeType.Record, string.Format("Registrant to previous person in {0} group", reloadedRegistrant.Registration.Group.Name));
                                }

                                // Record this to the Person's and Registrants Notes and History...

                                reloadedRegistrant.GroupMemberId = groupMember.Id;
                            }
                        }
                        if (reloadedRegistrant.Registration.FirstName.IsNotNullOrWhiteSpace() && reloadedRegistrant.Registration.LastName.IsNotNullOrWhiteSpace())
                        {
                            reloadedRegistrant.Registration.SavePersonNotesAndHistory(reloadedRegistrant.Registration.FirstName, reloadedRegistrant.Registration.LastName, this.CurrentPersonAliasId, previousRegistrantPersonIds);
                        }
                        newRockContext.SaveChanges();

                    }
                }

                HistoryService.SaveChanges(
                    rockContext,
                    typeof(Registration),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registrant.RegistrationId,
                    registrantChanges,
                    "Registrant: " + registrantName,
                    null, null);
            }

            NavigateToRegistration();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            NavigateToRegistration();
        }

        protected void lbWizardTemplate_Click(object sender, EventArgs e)
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Get(RockPage.PageId);
            if (pageCache != null &&
                pageCache.ParentPage != null &&
                pageCache.ParentPage.ParentPage != null &&
                pageCache.ParentPage.ParentPage.ParentPage != null)
            {
                qryParams.Add("RegistrationTemplateId", RegistrationTemplate != null ? RegistrationTemplate.Id.ToString() : "0");
                NavigateToPage(pageCache.ParentPage.ParentPage.ParentPage.Guid, qryParams);
            }
        }

        /// <summary>
        /// Handles the Click event of the lbWizardInstance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbWizardInstance_Click(object sender, EventArgs e)
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Get(RockPage.PageId);
            if (pageCache != null &&
                pageCache.ParentPage != null &&
                pageCache.ParentPage.ParentPage != null)
            {
                qryParams.Add("RegistrationInstanceId", RegistrationInstanceId.ToString());
                NavigateToPage(pageCache.ParentPage.ParentPage.Guid, qryParams);
            }
        }

        protected void lbWizardRegistration_Click(object sender, EventArgs e)
        {
            NavigateToRegistration();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            RegistrantState = null;
            LoadState();
            BuildControls(true);
        }

        #endregion

        #region Methods
        private void RegisterClientScript()
        {
            if (RegistrantState.Id > 0 && RegistrantState.GroupMemberId.HasValue)
            {
                string editScript = string.Format(@"
    $('a.js-edit-registrant').click(function( e ){{
        e.preventDefault();
        if( $('#{2} .js-person-id').val() !=='{1}'){{
        var  newPerson = $('#{2} .js-person-name' ).val();
        var message = 'This Registration is linked to a group. {0} will be deleted from the group and '+ newPerson +' will be added to the group.';
        Rock.dialogs.confirm(message, function (result) {{
            if (result) {{
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }}
        }});
        }} else {{
            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
        }}
    }});
", RegistrantState.PersonName, RegistrantState.PersonId.Value, ppPerson.ClientID);
                ScriptManager.RegisterStartupScript(btnSave, btnSave.GetType(), "editRegistrantScript", editScript, true);
            }
        }
        private void LoadState()
        {
            int? registrantId = PageParameter("RegistrantId").AsIntegerOrNull();
            int? registrationId = PageParameter("RegistrationId").AsIntegerOrNull();

            if (RegistrantState == null)
            {
                var rockContext = new RockContext();
                RegistrationRegistrant registrant = null;

                if (registrantId.HasValue && registrantId.Value != 0)
                {
                    registrant = new RegistrationRegistrantService(rockContext)
                        .Queryable("Registration.RegistrationInstance.RegistrationTemplate.Forms.Fields,Registration.RegistrationInstance.RegistrationTemplate.Fees,PersonAlias.Person,Fees").AsNoTracking()
                        .Where(r => r.Id == registrantId.Value)
                        .FirstOrDefault();

                    if (registrant != null &&
                        registrant.Registration != null &&
                        registrant.Registration.RegistrationInstance != null &&
                        registrant.Registration.RegistrationInstance.RegistrationTemplate != null)
                    {
                        RegistrantState = new RegistrantInfo(registrant, rockContext);
                        RegistrationTemplateId = registrant.Registration.RegistrationInstance.RegistrationTemplateId;
                        RegistrationInstanceId = registrant.Registration.RegistrationInstanceId;

                        if (registrant.Registration.Group != null)
                        {
                            RegistrationGroup = registrant.Registration.Group.Clone(false);
                        }

                        lWizardTemplateName.Text = registrant.Registration.RegistrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = registrant.Registration.RegistrationInstance.Name;
                        lWizardRegistrationName.Text = registrant.Registration.ToString();
                        lWizardRegistrantName.Text = registrant.ToString();

                        tglWaitList.Checked = !registrant.OnWaitList;
                    }
                }

                if (RegistrationTemplate == null && registrationId.HasValue && registrationId.Value != 0)
                {
                    var registration = new RegistrationService(rockContext)
                        .Queryable("RegistrationInstance.RegistrationTemplate.Forms.Fields,RegistrationInstance.RegistrationTemplate.Fees").AsNoTracking()
                        .Where(r => r.Id == registrationId.Value)
                        .FirstOrDefault();

                    if (registration != null &&
                        registration.RegistrationInstance != null &&
                        registration.RegistrationInstance.RegistrationTemplate != null)
                    {
                        RegistrationTemplateId = registration.RegistrationInstance.RegistrationTemplateId;
                        RegistrationInstanceId = registration.RegistrationInstanceId;
                        if (registration.Group != null)
                        {
                            RegistrationGroup = registration.Group.Clone(false);
                        }

                        lWizardTemplateName.Text = registration.RegistrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = registration.RegistrationInstance.Name;
                        lWizardRegistrationName.Text = registration.ToString();
                        lWizardRegistrantName.Text = "New Registrant";
                    }
                }

                if (RegistrationTemplate != null)
                {
                    tglWaitList.Visible = RegistrationTemplate.WaitListEnabled;
                }

                if (RegistrationTemplate != null && RegistrantState == null)
                {
                    RegistrantState = new RegistrantInfo();
                    RegistrantState.RegistrationId = registrationId ?? 0;
                    if (RegistrationTemplate.SetCostOnInstance.HasValue && RegistrationTemplate.SetCostOnInstance.Value)
                    {
                        var instance = new RegistrationInstanceService(rockContext).Get(RegistrationInstanceId);
                        if (instance != null)
                        {
                            RegistrantState.Cost = instance.Cost ?? 0.0m;
                        }
                    }
                    else
                    {
                        RegistrantState.Cost = RegistrationTemplate.Cost;
                    }
                }

                if (registrant != null && registrant.PersonAlias != null && registrant.PersonAlias.Person != null)
                {
                    ppPerson.SetValue(registrant.PersonAlias.Person);
                }
                else
                {
                    ppPerson.SetValue(null);
                }

                if (RegistrationTemplate != null && RegistrationTemplate.RequiredSignatureDocumentTemplate != null)
                {
                    fuSignedDocument.Label = RegistrationTemplate.RequiredSignatureDocumentTemplate.Name;
                    if (RegistrationTemplate.RequiredSignatureDocumentTemplate.BinaryFileType != null)
                    {
                        fuSignedDocument.BinaryFileTypeGuid = RegistrationTemplate.RequiredSignatureDocumentTemplate.BinaryFileType.Guid;
                    }

                    if (ppPerson.PersonId.HasValue)
                    {
                        var signatureDocument = new SignatureDocumentService(rockContext)
                        .Queryable().AsNoTracking()
                        .Where(d =>
                           d.SignatureDocumentTemplateId == RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value &&
                           d.AppliesToPersonAlias != null &&
                           d.AppliesToPersonAlias.PersonId == ppPerson.PersonId.Value &&
                           d.LastStatusDate.HasValue &&
                           d.Status == SignatureDocumentStatus.Signed &&
                           d.BinaryFile != null)
                        .OrderByDescending(d => d.LastStatusDate.Value)
                        .FirstOrDefault();

                        if (signatureDocument != null)
                        {
                            hfSignedDocumentId.Value = signatureDocument.Id.ToString();
                            fuSignedDocument.BinaryFileId = signatureDocument.BinaryFileId;
                        }
                    }

                    fuSignedDocument.Visible = true;
                }
                else
                {
                    fuSignedDocument.Visible = false;
                }


                if (RegistrantState != null)
                {
                    cbCost.Text = RegistrantState.Cost.ToString("N2");
                    cbDiscountApplies.Checked = RegistrantState.DiscountApplies;
                }
            }
        }

        private void NavigateToRegistration()
        {
            if (RegistrantState != null)
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add("RegistrationId", RegistrantState.RegistrationId.ToString());
                NavigateToParentPage(qryParams);
            }
        }

        #region Build Controls
        /// <summary>
        /// Builds the controls for Fields and Fees.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildControls(bool setValues)
        {
            if (RegistrantState != null && RegistrationTemplate != null)
            {
                BuildFields(setValues);
                BuildFees(setValues);
            }
        }


        /// <summary>
        /// Builds the controls for the Fields placeholder.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildFields(bool setValues)
        {
            phFields.Controls.Clear();

            if (RegistrationGroup != null)
            {
                var ddlGroup = new RockDropDownList();
                ddlGroup.ID = "ddlGroup";
                ddlGroup.Required = false;
                ddlGroup.Label = "Target Group";
                ddlGroup.ValidationGroup = BlockValidationGroup;
                ddlGroup.Items.Add(new ListItem("", ""));
                ddlGroup.Items.Add(new ListItem(RegistrationGroup.Name, RegistrationGroup.Id.ToString()));
                ddlGroup.SetValue(RegistrationGroup);
                phFields.Controls.Add(ddlGroup);
            }

            if (RegistrationTemplate.Forms != null)
            {
                foreach (var form in RegistrationTemplate.Forms.OrderBy(f => f.Order))
                {
                    if (form.Fields != null)
                    {
                        foreach (var field in form.Fields.OrderBy(f => f.Order))
                        {
                            
                            object fieldValue = null;
                            if (RegistrantState.FieldValues.ContainsKey(field.Id))
                            {
                                fieldValue = RegistrantState.FieldValues[field.Id].FieldValue;
                            }

                            bool hasDependantVisibilityRule = form.Fields.Any(a => a.FieldVisibilityRules.RuleList.Any(r => r.ComparedToFormFieldGuid == field.Guid));

                            if (field.FieldSource == RegistrationFieldSource.PersonField)
                            {
                                CreatePersonField(hasDependantVisibilityRule, field, setValues, fieldValue);
                            }
                            else if (field.AttributeId.HasValue)
                            {
                                string value = setValues && fieldValue != null ? fieldValue.ToString() : null;

                                var attribute = AttributeCache.Get(field.AttributeId.Value);
                                if ((setValues && value == null) || (value.IsNullOrWhiteSpace() && field.IsRequired == true))
                                {
                                    // If the value was not set already, or if it is required and currently empty then use the default
                                    // Intentionally leaving the possibility of saving an empty string as the value for non-required fields.
                                    value = attribute.DefaultValue;
                                }

                                FieldVisibilityWrapper fieldVisibilityWrapper = new FieldVisibilityWrapper
                                {
                                    ID = "_fieldVisibilityWrapper_attribute_" + attribute.Id.ToString(),
                                    FormFieldId = field.Id,
                                    FieldVisibilityRules = field.FieldVisibilityRules
                                };

                                fieldVisibilityWrapper.EditValueUpdated += FieldVisibilityWrapper_EditValueUpdated;

                                phFields.Controls.Add(fieldVisibilityWrapper);

                                var editControl = attribute.AddControl(fieldVisibilityWrapper.Controls, value, BlockValidationGroup, setValues, true, field.IsRequired, null, field.Attribute.Description);
                                fieldVisibilityWrapper.EditControl = editControl;

                                if (hasDependantVisibilityRule && attribute.FieldType.Field.HasChangeHandler(editControl))
                                {
                                    attribute.FieldType.Field.AddChangeHandler(editControl, () =>
                                    {
                                        fieldVisibilityWrapper.TriggerEditValueUpdated(editControl, new FieldVisibilityWrapper.FieldEventArgs(attribute, editControl));
                                    });
                                }
                            }
                            
                        }
                    }
                }

                FieldVisibilityWrapper.ApplyFieldVisibilityRules( phFields );
            }
        }

        private void FieldVisibilityWrapper_EditValueUpdated(object sender, FieldVisibilityWrapper.FieldEventArgs args)
        {
            FieldVisibilityWrapper.ApplyFieldVisibilityRules(phFields);
        }
        /// <summary>
        /// Gets the fee values from RegistrantState
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <returns></returns>
        private List<FeeInfo> GetFeeValues(RegistrationTemplateFee fee)
        {
            var feeValues = new List<FeeInfo>();
            if (RegistrantState.FeeValues.ContainsKey(fee.Id))
            {
                feeValues = RegistrantState.FeeValues[fee.Id];
            }

            return feeValues;
        }

        /// <summary>
        /// Builds the fees controls in the fee placeholder.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildFees(bool setValues)
        {
            phFees.Controls.Clear();
            var registrationInstance = new RegistrationInstanceService(new RockContext()).GetNoTracking(RegistrationInstanceId);

            if (RegistrationTemplate.Fees != null && RegistrationTemplate.Fees.Any())
            {
                divFees.Visible = true;

                foreach (var fee in RegistrationTemplate.Fees.OrderBy(f => f.Order))
                {
                    var feeValues = GetFeeValues(fee);
                    fee.AddFeeControl(phFees, registrationInstance, true, feeValues, null);
                }
            }
            else
            {
                divFees.Visible = false;
            }
        }

        #endregion

        #region Parse Controls

        private void ParseControls()
        {
            if (RegistrantState != null && RegistrationTemplate != null)
            {
                ParseFields();
                ParseFees();
            }
        }

        private void ParseFields()
        {
            if (RegistrationGroup != null)
            {
                var control = phFields.FindControl("ddlGroup");
                if (control != null)
                {
                    RegistrantPlacedInGroup = ((RockDropDownList)control).SelectedValueAsInt() > 0;
                }
            }

            if (RegistrationTemplate.Forms != null)
            {
                foreach (var form in RegistrationTemplate.Forms.OrderBy(f => f.Order))
                {
                    if (form.Fields != null)
                    {
                        foreach (var field in form.Fields.OrderBy(f => f.Order))
                        {
                            object value = null;

                            {

                                if (field.FieldSource == RegistrationFieldSource.PersonField)
                                {
                                    switch (field.PersonFieldType)
                                    {
                                        case RegistrationPersonFieldType.FirstName:
                                            var tbFirstName = phFields.FindControl("tbFirstName") as RockTextBox;
                                            string firstName = tbFirstName != null ? tbFirstName.Text : null;
                                            value = string.IsNullOrWhiteSpace(firstName) ? null : firstName;
                                            break;
                                        case RegistrationPersonFieldType.LastName:
                                            var tbLastName = phFields.FindControl("tbLastName") as RockTextBox;
                                            string lastName = tbLastName != null ? tbLastName.Text : null;
                                            value =  string.IsNullOrWhiteSpace(lastName) ? null : lastName;
                                            break;

                                        case RegistrationPersonFieldType.Campus:
                                            var cpHomeCampus = phFields.FindControl("cpHomeCampus") as CampusPicker;
                                            value = cpHomeCampus != null ? cpHomeCampus.SelectedCampusId : null;
                                            break;

                                        case RegistrationPersonFieldType.Address:
                                            var location = new Location();
                                            var acAddress = phFields.FindControl("acAddress") as AddressControl;
                                            if (acAddress != null)
                                            {
                                                acAddress.GetValues(location);
                                                value = location;
                                            }

                                            break;

                                        case RegistrationPersonFieldType.Email:
                                            var tbEmail = phFields.FindControl("tbEmail") as EmailBox;
                                            string email = tbEmail != null ? tbEmail.Text : null;
                                            value =  string.IsNullOrWhiteSpace(email) ? null : email;
                                            break;

                                        case RegistrationPersonFieldType.Birthdate:
                                            var bpBirthday = phFields.FindControl("bpBirthday") as BirthdayPicker;
                                            value =  bpBirthday != null ? bpBirthday.SelectedDate : null;
                                            break;

                                        case RegistrationPersonFieldType.Grade:
                                            var gpGrade = phFields.FindControl("gpGrade") as GradePicker;
                                            value =  gpGrade != null ? Person.GraduationYearFromGradeOffset(gpGrade.SelectedValueAsInt(false)) : null;
                                            break;

                                        case RegistrationPersonFieldType.Gender:
                                            var ddlGender = phFields.FindControl("ddlGender") as RockDropDownList;
                                            value =  ddlGender != null ? ddlGender.SelectedValueAsInt() : null;
                                            break;

                                        case RegistrationPersonFieldType.MaritalStatus:
                                            var dvpMaritalStatus = phFields.FindControl("dvpMaritalStatus") as RockDropDownList;
                                            value =  dvpMaritalStatus != null ? dvpMaritalStatus.SelectedValueAsInt() : null;
                                            break;

                                        case RegistrationPersonFieldType.AnniversaryDate:
                                            var dppAnniversaryDate = phFields.FindControl("dppAnniversaryDate") as DatePartsPicker;
                                            value = dppAnniversaryDate != null ? dppAnniversaryDate.SelectedDate : null;
                                            break;

                                        case RegistrationPersonFieldType.MobilePhone:
                                            var mobilePhoneNumber = new PhoneNumber();
                                            var ppMobile = phFields.FindControl("ppMobile") as PhoneNumberBox;
                                            if (ppMobile != null)
                                            {
                                                mobilePhoneNumber.CountryCode = PhoneNumber.CleanNumber(ppMobile.CountryCode);
                                                mobilePhoneNumber.Number = PhoneNumber.CleanNumber(ppMobile.Number);
                                                value =  mobilePhoneNumber;
                                            }

                                            break;
                                        case RegistrationPersonFieldType.HomePhone:
                                            var homePhoneNumber = new PhoneNumber();
                                            var ppHome = phFields.FindControl("ppHome") as PhoneNumberBox;
                                            if (ppHome != null)
                                            {
                                                homePhoneNumber.CountryCode = PhoneNumber.CleanNumber(ppHome.CountryCode);
                                                homePhoneNumber.Number = PhoneNumber.CleanNumber(ppHome.Number);
                                                value =  homePhoneNumber;
                                            }

                                            break;

                                        case RegistrationPersonFieldType.WorkPhone:
                                            var workPhoneNumber = new PhoneNumber();
                                            var ppWork = phFields.FindControl("ppWork") as PhoneNumberBox;
                                            if (ppWork != null)
                                            {
                                                workPhoneNumber.CountryCode = PhoneNumber.CleanNumber(ppWork.CountryCode);
                                                workPhoneNumber.Number = PhoneNumber.CleanNumber(ppWork.Number);
                                                value =  workPhoneNumber;
                                            }

                                            break;
                                        case RegistrationPersonFieldType.ConnectionStatus:
                                            var dvpConnectionStatus = phFields.FindControl("dvpConnectionStatus") as RockDropDownList;
                                            value = dvpConnectionStatus != null ? dvpConnectionStatus.SelectedValueAsInt() : null;
                                            break;
                                    }
                                }
                                else if (field.AttributeId.HasValue)
                                {
                                    var attribute = AttributeCache.Get(field.AttributeId.Value);
                                    string fieldId = "attribute_field_" + attribute.Id.ToString();

                                    Control control = phFields.FindControl(fieldId);
                                    if (control != null)
                                    {
                                        value = attribute.FieldType.Field.GetEditValue(control, attribute.QualifierValues);
                                    }
                                }

                                if (value != null)
                                {
                                    RegistrantState.FieldValues.AddOrReplace(field.Id, new FieldValueObject(field, value));
                                }
                                else
                                {
                                    RegistrantState.FieldValues.Remove(field.Id);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Loop through all the fees adn call ParseFee for each one. Creates the fee controls and populates with data.
        /// </summary>
        private void ParseFees()
        {
            if (RegistrationTemplate.Fees != null)
            {
                foreach (var fee in RegistrationTemplate.Fees.OrderBy(f => f.Order))
                {
                    List<FeeInfo> feeValues = fee.GetFeeInfoFromControls(phFees);
                    if (fee != null)
                    {
                        RegistrantState.FeeValues.AddOrReplace(fee.Id, feeValues);
                    }
                }
            }
        }


        /// <summary>
        /// Saves the person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="familyGuid">The family unique identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="adultRoleId">The adult role identifier.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <returns></returns>
        private Person SavePerson(RockContext rockContext, Person person, Guid familyGuid, int? campusId, Location location, int adultRoleId, int childRoleId,
            Dictionary<Guid, int> multipleFamilyGroupIds, ref int? singleFamilyId)
        {
            int? familyId = null;

            if (person.Id > 0)
            {
                rockContext.SaveChanges();

                // Set the family guid for any other registrants that were selected to be in the same family
                var family = person.GetFamilies(rockContext).FirstOrDefault();
                if (family != null)
                {
                    familyId = family.Id;
                    multipleFamilyGroupIds.AddOrIgnore(familyGuid, family.Id);
                    if (!singleFamilyId.HasValue)
                    {
                        singleFamilyId = family.Id;
                    }

                    if (campusId.HasValue)
                    {
                        family.CampusId = campusId;
                    }
                }
            }
            else
            {
                //
                // not adding new people via this registration form
                // relies on the person record already existing
                //
                //// If we've created the family aready for this registrant, add them to it
                //if (
                //        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid ) ) ||
                //        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                //    )
                //{
                //    // Add person to existing family
                //    var age = person.Age;
                //    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                //    familyId = RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
                //        multipleFamilyGroupIds[familyGuid] :
                //        singleFamilyId.Value;
                //    PersonService.AddPersonToFamily( person, true, multipleFamilyGroupIds[familyGuid], familyRoleId, rockContext );

                //}

                //// otherwise create a new family
                //else
                {
                    // Create Person/Family
                    var familyGroup = PersonService.SaveNewPerson(person, rockContext, campusId, false);
                    if (familyGroup != null)
                    {
                        familyId = familyGroup.Id;

                        // Store the family id for next person
                        multipleFamilyGroupIds.AddOrIgnore(familyGuid, familyGroup.Id);
                        if (!singleFamilyId.HasValue)
                        {
                            singleFamilyId = familyGroup.Id;
                        }
                    }
                }
            }

            if (familyId.HasValue && location != null)
            {
                var homeLocationType = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid());
                if (homeLocationType != null)
                {
                    var familyGroup = new GroupService(rockContext).Get(familyId.Value);
                    if (familyGroup != null)
                    {
                        GroupService.AddNewGroupAddress(
                            rockContext,
                            familyGroup,
                            Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                            location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country, true);
                    }
                }
            }

            return new PersonService(rockContext).Get(person.Id);
        }

        /// <summary>
        /// Saves the phone.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="person">The person.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SavePhone(object fieldValue, Person person, Guid phoneTypeGuid, History.HistoryChangeList changes)
        {
            var phoneNumber = fieldValue as PhoneNumber;
            if (phoneNumber != null)
            {
                string cleanNumber = PhoneNumber.CleanNumber(phoneNumber.Number);
                if (!string.IsNullOrWhiteSpace(cleanNumber))
                {
                    var numberType = DefinedValueCache.Get(phoneTypeGuid);
                    if (numberType != null)
                    {
                        var phone = person.PhoneNumbers.FirstOrDefault(p => p.NumberTypeValueId == numberType.Id);
                        string oldPhoneNumber = string.Empty;
                        if (phone == null)
                        {
                            phone = new PhoneNumber { NumberTypeValueId = numberType.Id };
                            person.PhoneNumbers.Add(phone);
                        }
                        else
                        {
                            oldPhoneNumber = phone.NumberFormattedWithCountryCode;
                        }
                        phone.CountryCode = PhoneNumber.CleanNumber(phoneNumber.CountryCode);
                        phone.Number = cleanNumber;

                        History.EvaluateChange(
                            changes,
                            string.Format("{0} Phone", numberType.Value),
                            oldPhoneNumber,
                            phoneNumber.NumberFormattedWithCountryCode);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the person field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private void CreatePersonField(bool hasDependantVisibilityRule, RegistrationTemplateFormField field, bool setValue, object fieldValue)
        {
            Control personFieldControl = field.GetPersonControl(setValue, fieldValue, false, BlockValidationGroup);

            if (personFieldControl != null)
            {
                var fieldVisibilityWrapper = new FieldVisibilityWrapper
                {
                    ID = "_fieldVisibilityWrapper_field_" + field.Guid.ToString("N"),
                    FormFieldId = field.Id,
                    FieldVisibilityRules = field.FieldVisibilityRules
                };

                fieldVisibilityWrapper.EditValueUpdated += FieldVisibilityWrapper_EditValueUpdated;

                phFields.Controls.Add(fieldVisibilityWrapper);

                fieldVisibilityWrapper.Controls.Add(personFieldControl);
                fieldVisibilityWrapper.EditControl = personFieldControl;

                if (hasDependantVisibilityRule && FieldVisibilityRules.IsFieldSupported(field.PersonFieldType))
                {
                    var fieldType = FieldVisibilityRules.GetSupportedFieldTypeCache(field.PersonFieldType).Field;

                    if (fieldType.HasChangeHandler(personFieldControl))
                    {
                        fieldType.AddChangeHandler(personFieldControl, () =>
                        {
                            fieldVisibilityWrapper.TriggerEditValueUpdated(personFieldControl, new FieldVisibilityWrapper.FieldEventArgs(null, personFieldControl));
                        });
                    }
                }
            }
        }

        #endregion

        #endregion

        protected void ppPerson_SelectPerson(object sender, EventArgs e)
        {
            if (RegistrantState != null && (ppPerson.PersonId.HasValue && ppPerson.PersonId > 0))
            {
                RockContext rockContext = new RockContext();
                var personService = new PersonService(rockContext);
                var registrantService = new RegistrationRegistrantService(rockContext);
                var registrantFeeService = new RegistrationRegistrantFeeService(rockContext);
                var registrationTemplateFeeService = new RegistrationTemplateFeeService(rockContext);
                RegistrationRegistrant registrant = null;
                if (RegistrantState.Id > 0)
                {
                    registrant = registrantService.Get(RegistrantState.Id);
                }

                var registration = new RegistrationService(rockContext).Get(RegistrantState.RegistrationId);
                var alreadyRegistered = registrantService.Queryable().Any(r => r.PersonAliasId == ppPerson.PersonAliasId && r.Registration.RegistrationInstanceId == registration.RegistrationInstanceId);
                if (!alreadyRegistered)
                {
                    var registrantChanges = new History.HistoryChangeList();
                    var personChanges = new History.HistoryChangeList();

                    if (registrant == null)
                    {
                        registrant = new RegistrationRegistrant();
                        registrant.RegistrationId = RegistrantState.RegistrationId;
                        registrantService.Add(registrant);
                        registrantChanges.AddChange(History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registrant");
                    }

                    if (!registrant.PersonAliasId.Equals(ppPerson.PersonAliasId))
                    {
                        string prevPerson = (registrant.PersonAlias != null && registrant.PersonAlias.Person != null) ?
                            registrant.PersonAlias.Person.FullName : string.Empty;
                        string newPerson = ppPerson.PersonName;
                        History.EvaluateChange(registrantChanges, "Person", prevPerson, newPerson);
                    }
                    registrant.PersonAliasId = ppPerson.PersonAliasId.Value;

                    // set cost and discounts
                    History.EvaluateChange(registrantChanges, "Cost", registrant.Cost, cbCost.Text.AsDecimal());
                    registrant.Cost = cbCost.Text.AsDecimal();

                    History.EvaluateChange(registrantChanges, "Discount Applies", registrant.DiscountApplies, cbDiscountApplies.Checked);
                    registrant.DiscountApplies = cbDiscountApplies.Checked;

                    // Get the name of registrant for history
                    string registrantName = "Unknown";
                    var person = new Person();
                    if (ppPerson.PersonId.HasValue)
                    {
                        person = personService.Get(ppPerson.PersonId.Value);
                        if (person != null)
                        {
                            registrantName = person.FullName;
                        }
                    }

                    if (!registrant.IsValid)
                    {
                        // Controls will render the error messages
                        return;
                    }

                    try
                    {
                        rockContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    HistoryService.SaveChanges(
                        rockContext,
                        typeof(Registration),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        registrant.RegistrationId,
                        registrantChanges,
                        "Registrant: " + registrantName,
                        null, null);

                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add("RegistrationId", RegistrantState.RegistrationId.ToString());
                    qryParams.Add("RegistrantId", registrant.Id.ToString());
                    NavigateToCurrentPage(qryParams);
                }
                else
                {
                    // person already a registrant in instance
                    ppPerson.SelectedValue = null;
                    ppPerson.PersonName = string.Empty;
                    return;
                }
            }
        }
    }
}
