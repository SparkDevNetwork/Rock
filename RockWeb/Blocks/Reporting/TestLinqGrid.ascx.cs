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
            gReport.AutoGenerateColumns = true;
            //gReport.CreatePreviewColumns( typeof( Person ) );

            using ( var context = new RockContext() )
            {
                var people = context.Set<Person>();
                var transactions = context.Set<FinancialTransaction>();

                var dataview = people.Where( p => p.LastName == "Turner" );
                //var parents = service.Transform(people, new Rock.Reporting.DataTransform.Person.ParentTransform());




                //var selection = dataview
                //    .Select( p => new
                //    {
                //        FirstName = p.FirstName,
                //        LastName = p.LastName,
                //        LastTransaction =
                //            transactions
                //                .GroupBy( t => t.AuthorizedPersonId )
                //                .Select( t => new
                //                {
                //                    PersonId = t.Key,
                //                    TransactionDateTime = t.Max( d => d.TransactionDateTime )
                //                } )
                //                .Where( q => q.PersonId == p.Id )
                //                .Select( q => q.TransactionDateTime )
                //                .FirstOrDefault()
                //    } );
                //gReport.DataSource = selection.ToList();




                //var selection = dataview
                //    .Select( p => new
                //    {
                //        FirstName = p.FirstName,
                //        LastName = p.LastName,
                //        LastTransaction =
                //            transactions
                //                .Where( t => t.AuthorizedPersonId == p.Id )
                //                .Max( t => t.TransactionDateTime )
                //    } );
                //gReport.DataSource = selection.ToList();



                var dynamicFields = new Dictionary<string, Type>();
                dynamicFields.Add( "FirstName", typeof( string ) );
                dynamicFields.Add( "LastName", typeof( string ) );
                //dynamicFields.Add( "LastTransaction", typeof( DateTime ) );

                Type dynamicType = Rock.Data.LinqRuntimeTypeBuilder.GetDynamicType( dynamicFields );

                ParameterExpression sourceItem = Expression.Parameter( dataview.ElementType, "x" );

                Expression<Func<Person, DateTime>> lastTransactionSelect = a => transactions.Where( t => t.AuthorizedPersonId == a.Id && t.TransactionDateTime.HasValue ).Max( t => t.TransactionDateTime.Value );

                var bindings = new List<MemberBinding>();
                bindings.Add( Expression.Bind( dynamicType.GetField( "FirstName" ), Expression.Property( sourceItem, dataview.ElementType.GetProperty( "FirstName" ) ) ) );
                bindings.Add( Expression.Bind( dynamicType.GetField( "LastName" ), Expression.Property( sourceItem, dataview.ElementType.GetProperty( "LastName" ) ) ) );
               // bindings.Add( Expression.Bind( dynamicType.GetField( "LastTransaction" ), Expression. lastTransactionSelect, sourceItem ).Compile() ) );

                Expression selector = Expression.Lambda( Expression.MemberInit( Expression.New( dynamicType.GetConstructor( Type.EmptyTypes ) ), bindings ), sourceItem );
                var query = dataview.Provider.CreateQuery(
                    Expression.Call(
                        typeof( Queryable ),
                        "Select",
                        new Type[] { dataview.ElementType, dynamicType },
                 Expression.Constant( dataview ), selector ) ).AsNoTracking();

                DataTable dataTable = new Service().GetDataTable( query.ToString(), CommandType.Text, null );

                gReport.DataSource = dataTable;

                //gReport.DataSource = dataview.Select( CreateNewStatement( "FirstName, LastName" ) ).ToList();

                gReport.DataBind();
            }



        }

        private Func<Person, Person> CreateNewStatement( string fields )
        {
            // input parameter "o"
            var xParameter = Expression.Parameter( typeof( Person ), "o" );

            // new statement "new Data()"
            var xNew = Expression.New( typeof( Person ) );

            // create initializers
            var bindings = fields.Split( ',' ).Select( o => o.Trim() )
                .Select( o => {

                    // property "Field1"
                    var mi = typeof( Person ).GetProperty( o );

                    // original value "o.Field1"
                    var xOriginal = Expression.Property( xParameter, mi );

                    // set value "Field1 = o.Field1"
                    return Expression.Bind( mi, xOriginal );
                }
            );

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit( xNew, bindings );

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var lambda = Expression.Lambda<Func<Person, Person>>( xInit, xParameter );

            // compile to Func<Data, Data>
            return lambda.Compile();
        }

        #endregion

    }
}