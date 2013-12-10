//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using Rock.Reporting.DataTransform.Person;
using System.Linq.Expressions;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [Description( "Block to execute a linq command and display the result (if any)." )]
    public partial class TestLinqGrid : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            gReport.GridRebind += gReport_GridRebind;
            RunCommand();
        }

        void gReport_GridRebind( object sender, EventArgs e )
        {
            RunCommand();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void RunCommand()
        {
            //BindWithLinq();
            BindWithExpressions();
        }

        private void BindWithLinq()
        {
            using ( var context = new RockContext() )
            {
                var people = context.Set<Person>();
                var transactions = context.Set<FinancialTransaction>();

                var dataview = people.Where( p => p.LastName == "Smith" );

                var selection = dataview
                        .Select( p => new
                        {
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            LastTransaction =
                                transactions
                                    .Where( t => t.AuthorizedPersonId == p.Id )
                                    .Max( t => t.TransactionDateTime )
                        } );

                gReport.AutoGenerateColumns = true;
                gReport.DataSource = selection.ToList();
                gReport.DataBind();
            }
        }

        private void BindWithExpressions()
        {
            using ( var context = new RockContext() )
            {
                var people = context.Set<Person>();
                var transactions = context.Set<FinancialTransaction>();

                var dataview = people.Where( p => p.LastName == "Smith" );

                // Person Expressions
                ParameterExpression personParameter = Expression.Parameter(typeof(Person), "p");
                MemberExpression idProperty = Expression.Property(personParameter, "Id");
                MemberExpression firstNameProperty = Expression.Property(personParameter, "FirstName");
                MemberExpression lastNameProperty = Expression.Property(personParameter, "LastName");

                // Get LastTransactionDate Expressions
                ParameterExpression transactionParameter = Expression.Parameter(typeof(FinancialTransaction), "t");
                MemberExpression authorizedPersonIdProperty = Expression.Property(transactionParameter, "AuthorizedPersonId");
                MemberExpression transactionDateTime = Expression.Property(transactionParameter,"TransactionDateTime");

                MethodInfo whereMethod = GetWhereMethod();
                MethodInfo maxMethod = GetMaxMethod();

                var personIdCompare = new Expression[] { 
                    Expression.Constant(transactions), 
                    Expression.Lambda<Func<FinancialTransaction, bool>>( Expression.Equal(authorizedPersonIdProperty, Expression.Convert(idProperty, typeof(int?))), new ParameterExpression[] { transactionParameter } ) 
                };
                var transactionDate = Expression.Lambda<Func<FinancialTransaction, DateTime?>>( transactionDateTime, new ParameterExpression[] { transactionParameter } );
                var lastTransactionDate = Expression.Call( null, maxMethod, new Expression[] { Expression.Call( null, whereMethod, personIdCompare ), transactionDate } );

                // Create the dynamic type and get constructor info
                var dynamicFields = new Dictionary<string, Type>();
                dynamicFields.Add( "Entity_FirstName", typeof( string ) );
                dynamicFields.Add( "Entity_LastName", typeof( string ) );
                dynamicFields.Add( "Data_LastTransaction", typeof( DateTime? ) );
                Type dynamicType = Rock.Data.LinqRuntimeTypeBuilder.GetDynamicType( dynamicFields );
                ConstructorInfo methodFromHandle = dynamicType.GetConstructor( Type.EmptyTypes );

                // Bind the fields of the dynamic type to their expressions
                var bindings = new List<MemberBinding>();
                bindings.Add( Expression.Bind( dynamicType.GetField( "Entity_FirstName" ), firstNameProperty ) );
                bindings.Add( Expression.Bind( dynamicType.GetField( "Entity_LastName" ), lastNameProperty ) );
                bindings.Add( Expression.Bind( dynamicType.GetField( "Data_LastTransaction" ), lastTransactionDate ) );

                // Create the expression for selecting anonymous type
                Expression selector = Expression.Lambda( Expression.MemberInit( Expression.New( dynamicType.GetConstructor( Type.EmptyTypes ) ), bindings ), personParameter );

                Expression select = Expression.Call( typeof( Queryable ), "Select", new Type[] { dataview.ElementType, dynamicType }, Expression.Constant( dataview ), selector );

                var query = dataview.Provider.CreateQuery(select);

                // Get sql from expression and convert to datatable
                var dt = new Service().GetDataTable( query.ToString(), CommandType.Text, null );

                gReport.AutoGenerateColumns = true;
                gReport.DataSource = dt;
                gReport.DataBind();
            }

        }

        private MethodInfo GetWhereMethod()
        {
            Func<FinancialTransaction, bool> fake = element => default( bool );
            Expression<Func<IEnumerable<FinancialTransaction>, IEnumerable<FinancialTransaction>>> lamda = list => list.Where( fake );
            return ( lamda.Body as MethodCallExpression ).Method;
        }

        private MethodInfo GetMaxMethod()
        {
            Func<FinancialTransaction, DateTime?> fake = element => default( DateTime? );
            Expression<Func<IEnumerable<FinancialTransaction>, DateTime?>> lamda = list => list.Max( fake );
            return ( lamda.Body as MethodCallExpression ).Method;
        }

        #endregion

    }
}