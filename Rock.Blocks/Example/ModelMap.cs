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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Lava;
using Rock.ViewModels.Blocks.Example.ModelMap;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Example
{
    [DisplayName( "Model Map" )]
    [Category( "Obsidian > Example" )]
    [Description( "Displays the details about each model class in Rock.Model." )]
    [IconCssClass( "ti ti-map" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "29AB673E-4D45-453E-91B5-02CFD819BB3A" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "B44723B8-2FCA-41C3-B89D-90CC08170B73" )]
    [Rock.SystemGuid.BlockTypeGuid( "DA2AAD13-209B-4885-8739-B7BE99F6510D" )]
    public class ModelMap : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EntityType = "EntityType";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Cached XML doc comments for Rock.dll
        /// </summary>
        private static Dictionary<string, XElement> _xmlComments;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ModelMapInitializationBox
            {
                InitialEntityTypeGuid = GetInitialEntityTypeGuid(),
                Categories = BuildCategories()
            };
        }

        /// <summary>
        /// Builds the list of model map categories from all registered entity types.
        /// </summary>
        private List<ModelMapCategoryBag> BuildCategories()
        {
            var categories = new Dictionary<string, ModelMapCategoryBag>( StringComparer.OrdinalIgnoreCase );

            // Make sure to register any types with the IncludeForModelMap attribute
            // so they show up in the list, even if they aren't entities
            RegisterIncludeForModelMapTypes();

            foreach ( var entity in EntityTypeCache.All() )
            {
                var type = entity.GetEntityType();
                if ( type == null )
                {
                    continue;
                }

                if ( !entity.IsEntity && type.GetCustomAttribute<IncludeForModelMapAttribute>() == null )
                {
                    continue;
                }

                var categoryName = GetCategoryName( type );

                if ( !categories.TryGetValue( categoryName, out var category ) )
                {
                    category = new ModelMapCategoryBag
                    {
                        Guid = Guid.NewGuid(),
                        Name = categoryName,
                        Models = new List<ListItemBag>()
                    };

                    categories[categoryName] = category;
                }

                category.Models.Add( entity.ToListItemBag() );
            }

            // Sort categories with "Other" at the end, then alphabetically.
            var sortedCategories = categories.Values
                .OrderBy( c => c.Name == "Other" )
                .ThenBy( c => c.Name )
                .ToList();

            // Also sort models within each category alphabetically.
            foreach ( var cat in sortedCategories )
            {
                cat.Models = cat.Models.OrderBy( m => m.Text ).ToList();
            }

            return sortedCategories;
        }

        /// <summary>
        /// Builds a <see cref="ModelMapModelBag"/> for the given type, including
        /// its properties, methods, XML comments, and obsolete status.
        /// </summary>
        /// <param name="type">The entity type to build the bag for.</param>
        private ModelMapModelBag BuildModelBag( Type type )
        {
            var modelBag = new ModelMapModelBag
            {
                Name = type.Name,
                IsObsolete = type.IsDefined( typeof( ObsoleteAttribute ) ) || type.IsDefined( typeof( RockObsolete ) ),
                Properties = new List<ModelMapPropertyBag>(),
                Methods = new List<ModelMapMethodBag>()
            };

            if ( modelBag.IsObsolete )
            {
                modelBag.ObsoleteMessage = GetObsoleteMessage( type );
            }

            var comments = GetComments( type );
            modelBag.Summary = comments?.Summary ?? string.Empty;
            modelBag.Example = comments?.Example ?? string.Empty;

            var actualTableName = type.GetCustomAttribute<TableAttribute>()?.Name;

            if ( actualTableName.IsNotNullOrWhiteSpace() && !string.Equals( actualTableName, type.Name, StringComparison.OrdinalIgnoreCase ) )
            {
                // TableName differs from type name; client will render alongside Name
                modelBag.TableName = actualTableName;
            }

            // Properties (only public getters)
            var properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                                 .Where( p => p.GetMethod?.IsPublic == true )
                                 .GroupBy( p => p.Name )
                                 .Select( g => g.OrderBy( p => p.DeclaringType != type ).First() )
                                 .OrderBy( p => p.Name );

            foreach ( var p in properties )
            {
                modelBag.Properties.Add( BuildPropertyBag( p, type ) );
            }

            // Methods
            var methods = type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static )
                              .Where( m => !m.IsSpecialName )
                              .GroupBy( m => $"{m.Name}({string.Join( ", ", m.GetParameters().Select( pi => pi.ParameterType.Name ) )})" )
                              .Select( g => g.OrderBy( m => m.DeclaringType != type ).First() )
                              .OrderBy( m => m.Name );

            foreach ( var m in methods )
            {
                modelBag.Methods.Add( BuildMethodBag( m, type ) );
            }

            return modelBag;
        }

        /// <summary>
        /// Builds a <see cref="ModelMapPropertyBag"/> for the given property.
        /// </summary>
        /// <param name="p">The property to describe.</param>
        /// <param name="declaringType">The type that owns this property, used to detect inheritance.</param>
        private ModelMapPropertyBag BuildPropertyBag( PropertyInfo p, Type declaringType )
        {
#pragma warning disable CS0618 // LavaIncludeAttribute is obsolete
            var property = new ModelMapPropertyBag
            {
                Id = p.MetadataToken,
                Name = p.Name,
                Comments = GetComments( p ),
                IsInherited = p.DeclaringType != declaringType,
                IsVirtual = p.GetGetMethod( true )?.IsVirtual == true && !p.GetGetMethod( true ).IsFinal,
                IsLavaInclude = p.IsDefined( typeof( LavaIncludeAttribute ) ) ||
                                p.IsDefined( typeof( LavaVisibleAttribute ) ) ||
                                p.IsDefined( typeof( DataMemberAttribute ) ),
                IsAttributeQualifier = p.IsDefined( typeof( EnableAttributeQualificationAttribute ) ),
                IsObsolete = p.IsDefined( typeof( ObsoleteAttribute ) ) || p.IsDefined( typeof( RockObsolete ) ),
                ObsoleteMessage = GetObsoleteMessage( p ),
                NotMapped = p.IsDefined( typeof( NotMappedAttribute ) ),
                Required = p.IsDefined( typeof( RequiredAttribute ) ),
                IsEnum = p.PropertyType.IsEnum || Nullable.GetUnderlyingType( p.PropertyType )?.IsEnum == true,
                IsDefinedValue = p.Name.EndsWith( "ValueId" ) && p.IsDefined( typeof( DefinedValueAttribute ) )
            };
#pragma warning restore CS0618

            if ( property.IsEnum )
            {
                property.KeyValues = new Dictionary<string, string>();
                var enumType = Nullable.GetUnderlyingType( p.PropertyType ) ?? p.PropertyType;
                foreach ( Enum v in Enum.GetValues( enumType ) )
                {
                    property.KeyValues[v.ToString( "D" )] = v.ToString();
                }

                property.EnumOrDefinedTypeDescription = "This is a hard coded list of values defined in the code as an enumeration.";
            }

            else if ( property.IsDefinedValue )
            {
                var attr = p.GetCustomAttribute<DefinedValueAttribute>();
                if ( attr?.DefinedTypeGuid.HasValue == true )
                {
                    var dt = DefinedTypeCache.Get( attr.DefinedTypeGuid.Value );
                    if ( dt != null )
                    {
                        property.KeyValues = dt.DefinedValues.ToDictionary( dv => $"{dv.Id} = {dv.Value}", dv => dv.Description );
                        property.EnumOrDefinedTypeDescription = $"These are found in the <a class='text-info' href='/admin/general/defined-types/{dt.Id}'>{dt.Name}</a> Defined Type.";
                    }
                }
            }

            return property;
        }

        /// <summary>
        /// Builds a <see cref="ModelMapMethodBag"/> for the given method.
        /// </summary>
        /// <param name="m">The method to describe.</param>
        /// <param name="declaringType">The type that owns this method, used to detect inheritance.</param>
        private ModelMapMethodBag BuildMethodBag( MethodInfo m, Type declaringType )
        {
            // pretty cool, right?
            var parameters = string.Join( ", ", m.GetParameters().Select( pi => $"{pi.ParameterType.Name} {pi.Name}" ) );

            return new ModelMapMethodBag
            {
                Id = m.MetadataToken,
                Signature = $"{m.Name}({parameters})",
                Comments = GetComments( m ),
                IsInherited = m.DeclaringType != declaringType,
                IsObsolete = m.IsDefined( typeof( ObsoleteAttribute ) ) || m.IsDefined( typeof( RockObsolete ) ),
                ObsoleteMessage = GetObsoleteMessage( m )
            };
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Reads the <see cref="PageParameterKey.EntityType"/> page parameter and resolves
        /// it to an entity type Guid, accepting a Guid, integer Id, or string name.
        /// </summary>
        private Guid? GetInitialEntityTypeGuid()
        {
            var param = PageParameter( PageParameterKey.EntityType );
            if ( param.IsNullOrWhiteSpace() )
            {
                return null;
            }

            EntityTypeCache entityType = null;

            var paramAsGuid = param.AsGuidOrNull();
            if ( paramAsGuid.HasValue )
            {
                entityType = EntityTypeCache.Get( paramAsGuid.Value );
            }
            else
            {
                var paramAsInt = param.AsIntegerOrNull();
                entityType = paramAsInt.HasValue
                    ? EntityTypeCache.Get( paramAsInt.Value )
                    : EntityTypeCache.Get( param );
            }

            return entityType?.Guid;
        }

        /// <summary>
        /// Returns the Rock domain category name for the given type.
        /// Falls back to "Other" if no <see cref="RockDomainAttribute"/> is present.
        /// </summary>
        /// <param name="type">The type to categorize.</param>
        private string GetCategoryName( Type type )
        {
            var domainAttr = type.GetCustomAttribute<RockDomainAttribute>( false );
            return domainAttr?.Name.IsNotNullOrWhiteSpace() == true ? domainAttr.Name : "Other";
        }

        /// <summary>
        /// Ensures that all types decorated with <see cref="IncludeForModelMapAttribute"/> are
        /// registered in <see cref="EntityTypeCache"/> so they appear in the model map.
        /// </summary>
        private void RegisterIncludeForModelMapTypes()
        {
            var assembly = Assembly.GetAssembly( typeof( IncludeForModelMapAttribute ) );
            var types = assembly.GetTypes().Where( t => t.IsDefined( typeof( IncludeForModelMapAttribute ), false ) );

            foreach ( var type in types )
            {
                EntityTypeCache.Get( type, true, null );
            }
        }

        /// <summary>
        /// Loads the Rock.dll XML documentation file into a dictionary.
        /// </summary>
        private Dictionary<string, XElement> LoadXmlComments()
        {
            var xmlComments = new Dictionary<string, XElement>();

            var rockDll = typeof( EntityType ).Assembly;
            string rockDllPath = rockDll.Location;
            string docuPath = Path.ChangeExtension( rockDllPath, ".XML" );

            if ( !File.Exists( docuPath ) && HttpContext.Current != null )
            {
                try
                {
                    docuPath = HttpContext.Current.Server.MapPath( "~/bin/Rock.XML" );
                }
                catch { }
            }

            if ( !File.Exists( docuPath ) )
            {
                return new Dictionary<string, XElement>();
            }

            var docuDoc = XDocument.Load( docuPath );

            foreach ( var member in docuDoc.Descendants( "member" ) )
            {
                var name = member.Attribute( "name" )?.Value;
                if ( string.IsNullOrWhiteSpace( name ) )
                {
                    continue;
                }

                xmlComments[name] = member;
            }

            return xmlComments;
        }

        /// <summary>
        /// Gets the XML doc comments for the given type, including summary and example.
        /// </summary>
        /// <param name="type">The type to look up.</param>
        private ModelMapXmlCommentBag GetComments( Type type )
        {
            var bag = new ModelMapXmlCommentBag();
            try
            {
                if ( _xmlComments == null || _xmlComments.Count == 0 )
                {
                    return bag;
                }

                var path = $"T:{type.FullName}";

                if ( !_xmlComments.TryGetValue( path, out var memberElement ) )
                {
                    return bag;
                }

                bag.Summary = IntoHtml( ReadInnerXml( memberElement.Element( "summary" ) ), type.FullName );
                bag.Example = IntoHtml( ReadInnerXml( memberElement.Element( "example" ) ), type.FullName );
            }
            catch { }

            return bag;
        }

        /// <summary>
        /// Gets the XML doc summary for the given property or method member.
        /// Follows inheritdoc references for properties.
        /// </summary>
        /// <param name="member">The property or method to look up.</param>
        private string GetComments( MemberInfo member )
        {
            try
            {
                if ( _xmlComments == null || _xmlComments.Count == 0 )
                {
                    return null;
                }

                string prefix;
                switch ( member.MemberType )
                {
                    case MemberTypes.Property:
                        prefix = "P:";
                        break;

                    case MemberTypes.Method:
                        prefix = "M:";
                        break;

                    default:
                        prefix = null;
                        break;
                }

                if ( prefix == null || member.DeclaringType?.FullName.IsNullOrWhiteSpace() == true )
                {
                    return null;
                }

                var path = $"{prefix}{member.DeclaringType.FullName}.{member.Name}";

                if ( !_xmlComments.TryGetValue( path, out var memberElement ) )
                {
                    return null;
                }

                // For properties that use <inheritdoc cref="P:..." />, follow the reference
                // and pull the summary from the referenced property's XML entry.
                if ( member.MemberType == MemberTypes.Property && memberElement.Element( "summary" ) == null )
                {
                    var rawXml = ReadInnerXml( memberElement );
                    var inheritMatch = Regex.Match( rawXml, @"<inheritdoc cref=""P:(.*?)""(?:\s*/>|>(.*?)</inheritdoc>)" );
                    if ( inheritMatch.Success )
                    {
                        var inheritedPath = "P:" + inheritMatch.Groups[1].Value;
                        if ( _xmlComments.TryGetValue( inheritedPath, out var inheritedElement ) )
                        {
                            return IntoHtml( ReadInnerXml( inheritedElement.Element( "summary" ) ), member.DeclaringType.FullName );
                        }

                        return null;
                    }
                }

                return IntoHtml( ReadInnerXml( memberElement.Element( "summary" ) ), member.DeclaringType.FullName );
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Reads the inner XML of an <see cref="XElement"/>, correctly positioning the reader
        /// via <c>MoveToContent()</c> before reading. Returns <see langword="null"/> if the element is null.
        /// </summary>
        /// <param name="element">The element to read inner XML from.</param>
        private static string ReadInnerXml( XElement element )
        {
            if ( element == null )
            {
                return null;
            }

            using ( var reader = element.CreateReader() )
            {
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }
        }

        /// <summary>
        /// Converts an XML doc inner XML string into safe HTML for use in v-html.
        /// Resolves type, property, and external see-refs into links or code labels,
        /// and maps para, c, and code tags to their HTML equivalents.
        /// </summary>
        /// <param name="innerXml">The raw inner XML from an XML doc element.</param>
        /// <param name="fullClassName">The declaring type's full name, used to shorten property ref labels.</param>
        private string IntoHtml( string innerXml, string fullClassName = null )
        {
            if ( string.IsNullOrWhiteSpace( innerXml ) )
            {
                return null;
            }

            // Collapse whitespace.
            innerXml = Regex.Replace( innerXml, @"\s+", " " );

            // Map XML doc structural tags to their HTML equivalents.
            innerXml = innerXml
                .Replace( "<para>", " " ).Replace( "</para>", " " )
                .Replace( "<example>", "<p>" ).Replace( "</example>", "</p>" )
                .Replace( "<code>", "<pre>" ).Replace( "</code>", "</pre>" )
                .Replace( "<c>", "<code>" ).Replace( "</c>", "</code>" );

            // Resolve all <see .../> and <seealso .../> references
            return Regex.Replace( innerXml, @"<see\w*([^>]*?)(?:/>|>(.*?)</see\w*>)", match =>
            {
                var attrs = match.Groups[1].Value;
                var innerText = match.Groups[2].Value;

                // langword="null" or langword="true", etc.
                var langwordMatch = Regex.Match( attrs, @"langword=""([^""]+)""" );
                if ( langwordMatch.Success )
                {
                    var word = langwordMatch.Groups[1].Value;
                    return $"<code>{word}</code>";
                }

                // External href - open in a new tab.
                var hrefMatch = Regex.Match( attrs, @"href=""([^""]+)""" );
                if ( hrefMatch.Success )
                {
                    var url = hrefMatch.Groups[1].Value;
                    return $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener\">{( innerText.IsNotNullOrWhiteSpace() ? innerText : url )}</a>";
                }

                // cref — split on the type prefix separator ( "T:", "P:", "M:", etc. ).
                var crefMatch = Regex.Match( attrs, @"cref=""([^:""]+):([^""]+)""" );
                if ( crefMatch.Success )
                {
                    var prefix = crefMatch.Groups[1].Value;
                    var value = crefMatch.Groups[2].Value;
                    var shortName = value.Split( '.' ).LastOrDefault() ?? value;

                    if ( prefix == "T" )
                    {
                        /*
                             2/26/2026 - MSE

                             Generic framework types (e.g. Dictionary<TKey, TValue>) compile to XML docs 
                             using a backtick (e.g. "Dictionary`2").

                             Reason: Prevent the ModelMap from attempting to hyperlink standard .NET generic collections.
                        */
                        if ( value.Contains( "`" ) )
                        {
                            var genericDisplay = innerText.IsNotNullOrWhiteSpace() ? innerText : shortName;
                            return $"<code>{genericDisplay}</code>";
                        }

                        // Render Type references as a link to automatically select that model.
                        var entityType = EntityTypeCache.Get( value );
                        var display = innerText.IsNotNullOrWhiteSpace() ? innerText : entityType?.FriendlyName ?? shortName;

                        if ( entityType != null )
                        {
                            var type = entityType.GetEntityType();
                            if ( type != null && !type.IsInterface )
                            {
                                return $"<a href=\"?EntityType={entityType.Guid}\" title=\"{shortName}\">{display}</a>";
                            }
                        }

                        // For interfaces or unknown types, render as a code block.
                        return $"<code>{display}</code>";
                    }

                    if ( prefix == "P" )
                    {
                        // Render Property references as a code block. If the reference is
                        // to a property within the current model, strip the class name prefix.
                        var propName = fullClassName.IsNotNullOrWhiteSpace() && value.StartsWith( fullClassName + ".", StringComparison.Ordinal )
                            ? value.Substring( fullClassName.Length + 1 )
                            : value;

                        return $"<code>{propName.Replace( "Rock.Model.", string.Empty )}</code>";
                    }

                    // M:, F:, etc. — render as a code block. Apply the same prefix-stripping
                    // as properties so cross-model methods show their type.
                    var finalName = innerText;
                    if ( finalName.IsNullOrWhiteSpace() )
                    {
                        var memberName = fullClassName.IsNotNullOrWhiteSpace() && value.StartsWith( fullClassName + ".", StringComparison.Ordinal )
                            ? value.Substring( fullClassName.Length + 1 )
                            : value;

                        memberName = memberName.Replace( "Rock.Model.", string.Empty );
                        finalName = prefix == "M" ? $"{memberName}()" : memberName;
                    }

                    return $"<code>{finalName}</code>";
                }

                return innerText;
            } ).Trim();
        }

        /// <summary>
        /// Returns the obsolete message for the given member, formatted with the Rock
        /// version if a <see cref="RockObsolete"/> attribute is present.
        /// </summary>
        /// <param name="member">The member to inspect.</param>
        private string GetObsoleteMessage( MemberInfo member )
        {
            if ( !member.IsDefined( typeof( ObsoleteAttribute ) ) )
            {
                return null;
            }

            try
            {
                string message = "";
                if ( member.IsDefined( typeof( RockObsolete ) ) )
                {
                    var rockObsolete = member.GetCustomAttribute<RockObsolete>();
                    if ( rockObsolete?.Version.IsNotNullOrWhiteSpace() == true )
                    {
                        if ( Rock.Utility.RockSemanticVersion.TryParse( rockObsolete.Version, out var version ) )
                        {
                            /*
                                 2/25/2026 - MSE

                                 Historically Rock versions were 1.x where the Minor version represented
                                 the actual release number (e.g. 1.8.0 -> v8). Modern versions just
                                 use the Major version as the release number (e.g. 18.1 -> v18.1).

                                 This ensures the obsolete message formats the version correctly.

                                 Reason: Keep obsolete version messaging consistent across version formats.
                            */
                            if ( version.Major == 1 )
                            {
                                message = $"[Obsoleted in v{version.Minor}";
                                message += version.Patch > 0 ? $".{version.Patch}] " : "] ";
                            }
                            else
                            {
                                message = $"[Obsoleted in v{version.Major}";
                                message += version.Minor > 0 ? $".{version.Minor}] " : "] ";
                            }
                        }
                        else
                        {
                            message = $"[Obsoleted in v{rockObsolete.Version}] ";
                        }
                    }
                }

                var obsoleteAttr = member.GetCustomAttribute<ObsoleteAttribute>();
                if ( obsoleteAttr != null && obsoleteAttr.Message.IsNotNullOrWhiteSpace() )
                {
                    message += obsoleteAttr.Message;
                }

                return message;
            }
            catch
            {
                return null;
            }
        }

        #endregion Helper Methods

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetModelDetails( Guid entityTypeGuid )
        {
            var entityType = EntityTypeCache.Get( entityTypeGuid );
            var type = entityType?.GetEntityType();

            if ( type == null )
            {
                return ActionNotFound();
            }

            if ( _xmlComments == null )
            {
                _xmlComments = LoadXmlComments();
            }

            return ActionOk( BuildModelBag( type ) );
        }

        #endregion Block Actions
    }
}
