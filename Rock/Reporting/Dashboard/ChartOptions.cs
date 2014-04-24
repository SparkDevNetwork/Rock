using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Reporting.Dashboard
{
    /// <summary>
    /// Class that can be JSON'd and used for Google Charts (properties are case sensitive)
    /// </summary>
    public class ChartOptions
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int? width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int? height { get; set; }
    }

    /// <summary>
    /// Class that can be JSON'd and used for Google Charts (properties are case sensitive)
    /// </summary>
    public class ChartTooltip : ColumnDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartTooltip"/> class.
        /// </summary>
        public ChartTooltip()
            : base( "Tooltip", ColumnDataType.@string )
        {
            role = "tooltip";
        }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        /// <value>
        /// The role.
        /// </value>
        public string role { get; set; }
    }

    /// <summary>
    /// Class that can be JSON'd and used for Google Charts (properties are case sensitive)
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDefinition"/> class.
        /// </summary>
        /// <param name="Label">The label.</param>
        /// <param name="type">The type.</param>
        public ColumnDefinition( string label, ColumnDataType type )
        {
            this.label = label;
            this.type = type.ConvertToString();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string label { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string type { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ColumnDataType
    {
        /// <summary>
        /// The date
        /// </summary>
        date,
        
        /// <summary>
        /// The number
        /// </summary>
        number,
        
        /// <summary>
        /// The string
        /// </summary>
        @string
    }
}
