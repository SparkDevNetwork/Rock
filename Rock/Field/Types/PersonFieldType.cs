//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a person
    /// </summary>
    [Serializable]
    public class PersonFieldType : FieldType
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var service = new PersonService();
                var person = service.Get( new Guid( value ) );
                if ( person != null )
                {
                    return person.FullName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            PersonPicker ppPerson = new PersonPicker();
            return ppPerson;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            PersonPicker ppPerson = control as PersonPicker;
            string result = null;

            if ( ppPerson != null )
            {
                Guid personGuid = Guid.Empty;
                int? personId = ppPerson.PersonId;

                var person = new PersonService().Get( personId ?? 0 );
                if ( person != null )
                {
                    personGuid = person.Guid;
                }

                result = personGuid.ToString();
            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                PersonPicker ppPerson = control as PersonPicker;
                if ( ppPerson != null )
                {
                    Guid personGuid = Guid.Empty;
                    Guid.TryParse( value, out personGuid );

                    var person = new PersonService().Get( personGuid );
                    ppPerson.SetValue( person );
                }
            }
        }
    }
}