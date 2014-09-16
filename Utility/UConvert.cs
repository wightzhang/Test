using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Globalization;
namespace APIService.Utility
{
    public class UConvert
    {
        public const double LONG_DISTANCE = 39940.67;			// 地球子午线长，单位：km
        public const double LATI_DISTANCE = 40075.36;			// 赤道长，单位：km
        public const double DIST_LONG_PER_DEGREE = (LONG_DISTANCE / 360);	// 纬度改变一度，对应经线的距离，单位：km
        public const double DIST_LONG_PER_CENT = DIST_LONG_PER_DEGREE / 60;	// 纬度改变一分，对应经线的距离，单位：km
        public const double DIST_LONG_PER_SEC = (DIST_LONG_PER_CENT) / 60;	// 纬度改变一秒，对应经线的距离，单位：km
        public static DateTime MIN_DATE_TIME = DateTime.ParseExact(@"1975-01-02 12:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InstalledUICulture);
        /// <summary>
        /// 将经纬度转换为千分之一分
        /// </summary>
        public static int ToCent(double data)
        {
            double Min = data * 60 * 1000;
            return Convert.ToInt32(Min);
        }
        /// <summary>
        /// 将经纬度转换为度
        /// </summary>
        public static double ToDegree(int data)
        {
            double degree = (double)data / (60 * 1000);
            return Math.Round(degree, 6);
        }
        private static DateTime origin = System.TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));  // time_t起始时间
        /// <summary>
        /// 转换为datetime
        /// </summary>
        /// <param name="time_t"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(int time_t)
        {
            DateTime convertedValue = origin + new TimeSpan(time_t * TimeSpan.TicksPerSecond);
            if (System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(convertedValue) == true)
            {
                System.Globalization.DaylightTime daylightTime = System.TimeZone.CurrentTimeZone.GetDaylightChanges(convertedValue.Year);
                convertedValue = convertedValue + daylightTime.Delta;
            }
            return convertedValue;
        }
        /// <summary>
        /// 转换为time_t
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int To_time_t(DateTime time)
        {
            DateTime convertedValue = time;
            if (System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(convertedValue) == true)
            {
                System.Globalization.DaylightTime daylightTime = System.TimeZone.CurrentTimeZone.GetDaylightChanges(convertedValue.Year);
                convertedValue = convertedValue - daylightTime.Delta;
            }
            long diff = convertedValue.Ticks - origin.Ticks;
            return (int)(diff / TimeSpan.TicksPerSecond);
        }

        public static string GetDirectionString(int LastDirection)
        {
            string szDirection = string.Empty;
            int Direction = LastDirection * 2;
            if (Direction == 0)
                szDirection = "North";
            else if (Direction < 90)
                szDirection = string.Format("North {0} degrees east", Direction);
            else if (Direction == 90)
                szDirection = "East";
            else if (Direction < 180)
                szDirection = string.Format("South {0} degrees East", Direction - 90);
            else if (Direction == 180)
                szDirection = "Due south";
            else if (Direction < 270)
                szDirection = string.Format("South {0} degrees west", Direction - 180);
            else if (Direction == 270)
                szDirection = "Due west";
            else if (Direction < 360)
                szDirection = string.Format("North {0} degrees west", Direction - 270);
            return szDirection;
        }

        // 相应经度改变一度对应纬线的距离，单位：km
        private static double DIST_LATI_PER_DEGREE(double dLati)
        {
            double dDistance = LATI_DISTANCE * Math.Sin((90.0f - dLati) * 3.1415926 / 180.0f) / 360;
            return dDistance;
        }
        // 相应经度改变一分对应纬线的距离，单位：km
        private static double DIST_LATI_PER_CENT(double dLati)
        {
            double dDistance = DIST_LATI_PER_DEGREE(dLati) / 60;
            return dDistance;
        }
        // 相应经度改变一秒对应纬线的距离，单位：km
        private static double DIST_LATI_PER_SEC(double dLati)
        {
            double dDistance = DIST_LATI_PER_CENT(dLati) / 60;
            return dDistance;
        }

        public static double GetMapDistance(double x1, double y1, double x2, double y2)
        {
            double dLongDis = (x1 - x2) * 3600.0f * DIST_LATI_PER_SEC(y1 / 2 + y2 / 2);
            double dLatiDis = (y2 - y1) * 3600.0f * DIST_LONG_PER_SEC;

            double dDistance = Math.Sqrt(Math.Pow(dLongDis, 2) + Math.Pow(dLatiDis, 2));
            return dDistance;
        }

        public static DateTime FilterDateTime(DateTime src)
        {
            if (src == null)
                return MIN_DATE_TIME;
            if (src < MIN_DATE_TIME)
                return MIN_DATE_TIME;
            return src;
        }

        public static object ByteToStruct(byte[] bytes, Type type)
        {
            object obj = null;
            int size = System.Runtime.InteropServices.Marshal.SizeOf(type);
            IntPtr structPtr = Marshal.AllocHGlobal(size); //分配内存
            Marshal.Copy(bytes, 0, structPtr, size);
            obj = Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr); //释放空间
            return obj;
        }

    }

}
