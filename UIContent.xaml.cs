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
using MySqlDataView.Logic;
using MySqlDataView.Common;
using System.Data;

namespace MySqlDataView {
    /// <summary>
    /// UIContent.xaml 的交互逻辑
    /// </summary>
    public partial class UIContent : Window {
        private Product product;
        private WindowGroup windowGroup;
        private WindowObject currWindow;
        private String where = "";
        private Dictionary<WindowObject, TabWithListView> tabList = new Dictionary<WindowObject, TabWithListView>();

        public UIContent( Product product, WindowGroup group ) {
            this.Title = product.Name;
            this.product = product;
            this.windowGroup = group;
            DbHelperMySqL.ConnectionString = product.ConnectionString;
            InitializeComponent();
            this.InitNavMenu();
            this.InitListViewSize();
        }

        private void InitListViewSize() {
            this.SizeChanged += delegate ( object sender, SizeChangedEventArgs e ) {
                tabList.Values.ToList().ForEach( item => item.ResizeListView() );
            };
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
                        var tabWithListView = NewTabItem( item,delegate(TabItem removeTarget) {
                            Contents.Items.Remove( removeTarget );
                            tabItems.Remove( item );
                            tabList.Remove( item );
                        } );
                        tabItem = tabWithListView.TabItem;
                        tabItems.Add( item, tabItem );
                        tabList.Add( item, tabWithListView );
                        Contents.Items.Add( tabItem );
                    }
                    Contents.SelectedIndex = Contents.Items.IndexOf( tabItem );
                };
                NavMenu.Items.Add( treeViewItem );
            }
        }


        private TabWithListView NewTabItem( WindowObject window, Action<TabItem> onRemove ) {
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

            Action resizeListView = delegate () {
                var height = Contents.ActualHeight - form.ActualHeight - tabItem.ActualHeight;
                //不知何故，窗口全屏状态下高度值计算出错
                if( this.WindowState == WindowState.Maximized ) {
                    height -= 10;
                } 
                list.Height = height;
            };

            tabItem.Loaded += delegate ( object sender, RoutedEventArgs e ) {
                resizeListView();
            };

            var tabWithListView = new TabWithListView();
            tabWithListView.TabItem = tabItem;
            tabWithListView.ResizeListView = delegate () {
                resizeListView();
            };
            return tabWithListView;
        }

        private WrapPanel NewFilterForm( WindowObject window ) {
            var wrapPannel = new WrapPanel();
            wrapPannel.Style = Resources[ "Form" ] as Style;
            var filterItems = window.FilterItems;
            if( filterItems.Count == 0 ) {
                return wrapPannel;
            }
            foreach( var item in filterItems ) {
                wrapPannel.Children.Add(NewFilterFormField(item));
            }
            var filterBtn = new Button();
            filterBtn.Style = Resources[ "FormFieldButton" ] as Style;
            filterBtn.Content = "筛选";
            filterBtn.Width = 50;
            filterBtn.Click += delegate ( object sender, RoutedEventArgs e ) {
                where = WhereGenerator.GetWhere(wrapPannel);
                var pager = Pager.Get(product, window, this);
                pager.SetCurrPageIndex(1);
                pager.PageChange();
            };
            wrapPannel.Children.Add(filterBtn);


            var resetBtn = new Button();
            resetBtn.Style = Resources[ "FormFieldButton" ] as Style;
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
            title.Style = Resources[ "FormFieldTitle" ] as Style;
            wrapPannel.Children.Add( title );

            FrameworkElement content;
            if( windowItem.Contents.Count <= 0 ) {
                if( windowItem.DataType == WindowItemDataType.DateTime ) {
                    content = FormFieldFactory.DatePicker( windowItem );
                    content.Style = Resources[ "FormFieldDatePicker" ] as Style;
                } else {
                    content = FormFieldFactory.TextBox( windowItem );
                    content.Style = Resources[ "FormField" ] as Style;
                }
            } else {
                content = FormFieldFactory.ComboBox( windowItem );
                content.Style = Resources[ "FormFieldComboBox" ] as Style;
            }
            content.Tag = windowItem;
            wrapPannel.Children.Add( content );
            return wrapPannel;
        }

        private ListView NewTabItemList( WindowObject window ) {
            var listView = new ListView();
            var gridView = new GridView();
            var listViewItemPadding = new Thickness( 10, 0, 10, 0 );
            foreach( var windowItem in window.ListItems ) {
                var column = new GridViewColumn();
                var titleTemplate = new TextBlock();
                titleTemplate.Text = windowItem.Title;
                titleTemplate.TextAlignment = TextAlignment.Left;
                titleTemplate.Padding = listViewItemPadding;
                column.Header = titleTemplate;

                var binding = new Binding();
                binding.Path = new PropertyPath( windowItem.Name );
                var dtpl = new DataTemplate();
                var fef = new FrameworkElementFactory( typeof( TextBlock ) );
                fef.SetBinding( TextBlock.TextProperty, binding );
                fef.SetValue( TextBlock.PaddingProperty, listViewItemPadding);
                fef.SetValue( TextBlock.TextAlignmentProperty, TextAlignment.Center );
                dtpl.VisualTree = fef;
                column.CellTemplate = dtpl;
                gridView.Columns.Add( column );
            }
            listView.View = gridView;

            Pager pager = Pager.NewOrGet( product, currWindow, this );

            var fields = string.Join(",", window.ListItems.Select( item => item.Name ).ToArray());
            fields += "," + window.UniqueID;
            pager.OnPageChanged += delegate () {
                ThreadPool.QueueUserWorkItem( delegate ( object state ) {
                    var sortString = window.SortField + " " + window.SortMode;
                    var whereString = "";
                    if( string.IsNullOrEmpty( where ) ) {
                        whereString = " 1 = 1 ";
                    } else {
                        whereString = where;
                    }
                    Console.WriteLine(whereString);
                    var dataListTask = DbHelperMySqL.QueryAsync( "select "+ fields + " from " + window.TableName + " where " + whereString + " order by " + sortString + " limit " + pager.GetLimit() );
                    var dataCountTask = DbHelperMySqL.GetSingleAsync( "select count(*) from " + window.TableName + " where " + whereString);
                    var dataList = dataListTask.Result;
                    var dataCount = dataCountTask.Result;
                    var objectList = Data2Object.Convert( dataList.Tables[ 0 ],window );
                    Dispatcher.Invoke( (Action)delegate () {
                        pager.SetDataCount( int.Parse( dataCount.ToString() ) );
                        pager.Render();
                        listView.ItemsSource = objectList;
                    } );
                } );
            };

            pager.PageChange();

            listView.MouseDoubleClick += delegate ( object sender, MouseButtonEventArgs e ) {
                var selectedItem = listView.SelectedItem as IDictionary<string, object>;
                if( selectedItem == null ) {
                    return;
                }
                var uiItemDetail = new UIItemDetail( window, selectedItem[window.UniqueID].ToString() );
                uiItemDetail.Owner = this;
                uiItemDetail.ShowDialog();
            };

            listView.ItemContainerStyle = Resources[ "ListViewItem" ] as Style;

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
