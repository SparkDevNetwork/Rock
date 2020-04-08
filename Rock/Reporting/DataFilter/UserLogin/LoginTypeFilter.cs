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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.UserLogin
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter User that are associated with a specific Login Type." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Login Type Filter" )]
    public class LoginTypeFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.UserLogin ).FullName; }
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
            return "Login Type";
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
    result='';
    var loginType = $('.js-loginType-dropdown', $content).find(':selected').text();
    if (loginType) {
     result =  'login type: ' + loginType;
  }
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
            string result = "Login Type";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 1 )
            {
                int loginTypeId = selectionValues[0].AsInteger();
                var loginType = EntityTypeCache.Get( loginTypeId );
                if ( loginType != null )
                {
                    result = "Login Type: " + loginType.FriendlyName;
                }
            }

            return result;
        }

        private RockDropDownList ddlLoginType = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            ddlLoginType = new RockDropDownList();
            ddlLoginType.CssClass = "js-loginType-dropdown";
            ddlLoginType.ID = filterControl.ID + "_ddlLoginType";
            ddlLoginType.Label = "Login Type";
            ddlLoginType.Help = "Select a specific Login Type";
            filterControl.Controls.Add( ddlLoginType );
            BindLoginType();

            return new Control[] { ddlLoginType };
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
            var loginTypeId = ( controls[0] as RockDropDownList ).SelectedValue;
            return loginTypeId;
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
                if ( selectionValues[0].AsIntegerOrNull().HasValue )
                {
                    ( controls[0] as RockDropDownList ).SetValue( selectionValues[0] );
                }
                else
                {
                    ( controls[0] as RockDropDownList ).SetValue( string.Empty );
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
            var rockContext = ( RockContext ) serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                UserLoginService userLoginService = new UserLoginService( rockContext );

                int? loginTypeId = selectionValues[0].AsIntegerOrNull();
                var userLoginQuery = userLoginService.Queryable();
                if ( !loginTypeId.HasValue )
                {
                    return null;
                }
                userLoginQuery = userLoginQuery.Where( a => a.EntityTypeId == loginTypeId );
                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.UserLogin>( userLoginQuery, parameterExpression, "a" );

                return extractedFilterExpression;
            }

            return null;
        }

        /// <summary>
        /// Binds the Login Type.
        /// </summary>
        private void BindLoginType()
        {
            ddlLoginType.Items.Clear();
            foreach ( var item in AuthenticationContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive )
                {
                    var entityType = item.Value.EntityType;
                    ddlLoginType.Items.Add( new ListItem( item.Metadata.ComponentName, entityType.Id.ToString() ) );
                }
            }

            ddlLoginType.Items.Insert( 0, new ListItem() );
        }

        #endregion
    }
}