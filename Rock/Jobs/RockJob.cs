using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    public abstract class RockJob
    {
        public Dictionary<string, AttributeValueCache> AttributeValues { get; internal set; }

        public abstract void Execute( RockJobContext context );

        //public abstract void Execute( RockJobContext context );

        public string GetAttributeValue( string key )
        {
            return AttributeValues.GetValueOrNull( key )?.Value;
        }
    }


    public class RockJobContext
    {
        public readonly Rock.Model.ServiceJob ServiceJob;

        public RockJobContext(ServiceJob serviceJob)
        {
            ServiceJob = serviceJob;
        }

        public string Result
        {
            get => ServiceJob.LastStatusMessage;
            set => ServiceJob.LastStatusMessage = value;
        }

        public void UpdateLastStatusMessage(string statusMessage )
        {
            ServiceJob.LastStatusMessage = statusMessage;
        }

        public RockJobDetail JobDetail { get; set; }

        public class RockJobDetail
        {
            public RockJobDataMap DataMap { get; internal set; }
            public string Description { get; internal set; }
        }

        
    }

    public class RockJobDataMap
    {
        public readonly Rock.Model.ServiceJob ServiceJob;

        public string GetString( string key )
        {
            return ServiceJob.GetAttributeValue( key );
        }

        public bool GetBoolean( string key )
        {
            return GetBooleanValue( key );
        }

        public bool GetBooleanValue( string key )
        {
            return GetString( key ).AsBoolean();
        }

        internal object Get( string key )
        {
            return GetString( key );
        }

        internal int GetInt( string key )
        {
            return GetString( key ).AsInteger();
        }
    }


    public class DisallowConcurrentExecution: System.Attribute
    {

    }
}
