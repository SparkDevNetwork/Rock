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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Workflow Action Form Editor
    /// </summary>
    public class WorkflowFormEditor : CompositeControl, IHasValidationGroup, INamingContainer
    {
        private HiddenField _hfFormGuid;
        private RockDropDownList _ddlNotificationSystemEmail;
        private RockCheckBox _cbIncludeActions;
        private RockCheckBox _cbAllowNotesEntry;

        private CodeEditor _ceHeaderText;
        private CodeEditor _ceFooterText;
        private WorkflowFormActionList _falActions;
        private RockDropDownList _ddlActionAttribute;

        private ModalDialog _mdFieldVisibilityRules;

        private static class ViewStateKey
        {
            public const string ValidationGroup = "ValidationGroup";
            public const string EditingAttributeRowGuid = "EditingAttributeRowGuid";
        }

        #region PersonEntry related

        private RockCheckBox _cbAllowPersonEntry;

        private Panel _pnlPersonEntry;
        private CodeEditor _cePersonEntryPreHtml;

        private RockCheckBox _cbPersonEntryShowCampus;
        private RockCheckBox _cbPersonEntryAutofillCurrentPerson;
        private RockCheckBox _cbPersonEntryHideIfCurrentPersonKnown;
        private RockDropDownList _ddlPersonEntrySpouseEntryOption;
        private RockDropDownList _ddlPersonEntryGenderEntryOption;
        private RockDropDownList _ddlPersonEntryEmailEntryOption;
        private RockDropDownList _ddlPersonEntryMobilePhoneEntryOption;
        private RockDropDownList _ddlPersonEntrySmsOptInEntryOption;
        private RockDropDownList _ddlPersonEntryBirthdateEntryOption;
        private RockDropDownList _ddlPersonEntryAddressEntryOption;
        private RockDropDownList _ddlPersonEntryMaritalStatusEntryOption;
        private RockDropDownList _ddlPersonEntryRaceEntryOption;
        private RockDropDownList _ddlPersonEntryEthnicityEntryOption;
        private RockTextBox _tbPersonEntrySpouseLabel;
        private DefinedValuePicker _dvpPersonEntryConnectionStatus;
        private DefinedValuePicker _dvpPersonEntryRecordStatus;
        private DefinedValuePicker _dvpPersonEntryGroupLocationType;
        private DefinedValuePicker _dvpPersonEntryCampusStatus;
        private DefinedValuePicker _dvpPersonEntryCampusType;
        private RockDropDownList _ddlPersonEntryPersonAttribute;
        private RockDropDownList _ddlPersonEntrySpouseAttribute;
        private RockDropDownList _ddlPersonEntryFamilyAttribute;
        private CodeEditor _cePersonEntryPostHtml;

        #endregion PersonEntry related

        #region Workflow Attribute Rows

        private Panel _pnlWorkflowAttributes;

        #endregion  Workflow Attribute Rows

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
                return ViewState[ViewStateKey.ValidationGroup] as string;
            }

            set
            {
                ViewState[ViewStateKey.ValidationGroup] = value;
            }
        }

        /// <summary>
        /// Gets or sets the editing attribute row unique identifier.
        /// </summary>
        /// <value>
        /// The editing attribute row unique identifier.
        /// </value>
        private string EditingAttributeRowGuid
        {
            get
            {
                return ViewState[ViewStateKey.EditingAttributeRowGuid] as string;
            }

            set
            {
                ViewState[ViewStateKey.EditingAttributeRowGuid] = value;
            }
        }

        /// <summary>
        /// Gets or sets the form.
        /// </summary>
        /// <value>
        /// The form.
        /// </value>
        public WorkflowActionForm GetForm()
        {
            EnsureChildControls();

            var formGuid = _hfFormGuid.Value.AsGuid();
            if ( formGuid == Guid.Empty )
            {
                return null;
            }

            var form = new WorkflowActionForm();
            form.Guid = formGuid;

            form.NotificationSystemCommunicationId = _ddlNotificationSystemEmail.SelectedValueAsId();
            form.IncludeActionsInNotification = _cbIncludeActions.Checked;
            form.Header = _ceHeaderText.Text;
            form.Footer = _ceFooterText.Text;
            form.Actions = _falActions.Value;
            form.AllowNotes = _cbAllowNotesEntry.Checked;
            form.AllowPersonEntry = _cbAllowPersonEntry.Checked;

            form.PersonEntryPreHtml = _cePersonEntryPreHtml.Text;
            form.PersonEntryPostHtml = _cePersonEntryPostHtml.Text;
            form.PersonEntryCampusIsVisible = _cbPersonEntryShowCampus.Checked;
            form.PersonEntryAutofillCurrentPerson = _cbPersonEntryAutofillCurrentPerson.Checked;
            form.PersonEntryHideIfCurrentPersonKnown = _cbPersonEntryHideIfCurrentPersonKnown.Checked;
            form.PersonEntrySpouseEntryOption = _ddlPersonEntrySpouseEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();

            form.PersonEntryGenderEntryOption = _ddlPersonEntryGenderEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();
            form.PersonEntryEmailEntryOption = _ddlPersonEntryEmailEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();
            form.PersonEntryMobilePhoneEntryOption = _ddlPersonEntryMobilePhoneEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();
            form.PersonEntrySmsOptInEntryOption = _ddlPersonEntrySmsOptInEntryOption.SelectedValueAsEnum<WorkflowActionFormShowHideOption>();
            form.PersonEntryBirthdateEntryOption = _ddlPersonEntryBirthdateEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();
            form.PersonEntryAddressEntryOption = _ddlPersonEntryAddressEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();
            form.PersonEntryMaritalStatusEntryOption = _ddlPersonEntryMaritalStatusEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();
            form.PersonEntryRaceEntryOption = _ddlPersonEntryRaceEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();
            form.PersonEntryEthnicityEntryOption = _ddlPersonEntryEthnicityEntryOption.SelectedValueAsEnum<WorkflowActionFormPersonEntryOption>();

            form.PersonEntrySpouseLabel = _tbPersonEntrySpouseLabel.Text;
            form.PersonEntryConnectionStatusValueId = _dvpPersonEntryConnectionStatus.SelectedDefinedValueId;
            form.PersonEntryRecordStatusValueId = _dvpPersonEntryRecordStatus.SelectedDefinedValueId;
            form.PersonEntryGroupLocationTypeValueId = _dvpPersonEntryGroupLocationType.SelectedDefinedValueId;
            form.PersonEntryCampusStatusValueId = _dvpPersonEntryCampusStatus.SelectedDefinedValueId;
            form.PersonEntryCampusTypeValueId = _dvpPersonEntryCampusType.SelectedDefinedValueId;

            form.PersonEntryPersonAttributeGuid = _ddlPersonEntryPersonAttribute.SelectedValueAsGuid();
            form.PersonEntrySpouseAttributeGuid = _ddlPersonEntrySpouseAttribute.SelectedValueAsGuid();
            form.PersonEntryFamilyAttributeGuid = _ddlPersonEntryFamilyAttribute.SelectedValueAsGuid();

            foreach ( var row in AttributeRows )
            {
                var formAttribute = new WorkflowActionFormAttribute();
                formAttribute.Attribute = row.Attribute;
                formAttribute.Guid = row.Guid;
                formAttribute.Order = row.Order;
                formAttribute.IsVisible = row.IsVisible;
                formAttribute.IsReadOnly = !row.IsEditable;
                formAttribute.IsRequired = row.IsRequired;
                formAttribute.HideLabel = row.HideLabel;
                formAttribute.PreHtml = row.PreHtml;
                formAttribute.PostHtml = row.PostHtml;
                formAttribute.FieldVisibilityRules = row.VisibilityRules;
                form.FormAttributes.Add( formAttribute );
            }

            form.ActionAttributeGuid = _ddlActionAttribute.SelectedValueAsGuid();

            return form;
        }

        /// <summary>
        /// Sets the form.
        /// </summary>
        /// <param name="workflowActionForm">The workflow action form.</param>
        /// <param name="workflowTypeAttributes">The workflow type attributes.</param>
        public void SetForm( WorkflowActionForm workflowActionForm, Dictionary<Guid, Rock.Model.Attribute> workflowTypeAttributes )
        {
            EnsureChildControls();

            if ( workflowActionForm == null )
            {
                _hfFormGuid.Value = string.Empty;
                _ddlNotificationSystemEmail.SelectedIndex = 0;
                _cbIncludeActions.Checked = true;
                _ceHeaderText.Text = string.Empty;
                _ceFooterText.Text = string.Empty;
                _falActions.Value = "Submit^^^Your information has been submitted successfully.";
                _ddlNotificationSystemEmail.SelectedIndex = 0;
                _cbAllowNotesEntry.Checked = false;
                _cbAllowPersonEntry.Checked = false;
                _pnlPersonEntry.Visible = false;
                return;
            }

            _hfFormGuid.Value = workflowActionForm.Guid.ToString();
            _ddlNotificationSystemEmail.SetValue( workflowActionForm.NotificationSystemCommunicationId );
            _cbIncludeActions.Checked = workflowActionForm.IncludeActionsInNotification;
            _ceHeaderText.Text = workflowActionForm.Header;
            _ceFooterText.Text = workflowActionForm.Footer;
            _falActions.Value = workflowActionForm.Actions;
            _cbAllowNotesEntry.Checked = workflowActionForm.AllowNotes ?? false;
            _cbAllowPersonEntry.Checked = workflowActionForm.AllowPersonEntry;
            _pnlPersonEntry.Visible = workflowActionForm.AllowPersonEntry;

            _cePersonEntryPreHtml.Text = workflowActionForm.PersonEntryPreHtml;
            _cePersonEntryPostHtml.Text = workflowActionForm.PersonEntryPostHtml;
            _cbPersonEntryShowCampus.Checked = workflowActionForm.PersonEntryCampusIsVisible;
            _cbPersonEntryAutofillCurrentPerson.Checked = workflowActionForm.PersonEntryAutofillCurrentPerson;
            _cbPersonEntryHideIfCurrentPersonKnown.Checked = workflowActionForm.PersonEntryHideIfCurrentPersonKnown;
            _ddlPersonEntrySpouseEntryOption.SetValue( ( int ) workflowActionForm.PersonEntrySpouseEntryOption );
            _ddlPersonEntryGenderEntryOption.SetValue( ( int ) workflowActionForm.PersonEntryGenderEntryOption );
            _ddlPersonEntryEmailEntryOption.SetValue( ( int ) workflowActionForm.PersonEntryEmailEntryOption );
            _ddlPersonEntryMobilePhoneEntryOption.SetValue( ( int ) workflowActionForm.PersonEntryMobilePhoneEntryOption );
            _ddlPersonEntrySmsOptInEntryOption.SetValue( ( int ) workflowActionForm.PersonEntrySmsOptInEntryOption );
            _ddlPersonEntryBirthdateEntryOption.SetValue( ( int ) workflowActionForm.PersonEntryBirthdateEntryOption );
            _ddlPersonEntryAddressEntryOption.SetValue( ( int ) workflowActionForm.PersonEntryAddressEntryOption );
            _ddlPersonEntryMaritalStatusEntryOption.SetValue( ( int ) workflowActionForm.PersonEntryMaritalStatusEntryOption );
            _ddlPersonEntryRaceEntryOption.SetValue ( ( int ) workflowActionForm.PersonEntryRaceEntryOption );
            _ddlPersonEntryEthnicityEntryOption.SetValue ( ( int ) workflowActionForm.PersonEntryEthnicityEntryOption );

            _tbPersonEntrySpouseLabel.Text = workflowActionForm.PersonEntrySpouseLabel;
            _dvpPersonEntryConnectionStatus.SetValue( workflowActionForm.PersonEntryConnectionStatusValueId );
            _dvpPersonEntryRecordStatus.SetValue( workflowActionForm.PersonEntryRecordStatusValueId );
            _dvpPersonEntryGroupLocationType.SetValue( workflowActionForm.PersonEntryGroupLocationTypeValueId );

            _dvpPersonEntryCampusStatus.Visible = workflowActionForm.PersonEntryCampusIsVisible;
            _dvpPersonEntryCampusType.Visible = workflowActionForm.PersonEntryCampusIsVisible;

            _dvpPersonEntryCampusStatus.SetValue( workflowActionForm.PersonEntryCampusStatusValueId );
            _dvpPersonEntryCampusType.SetValue( workflowActionForm.PersonEntryCampusTypeValueId );

            _ddlPersonEntryPersonAttribute.Items.Clear();
            _ddlPersonEntryPersonAttribute.Items.Add( new ListItem() );
            _ddlPersonEntrySpouseAttribute.Items.Clear();
            _ddlPersonEntrySpouseAttribute.Items.Add( new ListItem() );
            _ddlPersonEntryFamilyAttribute.Items.Clear();
            _ddlPersonEntryFamilyAttribute.Items.Add( new ListItem() );

            foreach ( var attributeItem in workflowTypeAttributes )
            {
                var fieldType = FieldTypeCache.Get( attributeItem.Value.FieldTypeId );
                if ( fieldType?.Field is Rock.Field.Types.GroupFieldType )
                {
                    _ddlPersonEntryFamilyAttribute.Items.Add( new ListItem( attributeItem.Value.Name, attributeItem.Key.ToString() ) );
                }

                if ( fieldType?.Field is Rock.Field.Types.PersonFieldType )
                {
                    _ddlPersonEntryPersonAttribute.Items.Add( new ListItem( attributeItem.Value.Name, attributeItem.Key.ToString() ) );
                    _ddlPersonEntrySpouseAttribute.Items.Add( new ListItem( attributeItem.Value.Name, attributeItem.Key.ToString() ) );
                }
            }

            _ddlPersonEntryPersonAttribute.SetValue( workflowActionForm.PersonEntryPersonAttributeGuid );
            _ddlPersonEntrySpouseAttribute.SetValue( workflowActionForm.PersonEntrySpouseAttributeGuid );
            _ddlPersonEntryFamilyAttribute.SetValue( workflowActionForm.PersonEntryFamilyAttributeGuid );

            // Remove any existing rows (shouldn't be any)
            foreach ( var attributeRow in Controls.OfType<WorkflowFormAttributeRow>() )
            {
                Controls.Remove( attributeRow );
            }

            foreach ( var formAttribute in workflowActionForm.FormAttributes.OrderBy( a => a.Order ) )
            {
                var row = new WorkflowFormAttributeRow();
                row.AttributeGuid = formAttribute.Attribute.Guid;
                row.AttributeName = formAttribute.Attribute.Name;
                row.Attribute = formAttribute.Attribute;
                row.Guid = formAttribute.Guid;
                row.IsVisible = formAttribute.IsVisible;
                row.IsEditable = !formAttribute.IsReadOnly;
                row.IsRequired = formAttribute.IsRequired;
                row.HideLabel = formAttribute.HideLabel;
                row.PreHtml = formAttribute.PreHtml;
                row.PostHtml = formAttribute.PostHtml;
                row.VisibilityRules = formAttribute.FieldVisibilityRules;
                row.FilterClick += filterClick;
                Controls.Add( row );
            }

            _ddlActionAttribute.Items.Clear();
            _ddlActionAttribute.Items.Add( new ListItem() );
            foreach ( var attributeItem in workflowTypeAttributes )
            {
                var fieldType = FieldTypeCache.Get( attributeItem.Value.FieldTypeId );
                if ( fieldType != null && fieldType.Field is Rock.Field.Types.TextFieldType )
                {
                    var li = new ListItem( attributeItem.Value.Name, attributeItem.Key.ToString() );
                    li.Selected = workflowActionForm.ActionAttributeGuid.HasValue && workflowActionForm.ActionAttributeGuid.Value.ToString() == li.Value;
                    _ddlActionAttribute.Items.Add( li );
                }
            }
        }

        /// <summary>
        /// Method that will be called when the row's filter button is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="WorkflowFormAttributeRow.FilterEventArgs"/> instance containing the event data.</param>
        private void filterClick( object sender, WorkflowFormAttributeRow.FilterEventArgs e )
        {
            var fvre = _mdFieldVisibilityRules.FindControl( "fvreWorkflowFields" ) as FieldVisibilityRulesEditor;
            var wfar = e.WorkflowFormAttributeRow;
            if ( fvre != null && wfar != null )
            {
                EditingAttributeRowGuid = wfar.Guid.ToString();

                fvre.ValidationGroup = "FieldFilter";
                fvre.FieldName = wfar.AttributeName;
                fvre.ComparableFields = AttributeRows
                    .Where( ar => ar.IsEditable && ar.IsVisible && ar.AttributeGuid != wfar.AttributeGuid )
                    .ToDictionary( ar => ar.AttributeGuid, ar => new FieldVisibilityRuleField
                    {
                        Guid = ar.AttributeGuid,
                        Attribute = ar.Attribute
                    } );
                fvre.SetFieldVisibilityRules( wfar.VisibilityRules );
                _mdFieldVisibilityRules.Show();
            }
        }

        /// <summary>
        /// Copies the editable properties from one workflow form to another
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static void CopyEditableProperties( WorkflowActionForm source, WorkflowActionForm target )
        {
            target.NotificationSystemCommunicationId = source.NotificationSystemCommunicationId;
            target.IncludeActionsInNotification = source.IncludeActionsInNotification;
            target.AllowNotes = source.AllowNotes;
            target.AllowPersonEntry = source.AllowPersonEntry;
            target.Header = source.Header;
            target.Footer = source.Footer;
            target.Actions = source.Actions;
            target.ActionAttributeGuid = source.ActionAttributeGuid;

            target.AllowPersonEntry = source.AllowPersonEntry;
            target.PersonEntryPreHtml = source.PersonEntryPreHtml;
            target.PersonEntryPostHtml = source.PersonEntryPostHtml;
            target.PersonEntryCampusIsVisible = source.PersonEntryCampusIsVisible;
            target.PersonEntryAutofillCurrentPerson = source.PersonEntryAutofillCurrentPerson;
            target.PersonEntryHideIfCurrentPersonKnown = source.PersonEntryHideIfCurrentPersonKnown;
            target.PersonEntrySpouseEntryOption = source.PersonEntrySpouseEntryOption;
            target.PersonEntryGenderEntryOption = source.PersonEntryGenderEntryOption;
            target.PersonEntryEmailEntryOption = source.PersonEntryEmailEntryOption;
            target.PersonEntryMobilePhoneEntryOption = source.PersonEntryMobilePhoneEntryOption;
            target.PersonEntrySmsOptInEntryOption = source.PersonEntrySmsOptInEntryOption;
            target.PersonEntryBirthdateEntryOption = source.PersonEntryBirthdateEntryOption;
            target.PersonEntryAddressEntryOption = source.PersonEntryAddressEntryOption;
            target.PersonEntryMaritalStatusEntryOption = source.PersonEntryMaritalStatusEntryOption;
            target.PersonEntrySpouseLabel = source.PersonEntrySpouseLabel;
            target.PersonEntryConnectionStatusValueId = source.PersonEntryConnectionStatusValueId;
            target.PersonEntryRecordStatusValueId = source.PersonEntryRecordStatusValueId;
            target.PersonEntryGroupLocationTypeValueId = source.PersonEntryGroupLocationTypeValueId;
            target.PersonEntryRaceEntryOption = source.PersonEntryRaceEntryOption;
            target.PersonEntryEthnicityEntryOption = source.PersonEntryEthnicityEntryOption;

            target.PersonEntryCampusStatusValueId = source.PersonEntryCampusStatusValueId;
            target.PersonEntryCampusTypeValueId = source.PersonEntryCampusTypeValueId;

            target.PersonEntryPersonAttributeGuid = source.PersonEntryPersonAttributeGuid;
            target.PersonEntrySpouseAttributeGuid = source.PersonEntrySpouseAttributeGuid;
            target.PersonEntryFamilyAttributeGuid = source.PersonEntryFamilyAttributeGuid;
        }

        /// <summary>
        /// Gets or sets the workflow activities.
        /// </summary>
        /// <value>
        /// The workflow activities.
        /// </value>
        public Dictionary<string, string> WorkflowActivities
        {
            get
            {
                EnsureChildControls();
                return _falActions.Activities;
            }

            set
            {
                EnsureChildControls();
                _falActions.Activities = value;
            }
        }

        /// <summary>
        /// Gets the attribute rows.
        /// </summary>
        /// <value>
        /// The attribute rows.
        /// </value>
        public List<WorkflowFormAttributeRow> AttributeRows
        {
            get
            {
                var rows = new List<WorkflowFormAttributeRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is WorkflowFormAttributeRow )
                    {
                        var workflowFormAttributeRow = control as WorkflowFormAttributeRow;
                        if ( workflowFormAttributeRow != null )
                        {
                            rows.Add( workflowFormAttributeRow );
                        }
                    }
                }

                return rows.OrderBy( r => r.Order ).ToList();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfFormGuid = new HiddenField
            {
                ID = "_hfFormGuid"
            };

            Controls.Add( _hfFormGuid );

            _ddlNotificationSystemEmail = new RockDropDownList
            {
                EnableViewState = false,
                DataValueField = "Id",
                DataTextField = "Title",
                Label = "Notification Email",
                Help = "An optional system email that should be sent to the person or people assigned to this activity (Any System Email with a category of 'Workflow').",
                ID = "_ddlNotificationSystemEmail"
            };

            Controls.Add( _ddlNotificationSystemEmail );

            var systemEmailCategory = CategoryCache.Get( Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_WORKFLOW.AsGuid() );
            if ( systemEmailCategory != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    _ddlNotificationSystemEmail.DataSource = new SystemCommunicationService( rockContext ).Queryable()
                        .Where( e => e.CategoryId == systemEmailCategory.Id )
                        .OrderBy( e => e.Title )
                        .Select( a => new { a.Id, a.Title } )
                        .ToList();
                    _ddlNotificationSystemEmail.DataBind();
                }
            }

            _ddlNotificationSystemEmail.Items.Insert( 0, new ListItem( "None", "0" ) );

            _cbIncludeActions = new RockCheckBox
            {
                Label = "Include Actions in Email",
                Help = "Should the email include the option for recipient to select an action directly from within the email? Note: This only applies if none of the form fields are required. The workflow will be persisted immediately prior to sending the email.",
                ID = "_cbIncludeActions"
            };

            Controls.Add( _cbIncludeActions );

            _ceHeaderText = new CodeEditor
            {
                Label = "Form Header",
                Help = "HTML to display above the form fields. <span class='tip tip-lava'></span> <span class='tip tip-html'>",
                ID = "_ebHeaderText",
                EditorMode = CodeEditorMode.Lava,
                EditorTheme = CodeEditorTheme.Rock,
                EditorHeight = "120"
            };

            Controls.Add( _ceHeaderText );

            _ceFooterText = new CodeEditor
            {
                Label = "Form Footer",
                Help = "HTML to display below the form fields. <span class='tip tip-lava'></span> <span class='tip tip-html'>",
                ID = "_ebFooterText",
                EditorMode = CodeEditorMode.Lava,
                EditorTheme = CodeEditorTheme.Rock,
                EditorHeight = "120"
            };

            Controls.Add( _ceFooterText );

            _falActions = new WorkflowFormActionList
            {
                ID = "_falActions"
            };

            Controls.Add( _falActions );

            _ddlActionAttribute = new RockDropDownList
            {
                EnableViewState = false,
                ID = "_ddlActionAttribute",
                Label = "Command Selected Attribute",
                Help = "Optional text attribute that should be updated with the selected command label."
            };

            Controls.Add( _ddlActionAttribute );

            _cbAllowNotesEntry = new RockCheckBox
            {
                Label = "Enable Note Entry",
                Help = "Should this form include an area for viewing and editing notes related to the workflow?",
                ID = "_cbAllowNotesEntry"
            };

            Controls.Add( _cbAllowNotesEntry );

            /* Person Entry related */

            _cbAllowPersonEntry = new RockCheckBox
            {
                Label = "Enable Person Entry",
                Help = "If enabled, the form will prompt to add a new person.",
                ID = "_cbAllowPersonEntry"
            };

            _cbAllowPersonEntry.AutoPostBack = true;
            _cbAllowPersonEntry.CheckedChanged += _cbAllowPersonEntry_CheckedChanged;

            Controls.Add( _cbAllowPersonEntry );

            _pnlPersonEntry = new Panel
            {
                ID = "_pnlPersonEntry",
                Visible = false
            };

            Controls.Add( _pnlPersonEntry );

            _pnlPersonEntry.Controls.Add( new Literal { Text = "<hr>" } );
            _pnlPersonEntry.Controls.Add( new Literal { Text = "<legend>Person Entry Configuration</legend>" } );

            _cePersonEntryPreHtml = new CodeEditor
            {
                ID = "_cePersonEntryPreHtml",
                Label = "Person Form Pre-HTML",
                Help = "HTML to display above the person entry form. <span class='tip tip-lava'></span> <span class='tip tip-html'>",
                EditorMode = CodeEditorMode.Lava,
                EditorTheme = CodeEditorTheme.Rock,
                EditorHeight = "120"
            };

            _pnlPersonEntry.Controls.Add( _cePersonEntryPreHtml );

            _cbPersonEntryShowCampus = new RockCheckBox
            {
                ID = "_cbPersonEntryShowCampus",
                Label = "Show Campus"
            };


            _cbPersonEntryShowCampus.AutoPostBack = true;
            _cbPersonEntryShowCampus.CheckedChanged += _cbPersonEntryShowCampus_CheckedChanged;

            _cbPersonEntryAutofillCurrentPerson = new RockCheckBox
            {
                ID = "_cbPersonEntryAutofillCurrentPerson",
                Label = "Autofill Current Person"
            };

            _cbPersonEntryHideIfCurrentPersonKnown = new RockCheckBox
            {
                ID = "_cbPersonEntryHideIfCurrentPersonKnown",
                Label = "Hide if Current Person Known"
            };

            _ddlPersonEntrySpouseEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntrySpouseEntryOption",
                Label = "Spouse Entry"
            };

            _ddlPersonEntrySpouseEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _ddlPersonEntryGenderEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryGenderEntryOption",
                Label = "Gender"
            };

            _ddlPersonEntryGenderEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _ddlPersonEntryEmailEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryEmailEntryOption",
                Label = "Email"
            };

            _ddlPersonEntryEmailEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _ddlPersonEntryMobilePhoneEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryMobilePhoneEntryOption",
                Label = "Mobile Phone"
            };

            _ddlPersonEntryMobilePhoneEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _ddlPersonEntrySmsOptInEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntrySmsOptInEntryOption",
                Label = "SMS Opt-In"
            };

            _ddlPersonEntrySmsOptInEntryOption.BindToEnum<WorkflowActionFormShowHideOption> ();

            _ddlPersonEntryBirthdateEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryBirthdateEntryOption",
                Label = "Birthdate"
            };

            _ddlPersonEntryBirthdateEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _ddlPersonEntryAddressEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryAddressEntryOption",
                Label = "Address"
            };

            _ddlPersonEntryAddressEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _ddlPersonEntryMaritalStatusEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryMaritalStatusEntryOption",
                Label = "Marital Status"
            };

            _ddlPersonEntryMaritalStatusEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _tbPersonEntrySpouseLabel = new RockTextBox
            {
                ID = "_tbPersonEntrySpouseLabel",
                Label = "Spouse Label"
            };

            _dvpPersonEntryConnectionStatus = new DefinedValuePicker
            {
                ID = "_dvpPersonEntryConnectionStatus",
                Label = "Connection Status",
                Required = true,
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() )
            };

            _dvpPersonEntryRecordStatus = new DefinedValuePicker
            {
                ID = "_dvpPersonEntryRecordStatus",
                Label = "Record Status",
                Required = true,
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() )
            };

            _dvpPersonEntryGroupLocationType = new DefinedValuePicker
            {
                ID = "_dvpPersonEntryGroupLocationType",
                Label = "Address Type",
                Required = true,
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() )
            };

            _dvpPersonEntryCampusStatus = new DefinedValuePicker
            {
                ID = "_dvpPersonEntryCampusStatus",
                Label = "Campus Status",
                Required = false,
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid() )
            };

            _dvpPersonEntryCampusType = new DefinedValuePicker
            {
                ID = "_dvpPersonEntryCampusType",
                Label = "Campus Type",
                Required = false,
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid() )
            };

            _ddlPersonEntryPersonAttribute = new RockDropDownList
            {
                ID = "_ddlPersonEntryPersonAttribute",
                Label = "Person Attribute",
                Required = true,
                Help = "Workflow attribute for entered person"
            };

            _ddlPersonEntrySpouseAttribute = new RockDropDownList
            {
                ID = "_ddlPersonEntrySpouseAttribute",
                Label = "Spouse Attribute",
                Help = "Attribute where the matched/created person's spouse will be stored for the workflow to use."
            };

            _ddlPersonEntryFamilyAttribute = new RockDropDownList
            {
                ID = "_ddlPersonEntryFamilyAttribute",
                Label = "Family Attribute",
                Help = "A group attribute where the matched/created family group will be stored for the workflow to use"
            };

            _cePersonEntryPostHtml = new CodeEditor
            {
                ID = "_cePersonEntryPostHtml",
                Label = "Person Form Post-HTML",
                Help = "HTML to display below the person entry form. <span class='tip tip-lava'></span> <span class='tip tip-html'>",
                EditorMode = CodeEditorMode.Lava,
                EditorTheme = CodeEditorTheme.Rock,
                EditorHeight = "120"
            };

            _ddlPersonEntryRaceEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryRaceEntryOption",
                Label = "Race",
                Required = false
            };

            _ddlPersonEntryRaceEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            _ddlPersonEntryEthnicityEntryOption = new RockDropDownList
            {
                ID = "_ddlPersonEntryEthnicityEntryOption",
                Label = "Ethnicity",
                Required = false
            };

            _ddlPersonEntryEthnicityEntryOption.BindToEnum<WorkflowActionFormPersonEntryOption>();

            /* Person Entry - Row 1*/
            Panel pnlPersonEntryRow1 = new Panel
            {
                ID = "pnlPersonEntryRow1",
                CssClass = "row"
            };

            Panel pnlPersonEntryRow1Col1 = new Panel
            {
                ID = "pnlPersonEntryRow1Col1",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow1Col2 = new Panel
            {
                ID = "pnlPersonEntryRow1Col2",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow1Col3 = new Panel
            {
                ID = "pnlPersonEntryRow1Col3",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow1Col4 = new Panel
            {
                ID = "pnlPersonEntryRow1Col4",
                CssClass = "col-xs-3"
            };

            _pnlPersonEntry.Controls.Add( pnlPersonEntryRow1 );
            pnlPersonEntryRow1.Controls.Add( pnlPersonEntryRow1Col1 );
            pnlPersonEntryRow1.Controls.Add( pnlPersonEntryRow1Col2 );
            pnlPersonEntryRow1.Controls.Add( pnlPersonEntryRow1Col3 );
            pnlPersonEntryRow1.Controls.Add( pnlPersonEntryRow1Col4 );
            pnlPersonEntryRow1Col1.Controls.Add( _cbPersonEntryAutofillCurrentPerson );
            pnlPersonEntryRow1Col2.Controls.Add( _cbPersonEntryHideIfCurrentPersonKnown );
            pnlPersonEntryRow1Col3.Controls.Add( _dvpPersonEntryRecordStatus );
            pnlPersonEntryRow1Col4.Controls.Add( _dvpPersonEntryConnectionStatus );

            /* Person Entry - Row 2*/
            Panel pnlPersonEntryRow2 = new Panel
            {
                ID = "pnlPersonEntryRow2",
                CssClass = "row"
            };

            Panel pnlPersonEntryRow2Col1 = new Panel
            {
                ID = "pnlPersonEntryRow2Col1",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow2Col2 = new Panel
            {
                ID = "pnlPersonEntryRow2Col2",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow2Col3 = new Panel
            {
                ID = "pnlPersonEntryRow2Col3",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow2Col4 = new Panel
            {
                ID = "pnlPersonEntryRow2Col4",
                CssClass = "col-xs-3"
            };

            _pnlPersonEntry.Controls.Add( pnlPersonEntryRow2 );
            pnlPersonEntryRow2.Controls.Add( pnlPersonEntryRow2Col1 );
            pnlPersonEntryRow2.Controls.Add( pnlPersonEntryRow2Col2 );
            pnlPersonEntryRow2.Controls.Add( pnlPersonEntryRow2Col3 );
            pnlPersonEntryRow2.Controls.Add( pnlPersonEntryRow2Col4 );

            pnlPersonEntryRow2Col1.Controls.Add( _cbPersonEntryShowCampus );
            pnlPersonEntryRow2Col2.Controls.Add( _dvpPersonEntryCampusType );
            pnlPersonEntryRow2Col3.Controls.Add( _dvpPersonEntryCampusStatus );

            /* Person Entry - Row 3*/
            Panel pnlPersonEntryRow3 = new Panel
            {
                ID = "pnlPersonEntryRow3",
                CssClass = "row"
            };

            Panel pnlPersonEntryRow3Col1 = new Panel
            {
                ID = "pnlPersonEntryRow3Col1",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow3Col2 = new Panel
            {
                ID = "pnlPersonEntryRow3Col2",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow3Col3 = new Panel
            {
                ID = "pnlPersonEntryRow3Col3",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow3Col4 = new Panel
            {
                ID = "pnlPersonEntryRow3Col4",
                CssClass = "col-xs-3"
            };

            _pnlPersonEntry.Controls.Add( pnlPersonEntryRow3 );

            pnlPersonEntryRow3.Controls.Add( pnlPersonEntryRow3Col1 );
            pnlPersonEntryRow3.Controls.Add( pnlPersonEntryRow3Col2 );
            pnlPersonEntryRow3.Controls.Add( pnlPersonEntryRow3Col3 );
            pnlPersonEntryRow3.Controls.Add( pnlPersonEntryRow3Col4 );

            pnlPersonEntryRow3Col1.Controls.Add( _ddlPersonEntryGenderEntryOption );
            pnlPersonEntryRow3Col2.Controls.Add( _ddlPersonEntryEmailEntryOption );
            pnlPersonEntryRow3Col3.Controls.Add( _ddlPersonEntryMobilePhoneEntryOption );
            pnlPersonEntryRow3Col4.Controls.Add( _ddlPersonEntrySmsOptInEntryOption );
            

            /* Person Entry - Row 4*/
            Panel pnlPersonEntryRow4 = new Panel
            {
                ID = "pnlPersonEntryRow4",
                CssClass = "row"
            };

            Panel pnlPersonEntryRow4Col1 = new Panel
            {
                ID = "pnlPersonEntryRow4Col1",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow4Col2 = new Panel
            {
                ID = "pnlPersonEntryRow4Col2",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow4Col3 = new Panel
            {
                ID = "pnlPersonEntryRow4Col3",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow4Col4 = new Panel
            {
                ID = "pnlPersonEntryRow4Col4",
                CssClass = "col-xs-3"
            };

            _pnlPersonEntry.Controls.Add( pnlPersonEntryRow4 );
            pnlPersonEntryRow4.Controls.Add( pnlPersonEntryRow4Col1 );
            pnlPersonEntryRow4.Controls.Add( pnlPersonEntryRow4Col2 );
            pnlPersonEntryRow4.Controls.Add( pnlPersonEntryRow4Col3 );
            pnlPersonEntryRow4.Controls.Add( pnlPersonEntryRow4Col4 );

            pnlPersonEntryRow4Col1.Controls.Add( _ddlPersonEntryAddressEntryOption );
            pnlPersonEntryRow4Col2.Controls.Add( _dvpPersonEntryGroupLocationType );
            pnlPersonEntryRow4Col3.Controls.Add( _ddlPersonEntryMaritalStatusEntryOption );
            pnlPersonEntryRow4Col4.Controls.Add( _ddlPersonEntryBirthdateEntryOption );

            /* Person Entry - Row 5*/
            Panel pnlPersonEntryRow5 = new Panel
            {
                ID = "pnlPersonEntryRow5",
                CssClass = "row"
            };

            Panel pnlPersonEntryRow5Col1 = new Panel
            {
                ID = "pnlPersonEntryRow5Col1",
                CssClass = "col-xs-6"
            };

            Panel pnlPersonEntryRow5Col2 = new Panel
            {
                ID = "pnlPersonEntryRow5Col2",
                CssClass = "col-xs-6"
            };

            Panel pnlPersonEntryRow5Col3 = new Panel
            {
                ID = "pnlPersonEntryRow5Col3",
                CssClass = "col-xs-6"
            };

            _pnlPersonEntry.Controls.Add( pnlPersonEntryRow5 );
            pnlPersonEntryRow5.Controls.Add( pnlPersonEntryRow5Col1 );
            pnlPersonEntryRow5.Controls.Add( pnlPersonEntryRow5Col2 );
            pnlPersonEntryRow5.Controls.Add( pnlPersonEntryRow5Col3 );
            pnlPersonEntryRow5Col1.Controls.Add( _ddlPersonEntrySpouseEntryOption );
            pnlPersonEntryRow5Col2.Controls.Add( _tbPersonEntrySpouseLabel );

            /* Person Entry - Row 6*/
            Panel pnlPersonEntryRow6 = new Panel
            {
                ID = "pnlPersonEntryRow6",
                CssClass = "row"
            };

            Panel pnlPersonEntryRow6Col1 = new Panel
            {
                ID = "pnlPersonEntryRow6Col1",
                CssClass = "col-xs-6"
            };

            Panel pnlPersonEntryRow6Col2 = new Panel
            {
                ID = "pnlPersonEntryRow6Col2",
                CssClass = "col-xs-6"
            };

            Panel pnlPersonEntryRow6Col3 = new Panel
            {
                ID = "pnlPersonEntryRow6Col3",
                CssClass = "col-xs-6"
            };

            Panel pnlPersonEntryRow6Col4 = new Panel
            {
                ID = "pnlPersonEntryRow6Col4",
                CssClass = "col-xs-3"
            };

            Panel pnlPersonEntryRow6Col5 = new Panel
            {
                ID = "pnlPersonEntryRow6Col5",
                CssClass = "col-xs-3"
            };

            _pnlPersonEntry.Controls.Add( pnlPersonEntryRow6 );
            pnlPersonEntryRow6.Controls.Add( pnlPersonEntryRow6Col1 );
            pnlPersonEntryRow6.Controls.Add( pnlPersonEntryRow6Col2 );
            pnlPersonEntryRow6.Controls.Add( pnlPersonEntryRow6Col3 );
            pnlPersonEntryRow6.Controls.Add( pnlPersonEntryRow6Col4 );
            pnlPersonEntryRow6.Controls.Add( pnlPersonEntryRow6Col5 );
            pnlPersonEntryRow6Col1.Controls.Add( _ddlPersonEntryPersonAttribute );
            pnlPersonEntryRow6Col2.Controls.Add( _ddlPersonEntrySpouseAttribute );
            pnlPersonEntryRow6Col3.Controls.Add( _ddlPersonEntryFamilyAttribute );
            pnlPersonEntryRow6Col4.Controls.Add( _ddlPersonEntryRaceEntryOption );
            pnlPersonEntryRow6Col5.Controls.Add( _ddlPersonEntryEthnicityEntryOption );

            /* Person Entry - Post-HTML*/
            _pnlPersonEntry.Controls.Add( _cePersonEntryPostHtml );

            _pnlPersonEntry.Controls.Add( new Literal { Text = "<hr>" } );

            /* Workflow Attributes */
            _pnlWorkflowAttributes = new Panel
            {
                ID = "_pnlWorkflowAttributes",
                CssClass = "form-group"
            };

            this.Controls.Add( _pnlWorkflowAttributes );

            /* Workflow Attributes Filter Modal */
            _mdFieldVisibilityRules = new ModalDialog
            {
                ID = nameof( _mdFieldVisibilityRules ),
                Title = "Conditional Display Logic"
            };

            _mdFieldVisibilityRules.SaveClick += _mdFieldVisibilityRules_SaveClick;
            var vsFieldVisibilityRules = new ValidationSummary
            {
                ID = "vsFieldVisibilityRules",
                HeaderText = "Please correct the following:",
                CssClass = "alert alert-validation",
                ValidationGroup = "FieldFilter"
            };

            _mdFieldVisibilityRules.Content.Controls.Add( vsFieldVisibilityRules );

            var fvreWorkflowFields = new FieldVisibilityRulesEditor
            {
                ID = "fvreWorkflowFields"
            };

            _mdFieldVisibilityRules.Content.Controls.Add( fvreWorkflowFields );
            Controls.Add( _mdFieldVisibilityRules );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _cbPersonEntryShowCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _cbPersonEntryShowCampus_CheckedChanged( object sender, EventArgs e )
        {
            _dvpPersonEntryCampusStatus.Visible = _cbPersonEntryShowCampus.Checked;
            _dvpPersonEntryCampusType.Visible = _cbPersonEntryShowCampus.Checked;
        }

        /// <summary>
        /// Handles the SaveClick event of the _mdFieldVisibilityRules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _mdFieldVisibilityRules_SaveClick( object sender, EventArgs e )
        {
            var attributeRowGuid = EditingAttributeRowGuid.AsGuidOrNull();
            if ( attributeRowGuid == null )
            {
                return;
            }

            var fvre = _mdFieldVisibilityRules.FindControl( "fvreWorkflowFields" ) as FieldVisibilityRulesEditor;
            if ( fvre == null )
            {
                return;
            }

            var attributeRow = AttributeRows.Where( ar => ar.Guid == attributeRowGuid.Value ).FirstOrDefault();
            if ( attributeRow == null )
            {
                return;
            }

            attributeRow.VisibilityRules = fvre.GetFieldVisibilityRules();
            _mdFieldVisibilityRules.Hide();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _cbAllowPersonEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _cbAllowPersonEntry_CheckedChanged( object sender, EventArgs e )
        {
            _pnlPersonEntry.Visible = _cbAllowPersonEntry.Checked;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( _hfFormGuid.Value.AsGuid() == Guid.Empty )
            {
                return;
            }

            _hfFormGuid.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _ddlNotificationSystemEmail.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbIncludeActions.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbAllowNotesEntry.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbAllowPersonEntry.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            _ceHeaderText.ValidationGroup = ValidationGroup;
            _ceHeaderText.RenderControl( writer );

            _pnlPersonEntry.RenderControl( writer );

            // Attributes
            if ( AttributeRows.Any() )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( "Form Fields" );

                writer.AddAttribute( "class", "help" );
                writer.AddAttribute( "href", "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( "class", "fa fa-question-circle" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute( "class", "alert alert-info" );
                writer.AddAttribute( "style", "display:none" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderBeginTag( HtmlTextWriterTag.Small );
                writer.Write( "The fields (attributes) to display on the entry form" );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag();      // Label

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-table table table-condensed table-light" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-columncommand" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "&nbsp;" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "px-0" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row no-gutters d-flex align-items-end text-xs" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Field" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-9" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "d-flex align-items-end" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq text-truncate text-wrap text-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Visible" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq text-truncate text-wrap text-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Editable" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq text-truncate text-wrap text-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Required" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq text-truncate text-wrap text-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Hide Label" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq text-truncate text-wrap text-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Pre-HTML" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq text-truncate text-wrap text-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Post-HTML" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq text-truncate text-wrap text-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "Logic" );
                writer.RenderEndTag();

                writer.RenderEndTag();      // row

                writer.RenderEndTag();      // col-xs-9

                writer.RenderEndTag();      // row

                writer.RenderEndTag();      // th

                writer.RenderEndTag();      // tr
                writer.RenderEndTag();      // THead

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "workflow-formfield-list" );
                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( var row in AttributeRows )
                {
                    row.RenderControl( writer );
                }

                writer.RenderEndTag();      // TBody

                writer.RenderEndTag();      // table

                writer.RenderEndTag();      // Div.form-group
            }

            _ceFooterText.ValidationGroup = ValidationGroup;
            _ceFooterText.RenderControl( writer );
            _falActions.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _ddlActionAttribute.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            _mdFieldVisibilityRules.RenderControl( writer );
        }

        /// <summary>
        /// Clears the rows.
        /// </summary>
        public void ClearRows()
        {
            for ( int i = Controls.Count - 1; i >= 0; i-- )
            {
                if ( Controls[i] is WorkflowFormAttributeRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }
    }
}