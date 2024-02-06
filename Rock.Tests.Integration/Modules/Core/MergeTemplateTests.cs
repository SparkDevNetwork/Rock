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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core
{
    /// <summary>
    /// Tests for the document merge process.
    /// </summary>
    [TestClass]
    public class MergeTemplateTests : DatabaseTestsBase
    {
        [TestMethod]
        public void MergeTemplateTest_GroupAttendanceRoster_HasHeaderAndDetailElements()
        {
            // Note that the test Group Name includes a reserved XML character to verify
            // that encoding is correctly handled in the final output.
            var testGroupName = "Pete's Group";
            var rockContext = new RockContext();

            // Create an Entity Set containing the Group Members of Pete's Group.
            var groupService = new GroupService( rockContext );
            var group = groupService.Queryable().GetByIdentifier( testGroupName );

            var mergeEntitySet = new EntitySet();
            mergeEntitySet.EntityTypeId = EntityTypeCache.GetId( typeof( GroupMember ) );

            group.Members.ToList()
                .ForEach( gm => mergeEntitySet.Items.Add( new EntitySetItem { EntityId = gm.Id } ) );

            var entitySetService = new EntitySetService( rockContext );
            entitySetService.Add( mergeEntitySet );

            rockContext.SaveChanges();

            // Build the merge data source.
            var builder = new MergeTemplateDataSourceBuilder();
            var mergeDataResult = builder.GetMergeObjectsFromEntitySet( mergeEntitySet.Id );

            // Merge the data source with the Group Attendance Roster template.
            var mergeTemplateService = new MergeTemplateService( rockContext );
            var mergeTemplate = mergeTemplateService.Queryable().GetByIdentifier( "Group Attendance Roster" );
            var mergeTemplateType = mergeTemplate.GetMergeTemplateType();

            var globalFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            foreach ( var kv in mergeDataResult.GlobalMergeObjects )
            {
                globalFields.AddOrIgnore( kv.Key, kv.Value );
            }

            // Get the merge output as an XML Document and validate the content.
            var wordTemplate = mergeTemplateType as WordDocumentMergeTemplateType;
            var outputDoc = wordTemplate.CreateDocumentAsXml( mergeTemplate,
                mergeDataResult.DetailMergeObjects.Values.ToList(),
                globalFields );

            Assert.IsTrue( !wordTemplate.Exceptions.Any(), "The merge failed with errors." );

            // Verify that the group name exists in the header.
            var nameNode = outputDoc.Descendants().Where( n => n.Name.LocalName == "t" && n.Value == testGroupName ).FirstOrDefault();
            Assert.IsNotNull( nameNode, "Expected Header element '{{ Group.Name }}' not found." );
            var headerNode = nameNode.Ancestors().Where( n => n.Name.LocalName == "hdr" ).FirstOrDefault();
            Assert.IsNotNull( headerNode, "Expected Header element '{{ Group.Name }}' not found." );

            // Verify that the group member names exist.
            foreach ( var gm in group.Members )
            {
                var name = $"{gm.Person.LastName}, {gm.Person.NickName}";
                var detailNodes = outputDoc.Descendants().Where( n => n.Name.LocalName == "t" && n.Value.Trim() == name );
                Assert.IsTrue( detailNodes.Any(), $"Expected Detail element {{{{ Row.PersonName }}}} not found. [Text='{name}']" );
            }
        }
    }

    /*
     * The following code region is duplicated from the MergeTemplateEntry.ascx block.
     * It exists here for testing purposes, and any changes made to this code during testing should be
     * copied to the block. In the future, this component will be moved to the Rock core assembly.
     */

    #region MergeTemplateDataSourceBuilder

    /// <summary>
    /// Builds a data source that can be combined with a Merge Template to produce final output.
    /// </summary>
    /// <remarks>
    /// Last Modified: DJL-2023-05-17
    /// </remarks>
    internal class MergeTemplateDataSourceBuilder
    {
        /// <summary>
        /// Gets the merge object list for the current EntitySet
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="fetchCount">The fetch count.</param>
        /// <returns></returns>
        public GetMergeObjectsResult GetMergeObjectsFromEntitySet( int entitySetId, bool combineFamilyMembers = false, int? fetchCount = null, int? databaseTimeoutSeconds = null )
        {
            var rockContext = new RockContext();
            if ( databaseTimeoutSeconds != null && databaseTimeoutSeconds.Value > 0 )
            {
                rockContext.Database.CommandTimeout = databaseTimeoutSeconds.Value;
            }

            var entitySetService = new EntitySetService( rockContext );
            var entitySet = entitySetService.Get( entitySetId );

            var result = new GetMergeObjectsResult();
            var mergeObjectsDictionary = new Dictionary<int, object>();
            var globalObjectDictionary = new Dictionary<string, object>();

            // If this EntitySet contains IEntity Items, add those first
            if ( entitySet.EntityTypeId.HasValue )
            {
                var qryEntity = entitySetService.GetEntityQuery( entitySetId );

                if ( fetchCount.HasValue )
                {
                    qryEntity = qryEntity.Take( fetchCount.Value );
                }

                var entityTypeCache = EntityTypeCache.Get( entitySet.EntityTypeId.Value );
                bool isPersonEntityType = entityTypeCache != null && entityTypeCache.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid();
                bool isGroupMemberEntityType = entityTypeCache != null && entityTypeCache.Guid == Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid();

                // Add parent items to the the global merge objects list.
                if ( isGroupMemberEntityType )
                {
                    var groups = qryEntity.OfType<GroupMember>().Select( a => a.Group ).DistinctBy( gm => gm.Id ).ToList();

                    globalObjectDictionary.AddOrReplace( "Groups", groups );
                    // Add the first entry as a singleton reference for convenience.
                    globalObjectDictionary.AddOrReplace( "Group", groups.FirstOrDefault() );
                }

                if ( ( isGroupMemberEntityType || isPersonEntityType ) && combineFamilyMembers )
                {
                    IQueryable<IEntity> qryPersons;
                    if ( isGroupMemberEntityType )
                    {
                        qryPersons = qryEntity.OfType<GroupMember>().Select( a => a.Person );
                    }
                    else
                    {
                        qryPersons = qryEntity;
                    }

                    Guid familyGroupType = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

                    // Create a query for the set of person Ids.
                    // Avoid using ToList() here - for large result sets, the materialized list may cause an overflow when used to filter subsequent queries.
                    var qryPersonIds = qryPersons.Select( a => a.Id );

                    if ( isGroupMemberEntityType )
                    {
                        qryPersons = qryPersons.Distinct();
                    }

                    var qryFamilyGroupMembers = new GroupMemberService( rockContext ).Queryable( "GroupRole,Person" ).AsNoTracking()
                        .Where( a => a.Group.GroupType.Guid == familyGroupType )
                        .Where( a => qryPersonIds.Contains( a.PersonId ) );

                    var qryCombined = qryFamilyGroupMembers.Join(
                        qryPersons,
                        m => m.PersonId,
                        p => p.Id,
                        ( m, p ) => new { GroupMember = m, Person = p } )
                        .GroupBy( a => a.GroupMember.GroupId )
                        .Select( x => new
                        {
                            GroupId = x.Key,
                            // Order People to match ordering in the GroupMembers.ascx block.
                            Persons =
                                    // Adult Male 
                                    x.Where( xx => xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                    xx.GroupMember.Person.Gender == Gender.Male ).OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person )
                                    // Adult Female
                                    .Concat( x.Where( xx => xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                    xx.GroupMember.Person.Gender != Gender.Male ).OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person ) )
                                    // non-adults
                                    .Concat( x.Where( xx => !xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                                    .OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person ) )
                        } );

                    foreach ( var combinedFamilyItem in qryCombined )
                    {
                        object mergeObject;

                        var personIds = combinedFamilyItem.Persons.Select( a => a.Id ).Distinct().ToArray();

                        var primaryGroupPerson = combinedFamilyItem.Persons.FirstOrDefault() as Person;

                        if ( mergeObjectsDictionary.ContainsKey( primaryGroupPerson.Id ) )
                        {
                            foreach ( var person in combinedFamilyItem.Persons )
                            {
                                if ( !mergeObjectsDictionary.ContainsKey( person.Id ) )
                                {
                                    primaryGroupPerson = person as Person;
                                    break;
                                }
                            }
                        }

                        // if we are combining from a GroupMemberEntityType list add the GroupMember attributes of the primary person in the combined list
                        if ( isGroupMemberEntityType )
                        {
                            var groupMember = qryEntity.OfType<GroupMember>().Where( a => a.PersonId == primaryGroupPerson.Id ).FirstOrDefault();
                            primaryGroupPerson.AdditionalLavaFields = primaryGroupPerson.AdditionalLavaFields ?? new Dictionary<string, object>();
                            if ( groupMember != null )
                            {
                                primaryGroupPerson.AdditionalLavaFields.AddOrIgnore( "GroupMember", groupMember );
                            }
                        }

                        if ( combinedFamilyItem.Persons.Count() > 1 )
                        {
                            var combinedPerson = primaryGroupPerson.ToJson().FromJsonOrNull<MergeTemplateCombinedPerson>();

                            var familyTitle = Person.CalculateFamilySalutation( primaryGroupPerson, new Person.CalculateFamilySalutationArgs( true ) { LimitToPersonIds = personIds, RockContext = rockContext } );
                            combinedPerson.FullName = familyTitle;

                            var firstNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).FirstName ).ToList();
                            var nickNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).NickName ).ToList();

                            combinedPerson.FirstName = firstNameList.AsDelimited( ", ", " & " );
                            combinedPerson.NickName = nickNameList.AsDelimited( ", ", " & " );
                            combinedPerson.LastName = primaryGroupPerson.LastName;
                            combinedPerson.SuffixValueId = null;
                            combinedPerson.SuffixValue = null;
                            mergeObject = combinedPerson;
                        }
                        else
                        {
                            mergeObject = primaryGroupPerson;
                        }

                        mergeObjectsDictionary.AddOrIgnore( primaryGroupPerson.Id, mergeObject );
                    }

                    // Add the records to the merge dictionary, preserving the selection order.
                    var orderedPersonIdList = qryPersonIds.ToList();

                    mergeObjectsDictionary = mergeObjectsDictionary.OrderBy( a => orderedPersonIdList.IndexOf( a.Key ) ).ToDictionary( x => x.Key, y => y.Value );
                }
                else if ( isGroupMemberEntityType )
                {
                    List<int> personIds = new List<int>();

                    foreach ( var groupMember in qryEntity.AsNoTracking().OfType<GroupMember>() )
                    {
                        var person = groupMember.Person;
                        if ( !personIds.Contains( person.Id ) )
                        {
                            // Attach the person record to rockContext so that navigation properties can be still lazy-loaded if needed (if the lava template needs it)
                            rockContext.People.Attach( person );
                        }

                        person.AdditionalLavaFields = new Dictionary<string, object>();
                        person.AdditionalLavaFields.Add( "GroupMember", groupMember );
                        mergeObjectsDictionary.AddOrIgnore( groupMember.PersonId, person );
                        personIds.Add( person.Id );
                    }
                }
                else
                {
                    foreach ( var item in qryEntity.AsNoTracking() )
                    {
                        mergeObjectsDictionary.AddOrIgnore( item.Id, item );
                    }
                }
            }

            var entitySetItemService = new EntitySetItemService( rockContext );
            string[] emptyJson = new string[] { string.Empty, "{}" };
            var entitySetItemMergeValuesQry = entitySetItemService.GetByEntitySetId( entitySetId, true ).Where( a => !emptyJson.Contains( a.AdditionalMergeValuesJson ) );

            if ( fetchCount.HasValue )
            {
                entitySetItemMergeValuesQry = entitySetItemMergeValuesQry.Take( fetchCount.Value );
            }

            // the entityId to use for NonEntity objects
            int nonEntityId = 1;

            // now, add the additional MergeValues regardless of if the EntitySet contains IEntity items or just Non-IEntity items
            foreach ( var additionalMergeValuesItem in entitySetItemMergeValuesQry.AsNoTracking() )
            {
                object mergeObject;
                int entityId;
                if ( additionalMergeValuesItem.EntityId > 0 )
                {
                    entityId = additionalMergeValuesItem.EntityId;
                }
                else
                {
                    // not pointing to an actual EntityId, so use the nonEntityId for ti
                    entityId = nonEntityId++;
                }

                if ( mergeObjectsDictionary.ContainsKey( entityId ) )
                {
                    mergeObject = mergeObjectsDictionary[entityId];
                }
                else
                {
                    if ( entitySet.EntityTypeId.HasValue )
                    {
                        // if already have real entities in our list, don't add additional items to the mergeObjectsDictionary
                        continue;
                    }

                    // non-Entity merge object, so just use Dictionary
                    mergeObject = new Dictionary<string, object>();
                    mergeObjectsDictionary.AddOrIgnore( entityId, mergeObject );
                }

                foreach ( var additionalMergeValue in additionalMergeValuesItem.AdditionalMergeValues )
                {
                    if ( mergeObject is IEntity )
                    {
                        // add the additional fields to AdditionalLavaFields
                        IEntity mergeEntity = ( mergeObject as IEntity );
                        mergeEntity.AdditionalLavaFields = mergeEntity.AdditionalLavaFields ?? new Dictionary<string, object>();
                        object mergeValueObject = additionalMergeValue.Value;

                        // if the mergeValueObject is a JArray (JSON Object), convert it into an ExpandoObject or List<ExpandoObject> so that Lava will work on it
                        if ( mergeValueObject is JArray )
                        {
                            var jsonOfObject = mergeValueObject.ToJson();
                            try
                            {
                                mergeValueObject = Rock.Lava.RockFilters.FromJSON( jsonOfObject );
                            }
                            catch ( Exception ex )
                            {
                                result.Error = new Exception( "MergeTemplateEntry couldn't do a FromJSON", ex );
                            }
                        }

                        mergeEntity.AdditionalLavaFields.AddOrIgnore( additionalMergeValue.Key, mergeValueObject );
                    }
                    else if ( mergeObject is IDictionary<string, object> )
                    {
                        // anonymous object with no fields yet
                        IDictionary<string, object> nonEntityObject = mergeObject as IDictionary<string, object>;
                        nonEntityObject.AddOrIgnore( additionalMergeValue.Key, additionalMergeValue.Value );
                    }
                    else
                    {
                        result.Error = new Exception( string.Format( "Unexpected MergeObject Type: {0}", mergeObject ) );
                        return result;
                    }
                }
            }

            result.GlobalMergeObjects = globalObjectDictionary;

            Dictionary<string, object> detailObjectsDictionary;
            if ( fetchCount.HasValue )
            {
                // make sure the result is limited to fetchCount (even though the above queries are also limited to fetch count)
                detailObjectsDictionary = mergeObjectsDictionary
                    .Take( fetchCount.Value )
                    .ToDictionary( k => k.Key.ToString(), v => v.Value );
            }
            else
            {
                detailObjectsDictionary = mergeObjectsDictionary
                    .ToDictionary( k => k.Key.ToString(), v => v.Value );
            }

            result.DetailMergeObjects = detailObjectsDictionary;

            return result;
        }

        #region Support Classes

        /// <summary>
        /// The result of the GetMergeObjects action.
        /// </summary>
        public class GetMergeObjectsResult
        {
            public Dictionary<string, object> GlobalMergeObjects;
            public Dictionary<string, object> DetailMergeObjects;
            public Exception Error;
        }

        /// <summary>
        /// Special class that overrides Person so that FullName can be set (vs readonly/derived)
        /// The class is specifically for MergeTemplates
        /// </summary>
        public class MergeTemplateCombinedPerson : Person
        {
            /// <summary>
            /// Override of FullName that should be set to whatever the FamilyTitle should be
            /// </summary>
            /// <value>
            /// A <see cref="System.String" /> representing the Family Title of a combined person
            /// </value>
            [DataMember]
            public new string FullName { get; set; }
        }

        #endregion
    }

    #endregion
}
