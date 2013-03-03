//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Attribute = Rock.Model.Attribute;

namespace Rock.Web.UI.Controls
{
    public class RockAttributeEditor : CompositeControl
    {
        #region UI Controls

        private HiddenField hfAttributeGuid;
        private HiddenField hfAttributeId;
        private Literal lAttributeActionTitle;

        // column 1
        private DataTextBox tbAttributeName;
        private DataTextBox tbAttributeKey;
        private DataTextBox tbAttributeCategory;
        private DataTextBox tbAttributeDescription;

        // column 2
        private DataDropDownList ddlAttributeFieldType;
        private PlaceHolder phAttributeFieldTypeQualifiers;
        private PlaceHolder phAttributeDefaultValue;
        private LabeledCheckBox cbAttributeMultiValue;
        private LabeledCheckBox cbAttributeRequired;
        private LabeledCheckBox cbShowInGrid;

        // buttons
        private LinkButton btnSaveAttribute;
        private LinkButton btnCancelAttribute;

        #endregion

        #region CompositeControl stuff

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            hfAttributeGuid = new HiddenField { ID = string.Format( "hfAttributeGuid_{0}", this.ID ) };
            hfAttributeId = new HiddenField { ID = string.Format( "hfAttributeId_{0}", this.ID ) };
            lAttributeActionTitle = new Literal { ID = string.Format( "lAttributeActionTitle_{0}", this.ID ) };

            tbAttributeName = new DataTextBox { ID = string.Format( "tbAttributeName_{0}", this.ID ) };
            tbAttributeName.SourceTypeName = "Rock.Model.Attribute, Rock";
            tbAttributeName.PropertyName = "Name";

            tbAttributeKey = new DataTextBox { ID = string.Format( "tbAttributeKey_{0}", this.ID ) };
            tbAttributeKey.SourceTypeName = "Rock.Model.Attribute, Rock";
            tbAttributeKey.PropertyName = "Name";

            tbAttributeCategory = new DataTextBox { ID = string.Format( "tbAttributeCategory_{0}", this.ID ) };
            tbAttributeCategory.SourceTypeName = "Rock.Model.Attribute, Rock";
            tbAttributeCategory.PropertyName = "Category";

            tbAttributeDescription = new DataTextBox { ID = string.Format( "tbAttributeDescription_{0}", this.ID ) };
            tbAttributeDescription.SourceTypeName = "Rock.Model.Attribute, Rock";
            tbAttributeDescription.PropertyName = "Description";

            ddlAttributeFieldType = new DataDropDownList { ID = string.Format( "ddlAttributeFieldType_{0}", this.ID ) };
            ddlAttributeFieldType.LabelText = "Field Type";
            ddlAttributeFieldType.SourceTypeName = "Rock.Model.FieldType, Rock";
            ddlAttributeFieldType.PropertyName = "Name";
            ddlAttributeFieldType.DataValueField = "Id";
            ddlAttributeFieldType.DataTextField = "Name";
            ddlAttributeFieldType.AutoPostBack = true;
            ddlAttributeFieldType.SelectedIndexChanged += ddlAttributeFieldType_SelectedIndexChanged;
            

            phAttributeFieldTypeQualifiers = new PlaceHolder { ID = string.Format( "phAttributeFieldTypeQualifiers_{0}", this.ID ) };

            phAttributeDefaultValue = new PlaceHolder { ID = string.Format( "phAttributeDefaultValue_{0}", this.ID ) };
            phAttributeDefaultValue.EnableViewState = false;

            cbAttributeMultiValue = new LabeledCheckBox { ID = string.Format( "cbAttributeMultiValue_{0}", this.ID ) };
            cbAttributeMultiValue.LabelText = "Allow Multiple Values";

            cbAttributeRequired = new LabeledCheckBox { ID = string.Format( "cbAttributeRequired_{0}", this.ID ) };
            cbAttributeRequired.LabelText = "Required";

