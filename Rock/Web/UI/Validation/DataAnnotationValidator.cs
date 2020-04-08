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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.UI.Controls;

// enable client side validation using jQuery see
// http://www.mourfield.com/Article/70/using-the-jquery-validation-plugin-to-build-a-data-annotations-aspnet-validator

namespace Rock.Web.UI.Validation
{
    //[ToolboxData("<{0}:DataAnnotationValidator runat="server"></{0}:DataAnnotationValidator>")]
    /// <summary>
    /// Data Annotation Validator for validating based on data attributes
    /// </summary>
    public class DataAnnotationValidator : BaseValidator
    {
        #region Properties

        /// <summary>
        /// The type of the source to check
        /// </summary>
        public string SourceTypeName { get; set; }

        /// <summary>
        /// The property that is annotated
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DataAnnotationValidator"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired
        {
            get
            {
                // get the property to validate
                PropertyInfo property = GetValidatedProperty();

                var attributes = property.GetCustomAttributes( typeof( RequiredAttribute ), true ).OfType<RequiredAttribute>();

                return attributes.Count() > 0;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// When overridden in a derived class, this method contains the code to determine whether the value in the input control is valid.
        /// </summary>
        /// <returns>
        /// true if the value in the input control is valid; otherwise, false.
        /// </returns>
        protected override bool EvaluateIsValid()
        {
            // get the property to validate
            PropertyInfo property = GetValidatedProperty();

            // get the control validation value
            string value = GetControlValidationValue( ControlToValidate );

            string propertyLabelText = null;
            
            Control control = FindControl( ControlToValidate );
            if ( control != null )
            {
                if ( control is IRockControl )
                {
                    propertyLabelText = ( control as IRockControl ).Label;
                }
            }

            if ( string.IsNullOrWhiteSpace( propertyLabelText ) )
            {
                propertyLabelText = PropertyName;
            }

            if ( ValueMustBeInteger )
            {
                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    int intValue;
                    if ( !int.TryParse( value, out intValue ) )
                    {
                        ErrorMessage = string.Format( "{0} must be an whole number", propertyLabelText );
                        return false;
                    }
                    else
                    {
                        if ( intValue < 0 )
                        {
                            ErrorMessage = string.Format( "{0} cannot be negative", propertyLabelText );
                            return false;
                        }
                    }
                }
            }

            foreach ( var attribute in property.GetCustomAttributes(
                     typeof( ValidationAttribute ), true )
                       .OfType<ValidationAttribute>() )
            {
                if ( !attribute.IsValid( value ) )
                {
                    if ( !string.IsNullOrWhiteSpace( attribute.ErrorMessage ) )
                    {
                        ErrorMessage = attribute.ErrorMessage;
                    }
                    else if ( attribute.ErrorMessageResourceType != null )
                    {
                        ErrorMessage = new System.Resources.ResourceManager( attribute.ErrorMessageResourceType ).GetString( attribute.ErrorMessageResourceName );
                    }
                    else
                    {
                        if ( attribute is MaxLengthAttribute )
                        {
                            ErrorMessage = string.Format( "{0} can't be longer than {1}", propertyLabelText, ( attribute as MaxLengthAttribute ).Length.ToString() );
                        }
                        else if ( attribute is RequiredAttribute )
                        {
                            ErrorMessage = string.Format( "A value for {0} is required", propertyLabelText );
                        }
                        else
                        {
                            ErrorMessage = string.Format( "Invalid value for {0}", propertyLabelText );
                        }
                    }

                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether [value must be integer].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [value must be integer]; otherwise, <c>false</c>.
        /// </value>
        public bool ValueMustBeInteger
        {
            get
            {
                // get the property to validate
                PropertyInfo property = GetValidatedProperty();

                return ( property.PropertyType.IsEquivalentTo( typeof( int? ) ) || property.PropertyType.IsEquivalentTo( typeof( int ) ) );
            }
        }

        /// <summary>
        /// Gets the maxlength of the value.
        /// </summary>
        /// <value>
        /// The length of the value.
        /// </value>
        public int ValueMaxLength
        {
            get
            {
                PropertyInfo pi = GetValidatedProperty();
                MaxLengthAttribute maxLengthAttribute = pi.GetCustomAttribute<MaxLengthAttribute>( true );
                if ( maxLengthAttribute != null )
                {
                    return maxLengthAttribute.Length;
                }
                else
                {
                    if ( ValueMustBeInteger )
                    {
                        return 10;
                    }
                    else
                    {
                        // 0 means not set (no limit)
                        return 0;
                    }
                }

            }
        }

        /// <summary>
        /// Gets the type of the validated.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Null SourceTypeName can't be validated</exception>
        private Type GetValidatedType()
        {
            if ( string.IsNullOrEmpty( SourceTypeName ) )
            {
                throw new InvalidOperationException(
                  "Null SourceTypeName can't be validated" );
            }

            Type validatedType = Type.GetType( SourceTypeName );
            if ( validatedType == null )
            {
                throw new InvalidOperationException(
                    string.Format( "{0}:{1}",
                      "Invalid SourceTypeName", SourceTypeName ) );
            }

            return validatedType;
        }

        /// <summary>
        /// Gets the validated property.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        private PropertyInfo GetValidatedProperty()
        {
            Type source = GetValidatedType();
            PropertyInfo property = source.GetProperty( PropertyName,
              BindingFlags.Public | BindingFlags.Instance );

            if ( property == null )
            {
                throw new InvalidOperationException(
                  string.Format( "{0}:{1}",
                    "Validated Property Does Not Exists", PropertyName ) );
            }
            return property;
        }

        #endregion
    }
}
