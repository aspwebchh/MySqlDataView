using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace MySqlDataView.Logic {
    public class ExportExcel {
        public static void Export( List<object> dt, string worksheetName, List<WindowItem> windowItems, string savePath ) {

            if( dt != null && dt.Count > 0 ) {
                
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();

                if( excel == null ) {
                    return;
                }

                //设置为不可见，操作在后台执行，为 true 的话会打开 Excel
                excel.Visible = false;

                //打开时设置为全屏显式
                //excel.DisplayFullScreen = true;

                //初始化工作簿
                Microsoft.Office.Interop.Excel.Workbooks workbooks = excel.Workbooks;

                //新增加一个工作簿，Add（）方法也可以直接传入参数 true
                Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add( Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet );
                //同样是新增一个工作簿，但是会弹出保存对话框
                //Microsoft.Office.Interop.Excel.Workbook workbook = excel.Application.Workbooks.Add(true);

                //新增加一个 Excel 表(sheet)
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[ 1 ];

                //设置表的名称
                worksheet.Name = worksheetName;
                try {
                    //创建一个单元格
                    Microsoft.Office.Interop.Excel.Range range;

                    int rowIndex = 1;       //行的起始下标为 1
                    int colIndex = 1;       //列的起始下标为 1

                    var one = dt[ 0 ] as IDictionary<string,object>;
                    var currIndex = 0;

                    Func<string, string> findColumnName = key => {
                        var found = from windowItem in windowItems where windowItem.Name == key select windowItem.Title;
                        if( found.Count() > 0 ) {
                            return found.ElementAt(0);
                        }
                        return key;
                    };

                    foreach( var key in one.Keys ) {
                        //设置第一行，即列名
                        worksheet.Cells[ rowIndex, colIndex + currIndex ] = findColumnName( key );
                        //获取第一行的每个单元格
                        range = worksheet.Cells[ rowIndex, colIndex + currIndex ];
                        //设置单元格的内部颜色
                        range.Interior.ColorIndex = 33;
                        //字体加粗
                        range.Font.Bold = true;
                        //设置为黑色
                        range.Font.Color = 0;
                        //设置为宋体
                        range.Font.Name = "Arial";
                        //设置字体大小
                        range.Font.Size = 12;
                        //水平居中
                        range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        //垂直居中
                        range.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                        currIndex++;
                    }

                    //跳过第一行，第一行写入了列名
                    rowIndex++;


                    //写入数据
                    for( int i = 0; i < dt.Count; i++ ) {
                        var j = 0;
                        var item = dt[ i ] as IDictionary<string, object>;
                        foreach( var key in item.Keys ) {
                            worksheet.Cells[ rowIndex + i, colIndex + j ] = item[ key ].ToString();
                            j++;
                        }
                    }

                    //设置所有单元格列宽为自动列宽
                    worksheet.Cells.Columns.AutoFit();
                    //worksheet.Cells.EntireColumn.AutoFit();

                    //是否提示，如果想删除某个sheet页，首先要将此项设为fasle。
                    excel.DisplayAlerts = false;

                    //保存写入的数据，这里还没有保存到磁盘
                    workbook.Saved = true;

                    //设置导出文件路径
                    //string path = @"C:\Users\宏鸿\Desktop\configs\";

                    //设置新建文件路径及名称
                    //string savePath = path + worksheetName + "-" + DateTime.Now.ToString( "yyyy-MM-dd-HH-mm-ss" ) + ".xlsx";

                    //创建文件
                    //FileStream file = new FileStream( savePath, FileMode.CreateNew );

                    //关闭释放流，不然没办法写入数据
                    //file.Close();
                    //file.Dispose();

                    //保存到指定的路径
                    workbook.SaveCopyAs( savePath );

                } catch( Exception ) {

                } finally {
                    workbook.Close( false, Type.Missing, Type.Missing );
                    workbooks.Close();

                    //关闭退出
                    excel.Quit();

                    //释放 COM 对象
                    Marshal.ReleaseComObject( worksheet );
                    Marshal.ReleaseComObject( workbook );
                    Marshal.ReleaseComObject( workbooks );
                    Marshal.ReleaseComObject( excel );

                    worksheet = null;
                    workbook = null;
                    workbooks = null;
                    excel = null;

                    GC.Collect();
                }
            }
        }
    }
}
