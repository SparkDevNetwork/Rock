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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// Builds the model map document by reflecting over Rock's registered entity
    /// types and resolving their properties, methods, XML doc comments, enums, and
    /// defined values.
    /// </summary>
    /// <remarks>
    /// The reflection and comment-parsing logic here is ported from the live
    /// <c>Rock.Blocks.Example.ModelMap</c> block. Because this tool bootstraps a
    /// RockApp with a real database connection, the DefinedTypeCache and enum
    /// resolution behave identically to the block; the only differences are the
    /// removal of web-only concerns (see <see cref="LoadXmlComments"/> and
    /// <see cref="IntoHtml"/>).
    /// </remarks>
    internal class ModelMapBuilder
    {
        #region Fields

        /// <summary>
        /// The full path to the RockWeb folder, used as a fallback source for the
        /// compiled XML documentation file.
        /// </summary>
        private readonly string _rockWebPath;

        /// <summary>
        /// Cached XML doc comments for Rock.dll, keyed by member path (e.g. "T:...", "P:...").
        /// </summary>
        private Dictionary<string, XElement> _xmlComments;

        /// <summary>
        /// The physical database schema (columns, indexes, foreign keys) keyed by
        /// table name. Populated once at the start of a build.
        /// </summary>
        private Dictionary<string, TableSchema> _schema = new Dictionary<string, TableSchema>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Warnings about types that could not be loaded and were skipped during
        /// discovery. Surfaced by the caller so any omission is visible.
        /// </summary>
        private readonly List<string> _skippedEntityTypeNames = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets any warnings about types that could not be loaded in this headless
        /// process and were skipped during discovery.
        /// </summary>
        public IReadOnlyList<string> SkippedEntityTypeNames => _skippedEntityTypeNames;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMapBuilder"/> class.
        /// </summary>
        /// <param name="rockWebPath">The full path to the RockWeb folder.</param>
        public ModelMapBuilder( string rockWebPath )
        {
            _rockWebPath = rockWebPath;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the complete model map document, grouping every model by domain.
        /// </summary>
        /// <param name="includeMethods">Whether to include each model's methods in the output.</param>
        /// <returns>The populated <see cref="ModelMapDocument"/>.</returns>
        public ModelMapDocument Build( bool includeMethods )
        {
            _xmlComments = LoadXmlComments();
            _schema = LoadDatabaseSchema();

            var domainModels = new Dictionary<string, List<ModelMapEntry>>( StringComparer.OrdinalIgnoreCase );

            foreach ( var type in DiscoverModelTypes() )
            {
                var categoryName = GetCategoryName( type );

                if ( !domainModels.TryGetValue( categoryName, out var models ) )
                {
                    models = new List<ModelMapEntry>();
                    domainModels[categoryName] = models;
                }

                models.Add( BuildModelBag( type, includeMethods ) );
            }

            // Sort domains with "Other" last, then alphabetically, and models
            // alphabetically within each domain, so the file is deterministic.
            var domains = domainModels
                .OrderBy( kvp => kvp.Key == "Other" )
                .ThenBy( kvp => kvp.Key, StringComparer.Ordinal )
                .Select( kvp => new ModelMapDomain
                {
                    Domain = kvp.Key,
                    Models = kvp.Value.OrderBy( m => m.Name, StringComparer.Ordinal ).ToList()
                } )
                .ToList();

            return new ModelMapDocument
            {
                /*
                    7/17/2026 - CLAUDE

                    DateTime.UtcNow is used here instead of RockDateTime.Now. This
                    headless tool does not initialize an organization time zone, so
                    RockDateTime.Now is not reliable, and a UTC stamp is the stable,
                    unambiguous choice for a generated-artifact timestamp.

                    Reason: Headless tool has no configured org time zone.
                */
                GeneratedAtUtc = DateTime.UtcNow,
                RockVersion = Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber(),
                Domains = domains
            };
        }

        /// <summary>
        /// Returns the Rock domain category name for the given type. Falls back to
        /// "Other" if no <see cref="RockDomainAttribute"/> is present.
        /// </summary>
        /// <param name="type">The type to categorize.</param>
        private string GetCategoryName( Type type )
        {
            var domainAttr = type.GetCustomAttribute<RockDomainAttribute>( false );
            return domainAttr?.Name.IsNotNullOrWhiteSpace() == true ? domainAttr.Name : "Other";
        }

        /// <summary>
        /// Discovers the model types to document by reflecting over the Rock
        /// assembly, rather than using <c>EntityTypeCache.All()</c>.
        /// </summary>
        /// <remarks>
        /// The live block enumerates DB-registered entity types, but that path
        /// eagerly loads every registered CLR type (including component and plugin
        /// types) and throws in a headless process when one cannot be loaded.
        /// Reflecting over the Rock assembly instead is both resilient and a more
        /// accurate reflection of the models actually in the code. The include
        /// filter mirrors the block's own rule: a type is included when it is an
        /// <see cref="IEntity"/> or is decorated with <see cref="IncludeForModelMapAttribute"/>.
        /// </remarks>
        private IEnumerable<Type> DiscoverModelTypes()
        {
            var rockAssembly = Assembly.GetAssembly( typeof( IEntity ) );

            foreach ( var type in GetLoadableTypes( rockAssembly ) )
            {
                if ( type.IsAbstract || type.IsInterface )
                {
                    continue;
                }

                if ( type.IsDefined( typeof( NotMappedAttribute ), false ) )
                {
                    continue;
                }

                var isEntity = typeof( IEntity ).IsAssignableFrom( type );
                var isIncludedForModelMap = type.IsDefined( typeof( IncludeForModelMapAttribute ), false );

                if ( isEntity || isIncludedForModelMap )
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Builds a <see cref="ModelMapEntry"/> for the given type, including its
        /// properties, methods, XML comments, database schema, and obsolete status.
        /// </summary>
        /// <param name="type">The entity type to build the entry for.</param>
        /// <param name="includeMethods">Whether to include the model's methods.</param>
        private ModelMapEntry BuildModelBag( Type type, bool includeMethods )
        {
            var entry = new ModelMapEntry
            {
                Name = type.Name,
                Comment = GetComment( type ),
                IsObsolete = type.IsDefined( typeof( ObsoleteAttribute ) ) || type.IsDefined( typeof( RockObsolete ) ),
                Properties = new List<ModelMapPropertyEntry>()
            };

            if ( entry.IsObsolete )
            {
                entry.ObsoleteMessage = GetObsoleteMessage( type );
            }

            // Resolve the physical table name (the [Table] attribute name, falling
            // back to the type name) and look up its schema.
            var tableName = type.GetCustomAttribute<TableAttribute>()?.Name ?? type.Name;
            _schema.TryGetValue( tableName, out var tableSchema );

            var attributeTableName = type.GetCustomAttribute<TableAttribute>()?.Name;
            if ( attributeTableName.IsNotNullOrWhiteSpace() && !string.Equals( attributeTableName, type.Name, StringComparison.OrdinalIgnoreCase ) )
            {
                entry.TableName = attributeTableName;
            }

            if ( tableSchema != null )
            {
                entry.Indexes = tableSchema.Indexes.OrderBy( i => i.Name, StringComparer.Ordinal ).ToList();
                entry.ForeignKeys = tableSchema.ForeignKeys
                    .OrderBy( f => f.ColumnName, StringComparer.Ordinal )
                    .ThenBy( f => f.ReferenceTableName, StringComparer.Ordinal )
                    .ToList();
            }

            // Properties (only public getters).
            var properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                                 .Where( p => p.GetMethod?.IsPublic == true )
                                 .GroupBy( p => p.Name )
                                 .Select( g => g.OrderBy( p => p.DeclaringType != type ).First() )
                                 .OrderBy( p => p.Name );

            foreach ( var p in properties )
            {
                entry.Properties.Add( BuildPropertyBag( p, type, tableSchema ) );
            }

            // Deterministic property ordering so run-to-run output is byte-stable.
            entry.Properties = entry.Properties.OrderBy( p => p.Name, StringComparer.Ordinal ).ToList();

            // Methods are only included when explicitly requested.
            if ( includeMethods )
            {
                var methods = type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static )
                                  .Where( m => !m.IsSpecialName )
                                  .GroupBy( m => $"{m.Name}({string.Join( ", ", m.GetParameters().Select( pi => pi.ParameterType.Name ) )})" )
                                  .Select( g => g.OrderBy( m => m.DeclaringType != type ).First() )
                                  .OrderBy( m => m.Name );

                entry.Methods = methods
                    .Select( m => BuildMethodBag( m, type ) )
                    .OrderBy( m => m.Signature, StringComparer.Ordinal )
                    .ToList();
            }

            return entry;
        }

        /// <summary>
        /// Builds a <see cref="ModelMapPropertyEntry"/> for the given property.
        /// </summary>
        /// <param name="p">The property to describe.</param>
        /// <param name="declaringType">The type that owns this property, used to detect inheritance.</param>
        /// <param name="tableSchema">The physical schema for the model's table, or <see langword="null"/>.</param>
        private ModelMapPropertyEntry BuildPropertyBag( PropertyInfo p, Type declaringType, TableSchema tableSchema )
        {
            var property = new ModelMapPropertyEntry
            {
                Name = p.Name,
                Comment = GetComment( p ),
                IsInherited = p.DeclaringType != declaringType,
                IsVirtual = p.GetGetMethod( true )?.IsVirtual == true && !p.GetGetMethod( true ).IsFinal,
                IsLavaInclude = p.IsDefined( typeof( LavaVisibleAttribute ) ) ||
                                p.IsDefined( typeof( DataMemberAttribute ) ),
                IsAttributeQualifier = p.IsDefined( typeof( EnableAttributeQualificationAttribute ) ),
                IsObsolete = p.IsDefined( typeof( ObsoleteAttribute ) ) || p.IsDefined( typeof( RockObsolete ) ),
                ObsoleteMessage = GetObsoleteMessage( p ),
                NotMapped = p.IsDefined( typeof( NotMappedAttribute ) ),
                Required = p.IsDefined( typeof( RequiredAttribute ) ),
                IsEnum = p.PropertyType.IsEnum || Nullable.GetUnderlyingType( p.PropertyType )?.IsEnum == true,
                IsDefinedValue = p.Name.EndsWith( "ValueId" ) && p.IsDefined( typeof( DefinedValueAttribute ) )
            };

            // Fill physical schema facts (SQL type, length, scale, nullability,
            // primary key) from the database when the property maps to a column.
            // The column name is the [Column] override if present, else the name.
            var columnName = p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name;
            if ( tableSchema != null && tableSchema.Columns.TryGetValue( columnName, out var column ) )
            {
                property.DataType = column.DataType;
                property.Length = column.Length;
                property.Scale = column.Scale;
                property.IsNullable = column.IsNullable;
                property.IsPrimaryKey = column.IsPrimaryKey;
            }

            if ( property.IsEnum )
            {
                property.EnumValues = new Dictionary<string, string>();
                var enumType = Nullable.GetUnderlyingType( p.PropertyType ) ?? p.PropertyType;
                foreach ( Enum v in Enum.GetValues( enumType ) )
                {
                    property.EnumValues[v.ToString( "D" )] = v.ToString();
                }
            }
            else if ( property.IsDefinedValue )
            {
                property.DefinedType = BuildDefinedTypeInfo( p );
            }

            return property;
        }

        /// <summary>
        /// Builds the defined type information for a defined-value property: the
        /// defined type's guid (always available from the attribute) plus its name
        /// and system-defined values resolved from the database.
        /// </summary>
        /// <param name="p">The defined-value property.</param>
        private ModelMapDefinedTypeInfo BuildDefinedTypeInfo( PropertyInfo p )
        {
            var attr = p.GetCustomAttribute<DefinedValueAttribute>();

            if ( attr?.DefinedTypeGuid.HasValue != true )
            {
                return null;
            }

            var definedTypeInfo = new ModelMapDefinedTypeInfo
            {
                Guid = attr.DefinedTypeGuid.Value
            };

            try
            {
                // Resolve the defined type name and its system-defined values from
                // the database. If the lookup fails, keep the guid (which came from
                // the attribute) and degrade gracefully rather than aborting.
                var definedType = DefinedTypeCache.Get( attr.DefinedTypeGuid.Value );
                if ( definedType != null )
                {
                    definedTypeInfo.Name = definedType.Name;

                    // Only system defined values are stable across installations, so
                    // non-system values are intentionally excluded from the map.
                    definedTypeInfo.Values = definedType.DefinedValues
                        .Where( dv => dv.IsSystem )
                        .Select( dv => new ModelMapDefinedValueInfo
                        {
                            Guid = dv.Guid,
                            Value = dv.Value,
                            Description = dv.Description
                        } )
                        .ToList();
                }
            }
            catch
            {
                // Intentionally ignored: defined value resolution is best-effort.
            }

            return definedTypeInfo;
        }

        /// <summary>
        /// Builds a <see cref="ModelMapMethodEntry"/> for the given method.
        /// </summary>
        /// <param name="m">The method to describe.</param>
        /// <param name="declaringType">The type that owns this method, used to detect inheritance.</param>
        private ModelMapMethodEntry BuildMethodBag( MethodInfo m, Type declaringType )
        {
            var parameters = string.Join( ", ", m.GetParameters().Select( pi => $"{pi.ParameterType.Name} {pi.Name}" ) );

            return new ModelMapMethodEntry
            {
                Signature = $"{m.Name}({parameters})",
                Comment = GetComment( m ),
                IsInherited = m.DeclaringType != declaringType,
                IsObsolete = m.IsDefined( typeof( ObsoleteAttribute ) ) || m.IsDefined( typeof( RockObsolete ) ),
                ObsoleteMessage = GetObsoleteMessage( m )
            };
        }

        /// <summary>
        /// Loads the physical database schema for all tables. Failure is
        /// non-fatal: the schema-derived fields are simply omitted and a warning
        /// is recorded.
        /// </summary>
        private Dictionary<string, TableSchema> LoadDatabaseSchema()
        {
            try
            {
                var connectionString = RockApp.Current.InitializationSettings.ConnectionString;
                return DatabaseSchemaReader.Load( connectionString );
            }
            catch ( Exception ex )
            {
                _skippedEntityTypeNames.Add( $"Database schema could not be read ({ex.GetType().Name}); DataType/Length/Scale/Indexes/ForeignKeys will be omitted." );
                return new Dictionary<string, TableSchema>( StringComparer.OrdinalIgnoreCase );
            }
        }

        /// <summary>
        /// Returns the types that can be loaded from an assembly, tolerating a
        /// <see cref="ReflectionTypeLoadException"/> by returning only the types
        /// that loaded successfully and recording how many could not be loaded.
        /// </summary>
        /// <param name="assembly">The assembly to enumerate.</param>
        private IEnumerable<Type> GetLoadableTypes( Assembly assembly )
        {
            try
            {
                return assembly.GetTypes();
            }
            catch ( ReflectionTypeLoadException ex )
            {
                // Some types reference dependencies that cannot be loaded in this
                // headless process; return only the types that loaded cleanly and
                // record the count so the omission is visible rather than silent.
                var loaded = ex.Types.Where( t => t != null ).ToList();
                var failedCount = ex.Types.Length - loaded.Count;

                if ( failedCount > 0 )
                {
                    _skippedEntityTypeNames.Add( $"{failedCount} type(s) in {assembly.GetName().Name} could not be loaded and were skipped." );
                }

                return loaded;
            }
        }

        /// <summary>
        /// Loads the Rock.dll XML documentation file into a dictionary keyed by member path.
        /// </summary>
        /// <remarks>
        /// Unlike the live block, there is no <c>HttpContext</c> fallback. The doc
        /// file is looked for next to the loaded Rock.dll first, then in the
        /// RockWeb <c>bin</c> folder as a fallback (a built RockWeb always has it).
        /// </remarks>
        private Dictionary<string, XElement> LoadXmlComments()
        {
            var emptyResult = new Dictionary<string, XElement>();

            var rockDllPath = typeof( EntityType ).Assembly.Location;
            var docuPath = Path.ChangeExtension( rockDllPath, ".XML" );

            if ( !File.Exists( docuPath ) )
            {
                var rockWebBinPath = Path.Combine( _rockWebPath, "bin", "Rock.XML" );
                if ( File.Exists( rockWebBinPath ) )
                {
                    docuPath = rockWebBinPath;
                }
            }

            if ( !File.Exists( docuPath ) )
            {
                return emptyResult;
            }

            var xmlComments = new Dictionary<string, XElement>();
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
        /// Gets the XML doc comment (summary, value, remarks, returns, example) for
        /// a type, property, or method. Follows <c>inheritdoc</c> references for
        /// properties. Returns <see langword="null"/> when no comment is found.
        /// </summary>
        /// <param name="member">The type, property, or method to look up.</param>
        private ModelMapComment GetComment( MemberInfo member )
        {
            try
            {
                if ( _xmlComments == null || _xmlComments.Count == 0 )
                {
                    return null;
                }

                var path = GetXmlMemberPath( member );
                if ( path == null || !_xmlComments.TryGetValue( path, out var memberElement ) )
                {
                    return null;
                }

                // For properties that use <inheritdoc cref="P:..." />, follow the
                // reference and read the referenced property's comment instead.
                if ( member.MemberType == MemberTypes.Property && memberElement.Element( "summary" ) == null )
                {
                    var rawXml = ReadInnerXml( memberElement );
                    var inheritMatch = Regex.Match( rawXml, @"<inheritdoc cref=""P:(.*?)""(?:\s*/>|>(.*?)</inheritdoc>)" );
                    if ( inheritMatch.Success )
                    {
                        var inheritedPath = "P:" + inheritMatch.Groups[1].Value;
                        if ( _xmlComments.TryGetValue( inheritedPath, out var inheritedElement ) )
                        {
                            memberElement = inheritedElement;
                        }
                    }
                }

                var declaringFullName = ( member as Type )?.FullName ?? member.DeclaringType?.FullName;

                var comment = new ModelMapComment
                {
                    Summary = IntoHtml( ReadInnerXml( memberElement.Element( "summary" ) ), declaringFullName ),
                    Value = IntoHtml( ReadInnerXml( memberElement.Element( "value" ) ), declaringFullName ),
                    Remarks = IntoHtml( ReadInnerXml( memberElement.Element( "remarks" ) ), declaringFullName ),
                    Returns = IntoHtml( ReadInnerXml( memberElement.Element( "returns" ) ), declaringFullName ),
                    Example = IntoHtml( ReadInnerXml( memberElement.Element( "example" ) ), declaringFullName )
                };

                return comment.IsEmpty ? null : comment;
            }
            catch
            {
                // Intentionally ignored: missing or malformed comments are non-critical.
                return null;
            }
        }

        /// <summary>
        /// Builds the XML documentation member path for a type ("T:"), property
        /// ("P:"), or method ("M:").
        /// </summary>
        /// <param name="member">The member to build the path for.</param>
        private static string GetXmlMemberPath( MemberInfo member )
        {
            if ( member is Type type )
            {
                return type.FullName.IsNotNullOrWhiteSpace() ? $"T:{type.FullName}" : null;
            }

            if ( member.DeclaringType?.FullName.IsNullOrWhiteSpace() != false )
            {
                return null;
            }

            switch ( member.MemberType )
            {
                case MemberTypes.Property:
                    return $"P:{member.DeclaringType.FullName}.{member.Name}";

                case MemberTypes.Method:
                    return $"M:{member.DeclaringType.FullName}.{member.Name}";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the inner XML of an <see cref="XElement"/>, correctly positioning
        /// the reader via <c>MoveToContent()</c> before reading. Returns
        /// <see langword="null"/> if the element is null.
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
        /// Converts an XML doc inner XML string into safe HTML. Resolves type,
        /// property, and external see-refs into code labels or links, and maps
        /// para, c, and code tags to their HTML equivalents.
        /// </summary>
        /// <remarks>
        /// Unlike the live block, type references are never rendered as
        /// <c>?EntityType={guid}</c> hyperlinks (those are only meaningful inside
        /// the block's web UI). They render as plain <c>&lt;code&gt;</c> labels
        /// using the resolved friendly name when available.
        /// </remarks>
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

            // Resolve all <see .../> and <seealso .../> references.
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
                        // Generic framework types (e.g. Dictionary`2) compile to XML
                        // docs using a backtick; render them as plain code.
                        if ( value.Contains( "`" ) )
                        {
                            var genericDisplay = innerText.IsNotNullOrWhiteSpace() ? innerText : shortName;
                            return $"<code>{genericDisplay}</code>";
                        }

                        // Render as a code label using the short type name. Friendly
                        // name resolution via EntityTypeCache is intentionally not
                        // used here: it would force the fragile headless load of an
                        // arbitrary referenced type, and the label is cosmetic.
                        var display = innerText.IsNotNullOrWhiteSpace() ? innerText : shortName;

                        return $"<code>{display}</code>";
                    }

                    if ( prefix == "P" )
                    {
                        // Render property references as code. If the reference is to
                        // a property within the current model, strip the class prefix.
                        var propName = fullClassName.IsNotNullOrWhiteSpace() && value.StartsWith( fullClassName + ".", StringComparison.Ordinal )
                            ? value.Substring( fullClassName.Length + 1 )
                            : value;

                        return $"<code>{propName.Replace( "Rock.Model.", string.Empty )}</code>";
                    }

                    // M:, F:, etc. — render as code, applying the same prefix-stripping.
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
        /// Returns the obsolete message for the given member, formatted with the
        /// Rock version if a <see cref="RockObsolete"/> attribute is present.
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
                            // Historically Rock versions were 1.x where the minor
                            // version was the release number (1.8.0 -> v8); modern
                            // versions use the major version (18.1 -> v18.1).
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
                // Intentionally ignored: obsolete formatting is best-effort.
                return null;
            }
        }

        #endregion
    }
}
