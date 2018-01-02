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
using System.Windows.Forms;
using System.Windows.Forms.Integration;


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
            } else {
                var textBox = new System.Windows.Controls.TextBox();
                textBox.Width = 200;
                control = textBox;
            }
            return control;
        }

        private void InitListPage() {

        }
    }
}
