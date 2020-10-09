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
                EnsureChildControls();
                return _childEditControl.Required;
            }
            set
            {
                EnsureChildControls();
                _parentEditControl.Required = value;
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
                EnsureChildControls();
                return _parentEditControl.SelectedValue;
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
                EnsureChildControls();
                return _childEditControl.SelectedValue;
            }

            set
            {
                EnsureChildControls();
                string childValue = value;
                if ( _childEditControl.SelectedValue != childValue )
                {
                    if ( SelectedParentValue.IsNullOrWhiteSpace() )
                    {
                        var parentValue = GetParentValueFromChild( childValue );
                        if ( _parentEditControl.SelectedValue != parentValue )
                        {
                            _parentEditControl.SelectedValue = parentValue;
                            LoadChildValues( parentValue );
                        }
                    }

                    _childEditControl.SelectedValue = childValue;
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

        /// <summary>
        /// Gets or sets the group control label.
        /// </summary>
        /// <value>
        /// The group control label.
        /// </value>
        public string GroupControlLabel
        {
            get
            {
                return ( ViewState["GroupControlLabel"] as string ) ?? "Group";
            }

            set
            {
                ViewState["GroupControlLabel"] = value;
            }
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

            _parentEditControl.ID = this.ID + "_parentEditControl";
            _parentEditControl.AutoPostBack = true;
            _parentEditControl.SelectedIndexChanged += _parentEditControl_SelectedIndexChanged;
            if ( parentFieldType == "rb" )
            {
                _parentEditControl = new RockRadioButtonList();
                ( ( RadioButtonList ) _parentEditControl ).RepeatDirection = RepeatDirection.Horizontal;

                if ( ParentRepeatColumns.IsNotNullOrWhiteSpace() )
                {
                    ( ( RadioButtonList ) _parentEditControl ).RepeatColumns = ParentRepeatColumns.AsInteger();
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
            Controls.Add( _parentEditControl );

            string childFieldType = ChildFieldType ?? "ddl";

            _childEditControl.ID = this.ID + "_childEditControl";

            if ( childFieldType == "rb" )
            {
                _childEditControl = new RockRadioButtonList();
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
            Controls.Add( _childEditControl );

            LoadGroupTypes();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _parentEditControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _parentEditControl_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupTypeId = _parentEditControl.SelectedValue.AsInteger();
            LoadGroups( groupTypeId );
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
            _childEditControl.Visible = GroupTypeId.HasValue;
            _parentEditControl.RenderControl( writer );
            _childEditControl.Label = this.GroupControlLabel;
            _childEditControl.RenderControl( writer );
        }

        /// <summary>
        /// Loads the group types.
        /// </summary>
        private void LoadGroupTypes()
        {
            _parentEditControl.Items.Clear();
            _parentEditControl.Items.Add( Rock.Constants.None.ListItem );

            var groupTypeService = new Rock.Model.GroupTypeService( new RockContext() );

            // get all group types that have the ShowInGroupList flag set
            var groupTypes = groupTypeService.Queryable().Where( a => a.ShowInGroupList ).OrderBy( a => a.Name ).ToList();

            foreach ( var g in groupTypes )
            {
                _parentEditControl.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) );
            }
        }

        /// <summary>
        /// Loads the groups.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        private void LoadChildValues( string selectedParentValue )
        {
            int? currentGroupId = this.GroupId;
            _childEditControl.SelectedValue = null;
            _childEditControl.Items.Clear();
            if ( groupTypeId.HasValue )
            {
                _childEditControl.Items.Add( Rock.Constants.None.ListItem );

                var groupService = new Rock.Model.GroupService( new RockContext() );
                var groups = groupService.Queryable().Where( r => r.GroupTypeId == groupTypeId.Value ).OrderBy( a => a.Name ).ToList();

                foreach ( var r in groups )
                {
                    var item = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    item.Selected = r.Id == currentGroupId;
                    _childEditControl.Items.Add( item );
                }
            }
        }

        public Dictionary<string, string> GetFilteredChildValues( string parentValue = null )
        {
            var items = new Dictionary<string, string>();

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
                            parentValue = keyValueArray[1].Trim()
                        }
                    }
                }
            }

            return parentValue;
        }
    }
}