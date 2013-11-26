using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Reporting
{
    public static class SelectNewExpressionExtractor
    {
        private class SelectExpressionExtractorVisitor : ExpressionVisitor
        {
            public List<Expression> SelectArguments { get; private set; }

            protected override Expression VisitLambda<T>( Expression<T> node )
            {
                if ( node.Body is System.Linq.Expressions.NewExpression )
                {
                    NewExpression newExpression = node.Body as System.Linq.Expressions.NewExpression;
                    SelectArguments = newExpression.Arguments.ToList();
                }

                return base.VisitLambda<T>( node );
            }

            protected override Expression VisitParameter( ParameterExpression node )
            {
                return base.VisitParameter( node );
            }
        }

        private class SelectExpressionInjectorVisitor : ExpressionVisitor
        {
            private MethodCallExpression _methodCallExpression;

            public MemberInitExpression Result { get; private set; }
            
            public SelectExpressionInjectorVisitor(MethodCallExpression methodCallExpression)
            {
                _methodCallExpression = methodCallExpression;
            }

            protected override Expression VisitLambda<T>( Expression<T> node )
            {
                return base.VisitLambda<T>( node );
            }

            protected override Expression VisitNew( NewExpression nex )
            {
                var args = nex.Arguments.ToList();
                args.Add( _methodCallExpression );

                List<DynamicProperty> props = new List<DynamicProperty>();
                MemberBinding[] bindings = new MemberBinding[args.Count];

                foreach (Expression arg in args)
                {
                    props.Add( new DynamicProperty( arg.ToString(), arg.Type ) );  
                }

                var type = System.Linq.Dynamic.DynamicExpression.CreateClass( props );

                for ( int i = 0; i < bindings.Length; i++ )
                    bindings[i] = Expression.Bind( type.GetProperty( props[i].Name ), args[i] );
                
                Result = Expression.MemberInit( Expression.New( type ), bindings );

                return ( Result as MemberInitExpression ).NewExpression;

                //return base.VisitNew( nex );

                //nex = Expression.New()

                //return base.VisitNew(nex);

            }

            protected override Expression VisitMemberInit( MemberInitExpression node )
            {
                return Result;
                //return base.VisitMemberInit( node );
            }

            protected override Expression VisitBlock( BlockExpression node )
            {
                return base.VisitBlock( node );
            }

            protected override Expression VisitMethodCall( MethodCallExpression node )
            {
                return base.VisitMethodCall( node );
            }

            protected override Expression VisitParameter( ParameterExpression node )
            {
                return base.VisitParameter( node );
            }
        } 
       
        

        
        public static MethodCallExpression ExtractMethodCallExpression(IQueryable qry)
        {
            SelectExpressionExtractorVisitor selectExpressionVisitor = new SelectExpressionExtractorVisitor();
            selectExpressionVisitor.Visit( qry.Expression );
            return selectExpressionVisitor.SelectArguments.OfType<MethodCallExpression>().FirstOrDefault();
        }

        public static void InjectMethodCallExpression(IQueryable qry, MethodCallExpression methodCallExpression)
        {
            SelectExpressionInjectorVisitor selectExpressionVisitor = new SelectExpressionInjectorVisitor( methodCallExpression );
            selectExpressionVisitor.Visit( qry.Expression );

            //var expression = selectExpressionVisitor.Result;

            //qry = qry.OfType<object>().Select( expression as MethodCallExpression );

            //TODO
        }
    }
}
