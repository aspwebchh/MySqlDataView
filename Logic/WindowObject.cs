using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MySqlDataView.Logic {
    public class WindowObject {
        public WindowObject() {
            this.Items = new List<WindowItem>();
        }


       public ListView ListView {
            get;set;
        }

        public string TableName {
            get;
            set;
        }

        public string UniqueID {
            get;set;
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

        public List<WindowItem> ListItems {
            get {
                return Items.Where( item => item.ItemType == WindowItemType.ListItem ).ToList();
            }
        }

        public List<WindowItem> FilterItems {
            get {
                return Items.Where( item => item.ItemType == WindowItemType.FilterItem ).ToList();
            }
        }

        public List<WindowItem> DetailItems {
            get {
                return Items.Where( item => item.ItemType == WindowItemType.DetailItem ).ToList();
            }
        }

        public List<WindowItem> ExportItems {
            get {
                return Items.Where( item => item.ItemType == WindowItemType.ExportItem ).ToList();
            }
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
