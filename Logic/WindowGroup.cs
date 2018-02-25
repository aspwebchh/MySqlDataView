using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDataView.Logic {
    public class WindowGroup {
        public WindowGroup() {
            this.Items = new List<WindowObject>();
        }

        public List<WindowObject> Items {
            get;
            private set;
        }
    }
}
