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
using System.Web.UI;

using Rock.Data;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Provides functionality shared between filters that filter entirely based on Campus(es)Picker.
    /// </summary>
    public abstract class BaseCampusFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether to include inactive campuses.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        internal virtual bool IncludeInactive => true;

        /// <summary>
        /// Gets a value indicating which text to use as the campus picker's label
        /// </summary>
        /// <value>
        ///     The text to use as the campus picker's label.
        /// </value>
        protected virtual string CampusPickerLabel => string.Empty;

        /// <summary>
        /// Gets the control class name.
        /// </summary>
        /// <value>
        /// The name of the control class.
        /// </value>
        internal virtual string ControlClassName => "js-campus-picker";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/baseCampusFilter.obs" ),
                Options = new Dictionary<string, string>
                {
                    { "multiple", "False" },
                    { "label", CampusPickerLabel },
                    { "includeInactive", IncludeInactive.ToTrueFalse() }
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var result = new Dictionary<string, string>();

            if ( selection.IsNotNullOrWhiteSpace() && selection.AsGuidOrNull() != null )
            {
                var campus = CampusCache.Get( selection.AsGuid() );
                if ( campus != null )
                {
                    result.AddOrReplace( "campus", ( new ListItemBag { Text = campus.Name, Value = campus.Guid.ToString() } ).ToCamelCaseJson( false, true ) );
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var campusSerialized = data.GetValueOrNull( "campus" );
            var campusBag = campusSerialized?.FromJsonOrNull<ListItemBag>();
            var campusGuidString = campusBag?.Value ?? "";
            var campus = CampusCache.Get( campusGuidString.AsGuid() );

            if ( campus != null )
            {
                return campus.Guid.ToString();
            }

            return string.Empty;
        }

        #endregion

        #region Methods

#if WEBFORMS

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            CampusPicker campusPicker = new CampusPicker();
            campusPicker.ID = filterControl.ID + "_0";
            campusPicker.Label = CampusPickerLabel;
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

#endif

        #endregion

    }
}
