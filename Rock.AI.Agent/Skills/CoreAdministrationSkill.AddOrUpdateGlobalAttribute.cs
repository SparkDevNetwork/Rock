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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds a global attribute or updates an existing one, and optionally sets its
    /// current value.
    /// </summary>
    /// <remarks>
    /// The key and field type are fixed at creation. Both are required when adding
    /// and neither can be changed on update, because a key is referenced from Lava
    /// and code and a field type change would reinterpret every stored value. The
    /// value parameter sets the single organization-wide value; omit it to leave
    /// the current value untouched while editing the definition.
    /// </remarks>
    [Description( "Adds a new global attribute or updates an existing one, and optionally sets its current organization-wide value. A global attribute is a setting referenced from Lava and code by its key." )]
    [AgentUsage( "key, name, and fieldTypeIdKey are required when adding. Supplying globalAttributeIdKey updates that global attribute and leaves any parameter you omit unchanged. The key and field type cannot be changed after creation." )]
    [AgentToolPrerequisite( "Call LookupFieldTypes to determine the fieldTypeIdKey, and ListCategories with the Attribute entity type to determine categoryIdKeys." )]
    [AgentToolGuid( "B7171D4A-A6DF-4954-8E0B-F7FD578BF349" )]
    public AgentToolResult AddOrUpdateGlobalAttribute(
        string globalAttributeIdKey = null,
        [Description( "The programmatic key, such as OrganizationName. Required when adding; cannot be changed afterwards." )]
        string key = null,
        string name = null,
        [Description( "The field type that governs how the value is edited and stored. Required when adding; cannot be changed afterwards." )]
        string fieldTypeIdKey = null,
        SetOrClear<string> description = null,
        bool? isRequired = null,
        [Description( "The default value applied when no explicit value is set." )]
        SetOrClear<string> defaultValue = null,
        [Description( "The current organization-wide value. Omit to leave the existing value unchanged while editing the definition." )]
        SetOrClear<string> value = null,
        [Description( "The categories to file the global attribute under. When provided, this replaces the existing set; omit to leave categories unchanged." )]
        List<string> categoryIdKeys = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var attributeService = new AttributeService( rockContext );

        var isNew = globalAttributeIdKey.IsNullOrWhiteSpace();

        // The attribute definition handed to SaveAttributeEdits is built fresh
        // rather than mutated in place, because that method clears and rebuilds an
        // existing attribute's categories from the object it is given.
        var editableAttribute = new Rock.Model.Attribute
        {
            EntityTypeQualifierColumn = string.Empty,
            EntityTypeQualifierValue = string.Empty
        };

        if ( !isNew )
        {
            var existing = helper.GetRequiredEntity<Rock.Model.Attribute>( globalAttributeIdKey, checkSecurity: false );

            if ( existing == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListGlobalAttributes )} function to determine the available global attributes." );
            }

            if ( existing.EntityTypeId.HasValue )
            {
                return Error( "That attribute is not a global attribute." )
                    .WithInstructions( $"Call the {nameof( ListGlobalAttributes )} function to determine the available global attributes." );
            }

            if ( key.IsNotNullOrWhiteSpace() && !key.Equals( existing.Key, System.StringComparison.OrdinalIgnoreCase ) )
            {
                return Error( "The key of a global attribute cannot be changed after it is created." );
            }

            if ( fieldTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var suppliedFieldType = helper.GetOptionalEntity<Rock.Model.FieldType>( fieldTypeIdKey, checkSecurity: false );

                if ( suppliedFieldType == null || suppliedFieldType.Id != existing.FieldTypeId )
                {
                    return Error( "The field type of a global attribute cannot be changed after it is created." );
                }
            }

            // Seed from the existing definition so an omitted parameter is preserved.
            editableAttribute.Id = existing.Id;
            editableAttribute.Guid = existing.Guid;
            editableAttribute.Key = existing.Key;
            editableAttribute.Name = existing.Name;
            editableAttribute.Description = existing.Description;
            editableAttribute.FieldTypeId = existing.FieldTypeId;
            editableAttribute.IsRequired = existing.IsRequired;
            editableAttribute.DefaultValue = existing.DefaultValue;
            editableAttribute.Order = existing.Order;
            editableAttribute.IsSystem = existing.IsSystem;
        }
        else
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( key )} is required when adding a global attribute." );
            }

            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( name )} is required when adding a global attribute." );
            }

            if ( fieldTypeIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( fieldTypeIdKey )} is required when adding a global attribute." )
                    .WithInstructions( $"Call the {nameof( LookupFieldTypes )} function to determine the available field types." );
            }

            if ( attributeService.GetGlobalAttribute( key ) != null )
            {
                return Error( $"A global attribute with the key '{key}' already exists." )
                    .WithInstructions( $"Call the {nameof( ListGlobalAttributes )} function to find it, then supply its globalAttributeIdKey to update it." );
            }

            var fieldType = helper.GetRequiredEntity<Rock.Model.FieldType>( fieldTypeIdKey, checkSecurity: false );

            if ( fieldType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( LookupFieldTypes )} function to determine the available field types." );
            }

            editableAttribute.Key = key;
            editableAttribute.FieldTypeId = fieldType.Id;
            editableAttribute.Name = key.SplitCase();
        }

        // Apply the supplied definition changes.
        if ( name.IsNotNullOrWhiteSpace() )
        {
            editableAttribute.Name = name;
        }

        if ( description != null )
        {
            editableAttribute.Description = description.ClearValue ? null : description.Value;
        }

        if ( isRequired.HasValue )
        {
            editableAttribute.IsRequired = isRequired.Value;
        }

        if ( defaultValue != null )
        {
            editableAttribute.DefaultValue = defaultValue.ClearValue ? null : defaultValue.Value;
        }

        // Resolve categories. When the parameter is provided it replaces the set,
        // so an existing attribute keeps its categories only when it is omitted.
        if ( categoryIdKeys != null )
        {
            foreach ( var categoryIdKey in categoryIdKeys )
            {
                var category = helper.GetRequiredEntity<Rock.Model.Category>( categoryIdKey );

                if ( category == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( ListCategories )} function with the Attribute entity type to determine the available categories." );
                }

                editableAttribute.Categories.Add( category );
            }
        }
        else if ( !isNew )
        {
            var existingCache = AttributeCache.Get( editableAttribute.Id, rockContext );

            if ( existingCache != null )
            {
                foreach ( var category in existingCache.Categories )
                {
                    var categoryEntity = new CategoryService( rockContext ).Get( category.Id );

                    if ( categoryEntity != null )
                    {
                        editableAttribute.Categories.Add( categoryEntity );
                    }
                }
            }
        }

        var savedAttribute = Rock.Attribute.Helper.SaveAttributeEdits( editableAttribute, entityTypeId: null, entityTypeQualifierColumn: string.Empty, entityTypeQualifierValue: string.Empty, rockContext );

        if ( savedAttribute == null )
        {
            return Error( "The global attribute could not be saved. Check that the name, key, and field type are valid." );
        }

        // Set the organization-wide value when one was supplied. This saves the
        // value and refreshes the global attributes cache in one step.
        if ( value != null )
        {
            var newValue = value.ClearValue ? string.Empty : value.Value;

            GlobalAttributesCache.Get().SetValue( savedAttribute.Key, newValue, true, rockContext );
        }

        // A newly created definition is not in the global attributes cache yet, so
        // clear it to force a reload on the next read.
        GlobalAttributesCache.Remove();

        var attributeCache = AttributeCache.Get( savedAttribute.Id, rockContext );

        var result = new GlobalAttributeDetailResult
        {
            Id = attributeCache.Id,
            Guid = attributeCache.Guid,
            Key = attributeCache.Key,
            Name = attributeCache.Name,
            Description = attributeCache.Description.IsNullOrWhiteSpace() ? null : attributeCache.Description,
            FieldType = KeyNameResult.FromCache( attributeCache.FieldType ),
            IsRequired = attributeCache.IsRequired,
            IsSystem = attributeCache.IsSystem,
            DefaultValue = attributeCache.DefaultValue,
            Value = GlobalAttributesCache.Get().GetValue( attributeCache.Key, rockContext ),
            Categories = attributeCache.Categories
                .Select( c => KeyNameResult.FromCache( c ) )
                .ToList()
        };

        return Success( result )
            .WithInstructions( isNew
                ? "The global attribute has been created."
                : "The global attribute has been updated." )
            .WithHistoryContent( new KeyNameResult( attributeCache.Id, attributeCache.Guid, attributeCache.Name ) );
    }

    #endregion
}
