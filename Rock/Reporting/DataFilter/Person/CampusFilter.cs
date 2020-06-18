﻿// <copyright>
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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people that are associated with a specific campus." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Campus Filter" )]
    public class CampusFilter : DataFilterComponent, IUpdateSelectionFromPageParameters
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
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section => "Additional Filters";

        /// <summary>
        /// Set this to show descriptive text that can help explain how complex filters work or offer assistance on possibly other filters that have better performance.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public override string Description => "Consider using the 'Primary Campus' filter if you are concerned with speed. This filter is slower as it checks the campus of all families the person might belong to.";

        /// <summary>
        /// Gets the control class name.
        /// </summary>
        /// <value>
        /// The name of the control class.
        /// </value>
        internal virtual string ControlClassName
        {
            get { return "js-campus-picker"; }
        }

        /// <summary>
        /// Gets a value indicating whether to include inactive campuses.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        internal virtual bool IncludeInactive
        {
            get { return true; }
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
            return "Campus";
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
            return $@"
function() {{
    var campusPicker = $('.{this.ControlClassName}', $content);
    var campusName = $(':selected', campusPicker).text();

    return 'Campus: ' + campusName;
}}";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Campus";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 1 )
            {
                Guid campusGuid = selectionValues[0].AsGuid();
                var campus = CampusCache.Get( campusGuid );
                if ( campus != null )
                {
                    result = "Campus: " + campus.Name;
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
            CampusPicker campusPicker = new CampusPicker();
            campusPicker.ID = filterControl.ID + "_0";
            campusPicker.Label = string.Empty;
            campusPicker.CssClass = $"{ControlClassName}";
            campusPicker.Campuses = CampusCache.All( IncludeInactive );

            filterControl.Controls.Add( campusPicker );

            return new Control[1] { campusPicker };
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
            var campusId = ( controls[0] as CampusPicker ).SelectedCampusId;
            if ( campusId.HasValue )
            {
                var campus = CampusCache.Get( campusId.Value );
                if ( campus != null )
                {
                    return campus.Guid.ToString();
                }
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
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var campusPicker = controls[0] as CampusPicker;
                var selectedCampus = CampusCache.Get( selectionValues[0].AsGuid() );
                if ( selectedCampus != null )
                {
                    campusPicker.SelectedCampusId = selectedCampus.Id;
                }
                else
                {
                    campusPicker.SelectedCampusId = null;
                }
            }
        }

        /// <summary>
        /// Updates the selection from page parameters.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="rockBlock">The rock block.</param>
        /// <returns></returns>
        public string UpdateSelectionFromPageParameters( string selection, Rock.Web.UI.RockBlock rockBlock )
        {
            string[] selectionValues = selection?.Split( '|' ) ?? new string[] { "" };
            if ( selectionValues.Length >= 1 )
            {
                var campusId = rockBlock.PageParameter( "CampusId" ).AsIntegerOrNull();
                if ( campusId == null )
                {
                    var campusEntity = rockBlock.ContextEntity<Campus>();
                    if ( campusEntity != null )
                    {
                        campusId = campusEntity.Id;
                    }
                }

                if ( campusId.HasValue )
                {
                    var selectedCampus = CampusCache.Get( campusId.Value );
                    if ( selectedCampus != null )
                    {
                        selectionValues[0] = selectedCampus.Guid.ToString();
                        return selectionValues.ToList().AsDelimited( "|" );
                    }
                }
            }

            return selection;
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
            var rockContext = (RockContext)serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var campus = CampusCache.Get( selectionValues[0].AsGuid() );
                if ( campus == null )
                {
                    return null;
                }

                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                var groupTypeFamily = GroupTypeCache.GetFamilyGroupType();
                int groupTypeFamilyId = groupTypeFamily != null ? groupTypeFamily.Id : 0;

                var groupMemberServiceQry = groupMemberService.Queryable()
                    .Where( xx => xx.Group.GroupTypeId == groupTypeFamilyId )
                    .Where( xx => ( xx.Group.CampusId ?? 0 ) == campus.Id );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}