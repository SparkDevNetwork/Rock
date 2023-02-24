using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister
{
    /// <summary>
    /// The information about registrants to be registered, updated or unregistered when registering for a sign-up project occurrence.
    /// </summary>
    public class SignUpRegisterRequestBag
    {
        /// <summary>
        /// Gets or sets the registrants to be registered, updated or unregistered.
        /// </summary>
        /// <value>
        /// The registrants to be registered, updated or unregistered.
        /// </value>
        public List<SignUpRegistrantBag> Registrants { get; set; }
    }
}
