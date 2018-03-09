using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDataView.Logic {
    public class Product {
        private List<ConnectionString> connectionStrings = new List<Logic.ConnectionString>();

        public String Name {
            get; set;
        }

        public String ConnectionString {
            get; set;
        }

        public String ID {
            get;set;
        }

        public WindowGroup WindowGroup {
            get;set;
        }

        public override string ToString() {
            return  "name:" + Name + "\n" + "connectionString:" + ConnectionString + "\nID：" + ID ;
        }

        public List<ConnectionString> ConnectionStrings {
            get {
                return connectionStrings;
            }
        }
    }
}
