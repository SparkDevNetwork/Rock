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
    public class ImageFieldAttribute : FieldAttribute
    {
        private const string IMAGE_GUID = "Value";

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
        [Obsolete( "Use the constructor that only takes a binaryFileTypeGuid and name." )]
        [RockObsolete( "20.0" )]
        public ImageFieldAttribute( string binaryFileTypeGuid, string name = "Image", string description = "", bool required = true, string defaultBinaryFileGuid = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.IMAGE.AsGuid(), name, description, required, defaultBinaryFileGuid, category, order, key )
        {
            var configValue = new Field.ConfigurationValue( binaryFileTypeGuid );
            FieldConfigurationValues.AddOrReplace( IMAGE_GUID, configValue );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFieldAttribute"/> class.
        /// </summary>
        /// <param name="binaryFileTypeGuid">The guid of the type of files</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// This is essentially a temporary constructor. Once the constructor
        /// takes multiple parameters is removed, this constructor can be marked
        /// as obsolete and a new constructor that takes only a name parameter
        /// can be added to match the pattern of all other field attributes.
        /// We can't go directly to a single name parameter because it would
        /// conflict with the original constructor that takes the binary file
        /// type guid as the first parameter.
        /// </remarks>
        public ImageFieldAttribute( string binaryFileTypeGuid, string name )
            : base( SystemGuid.FieldType.IMAGE.AsGuid(), name )
        {
            BinaryFileTypeGuid = binaryFileTypeGuid;
        }

        /// <summary>
        /// The unique identifier of the binary file type that should be used
        /// when uploading files.
        /// </summary>
        public string BinaryFileTypeGuid
        {
            get => FieldConfigurationValues.GetValueOrNull( IMAGE_GUID );
            set => FieldConfigurationValues.AddOrReplace( IMAGE_GUID, new Field.ConfigurationValue( value ) );
        }
    }
}
