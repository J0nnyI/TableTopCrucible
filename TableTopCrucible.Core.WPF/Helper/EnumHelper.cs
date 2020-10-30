using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public static class EnumHelper
    {
        //https://stackoverflow.com/questions/27572843/is-it-possible-to-databind-to-a-enum-and-show-user-friendly-values/27573155#27573155
        public static List<KeyValuePair<T, string>> GetValuesForComboBox<T>(this T theEnum) where T:Enum
        {
            List<KeyValuePair<T, string>> _comboBoxItemSource = null;
            if (_comboBoxItemSource == null)
            {
                _comboBoxItemSource = new List<KeyValuePair<T, string>>();
                foreach (T level in Enum.GetValues(typeof(T)))
                {
                    string Description = string.Empty;
                    FieldInfo fieldInfo = level.GetType().GetField(level.ToString());
                    DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        Description = attributes.FirstOrDefault().Description;
                    }
                    KeyValuePair<T, string> TypeKeyValue = new KeyValuePair<T, string>(level, Description);
                    _comboBoxItemSource.Add(TypeKeyValue);
                }
            }
            return _comboBoxItemSource;
        }
    }
}
