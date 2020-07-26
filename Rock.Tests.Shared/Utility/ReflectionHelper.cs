﻿// <copyright>
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

using System;
using System.Reflection;


// From https://github.com/haacked/Subtext/tree/master/src/UnitTests.Subtext
namespace Rock.Tests.Shared
{
    /// <summary>
    /// Helper class to simplify common reflection tasks.
    /// </summary>
    public sealed class ReflectionHelper
    {
        private ReflectionHelper() { }

        /// <summary>
        /// Returns the value of the private member specified.
        /// </summary>
        /// <param name="fieldName">Name of the member.</param>
        /// /// <param name="type">Type of the member.</param>
        public static T GetStaticFieldValue<T>( string fieldName, Type type )
        {
            FieldInfo field = type.GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Static );
            if ( field != null )
            {
                return ( T ) field.GetValue( type );
            }
            return default( T );
        }

        /// <summary>
        /// Returns the value of the private member specified.
        /// </summary>
        /// <param name="fieldName">Name of the member.</param>
        /// <param name="typeName"></param>
        public static T GetStaticFieldValue<T>( string fieldName, string typeName )
        {
            Type type = Type.GetType( typeName, true );
            FieldInfo field = type.GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Static );
            if ( field != null )
            {
                return ( T ) field.GetValue( type );
            }
            return default( T );
        }

        /// <summary>
        /// Sets the value of the private static member.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public static void SetStaticFieldValue<T>( string fieldName, Type type, T value )
        {
            FieldInfo field = type.GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Static );
            if ( field == null )
                throw new ArgumentException( string.Format( "Could not find the private instance field '{0}'", fieldName ) );

            field.SetValue( null, value );
        }

        /// <summary>
        /// Sets the value of the private static member.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <param name="value"></param>
        public static void SetStaticFieldValue<T>( string fieldName, string typeName, T value )
        {
            Type type = Type.GetType( typeName, true );
            FieldInfo field = type.GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Static );
            if ( field == null )
                throw new ArgumentException( string.Format( "Could not find the private instance field '{0}'", fieldName ) );

            field.SetValue( null, value );
        }

        /// <summary>
        /// Returns the value of the private member specified.
        /// </summary>
        /// <param name="fieldName">Name of the member.</param>
        /// <param name="source">The object that contains the member.</param>
        public static T GetPrivateInstanceFieldValue<T>( string fieldName, object source )
        {
            FieldInfo field = source.GetType().GetField( fieldName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance );
            if ( field != null )
            {
                return ( T ) field.GetValue( source );
            }
            return default( T );
        }

        /// <summary>
        /// Returns the value of the private member specified.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="source">The object that contains the member.</param>
        /// <param name="value">The value to set the member to.</param>
        public static void SetPrivateInstanceFieldValue( string memberName, object source, object value )
        {
            FieldInfo field = source.GetType().GetField( memberName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance );
            if ( field == null )
                throw new ArgumentException( string.Format( "Could not find the private instance field '{0}'", memberName ) );

            field.SetValue( source, value );
        }

        public static object Instantiate( string typeName )
        {
            return Instantiate( typeName, null, null );
        }

        public static object Instantiate( string typeName, Type[] constructorArgumentTypes, params object[] constructorParameterValues )
        {
            return Instantiate( Type.GetType( typeName, true ), constructorArgumentTypes, constructorParameterValues );
        }

        public static object Instantiate( Type type, Type[] constructorArgumentTypes, params object[] constructorParameterValues )
        {
            ConstructorInfo constructor = type.GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, null, constructorArgumentTypes, null );
            return constructor.Invoke( constructorParameterValues );
        }

        public static T InstantiateInternalObject<T>( string typeName, params object[] constructorParameterValues ) where T : class
        {
            var assembly = typeof( T ).Assembly;
            var constructorParameterTypes = new Type[constructorParameterValues.Length];
            for ( var i = 0; i < constructorParameterValues.Length; i++ )
            {
                constructorParameterTypes[i] = constructorParameterValues[i].GetType();
            }

            var assemblyType = assembly.GetType( typeName );
            if ( assembly == null )
            {
                return default( T );
            }

            var typeConstructor = assemblyType.GetConstructor( constructorParameterTypes );
            if ( typeConstructor == null )
            {
                return default( T );
            }

            return typeConstructor.Invoke( constructorParameterValues ) as T;
        }

        /// <summary>
        /// Invokes a non-public static method.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static TReturn InvokeNonPublicMethod<TReturn>( Type type, string methodName, params object[] parameters )
        {
            Type[] paramTypes = Array.ConvertAll( parameters, new Converter<object, Type>( delegate ( object o )
            { return o.GetType(); } ) );

            MethodInfo method = type.GetMethod( methodName, BindingFlags.NonPublic | BindingFlags.Static, null, paramTypes, null );
            if ( method == null )
                throw new ArgumentException( string.Format( "Could not find a method with the name '{0}'", methodName ), "method" );

            return ( TReturn ) method.Invoke( null, parameters );
        }

        public static TReturn InvokeNonPublicMethod<TReturn>( object source, string methodName, params object[] parameters )
        {
            Type[] paramTypes = Array.ConvertAll( parameters, new Converter<object, Type>( delegate ( object o )
            { return o.GetType(); } ) );

            MethodInfo method = source.GetType().GetMethod( methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null );
            if ( method == null )
                throw new ArgumentException( string.Format( "Could not find a method with the name '{0}'", methodName ), "method" );

            return ( TReturn ) method.Invoke( source, parameters );
        }

        public static TReturn InvokeProperty<TReturn>( object source, string propertyName )
        {
            PropertyInfo propertyInfo = source.GetType().GetProperty( propertyName );
            if ( propertyInfo == null )
                throw new ArgumentException( string.Format( "Could not find a propertyName with the name '{0}'", propertyName ), "propertyName" );

            return ( TReturn ) propertyInfo.GetValue( source, null );
        }

        public static TReturn InvokeNonPublicProperty<TReturn>( object source, string propertyName )
        {
            PropertyInfo propertyInfo = source.GetType().GetProperty( propertyName, BindingFlags.NonPublic | BindingFlags.Instance, null, typeof( TReturn ), new Type[0], null );
            if ( propertyInfo == null )
                throw new ArgumentException( string.Format( "Could not find a propertyName with the name '{0}'", propertyName ), "propertyName" );

            return ( TReturn ) propertyInfo.GetValue( source, null );
        }

        public static object InvokeNonPublicProperty( object source, string propertyName )
        {
            PropertyInfo propertyInfo = source.GetType().GetProperty( propertyName, BindingFlags.NonPublic | BindingFlags.Instance );
            if ( propertyInfo == null )
                throw new ArgumentException( string.Format( "Could not find a propertyName with the name '{0}'", propertyName ), "propertyName" );

            return propertyInfo.GetValue( source, null );
        }

        public static T CreateInstance<T>( params object[] args )
        {
            var type = typeof( T );
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null );
            return ( T ) instance;
        }

        public static TReturn GetFieldValue<TReturn>( object source, string fieldName )
        {
            FieldInfo fieldInfo = source.GetType().GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance );
            if ( fieldInfo == null )
                throw new ArgumentException( string.Format( "Could not find a field with the name '{0}'", fieldName ), "propertyName" );

            return ( TReturn ) fieldInfo.GetValue( source );
        }
    }
}
