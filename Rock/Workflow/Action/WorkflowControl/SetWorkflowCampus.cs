using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Workflow.Action.WorkflowControl
{
    /// <summary>
    /// Sets the CampusId of the workflow
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Sets the workflow campus to the given campus attribute or primary campus of a given person attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Workflow Set Campus" )]

    [PersonField( "Person", "The person whose primary campus to set the workflow campusId to. Leave blank to set person to nobody.", false, "", "", 1 )]
    [CampusField( "Campus", "The campus for the request. If blank the person's campus will be used.", false, "", "", 4 )]
    public class SetWorkflowCampus : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><see cref="bool"/></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            Guid? personAliasGuid = GetAttributeValue( action, "Person" ).AsGuidOrNull();
            // get campus
            var campusCache = CampusCache.Get( GetAttributeValue( action, "Campus" ).AsGuid() );
            int? campusId = campusCache?.Id;
            string campusName = campusCache?.Name;

            if ( !campusId.HasValue && personAliasGuid.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid.Value );
                if ( personAlias != null && personAlias.Person != null )
                {
                    var campus = personAlias.Person.GetCampus();
                    campusId = campus?.Id;
                    campusName = campus?.Name;
                }
            }

            if ( campusId.HasValue )
            {
                action.Activity.Workflow.CampusId = campusId;
                action.AddLogEntry( string.Format( "Assigned Campus to '{0} ({1})' ", campusName, campusId ) );
                return true;
            }

            return false;
        }
    }
}
