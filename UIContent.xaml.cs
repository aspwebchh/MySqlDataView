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
using System.Threading.Tasks;
using MySqlDataView.Logic;
using MySqlDataView.Common;
using System.Data;
using System.IO;
using WinForm = System.Windows.Forms;

namespace MySqlDataView {
    /// <summary>
    /// UIContent.xaml 的交互逻辑
    /// </summary>
    public partial class UIContent : Window {
        private Product product;
        private WindowGroup windowGroup;
        private WindowObject currWindow;
        private string where = "";
        private Dictionary<WindowObject, TabWithListView> tabList = new Dictionary<WindowObject, TabWithListView>();
        private DatabaseType databaseType;

        public UIContent( Product product, WindowGroup group, DatabaseType databaseType ) {
            this.databaseType = databaseType;
            this.Title = product.Name;
            this.product = product;
            this.windowGroup = group;
            InitializeComponent();
            this.InitNavMenu();
            this.InitListViewSize();

            Closed += delegate ( object sender, EventArgs e ) {
                UIMain.ShowWindow();
                Pager.ClearPageList();
                windowGroup.Items.ForEach( item => {
                    item.InitialLoading = false;
                } );
            };
        }

        private void InitListViewSize() {
            this.SizeChanged += delegate ( object sender, SizeChangedEventArgs e ) {
                tabList.Values.ToList().ForEach( item => item.ResizeListView() );
            };
        }

        private void SetCurrTabState( WindowObject window ) {
            currWindow = window;
            Pager.Get( product, window, this ).Render();
        }

