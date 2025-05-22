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
using System.Web.UI;

using Rock.Data;
using Rock.Net;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Provides functionality shared between filters that filter entirely based on Campus(es)Picker.
    /// </summary>
    public abstract class BaseCampusesFilter : DataFilterComponent
    {
        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Obsidian/Reporting/DataFilters/baseCampusFilter.obs";

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
        internal virtual string ControlClassName => "js-campuses-picker";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var result = new Dictionary<string, string>();

            result.AddOrReplace( "multiple", "True" );
            result.AddOrReplace( "label", CampusPickerLabel );
            result.AddOrReplace( "includeInactive", IncludeInactive.ToTrueFalse() );

            if ( selection.IsNotNullOrWhiteSpace() )
            {
                var selectionValues = selection.Split( '|' );
                var campuses = new List<ListItemBag>();
                if ( selectionValues.Length >= 1 )
                {
                    var campusGuidList = selectionValues[0].Split( ',' ).AsGuidList();
                    foreach ( var campusGuid in campusGuidList )
                    {
                        var campus = CampusCache.Get( campusGuid );
                        if ( campus != null )
                        {
                            campuses.Add( new ListItemBag { Text = campus.Name, Value = campus.Guid.ToString() } );
                        }
                    }
                }

                result.AddOrReplace( "campus", campuses.ToCamelCaseJson( false, true ) );
            }

            return result;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var campusSerialized = data.GetValueOrNull( "campus" );
            var campusBags = campusSerialized?.FromJsonOrNull<ListItemBag[]>();
            var campusGuids = campusBags?.Select( s => s.Value ).ToList();

            if ( campusGuids != null && campusGuids.Any() )
            {
                return campusGuids.AsDelimited( "," );
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
            CampusesPicker campusesPicker = new CampusesPicker();
            campusesPicker.ID = filterControl.ID + "_0";
            campusesPicker.Label = CampusPickerLabel;
            campusesPicker.CssClass = $"{ControlClassName} campuses-picker";
            campusesPicker.Campuses = CampusCache.All( IncludeInactive );

            filterControl.Controls.Add( campusesPicker );

            return new Control[1] { campusesPicker };
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
            var campusIds = ( controls[0] as CampusesPicker ).SelectedCampusIds;
            if ( campusIds != null && campusIds.Any() )
            {
                List<Guid> campusGuids = new List<Guid>();
                foreach ( var campusId in campusIds )
                {
                    var campus = CampusCache.Get( campusId );
                    if ( campus != null )
                    {
                        campusGuids.Add( campus.Guid );
                    }
                }

                return campusGuids.Select( s => s.ToString() ).ToList().AsDelimited( "," );
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
                var campusGuidList = selectionValues[0].Split( ',' ).AsGuidList();
                List<int> campusIds = new List<int>();
                foreach ( var campusGuid in campusGuidList )
                {
                    var campus = CampusCache.Get( campusGuid );
                    if ( campus != null )
                    {
                        campusIds.Add( campus.Id );
                    }
                }

                var campusesPicker = controls[0] as CampusesPicker;
                campusesPicker.SelectedCampusIds = campusIds;
            }
        }

#endif

        #endregion

    }
}
