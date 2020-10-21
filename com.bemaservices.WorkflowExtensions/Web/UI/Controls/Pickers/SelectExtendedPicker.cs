// <copyright>
// Copyright by BEMA Software Services
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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Web.UI.Controls;

namespace com.bemaservices.WorkflowExtensions.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a group type and then a group of that group type
    /// </summary>
    public class SelectExtendedPicker : CompositeControl, IRockControl
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
                bool required = false;
                EnsureChildControls();

                string childFieldType = ChildFieldType ?? "ddl";

                if ( childFieldType == "rb" )
                {
                    required = ( ( RockRadioButtonList ) _childEditControl ).Required;
                }
                else if ( childFieldType == "cb" )
                {
                    required = ( ( RockCheckBoxList ) _childEditControl ).Required;
                }
                else if ( childFieldType == "ddl_multi_enhanced" )
                {
                    required = ( ( RockListBox ) _childEditControl ).Required;
                }
                else
                {
                    required = ( ( RockDropDownList ) _childEditControl ).Required;
                }

                return required;
            }
            set
            {
                EnsureChildControls();

                string childFieldType = ChildFieldType ?? "ddl";

                if ( childFieldType == "rb" )
                {
                    ( ( RockRadioButtonList ) _childEditControl ).Required = value;
                }
                else if ( childFieldType == "cb" )
                {
                    ( ( RockCheckBoxList ) _childEditControl ).Required = value;
                }
                else if ( childFieldType == "ddl_multi_enhanced" )
                {
                    ( ( RockListBox ) _childEditControl ).Required = value;
                }
                else
                {
                    ( ( RockDropDownList ) _childEditControl ).Required = value;
                }
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

        private ListControl _parentEditControl;
        private ListControl _childEditControl;

        #endregion

        #region Properties

        public string SelectedParentValue
        {
            get
            {
                List<string> selectedValues = new List<string>();
                EnsureChildControls();

                string parentFieldType = ParentFieldType ?? "ddl";

                if ( parentFieldType == "rb" )
                {
                    selectedValues.Add( ( ( RockRadioButtonList ) _parentEditControl ).SelectedValue );
                }
                else if ( parentFieldType == "cb" )
                {
                    selectedValues = ( ( RockCheckBoxList ) _parentEditControl ).SelectedValues;
                }
                else if ( parentFieldType == "ddl_multi_enhanced" )
                {
                    selectedValues = ( ( RockListBox ) _parentEditControl ).SelectedValues;
                }
                else
                {
                    selectedValues.Add( ( ( RockDropDownList ) _parentEditControl ).SelectedValue );
                }
                return selectedValues.AsDelimited( "," );
            }

            set
            {
                EnsureChildControls();
                _parentEditControl.SetValue( value );
                LoadChildValues( value );
            }
        }

        public string ParentValues
        {
            get { return ViewState["ParentValues"] as string; }
            set { ViewState["ParentValues"] = value; }
        }
        public string ParentFieldType
        {
            get { return ViewState["ParentFieldType"] as string; }
            set { ViewState["ParentFieldType"] = value; }
        }
        public string ParentRepeatColumns
        {
            get { return ViewState["ParentRepeatColumns"] as string; }
            set { ViewState["ParentRepeatColumns"] = value; }
        }

        public string SelectedChildValue
        {
            get
            {
                List<string> selectedValues = new List<string>();
                EnsureChildControls();

                string childFieldType = ChildFieldType ?? "ddl";

                if ( childFieldType == "rb" )
                {
                    selectedValues.Add( ( ( RockRadioButtonList ) _childEditControl ).SelectedValue );
                }
                else if ( childFieldType == "cb" )
                {
                    selectedValues = ( ( RockCheckBoxList ) _childEditControl ).SelectedValues;
                }
                else if ( childFieldType == "ddl_multi_enhanced" )
                {
                    selectedValues = ( ( RockListBox ) _childEditControl ).SelectedValues;
                }
                else
                {
                    selectedValues.Add( ( ( RockDropDownList ) _childEditControl ).SelectedValue );
                }
                return selectedValues.AsDelimited( "," );
            }

            set
            {
                EnsureChildControls();
                var childValues = value.SplitDelimitedValues();
                if ( SelectedChildValue != value )
                {
                    if ( SelectedParentValue.IsNullOrWhiteSpace() )
                    {
                        var parentValue = GetParentValueFromChild( childValues.FirstOrDefault() );
                        if ( _parentEditControl.SelectedValue != parentValue )
                        {
                            _parentEditControl.SelectedValue = parentValue;
                            LoadChildValues( parentValue );
                        }
                    }

                    string childFieldType = ChildFieldType ?? "ddl";

                    if ( childFieldType == "rb" )
                    {
                        ( ( RockRadioButtonList ) _childEditControl ).SelectedValue = value;
                    }
                    else if ( childFieldType == "cb" )
                    {
                        ( ( RockCheckBoxList ) _childEditControl ).SetValues(childValues);
                    }
                    else if ( childFieldType == "ddl_multi_enhanced" )
                    {
                        ( ( RockListBox ) _childEditControl ).SetValues( childValues );
                    }
                    else
                    {
                        ( ( RockDropDownList ) _childEditControl ).SelectedValue = value;
                    }
                }
            }
        }

        public string ChildValues
        {
            get { return ViewState["ChildValues"] as string; }
            set { ViewState["ChildValues"] = value; }
        }
        public string ChildFieldType
        {
            get { return ViewState["ChildFieldType"] as string; }
            set { ViewState["ChildFieldType"] = value; }
        }
        public string ChildRepeatColumns
        {
            get { return ViewState["ChildRepeatColumns"] as string; }
            set { ViewState["ChildRepeatColumns"] = value; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeGroupPicker"/> class.
        /// </summary>
        public SelectExtendedPicker()
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

            string parentFieldType = ParentFieldType ?? "ddl";
            if ( parentFieldType == "rb" )
            {
                _parentEditControl = new RockRadioButtonList();
                ( ( RockRadioButtonList ) _parentEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                if ( ParentRepeatColumns.IsNotNullOrWhiteSpace() )
                {
                    ( ( RockRadioButtonList ) _parentEditControl ).RepeatColumns = ParentRepeatColumns.AsInteger();
                }
            }
            else if ( parentFieldType == "cb" )
            {
                _parentEditControl = new RockCheckBoxList();
                ( ( RockCheckBoxList ) _parentEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                if ( ParentRepeatColumns.IsNotNullOrWhiteSpace() )
                {
                    ( ( RockCheckBoxList ) _parentEditControl ).RepeatColumns = ParentRepeatColumns.AsInteger();
                }
            }
            else if ( parentFieldType == "ddl_multi_enhanced" )
            {
                _parentEditControl = new RockListBox();
                ( ( RockListBox ) _parentEditControl ).DisplayDropAsAbsolute = true;
            }
            else
            {
                _parentEditControl = new RockDropDownList();
                ( ( RockDropDownList ) _parentEditControl ).EnhanceForLongLists = parentFieldType == "ddl_single_enhanced";
                ( ( RockDropDownList ) _parentEditControl ).DisplayEnhancedAsAbsolute = true;
                _parentEditControl.Items.Add( new ListItem() );
            }

            _parentEditControl.ID = this.ID + "_parentEditControl";
            _parentEditControl.AutoPostBack = true;
            _parentEditControl.SelectedIndexChanged += _parentEditControl_SelectedIndexChanged;
            Controls.Add( _parentEditControl );

            string childFieldType = ChildFieldType ?? "ddl";
            if ( childFieldType == "rb" )
            {
                _childEditControl = new RockRadioButtonList();
                ( ( RockRadioButtonList ) _childEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                if ( ChildRepeatColumns.IsNotNullOrWhiteSpace() )
                {
                    ( ( RockRadioButtonList ) _childEditControl ).RepeatColumns = ChildRepeatColumns.AsInteger();
                }
            }
            else if ( childFieldType == "cb" )
            {
                _childEditControl = new RockCheckBoxList();
                ( ( RockCheckBoxList ) _childEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                if ( ChildRepeatColumns.IsNotNullOrWhiteSpace() )
                {
                    ( ( RockCheckBoxList ) _childEditControl ).RepeatColumns = ChildRepeatColumns.AsInteger();
                }
            }
            else if ( childFieldType == "ddl_multi_enhanced" )
            {
                _childEditControl = new RockListBox();
                ( ( RockListBox ) _childEditControl ).DisplayDropAsAbsolute = true;
            }
            else
            {
                _childEditControl = new RockDropDownList();
                ( ( RockDropDownList ) _childEditControl ).EnhanceForLongLists = childFieldType == "ddl_single_enhanced";
                ( ( RockDropDownList ) _childEditControl ).DisplayEnhancedAsAbsolute = true;
                _childEditControl.Items.Add( new ListItem() );
            }

            _childEditControl.ID = this.ID + "_childEditControl";
            Controls.Add( _childEditControl );

            LoadParentValues();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _parentEditControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _parentEditControl_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadChildValues( _parentEditControl.SelectedValue );
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
            _childEditControl.Visible = SelectedParentValue.IsNotNullOrWhiteSpace();
            _parentEditControl.Style.Add( "margin-bottom", "15px" );
            _parentEditControl.RenderControl( writer );
            _childEditControl.RenderControl( writer );
        }

        /// <summary>
        /// Loads the parent values
        /// </summary>
        private void LoadParentValues()
        {
            _parentEditControl.Items.Clear();
            _parentEditControl.Items.Add( Rock.Constants.None.ListItem );

            var items = new Dictionary<string, string>();
            var options = new Rock.Lava.CommonMergeFieldsOptions();
            options.GetLegacyGlobalMergeFields = false;
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, options );

            string listSource = ParentValues.ResolveMergeFields( mergeFields );

            if ( listSource.ToUpper().Contains( "SELECT" ) && listSource.ToUpper().Contains( "FROM" ) )
            {
                var tableValues = new List<string>();
                DataTable dataTable = Rock.Data.DbService.GetDataTable( listSource, CommandType.Text, null );
                if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                {
                    foreach ( DataRow row in dataTable.Rows )
                    {
                        items.AddOrIgnore( row["value"].ToString(), row["text"].ToString() );
                    }
                }
            }

            else
            {
                foreach ( string keyvalue in listSource.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    var keyValueArray = keyvalue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( keyValueArray.Length > 0 )
                    {
                        items.AddOrIgnore( keyValueArray[0].Trim(), keyValueArray.Length > 1 ? keyValueArray[1].Trim() : keyValueArray[0].Trim() );
                    }
                }
            }

            foreach ( var item in items )
            {
                _parentEditControl.Items.Add( new ListItem( item.Value, item.Key ) );
            }
        }

        private void LoadChildValues( string selectedParentValue )
        {
            string currentChildValue = this.SelectedChildValue;
            _childEditControl.SelectedValue = null;
            _childEditControl.Items.Clear();
            if ( selectedParentValue.IsNotNullOrWhiteSpace() )
            {
                // _childEditControl.Items.Add( Rock.Constants.None.ListItem );

                foreach ( var keyVal in GetFilteredChildValues( ChildValues, selectedParentValue ) )
                {

                    var item = new ListItem( keyVal.Value, keyVal.Key );
                    item.Selected = keyVal.Key == currentChildValue;
                    _childEditControl.Items.Add( item );
                }
            }
        }

        public Dictionary<string, string> GetFilteredChildValues( string childValues, string parentValue = null )
        {
            var items = new Dictionary<string, string>();
            var options = new Rock.Lava.CommonMergeFieldsOptions();
            options.GetLegacyGlobalMergeFields = false;
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, options );

            string listSource = childValues.ResolveMergeFields( mergeFields );

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

            return items;
        }

        public string GetParentValueFromChild( string childValue )
        {
            var parentValue = String.Empty;

            string listSource = ChildValues;

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
                        if ( childValue.IsNullOrWhiteSpace() || row["childvalue"].ToString() == childValue )
                        {
                            parentValue = row["parentvalue"].ToString();
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
                        if ( childValue.IsNullOrWhiteSpace() || keyValueArray[0].Trim() == childValue )
                        {
                            parentValue = keyValueArray[1].Trim();
                        }
                    }
                }
            }

            return parentValue;
        }
    }
}