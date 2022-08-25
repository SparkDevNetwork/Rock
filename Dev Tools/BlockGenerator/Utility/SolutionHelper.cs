using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using EnvDTE;

using EnvDTE80;

namespace BlockGenerator.Utility
{
    /// <summary>
    /// Helper class to provide access to working with Visual Studio solution
    /// files and projects.
    /// </summary>
    public class SolutionHelper : IDisposable
    {
        #region External Methods

        [DllImport( "ole32.dll" )]
        private static extern void CreateBindCtx( int reserved, out IBindCtx ppbc );

        [DllImport( "ole32.dll" )]
        private static extern void GetRunningObjectTable( int reserved, out IRunningObjectTable prot );

        #endregion

        #region Fields

        /// <summary>
        /// The DTE object that is used to communicate with the currently running
        /// Visual Studio environment.
        /// </summary>
        private readonly DTE2 _dte;

        /// <summary>
        /// The path the solution file resides in. Used to find the paths to the
        /// project files.
        /// </summary>
        private readonly string _solutionPath;

        /// <summary>
        /// A collection of offline projects that have already been loaded.
        /// Offline projects are those accessed directly on the filesystem without
        /// going through Visual Studio.
        /// </summary>
        private readonly Microsoft.Build.Evaluation.ProjectCollection _offlineProjects;

        /// <summary>
        /// The dirty offline projects that have been modified and need to
        /// be saved.
        /// </summary>
        private readonly List<Microsoft.Build.Evaluation.Project> _dirtyOfflineProjects = new List<Microsoft.Build.Evaluation.Project>();

        /// <summary>
        /// The dirty online projects (Visual Studio DTE) that have been modified
        /// and need to be saved.
        /// </summary>
        private readonly List<Project> _dirtyOnlineProjects = new List<Project>();

        #endregion

        #region Constructors

