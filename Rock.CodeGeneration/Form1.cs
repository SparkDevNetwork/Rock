using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Rock;

namespace Rock.CodeGeneration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Form1 : Form
    {
        PluralizationService pls = PluralizationService.CreateService( new CultureInfo( "en-US" ) );
        Assembly rockAssembly = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1" /> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            rockAssembly = typeof( Rock.Data.IEntity ).Assembly;
            FileInfo fi = new FileInfo( ( new System.Uri( rockAssembly.CodeBase ) ).AbsolutePath );
            lblAssemblyPath.Text = fi.FullName;
            lblAssemblyDateTime.Text = fi.LastWriteTime.ToElapsedString();
            Cursor = Cursors.WaitCursor;

            cblModels.Items.Clear();

            string assemblyFileName = fi.FullName;

            lblAssemblyPath.Text = assemblyFileName;

            toolTip1.SetToolTip( lblAssemblyDateTime, fi.LastWriteTime.ToString() );

            var assembly = Assembly.LoadFrom( assemblyFileName );

            foreach ( Type type in assembly.GetTypes().OfType<Type>().OrderBy( a => a.FullName ) )
            {
                if ( type.Namespace != null && !type.Namespace.StartsWith( "Rock.Data" ) && !type.IsAbstract && type.GetCustomAttribute<NotMappedAttribute>() == null )
                {
                    if ( typeof( Rock.Data.IEntity ).IsAssignableFrom( type ) || type.GetCustomAttribute( typeof( TableAttribute ) ) != null )
                    {
                        cblModels.Items.Add( type );
                    }
                }
            }

            CheckAllItems( true );
            cbSelectAll.Checked = true;

            Cursor = Cursors.Default;


            var projectName = Path.GetFileNameWithoutExtension( lblAssemblyPath.Text );

            tbServiceFolder.Text = Path.Combine( RootFolder().FullName, projectName );
            tbRestFolder.Text = Path.Combine( RootFolder().FullName, projectName + ".Rest" );
            tbClientFolder.Text = Path.Combine( RootFolder().FullName, projectName + ".Client" );
            tbDatabaseFolder.Text = Path.Combine( RootFolder().FullName, "Database" );

            if ( projectName != "Rock" )
            {
                tbRestFolder.Text = Path.Combine( RootFolder().FullName, projectName + "\\Rest" );
                tbDatabaseFolder.Text = Path.Combine( RootFolder().FullName, Path.GetFileNameWithoutExtension( projectName ) + ".Database" );
            }

            SqlConnection sqlconn = GetSqlConnection( RootFolder().FullName );
            if ( sqlconn != null )
            {
                lblDatabase.Text = sqlconn.Database;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbSelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void cbSelectAll_CheckedChanged( object sender, EventArgs e )
        {
            CheckAllItems( cbSelectAll.Checked );
        }

        /// <summary>
        /// Handles the Click event of the btnGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void btnGenerate_Click( object sender, EventArgs e )
        {
            tbResults.Text = string.Empty;
            string serviceFolder = tbServiceFolder.Text;
            string restFolder = tbRestFolder.Text;
            string rockClientFolder = tbClientFolder.Text;

            Cursor = Cursors.WaitCursor;

            progressBar1.Visible = true;
            progressBar1.Maximum = cblModels.CheckedItems.Count;
            progressBar1.Value = 0;
            if ( cblModels.CheckedItems.Count > 0 )
            {
                var rootFolder = RootFolder();
                if ( rootFolder != null )
                {
                    if ( cbClient.Checked )
                    {
                        var codeGenFolder = Path.Combine( rockClientFolder, "CodeGenerated" );
                        if ( Directory.Exists( codeGenFolder ) )
                        {
                            Directory.Delete( codeGenFolder, true );
                        }

                        Directory.CreateDirectory( Path.Combine( rockClientFolder, "CodeGenerated" ) );
                    }

                    foreach ( object item in cblModels.CheckedItems )
                    {
                        progressBar1.Value++;
                        Type type = ( Type ) item;

                        // only generate Service and REST for IEntity types
                        if ( typeof( Rock.Data.IEntity ).IsAssignableFrom( type ) )
                        {

                            if ( cbService.Checked )
                            {
                                WriteServiceFile( serviceFolder, type );
                            }

                            if ( cbRest.Checked )
                            {
                                WriteRESTFile( restFolder, type );
                            }
                        }

                        if ( cbClient.Checked )
                        {
                            WriteRockClientFile( rockClientFolder, type );
                        }
                    }

                    var projectName = Path.GetFileNameWithoutExtension( lblAssemblyPath.Text );

                    if ( cbClient.Checked )
                    {
                        WriteRockClientIncludeClientFiles( rockClientFolder, cblModels.CheckedItems.OfType<Type>().ToList() );
                        WriteRockClientSystemGuidFiles( rockClientFolder );
                        WriteRockClientEnumsFile( rockClientFolder );
                    }

                    if ( cbDatabaseProcs.Checked )
                    {
                        WriteDatabaseProcsScripts( tbDatabaseFolder.Text, projectName );
                    }

                    if ( cbEnsureCopyrightHeaders.Checked )
                    {
                        EnsureCopyrightHeaders( rootFolder.FullName );
                    }
                }
            }

            ReportRockCodeWarnings();

            progressBar1.Visible = false;
            Cursor = Cursors.Default;
            MessageBox.Show( "Files have been generated" );
        }

        /// <summary>
        /// Reports the rock code warnings.
        /// </summary>
        public void ReportRockCodeWarnings()
        {
            StringBuilder missingDbSetWarnings = new StringBuilder();
            StringBuilder rockObsoleteWarnings = new StringBuilder();
            StringBuilder singletonClassVariablesWarnings = new StringBuilder();
            List<string> obsoleteList = new List<string>();
            List<Assembly> rockAssemblyList = new List<Assembly>();
            rockAssemblyList.Add( typeof( Rock.Data.RockContext ).Assembly );
            rockAssemblyList.Add( typeof( Rock.Rest.ApiControllerBase ).Assembly );

            /* List any EntityTypes that don't have an associated DbSet<T> in RockContext */
            var dbSetEntityType = typeof( Rock.Data.RockContext ).GetProperties().Where( a => a.PropertyType.IsGenericType && a.PropertyType.Name == "DbSet`1" ).Select( a => a.PropertyType.GenericTypeArguments[0] ).ToList();
            var entityTypes = cblModels.Items.Cast<Type>().ToList();
            var missingDbSets = entityTypes.Where( a => !dbSetEntityType.Any( x => x.FullName == a.FullName ) ).ToList();
            if ( missingDbSets.Any() )
            {
                missingDbSetWarnings.AppendLine( missingDbSets.Select( a => $" - {a.Name}" ).ToList().AsDelimited( "\r\n" ) + "\r\n\r\n" );
            }

            foreach ( var rockAssembly in rockAssemblyList )
            {
                Type[] allTypes = rockAssembly.GetTypes();

                // ignore anonymous types (see https://stackoverflow.com/a/2483048/1755417)
                allTypes = allTypes.Where( a =>
                    a.IsClass == true &&
                    a.GetCustomAttributes<CompilerGeneratedAttribute>()?.Any() != true
                    && a.GetCustomAttributes<DebuggerDisplayAttribute>()?.Any() != true ).ToArray();

                foreach ( var type in allTypes.OrderBy( a => a.FullName ) )
                {
                    /* See if the class is Obsolete/RockObsolete */
                    ObsoleteAttribute typeObsoleteAttribute = type.GetCustomAttribute<ObsoleteAttribute>();
                    if ( typeObsoleteAttribute != null )
                    {
                        var rockObsolete = type.GetCustomAttribute<RockObsolete>();
                        if ( rockObsolete == null )
                        {
                            rockObsoleteWarnings.AppendLine( $" - {type}" );
                        }
                        else
                        {
                            obsoleteList.Add( $"{rockObsolete.Version},{type.Name},class,{typeObsoleteAttribute.IsError}" );
                        }
                    }

                    // get all members so we can see if there are warnings that we want to show
                    var memberList = type
                        .GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static )
                        .OrderBy( a => a.Name )
                        .ToList();

                    foreach ( MemberInfo member in memberList )
                    {
                        /* See if member is Obsolete/RockObsolete */
                        ObsoleteAttribute memberObsoleteAttribute = member.GetCustomAttribute<ObsoleteAttribute>();
                        if ( memberObsoleteAttribute != null && rockAssembly == member.Module.Assembly && member.DeclaringType == type )
                        {
                            var rockObsolete = member.GetCustomAttribute<RockObsolete>();
                            if ( rockObsolete == null )
                            {
                                rockObsoleteWarnings.AppendLine( $" - {member.DeclaringType}.{member.Name}" );
                            }
                            else
                            {
                                string messagePrefix = null;
                                if ( rockObsolete.Version == "1.8" || rockObsolete.Version.StartsWith( "1.8." ) || rockObsolete.Version == "1.7" || rockObsolete.Version.StartsWith( "1.7." ) )
                                {
                                    if ( !memberObsoleteAttribute.IsError || rockObsolete.Version == "1.7" || rockObsolete.Version.StartsWith( "1.7." ) )
                                    {
                                        messagePrefix = "###WARNING###:";
                                    }
                                }

                                obsoleteList.Add( $"{messagePrefix}{rockObsolete.Version},{type.Name} {member.Name},{member.MemberType},{memberObsoleteAttribute.IsError}" );
                            }
                        }

                        /* See if a singleton has class variables that are not thread-safe
                           NOTE: This won't catch all of them, but hopefully most
                         */

                        // types that OK based on how they are used
                        var ignoredThreadSafeTypeWarning = new Type[] {
                            typeof(Rock.UniversalSearch.IndexComponents.Lucene),
                        };

                        // fields that OK based on how we use them
                        var ignoredThreadSafeFieldWarning = new string[]
                        {
                            "Rock.Extension.Component.Attributes",
                            "Rock.Extension.Component.AttributeValues",
                            "Rock.Web.HttpModules.ResponseHeaders.Headers",
                            "Rock.Field.FieldType.QualifierUpdated"
                        };

                        if ( typeof( Rock.Field.FieldType ).IsAssignableFrom( type )
                            || typeof( Rock.Extension.Component ).IsAssignableFrom( type )
                            )
                        {
                            if ( member is FieldInfo fieldInfo )
                            {
                                if ( ignoredThreadSafeTypeWarning.Contains( type ) )
                                {
                                    continue;
                                }

                                /* 2020-05-11 MDP - To detect non-thread safe fields and properties
                                    - All properties have a field behind them, even ones with a simple get/set (those will be named *k__BackingField)
                                    - So this will also end up finding non-threadsafe properties as well

                                    - A class level variable on a singleton is not thread safe, except for the following situations
                                       -- It is a constant (IsLiteral)
                                       -- It is a readonly field (IsInitOnly)
                                       -- Is a static field with a [ThreadStatic] attribute.
                                           -- Note: If has to both [ThreadStatic] AND a static field to be threadsafe.
                                 */

                                // Also, don't worry about values that are only set in the Constructor (IsInitOnly), since Singletons only get constructed once
                                if ( !( fieldInfo.IsLiteral || fieldInfo.IsInitOnly ) )
                                {
                                    var isThreadStatic = ( fieldInfo.IsStatic && fieldInfo.GetCustomAttribute<System.ThreadStaticAttribute>() != null );
                                    if ( !isThreadStatic )
                                    {

                                        string fieldOrPropertyName = fieldInfo.Name;
                                        Regex regexBackingField = new Regex( @"\<(.*)\>k__BackingField" );
                                        var match = regexBackingField.Match( fieldInfo.Name );

                                        // if the field appears to be a backing field, we can take a guess at what the associated property is
                                        // then report that as not-threadsafe
                                        if ( match.Groups.Count == 2 )
                                        {
                                            var propertyName = match.Groups[1].Value;
                                            if ( memberList.Any( a => a.Name == propertyName ) )
                                            {
                                                fieldOrPropertyName = propertyName;
                                            }
                                        }

                                        string fullyQualifiedFieldName = $"{type.FullName}.{fieldOrPropertyName}";
                                        if ( !ignoredThreadSafeFieldWarning.Contains( fullyQualifiedFieldName ) )
                                        {
                                            singletonClassVariablesWarnings.AppendLine( $" - {fullyQualifiedFieldName}" );
                                        }
                                    }
                                }
                            }
                        }


                    }
                }
            }

            StringBuilder warnings = new StringBuilder();
            if ( singletonClassVariablesWarnings.Length > 0 )
            {
                warnings.AppendLine( "Singleton non-threadsafe class variables." );
                warnings.Append( singletonClassVariablesWarnings );
            }

            if ( missingDbSetWarnings.Length > 0 )
            {
                warnings.AppendLine();
                warnings.AppendLine( "RockContext missing DbSet<T>s" );
                warnings.Append( missingDbSetWarnings );
            }

            if ( rockObsoleteWarnings.Length > 0 )
            {
                warnings.AppendLine();
                warnings.AppendLine( "[Obsolete] that does't have [RockObsolete]" );
                warnings.Append( rockObsoleteWarnings );
            }

            if ( cbGenerateObsoleteExport.Checked )
            {
                warnings.AppendLine();

                obsoleteList = obsoleteList.OrderBy( a => a.Split( new char[] { ',' } )[0] ).ToList();
                warnings.Append( $"Version,Name,Type,IsError" + Environment.NewLine + obsoleteList.AsDelimited( Environment.NewLine ) );
            }

            tbResults.Text = warnings.ToString();
        }

        /// <summary>
        /// Writes the database procs scripts.
        /// </summary>
        /// <param name="databaseRootFolder">The database root folder.</param>
        /// <param name="projectName">Name of the project.</param>
        public void WriteDatabaseProcsScripts( string databaseRootFolder, string projectName )
        {
            // ignore any diagramming procs that might have been added by SMSS
            string[] procsToIgnore = {
                "fn_diagramobjects",
                "sp_alterdiagram",
                "sp_creatediagram",
                "sp_dropdiagram",
                "sp_helpdiagramdefinition",
                "sp_helpdiagrams",
                "sp_renamediagram",
                "sp_upgraddiagrams" };

            SqlConnection sqlconn = GetSqlConnection( new DirectoryInfo( databaseRootFolder ).Parent.FullName );
            sqlconn.Open();
            var qryProcs = sqlconn.CreateCommand();
            qryProcs.CommandType = System.Data.CommandType.Text;
            qryProcs.CommandText = "select ROUTINE_SCHEMA, ROUTINE_NAME, ROUTINE_TYPE FROM INFORMATION_SCHEMA.ROUTINES";
            var readerProcs = qryProcs.ExecuteReader();
            string procPrefixFilter;
            if ( projectName == "Rock" )
            {
                procPrefixFilter = string.Empty;
            }
            else
            {
                procPrefixFilter = "_" + Path.GetFileNameWithoutExtension( projectName ).Replace( ".", "_" );
            }

            while ( readerProcs.Read() )
            {
                string routineSchema = readerProcs["ROUTINE_SCHEMA"] as string;
                string routineName = readerProcs["ROUTINE_NAME"] as string;
                string routineType = readerProcs["ROUTINE_TYPE"] as string;
                var helpTextCommand = sqlconn.CreateCommand();
                helpTextCommand.CommandText = string.Format( "EXEC sp_helptext '{0}.{1}';", routineSchema, routineName );
                var helpTextReader = helpTextCommand.ExecuteReader();
                var script = string.Empty;
                while ( helpTextReader.Read() )
                {
                    script += helpTextReader[0];
                }

                string folder;
                if ( routineType == "PROCEDURE" )
                {
                    folder = "Procedures";
                }
                else
                {
                    folder = "Functions";
                }

                string filePath = Path.Combine( databaseRootFolder, folder, routineName + ".sql" );
                Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );

                string existingScript = string.Empty;
                if ( File.Exists( filePath ) )
                {
                    existingScript = File.ReadAllText( filePath );
                }

                if ( routineType == "PROCEDURE" )
                {
                    if ( !existingScript.StartsWith( "IF EXISTS (" ) )
                    {
                        script = Regex.Replace( script, "(^\\s*)CREATE\\s*PROCEDURE", "$1ALTER PROCEDURE", RegexOptions.IgnoreCase | RegexOptions.Multiline );
                    }
                    else
                    {
                        string dropIfExistsScript = $@"IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[{routineSchema}].[{routineName}]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [{routineSchema}].{routineName}
GO

";
                        script = dropIfExistsScript + script;
                    }
                }
                else
                {
                    script = Regex.Replace( script, "(^\\s*)CREATE\\s*FUNCTION", "$1ALTER FUNCTION", RegexOptions.IgnoreCase | RegexOptions.Multiline );
                }

                if ( string.IsNullOrEmpty( procPrefixFilter ) || routineName.StartsWith( procPrefixFilter, StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( !procsToIgnore.Contains( routineName ) )
                    {
                        File.WriteAllText( filePath, script.Trim() );
                    }
                }
            }

            var qryViews = sqlconn.CreateCommand();
            qryViews.CommandText = "SELECT TABLE_NAME, VIEW_DEFINITION FROM INFORMATION_SCHEMA.VIEWS";
            var readerViews = qryViews.ExecuteReader();
            while ( readerViews.Read() )
            {
                string viewName = readerViews["TABLE_NAME"] as string;
                string script = readerViews["VIEW_DEFINITION"] as string;

                string filePath = Path.Combine( databaseRootFolder, "Views", viewName + ".sql" );
                Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );

                string existingScript = string.Empty;
                if ( File.Exists( filePath ) )
                {
                    existingScript = File.ReadAllText( filePath );
                }

                if ( !existingScript.StartsWith( "IF OBJECT_ID(" ) )
                {
                    script = Regex.Replace( script, "(^\\s*)CREATE\\s*VIEW", "$1ALTER VIEW", RegexOptions.IgnoreCase | RegexOptions.Multiline );
                }
                else
                {
                    string dropIfExistsScript = $@"IF OBJECT_ID(N'[dbo].[{viewName}]', 'V') IS NOT NULL
    DROP VIEW {viewName}
GO

";
                    script = dropIfExistsScript + script;
                }

                if ( string.IsNullOrEmpty( procPrefixFilter ) || viewName.StartsWith( procPrefixFilter, StringComparison.OrdinalIgnoreCase ) )
                {
                    File.WriteAllText( filePath, script.Trim() );
                }
            }
        }

        /// <summary>
        /// Checks or unchecks all the items in the list
        /// </summary>
        /// <param name="selected"></param>
        private void CheckAllItems( bool selected )
        {
            for ( int i = 0; i < cblModels.Items.Count; i++ )
            {
                cblModels.SetItemChecked( i, selected );
            }
        }

        /// <summary>
        /// Writes the Service file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteServiceFile( string rootFolder, Type type )
        {
            string dbContextFullName = Rock.Reflection.GetDbContextForEntityType( type ).GetType().FullName;
            if ( dbContextFullName.StartsWith( "Rock.Data." ) )
            {
                dbContextFullName = dbContextFullName.Replace( "Rock.Data.", "" );
            }

            var properties = GetEntityProperties( type, false, true );

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Rock Community License (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.rockrms.com/license" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );

            sb.AppendLine( "using System;" );
            sb.AppendLine( "using System.Linq;" );
            sb.AppendLine( "" );
            sb.AppendLine( "using Rock.Data;" );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace {0}" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "{" );
            sb.AppendLine( "    /// <summary>" );
            sb.AppendFormat( "    /// {0} Service class" + Environment.NewLine, type.Name );
            sb.AppendLine( "    /// </summary>" );
            sb.AppendFormat( "    public partial class {0}Service : Service<{0}>" + Environment.NewLine, type.Name );
            sb.AppendLine( "    {" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendFormat( "        /// Initializes a new instance of the <see cref=\"{0}Service\"/> class" + Environment.NewLine, type.Name );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( "        /// <param name=\"context\">The context.</param>" );

            sb.AppendFormat( "        public {0}Service({1} context) : base(context)" + Environment.NewLine, type.Name, dbContextFullName );
            sb.AppendLine( "        {" );
            sb.AppendLine( "        }" );

            sb.Append( GetCanDeleteCode( rootFolder, type ) );

            sb.AppendLine( "    }" );

            sb.AppendFormat( @"
    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class {0}ExtensionMethods
    {{
        /// <summary>
        /// Clones this {0} object to a new {0} object
        /// </summary>
        /// <param name=""source"">The source.</param>
        /// <param name=""deepCopy"">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static {0} Clone( this {0} source, bool deepCopy )
        {{
            if (deepCopy)
            {{
                return source.Clone() as {0};
            }}
            else
            {{
                var target = new {0}();
                target.CopyPropertiesFrom( source );
                return target;
            }}
        }}
", type.Name );

            sb.AppendFormat( @"
        /// <summary>
        /// Copies the properties from another {0} object to this {0} object
        /// </summary>
        /// <param name=""target"">The target.</param>
        /// <param name=""source"">The source.</param>
        public static void CopyPropertiesFrom( this {0} target, {0} source )
        {{
", type.Name );

            foreach ( var property in properties )
            {
                PropertyInfo propertyInfo = property.Value;
                var obsolete = propertyInfo.GetCustomAttribute<ObsoleteAttribute>();

                // wrap with a pragma to disable the obsolete warning (since we do want to copy obsolete values when cloning, unless this is obsolete.IsError )
                if ( obsolete != null )
                {
                    if ( obsolete.IsError == false )
                    {
                        sb.AppendLine( $"            #pragma warning disable 612, 618" );
                        sb.AppendLine( $"            target.{property.Key} = source.{property.Key};" );
                        sb.AppendLine( $"            #pragma warning restore 612, 618" );
                    }
                }
                else
                {
                    sb.AppendLine( $"            target.{property.Key} = source.{property.Key};" );
                }
            }

            sb.Append( @"
        }
    }
" );
            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, type.Namespace ).FullName, "CodeGenerated", type.Name + "Service.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// 
        /// </summary>
        private class TableColumnInfo
        {
            public string Table { get; set; }
            public string Column { get; set; }
            public bool IsPartOfPrimaryKey { get; set; }
            public bool Ignore { get; set; }
            public bool HasEntityModel { get; set; }
            public override string ToString()
            {
                return string.Format( "{0} | {1} {2} {3} {4}", Table, Column, IsPartOfPrimaryKey ? "| PrimaryKey" : null, Ignore ? "| Ignored" : null, HasEntityModel ? null : " | No Entity Model" );
            }
        }

        /// <summary>
        /// Gets the can delete code.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string GetCanDeleteCode( string serviceFolder, Type type )
        {

            SqlConnection sqlconn = GetSqlConnection( new DirectoryInfo( serviceFolder ).Parent.FullName );
            if ( sqlconn == null )
            {
                return string.Empty;
            }

            sqlconn.Open();

            SqlCommand sqlCommand = sqlconn.CreateCommand();
            TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if ( tableAttribute == null )
            {
                // not a real table
                return string.Empty;
            }

            string sql = $"exec sp_fkeys @pktable_name = '{tableAttribute.Name}', @pktable_owner = 'dbo'";
            sqlCommand.CommandText = sql;
            sqlCommand.Parameters.Add( new SqlParameter( "@refTable", tableAttribute.Name ) );
            var reader = sqlCommand.ExecuteReader();

            List<TableColumnInfo> parentTableColumnNameList = new List<TableColumnInfo>();
            while ( reader.Read() )
            {
                string parentTable = reader["FKTABLE_NAME"] as string;
                string columnName = reader["FKCOLUMN_NAME"] as string;
                bool isCascadeDelete = reader["DELETE_RULE"] as short? == 0;
                
                bool ignoreCanDelete = false;
                bool hasEntityModel = true;

                if ( isCascadeDelete )
                {
                    continue;
                }

                bool isPrimaryKey = false;
                Type parentEntityType = Type.GetType( string.Format( "Rock.Model.{0}, {1}", parentTable, type.Assembly.FullName ) );
                if ( parentEntityType != null )
                {
                    PropertyInfo columnProp = parentEntityType.GetProperty( columnName );
                    if ( columnProp != null )
                    {
                        if ( columnProp.GetCustomAttribute<Rock.Data.IgnoreCanDelete>() != null )
                        {
                            ignoreCanDelete = true;
                        }

                        isPrimaryKey = columnProp.GetCustomAttribute<KeyAttribute>() != null;
                    }
                }
                else
                {
                    hasEntityModel = false;
                }


                parentTableColumnNameList.Add( new TableColumnInfo { Table = parentTable, Column = columnName, IsPartOfPrimaryKey = isPrimaryKey, Ignore = ignoreCanDelete, HasEntityModel = hasEntityModel } );
            }

            parentTableColumnNameList = parentTableColumnNameList.OrderBy( a => a.Table ).ThenBy( a => a.Column ).ToList();

            string canDeleteBegin = string.Format( @"
        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name=""item"">The item.</param>
        /// <param name=""errorMessage"">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( {0} item, out string errorMessage )
        {{
            errorMessage = string.Empty;
", type.Name );

            string canDeleteMiddle = string.Empty;

            foreach ( var item in parentTableColumnNameList )
            {
                // Ignore custom tables
                if ( item.Table.StartsWith( "_" ) )
                {
                    continue;
                }

                // detect associative table where the foreign key column is also part of the primary key.  EF will automatically take care of it on the DELETE
                if ( item.IsPartOfPrimaryKey || item.Ignore )
                {
                    canDeleteMiddle += string.Format(
        @"            
            // ignoring {0},{1} 
", item.Table, item.Column );
                    continue;
                }

                if ( !item.HasEntityModel )
                {
                    // if the table is in the database, but isn't a Rock Entity, skip it
                    canDeleteMiddle += "";
                    continue;
                }

                string parentTable = item.Table;
                string columnName = item.Column;
                string relationShipText;
                string pluralizeCode;

                if ( columnName.StartsWith( "Parent" + type.Name ) )
                {
                    relationShipText = "contains one or more child";
                    pluralizeCode = ".Pluralize().ToLower()";
                }
                else
                {
                    relationShipText = "is assigned to a";
                    pluralizeCode = "";
                }

                // #pragma warning disable 612, 618
                var entityTypes = cblModels.Items.Cast<Type>().ToList();

                var parentTableType = entityTypes.Where( a => a.GetCustomAttribute<TableAttribute>()?.Name == parentTable || a.Name == parentTable ).FirstOrDefault();
                var obsolete = parentTableType?.GetCustomAttribute<ObsoleteAttribute>();

                if ( obsolete != null && obsolete.IsError == false )
                {
                    canDeleteMiddle += $@"
            #pragma warning disable 612, 618 // {parentTableType.Name} is obsolete, but we still need this code generated";
                }

                canDeleteMiddle +=
        $@" 
            if ( new Service<{parentTable}>( Context ).Queryable().Any( a => a.{columnName} == item.Id ) )
            {{
                errorMessage = string.Format( ""This {{0}} {relationShipText} {{1}}."", {type.Name}.FriendlyTypeName, {parentTable}.FriendlyTypeName{pluralizeCode} );
                return false;
            }}  
";

                if ( obsolete != null && obsolete.IsError == false )
                {
                    canDeleteMiddle += @"            #pragma warning restore 612, 618
";
                }
            }


            string canDeleteEnd = @"            return true;
        }
";


            return canDeleteBegin + canDeleteMiddle + canDeleteEnd;

        }

        private static SqlConnection GetSqlConnection( string rootFolder )
        {
            var file = new FileInfo( Path.Combine( rootFolder, @"RockWeb\web.ConnectionStrings.config" ) );
            if ( !file.Exists )
            {
                return null;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load( file.FullName );
            XmlNode root = xmlDoc.DocumentElement;
            XmlNode node = root.SelectNodes( "add[@name = \"RockContext\"]" )[0];
            SqlConnection sqlconn = new SqlConnection( node.Attributes["connectionString"].Value );
            return sqlconn;
        }

        /// <summary>
        /// Writes the REST file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteRESTFile( string rootFolder, Type type )
        {
            string pluralizedName = type.Name.Pluralize();
            string restNamespace = type.Assembly.GetName().Name + ".Rest.Controllers";
            string dbContextFullName = Rock.Reflection.GetDbContextForEntityType( type ).GetType().FullName;

            var obsolete = type.GetCustomAttribute<ObsoleteAttribute>();
            var rockObsolete = type.GetCustomAttribute<RockObsolete>();

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Rock Community License (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.rockrms.com/license" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );
            sb.AppendLine( "" );

            sb.AppendLine( $"using {type.Namespace};" );
            sb.AppendLine( "" );

            sb.AppendLine( $"namespace {restNamespace}" );
            sb.AppendLine( "{" );
            sb.AppendLine( "    /// <summary>" );
            sb.AppendLine( $"    /// {pluralizedName} REST API" );
            sb.AppendLine( "    /// </summary>" );

            if ( obsolete != null && obsolete.IsError == false )
            {
                if ( rockObsolete != null )
                {
                    sb.AppendLine( $"    [RockObsolete( \"{rockObsolete.Version}\" )]" );
                }

                sb.AppendLine( $"    [System.Obsolete( \"{obsolete.Message}\" )]" );
            }

            sb.AppendLine( $"    public partial class {pluralizedName}Controller : Rock.Rest.ApiController<{type.Namespace}.{type.Name}>" );
            sb.AppendLine( "    {" );
            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( $"        /// Initializes a new instance of the <see cref=\"{ pluralizedName}Controller\"/> class." );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( $"        public {pluralizedName}Controller() : base( new {type.Namespace}.{type.Name}Service( new {dbContextFullName}() ) ) {{ }} " );
            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            var filePath1 = Path.Combine( rootFolder, "Controllers" );
            var file = new FileInfo( Path.Combine( filePath1, "CodeGenerated", pluralizedName + "Controller.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the Service file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        /// <summary>
        /// Gets the root folder to use as the base for the namespace folders
        /// </summary>
        /// <returns></returns>
        private DirectoryInfo RootFolder()
        {
            // Should probably be read from config file, or selected from directory dialog.  
            // For now, just traverses parent folders looking for Rock.sln file
            var dirInfo = new DirectoryInfo( Path.GetDirectoryName( lblAssemblyPath.Text ) );
            while ( dirInfo != null && !dirInfo.GetDirectories().Any( a => a.Name == "RockWeb" ) )
            {
                dirInfo = dirInfo.Parent;
            }
            return dirInfo;
        }

        /// <summary>
        /// Gets the namespace folder for a selected type
        /// </summary>
        private DirectoryInfo NamespaceFolder( string rootFolder, string objectNamespace )
        {
            DirectoryInfo di = new DirectoryInfo( rootFolder );

            var commonParts = new List<string>();
            while ( objectNamespace.ToLower().StartsWith( di.Name.ToLower() + "." ) )
            {
                commonParts.Add( di.Name );
                objectNamespace = objectNamespace.Substring( di.Name.Length + 1 );
                di = di.Parent;
            }

            rootFolder = di.FullName;
            foreach ( string part in commonParts )
            {
                rootFolder = Path.Combine( rootFolder, part );
            }

            return new DirectoryInfo( Path.Combine( rootFolder, objectNamespace.Replace( '.', '\\' ) ) );
        }

        /// <summary>
        /// Get the property type name.  Will handle nullable and complex generic types.
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        private string PropertyTypeName( Type propertyType )
        {
            if ( propertyType.IsGenericType )
            {
                if ( propertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    return GetKeyName( propertyType.GetGenericArguments()[0] ) + "?";
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat( "{0}<", propertyType.Name.Split( ( char ) 96 )[0] );

                    foreach ( var argType in propertyType.GetGenericArguments() )
                    {
                        sb.AppendFormat( "{0}, ", PropertyTypeName( argType ) );
                    }

                    sb.Remove( sb.Length - 2, 2 );
                    sb.Append( ">" );

                    return sb.ToString();
                }
            }
            else
            {
                return GetKeyName( propertyType );
            }
        }

        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="sb">The sb.</param>
        private void WriteFile( FileInfo file, StringBuilder sb )
        {
            if ( !file.Directory.Exists )
            {
                file.Directory.Create();
            }

            using ( var outputFile = new StreamWriter( file.FullName ) )
            {
                outputFile.Write( sb.ToString() );
            }
        }

        /// <summary>
        /// Replace any type names that have an equivelant C# alias
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private string GetKeyName( string typeName )
        {
            switch ( typeName )
            {
                case "Boolean":
                    return "bool";
                case "Byte":
                    return "byte";
                case "Char":
                    return "char";
                case "Decimal":
                    return "decimal";
                case "Double":
                    return "double";
                case "Single":
                    return "float";
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";
                case "SByte":
                    return "sbyte";
                case "Int16":
                    return "short";
                case "String":
                    return "string";
                case "DbGeography":
                    return "object";
            }

            return typeName;
        }

        private string GetKeyName( Type type )
        {
            if ( type.IsEnum )
            {
                if ( type.Namespace == "Rock.Model" )
                {
                    return "Rock.Client.Enums." + type.Name;
                }
                else
                {
                    return GetKeyName( "Int32" ) + " /* " + type.Name + "*/";
                }
            }
            else
            {
                return GetKeyName( type.Name );
            }
        }

        /// <summary>
        /// Gets the entity properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="includeRockClientIncludes">if set to <c>true</c> [include rock client includes].</param>
        /// <param name="includeObsolete">if set to <c>true</c> [include obsolete].</param>
        /// <returns></returns>
        private Dictionary<string, PropertyInfo> GetEntityProperties( Type type, bool includeRockClientIncludes, bool includeObsolete )
        {
            var properties = new Dictionary<string, PropertyInfo>();

            var interfaces = type.GetInterfaces();

            foreach ( var property in type.GetProperties().SortByStandardOrder() )
            {
                bool include = false;
                if ( includeRockClientIncludes && property.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>() != null )
                {
                    include = true;
                }

                var getMethod = property.GetGetMethod();
                if ( getMethod == null )
                {
                    continue;
                }

                if ( getMethod.IsVirtual )
                {
                    if ( !include && !getMethod.IsFinal )
                    {
                        continue;
                    }

                    bool interfaceProperty = false;
                    foreach ( Type interfaceType in interfaces )
                    {
                        if ( interfaceType.GetProperties().Any( p => p.Name == property.Name ) )
                        {
                            interfaceProperty = true;
                            break;
                        }
                    }
                    if ( !include && !interfaceProperty )
                    {
                        continue;
                    }
                }

                if ( !property.GetCustomAttributes( typeof( DatabaseGeneratedAttribute ) ).Any() )
                {
                    if ( ( property.GetCustomAttribute<ObsoleteAttribute>() == null || includeObsolete ) )
                    {
                        if ( property.SetMethod != null && property.SetMethod.IsPublic && property.GetMethod.IsPublic )
                        {
                            properties.Add( property.Name, property );
                        }
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// Writes the rock client model enums file.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        private void WriteRockClientEnumsFile( string rootFolder )
        {
            rockAssembly = typeof( Rock.Data.IEntity ).Assembly;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Rock Community License (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.rockrms.com/license" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );
            sb.AppendLine( "using System;" );
            sb.AppendLine( "using System.Collections.Generic;" );
            sb.AppendLine( "" );

            sb.AppendLine( "namespace Rock.Client.Enums" );
            sb.AppendLine( "{" );
            sb.AppendLine( "    #pragma warning disable CS1591" );

            foreach ( var enumType in rockAssembly.GetTypes().Where( a => a.IsEnum ).OrderBy( a => a.Name ) )
            {
                if ( enumType.Namespace == "Rock.Model" )
                {
                    sb.AppendLine( "    /// <summary>" );
                    sb.AppendLine( "    /// </summary>" );
                    if ( enumType.GetCustomAttribute<FlagsAttribute>() != null )
                    {
                        sb.AppendLine( "    [Flags]" );
                    }
                    sb.AppendFormat( "    public enum {0}" + Environment.NewLine, enumType.Name );
                    sb.AppendLine( "    {" );
                    var enumValues = Enum.GetValues( enumType );
                    foreach ( var enumValueName in Enum.GetNames( enumType ) )
                    {
                        // mark Obsolete Enum values
                        object value = Enum.Parse( enumType, enumValueName );
                        var fieldInfo = value.GetType().GetField( enumValueName );
                        var obsolete = fieldInfo?.GetCustomAttribute<ObsoleteAttribute>();

                        if ( obsolete != null )
                        {
                            sb.AppendLine();
                            sb.AppendLine( $"        [Obsolete( \"{obsolete.Message}\", {obsolete.IsError.ToTrueFalse().ToLower()} )]" );
                        }

                        int enumValue = ( int ) Convert.ChangeType( Enum.Parse( enumType, enumValueName ), typeof( int ) );
                        string enumValueParam = enumValue >= 0 ? " = 0x" + enumValue.ToString( "x" ) : " = " + enumValue.ToString();
                        sb.AppendFormat( "        {0}{1},", enumValueName, enumValueParam );
                        sb.AppendLine( "" );
                    }

                    sb.AppendLine( "    }" );
                    sb.AppendLine( "" );
                }
            }

            sb.AppendLine( "    #pragma warning restore CS1591" );
            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( rootFolder, "CodeGenerated\\Enums", "RockEnums.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the rock client system unique identifier files.      /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        private void WriteRockClientSystemGuidFiles( string rootFolder )
        {
            rockAssembly = typeof( Rock.Data.IEntity ).Assembly;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Rock Community License (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.rockrms.com/license" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );
            sb.AppendLine( "using System;" );
            sb.AppendLine( "using System.Collections.Generic;" );
            sb.AppendLine( "" );

            sb.AppendLine( "namespace Rock.Client.SystemGuid" );
            sb.AppendLine( "{" );
            sb.AppendLine( "    #pragma warning disable CS1591" );

            foreach ( var systemGuidType in rockAssembly.GetTypes().Where( a => a.Namespace == "Rock.SystemGuid" ).OrderBy( a => a.Name ) )
            {
                sb.AppendLine( "    /// <summary>" );
                sb.AppendLine( "    /// </summary>" );
                sb.AppendFormat( "    public class {0}" + Environment.NewLine, systemGuidType.Name );
                sb.AppendLine( "    {" );
                foreach ( var field in systemGuidType.GetFields().OrderBy( a => a.Name ) )
                {
                    sb.AppendFormat( "        public const string {0} = \"{1}\";", field.Name, field.GetValue( systemGuidType ) );
                    sb.AppendLine( "" );
                }

                sb.AppendLine( "    }" );
                sb.AppendLine( "" );
            }

            sb.AppendLine( "    #pragma warning restore CS1591" );
            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( rootFolder, "CodeGenerated\\SystemGuid", "RockSystemGuids.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the rock client include client files.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="alreadyIncludedTypes">The already included types.</param>
        private void WriteRockClientIncludeClientFiles( string rootFolder, IEnumerable<Type> alreadyIncludedTypes )
        {
            foreach ( var rockClientIncludeType in rockAssembly.GetTypes().Where( a => a.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>() != null ).OrderBy( a => a.Name ) )
            {
                if ( !alreadyIncludedTypes.Any( a => a == rockClientIncludeType ) )
                {
                    WriteRockClientFile( rootFolder, rockClientIncludeType );
                }
            }
        }

        /// <summary>
        /// Writes the DTO file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteRockClientFile( string rootFolder, Type type )
        {
            // make a copy of the EntityProperties since we are deleting some for this method
            var entityProperties = GetEntityProperties( type, true, true ).ToDictionary( k => k.Key, v => v.Value );

            var dataMembers = type.GetProperties().SortByStandardOrder()
                .Where( a => a.GetCustomAttribute<DataMemberAttribute>() != null )
                .Where( a => a.GetCustomAttribute<ObsoleteAttribute>() == null )
                .Where( a => ( a.GetCustomAttribute<NotMappedAttribute>() == null || a.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>() != null ) )
                .Where( a => !entityProperties.Keys.Contains( a.Name ) );

            var rockClientIncludeAttribute = type.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>();
            string comments = null;

            if ( rockClientIncludeAttribute != null )
            {
                comments = rockClientIncludeAttribute.DocumentationMessage;
            }

            if ( !entityProperties.Any() && !dataMembers.Any() )
            {
                return;
            }

            string lcName = type.Name.Substring( 0, 1 ).ToLower() + type.Name.Substring( 1 );

            bool isSecured = typeof( Rock.Security.ISecured ).IsAssignableFrom( type );

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Rock Community License (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.rockrms.com/license" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );
            sb.AppendLine( "using System;" );
            sb.AppendLine( "using System.Collections.Generic;" );
            sb.AppendLine( "" );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace Rock.Client" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "{" );

            sb.AppendLine( "    /// <summary>" );


            if ( !string.IsNullOrWhiteSpace( comments ) )
            {
                sb.AppendFormat( "    /// {0}" + Environment.NewLine, comments );
            }
            else
            {
                sb.AppendFormat( "    /// Base client model for {0} that only includes the non-virtual fields. Use this for PUT/POSTs" + Environment.NewLine, type.Name );
            }

            sb.AppendLine( "    /// </summary>" );

            sb.AppendFormat( "    public partial class {0}Entity" + Environment.NewLine, type.Name );
            sb.AppendLine( "    {" );

            foreach ( var keyVal in entityProperties )
            {
                var propertyName = keyVal.Key;
                var propertyInfo = keyVal.Value;
                ObsoleteAttribute obsolete = propertyInfo.GetCustomAttribute<ObsoleteAttribute>();
                RockObsolete rockObsolete = propertyInfo.GetCustomAttribute<RockObsolete>();
                var propertyRockClientIncludeAttribute = propertyInfo.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>();
                string propertyComments = null;

                if ( propertyRockClientIncludeAttribute != null )
                {
                    propertyComments = propertyRockClientIncludeAttribute.DocumentationMessage;
                }

                if ( !string.IsNullOrWhiteSpace( propertyComments ) )
                {
                    sb.AppendLine( "        /// <summary>" );
                    sb.AppendFormat( "        /// {0}" + Environment.NewLine, propertyComments );
                    sb.AppendLine( "        /// </summary>" );
                }
                else
                {
                    sb.AppendLine( "        /// <summary />" );
                }

                if ( obsolete != null )
                {
                    if ( rockObsolete != null )
                    {
                        // [RockObsolete( "1.9" )]
                        sb.AppendLine( $"        // Made Obsolete in Rock \"{rockObsolete.Version}\"" );
                    }

                    //[Obsolete( "Use PreventInactivePeople instead.", true )]
                    sb.AppendLine( $"        [Obsolete( \"{obsolete.Message}\", {obsolete.IsError.ToTrueFalse().ToLower()} )]" );
                }

                // if the property has auto-property ( ex: IsActive {get; set) = true;) lets put the same thing on the code generated rock.client class
                Type[] autoPropertyTypesToCheck = new Type[] { typeof( string ), typeof( bool ), typeof( int ), typeof( bool? ), typeof( int? ) };
                object autoPropertyValue = null;
                if ( autoPropertyTypesToCheck.Contains( propertyInfo.PropertyType ) || propertyInfo.PropertyType.IsEnum )
                {
                    // create an instance of the type to detect any auto-properties that have a default value set
                    object typeInstance = null;
                    if ( type.GetConstructor( new Type[0] ) != null )
                    {
                        typeInstance = Activator.CreateInstance( type );
                    }

                    if ( typeInstance != null )
                    {

                        // we can rule out the existence of the autoProperty if the getter doesn't have CompilerGeneratedAttribute
                        //if ( propertyInfo.GetGetMethod().GetCustomAttribute<CompilerGeneratedAttribute>() != null )
                        {
                            autoPropertyValue = propertyInfo.GetValue( typeInstance );
                        }
                    }
                }

                sb.Append( $"        public {this.PropertyTypeName( propertyInfo.PropertyType )} {propertyName} {{ get; set; }}" );

                if ( autoPropertyValue != null )
                {
                    string defaultValueCode = null;
                    if ( autoPropertyValue is string )
                    {
                        var escapedDefaultValue = ( autoPropertyValue as string ).Replace( "\"", "\"\"" );
                        defaultValueCode = $"@\"{ escapedDefaultValue}\"";
                    }
                    else if ( autoPropertyValue is bool )
                    {
                        if ( ( bool ) autoPropertyValue != false )
                        {
                            defaultValueCode = ( bool ) autoPropertyValue ? "true" : "false";
                        }
                    }
                    else if ( autoPropertyValue is int )
                    {
                        if ( ( int ) autoPropertyValue != 0 )
                        {
                            defaultValueCode = autoPropertyValue.ToString();
                        }
                    }
                    else if ( autoPropertyValue.GetType().IsEnum )
                    {
                        if ( ( int ) autoPropertyValue != 0 )
                        {
                            defaultValueCode = $"Rock.Client.Enums.{autoPropertyValue.GetType().Name}.{autoPropertyValue}";
                        }
                    }

                    if ( defaultValueCode != null )
                    {
                        sb.Append( $" = {defaultValueCode};" );
                    }
                }

                sb.AppendLine( "" );

                sb.AppendLine( "" );
            }

            sb.AppendFormat(
        @"        /// <summary>
        /// Copies the base properties from a source {0} object
        /// </summary>
        /// <param name=""source"">The source.</param>
        public void CopyPropertiesFrom( {0} source )
        {{
", type.Name );

            foreach ( var keyVal in entityProperties )
            {

                var obsolete = keyVal.Value.GetCustomAttribute<ObsoleteAttribute>();

                // wrap with a pragma to disable the obsolete warning (since we do want to copy obsolete values when cloning, unless this is obsolete.IsError )
                if ( obsolete != null )
                {
                    if ( obsolete.IsError == false )
                    {
                        sb.AppendLine( $"            #pragma warning disable 612, 618" );
                        sb.AppendLine( $"            this.{keyVal.Key} = source.{keyVal.Key};" );
                        sb.AppendLine( $"            #pragma warning restore 612, 618" );
                    }
                }
                else
                {
                    sb.AppendLine( $"            this.{keyVal.Key} = source.{keyVal.Key};" );
                }
            }

            sb.Append( @"
        }
" );

            sb.AppendLine( "    }" );

            sb.AppendLine( "" );

            sb.AppendLine( "    /// <summary>" );

            if ( !string.IsNullOrWhiteSpace( comments ) )
            {
                sb.AppendFormat( "    /// {0}" + Environment.NewLine, comments );
            }
            else
            {
                sb.AppendFormat( "    /// Client model for {0} that includes all the fields that are available for GETs. Use this for GETs (use {0}Entity for POST/PUTs)" + Environment.NewLine, type.Name );
            }

            sb.AppendLine( "    /// </summary>" );

            sb.AppendFormat( "    public partial class {0} : {0}Entity" + Environment.NewLine, type.Name );
            sb.AppendLine( "    {" );

            foreach ( var dataMember in dataMembers )
            {
                var dataMemberRockClientIncludeAttribute = dataMember.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>();
                string dataMemberComments = null;

                if ( dataMemberRockClientIncludeAttribute != null )
                {
                    dataMemberComments = dataMemberRockClientIncludeAttribute.DocumentationMessage;
                }

                if ( !string.IsNullOrWhiteSpace( dataMemberComments ) )
                {
                    sb.AppendLine( "        /// <summary>" );
                    sb.AppendFormat( "        /// {0}" + Environment.NewLine, dataMemberComments );
                    sb.AppendLine( "        /// </summary>" );
                }
                else
                {
                    sb.AppendLine( "        /// <summary />" );
                }

                sb.AppendFormat( "        public {0} {1} {{ get; set; }}" + Environment.NewLine, PropertyTypeName( dataMember.PropertyType ), dataMember.Name );
                sb.AppendLine( "" );
            }

            // if this is a IHasAttributes type, generate Attribute/AttributeValues since they can be fetched thru REST when ?loadAttributes is specified
            if ( typeof( Rock.Attribute.IHasAttributes ).IsAssignableFrom( type ) )
            {
                sb.AppendLine( "        /// <summary>" );
                sb.AppendLine( "        /// NOTE: Attributes are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' " );
                sb.AppendLine( "        /// </summary>" );
                sb.AppendLine( "        public Dictionary<string, Rock.Client.Attribute> Attributes { get; set; }" );
                sb.AppendLine( "" );
                sb.AppendLine( "        /// <summary>" );
                sb.AppendLine( "        /// NOTE: AttributeValues are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' " );
                sb.AppendLine( "        /// </summary>" );
                sb.AppendLine( "        public Dictionary<string, Rock.Client.AttributeValue> AttributeValues { get; set; }" );
            }

            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( rootFolder, "CodeGenerated", type.Name + ".cs" ) );
            WriteFile( file, sb );
        }

        private void tbServiceFolder_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            fbdServiceOutput.SelectedPath = tbServiceFolder.Text;
            if ( fbdServiceOutput.ShowDialog() == DialogResult.OK )
            {
                tbServiceFolder.Text = fbdServiceOutput.SelectedPath;
            }
        }

        private void tbRestFolder_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            fbdRestOutput.SelectedPath = tbRestFolder.Text;
            if ( fbdRestOutput.ShowDialog() == DialogResult.OK )
            {
                tbRestFolder.Text = fbdRestOutput.SelectedPath;
            }
        }

        private void tbClientFolder_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            fdbRockClient.SelectedPath = tbClientFolder.Text;
            if ( fdbRockClient.ShowDialog() == DialogResult.OK )
            {
                tbClientFolder.Text = fdbRockClient.SelectedPath;
            }
        }

        /// <summary>
        /// The ignore files
        /// </summary>
        static string[] IgnoreFiles = new string[] { "\\DoubleMetaphone.cs", "\\Rock.Version\\AssemblySharedInfo.cs" };

        /// <summary>
        /// The ignore folders
        /// </summary>
        static string[] IgnoreFolders = new string[] { "\\CodeGenerated", "\\obj" };

        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        static void EnsureCopyrightHeaders( string rootFolder )
        {
            string rockDirectory = rootFolder.EnsureTrailingBackslash();

            int updatedFileCount = 0;
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "RockWeb\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Checkr\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.DownhillCss\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Mailgun\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Mandrill\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Migrations\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.NMI\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.PayFlowPro\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Rest\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Security.Authentication.Auth0\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.SignNow\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Slingshot\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Slingshot.Model\\" );
            //updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Specs\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.StatementGenerator\\" );
            //updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Tests\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.TransNational.Pi\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Version\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.WebStartup\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Applications\\" );

            Console.WriteLine( "\n\nDone!  Files Updated: {0}\n\nPress any key to continue.", updatedFileCount );
            Console.ReadLine();
        }

        /// <summary>
        /// Fixups the copyright headers.
        /// </summary>
        /// <param name="searchDirectory">The search directory.</param>
        private static int FixupCopyrightHeaders( string searchDirectory )
        {
            int result = 0;

            List<string> sourceFilenames = Directory.GetFiles( searchDirectory, "*.cs", SearchOption.AllDirectories ).ToList();

            // exclude files that come from the localhistory VS extension
            sourceFilenames = sourceFilenames.Where( a => !a.Contains( ".localhistory" ) ).ToList();

            // this was was our standard copyright badge up until 1/17/2014. Look for it in case it sneaks back in
            const string oldCopyrightBadge1 = @"// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//";

            // standard copyright badge 4/1/2016 to 5/22/2016
            const string oldCopyrightBadge2 = @"// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
";

            // standard copyright badge starting 5/23/2016
            const string newCopyrightBadgeStart = @"// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>";

            const string newCopyrightBadge = newCopyrightBadgeStart + @"
//
";
            foreach ( string fileName in sourceFilenames )
            {
                bool skipFile = false;
                foreach ( var f in IgnoreFolders )
                {
                    if ( fileName.Contains( f ) )
                    {
                        skipFile = true;
                    }

                }

                foreach ( var f in IgnoreFiles )
                {
                    if ( Path.GetFullPath( fileName ).EndsWith( f, StringComparison.OrdinalIgnoreCase ) )
                    {
                        skipFile = true;
                    }
                }

                if ( skipFile )
                {
                    continue;
                }

                string origFileContents = File.ReadAllText( fileName );

                if ( origFileContents.Contains( "<auto-generated" ) )
                {
                    continue;
                }

                if ( origFileContents.StartsWith( newCopyrightBadgeStart ) )
                {
                    continue;
                }

                // get rid of any incorrect header by finding keyword using or namespace
                int positionNamespace = origFileContents.IndexOf( "namespace ", 0 );
                int positionUsing = origFileContents.IndexOf( "using ", 0 );
                int codeStart = positionNamespace > positionUsing ? positionUsing : positionNamespace;
                codeStart = codeStart < 0 ? 0 : codeStart;

                string newFileContents = origFileContents.Substring( codeStart );

                // try to clean up cases where the badge is after some of the using statements
                newFileContents = newFileContents.Replace( oldCopyrightBadge1, string.Empty ).Replace( newCopyrightBadge, string.Empty );
                newFileContents = newFileContents.Replace( oldCopyrightBadge2, string.Empty ).Replace( newCopyrightBadge, string.Empty );

                newFileContents = newCopyrightBadge + newFileContents.TrimStart();

                if ( !origFileContents.Equals( newFileContents ) )
                {
                    Console.WriteLine( "Updating header in {0}", fileName );
                    result++;

                    System.Text.Encoding encoding;
                    using ( var r = new StreamReader( fileName, detectEncodingFromByteOrderMarks: true ) )
                    {
                        encoding = r.CurrentEncoding;
                    }

                    File.WriteAllText( fileName, newFileContents, encoding );
                }
            }
            return result;
        }


    }

    public static class HelperExtensions
    {
        public static PropertyInfo[] SortByStandardOrder( this PropertyInfo[] properties )
        {
            string[] baseModelPropertyTypeNames = new string[] { "Id", "CreatedDateTime", "ModifiedDateTime", "CreatedByPersonAliasId", "ModifiedByPersonAliasId", "Guid", "ForeignId" };
            var result = new List<PropertyInfo>();

            // Have Standard Order by "Id", <alphabetic list of other fields>, "CreatedDateTime", "ModifiedDateTime", "CreatedByPersonAliasId", "ModifiedByPersonAliasId", "Guid", "ForeignId"
            result.AddRange( properties.Where( a => !baseModelPropertyTypeNames.Contains( a.Name ) ).OrderBy( a => a.Name ) );

            foreach ( var name in baseModelPropertyTypeNames )
            {
                var property = properties.FirstOrDefault( a => a.Name == name );
                if ( property != null )
                {
                    if ( property.Name == "Id" )
                    {
                        result.Insert( 0, property );
                    }
                    else
                    {
                        result.Add( property );
                    }
                }
            }

            return result.ToArray();
        }
    }
}
