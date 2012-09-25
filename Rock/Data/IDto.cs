using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Data
{
	public interface IDto
	{
		int Id { get; set; }
		Guid Guid { get; set; }
		void CopyFromModel( IModel model );
		void CopyToModel( IModel model );
	}
}
