// Taken from MIT licensed LinqKit project: https://github.com/scottksmith95/LINQKit
//
// We are not using the NuGet package to avoid the confusion of multiple extensions
// methods that has come about as the package has grown. Such as multiple AsExpandable()
// in different namespaces, AsExpandableEF(), AsExpandableCore(), etc. Most of these all
// do the same thing. This alone is confusing enough. But if you use the AsExpandable()
// from the LinqKit.Core namespace, then certain features don't work.
//
// To avoid this confusion, we have included the relevant code directly in Rock. The
// original code has not seen significant changes in the past few years, so we consider
// this relatively safe. There is an open enhancement request to Visual Studio to
// add support for ignoring extension methods from certain namespaces. We are hopeful
// this would allow us to also ignore specific types from certain namespaces. If so we
// could eventually switch to the official LinqKit. Or in the future LinqKit may drop
// EF6 support and clean up all the different extension methods.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

#if NET6_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#endif

namespace Rock.Data.LinqKit
{
    /// <summary>
    /// An IQueryable wrapper that allows us to visit the query's expression tree just before LINQ to SQL gets to it.
    /// This is based on the excellent work of Tomas Petricek: http://tomasp.net/blog/linq-expand.aspx
    /// </summary>
#if NET6_0_OR_GREATER
    internal class ExpandableQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IOrderedQueryable, IAsyncEnumerable<T>
#else
    internal class ExpandableQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IOrderedQueryable, IDbAsyncEnumerable<T>
#endif
    {
        readonly IQueryProvider _provider;
        readonly IQueryable<T> _inner;

        internal IQueryable<T> InnerQuery => _inner; // Original query, that we're wrapping

        internal ExpandableQuery( IQueryable<T> inner, Func<Expression, Expression> queryOptimizer )
        {
            _inner = inner;
#if NET6_0_OR_GREATER
            var queryCompiler = GetQueryCompiler(inner);
#endif
            _provider =
#if NET6_0_OR_GREATER
                queryCompiler != null ? (IQueryProvider)new ExpandableIncludableQueryProvider<T>(this, queryOptimizer, queryCompiler) :
#endif
                new ExpandableQueryProvider<T>( this, queryOptimizer );
        }

        Expression IQueryable.Expression => _inner.Expression;

        Type IQueryable.ElementType => typeof( T );

        IQueryProvider IQueryable.Provider => _provider;

        /// <summary> IQueryable enumeration </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        /// <summary>
        /// IQueryable string presentation.
        /// </summary>
        public override string ToString() { return _inner.ToString(); }

#if NET6_0_OR_GREATER
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_inner is IAsyncEnumerable<T>)
            {
                return ((IAsyncEnumerable<T>)_inner).GetAsyncEnumerator(cancellationToken);
            }

            throw new InvalidOperationException();
        }
#else
        /// <summary> Enumerator for async-await </summary>
        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            var asyncEnumerable = _inner as IDbAsyncEnumerable<T>;
            if ( asyncEnumerable != null )
            {
                return asyncEnumerable.GetAsyncEnumerator();
            }
            return new ExpandableDbAsyncEnumerator<T>( _inner.GetEnumerator() );
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }
#endif

#if NET6_0_OR_GREATER
        private static IQueryCompiler GetQueryCompiler(IQueryable<T> query)
        {
            if (query is ExpandableQuery<T> expandableQuery)
            {
                return GetQueryCompiler(expandableQuery.InnerQuery);
            }
            if (query.Provider is EntityQueryProvider)
            {
                return (IQueryCompiler)typeof(EntityQueryProvider).GetField("_queryCompiler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(query.Provider);
            }
            return null;
        }
#endif
    }

    internal class ExpandableQueryOfClass<T> : ExpandableQuery<T>
        where T : class
    {
        public ExpandableQueryOfClass( IQueryable<T> inner, Func<Expression, Expression> queryOptimizer ) : base( inner, queryOptimizer )
        {
        }

#if NET6_0_OR_GREATER
        public IQueryable<T> Include<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath)
        {
            return ((IQueryable<T>)InnerQuery.Include(navigationPropertyPath)).AsExpandable();
        }
#else
        public IQueryable<T> Include( string path )
        {
            return InnerQuery.Include( path ).AsExpandable();
        }
#endif
    }

