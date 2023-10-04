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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace org.lakepointe.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Show the date of the first attendance of a person on a campus in a group of groups." )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select the date of the first attendance of a person on a campus in a group of groups.." )]
    public class FirstAttendanceOnCampus : DataSelectComponent
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Groups"; }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "First Attendance In Selected Groups On Campus";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( DateTime? ); }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "First Attendance On Campus";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "First Attendance On Campus";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var options = selection.Split( '|' );
            if (options.Count() < 2)
            {
                return null;
            }

            var groupGuids = options[0].Split( ',' ).AsGuidList();
            var campusGuid = options[1].AsGuid();

            AttendanceService attendanceService = new AttendanceService( context );
            var groupAttendanceQry = attendanceService.Queryable( "Group,Campus,PersonAlias" )
                .Where( a => groupGuids.Contains( a.Occurrence.Group.Guid ) && a.Campus.Guid == campusGuid && a.DidAttend == true );

            var qry = new PersonService( context ).Queryable()
                .Select( p => groupAttendanceQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Min( xx => xx.StartDateTime ) );

            Expression selectExpression = SelectExpressionExtractor.Extract( qry, entityIdProperty, "p" );

            return selectExpression;
        }

        /// <summary>
        /// The GroupPicker
        /// </summary>
        private GroupPicker _groupPicker = null;

        private CampusPicker _campusPicker = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            IEnumerable<string> selectedGroups = null;
            if (_groupPicker != null)
            {
                selectedGroups = _groupPicker.ItemIds;
            }
            
            _groupPicker = new GroupPicker();
            _groupPicker.AllowMultiSelect = true;
            _groupPicker.AddCssClass( "js-group-picker" );
            _groupPicker.ID = parentControl.ID + "_0";
            _groupPicker.Label = "Groups";
            _groupPicker.ItemIds = selectedGroups ?? new List<string>();
            parentControl.Controls.Add( _groupPicker );

            int? selectedCampus = null;
            if ( _campusPicker != null )
            {
                selectedCampus = _campusPicker.SelectedCampusId;
            }

            _campusPicker = new CampusPicker();
            _campusPicker.Campuses = CampusCache.All( false );
            _campusPicker.AddCssClass( "js-campus-picker" );
            _campusPicker.ID = parentControl.ID + "_1";
            _campusPicker.Label = "Campus";
            _campusPicker.SelectedCampusId = selectedCampus ?? 0;
            parentControl.Controls.Add( _campusPicker );

            return new Control[2] { _groupPicker, _campusPicker };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            if ( _groupPicker == null || _campusPicker == null )
            {
                return string.Empty;
            }

            var context = new RockContext();
            var groupGuids = new GroupService( context ).GetByIds( _groupPicker.ItemIds.AsIntegerList() ).Select( a => a.Guid ).ToList();
            var campusGuids = new CampusService( context ).GetByIds( new List<int> { _campusPicker.SelectedCampusId.HasValue ? _campusPicker.SelectedCampusId.Value : 0 } ).Select( c => c.Guid ).ToList();
            return string.Format( "{0}|{1}", groupGuids.AsDelimited( "," ), campusGuids.FirstOrDefault() );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var options = selection.Split( '|' );
            if (_groupPicker != null && _campusPicker != null && options.Count() >= 2 )
            {
                var context = new RockContext();

                var groupGuids = options[0].Split( ',' ).AsGuidList();
                var groups = new GroupService( context ).GetByGuids( groupGuids );
                _groupPicker.SetValues( groups );

                var campusGuid = options[1].AsGuid();
                var campus = new CampusService( context ).GetByGuids( new List<Guid> { campusGuid } ).FirstOrDefault();
                if ( campus != null )
                {
                    _campusPicker.SelectedCampusId = campus.Id;
                }
            }
        }

        #endregion
    }
}
