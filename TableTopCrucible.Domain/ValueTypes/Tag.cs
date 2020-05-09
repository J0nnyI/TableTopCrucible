using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.ValueTypes
{
    public struct Tag
    {
        private readonly string _tag;
        public Tag(string tag)
        {
            this._tag = tag;
        }
        public override string ToString() => this._tag;
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Tag tag:
                    return this._tag == tag._tag;
                case string tag:
                    return this._tag == tag;
                default:
                    return false;
            }
        }
        public override int GetHashCode() => this._tag.GetHashCode();
        public static explicit operator Tag(string tag) => new Tag(tag);
        public static explicit operator string(Tag tag) => tag._tag;
        public static bool operator ==(Tag tag1, Tag tag2)
        {
            return tag1._tag == tag2._tag;
        }
        public static bool operator !=(Tag tag1, Tag tag2)
        {
            return tag1._tag != tag2._tag;
        }

    }
}
