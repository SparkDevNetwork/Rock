//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Rock.Data
{
    /// <summary>
    /// Static class to support factory method implementation.
    /// </summary>
    public class RepositoryFactory<T> where T : Entity<T>, new()
    {
        /// <summary>
        /// Finds a repository object based on app settings in web/app.config file.
        /// </summary>
        /// <returns>
        /// IRepository of type T
        /// </returns>
        public IRepository<T> FindRepository()
        {
            var type = typeof( T );
            var key = type.ToString() + "RepositoryType";
            key = key.Substring( key.LastIndexOf( '.' ) + 1 );
            // Check <appSettings> in .config file first...
            var repositoryType = ConfigurationManager.AppSettings[key];

            // If empty, return a default of the EFRepository<T>...
            if ( string.IsNullOrEmpty( repositoryType ) )
            {
                return new EFRepository<T>();
            }

            var settingArray = repositoryType.Split( new[] { ',' } );
            string className, assemblyName;

            // Is this a generic Repository?
            if ( Regex.IsMatch( repositoryType, @"`\d\[\[.+\]\]," ) )
            {
                className = settingArray[0] + "," + settingArray[1];
                assemblyName = settingArray[2];
            }
            // Or is it already typed?
            else
            {
                className = settingArray[0];
                assemblyName = settingArray[1];
            }

            return (IRepository<T>)Activator.CreateInstance( assemblyName, className ).Unwrap();
        }
    }
}