using System;

namespace SharedLib.Class
{
    public class CheckParameter
    {
        public static bool IsDateParameter(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return DateTime.TryParse(input, out DateTime FilterDate);
            }
            return false;
        }
        public static bool IsIndexParameter(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return int.TryParse(input, out int index);
            }
            return false;
        }
    }
}
