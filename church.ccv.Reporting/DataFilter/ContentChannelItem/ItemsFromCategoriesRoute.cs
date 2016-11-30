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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace church.ccv.Reporting.DataFilter.ContentChannelItem
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filters Content Channel Items to show ones that have any of the Categories specified in the route" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Content Channel Item Categories From Route Filter" )]
    public class ItemsFromCategoriesRoute : DataFilterComponent
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
            return "Content Channel Items from Categories in Route";
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
            lDescription.Text =
@"<p>This filter will look at the current route <pre>/SomePage/{Categories}</pre> and for the comma-delimited categories to filter by.</p>
<p>Note: Requires the Route to have a Url Template that includes {Categories} and that the Categories are DefinedValues. For example, /LifeStories/{Categories}.</p>
<p>Then to filter by categories, the url would look something like  /LifeStories/Faith,Neighbors,Inspiration</p>
";

            filterControl.Controls.Add( lDescription );
            var definedTypePicker = new RockDropDownList { ID = filterControl.ID + "_definedTypePicker" };
            definedTypePicker.Label = "Defined Type for Categories";
            definedTypePicker.Required = true;
            definedTypePicker.Items.Add( new ListItem() );

            var definedTypes = new DefinedTypeService( new RockContext() ).Queryable().OrderBy( d => d.Name );
            if ( definedTypes.Any() )
            {
                foreach ( var definedType in definedTypes )
                {
                    definedTypePicker.Items.Add( new ListItem( definedType.Name, definedType.Guid.ToString().ToUpper() ) );
                }
            }

            filterControl.Controls.Add( definedTypePicker );

            return new Control[] { lDescription, definedTypePicker };
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
            var definedTypePicker = controls[1] as RockDropDownList;
            return definedTypePicker.SelectedValue;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var definedTypePicker = controls[1] as RockDropDownList;
            definedTypePicker.SetValue( selection.AsGuidOrNull() );
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
                var definedType = DefinedTypeCache.Read( selection.AsGuid() );
                int definedTypeId = definedType != null ? definedType.Id : 0;

                var categoryNames = ( currentRockPage.PageParameter( "Categories" ) ?? string.Empty ).SplitDelimitedValues();

                var categoriesDefinedValueGuidList = new DefinedValueService( rockContext ).Queryable().Where( a => a.DefinedTypeId == definedTypeId && categoryNames.Contains( a.Value ) ).Select( a => a.Guid ).ToList();

                int entityTypeId = Rock.Web.Cache.EntityTypeCache.Read<Rock.Model.ContentChannelItem>().Id;

                // get all the content channel item ids that include any of the categories from the current content channel(s) on the page
                List<int> contentChannelItemIds = new List<int>();
                foreach ( var categoryGuid in categoriesDefinedValueGuidList.Select( a => a.ToString() ) )
                {
                    var relatedContentChannelItemIdsForCategory = new AttributeValueService( rockContext ).Queryable()
                            .Where( a => a.Attribute.Key == "Categories" )
                            .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                            .Where( a => a.EntityId.HasValue )
                            .Where( a => a.Value.Contains( categoryGuid ) )
                            .Select( a => a.EntityId.Value ).ToList();

                    contentChannelItemIds.AddRange( relatedContentChannelItemIdsForCategory );
                }

                // now that we have ids for the related ContentChannelItem
                var qry = new ContentChannelItemService( (RockContext)serviceInstance.Context ).Queryable()
                                .Where( p => contentChannelItemIds.Contains( p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.ContentChannelItem>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}