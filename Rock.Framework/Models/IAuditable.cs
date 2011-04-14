using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Models
{
    internal interface IAuditable
    {
        int Id { get; }
        DateTime? ModifiedDateTime { get; set; }
        DateTime? CreatedDateTime { get; set; }
        int? CreatedByPersonId { get; set; }
        int? ModifiedByPersonId { get; set; }
    }
}
