using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace ReportingTest
{
    class Program
    {
        public class DataSelectData
        {
            public int PersonId;
            public string Data;
        }

        public class DataSelectorInfo
        {
            public IQueryable<IEntity> Qry;
            public Expression<Func<IEntity, DataSelectData>> SelectExpression;
        }

        static void Main( string[] args )
        {
            using ( new UnitOfWorkScope() )
            {

                DateTime epoch2000 = new DateTime( 2000, 1, 1 );
               

                // Sample DataSelect #1
                IQueryable<IEntity> lastTransactionQry = new FinancialTransactionService().Queryable()
                    .OrderByDescending( o => o.TransactionDateTime );

                Expression<Func<IEntity, DataSelectData>> lastTranSelect = a => new DataSelectData
                {
                    PersonId = ( a as FinancialTransaction ).AuthorizedPersonId ?? 0,
                    Data = ( a as FinancialTransaction ).Summary
                };

                // Sample DataSelect #2
                IQueryable<IEntity> firstTransactionQry = new FinancialTransactionService().Queryable()
                    .OrderBy( o => o.TransactionDateTime );

                Expression<Func<IEntity, DataSelectData>> firstTranSelect = a => new DataSelectData
                {
                    PersonId = ( a as FinancialTransaction ).AuthorizedPersonId ?? 0,
                    Data = ( a as FinancialTransaction ).Summary
                };

                // Reporting Side
                var nullQry = new PersonService().Queryable().Where( a => 1 == 2 );
                Expression<Func<IEntity, DataSelectData>> nullSelect = a => new DataSelectData
                {
                    PersonId = 0,
                    Data = null
                };

                List<DataSelectorInfo> dataSelectList = new List<DataSelectorInfo>();
                dataSelectList.Add( new DataSelectorInfo { Qry = firstTransactionQry, SelectExpression = firstTranSelect } );
                dataSelectList.Add( new DataSelectorInfo { Qry = lastTransactionQry, SelectExpression = lastTranSelect } );

                IQueryable<IEntity> DataQry0 = nullQry;
                Expression<Func<IEntity, DataSelectData>> SelectExpression0 = nullSelect;
                IQueryable<IEntity> DataQry1 = nullQry;
                Expression<Func<IEntity, DataSelectData>> SelectExpression1 = nullSelect;
                IQueryable<IEntity> DataQry2 = nullQry;
                Expression<Func<IEntity, DataSelectData>> SelectExpression2 = nullSelect;
                IQueryable<IEntity> DataQry3 = nullQry;
                Expression<Func<IEntity, DataSelectData>> SelectExpression3 = nullSelect;

                for ( int i = 0; i < dataSelectList.Count(); i++ )
                {
                    switch ( i )
                    {
                        case 0:
                            {
                                DataQry0 = dataSelectList[i].Qry;
                                SelectExpression0 = dataSelectList[i].SelectExpression;
                            }
                            break;
                        case 1:
                            {
                                DataQry1 = dataSelectList[i].Qry;
                                SelectExpression1 = dataSelectList[i].SelectExpression;
                            }
                            break;
                        case 2:
                            {
                                DataQry2 = dataSelectList[i].Qry;
                                SelectExpression2 = dataSelectList[i].SelectExpression;
                            }
                            break;
                        case 3:
                            {
                                DataQry3 = dataSelectList[i].Qry;
                                SelectExpression3 = dataSelectList[i].SelectExpression;
                            }
                            break;

                    }
                }

                

                // reportqry Select
                Expression<Func<IEntity, object>> expressionFunc0 = a => new
                {
                    Entity = a
                };

                Expression<Func<IEntity, object>> expressionFunc1 = a => new
                {
                    Entity = a,
                    Data0 = DataQry0.Select( SelectExpression0 ).Where( x => x.PersonId == a.Id ).Select( s => s.Data ).FirstOrDefault(),
                };

                Expression<Func<IEntity, object>> expressionFunc2 = a => new
                {
                    Entity = a,
                    Data0 = DataQry0.Select( SelectExpression0 ).Where( x => x.PersonId == a.Id ).Select( s => s.Data ).FirstOrDefault(),
                    Data1 = DataQry1.Select( SelectExpression1 ).Where( x => x.PersonId == a.Id ).Select( s => s.Data ).FirstOrDefault(),
                };

                Expression<Func<IEntity, object>> expressionFunc3 = a => new
                {
                    Entity = a,
                    Data0 = DataQry0.Select( SelectExpression0 ).Where( x => x.PersonId == a.Id ).Select( s => s.Data ).FirstOrDefault(),
                    Data1 = DataQry1.Select( SelectExpression1 ).Where( x => x.PersonId == a.Id ).Select( s => s.Data ).FirstOrDefault(),
                    Data2 = DataQry2.Select( SelectExpression2 ).Where( x => x.PersonId == a.Id ).Select( s => s.Data ).FirstOrDefault(),
                };

                List<Expression<Func<IEntity, object>>> expressionFunctions = new List<Expression<Func<IEntity, object>>>();
                expressionFunctions.Add( expressionFunc0 );
                expressionFunctions.Add( expressionFunc1 );
                expressionFunctions.Add( expressionFunc2 );
                expressionFunctions.Add( expressionFunc3 );

                Expression<Func<IEntity, object>> expressionFunc = expressionFunctions[dataSelectList.Count()];

                var personQry = new PersonService().Queryable().Take( 10 );
                var qry2 = personQry.Select( expressionFunc );

                var list = qry2.ToList();

            }
        }

        private static object SqlFunctions( FinancialTransaction financialTransaction )
        {
            throw new NotImplementedException();
        }
    }
}
