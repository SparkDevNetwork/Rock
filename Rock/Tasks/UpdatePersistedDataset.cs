using Rock.Data;
using Rock.Model;
using System.Linq;

namespace Rock.Tasks
{
    /// <summary>
    /// Updates <see cref="Rock.Model.PersistedDataset"/> 
    /// </summary>
    public sealed class UpdatePersistedDataset : BusStartedTask<UpdatePersistedDataset.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var persistedDatasetService = new PersistedDatasetService( rockContext );
                var persistedDataset = persistedDatasetService.Queryable().FirstOrDefault( d => d.AccessKey == message.AccessKey );

                if ( persistedDataset == null )
                {
                    return;
                }

                persistedDataset.UpdateResultData();
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Message class for <see cref="UpdatePersistedDataset"/>
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// The AccessKey of the dataset to be updated
            /// </summary>
            public string AccessKey { get; set; }
        }
    }
}
