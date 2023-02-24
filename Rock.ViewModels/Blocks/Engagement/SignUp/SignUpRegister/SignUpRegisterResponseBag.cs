using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister
{
    /// <summary>
    /// The information about registrants that were successfully registered, unregistered or unable to be registered, for a given register request.
    /// </summary>
    public class SignUpRegisterResponseBag
    {
        /// <summary>
        /// Gets or sets the full names of individuals who were successfully registered for a sign-up project.
        /// </summary>
        /// <value>
        /// The full names of individuals who were successfully registered for a sign-up project.
        /// </value>
        public List<string> RegisteredRegistrantNames { get; set; }

        /// <summary>
        /// Gets or sets the full names of individuals who were unregistered for a sign-up project.
        /// </summary>
        /// <value>
        /// The full names of individuals who were unregistered for a sign-up project.
        /// </value>
        public List<string> UnregisteredRegistrantNames { get; set; }

        /// <summary>
        /// Gets or sets the full names of individuals who were unable to be registered for a sign-up project.
        /// </summary>
        /// <value>
        /// The full names of individuals who were unable to be registered for a sign-up project.
        /// </value>
        public List<string> UnsuccessfulRegistrantNames { get; set; }
    }
}
