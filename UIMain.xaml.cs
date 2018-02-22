using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using WinForm = System.Windows.Forms;
using WebServiceCaller.Logic;

namespace WebServiceCaller {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UIMain : Window {

        public UIMain() {
            InitializeComponent();
            LoadConfig();
        }

        private void LoadConfig() {
            if( !File.Exists( "./config.xml" ) ) {
                return;
            }
            var config = XmlConfigParser.Parse( "./config.xml" );
            foreach( var product in config.Products ) {
                var button = new Button();
                button.Content = product.Name;
                button.Margin = new Thickness( 0, 0, 10, 0 );
                button.Click += delegate ( object sender, RoutedEventArgs e ) {
                    var content = new UIContent( product, config.WindowsGroup );
                    content.Owner = this;
                    content.ShowDialog();
                };
                Content.Children.Add( button );
            }
        }

        private void MenuItem_Click_LoadConfigFile( object sender, RoutedEventArgs e ) {
            var dialog = new WinForm.OpenFileDialog();
            dialog.Filter = "配置文件|*.xml";
            dialog.Multiselect = false;
            dialog.SupportMultiDottedExtensions = true;
            if( dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                var files = dialog.FileNames;
                Handle( files[ 0 ] );
            }
        }

        private void Handle( string xmlConfigFilePath ) {
            try {
                XmlConfigParser.Parse( xmlConfigFilePath );
                File.Copy( xmlConfigFilePath, "./config.xml", true );
                LoadConfig();
                MessageBox.Show( "加载成功" );
            } catch( XmlConfigParseError err ) {
                MessageBox.Show( "配置文件格式不正确：" + err.Message );
            }
        }
    }
}
