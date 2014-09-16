using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Configuration;
using DAL;
using Microsoft.Office.Interop.Excel;
using UCAST.DMDTMonitor.Service.Model;
using System.Runtime.InteropServices;
using APIService.Utility;
using ServiceUtility;
using Application = Microsoft.Office.Interop.Excel.Application;
using DataTable = System.Data.DataTable;

namespace TaxiDataPool_Update_
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "本次为第一次运行";
            textBox1.ReadOnly = true;
            string str =
                DateTime.Now.AddMonths(1).Year + "-" +
                DateTime.Now.AddMonths(1).Month + "-01 1:00:00";
            textBox2.Text = str;
            textBox2.ReadOnly = true;
            timer1.Start();
        }

        //bool flag = false;  

        private static DataTable xSheetTable;
        private static DataTable XSheet2Table;

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Visible = false;
            int firstTaxiOnRoad = 0;
            Application xApp = new Application();
            xApp.Visible = false;

            //打开现有Excel文件
            Workbook xBook =
                xApp.Workbooks.Open(ConfigurationManager.AppSettings[@"ExcelPath"].ToString(),
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            //xBook=xApp.Workbooks.Add(Missing.Value);

            DateTime stime = dateTimePicker2.Value;
            DateTime etime = dateTimePicker1.Value;

            Worksheet xSheet1 = (Worksheet)xBook.Sheets[1];
            xSheet1.Cells[2, 1] = dateTimePicker1.Value.Year;//填入年份
            xSheet1.Cells[2, 2] = dateTimePicker2.Value.Month;//填入月份
            Worksheet xSheet2 = (Worksheet)xBook.Sheets[2];
            Worksheet xSheet3 = (Worksheet)xBook.Sheets[3];
            Worksheet xSheet4 = (Worksheet)xBook.Sheets[4];
            Worksheet xSheet5 = (Worksheet)xBook.Sheets[5];
            Worksheet xSheet6 = (Worksheet)xBook.Sheets[6];
            Worksheet xSheet7 = (Worksheet)xBook.Sheets[7];
            Worksheet xSheet8 = (Worksheet)xBook.Sheets[8];

            Dictionary<int, int> dic = new Dictionary<int, int>();

            label3.Text = " 该月车辆数据已生成成功";
            TrackData dal = new TrackData();

            List<DateTime> listDt = new List<DateTime>();

            //将需要导出的日期放入列表中
            while (etime.Date.CompareTo(stime.Date) >= 0)
            {
                listDt.Add(stime);
                stime = stime.AddDays(1);
            }

            stime = dateTimePicker2.Value;

            int datecount = listDt.Count;

            DataSet ds = dal.GetAllCarMDTID();
            int carCount = ds.Tables[0].Rows.Count;

            xSheet2.Cells[2, 5] = carCount;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            button1.Text = "正在获取中";
            button1.Enabled = false;

            if (carCount > 0)
            {
                int excelMileageRow = 8;
                //int[] LogCarMaxiedMileageCount = new int[32];

                LogHelper.WriteInfo(DateTime.Now.ToString());
                DateTime MileageETime = Convert.ToDateTime(etime.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
                DataSet ds2 = new DataSet();
                ds2 = dal.GetMileageForDay(stime.Date, MileageETime);//一次性获取所有的里程计价器数据
                Dictionary<string, string> dict = new Dictionary<string, string>();
                for (int l = 0; l < ds2.Tables[0].Rows.Count; l++)//存入字典,方便取用
                {
                    dict.Add(ds2.Tables[0].Rows[l]["carno"].ToString().Trim() +
                        Convert.ToDateTime(ds2.Tables[0].Rows[l]["tDate"]).Date,
                        ds2.Tables[0].Rows[l]["emptymileage"] + "," +
                        ds2.Tables[0].Rows[l]["heavymileage"] + "," +
                        ds2.Tables[0].Rows[l]["TotalFare"]);
                }

                for (int i = 0; i < carCount; i++)//循环所有车辆
                {
                    LogHelper.WriteInfo(firstTaxiOnRoad.ToString());
                    LogHelper.WriteInfo(DateTime.Now.ToString());
                    excelMileageRow++;

                    //if (ExcelMileageRow == 500)
                    //{
                    //    ExcelMileageRow.ToString();
                    //}
                    int mdtid = Convert.ToInt32(ds.Tables[0].Rows[i]["MDTID"]);
                    string carno = ds.Tables[0].Rows[i]["carno"].ToString().Trim();

                    xSheet1.Cells[i + 4, 1] = carno;//填入车牌,通过工作表的函数 自动填充其他工作表所要用到的车牌


                    DataSet ds4 = dal.GetTotalPassenger(carno, stime, etime);
                    if (ds4.Tables[0].Rows.Count > 0)
                    {
                        xSheet1.Cells[i + 4, 2] = ds4.Tables[0].Rows.Count;
                    }
                    else
                    {
                        xSheet1.Cells[i + 4, 2] = 0;
                    }

                    xSheetTable = new DataTable(carno);

                    xSheetTable.Columns.Add("dateTime", typeof(DateTime));
                    xSheetTable.Columns.Add("xSheet2", typeof(float));
                    xSheetTable.Columns.Add("xSheet3", typeof(float));
                    xSheetTable.Columns.Add("xSheet4", typeof(float));
                    xSheetTable.Columns.Add("xSheet5", typeof(float));
                    xSheetTable.Columns.Add("xSheet6", typeof(float));

                    DataColumn[] clos = new DataColumn[1];
                    clos[0] = xSheetTable.Columns["dateTime"];
                    xSheetTable.PrimaryKey = clos;

                    //开始导出
                    if (datecount != 0)
                    {  //ArrayList[] arr = new ArrayList[ListDT.Count];

                        Thread[] t = new Thread[datecount];

                        for (int x = 0; x < datecount; x++)
                        {
                            double revenue;
                            double vacant;
                            double hired;
                            double mileage;
                            if (dict.ContainsKey(carno.Trim() + listDt[x].Date))
                            {
                                string[] arr = dict[carno.Trim() + listDt[x].Date].Split(',');
                                //xSheet5.Cells[ExcelMileageRow, stime.Day + 1] = Convert.ToDouble(arr[2]) / 100;//计价器数据
                                //xSheet4.Cells[ExcelMileageRow, stime.Day + 1] = Convert.ToDouble(arr[0]) / 10;//空车里程数
                                //xSheet3.Cells[ExcelMileageRow, stime.Day + 1] = Convert.ToDouble(arr[1]) / 10;//重车里程数
                                //xSheet2.Cells[ExcelMileageRow, stime.Day + 1] = (Convert.ToDouble(arr[0]) + Convert.ToDouble(arr[1])) / 10;//总里程数
                                revenue = Convert.ToDouble(arr[2]) / 100;
                                vacant = Convert.ToDouble(arr[0]) / 10;
                                hired = Convert.ToDouble(arr[1]) / 10;
                                mileage = (Convert.ToDouble(arr[0]) + Convert.ToDouble(arr[1])) / 10;
                            }
                            else
                            {
                                //xSheet5.Cells[ExcelMileageRow, stime.Day + 1] = 0;//计价器数据
                                //xSheet4.Cells[ExcelMileageRow, stime.Day + 1] = 0;//空车里程数
                                //xSheet3.Cells[ExcelMileageRow, stime.Day + 1] = 0;//重车里程数
                                //xSheet2.Cells[ExcelMileageRow, stime.Day + 1] = 0;//总里程数
                                revenue = 0;
                                vacant = 0;
                                hired = 0;
                                mileage = 0;
                            }
                            //ExportPara threadPara = new ExportPara();
                            GetxSheetData getdata = new GetxSheetData
                            {
                                Mdtid = mdtid,
                                Current = listDt[x],
                                FirstTaxiOnRoad = firstTaxiOnRoad,
                                Carno = carno,
                                Mileage = mileage,
                                Hired = hired,
                                Revenue = revenue,
                                Vacant = vacant,
                                Dr = xSheetTable.NewRow()
                            };

                            t[x] = new Thread(getdata.Exportt);

                            t[x].Start();

                            Thread.Sleep(20);

                        }

                        for (int y = 0; y < datecount; y++)
                        {
                            while (t[y].IsAlive)
                            {
                                t[y].Join();//join方法可行
                                //Thread.Sleep(10);
                            }
                        }

                        DataView dv = xSheetTable.DefaultView;

                        var query =
                            from item in xSheetTable.AsEnumerable()
                            orderby item["dateTime"]
                            select item;

                        dv = query.AsDataView();
                        DataTable newtable = dv.ToTable();

                        //xSheet_ds.Tables.Add(newtable);
                        for (int j = 0; j < listDt.Count; j++)
                        {
                            try
                            {
                                if (!Convert.IsDBNull(newtable.Rows[j]["dateTime"]))
                                {
                                    xSheet2.Cells[8, j + 2] = newtable.Rows[j]["dateTime"];
                                }
                                if (!Convert.IsDBNull(newtable.Rows[j]["xSheet2"]))
                                {
                                    xSheet2.Cells[excelMileageRow, j + 2] = Convert.ToDouble(newtable.Rows[j]["xSheet2"]).ToString("F1");
                                }
                                if (!Convert.IsDBNull(newtable.Rows[j]["xSheet3"]))
                                {
                                    xSheet3.Cells[excelMileageRow, j + 2] = Convert.ToDouble(newtable.Rows[j]["xSheet3"]).ToString("F1");
                                }
                                if (!Convert.IsDBNull(newtable.Rows[j]["xSheet4"]))
                                {
                                    xSheet4.Cells[excelMileageRow, j + 2] = Convert.ToDouble(newtable.Rows[j]["xSheet4"]).ToString("F1");
                                }
                                if (!Convert.IsDBNull(newtable.Rows[j]["xSheet5"]))
                                {
                                    xSheet5.Cells[excelMileageRow, j + 2] = Convert.ToDouble(newtable.Rows[j]["xSheet5"]).ToString("F2");
                                }
                                if (!Convert.IsDBNull(newtable.Rows[j]["xSheet6"]))
                                {
                                    xSheet6.Cells[excelMileageRow, j + 2] = Convert.ToDouble(newtable.Rows[j]["xSheet6"]).ToString("F1");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteError(ex.Message + xSheetTable.Rows.Count);
                            }

                        }

                        xSheetTable.Dispose();
                    }
                    //LogHelper.WriteInfo(DateTime.Now.ToString());
                    firstTaxiOnRoad++;
                }
                LogHelper.WriteInfo(DateTime.Now.ToString());


            }

            //LogHelper.WriteInfo(DateTime.Now.ToString());

            //wrietesheet7();
            //Thread[] th7 = new Thread[datecount];
            //int temp = 0;

            //xSheet2_table = new System.Data.DataTable();
            //xSheet2_table.Columns.Add("col0", typeof(DateTime));

            //for (int n = 1; n < 25; n++)
            //{
            //    string names = "col" + n.ToString();
            //    xSheet2_table.Columns.Add(names, typeof(Int32));
            //}

            //foreach (DateTime dt in ListDT)
            //{
            //    GetSheet7Data getdata7 = new GetSheet7Data();
            //    getdata7.Current7 = dt;
            //    getdata7.dr7 = xSheet2_table.NewRow();
            //    th7[temp] = new Thread(getdata7.wrietesheet7);
            //    th7[temp].Start();
            //    temp++;
            //}

            //for (int y = 0; y < datecount; y++)
            //{
            //    while (th7[y].IsAlive)
            //    {
            //        th7[y].Join();//join方法可行
            //        //Thread.Sleep(10);
            //    }
            //}

            //DataView dv7 = new DataView();
            //dv7 = xSheet2_table.DefaultView;

            //var query7 =
            //    from item in xSheet2_table.AsEnumerable()
            //    orderby item["col0"]
            //    select item;

            //dv7 = query7.AsDataView();
            //System.Data.DataTable newtable7 = dv7.ToTable();

            //for (int u = 0; u < xSheet2_table.Rows.Count; u++)
            //{
            //    for (int v = 0; v < xSheet2_table.Columns.Count; v++)
            //    {
            //        xSheet7.Cells[6 + u, v + 1] = newtable7.Rows[u][v];
            //    }
            //}

            sw.Stop();
            label6.Text = sw.Elapsed.ToString();
            button1.Text = "获取结束";
            LogHelper.WriteInfo(DateTime.Now.ToString());

            string path = ConfigurationManager.AppSettings[@"ExcelSavePath"].ToString();
            xBook.SaveAs(ConfigurationManager.AppSettings[@"ExcelSavePath"].ToString() + "Taxi_Data_" + etime.Month + ".xls",
                Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                XlSaveAsAccessMode.xlNoChange,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            xSheet1 = null;
            xSheet2 = null;
            xSheet3 = null;
            xSheet4 = null;
            xSheet5 = null;
            xSheet6 = null;
            xSheet7 = null;
            xSheet8 = null;
            xBook = null;
            xApp.Quit();
            label3.Visible = true;
        }

        private class GetxSheetData
        {
            public int Mdtid { get; set; }
            public int FirstTaxiOnRoad { get; set; }
            public string Carno { get; set; }
            public DateTime Current { get; set; }
            public DataRow Dr { get; set; }
            public double Mileage { get; set; }
            public double Hired { get; set; }
            public double Vacant { get; set; }
            public double Revenue { get; set; }

            public void Exportt()
            {
                //DataRow dr = xSheet_table.NewRow();
                //ExportPara exoprtPara = _exportPara as ExportPara;
                //DateTime current;
                Dr["dateTime"] = Current.Date;
                TimeChange modeltime = ChangeDateTime(Current);

                //int mdtid = exoprtPara.Mdtid;
                //int TaxiOn = exoprtPara.FirstTaxiOnRoad;
                //string carno = exoprtPara.Carno;
                //LogHelper.WriteInfo(current.ToShortDateString());
                //终止时间，每天的23：59：59
                //DateTime ETime = Convert.ToDateTime(current.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

                #region --读数据库
                //DataSet ds2 = new DataSet();
                //TrackData dal = new TrackData();

                //ds2 = dal.GetMileageForDay(current.Date, ETime, carno.Trim());//从数据库中读取里程


                //if (ds2.Tables[0].Rows.Count > 0)
                //{
                //    double TheDayEmptyMil = 0;
                //    double TheDatHeavyMil = 0;
                //    for (int g = 0; g < ds2.Tables[0].Rows.Count; g++)
                //    {
                //        TheDayEmptyMil += Convert.ToDouble(ds2.Tables[0].Rows[g]["emptymileage"]);
                //        TheDatHeavyMil += Convert.ToDouble(ds2.Tables[0].Rows[g]["heavymileage"]);
                //    }
                //    double TotalMil = TheDayEmptyMil + TheDatHeavyMil;
                //    dr["xSheet4"] = TheDayEmptyMil / 10;//空车里程数
                //    dr["xSheet3"] = TheDatHeavyMil / 10;//重车里程数
                //    dr["xSheet2"] = TotalMil / 10;//总里程数
                //}

                //else
                //{
                //    //若无数据,显示为0
                //    dr["xSheet2"] = 0;
                //    dr["xSheet3"] = 0;
                //    dr["xSheet4"] = 0;
                //}

                //DataSet ds6 = new DataSet();

                //ds6 = dal.GetVehicleIncomeByDay(current.Date, ETime, carno);//读取收入
                //if (ds6.Tables[0].Rows.Count > 0)
                //{
                //    dr["xSheet5"] = ds6.Tables[0].Rows[0]["TotalFare"].ToString();//每天收入
                //}
                //else
                //{
                //    dr["xSheet5"] = 0;
                //}
                #endregion

                //读取轨迹文件
                int[] perarr = new int[24];

                int logtime = 0;
                int cou;
                int result = 0;
                Type anytype = typeof(CAR_OffSpeed_T);

                int size = Marshal.SizeOf(anytype);
                byte[] hd = new byte[size];
                DateTime time = Current.Date;

                string localpath = ConfigurationManager.AppSettings[@"TrackPath"].ToString();
                const string trackFilePathFormat = "{0}{1}\\{2}\\{3}\\{4}.{5}{2}{3}";

                //int sum = 0;//累积报点数
                var mdtids = String.Format("{0:X8}", Mdtid);
                var filepath = string.Format(trackFilePathFormat, localpath, modeltime.Year,
                    modeltime.Month, modeltime.Day, mdtids, modeltime.ShortYr);
                //filepath = "..\\00000419.130619";
                //Track model = new Track();
                //model.MDTID = mdtid;
                //model.CreateTime = time;
                int strcounMin = 0;

                try
                {
                    StreamReader objReader = new StreamReader(filepath);
                    BinaryReader objByteReader = new BinaryReader(objReader.BaseStream);
                    DateTime logstime = new DateTime();
                    do
                    {
                        try
                        {
                            result = objByteReader.Read(hd, 0, size);
                            if (result == size)
                            {
                                CAR_OffSpeed_T baseinfo = (CAR_OffSpeed_T)ByteToStruct(hd, anytype);
                                CarTrackExt cartrackext = new CarTrackExt();
                                cartrackext.Track = baseinfo;
                                //cartrackext.CarNo = mdtid.ToString();

                                // --出租车在线时长
                                if (cartrackext.Track.BExtStatus % 2 == 1)//点火
                                {
                                    if (logtime == 1)//前一个状态也为点火
                                    {
                                        if (cartrackext.Track.CarTrack_Time.Hour - logstime.Hour >= 0)//俩个状态时间符合规范
                                        {
                                            strcounMin += (cartrackext.Track.CarTrack_Time.Hour * 60
                                                + cartrackext.Track.CarTrack_Time.Minute)
                                                - (logstime.Hour * 60 + logstime.Minute);
                                        }
                                    }
                                    logtime = 1;
                                    logstime = cartrackext.Track.CarTrack_Time;
                                }
                                if (cartrackext.Track.BExtStatus % 2 == 0) //熄火
                                {
                                    DateTime logetime;
                                    if (logtime == 1)//上一个状态为点火
                                    {
                                        logetime = cartrackext.Track.CarTrack_Time;
                                        if ((logetime.TimeOfDay.Hours - logstime.TimeOfDay.Hours) >= 0)
                                        {
                                            strcounMin += (logetime.TimeOfDay.Hours * 60
                                                + logetime.TimeOfDay.Minutes)
                                                - (logstime.TimeOfDay.Hours * 60
                                                + logstime.TimeOfDay.Minutes);
                                        }

                                    }
                                    logtime = 0;
                                    logetime = cartrackext.Track.CarTrack_Time;
                                }
                                //pertotal[cartrackext.Track.CarTrack_Time.Hour] += 1;
                            }
                        }
                        catch
                        { }
                    } while (result > 0);

                    objReader.Dispose();
                    objByteReader.Dispose();
                }
                catch (Exception Ex)
                { }

                cou = strcounMin / 60;
                double tempstr = Convert.ToDouble(strcounMin % 60) / 60;
                Dr["xSheet6"] = cou + Math.Round(tempstr, 1);//每天在线时
                Dr["xSheet2"] = Mileage;
                Dr["xSheet3"] = Hired;
                Dr["xSheet4"] = Vacant;
                Dr["xSheet5"] = Revenue;
                try
                {
                    xSheetTable.Rows.Add(Dr);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(ex.Message);
                }

            }

        }

        public class GetSheet7Data
        {
            public DateTime Current7 { get; set; }
            public DataRow dr7 { get; set; }

            public void wrietesheet7()
            {
                //DateTime stime = dateTimePicker2.Value;
                //DateTime etime = dateTimePicker1.Value;
                //int FirstTaxiOnRoad = 0;
                //List<DateTime> listDT = new List<DateTime>();
                //将需要导出的日期放入列表中
                //while (etime.Date.CompareTo(stime.Date) >= 0)
                //{
                //    listDT.Add(stime);
                //    stime = stime.AddDays(1);
                //}

                TrackData track = new TrackData();
                DataSet ds = new DataSet();
                ds = track.GetAllCarMDTID();

                int carcount = ds.Tables[0].Rows.Count;


                string localpath = ConfigurationManager.AppSettings[@"TrackPath"];
                const string trackFilePathFormat = "{0}{1}\\{2}\\{3}\\{4}.{5}{2}{3}";
                Type anytype = typeof(CAR_OffSpeed_T);
                int size = Marshal.SizeOf(anytype);

                //for (int n = 0; n < listDT.Count; n++)
                //{
                LogHelper.WriteInfo(DateTime.Now.ToString());
                //FirstTaxiOnRoad = 0;

                TimeChange modeltime = ChangeDateTime(Current7);

                //DataRow sheet_row = xSheet2_table.NewRow();

                dr7[0] = Current7.Date;

                for (int m = 1; m < 25; m++)
                {
                    dr7[m] = 0;
                }

                for (int i = 0; i < carcount; i++)//循环所有车辆
                {
                    int[] pertotal = new int[24];
                    int mdtid = Convert.ToInt32(ds.Tables[0].Rows[i]["MDTID"]);

                    //int logtime = 0;
                    int result = 0;

                    byte[] hd = new byte[size];
                    //DateTime time = dt.Date;

                    //int sum = 0;//累积报点数
                    var mdtids = String.Format("{0:X8}", mdtid);
                    var filepath = string.Format(trackFilePathFormat, localpath, modeltime.Year,
                        modeltime.Month, modeltime.Day, mdtids, modeltime.ShortYr);
                    //filepath = "..\\00000419.130619";
                    //Track model = new Track();
                    //model.MDTID = mdtid;
                    //model.CreateTime = time;
                    //int strcounMin = 0;
                    try
                    {
                        StreamReader objReader = new StreamReader(filepath);
                        BinaryReader objByteReader = new BinaryReader(objReader.BaseStream);

                        do
                        {
                            try
                            {
                                result = objByteReader.Read(hd, 0, size);
                                if (result == size)
                                {
                                    CAR_OffSpeed_T baseinfo = (CAR_OffSpeed_T)ByteToStruct(hd, anytype);
                                    CarTrackExt cartrackext = new CarTrackExt();
                                    cartrackext.Track = baseinfo;

                                    #region --出租车在线时长
                                    //if (cartrackext.Track.BExtStatus % 2 == 1)//点火
                                    //{
                                    //    if (logtime == 1)//前一个状态也为点火
                                    //    {
                                    //        if (cartrackext.Track.CarTrack_Time.Hour - logstime.Hour >= 0)//俩个状态时间符合规范
                                    //        {
                                    //            strcounMin += (cartrackext.Track.CarTrack_Time.Hour * 60 
                                    //                + cartrackext.Track.CarTrack_Time.Minute) 
                                    //                - (logstime.Hour * 60 + logstime.Minute);
                                    //        }
                                    //    }
                                    //    logtime = 1;
                                    //    logstime = cartrackext.Track.CarTrack_Time;
                                    //}
                                    //if (cartrackext.Track.BExtStatus % 2 == 0) //熄火
                                    //{
                                    //    if (logtime == 1)//上一个状态为点火
                                    //    {
                                    //        logetime = cartrackext.Track.CarTrack_Time;
                                    //        if ((logetime.TimeOfDay.Hours - logstime.TimeOfDay.Hours) >= 0)
                                    //        {
                                    //            strcounMin += (logetime.TimeOfDay.Hours * 60 
                                    //                + logetime.TimeOfDay.Minutes) 
                                    //                - (logstime.TimeOfDay.Hours * 60 
                                    //                + logstime.TimeOfDay.Minutes);
                                    //        }

                                    //    }
                                    //    logtime = 0;
                                    //    logetime = cartrackext.Track.CarTrack_Time;
                                    //}
                                    #endregion

                                    pertotal[cartrackext.Track.CarTrack_Time.Hour] += 1;

                                }
                            }
                            catch
                            { }
                        } while (result > 0);

                        objReader.Dispose();
                        objByteReader.Dispose();
                    }
                    catch (Exception Ex)
                    { }

                    int[] Perarr = new int[24];
                    for (int m = 0; m < 24; m++)
                    {
                        if (pertotal[m] >= 10)
                        {
                            Perarr[m] = 1;
                        }
                        else
                        {
                            Perarr[m] = 0;
                        }
                        if (Convert.IsDBNull(dr7[m + 1]))
                        {
                            dr7[m + 1] = 0;
                        }
                        dr7[m + 1] = Convert.ToInt32(dr7[m + 1]) + Perarr[m];
                    }
                    //FirstTaxiOnRoad++;
                }
                XSheet2Table.Rows.Add(dr7);
                //}
            }

        }


        public static TimeChange ChangeDateTime(DateTime dtBegin)
        {
            TimeChange timeModel = new TimeChange();
            timeModel.Year = dtBegin.Year.ToString();
            timeModel.ShortYr = (dtBegin.Year - 2000).ToString().PadLeft(2, '0'); String.Format("{0:D2}", dtBegin.Year);
            timeModel.Month = String.Format("{0:D2}", dtBegin.Month);
            timeModel.Day = String.Format("{0:D2}", dtBegin.Day);
            return timeModel;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]

        public struct CAR_OffSpeed_T
        {
            /*
             * time_t timeReport; //回报时间
             * BYTE nStatus; // 状态
             * DWORD dwLongitude; // 经度
             * DWORD dwLatitude; // 纬度
             * USHORT nSpeed; // 速度 km/h
             * BYTE nDirection; // 方向 单位2度
             * BYTE nExtStatus; // 扩展状态
             * BYTE nReserved; // 保留状态
            */
            private uint _uiTime_T;
            private byte _bStatus;
            private uint _uiLongitude;
            private uint _uiLatitude;
            private ushort _usSpeed;
            private byte _bDirection;
            private byte _bExtStatus;
            private byte _bReserved;
            public DateTime CarTrack_Time
            {
                get { return UConvert.ToDateTime(Convert.ToInt32(_uiTime_T)); }
                set { _uiTime_T = (uint)UConvert.To_time_t(value); }
            }
            public byte BStatus
            {
                get { return _bStatus; }
            }
            public double Longitude
            {
                get { return UConvert.ToDegree(Convert.ToInt32(_uiLongitude)); }
                set { _uiLongitude = (uint)UConvert.ToCent(value); }
            }
            public double Latitude
            {
                get { return UConvert.ToDegree(Convert.ToInt32(_uiLatitude)); }
                set { _uiLatitude = (uint)UConvert.ToCent(value); }
            }
            public ushort Speed
            {
                get { return _usSpeed; }
                set { _usSpeed = value; }
            }
            public byte BDirection
            {
                get { return _bDirection; }
            }
            public byte BExtStatus
            {
                get { return _bExtStatus; }
            }
            public byte BReserved
            {
                get { return _bReserved; }
            }


        }

        //车辆信息
        public class CarTrackExt
        {
            private CAR_OffSpeed_T _Track;

            public CAR_OffSpeed_T Track
            {
                get { return _Track; }
                set { _Track = value; }
            }

            private string _CarNo;

            public string CarNo
            {
                get { return _CarNo; }
                set { _CarNo = value; }
            }

            public double Longitude
            {
                get { return _Track.Longitude; }
                set { _Track.Longitude = value; }
            }
            public double _Latitude;
            public double Latitude
            {
                get { return _Track.Latitude; }
                set { _Track.Latitude = value; }
            }
            public ushort _Speed;
            public ushort Speed
            {
                get { return _Track.Speed; }
                set { _Track.Speed = value; }
            }
            public DateTime _CarTrack_Time;
            public DateTime CarTrack_Time
            {

                get { return _Track.CarTrack_Time; }
                set { _Track.CarTrack_Time = value; }
            }
        }

        private static object ByteToStruct(byte[] bytes, Type type)
        {
            object obj = null;
            int size = Marshal.SizeOf(type);
            IntPtr structPtr = Marshal.AllocHGlobal(size); //分配内存
            Marshal.Copy(bytes, 0, structPtr, size);
            obj = Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr); //释放空间
            return obj;
        }

        //定时导出        
        /*
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Day == 1 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                if (flag == false)
                {
                    
                }
                int FirstTaxiOnRoad = 0;
                Microsoft.Office.Interop.Excel.Application xApp = new Microsoft.Office.Interop.Excel.Application();
                xApp.Visible = false;
                Microsoft.Office.Interop.Excel.Workbook xBook = xApp.Workbooks.Open(@"D:\\WebTaxiDataPool\\DownFile\\Taxi Data Jul 2013 Plus_001.xls",//打开现有Excel文件
               Missing.Value, Missing.Value, Missing.Value, Missing.Value
               , Missing.Value, Missing.Value, Missing.Value, Missing.Value
               , Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                //xBook=xApp.Workbooks.Add(Missing.Value);

                Microsoft.Office.Interop.Excel.Worksheet xSheet1 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[1];
                xSheet1.Cells[2, 1] = dateTimePicker1.Value.Year;//填入年份
                xSheet1.Cells[2, 2] = dateTimePicker2.Value.Month;//填入月份
                Microsoft.Office.Interop.Excel.Worksheet xSheet2 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[2];
                Microsoft.Office.Interop.Excel.Worksheet xSheet3 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[3];
                Microsoft.Office.Interop.Excel.Worksheet xSheet4 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[4];
                Microsoft.Office.Interop.Excel.Worksheet xSheet5 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[5];
                Microsoft.Office.Interop.Excel.Worksheet xSheet6 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[6];
                Microsoft.Office.Interop.Excel.Worksheet xSheet7 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[7];
                Microsoft.Office.Interop.Excel.Worksheet xSheet8 = (Microsoft.Office.Interop.Excel.Worksheet)xBook.Sheets[8];
                Dictionary<int, int> dic = new Dictionary<int, int>();
                DateTime stime = DateTime.Now.AddMonths(-1);
                DateTime etime = DateTime.Now.AddDays(-1);
                label3.Text = stime.Year + stime.Month + " 该月车辆数据已生成成功";
                DAL.TrackData dal = new DAL.TrackData();
                
                DataSet ds = new DataSet();
                ds = dal.GetAllCarMDTID();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    button1.Text = "正在获取中";
                    int ExcelMileageRow = 8;
                    int[] LogCarMaxiedMileageCount = new int[32];
                    xSheet2.Cells[2, 5] = ds.Tables[0].Rows.Count - 600;
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    for (int i = 0; i < ds.Tables[0].Rows.Count - 600; i++)//循环所有车辆
                    {
                        ExcelMileageRow++;
                        if (ExcelMileageRow == 500)
                        {
                            ExcelMileageRow.ToString();
                        }
                        int mdtid = Convert.ToInt32(ds.Tables[0].Rows[i]["MDTID"]);
                        string carno = ds.Tables[0].Rows[i]["carno"].ToString().Trim();
                        xSheet1.Cells[i + 4, 1] = carno;//填入车牌,通过工作表的函数 自动填充其他工作表所要用到的车牌
                        stime = DateTime.Now.AddMonths(-1);
                        DataSet ds4 = new DataSet();
                        ds4 = dal.GetTotalPassenger(carno, stime, etime);
                        if (ds4.Tables[0].Rows.Count > 0)
                        {
                            xSheet1.Cells[i + 4, 2] = ds4.Tables[0].Rows.Count;
                        }
                        else
                        {
                            xSheet1.Cells[i + 4, 2] = 0;
                        }
                        while (etime.Date.CompareTo(stime.Date) >= 0)//根据选择的时间段进行数据填充
                        {
                            //export(xBook, mdtid, dal, ExcelMileageRow, FirstTaxiOnRoad, stime, carno);//调用填充方法
                            stime = stime.AddDays(1);
                        }
                        FirstTaxiOnRoad++;
                    }
                    sw.Stop();
                   // button1.Text = "消耗时间=" + sw.Elapsed.ToString();
                }
                xBook.SaveAs("D:\\WebTaxiDataPool\\DownFile\\Taxi Data " + etime.Month + ".xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value,
    Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Missing.Value, Missing.Value, Missing.Value,
    Missing.Value, Missing.Value);

                //  xApp.SaveWorkspace("D:\\Taxi Data Jul 1.xls", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);  
                xSheet1 = null;
                xSheet2 = null;
                xSheet3 = null;
                xSheet4 = null;
                xSheet5 = null;
                xSheet6 = null;
                xSheet7 = null;
                xSheet8 = null;
                xBook = null;
                xApp.Quit();
                textBox1.Text = DateTime.Now.ToString();
                textBox2.Text = DateTime.Now.AddMonths(1).ToString();
            }
        }
        */
    }
}
