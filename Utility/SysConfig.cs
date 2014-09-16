//************************************************************************
//DMDTMonitor
//Ver:1.0
//Copyright:ShangHai UCAST LTD
//Url:http://www.ucastcomputer.com/
//Author:HuangRui
//Email:hr520xx@hotmail.com
//Date:2009-01-22
//************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Linq;

namespace Utility
{
    public class SysConfig
    {
       

        private static bool _IsDebug = (ConfigurationManager.AppSettings[@"IsDebug"] == @"true");

        public static bool IsDebug
        {
            get { return SysConfig._IsDebug; }
            set { SysConfig._IsDebug = value; }
        }
        private static string _ConnectionString = ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString();
        public static string ConnectionString
        {
            get { return SysConfig._ConnectionString; }
            set { SysConfig._ConnectionString = value; }
        }
        private static string _User = ConfigurationManager.AppSettings[@"User"];
        

        public static string User
        {
            get { return SysConfig._User; }
            set { SysConfig._User = value; }
        }
        private static int _UserID = Convert.ToInt32(ConfigurationManager.AppSettings[@"UserID"]);

        public static int UserID
        {
            get { return SysConfig._UserID; }
            set { SysConfig._UserID = value; }
        }
        private static string _DualCommnicates = ConfigurationManager.AppSettings[@"DualCommnicates"];

        public static string DualCommnicates
        {
            get { return SysConfig._DualCommnicates; }
            set { SysConfig._DualCommnicates = value; }
        }
        private static string _ServiceOrder = ConfigurationManager.AppSettings[@"ServiceOrder"];

        public static string ServiceOrder
        {
            get { return SysConfig._ServiceOrder; }
            set { SysConfig._ServiceOrder = value; }
        }
        private static string _APIOrderID = ConfigurationManager.AppSettings[@"APIOrderID"];

        public static string APIOrderID
        {
            get { return SysConfig._APIOrderID; }
            set { SysConfig._APIOrderID = value; }
        }

        private static string _APIOrderIDTwo = ConfigurationManager.AppSettings[@"APIOrderIDTwo"];

        public static string APIOrderIDTwo
        {
            get { return SysConfig._APIOrderIDTwo; }
            set { SysConfig._APIOrderIDTwo = value; }
        }
        private static string _JobNoTwo = ConfigurationManager.AppSettings[@"JobNoTwo"];

        public static string JobNoTwo
        {
            get { return SysConfig._JobNoTwo; }
            set { SysConfig._JobNoTwo = value; }
        }
        private static string _JobNo = ConfigurationManager.AppSettings[@"JobNo"];

        public static string JobNo
        {
            get { return SysConfig._JobNo; }
            set { SysConfig._JobNo = value; }
        }
        private static string _CheckNo = ConfigurationManager.AppSettings[@"CheckNo"];

        public static string CheckNo
        {
            get { return SysConfig._CheckNo; }
            set { SysConfig._CheckNo = value; }
        }
        private static string _CheckNoTwo = ConfigurationManager.AppSettings[@"CheckNoTwo"];

        public static string CheckNoTwo
        {
            get { return SysConfig._CheckNoTwo; }
            set { SysConfig._CheckNoTwo = value; }
        }
       
    }
}