            cbShowInGrid = new LabeledCheckBox { ID = string.Format( "cbShowInGrid_{0}", this.ID ) };
            cbShowInGrid.LabelText = "Show in Grid";

            btnSaveAttribute = new LinkButton { ID = string.Format( "btnSaveAttribute_{0}", this.ID ) };
            btnSaveAttribute.Text = "OK";
            btnSaveAttribute.CssClass = "btn btn-primary";
            btnSaveAttribute.Click += btnSaveAttribute_Click;

            btnCancelAttribute = new LinkButton { ID = string.Format( "btnCancelAttribute_{0}", this.ID ) };
            btnCancelAttribute.Text = "Cancel";
            btnCancelAttribute.CssClass = "btn";
            btnCancelAttribute.CausesValidation = false;
            btnCancelAttribute.Click += btnCancelAttribute_Click;

            Controls.Add( hfAttributeGuid );
            Controls.Add( hfAttributeId );
            Controls.Add( lAttributeActionTitle );
            Controls.Add( tbAttributeName );
            Controls.Add( tbAttributeKey );
            Controls.Add( tbAttributeCategory );
            Controls.Add( tbAttributeDescription );
            Controls.Add( ddlAttributeFieldType );
            Controls.Add( phAttributeFieldTypeQualifiers );
            Controls.Add( phAttributeDefaultValue );
            Controls.Add( cbAttributeMultiValue );
            Controls.Add( cbAttributeRequired );
            Controls.Add( cbShowInGrid );
            Controls.Add( btnSaveAttribute );
            Controls.Add( btnCancelAttribute );
        }

        protected override void Render( HtmlTextWriter writer )
        {
            hfAttributeGuid.RenderControl( writer );
            hfAttributeId.RenderControl( writer );
            
            writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );
            
            writer.RenderBeginTag( HtmlTextWriterTag.Legend );
            lAttributeActionTitle.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbAttributeName.RenderControl( writer );
            tbAttributeKey.RenderControl( writer );
            tbAttributeCategory.RenderControl( writer );
            tbAttributeDescription.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlAttributeFieldType.RenderControl( writer );
            phAttributeFieldTypeQualifiers.RenderControl( writer );
            writer.WriteLine( "<p>Default Value</p>" );
            phAttributeDefaultValue.RenderControl( writer );
            cbAttributeMultiValue.RenderControl( writer );
            cbAttributeRequired.RenderControl( writer );
            cbShowInGrid.RenderControl( writer );
            writer.RenderEndTag();
            
            // row-fluid </div>
            writer.RenderEndTag();

            // </fieldset>
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            btnSaveAttribute.RenderControl( writer );
            btnCancelAttribute.RenderControl( writer );
            writer.RenderEndTag();
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            EnsureChildControls();

            if ( Page.IsPostBack )
            {
                int fieldTypeId = ddlAttributeFieldType.SelectedValueAsInt() ?? 0;
                SetAttributeEditControl( fieldTypeId, false, null );
                BuildConfigControls();
            }

            string populateAttributeKeyScript = @"
function populateAttributeKey(nameControlId, keyControlId ) {
    // if the attribute key hasn't been filled in yet, populate it with the attribute name minus whitespace
    var keyControl = $('#' + keyControlId);
    var keyValue = keyControl.val();
    if (keyValue == '') {
        var nameValue = $('#' + nameControlId).val();
        nameValue = nameValue.replace(/\s+/g, '');
        keyControl.val(nameValue);
    }
}
";

            tbAttributeName.Attributes["onblur"] = string.Format( "populateAttributeKey('{0}','{1}')", tbAttributeName.ClientID, tbAttributeKey.ClientID );

