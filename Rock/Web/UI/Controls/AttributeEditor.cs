//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    public class AttributeEditor : CompositeControl
    {
        #region UI Controls

        private Literal lAttributeActionTitle;

        // column 1
        private DataTextBox tbName;
        private DataTextBox tbKey;
        private CustomValidator cvKey;
        private CategoryPicker cpCategories;
        private DataTextBox tbDescription;

        // column 2
        private DataDropDownList ddlFieldType;

        private List<Control> QualifierControls;
        private Control DefaultValueControl;

        private RockCheckBox cbMultiValue;
        private RockCheckBox cbRequired;
        private RockCheckBox cbShowInGrid;

        // buttons
        private LinkButton btnSave;
        private LinkButton btnCancel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeEditor" /> class.
        /// </summary>
        public AttributeEditor()
        {
            lAttributeActionTitle = new Literal();
            tbName = new DataTextBox();
            tbKey = new DataTextBox();
            cvKey = new CustomValidator();
            cpCategories = new CategoryPicker();
            tbDescription = new DataTextBox();

            // Create FieldType Dropdown
            FieldTypeService fieldTypeService = new FieldTypeService();
            fieldTypeService.RegisterFieldTypes( System.Web.HttpContext.Current.Server.MapPath( "~" ) );
            List<Rock.Model.FieldType> fieldTypes = fieldTypeService.Queryable().OrderBy( a => a.Name ).ToList();

            ddlFieldType = new DataDropDownList();
            ddlFieldType.DataValueField = "Id";
            ddlFieldType.DataTextField = "Name";
            ddlFieldType.DataSource = fieldTypes;
            ddlFieldType.DataBind();
            ddlFieldType.AutoPostBack = true;
            ddlFieldType.SelectedIndexChanged += ddlFieldType_SelectedIndexChanged;

            cbMultiValue = new RockCheckBox();
            cbRequired = new RockCheckBox();
            cbShowInGrid = new RockCheckBox();
        }

        void ddlFieldType_SelectedIndexChanged( object sender, EventArgs e )
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the action title.
        /// </summary>
        /// <value>
        /// The action title.
        /// </value>
        public string ActionTitle
        {
            get { return lAttributeActionTitle.Text; }
            set { lAttributeActionTitle.Text = value; }
        }

        /// <summary>
        /// Gets or sets the attribute id.
        /// </summary>
        /// <value>
        /// The attribute id.
        /// </value>
        public int? AttributeId
        {
            get { return ViewState["AttributeId"] as int?; }
            set { ViewState["AttributeId"] = value; }
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
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return tbName.Text; }
            set { tbName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key
        {
            get { return tbKey.Text; }
            set { tbKey.Text = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return tbDescription.Text; }
            set { tbDescription.Text = value; }
        }

        /// <summary>
        /// Gets or sets the categories ids.
        /// </summary>
        /// <value>
        /// The categories ids.
        /// </value>
        public IEnumerable<int> CategoryIds
        {
            get { return cpCategories.SelectedValuesAsInt();  }
            set { cpCategories.SetValues(value); }
        }

        /// <summary>
        /// Gets or sets the field type id.
        /// </summary>
        /// <value>
        /// The field type id.
        /// </value>
        public int? FieldTypeId
        {
            get { return ViewState["FieldTypeId"] as int?; }
            set { ViewState["FieldTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the qualifiers.
        /// </summary>
        /// <value>
        /// The qualifiers.
        /// </value>
        public Dictionary<string, ConfigurationValue> Qualifiers
        {
            get { return ViewState["Qualifiers"] as Dictionary<string, ConfigurationValue> ?? 
                new Dictionary<string, ConfigurationValue>(); }
            set { ViewState["Qualifiers"] = value; }
        }

        /// <summary>
        /// Gets or sets any key values that should not be allowed to be used
        /// </summary>
        /// <value>
        /// The reserved key names.
        /// </value>
        public List<string> ReservedKeyNames
        {
            get { return ViewState["ReservedKeyNames"] as List<string> ?? new List<string>(); }
            set { ViewState["ReservedKeyNames"] = value; }
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public string DefaultValue
        {
            get { return ViewState["DefaultValue"] as string; }
            set { ViewState["DefaultValue"] = value; }
        }


        /// <summary>
        /// Gets or sets the entity type that attribute is being used for.  If specified, the the list of available categories
        /// will only display categories for attribute belonging to this type of entity
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public int? AttributeEntityTypeId
        {
            get { return ViewState["AttributeEntityTypeId"] as int?; }
            set { ViewState["AttributeEntityTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [multi value].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [multi value]; otherwise, <c>false</c>.
        /// </value>
        public bool MultiValue
        {
            get { return cbMultiValue.Checked; }
            set { cbMultiValue.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AttributeEditor" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required
        {
            get { return cbRequired.Checked; }
            set { cbRequired.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in grid]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInGrid
        {
            get { return cbShowInGrid.Checked; }
            set { cbShowInGrid.Checked = value; }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                FieldTypeId = ddlFieldType.SelectedValueAsInt();
                if ( FieldTypeId.HasValue && FieldTypeId != 0 )
                {
                    var field = Rock.Web.Cache.FieldTypeCache.Read( FieldTypeId.Value ).Field;

                    if ( QualifierControls != null )
                    {
                        Qualifiers = field.ConfigurationValues( QualifierControls );
                    }

                    if ( DefaultValueControl != null )
                    {
                        DefaultValue = field.GetEditValue( DefaultValueControl, Qualifiers );
                    }
                }
            }
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            // Recreate Controls right before saving the current state (Last point to create them based on viewstate properties)
            RecreateChildControls();
            
            return base.SaveViewState();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the controls properties from the attribute
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="objectType">Type of the object that attribute is being created for.  If specified, the UI 
        /// will prevent user from creating an attribute with the same key as an existing property name of the object.</param>
        public void SetAttributeProperties( Rock.Model.Attribute attribute, Type objectType = null )
        {
            if ( attribute != null )
            {
                this.AttributeId = attribute.Id;
                this.AttributeGuid = attribute.Guid;
                this.Name = attribute.Name;
                this.Key = attribute.Key;
                this.CategoryIds = attribute.Categories.Select( c => c.Id ).ToList();
                this.Description = attribute.Description;
                this.FieldTypeId = attribute.FieldTypeId;
                this.DefaultValue = attribute.DefaultValue;
                this.MultiValue = attribute.IsMultiValue;
                this.Required = attribute.IsRequired;
                this.ShowInGrid = attribute.IsGridColumn;

                this.Qualifiers = new Dictionary<string, ConfigurationValue>();
                if ( attribute.AttributeQualifiers != null )
                {
                    foreach ( Rock.Model.AttributeQualifier qualifier in attribute.AttributeQualifiers )
                    {
                        this.Qualifiers.Add( qualifier.Key, new ConfigurationValue( qualifier.Value ) );
                    }
                }
            }

            if ( objectType != null )
            {
                ReservedKeyNames = new List<string>();
                foreach ( var propInfo in objectType.GetProperties() )
                {
                    ReservedKeyNames.Add( propInfo.Name );
                }
            }

        }

        /// <summary>
        /// Updates the attribute with the value of the control's properties NOTE: This call should be wrapped in a 
        /// Unit of Work.
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
                attribute.Description = this.Description;
                attribute.FieldTypeId = this.FieldTypeId ?? 0;
                attribute.DefaultValue = this.DefaultValue;
                attribute.IsMultiValue = this.MultiValue;
                attribute.IsRequired = this.Required;
                attribute.IsGridColumn = this.ShowInGrid;

                attribute.Categories.Clear();
                new CategoryService().Queryable().Where( c => this.CategoryIds.Contains( c.Id ) ).ToList().ForEach( c =>
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
            }
        }

        #endregion

        #region CompositeControl Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            lAttributeActionTitle = new Literal { ID = string.Format( "lAttributeActionTitle_{0}", this.ID ) };
            Controls.Add( lAttributeActionTitle );

            tbName.ID = string.Format( "tbName_{0}", this.ID );
            tbName.SourceTypeName = "Rock.Model.Attribute, Rock";
            tbName.PropertyName = "Name";
            tbName.Required = true;
            Controls.Add( tbName );

            tbKey.ID = string.Format( "tbKey_{0}", this.ID );
            tbKey.SourceTypeName = "Rock.Model.Attribute, Rock";
            tbKey.PropertyName = "Key";
            tbKey.Required = true;
            Controls.Add( tbKey );

            cvKey.ID = string.Format("cvKey_{0}", this.ID);
            cvKey.ControlToValidate = tbKey.ID;
            cvKey.ClientValidationFunction = "validateKey";
            cvKey.ServerValidate += cvKey_ServerValidate;
            cvKey.Display = ValidatorDisplay.Dynamic;
            cvKey.CssClass = "validation-error help-inline";
            cvKey.ErrorMessage = "There is already an existing property with the key value you entered.  Please select a different key value";
            Controls.Add( cvKey );

            cpCategories.Label = "Categories";
            cpCategories.ID = string.Format( "cpCategories_{0}", this.ID );
            cpCategories.AllowMultiSelect = true;
            cpCategories.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
            cpCategories.EntityTypeQualifierColumn = "EntityTypeId";
            Controls.Add( cpCategories );

            tbDescription.ID = string.Format( "tbDescription_{0}", this.ID );
            tbDescription.TextMode = TextBoxMode.MultiLine;
            tbDescription.Rows = 3;
            tbDescription.SourceTypeName = "Rock.Model.Attribute, Rock";
            tbDescription.PropertyName = "Description";
            Controls.Add( tbDescription );

            ddlFieldType.ID = string.Format( "ddlFieldType_{0}", this.ID );
            ddlFieldType.Label = "Field Type";
            ddlFieldType.SourceTypeName = "Rock.Model.FieldType, Rock";
            ddlFieldType.PropertyName = "Name";
            ddlFieldType.AutoPostBack = true;
            Controls.Add( ddlFieldType );

            if ( FieldTypeId.HasValue && FieldTypeId != 0 )
            {
                var field = Rock.Web.Cache.FieldTypeCache.Read( FieldTypeId.Value ).Field;

                QualifierControls = field.ConfigurationControls();
                int i= 0;
                foreach ( var control in QualifierControls )
                {
                    control.ID = string.Format( "qualifier{0}_{1}_{2}", i++, FieldTypeId.Value, this.ID );
                    Controls.Add( control );
                }

                DefaultValueControl = field.EditControl( Qualifiers, string.Format( "defaultValue_{0}_{1}", FieldTypeId.Value, this.ID ) );
                if ( DefaultValueControl != null )
                {
                    if ( DefaultValueControl is IRequiredControl )
                    {
                        ( (IRequiredControl)DefaultValueControl ).Required = false;
                    }

                    Controls.Add( DefaultValueControl );
                }
            }

            cbMultiValue.ID = string.Format( "cbMultiValue_{0}", this.ID );
            cbMultiValue.Text = "Allow Multiple Values";
            Controls.Add( cbMultiValue );

            cbRequired.ID = string.Format( "cbRequired_{0}", this.ID );
            cbRequired.Text = "Required";
            Controls.Add( cbRequired );

            cbShowInGrid.ID = string.Format( "cbShowInGrid_{0}", this.ID );
            cbShowInGrid.Text = "Show in Grid";
            Controls.Add( cbShowInGrid );

            btnSave = new LinkButton { ID = string.Format( "btnSave_{0}", this.ID ) };
            btnSave.Text = "OK";
            btnSave.CssClass = "btn btn-primary";
            btnSave.Click += btnSave_Click;
            Controls.Add( btnSave );

            btnCancel = new LinkButton { ID = string.Format( "btnCancel_{0}", this.ID ) };
            btnCancel.Text = "Cancel";
            btnCancel.CssClass = "btn";
            btnCancel.CausesValidation = false;
            btnCancel.Click += btnCancel_Click;
            Controls.Add( btnCancel );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );
            
            writer.RenderBeginTag( HtmlTextWriterTag.Legend );
            lAttributeActionTitle.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbName.Attributes["onblur"] = string.Format( "populateAttributeKey('{0}','{1}')", tbName.ClientID, tbKey.ClientID );
            tbName.RenderControl( writer );
            tbKey.RenderControl( writer );
            cvKey.RenderControl( writer );

            if ( this.AttributeEntityTypeId.HasValue )
            {
                cpCategories.EntityTypeQualifierValue = this.AttributeEntityTypeId.Value.ToString();
            }
            cpCategories.RenderControl( writer );

            tbDescription.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlFieldType.SetValue( FieldTypeId );
            ddlFieldType.RenderControl( writer );

            if ( FieldTypeId.HasValue && FieldTypeId != 0 )
            {
                var field = Rock.Web.Cache.FieldTypeCache.Read( FieldTypeId.Value ).Field;

                if ( QualifierControls != null )
                {
                    field.SetConfigurationValues( QualifierControls, Qualifiers );

                    int i = 0;
                    foreach ( var configValue in field.ConfigurationValues( null ) )
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-group" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        writer.Write( configValue.Value.Name );
                        writer.RenderEndTag();

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        QualifierControls[i].RenderControl( writer );
                        writer.RenderEndTag();

                        writer.RenderEndTag();

                        i++;
                    }
                }
                if ( DefaultValueControl != null )
                {
                    field.SetEditValue( DefaultValueControl, Qualifiers, DefaultValue );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-group" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Default Value" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    DefaultValueControl.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.RenderEndTag();
                }
            }

            cbMultiValue.RenderControl( writer );
            cbRequired.RenderControl( writer );
            cbShowInGrid.RenderControl( writer );
            writer.RenderEndTag();
            
            // row-fluid </div>
            writer.RenderEndTag();

            // </fieldset>
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            btnSave.RenderControl( writer );
            writer.Write( Environment.NewLine );
            btnCancel.RenderControl( writer );
            writer.RenderEndTag();

            RegisterClientScript();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            string script = string.Format( @"

var reservedKeyNames = {0};

function populateAttributeKey(nameControlId, keyControlId ) {{
    // if the attribute key hasn't been filled in yet, populate it with the attribute name minus whitespace
    var keyControl = $('#' + keyControlId);
    var keyValue = keyControl.val();
    if (keyValue == '') {{

        keyValue = $('#' + nameControlId).val().replace(/\s+/g, '');
        var newKeyValue = keyValue;
        
        var i = 1;
        while ($.inArray(newKeyValue, reservedKeyNames) >= 0) {{
            newKeyValue = keyValue + i++;
        }}
            
        keyControl.val(newKeyValue);
    }}
}}

function validateKey(sender, args) {{
    args.IsValid = ( $.inArray( $('#{1}').val(), reservedKeyNames ) < 0 );
}}
",
                ReservedKeyNames.ToJson(), tbKey.ClientID);

            ScriptManager.RegisterStartupScript( this, this.GetType(), "AttributeEditor", script, true );
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when [save click].
        /// </summary>
        public event EventHandler SaveClick;

        /// <summary>
        /// Occurs when [cancel click].
        /// </summary>
        public event EventHandler CancelClick;

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the ServerValidate event of the cvKey control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs" /> instance containing the event data.</param>
        void cvKey_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = !ReservedKeyNames.Contains( tbKey.Text.Trim(), StringComparer.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( SaveClick != null )
            {
                SaveClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( CancelClick != null )
            {
                CancelClick( sender, e );
            }
        }

        #endregion

    }
}
