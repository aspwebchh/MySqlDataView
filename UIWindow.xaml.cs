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


namespace WebServiceCaller {
    class TestData {
        public string Title {
            get; set;
        }
        public string Content {
            get;set;
        }

        public string Count {
            get;set;
        }
    }
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
            testData.Add( new TestData {
                Title = "title",
                Content = "Content",
                Count = "1"
            } );
            testData.Add( new TestData {
                Title = "title",
                Content = "Content",
                Count = "1"
            } );
            listView.ItemsSource = testData;

            return listView;
        }

        private void InitListPage() {

        }
    }
}
