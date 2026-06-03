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
    public class RegistrantWaitListMoveRecipientBag
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of person who registered
        /// </summary>
        public string RegistrantName { get; set; }

        /// <summary>
        /// Indicates if the recipient is selected to receive the email notification about being moved off the wait-list.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> RegisteredNames { get; set; }

    }
}
