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
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Workflow.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    public class WorkflowListAddLastUpdated : Migration
    {
        public override void Up()
        {
            // Add Custom Column to system block on Page 288: Workflow List, Manage Workflows

            // Attrib Value for Block:Workflow List, Attribute:core.CustomGridColumnsConfig Page: Manage Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BCC61035-DA99-47EE-A376-71D430455DB4","84B029A1-E1AD-4064-B1D2-C837C21BC5EF",@"{""ColumnsConfig"":[{""HeaderText"":""Last Updated"",""HeaderClass"":"""",""ItemClass"":"""",""LavaTemplate"":""{% workflow id:'{{Row.Id}}' %}\n {% assign lastModified = workflow.CreatedDateTime %}\n {% attribute where:'EntityTypeId == 113 && EntityTypeQualifierColumn == \""WorkflowTypeId\"" && EntityTypeQualifierValue == \""{{workflow.WorkflowTypeId}}\""'%}\n \n {% for attributeItem in attributeItems %}\n {% attributevalue where:'AttributeId == {{attributeItem.Id}} && EntityId == {{workflow.Id}}' %}\n {% for attributevalueItem in attributevalueItems %}\n {% if attributevalueItem.ModifiedDateTime > lastModified %}\n {% assign lastModified = attributevalueItem.ModifiedDateTime %}\n {% endif %}\n {% endfor %}\n {% endattributevalue %}\n {% endfor %}\n \n {% endattribute %}\n {{lastModified | HumanizeDateTime}}\n{% endworkflow %}"",""PositionOffsetType"":0,""PositionOffset"":7}]}");

        }

        public override void Down()
        {
            //throw new NotImplementedException();
        }
    }
}