        /// <summary>
        /// Loads the solution via the most appropriate method, either via OLE
        /// communication to a running Visual Studio instance or by direct
        /// file access.
        /// </summary>
        /// <param name="solutionFilePath">The solution file path, this is the absolute path and filename for Rock.sln.</param>
        /// <returns>An instance of <see cref="SolutionHelper"/> that can be used to work with the projects.</returns>
        /// <exception cref="System.Exception">Solution was not found.</exception>
        public static SolutionHelper LoadSolution( string solutionFilePath )
        {
            if ( !File.Exists( solutionFilePath ) )
            {
                throw new Exception( "Solution was not found." );
            }

            var solutionPath = Path.GetDirectoryName( solutionFilePath );

            var dte = GetRunningBySolution( solutionFilePath );

            return new SolutionHelper( solutionPath, dte );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionHelper"/> class.
        /// </summary>
        /// <param name="solutionPath">The solution path without the filename.</param>
        /// <param name="dte">The DTE object used for direct Visual Studio intreaction.</param>
        private SolutionHelper( string solutionPath, DTE2 dte )
        {
            _solutionPath = solutionPath;
            _dte = dte;

            _offlineProjects = new Microsoft.Build.Evaluation.ProjectCollection();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _offlineProjects.Dispose();
        }

        #endregion

        #region Methods

        private static DTE2 GetRunningBySolution( string solutionPath )
        {
            GetRunningObjectTable( 0, out var rot );
            rot.EnumRunning( out var enumMoniker );
            enumMoniker.Reset();
            IntPtr fetched = IntPtr.Zero;
            IMoniker[] moniker = new IMoniker[1];

            while ( enumMoniker.Next( 1, moniker, fetched ) == 0 )
            {
                CreateBindCtx( 0, out var bindCtx );

                moniker[0].GetDisplayName( bindCtx, null, out var displayName );

                if ( displayName.StartsWith( "!VisualStudio.DTE" ) )
                {
                    try
                    {
                        if ( rot.GetObject( moniker[0], out var comObject ) == 0 )
                        {
                            var dte = ( DTE2 ) comObject;
                            if ( dte.Solution.FullName == solutionPath )
                            {
                                return dte;
                            }
                        }
                    }
                    catch
                    {
                        /* Ignore and continue. */
                    }
                }
            }

            return null;
        }

        private Project GetOnlineProjectByName( string projectName )
        {
            return _dte?.Solution.Projects
                .Cast<Project>()
                .FirstOrDefault( p => p.Name == projectName );
        }

        private ProjectItem GetOnlineProjectFolderFromPath( Project project, string path, bool createIfMissing )
        {
            ProjectItem item = null;
            var segments = path.Split( '\\' );
            var items = project.ProjectItems;
            var parentItems = project.ProjectItems;

            for ( int i = 0; i < segments.Length; i++ )
            {
                item = parentItems.Cast<ProjectItem>().FirstOrDefault( pi => pi.Name == segments[i] );

                if ( item == null )
                {
                    if ( !createIfMissing )
                    {
                        return null;
                    }

                    var pathComponents = new List<string>();
                    pathComponents.Add( Path.GetDirectoryName( project.FullName ) );
                    pathComponents.AddRange( segments.Take( i + 1 ) );
                    var folderPath = Path.Combine( pathComponents.ToArray() );

                    item = parentItems.AddFromDirectory( folderPath );
                    System.Threading.Thread.Sleep( 10 );

                    // Remove any recursive items that were added.
                    foreach ( var itemToDelete in item.ProjectItems.Cast<ProjectItem>().ToList() )
                    {
                        itemToDelete.Remove();
                        System.Threading.Thread.Sleep( 10 );
                    }

                    if ( !_dirtyOnlineProjects.Contains( project ) )
                    {
                        _dirtyOnlineProjects.Add( project );
                    }
                }

                parentItems = item.ProjectItems;
            }

            return item;
        }

        private Microsoft.Build.Evaluation.Project GetOfflineProjectByName( string projectName )
        {
            var projectPath = Path.Combine( _solutionPath, projectName );

            var project = _offlineProjects.LoadedProjects
                .FirstOrDefault( p => p.GetPropertyValue( "AssemblyName" ) == projectName || p.GetPropertyValue( "Name" ) == projectName );

            if ( project == null )
            {
                if ( File.Exists( Path.Combine( projectPath, $"{projectName}.csproj" ) ) )
                {
                    project = _offlineProjects.LoadProject( Path.Combine( projectPath, $"{projectName}.csproj" ) );
                }
                else if ( File.Exists( Path.Combine( projectPath, $"{projectName}.njsproj" ) ) )
                {
                    project = _offlineProjects.LoadProject( Path.Combine( projectPath, $"{projectName}.njsproj" ) );
                }
                else
                {
                    throw new Exception( $"Unable to find project file for project '{projectName}'." );
                }
            }

            return project;
        }

        public void AddCompileFileToProject( string projectName, string absoluteFilePath )
        {
            if ( _dte != null )
            {
                var project = GetOnlineProjectByName( projectName );
                var projectPath = Path.GetDirectoryName( project.FullName );

                if ( !absoluteFilePath.StartsWith( projectPath ) )
                {
                    throw new Exception( "Attempted to add file from outside project path." );
                }

                var relativeFilePath = absoluteFilePath.Substring( projectPath.Length + 1 );

                var folder = GetOnlineProjectFolderFromPath( project, Path.GetDirectoryName( relativeFilePath ), true );

                if ( !folder.ProjectItems.Cast<ProjectItem>().Any( pi => pi.Name == Path.GetFileName( absoluteFilePath ) ) )
                {
                    folder.ProjectItems.AddFromFile( absoluteFilePath );
                    System.Threading.Thread.Sleep( 10 );

                    if ( !_dirtyOnlineProjects.Contains( project ) )
                    {
                        _dirtyOnlineProjects.Add( project );
                    }
                }
            }
            else
            {
                var project = GetOfflineProjectByName( projectName );

                var projectPath = project.DirectoryPath;

                if ( !absoluteFilePath.StartsWith( projectPath ) )
                {
                    throw new Exception( "Attempted to add file from outside project path." );
                }

                var relativeFilePath = absoluteFilePath.Substring( projectPath.Length + 1 );

                if ( !project.Items.Any( pi => pi.EvaluatedInclude == relativeFilePath ) )
                {
                    if ( relativeFilePath.EndsWith( ".ts" ) )
                    {
                        var folderRelativePath = Path.GetDirectoryName( relativeFilePath ) + "\\";

                        if ( !project.Items.Any( pi => pi.EvaluatedInclude == folderRelativePath && pi.ItemType == "Folder" ) )
                        {
                            project.AddItem( "Folder", folderRelativePath );
                        }

                        project.AddItem( "TypeScriptCompile", relativeFilePath );
                    }
                    else
                    {
                        project.AddItem( "Compile", relativeFilePath );
                    }

                    if ( !_dirtyOfflineProjects.Contains( project ) )
                    {
                        _dirtyOfflineProjects.Add( project );
                    }
                }
            }
        }

        /// <summary>
        /// Saves changes to any projects that have been modified. This persists
        /// the changes to disk immediately, but the changes may eventually be
        /// persisted even if this isn't called.
        /// </summary>
        public void Save()
        {
            foreach ( var onlineProject in _dirtyOnlineProjects )
            {
                onlineProject.Save();
                System.Threading.Thread.Sleep( 100 );
            }

            foreach ( var offlineProject in _dirtyOfflineProjects )
            {
                offlineProject.Save();
            }

            _dirtyOnlineProjects.Clear();
            _dirtyOfflineProjects.Clear();
        }

        #endregion
    }
}
