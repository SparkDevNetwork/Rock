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

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on User login" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In User Login Filter" )]
    public class InUserLoginFilter : DataFilterComponent
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
            return "In User Login";
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
    var result =  $('.js-loginStatus-radio input:first', $content).is(':checked') ? 'user has login' : 'User Does Not have a login';
  
  var loggedInSince = $('.js-loggedSince-date-range, $content).find(.js-slidingdaterange-text-value').val();
   if (loggedInSince) {
     result = result + ', last logged in Date Range: ' + loggedInSince;
  }

  var createdSince = $('.js-createdSince-date-range, $content).find(.js-slidingdaterange-text-value').val();
   if (createdSince) {
     result = result + ', created in Date Range: ' + createdSince;
  }

  var loginType = $('.js-loginType-dropdown', $content).find(':selected').text();
  if (loginType) {
     result = result + ', login type: ' + loginType;
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
            string result = "Does Not have a login";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 4 )
            {
                var loginStatus = selectionValues[0].AsIntegerOrNull();
                if ( loginStatus == 1 )
                {
                    result = "Has login";
                }
                result = string.Format( "User {0}", result );

                var hasLoggedSinceValue = SlidingDateRangePicker.FormatDelimitedValues( selectionValues[1].Replace( ',', '|' ) );
                if ( !string.IsNullOrEmpty( hasLoggedSinceValue ) )
                {
                    result += string.Format( ", last logged in Date Range: {0}", hasLoggedSinceValue );
                }

                var createdSinceValue = SlidingDateRangePicker.FormatDelimitedValues( selectionValues[2].Replace( ',', '|' ) );
                if ( !string.IsNullOrEmpty( createdSinceValue ) )
                {
                    result += string.Format( ", created in Date Range: {0}", createdSinceValue );
                }

                int? loginTypeId = selectionValues[3].AsIntegerOrNull();

                if ( loginTypeId.HasValue )
                {
                    var loginEntityType = EntityTypeCache.Read( loginTypeId.Value );
                    result += string.Format( ", login type: {0}", loginEntityType.FriendlyName );
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
            RockRadioButtonList loginStatus = new RockRadioButtonList();
            loginStatus.CssClass = "js-registration-type";
            loginStatus.ID = filterControl.ID + "_loginStatus";
            loginStatus.RepeatDirection = RepeatDirection.Horizontal;
            loginStatus.Label = "Login Status";
            loginStatus.Help = "Choose whether to filter by the person who has user login or not.";
            loginStatus.AddCssClass( "js-loginStatus-radio" );
            loginStatus.Items.Add( new ListItem( "Has User Login", "1" ) );
            loginStatus.Items.Add( new ListItem( "Doesnot have a Login", "2" ) );
            filterControl.Controls.Add( loginStatus );

            SlidingDateRangePicker hasLoggedDateRangePicker = new SlidingDateRangePicker();
            hasLoggedDateRangePicker.ID = filterControl.ID + "_hasSince";
            hasLoggedDateRangePicker.AddCssClass( "js-loggedSince-date-range" );
            hasLoggedDateRangePicker.Label = "Has Logged Since";
            hasLoggedDateRangePicker.Help = "The date range for the User Last Logged In";
            filterControl.Controls.Add( hasLoggedDateRangePicker );

            SlidingDateRangePicker createdDateRangePicker = new SlidingDateRangePicker();
            createdDateRangePicker.ID = filterControl.ID + "_createdSince";
            createdDateRangePicker.AddCssClass( "js-createdSince-date-range" );
            createdDateRangePicker.Label = "Created Since";
            createdDateRangePicker.Help = "The date range of Person Created In";
            filterControl.Controls.Add( createdDateRangePicker );


            ddlLoginType = new RockDropDownList();
            ddlLoginType.CssClass = "js-loginType-dropdown";
            ddlLoginType.ID = filterControl.ID + "_ddlLoginType";
            ddlLoginType.Label = "Login Type";
            ddlLoginType.Help = "Select a specific Login Type";
            filterControl.Controls.Add( ddlLoginType );
            BindLoginType();
            var controls = new Control[4] { loginStatus, hasLoggedDateRangePicker, createdDateRangePicker, ddlLoginType };

            var defaultDelimitedValues = createdDateRangePicker.DelimitedValues.Replace( "|", "," );
            SetSelection(
                entityType,
                controls,
                string.Format( "2|{0}|{1}|{2}", defaultDelimitedValues, defaultDelimitedValues, string.Empty ) );
            return controls;
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
            var loginStatus = ( controls[0] as RockRadioButtonList );
            var hasLoggedDateRangePicker = ( controls[1] as SlidingDateRangePicker );
            var createdDateRangePicker = ( controls[2] as SlidingDateRangePicker );
            var ddlLoginType = ( controls[3] as RockDropDownList );

            var hasLoggedSinceValues = hasLoggedDateRangePicker.DelimitedValues.Replace( '|', ',' );
            var createdDateRangeValues = createdDateRangePicker.DelimitedValues.Replace( '|', ',' );

            return loginStatus.SelectedValue + "|" + hasLoggedSinceValues + "|" + createdDateRangeValues + "|" + ddlLoginType.SelectedValue;
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
            if ( selectionValues.Length >= 4 )
            {

                var ddlLoginType = ( controls[3] as RockDropDownList );

                int? loginStatusId = selectionValues[0].AsIntegerOrNull();
                if ( loginStatusId.HasValue )
                {
                    ( controls[0] as RockRadioButtonList ).SetValue( loginStatusId );
                }

                var hasLoggedDateRangePicker = ( controls[1] as SlidingDateRangePicker );
                if ( selectionValues.Length >= 2 )
                {
                    hasLoggedDateRangePicker.DelimitedValues = selectionValues[1].Replace( ',', '|' );
                }

                var createdDateRangePicker = ( controls[2] as SlidingDateRangePicker );
                if ( selectionValues.Length >= 3 )
                {
                    createdDateRangePicker.DelimitedValues = selectionValues[2].Replace( ',', '|' );
                }

                if ( selectionValues[3].AsIntegerOrNull().HasValue )
                {
                    ( controls[3] as RockDropDownList ).SetValue( selectionValues[3] );
                }
                else
                {
                    ( controls[3] as RockDropDownList ).SetValue( string.Empty );
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
            if ( selectionValues.Length >= 2 )
            {
                UserLoginService userLoginService = new UserLoginService( ( RockContext ) serviceInstance.Context );
                var userLoginQuery = userLoginService.Queryable();

                int loginStatusId = selectionValues[0].AsInteger();
                var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable();
                if ( loginStatusId == 2 )
                {
                    qry = qry
                        .Where( p => !userLoginQuery.Any( xx => xx.PersonId == p.Id ) );
                }
                else
                {
                    if ( selectionValues.Length >= 2 )
                    {
                        string slidingDelimitedValues = selectionValues[1].Replace( ',', '|' );
                        var hasLoggedInSince = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );
                        if ( hasLoggedInSince.Start.HasValue )
                        {
                            userLoginQuery = userLoginQuery.Where( a => a.LastLoginDateTime >= hasLoggedInSince.Start );
                        }
                        if ( hasLoggedInSince.End.HasValue )
                        {
                            userLoginQuery = userLoginQuery.Where( a => a.LastLoginDateTime <= hasLoggedInSince.End );
                        }
                    }

                    if ( selectionValues.Length >= 3 )
                    {
                        string slidingDelimitedValues = selectionValues[2].Replace( ',', '|' );
                        var createdDateRangeValues = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );
                        if ( createdDateRangeValues.Start.HasValue )
                        {
                            userLoginQuery = userLoginQuery.Where( a => a.CreatedDateTime >= createdDateRangeValues.Start );
                        }
                        if ( createdDateRangeValues.End.HasValue )
                        {
                            userLoginQuery = userLoginQuery.Where( a => a.CreatedDateTime <= createdDateRangeValues.End );
                        }
                    }
                    int? loginTypeId = selectionValues[3].AsIntegerOrNull();
                    if ( loginTypeId.HasValue)
                    {

                        userLoginQuery = userLoginQuery.Where( a => a.EntityTypeId == loginTypeId );
                    }

                    qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                        .Where( p => userLoginQuery.Any( xx => xx.PersonId == p.Id ) );
                }
                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

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