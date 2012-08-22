using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Data
{
    /// <summary>
    /// Data Transfer Object interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDTO<T>
    {
        /// <summary>
        /// Instantiate new data transfer object
        /// </summary>
        IDTO();

        /// <summary>
        /// Instantiate new data transfer object from model
        /// </summary>
        /// <param name="model"></param>
        IDTO( T model );

        /// <summary>
        /// Copy data from model
        /// </summary>
        /// <param name="model"></param>
        void CopyFromModel( T model );

        /// <summary>
        /// Copy data to model
        /// </summary>
        /// <param name="model"></param>
        void CopyToModel( T model );
    }
}