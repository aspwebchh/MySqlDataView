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
        private String where = "";

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
                        tabItem = NewTabItem( item,delegate(TabItem removeTarget) {
                            Contents.Items.Remove( removeTarget );
                            tabItems.Remove( item );
                        } );
                        tabItems.Add( item, tabItem );
                        Contents.Items.Add( tabItem );
                    }
                    Contents.SelectedIndex = Contents.Items.IndexOf( tabItem );
                };
                NavMenu.Items.Add( treeViewItem );
            }
        }

        private TabItem NewTabItem( WindowObject window, Action<TabItem> onRemove ) {
            var stackPannel = new StackPanel();
            var form = NewFilterForm(window);
            var list = NewTabItemList(window);
            stackPannel.Children.Add(form);
            stackPannel.Children.Add( list );
            var tabItem = new TabItem();
            var contextMenu = new ContextMenu();
            var closeItem = new MenuItem();
            closeItem.Header = "关闭";
            closeItem.Click += delegate ( object sender, RoutedEventArgs e ) {
                onRemove( tabItem );
            };
            contextMenu.Items.Add( closeItem );

            TextBlock tabTitle = new TextBlock();
            tabTitle.Text = window.Title;
            tabTitle.ContextMenu = contextMenu;

            tabItem.Header = tabTitle;
            tabItem.Content = stackPannel;
            tabItem.MouseLeftButtonUp += delegate ( object sender, MouseButtonEventArgs e ) {
                SetCurrTabState( window );
            };
            tabItem.Loaded += delegate ( object sender, RoutedEventArgs e ) {
                list.Height = Contents.ActualHeight - form.ActualHeight - tabItem.ActualHeight;
            };
            return tabItem;
        }


        private WrapPanel NewFilterForm( WindowObject window ) {
            var wrapPannel = new WrapPanel();
            var filterItems = window.FilterItems;
            if( filterItems.Count == 0 ) {
                return wrapPannel;
            }
            foreach( var item in filterItems ) {
                wrapPannel.Children.Add(NewFilterFormField(item));
            }
            var filterBtn = new Button();
            filterBtn.Content = "筛选";
            filterBtn.Width = 50;
            filterBtn.Click += delegate ( object sender, RoutedEventArgs e ) {
                where = WhereGenerator.GetWhere(wrapPannel, window.FilterItems);
                var pager = Pager.Get(product, window, this);
                pager.SetCurrPageIndex(1);
                pager.PageChange();
            };
            wrapPannel.Children.Add(filterBtn);


            var resetBtn = new Button();
            resetBtn.Content = "重置";
            resetBtn.Width = 50;
            resetBtn.Click += delegate ( object sender, RoutedEventArgs e ) {
                WhereGenerator.ClearFilterFormField(wrapPannel);
                where = "";
                var pager = Pager.Get(product, window, this);
                pager.SetCurrPageIndex(1);
                pager.PageChange();
            };
            wrapPannel.Children.Add(resetBtn);

            return wrapPannel;
        }

        private WrapPanel NewFilterFormField(WindowItem windowItem) {
            var wrapPannel = new WrapPanel();
            var title = FormFieldFactory.TextBlock( windowItem );
            wrapPannel.Children.Add( title );

            UIElement content;
            if( windowItem.Contents.Count <= 0 ) {
                content = FormFieldFactory.TextBox( windowItem );
            } else {
                content = FormFieldFactory.ComboBox( windowItem );
            }
            wrapPannel.Children.Add( content );
            return wrapPannel;
        }

        private ListView NewTabItemList( WindowObject window ) {
            var listView = new ListView();
            var gridView = new GridView();
            foreach( var windowItem in window.ListItems ) {
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

            var fields = string.Join(",", window.ListItems.Select( item => item.Name ).ToArray());
            pager.OnPageChanged += delegate () {
                ThreadPool.QueueUserWorkItem( delegate ( object state ) {
                    var sortString = window.SortField + " " + window.SortMode;
                    var whereString = "";
                    if( string.IsNullOrEmpty( where ) ) {
                        whereString = " 1 = 1 ";
                    } else {
                        whereString = where;
                    }
                    var dataListTask = DbHelperMySqL.QueryAsync( "select "+ fields + " from " + window.TableName + " where " + whereString + " order by " + sortString + " limit " + pager.GetLimit() );
                    var dataCountTask = DbHelperMySqL.GetSingleAsync( "select count(*) from " + window.TableName + " where " + whereString);
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

            listView.MouseLeftButtonUp += delegate ( object sender, MouseButtonEventArgs e ) {
                var selectedItem = listView.SelectedItem as IDictionary<string, object>;
                var uiItemDetail = new UIItemDetail(window.ListItems,  selectedItem );
                uiItemDetail.Owner = this;
                uiItemDetail.ShowDialog();
            };
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
