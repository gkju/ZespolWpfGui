using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZespolLib;

namespace ZespolWpfGui.SettingsUtils
{
    public class RemoteZespol : MemberWiseCloneable<RemoteZespol>
    {
        public Zespol zespol { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string RemoteName { get; set; }
        public string RemoteUrl { get; set; }
    }
}
