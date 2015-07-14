// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml.Linq;


using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    [DisplayName( "Model Map" )]
    [Category( "Examples" )]
    [Description( "Displays details about each model classes in Rock.Model." )]
    [IntegerField("Minutes To Cache", "Numer of whole minutes to cache the class data (since reflecting on the assembly can be time consuming).", false, 60 )]
    public partial class ModelMap : RockBlock
    {
        //Cache _classes = new Cache();
        XDocument _docuDoc = new XDocument();
        ObjectCache cache = MemoryCache.Default;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var sb = new StringBuilder();

            List<MClass> allClasses = GetModelClasses() as List<MClass>;
            if ( allClasses != null )
            { 
                foreach ( var aClass in allClasses.OrderBy( a => a.Name ).ToList() )
                {
                    sb.Append( ClassNode( aClass ) );
                }
            }
            else
            {
                sb.AppendLine( "Error reading classes from assembly." );
            }
            lClasses.Text = sb.ToString();
        }

        /// <summary>
        /// Build a "node" of the class/model with its properties and methods inside.
        /// </summary>
        /// <param name="aClass"></param>
        /// <param name="expandedClassNameGuidList"></param>
        /// <returns></returns>
        private string ClassNode( MClass aClass )
        {
            var sb = new StringBuilder();

            string classGuid = this.PageParameter( "classGuid" );
            bool isSelected = false;
            if ( !string.IsNullOrWhiteSpace( classGuid ) )
            {
                isSelected = aClass.Guid == classGuid;
            }

            var name = HttpUtility.HtmlEncode( aClass.Name );
            sb.AppendFormat(
                "<div class='panel panel-default' data-id='{0}'><div class='panel-heading js-example-toggle'><h1 class='panel-title rollover-container'><a name='{1}' href='?classGuid={0}#{1}' class='link-address rollover-item' style='margin-left: -20px;'><i class='fa fa-link'></i></a> <strong>{1}</strong> <small class='pull-right'><i class='fa js-toggle {3}'></i> Show Details</small></h1><p class='description'>{2}</p></div>",
                aClass.Guid,
                name,
                aClass.Comment.Summary,
                isSelected ? "fa-circle" : "fa-circle-o"
                );

            if ( aClass.Properties.Any() || aClass.Methods.Any() )
            {
                sb.AppendFormat( "<div class='panel-body' {0}>", !isSelected ? "style='display: none;'" : string.Empty );

                if ( aClass.Properties.Any() )
                {
                    sb.AppendLine( "<h2>Properties</h2><ul>" );
                    foreach ( var property in aClass.Properties.OrderBy( p => p.Name ) )
                    {
                        //  data-expanded='false' data-model='Block' data-id='b{0}'
                        sb.AppendFormat( "<li data-id='p{0}'><strong><tt>{1}</tt></strong>{3}{4}{2}</li>{5}",
                            property.Id,
                            HttpUtility.HtmlEncode( property.Name ),
                            ( property.Comment != null && !string.IsNullOrWhiteSpace( property.Comment.Summary ) ) ? " - " + HttpUtility.HtmlEncode( property.Comment.Summary ) : "",
                            property.IsLavaInclude ? " <small><span class='tip tip-lava'></span></small> " : string.Empty,
                            property.NotMapped ? " <span class='fa-stack small'><i class='fa fa-database fa-stack-1x'></i><i class='fa fa-ban fa-stack-2x text-danger'></i></span> " : string.Empty,
                            Environment.NewLine );
                    }
                    sb.AppendLine( "</ul>" );
                }

                if ( aClass.Methods.Any() )
                {
                    sb.AppendLine( "<h2>Methods</h2><ul>" );
                    foreach ( var method in aClass.Methods.OrderBy( m => m.Name ) )
                    {
                        //<li data-expanded='false' data-model='Block' data-id='b{0}'><span>{1}{2}:{3}</span></li>{4}
                        sb.AppendFormat( "<li data-id='m{0}'><strong><tt>{1}</tt></strong> {2}</li>{3}",
                            method.Id,
                            HttpUtility.HtmlEncode( method.Signature ),
                            ( method.Comment != null && !string.IsNullOrWhiteSpace( method.Comment.Summary ) ) ? " - " + HttpUtility.HtmlEncode( method.Comment.Summary ) : "",
                            Environment.NewLine );
                    }
                    sb.AppendLine( "</ul>" );
                }

                sb.AppendLine( "</div>" );
            }

            sb.AppendLine( "</div>" );

            return sb.ToString();
        }

        /// <summary>
        /// Reads the model classes from the cache or fetches them and caches them for reuse.
        /// </summary>
        /// <returns></returns>
        private List<MClass> GetModelClasses()
        {
            List<MClass> list = cache.Get( "classes" ) as List<MClass>;

            //cache.Remove( "classes" );
            if ( list == null )
            {
                list = ReadClassesFromAssembly();
                var cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddMinutes( GetAttributeValue("MinutesToCache").AsInteger() );
                cache.Set( "classes", list, cacheItemPolicy );
            }
            else
            {
                try
                {
                    list = cache.Get( "classes" ) as List<MClass>;
                }
                catch ( InvalidCastException )
                {
                    list = ReadClassesFromAssembly();
                    var cacheItemPolicy = new CacheItemPolicy();
                    cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddMinutes( GetAttributeValue( "MinutesToCache" ).AsInteger() );
                    cache.Set( "classes", list, cacheItemPolicy );
                }
            }

            return (List<MClass>)list;
        }

        /// <summary>
        /// Reads the classes from assembly.
        /// </summary>
        /// <returns></returns>
        private List<MClass> ReadClassesFromAssembly()
        {
            List<MClass> classes = new List<MClass>();

            Assembly rockDll = typeof( Rock.Model.EntitySet ).Assembly;

            ReadXMLComments( rockDll );

            foreach ( Type type in rockDll.GetTypes().OrderBy( t => t.Name ).ToArray() )
            {
                if ( type.FullName.StartsWith( "Rock.Model" ) )
                {
                    if ( type.InheritsOrImplements( typeof( Rock.Data.Entity<> ) ) )
                    {
                        var mClass = GetPropertiesAndMethods( type );
                        classes.Add( mClass );
                    }
                }
            }
            return classes;
        }

        /// <summary>
        /// Reads the XML comments from the assembly XML file.
        /// </summary>
        /// <param name="rockDll">The rock DLL.</param>
        private void ReadXMLComments( Assembly rockDll )
        {
            string rockDllPath = rockDll.Location;

            string docuPath = rockDllPath.Substring( 0, rockDllPath.LastIndexOf( "." ) ) + ".XML";

            if ( !File.Exists( docuPath ) )
            {
                docuPath =  HttpContext.Current.Server.MapPath("~") + @"bin\Rock.XML";
            }

            if ( File.Exists( docuPath ) )
            {
                _docuDoc = XDocument.Load( docuPath );
            }
        }

        /// <summary>
        /// Gets the properties and methods.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private MClass GetPropertiesAndMethods( Type type )
        {
            MClass mClass = new MClass
            {
                Name = type.Name,
                Guid = type.GUID.ToStringSafe(),
                Comment = GetComments( type )
            };

            PropertyInfo[] properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance ).Where( m => ( m.MemberType == MemberTypes.Method || m.MemberType == MemberTypes.Property ) ).ToArray();
            foreach ( PropertyInfo p in properties.OrderBy( i => i.Name ).ToArray() )
            {

                mClass.Properties.Add( new MProperty
                {
                    Name = p.Name,
                    IsLavaInclude = p.IsDefined( typeof( LavaIncludeAttribute ) ),
                    NotMapped = p.IsDefined( typeof( NotMappedAttribute ) ),
                    Id = p.MetadataToken,
                    Comment = GetComments( p )
                } );
            }

            MethodInfo[] methods = type.GetMethods( BindingFlags.Public | BindingFlags.Instance ).Where( m => !m.IsSpecialName && ( m.MemberType == MemberTypes.Method || m.MemberType == MemberTypes.Property ) ).ToArray();
            foreach ( MethodInfo m in methods.OrderBy( i => i.Name ).ToArray() )
            {
                // crazy, right?
                var param = string.Join( ", ", m.GetParameters().Select( pi => { var x = pi.ParameterType + " " + pi.Name; return x; } ) );

                mClass.Methods.Add( new MMethod
                {
                    Name = m.Name,
                    Id = m.MetadataToken,
                    Signature = string.Format( "{0}({1})", m.Name, param ),
                    Comment = GetComments( m )
                } );
            }

            return mClass;
        }

        /// <summary>
        /// Gets the comments from the data in the assembly's XML file for the 
        /// given member object.
        /// </summary>
        /// <param name="p">The MemberInfo instance.</param>
        /// <returns>an XmlComment object</returns>
        private XmlComment GetComments( MemberInfo p )
        {
            var prefix = string.Empty;

            if ( p.MemberType == MemberTypes.Property )
            {
                prefix = "P:";
            }
            else if ( p.MemberType == MemberTypes.Method )
            {
                prefix = "M:";
            }
            else if ( p.MemberType == MemberTypes.TypeInfo )
            {
                prefix = "T:";
            }
            else
            {
                return null;
            }

            XmlComment xmlComment = new XmlComment();

            string path = string.Format( "{0}{1}.{2}", prefix, ( p.DeclaringType != null ) ? p.DeclaringType.FullName : "Rock.Model", p.Name );

            var name = _docuDoc.Descendants( "member" ).FirstOrDefault( x => x.Attribute( "name" ).Value == path );
            if ( name != null )
            {
                xmlComment.Summary = System.Text.RegularExpressions.Regex.Replace( name.Element( "summary" ).ValueSafe(), @"\s+", " " );
                xmlComment.Value = name.Element( "value" ).ValueSafe();
                xmlComment.Remarks = name.Element( "remarks" ).ValueSafe();
                xmlComment.Returns = name.Element( "returns" ).ValueSafe();
                //xmlComment.Params = name.Elements("param").SelectMany( p.Name ).ToArray(); //  new { i.category_id, i.category_name }
            }

            return xmlComment;
        }
    }

    #region Helper Classes

    class MClass
    {
        public string Name { get; set; }
        public XmlComment Comment { get; set; }
        public string Guid { get; set; }
        public List<MProperty> Properties { get; set; }
        public List<MMethod> Methods { get; set; }

        public MClass()
        {
            Properties = new List<MProperty>();
            Methods = new List<MMethod>();
        }    
    }

    class MProperty
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsLavaInclude { get; set; }
        public bool NotMapped { get; set; }
        public XmlComment Comment { get; set; }
    }

    class MMethod
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Signature { get; set; }
        public XmlComment Comment { get; set; }
    }

    class XmlComment
    {
        public string Summary { get; set; }
        public string Value { get; set; }
        public string Remarks { get; set; }
        public string[] Params { get; set; }
        public string Returns { get; set; }
    }
    # endregion

    #region Extension Methods

    public static class Extensions
    {
        public static string ValueSafe( this XElement element, string defaultValue = "" )
        {
            if ( element != null )
            {
                return element.Value;
            }

            return defaultValue;
        }

        public static bool InheritsOrImplements( this Type child, Type parent )
        {
            parent = ResolveGenericTypeDefinition( parent );

            var currentChild = child.IsGenericType
                                ? child.GetGenericTypeDefinition()
                                : child;

            while ( currentChild != typeof( object ) )
            {
                if ( parent == currentChild || HasAnyInterfaces( parent, currentChild ) )
                {
                    return true;
                }

                currentChild = currentChild.BaseType != null
                            && currentChild.BaseType.IsGenericType
                                ? currentChild.BaseType.GetGenericTypeDefinition()
                                : currentChild.BaseType;

                if ( currentChild == null )
                {
                    return false;
                }
            }
            return false;
        }

        private static bool HasAnyInterfaces( Type parent, Type child )
        {
            return child.GetInterfaces()
                .Any( childInterface =>
                {
                    var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                    return currentInterface == parent;
                } );
        }

        private static Type ResolveGenericTypeDefinition( Type parent )
        {
            var shouldUseGenericType = true;
            if ( parent.IsGenericType && parent.GetGenericTypeDefinition() != parent )
            {
                shouldUseGenericType = false;
            }

            if ( parent.IsGenericType && shouldUseGenericType )
            {
                parent = parent.GetGenericTypeDefinition();
            }
            return parent;
        }
    }

    #endregion

}