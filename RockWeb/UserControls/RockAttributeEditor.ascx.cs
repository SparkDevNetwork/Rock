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

namespace RockWeb.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RockAttributeEditor : UserControl
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                int fieldTypeId = ddlAttributeFieldType.SelectedValueAsInt() ?? 0;
                SetAttributeEditControl( fieldTypeId, false, null );
                BuildConfigControls();
            }
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
        public void GetAttributeValues( Attribute attribute )
        {
            attribute.Guid = new Guid( hfAttributeGuid.Value );
            attribute.Id = int.Parse( hfAttributeId.Value );
            attribute.Key = tbAttributeKey.Text;
            attribute.Name = tbAttributeName.Text;
            attribute.Category = tbAttributeCategory.Text;
            attribute.Description = tbAttributeDescription.Text;
            attribute.FieldTypeId = int.Parse( ddlAttributeFieldType.SelectedValue );

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
        public void EditAttribute( Attribute attribute, string actionTitle )
        {
            FieldTypeService fieldTypeService = new FieldTypeService();
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