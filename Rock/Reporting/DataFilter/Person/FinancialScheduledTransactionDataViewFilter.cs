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
using System.Web.UI;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///     A Data Filter to select Donor by their Transactions from a Financial Transaction View.
    /// </summary>
    [Description( "Select Person by their Scheduled Transactions from a Financial Scheduled Transaction View." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Financial Scheduled Transaction View" )]
    [Rock.SystemGuid.EntityTypeGuid( "00095E53-54D2-4A76-957D-36FE436454B4" )]
    public class FinancialScheduledTransactionDataViewFilter :  RelatedDataViewFilterBase<Rock.Model.Person, Rock.Model.FinancialScheduledTransaction>
    {
        private const string _CtlCombineGiving = "cbCombineGiving";

        #region Overrides

        /// <inheritdoc/>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var ddlDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );
            var cbCombineGiving = controls.GetByName<RockCheckBox>( _CtlCombineGiving );

            var settings = new SelectionConfig();

            settings.DataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( ddlDataView.SelectedValue );
            settings.CombineGiving = cbCombineGiving.Checked;

            return settings.ToSelectionString();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var ddlDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );
            var cbCombineGiving = controls.GetByName<RockCheckBox>( _CtlCombineGiving );

            var settings = new SelectionConfig( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            ddlDataView.SetValue( DataComponentSettingsHelper.GetDataViewId( settings.DataViewGuid ) );
            cbCombineGiving.Checked = settings.CombineGiving;
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl, FilterMode filterMode )
        {
            var ddlDataView = new DataViewItemPicker();
            ddlDataView.ID = filterControl.GetChildControlInstanceName( _CtlDataView );
            ddlDataView.Label = "Financial Scheduled Transaction Data View";
            ddlDataView.Help = "A Data View that provides the set of Financial Scheduled Transactions with which the Person may be connected.";
            ddlDataView.CssClass = "js-data-view-picker";
            ddlDataView.EntityTypeId = EntityTypeCache.Get( typeof( FinancialScheduledTransaction ) ).Id;
            filterControl.Controls.Add( ddlDataView );

            RockCheckBox cbCombineGiving = new RockCheckBox();
            cbCombineGiving.ID = filterControl.GetChildControlInstanceName( _CtlCombineGiving );
            cbCombineGiving.Label = "Include Individuals in the Same Giving Group";
            cbCombineGiving.CssClass = "js-combine-giving";
            cbCombineGiving.Help = "Combine individuals in the same giving group when reporting the list of individuals.";
            filterControl.Controls.Add( cbCombineGiving );

            return new Control[] { ddlDataView, cbCombineGiving };
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new SelectionConfig( selection );

            var context = (RockContext)serviceInstance.Context;

            // Get the Financial Transaction Data View.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the Person's Financial Transaction.
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( context );

            var financialScheduledTransactionQuery = financialScheduledTransactionService.Queryable();

            if ( dataView != null )
            {
                financialScheduledTransactionQuery = DataComponentSettingsHelper.FilterByDataView( financialScheduledTransactionQuery, dataView, financialScheduledTransactionService );
            }

            var transactionPersonsKey = financialScheduledTransactionQuery.Select( a => a.AuthorizedPersonAliasId );
            // Get all of the Person corresponding to the qualifying Financial Transactions.
            var qry = new PersonService( context ).Queryable();

            if ( settings.CombineGiving )
            {
                var transactionPersonsGivingGroupIds = financialScheduledTransactionQuery
                    .Where( f => f.AuthorizedPersonAlias.Person.GivingGroupId.HasValue )
                    .Select( a => a.AuthorizedPersonAlias.Person.GivingGroupId );

                qry = qry.Where( g => g.Aliases.Any( k => transactionPersonsKey.Contains( k.Id ) )
                    || transactionPersonsGivingGroupIds.Contains( g.GivingGroupId ) );
            }
            else
            {
                qry = qry.Where( g => g.Aliases.Any( k => transactionPersonsKey.Contains( k.Id ) ) );
            }

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "g" );

            return extractedFilterExpression;
        }

        #endregion

        /// <summary>
        /// Class SelectionConfig.
        /// </summary>
        protected class SelectionConfig : FilterSettings
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            /// <param name="selection"></param>
            public SelectionConfig( string selection ) : base( selection )
            {
            }

            /// <summary>
            /// Gets or sets a value indicating whether to include individuals in the same giving group
            /// as the filtered persons.
            /// </summary>
            /// <value>
            ///   <c>true</c> if [combine giving]; otherwise, <c>false</c>.
            /// </value>
            public bool CombineGiving { get; set; }

            /// <inheritdoc />
            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( this.DataViewGuid.ToStringSafe() );
                settings.Add( CombineGiving.ToString() );

                return settings;
            }

            /// <inheritdoc/>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                this.DataViewGuid = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 ).AsGuidOrNull();
                this.CombineGiving = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 1 ).AsBoolean();
            }
        }
    }
}