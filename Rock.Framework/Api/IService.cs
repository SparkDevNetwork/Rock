using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Api
{
    /// <summary>
    /// Interface used for the MEF import/export signature of all WCF REST Api services
    /// </summary>
    public interface IService
    {
    }

    /// <summary>
    /// Interface used for the MEF metadata of all WCF REST Api services
    /// </summary>
    public interface IServiceData
    {
        string RouteName { get; }
    }

}
