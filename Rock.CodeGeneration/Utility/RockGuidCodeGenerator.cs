using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Rock;
using Rock.SystemGuid;

namespace Rock.CodeGeneration
{
    internal static class RockGuidCodeGenerator
    {
        private static Dictionary<string, string[]> RockGuidSearchFileNamesLookup = new Dictionary<string, string[]>();

        private static RockGuidAttribute TryGetCustomAttribute<GA>( this Type type ) where GA : RockGuidAttribute
        {
            try
            {
                return type.GetCustomAttribute<GA>( inherit: false);
            }
            catch ( FormatException )
            {
                Debug.WriteLine( $"Invalid RockGuidAttribute on {type} " );
                return null;
            }
        }

        private static RockGuidAttribute TryGetCustomAttribute<GA>( this MethodInfo methodInfo ) where GA : RockGuidAttribute
        {
            try
            {
                return methodInfo.GetCustomAttribute<GA>( inherit: false);
            }
            catch ( FormatException )
            {
                Debug.WriteLine( $"Invalid RockGuidAttribute on {methodInfo} " );
                return null;
            }
        }

        public static void EnsureRockGuidAttributes( string rootFolder )
        {
            // Add any missing BlockTypeGuid attributes on WebForm blocktypes
            EnsureRockGuidAttributesForWebFormBlockTypes( rootFolder );

            // RestController Guids
            EnsureRockGuidAttributesForType<System.Web.Http.ApiController, RestControllerGuidAttribute>( rootFolder,
                GetDatabaseGuidLookup( rootFolder, $"SELECT [Guid], [ClassName] FROM [RestController]", "ClassName" ),
                new Type[1] { typeof( Rock.Rest.ApiControllerBase ) } );

            // RestAction Guids
            EnsureRockGuidAttributesForControllerActionMethods( rootFolder );

            // Generate FieldTypeGuid attribute for FieldType
            EnsureRockGuidAttributesForType<Rock.Field.FieldType, FieldTypeGuidAttribute>( rootFolder, GetDatabaseGuidLookup( rootFolder, $"SELECT [Guid], [Class] FROM [FieldType]", "Class" ) );

            /* Generator EntityType Guid attribute for everything that ends up creating EntityType record */
            var entityTypeDatabaseGuidLookup = GetDatabaseGuidLookup( rootFolder, "SELECT [Guid], [Name] FROM [EntityType]", "Name" );

            // Rock.Data.Model, Rock.Data.Entity
            EnsureRockGuidAttributesForType<Rock.Data.IEntity, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );

            // Various MEF Entities
            // Note that the EnsureRockGuidAttributesForType<Rock.Extension.Component> would also find all these, but doing it by specific type just in case.
            EnsureRockGuidAttributesForType<Rock.Achievement.AchievementComponent, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );
            EnsureRockGuidAttributesForType<Rock.Badge.BadgeComponent, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );
            EnsureRockGuidAttributesForType<Rock.Reporting.DataFilterComponent, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );
            EnsureRockGuidAttributesForType<Rock.Reporting.DataSelectComponent, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );
            EnsureRockGuidAttributesForType<Rock.Reporting.DataTransformComponent, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );
            EnsureRockGuidAttributesForType<Rock.Workflow.ActionComponent, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );

            // Everything else that inherits from Rock.Extension.Component
            EnsureRockGuidAttributesForType<Rock.Extension.Component, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );

            string rockEntityBlockTypeLookupSql = @"SELECT bt.Guid
    , et.Name [Class]
FROM BlockType bt
JOIN EntityType et
    ON bt.EntityTypeId IS NOT NULL
        AND et.Id = bt.EntityTypeId
