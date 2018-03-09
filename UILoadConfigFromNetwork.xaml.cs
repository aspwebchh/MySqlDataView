using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using MySqlDataView.Logic;

namespace MySqlDataView {
    /// <summary>
    /// UILoadConfigFromNetwork.xaml 的交互逻辑
    /// </summary>
    public partial class UILoadConfigFromNetwork : Window {
        public UILoadConfigFromNetwork() {
            InitializeComponent();
        }

        public string ExtractUrlPath( string url ) {
            var pathAndFile = Common.Common.PathAndFile( url );
            return pathAndFile.Item1;
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            var url = URL.Text.Trim();
            if( !Common.Common.IsUrl( url ) ) {
                MessageBox.Show( "url格式不正确" );
                return;
            }
            var urlList = new List<string>();
 
            XmlConfigParser.LoadExtConfigFn = delegate ( string extFile ) {
                var lxcr = new LoadExtConfigResult();
                var extUrl = ExtractUrlPath( url ) + extFile;
                try {
                    var data = XmlExtConfigParser.ParseUrl( extUrl );
                    lxcr.Success = true;
                    lxcr.Data = data;
                    urlList.Add( extUrl );
                } catch(Exception ex ) {
                    lxcr.Success = false;
                    lxcr.Message = ex.Message;
                }
                return lxcr;
            };
            try {
                var config = XmlConfigParser.ParseUrl( url );
                urlList = urlList.Distinct().ToList();
                var task = SaveMainConfigFile( url );
                var taskList = SaveExtConfigFile( urlList );
                taskList.Add( task );
                taskList.ForEach( item => item.Wait() );
                var mainWindow = this.Owner as UIMain;
                mainWindow.LoadConfig();
                MessageBox.Show( "加载成功" );
                this.Close();
            } catch(Exception ex ) {
                MessageBox.Show( "配置文件解析异常：" + ex.Message );
            }
        }

        private Task SaveMainConfigFile( string mainConfigUrl ) {
            var path = "./config/config.xml";
            return SaveConfigFile( path, mainConfigUrl );
        }

        private List<Task> SaveExtConfigFile( List<string> extConfigUrls ) {
            var tasklist = new List<Task>();
            foreach( var url in extConfigUrls ) {
                var file = Common.Common.PathAndFile( url ).Item2;
                var savePath = "./config/" + file;
                var task = SaveConfigFile( savePath, url );
                tasklist.Add( task );
            }
            return tasklist;
        }

        private Task SaveConfigFile( string savePath, string url ) {
            return Task.Factory.StartNew( delegate {
                var content = Common.Common.ReadUrlContent( url );
                if( !Directory.Exists( "./config" ) ) {
                    Directory.CreateDirectory( "./config" );
                }
                var mode = File.Exists( savePath ) ? FileMode.Truncate : FileMode.CreateNew;
                using( var fs = new FileStream( savePath, mode, FileAccess.ReadWrite ) ) {
                    var sw = new StreamWriter( fs );
                    sw.Write( content );
                    sw.Flush();
                }
            } );
        }
    }
}
