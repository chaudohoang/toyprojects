using System.Windows;

namespace QuickLauncher
{
    // Kit Helper's dark styles, plus the game tile. A class (not a pack URI) so loading it does
    // not depend on the assembly name, which contains a space.
    public partial class Theme : ResourceDictionary
    {
        public Theme() { InitializeComponent(); }
    }
}
