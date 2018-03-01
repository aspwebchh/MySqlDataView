namespace MySqlDataView.Logic {
    public class KeyVal<Key, Val> {

        private Key key;
        private Val val;

        public KeyVal( Key key, Val val ) {
            this.key = key;
            this.val = val;
        }

        public Key GetKey() {
            return key;
        }

        public Val GetVal() {
            return val;
        }
    }
}