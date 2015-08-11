// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// Provides common functions and data for implementing a Rock Data Filter Component.
    /// A Data Filter Component performs the following functions:
    /// 1. Supplies metadata that describes the filter.
    /// 2. Handles user-interface processing for getting and setting filter values.
    /// 3. Generates a Linq filter expression that can be used as a Where clause to filter Entities of the type to which the filter applies.
    /// </summary>
    public abstract class DataFilterComponent : Component
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public abstract string AppliesToEntityType { get; }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public virtual string Section
        {
            get { return "Additional Filters"; }
        }

        /// <summary>
        /// Gets the default settings for all Attributes associated with this filter.
        /// </summary>
        /// <value>
        /// A set of key-value pairs representing the Attribute names and default values.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                return defaults;
            }
        }

        /// <summary>
        /// Gets or sets the dictionary of options that the filter may use.
        /// </summary>
        /// <value>
        /// A set of key-value pairs representing the option names and values.
        /// </value>
        public virtual Dictionary<string, object> Options {get; set;}

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>The name of the filter.</returns>
        public abstract string GetTitle( Type entityType );

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>The client format script.</returns>
        public virtual string GetClientFormatSelection( Type entityType )
        {
            return string.Format( "'{0} ' + $('select', $content).find(':selected').text() + ' \\'' + $('input', $content).val() + '\\''", GetTitle( entityType ) );
        }

        /// <summary>
        /// Provides a user-friendly description of the specified filter values.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>A string containing the user-friendly description of the settings.</returns>
        public virtual string FormatSelection( Type entityType, string selection )
        {
            ComparisonType comparisonType = ComparisonType.StartsWith;
            string value = string.Empty;

            string[] options = selection.Split( '|' );
            if ( options.Length > 0 )
            {
                comparisonType = options[0].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
            }

            if ( options.Length > 1 )
            {
                value = options[1];
            }

            return string.Format( "{0} {1} '{2}'", GetTitle( entityType ), comparisonType.ConvertToString(), value );
        }

        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
        /// Implement this version of CreateChildControls if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <returns></returns>
        public virtual Control[] CreateChildControls( Type entityType, FilterField filterControl, FilterMode filterMode )
        {
            return CreateChildControls( entityType, filterControl );
        }

        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
        /// Implement this version of CreateChildControls if your DataFilterComponent works the same in all filter modes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        /// <returns>The array of new controls created to implement the filter.</returns>
        public virtual Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddl = ComparisonHelper.ComparisonControl( ComparisonHelper.StringFilterComparisonTypes );
            ddl.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( ddl );

            var tb = new TextBox();
            tb.ID = filterControl.ID + "_1";
            filterControl.Controls.Add( tb );

            return new Control[] { ddl, tb };
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// Implement this version of RenderControls if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="filterMode">The filter mode.</param>
        public virtual void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls, FilterMode filterMode )
        {
            RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// Implement this version of RenderControls if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public virtual void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            foreach ( var control in controls )
            {
                control.RenderControl( writer );
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets a formatted string representing the current filter control values.
        /// Implement this version of GetSelection if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual string GetSelection( Type entityType, Control[] controls, FilterMode filterMode )
        {
            return GetSelection( entityType, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// Implement this version of GetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>A formatted string.</returns>
        public virtual string GetSelection( Type entityType, Control[] controls )
        {
            string comparisonType = ( (DropDownList)controls[0] ).SelectedValue;
            string value = ( (TextBox)controls[1] ).Text;
            return string.Format( "{0}|{1}", comparisonType, value );
        }

        /// <summary>
        /// Sets the filter control values from a formatted string.
        /// Implement this version of SetSelection if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <param name="filterMode">The filter mode.</param>
        public virtual void SetSelection( Type entityType, Control[] controls, string selection, FilterMode filterMode )
        {
            SetSelection( entityType, controls, selection );
        }

        /// <summary>
        /// Sets the selection.
        /// Implement this version of SetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public virtual void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length >= 2 )
            {
                ( (DropDownList)controls[0] ).SelectedValue = options[0];
                ( (TextBox)controls[1] ).Text = options[1];
            }
        }

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>A Linq Expression that can be used to filter an IQueryable.</returns>
        public abstract Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection );

        #endregion

        #region Protected Methods

        #endregion

        #region Static Properties

        /// <summary>
        /// Registers Javascript to hide/show .js-filter-control child elements of a .js-filter-compare dropdown
        /// see RockWeb\Scripts\Rock\reportingInclude.js
        /// </summary>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        public void RegisterFilterCompareChangeScript( FilterField filterControl )
        {
            ReportingHelper.RegisterJavascriptInclude( filterControl );
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// 
    /// </summary>
    public enum FilterMode
    {
        /// <summary>
        /// Render the UI and process the filter as a simple filter
        /// This mode can be set if the filter just needs to be simple with minimal UI (like on a public page)
        /// </summary>
        SimpleFilter,

        /// <summary>
        /// Render and process as an advanced filter 
        /// This will be the mode when configuring as a Data Filter
        /// </summary>
        AdvancedFilter
    }

    #endregion
}