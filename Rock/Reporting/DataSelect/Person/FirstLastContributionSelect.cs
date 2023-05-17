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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock;
using Rock.Web.Cache;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Selects Last Contribution Date for a Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person Last Contribution" )]
    [Rock.SystemGuid.EntityTypeGuid( "2F7330D9-04FF-4F8B-9B7D-1479F9027C5D")]
    public class LastContributionSelect : FirstLastContributionSelect
    {
        /// <summary>
        /// Gets the first or last.
        /// </summary>
        /// <value>
        /// The first or last.
        /// </value>
        internal override FirstLastContributionSelect.FirstLast FirstOrLast
        {
            get { return FirstLast.Last; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Description( "Selects First Contribution Date for a Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person First Contribution" )]
    [Rock.SystemGuid.EntityTypeGuid( "6C7C6C7A-3355-4FBF-9D39-AEB50E46A7C1")]
    public class FirstContributionSelect : FirstLastContributionSelect
    {
        /// <summary>
        /// Gets the first or last.
        /// </summary>
        /// <value>
        /// The first or last.
        /// </value>
        internal override FirstLastContributionSelect.FirstLast FirstOrLast
        {
            get { return FirstLast.First; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class FirstLastContributionSelect : DataSelectComponent
    {
        /// <summary>
        /// 
        /// </summary>
        internal enum FirstLast
        {
            First,
            Last
        }

        /// <summary>
        /// Gets the first or last.
        /// </summary>
        /// <value>
        /// The first or last.
        /// </value>
        internal abstract FirstLast FirstOrLast { get; }

        #region Properties

        /// <summary>
        /// Gets the name of the entity type.
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
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return base.Section;
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
                return FirstOrLast.ConvertToString() + "TransactionDateTime";
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
            get { return typeof( DateTime? ); }
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
                return FirstOrLast.ConvertToString() + " Contribution Date";
            }
        }

        #endregion

        #region Methods

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
            return FirstOrLast.ConvertToString() + " Contribution";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            string[] selections = selection.Split( '|' );
            string accountIds = selections[0] ?? string.Empty;

            // Default this to true if it is not available so the behavior of existing dataviews (< v9.0) are not changed without user input.
            bool useSundayDate = true;
            if ( selections.Count() > 1 )
            {
                useSundayDate = selections[1].AsBooleanOrNull() ?? true;
            }

            string transactionDateProperty = useSundayDate ? "SundayDate" : "TransactionDateTime";

            // transactions
            var transactionDetails = context.Set<FinancialTransactionDetail>();

            // t
            ParameterExpression transactionDetailParameter = Expression.Parameter( typeof( FinancialTransactionDetail ), "t" );

            // t.Transaction
            MemberExpression transactionProperty = Expression.Property( transactionDetailParameter, "Transaction" );

            // t.Transaction.AuthorizedPersonAlias
            MemberExpression authorizedPersonAliasProperty = Expression.Property( transactionProperty, "AuthorizedPersonAlias" );

            // t.Transaction.AuthorizedPersonAlias.PersonId
            MemberExpression authorizedPersonIdProperty = Expression.Property( authorizedPersonAliasProperty, "PersonId" );

            // t.Transaction.AuthorizedPersonAlias.PersonId == Convert(p.Id)
            Expression whereClause = Expression.Equal( authorizedPersonIdProperty, Expression.Convert( entityIdProperty, typeof( int ) ) );

            // t.Transaction.TransactionTypeValueId
            MemberExpression transactionTypeValueIdProperty = Expression.Property( transactionProperty, "TransactionTypeValueId" );

            int transactionTypeContributionId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            // t.Transaction.TransactionTypeValueId == transactionTypeContributionId
            whereClause = Expression.And( whereClause, Expression.Equal( transactionTypeValueIdProperty, Expression.Constant( transactionTypeContributionId ) ) );

            // get the selected AccountId(s).  If there are any, limit to transactions that for that Account
            if ( !string.IsNullOrWhiteSpace( accountIds ) )
            {
                // accountIds
                var selectedAccountGuidList = accountIds.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
                var selectedAccountIdList = FinancialAccountCache.GetByGuids( selectedAccountGuidList ).Select( a => a.Id ).ToList();

                if ( selectedAccountIdList.Count() > 0 )
                {
                    // t.AccountId
                    MemberExpression accountIdProperty = Expression.Property( transactionDetailParameter, "AccountId" );

                    // accountIds.Contains(t.AccountId)
                    Expression selectedAccountIds = Expression.Constant( selectedAccountIdList );
                    Expression containsExpression = Expression.Call( selectedAccountIds, "Contains", new Type[] { }, accountIdProperty );

                    // t.authorizedPersonId == Convert(p.Id) && accountIds.Contains(t.AccountId)
                    whereClause = Expression.And( whereClause, containsExpression );
                }
            }

            // t => t.Transaction.AuthorizedPersonId == Convert(p.Id)
            var compare = new Expression[] { 
                    Expression.Constant(transactionDetails), 
                    Expression.Lambda<Func<FinancialTransactionDetail, bool>>( whereClause, new ParameterExpression[] { transactionDetailParameter } ) 
                };

            // transactions.Where( t => t.Transaction.AuthorizedPersonId == Convert(p.Id)
            Expression whereExpression = Expression.Call( typeof( Queryable ), "Where", new Type[] { typeof( FinancialTransactionDetail ) }, compare );

            // t.Transaction.TransactionDateTime
            MemberExpression transactionDateTime = Expression.Property( transactionProperty, transactionDateProperty );

            // t => t.Transaction.transactionDateTime
            var transactionDate = Expression.Lambda<Func<FinancialTransactionDetail, DateTime?>>( transactionDateTime, new ParameterExpression[] { transactionDetailParameter } );

            // transaction.Where( t => t.Transaction.AuthorizedPersonId == Convert(p.Id).Max( t => t.Transaction.transactionDateTime)
            string methodName = FirstOrLast == FirstLast.Last ? "Max" : "Min";
            Expression maxMinExpression = Expression.Call( typeof( Queryable ), methodName, new Type[] { typeof( FinancialTransactionDetail ), typeof( DateTime? ) }, whereExpression, transactionDate );

            return maxMinExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            AccountPicker accountPicker = new AccountPicker();
            accountPicker.AllowMultiSelect = true;
            accountPicker.ID = parentControl.ID + "_accountPicker";
            accountPicker.Label = "Account";
            accountPicker.Help = string.Format(
                "Pick accounts to show the {0} time the person made a contribution into any of those accounts. Leave blank if you don't want to limit it to specific accounts.",
                FirstOrLast.ConvertToString().ToLower() );
            parentControl.Controls.Add( accountPicker );

            RockCheckBox cbUseSundayDate = new RockCheckBox();
            cbUseSundayDate.ID = parentControl.ID + "_cbUseSundayDate";
            cbUseSundayDate.Label = "Use Sunday Date";
            cbUseSundayDate.Help = "Use the Sunday Date instead of the actual transaction date.";
            parentControl.Controls.Add( cbUseSundayDate );

            return new System.Web.UI.Control[] { accountPicker, cbUseSundayDate };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            string delimitedAccountGuids = string.Empty;
            string useSundayDate = string.Empty;

            if ( controls.Count() > 0 )
            {
                AccountPicker accountPicker = controls[0] as AccountPicker;
                if ( accountPicker != null )
                {
                    var accountIds = accountPicker.SelectedValues.AsIntegerList(); 
                    var accountGuids = FinancialAccountCache.GetByIds( accountIds ).Select( a => a.Guid );
                    delimitedAccountGuids = accountGuids.Select( a => a.ToString() ).ToList().AsDelimited( "," );
                }

                if ( controls.Count() > 1 )
                {
                    RockCheckBox cbUseSundayDate = controls[1] as RockCheckBox;
                    if ( cbUseSundayDate != null )
                    {
                        useSundayDate = cbUseSundayDate.Checked.ToString();
                    }
                }

                if ( delimitedAccountGuids != string.Empty || useSundayDate != string.Empty )
                {
                    return $"{delimitedAccountGuids}|{useSundayDate}";
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            string[] selections = selection.Split( '|' );

            if ( controls.Count() > 0 )
            {
                AccountPicker accountPicker = controls[0] as AccountPicker;
                if ( accountPicker != null )
                {
                    string[] selectionAccountGuidValues = selections[0].Split( ',' );
                    var accountList = new List<FinancialAccountCache>();
                    foreach ( string accountGuid in selectionAccountGuidValues )
                    {
                        var account = FinancialAccountCache.Get( accountGuid.AsGuid() );
                        if ( account != null )
                        {
                            accountList.Add( account );
                        }
                    }

                    accountPicker.SetValuesFromCache( accountList );
                }
            }

            if ( controls.Count() > 1 )
            {
                RockCheckBox cbUseSundayDate = controls[1] as RockCheckBox;
                if ( cbUseSundayDate != null && selections.Count() > 1 )
                {
                    cbUseSundayDate.Checked = selections[1].AsBooleanOrNull() ?? false;
                }
            }
        }

        #endregion
    }
}
