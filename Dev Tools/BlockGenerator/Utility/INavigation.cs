using System.Threading.Tasks;
using System.Windows.Controls;

namespace BlockGenerator.Utility
{
    public interface INavigation
    {
        Task PushPageAsync( Page page );

        Task PopPageAsync();

        Task PopToRootAsync();
    }
}
