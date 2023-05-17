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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

/*
 * 9/21/2022 - DSH
 * 
 * This logic was mostly taken from the MIT licensed SignalR project.
 * Adjustments were made to add support for topic identifiers.
 */
namespace Rock.RealTime
{
    /// <summary>
    /// Builds dynamic proxies for the interface defined by <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The interface that defines the methods that are implemented by clients.</typeparam>
    /// <typeparam name="TProxy">The native client proxy type for the custom proxy.</typeparam>
    internal static class TopicProxyBuilder<T, TProxy>
        where T : class
        where TProxy : class
    {
        /// <summary>
        /// The module (assembly) name to use when building dynamic classes.
        /// </summary>
        private const string DynamicModuleName = "Rock.RealTime.Dynamic";

        /// <summary>
        /// Builds a factory function that will create proxy instances for the
        /// interface specified by <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A function that takes a client proxy and topic identifier as its parameters.</returns>
        public static Func<TProxy, string, T> BuildFactory()
        {
            var assemblyName = new AssemblyName( DynamicModuleName );
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
            var moduleBuilder = assemblyBuilder.DefineDynamicModule( DynamicModuleName );
            var clientType = GenerateInterfaceImplementation( moduleBuilder );

            var factoryMethod = clientType.GetMethod( "Build", BindingFlags.Public | BindingFlags.Static );

            return ( TProxy client, string topicIdentifier ) => ( T ) factoryMethod.Invoke( null, new object[] { client, topicIdentifier } );
        }

        /// <summary>
        /// Create a dynamically constructed Type proxy for the interface
        /// specified by <typeparamref name="T"/>.
        /// </summary>
        /// <param name="moduleBuilder">The assembly module builder that will contain this type.</param>
        /// <returns>The <see cref="Type"/> that represents the newly created proxy class.</returns>
        private static Type GenerateInterfaceImplementation( ModuleBuilder moduleBuilder )
        {
            var name = DynamicModuleName + "." + typeof( T ).Name + "Impl";

            var type = moduleBuilder.DefineType( name, TypeAttributes.Public, typeof( object ), new[] { typeof( T ) } );

            var clientField = type.DefineField( "_client", typeof( TProxy ), FieldAttributes.Private | FieldAttributes.InitOnly );
            var topicIdentifierField = type.DefineField( "_topicIdentifier", typeof( string ), FieldAttributes.Private | FieldAttributes.InitOnly );

            var ctor = BuildConstructor( type, clientField, topicIdentifierField );

            BuildFactoryMethod( type, ctor );

            foreach ( var method in GetAllInterfaceMethods( typeof( T ) ) )
            {
                BuildMethod( type, method, clientField, topicIdentifierField );
            }

            return type.CreateTypeInfo();
        }

        /// <summary>
        /// Creates a constructor that will be used to initialize the dynamic
        /// proxy object.
        /// </summary>
        /// <remarks>
        /// Emitted code is equivalent to:
        /// <code>
        /// public ctor( IClientProxy client, string topicIdentifier )
        ///     : base()
        /// {
        ///     this._client = client;
        ///     this._topicIdentifier = topicIdentifier;
        /// }
        /// </code>
        /// </remarks>
        /// <param name="type">The builder that will be used to generate the type at runtime.</param>
        /// <param name="clientField">The field definition that represents the <c>_client</c> private field.</param>
        /// <param name="topicIdentifierField">The field definition that represents the <c>_topicIdentifier</c> private field.</param>
        /// <returns>The definition to that will emit the constructor at runtime.</returns>
        private static ConstructorInfo BuildConstructor( TypeBuilder type, FieldInfo clientField, FieldInfo topicIdentifierField )
        {
            var ctor = type.DefineConstructor( MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof( TProxy ), typeof( string ) } );

            var generator = ctor.GetILGenerator();

            // base()
            generator.Emit( OpCodes.Ldarg_0 );
            generator.Emit( OpCodes.Call, typeof( object ).GetConstructors().Single() );

            // this._client = client
            generator.Emit( OpCodes.Ldarg_0 ); // "this"
            generator.Emit( OpCodes.Ldarg_1 ); // First constructor parameter.
            generator.Emit( OpCodes.Stfld, clientField ); // this._client = client

            // this._topicIdentifier = topicIdentifier
            generator.Emit( OpCodes.Ldarg_0 ); // "this"
            generator.Emit( OpCodes.Ldarg_2 ); // Second constructor parameter.
            generator.Emit( OpCodes.Stfld, topicIdentifierField ); // this._topicdentifier = topicIdentifier

            generator.Emit( OpCodes.Ret );

            return ctor;
        }

