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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the Last Public Note of the Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Last Public Note for a Person" )]
    public class LastNoteSelect : DataSelectComponent
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
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "Last Note";
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
            get { return typeof( string ); }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override DataControlField GetGridField( Type entityType, string selection )
        {
            BoundField result = new BoundField();
            result.HtmlEncode = false;

            return result;
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
                return "Last Note";
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
            return "Last Note";
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
            string[] selectionValues = selection.Split( '|' );
            int? noteTypeId = null;
            if ( selectionValues.Count() > 0 )
            {
                noteTypeId = selectionValues[0].AsIntegerOrNull();
            }

            var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();

            // only get PersonNotes that are not Private
            var qryNotes = new NoteService( context ).Queryable().Where( a => a.NoteType.EntityTypeId == entityTypeIdPerson && a.IsPrivateNote == false );

            if ( noteTypeId.HasValue )
            {
                qryNotes = qryNotes.Where( n => n.NoteTypeId == noteTypeId.Value );
            }

            var qryPersonNotes = new PersonService( context ).Queryable().Select( p => qryNotes.Where( xx => xx.Id == qryNotes.Where( a => a.EntityId == p.Id ).Max( x => x.Id ) ).Select( s => s.Text ).FirstOrDefault() );

            var selectNoteExpression = SelectExpressionExtractor.Extract( qryPersonNotes, entityIdProperty, "p" );

            return selectNoteExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockDropDownList ddlNoteType = new RockDropDownList();
            ddlNoteType.ID = parentControl.ID + "_ddlNoteType";
            ddlNoteType.Label = "Note Type";
            parentControl.Controls.Add( ddlNoteType );

            var noteTypeService = new NoteTypeService( new RockContext() );
            var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
            var noteTypes = noteTypeService.Queryable().Where( a => a.EntityTypeId == entityTypeIdPerson ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( a => new
            {
                a.Id,
                a.Name
            } ).ToList();

            ddlNoteType.Items.Clear();
            ddlNoteType.Items.Add( new ListItem() );
            ddlNoteType.Items.AddRange( noteTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            return new System.Web.UI.Control[] { ddlNoteType };
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
            if ( controls.Count() >= 0 )
            {
                RockDropDownList ddlNoteType = controls[0] as RockDropDownList;
                return string.Format( "{0}", ddlNoteType.SelectedValue );
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            if ( controls.Count() >= 0 )
            {
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 1 )
                {
                    RockDropDownList ddlNoteType = controls[0] as RockDropDownList;
                    ddlNoteType.SelectedValue = selectionValues[0];
                }
            }
        }

        #endregion
    }
}
