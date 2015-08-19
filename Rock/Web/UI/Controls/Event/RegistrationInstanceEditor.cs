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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

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
        AccountPicker _apAccount;
        PersonPicker _ppContact;
        PhoneNumberBox _pnContactPhone;
        EmailBox _ebContactEmail;
        DateTimePicker _dtpSendReminder;
        RockCheckBox _cbReminderSent;
        CodeEditor _ceAdditionalReminderDetails;
        CodeEditor _ceAdditionalConfirmationDetails;

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
                return _ceAdditionalReminderDetails.Text;
            }
            set
            {
                EnsureChildControls();
                _ceAdditionalReminderDetails.Text = value;
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
                return _ceAdditionalConfirmationDetails.Text;
            }
            set
            {
                EnsureChildControls();
                _ceAdditionalConfirmationDetails.Text = value;
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
                _ppContact.ValidationGroup = value;
                _pnContactPhone.ValidationGroup = value;
                _ebContactEmail.ValidationGroup = value;
                _apAccount.ValidationGroup = value;
                _dtpSendReminder.ValidationGroup = value;
                _cbReminderSent.ValidationGroup = value;
                _ceAdditionalReminderDetails.ValidationGroup = value;
                _ceAdditionalConfirmationDetails.ValidationGroup = value;
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
                _ppContact.SetValue( instance.ContactPersonAlias != null ? instance.ContactPersonAlias.Person : null );
                _pnContactPhone.Text = instance.ContactPhone;
                _ebContactEmail.Text = instance.ContactEmail;
                _apAccount.SetValue( instance.AccountId );
                _apAccount.Visible = instance.RegistrationTemplate != null && instance.RegistrationTemplate.FinancialGatewayId.HasValue;
                _dtpSendReminder.SelectedDateTime = instance.SendReminderDateTime;
                _cbReminderSent.Checked = instance.ReminderSent;
                _ceAdditionalReminderDetails.Text = instance.AdditionalReminderDetails;
                _ceAdditionalConfirmationDetails.Text = instance.AdditionalConfirmationDetails;
            }
            else
            {
                _tbName.Text = string.Empty;
                _cbIsActive.Checked = true;
                _ceDetails.Text = string.Empty;
                _dtpStart.SelectedDateTime = null;
                _dtpEnd.SelectedDateTime = null;
                _nbMaxAttendees.Text = string.Empty;
                _ppContact.SetValue( null );
                _pnContactPhone.Text = string.Empty;
                _ebContactEmail.Text = string.Empty;
                _apAccount.SetValue( null );
                _dtpSendReminder.SelectedDateTime = null;
                _cbReminderSent.Checked = false;
                _ceAdditionalReminderDetails.Text = string.Empty;
                _ceAdditionalConfirmationDetails.Text = string.Empty;
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
                instance.ContactPersonAliasId = _ppContact.PersonAliasId;
                instance.ContactPhone = _pnContactPhone.Text;
                instance.ContactEmail = _ebContactEmail.Text;
                instance.AccountId = _apAccount.SelectedValue.AsIntegerOrNull();
                instance.SendReminderDateTime = _dtpSendReminder.SelectedDateTime;
                instance.ReminderSent = _cbReminderSent.Checked;
                instance.AdditionalReminderDetails = _ceAdditionalReminderDetails.Text;
                instance.AdditionalConfirmationDetails = _ceAdditionalConfirmationDetails.Text;
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

                _ceAdditionalReminderDetails = new CodeEditor();
                _ceAdditionalReminderDetails.ID = this.ID + "_ceAdditionalReminderDetails";
                _ceAdditionalReminderDetails.Label = "Additional Reminder Details";
                _ceAdditionalReminderDetails.Help = "These reminder details will be appended to those in the registration template when sending the reminder email.";
                _ceAdditionalReminderDetails.EditorMode = CodeEditorMode.Html;
                _ceAdditionalReminderDetails.EditorTheme = CodeEditorTheme.Rock;
                _ceAdditionalReminderDetails.EditorHeight = "100";
                Controls.Add( _ceAdditionalReminderDetails );

                _ceAdditionalConfirmationDetails = new CodeEditor();
                _ceAdditionalConfirmationDetails.ID = this.ID + "_ceAdditionalConfirmationDetails";
                _ceAdditionalConfirmationDetails.Label = "Additional Confirmation Details";
                _ceAdditionalConfirmationDetails.Help = "These confirmation details will be appended to those from the registration template when displayed at the end of the registration process.";
                _ceAdditionalConfirmationDetails.EditorMode = CodeEditorMode.Html;
                _ceAdditionalConfirmationDetails.EditorTheme = CodeEditorTheme.Rock;
                _ceAdditionalConfirmationDetails.EditorHeight = "100";
                Controls.Add( _ceAdditionalConfirmationDetails );

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
            writer.RenderEndTag();  // col-md-6

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _apAccount.RenderControl( writer );
            _ppContact.RenderControl( writer );
            _pnContactPhone.RenderControl( writer );
            _ebContactEmail.RenderControl( writer );
            writer.RenderEndTag();  // col-md-6

            writer.RenderEndTag();  // row

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-8" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        _dtpSendReminder.RenderControl( writer );
                        writer.RenderEndTag();  // col-xs-6

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-4" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        _cbReminderSent.RenderControl( writer );
                        writer.RenderEndTag();  // col-xs-6

                    writer.RenderEndTag();  // row

                writer.RenderEndTag();  // col-md-6

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderEndTag();  // col-md-6

            writer.RenderEndTag();  // row

            _ceAdditionalReminderDetails.RenderControl( writer );

            _ceAdditionalConfirmationDetails.RenderControl( writer );

        }

    }
}