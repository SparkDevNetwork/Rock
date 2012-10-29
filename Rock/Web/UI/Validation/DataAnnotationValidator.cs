//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;

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
                // get the type that we are going to validate
                Type source = GetValidatedType();

                // get the property to validate
                PropertyInfo property = GetValidatedProperty( source );

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
            // get the type that we are going to validate
            Type source = GetValidatedType();

            // get the property to validate
            PropertyInfo property = GetValidatedProperty( source );

            // get the control validation value
            string value = GetControlValidationValue( ControlToValidate );

            if ( ValueMustBeInteger )
            {
                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    int intValue;
                    if ( !int.TryParse( value, out intValue ) )
                    {
                        ErrorMessage = "Value must be an integer";
                        return false;
                    }
                    else
                    {
                        if ( intValue < 0 )
                        {
                            ErrorMessage = "Value cannot be negative";
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
                            ErrorMessage = "Max length is " + ( attribute as MaxLengthAttribute ).Length.ToString();
                        }
                        else if ( attribute is RequiredAttribute )
                        {
                            ErrorMessage = "Value is required";
                        }
                        else
                        {
                            ErrorMessage = "Invalid value";
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
                // get the type that we are going to validate
                Type source = GetValidatedType();

                // get the property to validate
                PropertyInfo property = GetValidatedProperty( source );

                return ( property.PropertyType.IsEquivalentTo( typeof( int? ) ) || property.PropertyType.IsEquivalentTo( typeof( int ) ) );
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

        private PropertyInfo GetValidatedProperty( Type source )
        {
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
