using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Rock.Data
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RockPluginDBInitializer<T> : IDatabaseInitializer<T> where T : DbContext
        
        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void InitializeDatabase( T context )
            
            throw new NotImplementedException();
        }
    }
}