        /// <summary>
        /// Builds a static factory method that will construct the type for us.
        /// This is needed to simplify the creation of new instances.
        /// </summary>
        /// <remarks>
        /// Emitted code is equivalent to:
        /// <code>
        /// public static DynamicType Build( IClientProxy client, string topicIdentifier )
        /// {
        ///     return new DynamicType( client, topicIdentifier );
        /// }
        /// </code>
        /// </remarks>
        /// <param name="type">The builder that will be used to generate the type at runtime.</param>
        /// <param name="ctor">The constructor to call to create a new instance of the type.</param>
        private static void BuildFactoryMethod( TypeBuilder type, ConstructorInfo ctor )
        {
            var method = type.DefineMethod( "Build", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof( T ), new Type[] { typeof( TProxy ), typeof( string ) } );

            var generator = method.GetILGenerator();

            generator.Emit( OpCodes.Ldarg_0 ); // Load the client argument onto the stack
            generator.Emit( OpCodes.Ldarg_1 ); // Load the topicIdentifier argument onto the stack
            generator.Emit( OpCodes.Newobj, ctor ); // Call the generated constructor with the proxy
            generator.Emit( OpCodes.Ret ); // Return the typed client
        }

        /// <summary>
        /// Creates a dynamic method on the dynamic type that will proxy
        /// the call to the client proxy using the proper topic identifier.
        /// </summary>
        /// <remarks>
        /// Emitted code is roughly equivalent to:
        /// <code>
        /// Task [methodName]( object argument1, object argument2, ...object argumentN, Task optionalCancellationToken )
        /// {
        ///     object[] messageArguments;
        ///
        ///     messageArguments = new object[N];
        ///     messageArguments[0] = argument1;
        ///     messageArguments[1] = argument2;
        ///     messageArguments[N-1] = argumentN;
        ///
        ///     return RealTimeHelper.SendMessageAsync( this._client, this._topicIdentifier, [methodName], messageArguments, optionalCancellationToken ?? CancellationToken.None );
        /// }
        /// </code>
        /// </remarks>
        /// <param name="type">The builder that will be used to generate the method at runtime.</param>
        /// <param name="interfaceMethodInfo">The definition of the method to be implemented.</param>
        /// <param name="clientField">The field definition that represents the <c>_client</c> private field.</param>
        /// <param name="topicIdentifierField">The field definition that represents the <c>_topicIdentifier</c> private field.</param>
        private static void BuildMethod( TypeBuilder type, MethodInfo interfaceMethodInfo, FieldInfo clientField, FieldInfo topicIdentifierField )
        {
            // Find the RealTimeHelper.SendMessageAsync method that will be
            // invoked by the proxy method.
            var invokeMethod = typeof( RealTimeHelper ).GetMethod(
                nameof( RealTimeHelper.SendMessageAsync ), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                new[] { typeof( object ), typeof( string ), typeof( string ), typeof( object[] ), typeof( CancellationToken ) }, null );

            // The name of the method to be invoked, used when calling
            // the SendMessageAsync() method later.
            var methodName = interfaceMethodInfo.Name;

            // Get the parameter types of the method and the return type.
            var parameters = interfaceMethodInfo.GetParameters();
            var paramTypes = parameters.Select( param => param.ParameterType ).ToArray();
            var returnType = interfaceMethodInfo.ReturnType;

            // Define a public method to implement the interface method.
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual
                | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            var methodBuilder = type.DefineMethod( interfaceMethodInfo.Name, methodAttributes );

            // Set the return type and parameter types that the proxy method
            // will implement.
            methodBuilder.SetReturnType( interfaceMethodInfo.ReturnType );
            methodBuilder.SetParameters( paramTypes );

            // Configures the proxy method to accept the same generic parameters
            // as defined on the interface.
            var genericTypeNames = paramTypes.Where( p => p.IsGenericParameter ).Select( p => p.Name ).Distinct().ToArray();

            if ( genericTypeNames.Length > 0 )
            {
                methodBuilder.DefineGenericParameters( genericTypeNames );
            }

            // Check to see if the last parameter of the method is a
            // CancellationToken. If so it will be removed from the list of
            // parameters that are automatically copied over.
            bool hasCancellationToken = paramTypes.LastOrDefault() == typeof( CancellationToken );
            if ( hasCancellationToken )
            {
                // Remove CancellationToken parameter. It will be handled later.
                paramTypes = paramTypes.Take( paramTypes.Length - 1 ).ToArray();
            }

            // Get a generator so we can start emiting IL code to the method body.
            var generator = methodBuilder.GetILGenerator();

            // object[] messageArguments;
            generator.DeclareLocal( typeof( object[] ) );

            // The first argument to RealTimeHelper.SendMessageAsync() is the proxy (this._client).
            generator.Emit( OpCodes.Ldarg_0 );
            generator.Emit( OpCodes.Ldfld, clientField );

            var isTypeLabel = generator.DefineLabel();

            // The second argument to RealTimeHelper.SendMessageAsync() is the topic identifier.
            generator.Emit( OpCodes.Ldarg_0 );
            generator.Emit( OpCodes.Ldfld, topicIdentifierField );

            // The third argument to RealTimeHelper.SendMessageAsync() is the method name.
            generator.Emit( OpCodes.Ldstr, methodName );

            // messageArguments = new object[paramTypes.Length];
            generator.Emit( OpCodes.Ldc_I4, paramTypes.Length );
            generator.Emit( OpCodes.Newarr, typeof( object ) );
            generator.Emit( OpCodes.Stloc_0 );

            // Store each parameter in the object array
            for ( var i = 0; i < paramTypes.Length; i++ )
            {
                // messageArguments[i] = arguments[i+1]
                generator.Emit( OpCodes.Ldloc_0 );
                generator.Emit( OpCodes.Ldc_I4, i );
                generator.Emit( OpCodes.Ldarg, i + 1 );
                generator.Emit( OpCodes.Box, paramTypes[i] );
                generator.Emit( OpCodes.Stelem_Ref );
            }

            // The fourth argument to RealTimeHelper.SendMessageAsync() is messageArguments.
            generator.Emit( OpCodes.Ldloc_0 );

            // If the method takes a cancellation token then pass it along to
            // the SendMessageAsync() method as the last parameter. Otherwise
            // pass in CancellationToken.None.
            if ( hasCancellationToken )
            {
                // Load the CancellationToken from the method parameters
                // and put it on the stack.
                generator.Emit( OpCodes.Ldarg, paramTypes.Length + 1 );
            }
            else
            {
                // Get a new CancellationToken.None and put it on the stack.
                generator.Emit( OpCodes.Call, typeof( CancellationToken ).GetProperty( "None", BindingFlags.Public | BindingFlags.Static ).GetMethod );
            }

            // RealTimeHelper.SendMessageAsync( this._client, this._topicIdentifier, methodName, methodArguments, optionalCancellationToken ?? CancellationToken.None );
            generator.Emit( OpCodes.Call, invokeMethod );

            // Return the Task returned by the call to SendMessageAsync().
            generator.Emit( OpCodes.Ret );
        }

        /// <summary>
        /// Get all interface methods, including inherited interfaces, and
        /// return them to the caller.
        /// </summary>
        /// <param name="interfaceType">The type that represents the interface to be implemented.</param>
        /// <returns>An enumeration of all methods on the interface or any inherited interfaces.</returns>
        private static IEnumerable<MethodInfo> GetAllInterfaceMethods( Type interfaceType )
        {
            foreach ( var parent in interfaceType.GetInterfaces() )
            {
                foreach ( var parentMethod in GetAllInterfaceMethods( parent ) )
                {
                    yield return parentMethod;
                }
            }

            foreach ( var method in interfaceType.GetMethods() )
            {
                yield return method;
            }
        }
    }
}
