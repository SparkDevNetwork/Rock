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

using Rock.Plugin;

namespace Rock.Migrations.HotFixMigrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 4, "1.4.0" )]
    public class FixBaptismBadge : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')
DECLARE @EntityId int = (SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid] = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE')

IF @AttributeId IS NOT NULL AND @EntityId IS NOT NULL
BEGIN
	UPDATE [AttributeValue]
		SET [Value] = '{% if Person.BaptismDate != empty -%}
    <div class=""badge badge-baptism"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName }}
            was baptized on {{ Person.BaptismDate }}."">
<i class=""badge-icon fa fa-tint""></i>
    </div>
{% else -%}
    <div class=""badge badge-baptism"" data-toggle=""tooltip"" data-original-title=""No baptism date entered for {{ Person.NickName }}."">
        <i class=""badge-icon badge-disabled fa fa-tint""></i>
    </div>
{% endif -%}'
		WHERE [AttributeId] = @AttributeId AND[EntityId] = @EntityId
        AND [CreatedDateTime] = [ModifiedDateTime]
END" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
