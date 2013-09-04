//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public sealed class ViewStateList<T> : IEnumerable<T> where T : Model<T>, new()
    {
        /// <summary>
        /// 
        /// </summary>
        private string internalListJson;

        [NonSerialized]
        private List<T> _cachedList;

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <returns></returns>
        private ReadOnlyCollection<T> GetList()
        {
            List<T> result;

            if ( _cachedList != null )
            {
                result = _cachedList;
            }
            else
            {
                result = JsonConvert.DeserializeObject<List<T>>( internalListJson );
                _cachedList = result;
            }

            return new ReadOnlyCollection<T>( result );
        }

        /// <summary>
        /// Serializes the list.
        /// </summary>
        /// <param name="list">The list.</param>
        private void SerializeList( List<T> list )
        {
            internalListJson = JsonConvert.SerializeObject( 
                list, 
                Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );

            _cachedList = list;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewStateList{T}" /> class.
        /// </summary>
        public ViewStateList()
        {
            SerializeList( new List<T>() );
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator<T> GetEnumerator()
        {
            return GetList().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetList().GetEnumerator();
        }

        /// <summary>
        /// Removes the entity.
        /// </summary>
        /// <param name="id">The id.</param>
        public void RemoveEntity( int id )
        {
            var list = GetList().ToList();
            list.RemoveEntity( id );
            SerializeList( list );
        }

        /// <summary>
        /// Removes the entity.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void RemoveEntity( Guid guid )
        {
            var list = GetList().ToList();
            list.RemoveEntity( guid );
            SerializeList( list );
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add( Model<T> item )
        {
            var list = GetList().ToList();
            list.Add( item as T );
            SerializeList( list );
        }

        /// <summary>
        /// Adds all.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddAll( List<T> items )
        {
            var list = GetList().ToList();
            items.ForEach( a => list.Add( a as T ) );
            SerializeList( list );
        }

        /// <summary>
        /// Inserts all.
        /// </summary>
        /// <param name="items">The items.</param>
        public void InsertAll( List<T> items )
        {
            var list = GetList().ToList();
            items.Reverse();
            items.ForEach( a => list.Insert( 0, a as T ) );
            SerializeList( list );
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            SerializeList( new List<T>() );
        }
    }
}