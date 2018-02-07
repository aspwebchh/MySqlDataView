using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServiceCaller.Logic {
   public  class WindowItem {
        public WindowItem() {
            this.Items = new List<WindowItem>();
        }

        public string Name {
            get;
            set;
        }

        public string Title {
            get;
            set;
        }

        public WindowItemType ItemType {
            get;
            set;
        }

        public WindowItemDataType DataType {
            get;set;
        }
        
        public List<WindowItem> Items {
            get;
            private set;
        }

        public static WindowItemDataType GetDataType( string dataType ) {
            if( dataType == "String" ) {
                return WindowItemDataType.String;
            } else if( dataType == "Int" ) {
                return WindowItemDataType.Integer;
            } else if( dataType == "DateTime" ) {
                return WindowItemDataType.DateTime;
            } else if( dataType == "Map" ) {
                return WindowItemDataType.Map;
            } else if( dataType == "List" ) {
                return WindowItemDataType.List;
            } else {
                throw new XmlConfigParseError( "DataType类型不存在" );
            }
        }

        public static WindowItemType GetItemType(string itemType) {
            if( itemType == "FilterItem" ) {
                return WindowItemType.FilterItem;
            } else if( itemType == "ListItem" ) {
                return WindowItemType.ListItem;
            } else {
                throw new XmlConfigParseError( "ItemType类型不存在" );
            }
        }
    }
}
