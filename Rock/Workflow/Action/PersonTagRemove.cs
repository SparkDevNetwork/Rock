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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [ActionCategory( "Tags" )]
    [Description( "Removes a person from an organization tag." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Tag Remove" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to remove from the tag.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [TextField( "Organization Tag", "The organization tag to remove the person from. <span class='tip tip-lava'></span>" )]
    public class PersonTagRemove : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            
            // get the tag
            string tagName = GetAttributeValue( action, "OrganizationTag" ).ResolveMergeFields( GetMergeFields( action ) ); ;
            if (!string.IsNullOrEmpty(tagName)) {

                // get person entity type
                var personEntityType = Rock.Web.Cache.EntityTypeCache.Read("Rock.Model.Person");

                // get tag
                TagService tagService = new TagService( rockContext );
                Tag orgTag = tagService.Queryable().Where( t => t.Name == tagName && t.EntityTypeId == personEntityType.Id && t.OwnerPersonAlias == null ).FirstOrDefault();

                if ( orgTag != null )
                {
                    // get person
                    string value = GetAttributeValue( action, "Person" );
                    Guid guidPersonAttribute = value.AsGuid();
                    if ( !guidPersonAttribute.IsEmpty() )
                    {
                        var attributePerson = AttributeCache.Read( guidPersonAttribute, rockContext );
                        if ( attributePerson != null )
                        {
                            string attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute );
                            if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                            {
                                if ( attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                                {
                                    Guid personAliasGuid = attributePersonValue.AsGuid();
                                    if ( !personAliasGuid.IsEmpty() )
                                    {
                                        var person = new PersonAliasService( rockContext ).Queryable()
                                            .Where( a => a.Guid.Equals( personAliasGuid ) )
                                            .Select( a => a.Person )
                                            .FirstOrDefault();
                                        if ( person != null )
                                        {
                                            var personAliasGuids = person.Aliases.Select( a => a.AliasPersonGuid ).ToList(); 
                                            TaggedItemService tiService = new TaggedItemService( rockContext );
                                            TaggedItem personTag = tiService.Queryable().Where( t => t.TagId == orgTag.Id && personAliasGuids.Contains( t.EntityGuid ) ).FirstOrDefault();
                                            if ( personTag != null )
                                            {
                                                tiService.Delete( personTag );
                                                rockContext.SaveChanges();
                                            }
                                            else
                                            {
                                                action.AddLogEntry( string.Format( "{0} was not in the {1} tag.", person.FullName, orgTag.Name ) );
                                            }
                                        }
                                        else
                                        {
                                            errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    action.AddLogEntry( string.Format( "{0} organization tag does not exist.", orgTag.Name ) );
                }
            } else {
                errorMessages.Add("No organization tag was provided");
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}