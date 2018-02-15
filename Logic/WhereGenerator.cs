using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WebServiceCaller.Logic {
    class WhereGenerator {
        public static void ClearFilterFormField( UIElement element) {
            if( element is TextBox ) {
                var txtbox = element as TextBox;
                txtbox.Text = "";
            } else if( element is WrapPanel ) {
                var childs = ( element as WrapPanel ).Children;
                foreach( UIElement item in childs ) {
                    ClearFilterFormField(item);
                }
            }
        }

        private static Dictionary<string, string> GetFilterFormFieldResult( UIElement element ) {
            var result = new Dictionary<string, string>();
            if( element is TextBox ) {
                var txtbox = element as TextBox;
                if( !string.IsNullOrEmpty(txtbox.Text.Trim()) ) {
                    result [txtbox.Name] = txtbox.Text.Trim();
                }
            } else if( element is WrapPanel ) {
                var childs = ( element as WrapPanel ).Children;
                foreach( UIElement item in childs ) {
                    var itemResult = GetFilterFormFieldResult(item);
                    foreach( var key in itemResult.Keys ) {
                        result [key] = itemResult [key];
                    }
                }
            }
            return result;
        }

        private static WindowItemMatchType FindMatchType(string key, List<WindowItem> formFields ) {
            return formFields.Find(item => item.Name == key).MatchType;
        }

        public static string GetWhere( UIElement element, List<WindowItem> formFields ) {
            Dictionary<string, string> formFieldsData = GetFilterFormFieldResult(element);
            if(formFields.Count == 0) {
                return "";
            }
            var where = " 1=1 ";
            foreach( var key in formFieldsData.Keys ) {
                var val = formFieldsData [key];
                var matchType = FindMatchType(key, formFields);
                var op = matchType == WindowItemMatchType.Like ? " like " : " = ";
                var right = matchType == WindowItemMatchType.Like ? "'%" + val + "%'" : "'" + val + "'";
                where += " and " + key  + op + right;
            }
            return where;
        }
    }
}
