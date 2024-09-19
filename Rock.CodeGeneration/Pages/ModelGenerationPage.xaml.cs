using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Rock;
using Rock.CodeGeneration.Utility;
using Rock.CodeGeneration.XmlDoc;
using Rock.Utility;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Logic for the Model Generation page.
    /// </summary>
    public partial class ModelGenerationPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Fields

        private readonly List<CheckedItem<Type>> _modelItems = new List<CheckedItem<Type>>();

        private readonly System.Windows.Forms.FolderBrowserDialog _fbdOutput = new System.Windows.Forms.FolderBrowserDialog();

        private readonly XmlDocReader _xmlDoc = SupportTools.GetXmlDocReader();

        #endregion

        #region Properties

        public string DisplayedAssemblyPath
        {
            get => _displayedAssemblyPath;
            set
            {
                _displayedAssemblyPath = value;
                OnPropertyChanged();
            }
        }
        private string _displayedAssemblyPath;

        public string AssemblyPath
        {
            get => _assemblyPath;
            set
            {
                _assemblyPath = value;
                OnPropertyChanged();
            }
        }
        private string _assemblyPath;

        public string AssemblyDateTime
        {
            get => _assemblyDateTime;
            set
            {
                _assemblyDateTime = value;
                OnPropertyChanged();
            }
        }
        private string _assemblyDateTime;

        public string AssemblyDateTimeElapsed
        {
            get => _assemblyDateTimeElapsed;
            set
            {
                _assemblyDateTimeElapsed = value;
                OnPropertyChanged();
            }
        }
        private string _assemblyDateTimeElapsed;

        public string DatabaseConnectionString
        {
            get => _databaseConnectionString;
            set
            {
                _databaseConnectionString = value;
                OnPropertyChanged();
            }
        }
        private string _databaseConnectionString;

        public string ServiceFolder
        {
            get => _serviceFolder;
            set
            {
                _serviceFolder = value;
                OnPropertyChanged();
            }
        }
        private string _serviceFolder;

        public string RestFolder
        {
            get => _restFolder;
            set
            {
                _restFolder = value;
                OnPropertyChanged();
            }
        }
        private string _restFolder;

        public string DatabaseFolder
        {
            get => _databaseFolder;
            set
            {
                _databaseFolder = value;
                OnPropertyChanged();
            }
        }
        private string _databaseFolder;

        public bool IsServiceChecked
        {
            get => _isServiceChecked;
            set
            {
                _isServiceChecked = value;
                OnPropertyChanged();
            }
        }
        private bool _isServiceChecked = true;

        public bool IsRestChecked
        {
            get => _isRestChecked;
            set
            {
                _isRestChecked = value;
                OnPropertyChanged();
            }
        }
        private bool _isRestChecked = true;

        public bool IsDatabaseProcsChecked
        {
            get => _isDatabaseProcsChecked;
            set
            {
                _isDatabaseProcsChecked = value;
                OnPropertyChanged();
            }
        }
        private bool _isDatabaseProcsChecked = true;

        public bool IsRockGuidsChecked
        {
            get => _isRockGuidsChecked;
            set
            {
                _isRockGuidsChecked = value;
                OnPropertyChanged();
            }
        }
        private bool _isRockGuidsChecked = true;

        public bool IsReportObsoleteChecked
        {
            get => _isReportObsoleteChecked;
            set
            {
                _isReportObsoleteChecked = value;
                OnPropertyChanged();
            }
        }
        private bool _isReportObsoleteChecked;

        public bool IsEnsureCopyrightHeadersChecked
        {
            get => _isEnsureCopyrightHeadersChecked;
            set
            {
                _isEnsureCopyrightHeadersChecked = value;
                OnPropertyChanged();
            }
        }
        private bool _isEnsureCopyrightHeadersChecked;

        public bool IsDisableHotfixMigrationsChecked
        {
            get => _isDisableHotfixMigrationsChecked;
            set
            {
                _isDisableHotfixMigrationsChecked = value;
                OnPropertyChanged();
            }
        }
        private bool _isDisableHotfixMigrationsChecked;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelGenerationPage" /> class.
        /// </summary>
        public ModelGenerationPage()
        {
            InitializeComponent();

            DataContext = this;
        }

        protected override void OnInitialized( EventArgs e )
        {
            base.OnInitialized( e );

            Task.Run( InitializeInterface );
        }

        /// <summary>
        /// Initialize the user interface. This happens on a background task
        /// so that the UI can update and not be frozen while we work.
        /// </summary>
        private void InitializeInterface()
        {
            Dispatcher.Invoke( () => Cursor = Cursors.Wait );

            var rockAssembly = typeof( Rock.Data.IEntity ).Assembly;
            FileInfo fi = new FileInfo( ( new System.Uri( rockAssembly.CodeBase ) ).AbsolutePath );
            string assemblyFileName = fi.FullName;

            AssemblyPath = assemblyFileName;
            DisplayedAssemblyPath = assemblyFileName;
            AssemblyDateTimeElapsed = fi.LastWriteTime.ToElapsedString();
            AssemblyDateTime = fi.LastWriteTime.ToString();

            var assembly = Assembly.LoadFrom( assemblyFileName );

            var rockWebBinRockDllFileName = Path.Combine( RootFolder().FullName, "RockWeb\\Bin\\Rock.dll" );
            if ( File.Exists( rockWebBinRockDllFileName ) )
            {
                var rockWebBinRockDll = Assembly.ReflectionOnlyLoadFrom( rockWebBinRockDllFileName );

                // if the files are identical (they should be), display the path of the RockWeb\Bin Rock.dll
                if ( rockWebBinRockDll.ManifestModule.ModuleVersionId == rockAssembly.ManifestModule.ModuleVersionId )
                {
                    DisplayedAssemblyPath = rockWebBinRockDllFileName;
                }
            }

            foreach ( Type type in assembly.GetTypes().OfType<Type>().OrderBy( a => a.FullName ) )
            {
                if ( type.Namespace != null && !type.Namespace.StartsWith( "Rock.Data" ) && !type.IsAbstract && type.GetCustomAttribute<NotMappedAttribute>() == null )
                {
                    if ( typeof( Rock.Data.IEntity ).IsAssignableFrom( type ) || type.GetCustomAttribute( typeof( TableAttribute ) ) != null )
                    {
                        _modelItems.Add( new CheckedItem<Type> { IsChecked = false, Name = type.FullName, Item = type } );
                    }
                }
            }

            CheckAllItems( true );

            var projectName = Path.GetFileNameWithoutExtension( assemblyFileName );

            SqlConnection sqlconn = CodeGenHelpers.GetSqlConnection( RootFolder().FullName );

            if ( sqlconn != null )
            {
                DatabaseConnectionString = sqlconn.Database;
            }

            ServiceFolder = Path.Combine( RootFolder().FullName, projectName );
            RestFolder = Path.Combine( RootFolder().FullName, projectName + ".Rest" );
            DatabaseFolder = Path.Combine( RootFolder().FullName, "Database" );

            if ( projectName != "Rock" )
            {
                RestFolder = Path.Combine( RootFolder().FullName, projectName + "\\Rest" );
                DatabaseFolder = Path.Combine( RootFolder().FullName, Path.GetFileNameWithoutExtension( projectName ) + ".Database" );
            }

            Dispatcher.Invoke( () =>
            {
                ModelsListBox.ItemsSource = _modelItems;

                Cursor = null;
            } );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the SelectAllCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void SelectAllCheckBox_CheckedChanged( object sender, RoutedEventArgs e )
        {
            CheckAllItems( SelectAllCheckBox.IsChecked ?? false );
        }

        /// <summary>
        /// Handles the Click event of the GenerateButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void GenerateButton_Click( object sender, RoutedEventArgs e )
        {
            GenerateButton.IsEnabled = false;

            ResultsTextBox.Text = string.Empty;
            var rootFolder = RootFolder();

            Cursor = Cursors.Wait;
            GenerateProgress.Visibility = Visibility.Visible;

            Task.Run( () =>
            {
                entityPropertyShouldBeVirtualWarnings = new List<string>();

                Dispatcher.Invoke( () =>
                {
                    GenerateProgress.Maximum = _modelItems.Count( i => i.IsChecked );
                    GenerateProgress.Value = 0;
                } );

                if ( _modelItems.Count( i => i.IsChecked ) > 0 )
                {
                    if ( rootFolder != null )
                    {
                        bool allModelsAreSelected = _modelItems.Count == _modelItems.Count( i => i.IsChecked );

                        if ( IsServiceChecked && allModelsAreSelected )
                        {
                            var codeGenFolder = Path.Combine( NamespaceFolder( ServiceFolder, "Rock.Model" ).FullName, "CodeGenerated" );
                            if ( Directory.Exists( codeGenFolder ) )
                            {
                                Directory.Delete( codeGenFolder, true );
                            }

                            Directory.CreateDirectory( codeGenFolder );
                        }

                        if ( IsRestChecked && allModelsAreSelected )
                        {
                            // var filePath1 = Path.Combine( rootFolder, "Controllers" );
                            // var file = new FileInfo( Path.Combine( filePath1, "CodeGenerated", pluralizedName + "Controller.CodeGenerated.cs" ) );

                            var codeGenFolder = Path.Combine( RestFolder, "Controllers", "CodeGenerated" );
                            if ( Directory.Exists( codeGenFolder ) )
                            {
                                Directory.Delete( codeGenFolder, true );
                            }

                            Directory.CreateDirectory( codeGenFolder );
                        }

                        foreach ( Type type in _modelItems.Where( i => i.IsChecked ).Select( i => i.Item ) )
                        {
                            Dispatcher.Invoke( () => GenerateProgress.Value++ );

                            // only generate Service and REST for IEntity types
                            if ( typeof( Rock.Data.IEntity ).IsAssignableFrom( type ) )
                            {

                                if ( IsServiceChecked )
                                {
                                    WriteServiceFile( ServiceFolder, type );
                                    EnsureEntityTypeSystemGuid( ServiceFolder, type );
                                }

                                if ( IsRestChecked )
                                {
                                    WriteRESTFile( RestFolder, type );
                                }
                            }
                        }

                        var projectName = Path.GetFileNameWithoutExtension( AssemblyPath );

                        if ( IsDatabaseProcsChecked )
                        {
                            WriteDatabaseProcsScripts( DatabaseFolder, projectName );
                        }

                        if ( IsRockGuidsChecked )
                        {
                            RockGuidCodeGenerator.EnsureRockGuidAttributes( rootFolder.FullName );
                        }

                        if ( IsEnsureCopyrightHeadersChecked )
                        {
                            EnsureCopyrightHeaders( rootFolder.FullName );
                        }

                        if ( IsDisableHotfixMigrationsChecked )
                        {
                            DisableHotFixMigrations( rootFolder.FullName );
                        }
                    }
                }

                var hasWarnings = ReportRockCodeWarnings();

                Dispatcher.Invoke( () =>
                {
                    GenerateProgress.Visibility = System.Windows.Visibility.Hidden;
                    Cursor = null;

                    if ( hasWarnings )
                    {
                        MessageBox.Show( "Files have been generated with warnings", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning );
                    }
                    else
                    {
                        MessageBox.Show( "Files have been generated" );
                    }
                } );
            } ).ContinueWith( t =>
            {
                if ( t.Exception != null )
                {
                    Dispatcher.Invoke( () =>
                    {
                        MessageBox.Show( t.Exception.InnerException.Message, "Error" );
                    } );
                }

                Dispatcher.Invoke( () => GenerateButton.IsEnabled = true );
            } );

        }

        /// <summary>
        /// Reports the rock code warnings
        /// and returns true if there are warnings.
        /// </summary>
        public bool ReportRockCodeWarnings()
        {
            bool hasWarnings = false;
            StringBuilder rockObsoleteWarnings = new StringBuilder();
            StringBuilder rockGuidWarnings = new StringBuilder();
            Dictionary<string, string> rockGuids = new Dictionary<string, string>(); // GUID, Class/Method
            List<string> singletonClassVariablesWarnings = new List<string>();
            List<string> obsoleteReportList = new List<string>();
            List<Assembly> rockAssemblyList = new List<Assembly>();
            rockAssemblyList.Add( typeof( Rock.Data.RockContext ).Assembly );
            rockAssemblyList.Add( typeof( Rock.Rest.ApiControllerBase ).Assembly );

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
                    var typeObsoleteAttribute = type.GetCustomAttribute<ObsoleteAttribute>();
                    var typeRockObsolete = type.GetCustomAttribute<RockObsolete>();
                    if ( typeObsoleteAttribute != null )
                    {
                        if ( typeRockObsolete == null )
                        {
                            rockObsoleteWarnings.AppendLine( $" - {type}" );
                        }
                        else
                        {
                            obsoleteReportList.Add( $"{typeRockObsolete.Version},{type.Name},class,{typeObsoleteAttribute.IsError}" );
                        }
                    }

                    /* See if there are any duplicate RockGuids */
                    List<Rock.SystemGuid.RockGuidAttribute> rockGuidAttributes;
                    try
                    {
                        rockGuidAttributes = type.GetCustomAttributes<SystemGuid.RockGuidAttribute>().ToList();
                    }
                    catch ( FormatException )
                    {
                        rockGuidWarnings.AppendLine( $"Invalid RockGuid on {type}" );
                        rockGuidAttributes = new List<SystemGuid.RockGuidAttribute>();
                    }

                    foreach ( var rockGuidAttribute in rockGuidAttributes )
                    {
                        if ( rockGuidAttribute.Guid.IsEmpty() )
                        {
                            rockGuidWarnings.AppendLine( $" - EMPTY,{type}" );
                        }
                        else
                        {
                            var rockGuid = rockGuidAttribute.Guid.ToString();
                            if ( !rockGuids.ContainsKey( rockGuid ) )
                            {
                                rockGuids.Add( rockGuid, $"{type}" );
                            }
                            else
                            {
                                rockGuidWarnings.AppendLine( $" - DUPLICATE,{type},{rockGuid},{rockGuids[rockGuid]}" );
                            }
                        }
                    }

                    // get all members so we can see if there are warnings that we want to show
                    var memberList = type
                        .GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static )
                        .OrderBy( a => a.Name )
                        .ToList();

                    foreach ( MemberInfo member in memberList )
                    {
                        if ( rockAssembly == member.Module.Assembly && member.DeclaringType == type )
                        {
                            /* See if member is Obsolete/RockObsolete */
                            var memberObsoleteAttribute = member.GetCustomAttribute<ObsoleteAttribute>();
                            var memberRockObsolete = member.GetCustomAttribute<RockObsolete>();
                            if ( memberObsoleteAttribute != null )
                            {
                                if ( memberRockObsolete == null )
                                {
                                    rockObsoleteWarnings.AppendLine( $" - {member.DeclaringType}.{member.Name}" );
                                }
                                else
                                {
                                    string messagePrefix = null;
                                    if ( memberRockObsolete.Version == "1.8" || memberRockObsolete.Version.StartsWith( "1.8." ) || memberRockObsolete.Version == "1.7" || memberRockObsolete.Version.StartsWith( "1.7." ) )
                                    {
                                        if ( !memberObsoleteAttribute.IsError || memberRockObsolete.Version == "1.7" || memberRockObsolete.Version.StartsWith( "1.7." ) )
                                        {
                                            messagePrefix = "###WARNING###:";
                                        }
                                    }

                                    obsoleteReportList.Add( $"{messagePrefix}{memberRockObsolete.Version},{type.Name} {member.Name},{member.MemberType},{memberObsoleteAttribute.IsError}" );
                                }
                            }
                        }

                        /* See if a singleton has class variables that are not thread-safe
                           NOTE: This won't catch all of them, but hopefully most
                         */

                        // types that OK based on how they are used.
                        var ignoredThreadSafeTypeWarning = new Type[] {
                            typeof(Rock.UniversalSearch.IndexComponents.Lucene),
                            typeof(Rock.Cms.ContentCollection.IndexComponents.Elasticsearch),
                            typeof(Rock.Cms.ContentCollection.IndexComponents.Lucene)
                        };

                        var ignoredThreadSafeFieldWarning = new string[]
                        {
                            // fields that OK based on how we use them
                            "Rock.Extension.Component.Attributes",
                            "Rock.Extension.Component.AttributeValues",
                            "Rock.Extension.Component._typeId",
                            "Rock.Extension.Component._typeGuid",
                            "Rock.Extension.Component._typeName",
                            "Rock.Web.HttpModules.ResponseHeaders.Headers",
                            "Rock.Field.FieldType.QualifierUpdated",

                             // Fields that probably should be fixed, but would take some time to figure out how to fix them.
                             "Rock.Field.Types.CurrencyFieldType.CurrencyCodeDefinedValueId",
                             "Rock.Field.Types.EnumFieldType`1._EnumValues",
                             "Rock.Financial.TestGateway.MostRecentException",
                             "Rock.Financial.TestRedirectionGateway.MostRecentException",
                             "Rock.Security.BackgroundCheck.ProtectMyMinistry._httpStatusCode",
                             "Rock.Security.ExternalAuthentication.Twitter._oauthToken",
                             "Rock.Security.ExternalAuthentication.Twitter._oauthTokenSecret",
                             "Rock.Security.ExternalAuthentication.Twitter._returnUrl",
                             "Rock.UniversalSearch.IndexComponents.Elasticsearch._client",
                             "Rock.Workflow.Action.AddStep._mergeFields",
                             "Rock.Workflow.Action.PrayerRequestAdd._action",
                             "Rock.Workflow.Action.PrayerRequestAdd._mergeField",
                             "Rock.Workflow.Action.PrayerRequestAdd._rockContext",
                             "Rock.Workflow.Action.PrayerRequestAdd._mergeFields"
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
                                            singletonClassVariablesWarnings.Add( $" - {fullyQualifiedFieldName}", true );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            StringBuilder warnings = new StringBuilder();
            if ( entityPropertyShouldBeVirtualWarnings.Count > 0 )
            {
                hasWarnings = true;
                warnings.AppendLine( "Model Properties that should be marked virtual" );
                foreach ( var warning in entityPropertyShouldBeVirtualWarnings )
                {
                    warnings.AppendLine( warning );
                }
            }

            if ( rockObsoleteWarnings.Length > 0 )
            {
                hasWarnings = true;
                warnings.AppendLine();
                warnings.AppendLine( "[Obsolete] that doesn't have [RockObsolete]" );
                warnings.Append( rockObsoleteWarnings );
            }

            if ( singletonClassVariablesWarnings.Count > 0 )
            {
                hasWarnings = true;
                warnings.AppendLine();
                warnings.AppendLine( "Singleton non-threadsafe class variables." );
                foreach ( var warning in singletonClassVariablesWarnings )
                {
                    warnings.AppendLine( warning );
                }
            }

            if ( rockGuidWarnings.Length > 0 )
            {
                hasWarnings = true;
                warnings.AppendLine();
                warnings.AppendLine( "[RockGuid] issues found." );
                warnings.Append( rockGuidWarnings );
            }

            if ( IsReportObsoleteChecked )
            {
                warnings.AppendLine();

                obsoleteReportList = obsoleteReportList.OrderBy( a => a.Split( new char[] { ',' } )[0] ).ToList();
                warnings.Append( $"Version,Name,Type,IsError" + Environment.NewLine + obsoleteReportList.AsDelimited( Environment.NewLine ) );
            }

            Dispatcher.Invoke( () => ResultsTextBox.Text = warnings.ToString().Trim() );

            return hasWarnings;
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

            SqlConnection sqlconn = CodeGenHelpers.GetSqlConnection( new DirectoryInfo( databaseRootFolder ).Parent.FullName );
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
            for ( int i = 0; i < _modelItems.Count; i++ )
            {
                _modelItems[i].IsChecked = selected;
            }
        }

        /// <summary>
        /// Ensures the entity type Guid is in SystemGuid\EntityType
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="type">The type.</param>
        private void EnsureEntityTypeSystemGuid( string rootFolder, Type type )
        {
            var entityTypeSystemGuid = type.GetCustomAttribute<SystemGuid.EntityTypeGuidAttribute>( inherit: false )?.Guid;
            if ( !entityTypeSystemGuid.HasValue )
            {
                return;
            }

            var guidString = entityTypeSystemGuid.ToString();

            var entityTypeSystemGuidFileName = new FileInfo( Path.Combine( rootFolder, "SystemGuid\\EntityType.cs" ) );

            var fileLines = File.ReadAllLines( entityTypeSystemGuidFileName.FullName );
            if ( fileLines.Any( x => x.IndexOf( guidString, StringComparison.OrdinalIgnoreCase ) > 0 ) )
            {
                // already in there
                return;
            }

            var entityTypeConstName = type.Name.SplitCase().Replace( " ", "_" ).ToUpper();

            string newEntityTypeGuidCode = $@"
        /// <summary>
        /// The EntityType Guid for <see cref=""{type.FullName}""/> 
        /// </summary>
        public const string {entityTypeConstName} = ""{guidString.ToUpper()}"";";

            var updatedFileLines = fileLines.Where( a => a != "}" && a != "    }");
            updatedFileLines = updatedFileLines.Append( newEntityTypeGuidCode );
            updatedFileLines = updatedFileLines.Append( "    }");
            updatedFileLines = updatedFileLines.Append( "}");
            File.WriteAllLines( entityTypeSystemGuidFileName.FullName, updatedFileLines.ToArray() );
        }

        /// <summary>
        /// Writes the Service file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteServiceFile( string rootFolder, Type type )
        {
            string dbContextFullName = Rock.Reflection.GetDbContextTypeForEntityType( type ).FullName;
            if ( dbContextFullName.StartsWith( "Rock.Data." ) )
            {
                dbContextFullName = dbContextFullName.Replace( "Rock.Data.", "" );
            }

            var properties = GetEntityProperties( type, true, true );

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
            sb.AppendLine( "" );

            sb.AppendLine( "using System;" );
            sb.AppendLine( "using System.Linq;" );
            sb.AppendLine( "" );
            sb.AppendLine( @"using Rock.Data;
" );

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

        /// <summary>
        /// Clones this {0} object to a new {0} object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name=""source"">The source.</param>
        /// <returns></returns>
        public static {0} CloneWithoutIdentity( this {0} source )
        {{
            var target = new {0}();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;", type.Name );

            // Only include these properties if the type is a model
            if ( type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof( Rock.Data.Model<> ) )
            {
                sb.AppendFormat( @"
            target.CreatedByPersonAliasId = null;
            target.CreatedDateTime = RockDateTime.Now;
            target.ModifiedByPersonAliasId = null;
            target.ModifiedDateTime = RockDateTime.Now;", type.Name );
            }

            sb.AppendLine( "" );
            sb.AppendLine( "" );
            sb.AppendLine( "            return target;" );
            sb.AppendLine( "        }" );

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

            sb.AppendLine( @"
        }
    }
}" );

            var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, type.Namespace ).FullName, "CodeGenerated", type.Name + "Service.CodeGenerated.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Gets the type of the typescript.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static (string type, HashSet<string> imports) GetTypescriptType( Type type, bool isRequired )
        {
            var imports = new HashSet<string>();
            var underlyingType = Nullable.GetUnderlyingType( type );
            var isNullable = underlyingType != null;

            if ( isNullable )
            {
                type = underlyingType;
            }

            // Default to "unknown" type
            var tsType = "unknown";

            // Switch on the typecode
            switch ( Type.GetTypeCode( type ) )
            {
                case TypeCode.Object:
                    if ( type == typeof( Guid ) )
                    {
                        tsType = "Guid";
                        imports.Add( "import { Guid } from \"@Obsidian/Types\";" );
                    }
                    else if ( type.IsArray )
                    {
                        var (itemType, innerImports) = GetTypescriptType( type.GetElementType(), false );
                        tsType = $"({itemType})[]";

                        foreach ( var import in innerImports )
                        {
                            imports.Add( import );
                        }
                    }
                    else
                    {
                        tsType = "Record<string, unknown>";
                    }
                    break;
                case TypeCode.DateTime:
                    tsType = "string";
                    break;
                case TypeCode.Boolean:
                    tsType = "boolean";
                    break;
                case TypeCode.String:
                    tsType = "string";
                    isNullable = !isRequired;
                    break;
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    tsType = "number";
                    break;
            }

            if ( isNullable )
            {
                return ($"{tsType} | null", imports);
            }

            return (tsType, imports);
        }

        /// <summary>
        /// Gets the license for an automatic generated code file.
        /// </summary>
        /// <value>
        /// The automatic generated license.
        /// </value>
        private string AutoGeneratedLicense => @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
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
// </copyright>
//";

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

            SqlConnection sqlconn = CodeGenHelpers.GetSqlConnection( new DirectoryInfo( serviceFolder ).Parent.FullName );
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
                var entityTypes = _modelItems.Select( t => t.Item ).Cast<Type>().ToList();

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

        public string PluralizeTypeName( Type type )
        {
            // This uses PluralizationService (Entity Framework) , which is designed to pluralize the names of types (not necessarily real worlds)
            // For Example, AttendanceOccurrence gets pluralized as AttendanceOccurrences, because it knows that Attendance and Occurrence
            // It figures out the last word in a CamelCased type name and pluralizes it.
            // are separate words using CamelCasing.

            var str = type.Name;

            // Pluralization services handles most words, but there are some exceptions (i.e. campus)
            switch ( str )
            {
                case "Campus":
                case "campus":
                    return str + "es";

                case "CAMPUS":
                    return str + "ES";

                default:
                    var pluralizationService = PluralizationService.CreateService( new CultureInfo( "en-US" ) );
                    return pluralizationService.Pluralize( str );
            }
        }

        private Dictionary<string, Guid> _restControllerGuidLookupFromDatabase = null;

        /// <summary>
        /// Writes the REST file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteRESTFile( string rootFolder, Type type )
        {
            if ( _restControllerGuidLookupFromDatabase == null )
            {
                _restControllerGuidLookupFromDatabase = RockGuidCodeGenerator.GetDatabaseGuidLookup( RootFolder().FullName, $"SELECT [Guid], [ClassName],[ModifiedDateTime] FROM [RestController]", "ClassName" ).Value;
            }

            string pluralizedName = PluralizeTypeName( type );
            string restNamespace = type.Assembly.GetName().Name + ".Rest.Controllers";
            string dbContextFullName = Rock.Reflection.GetDbContextTypeForEntityType( type ).FullName;

            var obsolete = type.GetCustomAttribute<ObsoleteAttribute>();
            var rockObsolete = type.GetCustomAttribute<RockObsolete>();
            var fullClassName = $"{restNamespace}.{pluralizedName}Controller";
            var restControllerType = Type.GetType( $"{fullClassName}, {typeof( Rock.Rest.ApiControllerBase ).Assembly.FullName}" );
            var restControllerGuid = restControllerType?.GetCustomAttribute<Rock.SystemGuid.RestControllerGuidAttribute>()?.Guid;

            if ( ( type.GetCustomAttribute<CodeGenExcludeAttribute>()?.ExcludedFeatures ?? CodeGenFeature.None ).HasFlag( CodeGenFeature.DefaultRestController ) )
            {
                return;
            }

            if ( restControllerGuid == null )
            {
                restControllerGuid = _restControllerGuidLookupFromDatabase.GetValueOrNull( fullClassName );
            }

            if ( restControllerGuid == null)
            {
                restControllerGuid = Guid.NewGuid();
            }

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
            sb.AppendLine( $"using Rock.SystemGuid;" );
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

            sb.AppendLine( $"    [RestControllerGuid( \"{ restControllerGuid.ToString().ToUpper()}\" )]" );
            sb.AppendLine( $"    public partial class {pluralizedName}Controller : Rock.Rest.ApiController<{type.Namespace}.{type.Name}>" );
            sb.AppendLine( "    {" );
            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( $"        /// Initializes a new instance of the <see cref=\"{ pluralizedName}Controller\"/> class." );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( $"        public {pluralizedName}Controller() : base( new {type.Namespace}.{type.Name}Service( new {dbContextFullName}() ) ) {{ }} " );
            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            var filePath1 = Path.Combine( rootFolder, "Controllers" );
            var file = new FileInfo( Path.Combine( filePath1, "CodeGenerated", pluralizedName + "Controller.CodeGenerated.cs" ) );
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
            var dirInfo = new DirectoryInfo( Path.GetDirectoryName( AssemblyPath ) );
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

        private List<string> entityPropertyShouldBeVirtualWarnings = null;

        /// <summary>
        /// Gets the entity properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="includeObsolete">if set to <c>true</c> [include obsolete].</param>
        /// <param name="reportVirtualPropertyWarnings">if set to <c>true</c> [report virtual property warnings].</param>
        /// <returns></returns>
        private Dictionary<string, PropertyInfo> GetEntityProperties( Type type, bool includeObsolete, bool reportVirtualPropertyWarnings )
        {
            var properties = new Dictionary<string, PropertyInfo>();

            var interfaces = type.GetInterfaces();
            var typeProperties = type.GetProperties().SortByStandardOrder();

            foreach ( var property in typeProperties )
            {
                var getMethod = property.GetGetMethod();
                if ( getMethod == null )
                {
                    continue;
                }

                if ( getMethod.IsVirtual )
                {
                    if ( !getMethod.IsFinal )
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
                    if ( !interfaceProperty )
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

                            if ( reportVirtualPropertyWarnings )
                            {
                                if ( property.PropertyType.IsClass
                                && !property.PropertyType.Namespace.StartsWith( "System" ) )
                                {
                                    entityPropertyShouldBeVirtualWarnings.Add( $" - {type.FullName} {property.Name}", true );
                                }
                            }
                        }
                    }
                }
            }

            return properties;
        }

        private void ServiceFolderTextBox_MouseDoubleClick( object sender, MouseButtonEventArgs e )
        {
            _fbdOutput.SelectedPath = ServiceFolder;
            if ( _fbdOutput.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                ServiceFolder = _fbdOutput.SelectedPath;
            }
        }

        private void RestFolderTextBox_MouseDoubleClick( object sender, MouseButtonEventArgs e )
        {
            _fbdOutput.SelectedPath = RestFolder;
            if ( _fbdOutput.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                RestFolder = _fbdOutput.SelectedPath;
            }
        }

        /// <summary>
        /// The ignore files
        /// </summary>
        static readonly string[] IgnoreFiles = new string[] { "\\DoubleMetaphone.cs", "\\Rock.Version\\AssemblySharedInfo.cs" };

        /// <summary>
        /// The ignore folders
        /// </summary>
        static readonly string[] IgnoreFolders = new string[] { "\\CodeGenerated", "\\obj", "\\Vendor" };

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
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "RockWeb\\Obsidian\\", "*.ts" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Checkr\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.DownhillCss\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Mailgun\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Mandrill\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Migrations\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.MyWell\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.NMI\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Oidc\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.PayFlowPro\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Rest\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Security.Authentication.Auth0\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.SendGrid\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.SignNow\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Slingshot\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Slingshot.Model\\" );
            //updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Specs\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.StatementGenerator\\" );
            //updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Tests\\" );

            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Version\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.ViewModels\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.WebStartup\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Applications\\" );
        }

        /// <summary>
        /// Fixups the copyright headers.
        /// </summary>
        /// <param name="searchDirectory">The search directory.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns></returns>
        private static int FixupCopyrightHeaders( string searchDirectory, string searchPattern = "*.cs" )
        {
            int result = 0;

            if ( !Directory.Exists( searchDirectory ) )
            {
                return 0;
            }

            var sourceFilenames = Directory.GetFiles( searchDirectory, searchPattern, SearchOption.AllDirectories ).ToList();

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

        /// <summary>
        /// Renames the Up() migration method into OldUp() and inserts a new empty Up() migration method.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        private static void DisableHotFixMigrations( string rootFolder )
        {
            string upMigrationText = "public override void Up()";
            string newUpMigrationText = @"public override void Up()
        {
            //----------------------------------------------------------------------------------
            // <auto-generated>
            //     This Up() migration method was generated by the Rock.CodeGeneration project.
            //     The purpose is to prevent hotfix migrations from running when they are not
            //     needed. The migrations in this file are run by an EF migration instead.
            // </auto-generated>
            //----------------------------------------------------------------------------------
        }

        private void OldUp()";

            // Get a list of cs files in the Plugin\HotFixes folder
            string hotfixFolder = Path.Combine( rootFolder, "Rock", "Plugin", "HotFixes" );
            List<string> hotfixMigrationFiles = Directory.GetFiles( hotfixFolder, "*.cs", SearchOption.TopDirectoryOnly ).Where( l => !l.Contains( "HotFixMigrationResource.Designer.cs" ) ).ToList();

            foreach ( var hotfixMigrationFile in hotfixMigrationFiles )
            {
                var migrationFileText = File.ReadAllText( hotfixMigrationFile );

                // If there is already an "OldUp() method then this file has already been commented out and can be skipped.
                if ( migrationFileText.Contains( "OldUp()" ) )
                {
                    continue;
                }

                File.WriteAllText( hotfixMigrationFile, migrationFileText.Replace( upMigrationText, newUpMigrationText ) );

                // Keeping this in case renaming the old up and creating a new empty Up is not accepted as a solution.
                //var fileLines = File.ReadLines( hotfixMigrationFile );
                //StringBuilder newText = new StringBuilder();
                //bool startCommenting = false;

                //foreach ( var fileLine in fileLines )
                //{

                //    if ( fileLine.Trim().Equals( "public override void Up()" ) )
                //    {
                //        startCommenting = true;
                //    }

                //    string newLineText = fileLine;

                //    if ( startCommenting )
                //    {
                //        if ( fileLine.TrimStart().Equals( "{" ) )
                //        {
                //            newLineText = fileLine + "/*";
                //        }
                //        else if ( fileLine.TrimStart().Equals( "}" ) )
                //        {
                //            newLineText = "*/" + fileLine;
                //            startCommenting = false;
                //        }
                //    }

                //    newText.AppendLine( newLineText );
                //}

                //File.WriteAllText( hotfixMigrationFile, newText.ToString() );
            }
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        private class CheckedItem<T> : Utility.BindingBase
        {
            public bool IsChecked
            {
                get => _isChecked;
                set
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
            private bool _isChecked;

            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
            private string _name;

            public T Item { get; set; }
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
