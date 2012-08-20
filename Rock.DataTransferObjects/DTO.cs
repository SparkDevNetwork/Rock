using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock
{
    public abstract class DTO<T>
    {
        /// <summary>
        /// The Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the Created Date Time.
        /// </summary>
        /// <value>
        /// Created Date Time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Modified Date Time.
        /// </summary>
        /// <value>
        /// Modified Date Time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Created By Person Id.
        /// </summary>
        /// <value>
        /// Created By Person Id.
        /// </value>
        public int? CreatedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the Modified By Person Id.
        /// </summary>
        /// <value>
        /// Modified By Person Id.
        /// </value>
        public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDTO"/> class.
        /// </summary>
        public DTO()
        {
        }
    }
}
