using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TableTopCrucible.WPF.MultiConverters
{
    public class BooleanMultiValueConverter : IMultiValueConverter
    {
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string rawPar)
            {

                if (rawPar.StartsWith("eval"))
                {
                    // 1st level: get all ors
                    bool res = rawPar.Replace("eval","").Split("or").Select(
                    x =>
                    {
                        // then get the ands between the ors
                        return x.Split("and")
                        .Select(y =>
                           {
                                // than get the values between the ands (index based)
                                return (values[int.Parse(y.Trim())] as bool?) == true;
                        })
                        // and check them via an and operator
                        .Any(y => !y != true);
                        // finaly check if any and operator is true
                        }).Any(y => y == true);
                    return res;
                }
                else
                {
                    switch (rawPar)
                    {
                        case string par when par.Contains("and"):
                            return !values.Any(x => x as bool? != true);
                        case string par when par.Contains("or"):
                            return values.Any(x => x as bool? == true);
                        case string par when par.Contains("xor"):
                            return values[0] as bool? == true ^ values[1] as bool? == true;
                        case string par when par.Contains("equals"):
                            return values.Any(x => x != values[0]);
                        default:
                            throw new InvalidOperationException($"invalid boolean operator parameter: {parameter}");
                    }
                }
            }
            throw new InvalidOperationException($"{nameof(BooleanMultiValueConverter)} requires an argument");
        }
        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

    }
}
