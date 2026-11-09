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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Adds the Escape filter to the step name and color in Step Flow footer templates that still match the shipped default.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 344, "17.8" )]
    public class EscapeStepFlowFooterLavaTemplate : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                11/09/26 - JMH

                Each saved Step Flow block keeps its own copy of the footer template,
                so changing the code default does not reach blocks whose settings were saved.
                Only values that match the shipped default once carriage returns are removed are changed,
                so customized templates are left alone and running this again changes nothing.
                The tokens are replaced in place, so the stored line endings are kept.

                Reason: Escape step names in saved Step Flow footers without touching customized templates.
            */
            Sql( @"
DECLARE @OldTemplate nvarchar(max) = N'<div class=""flow-legend"">
{% for stepItem in Steps %}
    <div class=""flow-key"">
        <span class=""color"" style=""background-color:{{stepItem.Color}};""></span>
        <span>{{forloop.index}}. {{stepItem.StepName}}</span>
    </div>
{% endfor %}
</div>'
SET @OldTemplate = REPLACE( @OldTemplate, NCHAR(13), N'' )

DECLARE @AttributeId int = (
    SELECT A.[Id]
      FROM [Attribute] A
     INNER JOIN [BlockType] BT
        ON A.[EntityTypeQualifierColumn] = 'BlockTypeId'
       AND A.[EntityTypeQualifierValue] = CAST( BT.[Id] AS nvarchar(50) )
     WHERE BT.[Guid] = '2B4E0128-BCDF-48BF-AEC9-85001169DA3E'
       AND A.[Key] = N'FooterLavaTemplate' )

IF @AttributeId IS NOT NULL
BEGIN
    UPDATE [Attribute]
       SET [DefaultValue] = REPLACE( REPLACE( [DefaultValue] COLLATE Latin1_General_BIN2
               , N'{{stepItem.Color}}', N'{{stepItem.Color | Escape}}' )
               , N'{{stepItem.StepName}}', N'{{stepItem.StepName | Escape}}' )
         , [IsDefaultPersistedValueDirty] = 1
     WHERE [Id] = @AttributeId
       AND REPLACE( [DefaultValue], NCHAR(13), N'' ) COLLATE Latin1_General_BIN2 = @OldTemplate

    UPDATE [AttributeValue]
       SET [Value] = REPLACE( REPLACE( [Value] COLLATE Latin1_General_BIN2
               , N'{{stepItem.Color}}', N'{{stepItem.Color | Escape}}' )
               , N'{{stepItem.StepName}}', N'{{stepItem.StepName | Escape}}' )
         , [IsPersistedValueDirty] = 1
     WHERE [AttributeId] = @AttributeId
       AND REPLACE( [Value], NCHAR(13), N'' ) COLLATE Latin1_General_BIN2 = @OldTemplate
END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
