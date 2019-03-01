// <copyright>
// Copyright by LCBC Church
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.lcbcchurch.Utility.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    class SecurityNote : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateNoteType( "Security Note", "Rock.Model.Person", true, "DABE0530-4E30-4A14-A1B8-16AE9AABE3EE", false );
            RockMigrationHelper.AddSecurityAuthForNoteType( "DABE0530-4E30-4A14-A1B8-16AE9AABE3EE", 2, Rock.Security.Authorization.VIEW, false, null, ( int ) Rock.Model.SpecialRole.AllUsers, "3E1038CB-CA9F-4622-89CD-FF52061E7080" ); // [All Users] - Deny
            RockMigrationHelper.AddSecurityAuthForNoteType( "DABE0530-4E30-4A14-A1B8-16AE9AABE3EE", 1, Rock.Security.Authorization.VIEW, true, "32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB", ( int ) Rock.Model.SpecialRole.None, "24106217-B672-4B46-8543-22B330956935" );  // [RSR - Safety Workers] - Allow
            RockMigrationHelper.AddSecurityAuthForNoteType( "DABE0530-4E30-4A14-A1B8-16AE9AABE3EE", 0, Rock.Security.Authorization.EDIT, true, "32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB", ( int ) Rock.Model.SpecialRole.None, "100BEE21-5815-4130-A70B-9AA7D6CA1E5B" );  // [RSR - Safety Workers] - Allow
            RockMigrationHelper.AddSecurityAuthForNoteType( "DABE0530-4E30-4A14-A1B8-16AE9AABE3EE", 0, Rock.Security.Authorization.VIEW, true, "628C51A8-4613-43ED-A18D-4A6FB999273E", ( int ) Rock.Model.SpecialRole.None, "E7853751-4350-4E4B-91AD-A7F420738B01" );  // [RSR - Rock Administration] - Allow

            RockMigrationHelper.UpdatePersonBadge( "Security Note Badge", "Label badge showing whether a security note exists on a person", "Rock.PersonProfile.Badge.Liquid", 0, "CAF90175-6E5C-4EBC-B2D2-2069C0C56CC5" );
            RockMigrationHelper.AddPersonBadgeAttributeValue( "CAF90175-6E5C-4EBC-B2D2-2069C0C56CC5", "01C9BA59-D8D4-4137-90A6-B3C06C70BBC3", @"{% assign noteExists = false %}
{% assign noteTypeGuid = 'DABE0530-4E30-4A14-A1B8-16AE9AABE3EE' %}
{% notetype where:'Guid == ""{{ noteTypeGuid}}""' %}
    {% for noteType in notetypeItems %}
        {% assign noteTypeId = noteType.Id %}
        {% note where:'NoteTypeId == ""{{ noteTypeId}}"" && EntityId == {{Person.Id}}' %}
            {% for note in noteItems %}
                {% assign noteExists = true %}
            {% endfor %}
        {% endnote %}
    {% endfor %}
{% endnotetype %}

{% if noteExists %}<span class='label label-danger'>Security Note Exists</span>{% endif %} " );

            RockMigrationHelper.UpdateBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "8E11F65B-7272-4E9F-A4F1-89CE08E658DE", "66972bff-42cd-49ab-9a7a-e1b9deca4ebf,caf90175-6e5c-4ebc-b2d2-2069c0c56cc5,b21dcd49-ac35-4b2b-9857-75213209b643,66972bff-42cd-49ab-9a7a-e1b9deca4eca" );
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }


    }
}
