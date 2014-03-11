// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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