namespace TableTopCrucible.Domain.ValueTypes
{
    public struct ItemName
    {
        private string _name { get; }
        public ItemName(string name)
        {
            this._name = name;
        }
        public override string ToString()
            => this._name;
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case string str:
                    return this._name == str;
                case ItemName name:
                    return this._name == name._name;
                default:
                    return false;
            }
        }
        public override int GetHashCode()
            => _name.GetHashCode();
        public static explicit operator ItemName(string text)
            => new ItemName(text);
        public static explicit operator string(ItemName name)
            => name._name;

    }
}
