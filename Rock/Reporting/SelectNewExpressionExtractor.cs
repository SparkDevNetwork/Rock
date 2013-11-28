using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            private int _position;
            
            public SelectExpressionInjectorVisitor(MethodCallExpression methodCallExpression, int position)
            {
                _methodCallExpression = methodCallExpression;
                _position = position;
            }

            protected override Expression VisitNew( NewExpression nex )
            {
                var args = nex.Arguments.ToList();
                //MethodCallExpression orig = args[_position] as MethodCallExpression;
                //MethodCallExpression newMethodCallExpression = _methodCallExpression;
                //orig.Update( newMethodCallExpression, _methodCallExpression.Arguments );
                args[_position] = _methodCallExpression;
                var result = nex.Update( args );

                var members = nex.Members.ToList();
                members[_position] = _methodCallExpression.Type;

                result = Expression.New( nex.Constructor, args, members );

               
                 return result;
            }

            protected override Expression VisitMember( MemberExpression node )
            {
                return base.VisitMember( node );
            }

            protected override MemberAssignment VisitMemberAssignment( MemberAssignment node )
            {
                return base.VisitMemberAssignment( node );
            }

            protected override MemberBinding VisitMemberBinding( MemberBinding node )
            {
                return base.VisitMemberBinding( node );
            }

            protected override Expression VisitMemberInit( MemberInitExpression node )
            {
                return base.VisitMemberInit( node );
            }

            protected override MemberListBinding VisitMemberListBinding( MemberListBinding node )
            {
                return base.VisitMemberListBinding( node );
            }

            protected override MemberMemberBinding VisitMemberMemberBinding( MemberMemberBinding node )
            {
                return base.VisitMemberMemberBinding( node );
            }
        } 
        
        public static MethodCallExpression ExtractMethodCallExpression(IQueryable qry)
        {
            SelectExpressionExtractorVisitor selectExpressionVisitor = new SelectExpressionExtractorVisitor();
            selectExpressionVisitor.Visit( qry.Expression );
            return selectExpressionVisitor.SelectArguments.OfType<MethodCallExpression>().FirstOrDefault();
        }

        public static void InjectMethodCallExpression(IQueryable qry, MethodCallExpression methodCallExpression, int position)
        {
            SelectExpressionInjectorVisitor selectExpressionVisitor = new SelectExpressionInjectorVisitor( methodCallExpression, position );
            selectExpressionVisitor.Visit( qry.Expression );
        }
    }
}