            ScriptManager.RegisterClientScriptBlock( this, this.GetType(), "PopulateAttributeKeyScript", populateAttributeKeyScript, true );
        }

        /// <summary>
        /// Gets the attribute id.
        /// </summary>
        /// <value>
        /// The attribute id.
        /// </value>
        public int AttributeId
        {
            get
            {
                return int.Parse( hfAttributeId.Value );
            }
        }

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public void GetAttributeValues( Rock.Model.Attribute attribute )
        {
            attribute.Guid = new Guid( hfAttributeGuid.Value );
            attribute.Id = int.Parse( hfAttributeId.Value );

            attribute.Key = tbAttributeKey.Text;
            attribute.Name = tbAttributeName.Text;
            attribute.Category = tbAttributeCategory.Text;
            attribute.Description = tbAttributeDescription.Text;
            attribute.FieldTypeId = int.Parse( ddlAttributeFieldType.SelectedValue );
            attribute.IsGridColumn = cbShowInGrid.Checked;

            FieldTypeCache fieldTypeCache = FieldTypeCache.Read( attribute.FieldTypeId );
            attribute.AttributeQualifiers = new List<AttributeQualifier>();

            List<Control> configControls = new List<Control>();
            foreach ( var key in fieldTypeCache.Field.ConfigurationKeys() )
            {
                configControls.Add( phAttributeFieldTypeQualifiers.FindControl( "configControl_" + key ) );
            }

            foreach ( var configValue in fieldTypeCache.Field.ConfigurationValues( configControls ) )
            {
                AttributeQualifier qualifier = new AttributeQualifier();
                qualifier.IsSystem = false;
                qualifier.Key = configValue.Key;
                qualifier.Value = configValue.Value.Value ?? string.Empty;
                attribute.AttributeQualifiers.Add( qualifier );
            }

            Dictionary<string, Rock.Field.ConfigurationValue> qualifierValues = fieldTypeCache.Field.ConfigurationValues( configControls );

            attribute.DefaultValue = fieldTypeCache.Field.GetEditValue( phAttributeDefaultValue.Controls[0], qualifierValues );

            attribute.IsMultiValue = cbAttributeMultiValue.Checked;
            attribute.IsRequired = cbAttributeRequired.Checked;
        }

        /// <summary>
        /// Edits the attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="actionTitle">The action title.</param>
        public void EditAttribute( Rock.Model.Attribute attribute, string actionTitle )
        {
            FieldTypeService fieldTypeService = new FieldTypeService();
            fieldTypeService.RegisterFieldTypes( this.Page.Server.MapPath( "~" ) );
            List<FieldType> fieldTypes = fieldTypeService.Queryable().OrderBy( a => a.Name ).ToList();

            ddlAttributeFieldType.DataSource = fieldTypes;
            ddlAttributeFieldType.DataBind();

            lAttributeActionTitle.Text = actionTitle;
            ddlAttributeFieldType.SetValue( attribute.FieldTypeId );
            if ( attribute.Guid != Guid.Empty )
            {
                hfAttributeGuid.Value = attribute.Guid.ToString();
                AttributeCache ac = AttributeCache.Read( attribute );
                BuildConfigControls( ac.QualifierValues );
            }
            else
            {
                hfAttributeGuid.Value = Guid.NewGuid().ToString();
                BuildConfigControls();
            }

            hfAttributeId.Value = attribute.Id.ToString();

            tbAttributeKey.Text = attribute.Key;
            tbAttributeName.Text = attribute.Name;
            tbAttributeCategory.Text = attribute.Category;
            tbAttributeDescription.Text = attribute.Description;

            SetAttributeEditControl( attribute.FieldTypeId, true, attribute.DefaultValue );

            cbAttributeMultiValue.Checked = attribute.IsMultiValue;
            cbAttributeRequired.Checked = attribute.IsRequired;
            cbShowInGrid.Checked = attribute.IsGridColumn;
        }

        /// <summary>
        /// Sets the attribute edit control.
        /// </summary>
        /// <param name="fieldTypeId">The field type id.</param>
        /// <param name="value">The value.</param>
        private void SetAttributeEditControl( int fieldTypeId, bool setValue, string value )
        {
            FieldTypeCache fieldTypeCache;
            if ( fieldTypeId == 0 )
            {
                fieldTypeCache = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT );
            }
            else
            {
                fieldTypeCache = FieldTypeCache.Read( fieldTypeId );
            }

            Dictionary<string, Rock.Field.ConfigurationValue> qualifierValues = new Dictionary<string, Rock.Field.ConfigurationValue>();

            string defaultValueControlID = string.Format( "defaultValueControl_FieldType_{0}", fieldTypeCache.Guid );
            Control defaultValueControl = phAttributeFieldTypeQualifiers.FindControl( defaultValueControlID );
            if ( defaultValueControl == null )
            {
                defaultValueControl = fieldTypeCache.Field.EditControl( qualifierValues );
                defaultValueControl.ID = defaultValueControlID;
                phAttributeDefaultValue.Controls.Clear();
                phAttributeDefaultValue.Controls.Add( defaultValueControl );
            }

            if ( setValue )
            {
                fieldTypeCache.Field.SetEditValue( defaultValueControl, qualifierValues, value );
            }
        }

        /// <summary>
        /// Occurs when [save click].
        /// </summary>
        public event EventHandler SaveClick;

        /// <summary>
        /// Occurs when [cancel click].
        /// </summary>
        public event EventHandler CancelClick;

        /// <summary>
        /// Handles the Click event of the btnSaveAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveAttribute_Click( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute;
            if ( hfAttributeId.IsZero() )
            {
                attribute = new Rock.Model.Attribute();
            }
            else
            {
                AttributeService attributeService = new AttributeService();
                attribute = attributeService.Get( hfAttributeId.ValueAsInt() );
            }

            GetAttributeValues( attribute );
            if ( !attribute.IsValid )
            {
                return;
            }

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
        protected void btnCancelAttribute_Click( object sender, EventArgs e )
        {
            if ( CancelClick != null )
            {
                CancelClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlAttributeFieldType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlAttributeFieldType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int fieldTypeId = ddlAttributeFieldType.SelectedValueAsInt() ?? 0;
            SetAttributeEditControl( fieldTypeId, true, string.Empty );
        }

        /// <summary>
        /// Builds the config controls.
        /// </summary>
        private void BuildConfigControls()
        {
            BuildConfigControls( null );
        }

        /// <summary>
        /// Builds the config controls.
        /// </summary>
        /// <param name="qualifierValues">The qualifier values.</param>
        private void BuildConfigControls( Dictionary<string, Rock.Field.ConfigurationValue> qualifierValues )
        {
            int fieldTypeId = ddlAttributeFieldType.SelectedValueAsInt() ?? 0;
            var fieldType = Rock.Web.Cache.FieldTypeCache.Read( fieldTypeId );
            if ( fieldType != null )
            {
                phAttributeFieldTypeQualifiers.Controls.Clear();
                var configControls = fieldType.Field.ConfigurationControls();

                int i = 0;
                foreach ( var configValue in fieldType.Field.ConfigurationValues( null ) )
                {
                    var ctrlGroup = new HtmlGenericControl( "div" );
                    phAttributeFieldTypeQualifiers.Controls.Add( ctrlGroup );
                    ctrlGroup.AddCssClass( "control-group" );

                    var lbl = new Label();
                    ctrlGroup.Controls.Add( lbl );
                    lbl.AddCssClass( "control-label" );
                    lbl.Text = configValue.Value.Name;

                    var ctrls = new HtmlGenericControl( "div" );
                    ctrlGroup.Controls.Add( ctrls );
                    ctrls.AddCssClass( "controls" );

                    Control control = configControls[i];
                    ctrls.Controls.Add( control );
                    control.ID = "configControl_" + configValue.Key;

                    i++;
                }

                if ( qualifierValues != null )
                {
                    fieldType.Field.SetConfigurationValues( configControls, qualifierValues );
                }
            }
        }

    }
}
