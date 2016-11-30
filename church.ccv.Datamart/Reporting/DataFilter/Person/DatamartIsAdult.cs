using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using church.ccv.Datamart.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace church.ccv.Datamart.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on Family Role in the Datamart Person Table" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person IsAdult Datamart Filter" )]
    public class DatamartIsAdult : DataFilterComponent
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
            return "Is Adult";
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
    var isAdult = $('.js-is-adult', $content).val();

    if (isAdult == 'Yes') {
        return 'Is Adult';
    } else {
        return 'Is not Adult';
    }    
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
            var isAdultValue = selection.AsBooleanOrNull() ?? false;
            if ( isAdultValue )
            {
                return "Is Adult";
            }
            else
            {
                return "Is not Adult";
            }
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockDropDownList ddlIsAdult = new RockDropDownList();
            ddlIsAdult.ID = filterControl.ID + "_ddlIsAdult";
            ddlIsAdult.Label = string.Empty;
            ddlIsAdult.CssClass = "js-is-adult";
            ddlIsAdult.Items.Add( "Yes" );
            ddlIsAdult.Items.Add( "No" );

            filterControl.Controls.Add( ddlIsAdult );

            return new Control[1] { ddlIsAdult };
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
            RockDropDownList ddlIsAdult = controls[0] as RockDropDownList;
            return ddlIsAdult.SelectedValue.AsBoolean().ToTrueFalse();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            RockDropDownList ddlIsAdult = controls[0] as RockDropDownList;
            ddlIsAdult.SetValue( selection.AsBoolean().ToYesNo() );
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

            var isAdultValue = selection.AsBooleanOrNull() ?? false;

            var datamartPersonService = new Service<DatamartPerson>( rockContext );
            var qryDatamartPerson = datamartPersonService.Queryable();
            if ( isAdultValue )
            {
                qryDatamartPerson.Where( a => a.FamilyRole == "Adult" );
            }
            else
            {
                qryDatamartPerson.Where( a => a.FamilyRole != "Adult" );
            }

            var qry = new PersonService( rockContext ).Queryable()
                .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}
