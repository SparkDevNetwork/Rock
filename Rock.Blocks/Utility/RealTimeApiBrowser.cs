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
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using Rock.Attribute;
using Rock.Model;

namespace Rock.Blocks.Utility
{
    /// <summary>
    /// Allows browsing of the RealTime topics and events.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "RealTime API Browser" )]
    [Category( "Obsidian > Utility" )]
    [Description( "Allows browsing of the RealTime topics and events." )]
    [IconCssClass( "fa fa-object-group" )]

    [Rock.SystemGuid.EntityTypeGuid( "FAA4B123-975F-450E-8440-3D985425F6AF" )]
    [Rock.SystemGuid.BlockTypeGuid( "B6F88763-1C9D-4F68-B201-3BB6EDAE3071" )]
    public class RealTimeApiBrowser: RockObsidianBlockType
    {
        private static ConcurrentDictionary<string, Dictionary<string, string>> _assemblyDocumentationCache = new ConcurrentDictionary<string, Dictionary<string, string>>();

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        public override object GetObsidianBlockInitialization()
        {
            GetData();

            return base.GetObsidianBlockInitialization();
        }

        private static List<TypeDeclaration> GetData()
        {
            var configurations = Rock.RealTime.RealTimeHelper.Engine.GetTopicConfigurations();

            foreach ( var configuration in configurations )
            {
                var members = GetOrLoadAssemblyDocs( configuration.ClientInterfaceType.Assembly );

                var typeId = new IdMap().Get( configuration.ClientInterfaceType );

                if ( members.TryGetValue( typeId, out var member ) )
                {
                }
            }

            return null;
        }

        private static Dictionary<string, string> GetOrLoadAssemblyDocs( Assembly assembly )
        {
            var filename = Path.ChangeExtension( assembly.CodeBase, "xml" );

            return _assemblyDocumentationCache.GetOrAdd( filename, _ =>
            {
                try
                {
                    var doc = XDocument.Load( filename );

                    return doc.Root.Element( "members" )
                        .Elements( "member" )
                        .Where( element => element.Attribute( "name" ) != null )
                        .ToDictionary( element => element.Attribute( "name" ).Value, element => element.ToString() );
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            } );
        }

        public class TypeDeclaration
        {
            public string Name { get; set; }

            public string Namespace { get; set; }

            public string Identifier { get; set; }

            public string Documentation { get; set; }

            public List<MethodDeclaration> Methods { get; set; }
        }

        public class MethodDeclaration
        {
            public string Name { get; set; }

            public List<string> Parameters { get; set; }

            public string Documentation { get; set; }
        }

        public class IdMap
        {
            private StringBuilder _builder = new StringBuilder();

            public string Get( Type type )
            {
                _builder.Length = 0;
                _builder.Append( "T:" );
                AppendType( _builder, type );

                return _builder.Length > 0 ? _builder.ToString() : null;
            }

            public string Get( MemberInfo member )
            {
                _builder.Length = 0;

                switch ( member.MemberType )
                {
                    case MemberTypes.Constructor:
                        _builder.Append( "M:" );
                        Append( ( ConstructorInfo ) member );
                        break;
                    case MemberTypes.Event:
                        _builder.Append( "E:" );
                        Append( ( EventInfo ) member );
                        break;
                    case MemberTypes.Field:
                        _builder.Append( "F:" );
                        Append( ( FieldInfo ) member );
                        break;
                    case MemberTypes.Method:
                        _builder.Append( "M:" );
                        Append( ( MethodInfo ) member );
                        break;
                    case MemberTypes.NestedType:
                        AppendType( _builder, ( Type ) member );
                        break;
                    case MemberTypes.Property:
                        _builder.Append( "P:" );
                        Append( ( PropertyInfo ) member );
                        break;
                }

                return _builder.Length > 0 ? _builder.ToString() : null;
            }

            private void Append( PropertyInfo property )
            {
                AppendType( _builder, property.DeclaringType );
                _builder.Append( '.' ).Append( property.Name );
                Append( property.GetIndexParameters() );
            }

            private void Append( MethodInfo method )
            {
                AppendType( _builder, method.DeclaringType );
                _builder.Append( '.' ).Append( method.Name );

                if ( method.IsGenericMethodDefinition )
                {
                    // Append arity
                    _builder.Append( "``" ).Append( method.GetGenericArguments().Length );
                }

                Append( method.GetParameters() );
            }

            private void Append( ParameterInfo[] parameters )
            {
                if ( parameters.Length == 0 )
                {
                    return;
                }
                _builder.Append( '(' );
                for ( var i = 0; i < parameters.Length; i++ )
                {
                    if ( i > 0 )
                    {
                        _builder.Append( ',' );
                    }
                    var p = parameters[i];
                    if ( p.ParameterType.IsByRef )
                    {
                        AppendType( _builder, p.ParameterType.GetElementType() );
                        _builder.Append( "@" );
                    }
                    else
                    {
                        AppendType( _builder, p.ParameterType );
                    }
                }
                _builder.Append( ')' );
            }

            private void Append( FieldInfo field )
            {
                AppendType( _builder, field.DeclaringType );
                _builder.Append( '.' ).Append( field.Name );
            }

            private void Append( EventInfo @event )
            {
                AppendType( _builder, @event.DeclaringType );
                _builder.Append( '.' ).Append( @event.Name );
            }

            private void Append( ConstructorInfo constructor )
            {
                AppendType( _builder, constructor.DeclaringType );
                _builder.Append( '.' ).Append( "#ctor" );
                Append( constructor.GetParameters() );
            }

            private void AppendType( StringBuilder sb, Type type, bool addTypeToMap = true )
            {
                // Generic parameters will only have the parameter name, i.e. "T".
                if ( !type.IsGenericParameter )
                {
                    if ( type.DeclaringType != null )
                    {
                        AppendType( sb, type.DeclaringType );
                        sb.Append( '.' );
                    }
                    else if ( !string.IsNullOrEmpty( type.Namespace ) )
                    {
                        sb.Append( type.Namespace );
                        sb.Append( '.' );
                    }

                    // We only append the name if it's a non-generic parameter.
                    sb.Append( type.Name );
                }
                else
                {
                    // Never add to map the generic parameters, they 
                    // can never be referenced in reflection.
                    addTypeToMap = false;

                    // If the generic parameter was declared in a method, 
                    // the arity is a double ``.
                    if ( type.DeclaringMethod != null )
                    {
                        sb.Append( "``" );
                        // Get the generic method parameter index.
                        sb.Append( type.DeclaringMethod
                            .GetGenericArguments()
                            .ToList()
                            .IndexOf( type ) );
                    }
                    else if ( type.DeclaringType != null )
                    {
                        sb.Append( "`" );
                        // Get the generic type parameter index.
                        sb.Append( type.DeclaringType
                            .GetGenericArguments()
                            .ToList()
                            .IndexOf( type ) );
                    }
                }

                if ( type.IsGenericType && !type.IsGenericTypeDefinition )
                {
                    // Remove "`1" suffix from type name
                    while ( char.IsDigit( sb[sb.Length - 1] ) )
                        sb.Length--;
                    sb.Length--;
                    {
                        var args = type.GetGenericArguments();
                        sb.Append( '{' );
                        for ( var i = 0; i < args.Length; i++ )
                        {
                            if ( i > 0 )
                            {
                                sb.Append( ',' );
                            }
                            AppendType( sb, args[i] );
                        }
                        sb.Append( '}' );
                    }
                }
            }
        }
    }
}
