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
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:CheckinGroupTypeEditor runat=server></{0}:CheckinGroupTypeEditor>" )]
    public class CheckinGroupTypeEditor : CompositeControl
    {
        private HiddenFieldWithClass _hfExpanded;
        private HiddenField _hfGroupTypeGuid;
        private HiddenField _hfGroupTypeId;
        private Label _lblGroupTypeName;
        private LinkButton _lbDeleteGroupType;

        private RockDropDownList _ddlGroupTypeInheritFrom;

        private DataTextBox _tbGroupTypeName;
        private PlaceHolder _phGroupTypeAttributes;
        private Grid _gCheckinLabels;

        private LinkButton _lbAddCheckinGroup;

        private LinkButton _lbAddCheckinGroupType;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="WorkflowActivityTypeEditor"/> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();
                return _hfExpanded.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// checkin-grouptype animation
$('.checkin-grouptype > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('i.checkin-grouptype-state', this).toggleClass('fa-chevron-down');
    $('i.checkin-grouptype-state', this).toggleClass('fa-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.checkin-grouptype a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.checkin-grouptype a.checkin-grouptype-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Add Sub-Area button will fire its event, but not the parent event 
$('.checkin-grouptype a.checkin-grouptype-add-sub-area').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Ad Check-in Group button will fire its event, but not the parent event 
$('.checkin-grouptype a.checkin-grouptype-add-checkin-group').click(function (event) {
    event.stopImmediatePropagation();
});

$('.checkin-grouptype > .panel-body').on('validation-error', function() {
    var $header = $(this).siblings('header');
    $(this).slideDown();

    $expanded = $header.children('input.filter-expanded');
    $expanded.val('True');

    $('i.checkin-grouptype-state', $header).removeClass('fa-chevron-down');
    $('i.checkin-grouptype-state', $header).addClass('fa-chevron-up');

    return false;
});

";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CheckinGroupTypeEditorScript", script, true );

            CreateGroupTypeAttributeControls( new RockContext() );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                HandleGridEvents();
            }
        }

        /// <summary>
        /// Handles the grid events.
        /// </summary>
        private void HandleGridEvents()
        {
            // manually wireup the grid events since they don't seem to do it automatically 
            string eventTarget = Page.Request.Params["__EVENTTARGET"];
            if ( eventTarget.StartsWith( _gCheckinLabels.UniqueID ) )
            {
                List<string> subTargetList = eventTarget.Replace( _gCheckinLabels.UniqueID, string.Empty ).Split( new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                EnsureChildControls();
                string lblAddControlId = subTargetList.Last();
                var lblAdd = _gCheckinLabels.Actions.FindControl( lblAddControlId );
                if ( lblAdd != null )
                {
                    AddCheckinLabel_Click( this, new EventArgs() );
                }
                else
                {
                    // rowIndex is determined by the numeric suffix of the control id after the Grid, subtract 2 (one for the header, and another to convert from 0 to 1 based index)
                    int rowIndex = subTargetList.First().AsNumeric().AsInteger() - 2;
                    RowEventArgs rowEventArgs = new RowEventArgs( rowIndex, this.CheckinLabels[rowIndex].AttributeKey );
                    DeleteCheckinLabel_Click( this, rowEventArgs );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is delete enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is delete enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleteEnabled
        {
            get
            {
                bool? b = ViewState["IsDeleteEnabled"] as bool?;
                return ( b == null ) ? true : b.Value;
            }

            set
            {
                ViewState["IsDeleteEnabled"] = value;
            }
        }

        /// <summary>
        /// special class for storing CheckinLabel attributes for the grid
        /// </summary>
        [Serializable]
        public class CheckinLabelAttributeInfo
        {
            /// <summary>
            /// Gets or sets the attribute key.
            /// </summary>
            /// <value>
            /// The attribute key.
            /// </value>
            [DataMember]
            public string AttributeKey { get; set; }

            /// <summary>
            /// Gets or sets the binary file unique identifier.
            /// </summary>
            /// <value>
            /// The binary file unique identifier.
            /// </value>
            [DataMember]
            public Guid BinaryFileGuid { get; set; }

            /// <summary>
            /// Gets or sets the name of the file.
            /// </summary>
            /// <value>
            /// The name of the file.
            /// </value>
            [DataMember]
            public string FileName { get; set; }
        }

        /// <summary>
        /// Gets or sets the checkin labels.
        /// Key is AttributeKey
        /// Value is BinaryFileName
        /// </summary>
        /// <value>
        /// The checkin labels.
        /// </value>
        public List<CheckinLabelAttributeInfo> CheckinLabels
        {
            get
            {
                return ViewState["CheckinLabels"] as List<CheckinLabelAttributeInfo>;
            }

            set
            {
                ViewState["CheckinLabels"] = value;
            }
        }

        /// <summary>
        /// Gets the group type unique identifier.
        /// </summary>
        /// <value>
        /// The group type unique identifier.
        /// </value>
        public Guid GroupTypeGuid
        {
            get
            {
                return new Guid( _hfGroupTypeGuid.Value );
            }
        }

        /// <summary>
        /// Gets the parent group type editor.
        /// </summary>
        /// <value>
        /// The parent group type editor.
        /// </value>
        public CheckinGroupTypeEditor ParentGroupTypeEditor
        {
            get
            {
                return this.Parent as CheckinGroupTypeEditor;
            }
        }

        /// <summary>
        /// Gets the inherited group type unique identifier.
        /// </summary>
        /// <value>
        /// The inherited group type unique identifier.
        /// </value>
        public int? InheritedGroupTypeId
        {
            get
            {
                return _ddlGroupTypeInheritFrom.SelectedValueAsId();
            }
        }

        /// <summary>
        /// Gets the type of the checkin group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public GroupType GetCheckinGroupType( RockContext rockContext )
        {
            EnsureChildControls();
            GroupType result = new GroupType();

            //// NOTE:  A GroupType that was added will have an Id of 0 since it hasn't been saved to the database. 
            //// So, we'll use Guid to uniquely identify in this Control since that'll work in both Saved and Unsaved cases.
            //// If it is saved, we do need the Id so that Attributes will work

            result.Id = _hfGroupTypeId.ValueAsInt();
            result.Guid = new Guid( _hfGroupTypeGuid.Value );

            result.Name = _tbGroupTypeName.Text;
            result.InheritedGroupTypeId = _ddlGroupTypeInheritFrom.SelectedValueAsId();

            result.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( _phGroupTypeAttributes, result );

            result.ChildGroupTypes = new List<GroupType>();
            int childGroupTypeOrder = 0;
            foreach ( CheckinGroupTypeEditor checkinGroupTypeEditor in this.Controls.OfType<CheckinGroupTypeEditor>() )
            {
                GroupType childGroupType = checkinGroupTypeEditor.GetCheckinGroupType( rockContext );
                childGroupType.Order = childGroupTypeOrder++;
                result.ChildGroupTypes.Add( childGroupType );
            }

            result.Groups = new List<Group>();
            int childGroupOrder = 0;
            foreach ( CheckinGroupEditor checkinGroupEditor in this.Controls.OfType<CheckinGroupEditor>() )
            {
                Group childGroup = checkinGroupEditor.GetGroup( rockContext );
                childGroup.Order = childGroupOrder++;
                result.Groups.Add( childGroup );
            }

            return result;
        }

        /// <summary>
        /// Sets the type of the group.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <param name="groupTypeName">Name of the group type.</param>
        /// <param name="inheritedGroupTypeId">The inherited group type identifier.</param>
        public void SetGroupType( int groupTypeId, Guid groupTypeGuid, string groupTypeName, int? inheritedGroupTypeId )
        {
            EnsureChildControls();
            _hfGroupTypeId.Value = groupTypeId.ToString();
            _hfGroupTypeGuid.Value = groupTypeGuid.ToString();
            _tbGroupTypeName.Text = groupTypeName;
            _ddlGroupTypeInheritFrom.SetValue( inheritedGroupTypeId );
        }

        /// <summary>
        /// Gets the checkin label attribute keys.
        /// </summary>
        /// <param name="groupTypeAttribute">The group type attribute.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Dictionary<string, Rock.Web.Cache.AttributeCache> GetCheckinLabelAttributes( Dictionary<string, Rock.Web.Cache.AttributeCache> groupTypeAttribute, RockContext rockContext )
        {
            return groupTypeAttribute
                .Where( a => a.Value.FieldType.Guid.Equals( new Guid( Rock.SystemGuid.FieldType.BINARY_FILE ) ) )
                .Where( a => a.Value.QualifierValues.ContainsKey( "binaryFileType" ) )
                .Where( a => a.Value.QualifierValues["binaryFileType"].Value.Equals( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL, StringComparison.OrdinalIgnoreCase ) )
                .ToDictionary( k => k.Key, v => v.Value );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfExpanded = new HiddenFieldWithClass();
            Controls.Add( _hfExpanded );
            _hfExpanded.ID = this.ID + "_hfExpanded";
            _hfExpanded.CssClass = "filter-expanded";
            _hfExpanded.Value = "False";

            _hfGroupTypeGuid = new HiddenField();
            _hfGroupTypeGuid.ID = this.ID + "_hfGroupTypeGuid";

            _hfGroupTypeId = new HiddenField();
            _hfGroupTypeId.ID = this.ID + "_hfGroupTypeId";

            _lblGroupTypeName = new Label();
            _lblGroupTypeName.ClientIDMode = ClientIDMode.Static;
            _lblGroupTypeName.ID = this.ID + "_lblGroupTypeName";

            _lbDeleteGroupType = new LinkButton();
            _lbDeleteGroupType.CausesValidation = false;
            _lbDeleteGroupType.ID = this.ID + "_lbDeleteGroupType";
            _lbDeleteGroupType.CssClass = "btn btn-xs btn-danger";
            _lbDeleteGroupType.Click += lbDeleteGroupType_Click;
            _lbDeleteGroupType.Controls.Add( new LiteralControl { Text = "<i class='fa fa-times'></i>" } );
            _lbDeleteGroupType.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', '{1}');", "check-in area", "Once saved, you will lose all attendance data." );

            _ddlGroupTypeInheritFrom = new RockDropDownList();
            _ddlGroupTypeInheritFrom.ID = this.ID + "_ddlGroupTypeInheritFrom";
            _ddlGroupTypeInheritFrom.Label = "Inherit from";
            _ddlGroupTypeInheritFrom.AutoPostBack = true;
            _ddlGroupTypeInheritFrom.SelectedIndexChanged += ddlGroupTypeInheritFrom_SelectedIndexChanged;

            _ddlGroupTypeInheritFrom.Items.Add( Rock.Constants.None.ListItem );
            var groupTypeCheckinFilterList = new GroupTypeService( new RockContext() ).Queryable()
                .Where( a => a.GroupTypePurposeValue.Guid == new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER ) )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name )
                .Select( a => new { a.Id, a.Name } ).ToList();

            foreach ( var groupType in groupTypeCheckinFilterList )
            {
                _ddlGroupTypeInheritFrom.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }

            _tbGroupTypeName = new DataTextBox();
            _tbGroupTypeName.ID = this.ID + "_tbGroupTypeName";
            _tbGroupTypeName.Label = "Check-in Area Name";

            // set label when they exit the edit field
            _tbGroupTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblGroupTypeName.ClientID );
            _tbGroupTypeName.SourceTypeName = "Rock.Model.GroupType, Rock";
            _tbGroupTypeName.PropertyName = "Name";

            _phGroupTypeAttributes = new PlaceHolder();
            _phGroupTypeAttributes.ID = this.ID + "_phGroupTypeAttributes";

            _lbAddCheckinGroupType = new LinkButton();
            _lbAddCheckinGroupType.ID = this.ID + "_lblbAddCheckinGroupType";
            _lbAddCheckinGroupType.CssClass = "btn btn-xs btn-action checkin-grouptype-add-sub-area";
            _lbAddCheckinGroupType.Click += lbAddCheckinGroupType_Click;
            _lbAddCheckinGroupType.CausesValidation = false;
            _lbAddCheckinGroupType.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> Add Sub-Area" } );

            _lbAddCheckinGroup = new LinkButton();
            _lbAddCheckinGroup.ID = this.ID + "_lbAddCheckinGroup";
            _lbAddCheckinGroup.CssClass = "btn btn-xs btn-action checkin-grouptype-add-checkin-group";
            _lbAddCheckinGroup.Click += lbAddGroup_Click;
            _lbAddCheckinGroup.CausesValidation = false;
            _lbAddCheckinGroup.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> Add Check-in Group" } );

            Controls.Add( _hfGroupTypeGuid );
            Controls.Add( _hfGroupTypeId );
            Controls.Add( _lblGroupTypeName );
            Controls.Add( _ddlGroupTypeInheritFrom );
            Controls.Add( _tbGroupTypeName );
            Controls.Add( _phGroupTypeAttributes );

            // Check-in Labels grid
            CreateCheckinLabelsGrid();

            Controls.Add( _lbDeleteGroupType );
            Controls.Add( _lbAddCheckinGroupType );
            Controls.Add( _lbAddCheckinGroup );
        }

        /// <summary>
        /// Creates the checkin labels grid.
        /// </summary>
        private void CreateCheckinLabelsGrid()
        {
            _gCheckinLabels = new Grid();

            // make the ID static so we can handle Postbacks from the Add and Delete actions
            _gCheckinLabels.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _gCheckinLabels.ID = this.ClientID + "_gCheckinLabels";
            _gCheckinLabels.DisplayType = GridDisplayType.Light;
            _gCheckinLabels.ShowActionRow = true;
            _gCheckinLabels.RowItemText = "Label";
            _gCheckinLabels.Actions.ShowAdd = true;

            //// Handle AddClick manually in OnLoad()
            //// gCheckinLabels.Actions.AddClick += AddCheckinLabel_Click;

            _gCheckinLabels.DataKeyNames = new string[] { "AttributeKey" };
            _gCheckinLabels.Columns.Add( new BoundField { DataField = "BinaryFileId", Visible = false } );
            _gCheckinLabels.Columns.Add( new BoundField { DataField = "FileName", HeaderText = "Name" } );

            DeleteField deleteField = new DeleteField();

            //// handle manually in OnLoad()
            //// deleteField.Click += DeleteCheckinLabel_Click;

            _gCheckinLabels.Columns.Add( deleteField );

            Controls.Add( _gCheckinLabels );
        }

        /// <summary>
        /// Handles the Click event of the DeleteCheckinLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void DeleteCheckinLabel_Click( object sender, RowEventArgs e )
        {
            if ( DeleteCheckinLabelClick != null )
            {
                DeleteCheckinLabelClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the AddCheckinLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddCheckinLabel_Click( object sender, EventArgs e )
        {
            if ( AddCheckinLabelClick != null )
            {
                AddCheckinLabelClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupTypeInheritFrom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupTypeInheritFrom_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.Expanded = true;

            using ( var rockContext = new RockContext() )
            {
                foreach ( var groupEditor in this.ControlsOfTypeRecursive<CheckinGroupEditor>().ToList() )
                {
                    Group group = groupEditor.GetGroup( rockContext );
                    groupEditor.CreateGroupAttributeControls( group, rockContext );
                }
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget checkin-grouptype" );
            writer.AddAttribute( "data-key", _hfGroupTypeGuid.Value );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix clickable" );
            writer.RenderBeginTag( "header" );

            // Hidden Field to track expansion
            _hfExpanded.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toggle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-title" );
            writer.RenderBeginTag( HtmlTextWriterTag.H3 );
            _lblGroupTypeName.Text = _tbGroupTypeName.Text;
            _lblGroupTypeName.RenderControl( writer );

            // H3 tag
            writer.RenderEndTag();

            // Name/Description div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right panel-actions" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( "<a class='btn btn-link btn-xs checkin-grouptype-reorder'><i class='fa fa-bars'></i></a>" );
            writer.WriteLine( string.Format( "<a class='btn btn-xs btn-link'><i class='checkin-grouptype-state fa {0}'></i></a>",
                Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" ) );

            if ( IsDeleteEnabled )
            {
                _lbDeleteGroupType.Visible = true;
                _lbDeleteGroupType.RenderControl( writer );
            }
            else
            {
                _lbDeleteGroupType.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right panel-actions btn-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _lbAddCheckinGroupType.RenderControl( writer );
            writer.WriteLine();
            _lbAddCheckinGroup.RenderControl( writer );
            writer.WriteLine();

            writer.RenderEndTag();

            // header div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );

            if ( !Expanded )
            {
                writer.AddStyleAttribute( "display", "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // make two span6 columns: Left Column for Name and Attributes. Right Column for Labels Grid
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // grouptype edit fields
            _tbGroupTypeName.RenderControl( writer );
            _ddlGroupTypeInheritFrom.RenderControl( writer );

            // attributes 
            _phGroupTypeAttributes.RenderControl( writer );

            writer.RenderEndTag();
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Check-in Labels grid
            writer.WriteLine( "<h3>Check-in Labels</h3>" );
            _gCheckinLabels.DataSource = this.CheckinLabels;
            _gCheckinLabels.DataBind();
            _gCheckinLabels.RenderControl( writer );

            // span6
            writer.RenderEndTag();

            // rowfluid
            writer.RenderEndTag();

            // groups
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "checkin-grouptype-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            foreach ( CheckinGroupTypeEditor checkinGroupTypeEditor in this.Controls.OfType<CheckinGroupTypeEditor>() )
            {
                checkinGroupTypeEditor.RenderControl( writer );
            }

            // checkin-grouptype-list div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "checkin-group-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            foreach ( CheckinGroupEditor checkinGroupEditor in this.Controls.OfType<CheckinGroupEditor>() )
            {
                checkinGroupEditor.RenderControl( writer );
            }

            // checkin-group-list div
            writer.RenderEndTag();

            // widget-content div
            writer.RenderEndTag();

            // section tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Creates the group type attribute controls.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void CreateGroupTypeAttributeControls( RockContext rockContext )
        {
            // make a fakeGroupType to use to get the Attribute Controls based on GroupType id and InheritedGroupTypeId
            GroupType fakeGroupType = new GroupType();
            fakeGroupType.Id = _hfGroupTypeId.ValueAsInt();
            fakeGroupType.InheritedGroupTypeId = this.InheritedGroupTypeId;

            fakeGroupType.LoadAttributes( rockContext );
            _phGroupTypeAttributes.Controls.Clear();

            // exclude checkin labels 
            List<string> checkinLabelAttributeNames = GetCheckinLabelAttributes( fakeGroupType.Attributes, rockContext ).Select( a => a.Value.Name ).ToList();
            Rock.Attribute.Helper.AddEditControls( fakeGroupType, _phGroupTypeAttributes, true, string.Empty, checkinLabelAttributeNames );
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteGroupType_Click( object sender, EventArgs e )
        {
            if ( DeleteGroupTypeClick != null )
            {
                DeleteGroupTypeClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddGroup_Click( object sender, EventArgs e )
        {
            if ( AddGroupClick != null )
            {
                AddGroupClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddCheckinGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCheckinGroupType_Click( object sender, EventArgs e )
        {
            if ( AddGroupTypeClick != null )
            {
                AddGroupTypeClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete group type click].
        /// </summary>
        public event EventHandler DeleteGroupTypeClick;

        /// <summary>
        /// Occurs when [add group click].
        /// </summary>
        public event EventHandler AddGroupClick;

        /// <summary>
        /// Occurs when [add group type click].
        /// </summary>
        public event EventHandler AddGroupTypeClick;

        /// <summary>
        /// Occurs when [delete checkin label click].
        /// </summary>
        public event EventHandler<RowEventArgs> DeleteCheckinLabelClick;

        /// <summary>
        /// Occurs when [add checkin label click].
        /// </summary>
        public event EventHandler AddCheckinLabelClick;
    }
}