using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using church.ccv.Datamart.Model;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace church.ccv.Datamart.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people that are associated with a specific neighborhood." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Neighborhood Filter" )]
    public class NeighborhoodFilter : DataFilterComponent
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
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Datamart Filters"; }
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
            return "Neighborhood";
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
    var neighborhoodPicker = $('.js-neighborhood-picker', $content);
    var neighborhoodName = $(':selected', neighborhoodPicker).text();

    return 'Neighborhood: ' + neighborhoodName;
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
            string result = "Neighborhood";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 1 )
            {
                string neighborhoodName = selectionValues[0];
                result = "Neighborhood: " + neighborhoodName;
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockDropDownList ddlNeighborhoodPicker = new RockDropDownList();
            ddlNeighborhoodPicker.ID = filterControl.ID + "_0";
            ddlNeighborhoodPicker.Label = string.Empty;
            ddlNeighborhoodPicker.CssClass = "js-neighborhood-picker";

            var neighborHoodNames = new church.ccv.Datamart.Model.DatamartNeighborhoodService( new RockContext() ).Queryable().Select( a =>
                a.NeighborhoodName ).Distinct().OrderBy( a => a ).ToList();

            ddlNeighborhoodPicker.Items.Clear();
            ddlNeighborhoodPicker.Items.Add( new ListItem() );
            ddlNeighborhoodPicker.Items.AddRange( neighborHoodNames.Select( a => new ListItem( a ) ).ToArray() );

            filterControl.Controls.Add( ddlNeighborhoodPicker );

            return new Control[1] { ddlNeighborhoodPicker };
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
            var neighborhoodName = ( controls[0] as RockDropDownList ).SelectedValue;
            return neighborhoodName;
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
                var ddlNeighborHood = controls[0] as RockDropDownList;
                ddlNeighborHood.SelectedValue = selectionValues[0];
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
            var rockContext = (RockContext)serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                string neighborhoodName = selectionValues[0];

                if ( !string.IsNullOrWhiteSpace( neighborhoodName ) )
                {
                    var datamartPersonService = new Service<DatamartPerson>( rockContext );
                    var qryDatamartPerson = datamartPersonService.Queryable().Where( a => a.NeighborhoodName == neighborhoodName );

                    var qry = new PersonService( rockContext ).Queryable()
                        .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                    Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                    return extractedFilterExpression;
                }
            }

            return null;
        }

        #endregion
    }
}
