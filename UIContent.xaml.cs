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
using System.Threading;
using WebServiceCaller.Logic;
using WebServiceCaller.Common;

namespace WebServiceCaller {
    /// <summary>
    /// UIContent.xaml 的交互逻辑
    /// </summary>
    public partial class UIContent : Window {
        private Product product;
        private WindowGroup windowGroup;
        private Pager pager;

        public UIContent( Product product, WindowGroup group ) {
            this.product = product;
            this.windowGroup = group;
            DbHelperMySqL.ConnectionString = product.ConnectionString;

            InitializeComponent();

            this.InitNavMenu();
            this.pager = new Pager( this );
        }

        private void InitNavMenu() {
            foreach( var item in windowGroup.Items ) {
                var tabItems = new Dictionary<WindowObject, TabItem>();
                var treeViewItem = new TreeViewItem();
                treeViewItem.Header = item.Title;
                treeViewItem.Style = Resources[ "TreeViewItem" ] as Style;
                treeViewItem.Selected += delegate ( object sender, RoutedEventArgs e ) {
                    TabItem tabItem;
                    if( tabItems.ContainsKey( item ) ) {
                        tabItem = tabItems[ item ];
                    } else {
                        tabItem = NewTabItem( item );
                        tabItems.Add( item, tabItem );
                        Contents.Items.Add( tabItem );
                    }
                    Contents.SelectedIndex = Contents.Items.IndexOf( tabItem );
                };
                NavMenu.Items.Add( treeViewItem );
            }
        }

        private TabItem NewTabItem( WindowObject window ) {
            var tabItem = new TabItem();
            tabItem.Header = window.Title;
            tabItem.Content = NewTabItemList( window );
            return tabItem;
        }


        private ListView NewTabItemList( WindowObject window ) {
            var listView = new System.Windows.Controls.ListView();
            var gridView = new GridView();
            foreach( var windowItem in window.Items ) {
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


            ThreadPool.QueueUserWorkItem( delegate( object state ) {
                var dataList = DbHelperMySqL.Query( "select * from "+ window.TableName +" limit 20" );
                var objectList = Data2Object.Convert( dataList.Tables[ 0 ] );
                Dispatcher.Invoke( delegate () {
                    listView.ItemsSource = objectList;
                } );
            } );
            return listView;
        }


        private void FirstPageButton_Click( object sender, RoutedEventArgs e ) {
           
        }

        private void PreviousPageButton_Click( object sender, RoutedEventArgs e ) {
            
        }

        private void NextPageButton_Click( object sender, RoutedEventArgs e ) {
           
        }

        private void LastPageButton_Click( object sender, RoutedEventArgs e ) {
            
        }
    }
}
