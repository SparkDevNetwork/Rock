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
using System.Collections.Generic;
using System.Linq;

namespace Rock.Utility
{
    /// <summary>
    ///     A base class that provides common functions to create or parse versioned settings strings used by various Rock components.
    /// </summary>
    public abstract class SettingsStringBase
    {
        /// <summary>
        /// The settings delimiter
        /// </summary>
        public const string SettingsDelimiter = "|";

        /// <summary>
        /// Gets the settings version.
        /// </summary>
        /// <value>
        /// The settings version.
        /// </value>
        public virtual int SettingsVersion
        {
            get { return 1; }
        }

        /// <summary>
        ///     Indicates if the current settings are valid.
        ///     Override this property to implement verification of custom settings.
        /// </summary>
        public virtual bool IsValid
        {
            get { return true; }
        }

        /// <summary>
        ///     Implemented by the derived class to set property values parsed from a settings string.
        ///     The derived class should implement parsing for previous versions of the settings string where needed.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="parameters"></param>
        protected abstract void OnSetParameters( int version, IReadOnlyList<string> parameters );

        /// <summary>
        ///     Implemented by the derived class to return an ordered set of property values that can be used to construct the
        ///     settings string.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<string> OnGetParameters();

        /// <summary>
        ///     Set values from a string representation of the settings.
        /// </summary>
        /// <param name="selectionString"></param>
        public void FromSelectionString( string selectionString )
        {
            // Selection String is of the format: version=X|param1|param2|param3...
            // If version parameter is missing, assume version = 1.                   
            var parameterValues = selectionString.Split( new string[] { SettingsDelimiter }, StringSplitOptions.None ).ToList();

            // Read the settings string version from the first parameter.
            // This allows us to cater for any future upgrades to the content and format of the settings string.
            string versionParameter = parameterValues.ElementAtOrDefault( 0 ).ToStringSafe().Trim();

            int version = 1;

            if (versionParameter.StartsWith( "version=" ))
            {
                versionParameter = versionParameter.Substring( 8 );

                int.TryParse( versionParameter, out version );

                // Remove the Version parameter from the list of settings.
                parameterValues.RemoveAt( 0 );
            }

            OnSetParameters( version, parameterValues );
        }

        /// <summary>
        ///     Returns a delimited string representation of the settings.
        /// </summary>
        /// <returns></returns>
        public string ToSelectionString()
        {
            var settings = new List<string>();

            var parameters = OnGetParameters();

            settings.Add( "version=" + SettingsVersion );

            if (parameters != null)
            {
                settings.AddRange( parameters );
            }

            return settings.AsDelimited( SettingsDelimiter );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToSelectionString();
        }
    }
}