//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Field;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.DropDownList"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:LabeledFieldType runat=server></{0}:LabeledFieldType>" )]
    public class LabeledFieldType : CompositeControl
    {
        private Label label;
        private DropDownList fieldTypeSelect;
        private Label[] configurationLabels;
        private Control[] configurationControls;

        private Rock.Web.Cache.FieldType _fieldType;
        public Rock.Web.Cache.FieldType FieldType
        {
            get 
            { 
                return _fieldType; 
            }
            set
            {
                _fieldType = value;
                ChildControlsCreated = false;
            }
        }

        public string LabelText
        {
            get
            {
                EnsureChildControls();
                return label.Text;
            }
            set
            {
                EnsureChildControls();
                label.Text = value;
            }
        }

        public Dictionary<string, ConfigurationValue > ConfigurationValues
        {
            get
            {
                EnsureChildControls();

                if ( FieldType != null && configurationControls != null )
                    return FieldType.Field.GetConfigurationValues( configurationControls );

                return null;
            }
            set
            {
                EnsureChildControls();

                if ( FieldType != null && configurationControls != null )
                    FieldType.Field.SetConfigurationValues( configurationControls, value );
            }
        }

        protected override void OnInit( System.EventArgs e )
        {
            Page.RegisterRequiresControlState( this );
            base.OnInit( e );
        }

        protected override void LoadControlState( object savedState )
        {
            if ( savedState != null )
                _fieldType = Rock.Web.Cache.FieldType.Read( ( int )savedState );
        }

        protected override object SaveControlState()
        {
            return _fieldType != null ? _fieldType.Id : 0;
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Create the field type's label
            label = new Label();
            label.ID = "lbl";
            label.EnableViewState = true;
            Controls.Add( label );

            // Create the dropdown list for listing the available field types
            Rock.Core.FieldTypeService fieldTypeService = new Core.FieldTypeService();
            var items = fieldTypeService.
                Queryable().
                Select( f => new { f.Id, f.Name } ).
                OrderBy( f => f.Name );

            fieldTypeSelect = new DropDownList();
            fieldTypeSelect.AutoPostBack = true;
            fieldTypeSelect.SelectedIndexChanged += new System.EventHandler( fieldTypeSelect_SelectedIndexChanged );
            fieldTypeSelect.Items.Clear();
            foreach ( var item in items )
                fieldTypeSelect.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            Controls.Add( fieldTypeSelect );

            if ( _fieldType != null )
            {
                var configValues = _fieldType.Field.GetConfigurationValues( null );
                if ( configValues != null )
                {
                    configurationLabels = new Label[configValues.Count];
                    configurationControls = _fieldType.Field.ConfigurationControls();

                    int i = 0;
                    foreach ( KeyValuePair<string, ConfigurationValue> configValue in configValues )
                    {
                        configurationLabels[i] = new Label();
                        configurationLabels[i].Text = configValue.Value.Name;
                        Controls.Add( configurationLabels[i] );

                        Controls.Add( configurationControls[i] );

                        i++;
                    }
                }
            }
        }

        void fieldTypeSelect_SelectedIndexChanged( object sender, System.EventArgs e )
        {
            DropDownList ddl = sender as DropDownList;
            if ( ddl != null )
            {
                int fieldTypeId = 0;
                if ( int.TryParse( ddl.SelectedValue, out fieldTypeId ) )
                    FieldType = Rock.Web.Cache.FieldType.Read( fieldTypeId );

                if ( SelectedIndexChanged != null )
                    SelectedIndexChanged( sender, e );
            }
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            label.RenderControl( writer );

            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( fieldTypeSelect != null && FieldType != null )
            {
                fieldTypeSelect.SelectedValue = FieldType.Id.ToString();
                fieldTypeSelect.RenderControl( writer );
            }

            writer.RenderEndTag();

            writer.RenderEndTag();

            if (configurationControls != null)
                for (int i = 0; i < configurationControls.Count(); i++)
                    RenderControlGroup(writer, configurationLabels[i], configurationControls[0]);
        }

        private void RenderControlGroup( HtmlTextWriter writer, Label label, Control control )
        {
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            label.RenderControl( writer );

            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            control.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        public event EventHandler<EventArgs> SelectedIndexChanged;
    }
}