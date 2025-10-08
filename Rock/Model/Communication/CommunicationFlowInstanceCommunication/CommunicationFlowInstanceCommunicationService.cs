using System.Linq;

namespace Rock.Model
{
    public partial class CommunicationFlowInstanceCommunicationService
    {
        internal IQueryable<CommunicationFlowInstanceCommunication> GetByCommunicationFlowInstance( int communicationFlowInstanceId )
        {
            return Queryable().Where( cfic => cfic.CommunicationFlowInstanceId == communicationFlowInstanceId );
        }
    }
}
