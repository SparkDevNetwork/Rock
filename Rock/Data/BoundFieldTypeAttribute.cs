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
using System.Web.UI.WebControls;

namespace Rock.Data
{
    /// <summary>
    /// Apply BoundFieldType to a property to specify a specific BoundField type to use when this value 
    /// is displayed in DataView and Report grids
    /// </summary>
    [AttributeUsage( AttributeTargets.Property )]
    public class BoundFieldTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the type of the bound field.
        /// </summary>
        /// <value>
        /// The type of the bound field.
        /// </value>
        public Type BoundFieldType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundFieldTypeAttribute"/> class.
        /// </summary>
        /// <param name="boundFieldType">Type of the bound field.</param>
        /// <exception cref="Rock.Data.BoundFieldTypeException"></exception>
        public BoundFieldTypeAttribute( Type boundFieldType )
        {
            if ( !( typeof( BoundField ).IsAssignableFrom( boundFieldType ) ) )
            {
                throw new BoundFieldTypeException();
            }

            this.BoundFieldType = boundFieldType;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BoundFieldTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundFieldTypeException"/> class.
        /// </summary>
        public BoundFieldTypeException()
            : base( "boundFieldType must be a BoundField type" )
        {

        }
    }
}
