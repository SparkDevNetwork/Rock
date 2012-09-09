using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Crm;

namespace Rock.Web.UI
{
	/// <summary>
	/// A Block used on the person detail page
	/// </summary>
	public class PersonBlock : Block
	{
		/// <summary>
		/// The current person being viewed
		/// </summary>
		protected Person Person { get; set; }

		public override List<string> RequiredContext
		{
			get
			{
				var requiredContext = base.RequiredContext;
				requiredContext.Add( "Rock.Crm.Person" );
				return requiredContext;
			}
		}

		protected override void OnLoad( EventArgs e )
		{
			this.Person = PageInstance.GetCurrentContext( "Rock.Crm.Person" ) as Rock.Crm.Person;

			base.OnLoad( e );
		}
	}

}