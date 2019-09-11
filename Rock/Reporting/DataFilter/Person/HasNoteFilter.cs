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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on notes" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Has Note Filter" )]
    public class HasNoteFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

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
            return "Person Note";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @" 
function() {
  var noteTypeName = $('.js-notetype', $content).find(':selected').text()
  var containsText = $('.js-notecontains', $content).val();
  var result = ""Has a "" + noteTypeName + "" note containing '"" + containsText + ""'"";

  return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Person Note";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                int noteTypeId = selectionValues[0].AsInteger();
                var selectedNoteType = NoteTypeCache.Get( noteTypeId );
                if ( selectedNoteType != null )
                {
                    result = $"Has a {selectedNoteType.Name} note";
                }
                else
                {
                    result = "Has a note";
                }

                var containingText = selectionValues[1];
                if ( containingText.IsNotNullOrWhiteSpace() )
                {
                    result += $" containing '{containingText}'";
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockDropDownList ddlNoteType = new RockDropDownList();
            ddlNoteType.ID = filterControl.ID + "_ddlNoteType";
            ddlNoteType.CssClass = "js-notetype";
            ddlNoteType.Label = "Note Type";
            filterControl.Controls.Add( ddlNoteType );

            var noteTypeService = new NoteTypeService( new RockContext() );
            var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
            var noteTypes = noteTypeService.Queryable().Where( a => a.EntityTypeId == entityTypeIdPerson )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            ddlNoteType.Items.Clear();
            ddlNoteType.Items.Add( new ListItem() );
            ddlNoteType.Items.AddRange( noteTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            var tbContains = new RockTextBox();
            tbContains.Label = "Contains";
            tbContains.ID = filterControl.ID + "_tbContains";
            tbContains.CssClass = "js-notecontains";
            filterControl.Controls.Add( tbContains );

            return new System.Web.UI.Control[2] { ddlNoteType, tbContains };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            if ( controls.Count() >= 2 )
            {
                RockDropDownList ddlNoteType = controls[0] as RockDropDownList;
                RockTextBox tbContains = controls[1] as RockTextBox;
                return $"{ddlNoteType.SelectedValue}|{tbContains.Text}";
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( controls.Count() >= 2 )
            {
                RockDropDownList ddlNoteType = controls[0] as RockDropDownList;
                RockTextBox tbContains = controls[1] as RockTextBox;

                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 2 )
                {
                    ddlNoteType.SelectedValue = selectionValues[0];
                    tbContains.Text = selectionValues[1];
                }
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
                var containsText = selectionValues[1];
                var noteQry = new NoteService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( x => x.NoteType.EntityTypeId == entityTypeIdPerson );

                int? noteTypeId = selectionValues[0].AsIntegerOrNull();
                if ( noteTypeId.HasValue )
                {
                    noteQry = noteQry.Where( x => x.NoteTypeId == noteTypeId );
                }

                if ( containsText.IsNotNullOrWhiteSpace() )
                {
                    noteQry = noteQry.Where( a => a.Text.Contains( containsText ) );
                }

                var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => noteQry.Any( x => x.EntityId == p.Id ) );

                return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
            }

            return null;
        }

        #endregion
    }
}