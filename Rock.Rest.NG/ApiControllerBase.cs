using System;

using Microsoft.AspNetCore.Mvc;

using Rock.Data;

namespace Rock.Rest
{
    /// <summary>
    /// Base class for Rock API controllers. Provides various helpful methods.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    public abstract class ApiControllerBase : ControllerBase
    {
        #region Constants

        /// <summary>
        /// The application/json media type.
        /// </summary>
        public const string MediaTypeApplicationJson = "application/json";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <value>
        /// The database context.
        /// </value>
        protected RockContext DataContext { get; }

        private Lazy<Rock.Model.Person> _currentPerson;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiControllerBase"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        protected ApiControllerBase( RockContext dataContext )
        {
            DataContext = dataContext;
            _currentPerson = new Lazy<Rock.Model.Person>( () => new Rock.Model.PersonService( dataContext ).Get( 1 ) );
        }

        #endregion

        /// <summary>
        /// Gets the current logged in person.
        /// </summary>
        /// <returns>The <see cref="Rock.Model.Person"/> that represents the logged in person or <c>null</c> if there isn't one.</returns>
        [NonAction]
        public virtual Rock.Model.Person GetCurrentPerson()
        {
            return _currentPerson.Value;
        }
    }
}
