using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceCaller.Logic {
    #region
    public enum WindowType {
        GetList, SubmitData
    }
    #endregion

    #region
    public enum WindowItemType {
        FilterItem, ListItem
    }

    public enum WindowItemDataType {
        Integer, String, DateTime, Map, List
    }

    public enum WindowItemMatchType {
        Equals, Like, Null
    }
    #endregion
}
