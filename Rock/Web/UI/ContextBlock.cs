using System;
using System.Collections.Generic;

namespace Rock.Web.UI
{
    /// <summary>
    /// A RockBlock that requires a model context.
    /// </summary>
    [Rock.Attribute.Property( 0, "Entity", "Filter", "Entity Name", false, "" )]
    public class ContextBlock : RockBlock
    {
        /// <summary>
        /// Type of entity to get context for
        /// </summary>
        protected string EntityType { get; private set; }

        /// <summary>
        /// The current entity (context item)
        /// </summary>
        protected Rock.Data.IModel Entity { get; private set; }

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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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