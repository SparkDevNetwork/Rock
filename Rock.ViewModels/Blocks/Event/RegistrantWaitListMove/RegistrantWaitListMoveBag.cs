using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Event.RegistrantWaitListMove
{
    /// <summary>
    /// 
    /// </summary>
    public class RegistrantWaitListMoveBag
    {
        /// <summary>
        /// 
        /// </summary>
        public List<RegistrantWaitListMoveRecipientBag> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the email bag containing the pre-populated and Lava-resolved
        /// email template fields shown to the user before sending.
        /// </summary>
        public RegistrantWaitListMoveEmailBag EmailBag { get; set; }

        /// <summary>
        /// Gets or sets the confirmation message shown after registrants are moved,
        /// indicating how many individuals were transitioned off the wait list.
        /// </summary>
        public string UpdateMessage { get; set; }

    }
}
