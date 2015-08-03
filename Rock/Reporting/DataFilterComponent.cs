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
    /// 
    /// </summary>
    public abstract class DataFilterComponent : Component
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.  Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public abstract string AppliesToEntityType { get; }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public virtual string Section
        {
            get { return "Additional Filters"; }
        }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
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
        /// Gets or sets the a dictionary of options that filter may or may not use (depending on filter)
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public virtual Dictionary<string, object> Options {get; set;}

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public abstract string GetTitle( Type entityType );

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public virtual string GetClientFormatSelection( Type entityType )
        {
            return string.Format( "'{0} ' + $('select', $content).find(':selected').text() + ' \\'' + $('input', $content).val() + '\\''", GetTitle( entityType ) );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
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
        /// Creates the child controls.
        /// Implement this version of CreateChildControls if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <returns></returns>
        public virtual Control[] CreateChildControls( Type entityType, FilterField filterControl, FilterMode filterMode )
        {
            return CreateChildControls( entityType, filterControl );
        }

        /// <summary>
        /// Implement this version of CreateChildControls if your DataFilterComponent works the same in all filter modes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <returns></returns>
        public virtual Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddl = ComparisonHelper.ComparisonControl( ComparisonHelper.StringFilterComparisonTypes );
            ddl.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( ddl );

            var tb = new TextBox();
            tb.ID = filterControl.ID + "_1";
            filterControl.Controls.Add( tb );

            return new Control[2] { ddl, tb };
        }

        /// <summary>
        /// Renders the controls.
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
        /// Renders the controls.
        /// Implement this version of RenderControls if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public virtual void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            foreach ( var control in controls )
            {
                control.RenderControl( writer );
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets the selection.
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
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public virtual string GetSelection( Type entityType, Control[] controls )
        {
            string comparisonType = ( (DropDownList)controls[0] ).SelectedValue;
            string value = ( (TextBox)controls[1] ).Text;
            return string.Format( "{0}|{1}", comparisonType, value );
        }

        /// <summary>
        /// Sets the selection.
        /// Implement this version of SetSelection if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
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
        /// Gets the expression.
        /// Implement this version of GetExpression if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public abstract Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection );

        #endregion

        #region Protected Methods

        #endregion

        #region Static Properties

        /// <summary>
        /// Registers Javascript to hide/show .js-filter-control child elements of a .js-filter-compare dropdown
        /// see RockWeb\Scripts\Rock\reportingInclude.js
        /// </summary>
        /// <value>
        /// </value>
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