using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.reallifeministries.RockExtensions.Workflow.Action
{
    /// <summary>
    /// Delays successful execution of action until a specified number of minutes have passed
    /// </summary>
    [Description( "Delays successful execution of action until a time" )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "DelayUntil" )]

    [WorkflowAttribute( "DateTime Attribute", "The attribute which has a time to delay until", true, "", "", 0,null,
        new string[] { "Rock.Field.Types.DateTimeFieldType" } )]

    class DelayUntil : Rock.Workflow.ActionComponent
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            DateTime? delayUntil = DateTime.Parse( GetAttributeValue( action, "DateTimeAttribute" ) );

            if (!delayUntil.HasValue)
            {
                return true;
            }

            if (delayUntil <= DateTime.Now)
            {
                return true;
            }
            else
            {
                // We have not reached delay until date
                return false;
            }
            
        }
    }
}