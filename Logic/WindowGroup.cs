using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceCaller.Logic {
    public class WindowGroup {
        public WindowGroup() {
            this.Items = new List<WindowObject>();
        }
        public string Title {
            get;set;
        }

        public List<WindowObject> Items {
            get;
            private set;
        }
    }
}
