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
using Rock.Field.Types;
using Rock.Model;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a <see cref="SignatureDocumentTemplate" />. Stored as the SignatureDocumentTemplate's Guid.
    /// </summary>
    public class SignatureDocumentTemplateFieldAttribute : FieldAttribute
    {
        private const string SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS = "SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS";


        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureDocumentTemplateFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SignatureDocumentTemplateFieldAttribute( string name ) : base( name )
        {
            FieldTypeClass = typeof( SignatureDocumentTemplateFieldType ).FullName;
        }

        /// <summary>
        /// This should normally be false. If set to true, templates that have external providers will be shown instead of
        /// ones that use Rock eSignatures.
        /// </summary>
        /// <value><c>true</c> if [show template that have external providers]; otherwise, <c>false</c>.</value>
        public bool ShowTemplatesThatHaveExternalProviders
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}
