using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.UI.WebControls;

// enable client side validation using jQuery see
// http://www.mourfield.com/Article/70/using-the-jquery-validation-plugin-to-build-a-data-annotations-aspnet-validator

namespace Rock.Validation
{
    //[ToolboxData("<{0}:DataAnnotationValidator runat="server"></{0}:DataAnnotationValidator>")]
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

        #endregion

        #region Methods

        protected override bool EvaluateIsValid()
        {
            // get the type that we are going to validate
            Type source = GetValidatedType();

            // get the property to validate
            PropertyInfo property = GetValidatedProperty( source );

            // get the control validation value
            string value = GetControlValidationValue( ControlToValidate );

            foreach ( var attribute in property.GetCustomAttributes(
                     typeof( ValidationAttribute ), true )
                       .OfType<ValidationAttribute>() )
            {
                if ( !attribute.IsValid( value ) )
                {
                    if ( attribute.ErrorMessage != null )
                        ErrorMessage = attribute.ErrorMessage;
                    else if ( attribute.ErrorMessageResourceType != null )
                        ErrorMessage = new System.Resources.ResourceManager( attribute.ErrorMessageResourceType ).GetString( attribute.ErrorMessageResourceName );

                    return false;
                }
            }
            return true;
        }

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
