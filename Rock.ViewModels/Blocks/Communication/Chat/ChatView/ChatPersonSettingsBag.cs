using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatView
{
    /// <summary>
    /// A bag of settings for a chat person.
    /// </summary>
    public class ChatPersonSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the chat is open direct message allowed.
        /// </summary>
        public bool IsChatOpenDirectMessageAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the chat profile is public.
        /// </summary>
        public bool IsChatProfilePublic { get; set; }
    }
}
