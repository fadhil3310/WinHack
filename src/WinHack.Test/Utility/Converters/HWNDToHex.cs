using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Windows.Win32.Foundation;

namespace WinHack.Test.Utility.Converters
{
		[ValueConversion(typeof(HWND), typeof(string))]
		public class HWNDToHex : IValueConverter
		{
				public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
				{
						HWND hwnd = (HWND)value;
						return hwnd.ToString() ?? "";
				}

				public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
				{
						throw new NotImplementedException();
				}
		}
}
