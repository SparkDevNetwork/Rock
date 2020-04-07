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
    /// A class Attribute that can be used by any object that inherits from <see cref="Rock.Attribute.IHasAttributes"/> to specify what attributes it needs.  The 
    /// Framework provides methods in the <see cref="Rock.Attribute.Helper"/> class to create, read, and update the attributes
    /// </summary>
    /// <remarks>
    /// If using a custom <see cref="Rock.Field.IFieldType"/> make sure that the fieldtype has been added to Rock.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class MemoFieldAttribute : FieldAttribute
    {

        private const string NUMBER_OF_ROWS = "numberofrows";
        private const string ALLOW_HTML = "allowhtml";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <param name="allowHtml">if set to <c>true</c> [allow HTML].</param>
        public MemoFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", 
            int order = 0, string key = null, int numberOfRows = 3, bool allowHtml = false )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.MemoFieldType ).FullName)
        {
            NumberOfRows = numberOfRows;
            AllowHtml = allowHtml;
        }

        /// <summary>
        /// Sets a value indicating whether [allow HTML].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow HTML]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowHtml
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_HTML ).AsBoolean();
            }
            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_HTML, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets the height of the editor.
        /// </summary>
        /// <value>
        /// The height of the editor.
        /// </value>
        public int NumberOfRows
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( NUMBER_OF_ROWS ).AsIntegerOrNull() ?? 3;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( NUMBER_OF_ROWS, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

    }
}