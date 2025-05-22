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
using System.Linq.Expressions;
#if WEBFORMS
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
#endif

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Net;
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
        public virtual string Section => "Additional Filters";

        /// <summary>
        /// Set this to show descriptive text that can help explain how complex filters work or offer assistance on possibly other filters that have better performance.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public override string Description => string.Empty;

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
        [Obsolete]
        [RockObsolete( "17.0" )]
        public virtual Dictionary<string, object> Options
        {
            get
            {
#if WEBFORMS
                if ( HttpContext.Current != null )
                {
                    return HttpContext.Current.Items[$"{this.GetType().FullName}:Options"] as Dictionary<string, object>;
                }
#endif

                return _nonHttpContextOptions;
            }

            set
            {
#if WEBFORMS
                if ( HttpContext.Current != null )
                {
                    HttpContext.Current.Items[$"{this.GetType().FullName}:Options"] = value;
                }
                else
#endif
                {
                    _nonHttpContextOptions = value;
                }
            }
        }

        /// <summary>
        /// The URL that will be used to load the Obsidian component. This may
        /// be a path prefixed with "~/" instead of a full absolute URL. This should
        /// return <c>null</c> to indicate Obsidian is not supported and an empty
        /// string to indicate it is supported but no UI is required.
        /// </summary>
        public virtual string ObsidianFileUrl => null;

        /// <summary>
        /// The _Options when HttpContext.Current is null
        /// NOTE: ThreadStatic is per thread, but ASP.NET threads are ThreadPool threads, so they will be used again.
        /// see https://www.hanselman.com/blog/ATaleOfTwoTechniquesTheThreadStaticAttributeAndSystemWebHttpContextCurrentItems.aspx
        /// So be careful and only use the [ThreadStatic] trick if absolutely necessary
        /// </summary>
        [ThreadStatic]
        [Obsolete]
        [RockObsolete( "17.0" )]
        private static Dictionary<string, object> _nonHttpContextOptions;
   
        #endregion

        #region Configuration

        /// <summary>
        /// Gets the component data that will be provided to the Obsidian component
        /// when it is initialized. This should include representations of the current
        /// values as well as any additional data required to initialize the UI.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> of the entity this applies to, such as <see cref="Model.Person"/>.</param>
        /// <param name="selection">The selection string from the database.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>A dictionary of strings that will be provided to the Obsidian component.</returns>
        [RockInternal( "17.0" )]
        public virtual Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the selection string that will be saved to the database from
        /// the data returned by the Obsidian component.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> of the entity this applies to, such as <see cref="Model.Person"/>.</param>
        /// <param name="data">The data the was returned by the Obsidian component.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>The string of text that represents the selection which will be written to the database.</returns>
        [RockInternal( "17.0" )]
        public virtual string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the related data view that this filter references. This is used
        /// to ensure data integrity so that the related data view can't be
        /// deleted while another data view is referencing it.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> of the entity this applies to, such as <see cref="Model.Person"/>.</param>
        /// <param name="selection">The selection string from the database.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>The identifier of the related data view or <c>null</c> if there isn't one.</returns>
        [RockInternal( "18.0" )]
        public virtual int? GetRelatedDataViewId( Type entityType, string selection, RockContext rockContext )
        {
            return null;
        }

#if WEBFORMS
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
        /// <returns>A formatted string representing the filter settings.</returns>
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
        /// <returns>A formatted string representing the filter settings.</returns>
        public virtual string GetSelection( Type entityType, Control[] controls )
        {
            string comparisonType = ( ( DropDownList ) controls[0] ).SelectedValue;
            string value = ( ( TextBox ) controls[1] ).Text;
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
        /// <param name="selection">A formatted string representing the filter settings.</param>
        public virtual void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length >= 2 )
            {
                ( ( DropDownList ) controls[0] ).SelectedValue = options[0];
                ( ( TextBox ) controls[1] ).Text = options[1];
            }
        }
#endif

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
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>A Linq Expression that can be used to filter an IQueryable.</returns>
        public abstract Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection );

        #endregion
    }
}
