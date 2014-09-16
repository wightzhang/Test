using System;
using System.Text;
using NLog;
namespace ServiceUtility
{
    public class LogHelper
    {
        private static readonly Logger Loginfo = LogManager.GetLogger("loginfo");
        private static readonly Logger Logerror = LogManager.GetLogger("logerror");
        private static readonly Logger Logdebug = LogManager.GetLogger("logdebug");
        private static readonly Logger LogOrderDriver = LogManager.GetLogger("logOrderDriver");
        private static readonly Logger LogBidDriver = LogManager.GetLogger("logBidDriver");

        public static void OrderDriverInfo(string orderDriverInfo, params object[] args)
        {
            if (LogOrderDriver.IsErrorEnabled)
            {
                LogOrderDriver.Info(orderDriverInfo,args);
            }
        }

        public static void DriverBidInfo(string orderBidInfo, params object[] args)
        {
            if (LogBidDriver.IsErrorEnabled)
            {
                LogBidDriver.Info(orderBidInfo,args);
            }
        }

        public static void WriteInfo(string info)
        {
            if (Loginfo.IsInfoEnabled)
            {
                Loginfo.Info(info);
            }
        }

        public static void WriteInfoError(string info)
        {
            if (Loginfo.IsErrorEnabled)
            {
                Loginfo.Error(info);
            }
        }

        public static void WriteDebug(string info)
        {
            if (Logdebug.IsDebugEnabled)
            {
                Logdebug.Debug(info);
            }
        }

        public static void WriteDebug(string info, Exception ex)
        {
            if (Logdebug.IsDebugEnabled)
            {
                Logdebug.Debug(info, ex);
            }
        }

        private static void WriteError(string info, Exception ex)
        {
            if (Logerror.IsErrorEnabled)
            {
                Logerror.Error(info, ex);
            }
        }

        public static void WriteError(string info)
        {
            if (Logerror.IsErrorEnabled)
            {
                Logerror.Error(info);
            }
        }

        public static void WriteError(Exception ex)
        {
            if (Logerror.IsErrorEnabled)
            {
                Logerror.Error(ex);
            }
        }
    }
}