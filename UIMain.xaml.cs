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
            Content.MouseDoubleClick += delegate ( object sender, MouseButtonEventArgs e ) {
                var selectItem = Content.SelectedValue as Product;
                var content = new UIContent( selectItem, selectItem.WindowGroup );
                content.Owner = this;
                content.Show();
                UIMain.HideWindow();
            };
            instance = this;
        }

        public void LoadConfig() {
            if( !File.Exists( "./config/config.xml" ) ) {
                return;
            }
            try {
                XmlConfigParser.LoadExtConfigFn = delegate ( string extFile ) {
                    var lxcr = new LoadExtConfigResult();
                    var extPath = "./config/" + extFile;
                    try {
                        var data = XmlExtConfigParser.Parse( extPath );
                        lxcr.Success = true;
                        lxcr.Data = data;
                    } catch( Exception e ) {
                        lxcr.Success = false;
                        lxcr.Message = e.Message;
                    }
                    return lxcr;
                };
                var config = XmlConfigParser.Parse( "./config/config.xml" );
                var dataSource = from product in config.Products
                                 select new {
                                     Name = product.Name,
                                     Value = product
                                 };
                Content.ItemsSource = dataSource;
                Content.DisplayMemberPath = "Name";
                Content.SelectedValuePath = "Value";
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
                Handle( files );
            }
        }

        private void Handle( string[] files ) {
            var resultPath = new List<dynamic>();
            XmlConfigParser.LoadExtConfigFn = delegate ( string extFile ) {
                var lxcr = new LoadExtConfigResult();
                var matches = files.Where( item => item.EndsWith( extFile ) );
                if( matches.Count() == 0 ) {
                    lxcr.Success = false;
                    lxcr.Message = extFile + "未指定";
                } else {
                    var extPath = matches.ElementAt( 0 );
                    try {
                        var data = XmlExtConfigParser.Parse( extPath );
                        lxcr.Success = true;
                        lxcr.Data = data;
                        resultPath.Add( new {
                            Path = extPath,
                            Type = ConfigFileType.Ext
                        } );
                    } catch( Exception e ) {
                        lxcr.Success = false;
                        lxcr.Message = e.Message;
                    }
                }
                return lxcr;
            };
            foreach( var path in files ) {
                var configFileType = XmlConfigParser.FileType( path );
                if( configFileType == ConfigFileType.Main ) {
                    try {
                        XmlConfigParser.Parse( path );
                        resultPath.Add( new {
                            Path = path,
                            Type = ConfigFileType.Main
                        } );
                    } catch( Exception e) {
                        MessageBox.Show( e.Message );
                        return;
                    }
                }
            }

            var mainConfigFileExist = resultPath.Where( item => item.Type == 1 ).Count() > 0;
            if( !mainConfigFileExist ) {
                MessageBox.Show( "加载失败" );
                return;
            }

            if( !Directory.Exists( "./config" ) ) {
                Directory.CreateDirectory( "./config" );
            }

            foreach( var item in resultPath ) {
                if( item.Type == ConfigFileType.Main ) {
                    File.Copy( item.Path, "./config/config.xml", true );
                } else if( item.Type == ConfigFileType.Ext ) {
                    File.Copy( item.Path, "./config/" + System.IO.Path.GetFileName( item.Path ), true );
                }
            }
            LoadConfig();
            MessageBox.Show( "加载成功" );
        }

        private void MenuItem_Click_LoadConfigFile_From_Network( object sender, RoutedEventArgs e ) {
            var window = new UILoadConfigFromNetwork();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
