using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


namespace com.reallifeministries.GroupMatching.Workflow.Action
{
   
    [Description( "Match groups and assign workgroup attributes from the matches" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Find Matching groups, assign to workflow attributes" )]

    [WorkflowAttribute( "PersonAttribute", "The workflow attribute containing the person.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "DaysOfWeekAttribute", "The workflow attribute containing the days of the week.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.DaysOfWeekFieldType" } )]
    [GroupTypeField("GroupType","The Group Type to Match")]
    [IntegerField("Number Of Matches", "The number of groups returned as matches",true,3)]

    [MemoField("AttributeAssignments", 
@"
    YAML syntax with keys representing Workgroup attribute names, and values representing the value in lava/liquid syntax.<br>
    LAVA/Liquid: 'matches' object is an array of GroupMatch objects.<br> 
    GroupMatch contains the following fields:  Group, Distance, MemberCount, Schedule.<br>
    ** matches Array index starts with zero (0) **<br>
    Example: 'matches[0].Group' would be the Group in match 1.<br>
    ** IF you want to assign a Group, you must use Group.Guid because that is what the workflow attribute expects ** <br>
    <br>
    <strong>Example</strong><br>
    <pre>
        GroupAttr: ""{{matches[0].Group.Guid}}""
        NumberOfMembers: ""{{matches[0].MemberCount}}""
    </pre>  
"   , true)]
    public class MatchGroupsAndAssignAttributes : ActionComponent
    {
         /// <summary>
        /// Executes the specified workflow action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            GroupType groupType = GetGroupType( rockContext, action );
            List<DayOfWeek> daysOfWeek = GetDaysOfWeek( rockContext, action );
            Person person = GetPerson( rockContext, action );
            int numMatches = GetNumberOfMatches( rockContext, action );

            if (person != null)
            {
                if (groupType != null)
                {
                    if (daysOfWeek != null)
                    {
                        var attrAssignments = GetAttributeValue( action, "AttributeAssignments" );
                        var ds = new Deserializer();

                        action.Activity.LoadAttributes();
                        action.Activity.Workflow.LoadAttributes();

                        var groupMatcher = new GroupMatcher(person,groupType,daysOfWeek);
                        groupMatcher.numMatches = numMatches;

                        var matches = groupMatcher.GetMatches();

                        var mergeFields = GetMergeFields( action );
                        mergeFields.Add( "Person", person );
                        mergeFields.Add( "matches", matches );

                        var attrList = ds.Deserialize< Dictionary<string, string> >( new StringReader( attrAssignments ) );
                        foreach(string key in attrList.Keys)
                        {
                            if (action.Activity.Attributes.ContainsKey( key ))
                            {
                                var lava = attrList[key];
                                try
                                {
                                    var value = lava.ResolveMergeFields( mergeFields );
                                    if (value.Contains( "Liquid error" ))
                                    {
                                        action.AddLogEntry( "Liquid Error: " + value );
                                    }
                                    else
                                    {
                                        action.Activity.SetAttributeValue( key, value );
                                    }
                                    
                                }
                                catch (DotLiquid.Exceptions.LiquidException ex)
                                {
                                    action.AddLogEntry( "Liquid Error: " + ex.Message );
                                }
                                
                            }
                            else if(action.Activity.Workflow.Attributes.ContainsKey( key ))
                            {
                                var lava = attrList[key];
                                try
                                {
                                    var value = lava.ResolveMergeFields( mergeFields );
                                    if (value.Contains( "Liquid error" ))
                                    {
                                        action.AddLogEntry( "Liquid Error: " + value );
                                    }
                                    else
                                    {
                                        action.Activity.Workflow.SetAttributeValue( key, value );
                                    }
                                }
                                catch (DotLiquid.Exceptions.LiquidException ex)
                                {
                                    action.AddLogEntry( "Liquid Error: " + ex.Message );
                                }
                                
                            }
                        }

                        return true;

                    }
                    else
                    {
                        errorMessages.Add( "Days of week attribute could not be found" );
                    }
                  
                }
                else
                {
                    errorMessages.Add( "Group Type could not be found" );
                }
            }
            else
            {
                errorMessages.Add( "Person could not be found" );
            }
            
            return false;
        }

        private GroupType GetGroupType( RockContext context, WorkflowAction action )
        {
            Guid groupTypeGuid = GetAttributeValue( action, "GroupType" ).AsGuid();
            if (groupTypeGuid != null)
            {
                var groupTypeService = new GroupTypeService(context);
                return groupTypeService.Get( groupTypeGuid );
            }
            return null;
        }

        private int GetNumberOfMatches( RockContext context, WorkflowAction action )
        {
            int num = Int32.Parse(GetAttributeValue( action, "NumberOfMatches" ));
            return num;
        }

        private List<DayOfWeek> GetDaysOfWeek( RockContext context, WorkflowAction action )
        {
            Guid daysOfWeekAttrGuid = GetAttributeValue( action, "DaysOfWeekAttribute" ).AsGuid();
            if (!daysOfWeekAttrGuid.IsEmpty())
            {
                var daysOfWeekAttributeInst = AttributeCache.Read( daysOfWeekAttrGuid, context );
                if (daysOfWeekAttributeInst != null)
                {
                    List<DayOfWeek> daysOfWeek = action.GetWorklowAttributeValue( daysOfWeekAttrGuid ).Split( ',' ).Select( a => (DayOfWeek)( a.AsInteger() ) ).ToList();
                    return daysOfWeek;
                }
            }
            return null;
        }
        private Person GetPerson( RockContext context, WorkflowAction action )
        {
            Guid personAttrGuid = GetAttributeValue( action, "PersonAttribute" ).AsGuid();
            if (!personAttrGuid.IsEmpty())
            {
                var personAttributeInst = AttributeCache.Read( personAttrGuid, context );
                if (personAttributeInst != null)
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( personAttrGuid );
                    Guid personAliasGuid = attributePersonValue.AsGuid();
                    var personAlias = (new PersonAliasService(context )).Get( personAliasGuid );
                    if (personAlias != null)
                    {
                        return personAlias.Person;
                    }
                }
            }
          
            return null;
        }
            
    }
}
