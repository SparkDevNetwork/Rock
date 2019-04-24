// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a binary file
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class FileFieldAttribute : FieldAttribute
    {
        private const string BINARY_FILE_TYPE = "binaryFileType";

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileFieldAttribute"/> class.
        /// </summary>
        /// <param name="binaryFileTypeGuid">The guid of the type of files</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultBinaryFileGuid">The default binary file guid.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public FileFieldAttribute( string binaryFileTypeGuid, string name = "Binary File", string description = "", bool required = true, string defaultBinaryFileGuid = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultBinaryFileGuid, category, order, key, typeof( Rock.Field.Types.FileFieldType ).FullName )
        {
            var configValue = new Field.ConfigurationValue( binaryFileTypeGuid );
            FieldConfigurationValues.Add( BINARY_FILE_TYPE, configValue );
        }
    }
}