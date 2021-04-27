using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZespolLib;

namespace ZespolWpfGui.SettingsUtils
{
    public class ServerRemote : MemberWiseCloneable<ServerRemote>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
}
