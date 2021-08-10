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
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the count of notes created by the person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select the count of notes created by the person" )]
    public class CreatedNotesCountSelect : DataSelectComponent
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
            get
            {
                return base.Section;
            }
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
                return "CreatedNotesCount";
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
            get { return typeof( int ); }
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
                return "Count of Notes Created";
            }
        }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        /// <value>
        /// The sort expression.
        /// </value>
        public override string SortProperties( string selection )
        {
            // disable sorting on this column since it is an IEnumerable
            return string.Empty;
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
            return "Count of Notes Created";
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
            var settings = new CreatedNotesCountSelectSettings( selection );

            if ( settings != null )
            {
                var noteQry = new NoteService( context ).Queryable().Where( x => x.CreatedByPersonAliasId.HasValue );

                if ( settings.NoteTypeIds != null && settings.NoteTypeIds.Any() )
                {
                    noteQry = noteQry.Where( xx => settings.NoteTypeIds.Contains( xx.NoteTypeId ) );
                }

                if ( settings.DelimitedValues.IsNotNullOrWhiteSpace() )
                {
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( settings.DelimitedValues );

                    if ( dateRange.Start.HasValue )
                    {
                        noteQry = noteQry.Where( i => i.CreatedDateTime >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        noteQry = noteQry.Where( i => i.CreatedDateTime <= dateRange.End.Value );
                    }
                }

                var qry = new PersonService( context ).Queryable()
                   .Select( p => noteQry.Where( l => l.CreatedByPersonAlias.PersonId == p.Id ).Count() );

                var selectExpression = SelectExpressionExtractor.Extract( qry, entityIdProperty, "p" );

                return selectExpression;
            }

            return null;
        }

        private const string _CtlNoteTypes = "cblNoteTypes";
        private const string _CtlSlidingDateRangePicker = "slidingDateRangePicker";

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            var cblNoteTypes = new RockListBox();
            cblNoteTypes.ID = parentControl.GetChildControlInstanceName( _CtlNoteTypes );
            cblNoteTypes.Label = "Note Types";
            cblNoteTypes.Help = "The type of note to filter by. Leave blank to include all note types.";
            parentControl.Controls.Add( cblNoteTypes );

            var noteTypeService = new NoteTypeService( new RockContext() );
            var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
            var noteTypes = noteTypeService.Queryable()
                .Where( a => a.EntityTypeId == entityTypeIdPerson )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            cblNoteTypes.Items.Clear();
            cblNoteTypes.Items.AddRange( noteTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = parentControl.GetChildControlInstanceName( _CtlSlidingDateRangePicker );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range that the note was created during.";
            slidingDateRangePicker.Required = false;
            parentControl.Controls.Add( slidingDateRangePicker );

            return new System.Web.UI.Control[] { cblNoteTypes, slidingDateRangePicker };
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            var cblNoteTypes = controls.GetByName<RockListBox>( _CtlNoteTypes );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRangePicker );

            var settings = new CreatedNotesCountSelectSettings();
            settings.NoteTypeIds.AddRange( cblNoteTypes.SelectedValuesAsInt );
            settings.DelimitedValues = slidingDateRangePicker.DelimitedValues;

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var cblNoteTypes = controls.GetByName<RockListBox>( _CtlNoteTypes );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRangePicker );

            var settings = new CreatedNotesCountSelectSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            cblNoteTypes.SetValues( settings.NoteTypeIds );
            slidingDateRangePicker.DelimitedValues = settings.DelimitedValues.ToStringSafe();
        }

        #endregion

        #region Settings

        /// <summary>
        ///     Settings for the Data Select Component "Count of Notes Created".
        /// </summary>
        private class CreatedNotesCountSelectSettings : SettingsStringBase
        {
            public readonly List<int> NoteTypeIds = new List<int>();

            public string DelimitedValues { get; set; }

            public CreatedNotesCountSelectSettings()
            {
                //
            }

            public CreatedNotesCountSelectSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                NoteTypeIds.Clear();

                var selectedNoteTypeIds = DataComponentSettingsHelper.GetParameterAsList( parameters, 0, "," );

                foreach ( var noteTypeId in selectedNoteTypeIds )
                {
                    NoteTypeIds.Add( noteTypeId.AsInteger() );
                }

                DelimitedValues = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 1 ).Replace( ";", "|" );
            }

            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( NoteTypeIds == null ? string.Empty : NoteTypeIds.AsDelimited( "," ) );

                settings.Add( DelimitedValues.Replace( "|", ";" ).ToStringSafe() );

                return settings;
            }
        }

        #endregion
    }
}
