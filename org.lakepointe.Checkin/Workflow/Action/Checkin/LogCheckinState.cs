using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    [ActionCategory( "LPC Check-In" )]
    [Description( "Creates a log file containing the check-in state." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Log Check-in state" )]
    public class LogCheckinState : CheckInActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {

                try
                {
                    string json = checkInState.ToJson();
                    string rawPath = string.Format( "/App_Data/Logs/checkin/checkin_{0:yyyyMMdd_HHmmssfff}.json", DateTime.Now );

                    string importPath = null;
                    if ( System.Web.HttpContext.Current != null )
                    {
                        importPath = System.Web.HttpContext.Current.Server.MapPath( rawPath );
                    }
                    else
                    {
                        return true;
                    }

                    using ( var sw = new StreamWriter( importPath ) )
                    {
                        sw.Write( json );
                    }

                }
                catch (Exception ex)
                {
                    errorMessages.Add( ex.Message );
                }

            }
            return true;

        }
    }

}
