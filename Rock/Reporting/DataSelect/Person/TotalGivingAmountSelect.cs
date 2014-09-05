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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the total giving amount of the person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Total Giving Amount" )]
    public class TotalGivingAmountSelect : DataSelectComponent
    {
        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "Total Giving";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get
            {
                return typeof( decimal? );
            }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Total Giving";
            }
        }

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
            return "Total Giving";
        }
        
        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            var comparisonControl = ComparisonHelper.ComparisonControl( ComparisonType.LessThan | ComparisonType.GreaterThanOrEqualTo | ComparisonType.EqualTo );
            comparisonControl.ID = parentControl.ID + "_0";
            parentControl.Controls.Add( comparisonControl );

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();

            NumberBox numberBoxAmount = new NumberBox();
            numberBoxAmount.PrependText = globalAttributes.GetValue( "CurrencySymbol" ) ?? "$";
            numberBoxAmount.NumberType = ValidationDataType.Currency;
            numberBoxAmount.ID = parentControl.ID + "_1";
            numberBoxAmount.Label = "Amount";

            parentControl.Controls.Add( numberBoxAmount );

            AccountPicker accountPicker = new AccountPicker();
            accountPicker.AllowMultiSelect = true;
            accountPicker.ID = parentControl.ID + "_accountPicker";
            accountPicker.AddCssClass( "js-account-picker" );
            accountPicker.Label = "Accounts";
            parentControl.Controls.Add( accountPicker );

            DateRangePicker dateRangePicker = new DateRangePicker();
            dateRangePicker.ID = parentControl.ID + "_2";
            dateRangePicker.Label = "Date Range";
            dateRangePicker.Required = true;
            parentControl.Controls.Add( dateRangePicker );

            var controls = new Control[4] { comparisonControl, numberBoxAmount, accountPicker, dateRangePicker };

            SetSelection( controls, string.Format( "{0}||||", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

            return controls;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            string comparisonType = ( (DropDownList)controls[0] ).SelectedValue;
            decimal? amount = ( controls[1] as NumberBox ).Text.AsDecimal();

            var accountIdList = ( controls[2] as AccountPicker ).SelectedValuesAsInt().ToList();
            string accountGuids = string.Empty;
            var accounts = new FinancialAccountService( new RockContext() ).GetByIds( accountIdList );
            if ( accounts != null && accounts.Any() )
            {
                accountGuids = accounts.Select( a => a.Guid ).ToList().AsDelimited( "," );
            }

            DateRangePicker dateRangePicker = controls[3] as DateRangePicker;
            return string.Format( "{0}|{1}|{2}|{3}|{4}", comparisonType, amount, dateRangePicker.LowerValue, dateRangePicker.UpperValue, accountGuids );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 4 )
            {
                var comparisonControl = controls[0] as DropDownList;
                var numberBox = controls[1] as NumberBox;
                var accountPicker = controls[2] as AccountPicker;
                var dateRangePicker = controls[3] as DateRangePicker;

                comparisonControl.SetValue( selectionValues[0] );
                decimal? amount = selectionValues[1].AsDecimal();
                if ( amount.HasValue )
                {
                    numberBox.Text = amount.Value.ToString( "F2" );
                }
                else
                {
                    numberBox.Text = string.Empty;
                }

                dateRangePicker.LowerValue = selectionValues[2].AsDateTime();
                dateRangePicker.UpperValue = selectionValues[3].AsDateTime();

                if ( selectionValues.Length >= 5 )
                {
                    var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                    var accounts = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids );
                    if ( accounts != null && accounts.Any() )
                    {
                        accountPicker.SetValues( accounts );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression GetExpression( Data.RockContext context, System.Linq.Expressions.MemberExpression entityIdProperty, string selection )
        {
            // TODO
            return Expression.Constant(50.00M);
        }
    }
}
