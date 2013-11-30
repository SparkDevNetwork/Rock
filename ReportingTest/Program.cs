using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock;
using Rock.Reporting;

namespace ReportingTest
{
    class Program
    {
        public class DataSelectData
        {
            public int PersonId;
            public object Data;
        }

        public class DataSelectorInfo
        {
            public IQueryable<IEntity> Qry;
            public Expression<Func<IEntity, DataSelectData>> SelectExpression;
            public string PropertyName;
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
                    Data = new
                        {
                            LastTransactionDateTime = ( a as FinancialTransaction ).TransactionDateTime
                        }
                };

                string lastTranSelectPropertyName = "LastTransactionDateTime";


                // Sample DataSelect #2
                IQueryable<IEntity> firstTransactionQry = new FinancialTransactionService().Queryable()
                    .OrderBy( o => o.TransactionDateTime );

                Expression<Func<IEntity, DataSelectData>> firstTranSelect = a => new DataSelectData
                {
                    PersonId = ( a as FinancialTransaction ).AuthorizedPersonId ?? 0,
                    Data = new
                        {
                            FirstTransactionDateTime = ( a as FinancialTransaction ).TransactionDateTime
                        }
                };

                string firstTranSelectPropertyName = "FirstTransactionDateTime";
                

                // Sample DataSelect #3
                Guid groupTypeFamily = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

                IQueryable<IEntity> familyNameQry = new GroupMemberService().Queryable()
                    .Where( a => a.Group.GroupType.Guid == groupTypeFamily );

                Expression<Func<IEntity, DataSelectData>> familyNameSelect = a => new DataSelectData
                {
                    PersonId = ( a as GroupMember ).PersonId,
                    Data = new
                        {
                            GroupName = ( a as GroupMember ).Group.Name
                        }
                };

                string familyNameSelectPropertyName = "GroupName";

                // Reporting Side
                var nullQry = new PersonService().Queryable().Where( a => 1 == 2 );
                Expression<Func<IEntity, DataSelectData>> nullSelect = a => new DataSelectData
                {
                    PersonId = 0,
                    Data = null
                };

                List<DataSelectorInfo> dataSelectList = new List<DataSelectorInfo>();
                dataSelectList.Add( new DataSelectorInfo { Qry = firstTransactionQry, SelectExpression = firstTranSelect, PropertyName = firstTranSelectPropertyName } );
                dataSelectList.Add( new DataSelectorInfo { Qry = lastTransactionQry, SelectExpression = lastTranSelect, PropertyName = lastTranSelectPropertyName } );
                dataSelectList.Add( new DataSelectorInfo { Qry = familyNameQry, SelectExpression = familyNameSelect, PropertyName = familyNameSelectPropertyName } );

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


                /* 
                 * Linq to Entities: See  http://msdn.microsoft.com/en-us/library/bb386964(v=vs.110).aspx 
                 * Supported and Unsupported LINQ Methods in LINQ to Entities: See http://msdn.microsoft.com/en-us/library/bb738550(v=vs.110).aspx
                 * 
                */

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

                var personQry = new PersonService().Queryable().Take( 1000 );
                var reportQry = personQry.Select( expressionFunc );

                Console.SetBufferSize( 120, 3000 );
                Console.SetWindowSize( 120, 30 );
                var list = reportQry.ToList();

                var entityFields = new List<EntityField>(); // Rock.Reporting.EntityHelper.GetEntityFields( personQry.ElementType, false );

                foreach ( var item in list )
                {
                    var entity = item.GetPropertyValue( "Entity" );
                    string fieldValues = string.Empty;
                    foreach (var field in entityFields)
                    {
                        fieldValues += "|" + entity.GetPropertyValue( field.Name );
                    }
                    
                    string dataSelectValues = string.Empty;
                    int dataItemIndex = 0;
                    foreach (var dataItem in dataSelectList)
                    {
                        var dataObject = item.GetPropertyValue( "Data" + dataItemIndex.ToString());
                        if ( dataObject != null )
                        {
                            dataSelectValues += "|" + dataObject.GetPropertyValue( dataSelectList[dataItemIndex].PropertyName );
                        }
                        else
                        {
                            dataSelectValues += "|";
                        }
                        dataItemIndex++;
                    }
                    Console.WriteLine( fieldValues + dataSelectValues );
                }

                Console.ReadLine();

            }
        }
    }
}
