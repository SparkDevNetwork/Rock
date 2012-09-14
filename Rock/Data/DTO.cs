//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Data
{
    public abstract class Dto<T>
        where T : Model<T>
    {
        /// <summary>
        /// The Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the Created Date Time.
        /// </summary>
        /// <value>
        /// Created Date Time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Modified Date Time.
        /// </summary>
        /// <value>
        /// Modified Date Time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Created By Person Id.
        /// </summary>
        /// <value>
        /// Created By Person Id.
        /// </value>
        public int? CreatedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the Modified By Person Id.
        /// </summary>
        /// <value>
        /// Modified By Person Id.
        /// </value>
        public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Initializes a new instance of the data transformation object.
        /// </summary>
        public Dto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the data transformation object from a model
        /// </summary>
        /// <param name="model"></param>
        public Dto(T model)
        {
        }

        /// <summary>
        /// Copies properties to the model
        /// </summary>
        /// <param name="dto"></param>
        public virtual void CopyFromModel( T model )
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Copies properties from the model
        /// </summary>
        /// <param name="model"></param>
        public virtual void CopyToModel( T model )
        {
            throw new System.NotImplementedException();
        }

	}
}