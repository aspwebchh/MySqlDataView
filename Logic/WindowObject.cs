using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServiceCaller.Logic {
    public class WindowObject {
        public WindowObject() {
            this.Items = new List<WindowItem>();
        }


        public string TableName {
            get;
            set;
        }

        public string Title {
            get;set;
        }

        public WindowType Type {
            get;set;
        }

        public string SortField {
            get;set;
        }

        public string SortMode {
            get;set;
        }

        public List<WindowItem> Items {
            get;
            private set;
        }

        public static WindowType GetType( string type ) {
            if( type == "GetList" ) {
                return WindowType.GetList;
            } else if( type == "SubmitData" ) {
                return WindowType.SubmitData;
            } else {
                throw new XmlConfigParseError("Window类型不存在");
            }
        }
    }
}
