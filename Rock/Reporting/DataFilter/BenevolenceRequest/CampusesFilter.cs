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
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.BenevolenceRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Filter benevolence requests that are associated with any of the selected campuses.")]
    [Export(typeof(DataFilterComponent))]
    [ExportMetadata("ComponentName", "Benevolence Request Campuses Filter")]
    public class CampusesFilter : DataFilterComponent, IUpdateSelectionFromPageParameters
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
            get { return typeof(Rock.Model.BenevolenceRequest).FullName; }
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

        /// <summary>
        /// Gets the control class name.
        /// </summary>
        /// <value>
        /// The name of the control class.
        /// </value>
        internal virtual string ControlClassName
        {
            get { return "js-campuses-picker"; }
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
        public override string GetTitle(Type entityType)
        {
            return "Campuses";
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
        public override string GetClientFormatSelection(Type entityType)
        {
            return $@"
function() {{
    var result = 'Campuses';
    var campusesPicker = $('.{this.ControlClassName}', $content);
    var checkedCampuses = $('.{this.ControlClassName}', $content).find(':checked').closest('label');
    if (checkedCampuses.length) {{
        var campusCommaList = checkedCampuses.map(function() {{ return $(this).text() }}).get().join(',');
        result = 'Campuses: ' + campusCommaList;
    }}

    return result;
}}";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection(Type entityType, string selection)
        {
            string result = "Campuses";
            string[] selectionValues = selection.Split('|');

            if (selectionValues.Length >= 1)
            {
                var campusGuidList = selectionValues[0].Split(',').AsGuidList();
                List<string> campusNames = new List<string>();
                foreach (var campusGuid in campusGuidList)
                {
                    var campus = CampusCache.Get(campusGuid);
                    if (campus != null)
                    {
                        campusNames.Add(campus.Name);
                    }
                }

                if (campusNames.Any())
                {
                    result = "Campuses: " + campusNames.AsDelimited(", ");
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls(Type entityType, FilterField filterControl)
        {
            CampusesPicker campusesPicker = new CampusesPicker();
            campusesPicker.ID = filterControl.ID + "_0";
            campusesPicker.Label = string.Empty;
            campusesPicker.CssClass = $"{ControlClassName} campuses-picker";
            campusesPicker.Campuses = CampusCache.All(IncludeInactive);

            filterControl.Controls.Add(campusesPicker);

            return new Control[1] { campusesPicker };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls(Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls)
        {
            base.RenderControls(entityType, filterControl, writer, controls);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection(Type entityType, Control[] controls)
        {
            var campusIds = (controls[0] as CampusesPicker).SelectedCampusIds;
            if (campusIds != null && campusIds.Any())
            {
                List<Guid> campusGuids = new List<Guid>();
                foreach (var campusId in campusIds)
                {
                    var campus = CampusCache.Get(campusId);
                    if (campus != null)
                    {
                        campusGuids.Add(campus.Guid);
                    }
                }

                return campusGuids.Select(s => s.ToString()).ToList().AsDelimited(",");
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection(Type entityType, Control[] controls, string selection)
        {
            string[] selectionValues = selection.Split('|');
            if (selectionValues.Length >= 1)
            {
                var campusGuidList = selectionValues[0].Split(',').AsGuidList();
                List<int> campusIds = new List<int>();
                foreach (var campusGuid in campusGuidList)
                {
                    var campus = CampusCache.Get(campusGuid);
                    if (campus != null)
                    {
                        campusIds.Add(campus.Id);
                    }
                }

                var campusesPicker = controls[0] as CampusesPicker;
                campusesPicker.SelectedCampusIds = campusIds;
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
        public override Expression GetExpression(Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection)
        {
            var rockContext = (RockContext)serviceInstance.Context;

            string[] selectionValues = selection.Split('|');
            if (selectionValues.Length >= 1)
            {
                var campusGuidList = selectionValues[0].Split(',').AsGuidList();
                List<int> campusIds = new List<int>();
                foreach (var campusGuid in campusGuidList)
                {
                    var campus = CampusCache.Get(campusGuid);
                    if (campus != null)
                    {
                        campusIds.Add(campus.Id);
                    }
                }

                if (!campusIds.Any())
                {
                    return null;
                }

                var qry = new BenevolenceRequestService((RockContext)serviceInstance.Context).Queryable()
                    .Where(p => campusIds.Contains(p.CampusId ?? 0));

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.BenevolenceRequest>(qry, parameterExpression, "p");

                return extractedFilterExpression;
            }

            return null;
        }

        /// <summary>
        /// Updates the selection from page parameters.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="rockBlock">The rock block.</param>
        /// <returns></returns>
        public string UpdateSelectionFromPageParameters(string selection, RockBlock rockBlock)
        {
            string[] selectionValues = selection?.Split('|') ?? new string[] { "" };
            if (selectionValues.Length >= 1)
            {
                // check for either a CampusId or CampusIds parameter
                var campusIds = rockBlock.PageParameter("CampusId")?.SplitDelimitedValues().AsIntegerList();
                campusIds = campusIds ?? rockBlock.PageParameter("CampusIds")?.SplitDelimitedValues().AsIntegerList() ?? new List<int>();

                if (campusIds.Any())
                {

                    var selectedCampusGuids = campusIds.Select(a => CampusCache.Get(a)).Where(a => a != null).Select(a => a.Guid).ToList();

                    selectionValues[0] = selectedCampusGuids.AsDelimited(",");
                    return selectionValues.ToList().AsDelimited("|");
                }
            }

            return selection;
        }

        #endregion

    }
}