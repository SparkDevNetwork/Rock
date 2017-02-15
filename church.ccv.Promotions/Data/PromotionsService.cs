// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;
using church.ccv.Promotions.Model;
using System.Data.Entity;
using Rock.Data;

namespace church.ccv.Promotions.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PromotionsService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromotionsService{T}"/> class.
        /// </summary>
        /// <param name="promotionsContext">The promotion context.</param>
        public PromotionsService( RockContext rockContext )
            : base( rockContext )
        {
            //PromotionsContext = promotionsContext;
        }

        /// <summary>
        /// Gets the promotion context.
        /// </summary>
        /// <value>
        /// The promotion context.
        /// </value>
        //public PromotionsContext PromotionsContext { get; private set; }

        //public DbSet<PromotionOccurrence> PromotionOccurrence { get; set; }
        //public DbSet<PromotionRequest> PromotionRequest { get; set; }
    }
}
