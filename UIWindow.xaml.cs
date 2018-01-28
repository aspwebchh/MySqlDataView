using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebServiceCaller.Logic;
using System.Windows.Forms.Integration;
using System.Data;
using MySql.Data.MySqlClient;


namespace WebServiceCaller {
    /// <summary>
    /// UIWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UIWindow : Window {
        private WindowObject windowInfo;

        public UIWindow(WindowObject windowInfo) {
            this.windowInfo = windowInfo;
            this.Title = this.windowInfo.Title;

            InitializeComponent();

            if( this.windowInfo.Type == WindowType.GetList ) {
                this.InitListPage();
            } else if( this.windowInfo.Type == WindowType.SubmitData ) {
                this.InitFormPage();
            }
        }


        #region

        private void InitFormPage() {
            foreach( var windowItem in this.windowInfo.Items ) {
                var itemControl = new WrapPanel();
                var title = new TextBlock();
                title.Text = windowItem.Title;
                var control = this.GenControlForFormPage( windowItem );
                itemControl.Children.Add( title );
                itemControl.Children.Add( control );
                Content.Children.Add( itemControl );
            }
        }


        private UIElement GenControlForFormPage( WindowItem windowItem ) {
            UIElement control;
            if( windowItem.DataType == WindowItemDataType.DateTime ) {
                var wfh = new WindowsFormsHost();
                var dtp = new System.Windows.Forms.DateTimePicker();
                wfh.Child = dtp;
                control = wfh;
            } else if( windowItem.DataType == WindowItemDataType.Integer ) {
                var textBox = new System.Windows.Controls.TextBox();
                textBox.Width = 100;
                control = textBox;
            } else if( windowItem.DataType == WindowItemDataType.String ) {
                var textBox = new System.Windows.Controls.TextBox();
                textBox.Width = 200;
                control = textBox;
            } else if( windowItem.DataType == WindowItemDataType.List ) {
                var list = this.CreateListControlForFormPage( windowItem.Items );
                control = list;
            } else {
                var textBox = new System.Windows.Controls.TextBox();
                textBox.Width = 200;
                control = textBox;
            }
            return control;
        }

        private System.Windows.Controls.ListView CreateListControlForFormPage( List<WindowItem> windowItemList ) {
            var listView = new System.Windows.Controls.ListView();
            var gridView = new GridView();
            foreach( var windowItem in windowItemList ) {
                var column = new GridViewColumn();
                column.Header = windowItem.Title;
                column.Width = 200;

                var binding = new Binding();
                binding.Path = new PropertyPath( windowItem.Name );
                var dtpl = new DataTemplate();
                var fef = new FrameworkElementFactory( typeof( TextBox ) );
                fef.SetBinding( TextBox.TextProperty, binding );
                fef.SetValue( TextBox.WidthProperty, 100.0 );
                dtpl.VisualTree = fef;
                column.CellTemplate = dtpl;
                gridView.Columns.Add( column );
            }
            listView.View = gridView;

            var testData = new List<object>();
            listView.ItemsSource = testData;

            return listView;
        }

        #endregion

        private void InitListPage() {
            var listView = new System.Windows.Controls.ListView();
            var gridView = new GridView();
            foreach( var windowItem in windowInfo.Items ) {
                var column = new GridViewColumn();
                var titleTemplate = new TextBlock();
                titleTemplate.Text = windowItem.Title;
                titleTemplate.TextAlignment = TextAlignment.Left;
                titleTemplate.Width = 200;
                column.Header = titleTemplate;

                var binding = new Binding();
                binding.Path = new PropertyPath( windowItem.Name );
                var dtpl = new DataTemplate();
                var fef = new FrameworkElementFactory( typeof( TextBlock ) );
                fef.SetBinding( TextBlock.TextProperty, binding );
               // fef.SetValue( TextBlock.WidthProperty, 100.0 );
                fef.SetValue( TextBlock.TextAlignmentProperty, TextAlignment.Center );
                dtpl.VisualTree = fef;
                column.CellTemplate = dtpl;
                gridView.Columns.Add( column );
            }
            listView.View = gridView;


            var testData = new List<object>();
            //testData.Add( new {
            //    title = "title",
            //    content = "content",
            //    add_time = DateTime.Now,
            //    read_count = 100
            //} );
            //testData.Add( new {
            //    title = "title",
            //    content = "content",
            //    add_time = DateTime.Now,
            //    read_count = 100
            //} );


            dynamic obj = new System.Dynamic.ExpandoObject();
            obj.title = "title";
            obj.content = "content";
            obj.add_time = DateTime.Now;
            obj.read_count = 100;

            testData.Add( obj );

           // var dataList =  Common.DbHelperMySqL.Query( "select * from article limit 10" );

            listView.ItemsSource = testData;
            Content.Children.Add( listView );
        }
    }
}
