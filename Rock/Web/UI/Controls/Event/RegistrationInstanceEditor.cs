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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Financial;
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
        DatePicker _dpPaymentDeadline;
        RockCheckBox _cbReminderSent;
        HtmlEditor _htmlRegistrationInstructions;
        HtmlEditor _htmlAdditionalReminderDetails;
        HtmlEditor _htmlAdditionalConfirmationDetails;
        RockDropDownList _ddlGatewayMerchants;
        RockDropDownList _ddlGatewayFunds;
        NumberBox _nbTimeoutLengthMinutes;
        NumberBox _nbTimeoutThreshold;

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

        private bool _showRegistrationTypeSection = true;


        /// <summary>
        /// Gets or sets a value indicating whether [show registration type section].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show registration type section]; otherwise, <c>false</c>.
        /// </value>

        public bool ShowRegistrationTypeSection
        {
            get => _showRegistrationTypeSection;
            set
            {
                EnsureChildControls();
                _showRegistrationTypeSection = value;
                if ( _tbName.RequiredFieldValidator != null )
                {
                    _tbName.RequiredFieldValidator.Enabled = value;
                }
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
        public int? MaxAttendees
        {
            get
            {
                EnsureChildControls();
                return _nbMaxAttendees.Text.AsIntegerOrNull();
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
                return _cbCost.Value;
            }
            set
            {
                EnsureChildControls();
                _cbCost.Value = value;
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
                return _cbMinimumInitialPayment.Value;
            }
            set
            {
                EnsureChildControls();
                _cbMinimumInitialPayment.Value = value;
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
                return _cbDefaultPaymentAmount.Value;
            }
            set
            {
                EnsureChildControls();
                _cbDefaultPaymentAmount.Value = value;
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
        public string RegistrationInstructions
        {
            get
            {
                EnsureChildControls();
                return _htmlRegistrationInstructions.Text;
            }
            set
            {
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
        /// Gets or sets the timeout length minutes.
        /// </summary>
        /// <value>
        /// The timeout length minutes.
        /// </value>
        public int? TimeoutLengthMinutes
        {
            get
            {
                EnsureChildControls();
                return _nbTimeoutLengthMinutes.IntegerValue;
            }
            set
            {
                EnsureChildControls();
                _nbTimeoutLengthMinutes.IntegerValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout threshold.
        /// </summary>
        /// <value>
        /// The timeout threshold.
        /// </value>
        public int? TimeoutThreshold
        {
            get
            {
                EnsureChildControls();
                return _nbTimeoutThreshold.IntegerValue;
            }
            set
            {
                EnsureChildControls();
                _nbTimeoutThreshold.IntegerValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the gateway entity type identifier.
        /// </summary>
        /// <value>
        /// The gateway entity type identifier.
        /// </value>
        private int? GatewayEntityTypeId { get; set; }

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
                return _tbUrlSlug.ValidationGroup;
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
                _dpPaymentDeadline.ValidationGroup = value;
                _cbReminderSent.ValidationGroup = value;
                _htmlAdditionalConfirmationDetails.ValidationGroup = value;
                _htmlRegistrationInstructions.ValidationGroup = value;
                _htmlAdditionalReminderDetails.ValidationGroup = value;
                _ddlGatewayFunds.ValidationGroup = value;
                _ddlGatewayMerchants.ValidationGroup = value;
                _nbTimeoutThreshold.ValidationGroup = value;
                _nbTimeoutLengthMinutes.ValidationGroup = value;
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
                _cbCost.Value = instance.Cost;
                _cbCost.Visible = instance.RegistrationTemplate != null && ( instance.RegistrationTemplate.SetCostOnInstance ?? false );
                _cbMinimumInitialPayment.Value = instance.MinimumInitialPayment;
                _cbMinimumInitialPayment.Visible = instance.RegistrationTemplate != null && ( instance.RegistrationTemplate.SetCostOnInstance ?? false );
                _cbDefaultPaymentAmount.Value = instance.DefaultPayment;
                _cbDefaultPaymentAmount.Visible = instance.RegistrationTemplate != null && ( instance.RegistrationTemplate.SetCostOnInstance ?? false );
                _apAccount.SetValue( instance.AccountId );
                _apAccount.Visible = instance.RegistrationTemplate != null && instance.RegistrationTemplate.FinancialGatewayId.HasValue;
                _dtpSendReminder.SelectedDateTime = instance.SendReminderDateTime;
                _dpPaymentDeadline.SelectedDate = instance.PaymentDeadlineDate;
                _cbReminderSent.Checked = instance.ReminderSent;
                _htmlRegistrationInstructions.Text = instance.RegistrationInstructions;
                _htmlAdditionalReminderDetails.Text = instance.AdditionalReminderDetails;
                _htmlAdditionalConfirmationDetails.Text = instance.AdditionalConfirmationDetails;
                _nbTimeoutThreshold.IntegerValue = instance.TimeoutThreshold;
                _nbTimeoutLengthMinutes.IntegerValue = instance.TimeoutLengthMinutes;

                if ( instance.RegistrationTemplate.FinancialGateway.IsRedirectionGateway() )
                {
                    GatewayEntityTypeId = instance.RegistrationTemplate.FinancialGateway.EntityTypeId;
                    var gateway = instance.RegistrationTemplate.FinancialGateway.GetGatewayComponent() as IRedirectionGatewayComponent;
                    _ddlGatewayMerchants.Label = gateway.MerchantFieldLabel;
                    _ddlGatewayMerchants.Visible = true;
                    _ddlGatewayMerchants.Items.Clear();
                    _ddlGatewayMerchants.Items.Add( new ListItem( string.Empty, string.Empty ) );

                    var merchants = gateway.GetMerchants().ToDictionary( x => x.Key, x => x.Value );
                    var merchantListItems = merchants.Select( x => new ListItem( x.Value, x.Key ) ).ToArray();
                    _ddlGatewayMerchants.Items.AddRange( merchantListItems );

                    _ddlGatewayFunds.Label = gateway.FundFieldLabel;
                    _ddlGatewayFunds.Visible = true;

                    // Only set the merchant value (in _ddlGatewayMerchants.SelectedValue) if the instance has a value
                    // and the value is in the list of merchants in the database (avoids hitting a null reference if
                    // the merchant has been deleted from the database).
                    bool shouldSetMerchant = instance.ExternalGatewayMerchantId.HasValue
                        && merchants.ContainsKey( instance.ExternalGatewayMerchantId.ToString() );

                    var merchantFunds = new Dictionary<string, string>();
                    bool shouldSetFund = false;

                    if ( shouldSetMerchant )
                    {
                        _ddlGatewayMerchants.SelectedValue = instance.ExternalGatewayMerchantId.ToString();

                        merchantFunds = gateway.GetMerchantFunds( instance.ExternalGatewayMerchantId.ToString() )
                            .ToDictionary( x => x.Key, x => x.Value );

                        // Only set the merchant fund value (in _ddlGatewayFunds.SelectedValue) if we have already set
                        // the merchant, the instance has a value, and the value is in the list of funds for the selected
                        // merchant.
                        shouldSetFund = instance.ExternalGatewayFundId.HasValue
                            && merchantFunds.ContainsKey( instance.ExternalGatewayFundId.ToString() );

                        // Add gateway merchant fund options.
                        var fundListItems = merchantFunds.Select( x => new ListItem( x.Value, x.Key ) ).ToArray();
                        _ddlGatewayFunds.Items.Clear();
                        _ddlGatewayFunds.Items.Add( new ListItem( string.Empty, string.Empty ) );
                        _ddlGatewayFunds.Items.AddRange( fundListItems );
                    }

                    if ( shouldSetFund )
                    {
                        _ddlGatewayFunds.SelectedValue = instance.ExternalGatewayFundId.ToString();
                    }

                    _apAccount.Visible = true;
                }
                else
                {
                    _ddlGatewayFunds.ClearSelection();
                    _ddlGatewayMerchants.ClearSelection();
                    _apAccount.Visible = true;
                }
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
                _cbCost.Value = null;
                _cbMinimumInitialPayment.Value = null;
                _cbDefaultPaymentAmount.Value = null;
                _apAccount.SetValue( null );
                _dtpSendReminder.SelectedDateTime = null;
                _dpPaymentDeadline.SelectedDate = null;
                _cbReminderSent.Checked = false;
                _nbTimeoutLengthMinutes.IntegerValue = null;
                _nbTimeoutThreshold.IntegerValue = null;
                _htmlRegistrationInstructions.Text = string.Empty;
                _htmlAdditionalReminderDetails.Text = string.Empty;
                _htmlAdditionalConfirmationDetails.Text = string.Empty;
                _ddlGatewayFunds.ClearSelection();
                _ddlGatewayMerchants.ClearSelection();
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
                if ( ShowActive && ShowRegistrationTypeSection )
                {
                    instance.IsActive = _cbIsActive.Checked;
                }
                instance.Details = _ceDetails.Text;
                instance.StartDateTime = _dtpStart.SelectedDateTime;
                instance.EndDateTime = _dtpEnd.SelectedDateTime;
                instance.MaxAttendees = _nbMaxAttendees.Text.AsIntegerOrNull();
                instance.RegistrationWorkflowTypeId = _wtpRegistrationWorkflow.SelectedValueAsInt();
                instance.ContactPersonAliasId = _ppContact.PersonAliasId;
                instance.ContactPhone = _pnContactPhone.Text;
                instance.ContactEmail = _ebContactEmail.Text;
                instance.Cost = _cbCost.Value;
                instance.MinimumInitialPayment = _cbMinimumInitialPayment.Value;
                instance.DefaultPayment = _cbDefaultPaymentAmount.Value;
                int accountId = _apAccount.SelectedValue.AsInteger();
                instance.AccountId = accountId > 0 ? accountId : ( int? ) null;
                instance.SendReminderDateTime = _dtpSendReminder.SelectedDateTime;
                instance.PaymentDeadlineDate = _dpPaymentDeadline.SelectedDate;
                instance.ReminderSent = _cbReminderSent.Checked;
                instance.RegistrationInstructions = _htmlRegistrationInstructions.Text;
                instance.AdditionalReminderDetails = _htmlAdditionalReminderDetails.Text;
                instance.AdditionalConfirmationDetails = _htmlAdditionalConfirmationDetails.Text;
                instance.TimeoutIsEnabled = _nbTimeoutLengthMinutes.IntegerValue.HasValue;
                instance.TimeoutLengthMinutes = _nbTimeoutLengthMinutes.IntegerValue;
                instance.TimeoutThreshold = _nbTimeoutThreshold.IntegerValue;

                var gateway = new FinancialGateway { EntityTypeId = GatewayEntityTypeId };
                var gatewayComponent = gateway.GetGatewayComponent() as IRedirectionGatewayComponent;
                if ( gatewayComponent != null )
                {
                    instance.ExternalGatewayMerchantId = _ddlGatewayMerchants.SelectedValue.AsIntegerOrNull();
                    if ( instance.ExternalGatewayMerchantId != null )
                    {
                        instance.ExternalGatewayFundId = _ddlGatewayFunds.SelectedValue.AsIntegerOrNull();
                    }
                }
                else
                {
                    instance.ExternalGatewayMerchantId = null;
                    instance.ExternalGatewayFundId = null;
                }
            }
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            GatewayEntityTypeId = ViewState["GatewayEntityTypeId"] as int?;
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["GatewayEntityTypeId"] = GatewayEntityTypeId;

            return base.SaveViewState();
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
                _nbMaxAttendees.Help = "Total number of people who can register for the event. Leave blank for unlimited.";
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
                _cbDefaultPaymentAmount.Help = "The default payment amount per registrant. Leave value blank to default to the full amount. NOTE: This requires that a Minimum Initial Payment is greater than 0.";
                Controls.Add( _cbDefaultPaymentAmount );

                _apAccount = new AccountPicker();
                _apAccount.ID = this.ID + "_apAccount";
                _apAccount.Label = "Account";
                _apAccount.Required = true;
                _apAccount.DisplayActiveOnly = true;
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

                _dpPaymentDeadline = new DatePicker();
                _dpPaymentDeadline.ID = this.ID + "_dpPaymentDeadline";
                _dpPaymentDeadline.Label = "Payment Deadline";
                _dpPaymentDeadline.Help = "The date that all payments must be completed by. This date will be used by the payment plan feature to calculate the payment schedule and amount.";
                Controls.Add( _dpPaymentDeadline );

                _cbReminderSent = new RockCheckBox();
                _cbReminderSent.ID = this.ID + "_cbReminderSent";
                _cbReminderSent.Label = "Reminder Sent";
                _cbReminderSent.Text = "Yes";
                Controls.Add( _cbReminderSent );

                _nbTimeoutLengthMinutes = new NumberBox();
                _nbTimeoutLengthMinutes.ID = ID + "_nbTimeoutLengthMinutes";
                _nbTimeoutLengthMinutes.Label = "Timeout Length";
                _nbTimeoutLengthMinutes.AppendText = "minutes";
                _nbTimeoutLengthMinutes.Help = "To help with registrations with limited slots a timeout can be applied to registration sessions. When applied, individuals will have the configured timeout duration to complete each page of the registration. Their spots are reserved until the timeout elapses, or they advance in the registration process.";
                Controls.Add( _nbTimeoutLengthMinutes );

                _nbTimeoutThreshold = new NumberBox();
                _nbTimeoutThreshold.ID = ID + "_nbTimeoutThreshold";
                _nbTimeoutThreshold.Label = "Timeout Threshold";
                _nbTimeoutThreshold.AppendText = "registrants";
                _nbTimeoutThreshold.Help = "The use of registration sessions can add stress to the registration experience. The Timeout Threshold determines the lower limit of spots available before the session feature is enabled. This allows early registrations to proceed without worrying about a session since they are not in danger of being in an oversell situation.";
                Controls.Add( _nbTimeoutThreshold );

                _htmlRegistrationInstructions = new HtmlEditor();
                _htmlRegistrationInstructions.ID = this.ID + "_htmlRegistrationInstructions";
                _htmlRegistrationInstructions.Toolbar = HtmlEditor.ToolbarConfig.Light;
                _htmlRegistrationInstructions.Label = "Registration Instructions";
                _htmlRegistrationInstructions.Help = "These instructions will appear at the beginning of the registration process. Instructions can be provided on the registration template also. Any instructions here will override the instructions on the template.";
                _htmlRegistrationInstructions.Height = 200;
                Controls.Add( _htmlRegistrationInstructions );

                _htmlAdditionalReminderDetails = new HtmlEditor();
                _htmlAdditionalReminderDetails.ID = this.ID + "_htmlAdditionalReminderDetails";
                _htmlAdditionalReminderDetails.Toolbar = HtmlEditor.ToolbarConfig.Light;
                _htmlAdditionalReminderDetails.Label = "Additional Reminder Details";
                _htmlAdditionalReminderDetails.Help = "These reminder details will be included in the reminder notification.";
                _htmlAdditionalReminderDetails.Height = 200;
                Controls.Add( _htmlAdditionalReminderDetails );

                _htmlAdditionalConfirmationDetails = new HtmlEditor();
                _htmlAdditionalConfirmationDetails.ID = this.ID + "_htmlAdditionalConfirmationDetails";
                _htmlAdditionalConfirmationDetails.Toolbar = HtmlEditor.ToolbarConfig.Light;
                _htmlAdditionalConfirmationDetails.Label = "Additional Confirmation Details";
                _htmlAdditionalConfirmationDetails.Help = "These confirmation details will be appended to those from the registration template when displayed at the end of the registration process.";
                _htmlAdditionalConfirmationDetails.Height = 200;
                Controls.Add( _htmlAdditionalConfirmationDetails );

                _ddlGatewayMerchants = new RockDropDownList
                {
                    ID = $"{ID}{nameof( _ddlGatewayMerchants )}",
                    Visible = false,
                    AutoPostBack = true,
                };

                _ddlGatewayMerchants.SelectedIndexChanged += _ddlGatewayMerchants_SelectedIndexChanged;

                Controls.Add( _ddlGatewayMerchants );

                _ddlGatewayFunds = new RockDropDownList
                {
                    ID = $"{ID}{nameof( _ddlGatewayFunds )}",
                    Visible = false
                };
                Controls.Add( _ddlGatewayFunds );

                _controlsLoaded = true;
            }
        }

        private void _ddlGatewayMerchants_SelectedIndexChanged( object sender, EventArgs e )
        {
            var gateway = new FinancialGateway { EntityTypeId = GatewayEntityTypeId };
            var gatewayComponent = gateway.GetGatewayComponent() as IRedirectionGatewayComponent;
            if ( gatewayComponent != null )
            {
                _ddlGatewayFunds.Items.Clear();
                if ( _ddlGatewayMerchants.SelectedValue != null )
                {
                    _ddlGatewayFunds.Items.Add( new ListItem( string.Empty, string.Empty ) );
                    _ddlGatewayFunds
                        .Items
                        .AddRange(
                            gatewayComponent.GetMerchantFunds( _ddlGatewayMerchants.SelectedValue )
                            .Select( x => new ListItem( x.Value, x.Key )
                        ).ToArray() );
                    _ddlGatewayFunds.ClearSelection();
                }
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
            if ( ShowRegistrationTypeSection )
            {
                RockControlHelper.RenderSection( "Registration Type", CssClass, writer, ( HtmlTextWriter ) =>
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div ); // row

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _tbName.RenderControl( writer );
                    writer.RenderEndTag();  // col-md-6

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _cbIsActive.RenderControl( writer );
                    _tbUrlSlug.RenderControl( writer );
                    writer.RenderEndTag();  // col-md-6

                    writer.RenderEndTag(); // row
                } );
            }

            RockControlHelper.RenderSection( "Registration Details", CssClass, writer, ( HtmlTextWriter ) =>
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _dtpStart.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _dtpEnd.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _nbMaxAttendees.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _wtpRegistrationWorkflow.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _dtpSendReminder.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _dpPaymentDeadline.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-8" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderEndTag();  // col-xs-8

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-4" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbReminderSent.Visible = _cbReminderSent.Checked;
                _cbReminderSent.RenderControl( writer );
                writer.RenderEndTag();  // col-xs-4

                writer.RenderEndTag();  // row
            } );

            RockControlHelper.RenderSection( "Registration Contact Information", CssClass, writer, ( HtmlTextWriter ) =>
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _ppContact.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _pnContactPhone.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _ebContactEmail.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();
            } );

            if ( _cbCost.Visible || _cbMinimumInitialPayment.Visible || _cbDefaultPaymentAmount.Visible || _apAccount.Visible || _ddlGatewayMerchants.Visible )
            {
                RockControlHelper.RenderSection( "Registration Financial Information", CssClass, writer, ( HtmlTextWriter ) =>
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _cbCost.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _cbMinimumInitialPayment.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _ddlGatewayMerchants.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _ddlGatewayFunds.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _apAccount.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _cbDefaultPaymentAmount.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.RenderEndTag();
                } );
            }

            RockControlHelper.RenderSection( "Registration Messages", CssClass, writer, ( HtmlTextWriter ) =>
            {
                _htmlRegistrationInstructions.RenderControl( writer );

                _htmlAdditionalReminderDetails.RenderControl( writer );

                _htmlAdditionalConfirmationDetails.RenderControl( writer );
            } );

            RockControlHelper.RenderSection( "Registration Session", CssClass, writer, ( HtmlTextWriter ) =>
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _nbTimeoutLengthMinutes.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _nbTimeoutThreshold.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();
            } );
        }
    }
}