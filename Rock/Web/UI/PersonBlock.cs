using Rock.Model;

namespace Rock.Web.UI
{
    /// <summary>
    /// Block displaying some information about a person model
    /// </summary>
    [ContextAware( typeof(Person) )]
    public abstract class PersonBlock : ContextEntityBlock
    {        
    }
}
