using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceCaller.Logic {
    class Window {
        public string Title {
            get;set;
        }

        public WindowType Type {
            get;set;
        }

        public List<WindowItem> Items {
            get;set;
        }
    }
}
