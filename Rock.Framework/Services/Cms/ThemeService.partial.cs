//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Rock.Services.Cms
{
    /// <summary>
    /// TODO:  The ThemeService is no longer used.  These methods need to be moved someplace
    /// else.  The Site list module uses them
    /// </summary>
	public partial class ThemeService
	{
		private string _themePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeService"/> class.
        /// </summary>
        /// <param name="rootPath">The root path.</param>
		public ThemeService( string rootPath )
		{
			_themePath = rootPath;

			if ( !rootPath.EndsWith( @"\" ) )
				_themePath += @"\";

			_themePath += @"Themes\";
		}

		/// <summary>
		/// Returns the valid themes found in this installation.
		/// </summary>
		/// <returns></returns>
		public string[] GetThemesNames()
		{
			string[] themeFolders = Directory.GetDirectories( _themePath );
			List<string> list = new List<string>();
			// TODO validate these are good, legal themes?
			for ( int i = 0; i < themeFolders.Length; i++ )
			{
				list.Add( themeFolders[i].Substring( _themePath.Length ) );
			}
			return list.ToArray();
		}

        /// <summary>
        /// Gets the theme layout names.
        /// </summary>
        /// <param name="themeName">Name of the theme.</param>
        /// <returns></returns>
		public string[] GetThemeLayoutNames( string themeName )
		{
			string path = string.Format( @"{0}{1}\Layouts\", _themePath, themeName );
			string[] layoutNames = Directory.GetFiles( path , "*.aspx" );
			
			List<string> list = new List<string>();
			// TODO validate these are good, legal layouts?
			string fileName = string.Empty;
			for ( int i = 0; i < layoutNames.Length; i++ )
			{
				fileName = layoutNames[i].Substring( path.Length );
				list.Add( fileName.Substring(0, fileName.Length - 5) );	// remove .aspx extension
			}
			list.Sort();
			return list.ToArray();
		}
	}
}