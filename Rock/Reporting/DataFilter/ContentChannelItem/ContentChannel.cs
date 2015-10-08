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
    [Description( "Filter Content Channel Items by Content Channel" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Content Channel Filter" )]
    public class ContentChannel : DataFilterComponent
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
            return "Content Channel";
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
  var contentChannelName = $('.js-content-channel-picker', $content).find(':selected').text()
  var result = 'Content Channel: ' + contentChannelName;

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
            string result = "Content Channel";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var contentChannel = new ContentChannelService( new RockContext() ).Get( selectionValues[0].AsGuid() );

                if ( contentChannel != null )
                {
                    result = string.Format( "Content Channel: {0}", contentChannel.Name );
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
            RockDropDownList contentChannelPicker = new RockDropDownList();
            contentChannelPicker.CssClass = "js-content-channel-picker";
            contentChannelPicker.ID = filterControl.ID + "_contentChannelPicker";
            contentChannelPicker.Label = "Content Channel";

            contentChannelPicker.Items.Clear();
            var contentChannelList = new ContentChannelService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
            foreach ( var contentChannel in contentChannelList )
            {
                contentChannelPicker.Items.Add( new ListItem( contentChannel.Name, contentChannel.Id.ToString() ) );
            }

            filterControl.Controls.Add( contentChannelPicker );

            return new Control[] { contentChannelPicker };
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
            int? contentChannelId = ( controls[0] as RockDropDownList ).SelectedValueAsId();
            Guid? contentChannelGuid = null;
            var contentChannel = new ContentChannelService( new RockContext() ).Get( contentChannelId ?? 0 );
            if ( contentChannel != null )
            {
                contentChannelGuid = contentChannel.Guid;
            }

            return contentChannelGuid.ToString();
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
                var contentChannel = new ContentChannelService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                if ( contentChannel != null )
                {
                    ( controls[0] as RockDropDownList ).SetValue( contentChannel.Id );
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
                var contentChannel = new ContentChannelService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                int? contentChannelId = null;
                if ( contentChannel != null )
                {
                    contentChannelId = contentChannel.Id;
                }

                var qry = new ContentChannelItemService( (RockContext)serviceInstance.Context ).Queryable()
                    .Where( p => p.ContentChannelId == contentChannelId );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.ContentChannelItem>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}