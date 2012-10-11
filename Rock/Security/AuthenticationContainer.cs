//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Web;

using Rock.Extension;

namespace Rock.Security
    
	/// <summary>
	/// MEF Container class for Authentication Componenets
	/// </summary>
    public class AuthenticationContainer : Container<AuthenticationComponent, IComponentData>
        
        private static AuthenticationContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static AuthenticationContainer Instance
            
            get
                
                if ( instance == null )
                    instance = new AuthenticationContainer();
                return instance;
            }
        }

        private AuthenticationContainer()
            
            Refresh();
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( AuthenticationComponent ) )]
        protected override IEnumerable<Lazy<AuthenticationComponent, IComponentData>> MEFComponents      get; set; }
#pragma warning restore
    }
}