using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlDataView.Logic;

namespace MySqlDataView.Common {
    public class Data2Object {

        public static DataTable ToListDataTable( DataTable dataTable, WindowObject window ) {
            foreach( DataRow row in dataTable.Rows ) {
                foreach( DataColumn col in dataTable.Columns ) {
                    if( col.DataType.FullName == "System.String" ) {
                        var items = from windowItem in window.ListItems where windowItem.Name == col.ColumnName select windowItem;
                        if( items.Count() > 0 ) {
                            var windowItem = items.ElementAt( 0 );
                            if( windowItem.DataType == WindowItemDataType.Html ) {
                                row[ col.ColumnName ] = Common.FilterHtml( row[ col.ColumnName ].ToString() );
                            }
                        }
                       // row[ col.ColumnName ] = Common.GetString( row[ col.ColumnName ].ToString(), 50 );
                    }
                }
            }
            return dataTable;
        }

        private static List<Object> Convert(DataTable data) {
            var objectList = new List<Object>();
            foreach( DataRow row in data.Rows ) {
                var dataItem = new System.Dynamic.ExpandoObject();
                var dataItemDic = dataItem as IDictionary<string, object>;
                foreach( DataColumn col in data.Columns ) {
                    var name = col.ColumnName;
                    var  val = row[ name ].ToString();
                    if( col.DataType.FullName == "MySql.Data.Types.MySqlDateTime" ) {
                        DateTime dt = DateTime.Parse( val );
                        val = dt.ToString( "yyyy-MM-dd HH:mm:ss" );
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
                foreach( var k in dicItem.Keys.ToList() ) {
                    foreach( var windowItem in hasContentsItems ) {
                        if( k == windowItem.Name ) {
                            var result = from keyVal in windowItem.Contents
                                     where keyVal.GetVal() == dicItem[ k ].ToString()
                                     select keyVal.GetKey();
                            if( result.Count() > 0 ) {
                                dicItem[ k ] = result.ToList()[ 0 ];
                            }
                        }
                    }
                }
                return dicItem;
            } ).ToList<object>();
        }
    }
}
