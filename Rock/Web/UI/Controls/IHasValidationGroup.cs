//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// This is a generic interface that most Rock UI controls implement to indicate that they support
    /// having a validation group.  By default the RockBlock that any of these controls are added to 
    /// will automatically set their validation group to be a value unique to the instance of the block
    /// </summary>
    public interface IHasValidationGroup
    {
        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        string ValidationGroup { get; set; }
    }
}
