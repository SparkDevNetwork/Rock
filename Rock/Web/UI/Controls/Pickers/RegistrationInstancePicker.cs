﻿// <copyright>
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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a Registration template and then a instance of that registration instance
    /// </summary>
    public class RegistrationInstancePicker : CompositeControl, IRockControl
    {

        #region IRockControl implementation (Custom implementation)

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _ddlRegistrationInstance.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlRegistrationInstance.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private RegistrationTemplatePicker _registrationTemplatePicker;
        private RockDropDownList _ddlRegistrationInstance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the registration template id.
        /// </summary>
        /// <value>
        /// The registration template id.
        /// </value>
        public int? RegistrationTemplateId
        {
            get
            {
                return ViewState["RegistrationTemplateId"] as int?;
            }

            set
            {
                ViewState["RegistrationTemplateId"] = value;
                if ( value.HasValue )
                {
                    LoadRegistrationInstances( value.Value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        public int? RegistrationInstanceId
        {
            get
            {
                EnsureChildControls();
                return _ddlRegistrationInstance.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                int registrationInstanceId = value ?? 0;
                if ( _ddlRegistrationInstance.SelectedValue != registrationInstanceId.ToString() )
                {
                    if ( !RegistrationTemplateId.HasValue )
                    {
                        var registrationInstance = new Rock.Model.RegistrationInstanceService( new RockContext() ).Get( registrationInstanceId );
                        if ( registrationInstance != null &&
                            _registrationTemplatePicker.SelectedValue != registrationInstance.RegistrationTemplateId.ToString() )
                        {
                            _registrationTemplatePicker.SetValue( registrationInstance.RegistrationTemplateId );

                            LoadRegistrationInstances( registrationInstance.RegistrationTemplateId );
                        }
                    }

                    _ddlRegistrationInstance.SelectedValue = registrationInstanceId.ToString();
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInstancePicker"/> class.
        /// </summary>
        public RegistrationInstancePicker()
            : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _registrationTemplatePicker = new RegistrationTemplatePicker();
            _registrationTemplatePicker.ID = this.ID + "_registrationTemplatePicker";
            _registrationTemplatePicker.SelectItem += _ddlRegistrationTemplate_SelectedIndexChanged;
            _registrationTemplatePicker.Label = "Registration Template";
            Controls.Add( _registrationTemplatePicker );

            _ddlRegistrationInstance = new RockDropDownList();
            _ddlRegistrationInstance.ID = this.ID + "_ddlRegistrationInstance";
            _ddlRegistrationInstance.Label = "Registration Instance";
            Controls.Add( _ddlRegistrationInstance );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            EnsureChildControls();
            var selectedRegistrationTemplateId = _registrationTemplatePicker.SelectedValue.AsIntegerOrNull();
            if ( selectedRegistrationTemplateId.HasValue && selectedRegistrationTemplateId.Value > 0 )
            {
                // Load the registration instances that correspond to the selected template.
                // This populates the dropdown list (_ddlRegistrationInstance).
                LoadRegistrationInstances( selectedRegistrationTemplateId.Value );

                // If the page is posting back (i.e., user submitted the form or caused an event),
                // we want to restore the selected registration instance value manually.
                if ( Page.IsPostBack )
                {
                    // Get the raw posted value from the form using the dropdown's UniqueID.
                    var postedValue = Page.Request[_ddlRegistrationInstance.UniqueID];
                    if ( postedValue.IsNotNullOrWhiteSpace() && _ddlRegistrationInstance.Items.FindByValue( postedValue ) != null )
                    {
                        // Manually restore the selected value
                        _ddlRegistrationInstance.SelectedValue = postedValue;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedItem event of the _registrationTemplatePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _ddlRegistrationTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            int registrationTemplateId = _registrationTemplatePicker.SelectedValue.AsInteger();
            LoadRegistrationInstances( registrationTemplateId );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            // Don't remove Id as this is required if this control is defined as attribute Field in Bulk Update
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "form-control-group " + this.FormGroupCssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( !RegistrationTemplateId.HasValue )
            {
                _registrationTemplatePicker.RenderControl( writer );
            }
            _ddlRegistrationInstance.RenderControl( writer );

            writer.RenderEndTag();
        }

        /// <summary>
        /// Loads the registrationInstances.
        /// </summary>
        /// <param name="registationTemplateId">The registration template identifier.</param>
        private void LoadRegistrationInstances( int? registationTemplateId )
        {
            int? currentRegistrationInstanceId = this.RegistrationInstanceId;
            _ddlRegistrationInstance.SelectedValue = null;
            _ddlRegistrationInstance.Items.Clear();
            if ( registationTemplateId.HasValue )
            {
                _ddlRegistrationInstance.Items.Add( Rock.Constants.None.ListItem );

                var registrationInstanceService = new Rock.Model.RegistrationInstanceService( new RockContext() );
                var registrationInstances = registrationInstanceService.Queryable().Where( r => r.RegistrationTemplateId == registationTemplateId.Value && r.IsActive ).OrderBy( a => a.Name ).ToList();

                foreach ( var r in registrationInstances )
                {
                    var item = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    item.Selected = r.Id == currentRegistrationInstanceId;
                    _ddlRegistrationInstance.Items.Add( item );
                }
            }
        }
    }
}