";

            // EntitType based IRockBlockType's need two Guids, EntityTypeGuid and BlockTypeGuid, so generate RockEntityTypeGuid and RockBlockTypeGuid for these.
            EnsureRockGuidAttributesForType<Rock.Blocks.IRockBlockType, EntityTypeGuidAttribute>( rootFolder, entityTypeDatabaseGuidLookup );
            EnsureRockGuidAttributesForType<Rock.Blocks.IRockBlockType, BlockTypeGuidAttribute>( rootFolder, GetDatabaseGuidLookup( rootFolder, rockEntityBlockTypeLookupSql, "Class" ) );
        }

        /// <summary>
        /// Ensures the type of the rock unique identifier attributes for.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="GA">The type of the ga.</typeparam>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="databaseGuidLookup">The database unique identifier lookup.</param>
        /// <param name="excludedTypes">The excluded types.</param>
        private static void EnsureRockGuidAttributesForType<T, GA>( string rootFolder, Lazy<Dictionary<string, Guid>> databaseGuidLookup, Type[] excludedTypes = null )
            where T : class
            where GA : RockGuidAttribute
        {
            Dictionary<Guid, string> systemGuidLookup = GetSystemGuidLookup();

            var types = Reflection.FindTypes( typeof( T ) ).Values.OrderBy( a => a.FullName ).ToList();

            if ( excludedTypes != null )
            {
                types = types.Where( a => !excludedTypes.Contains( a ) ).ToList();
            }


            // only do types that don't have already have RockGuid attribute
            types = types.Where( a => a.TryGetCustomAttribute<GA>() == null ).ToList();

            if ( !types.Any() )
            {
                return;
            }

            var processedTypes = new HashSet<Type>();
            var nameSpaces = types.Select( a => a.Namespace ).Distinct().ToArray();

            var guidTypeName = typeof( GA ).Name.Replace( "Attribute", string.Empty );

            var regExHasRockGuidWithGuid = new Regex( @"(:?^|\s)\[.*" + guidTypeName + @"\s*\(.*(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}\s*""\s*\)" );
            var regExHasRockGuidWithSystemGuidConst = new Regex( @"(:?^|\s)\[.*"+ guidTypeName + @"\s*\(.*Rock.SystemGuid.*\s*\)" );

            Dictionary<Type, List<string>> possibleClassDeclarationsCache = new Dictionary<Type, List<string>>();

            foreach ( var fileName in GetRockGuidSearchFileNames( rootFolder ) )
            {
                types = types.Where( a => !processedTypes.Contains( a ) ).ToList();

                if ( !types.Any() )
                {
                    return;
                }

                var sourceFileText = File.ReadAllText( fileName );

                var fileHasNameSpaces = nameSpaces.Any( x => sourceFileText.Contains( $"namespace {x}" ) );
                if ( !fileHasNameSpaces )
                {
                    continue;
                }

                var sourceFileLines = File.ReadAllLines( fileName );
                bool fileUsingsIncludeRockGuidNameSpace = sourceFileText.Contains( $"using {typeof( GA ).Namespace};" );

                foreach ( var type in types )
                {
                    var possibleClassDeclarations = possibleClassDeclarationsCache.GetValueOrNull( type );
                    if ( possibleClassDeclarations == null )
                    {
                        possibleClassDeclarations = new List<string>();
                        possibleClassDeclarations.Add( $"public class {type.Name} : " );
                        possibleClassDeclarations.Add( $"public partial class {type.Name} : " );
                        if ( type.IsSealed )
                        {
                            possibleClassDeclarations.Add( $"public sealed class {type.Name} : " );
                        }

                        if ( !type.IsPublic )
                        {
                            possibleClassDeclarations.Add( $"internal class {type.Name} : " );
                            possibleClassDeclarations.Add( $"internal sealed class {type.Name} : " );
                        }

                        possibleClassDeclarationsCache.Add( type, possibleClassDeclarations );
                    }

                    var fileMightHave = possibleClassDeclarations.Any( d => sourceFileText.Contains( d ) );
                    if ( !fileMightHave )
                    {
                        continue;
                    }

                    var sourceFileLine = sourceFileLines.Where( line => possibleClassDeclarations.Any( d => line.Contains( d ) ) ).FirstOrDefault();
                    if ( sourceFileLine.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var databaseRockGuidValue = databaseGuidLookup.Value.GetValueOrNull( type.FullName )?.ToString().ToUpper();
                    var rockGuidAttributeValue = type.TryGetCustomAttribute<GA>()?.Guid.ToString().ToUpper();
                    string databaseRockGuidSystemGuidName = databaseRockGuidValue != null ? systemGuidLookup.GetValueOrNull( new Guid( databaseRockGuidValue ) ) : null;
                    string rockGuidDeclaration;
                    var attributeTypeName = typeof( GA ).Name.ReplaceIfEndsWith( "Attribute", string.Empty );
                    var attributeTypeFullName = typeof( GA ).FullName.ReplaceIfEndsWith( "Attribute", string.Empty );
                    if ( fileUsingsIncludeRockGuidNameSpace )
                    {
                        rockGuidDeclaration = attributeTypeName;
                    }
                    else
                    {
                        rockGuidDeclaration = attributeTypeFullName;
                    }

                    if ( rockGuidAttributeValue == null )
                    {
                        string guidLine;
                        if ( databaseRockGuidSystemGuidName.IsNotNullOrWhiteSpace() )
                        {
                            guidLine = $"    [{rockGuidDeclaration}( {databaseRockGuidSystemGuidName} )]";
                        }
                        else
                        {
                            var guidValue = databaseRockGuidSystemGuidName ?? databaseRockGuidValue ?? Guid.NewGuid().ToString().ToUpper();
                            guidLine = $"    [{rockGuidDeclaration}( \"{guidValue}\")]";
                        }

                        // If this type isn't in the EntityType (or BlockType, etc) table yet, and it doesn't have a EntityTypeGuid (or BlockTypeGuid, etc) yet, see if we already code generated this with a new guid                     
                        var alreadyCodeGeneratedWithSomeRockGuidButNotCompiled = sourceFileLines.Any( ln => regExHasRockGuidWithGuid.IsMatch( ln ) || regExHasRockGuidWithSystemGuidConst.IsMatch( ln ) );

                        // Just in case, also look for exact guid 
                        var alreadyCodeGeneratedWithSameGuidButNotCompiled = sourceFileText.Contains( guidLine );

                        if ( !( alreadyCodeGeneratedWithSomeRockGuidButNotCompiled || alreadyCodeGeneratedWithSameGuidButNotCompiled ) )
                        {
                            sourceFileText = sourceFileText.Replace( sourceFileLine, $"{guidLine}{Environment.NewLine}{sourceFileLine}" );
                            File.WriteAllText( fileName, sourceFileText );
                        }
                    }
                    else
                    {
                        // Shouldn't happen since we are only processing Types that don't already have a RockGuid Attribute
                        if ( databaseRockGuidValue.IsNotNullOrWhiteSpace() && rockGuidAttributeValue != databaseRockGuidValue )
                        {
                            sourceFileText = sourceFileText.Replace( $"[{attributeTypeFullName}(\"{rockGuidAttributeValue}\")]", $"{rockGuidDeclaration}(\"{databaseRockGuidSystemGuidName ?? databaseRockGuidValue}\" )]" );
                            sourceFileText = sourceFileText.Replace( $"[{attributeTypeName}(\"{rockGuidAttributeValue}\")]", $"[{rockGuidDeclaration}(\"{databaseRockGuidSystemGuidName ?? databaseRockGuidValue}\" )]" );
                        }
                    }

                    processedTypes.Add( type );

                    var classCount = Regex.Matches( sourceFileText, "(public class |public partial class |internal class |internal partial class )" ).Count;
                    if ( classCount < 2 )
                    {
                        // found the type in this file and there is only one class in this file, so skip to the next file
                        break;
                    }
                }
            }

            foreach ( var unprocessedType in types.Where( a => !processedTypes.Contains( a ) ).ToList() )
            {
                // If there are still types left, and we've looked thru all the files, then show a debug message
                // Note that we don't look in code-generated files, so this might be OK
                Debug.WriteLine( $"INFO: Couldn't find source code for {unprocessedType.FullName}" );
            }
        }

        private static Dictionary<Guid, string> GetSystemGuidLookup()
        {
            var systemGuidAssembly = typeof( Rock.SystemGuid.DefinedValue ).Assembly;
            var systemGuidTypeFields = CodeGenHelpers.GetSystemGuidTypes( systemGuidAssembly )
                            .Where( a => a.Namespace == "Rock.SystemGuid" )
                                .SelectMany( a =>
                                     a.GetFields()
                                         .Select( s => new { Type = a, Field = s, GuidValue = s.GetValue( null ) as string } ) ).ToList();

            var systemGuidTypeFieldsUnique = systemGuidTypeFields
                .GroupBy( x => x.Field.GetValue( null ) )
                .Select( a => new
                {
                    GuidValue = a.Key,

                    // It is possible that SystemGuids might share the same value, probably because of Obsolete ones. So
                    // just in case there are SystemGuids that share the same value, prefer the ones that are not obsolete, and then take the first one
                    Value = a.OrderBy( x => x.Field.GetCustomAttribute<System.ObsoleteAttribute>() == null ? 0 : 1 ).FirstOrDefault()
                } );

            var systemGuidLookup = systemGuidTypeFieldsUnique.ToDictionary( k => k.Value.GuidValue.AsGuid(), v => $"{v.Value.Type.FullName}.{v.Value.Field.Name}" );
            return systemGuidLookup;
        }

        internal static Lazy<Dictionary<string, Guid>> GetDatabaseGuidLookup( string rootFolder, string sql, string classField )
        {
            return new Lazy<Dictionary<string, Guid>>( () =>
            {
                SqlConnection sqlconn = CodeGenHelpers.GetSqlConnection( rootFolder );
                sqlconn.Open();
                var qry = sqlconn.CreateCommand();
                qry.CommandType = System.Data.CommandType.Text;
                qry.CommandText = sql;
                DataSet dataSet = new DataSet();
                new SqlDataAdapter( qry ).Fill( dataSet );
                var databaseGuidLookup = dataSet.Tables[0].Rows.OfType<DataRow>().ToList().ToDictionary( k => k.Field<string>( classField ), v => v.Field<Guid>( "Guid" ) );
                return databaseGuidLookup;
            }
            );
        }

        private static string[] GetRockGuidSearchFileNames( string rootFolder )
        {
            var result = RockGuidSearchFileNamesLookup.GetValueOrNull( rootFolder );
            if ( result == null )
            {
                var searchDirectory = rootFolder.EnsureTrailingBackslash();
                var sourceFileNames = Directory.EnumerateFiles( searchDirectory, "*.cs", SearchOption.AllDirectories );

                sourceFileNames = sourceFileNames.Where( a => !a.Contains( ".localhistory" ) );
                sourceFileNames = sourceFileNames.Where( a => !a.Contains( "\\CodeGenerated\\" ) );

                result = sourceFileNames.OrderBy( a => a.Contains( @"\Rock\Rock\" ) ? 0 : 1 ).ToArray();
                RockGuidSearchFileNamesLookup.Add( rootFolder, result );
            }

            return result;
        }

        private static void EnsureRockGuidAttributesForControllerActionMethods( string rootFolder )
        {
            var processedMethodInfos = new HashSet<MethodInfo>();
            var methodInfoList = typeof( Rock.Rest.ApiControllerBase ).Assembly
                .GetTypes()
                .SelectMany( a => a.GetMethods() )
                .Where( a => a.GetCustomAttributes<System.Web.Http.RouteAttribute>()?.Any() == true )
                .OrderBy( a => a.Name ).ToList();

            SqlConnection sqlconn = CodeGenHelpers.GetSqlConnection( rootFolder );
            sqlconn.Open();
            var qry = sqlconn.CreateCommand();
            qry.CommandType = System.Data.CommandType.Text;
            qry.CommandText = @"SELECT [ApiId], [Guid] FROM RestAction ORDER BY ApiId ";
            DataSet dataSet = new DataSet();
            new SqlDataAdapter( qry ).Fill( dataSet );
            var restActions = dataSet.Tables[0].Rows.OfType<DataRow>().ToList().Select( a => new
            {
                ApiID = a.Field<string>( "ApiId" ),
                Guid = a.Field<Guid>( "Guid" )
            } ).OrderBy( a => a.ApiID ).ToList();

            var methodsLookup = methodInfoList.Select( methodInfo => new
            {
                ControllerName = methodInfo.DeclaringType.Name,
                StrippedMethodName = Regex.Replace( methodInfo.ToString(), @"((?:(?:\w+)?\.(?<name>[a-z_A-Z]\w+))+)", "${name}" ),
                MethodInfo = methodInfo,
                RouteAttribute = methodInfo.GetCustomAttributes<System.Web.Http.RouteAttribute>()?.FirstOrDefault(),
                RestActionGuidAttribute = methodInfo.TryGetCustomAttribute<RestActionGuidAttribute>()
            } ).OrderBy( a => a.ControllerName ).ThenBy( a => a.StrippedMethodName ).ToList();

            foreach ( var restAction in restActions )
            {
                var restActionApiParts = restAction.ApiID.Split( '^' );
                if ( restActionApiParts.Count() != 2 )
                {
                    continue;
                }

                var controllerName = restActionApiParts[0]
                    .Replace( "DELETE", "" )
                    .Replace( "POST", "" )
                    .Replace( "PATCH", "" )
                    .Replace( "PUT", "" )
                    .Replace( "OPTIONS", "" )
                    .Replace( "GET", "" ) + "Controller";

                var apiIdMethodName = restActionApiParts[1];
                var method = methodsLookup.Where( a => a.ControllerName == controllerName && a.StrippedMethodName == apiIdMethodName ).FirstOrDefault();
                if ( method == null )
                {
                    continue;
                }

                if ( method.RestActionGuidAttribute != null )
                {
                    continue;
                }

                var routeCode = $"Route( \"{method.RouteAttribute.Template}\" )]";
                var guidLine = $"[Rock.SystemGuid.RestActionGuid( \"{restAction.Guid.ToString().ToUpper()}\" )]";

                var possibleFileNames = GetRockGuidSearchFileNames( rootFolder ).Where( a => a.Contains( controllerName ) ).ToList();

                foreach ( var fileName in possibleFileNames )
                {
                    var sourceFileText = File.ReadAllText( fileName );
                    if ( sourceFileText.Contains( routeCode ) )
                    {
                        var fileLines = File.ReadAllLines( fileName );

                        var alreadyCodeGeneratedButNotCompiled = fileLines.Any( l => l.Contains( guidLine ) );
                        if ( !alreadyCodeGeneratedButNotCompiled )
                        {
                            var routeLines = fileLines.Where( l => l.Contains( routeCode ) ).ToList();
                            string routeLine = null;

                            int lineNumber = 0;
                            foreach ( var fileLine in fileLines )
                            {
                                if ( fileLine.Contains( routeCode ) )
                                {
                                    routeLine = fileLine;
                                }

                                // if the RouteLine is followed by line that has the method name and parameters, we probably got the right one
                                if ( routeLine.IsNotNullOrWhiteSpace() && fileLine.Contains( $" {method.MethodInfo.Name}(" ) )
                                {
                                    if ( routeLines.Count() > 1 )
                                    {
                                        var parameterNames = method.MethodInfo.GetParameters().Select( a => a.Name ).ToArray();
                                        if ( !parameterNames.All( x => fileLine.Contains( x ) ) )
                                        {
                                            continue;
                                        }
                                    }

                                    var updatedSourceFileLines = fileLines.ToList();
                                    updatedSourceFileLines.Insert( lineNumber, $"        {guidLine}" );
                                    var updatedSourceContents = updatedSourceFileLines.JoinStrings( Environment.NewLine ).TrimEnd();
                                    File.WriteAllText( fileName, updatedSourceContents );
                                    break;

                                }

                                lineNumber++;
                            }
                        }

                        break;
                    }
                }

            }
        }

        private static void EnsureRockGuidAttributesForWebFormBlockTypes( string rootFolder )
        {
            SqlConnection sqlconn = CodeGenHelpers.GetSqlConnection( rootFolder );
            sqlconn.Open();
            var qry = sqlconn.CreateCommand();
            qry.CommandType = System.Data.CommandType.Text;
            qry.CommandText = @"SELECT [Path], [Guid] FROM [BlockType] ORDER BY [Path] ";
            DataSet dataSet = new DataSet();
            new SqlDataAdapter( qry ).Fill( dataSet );
            var blockTypesFromDatabase = dataSet.Tables[0].Rows.OfType<DataRow>().ToList().Select( a => new
            {
                Path = a.Field<string>( "Path" ),
                Guid = a.Field<Guid>( "Guid" )
            } ).OrderBy( a => a.Path ).ToList();

            var blockClassSearch = new Regex( @"\s*public partial class .* : .*(Block|DashboardWidget).*" );

            var systemGuidLookup = GetSystemGuidLookup();

            var regExHasBlockTypeGuidWithGuid = new Regex( @"(:?^|\s)\[.*BlockTypeGuid\s*\(.*(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}\s*""\s*\)" );
            var regExHasBlockTypeGuidWithSystemGuidConst = new Regex( @"(:?^|\s)\[.*BlockTypeGuid\s*\(.*Rock.SystemGuid.*\s*\)" );

            foreach ( var blockTypeFromDatabase in blockTypesFromDatabase.Where( a => a.Path.IsNotNullOrWhiteSpace() ) )
            {
                var blockTypeFileName = blockTypeFromDatabase.Path.Replace( @"/", @"\" ).Replace( "~", Path.Combine( rootFolder, "RockWeb" ) ) + ".cs";
                if ( !File.Exists( blockTypeFileName ) )
                {
                    continue;
                }

                var blockTypeSourceLines = File.ReadAllLines( blockTypeFileName );

                // Only do BlockTypes that don't have already have RockGuid attribute.
                // Note that if block is in database with a different guid, it could be that the block hasn't been compiled/discovered yet.
                // If so, we'll keep the guid the guid that is in the source code, and Rock will update the Guid in the Database when the block is compiled/discovered
                // Reg finds pattern of a BlockType guid, but excludes from match if commented out (non whitespace before the [)
                var alreadyHasBlockTypeGuidAttribute = blockTypeSourceLines.Any( ln => regExHasBlockTypeGuidWithGuid.IsMatch( ln ) || regExHasBlockTypeGuidWithSystemGuidConst.IsMatch( ln ) );
                if ( alreadyHasBlockTypeGuidAttribute )
                {
                    continue;
                }

                var systemGuidConstName = systemGuidLookup.GetValueOrNull( blockTypeFromDatabase.Guid );
                string rockGuidLine;
                if ( systemGuidConstName != null )
                {
                    rockGuidLine = $"[Rock.SystemGuid.BlockTypeGuid( {systemGuidConstName} )]";
                }
                else
                {
                    rockGuidLine = $"[Rock.SystemGuid.BlockTypeGuid( \"{blockTypeFromDatabase.Guid.ToString().ToUpper()}\" )]";
                }

                bool alreadyHasRockGuidAttribute = blockTypeSourceLines.Where( a => a.Contains( rockGuidLine ) ).Any();
                if ( alreadyHasRockGuidAttribute )
                {
                    // shouldn't happen since we already existed if it has any BlockTypeGuid attribute in source, but just in case
                    continue;
                }

                int lineNumber = 0;
                foreach ( var blockTypeSourceLine in blockTypeSourceLines )
                {
                    if ( blockClassSearch.IsMatch( blockTypeSourceLine ) )
                    {
                        var updatedSourceLines = blockTypeSourceLines.ToList();
                        updatedSourceLines.Insert( lineNumber, "    " + rockGuidLine );
                        File.WriteAllText( blockTypeFileName, updatedSourceLines.JoinStrings( Environment.NewLine ).TrimEnd() );
                        break;
                    }

                    lineNumber++;
                }

                Debug.WriteLine( $"Couldn't find blockClassSearch for {blockTypeFileName}" );
            }
        }
    }
}