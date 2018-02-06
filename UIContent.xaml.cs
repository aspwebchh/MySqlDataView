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
using System.Data;

namespace WebServiceCaller {
    /// <summary>
    /// UIContent.xaml 的交互逻辑
    /// </summary>
    public partial class UIContent : Window {
        private Product product;
        private WindowGroup windowGroup;
        private WindowObject currWindow;

        public UIContent( Product product, WindowGroup group ) {
            this.product = product;
            this.windowGroup = group;
            DbHelperMySqL.ConnectionString = product.ConnectionString;

            InitializeComponent();

            this.InitNavMenu();
        }

        private void SetCurrTabState(WindowObject window) {
            currWindow = window;
            Pager.Get(product, window, this).Render();
        }

        private void InitNavMenu() {
            foreach( var item in windowGroup.Items ) {
                var tabItems = new Dictionary<WindowObject, TabItem>();
                var treeViewItem = new TreeViewItem();
                treeViewItem.Header = item.Title;
                treeViewItem.Style = Resources[ "TreeViewItem" ] as Style;
                treeViewItem.Selected += delegate ( object sender, RoutedEventArgs e ) {
                    SetCurrTabState(item);

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
            tabItem.MouseLeftButtonUp += delegate ( object sender, MouseButtonEventArgs e ) {
                SetCurrTabState( window );
            };
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

            Pager pager = Pager.NewOrGet( product, currWindow, this );

            var fields = string.Join(",", window.Items.Select( item => item.Name ).ToArray());
            pager.OnPageChanged += delegate () {
                ThreadPool.QueueUserWorkItem( delegate ( object state ) {
                    var sortString = window.SortField + " " + window.SortMode;
                    var dataListTask = DbHelperMySqL.QueryAsync( "select "+ fields + " from " + window.TableName + " order by " + sortString + " limit " + pager.GetLimit() );
                    var dataCountTask = DbHelperMySqL.GetSingleAsync( "select count(*) from " + window.TableName );
                    var dataList = dataListTask.Result;
                    var dataCount = dataCountTask.Result;
                    var objectList = Data2Object.Convert( dataList.Tables[ 0 ] );
                    Dispatcher.Invoke( (Action)delegate () {
                        pager.SetDataCount( int.Parse( dataCount.ToString() ) );
                        pager.Render();
                        listView.ItemsSource = objectList;
                    } );
                } );
            };

            pager.PageChange();

            return listView;
        }


        private void FirstPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow , this).First();
        }

        private void PreviousPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow , this).Prev();
        }

        private void NextPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow , this).Next();
        }

        private void LastPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow , this).Last();
        }
    }
}
