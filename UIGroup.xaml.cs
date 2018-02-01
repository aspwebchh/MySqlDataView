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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebServiceCaller.Logic;

namespace WebServiceCaller {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UIGroup : Window {
        public UIGroup() {
            InitializeComponent();

            var config = XmlConfigParser.Parse( @"C:\dev\web_service_caller\XmlConfig\Demo.xml" );

            foreach( var product in config.Products ) {
                var button = new Button();
                button.Content = product.Name;
                button.Margin = new Thickness( 0, 0, 10, 0 );
                button.Click += delegate ( object sender, RoutedEventArgs e ) {
                    var content = new UIContent(product,config.WindowsGroup);
                    content.Owner = this;
                    content.ShowDialog();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
                };
                Content.Children.Add( button );
            }

            /*

            var group = config.WindowsGroup;
            var groupBox = new GroupBox();
            groupBox.Header = group.Title;

            var container = new WrapPanel();
            foreach( var window in group.Items ) {
                var control = new Button();
                control.Content = window.Title;
                control.Click += delegate ( object sender, RoutedEventArgs e ) {
                    var uiWindow = new UIWindow( window );
                    uiWindow.Owner = this;
                    uiWindow.ShowDialog();
                };
                container.Children.Add( control );
            }

            groupBox.Content = container;
            Content.Children.Add( groupBox );*/
        }
    }
}
