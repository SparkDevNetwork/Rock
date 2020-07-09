// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace com.bemaservices.WorkflowExtensions.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    [Serializable]
    public class SelectExtendedFieldType : Rock.Field.FieldType
    {

        #region Configuration

        private const string PARENT_VALUES_KEY = "parent_values";
        private const string PARENT_FIELDTYPE_KEY = "parent_fieldtype";
        private const string PARENT_REPEAT_COLUMNS = "parent_repeatColumns";

        private const string CHILD_VALUES_KEY = "child_values";
        private const string CHILD_FIELDTYPE_KEY = "child_fieldtype";
        private const string CHILD_REPEAT_COLUMNS = "child_repeatColumns";

        private ListControl _parentEditControl;
        private ListControl _childEditControl;

        private Dictionary<string, ConfigurationValue> _configurationValues;


        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( PARENT_VALUES_KEY );
            configKeys.Add( PARENT_FIELDTYPE_KEY );
            configKeys.Add( PARENT_REPEAT_COLUMNS );

            configKeys.Add( CHILD_VALUES_KEY );
            configKeys.Add( CHILD_FIELDTYPE_KEY );
            configKeys.Add( CHILD_REPEAT_COLUMNS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var tbParentValues = new RockTextBox();
            tbParentValues.TextMode = TextBoxMode.MultiLine;
            tbParentValues.Rows = 3;
            tbParentValues.AutoPostBack = true;
            tbParentValues.TextChanged += OnQualifierUpdated;
            tbParentValues.Label = "Values";
            tbParentValues.Help = "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>.";
            controls.Add( tbParentValues );

            var ddlParentFieldType = new RockDropDownList();
            ddlParentFieldType.Items.Add( new ListItem( "Drop Down List", "ddl" ) );
            ddlParentFieldType.Items.Add( new ListItem( "Single Select Drop Down List (Enhanced for Long Lists)", "ddl_single_enhanced" ) );
            //ddlParentFieldType.Items.Add( new ListItem( "Multi Select Drop Down List (Enhanced for Long Lists)", "ddl_multi_enhanced" ) );
            ddlParentFieldType.Items.Add( new ListItem( "Radio Buttons", "rb" ) );
            //  ddlParentFieldType.Items.Add( new ListItem( "Check Boxes", "cb" ) );
            ddlParentFieldType.AutoPostBack = true;
            ddlParentFieldType.SelectedIndexChanged += OnQualifierUpdated;
            ddlParentFieldType.Label = "Control Type";
            ddlParentFieldType.Help = "The type of control to use for selecting a single value from the list.";
            controls.Add( ddlParentFieldType );

            var tbParentRepeatColumns = new NumberBox();
            tbParentRepeatColumns.Label = "Columns";
            tbParentRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space.";
            tbParentRepeatColumns.MinimumValue = "0";
            tbParentRepeatColumns.AutoPostBack = true;
            tbParentRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbParentRepeatColumns );

            var tbChildValues = new RockTextBox();
            tbChildValues.TextMode = TextBoxMode.MultiLine;
            tbChildValues.Rows = 3;
            tbChildValues.AutoPostBack = true;
            tbChildValues.TextChanged += OnQualifierUpdated;
            tbChildValues.Label = "Child Values";
            tbChildValues.Help = "The source of the values to display in a child list.  Format is either 'value1^parent1,value2^parent2,value3^parent3,...', 'value1^parent1^text1,value2^parent2^text2,value3^parent3^text3,...', or a SQL Select statement that returns result set with 'Value', 'ParentValue', and 'Text' columns <span class='tip tip-lava'></span>.";
            controls.Add( tbChildValues );

            var ddlChildFieldType = new RockDropDownList();
            ddlChildFieldType.Items.Add( new ListItem( "Drop Down List", "ddl" ) );
            ddlChildFieldType.Items.Add( new ListItem( "Single Select Drop Down List (Enhanced for Long Lists)", "ddl_single_enhanced" ) );
            ddlChildFieldType.Items.Add( new ListItem( "Multi Select Drop Down List (Enhanced for Long Lists)", "ddl_multi_enhanced" ) );
            ddlChildFieldType.Items.Add( new ListItem( "Radio Buttons", "rb" ) );
            ddlChildFieldType.Items.Add( new ListItem( "Check Boxes", "cb" ) );
            ddlChildFieldType.AutoPostBack = true;
            ddlChildFieldType.SelectedIndexChanged += OnQualifierUpdated;
            ddlChildFieldType.Label = "Child Control Type";
            ddlChildFieldType.Help = "The type of control to use for selecting a single value from the child list.";
            controls.Add( ddlChildFieldType );

            var tbChildRepeatColumns = new NumberBox();
            tbChildRepeatColumns.Label = "Child Columns";
            tbChildRepeatColumns.Help = "Select how many columns the child list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space.";
            tbChildRepeatColumns.MinimumValue = "0";
            tbChildRepeatColumns.AutoPostBack = true;
            tbChildRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbChildRepeatColumns );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();

            string description = "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>.";
            configurationValues.Add( PARENT_VALUES_KEY, new ConfigurationValue( "Parent Values", description, string.Empty ) );

            description = "The type of control to use for selecting a single value from the list.";
            configurationValues.Add( PARENT_FIELDTYPE_KEY, new ConfigurationValue( "Parent Control Type", description, "ddl" ) );

            description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( PARENT_REPEAT_COLUMNS, new ConfigurationValue( "Parent Repeat Columns", description, string.Empty ) );

            description = "The source of the values to display in a child list.  Format is either 'value1^parent1,value2^parent2,value3^parent3,...', 'value1^parent1^text1,value2^parent2^text2,value3^parent3^text3,...', or a SQL Select statement that returns result set with 'Value', 'ParentValue', and 'Text' columns <span class='tip tip-lava'></span>.";
            configurationValues.Add( CHILD_VALUES_KEY, new ConfigurationValue( "Child Values", description, string.Empty ) );

            description = "The type of control to use for selecting a single value from the child list.";
            configurationValues.Add( CHILD_FIELDTYPE_KEY, new ConfigurationValue( "Child Control Type", description, "ddl" ) );

            description = "Select how many columns the child list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( CHILD_REPEAT_COLUMNS, new ConfigurationValue( "Child Repeat Columns", description, string.Empty ) );

            if ( controls != null && controls.Count > 5 )
            {
                var tbParentValues = controls[0] as RockTextBox;
                var ddlParentFieldType = controls[1] as RockDropDownList;
                var tbParentRepeatColumns = controls[2] as NumberBox;

                var tbChildValues = controls[3] as RockTextBox;
                var ddlChildFieldType = controls[4] as RockDropDownList;
                var tbChildRepeatColumns = controls[5] as NumberBox;

                tbParentRepeatColumns.Visible = ddlParentFieldType.SelectedValue == "rb" || ddlParentFieldType.SelectedValue == "cb" ? true : false;
                tbChildRepeatColumns.Visible = ddlChildFieldType.SelectedValue == "rb" || ddlChildFieldType.SelectedValue == "cb" ? true : false;

                configurationValues[PARENT_VALUES_KEY].Value = tbParentValues.Text;
                configurationValues[PARENT_FIELDTYPE_KEY].Value = ddlParentFieldType.SelectedValue;
                configurationValues[PARENT_REPEAT_COLUMNS].Value = tbParentRepeatColumns.Visible ? tbParentRepeatColumns.Text : string.Empty;

                configurationValues[CHILD_VALUES_KEY].Value = tbChildValues.Text;
                configurationValues[CHILD_FIELDTYPE_KEY].Value = ddlChildFieldType.SelectedValue;
                configurationValues[CHILD_REPEAT_COLUMNS].Value = tbChildRepeatColumns.Visible ? tbChildRepeatColumns.Text : string.Empty;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && controls.Count > 5 && configurationValues != null )
            {
                var tbParentValues = controls[0] as RockTextBox;
                var ddlParentFieldType = controls[1] as RockDropDownList;
                var tbParentRepeatColumns = controls[2] as NumberBox;

                var tbChildValues = controls[3] as RockTextBox;
                var ddlChildFieldType = controls[4] as RockDropDownList;
                var tbChildRepeatColumns = controls[5] as NumberBox;

                tbParentValues.Text = configurationValues.ContainsKey( PARENT_VALUES_KEY ) ? configurationValues[PARENT_VALUES_KEY].Value : string.Empty;
                ddlParentFieldType.SelectedValue = configurationValues.ContainsKey( PARENT_FIELDTYPE_KEY ) ? configurationValues[PARENT_FIELDTYPE_KEY].Value : ddlParentFieldType.SelectedValue;
                tbParentRepeatColumns.Text = configurationValues.ContainsKey( PARENT_REPEAT_COLUMNS ) ? configurationValues[PARENT_REPEAT_COLUMNS].Value : string.Empty;
                tbParentRepeatColumns.Visible = ddlParentFieldType.SelectedValue == "rb" ? true : false;

                tbChildValues.Text = configurationValues.ContainsKey( CHILD_VALUES_KEY ) ? configurationValues[CHILD_VALUES_KEY].Value : string.Empty;
                ddlChildFieldType.SelectedValue = configurationValues.ContainsKey( CHILD_FIELDTYPE_KEY ) ? configurationValues[CHILD_FIELDTYPE_KEY].Value : ddlChildFieldType.SelectedValue;
                tbChildRepeatColumns.Text = configurationValues.ContainsKey( CHILD_REPEAT_COLUMNS ) ? configurationValues[CHILD_REPEAT_COLUMNS].Value : string.Empty;
                tbChildRepeatColumns.Visible = ddlChildFieldType.SelectedValue == "rb" ? true : false;
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            string parentValue = string.Empty;
            string childValue = string.Empty;

            string formattedParentValue = string.Empty;
            string formattedChildValue = string.Empty;

            string[] parts = ( value ?? string.Empty ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            if ( parts.Length > 0 )
            {
                parentValue = parts[0];
                if ( parts.Length > 1 )
                {
                    childValue = parts[1];
                }
            }

            if ( !string.IsNullOrWhiteSpace( parentValue ) && configurationValues.ContainsKey( PARENT_VALUES_KEY ) )
            {
                var configuredValues = Helper.GetConfiguredValues( configurationValues, PARENT_VALUES_KEY );
                var selectedValues = parentValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                formattedParentValue = configuredValues
                    .Where( v => selectedValues.Contains( v.Key ) )
                    .Select( v => v.Value )
                    .ToList()
                    .AsDelimited( ", " );
            }

            if ( !string.IsNullOrWhiteSpace( childValue ) && configurationValues.ContainsKey( CHILD_VALUES_KEY ) )
            {
                var configuredValues = GetConfiguredChildValues( configurationValues, CHILD_VALUES_KEY );
                var selectedValues = childValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                formattedChildValue = configuredValues
                    .Where( v => selectedValues.Contains( v.Key ) )
                    .Select( v => v.Value )
                    .ToList()
                    .AsDelimited( ", " );
            }

            formattedValue = string.Format( "{0}: {1}", formattedParentValue, formattedChildValue );

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            Panel pnlRange = new Panel { ID = id, CssClass = "form-control-group" };

            if ( configurationValues != null )
            {
                _parentEditControl = null;
                _childEditControl = null;

                string parentFieldType = configurationValues.ContainsKey( PARENT_FIELDTYPE_KEY ) ? configurationValues[PARENT_FIELDTYPE_KEY].Value : "ddl";
                string childFieldType = configurationValues.ContainsKey( CHILD_FIELDTYPE_KEY ) ? configurationValues[CHILD_FIELDTYPE_KEY].Value : "ddl";

                if ( parentFieldType == "rb" )
                {
                    _parentEditControl = new RockRadioButtonList { ID = string.Format( "{0}_parent", id ) };
                    ( ( RadioButtonList ) _parentEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                    if ( configurationValues.ContainsKey( PARENT_REPEAT_COLUMNS ) )
                    {
                        ( ( RadioButtonList ) _parentEditControl ).RepeatColumns = configurationValues[PARENT_REPEAT_COLUMNS].Value.AsInteger();
                    }
                }
                else if ( parentFieldType == "cb" )
                {
                    _parentEditControl = new RockCheckBoxList { ID = string.Format( "{0}_parent", id ) };
                    ( ( RockCheckBoxList ) _parentEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                    if ( configurationValues.ContainsKey( PARENT_REPEAT_COLUMNS ) )
                    {
                        ( ( RockCheckBoxList ) _parentEditControl ).RepeatColumns = configurationValues[PARENT_REPEAT_COLUMNS].Value.AsInteger();
                    }
                }
                else if ( parentFieldType == "ddl_multi_enhanced" )
                {
                    _parentEditControl = new RockListBox { ID = string.Format( "{0}_parent", id ) };
                    ( ( RockListBox ) _parentEditControl ).DisplayDropAsAbsolute = true;
                }
                else
                {
                    _parentEditControl = new RockDropDownList { ID = string.Format( "{0}_parent", id ) };
                    ( ( RockDropDownList ) _parentEditControl ).EnhanceForLongLists = parentFieldType == "ddl_single_enhanced";
                    ( ( RockDropDownList ) _parentEditControl ).DisplayEnhancedAsAbsolute = true;
                    _parentEditControl.Items.Add( new ListItem() );
                }

                pnlRange.Controls.Add( _parentEditControl );

                HtmlGenericControl lineBreak = new HtmlGenericControl( "br" );
                pnlRange.Controls.Add( lineBreak );

                if ( childFieldType == "rb" )
                {
                    _childEditControl = new RockRadioButtonList { ID = string.Format( "{0}_child", id ) };
                    ( ( RadioButtonList ) _childEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                    if ( configurationValues.ContainsKey( CHILD_REPEAT_COLUMNS ) )
                    {
                        ( ( RadioButtonList ) _childEditControl ).RepeatColumns = configurationValues[CHILD_REPEAT_COLUMNS].Value.AsInteger();
                    }
                }
                else if ( childFieldType == "cb" )
                {
                    _childEditControl = new RockCheckBoxList { ID = string.Format( "{0}_child", id ) };
                    ( ( RockCheckBoxList ) _childEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                    if ( configurationValues.ContainsKey( CHILD_REPEAT_COLUMNS ) )
                    {
                        ( ( RockCheckBoxList ) _childEditControl ).RepeatColumns = configurationValues[CHILD_REPEAT_COLUMNS].Value.AsInteger();
                    }
                }
                else if ( childFieldType == "ddl_multi_enhanced" )
                {
                    _childEditControl = new RockListBox { ID = string.Format( "{0}_child", id ) };
                    ( ( RockListBox ) _childEditControl ).DisplayDropAsAbsolute = true;
                }
                else
                {
                    _childEditControl = new RockDropDownList { ID = string.Format( "{0}_child", id ) };
                    ( ( RockDropDownList ) _childEditControl ).EnhanceForLongLists = childFieldType == "ddl_single_enhanced";
                    ( ( RockDropDownList ) _childEditControl ).DisplayEnhancedAsAbsolute = true;
                    _childEditControl.Items.Add( new ListItem() );
                }

                pnlRange.Controls.Add( _childEditControl );

                foreach ( var keyVal in Helper.GetConfiguredValues( configurationValues, PARENT_VALUES_KEY ) )
                {
                    _parentEditControl.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                }

                //foreach ( var keyVal in GetConfiguredChildValues( configurationValues, CHILD_VALUES_KEY ) )
                //{
                //    _childEditControl.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                //}

                _parentEditControl.AutoPostBack = true;
                _parentEditControl.SelectedIndexChanged += EditParentControl_SelectedIndexChanged;
                _configurationValues = configurationValues;
            }

            return pnlRange;

        }

        private void EditParentControl_SelectedIndexChanged( object sender, EventArgs e )
        {
            _childEditControl.Items.Clear();
            if ( _parentEditControl.SelectedValue.IsNotNullOrWhiteSpace() )
            {
                foreach ( var keyVal in GetConfiguredChildValues( _configurationValues, CHILD_VALUES_KEY, _parentEditControl.SelectedValue ) )
                {
                    _childEditControl.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                }
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Panel pnlRange = control as Panel;
            if ( pnlRange != null )
            {
                List<string> parentValues = new List<string>();
                List<string> childValues = new List<string>();

                ListControl parentControl = pnlRange.Controls.OfType<ListControl>().FirstOrDefault( a => a.ID.EndsWith( "_parent" ) );
                ListControl childControl = pnlRange.Controls.OfType<ListControl>().FirstOrDefault( a => a.ID.EndsWith( "_child" ) );

                foreach ( ListItem li in parentControl.Items )
                {
                    if ( li.Selected )
                    {
                        parentValues.Add( li.Value );
                    }
                }

                foreach ( ListItem li in childControl.Items )
                {
                    if ( li.Selected )
                    {
                        childValues.Add( li.Value );
                    }
                }
                return string.Format( "{0}|{1}", parentValues.AsDelimited<string>( "," ), childValues.AsDelimited<string>( "," ) );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            Panel pnlRange = control as Panel;
            if ( pnlRange != null )
            {

                ListControl parentControl = pnlRange.Controls.OfType<ListControl>().FirstOrDefault( a => a.ID.EndsWith( "_parent" ) );
                ListControl childControl = pnlRange.Controls.OfType<ListControl>().FirstOrDefault( a => a.ID.EndsWith( "_child" ) );

                if ( value != null )
                {
                    string[] valuePair = value.Split( new char[] { '|' }, StringSplitOptions.None );
                    if ( valuePair.Length == 2 )
                    {
                        if ( valuePair[0] != null )
                        {
                            List<string> values = new List<string>();
                            values.AddRange( valuePair[0].Split( ',' ) );

                            foreach ( ListItem li in parentControl.Items )
                            {
                                li.Selected = values.Contains( li.Value );
                            }

                        }

                        if ( valuePair[1] != null )
                        {
                            List<string> values = new List<string>();
                            values.AddRange( valuePair[1].Split( ',' ) );

                            foreach ( ListItem li in childControl.Items )
                            {
                                li.Selected = values.Contains( li.Value );
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

        /// <summary>
        /// Gets the filter compare control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            // This fieldtype does not support filtering
            return null;
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        public static Dictionary<string, string> GetConfiguredChildValues( Dictionary<string, ConfigurationValue> configurationValues, string propertyName, string parentValue = null )
        {
            var items = new Dictionary<string, string>();

            if ( configurationValues.ContainsKey( propertyName ) )
            {
                string listSource = configurationValues[propertyName].Value;

                var options = new Rock.Lava.CommonMergeFieldsOptions();
                options.GetLegacyGlobalMergeFields = false;
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, options );

                listSource = listSource.ResolveMergeFields( mergeFields );

                if ( listSource.ToUpper().Contains( "SELECT" ) && listSource.ToUpper().Contains( "FROM" ) )
                {
                    var tableValues = new List<string>();
                    DataTable dataTable = Rock.Data.DbService.GetDataTable( listSource, CommandType.Text, null );
                    if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "ParentValue" ) && dataTable.Columns.Contains( "Text" ) )
                    {
                        foreach ( DataRow row in dataTable.Rows )
                        {
                            if ( parentValue.IsNullOrWhiteSpace() || row["parentvalue"].ToString() == parentValue )
                            {
                                items.AddOrIgnore( row["value"].ToString(), row["text"].ToString() );
                            }
                        }
                    }
                }
                else
                {
                    foreach ( string keyvalue in listSource.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        var keyValueArray = keyvalue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( keyValueArray.Length > 1 )
                        {
                            if ( parentValue.IsNullOrWhiteSpace() || keyValueArray[1].Trim() == parentValue )
                            {
                                items.AddOrIgnore( keyValueArray[0].Trim(), keyValueArray.Length > 2 ? keyValueArray[2].Trim() : keyValueArray[0].Trim() );
                            }
                        }
                    }
                }
            }

            return items;
        }

    }
}
