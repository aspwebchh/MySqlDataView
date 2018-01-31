using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceCaller.Logic {
    public class Product {
        public String Name {
            get; set;
        }

        public String ConnectionString {
            get; set;
        }

        public override string ToString() {
            return "name:" + Name + "\n" + "connectionString:" + ConnectionString;
        }
    }
}
