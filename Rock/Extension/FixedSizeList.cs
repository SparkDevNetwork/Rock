// <copyright>
// Copyright by the Spark Development Network
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
using System.Collections.Generic;

namespace Rock.Extension
{
    /// <summary>
    /// List that automatically delete oldest entries when it is bigger than preset size
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.List{T}" />
    public class FixedSizeList<T> : List<T>
    {
        /// <summary>
        /// Gets the list size.
        /// </summary>
        /// <value>
        /// The list size.
        /// </value>
        public int Size { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedSizeList{T}"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public FixedSizeList(int size) : base()
        {
            Size = size;
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="T:System.Collections.Generic.List`1" />. Truncate from the start of the list if it is bigger than the preset size.
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
        public new void Add(T item)
        {
            base.Add( item );
            RemoveExtra();
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="T:System.Collections.Generic.List`1" />. Truncate from the start of the list if it is bigger than the preset size.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        public new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange( collection );
            RemoveExtra();
        }

        /// <summary>
        /// Truncate from the start of the list if it is bigger than the preset size.
        /// </summary>
        private void RemoveExtra()
        {
            if ( this.Count > Size )
            {
                this.RemoveRange( 0, this.Count - Size );
            }
        }
    }
}
