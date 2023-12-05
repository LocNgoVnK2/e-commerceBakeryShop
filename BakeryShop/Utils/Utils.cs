using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BakeryShop.Utils
{
    public static class Utils
    {
        public static double ExtractDistance(string distanceString)
        {
            string numericPart = new string(distanceString.Where(c => char.IsDigit(c) || c == '.').ToArray());

            if (double.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            else
            {

                return 0.0;
            }
        }
    }
}
