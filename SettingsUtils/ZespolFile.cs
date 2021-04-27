using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZespolLib;

namespace ZespolWpfGui.SettingsUtils
{
    [Serializable]
    public class ZespolFile : MemberWiseCloneable<ZespolFile>
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
