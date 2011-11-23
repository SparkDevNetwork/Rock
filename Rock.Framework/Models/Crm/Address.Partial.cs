using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations;

namespace Rock.Models.Crm
{
    public partial class Address
    {
        internal void UpdateRaw()
        {
            if ( this.Raw.Trim() == string.Empty )
                this.Raw = string.Format( "{0} {1} {2}, {3} {4}",
                    this.Street1, this.Street2, this.City, this.State, this.Zip );
        }
    }
}
