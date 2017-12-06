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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace com.centralaz.Workflow.Action.Groups
{
    /// <summary>
    /// Gets a child group of the configured parent group that matches the given campus attribute.
    /// </summary>
    [ActionCategory( "com_centralaz: Person" )]
    [Description( "Sets a given workflow attribute of type Address to a given Person's address." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Get Child Group for Campus" )]

    [WorkflowAttribute( "Person Attribute", "The Person attribute that the address will be taken from.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 12 )]

    [WorkflowAttribute( "Address Attribute", "The attribute that will be set.", true, "", "", 2, "Address",
        new string[] { "Rock.Field.Types.AddressFieldType" } )]

    public class SetAddressFromPerson : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            Location address = null;
            Person person = null;

            // get the person attribute
            PersonAlias personAlias = null;
            var attributeGuid = GetAttributeValue( action, "PersonAttribute" ).AsGuid();
            Guid personAliasGuid = action.GetWorklowAttributeValue( attributeGuid ).AsGuid();
            personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid );
                          
            if ( personAlias != null )
            {
                person = personAlias.Person;
                if ( person == null )
                {
                    errorMessages.Add( "The person provided does not exist." );
                    return false;
                }
            }
            else
            {
                errorMessages.Add( "The PersonAlias provided does not exist." );
                return false;
            }          

            //Get specified Address
            Guid addressTypeGuid = Guid.Empty;
            if ( !Guid.TryParse( GetAttributeValue( action, "AddressType" ), out addressTypeGuid ) )
            {
                addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            }

            var addressType = DefinedValueCache.Read( addressTypeGuid );
            if ( addressType != null )
            {
                var groupLocation = new PersonService( new RockContext() ).GetFirstLocation( person.Id, addressType.Id );
                if ( groupLocation != null )
                {
                    address = groupLocation.Location;
                }
            }

            if ( address != null )
            {
                // get the address attribute where we'll store the person's address.
                var addressAttributeGuid = GetAttributeValue( action, "Address" ).AsGuid();
                var addressAttribute = AttributeCache.Read( addressAttributeGuid, rockContext );
                if ( addressAttribute != null )
                {
                    SetWorkflowAttributeValue( action, addressAttribute.Guid, address.Guid.ToStringSafe() );
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", addressAttribute.Name, address.Guid ) );
                }
                else
                {
                    errorMessages.Add( "Invalid address attribute provided." );
                    return false;
                }
            }
            else
            {
                action.AddLogEntry( string.Format( "An address of type {0} for {1} could not be found.", addressType.Value, person.FullName ) );
            }

            return true;
        }
    }
}