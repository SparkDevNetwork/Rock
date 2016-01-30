using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace church.ccv.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Selects First Contribution Record for a Person which can be used as a merge field for Report Lava Selects" )]
    [Export( typeof( Rock.Reporting.DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person First Contribution Record" )]
    public class FirstContributionRecordSelect : Rock.Reporting.DataSelect.Person.FirstContributionSelect
    {
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
                return "CCV Custom";
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
                return "FirstTransaction";
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
            get { return typeof( Rock.Model.FinancialTransaction ); }
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
                return "First Contribution Record";
            }
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            var baseControls = base.CreateChildControls( parentControl );

            HelpBlock helpBlock = new HelpBlock();
            helpBlock.ID = parentControl.ID + "_helpBlock";
            helpBlock.Text = "Use this field along with some Lava Select fields to display the various details about the transaction.  For example: Set 'Show in Grid' to false, then add a Lava field with <code>{{ FirstContributionRecord.SourceTypeValue.Value }}</code>";

            parentControl.Controls.Add( helpBlock );

            var result = baseControls.ToList();
            result.Add( helpBlock );

            return baseControls;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression GetExpression( Rock.Data.RockContext context, System.Linq.Expressions.MemberExpression entityIdProperty, string selection )
        {
            int transactionTypeContributionId = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            var financialTransactionQryFirst = new FinancialTransactionService( context ).Queryable().Where( a => a.TransactionTypeValueId == transactionTypeContributionId );

            var selectionParts = selection.Split( '|' );

            if ( selectionParts.Length > 0 )
            {
                // accountIds
                var accountsSelection = selectionParts[0];
                var selectedAccountGuidList = accountsSelection.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
                var selectedAccountIdList = new FinancialAccountService( context ).GetByGuids( selectedAccountGuidList ).Select( a => a.Id ).ToList();
                financialTransactionQryFirst.Where( a => a.TransactionDetails.Any( x => selectedAccountIdList.Contains( x.AccountId ) ) );
            }

            var financialTransactionQry = new FinancialTransactionService( context ).Queryable();

            var personFirstTransactionQry = new PersonService( context ).Queryable().Select( p =>
                financialTransactionQry.Where(
                    a => a.Id == financialTransactionQryFirst.Where( x => x.AuthorizedPersonAlias.PersonId == p.Id ).Select( x => x.Id ).Min() )
                    .FirstOrDefault()
                );

            var selectExpression = SelectExpressionExtractor.Extract( personFirstTransactionQry, entityIdProperty, "p" );

            return selectExpression;
        }
    }
}