#if NET6_0_OR_GREATER
    class ExpandableIncludableQueryProvider<T> : EntityQueryProvider
    {
        private readonly IAsyncQueryProvider _innerProvider;

        internal ExpandableIncludableQueryProvider(ExpandableQuery<T> query, Func<Expression, Expression> queryOptimizer, IQueryCompiler queryCompiler)
            : base(queryCompiler)
        {
            _innerProvider = new ExpandableQueryProvider<T>(query, queryOptimizer);
        }

        public override IQueryable CreateQuery(Expression expression) => _innerProvider.CreateQuery(expression);
        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression) => _innerProvider.CreateQuery<TElement>(expression);
        public override object Execute(Expression expression) => _innerProvider.Execute(expression);
        public override TResult Execute<TResult>(Expression expression) => _innerProvider.Execute<TResult>(expression);

        public override TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default) => _innerProvider.ExecuteAsync<TResult>(expression, cancellationToken);
    }
#endif

    class ExpandableQueryProvider<T> : IQueryProvider
#if NET6_0_OR_GREATER
        , IAsyncQueryProvider
#else
        , IDbAsyncQueryProvider
#endif
    {
        readonly ExpandableQuery<T> _query;
        readonly Func<Expression, Expression> _queryOptimizer;

        internal ExpandableQueryProvider( ExpandableQuery<T> query, Func<Expression, Expression> queryOptimizer )
        {
            _query = query;
            _queryOptimizer = queryOptimizer;
        }

        // The following four methods first call ExpressionExpander to visit the expression tree, then call
        // upon the inner query to do the remaining work.
        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>( Expression expression )
        {
            var expanded = expression.Expand();
            var optimized = _queryOptimizer( expanded );
            return _query.InnerQuery.Provider.CreateQuery<TElement>( optimized ).AsExpandable();
        }

        IQueryable IQueryProvider.CreateQuery( Expression expression )
        {
            return _query.InnerQuery.Provider.CreateQuery( expression.Expand() );
        }

        TResult IQueryProvider.Execute<TResult>( Expression expression )
        {
            var expanded = expression.Expand();
            var optimized = _queryOptimizer( expanded );
            return _query.InnerQuery.Provider.Execute<TResult>( optimized );
        }

        object IQueryProvider.Execute( Expression expression )
        {
            var expanded = expression.Expand();
            var optimized = _queryOptimizer( expanded );
            return _query.InnerQuery.Provider.Execute( optimized );
        }

#if NET6_0_OR_GREATER
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var asyncProvider = _query.InnerQuery.Provider as IAsyncQueryProvider;
            var expanded = expression.Expand();
            var optimized = _queryOptimizer(expanded);
            if (asyncProvider != null)
            {
                return asyncProvider.ExecuteAsync<TResult>(optimized, cancellationToken);
            }

            return _query.InnerQuery.Provider.Execute<TResult>(optimized);
        }
#else
        public Task<TResult> ExecuteAsync<TResult>( Expression expression, CancellationToken cancellationToken )
        {
            var asyncProvider = _query.InnerQuery.Provider as IDbAsyncQueryProvider;

            var expanded = expression.Expand();
            var optimized = _queryOptimizer( expanded );
            if ( asyncProvider != null )
            {
                return asyncProvider.ExecuteAsync<TResult>( optimized, cancellationToken );
            }

            return Task.FromResult( _query.InnerQuery.Provider.Execute<TResult>( optimized ) );
        }

        public Task<object> ExecuteAsync( Expression expression, CancellationToken cancellationToken )
        {
            var asyncProvider = _query.InnerQuery.Provider as IDbAsyncQueryProvider;
            var expanded = expression.Expand();
            var optimized = _queryOptimizer( expanded );
            if ( asyncProvider != null )
            {
                return asyncProvider.ExecuteAsync( optimized, cancellationToken );
            }
            return Task.FromResult( _query.InnerQuery.Provider.Execute( optimized ) );
        }
#endif
    }

    /// <summary>Class for async-await style list enumeration support (e.g. .ToListAsync())</summary>
    public sealed class ExpandableDbAsyncEnumerator<T> : IDisposable,
#if NET6_0_OR_GREATER
        IAsyncEnumerator<T>
#else
        IDbAsyncEnumerator<T>
