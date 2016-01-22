/** -*- C# -*-
 * Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4n.
 *
 * ht4n is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */
namespace Hypertable.Explorer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;

    /// <summary>
    /// The timestamp to string converter.
    /// </summary>
    [ValueConversion(typeof(Key), typeof(string))]
    internal sealed class TimestampToStringConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            var key = (Key)value;
            if (targetType.IsAssignableFrom(typeof(string)))
            {
                return key.DateTime.ToString("yyyy-MM-dd HH:mm:ss.sss", CultureInfo.InvariantCulture);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// The cell info value to string converter.
    /// </summary>
    [ValueConversion(typeof(CellInfo), typeof(string))]
    internal sealed class CellInfoValueToStringConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The to char.
        /// </summary>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// </returns>
        public static char ToChar(byte b)
        {
            return b > 31 && !(b > 126 && b < 176) ? (char)b : '.';
        }

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            var cellInfo = (CellInfo)value;
            if (targetType.IsAssignableFrom(typeof(string)))
            {
                var sb = new StringBuilder(CellInfo.Limit + 4);
                if (cellInfo.ValueInfo != null)
                {
                    foreach (var b in cellInfo.ValueInfo)
                    {
                        sb.Append(ToChar(b));
                    }

                    if (cellInfo.CellValueSize > CellInfo.Limit)
                    {
                        sb.Append(" (*)");
                    }
                }

                return sb.ToString();
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// The sixteen bytes.
    /// </summary>
    internal struct SixteenBytes
    {
        #region Constants

        /// <summary>
        /// The size.
        /// </summary>
        public const int Size = 16;

        #endregion

        #region Fields

        /// <summary>
        /// The index.
        /// </summary>
        public int Index;

        /// <summary>
        /// The value.
        /// </summary>
        public byte[] Value;

        #endregion
    }

    /// <summary>
    /// The bytes to sixteen bytes converter.
    /// </summary>
    [ValueConversion(typeof(byte[]), typeof(IEnumerable))]
    internal sealed class BytesToSixteenBytesConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            var bytes = (byte[])value;
            if (targetType.IsAssignableFrom(typeof(IEnumerable)))
            {
                var observable = new ObservableList<SixteenBytes>();
                if (bytes != null && bytes.Length > 0)
                {
                    Task.Factory.StartNew(
                        () =>
                            {
                                const int ChunkSize = 16;
                                var chunk = new List<SixteenBytes>(ChunkSize);
                                int n;
                                var len = bytes.Length;
                                for (n = 0; n < len; n += SixteenBytes.Size)
                                {
                                    chunk.Add(new SixteenBytes { Value = bytes, Index = n });
                                    if (chunk.Count == chunk.Capacity)
                                    {
                                        observable.AddRange(chunk);
                                        chunk = new List<SixteenBytes>(ChunkSize);
                                    }
                                }

                                if (n < len)
                                {
                                    chunk.Add(new SixteenBytes { Value = bytes, Index = n });
                                }

                                if (chunk.Count > 0)
                                {
                                    observable.AddRange(chunk);
                                }
                            });
                }

                return observable;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// The sixteen bytes to address converter.
    /// </summary>
    [ValueConversion(typeof(SixteenBytes), typeof(string))]
    internal sealed class SixteenBytesToAddrConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            var sixteenBytes = (SixteenBytes)value;
            if (targetType.IsAssignableFrom(typeof(string)))
            {
                return (sixteenBytes.Index / SixteenBytes.Size).ToString("X8", CultureInfo.InvariantCulture);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// The sixteen bytes to hex converter.
    /// </summary>
    [ValueConversion(typeof(SixteenBytes), typeof(string))]
    internal sealed class SixteenBytesToHexConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            var sixteenBytes = (SixteenBytes)value;
            if (targetType.IsAssignableFrom(typeof(string)))
            {
                var sb = new StringBuilder(SixteenBytes.Size * 3);
                for (var n = sixteenBytes.Index; n < Math.Min(sixteenBytes.Value.Length, sixteenBytes.Index + SixteenBytes.Size); ++n)
                {
                    sb.Append(sixteenBytes.Value[n].ToString("X2", CultureInfo.InvariantCulture));
                    sb.Append(" ");
                }

                sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// The sixteen bytes to string converter.
    /// </summary>
    [ValueConversion(typeof(SixteenBytes), typeof(string))]
    internal sealed class SixteenBytesToStringConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            var sixteenBytes = (SixteenBytes)value;
            if (targetType.IsAssignableFrom(typeof(string)))
            {
                var sb = new StringBuilder(SixteenBytes.Size);
                for (var n = sixteenBytes.Index; n < Math.Min(sixteenBytes.Value.Length, sixteenBytes.Index + SixteenBytes.Size); ++n)
                {
                    sb.Append(CellInfoValueToStringConverter.ToChar(sixteenBytes.Value[n]));
                }

                return sb.ToString();
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}