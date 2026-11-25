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
namespace Rock.Data.Migrations;

/// <summary>
/// Fixes columns whose nullability did not match the model, as found by the
/// schema mapping check: RegistrationTemplate.Description and
/// WorkflowActionForm.PersonEntrySpouseLabel now allow NULL, and
/// MetaLastNameLookup.LastName is now NOT NULL.
/// </summary>
[SqlMigration( 20261125224041, "21.0" )]
internal class FixSchemaCheckNullabilityDrift : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        AllowNullRegistrationTemplateDescription();
        AllowNullWorkflowActionFormPersonEntrySpouseLabel();
        RequireMetaLastNameLookupLastName();
    }

    /// <summary>
    /// The model allows a NULL description, but the column was NOT NULL, so
    /// saving a template without a description failed.
    /// </summary>
    private void AllowNullRegistrationTemplateDescription()
    {
        Sql( @"
IF EXISTS ( SELECT 1 FROM [sys].[columns] WHERE [object_id] = OBJECT_ID( 'dbo.RegistrationTemplate' ) AND [name] = 'Description' AND [is_nullable] = 0 )
BEGIN
    ALTER TABLE [dbo].[RegistrationTemplate] ALTER COLUMN [Description] [nvarchar](max) NULL;
END" );
    }

    /// <summary>
    /// The model allows a NULL spouse label (the Form Builder can send one),
    /// but the column was NOT NULL, so that save failed. The column keeps its
    /// 'Spouse' default.
    /// </summary>
    private void AllowNullWorkflowActionFormPersonEntrySpouseLabel()
    {
        Sql( @"
IF EXISTS ( SELECT 1 FROM [sys].[columns] WHERE [object_id] = OBJECT_ID( 'dbo.WorkflowActionForm' ) AND [name] = 'PersonEntrySpouseLabel' AND [is_nullable] = 0 )
BEGIN
    ALTER TABLE [dbo].[WorkflowActionForm] ALTER COLUMN [PersonEntrySpouseLabel] [nvarchar](50) NULL;
END" );
    }

    /// <summary>
    /// The model requires a last name. A lookup row without one can never
    /// match a name, so it is deleted before the column becomes NOT NULL.
    /// </summary>
    private void RequireMetaLastNameLookupLastName()
    {
        Sql( @"
IF EXISTS ( SELECT 1 FROM [sys].[columns] WHERE [object_id] = OBJECT_ID( 'dbo.MetaLastNameLookup' ) AND [name] = 'LastName' AND [is_nullable] = 1 )
BEGIN
    DELETE FROM [dbo].[MetaLastNameLookup] WHERE [LastName] IS NULL;

    ALTER TABLE [dbo].[MetaLastNameLookup] ALTER COLUMN [LastName] [nvarchar](100) NOT NULL;
END" );
    }
}
