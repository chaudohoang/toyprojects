using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunAs
{
    class TargetFile
    {
   

        // Auto-Impl Properties for trivial get and set
        public string FilePath { get; set; }
        public string Domain { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
        public string Hash { get; set; }
        public string LauncherFilePath { get; set; }
        public bool PathIsNetWork { get; set; }

        
      

    }
}
