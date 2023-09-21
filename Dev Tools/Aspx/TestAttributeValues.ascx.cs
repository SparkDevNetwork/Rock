using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    [DisplayName( "Test Attribute Values" )]
    [Category( "Examples" )]
    [Description( "Tests new attribute value code." )]
    [Rock.SystemGuid.BlockTypeGuid( "cad1b43e-4a40-4af7-94a8-c4da1af9b846" )]
    public partial class TestAttributeValues : RockBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            var loadAll = false;

            if ( !CompareEntities<Group>( loadAll, out var count, out var errorMessage ) )
            {
                ltMessage.Text = errorMessage;
            }
            else
            {
                ltMessage.Text = $"{count:N0} records were equal.";
            }
        }

        private bool CompareEntities<TEntity>( bool loadAll, out int count, out string errorMessage )
            where TEntity : Rock.Data.Entity<TEntity>, IEntity, IHasAttributes, new()
        {
            using ( var rockContextA = new RockContext() )
            {
                var serviceA = rockContextA.Set<TEntity>();
                var setA = serviceA.Where( a => loadAll || a.Id == 2 ).OrderBy( a => a.Id ).ToList();

                setA.LoadAttributes( rockContextA );

                using ( var rockContextB = new RockContext() )
                {
                    var serviceB = rockContextB.Set<TEntity>();
                    var setB = serviceB.Where( a => loadAll || a.Id == 2 ).OrderBy( a => a.Id ).ToList();

                    using ( var rockContextC = new RockContext() )
                    {
                        var serviceC = rockContextC.Set<TEntity>();
                        var setC = serviceC.Where( a => loadAll || a.Id == 2 ).OrderBy( a => a.Id ).ToList();

                        if ( !CompareEntities( setA, setB, setC, rockContextB, rockContextC, out errorMessage ) )
                        {
                            count = 0;
                            return false;
                        }
                        else
                        {
                            count = setA.Count;
                            return true;
                        }
                    }
                }
            }
        }

        private bool CompareEntities<TEntity>( List<TEntity> setA, List<TEntity> setB, List<TEntity> setC, RockContext rockContextB, RockContext rockContextC, out string errorMessage )
            where TEntity : IHasAttributes
        {
            if ( setA.Count != setB.Count )
            {
                errorMessage = "Entity set count mismatch.";
                return false;
            }

            for ( int i = 0; i < setA.Count; i++ )
            {
                if ( !CompareEntityAttributes<TEntity>( setA[i], setB[i], setC[i], rockContextB, rockContextC, out errorMessage ) )
                {
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        private bool CompareEntityAttributes<TEntity>( IHasAttributes entityA, IHasAttributes entityB, IHasAttributes entityC, RockContext rockContextB, RockContext rockContextC, out string errorMessage )
        {
            if ( entityA.Id != entityB.Id || entityA.Id != entityC.Id )
            {
                errorMessage = $"Identifier mismatch: '{entityA}:{entityA.Id}', '{entityB}:{entityB.Id}', '{entityC}:{entityC.Id}'.";
                return false;
            }

            entityB.LoadAttributes( rockContextB );

            var entityCAttributes = Helper.LoadAttributes( typeof( TEntity ), entityC.Id, rockContextC );

            if ( entityA.Attributes.Count != entityB.Attributes.Count || entityA.Attributes.Count != entityCAttributes.Attributes.Count )
            {
                errorMessage = $"Attribute count mismatch in '{entityA}:{entityA.Id}'.";
                return false;
            }

            var keysA = entityA.Attributes.Keys.OrderBy( k => k ).ToList();
            var keysB = entityB.Attributes.Keys.OrderBy( k => k ).ToList();
            var keysC = entityCAttributes.Attributes.Keys.OrderBy( k => k ).ToList();

            for ( int i = 0; i < keysA.Count; i++ )
            {
                if ( keysA[i] != keysB[i] || keysA[i] != keysC[i] )
                {
                    errorMessage = $"Attribute index {i} mismatched keys: '{keysA[i]}', '{keysB[i]}', '{keysC[i]}' in '{entityA}:{entityA.Id}'.";
                    return false;
                }

                var key = keysA[i];
                var valueA = entityA.GetAttributeValue( key );
                var valueB = entityB.GetAttributeValue( key );
                var valueC = entityCAttributes.GetAttributeValue( key );

                if ( valueA.IsNullOrWhiteSpace() && valueB.IsNullOrWhiteSpace() && valueC.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                if ( valueA != valueB || valueA != valueB )
                {
                    errorMessage = $"Attribute '{key}' value mismatch: {QuotedValue( valueA )}, {QuotedValue( valueB )}, {QuotedValue( valueC )} in '{entityA}:{entityA.Id}'.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        private static string QuotedValue( string value )
        {
            return value == null ? "null" : $"'{value}'";
        }
    }
}
