//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Data
{
    /// <summary>
    /// derived from http://mrbigglesworth79.blogspot.in/2011/12/partial-validation-with-data.html
    /// </summary>
    public class IgnoreModelErrorsAttribute : System.Attribute
    {
        /// <summary>
        /// The keys string
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreModelErrorsAttribute"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public IgnoreModelErrorsAttribute( string[] keys )
            : base()
        {
            this.Keys = keys;
        }
    }
}
