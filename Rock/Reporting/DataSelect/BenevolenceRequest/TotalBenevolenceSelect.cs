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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Model;

namespace Rock.Reporting.DataSelect.BenevolenceRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Select the Total Amount of a Benevolence Request")]
    [Export(typeof(DataSelectComponent))]
    [ExportMetadata("ComponentName", "Select Benevolence Total Amount")]
    [Rock.SystemGuid.EntityTypeGuid( "94B231F7-BBB7-48D1-94D1-CAEB0E16123A")]
    public class TotalAmountSelect : DataSelectComponent
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
                return typeof(Rock.Model.BenevolenceRequest).FullName;
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
                return "Total Amount";
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
            get { return typeof(decimal?); }
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField(Type entityType, string selection)
        {
            return new Rock.Web.UI.Controls.CurrencyField();
        }
#endif

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
                return "Total Amount";
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
        public override string GetTitle(Type entityType)
        {
            return "Total Amount";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression(RockContext context, MemberExpression entityIdProperty, string selection)
        {
            var totalAmountQuery = new BenevolenceRequestService(context).Queryable()
                .Select(p => p.BenevolenceResults.Sum(a => a.Amount));

            var selectTotalAmountExpression = SelectExpressionExtractor.Extract(totalAmountQuery, entityIdProperty, "p");

            return selectTotalAmountExpression;
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls(System.Web.UI.Control parentControl)
        {
            return new System.Web.UI.Control[] { };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls(System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls)
        {
            base.RenderControls(parentControl, writer, controls);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection(System.Web.UI.Control[] controls)
        {
            return null;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection(System.Web.UI.Control[] controls, string selection)
        {
            // nothing to do
        }
#endif

        #endregion
    }
}
