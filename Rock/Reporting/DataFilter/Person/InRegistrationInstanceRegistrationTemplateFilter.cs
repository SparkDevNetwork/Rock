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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they are in a registration instance of the specified registration template or templates" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Registration Template(s) Filter" )]
    public class InRegistrationInstanceRegistrationTemplateFilter : DataFilterComponent
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
            return "In Registration Template(s)";
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
            return "Rock.reporting.formatFilterForRegistrationTemplateFilterField('In templates:', $content)";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Registrant";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var rockContext = new RockContext();
                var registrationTemplateGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var registrationTemplates = new RegistrationTemplateService( rockContext ).GetByGuids( registrationTemplateGuids );

                SlidingDateRangePicker fakeSlidingDateRangePicker = null;

                bool includeInactiveRegistrationInstances = false;
                if ( selectionValues.Length >= 2 )
                {
                    includeInactiveRegistrationInstances = selectionValues[1].AsBooleanOrNull() ?? false;

                    if ( selectionValues.Length >= 3 )
                    {
                        fakeSlidingDateRangePicker = new SlidingDateRangePicker();

                        // convert comma delimited to pipe
                        fakeSlidingDateRangePicker.DelimitedValues = selectionValues[2].Replace( ',', '|' );
                    }
                }

                if ( registrationTemplates != null )
                {
                    result = string.Format( registrationTemplates.Count() > 0 ? "In Registration Templates: {0}" : "In a Registration", registrationTemplates.Select( a => a.Name ).ToList().AsDelimited( ", ", " or " ) );

                    if ( includeInactiveRegistrationInstances )
                    {
                        result += ", including inactive registration instances";
                    }

                    if ( fakeSlidingDateRangePicker != null )
                    {
                        result += string.Format( ", registered in Date Range: {0}", SlidingDateRangePicker.FormatDelimitedValues( fakeSlidingDateRangePicker.DelimitedValues ) );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The RegistrationTemplatePicker
        /// </summary>
        private RegistrationTemplatePicker rp = null;

        /// <summary>
        /// The "Include Inactive" checkbox
        /// </summary>
        private RockCheckBox cbIncludeInactiveRegistrationInstances = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            rp = new RegistrationTemplatePicker();
            rp.ID = filterControl.ID + "_rp";
            rp.Label = "RegistrationTemplate(s)";
            rp.CssClass = "js-group-picker";
            rp.AllowMultiSelect = true;
            rp.Help = "Select the registration templates that you want the registrants for. Leaving this blank will not restrict results to a registration template.";
            filterControl.Controls.Add( rp );

            cbIncludeInactiveRegistrationInstances = new RockCheckBox();
            cbIncludeInactiveRegistrationInstances.ID = filterControl.ID + "_cbIncludeInactiveRegistrationInstances";
            cbIncludeInactiveRegistrationInstances.Text = "Include Inactive Registration Instances";
            cbIncludeInactiveRegistrationInstances.CssClass = "js-include-inactive-groups";
            cbIncludeInactiveRegistrationInstances.AutoPostBack = true;
            filterControl.Controls.Add( cbIncludeInactiveRegistrationInstances );

            PanelWidget pwAdvanced = new PanelWidget();
            filterControl.Controls.Add( pwAdvanced );
            pwAdvanced.ID = filterControl.ID + "_pwAttributes";
            pwAdvanced.Title = "Advanced Filters";
            pwAdvanced.CssClass = "advanced-panel";

            SlidingDateRangePicker registeredOnDateRangePicker = new SlidingDateRangePicker();
            registeredOnDateRangePicker.ID = pwAdvanced.ID + "_addedOnDateRangePicker";
            registeredOnDateRangePicker.AddCssClass( "js-sliding-date-range" );
            registeredOnDateRangePicker.Label = "Date Registered:";
            registeredOnDateRangePicker.Help = "Select the date range that the person was registered. Leaving this blank will not restrict results to a date range.";
            pwAdvanced.Controls.Add( registeredOnDateRangePicker );

            return new Control[4] { rp, cbIncludeInactiveRegistrationInstances, registeredOnDateRangePicker, pwAdvanced };
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
            if ( controls.Count() < 4 )
            {
                return;
            }

            RegistrationTemplatePicker registrationTemplatePicker = controls[0] as RegistrationTemplatePicker;
            RockCheckBox cbIncludeInactiveRegistrationInstances = controls[1] as RockCheckBox;
            PanelWidget pwAdvanced = controls[3] as PanelWidget;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            registrationTemplatePicker.RenderControl( writer );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbIncludeInactiveRegistrationInstances.ContainerCssClass = "margin-l-md";
            cbIncludeInactiveRegistrationInstances.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            pwAdvanced.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            if ( controls.Count() < 3 )
            {
                return null;
            }

            RegistrationTemplatePicker registrationTemplatePicker = controls[0] as RegistrationTemplatePicker;
            RockCheckBox cbInactiveRegistrationInstances = controls[1] as RockCheckBox;
            SlidingDateRangePicker registeredOnDateRangePicker = controls[2] as SlidingDateRangePicker;

            List<int> registrationTemplateIdList = registrationTemplatePicker.SelectedValues.AsIntegerList();
            var registrationTemplateGuids = new RegistrationTemplateService( new RockContext() ).GetByIds( registrationTemplateIdList ).Select( a => a.Guid ).Distinct().ToList();

            // convert pipe to comma delimited
            var delimitedValues = registeredOnDateRangePicker.DelimitedValues.Replace( "|", "," );

            return string.Format(
                "{0}|{1}|{2}",
                registrationTemplateGuids.AsDelimited( "," ),
                cbIncludeInactiveRegistrationInstances.Checked.ToString(),
                delimitedValues );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( controls.Count() < 3 )
            {
                return;
            }

            RegistrationTemplatePicker registrationTemplatePicker = controls[0] as RegistrationTemplatePicker;
            RockCheckBox cbIncludeInactive = controls[1] as RockCheckBox;
            SlidingDateRangePicker registeredOnDateRangePicker = controls[2] as SlidingDateRangePicker;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                List<Guid> registrationTemplateGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var registrationTemplates = new RegistrationTemplateService( new RockContext() ).GetByGuids( registrationTemplateGuids );
                if ( registrationTemplates != null )
                {
                    registrationTemplatePicker.SetValues( registrationTemplates );
                }

                if ( selectionValues.Length >= 2 )
                {
                    cbIncludeInactiveRegistrationInstances.Checked = selectionValues[1].AsBooleanOrNull() ?? false;
                }
                else
                {
                    // if options where saved before this option was added, set to false, even though it would have included inactive before
                    cbIncludeInactiveRegistrationInstances.Checked = false;
                }

                if ( selectionValues.Length >= 3 )
                {
                    // convert comma delimited to pipe
                    registeredOnDateRangePicker.DelimitedValues = selectionValues[2].Replace( ',', '|' );
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
            if ( selectionValues.Length >= 1 )
            {
                List<Guid> registrationTemplateGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var registrationInstanceService = new RegistrationInstanceService( (RockContext)serviceInstance.Context );
                var registrationInstanceIds = registrationInstanceService.Queryable().Where( ri => registrationTemplateGuids.Contains( ri.RegistrationTemplate.Guid ) ).Select( ri => ri.Id ).Distinct().ToList();

                RegistrationRegistrantService registrationRegistrantService = new RegistrationRegistrantService( (RockContext)serviceInstance.Context );


                bool includeInactiveRegistrationInstances = false;

                if ( selectionValues.Length >= 2 )
                {
                    includeInactiveRegistrationInstances = selectionValues[1].AsBooleanOrNull() ?? true; ;
                }
                else
                {
                    // if options where saved before this option was added, set to false, even though it would have included inactive before
                    includeInactiveRegistrationInstances = false;
                }

                var registrationRegistrantServiceQry = registrationRegistrantService.Queryable();

                if ( registrationTemplateGuids.Count > 0 )
                {
                    registrationRegistrantServiceQry = registrationRegistrantServiceQry.Where( xx => registrationInstanceIds.Contains( xx.Registration.RegistrationInstanceId ) );
                }

                if ( selectionValues.Length >= 3 )
                {
                    string slidingDelimitedValues = selectionValues[2].Replace( ',', '|' );
                    DateRange dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        registrationRegistrantServiceQry = registrationRegistrantServiceQry.Where( xx => xx.CreatedDateTime >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        registrationRegistrantServiceQry = registrationRegistrantServiceQry.Where( xx => xx.CreatedDateTime < dateRange.End.Value );
                    }
                }

                var qry = new PersonService( (RockContext)serviceInstance.Context ).Queryable()
                    .Where( p => registrationRegistrantServiceQry.Any( xx => xx.PersonAlias.PersonId == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}