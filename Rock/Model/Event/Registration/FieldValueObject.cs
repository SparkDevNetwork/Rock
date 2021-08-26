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

using System;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Newtonsoft.Json.JsonConverter( typeof( FieldValueConverter ) )]
    public class FieldValueObject
    {
        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public RegistrationFieldSource FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        public RegistrationPersonFieldType PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets the field value.
        /// </summary>
        /// <value>
        /// The field value.
        /// </value>
        public object FieldValue { get; set; }

        /// <summary>
        /// Gets the type of the field value.
        /// </summary>
        /// <value>
        /// The type of the field value.
        /// </value>
        public Type FieldValueType
        {
            get
            {
                Type valueType = typeof( string );
                if ( FieldSource == RegistrationFieldSource.PersonField )
                {
                    switch ( PersonFieldType )
                    {
                        case RegistrationPersonFieldType.Campus:
                        case RegistrationPersonFieldType.MaritalStatus:
                        case RegistrationPersonFieldType.Grade:
                        case RegistrationPersonFieldType.ConnectionStatus:
                            return typeof( int? );

                        case RegistrationPersonFieldType.Address:
                            return typeof( Location );

                        case RegistrationPersonFieldType.Birthdate:
                            return typeof( DateTime? );

                        case RegistrationPersonFieldType.Gender:
                            return typeof( Gender );

                        case RegistrationPersonFieldType.MobilePhone:
                        case RegistrationPersonFieldType.HomePhone:
                        case RegistrationPersonFieldType.WorkPhone:
                            return typeof( PhoneNumber );
                    }
                }

                return typeof( string );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueObject"/> class.
        /// </summary>
        public FieldValueObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueObject"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="fieldValue">The field value.</param>
        public FieldValueObject( RegistrationTemplateFormField field, object fieldValue )
        {
            FieldSource = field.FieldSource;
            PersonFieldType = field.PersonFieldType;
            FieldValue = fieldValue;
        }
    }
}
