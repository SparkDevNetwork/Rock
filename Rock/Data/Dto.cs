//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Rock.Data
{
    /// <summary>
    /// Data transfer object
    /// </summary>
    [Serializable]
    public abstract class Dto : IDto, DotLiquid.ILiquidizable
    {
        /// <summary> 
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [DataMember]
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        [DataMember]
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Creates a dictionary object.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "Id", this.Id );
            dictionary.Add( "Guid", this.Guid );
            return dictionary;
        }

        /// <summary>
        /// Sets property values from a dictionary
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public virtual IDto FromDictionary( IDictionary<string, object> dictionary )
        {
            var properties = this.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance )
                .Where( p => dictionary.ContainsKey( p.Name ) );

            foreach ( var prop in properties )
            {
                try { prop.SetValue( this, dictionary[prop.Name] ); }
                catch ( Exception ) { }
            }

            return this;
        }

        /// <summary>
        /// Creates a dynamic object.
        /// </summary>
        /// <returns></returns>
        public virtual dynamic ToDynamic()
        {
            dynamic expando = new ExpandoObject();
            expando.Id = this.Id;
            expando.Guid = this.Guid;
            return expando;
        }

        /// <summary>
        /// Froms the dynamic.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public virtual IDto FromDynamic( object obj )
        {
            var o = obj as ExpandoObject;
            if ( o != null )
            {
                var dict = o as IDictionary<string, object>;
                if ( dict != null )
                {
                    FromDictionary( dict );
                }
            }

            return this;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public virtual void CopyFromModel( IEntity model )
        {
            this.Id = model.Id;
            this.Guid = model.Guid;
        }

        /// <summary>
        /// Copies to model.
        /// </summary>
        /// <param name="model">The model.</param>
        public virtual void CopyToModel( IEntity model )
        {
            model.Id = this.Id;
            model.Guid = this.Guid;
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        public virtual object ToLiquid()
        {
            return this.ToDictionary();
        }
    }
}