using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDataView.Logic {
   public  class XmlConfigParseError : Exception{
        public XmlConfigParseError( string msg ) : base(msg) {
           
        }
    }
}
