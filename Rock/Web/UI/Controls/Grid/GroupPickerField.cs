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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for editing a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:GroupPickerField runat=server></{0}:GroupPickerField>" )]
    public class GroupPickerField : RockTemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Gets or sets the root group identifier.
        /// </summary>
        /// <value>
        /// The root group identifier.
        /// </value>
        public int? RootGroupId
        {
            get
            {
                return ViewState["RootGroupId"] as int?;
            }
            set
            {
                ViewState["RootGroupId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the column.
        /// </summary>
        /// <value>
        /// The index of the column.
        /// </value>
        public int ColumnIndex
        {
            get
            {
                return ViewState["ColumnIndex"] as int? ?? 0;
            }
            set
            {
                ViewState["ColumnIndex"] = value;
            }
        }
        
        /// <summary>
        /// When exporting a grid with an Export source of ColumnOutput, this property controls whether a column is included
        /// in the export or not
        /// </summary>
        public override ExcelExportBehavior ExcelExportBehavior
        {
            get
            {
                return ExcelExportBehavior.NeverInclude;
            }
            set
            {
                base.ExcelExportBehavior = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPickerField" /> class.
        /// </summary>
        public GroupPickerField()
            : base()
        {
        }

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            GroupPickerFieldTemplate groupPickerFieldTemplate = new GroupPickerFieldTemplate();
            groupPickerFieldTemplate.GroupPickerSelect += groupPickerFieldTemplate_GroupSelect;
            this.ItemTemplate = groupPickerFieldTemplate;
            
            ParentGrid = control as Grid;
            if ( ParentGrid != null )
            {
                ColumnIndex = ParentGrid.GetColumnIndex( this );
            }

            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the GroupSelect event of the groupPickerFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GroupPickerRowEventArgs"/> instance containing the event data.</param>
        void groupPickerFieldTemplate_GroupSelect( object sender, GroupPickerRowEventArgs e )
        {
            OnGroupSelect( e );
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

        /// <summary>
        /// Occurs when [group select].
        /// </summary>
        public event EventHandler<GroupPickerRowEventArgs> GroupSelect;

        /// <summary>
        /// Raises the <see cref="E:GroupSelect" /> event.
        /// </summary>
        /// <param name="e">The <see cref="GroupPickerRowEventArgs"/> instance containing the event data.</param>
        public virtual void OnGroupSelect( GroupPickerRowEventArgs e )
        {
            if ( GroupSelect != null )
                GroupSelect( this, e );
        }
    }

    /// <summary>
    /// Template used by the <see cref="GroupPickerField"/> control
    /// </summary>
    public class GroupPickerFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets the index of the column.
        /// </summary>
        /// <value>
        /// The index of the column.
        /// </value>
        public int ColumnIndex { get; private set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                GroupPickerField groupPickerField = cell.ContainingField as GroupPickerField;
                ParentGrid = groupPickerField.ParentGrid;
                ColumnIndex = groupPickerField.ColumnIndex;

                GroupPicker gp = new GroupPicker();
                gp.ID = "groupPicker_" + ColumnIndex.ToString();
                gp.RootGroupId = groupPickerField.RootGroupId;
                gp.SelectItem += gp_SelectItem;
                cell.Controls.Add( gp );
            }
        }

        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        private Grid ParentGrid { get; set; }

        /// <summary>
        /// Handles the SelectItem event of the gp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gp_SelectItem( object sender, EventArgs e )
        {
            if ( GroupPickerSelect != null  )
            {
                GroupPicker gp = sender as GroupPicker;
                if ( gp != null )
                {
                    GridViewRow row = (GridViewRow)( (LinkButton)sender ).Parent.Parent;
                    GroupPickerRowEventArgs args = new GroupPickerRowEventArgs( row, gp.SelectedValueAsInt() );
                    GroupPickerSelect( sender, args );
                }
            }
        }

        /// <summary>
        /// Occurs when [link button click].
        /// </summary>
        internal event EventHandler<GroupPickerRowEventArgs> GroupPickerSelect;
    }

    /// <summary>
    /// Event argument for Group Picker field
    /// </summary>
    public class GroupPickerRowEventArgs : RowEventArgs
    {
        /// <summary>
        /// Gets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPickerRowEventArgs"/> class.
        /// </summary>
        /// <param name="row">The row.</param>
        public GroupPickerRowEventArgs( GridViewRow row )
            : base( row )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPickerRowEventArgs"/> class.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="groupId">The group identifier.</param>
        public GroupPickerRowEventArgs( GridViewRow row, int? groupId )
            : base( row )
        {
            GroupId = groupId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPickerRowEventArgs"/> class.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="rowKeyValue">The row key value.</param>
        public GroupPickerRowEventArgs( int rowIndex, object rowKeyValue )
            : base( rowIndex, rowKeyValue )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPickerRowEventArgs"/> class.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="rowKeyValue">The row key value.</param>
        /// <param name="groupId">The group identifier.</param>
        public GroupPickerRowEventArgs( int rowIndex, object rowKeyValue, int? groupId )
            : base( rowIndex, rowKeyValue )
        {
            GroupId = groupId;
        }
    }

}