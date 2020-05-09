using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.ValueTypes
{
    public struct Thumbnail
    {
        private readonly string _thumbnail;
        public Thumbnail(string thumbnail)
        {
            this._thumbnail = thumbnail;
        }
        public override string ToString() => this._thumbnail;
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Thumbnail thumbnail:
                    return this._thumbnail == thumbnail._thumbnail;
                case string thumbnail:
                    return this._thumbnail == thumbnail;
                default:
                    return false;
            }
        }
        public override int GetHashCode() => this._thumbnail.GetHashCode();
        public static explicit operator Thumbnail(string thumbnail) => new Thumbnail(thumbnail);
        public static explicit operator string(Thumbnail thumbnail) => thumbnail._thumbnail;
        public static bool operator ==(Thumbnail thumbnail1, Thumbnail thumbnail2)
        {
            return thumbnail1._thumbnail == thumbnail2._thumbnail;
        }
        public static bool operator !=(Thumbnail thumbnail1, Thumbnail thumbnail2)
        {
            return thumbnail1._thumbnail != thumbnail2._thumbnail;
        }

    }
}
