//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using Rock.Extension;

namespace Rock.Address
    
    /// <summary>
    /// Singleton class that uses MEF to load and cache all of the StandardizeComponent classes
    /// </summary>
    public class StandardizeContainer : Container<StandardizeComponent, IComponentData>
        
        private static StandardizeContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static StandardizeContainer Instance
            
            get
                
                if ( instance == null )
                    instance = new StandardizeContainer();
                return instance;
            }
        }

        private StandardizeContainer()
            
            Refresh();
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( StandardizeComponent ) )]
        protected override IEnumerable<Lazy<StandardizeComponent, IComponentData>> MEFComponents      get; set; }
#pragma warning restore

    }
}