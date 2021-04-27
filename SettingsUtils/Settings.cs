using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZespolWpfGui.SettingsUtils;

namespace ZespolWpfGui
{
    [Serializable]
    public class Settings
    {
        public List<ZespolFile> RememberedFiles { get; set; } = new List<ZespolFile>();
        public List<ServerRemote> RememberedRemotes { get; set; } = new List<ServerRemote>();

        public Settings()
        {

        }
    }
}
