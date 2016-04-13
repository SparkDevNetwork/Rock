﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using dotless.Core;
using dotless.Core.configuration;

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class RockTheme
    {
        static private string _themeDirectory = HttpRuntime.AppDomainAppPath + "Themes";
        
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the logical path.
        /// </summary>
        /// <value>
        /// The logical path.
        /// </value>
        public string RelativePath { get; set; }

        /// <summary>
        /// Gets or sets the physical path.
        /// </summary>
        /// <value>
        /// The physical path.
        /// </value>
        public string AbsolutePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allows compile].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows compile]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowsCompile { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockTheme"/> class.
        /// </summary>
        public RockTheme()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockTheme"/> class.
        /// </summary>
        /// <param name="themeName">Name of the theme.</param>
        public RockTheme(string themeName )
        {
            string themePath = _themeDirectory + @"\" + themeName;

            if (Directory.Exists( themePath ) )
            {
                DirectoryInfo themeDirectory = new DirectoryInfo( themePath );
                this.Name = themeDirectory.Name;
                this.AbsolutePath = themeDirectory.FullName;
                this.RelativePath = "/Themes/" + this.Name;

                this.IsSystem = File.Exists( themeDirectory.FullName + @"\.system" );
                this.AllowsCompile = !File.Exists( themeDirectory.FullName + @"\Styles\.nocompile" );
            }
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        public bool Compile(out string messages)
        {
            messages = string.Empty;
            bool result = true;

            try
            {
                DirectoryInfo themeDirectory = new DirectoryInfo( this.AbsolutePath + @"\Styles" );
                if ( themeDirectory.Exists )
                {
                    FileInfo[] files = themeDirectory.GetFiles();

                    DotlessConfiguration dotLessConfiguration = new DotlessConfiguration();
                    dotLessConfiguration.MinifyOutput = true;
                    //dotLessConfiguration.RootPath = themeDirectory.FullName;

                    Directory.SetCurrentDirectory( themeDirectory.FullName );

                    if ( files != null )
                    {
                        if ( this.AllowsCompile )
                        {
                            // don't compile files that start with an underscore
                            foreach ( var file in files.Where( f => f.Name.EndsWith( ".less" ) && !f.Name.StartsWith( "_" ) ) )
                            {
                                string cssSource = LessWeb.Parse( File.ReadAllText( file.FullName ), dotLessConfiguration );
                                File.WriteAllText( file.DirectoryName + @"\" + file.Name.Replace( ".less", ".css" ), cssSource );
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                result = false;
                messages = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <returns></returns>
        public bool Compile( )
        {
            string messages = string.Empty;
            return Compile( out messages );
        }

        /// <summary>
        /// Compiles all themes.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public static bool CompileAll(out string messages )
        {
            messages = string.Empty;
            bool allCompiled = true;

            foreach(var theme in GetThemes() )
            {
                string themeMessage = string.Empty;
                bool themeSuccess = theme.Compile( out themeMessage );

                if ( !themeSuccess )
                {
                    allCompiled = false;
                    messages += string.Format( "Failed to compile the theme {0} ({1}).", theme.Name, themeMessage );
                }
            }

            return allCompiled;
        }

        /// <summary>
        /// Compiles all themes.
        /// </summary>
        /// <returns></returns>
        public static bool CompileAll()
        {
            string messages = string.Empty;
            return CompileAll( out messages );
        }

        /// <summary>
        /// Gets the themes.
        /// </summary>
        /// <returns></returns>
        public static List<RockTheme> GetThemes()
        {
            List<RockTheme> themes = new List<RockTheme>();

            DirectoryInfo themeDirectory = new DirectoryInfo( _themeDirectory );

            var themeDirectories = themeDirectory.GetDirectories();

            foreach ( var theme in themeDirectories )
            {
                themes.Add( new RockTheme( theme.Name ) );
            }

            return themes;
        }

        /// <summary>
        /// Clones the theme.
        /// </summary>
        public static bool CloneTheme(string sourceTheme, string targetTheme, out string messages)
        {
            messages = string.Empty;
            bool resultSucess = true;

            string sourceFullPath = _themeDirectory + @"\" + sourceTheme;
            string targetFullPath = _themeDirectory + @"\" + CleanThemeName( targetTheme );

            if ( !Directory.Exists( targetFullPath ) )
            {
                try
                {
                    DirectoryInfo diSource = new DirectoryInfo( sourceFullPath );
                    DirectoryInfo diTarget = new DirectoryInfo( targetFullPath );

                    CopyAll( diSource, diTarget );

                    // delete the .system file if it exists
                    string systemFile = targetFullPath + @"\.system";
                    if (File.Exists( systemFile ) )
                    {
                        File.Delete( systemFile );
                    }
                }
                catch ( Exception ex )
                {
                    resultSucess = false;
                    messages = ex.Message;
                }
            }
            else
            {
                resultSucess = false;
                messages = "The target theme name already exists.";
            }
            
            return resultSucess;
        }

        /// <summary>
        /// Clones the theme.
        /// </summary>
        /// <param name="sourceTheme">The source theme.</param>
        /// <param name="targetTheme">The target theme.</param>
        /// <returns></returns>
        public static bool CloneTheme( string sourceTheme, string targetTheme )
        {
            string messages = string.Empty;
            return CloneTheme( sourceTheme, targetTheme, out messages );
        }

        /// <summary>
        /// Deletes the theme.
        /// </summary>
        /// <param name="themeName">Name of the theme.</param>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public static bool DeleteTheme(string themeName, out string messages )
        {
            messages = string.Empty;
            bool resultSucess = true;

            var theme = new RockTheme( themeName );

            if ( !theme.IsSystem )
            {
                try
                {
                    Directory.Delete( theme.AbsolutePath, true );
                }
                catch ( Exception ex )
                {
                    resultSucess = false;
                    messages = ex.Message;
                }
            } 
            else
            {
                resultSucess = false;
                messages = "Can not delete a system theme.";
            }

            return resultSucess;
        }

        /// <summary>
        /// Deletes the theme.
        /// </summary>
        /// <param name="themeName">Name of the theme.</param>
        /// <returns></returns>
        public static bool DeleteTheme( string themeName )
        {
            string messages = string.Empty;
            return DeleteTheme( themeName, out messages );
        }

        /// <summary>
        /// Cleans the name of the theme.
        /// </summary>
        /// <param name="themeName">Name of the theme.</param>
        /// <returns></returns>
        public static string CleanThemeName(string themeName )
        {
            return Path.GetInvalidFileNameChars().Aggregate( themeName, ( current, c ) => current.Replace( c.ToString(), string.Empty ) ).Replace(" ", "");
        }

        /// <summary>
        /// Copies all.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        private static void CopyAll( DirectoryInfo source, DirectoryInfo target )
        {
            // from: msdn.microsoft.com/en-us/library/system.io.directoryinfo.aspx

            Directory.CreateDirectory( target.FullName );

            // Copy each file into the new directory.
            foreach ( FileInfo fi in source.GetFiles() )
            {
                fi.CopyTo( Path.Combine( target.FullName, fi.Name ), true );
            }

            // Copy each subdirectory using recursion.
            foreach ( DirectoryInfo diSourceSubDir in source.GetDirectories() )
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory( diSourceSubDir.Name );
                CopyAll( diSourceSubDir, nextTargetSubDir );
            }
        }
    }
}
