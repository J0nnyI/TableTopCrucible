
using System;
using System.Windows;

using TableTopCrucible.Data.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public struct TagEx : IComparable
    {
        public TagEx(Tag tag, bool isPrimary)
        {
            Tag = tag;
            IsPrimary = isPrimary;
        }

        public Tag Tag { get; }
        public bool IsPrimary { get; }

        public int CompareTo(object obj)
        {
            try
            {
                if (obj is TagEx tag)
                {
                    if (this.IsPrimary && !tag.IsPrimary)
                        return -1;
                    if (!this.IsPrimary && tag.IsPrimary)
                        return 1;
                    return Tag.CompareTo(tag.Tag);
                }
                MessageBox.Show("wtf?");
                return 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return -1;
        }
        public override string ToString() => Tag.ToString();
    }
}