#endif
    {
        private readonly IEnumerator<T> _inner;

        /// <summary>Class for async-await style list enumeration support (e.g. .ToListAsync())</summary>
        public ExpandableDbAsyncEnumerator( IEnumerator<T> inner )
        {
            _inner = inner;
        }

        /// <summary>Dispose, .NET using-pattern</summary>
        public void Dispose()
        {
            _inner.Dispose();
        }

        /// <summary>Enumerator-pattern: MoveNextAsync</summary>
        public Task<bool> MoveNextAsync( CancellationToken cancellationToken )
        {
            return Task.FromResult( _inner.MoveNext() );
        }

#if NET6_0_OR_GREATER
        /// <summary>Enumerator-pattern: MoveNextAsync</summary>
        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        /// <summary>DisposeAsync pattern</summary>
        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }

        /// <summary>Enumerator-pattern: MoveNext</summary>
        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }
#endif
        /// <summary>Enumerator-pattern: Current item</summary>
        public T Current => _inner.Current;

#if !NET6_0_OR_GREATER
        object IDbAsyncEnumerator.Current => Current;
#endif
    }

    internal static class ExtensionsCore
    {
        public static Expression<TDelegate> Expand<TDelegate>( this Expression<TDelegate> expr )
        {
            return ( Expression<TDelegate> ) new ExpressionExpander().Visit( expr );
        }

        public static IQueryable<T> Expand<T>( this IQueryable<T> query )
        {
            return query.Provider.CreateQuery<T>( new ExpressionExpander().Visit( query.Expression ) );
        }

        public static Expression Expand( this Expression expr )
        {
            return new ExpressionExpander().Visit( expr );
        }

        public static TResult Invoke<T1, TResult>( this Expression<Func<T1, TResult>> expr, T1 arg1 )
        {
            return expr.Compile().Invoke( arg1 );
        }
    }

    /// <summary>
    /// Custom expression visitor for ExpandableQuery. This expands calls to Expression.Compile() and
    /// collapses captured lambda references in subqueries which LINQ to SQL can't otherwise handle.
    /// </summary>
    internal class ExpressionExpander : ExpressionVisitor
    {
        readonly Dictionary<MemberInfo, LambdaExpression> _expandableCache = new Dictionary<MemberInfo, LambdaExpression>();

        internal ExpressionExpander() { }

        protected LambdaExpression EvaluateTarget( Expression target )
        {
            if ( target is LambdaExpression lambdaExpression )
            {
                return lambdaExpression;
            }

            if ( target.NodeType == ExpressionType.Call )
            {
                var mc = ( MethodCallExpression ) target;
                if ( mc.Method.Name == "Compile" && mc.Method.DeclaringType?.GetGenericTypeDefinition() == typeof( Expression<> ) )
                {
                    target = mc.Object;
                }
            }

            var lambda = target.EvaluateExpression() as LambdaExpression;

            if ( lambda == null )
            {
                throw new InvalidOperationException( $"Invoke cannot evaluate LambdaExpression from '{target}'. Ensure that your function/property/member returns LambdaExpression" );
            }

            return lambda;
        }

        /// <summary>
        /// Flatten calls to Invoke so that Entity Framework can understand it. Calls to Invoke are generated
        /// by PredicateBuilder.
        /// </summary>
        protected override Expression VisitInvocation( InvocationExpression iv )
        {
            var target = iv.Expression;

            var lambda = EvaluateTarget( target );

            var body = ExpressionReplacer.GetBody( lambda, iv.Arguments );

            return Visit( body );
        }

        protected bool GetExpandLambda( MemberInfo memberInfo, out LambdaExpression expandLambda )
        {
            if ( _expandableCache.TryGetValue( memberInfo, out expandLambda ) )
            {
                return expandLambda != null;
            }

            var canExpand = memberInfo.DeclaringType != null;
            if ( canExpand )
            {
                // shortcut for standard methods
                canExpand = memberInfo.DeclaringType != typeof( Enumerable ) &&
                            memberInfo.DeclaringType != typeof( Queryable );
            }

            if ( canExpand )
            {
                var attr = memberInfo.GetCustomAttributes( typeof( ExpandableAttribute ), true ).FirstOrDefault() as ExpandableAttribute;

                if ( attr != null )
                {
                    var methodName = string.IsNullOrEmpty( attr.MethodName ) ? memberInfo.Name : attr.MethodName;

                    Expression expr;

                    if ( memberInfo is MethodInfo method && method.IsGenericMethod )
                    {
                        var args = method.GetGenericArguments();

                        expr = Expression.Call( memberInfo.DeclaringType, methodName, args );
                    }
                    else
                    {
                        expr = Expression.Call( memberInfo.DeclaringType, methodName, new Type[0] );
                    }

                    expandLambda = expr.EvaluateExpression() as LambdaExpression;
                    if ( expandLambda == null )
                    {
                        throw new InvalidOperationException(
                            $"Expandable method from '{memberInfo.DeclaringType}.{methodName}()' have returned not a LambdaExpression." );
                    }

                    _expandableCache.Add( memberInfo, expandLambda );
                    return true;
                }
            }

            _expandableCache.Add( memberInfo, null );
            return false;
        }

        protected override Expression VisitMethodCall( MethodCallExpression m )
        {
            if ( m.Method.Name == nameof( ExtensionsCore.Invoke ) && m.Method.DeclaringType == typeof( ExtensionsCore ) )
            {
                var target = m.Arguments[0];
                var lambda = EvaluateTarget( target );

                var replaceVars = new Dictionary<Expression, Expression>();
                for ( int i = 0; i < lambda.Parameters.Count; i++ )
                {
                    replaceVars.Add( lambda.Parameters[i], Visit( m.Arguments[i + 1] ) );
                }

                var body = ExpressionReplacer.Replace( lambda.Body, replaceVars );

                return Visit( body );
            }

            if ( GetExpandLambda( m.Method, out var methodLambda ) )
            {
                var replaceVars = new Dictionary<Expression, Expression>();

                if ( m.Method.IsStatic )
                {
                    for ( int i = 0; i < methodLambda.Parameters.Count; i++ )
                    {
                        replaceVars.Add( methodLambda.Parameters[i], m.Arguments[i] );
                    }
                }
                else
                {
                    replaceVars.Add( methodLambda.Parameters[0], m.Object );
                    for ( int i = 1; i < methodLambda.Parameters.Count; i++ )
                    {
                        replaceVars.Add( methodLambda.Parameters[i], m.Arguments[i - 1] );
                    }
                }

                var newExpr = ExpressionReplacer.Replace( methodLambda.Body, replaceVars );

                return Visit( newExpr );
            }

            // Expand calls to an expression's Compile() method:
            if ( m.Method.Name == nameof( LambdaExpression.Compile ) && m.Object is MemberExpression )
            {
                var me = ( MemberExpression ) m.Object;
                var newExpr = TransformExpr( me );
                if ( newExpr != me )
                {
                    return Visit( newExpr );
                }
            }

            // Strip out any nested calls to AsExpandable():
            if ( m.Method.Name == nameof( ExtensionMethods.AsExpandable ) && m.Method.DeclaringType.Name == nameof( ExtensionMethods ) && m.Method.DeclaringType.Namespace.StartsWith( "Rock" ) )
            {
                return m.Arguments[0];
            }

            return base.VisitMethodCall( m );
        }

        protected override Expression VisitMember( MemberExpression m )
        {
            if ( GetExpandLambda( m.Member, out var methodLambda ) )
            {
                var newExpr = ExpressionReplacer.GetBody( methodLambda, m.Expression );

                return Visit( newExpr );
            }

            // Strip out any references to expressions captured by outer variables - LINQ to SQL can't handle these:
            return m.Member.DeclaringType != null && m.Member.DeclaringType.Name.StartsWith( "<>" ) ?
                TransformExpr( m )
                : base.VisitMember( m );
        }

        Expression TransformExpr( MemberExpression input )
        {
            if ( input == null )
            {
                return null;
            }

            var field = input.Member as FieldInfo;

            if ( field == null )
            {
                return input;
            }

            // Collapse captured outer variables
            if ( input.Member.ReflectedType != null && ( !input.Member.ReflectedType.IsNestedPrivate
                || !input.Member.ReflectedType.Name.StartsWith( "<>" ) ) ) // captured outer variable
            {
                return TryVisitExpressionFunc( input, field );
            }

            var expression = input.Expression as ConstantExpression;
            if ( expression != null )
            {
                var obj = expression.Value;
                if ( obj == null )
                {
                    return input;
                }

                var t = obj.GetType();
                if ( !t.GetTypeInfo().IsNestedPrivate || !t.Name.StartsWith( "<>" ) )
                {
                    return input;
                }

                var fi = ( FieldInfo ) input.Member;
                var result = fi.GetValue( obj );
                var exp = result as Expression;
                if ( exp != null )
                {
                    return Visit( exp );
                }
            }

            return TryVisitExpressionFunc( input, field );
        }

        private Expression TryVisitExpressionFunc( MemberExpression input, FieldInfo field )
        {
            var propertyInfo = input.Member as PropertyInfo;
            if ( field.FieldType.GetTypeInfo().IsSubclassOf( typeof( Expression ) ) || propertyInfo != null && propertyInfo.PropertyType.GetTypeInfo().IsSubclassOf( typeof( Expression ) ) )
            {
                return Visit( Expression.Lambda<Func<Expression>>( input ).Compile()() );
            }

            return input;
        }
    }

    internal class ExpressionReplacer : ExpressionVisitor
    {
        private readonly IDictionary<Expression, Expression> _replaceMap;

        public ExpressionReplacer( IDictionary<Expression, Expression> replaceMap )
        {
            _replaceMap = replaceMap ?? throw new ArgumentNullException( nameof( replaceMap ) );
        }

        public override Expression Visit( Expression exp )
        {
            if ( exp != null && _replaceMap.TryGetValue( exp, out var replacement ) )
            {
                return replacement;
            }

            return base.Visit( exp );
        }

        public static Expression Replace( Expression expr, Expression fromExpr, Expression toExpr )
        {
            return new ExpressionReplacer( new Dictionary<Expression, Expression> { { fromExpr, toExpr } } ).Visit( expr );
        }

        public static Expression Replace( Expression expr, IDictionary<Expression, Expression> replaceMap )
        {
            return new ExpressionReplacer( replaceMap ).Visit( expr );
        }

        public static Expression GetBody( LambdaExpression lambda, params Expression[] toExpressions )
        {
            if ( lambda.Parameters.Count != toExpressions.Length )
            {
                throw new InvalidOperationException( "Wrong parameter replacement count." );
            }

            var dictionary = new Dictionary<Expression, Expression>();
            for ( int i = 0; i < lambda.Parameters.Count; i++ )
            {
                dictionary.Add( lambda.Parameters[i], toExpressions[i] );
            }

            return Replace( lambda.Body, dictionary );
        }

        public static Expression GetBody( LambdaExpression lambda, ReadOnlyCollection<Expression> toExpressions )
        {
            if ( lambda.Parameters.Count != toExpressions.Count )
            {
                throw new InvalidOperationException( "Wrong parameter replacement count." );
            }

            var dictionary = new Dictionary<Expression, Expression>();
            for ( int i = 0; i < lambda.Parameters.Count; i++ )
            {
                dictionary.Add( lambda.Parameters[i], toExpressions[i] );
            }

            return Replace( lambda.Body, dictionary );
        }
    }

    internal static class ExpressionHelpers
    {
        public static object EvaluateExpression( this Expression expr )
        {
            if ( expr == null )
            {
                return null;
            }

            switch ( expr.NodeType )
            {
                case ExpressionType.Constant:
                    return ( ( ConstantExpression ) expr ).Value;

                case ExpressionType.MemberAccess:
                    {
                        var member = ( MemberExpression ) expr;

                        if ( member.Member is FieldInfo field )
                        {
                            return field.GetValue( member.Expression.EvaluateExpression() );
                        }

                        if ( member.Member is PropertyInfo property )
                        {
                            return property.GetValue( member.Expression.EvaluateExpression(), null );
                        }

                        break;
                    }

                case ExpressionType.Call:
                    {
                        var mc = ( MethodCallExpression ) expr;
                        var arguments = mc.Arguments.Select( EvaluateExpression ).ToArray();
                        var instance = mc.Object.EvaluateExpression();
                        return mc.Method.Invoke( instance, arguments );
                    }
            }

            return Expression.Lambda( expr ).Compile().DynamicInvoke();
        }

        public static ParameterExpression CreateParameterExpression( Type type )
        {
            return Expression.Parameter( type );
        }
    }
}
