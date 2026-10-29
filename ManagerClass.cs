    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    internal class ManagerClass
    {
        public static Frame MainFrame { get; set; }
        public static Users CurrentUser { get; set; }
        public static List<OrderProducts> Cart = new List<OrderProducts>();
    }
}
