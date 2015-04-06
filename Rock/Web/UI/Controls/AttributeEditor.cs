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
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Custom attribute editor control
    /// </summary>
    public class AttributeEditor : CompositeControl, IHasValidationGroup
    {
        #region Private Variables

        private bool _controlsLoaded = false;

        #endregion

        #region Controls

        private HtmlInputHidden _hfExistingKeyNames;
        private Literal _lAttributeActionTitle;
        private ValidationSummary _validationSummary;

        private RockTextBox _tbName;
        private RockTextBox _tbDescription;

        private CategoryPicker _cpCategories;
        private RockTextBox _tbKey;
        private CustomValidator _cvKey;
        private RockTextBox _tbIconCssClass;
        private RockCheckBox _cbRequired;
        private RockCheckBox _cbShowInGrid;

        private RockDropDownList _ddlFieldType;
        private PlaceHolder _phQualifiers;
        private PlaceHolder _phDefaultValue;

        private LinkButton _btnSave;
        private LinkButton _btnCancel;

        #endregion Controls

        #region Properties

        /// <summary>
        /// Gets or sets the attribute id.
        /// </summary>
        /// <value>
        /// The attribute id.
        /// </value>
        public int? AttributeId
        {
            get
            {
                return ViewState["AttributeId"] as int?;
            }
            set
            {
                ViewState["AttributeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute GUID.
        /// </summary>
        /// <value>
        /// The attribute GUID.
        /// </value>
        public Guid AttributeGuid
        {
            get
            {
                string guid = ViewState["AttributeGuid"] as string;
                if ( guid == null )
                {
                    return Guid.NewGuid();
                }
                else
                {
                    return new Guid( guid );
                }
            }
            set { ViewState["AttributeGuid"] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the attribute entity type id.
        /// </summary>
        /// <value>
        /// The attribute entity type id.
        /// </value>
        public int? AttributeEntityTypeId
        {
            get
            {
                return ViewState["AttributeEntityTypeId"] as int?;
            }
            set
            {
                ViewState["AttributeEntityTypeId"] = value;
                EnsureChildControls();
                _cpCategories.EntityTypeQualifierValue = value.HasValue && value.Value != 0 ? value.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show actions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show actions]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowActions
        {
            get { return ViewState["ShowActions"] as bool? ?? true; }
            set { ViewState["ShowActions"] = value; }
        }

        /// <summary>
        /// Gets or sets the action title.
        /// </summary>
        /// <value>
        /// The action title.
        /// </value>
        public string ActionTitle
        {
            get 
            {
                EnsureChildControls();
                return _lAttributeActionTitle.Text;
            }
            set 
            {
                EnsureChildControls();
                _lAttributeActionTitle.Text = value;
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
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                EnsureChildControls();
                return _tbDescription.Text;
            }
            set
            {
                EnsureChildControls();
                _tbDescription.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key
        {
            get
            {
                EnsureChildControls();
                return _tbKey.Text;
            }
            set
            {
                EnsureChildControls();
                _tbKey.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass
        {
            get
            {
                EnsureChildControls();
                return _tbIconCssClass.Text;
            }
            set
            {
                EnsureChildControls();
                _tbIconCssClass.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the category ids.
        /// </summary>
        /// <value>
        /// The category ids.
        /// </value>
        public IEnumerable<int> CategoryIds
        {
            get
            {
                EnsureChildControls();
                return _cpCategories.SelectedValuesAsInt();
            }
            set
            {
                EnsureChildControls();
                _cpCategories.SetValues( value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AttributeEditor"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _cbRequired.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbRequired.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Show in Grid option is displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if Show in Grid option is visible; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInGridVisible
        {
            get
            {
                EnsureChildControls();
                return _cbShowInGrid.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbShowInGrid.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in grid]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInGrid
        {
            get
            {
                EnsureChildControls();
                return _cbShowInGrid.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbShowInGrid.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the field type id.
        /// </summary>
        /// <value>
        /// The field type id.
        /// </value>
        public int? FieldTypeId
        {
            get
            {
                EnsureChildControls();
                return _ddlFieldType.SelectedValueAsInt();
            }
            set
            {
                EnsureChildControls();
                if ( value != _ddlFieldType.SelectedValueAsInt() )
                {
                    _ddlFieldType.SetValue( value );
                    CreateFieldTypeDetailControls( value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the qualifiers.
        /// </summary>
        /// <value>
        /// The qualifiers.
        /// </value>
        public Dictionary<string, ConfigurationValue> Qualifiers
        {
            get
            {
                return ViewState["Qualifiers"] as Dictionary<string, ConfigurationValue> ??
                    new Dictionary<string, ConfigurationValue>();
            }
            set
            {
                ViewState["Qualifiers"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public string DefaultValue
        {
            get
            {
                return ViewState["DefaultValue"] as string;
            }
            set
            {
                ViewState["DefaultValue"] = value;
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
                return ViewState["ValidationGroup"] as string; 
            }
            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the reserved key names.
        /// </summary>
        /// <value>
        /// The reserved key names.
        /// </value>
        public List<string> ReservedKeyNames
        {
            get
            { 
                return ViewState["ReservedKeyNames"] as List<string> ?? new List<string>(); 
            }
            set 
            { 
                ViewState["ReservedKeyNames"] = value; 
            }
        }

        /// <summary>
        /// Gets or sets the object property names.
        /// </summary>
        /// <value>
        /// The object property names.
        /// </value>
        private List<string> ObjectPropertyNames
        {
            get
            {
                return ViewState["ObjectPropertyNames"] as List<string> ?? new List<string>();
            }
            set
            {
                ViewState["ObjectPropertyNames"] = value;
            }
        }

        #endregion

        #region Overridden Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CreateFieldTypeDetailControls( ViewState["FieldTypeId"] as int? );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            if ( !_controlsLoaded )
            {
                Controls.Clear();

                _hfExistingKeyNames = new HtmlInputHidden();
                _hfExistingKeyNames.AddCssClass( "js-existing-key-names" );
                _hfExistingKeyNames.ID = this.ID + "_hfExistingKeyNames";
                Controls.Add( _hfExistingKeyNames );

                _lAttributeActionTitle = new Literal();
                _lAttributeActionTitle.ID = "lAttributeActionTitle";
                Controls.Add( _lAttributeActionTitle );

                _validationSummary = new ValidationSummary();
                _validationSummary.ID = "valiationSummary";
                _validationSummary.CssClass = "alert alert-danger";
                _validationSummary.HeaderText = "Please Correct the Following";
                Controls.Add( _validationSummary ); 
                
                _tbName = new RockTextBox();
                _tbName.ID = "tbName";
                _tbName.Label = "Name";
                _tbName.Required = true;
                Controls.Add( _tbName );

                _tbDescription = new RockTextBox();
                _tbDescription.Label = "Description";
                _tbDescription.ID = "tbDescription";
                _tbDescription.TextMode = TextBoxMode.MultiLine;
                _tbDescription.Rows = 3;
                Controls.Add( _tbDescription );

                _cpCategories = new CategoryPicker();
                _cpCategories.ID = "cpCategories_" + this.ID.ToString();
                _cpCategories.Label = "Categories";
                _cpCategories.AllowMultiSelect = true;
                _cpCategories.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
                _cpCategories.EntityTypeQualifierColumn = "EntityTypeId";
                Controls.Add( _cpCategories );

                _tbKey = new RockTextBox();
                _tbKey.ID = "tbKey";
                _tbKey.Label = "Key";
                _tbKey.Required = true;
                Controls.Add( _tbKey );

                _cvKey = new CustomValidator();
                _cvKey.ID = "cvKey";
                _cvKey.ControlToValidate = _tbKey.ID;
                _cvKey.ClientValidationFunction = "validateKey";
                _cvKey.ServerValidate += cvKey_ServerValidate;
                _cvKey.Display = ValidatorDisplay.Dynamic;
                _cvKey.CssClass = "validation-error help-inline";
                _cvKey.ErrorMessage = "There is already an existing property with the key value you entered.  Please select a different key value.";
                Controls.Add( _cvKey );

                _tbIconCssClass = new RockTextBox();
                _tbIconCssClass.ID = "_tbIconCssClass";
                _tbIconCssClass.Label = "Icon CSS Class";
                Controls.Add( _tbIconCssClass );

                _cbRequired = new RockCheckBox();
                _cbRequired.ID ="cbRequired";
                _cbRequired.Label = "Required";
                _cbRequired.Text = "Require a value";
                Controls.Add( _cbRequired );

                _cbShowInGrid = new RockCheckBox();
                _cbShowInGrid.ID = "cbShowInGrid";
                _cbShowInGrid.Label = "Show in Grid";
                _cbShowInGrid.Text = "Yes";
                _cbShowInGrid.Help = "If selected, this attribute will be included in a grid.";
                Controls.Add( _cbShowInGrid );

                _ddlFieldType = new RockDropDownList();
                _ddlFieldType.ID = "ddlFieldType";
                _ddlFieldType.Label = "Field Type";
                _ddlFieldType.AutoPostBack = true;
                _ddlFieldType.SelectedIndexChanged += _ddlFieldType_SelectedIndexChanged;
                Controls.Add( _ddlFieldType );

                if ( !Page.IsPostBack )
                {
                    _ddlFieldType.DataValueField = "Id";
                    _ddlFieldType.DataTextField = "Name";
                    _ddlFieldType.DataSource = FieldTypeCache.All();
                    _ddlFieldType.DataBind();
                }

                _phQualifiers = new PlaceHolder();
                _phQualifiers.ID = "phQualifiers";
                _phQualifiers.EnableViewState = false;
                Controls.Add( _phQualifiers );

                _phDefaultValue = new PlaceHolder();
                _phDefaultValue.ID = "phDefaultValue";
                _phDefaultValue.EnableViewState = false;
                Controls.Add( _phDefaultValue );

                _btnSave = new LinkButton();
                _btnSave.ID = "btnSave";
                _btnSave.Text = "OK";
                _btnSave.CssClass = "btn btn-primary";
                _btnSave.Click += btnSave_Click;
                Controls.Add( _btnSave );

                _btnCancel = new LinkButton();
                _btnCancel.ID = "btnCancel";
                _btnCancel.Text = "Cancel";
                _btnCancel.CssClass = "btn btn-link";
                _btnCancel.CausesValidation = false;
                _btnCancel.Click += btnCancel_Click;
                Controls.Add( _btnCancel );

                _tbName.Attributes["onblur"] = string.Format( "populateAttributeKey('{0}','{1}')", _tbName.ClientID, _tbKey.ClientID );

                _controlsLoaded = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack && FieldTypeId.HasValue )
            {
                var field = Rock.Web.Cache.FieldTypeCache.Read( FieldTypeId.Value ).Field;
                var qualifierControls = new List<Control>();
                foreach ( Control control in _phQualifiers.Controls )
                {
                    qualifierControls.Add( control );
                }

                DefaultValue = _phDefaultValue.Controls.Count >= 1 ?
                    field.GetEditValue( _phDefaultValue.Controls[0], Qualifiers ) : string.Empty;

                Qualifiers = field.ConfigurationValues( qualifierControls );
            }
            
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            // Recreate the qualifiers and default control in case they changed due to new field type or
            // new qualifier values
            CreateFieldTypeDetailControls( FieldTypeId, true );

            // Set the validation group for all controls
            string validationGroup = ValidationGroup;
            _validationSummary.ValidationGroup = validationGroup;
            _tbName.ValidationGroup = validationGroup;
            _tbDescription.ValidationGroup = validationGroup;
            _cpCategories.ValidationGroup = validationGroup;
            _tbKey.ValidationGroup = validationGroup;
            _cvKey.ValidationGroup = validationGroup;
            _tbIconCssClass.ValidationGroup = validationGroup;
            _cbRequired.ValidationGroup = validationGroup;
            _cbShowInGrid.ValidationGroup = validationGroup;
            _ddlFieldType.ValidationGroup = validationGroup;
            foreach ( var control in _phQualifiers.Controls )
            {
                if ( control is IRockControl )
                {
                    ( (IRockControl)control ).ValidationGroup = validationGroup;
                }
            }
            foreach ( var control in _phDefaultValue.Controls )
            {
                var rockControl = control as IRockControl;
                if ( rockControl != null )
                {
                    rockControl.ValidationGroup = validationGroup;
                }
            }
            _btnSave.ValidationGroup = validationGroup;
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );

            writer.RenderBeginTag( HtmlTextWriterTag.Legend );
            _lAttributeActionTitle.RenderControl( writer );
            writer.RenderEndTag();

            var existingKeyNames = new List<string>();
            ReservedKeyNames.ForEach( n => existingKeyNames.Add(n));
            ObjectPropertyNames.ForEach( n => existingKeyNames.Add(n));
            _hfExistingKeyNames.Value = existingKeyNames.ToJson();
            _hfExistingKeyNames.RenderControl( writer );

            _validationSummary.RenderControl( writer );

            // row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbName.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbDescription.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // row 3
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // row 3 col 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cpCategories.RenderControl( writer );
            _tbKey.RenderControl( writer );
            _cvKey.RenderControl( writer );
            _tbIconCssClass.RenderControl( writer );
            _cbRequired.RenderControl( writer );
            _cbShowInGrid.RenderControl( writer );
            writer.RenderEndTag();

            // row 3 col 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _ddlFieldType.RenderControl( writer );
            _phQualifiers.RenderControl( writer );
            _phDefaultValue.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // </fieldset>
            writer.RenderEndTag();

            if ( ShowActions )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _btnSave.RenderControl( writer );
                writer.Write( Environment.NewLine );
                _btnCancel.RenderControl( writer );
                writer.RenderEndTag();
            }

            RegisterClientScript();
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["FieldTypeId"] = FieldTypeId;

            return base.SaveViewState();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the ServerValidate event of the cvKey control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvKey_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = 
                !ReservedKeyNames.Contains( _tbKey.Text.Trim(), StringComparer.CurrentCultureIgnoreCase ) &&
                !ObjectPropertyNames.Contains( _tbKey.Text.Trim(), StringComparer.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlFieldType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void _ddlFieldType_SelectedIndexChanged( object sender, EventArgs e )
        {
            Qualifiers = new Dictionary<string, ConfigurationValue>();
            DefaultValue = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( SaveClick != null )
            {
                SaveClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( CancelClick != null )
            {
                CancelClick( sender, e );
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the attribute properties.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="objectType">Type of the object.</param>
        public void SetAttributeProperties( Rock.Model.Attribute attribute, Type objectType = null )
        {
            if ( attribute != null )
            {
                this.AttributeId = attribute.Id;
                this.AttributeGuid = attribute.Guid;
                this.Name = attribute.Name;
                this.Key = attribute.Key;
                this.IconCssClass = attribute.IconCssClass;
                this.CategoryIds = attribute.Categories.Select( c => c.Id ).ToList();
                this.Description = attribute.Description;
                this.FieldTypeId = attribute.FieldTypeId;
                this.Required = attribute.IsRequired;
                this.ShowInGrid = attribute.IsGridColumn;

                var qualifiers = new Dictionary<string, ConfigurationValue>();
                if ( attribute.AttributeQualifiers != null )
                {
                    foreach ( Rock.Model.AttributeQualifier qualifier in attribute.AttributeQualifiers )
                    {
                        qualifiers.Add( qualifier.Key, new ConfigurationValue( qualifier.Value ) );
                    }
                }

                this.Qualifiers = qualifiers;
                this.DefaultValue = attribute.DefaultValue;
            }

            if ( objectType != null )
            {
                this.AttributeEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( objectType ).Id;

                ObjectPropertyNames = new List<string>();
                foreach ( var propInfo in objectType.GetProperties() )
                {
                    ObjectPropertyNames.Add( propInfo.Name );
                }
            }

        }

        /// <summary>
        /// Gets the attribute properties.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public void GetAttributeProperties( Rock.Model.Attribute attribute )
        {
            if ( attribute != null )
            {
                attribute.Id = this.AttributeId ?? 0;
                attribute.Guid = this.AttributeGuid;
                attribute.Name = this.Name;
                attribute.Key = this.Key;
                attribute.IconCssClass = this.IconCssClass;
                attribute.Description = this.Description;
                attribute.FieldTypeId = this.FieldTypeId ?? 0;
                attribute.IsMultiValue = false;
                attribute.IsRequired = this.Required;
                attribute.IsGridColumn = this.ShowInGrid;

                attribute.Categories.Clear();
                new CategoryService( new RockContext() ).Queryable().Where( c => this.CategoryIds.Contains( c.Id ) ).ToList().ForEach( c =>
                    attribute.Categories.Add( c ) );

                attribute.AttributeQualifiers.Clear();
                foreach ( var qualifier in Qualifiers )
                {
                    AttributeQualifier attributeQualifier = new AttributeQualifier();
                    attributeQualifier.IsSystem = false;
                    attributeQualifier.Key = qualifier.Key;
                    attributeQualifier.Value = qualifier.Value.Value ?? string.Empty;
                    attribute.AttributeQualifiers.Add( attributeQualifier );
                }

                attribute.DefaultValue = this.DefaultValue;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the field type detail controls.
        /// </summary>
        /// <param name="fieldTypeId">The field type id.</param>
        /// <param name="recreate">if set to <c>true</c> [recreate].</param>
        private void CreateFieldTypeDetailControls(int? fieldTypeId, bool recreate = false )
        {
            EnsureChildControls();

            if ( recreate )
            {
                _phQualifiers.Controls.Clear();
                _phDefaultValue.Controls.Clear();
            }

            if ( fieldTypeId.HasValue )
            {
                var field = Rock.Web.Cache.FieldTypeCache.Read( fieldTypeId.Value ).Field;

                var configControls = field.ConfigurationControls();
                if ( recreate )
                {
                    field.SetConfigurationValues( configControls, Qualifiers );
                }

                int i = 0;
                foreach(var control in configControls )
                {
                    control.ID = string.Format( "qualifier_{0}", i++ );
                    _phQualifiers.Controls.Add( control );
                }

                // default control id needs to be unique to field type because some field types will transform
                // field (i.e. ckeditor) and switching field types will not reset that
                var defaultControl = field.EditControl( Qualifiers, string.Format( "defaultValue_{0}", fieldTypeId.Value ) );
                if ( defaultControl != null )
                {
                    if (recreate)
                    {
                        field.SetEditValue( defaultControl, Qualifiers, DefaultValue );
                    }

                    if ( defaultControl is IRockControl )
                    {
                        var rockControl = defaultControl as IRockControl;
                        rockControl.Required = false;
                        rockControl.Label = "Default Value";
                    }

                    _phDefaultValue.Controls.Add( defaultControl );
                }
            }
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            string script = @"
    function populateAttributeKey(nameControlId, keyControlId ) {
        // if the attribute key hasn't been filled in yet, populate it with the attribute name minus whitespace
        var keyControl = $('#' + keyControlId);
        var keyValue = keyControl.val();

        var reservedKeyJson = keyControl.closest('fieldset').find('.js-existing-key-names').val();
        var reservedKeyNames = eval('(' + reservedKeyJson + ')');

        if (keyValue == '') {

            keyValue = $('#' + nameControlId).val().replace(/\s+/g, '');
            var newKeyValue = keyValue;
        
            var i = 1;
            while ($.inArray(newKeyValue, reservedKeyNames) >= 0) {
                newKeyValue = keyValue + i++;
            }
            
            keyControl.val(newKeyValue);
        }
    }

    function validateKey(sender, args) {
        var keyControl = $('#' + sender.controltovalidate);
        var reservedKeyJson = keyControl.closest('fieldset').find('.js-existing-key-names').val();
        var reservedKeyNames = eval('(' + reservedKeyJson + ')');
        args.IsValid = ( $.inArray( keyControl.val(), reservedKeyNames ) < 0 );
    }
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "AttributeEditor", script, true );
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when Save is clicked
        /// </summary>
        public event EventHandler SaveClick;

        /// <summary>
        /// Occurs when Cancel is clicked.
        /// </summary>
        public event EventHandler CancelClick;

        #endregion

    }

}
