using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Rest
{
    public class RockAssembliesResolver : System.Web.Http.Dispatcher.IAssembliesResolver
    {
        public ICollection<Assembly> GetAssemblies()
        {
            return Reflection.GetPluginAssemblies();
        }
    }
}
