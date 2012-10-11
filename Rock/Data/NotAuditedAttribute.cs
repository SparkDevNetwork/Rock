//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Data
    
    /// <summary>
    /// Custom attribute used to decorate model classes or specific model properties that should 
	/// not be audited.  If attributed to class, no class changes will be audited, if attributed
	/// to properties, changes will be audited only if properties withouth attribute have changed.
	/// This would typically include logging tables (i.e. Exception, EntityChange, etc)
	/// Specific properties can also 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property )]
    public class NotAuditedAttribute : System.Attribute
        
    }
}