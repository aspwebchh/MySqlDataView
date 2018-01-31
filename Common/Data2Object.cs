using System;
using System.Collections.Generic;
using System.Data;

namespace WebServiceCaller.Common {
    public class Data2Object {
        public static List<Object> Convert(DataTable data) {
            var objectList = new List<Object>();
            foreach( DataRow row in data.Rows ) {
                dynamic dataItem = new System.Dynamic.ExpandoObject();
                var dataItemDic = dataItem as IDictionary<string, object>;
                foreach( DataColumn col in data.Columns ) {
                    var name = col.ColumnName;
                    var val = row[name].ToString();
                    dataItemDic[ name ] = val;
                }
                objectList.Add( dataItem );
            }

            return objectList;
        } 
    }
}
