using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZespolWpfGui.SettingsUtils
{
    [Serializable]
    public class ZespolFile
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public ZespolFile Clone()
        {
            return (ZespolFile) MemberwiseClone();
        }
    }
}
