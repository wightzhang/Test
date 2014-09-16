using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using UCAST.DMDTMonitor.Service.Model;
using Utility;
namespace DAL
{
    public class TrackData
    {
        public TrackData()
        { }
        /// <summary>
        /// 获得所有的MDTID设备
        /// </summary>
        /// <returns></returns>
        public DataSet GetAllCarMDTID()
        {
            DataSet ds = new DataSet();
            StringBuilder str = new StringBuilder();
            str.Append("select CarNo,MDTID from DMDTBusiness..carinfo where DeleteFlag=0 and mdtid>0 ");
            ds = SqlHelper.Query(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString());
            return ds;
        }
        //public int InsertTrackData(Track model)
        //{
        //    int result = 0;
        //    StringBuilder str = new StringBuilder();
        //    str.Append("Insert into TrackInfo(MDTID,CreateTime,ReportPoints) values(@MDTID,@CreateTime,@ReportPoints)");
        //    SqlParameter[] para = {
        //                             new SqlParameter(@"@MDTID",SqlDbType.Int),
        //                             new SqlParameter(@"@CreateTime",SqlDbType.DateTime),
        //                             new SqlParameter(@"@ReportPoints",SqlDbType.Int)
        //                         };
        //    para[0].Value = model.MDTID;
        //    para[1].Value = model.CreateTime;
        //    para[2].Value = model.ReportPoints;
        //    result = SqlHelper.ExecuteSql(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
        //    return result;
        //}
        public int InsertTrackData(Track model)
        {
            int result = 0;
            StringBuilder str = new StringBuilder();
            str.Append("Insert into CarInfoHistory(MDTID,LastLongitude,LastLatitude,CreateTime,Laststatus) values(@MDTID,@LastLongitude,@LastLatitude,@CreateTime,@Laststatus)");
            SqlParameter[] para = {
                                     new SqlParameter(@"@MDTID",SqlDbType.Int),
                                     new SqlParameter(@"@LastLongitude",SqlDbType.Decimal),
                                     new SqlParameter(@"@LastLatitude",SqlDbType.Decimal),
                                     new SqlParameter(@"@CreateTime",SqlDbType.DateTime),
                                     new SqlParameter(@"@Laststatus",SqlDbType.Int)
                                 };
            para[0].Value = model.MDTID;
            para[1].Value = model.LastLongitude;
            para[2].Value = model.LastLatitude;
            para[3].Value = model.CreateTime;
            para[4].Value = model.Laststatus;
            result = SqlHelper.ExecuteSql(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
            return result;
        }
        /// <summary>
        /// 查询所有轨迹信息
        /// </summary>
        /// <returns></returns>
        public DataSet SelectAllTrackInfo()
        {
            DataSet ds = new DataSet();
            StringBuilder str = new StringBuilder();
            str.Append("select * from TrackInfo");
            ds = SqlHelper.Query(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString());
            return ds;
        }
        public DataSet SelectTheMDTIDTrackInfo(int mdtid,DateTime time)
        {
            DataSet ds = new DataSet();
            StringBuilder str = new StringBuilder();
            str.Append("select count(*) from TrackInfo where mdtid=@mdtid and createtime=@time");
            SqlParameter[] para = {
                                     new SqlParameter(@"@mdtid",SqlDbType.Int),
                                     new SqlParameter(@"@time",SqlDbType.DateTime)
                                 };
            para[0].Value = mdtid;
            para[1].Value = time;
            ds =SqlHelper.Query(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
            return ds;
        }
        public int  UpdateTrackInfo(Track model)
        {
            int result = 0;
            StringBuilder str = new StringBuilder();
            str.Append("Update TrackInfo set MDTID=@MDTID,LastLongitude=@LastLongitude,LastLatitude=@LastLatitude,CreateTime=@CreateTime,Laststatus=@Laststatus where MDTID=@MDTID and CreateTime=@CreateTime");
            SqlParameter[] para = {
                                     new SqlParameter(@"@MDTID",SqlDbType.Int),
                                     new SqlParameter(@"@LastLongitude",SqlDbType.Decimal),
                                     new SqlParameter(@"@LastLatitude",SqlDbType.Decimal),
                                     new SqlParameter(@"@CreateTime",SqlDbType.DateTime),
                                     new SqlParameter(@"@Laststatus",SqlDbType.Int)
                                 };
            para[0].Value = model.MDTID;
            para[1].Value = model.LastLongitude;
            para[2].Value = model.LastLatitude;
            para[3].Value = model.CreateTime;
            para[4].Value = model.Laststatus;
            result = SqlHelper.ExecuteSql(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
            return result;
        }

        public DataSet GetMileageForDay(DateTime stime, DateTime etime)
        {
            StringBuilder str = new StringBuilder();
             str.Append("select CarNo,convert(varchar(10),StartHiredTime,120) as tDate, ");
             str.Append("sum(emptymileage) as emptymileage ,sum(heavymileage) as heavymileage,sum(totalfare) as TotalFare ");
             str.Append("from(");
             str.Append("select DISTINCT carno,reportno,startforhiretime,starthiredtime,emptymileage,heavymileage,totalfare  ");
             str.Append("from taximeterforprimer  a ");
             str.Append("inner join carinfo b on a.mdtid=b.mdtid  and b.deleteflag=0 ");
             str.Append("where StartHiredTime>=@stime and StartHiredTime<=@etime ) as a ");
             str.Append("GROUP BY CarNo,convert(varchar(10),StartHiredTime,120) ");
             str.Append("order by convert(varchar(10),StartHiredTime,120),CarNo");
            //StringBuilder str = new StringBuilder();
            //str.Append("select DISTINCT startforhiretime,starthiredtime,emptymileage,heavymileage from taximeterforprimer  ");
            //str.Append(" a inner join carinfo b on a.mdtid=b.mdtid  ");
            //str.Append(" where logtime>=@stime and logtime<=@etime and b.carno=@carno and b.deleteflag=0");
            SqlParameter[] para ={
                                   new SqlParameter("@stime",SqlDbType.DateTime),
                                   new SqlParameter("@etime",SqlDbType.DateTime)
                                  
                               };
            para[0].Value = stime;
            para[1].Value = etime;
         
            DataSet ds = new DataSet();
            ds = SqlHelper.Query(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
            return ds;
        }
        public DataSet GetVehicleIncomeByDay(DateTime stime, DateTime etime, string carno)
        {
            DataSet ds = new DataSet();
            StringBuilder str = new StringBuilder();
            str.Append("  select DISTINCT  B.CarNo,convert(varchar(10),A.StartHiredTime,120) as DayTime ,SUM(A.TotalFare * 0.01) AS TotalFare");
            str.Append(" from TaxiMeterForPrimer A inner join CarInfo B on A.MDTID=B.MDTID AND B.DeleteFlag = 0 where A.StartHiredTime >= @stime and A.StartHiredTime<=@etime ");
            str.Append("and b.carno=@carno ");
            str.Append("GROUP BY B.CarNo,convert(varchar(10),A.StartHiredTime,120)");
            SqlParameter[] para ={
                                  new SqlParameter("@stime",SqlDbType.DateTime),
                                  new SqlParameter("@etime",SqlDbType.DateTime),
                                  new SqlParameter("@carno",SqlDbType.Char,20)
                              };
            para[0].Value = stime;
            para[1].Value = etime;
            para[2].Value = carno;
            ds = SqlHelper.Query(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
            return ds;
        }
        public DataSet GetTotalPassenger(string carno, DateTime stime, DateTime etime)
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("select * from TaxiMeterForPrimer a inner join carinfo b on a.mdtid=b.mdtid  where b.mdtid>0 and b.deleteflag=0 and  b.carno='{0}' and  logtime>='{1}' and logtime<='{2}'  ",carno,stime,etime);
            DataSet ds = new DataSet();
            ds = SqlHelper.Query(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString());
            return ds;
        }
        /// <summary>
        /// Taxi On Road
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public int InsertTaxiOnRoadInfo(UCAST.DMDTMonitor.Service.Model.TaxiOnRoadInfo model)
        //{
        //    StringBuilder str = new StringBuilder();
        //    str.Append("insert into TaxiOnRoadInfo([Date],[Seven],[SevenThirty],[Eight],[EightThirty],[Nine],[Sixteen],[SixteenThirty],[Seventeen],[SeventeenThirty],[Eightteen],[EightteenThirty],[Nineteen],[NineteenThirty],[Total],[CarCount]) values(@Date,@Seven,@SevenThirty,@Eight,@EightThirty,@Nine,@Sixteen,@SixteenThirty,@Seventeen,@SeventeenThirty,@Eightteen,@EightteenThirty,@Nineteen,@NineteenThirty,@Total,@CarCount)");
        //    SqlParameter[] para ={
        //                            new SqlParameter("@Date",SqlDbType.DateTime),
        //                            new SqlParameter("@Seven",SqlDbType.Int),
        //                            new SqlParameter("@SevenThirty",SqlDbType.Int),
        //                            new SqlParameter("@Eight",SqlDbType.Int),
        //                            new SqlParameter("@EightThirty",SqlDbType.Int),
        //                            new SqlParameter("@Nine",SqlDbType.Int),
        //                            new SqlParameter("@Sixteen",SqlDbType.Int),
        //                            new SqlParameter("@SixteenThirty",SqlDbType.Int),
        //                            new SqlParameter("@Seventeen",SqlDbType.Int),
        //                            new SqlParameter("@SeventeenThirty",SqlDbType.Int),
        //                            new SqlParameter("@Eightteen",SqlDbType.Int),
        //                            new SqlParameter("@EightteenThirty",SqlDbType.Int),
        //                            new SqlParameter("@Nineteen",SqlDbType.Int),
        //                            new SqlParameter("@NineteenThirty",SqlDbType.Int),
        //                            new SqlParameter("@Total",SqlDbType.Int),
        //                            new SqlParameter("@CarCount",SqlDbType.Int)
        //                        };
        //    para[0].Value = model.Date;
        //    para[1].Value = model.Seven;
        //    para[2].Value = model.SevenThirty;
        //    para[3].Value = model.Eigth;
        //    para[4].Value = model.EigthThirty;
        //    para[5].Value = model.Nine;
        //    para[6].Value = model.Sixteen;
        //    para[7].Value = model.SixteenThirty;
        //    para[8].Value = model.Seventeen;
        //    para[9].Value = model.SeventeenThirty; 
        //    para[10].Value = model.Eightteen;
        //    para[11].Value = model.EightteenThirty;
        //    para[12].Value = model.Nineteen;
        //    para[13].Value = model.NineteenThirty;
        //    para[14].Value = model.Total;
        //    para[15].Value = model.CarCount;
        //    int result = 0;
        //    result = SqlHelper.ExecuteSql(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
        //    return result;
        //}
        /// <summary>
        /// Per Morning And Evening For Taxi On Road
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public int InsertPerTaxiOnRoadInDay(Model.PerMorningAndEveningForTaxiOnRoad model)
        //{
        //    StringBuilder str = new StringBuilder();
        //    str.Append("INSERT INTO [PerTaxiOnRoadInDay]([Date],[Six],[Seven],[Eight],[Nine],[Ten],[Seventeen],[Eightteen],[Nineteen],[Twenty],[TwentyOne],[TwentyTwo]) ");
        //    str.Append(" values(@Date,@Six,@Seven,@Eight,@Nine,@Ten,@Seventeen,@Eightteen,@Nineteen,@Twenty,@TwentyOne,@TwentyTwo)");
        //    SqlParameter[] para ={
        //                            new SqlParameter("@Date",SqlDbType.DateTime),
        //                            new SqlParameter("@Six",SqlDbType.Int),
        //                            new SqlParameter("@Seven",SqlDbType.Int),
        //                            new SqlParameter("@Eight",SqlDbType.Int),
        //                            new SqlParameter("@Nine",SqlDbType.Int),
        //                            new SqlParameter("@Ten",SqlDbType.Int),
        //                            new SqlParameter("@Seventeen",SqlDbType.Int),
        //                            new SqlParameter("@Eightteen",SqlDbType.Int),
        //                            new SqlParameter("@Nineteen",SqlDbType.Int),
        //                            new SqlParameter("@Twenty",SqlDbType.Int),
        //                            new SqlParameter("@TwentyOne",SqlDbType.Int),
        //                            new SqlParameter("@TwentyTwo",SqlDbType.Int)
        //                        };
        //    para[0].Value = model.Date;
        //    para[1].Value = model.Six;
        //    para[2].Value = model.Seven;
        //    para[3].Value = model.Eight;
        //    para[4].Value = model.Nine;
        //    para[5].Value = model.Ten;
        //    para[6].Value = model.Seventeen;
        //    para[7].Value = model.Eightteen;
        //    para[8].Value = model.Nineteen;
        //    para[9].Value = model.Twenty;
        //    para[10].Value = model.TwentyOne;
        //    para[11].Value = model.TwentyTwo;
        //    int result = 0;
        //    result = SqlHelper.ExecuteSql(ConfigurationManager.ConnectionStrings[@"ConnectionString"].ToString(), str.ToString(), para);
        //    return result;
        //}
    }
}
