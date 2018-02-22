using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebServiceCaller.Logic;

namespace WebServiceCaller {
    /// <summary>
    /// UIItemDetail.xaml 的交互逻辑
    /// </summary>
    public partial class UIItemDetail : Window {
        public UIItemDetail() {
            InitializeComponent();
        }

        public UIItemDetail( List<WindowItem> items, IDictionary<string, object> data ) {
            InitializeComponent();
            var objList = Dic2List( items, data );
            ContentList.ItemsSource = objList;
        }

        private List<Object> Dic2List( List<WindowItem> items, IDictionary<string, object> data ) {
            Func<String,String> getTitle = delegate (string key) {
                var found = items.Find( windowItem => key == windowItem.Name );
                if( found != null ) {
                    return found.Title;
                } else {
                    return key;
                }
            };
            return data.Select( item => new {
                Name = getTitle(item.Key),
                Value = item.Value
            } ).ToList<Object>();
        }
    }
}
