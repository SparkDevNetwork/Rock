// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
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
using Rock.Web.Cache;

namespace org.secc.PastoralCare.Migrations
{
    [MigrationNumber( 6, "1.2.0" )]
    class Pastoral_FixLegacyLava : Migration
    {
        public override void Up()
        {
            Sql(@"  UPDATE [AttributeValue] 
                      SET Value =
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        REPLACE(Value,
                                            'Workflow.PersonToVisit',
                                            'Workflow | Attribute:''PersonToVisit'''
                                        ),
                                        'Workflow.NursingHome',
                                        'Workflow | Attribute:''NursingHome'''
                                    ),
                                    'Workflow.PersonToVisit',
                                    'Workflow | Attribute:''PersonToVisit'''
                                ),
                                'Workflow.Hospital',
                                'Workflow | Attribute:''Hospital'''
                            ),
                            'Workflow.HomeboundPerson',
                            'Workflow | Attribute:''HomeboundPerson'''
                        )
                      WHERE
                        AttributeId = (SELECT id FROM Attribute WHERE Guid = '93852244-A667-4749-961A-D47F88675BE4'); 
            ");
            Sql(@"  
	               UPDATE [WorkflowActionForm]
	               SET Header =
		               REPLACE(
				            REPLACE(
					            REPLACE(Header, 
						            'activity.Visitor', 
						            'activity | Attribute:''Visitor'''
					            ), 
					            'activity.VisitDate', 
					            'activity | Attribute:''VisitDate'''
				            ), 
				            'activity.VisitNote', 
				            'activity | Attribute:''VisitNote'''
			            ),
		            Footer =
			            REPLACE(
				            REPLACE(
					            REPLACE(Footer, 
						            'activity.Visitor', 
						            'activity | Attribute:''Visitor'''
					            ), 
					            'activity.VisitDate', 
					            'activity | Attribute:''VisitDate'''
				            ), 
				            'activity.VisitNote', 
				            'activity | Attribute:''VisitNote'''
			            )
		            WHERE ID IN (
			            SELECT WorkflowFormID FROM  WorkflowActionType WHERE ActivityTypeId in ( 
				            SELECT Id FROM WorkflowActivityType WHERE WorkflowTypeId in (
					            SELECT id FROM WorkflowType WHERE 
						            Guid IN (
							            '3621645F-FBD0-4741-90EC-E032354AA375',
							            '7818DFD9-E347-43B2-95E3-8FBF83AB962D',
							            '314CC992-C90C-4D7D-AEC6-09C0FB4C7A38'
						            )
				            )
			            )
		            ); 
            ");
        }
        public override void Down()
        {
            // There is no going back.
        }
    }
}

