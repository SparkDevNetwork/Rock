using System;
using System.Collections.Generic;
using System.Reflection;

using Castle.DynamicProxy;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Proxies.Internal;

using IEntity = Rock.Data.IEntity;

namespace Rock.CoreShims
{
    /// <summary>
    /// This class provides a custom implementation of the default proxy factory
    /// in Entity Framework. This allows us to provide a custom <see cref="IProxyGenerationHook"/>
    /// to Castle Core to decide which properties/methods are proxied.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Performance Benchmarks, time to initialize RockContext inside debugger / outside debugger:
    ///     </para>
    ///     <para>
    ///         Without Lazy Loading: 9,031ms / 3,247ms
    ///     </para>
    ///     <para>
    ///         With Lazy Loading and default proxy factory: 137,960ms / 66,058ms
    ///     </para>
    ///     <para>
    ///         With Lazy Loading and RockProxyFactory: 17,972ms / 5,674ms
    ///     </para>
    /// </remarks>
    /// <seealso cref="Microsoft.EntityFrameworkCore.Proxies.Internal.IProxyFactory" />
#pragma warning disable EF1001 // Internal EF Core API usage.
    internal class RockProxyFactory : IProxyFactory
    {
        private readonly ProxyGenerationOptions _proxyGenerationOptions = new ProxyGenerationOptions( new RockProxyGenerationHook() );

        private readonly ProxyGenerator _generator = new ProxyGenerator();

        public virtual object Create( DbContext context, Type type, params object[] constructorArguments )
        {
            var entityType = context.Model.FindRuntimeEntityType( type );

            if ( entityType == null )
            {
                throw new InvalidOperationException( $"Entity Type {type.FullName} not found." );
            }

            return CreateProxy( context, entityType, constructorArguments );
        }

        public virtual Type CreateProxyType( ProxiesOptionsExtension options, IEntityType entityType )
        {
            return _generator.ProxyBuilder.CreateClassProxyType( entityType.ClrType, GetInterfacesToProxy(), _proxyGenerationOptions );
        }

        public virtual object CreateLazyLoadingProxy( DbContext context, IEntityType entityType, ILazyLoader loader, object[] constructorArguments )
        {
            return CreateLazyLoadingProxy(
                entityType,
                context.GetService<ILazyLoader>(),
                constructorArguments );
        }

        private object CreateLazyLoadingProxy( IEntityType entityType, ILazyLoader loader, object[] constructorArguments )
        {
            return _generator.CreateClassProxy(
                entityType.ClrType,
                GetInterfacesToProxy(),
                _proxyGenerationOptions,
                constructorArguments,
                GetNotifyChangeInterceptors( new LazyLoadingInterceptor( entityType, loader ) ) );
        }

        public virtual object CreateProxy( DbContext context, IEntityType entityType, object[] constructorArguments )
        {
            return CreateLazyLoadingProxy(
                entityType,
                context.GetService<ILazyLoader>(),
                constructorArguments );
        }

        private Type[] GetInterfacesToProxy()
        {
            return new Type[] { typeof( IProxyLazyLoader ) };
        }

        private Castle.DynamicProxy.IInterceptor[] GetNotifyChangeInterceptors( LazyLoadingInterceptor lazyLoadingInterceptor )
        {
            return new Castle.DynamicProxy.IInterceptor[] { lazyLoadingInterceptor };
        }

        class RockProxyGenerationHook : IProxyGenerationHook
        {
            protected static readonly ICollection<Type> SkippedTypes = new[]
            {
                typeof(object),
                typeof(MarshalByRefObject),
                typeof(ContextBoundObject)
            };

            public virtual bool ShouldInterceptMethod( Type type, MethodInfo methodInfo )
            {
                var status = SkippedTypes.Contains( methodInfo.DeclaringType ) == false;

                if ( status && typeof( IEntity ).IsAssignableFrom( type ) )
                {
                    var methodParameters = methodInfo.GetParameters();

                    if ( methodInfo.Name.StartsWith( "get_" ) && methodParameters.Length == 0 )
                    {
                        status = IsNavigationType( methodInfo.ReturnType );
                    }
                    else if ( methodInfo.Name.StartsWith( "set_" ) && methodParameters.Length == 1 )
                    {
                        status = IsNavigationType( methodParameters[0].ParameterType );
                    }
                    else
                    {
                        status = false;
                    }
                }

                //if ( !status )
                //{
                //    System.Diagnostics.Debug.WriteLine( $"Will proxy {type.Name}.{methodInfo.Name}" );
                //}

                return status;
            }

            public virtual void NonProxyableMemberNotification( Type type, MemberInfo memberInfo )
            {
            }

            public virtual void MethodsInspected()
            {
            }

            public override bool Equals( object obj )
            {
                return obj != null && obj.GetType() == GetType();
            }

            public override int GetHashCode()
            {
                return GetType().GetHashCode();
            }

            private bool IsNavigationType( Type type )
            {
                var nullableType = Nullable.GetUnderlyingType( type );

                if ( nullableType != null )
                {
                    type = nullableType;
                }

                if ( type.IsGenericType )
                {
                    var genericType = type.GetGenericTypeDefinition();
                    var genericArgs = type.GetGenericArguments();

                    if ( genericArgs.Length == 1 && genericType == typeof( ICollection<> ) )
                    {
                        var collectionType = typeof( ICollection<> ).MakeGenericType( genericArgs[0] );

                        if ( collectionType.IsAssignableFrom( type ) )
                        {
                            return IsNavigationType( genericArgs[0] );
                        }
                    }
                }

                return typeof( IEntity ).IsAssignableFrom( type );
            }

        }
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}