        private void InitNavMenu() {
            foreach( var item in windowGroup.Items ) {
                var tabItems = new Dictionary<WindowObject, TabItem>();
                var treeViewItem = new TreeViewItem();
                treeViewItem.Header = item.Title;
                treeViewItem.Style = Resources[ "TreeViewItem" ] as Style;
                treeViewItem.Selected += delegate ( object sender, RoutedEventArgs e ) {
                    SetCurrTabState( item );

                    TabItem tabItem;
                    if( tabItems.ContainsKey( item ) ) {
                        tabItem = tabItems[ item ];
                    } else {
                        var tabWithListView = NewTabItem( item, delegate ( TabItem removeTarget ) {
                            Contents.Items.Remove( removeTarget );
                            tabItems.Remove( item );
                            tabList.Remove( item );
                            treeViewItem.IsSelected = false;
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
            var form = NewFilterForm( window );
            var list = window.ListView = NewTabItemList( window );
            stackPannel.Children.Add( form );
            stackPannel.Children.Add( list );

            var tabItem = new TabItem();
            var contextMenu = new ContextMenu();

            #region
            //关闭
            var closeItem = new MenuItem();
            closeItem.Header = "关闭";
            closeItem.Click += delegate {
                onRemove( tabItem );
            };
            contextMenu.Items.Add( closeItem );

            //导出
            Action<string> exportHandle = path => {
                var cols = string.Join( ",", window.ExportItems.Select( item => item.Name ).ToArray() );
                var sql = "select " + cols + " from " + window.TableName;
                var countSql = "select count(*) from " + window.TableName;
                if( !string.IsNullOrEmpty( where ) ) {
                    sql += " where " + where;
                    countSql += " where " + where;
                }
                try {
                    if( string.IsNullOrWhiteSpace( DbHelperMySqL.ConnectionString ) ) {
                        MessageBox.Show( "未选择数据库" );
                        return;
                    }
                    var countTask = DbHelperMySqL.GetSingleAsync( countSql );
                    var dataTask = DbHelperMySqL.QueryAsync( sql );
                    var count = Convert.ToInt32( countTask.Result.ToString() );
                    if( count > 10000 ) {
                        MessageBox.Show( "最多只能导出10000条" );
                        return;
                    }
                    var ds = dataTask.Result;
                    var dt = Data2Object.ToListDataTable( ds.Tables[ 0 ], window );
                    var objList = Data2Object.Convert( dt, window );
                    ExportExcel.Export( objList, window.Title, window.ExportItems, path );
                    MessageBox.Show( "导出完成" );
                } catch( Exception e ) {
                    MessageBox.Show( e.Message );
                }

            };

            var exportItem = new MenuItem();
            exportItem.Header = "导出";
            exportItem.Click += delegate {
                var exportItems = window.ExportItems;
                if( exportItems.Count == 0 ) {
                    MessageBox.Show( "未配置导出项" );
                    return;
                }

                var sfd = new WinForm.SaveFileDialog();
                sfd.Filter = "Excel2007文档|*.xlsx";
                sfd.FileName = window.Title;
                if( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                    var filePath = string.Empty;
                    using( var fs = (FileStream)sfd.OpenFile() ) {
                        filePath = fs.Name;
                    }
                    Task.Factory.StartNew( () => {
                        exportHandle( filePath );
                    } );

                }
            };
            contextMenu.Items.Add( exportItem );

            #endregion

            TextBlock tabTitle = new TextBlock();
            tabTitle.Text = window.Title;
            tabTitle.ContextMenu = contextMenu;

            tabItem.Header = tabTitle;

            tabItem.Content = stackPannel;
            tabItem.MouseLeftButtonUp += delegate {
                SetCurrTabState( window );
            };

            Action resizeListView = delegate () {
                var height = Contents.ActualHeight - form.ActualHeight - tabItem.ActualHeight;
                //不知何故，窗口全屏状态下高度值计算出错
                if( this.WindowState == WindowState.Maximized ) {
                    height -= 10;
                }
                list.Height = height - 25;
            };

            tabItem.Loaded += delegate {
                resizeListView();
            };

            var tabWithListView = new TabWithListView();
            tabWithListView.TabItem = tabItem;
            tabWithListView.ResizeListView = delegate {
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
            databaseType.AppendFilterFormFieldTo( wrapPannel, this );
            foreach( var item in filterItems ) {
                wrapPannel.Children.Add( NewFilterFormField( item ) );
            }
            var filterBtn = new Button();
            filterBtn.Style = Resources[ "FormFieldButton" ] as Style;
            filterBtn.Content = "筛选";
            filterBtn.Width = 50;
            filterBtn.Click += delegate ( object sender, RoutedEventArgs e ) {
                var setConnStrResult = databaseType.SetConnectionString( wrapPannel );
                if( !setConnStrResult.Item1 ) {
                    MessageBox.Show( setConnStrResult.Item2 );
                    return;
                }
                where = WhereGenerator.GetWhere( wrapPannel );
                var pager = Pager.Get( product, window, this );
                pager.SetCurrPageIndex( 1 );
                pager.PageChange();
            };
            wrapPannel.Children.Add( filterBtn );


            var resetBtn = new Button();
            resetBtn.Style = Resources[ "FormFieldButton" ] as Style;
            resetBtn.Content = "重置";
            resetBtn.Width = 50;
            resetBtn.Click += delegate ( object sender, RoutedEventArgs e ) {
                databaseType.EmptyListView( currWindow.ListView );
                databaseType.ResetConnectionString();
                WhereGenerator.ClearFilterFormField( wrapPannel );
                where = "";
                var pager = Pager.Get( product, window, this );
                pager.SetCurrPageIndex( 1 );
                databaseType.PageChange( pager );
            };
            wrapPannel.Children.Add( resetBtn );
            return wrapPannel;
        }

        public WrapPanel NewFilterFormField( WindowItem windowItem ) {
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
                fef.SetValue( TextBlock.PaddingProperty, listViewItemPadding );
                fef.SetValue( TextBlock.TextAlignmentProperty, TextAlignment.Center );
                dtpl.VisualTree = fef;
                column.CellTemplate = dtpl;
                gridView.Columns.Add( column );
            }
            listView.View = gridView;

            Pager pager = Pager.NewOrGet( product, currWindow, this );

            var fields = string.Join( ",", window.ListItems.Select( item => item.Name ).ToArray() );
            fields += "," + window.UniqueID;

            pager.OnPageChanged += delegate () {
                if( databaseType is DatabaseMultiple && !currWindow.InitialLoading ) {
                    return;
                }
                ThreadPool.QueueUserWorkItem( delegate ( object state ) {
                    var sortString = window.SortField + " " + window.SortMode;
                    var whereString = "";
                    if( string.IsNullOrEmpty( where ) ) {
                        whereString = " 1 = 1 ";
                    } else {
                        whereString = where;
                    }
                    try {
                        if( string.IsNullOrWhiteSpace( DbHelperMySqL.ConnectionString ) ) {
                            MessageBox.Show( "未选择数据库" );
                            return;
                        }
                        var dataListTask = DbHelperMySqL.QueryAsync( "select " + fields + " from " + window.TableName + " where " + whereString + " order by " + sortString + " limit " + pager.GetLimit() );
                        var dataCountTask = DbHelperMySqL.GetSingleAsync( "select count(*) from " + window.TableName + " where " + whereString );
                        var dataList = dataListTask.Result;
                        var dataCount = dataCountTask.Result;
                        var dataTable = Data2Object.ToListDataTable( dataList.Tables[ 0 ], window );
                        var objectList = Data2Object.Convert( dataTable, window );
                        Dispatcher.Invoke( (Action)delegate () {
                            pager.SetDataCount( int.Parse( dataCount.ToString() ) );
                            pager.Render();
                            listView.ItemsSource = objectList;
                        } );
                    } catch( Exception ex ) {
                        MessageBox.Show( ex.Message );
                    }
                } );
            };

            pager.PageChange();
            currWindow.InitialLoading = true;

            listView.MouseDoubleClick += delegate ( object sender, MouseButtonEventArgs e ) {
                var selectedItem = listView.SelectedItem as IDictionary<string, object>;
                if( selectedItem == null ) {
                    return;
                }
                var uiItemDetail = new UIItemDetail( window, selectedItem[ window.UniqueID ].ToString() );
                uiItemDetail.Owner = this;
                uiItemDetail.ShowDialog();
            };

            listView.ItemContainerStyle = Resources[ "ListViewItem" ] as Style;
            listView.Style = Resources[ "ListView" ] as Style;

            return listView;
        }

        private void ToTopOnListView() {
            currWindow.ListView.ScrollIntoView( currWindow.ListView.Items[ 0 ] );
        }

        private void FirstPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow, this ).First();
            ToTopOnListView();
        }

        private void PreviousPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow, this ).Prev();
            ToTopOnListView();
        }

        private void NextPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow, this ).Next();
            ToTopOnListView();
        }

        private void LastPageButton_Click( object sender, RoutedEventArgs e ) {
            Pager.Get( product, currWindow, this ).Last();
            ToTopOnListView();
        }
    }
}
