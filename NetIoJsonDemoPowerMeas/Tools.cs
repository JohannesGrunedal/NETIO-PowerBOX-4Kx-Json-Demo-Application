namespace NetIoJsonDemo
{
    using NetIo;
    using System;
    using System.Text.RegularExpressions;
    using System.Windows.Controls;

    public static class Tools
    {
        /// <summary>
        /// Convert input string to enum.        
        /// </summary>
        /// <typeparam name="T">Enum type to convert to</typeparam>
        /// <param name="enumString">String to convert to enum</param>
        /// <returns></returns>
        public static T StringToEnum<T>(this string enumString)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), enumString);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Convert input enum to its string representation.
        /// </summary>
        /// <param name="typ">Enum type</param>
        /// <param name="isCapital">Set to true to convert enum value to upper case letter</param>
        /// <returns></returns>
        public static string EnumToString(this Enum typ, bool isCapital = false)
        {
            if (isCapital)
            {
                return Enum.GetName(typ.GetType(), typ).ToUpper();
            }
            else
            {
                return Enum.GetName(typ.GetType(), typ);
            }
        }

        /// <summary>
        /// Convert double to string with set number of decimals.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static string DoubleToString(this double number, int decimals = 0)
        {
            return number.ToString($"N{decimals}").Replace(",", ".");
        }

        public static NetIoDriver.OutputName ButtonToOutput(this Button button)
        {
            return (NetIoDriver.OutputName)Enum.Parse(typeof(NetIoDriver.OutputName), Regex.Match(button.Name, @"Output_(\d{1}|All)").Value);
        }

        public static NetIoDriver.Action ButtonToAction(this Button button)
        {
            return (NetIoDriver.Action)Enum.Parse(typeof(NetIoDriver.Action), Regex.Match(button.Name, @"(On|Off)").Value);
        }
    }
}
