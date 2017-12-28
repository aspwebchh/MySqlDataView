using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceCaller.Logic {
    class WindowItem {
        public string Name {
            get;
            set;
        }

        public string Title {
            get;
            set;
        }

        public WindowItemDataType DataType {
            get;set;
        }
        
        public List<WindowItem> Items {
            get;set;
        }
    }
}
