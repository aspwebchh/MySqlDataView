using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlDataView.Logic;

namespace MySqlDataView.Common {
    public class Data2Object {
        public static List<Object> Convert(DataTable data) {
            var objectList = new List<Object>();
            foreach( DataRow row in data.Rows ) {
                var dataItem = new System.Dynamic.ExpandoObject();
                var dataItemDic = dataItem as IDictionary<string, object>;
                foreach( DataColumn col in data.Columns ) {
                    var name = col.ColumnName;
                    var  val = row[ name ].ToString();
                    if( col.DataType.FullName == "MySql.Data.Types.MySqlDateTime" ) {
                        DateTime dt = DateTime.Parse( val );
                        val = dt.ToString( "yyyy-MM-dd hh:mm:ss" );
                    } 
                    dataItemDic[ name ] = val;
                }
                objectList.Add( dataItem );
            }
            return objectList;
        }

        public static List<Object> Convert( DataTable data, WindowObject window ) {
            var objectList = Convert( data );
            var hasContentsItems = window.ListItems.Where( item => item.Contents.Count > 0 ).ToList();    
            if( hasContentsItems.Count == 0 ) {
                return objectList;
            }
            return objectList.Select( item => {
                var dicItem = item as IDictionary<string, object>;
                hasContentsItems.ForEach( windowItem => {
                    var found = windowItem.Contents.Find( keyVal => keyVal.GetVal() == dicItem[ windowItem.Name ].ToString() );
                    if( found != null ) {
                        dicItem[ windowItem.Name ] = found.GetKey();
                    }
                } );
                return dicItem;
            } ).ToList<object>();
        }
    }
}
