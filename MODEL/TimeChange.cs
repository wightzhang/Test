using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCAST.DMDTMonitor.Service.Model
{
   public class TimeChange
    {
       public TimeChange()
       { }
       private string _year;

       public string Year
       {
           get { return _year; }
           set { _year = value; }
       }
       private string _month;

       public string Month
       {
           get { return _month; }
           set { _month = value; }
       }
       private string _day;

       public string Day
       {
           get { return _day; }
           set { _day = value; }
       }
       private string _shortYr;

       public string ShortYr
       {
           get { return _shortYr; }
           set { _shortYr = value; }
       }
    }
}
