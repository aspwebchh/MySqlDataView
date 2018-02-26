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
using MySqlDataView.Logic;

namespace MySqlDataView {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UIMain : Window {

        private static UIMain instance;

        public static void HideWindow() {
            instance.Hide();
        }


        public static void ShowWindow() {
            instance.Show();
        }

        public UIMain() {
            InitializeComponent();
            LoadConfig();
            instance = this;
        }

        private void LoadConfig() {
            if( !File.Exists( "./config/config.xml" ) ) {
                return;
            }
            try {
                var config = XmlConfigParser.Parse( "./config/config.xml" );
                var dataSource = from product in config.Products
                                 select new {
                                     Name = product.Name,
                                     Value = product
                                 };
                Content.ItemsSource = dataSource;
                Content.DisplayMemberPath = "Name";
                Content.SelectedValuePath = "Value";
                Content.MouseDoubleClick += delegate ( object sender, MouseButtonEventArgs e ) {
                    var selectItem = Content.SelectedValue as Product;
                    var content = new UIContent( selectItem, selectItem.WindowGroup );
                    content.Owner = this;
                    content.Show();
                    UIMain.HideWindow();
                };
            } catch(XmlConfigParseError e) {
                MessageBox.Show( "配置文件解析异常：" +  e.Message );
            }
        }

        private void MenuItem_Click_LoadConfigFile( object sender, RoutedEventArgs e ) {
            var dialog = new WinForm.OpenFileDialog();
            dialog.Filter = "配置文件|*.xml";
            dialog.Multiselect = true;
            dialog.SupportMultiDottedExtensions = true;
            if( dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                var files = dialog.FileNames;
                files.ToList().ForEach( Handle );
            }
        }

        private void Handle( string xmlConfigFilePath ) {
            try {
                XmlConfigParser.Parse( xmlConfigFilePath );
                if( !Directory.Exists( "./config" ) ) {
                    Directory.CreateDirectory( "./config" );
                }
                File.Copy( xmlConfigFilePath, "./config/config.xml", true );
                LoadConfig();
                MessageBox.Show( "加载成功" );
            } catch( XmlConfigParseError err ) {
                MessageBox.Show( "配置文件格式不正确：" + err.Message );
            }
        }
    }
}
