using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceCaller.Logic {
    public class Pager {
        UIContent uiContent;
        public Pager(UIContent uiContent) {
            this.uiContent = uiContent;
            this.Reset();
        }

        public void Reset() {
            uiContent.rCurrent.Text = "0";
            uiContent.rTotal.Text = "0";
        }
    }
}
