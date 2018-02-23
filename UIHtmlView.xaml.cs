using System;
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

namespace MySqlDataView {
    /// <summary>
    /// UIHtmlView.xaml 的交互逻辑
    /// </summary>
    public partial class UIHtmlView : Window {
        public UIHtmlView() {
            InitializeComponent();
        }

        public UIHtmlView( string htmlText ) {
            InitializeComponent();
            htmlText = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body>" + htmlText + "</body></html>";
            WebView.NavigateToString( htmlText );
        }
    }
}
