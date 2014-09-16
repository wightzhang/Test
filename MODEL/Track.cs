using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCAST.DMDTMonitor.Service.Model
{
    public class Track
    {
        public Track()
        { }
        private int _MDTID;

        public int MDTID
        {
            get { return _MDTID; }
            set { _MDTID = value; }
        }
        private double _LastLatitude;

        public double LastLatitude
        {
            get { return _LastLatitude; }
            set { _LastLatitude = value; }
        }
        private double _LastLongitude;

        public double LastLongitude
        {
            get { return _LastLongitude; }
            set { _LastLongitude = value; }
        }
        private DateTime _CreateTime;

        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        private int _Laststatus;

        public int Laststatus
        {
            get { return _Laststatus; }
            set { _Laststatus = value; }
        }

    }
}
