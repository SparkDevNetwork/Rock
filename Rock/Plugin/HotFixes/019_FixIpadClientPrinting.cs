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
    /// 
    /// </summary>
    [MigrationNumber( 19, "1.6.1" )]
    public class FixIpadClientPrinting : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @LabelFileId INT = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = 'C2E6B0A0-3991-4FAF-9E2F-49CF321CBB0D' )
    UPDATE [BinaryFileData] 
    SET [Content] =	0x1043547E7E43442C7E43435E7E43547E0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534432345E4A55535E4C524E5E4349305E585A0A5E58410A5E4D4D540A5E50573831320A5E4C4C303430360A5E4C53300A5E46543435322C3131395E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530A5E465431322C3235345E41304E2C3133352C3134365E46485C5E4644355E46530A5E465431342C3330395E41304E2C34352C34355E46485C5E4644365E46530A5E43575A2C453A524F433030302E464E545E46543239332C38325E415A4E2C37332C36340A5E46485C5E4644425E46530A5E43575A2C453A524F433030302E464E545E46543337382C38315E415A4E2C37332C36340A5E46485C5E4644465E46530A5E46543239392C3132305E41304E2C32382C32385E46485C5E4644345E46530A5E46423333302C322C302C4C5E4654382C3338325E41304E2C32382C32385E46485C5E4644395E46530A5E43575A2C453A524F433030302E464E545E46543630352C3338335E415A4E2C37332C36345E46485C5E4644375E46530A5E43575A2C453A524F433030302E464E545E46543731352C3338365E415A4E2C37332C36345E46485C5E4644385E46530A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0A5E5051312C302C312C595E585A0A
    WHERE [Id] = @LabelFileId

    UPDATE [AttributeValue] SET [Value] = '{% assign personAllergy = Person | Attribute:''Allergy'' %}{% if personAllergy != '''' %}{{ personAllergy | Truncate:100,''...'' }}{% endif %}'
    WHERE [Guid] = '4315A58E-6514-49A8-B80C-22AC7710AC19' AND [ModifiedDateTime] IS NULL

    UPDATE [AttributeValue] SET [Value] = '{% assign personLegalNotes = Person | Attribute:''LegalNotes'' %}{% if personLegalNotes != '''' %}{{ personLegalNotes | Truncate:100,''...'' }}{% endif %}'
    WHERE [Guid] = '89C604FA-61A9-4255-AE1F-B6381B23603F' AND [ModifiedDateTime] IS NULL

    UPDATE [AttributeValue] SET [Value] = '{% assign personAllergy = Person | Attribute:''Allergy'' %}{% if personAllergy != '''' -%}A{% endif -%}'
    WHERE [Guid] = '5DD35431-D22D-4410-9A55-55EAC9859C35' AND [ModifiedDateTime] IS NULL

    UPDATE [AttributeValue] SET [Value] = '{% assign personLegalNotes = Person | Attribute:''LegalNotes'' %}{% if personLegalNotes != '''' %}L{% endif %}'
    WHERE [Guid] = '872DBF30-E0C0-4810-A36E-D28FC3124A51' AND [ModifiedDateTime] IS NULL
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
