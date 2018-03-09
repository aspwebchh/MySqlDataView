using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MySqlDataView.Logic {
    public delegate void PageChanged();

    public class Pager {
        private static Dictionary<string, Pager> pagerList = new Dictionary<string, Pager>();

        private static string getPagerKey( Product product, WindowObject window ) {
            var key = product.ID + "-" + window.Title;
            return key;
        }

        public static Pager NewOrGet( Product product, WindowObject window, UIContent uiContent ) {
            var key = getPagerKey( product, window );
            if( pagerList.ContainsKey( key ) ) {
                pagerList[ key].SetUIContent( uiContent );
                return pagerList[ key ];
            }
            var pager = new Pager( uiContent );
            pagerList.Add( key, pager );
            return pager;
        }

        public static Pager Get( Product product, WindowObject window, UIContent uiContent ) {
            var key = getPagerKey( product, window );
            if( window == null ) {
                return new NullPager();
            }
            if( pagerList.ContainsKey( key ) ) {
                pagerList[ key ].SetUIContent( uiContent );
                return pagerList[ key ];
            } 
            return new NullPager();
        }


        public event PageChanged OnPageChanged;

        const int PAGE_SIZE = 40;
        UIContent uiContent;

        int currPageIndex = 1;
        int dataCount = 0;

        public Pager( UIContent uiContent ) {
            this.uiContent = uiContent;
            this.Reset();
            this.PageChange();
        }

        public void SetUIContent( UIContent uiContent ) {
            this.uiContent = uiContent;
        }

        public Pager() {

        }   

        public int PageCount {
            get {
                return this.dataCount / PAGE_SIZE + 1;
            }
        }

        public void PageChange() {
            if( this.OnPageChanged != null ) {
                this.OnPageChanged();
            }
        }

        public void  Reset() {
            uiContent.rCurrent.Text = "0";
            uiContent.rTotal.Text = "0";
            uiContent.rDataCount.Text = "0";
        }

        public virtual void Render() {
            Reset();
            uiContent.rCurrent.Text = this.currPageIndex.ToString();
            uiContent.rTotal.Text = PageCount.ToString();
            uiContent.rDataCount.Text = this.dataCount.ToString();
        }

        public void SetCurrPageIndex( int pageIndex ) {
            currPageIndex = pageIndex;
        }

        public void SetDataCount( int dataCount ) {
            this.dataCount = dataCount;
        }

        public string GetLimit() {
            return ( this.currPageIndex - 1 ) * PAGE_SIZE + "," + PAGE_SIZE;
        }

        public virtual void Next() {
            if( this.currPageIndex >= PageCount ) {
                return;
            }
            this.currPageIndex++;
            this.PageChange();
        }

        public virtual void Prev() {
            if( this.currPageIndex <= 1 ) {
                return;
            }
            this.currPageIndex--;
            this.PageChange();
        }

        public virtual void First() {
            this.currPageIndex = 1;
            this.PageChange();
        }

        public virtual void Last() {
            this.currPageIndex = PageCount;
            this.PageChange();
        }
    }

    public class NullPager : Pager {
        public override void First() {
            MessageBox.Show( "空" );
        }

        public override void Last() {
            MessageBox.Show( "空" );
        }

        public override void Next() {
            MessageBox.Show( "空" );
        }

        public override void Prev() {
            MessageBox.Show( "空" );
        }

        public override void Render() {
            
        }
    }
}
