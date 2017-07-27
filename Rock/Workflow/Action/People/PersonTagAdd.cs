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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Adds person to organization tag
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Adds a person to an organization tag." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Tag Add" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to add to the tag.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [TextField( "Organization Tag", "The organization tag to add the person to. If the tag does not exists it will be created. <span class='tip tip-lava'></span>" )]
    public class PersonTagAdd : ActionComponent
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
            string tagName = GetAttributeValue( action, "OrganizationTag" ).ResolveMergeFields( GetMergeFields( action ) );
            if (!string.IsNullOrEmpty(tagName)) {

                // get person entity type
                var personEntityType = Rock.Web.Cache.EntityTypeCache.Read("Rock.Model.Person");

                // get tag
                TagService tagService = new TagService( rockContext );
                Tag orgTag = tagService.Queryable().Where( t => t.Name == tagName && t.EntityTypeId == personEntityType.Id && t.OwnerPersonAlias == null ).FirstOrDefault();

                if ( orgTag == null )
                {
                    // add tag first
                    orgTag = new Tag();
                    orgTag.Name = tagName;
                    orgTag.EntityTypeQualifierColumn = string.Empty;
                    orgTag.EntityTypeQualifierValue = string.Empty;
                    
                    orgTag.EntityTypeId = personEntityType.Id;
                    tagService.Add( orgTag );
                    rockContext.SaveChanges();

                    // new up a list of items for later count
                    orgTag.TaggedItems = new List<TaggedItem>();
                }

                // get the person and add them to the tag
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
                                        // add person to tag if they are not already in it
                                        if ( orgTag.TaggedItems.Where( i => i.EntityGuid == person.PrimaryAlias.AliasPersonGuid && i.TagId == orgTag.Id ).Count() == 0 )
                                        {
                                            TaggedItem taggedPerson = new TaggedItem();
                                            taggedPerson.Tag = orgTag;
                                            taggedPerson.EntityGuid = person.PrimaryAlias.AliasPersonGuid;
                                            orgTag.TaggedItems.Add( taggedPerson );
                                            rockContext.SaveChanges();
                                        }
                                        else
                                        {
                                            action.AddLogEntry( string.Format( "{0} already tagged with {1}", person.FullName, orgTag.Name ) );
                                        }

                                        return true;
                                    }
                                    else
                                    {
                                        errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                                    }
                                }
                            }
                            else
                            {
                                errorMessages.Add( "The attribute used to provide the person was not of type 'Person'." );
                            }
                        }
                    }
                }
            } else {
                errorMessages.Add("No organization tag was provided");
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}