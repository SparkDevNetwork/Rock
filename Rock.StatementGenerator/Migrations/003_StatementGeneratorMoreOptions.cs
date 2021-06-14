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

using Rock.Plugin;

namespace Rock.StatementGenerator.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 3, "1.7.4" )]
    [Obsolete( "Use FinancialStatementTemplate instead" )]
    [RockObsolete( "12.4" )]
    public class StatementGeneratorMoreOptions : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Update Help Text of FooterHtml
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.StatementGenerator.SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE, Rock.SystemGuid.FieldType.CODE_EDITOR,
                "Footer HTML", "FooterHtml", @"Advanced: If you want custom Html for your footer, you can create an HTML doc that will generate the footer. See https://wkhtmltopdf.org/usage/wkhtmltopdf.txt
Note: The Footer will be placed in the bottom margin of the page. To adjust the footer height, set margin.bottom in PDF Object Settings.", 102, string.Empty, Rock.StatementGenerator.SystemGuid.Attribute.DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_FOOTERHTML );

            // Add new PDFObjectSettings option
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.StatementGenerator.SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE, Rock.SystemGuid.FieldType.KEY_VALUE_LIST,
                "PDF Object Settings (Advanced)", "PDFObjectSettings", "See https://wkhtmltopdf.org/libwkhtmltox/pagesettings.html#pagePdfObject", 103, "margin.top^10|margin.bottom^10|margin.left^10|margin.right^10|size.pageSize^letter", Rock.StatementGenerator.SystemGuid.Attribute.DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_PDFOBJECTSETTINGS );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
