using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDetailBlock
    {
        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        void ShowDetail( string itemKey, int itemKeyValue );
    }
}