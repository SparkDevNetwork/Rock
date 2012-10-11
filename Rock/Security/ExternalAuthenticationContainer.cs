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
	/// MEF Container for External Authentication Components
	/// </summary>
	public class ExternalAuthenticationContainer : Container<ExternalAuthenticationComponent, IComponentData>
        
		private static ExternalAuthenticationContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
		public static ExternalAuthenticationContainer Instance
            
            get
                
                if ( instance == null )
                    instance = new ExternalAuthenticationContainer();
                return instance;
            }
        }

		private ExternalAuthenticationContainer()
            
            Refresh();
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( ExternalAuthenticationComponent ) )]
		protected override IEnumerable<Lazy<ExternalAuthenticationComponent, IComponentData>> MEFComponents      get; set; }
#pragma warning restore
    }
}