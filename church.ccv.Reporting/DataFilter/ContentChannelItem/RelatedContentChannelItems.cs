// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace church.ccv.Reporting.DataFilter.ContentChannelItem
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filters Content Channel Items to show items that have any of the same categories as the item(s) specified by the current route." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Content Channel Item Related By Categories Filter" )]
    public class RelatedContentChannelItems : DataFilterComponent
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
            get { return typeof( Rock.Model.ContentChannelItem ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "CCV Custom"; }
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
            return "Related Content Channel Items from Route/Category";
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
            return string.Format(
                @"
                function() {{
                  return '{0}';
                }}",
                GetTitle( entityType ) );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            return GetTitle( entityType );
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            Literal lDescription = new Literal();
            lDescription.Text = "This filter has no settings. It will look at the current route and return content channel items of the same content channel type that have the similar values for the Categories attribute value";

            return new Control[] { lDescription };
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
            // intentionally blank
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
            var currentRockPage = HttpContext.Current.Handler as RockPage;
            if ( currentRockPage != null )
            {
                // figure what content channel(s) are specified for the current page.  Note: This assumes that the main content channel for the page using Route Filtering
                var pageParams = currentRockPage.PageParameters();
                List<int> currentContentChannelItemIds = new List<int>();
                int entityTypeId = Rock.Web.Cache.EntityTypeCache.Read<Rock.Model.ContentChannelItem>().Id;
                foreach ( var pageParam in pageParams )
                {
                    var attributeKey = pageParam.Key;
                    var attributeValue = pageParam.Value.ToString();
                    var contentChannelItemIdsForParam = new AttributeValueService( rockContext ).Queryable()
                        .Where( a => a.Attribute.Key == attributeKey )
                        .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                        .Where( a => a.Value == attributeValue )
                        .Where( a => a.EntityId.HasValue )
                        .Select( a => a.EntityId.Value ).ToList();

                    currentContentChannelItemIds.AddRange( contentChannelItemIdsForParam );
                }

                // get the list of categories for the current content channel(s) on the page.  Note: Assumes ContentChannelItem has an AttributeKey of "Categories"
                var currentChannelItemsList = new ContentChannelItemService( rockContext ).Queryable().Where( a => currentContentChannelItemIds.Contains( a.Id ) ).ToList();
                var currentCategoryGuidList = new List<Guid>();
                foreach ( var currentContentChannelItem in currentChannelItemsList )
                {
                    currentContentChannelItem.LoadAttributes();
                    var categories = ( currentContentChannelItem.GetAttributeValue( "Categories" ) ?? string.Empty ).SplitDelimitedValues().AsGuidList();
                    currentCategoryGuidList.AddRange( categories );
                }

                // now, get all the content channel item ids that include any of the categories from the current content channel(s) on the page
                List<int> relatedOtherContentChannelItemIds = new List<int>();
                foreach ( var categoryGuid in currentCategoryGuidList.Select( a => a.ToString() ) )
                {
                    var relatedContentChannelItemIdsForCategory = new AttributeValueService( rockContext ).Queryable()
                            .Where( a => a.Attribute.Key == "Categories" )
                            .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                            .Where( a => a.EntityId.HasValue )
                            .Where( a => a.Value.Contains( categoryGuid ) )
                            .Select( a => a.EntityId.Value ).ToList();

                    relatedOtherContentChannelItemIds.AddRange( relatedContentChannelItemIdsForCategory );
                }

                // now that we have ids for the related ContentChannelItem
                var qry = new ContentChannelItemService( (RockContext)serviceInstance.Context ).Queryable()
                                .Where( p => relatedOtherContentChannelItemIds.Contains( p.Id ) && !currentContentChannelItemIds.Contains( p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.ContentChannelItem>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}