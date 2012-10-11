//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Security
    
    /// <summary>
    /// A class Attribute that can be used by objects that implement ISecured to add additional supported actions
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public class AdditionalActionsAttribute : System.Attribute
        
        /// <summary>
        /// Gets or sets the additional actions to support
        /// </summary>
        /// <value>
        /// The Actions.
        /// </value>
        public List<string> AdditionalActions      get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalActionsAttribute"/> class.
        /// </summary>
        /// <param name="actions">The actions.</param>
        public AdditionalActionsAttribute( string[] actions )
            
            this.AdditionalActions = actions.ToList<string>();
        }

    }
}