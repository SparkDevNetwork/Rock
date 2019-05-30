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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used to edit registration instance details
    /// </summary>
    [ToolboxData( "<{0}:RegistrationInstanceEditor runat=server></{0}:RegistrationInstanceEditor>" )]
    public class RegistrationInstanceEditor : CompositeControl, IHasValidationGroup
    {
        private bool _controlsLoaded = false;

        RockTextBox _tbName;
        RockCheckBox _cbIsActive;
        RockTextBox _tbUrlSlug;
        CodeEditor _ceDetails;
        DateTimePicker _dtpStart;
        DateTimePicker _dtpEnd;
        NumberBox _nbMaxAttendees;
        WorkflowTypePicker _wtpRegistrationWorkflow;
        CurrencyBox _cbCost;
        CurrencyBox _cbMinimumInitialPayment;
        CurrencyBox _cbDefaultPaymentAmount;
        AccountPicker _apAccount;
        PersonPicker _ppContact;
        PhoneNumberBox _pnContactPhone;
        EmailBox _ebContactEmail;
        DateTimePicker _dtpSendReminder;
        RockCheckBox _cbReminderSent;
        HtmlEditor _htmlRegistrationInstructions;
        HtmlEditor _htmlAdditionalReminderDetails;
        HtmlEditor _htmlAdditionalConfirmationDetails;

        /// <summary>
        /// Gets or sets a value indicating whether active checkbox should be displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show active]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowActive
        {
            get
            {
                EnsureChildControls();
                return _cbIsActive.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbIsActive.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show URL slug].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show URL slug]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowUrlSlug
        {
            get
            {
                EnsureChildControls();
                return _tbUrlSlug.Visible;
            }
            set
            {
                EnsureChildControls();
                _tbUrlSlug.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                EnsureChildControls();
                return _tbName.Text;
            }
            set
            {
                EnsureChildControls();
                _tbName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RegistrationInstanceEditor"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool Active 
        {
            get
            {
                EnsureChildControls();
                return _cbIsActive.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbIsActive.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL slug.
        /// </summary>
        /// <value>
        /// The URL slug.
        /// </value>
        public string UrlSlug
        {
            get
            {
                EnsureChildControls();
                return _tbUrlSlug.Text;
            }
            set
            {
                EnsureChildControls();
                _tbUrlSlug.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public string Details
        {
            get
            {
                EnsureChildControls();
                return _ceDetails.Text;
            }
            set
            {
                EnsureChildControls();
                _ceDetails.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime? Start
        {
            get
            {
                EnsureChildControls();
                return _dtpStart.SelectedDateTime;
            }
            set
            {
                EnsureChildControls();
                _dtpStart.SelectedDateTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public DateTime? End
        {
            get
            {
                EnsureChildControls();
                return _dtpEnd.SelectedDateTime;
            }
            set
            {
                EnsureChildControls();
                _dtpEnd.SelectedDateTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum attendees.
        /// </summary>
        /// <value>
        /// The maximum attendees.
        /// </value>
        public int MaxAttendees
        {
            get
            {
                EnsureChildControls();
                return _nbMaxAttendees.Text.AsInteger();
            }
            set
            {
                EnsureChildControls();
                _nbMaxAttendees.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the registration workflow type id.
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>        
        public int? RegistrationWorkflowTypeId
        {
            get
            {
                EnsureChildControls();
                return _wtpRegistrationWorkflow.SelectedValueAsInt();
            }
            set
            {
                EnsureChildControls();
                _wtpRegistrationWorkflow.SetValue( value );
            }
        }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal? Cost
        {
            get
            {
                EnsureChildControls();
                return _cbCost.Text.AsDecimalOrNull();
            }
            set
            {
                EnsureChildControls();
                _cbCost.Text = value.HasValue ? value.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the minimum initial payment.
        /// </summary>
        /// <value>
        /// The minimum initial payment.
        /// </value>
        public decimal? MinimumInitialPayment
        {
            get
            {
                EnsureChildControls();
                return _cbMinimumInitialPayment.Text.AsDecimalOrNull();
            }
            set
            {
                EnsureChildControls();
                _cbMinimumInitialPayment.Text = value.HasValue ? value.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the default payment amount.
        /// </summary>
        /// <value>
        /// The default payment amount.
        /// </value>
        public decimal? DefaultPaymentAmount
        {
            get
            {
                EnsureChildControls();
                return _cbDefaultPaymentAmount.Text.AsDecimalOrNull();
            }
            set
            {
                EnsureChildControls();
                _cbDefaultPaymentAmount.Text = value.HasValue ? value.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show cost].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show cost]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCost
        {
            get
            {
                EnsureChildControls();
                return _cbCost.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbCost.Visible = value;
                _cbMinimumInitialPayment.Visible = value;
                _cbDefaultPaymentAmount.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int? AccountId
        {
            get
            {
                EnsureChildControls();
                return _apAccount.SelectedValue.AsIntegerOrNull();
            }
            set
            {
                EnsureChildControls();
                _apAccount.SetValue( value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show account].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show account]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAccount
        {
            get
            {
                EnsureChildControls();
                return _apAccount.Visible;
            }
            set
            {
                EnsureChildControls();
                _apAccount.Visible = value;
            }
        }

        /// <summary>
        /// Gets the contact person alias identifier.
        /// </summary>
        /// <value>
        /// The contact person alias identifier.
        /// </value>
        public int? ContactPersonAliasId
        {
            get
            {
                EnsureChildControls();
                return _ppContact.PersonAliasId;
            }
        }

        /// <summary>
        /// Sets the contact person alias.
        /// </summary>
        /// <value>
        /// The contact person alias.
        /// </value>
        public PersonAlias ContactPersonAlias
        {
            set
            {
                EnsureChildControls();
                _ppContact.SetValue( value != null ? value.Person : null );
            }
        }

        /// <summary>
        /// Gets or sets the contact phone.
        /// </summary>
        /// <value>
        /// The contact phone.
        /// </value>
        public string ContactPhone
        {
            get
            {
                EnsureChildControls();
                return _pnContactPhone.Text;
            }
            set
            {
                EnsureChildControls();
                _pnContactPhone.Text = value;
            }
        }
        /// <summary>
        /// Gets or sets the contact email.
        /// </summary>
        /// <value>
        /// The contact email.
        /// </value>
        public string ContactEmail
        {
            get
            {
                EnsureChildControls();
                return _ebContactEmail.Text;
            }
            set
            {
                EnsureChildControls();
                _ebContactEmail.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the send reminder.
        /// </summary>
        /// <value>
        /// The send reminder.
        /// </value>
        public DateTime? SendReminder
        {
            get
            {
                EnsureChildControls();
                return _dtpSendReminder.SelectedDateTime;
            }
            set
            {
                EnsureChildControls();
                _dtpSendReminder.SelectedDateTime = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [reminder sent].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reminder sent]; otherwise, <c>false</c>.
        /// </value>
        public bool ReminderSent
        {
            get
            {
                EnsureChildControls();
                return _cbReminderSent.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbReminderSent.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the registration instructions.
        /// </summary>
        /// <value>
        /// The registration instructions.
        /// </value>
        public string RegistrationInstructions {
            get {
                EnsureChildControls();
                return _htmlRegistrationInstructions.Text;
            }
            set {
                EnsureChildControls();
                _htmlRegistrationInstructions.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the additional reminder details.
        /// </summary>
        /// <value>
        /// The additional reminder details.
        /// </value>
        public string AdditionalReminderDetails
        {
            get
            {
                EnsureChildControls();
                return _htmlAdditionalReminderDetails.Text;
            }
            set
            {
                EnsureChildControls();
                _htmlAdditionalReminderDetails.Text = value; 
            }
        }

        /// <summary>
        /// Gets or sets the additional confirmation details.
        /// </summary>
        /// <value>
        /// The additional confirmation details.
        /// </value>
        public string AdditionalConfirmationDetails
        {
            get
            {
                EnsureChildControls();
                return _htmlAdditionalConfirmationDetails.Text; 
            }
            set
            {
                EnsureChildControls();
                _htmlAdditionalConfirmationDetails.Text = value; 
            }
        }

        
        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _tbName.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _tbName.ValidationGroup = value;
                _cbIsActive.ValidationGroup = value;
                _tbUrlSlug.ValidationGroup = value;
                _ceDetails.ValidationGroup = value;
                _dtpStart.ValidationGroup = value;
                _dtpEnd.ValidationGroup = value;
                _nbMaxAttendees.ValidationGroup = value;
                _wtpRegistrationWorkflow.ValidationGroup = value;
                _ppContact.ValidationGroup = value;
                _pnContactPhone.ValidationGroup = value;
                _ebContactEmail.ValidationGroup = value;
                _cbCost.ValidationGroup = value;
                _cbMinimumInitialPayment.ValidationGroup = value;
                _cbDefaultPaymentAmount.ValidationGroup = value;
                _apAccount.ValidationGroup = value;
                _dtpSendReminder.ValidationGroup = value;
                _cbReminderSent.ValidationGroup = value;
                _htmlAdditionalConfirmationDetails.ValidationGroup = value;
                _htmlRegistrationInstructions.ValidationGroup = value;
                _htmlAdditionalReminderDetails.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Reads the instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void SetValue( RegistrationInstance instance )
        {
            EnsureChildControls();

            if ( instance != null )
            {
                _tbName.Text = instance.Name;
                if ( ShowActive )
                {
                    _cbIsActive.Checked = instance.IsActive;
                }
                _ceDetails.Text = instance.Details;
                _dtpStart.SelectedDateTime = instance.StartDateTime;
                _dtpEnd.SelectedDateTime = instance.EndDateTime;
                _nbMaxAttendees.Text = instance.MaxAttendees.ToString();
                _wtpRegistrationWorkflow.SetValue( instance.RegistrationWorkflowTypeId );

                Person contactPerson = null;
                if ( instance.ContactPersonAlias != null && instance.ContactPersonAlias.Person != null )
                {
                    contactPerson = instance.ContactPersonAlias.Person;
                }
                else if ( instance.ContactPersonAliasId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        contactPerson = new PersonAliasService( rockContext )
                            .Queryable()
                            .Where( p => p.Id == instance.ContactPersonAliasId.Value )
                            .Select( p => p.Person )
                            .FirstOrDefault();
                    }
                }
                _ppContact.SetValue( contactPerson );

                _pnContactPhone.Text = instance.ContactPhone;
                _ebContactEmail.Text = instance.ContactEmail;
                _cbCost.Text = instance.Cost.HasValue ? instance.Cost.Value.ToString() : string.Empty;
                _cbCost.Visible = instance.RegistrationTemplate != null && ( instance.RegistrationTemplate.SetCostOnInstance ?? false );
                _cbMinimumInitialPayment.Text = instance.MinimumInitialPayment.HasValue ? instance.MinimumInitialPayment.Value.ToString() : string.Empty;
                _cbMinimumInitialPayment.Visible = instance.RegistrationTemplate != null && ( instance.RegistrationTemplate.SetCostOnInstance ?? false );
                _cbDefaultPaymentAmount.Text = instance.DefaultPayment.HasValue ? instance.DefaultPayment.Value.ToString() : string.Empty;
                _cbDefaultPaymentAmount.Visible = instance.RegistrationTemplate != null && ( instance.RegistrationTemplate.SetCostOnInstance ?? false );
                _apAccount.SetValue( instance.AccountId );
                _apAccount.Visible = instance.RegistrationTemplate != null && instance.RegistrationTemplate.FinancialGatewayId.HasValue;
                _dtpSendReminder.SelectedDateTime = instance.SendReminderDateTime;
                _cbReminderSent.Checked = instance.ReminderSent;
                _htmlRegistrationInstructions.Text = instance.RegistrationInstructions;
                _htmlAdditionalReminderDetails.Text = instance.AdditionalReminderDetails;
                _htmlAdditionalConfirmationDetails.Text = instance.AdditionalConfirmationDetails;
            }
            else
            {
                _tbName.Text = string.Empty;
                _cbIsActive.Checked = true;
                _ceDetails.Text = string.Empty;
                _dtpStart.SelectedDateTime = null;
                _dtpEnd.SelectedDateTime = null;
                _nbMaxAttendees.Text = string.Empty;
                _wtpRegistrationWorkflow.SetValue( null );
                _ppContact.SetValue( null );
                _pnContactPhone.Text = string.Empty;
                _ebContactEmail.Text = string.Empty;
                _cbCost.Text = string.Empty;
                _cbMinimumInitialPayment.Text = string.Empty;
                _cbDefaultPaymentAmount.Text = string.Empty;
                _apAccount.SetValue( null );
                _dtpSendReminder.SelectedDateTime = null;
                _cbReminderSent.Checked = false;
                _htmlRegistrationInstructions.Text = string.Empty;
                _htmlAdditionalReminderDetails.Text = string.Empty;
                _htmlAdditionalConfirmationDetails.Text = string.Empty;
            }
        }

        /// <summary>
        /// Updates the instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void GetValue( RegistrationInstance instance )
        {
            EnsureChildControls();

            if ( instance != null )
            {
                instance.Name = _tbName.Text;
                if ( ShowActive )
                {
                    instance.IsActive = _cbIsActive.Checked;
                }
                instance.Details = _ceDetails.Text;
                instance.StartDateTime = _dtpStart.SelectedDateTime;
                instance.EndDateTime = _dtpEnd.SelectedDateTime;
                instance.MaxAttendees = _nbMaxAttendees.Text.AsInteger();
                instance.RegistrationWorkflowTypeId = _wtpRegistrationWorkflow.SelectedValueAsInt();
                instance.ContactPersonAliasId = _ppContact.PersonAliasId;
                instance.ContactPhone = _pnContactPhone.Text;
                instance.ContactEmail = _ebContactEmail.Text;
                instance.Cost = _cbCost.Text.AsDecimalOrNull();
                instance.MinimumInitialPayment = _cbMinimumInitialPayment.Text.AsDecimalOrNull();
                instance.DefaultPayment = _cbDefaultPaymentAmount.Text.AsDecimalOrNull();
                int accountId = _apAccount.SelectedValue.AsInteger();
                instance.AccountId = accountId > 0 ? accountId : (int?)null;
                instance.SendReminderDateTime = _dtpSendReminder.SelectedDateTime;
                instance.ReminderSent = _cbReminderSent.Checked;
                instance.RegistrationInstructions = _htmlRegistrationInstructions.Text;
                instance.AdditionalReminderDetails = _htmlAdditionalReminderDetails.Text; 
                instance.AdditionalConfirmationDetails = _htmlAdditionalConfirmationDetails.Text; 
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            if ( !_controlsLoaded )
            {
                Controls.Clear();

                _tbName = new RockTextBox();
                _tbName.ID = this.ID + "_tbName";
                _tbName.Label = "Registration Instance Name";
                _tbName.Help = "The name will be used to describe the registration on the registration screens and emails.";
                _tbName.Required = true;
                Controls.Add( _tbName );

                _cbIsActive = new RockCheckBox();
                _cbIsActive.ID = this.ID + "_cbIsActive";
                _cbIsActive.Label = "Active";
                _cbIsActive.Text = "Yes";
                Controls.Add( _cbIsActive );

                _tbUrlSlug = new RockTextBox();
                _tbUrlSlug.ID = this.ID + "_tbUrlSlug";
                _tbUrlSlug.Label = "URL Slug";
                _tbUrlSlug.Visible = false;
                Controls.Add( _tbUrlSlug );


                _ceDetails = new CodeEditor();
                _ceDetails.ID = this.ID + "_ceDetails";
                _ceDetails.Label = "Details";
                _ceDetails.EditorMode = CodeEditorMode.Html;
                _ceDetails.EditorTheme = CodeEditorTheme.Rock;
                _ceDetails.EditorHeight = "100";
                _ceDetails.Visible = false; // hiding this out for now. Struggling where we'd even use this, but instead of removing it we'll just comment it out for now.
                Controls.Add( _ceDetails );
                 

                _dtpStart = new DateTimePicker();
                _dtpStart.ID = this.ID + "_dtpStart";
                _dtpStart.Label = "Registration Starts";
                Controls.Add( _dtpStart );

                _dtpEnd = new DateTimePicker();
                _dtpEnd.ID = this.ID + "_dtpEnd";
                _dtpEnd.Label = "Registration Ends";
                Controls.Add( _dtpEnd );

                _nbMaxAttendees = new NumberBox();
                _nbMaxAttendees.ID = this.ID + "_nbMaxAttendees";
                _nbMaxAttendees.Label = "Maximum Attendees";
                _nbMaxAttendees.NumberType = ValidationDataType.Integer;
                Controls.Add( _nbMaxAttendees );

                _wtpRegistrationWorkflow = new WorkflowTypePicker();
                _wtpRegistrationWorkflow.ID = this.ID + "_wtpRegistrationWorkflow";
                _wtpRegistrationWorkflow.Label = "Registration Workflow";
                _wtpRegistrationWorkflow.Help = "An optional workflow type to launch when a new registration is completed.";
                Controls.Add( _wtpRegistrationWorkflow );

                _cbCost = new CurrencyBox();
                _cbCost.ID = this.ID + "_cbCost";
                _cbCost.Label = "Cost";
                _cbCost.Help = "The cost per registrant";
                Controls.Add( _cbCost );

                _cbMinimumInitialPayment = new CurrencyBox();
                _cbMinimumInitialPayment.ID = this.ID + "_cbMinimumInitialPayment";
                _cbMinimumInitialPayment.Label = "Minimum Initial Payment";
                _cbMinimumInitialPayment.Help = "The minimum amount required per registrant. Leave value blank if full amount is required.";
                Controls.Add( _cbMinimumInitialPayment );

                _cbDefaultPaymentAmount = new CurrencyBox();
                _cbDefaultPaymentAmount.ID = this.ID + "_cbDefaultPaymentAmount";
                _cbDefaultPaymentAmount.Label = "Default Payment Amount";
                _cbDefaultPaymentAmount.Help = "The default payment amount per registrant. Leave value blank to default to the full amount. NOTE: This requires that a Minimum Initial Payment is defined.";
                Controls.Add( _cbDefaultPaymentAmount );

                _apAccount = new AccountPicker();
                _apAccount.ID = this.ID + "_apAccount";
                _apAccount.Label = "Account";
                _apAccount.Required = true;
                Controls.Add( _apAccount );

                _ppContact = new PersonPicker();
                _ppContact.ID = this.ID + "_ppContact";
                _ppContact.Label = "Contact";
                _ppContact.SelectPerson += _ppContact_SelectPerson;
                _ppContact.EnableSelfSelection = true;
                Controls.Add( _ppContact );

                _pnContactPhone = new PhoneNumberBox();
                _pnContactPhone.ID = this.ID + "_pnContactPhone";
                _pnContactPhone.Label = "Contact Phone";
                Controls.Add( _pnContactPhone );

                _ebContactEmail = new EmailBox();
                _ebContactEmail.ID = this.ID + "_ebContactEmail";
                _ebContactEmail.Label = "Contact Email";
                Controls.Add( _ebContactEmail );

                _dtpSendReminder = new DateTimePicker();
                _dtpSendReminder.ID = this.ID + "_dtpSendReminder";
                _dtpSendReminder.Label = "Send Reminder Date";
                Controls.Add( _dtpSendReminder );

                _cbReminderSent = new RockCheckBox();
                _cbReminderSent.ID = this.ID + "_cbReminderSent";
                _cbReminderSent.Label = "Reminder Sent";
                _cbReminderSent.Text = "Yes";
                Controls.Add( _cbReminderSent );

                _htmlRegistrationInstructions = new HtmlEditor();
                _htmlRegistrationInstructions.ID = this.ID + "_htmlRegistrationInstructions";
                _htmlRegistrationInstructions.Toolbar = HtmlEditor.ToolbarConfig.Light;
                _htmlRegistrationInstructions.Label = "Registration Instructions";
                _htmlRegistrationInstructions.Help = "These instructions will appear at the beginning of the registration process when selecting how many registrants for the registration. These instructions can be provided on the registration template also. Any instructions here will override the instructions on the template.";
                _htmlRegistrationInstructions.Height = 200;
                Controls.Add(_htmlRegistrationInstructions);

                _htmlAdditionalReminderDetails = new HtmlEditor();
                _htmlAdditionalReminderDetails.ID = this.ID + "_htmlAdditionalReminderDetails";
                _htmlAdditionalReminderDetails.Toolbar = HtmlEditor.ToolbarConfig.Light;
                _htmlAdditionalReminderDetails.Label = "Additional Reminder Details";
                _htmlAdditionalReminderDetails.Help = "These confirmation details will be appended to those from the registration template when displayed at the end of the registration process.";
                _htmlAdditionalReminderDetails.Height = 200;
                Controls.Add( _htmlAdditionalReminderDetails );

                _htmlAdditionalConfirmationDetails = new HtmlEditor();
                _htmlAdditionalConfirmationDetails.ID = this.ID + "_htmlAdditionalConfirmationDetails";
                _htmlAdditionalConfirmationDetails.Toolbar = HtmlEditor.ToolbarConfig.Light;
                _htmlAdditionalConfirmationDetails.Label = "Additional Confirmation Details";
                _htmlAdditionalConfirmationDetails.Help = "These confirmation details will be appended to those from the registration template when displayed at the end of the registration process.";
                _htmlAdditionalConfirmationDetails.Height = 200;
                Controls.Add( _htmlAdditionalConfirmationDetails );

                _controlsLoaded = true;
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the _ppContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _ppContact_SelectPerson( object sender, EventArgs e )
        {
            if ( _ppContact.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    Guid workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();
                    var contactInfo = new PersonService( rockContext )
                        .Queryable()
                        .Where( p => p.Id == _ppContact.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( workPhoneGuid ) )
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault()
                        } )
                        .FirstOrDefault();

                    if ( string.IsNullOrWhiteSpace( _ebContactEmail.Text ) && contactInfo != null )
                    {
                        _ebContactEmail.Text = contactInfo.Email;
                    }

                    if ( string.IsNullOrWhiteSpace( _pnContactPhone.Text ) && contactInfo != null )
                    {
                        _pnContactPhone.Text = contactInfo.Phone;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbName.RenderControl( writer );
            writer.RenderEndTag();  // col-md-6

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbIsActive.RenderControl( writer );
            _tbUrlSlug.RenderControl( writer );
            writer.RenderEndTag();  // col-md-6

            writer.RenderEndTag();  // row

            _ceDetails.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    _dtpStart.RenderControl( writer );
                    _dtpEnd.RenderControl( writer );
                    _nbMaxAttendees.RenderControl( writer );
                    _wtpRegistrationWorkflow.RenderControl( writer );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-8" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        _dtpSendReminder.RenderControl( writer );
                        writer.RenderEndTag();  // col-xs-6

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-4" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        _cbReminderSent.Visible = _cbReminderSent.Checked;
                        _cbReminderSent.RenderControl( writer );
                        writer.RenderEndTag();  // col-xs-6

                    writer.RenderEndTag();  // row

                writer.RenderEndTag();  // col-md-6
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    _ppContact.RenderControl( writer );
                    _pnContactPhone.RenderControl( writer );
                    _ebContactEmail.RenderControl( writer );

                    _cbCost.RenderControl( writer );
                    _cbMinimumInitialPayment.RenderControl( writer );
                    _cbDefaultPaymentAmount.RenderControl( writer );
                    _apAccount.RenderControl( writer );

                writer.RenderEndTag();  // col-md-6
            writer.RenderEndTag();  // row

            _htmlRegistrationInstructions.RenderControl(writer);

            _htmlAdditionalReminderDetails.RenderControl( writer );

            _htmlAdditionalConfirmationDetails.RenderControl( writer );

        }

    }
}