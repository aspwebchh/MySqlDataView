using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MySqlDataView.Logic {
    public class TabWithListView {
        public TabItem TabItem {
            get; set;
        }

        public Action ResizeListView {
            get; set;
        }
    }
}
