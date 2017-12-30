using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceCaller.Logic {
   public  class XmlConfigParseError : Exception{
        public XmlConfigParseError( string msg ) : base(msg) {
           
        }
    }
}
