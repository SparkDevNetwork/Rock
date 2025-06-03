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
using Rock.Data;
using Rock.Web.Cache;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System;
using Rock.AI.Agent;
using System.Reflection;
using Rock.SystemGuid;
using Rock.Observability;

namespace Rock.Model
{
    public partial class AISkillService
    {
        public static void RegisterSkills()
        {
            using ( ObservabilityHelper.StartActivity( "Register AI Skills" ) )
            {
                var skillTypes = Reflection.FindTypes( typeof( IAgentSkill ) )
                    .Select( t => t.Value )
                    .Where( t => t.IsClass
                        && !t.IsAbstract
                        && t.GetCustomAttribute<AgentSkillGuidAttribute>() != null )
                    .ToList();

                foreach ( var skillType in skillTypes )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        RegisterSkillType( skillType, rockContext );
                    }
                }
            }
        }

        private static void RegisterSkillType( Type skillType, RockContext rockContext )
        {
            //var skillService = new AISkillService( rockContext );

            //var entityTypeService = new EntityTypeService( rockContext );
            //var entityType = entityTypeService.Get( skillType, true, null );

            //    var reflectedTypeNames = reflectedTypes.Select( a => a.FullName ).ToArray();
            //    var reflectedTypeGuids = entityTypesFromReflection.Values.Select( a => a.Guid ).ToArray();

            //    var duplicateGuids = reflectedTypeGuids
            //        .GroupBy( g => g )
            //        .Where( g => g.Count() > 1 )
            //        .Select( g => g.Key.ToString() )
            //        .ToList();

            //    // Throw an error if duplicate guids were detected in the code.
            //    // This will intentionally prevent Rock from starting so the
            //    // developer can quickly find and fix the error.
            //    if ( duplicateGuids.Any() )
            //    {
            //        throw new Exception( $"Duplicate EntityType system guids detected: {duplicateGuids.JoinStrings( ", " )}" );
            //    }

            //    // Get all the EntityType records from the Database without filtering them (we'll have to deal with them all)
            //    // Then we'll split them into a list of ones that don't exist and ones that still exist
            //    var entityTypeInDatabaseList = entityTypeService.Queryable().ToList();

            //    // Find any existing self-discovered EntityType records that no longer exist in reflectedTypes
            //    // but have C# narrow it down to ones that aren't in the reflectedTypeNames list
            //    var reflectedEntityTypesThatNoLongerExist = entityTypeInDatabaseList
            //        .Where( e => !string.IsNullOrEmpty( e.AssemblyName ) )
            //        .ToList()
            //        .Where( e => !reflectedTypeNames.Contains( e.Name ) && !reflectedTypeGuids.Contains( e.Guid ) )
            //        .OrderBy( a => a.Name )
            //        .ToList();

            //    // clean up entitytypes that don't have an Assembly, but somehow have IsEntity or IsSecured set
            //    foreach ( var entityTypesWithoutAssembliesButIsEntity in entityTypeInDatabaseList.Where( e => string.IsNullOrEmpty( e.AssemblyName ) && ( e.IsEntity || e.IsSecured ) ) )
            //    {
            //        if ( entityTypesWithoutAssembliesButIsEntity.AssemblyName.IsNullOrWhiteSpace() )
            //        {
            //            entityTypesWithoutAssembliesButIsEntity.IsEntity = false;
            //            entityTypesWithoutAssembliesButIsEntity.IsSecured = false;
            //        }
            //    }

            //    foreach ( var oldEntityType in reflectedEntityTypesThatNoLongerExist )
            //    {

            //        Type foundType = null;
            //        // if this isn't one of the EntityTypes that we self-register,
            //        // see if it was manually registered first (with EntityTypeCache.Get(Type type, bool createIfNotExists))
            //        try
            //        {
            //            foundType = Type.GetType( oldEntityType.AssemblyName, false );
            //        }
            //        catch
            //        {
            //            /* 2020-05-22 MDP
            //             * GetType (string typeName, bool throwOnError) can throw exceptions even if throwOnError is false!
            //             * see https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype?view=netframework-4.5.2#System_Type_GetType_System_String_System_Boolean_

            //              so, if this happens, we'll ignore any error it returns in those cases too
            //             */
            //        }

            //        if ( foundType == null )
            //        {
            //            // it was manually registered but we can't create a Type from it
            //            // so we'll update the EntityType.AssemblyName to null
            //            // and set IsSecured and IsEntity to False (since a NULL type doesn't implement ISecured or IEntity)
            //            oldEntityType.AssemblyName = null;
            //            oldEntityType.IsSecured = false;
            //            oldEntityType.IsEntity = false;
            //        }
            //    }

            //    // Now get the entityType records that are still in the list of types we found thru reflection
            //    // but we'll have C# narrow it down to ones that aren't in the reflectedTypeNames list
            //    var reflectedEntityTypesThatStillExist = entityTypeInDatabaseList
            //        .Where( e => reflectedTypeNames.Contains( e.Name ) || reflectedTypeGuids.Contains( e.Guid ) )
            //        .ToList();

            //    // Update any existing entities
            //    foreach ( var existingEntityType in reflectedEntityTypesThatStillExist )
            //    {
            //        var entityTypeFromReflection = entityTypesFromReflection.GetValueOrNull( existingEntityType.Name );
            //        if ( entityTypeFromReflection == null )
            //        {
            //            // Check if the entity type had its class name change by
            //            // seeing if we already have one with the same guid.
            //            entityTypeFromReflection = entityTypesFromReflection.Values.FirstOrDefault( e => e.Guid == existingEntityType.Guid );

            //            if ( entityTypeFromReflection == null )
            //            {
            //                continue;
            //            }
            //        }

            //        if ( existingEntityType.Name != entityTypeFromReflection.Name ||
            //            existingEntityType.IsEntity != entityTypeFromReflection.IsEntity ||
            //            existingEntityType.IsSecured != entityTypeFromReflection.IsSecured ||
            //            existingEntityType.FriendlyName != ( existingEntityType.FriendlyName ?? entityTypeFromReflection.FriendlyName ) ||
            //            existingEntityType.AssemblyName != entityTypeFromReflection.AssemblyName )
            //        {
            //            existingEntityType.Name = entityTypeFromReflection.Name;
            //            existingEntityType.IsEntity = entityTypeFromReflection.IsEntity;
            //            existingEntityType.IsSecured = entityTypeFromReflection.IsSecured;
            //            existingEntityType.FriendlyName = existingEntityType.FriendlyName ?? entityTypeFromReflection.FriendlyName;
            //            existingEntityType.AssemblyName = entityTypeFromReflection.AssemblyName;
            //        }

            //        var reflectedType = reflectedTypeLookupByName.GetValueOrNull( existingEntityType.Name );
            //        var reflectedTypeGuid = reflectedType?.GetCustomAttribute<Rock.SystemGuid.EntityTypeGuidAttribute>( inherit: false )?.Guid;
            //        if ( reflectedTypeGuid != null && reflectedTypeGuid.Value != existingEntityType.Guid )
            //        {
            //            /*
            //             * 2022-07-11 ETD
            //             * Since the GUID has changed we need to check for AttributeValues that use the old GUID and update it to the new one.
            //             * Do this on the same context since the two changes should occur at the same time.
            //             * Limit to attributes that are chosen by EntityType, Component, and Components FieldTypes since those are what store the GUIDs as attribute values.
            //             * The filtered attributes list should be less than 300, even on very large DBs.
            //             */

            //            if ( existingEntityType.Guid != null )
            //            {
            //                var attributeService = new AttributeService( rockContext );
            //                var attributeValueService = new AttributeValueService( rockContext );
            //                var entityTypeFieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.ENTITYTYPE ).Id;
            //                var componentFieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.COMPONENT ).Id;
            //                var componentsFieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.COMPONENTS ).Id;
            //                var existingEntityTypeString = existingEntityType.Guid.ToString().ToLower();
            //                var reflectedTypeGuidString = reflectedTypeGuid.Value.ToString().ToLower();

            //                var attributeIdsUsingFieldType = attributeService.Queryable()
            //                    .Where( a => a.FieldTypeId == entityTypeFieldTypeId
            //                        || a.FieldTypeId == componentFieldTypeId
            //                        || a.FieldTypeId == componentsFieldTypeId )
            //                    .Select( a => a.Id )
            //                    .ToList();

            //                rockContext.Database.CommandTimeout = 150;

            //                foreach ( var attributeIdUsingFieldType in attributeIdsUsingFieldType )
            //                {
            //                    var attributeValues = attributeValueService.Queryable().Where( av => av.AttributeId == attributeIdUsingFieldType && av.Value.Contains( existingEntityTypeString ) ).ToList();

            //                    foreach ( var attributeValue in attributeValues )
            //                    {
            //                        attributeValue.Value = attributeValue.Value.ToLower().Replace( existingEntityTypeString, reflectedTypeGuidString );
            //                    }
            //                }
            //            }

            //            // Now update the Guid of the EntityType
            //            existingEntityType.Guid = reflectedTypeGuid.Value;
            //        }

            //        entityTypesFromReflection.Remove( existingEntityType.Name );
            //    }

            //    // Add the newly discovered entities 
            //    foreach ( var entityType in entityTypesFromReflection.Values )
            //    {
            //        // Don't add the EntityType entity as it will probably have been automatically 
            //        // added by the audit on a previous save in this method.
            //        if ( entityType.Name != "Rock.Model.EntityType" )
            //        {
            //            // double check that another thread didn't add this EntityType.
            //            if ( !entityTypeService.AlreadyExists( entityType.Name ) )
            //            {
            //                entityTypeService.Add( entityType );
            //            }
            //        }
            //    }

            //    try
            //    {
            //        rockContext.SaveChanges();
            //    }
            //    catch ( Exception thrownException )
            //    {
            //        // if the exception was due to a duplicate Guid, throw as a duplicateGuidException. That'll make it easier to troubleshoot.
            //        var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, null );
            //        if ( duplicateGuidException != null )
            //        {
            //            throw duplicateGuidException;
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }

            //    // make sure the EntityTypeCache is synced up with any changes that were made
            //    foreach ( var entityTypeModel in entityTypeService.Queryable().AsNoTracking() )
            //    {
            //        EntityTypeCache.Get( entityTypeModel );
            //    }
            //}
        }
    }
}
