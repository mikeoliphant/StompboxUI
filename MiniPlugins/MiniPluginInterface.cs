using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UILayout;

namespace MiniPlugins
{
    public class MiniPluginInterface : Dock
    {
        public MiniPluginInterface()
        {
            Children.Add(new TextBlock("Hello"));
        }
    }
}
