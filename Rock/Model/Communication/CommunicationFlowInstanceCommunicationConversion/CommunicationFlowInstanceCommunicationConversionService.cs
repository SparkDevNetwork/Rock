using System.Linq;

namespace Rock.Model
{
    public partial class CommunicationFlowInstanceCommunicationConversionService
    {
        internal IQueryable<CommunicationFlowInstanceCommunicationConversion> GetByCommunicationFlowInstance( int communicationFlowInstanceId )
        {
            return Queryable().Where( cfich => cfich.CommunicationFlowInstanceCommunication.CommunicationFlowInstanceId == communicationFlowInstanceId );
        }
    }
}
