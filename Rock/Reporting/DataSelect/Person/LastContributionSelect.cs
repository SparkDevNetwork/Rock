//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Selects Last Contribution Date for a Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person Last Contribution" )]
    public class LastContributionSelect : DataSelectComponent<Rock.Model.Person>
    {

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
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "LastTransactionDateTime";
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
            get { return typeof(DateTime?); }
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
                return "Last Contribution Date";
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
        ///   </value>
        public override string GetTitle(Type entityType)
        {
            return "Last Contribution";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, Expression entityIdProperty, string selection )
        {
            // transactions
            var transactionDetails = context.Set<FinancialTransactionDetail>();

            // t
            ParameterExpression transactionDetailParameter = Expression.Parameter( typeof( FinancialTransactionDetail ), "t" );

            // t.Transaction
            MemberExpression transactionProperty = Expression.Property( transactionDetailParameter, "Transaction" );

            // t.Transaction.AuthorizedPersonId
            MemberExpression authorizedPersonIdProperty = Expression.Property( transactionProperty, "AuthorizedPersonId" );

            // t.Transaction.AuthorizedPersonId == Convert(p.Id)
            Expression whereClause = Expression.Equal(authorizedPersonIdProperty, Expression.Convert(entityIdProperty, typeof(int?)));

            // get the selected AccountId(s).  If there are any, limit to transactions that for that Account
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                // accountIds
                var selectedAccountIdList = selection.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsInteger() ?? 0 ).ToList();
                if ( selectedAccountIdList.Count() > 0 )
                {
                    // t.AccountId
                    MemberExpression accountIdProperty = Expression.Property( transactionDetailParameter, "AccountId" );

                    // accountIds.Contains(t.AccountId)
                    Expression selectedAccountIds = Expression.Constant(selectedAccountIdList);
                    Expression containsExpression = Expression.Call(selectedAccountIds, "Contains", new Type[] {}, accountIdProperty );

                    // t.authorizedPersonId == Convert(p.Id) && accountIds.Contains(t.AccountId)
                    whereClause = Expression.And(whereClause, containsExpression );
                }
            }

            // t => t.Transaction.AuthorizedPersonId == Convert(p.Id)
            var compare = new Expression[] { 
                    Expression.Constant(transactionDetails), 
                    Expression.Lambda<Func<FinancialTransactionDetail, bool>>( whereClause , new ParameterExpression[] { transactionDetailParameter } ) 
                };

            // transactions.Where( t => t.Transaction.AuthorizedPersonId == Convert(p.Id)
            Expression whereExpression = Expression.Call( typeof( Queryable ), "Where", new Type[] { typeof( FinancialTransactionDetail ) }, compare );

            // t.Transaction.TransactionDateTime
            MemberExpression transactionDateTime = Expression.Property( transactionProperty, "TransactionDateTime" );

            // t => t.Transaction.transactionDateTime
            var transactionDate = Expression.Lambda<Func<FinancialTransactionDetail, DateTime?>>( transactionDateTime, new ParameterExpression[] { transactionDetailParameter } );

            // transaction.Where( t => t.Transaction.AuthorizedPersonId == Convert(p.Id).Max( t => t.Transaction.transactionDateTime)
            Expression maxExpression = Expression.Call( typeof( Queryable ), "Max", new Type[] { typeof( FinancialTransactionDetail ), typeof( DateTime? ) }, whereExpression, transactionDate );

            return maxExpression;
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
            accountPicker.Help = "Pick accounts to show the last time the person made a contribution into any of those accounts. Leave blank if you don't want to limit it to specific accounts.";
            parentControl.Controls.Add( accountPicker );

            return new System.Web.UI.Control[] { accountPicker };
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            // TODO: 
            return base.FormatSelection( selection );
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the widget is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// The client format script.
        ///   </value>
        public override string GetClientFormatSelection()
        {
            // TODO: 
            return base.GetClientFormatSelection();
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            // TODO: 
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            if ( controls.Count() == 1 )
            {
                AccountPicker accountPicker = controls[0] as AccountPicker;
                if ( accountPicker != null )
                {
                    return accountPicker.SelectedValueAsId().ToString();
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
            if ( controls.Count() == 1 )
            {
                AccountPicker accountPicker = controls[0] as AccountPicker;
                if ( accountPicker != null )
                {
                    var account = new FinancialAccountService().Get( selection.AsInteger() ?? 0 );
                    accountPicker.SetValue( account );
                }
            }
        }

        #endregion

    }
}
