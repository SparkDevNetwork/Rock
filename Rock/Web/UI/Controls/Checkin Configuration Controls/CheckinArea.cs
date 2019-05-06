// <copyright>
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
    [ToolboxData( "<{0}:CheckinArea runat=server></{0}:CheckinArea>" )]
    public class CheckinArea : CompositeControl
    {
        private Label _lblGroupTypeName;

        private DataTextBox _tbGroupTypeName;

        private RockDropDownList _ddlGroupTypeInheritFrom;
        private RockDropDownList _ddlAttendanceRule;
        private RockDropDownList _ddlPrintTo;

        private PlaceHolder _phGroupTypeAttributes;

        private Grid _gCheckinLabels;

        /// <summary>
        /// Gets the group type unique identifier.
        /// </summary>
        /// <value>
        /// The group type unique identifier.
        /// </value>
        public Guid GroupTypeGuid;

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            GroupTypeGuid = ViewState["GroupTypeGuid"] as Guid? ?? Guid.NewGuid();
            using ( var rockContext = new RockContext() )
            {
                var groupType = new GroupTypeService( rockContext ).Get( GroupTypeGuid );
                if ( groupType != null )
                {
                    groupType.InheritedGroupTypeId = ViewState["InheritedGroupTypeId"] as int?;
                    CreateGroupTypeAttributeControls( groupType, rockContext );
                }
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
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            EnsureChildControls();

            ViewState["GroupTypeGuid"] = GroupTypeGuid;
            ViewState["InheritedGroupTypeId"] = _ddlGroupTypeInheritFrom.SelectedValueAsId();

            return base.SaveViewState();
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
                var lblAdd = _gCheckinLabels.Actions.FindControl( lblAddControlId ) as LinkButton;
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
        /// Gets the type of the checkin group.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        public void GetGroupTypeValues( GroupType groupType )
        {
            EnsureChildControls();

            groupType.Name = _tbGroupTypeName.Text;
            groupType.InheritedGroupTypeId = _ddlGroupTypeInheritFrom.SelectedValueAsId();
            groupType.AttendanceRule = _ddlAttendanceRule.SelectedValueAsEnum<AttendanceRule>();
            groupType.AttendancePrintTo = _ddlPrintTo.SelectedValueAsEnum<PrintTo>();

            // Reload Attributes
            groupType.Attributes = null;
            groupType.LoadAttributes();

            Rock.Attribute.Helper.GetEditValues( _phGroupTypeAttributes, groupType );
        }

        /// <summary>
        /// Sets the type of the group.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetGroupType( GroupType groupType, RockContext rockContext )
        {
            EnsureChildControls();

            if ( groupType != null )
            {
                GroupTypeGuid = groupType.Guid;
                _tbGroupTypeName.Text = groupType.Name;
                _ddlGroupTypeInheritFrom.SetValue( groupType.InheritedGroupTypeId );
                _ddlAttendanceRule.SetValue( (int)groupType.AttendanceRule );
                _ddlPrintTo.SetValue( (int)groupType.AttendancePrintTo );

                CreateGroupTypeAttributeControls( groupType, rockContext );
            }
        }

        /// <summary>
        /// Gets the checkin label attribute keys.
        /// </summary>
        /// <param name="groupTypeAttribute">The group type attribute.</param>
        /// <returns></returns>
        public static Dictionary<string, Rock.Web.Cache.AttributeCache> GetCheckinLabelAttributes( Dictionary<string, Rock.Web.Cache.AttributeCache> groupTypeAttribute )
        {
            return groupTypeAttribute
                .Where( a => a.Value.FieldType.Guid.Equals( new Guid( Rock.SystemGuid.FieldType.LABEL ) ) )
                .ToDictionary( k => k.Key, v => v.Value );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _lblGroupTypeName = new Label();
            _lblGroupTypeName.ClientIDMode = ClientIDMode.Static;
            _lblGroupTypeName.ID = this.ID + "_lblGroupTypeName";

            _tbGroupTypeName = new DataTextBox();
            _tbGroupTypeName.ID = this.ID + "_tbGroupTypeName";
            _tbGroupTypeName.Label = "Check-in Area Name";

            // set label when they exit the edit field
            _tbGroupTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblGroupTypeName.ClientID );
            _tbGroupTypeName.SourceTypeName = "Rock.Model.GroupType, Rock";
            _tbGroupTypeName.PropertyName = "Name";

            _ddlGroupTypeInheritFrom = new RockDropDownList();
            _ddlGroupTypeInheritFrom.ID = this.ID + "_ddlGroupTypeInheritFrom";
            _ddlGroupTypeInheritFrom.Label = "Inherit from";

            _ddlGroupTypeInheritFrom.Items.Add( Rock.Constants.None.ListItem );
            var groupTypeCheckinFilterList = new GroupTypeService( new RockContext() ).Queryable()
                .Where( a => a.GroupTypePurposeValue.Guid == new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER ) )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name )
                .Select( a => new { a.Id, a.Name } ).ToList();

            foreach ( var groupType in groupTypeCheckinFilterList )
            {
                _ddlGroupTypeInheritFrom.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }
            _ddlGroupTypeInheritFrom.AutoPostBack = true;
            _ddlGroupTypeInheritFrom.SelectedIndexChanged += _ddlGroupTypeInheritFrom_SelectedIndexChanged;
            _ddlAttendanceRule = new RockDropDownList();
            _ddlAttendanceRule.ID = this.ID + "_ddlAttendanceRule";
            _ddlAttendanceRule.Label = "Check-in Rule";
            _ddlAttendanceRule.Help = "The rule that check in should use when a person attempts to check in to a group of this type.  If 'None' is selected, user will not be added to group and is not required to belong to group.  If 'Add On Check In' is selected, user will be added to group if they don't already belong.  If 'Already Belongs' is selected, user must already be a member of the group or they will not be allowed to check in.";
            _ddlAttendanceRule.BindToEnum<Rock.Model.AttendanceRule>();

            _ddlPrintTo = new RockDropDownList();
            _ddlPrintTo.ID = this.ID + "_ddlPrintTo";
            _ddlPrintTo.Label = "Print To";
            _ddlPrintTo.Help = "When printing check-in labels, should the device's printer or the location's printer be used?  Note: the device has a similar setting which takes precedence over this setting.";
            _ddlPrintTo.Items.Add( new ListItem( "Device Printer", "1" ) );
            _ddlPrintTo.Items.Add( new ListItem( "Location Printer", "2" ) );

            _phGroupTypeAttributes = new PlaceHolder();
            _phGroupTypeAttributes.ID = this.ID + "_phGroupTypeAttributes";

            Controls.Add( _lblGroupTypeName );
            Controls.Add( _ddlGroupTypeInheritFrom );
            Controls.Add( _ddlAttendanceRule );
            Controls.Add( _ddlPrintTo );
            Controls.Add( _tbGroupTypeName );
            Controls.Add( _phGroupTypeAttributes );

            // Check-in Labels grid
            CreateCheckinLabelsGrid();
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
            _gCheckinLabels.CssClass = "margin-b-md";
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
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Style, "margin-top:0;" );
                writer.RenderBeginTag( HtmlTextWriterTag.H3 );
                _lblGroupTypeName.Text = _tbGroupTypeName.Text;
                _lblGroupTypeName.RenderControl( writer );
                writer.RenderEndTag();

                _tbGroupTypeName.RenderControl( writer );
                _ddlGroupTypeInheritFrom.RenderControl( writer );
                _ddlAttendanceRule.RenderControl( writer );
                _ddlPrintTo.RenderControl( writer );

                _phGroupTypeAttributes.RenderControl( writer );

                writer.WriteLine( "<h3>Check-in Labels</h3>" );
                if ( this.CheckinLabels != null )
                {
                    _gCheckinLabels.DataSource = this.CheckinLabels;
                    _gCheckinLabels.DataBind();
                }
                _gCheckinLabels.RenderControl( writer );
            }
        }

        /// <summary>
        /// Creates the group type attribute controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="rockContext">The rock context.</param>
        public void CreateGroupTypeAttributeControls( GroupType groupType,  RockContext rockContext )
        {
            EnsureChildControls();

            _phGroupTypeAttributes.Controls.Clear();

            if ( groupType != null )
            {
                if ( groupType.Attributes == null )
                {
                    groupType.LoadAttributes( rockContext );
                }

                // exclude checkin labels 
                List<string> checkinLabelAttributeNames = GetCheckinLabelAttributes( groupType.Attributes ).Select( a => a.Value.Name ).ToList();
                Rock.Attribute.Helper.AddEditControls( groupType, _phGroupTypeAttributes, true, string.Empty, checkinLabelAttributeNames );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlGroupTypeInheritFrom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _ddlGroupTypeInheritFrom_SelectedIndexChanged( object sender, EventArgs e )
        {
            EnsureChildControls();
            using ( var rockContext = new RockContext() )
            {
                var groupType = new GroupTypeService( rockContext ).Get( GroupTypeGuid );
                if ( groupType != null )
                {
                    GetGroupTypeValues( groupType );
                    CreateGroupTypeAttributeControls( groupType, rockContext );
                }
            }
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
        /// Occurs when [delete checkin label click].
        /// </summary>
        public event EventHandler<RowEventArgs> DeleteCheckinLabelClick;

        /// <summary>
        /// Occurs when [add checkin label click].
        /// </summary>
        public event EventHandler AddCheckinLabelClick;
    }
}