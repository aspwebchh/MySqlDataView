using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDataView.Logic {
    public class LoadExtConfigResult {
        public string Message {
            get;set;
        }

        public bool Success {
            get;set;
        }

        public List<KeyVal<string, string>> Data {
            get;set;
        }
    }
}
