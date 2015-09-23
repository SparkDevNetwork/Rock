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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.ContentChannelItem
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter Content Channel Items by Content Channel Type" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Content Channel Type Filter" )]
    public class ContentChannelType : DataFilterComponent
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
            return "Content Channel Type";
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
  var contentChannelTypeName = $('.js-content-channel-type-picker', $content).find(':selected').text()
  var result = 'Content Channel Type: ' + contentChannelTypeName;

  return result;
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
            string result = "Content Channel Type";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( selectionValues[0].AsGuid() );

                if ( contentChannelType != null )
                {
                    result = string.Format( "Content Channel type: {0}", contentChannelType.Name );
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockDropDownList contentChannelTypePicker = new RockDropDownList();
            contentChannelTypePicker.CssClass = "js-content-channel-type-picker";
            contentChannelTypePicker.ID = filterControl.ID + "_contentChannelTypePicker";
            contentChannelTypePicker.Label = "Content Channel Type";

            contentChannelTypePicker.Items.Clear();
            var contentChannelTypeList = new ContentChannelTypeService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
            foreach ( var contentChannelType in contentChannelTypeList )
            {
                contentChannelTypePicker.Items.Add( new ListItem( contentChannelType.Name, contentChannelType.Id.ToString() ) );
            }

            filterControl.Controls.Add( contentChannelTypePicker );

            return new Control[] { contentChannelTypePicker };
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
            int? contentChannelTypeId = ( controls[0] as RockDropDownList ).SelectedValueAsId();
            Guid? contentChannelTypeGuid = null;
            var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( contentChannelTypeId ?? 0 );
            if ( contentChannelType != null )
            {
                contentChannelTypeGuid = contentChannelType.Guid;
            }

            return contentChannelTypeGuid.ToString();
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
                var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                if ( contentChannelType != null )
                {
                    ( controls[0] as RockDropDownList ).SetValue( contentChannelType.Id );
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
                var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                int? contentChannelTypeId = null;
                if ( contentChannelType != null )
                {
                    contentChannelTypeId = contentChannelType.Id;
                }

                var qry = new ContentChannelItemService( (RockContext)serviceInstance.Context ).Queryable()
                    .Where( p => p.ContentChannelTypeId == contentChannelTypeId );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.ContentChannelItem>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}