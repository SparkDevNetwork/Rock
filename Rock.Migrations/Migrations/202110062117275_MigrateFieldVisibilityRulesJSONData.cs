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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class MigrateFieldVisibilityRulesJSONData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // The ComparedToRegistrationTemplateFormFieldGuid property got renamed to to ComparedToFormFieldGuid,
            // so if the JSON still has ComparedToRegistrationTemplateFormFieldGuid in it, and doesn't have
            // ComparedToFormFieldGuid in it yet, replace the field name in the JSON.
            // This fixes an issue where FieldVisibility filters would not work correctly in Registration Entry after upgrading to 12.5.

            Sql( @"UPDATE [RegistrationTemplateFormField] 
    SET [FieldVisibilityRulesJSON]  =  REPLACE([FieldVisibilityRulesJSON],   'ComparedToRegistrationTemplateFormFieldGuid',   'ComparedToFormFieldGuid') 
    WHERE [FieldVisibilityRulesJSON] LIKE  '%ComparedToRegistrationTemplateFormFieldGuid%'
    AND [FieldVisibilityRulesJSON] NOT LIKE '%ComparedToFormFieldGuid%'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
