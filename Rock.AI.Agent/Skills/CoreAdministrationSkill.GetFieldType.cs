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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single field type in full detail, including the configuration
    /// qualifiers it accepts.
    /// </summary>
    [Description( "Gets a single field type in full detail, including the configuration qualifiers accepted when creating an attribute with it." )]
    [AgentPurpose( "Retrieves the configuration qualifiers needed to create an attribute of a given field type." )]
    [AgentToolPrerequisite( "Call LookupFieldTypes to determine the fieldTypeIdKey." )]
    [AgentToolGuid( "CD8C8E44-F60C-4F1A-A480-683C600C526E" )]
    public AgentToolResult GetFieldType( string fieldTypeIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var fieldType = helper.GetRequiredEntity<Rock.Model.FieldType>( fieldTypeIdKey );

        if ( fieldType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( LookupFieldTypes )} function to determine the available field types." );
        }

        var fieldTypeCache = FieldTypeCache.Get( fieldType.Id, AgentRequestContext.RockContext );
        var configurationKeys = fieldTypeCache?.Field?.ConfigurationKeys() ?? new List<string>();

        var supplements = GetConfigurationKeySupplements( fieldType.Class );

        var result = new FieldTypeDetailResult
        {
            Id = fieldType.Id,
            Guid = fieldType.Guid,
            Name = fieldType.Name,
            Class = fieldType.Class,
            Description = fieldType.Description,
            ConfigurationKeys = configurationKeys
                .Select( key => new FieldTypeConfigurationKeyResult
                {
                    Key = key,
                    Description = supplements.TryGetValue( key, out var supplement )
                        ? supplement.Description

                        // Say the key is undocumented rather than inventing a
                        // description. A plausible guess is worse than an
                        // admission, because a caller cannot tell them apart.
                        : "Undocumented. This field type does not describe what this qualifier controls.",
                    ExampleValue = supplements.TryGetValue( key, out var example )
                        ? example.ExampleValue
                        : null
                } )
                .OrderBy( ck => ck.Key )
                .ToList()
        };

        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this field type." );
        }

        return Success( result )
            .WithHistoryContent( new KeyNameResult( fieldType.Id, fieldType.Guid, fieldType.Name ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the hand-authored descriptions for a field type's configuration
    /// qualifiers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Rock.Field.IFieldType.ConfigurationKeys"/> returns bare key
    /// names. Little in Rock consumes it, so many field types never populated it,
    /// and where it exists it says nothing about what a key does or what value
    /// format it takes. Bare keys are therefore not enough to create an attribute
    /// correctly, which is what makes these supplements required rather than a
    /// convenience.
    /// </para>
    /// <para>
    /// The long-term fix is a new IFieldType member describing qualifiers,
    /// implemented across all field types. That is a Rock core project and does
    /// not gate this skill. Until then this covers the field types that authoring
    /// actually reaches for.
    /// </para>
    /// </remarks>
    /// <param name="fieldTypeClass">The full class name of the field type.</param>
    /// <returns>A map of qualifier key to its description and example, empty when the field type has no supplement.</returns>
    private static Dictionary<string, ConfigurationKeySupplement> GetConfigurationKeySupplements( string fieldTypeClass )
    {
        if ( fieldTypeClass.IsNullOrWhiteSpace() || !_configurationKeySupplements.ContainsKey( fieldTypeClass ) )
        {
            return _emptySupplements;
        }

        return _configurationKeySupplements[fieldTypeClass];
    }

    /// <summary>
    /// The description and example for one configuration qualifier.
    /// </summary>
    private sealed class ConfigurationKeySupplement
    {
        /// <summary>
        /// What the qualifier controls.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A representative value showing the expected format.
        /// </summary>
        public string ExampleValue { get; set; }
    }

    /// <summary>
    /// Returned for any field type with no supplement, so callers never have to
    /// null check.
    /// </summary>
    private static readonly Dictionary<string, ConfigurationKeySupplement> _emptySupplements = new Dictionary<string, ConfigurationKeySupplement>();

    /// <summary>
    /// Hand-authored qualifier descriptions, keyed by field type class name and
    /// then by qualifier key. Field types absent from this map return their bare
    /// keys with an undocumented note.
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, ConfigurationKeySupplement>> _configurationKeySupplements = new Dictionary<string, Dictionary<string, ConfigurationKeySupplement>>
    {
        ["Rock.Field.Types.TextFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["ispassword"] = new ConfigurationKeySupplement { Description = "Masks the value as it is typed and hides it when displayed.", ExampleValue = "False" },
            ["maxcharacters"] = new ConfigurationKeySupplement { Description = "The maximum number of characters accepted. Leave blank for no limit.", ExampleValue = "100" },
            ["showcountdown"] = new ConfigurationKeySupplement { Description = "Shows a remaining character count. Only meaningful with maxcharacters set.", ExampleValue = "False" },
            ["isfirstname"] = new ConfigurationKeySupplement { Description = "Treats the value as a first name, which enables nickname aware matching.", ExampleValue = "False" },
            ["allowhtml"] = new ConfigurationKeySupplement { Description = "Permits HTML in the value rather than encoding it on display.", ExampleValue = "False" },
            ["allowlava"] = new ConfigurationKeySupplement { Description = "Resolves Lava in the value when it is displayed.", ExampleValue = "False" }
        },

        ["Rock.Field.Types.MemoFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["numberofrows"] = new ConfigurationKeySupplement { Description = "The visible height of the editor, in rows.", ExampleValue = "3" },
            ["allowhtml"] = new ConfigurationKeySupplement { Description = "Permits HTML in the value rather than encoding it on display.", ExampleValue = "False" },
            ["maxcharacters"] = new ConfigurationKeySupplement { Description = "The maximum number of characters accepted. Leave blank for no limit.", ExampleValue = "500" },
            ["showcountdown"] = new ConfigurationKeySupplement { Description = "Shows a remaining character count. Only meaningful with maxcharacters set.", ExampleValue = "False" }
        },

        ["Rock.Field.Types.SelectSingleFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["values"] = new ConfigurationKeySupplement { Description = "The options to choose from, as a comma separated list. Each option is either a bare value or a stored value and display text joined by a caret. The stored value is what the attribute holds.", ExampleValue = "S^Small,M^Medium,L^Large" },
            ["fieldtype"] = new ConfigurationKeySupplement { Description = "How the options are rendered. 'ddl' for a dropdown list, 'rb' for radio buttons.", ExampleValue = "ddl" },
            ["repeatColumns"] = new ConfigurationKeySupplement { Description = "How many columns radio buttons are laid out across. Ignored for a dropdown list.", ExampleValue = "4" }
        },

        ["Rock.Field.Types.SelectMultiFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["values"] = new ConfigurationKeySupplement { Description = "The options to choose from, as a comma separated list. Each option is either a bare value or a stored value and display text joined by a caret.", ExampleValue = "S^Small,M^Medium,L^Large" },
            ["enhancedselection"] = new ConfigurationKeySupplement { Description = "Uses a searchable multi select control instead of a checkbox list.", ExampleValue = "False" },
            ["repeatColumns"] = new ConfigurationKeySupplement { Description = "How many columns the checkbox list is laid out across.", ExampleValue = "4" },
            ["repeatDirection"] = new ConfigurationKeySupplement { Description = "The direction the checkbox list flows, 'Horizontal' or 'Vertical'.", ExampleValue = "Horizontal" }
        },

        ["Rock.Field.Types.BooleanFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["truetext"] = new ConfigurationKeySupplement { Description = "The label shown for the true value.", ExampleValue = "Yes" },
            ["falsetext"] = new ConfigurationKeySupplement { Description = "The label shown for the false value.", ExampleValue = "No" },
            ["BooleanControlType"] = new ConfigurationKeySupplement { Description = "How the value is rendered: 'Checkbox', 'Toggle', or 'DropDown'.", ExampleValue = "Checkbox" }
        },

        ["Rock.Field.Types.DefinedValueFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["definedtype"] = new ConfigurationKeySupplement { Description = "The Id of the defined type whose values may be selected. This qualifier is required.", ExampleValue = "27" },
            ["allowmultiple"] = new ConfigurationKeySupplement { Description = "Permits selecting more than one value.", ExampleValue = "False" },
            ["displaydescription"] = new ConfigurationKeySupplement { Description = "Shows each value's description instead of its value text.", ExampleValue = "False" },
            ["enhancedselection"] = new ConfigurationKeySupplement { Description = "Uses a searchable select control, which is worth enabling for long defined types.", ExampleValue = "False" },
            ["includeInactive"] = new ConfigurationKeySupplement { Description = "Includes inactive defined values in the choices.", ExampleValue = "False" },
            ["AllowAddingNewValues"] = new ConfigurationKeySupplement { Description = "Lets a person add a new defined value from the picker.", ExampleValue = "False" },
            ["RepeatColumns"] = new ConfigurationKeySupplement { Description = "How many columns the choices are laid out across.", ExampleValue = "4" },
            ["SelectableDefinedValuesId"] = new ConfigurationKeySupplement { Description = "A comma separated list of defined value Ids to limit the choices to a subset of the type.", ExampleValue = "142,143,144" }
        },

        ["Rock.Field.Types.PersonFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["EnableSelfSelection"] = new ConfigurationKeySupplement { Description = "Lets the current person select themselves.", ExampleValue = "False" },
            ["includeBusinesses"] = new ConfigurationKeySupplement { Description = "Includes business records in the search results, not just people.", ExampleValue = "False" }
        },

        ["Rock.Field.Types.CampusFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["includeInactive"] = new ConfigurationKeySupplement { Description = "Includes inactive campuses in the choices.", ExampleValue = "False" },
            ["filterCampusTypes"] = new ConfigurationKeySupplement { Description = "A comma separated list of campus type defined value Ids to limit the choices to.", ExampleValue = "1,2" },
            ["filterCampusStatus"] = new ConfigurationKeySupplement { Description = "A comma separated list of campus status defined value Ids to limit the choices to.", ExampleValue = "3" },
            ["forceVisible"] = new ConfigurationKeySupplement { Description = "Shows the picker even when the organization has only one campus.", ExampleValue = "False" },
            ["SelectableCampusIds"] = new ConfigurationKeySupplement { Description = "A comma separated list of campus Ids to limit the choices to.", ExampleValue = "1,3" }
        },

        ["Rock.Field.Types.BinaryFileFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["binaryFileType"] = new ConfigurationKeySupplement { Description = "The Guid of the binary file type the uploaded file is stored under. This qualifier is required.", ExampleValue = "C1142570-8CD6-4A20-83B1-ACB47C1CD377" }
        },

        ["Rock.Field.Types.ImageFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["binaryFileType"] = new ConfigurationKeySupplement { Description = "The Guid of the binary file type the uploaded image is stored under.", ExampleValue = "C1142570-8CD6-4A20-83B1-ACB47C1CD377" },
            ["enableCrop"] = new ConfigurationKeySupplement { Description = "Prompts the uploader to crop the image before saving.", ExampleValue = "False" },
            ["targetWidth"] = new ConfigurationKeySupplement { Description = "The width the image is resized to, in pixels.", ExampleValue = "512" },
            ["targetHeight"] = new ConfigurationKeySupplement { Description = "The height the image is resized to, in pixels.", ExampleValue = "512" },
            ["minimumWidth"] = new ConfigurationKeySupplement { Description = "The smallest accepted image width, in pixels.", ExampleValue = "128" },
            ["minimumHeight"] = new ConfigurationKeySupplement { Description = "The smallest accepted image height, in pixels.", ExampleValue = "128" }
        },

        ["Rock.Field.Types.UrlLinkFieldType"] = new Dictionary<string, ConfigurationKeySupplement>
        {
            ["ShouldRequireTrailingForwardSlash"] = new ConfigurationKeySupplement { Description = "Requires the entered URL to end with a forward slash.", ExampleValue = "False" },
            ["ShouldAlwaysShowCondensed"] = new ConfigurationKeySupplement { Description = "Displays the URL text only, without rendering it as a link.", ExampleValue = "False" }
        }
    };

    #endregion
}
