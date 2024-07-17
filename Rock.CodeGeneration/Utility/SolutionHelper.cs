using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using EnvDTE;

using EnvDTE80;

namespace Rock.CodeGeneration.Utility
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
            if ( _offlineProjects != null )
            {
                _offlineProjects.Dispose();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the running DTE2 object that is handling the full solution
        /// path and filename.
        /// </summary>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns>A <see cref="DTE2"/> object that represents the connection to Visual Studio.</returns>
        private static DTE2 GetRunningBySolution( string solutionPath )
        {
            // Initialize the enumerator to find all running processes.
            GetRunningObjectTable( 0, out var rot );
            rot.EnumRunning( out var enumMoniker );
            enumMoniker.Reset();
            IntPtr fetched = IntPtr.Zero;
            IMoniker[] moniker = new IMoniker[1];

            // Loop through each process and look for one that matches our criteria.
            while ( enumMoniker.Next( 1, moniker, fetched ) == 0 )
            {
                CreateBindCtx( 0, out var bindCtx );

                moniker[0].GetDisplayName( bindCtx, null, out var displayName );

                // Check any process that has the special token to indicate
                // Visual Studio.
                if ( !displayName.StartsWith( "!VisualStudio.DTE" ) )
                {
                    continue;
                }

                try
                {
                    // Try to get the COM object that lets us talk to the process
                    // and then check if the open solution is the one we want.
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

            return null;
        }

        /// <summary>
        /// Gets a reference to the online Project COM object from the project name.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>A <see cref="Project"/> that represents the solution project.</returns>
        private Project GetOnlineProjectByName( string projectName )
        {
            return _dte?.Solution.Projects
                .Cast<Project>()
                .FirstOrDefault( p => p?.Name == projectName );
        }

        /// <summary>
        /// Gets a reference to the folder object for an online project.
        /// </summary>
        /// <param name="project">The project to be searched.</param>
        /// <param name="path">The path that makes up the folder we are looking for.</param>
        /// <param name="createIfMissing">If set to <c>true</c> then create the folder components that are missing.</param>
        /// <returns>A <see cref="ProjectItem"/> that represents the folder; or <c>null</c> if it wasn't found.</returns>
        private ProjectItem GetOnlineProjectFolderFromPath( Project project, string path, bool createIfMissing )
        {
            ProjectItem item = null;
            var segments = path.Split( '\\' );
            var items = project.ProjectItems;
            var parentItems = project.ProjectItems;

            for ( int i = 0; i < segments.Length; i++ )
            {
                // Check if the folder component already exists.
                item = parentItems.Cast<ProjectItem>().FirstOrDefault( pi => pi.Name == segments[i] );

                if ( item == null )
                {
                    // Doesn't exist and we aren't creating, so fail.
                    if ( !createIfMissing )
                    {
                        return null;
                    }

                    // In order to add the directory, we need to construct the
                    // full file-system path to the directory.
                    var pathComponents = new List<string>
                    {
                        Path.GetDirectoryName( project.FullName )
                    };
                    pathComponents.AddRange( segments.Take( i + 1 ) );
                    var folderPath = Path.Combine( pathComponents.ToArray() );

                    // Request Visual Studio add a new folder from the directory.
                    item = parentItems.AddFromDirectory( folderPath );

                    // Wait a moment for the COM object to settle, otherwise
                    // random errors crop up.
                    System.Threading.Thread.Sleep( 10 );

                    // Remove any recursive items that were added. This is done
                    // since there might be items in the directory that aren't
                    // supposed to be in the project.
                    foreach ( var itemToDelete in item.ProjectItems.Cast<ProjectItem>().ToList() )
                    {
                        itemToDelete.Remove();

                        // Wait a moment for the COM object to settle, otherwise
                        // random errors crop up.
                        System.Threading.Thread.Sleep( 10 );
                    }

                    // Mark the project as dirty if it isn't already.
                    if ( !_dirtyOnlineProjects.Contains( project ) )
                    {
                        _dirtyOnlineProjects.Add( project );
                    }
                }

                parentItems = item.ProjectItems;
            }

            return item;
        }

        /// <summary>
        /// Gets a reference to an offline project from its name.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>A <see cref="Microsoft.Build.Evaluation.Project"/> instance that represents the project.</returns>
        /// <exception cref="Exception">Unable to find project file for project '{projectName}'.</exception>
        private Microsoft.Build.Evaluation.Project GetOfflineProjectByName( string projectName )
        {
            // Construct the full path to the project file, without extension.
            var projectPath = Path.Combine( _solutionPath, projectName );

            // Check if the project is already loaded.
            var project = _offlineProjects.LoadedProjects
                .FirstOrDefault( p => p.GetPropertyValue( "AssemblyName" ) == projectName || p.GetPropertyValue( "Name" ) == projectName );

            if ( project == null )
            {
                // The project isn't loaded, so try to load it as either a C#
                // project or a Node project.
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

        /// <summary>
        /// Adds the file to project and mark it as a file to be compiled.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="absoluteFilePath">The absolute path and filename to the file that should be added.</param>
        /// <exception cref="Exception">Attempted to add file from outside project path.</exception>
        public void AddCompileFileToProject( string projectName, string absoluteFilePath )
        {
            // Add the file to either a running Visual Studio instance or
            // directly to the files on disk.
            if ( _dte != null )
            {
                // Get a reference to the project and determine the file system
                // path of the project.
                var project = GetOnlineProjectByName( projectName );
                var projectPath = Path.GetDirectoryName( project.FullName );

                if ( !absoluteFilePath.StartsWith( projectPath ) )
                {
                    throw new Exception( "Attempted to add file from outside project path." );
                }

                // Get the folder in the project that will contain the file.
                var relativeFilePath = absoluteFilePath.Substring( projectPath.Length + 1 );
                var folder = GetOnlineProjectFolderFromPath( project, Path.GetDirectoryName( relativeFilePath ), true );

                // If the file doesn't already exist in the project then add it.
                if ( !folder.ProjectItems.Cast<ProjectItem>().Any( pi => pi.Name == Path.GetFileName( absoluteFilePath ) ) )
                {
                    folder.ProjectItems.AddFromFile( absoluteFilePath );

                    // Wait a moment for the COM object to settle, otherwise
                    // random errors crop up.
                    System.Threading.Thread.Sleep( 10 );

                    // Mark the project as dirty if it isn't already.
                    if ( !_dirtyOnlineProjects.Contains( project ) )
                    {
                        _dirtyOnlineProjects.Add( project );
                    }
                }
            }
            else
            {
                // Get a reference to the project and determine the file system
                // path of the project.
                var project = GetOfflineProjectByName( projectName );
                var projectPath = project.DirectoryPath;

                if ( !absoluteFilePath.StartsWith( projectPath ) )
                {
                    throw new Exception( "Attempted to add file from outside project path." );
                }

                var relativeFilePath = absoluteFilePath.Substring( projectPath.Length + 1 );

                // If the file doesn't already exist in the project file then add it.
                if ( !project.Items.Any( pi => pi.EvaluatedInclude == relativeFilePath ) )
                {
                    project.AddItem( "Compile", relativeFilePath );

                    // Mark the project as dirty if it isn't already.
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

                // Wait a moment for the COM object to settle, otherwise
                // random errors crop up.
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
