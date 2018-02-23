﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySqlDataView.Logic;
using MySqlDataView.Common;

namespace MySqlDataView {
    /// <summary>
    /// UIItemDetail.xaml 的交互逻辑
    /// </summary>
    public partial class UIItemDetail : Window {
        private WindowObject window;
        private IDictionary<string, object> sourceData;

        public UIItemDetail() {
            InitializeComponent();
        }

        public UIItemDetail( WindowObject window, string id ) {
            InitializeComponent();

            this.window = window;

            var fields = string.Join( ",", window.DetailItems.Select( item => item.Name ).ToArray() );
            var sql = "select " + fields + " from " + window.TableName + " where " + window.UniqueID + "=" + id;
            var result = DbHelperMySqL.Query( sql );
            if( result.Tables[ 0 ].Rows.Count > 0 ) {
                var dicList = Data2Object.Convert( result.Tables[ 0 ] );
                var oneDic = sourceData = dicList[ 0 ] as IDictionary<string, object>;
                var objList = Dic2List( window.DetailItems, oneDic );
                ContentList.ItemsSource = objList;
            }
        }

        private List<Object> Dic2List( List<WindowItem> items, IDictionary<string, object> data ) {
            Func<String,String> getTitle = delegate (string key) {
                var found = items.Find( windowItem => key == windowItem.Name );
                if( found != null ) {
                    return found.Title;
                } else {
                    return key;
                }
            };
            Func<KeyValuePair<string, object>, String> getVal = delegate ( KeyValuePair<string,object> keyVal  ) {
                var found = items.Find( windowItem => keyVal.Key == windowItem.Name );
                const int VALUE_LEN = 100;
                if( found != null ) {
                    if( found.DataType == WindowItemDataType.Html ) {
                        return "html字符串";
                    } else {
                        return Common.Common.GetString( keyVal.Value.ToString(), VALUE_LEN );
                    }
                } else {
                    return Common.Common.GetString( keyVal.Value.ToString(), VALUE_LEN );
                }
            };
            return data.Select( item => new {
                Name = getTitle(item.Key),
                Value = getVal(item)
            } ).ToList<Object>();
        }

        private string FindKeyByTitleFromWindow(string title) {
            var found = window.DetailItems.Find( windowItem => title == windowItem.Title );
            if( found != null ) {
                return found.Name;
            } else {
                return "";
            }
        }

        private string FindValueByKey( string key ) {
            if( string.IsNullOrEmpty( key ) ) {
                return "";
            } else {
                return sourceData[key].ToString();
            }
        }

        private void MenuItem_Click_View_Html( object sender, RoutedEventArgs e ) {
            var selectItem = ContentList.SelectedItem;
            var type = selectItem.GetType();
            var name = type.GetProperty( "Name" ).GetValue( selectItem ).ToString();
            var key = FindKeyByTitleFromWindow( name );
            var text = FindValueByKey( key );

            var htmlView = new UIHtmlView( text );
            htmlView.Owner = this;
            htmlView.ShowDialog();            
        }
    }
}
