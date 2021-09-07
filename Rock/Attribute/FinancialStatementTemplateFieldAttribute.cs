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

using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a FinancialStatementTemplate
    /// Stored as FinancialStatementTemplate.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class FinancialStatementTemplateFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialStatementTemplateFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FinancialStatementTemplateFieldAttribute( string name ) : base( name )
        {
        }

        /// <summary>
        /// Gets or sets the class name of the <see cref="Rock.Field.IFieldType" /> to be used for the attribute.
        /// </summary>
        /// <value>
        /// The field type class.
        /// </value>
        public override string FieldTypeClass
        {
            get => typeof( FinancialStatementTemplateFieldType ).FullName;
            set => base.FieldTypeClass = typeof( FinancialStatementTemplateFieldType ).FullName;
        }
    }
}