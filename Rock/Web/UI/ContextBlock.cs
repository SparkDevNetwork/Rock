using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Crm;

namespace Rock.Web.UI
{
	/// <summary>
	/// A Block that requires a model context.
	/// </summary>
	[Rock.Attribute.Property( 0, "Entity", "Filter", "Entity Name", false, "" )]
	public class ContextBlock : Block
	{
		/// <summary>
		/// Type of entity to get context for
		/// </summary>
		protected string EntityType { get; private set; }

		/// <summary>
		/// The current entity (context item)
		/// </summary>
		protected Rock.Data.IEntity Entity { get; private set; }

		/// <summary>
		/// Gets a list of any context entities that the block requires.
		/// </summary>
		public override List<string> RequiredContext
		{
			get
			{
				var requiredContext = base.RequiredContext;
				requiredContext.Add( AttributeValue( "Entity" ) );
				return requiredContext;
			}
		}

		protected override void OnInit( EventArgs e )
		{
			EntityType = AttributeValue( "Entity" );
			if ( string.IsNullOrWhiteSpace( EntityType ) )
				EntityType = PageParameter( "Entity" );

			Entity = CurrentPage.GetCurrentContext( EntityType );

			base.OnInit( e );
		}
	}

}