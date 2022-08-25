using System.Threading.Tasks;
using System.Windows.Controls;

namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// Represents an object that supports page navigation.
    /// </summary>
    public interface INavigation
    {
        /// <summary>
        /// Pushes the page onto the navigation stack.
        /// </summary>
        /// <param name="page">The page to be pushed onto the navigation stack.</param>
        /// <returns>A <see cref="Task"/> that represents when the page has been pushed.</returns>
        Task PushPageAsync( Page page );

        /// <summary>
        /// Removes the top-most page off the navigation stack.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents when the page has been removed.</returns>
        Task PopPageAsync();

        /// <summary>
        /// Removes all pages from the navigation stack except for the root page.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents when all pages have been removed.</returns>
        Task PopToRootAsync();
    }
}
