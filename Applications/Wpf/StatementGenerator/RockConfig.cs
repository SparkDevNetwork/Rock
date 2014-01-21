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
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class RockConfig
    {
        /// <summary>
        /// The file name
        /// </summary>
        private static string fileName = "rockConfig.xml";

        /// <summary>
        /// The default logo file
        /// </summary>
        public static string DefaultLogoFile = "logo.jpg";

        /// <summary>
        /// Gets or sets the rock base URL.
        /// </summary>
        /// <value>
        /// The rock base URL.
        /// </value>
        [XmlElement]
        [DataMember]
        public string RockBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [XmlElement]
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [XmlElement]
        [DataMember]
        private byte[] PasswordEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get
            {
                try
                {
                    byte[] clearTextPasswordBytes = ProtectedData.Unprotect( PasswordEncrypted ?? new byte[] { 0 }, null, DataProtectionScope.CurrentUser );
                    return Encoding.Unicode.GetString( clearTextPasswordBytes );
                }
                catch ( CryptographicException )
                {
                    return string.Empty;
                }
            }

            set
            {
                try
                {
                    byte[] clearTextPasswordBytes = Encoding.Unicode.GetBytes( value );
                    PasswordEncrypted = ProtectedData.Protect( clearTextPasswordBytes, null, DataProtectionScope.CurrentUser );
                }
                catch ( CryptographicException )
                {
                    PasswordEncrypted = new byte[] { 0 };
                }
            }
        }

        /// <summary>
        /// Gets or sets the layout file.
        /// </summary>
        /// <value>
        /// The layout file.
        /// </value>
        [XmlElement]
        [DataMember]
        public string LayoutFile { get; set; }

        /// <summary>
        /// Gets or sets the logo file.
        /// </summary>
        /// <value>
        /// The logo file.
        /// </value>
        [XmlElement]
        [DataMember]
        public string LogoFile {
            get
            {
                string result = (_logoFile ?? string.Empty).Trim();
                if ( !string.IsNullOrWhiteSpace( result ) )
                {
                    if ( File.Exists( result ) )
                    {
                        return result;
                    }
                }

                return DefaultLogoFile;
            }
            set
            {
                _logoFile = value;
            }
        }
        private string _logoFile;
        

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            DataContractSerializer s = new DataContractSerializer( this.GetType() );
            FileStream fs = new FileStream( fileName, FileMode.Create );
            s.WriteObject( fs, this );
            fs.Close();

            _rockConfig = null;
        }

        /// <summary>
        /// Gets or sets the _rock config.
        /// </summary>
        /// <value>
        /// The _rock config.
        /// </value>
        private static RockConfig _rockConfig { get; set; }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <returns></returns>
        public static RockConfig Load()
        {
            try
            {
                if ( _rockConfig != null )
                {
                    return _rockConfig;
                }

                if ( File.Exists( fileName ) )
                {
                    FileStream fs = new FileStream( fileName, FileMode.OpenOrCreate );
                    try
                    {
                        DataContractSerializer s = new DataContractSerializer( typeof( RockConfig ) );
                        _rockConfig = s.ReadObject( fs ) as RockConfig;
                        return _rockConfig;
                    }
                    finally
                    {
                        fs.Close();
                    }
                }

                return new RockConfig();
            }
            catch
            {
                return new RockConfig();
            }
        }
    }
}
