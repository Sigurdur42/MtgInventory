using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Serilog;

namespace MtgInventory.Views
{
    public class StringToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = value as string;
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            try
            {
                var localFile = new FileInfo(name);
                return !localFile.Exists ? null : new Bitmap(localFile.FullName);
            }
            catch (Exception error)
            {
                Log.Error($"Error loading image file: {error}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}