using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    interface ILabeledControl
    {
        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        string LabelText { get; set; }
    }
}
