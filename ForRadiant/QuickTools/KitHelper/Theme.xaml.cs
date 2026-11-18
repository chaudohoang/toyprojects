using System.Windows;

namespace KitHelper
{
    // Dark styles shared by every window. A class (not a pack URI) so loading it does not
    // depend on the assembly name, which contains a space.
    public partial class Theme : ResourceDictionary
    {
        public Theme() { InitializeComponent(); }
    }
}
