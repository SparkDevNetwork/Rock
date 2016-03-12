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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more Binary File Types stored as a comma-delimited list of BinaryFileType.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class BinaryFileTypesFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileTypesFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultBinaryFileTypeGuids">The default binary file type guids.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public BinaryFileTypesFieldAttribute( string name = "Binary File Types", string description = "", bool required = true, string defaultBinaryFileTypeGuids = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultBinaryFileTypeGuids, category, order, key, typeof( Rock.Field.Types.BinaryFileTypesFieldType ).FullName )
        {
        }
    }
}