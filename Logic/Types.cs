using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlDataView.Logic {
    #region
    public enum WindowType {
        GetList, SubmitData
    }
    #endregion

    #region
    public enum WindowItemType {
        FilterItem, ListItem, DetailItem
    }

    public enum WindowItemDataType {
        Integer, String, DateTime, Html
    }

    public enum WindowItemMatchType {
        Equals, Like, Null
    }
    #endregion
}
