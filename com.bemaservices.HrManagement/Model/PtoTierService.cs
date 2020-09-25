// <copyright>
// Copyright by BEMA Information Technologies
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using Rock;
using Rock.Data;

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PtoTierService : Service<PtoTier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoTierService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PtoTierService( RockContext context ) : base( context ) { }

        private void InitModel<T>( ref T model ) where T : Rock.Data.IModel
        {
            model.CreatedByPersonAlias = null;
            model.CreatedByPersonAliasId = null;
            model.CreatedDateTime = RockDateTime.Now;
            model.ModifiedByPersonAlias = null;
            model.ModifiedByPersonAliasId = null;
            model.ModifiedDateTime = RockDateTime.Now;
            model.Id = 0;
            model.Guid = Guid.NewGuid();
        }

        public int Copy( int ptoTierId )
        {
            var ptoTier = this.Get( ptoTierId );
            RockContext rockContext = ( RockContext ) Context;

            int newPtoTierId = 0;

            PtoTier newPtoTier = new PtoTier();

            rockContext.WrapTransaction( () =>
            {
                newPtoTier.CopyPropertiesFrom( ptoTier );
                InitModel( ref newPtoTier );
                newPtoTier.Name = ptoTier.Name + " - Copy";
                this.Add( newPtoTier );
                rockContext.SaveChanges();

                newPtoTierId = newPtoTier.Id;

            } );

            rockContext.SaveChanges();

            foreach ( var ptoBracket in ptoTier.PtoBrackets )
            {
                var newPtoBracket = ptoBracket.Clone( false );
                newPtoBracket.CopyPropertiesFrom( ptoBracket );
                InitModel( ref newPtoBracket );
                newPtoBracket.PtoTierId = newPtoTierId;
                newPtoTier.PtoBrackets.Add( newPtoBracket );
                rockContext.SaveChanges();
                foreach ( var ptoBracketType in ptoBracket.PtoBracketTypes )
                {
                    var newPtoBracketType = ptoBracketType.Clone( false );
                    InitModel( ref newPtoBracketType );
                    newPtoBracketType.CopyPropertiesFrom( ptoBracketType );
                    newPtoBracketType.PtoBracketId = newPtoBracket.Id;
                    newPtoBracket.PtoBracketTypes.Add( newPtoBracketType );
                }

            }

            return newPtoTierId;
        }

    }

    public static partial class PtoTeirExtensionMethods
    {
        /// <summary>
        /// Clones this PtoTier object to a new PtoTier object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static PtoTier Clone( this PtoTier source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as PtoTier;
            }
            else
            {
                var target = new PtoTier();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another PtoTier object to this PtoTier object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this PtoTier target, PtoTier source )
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Description = source.Description;
            target.IsActive = source.IsActive;
            target.Color = source.Color;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
