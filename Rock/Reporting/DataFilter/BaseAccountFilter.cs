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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "_ by Account" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Account Filter" )]
    public abstract class BaseAccountFilter<T> : DataFilterComponent where T : IEntity
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
            get { return typeof( T ).FullName; }
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
            return "Account";
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
  
  var accountPicker = $('.js-account-picker', $content);
  var accountNames = accountPicker.find('.selected-names').text()
  var result = 'Account: ' + accountNames;

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
            string result = "Account";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var accountGuids = selectionValues[0].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                var accountNames = FinancialAccountCache.GetByGuids( accountGuids ).Select( a => a.Name ).ToList().AsDelimited( "," );

                if ( accountNames != null )
                {
                    result = string.Format( "Account: {0}", accountNames );
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
            AccountPicker accountPicker = new AccountPicker();

            // NOTE: This filter is already designed to handle either Multiselect or SingleSelect mode, so all you have to is set this to true or false
            accountPicker.AllowMultiSelect = true;
            
            accountPicker.ID = filterControl.ID + "_accountPicker";
            accountPicker.AddCssClass( "js-account-picker" );
            accountPicker.Label = "Account";
            filterControl.Controls.Add( accountPicker );

            return new Control[] { accountPicker };
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
            var accountIdList = ( controls[0] as AccountPicker ).SelectedValuesAsInt().ToList();
            string accountGuids = string.Empty;
            var accounts = FinancialAccountCache.GetByIds( accountIdList );
            if ( accounts != null && accounts.Any() )
            {
                accountGuids = accounts.Select( a => a.Guid ).ToList().AsDelimited( "," );
            }

            return accountGuids;
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
                var accountGuids = selectionValues[0].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                var accounts = FinancialAccountCache.GetByGuids( accountGuids );
                if ( accounts != null && accounts.Any() )
                {
                    ( controls[0] as AccountPicker ).SetValuesFromCache( accounts );
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
        public abstract override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection );

        #endregion
    }
}