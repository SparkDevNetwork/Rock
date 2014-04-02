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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Data;
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
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                Guid guid = value.AsGuid();
                formattedValue = new PersonAliasService( new RockContext() ).Queryable()
                    .Where( a => a.Guid.Equals( guid ) )
                    .Select( a => a.Person.NickName + " " + a.Person.LastName )
                    .FirstOrDefault();
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new PersonPicker { ID = id }; 
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

                if ( personId.HasValue )
                {
                    var rockContext = new RockContext();

                    var personAliasService = new PersonAliasService( rockContext );
                    var personAlias = personAliasService.Queryable()
                        .Where( a => a.AliasPersonId == personId )
                        .FirstOrDefault();
                    if ( personAlias != null )
                    {
                        result = personAlias.Guid.ToString();
                    }
                    else
                    {
                        // If the personId is valid, there should be a personAlias with the AliasPersonID equal 
                        // to that personId.  If there isn't for some reason, create it now...
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            personAlias = new PersonAlias();
                            personAlias.Guid = Guid.NewGuid();
                            personAlias.AliasPersonId = person.Id;
                            personAlias.AliasPersonGuid = person.Guid;
                            personAlias.PersonId = person.Id;
                            result = personAlias.Guid.ToString();

                            personAliasService.Add( personAlias );
                            rockContext.SaveChanges();
                        }
                    }
                }
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
                    Guid guid = Guid.Empty;
                    Guid.TryParse( value, out guid );

                    var person = new PersonAliasService( new RockContext() ).Queryable()
                        .Where( a => a.Guid.Equals(guid))
                        .Select( a => a.Person)
                        .FirstOrDefault();
                    ppPerson.SetValue( person );
                }
            }
        }
    }
}