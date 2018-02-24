using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDataView.Logic {


    public  class WindowItem {

        public WindowItem() {
            this.Contents = new List<KeyVal<string, string>>();
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

        public WindowItemMatchType MatchType {
            get;set;
        }
        
        public List<KeyVal<string, string>> Contents {
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
            } else if(dataType == "Html") {
                return WindowItemDataType.Html;
            } else {
                throw new XmlConfigParseError( "DataType类型不存在" );
            }
        }

        public static WindowItemType GetItemType(string itemType) {
            if( itemType == "FilterItem" ) {
                return WindowItemType.FilterItem;
            } else if( itemType == "ListItem" ) {
                return WindowItemType.ListItem;
            } else if( itemType == "DetailItem" ) {
                return WindowItemType.DetailItem;
            } else {
                throw new XmlConfigParseError( "ItemType类型不存在" );
            }
        }

        public static WindowItemMatchType GetMatchType( string matchType, WindowItemType itemType ) {
            if( itemType != WindowItemType.FilterItem) {
                return WindowItemMatchType.Null;
            }
            if( matchType == "Like" ) {
                return WindowItemMatchType.Like;
            } else if( matchType == "Equals" ) {
                return WindowItemMatchType.Equals;
            } else if( matchType == "GT" ) {
                return WindowItemMatchType.GT;
            } else if( matchType == "LT" ) {
                return WindowItemMatchType.LT;
            } else {
                throw new XmlConfigParseError( "MatchType类型不存在" );
            }
        }
    }
}
