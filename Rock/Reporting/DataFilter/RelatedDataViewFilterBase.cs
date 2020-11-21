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
using System.Web.UI;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// A base implementation of a Data View filter component that filters the target entity by a specified foreign-key property to match a set of candidate entities provided by another Data View.
    /// </summary>
    public abstract class RelatedDataViewFilterBase<TTargetEntity, TRelatedEntity> : DataFilterComponent, IRelatedChildDataView
        where TTargetEntity : IEntity
        where TRelatedEntity : IEntity
    {
        #region Abstract Methods

        #endregion

        #region Settings

        /// <summary>
        /// Settings for the RelatedDataViewFilter Data Filter Component.
        /// </summary>
        public class FilterSettings : SettingsStringBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FilterSettings"/> class.
            /// </summary>
            public FilterSettings()
            {
                //
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FilterSettings"/> class.
            /// </summary>
            /// <param name="settingsString">The settings string.</param>
            public FilterSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            /// <summary>
            /// The unique identifier for the Data View that returns the list of related entities used as the filter values.
            /// </summary>
            public Guid? DataViewGuid { get; set; }

            /// <summary>
            /// Indicates if the current settings are valid.
            /// </summary>
            /// <value>
            /// True if the settings are valid.
            /// </value>
            public override bool IsValid
            {
                get
                {
                    return this.DataViewGuid.HasValue;
                }
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                // Parameter 1: Data View
                this.DataViewGuid = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 ).AsGuidOrNull();
            }

            /// <summary>
            /// Gets an ordered set of property values that can be used to construct the
            /// settings string.
            /// </summary>
            /// <returns>
            /// An ordered collection of strings representing the parameter values.
            /// </returns>
            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( this.DataViewGuid.ToStringSafe() );

                return settings;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( TTargetEntity ).FullName; }
        }

        /// <summary>
        /// Get the name of the target entity that is returned in the result set.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetTargetEntityName()
        {
            return typeof( TTargetEntity ).Name.SplitCase();
        }

        /// <summary>
        /// Get the name of the related entity that is used to filter the result set.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetRelatedEntityName()
        {
            return typeof( TRelatedEntity ).Name.SplitCase();
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Related Data Views"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        public override string GetTitle( Type entityType )
        {
            return $"{ GetRelatedEntityName() } Data View";
        }

        /// <summary>
        /// Override this method to customise the DataViewItemPicker displayed for this filter by setting the Label text, Help text and other properties as necessary.
        /// </summary>
        /// <param name="picker"></param>
        protected virtual void OnConfigureDataViewItemPicker( DataViewItemPicker picker )
        {
            picker.Label = string.Format( "Is associated with any {0} in this Data View", GetRelatedEntityName() );
            picker.Help = string.Format( "A Data View that provides the set of {0} with which the {1} may be connected.", GetRelatedEntityName().Pluralize(), GetTargetEntityName() );
        }

        /// <summary>
        ///     Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        ///     will set the description of the filter to whatever is returned by this property.  If including script, the
        ///     controls parent container can be referenced through a '$content' variable that is set by the control before
        ///     referencing this property.
        /// </summary>
        /// <value>
        ///     The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            var script = @"
function() {
  var dataViewName = $('.js-data-view-picker', $content).find('.js-item-name-value').val().trim();  
  if (dataViewName == '') {
    dataViewName = '(none)';
  }
  result = 'Is associated with any <RelatedEntityName> in Data View: ' + dataViewName;  
  return result;
}
";

            script = script.Replace( "<RelatedEntityName>", GetRelatedEntityName() );

            return script;
        }

        /// <summary>
        /// Provides a user-friendly description of the specified filter values.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A string containing the user-friendly description of the settings.
        /// </returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var settings = new FilterSettings( selection );

            var relatedEntityName = GetRelatedEntityName();

            string result = $"Connected to { relatedEntityName }";

            if ( !settings.IsValid )
            {
                return result;
            }

            using ( var context = new RockContext() )
            {
                var dataView = new DataViewService( context ).Get( settings.DataViewGuid.GetValueOrDefault() );

                var dataViewName = ( dataView != null ? dataView.ToString() : string.Empty );

                result = $"Is associated with any { relatedEntityName } in Data View: { dataViewName }";
            }

            return result;
        }

        private const string _CtlDataView = "ddlDataView";

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField parentControl )
        {
            var ddlDataView = new DataViewItemPicker();

            ddlDataView.ID = parentControl.GetChildControlInstanceName( _CtlDataView );
            ddlDataView.CssClass = "js-data-view-picker";

            parentControl.Controls.Add( ddlDataView );

            // Populate the Data View Picker
            int entityTypeId = EntityTypeCache.Get( typeof( TRelatedEntity ) ).Id;
            ddlDataView.EntityTypeId = entityTypeId;

            this.OnConfigureDataViewItemPicker( ddlDataView );

            return new Control[] { ddlDataView };
        }

        /// <summary>
        /// Gets the selection.
        /// Implement this version of GetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>
        /// A formatted string.
        /// </returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var ddlDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );

            var settings = new FilterSettings();

            settings.DataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( ddlDataView.SelectedValue );

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// Implement this version of SetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var ddlDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );

            var settings = new FilterSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            ddlDataView.SetValue( DataComponentSettingsHelper.GetDataViewId( settings.DataViewGuid ) );
        }

        /// <summary>
        /// Gets the related data view identifier.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public int? GetRelatedDataViewId( Control[] controls )
        {
            if ( controls == null )
            {
                return null;
            }

            var ddlDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );
            if ( ddlDataView == null )
            {
                return null;
            }

            return ddlDataView.SelectedValueAsId();
        }
        #endregion
    }
}