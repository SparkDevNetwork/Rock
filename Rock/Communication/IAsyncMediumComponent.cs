using System.Threading.Tasks;

namespace Rock.Communication
{
    /// <summary>
    /// In order to take advantage of async your Medium Component should implement this interface.
    /// </summary>
    public interface IAsyncMediumComponent
    {
        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        Task SendAsync( Model.Communication communication );

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <returns></returns>
        Task<SendMessageResult> SendAsync( RockMessage rockMessage );
    }
}
