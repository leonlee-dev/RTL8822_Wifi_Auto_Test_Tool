//#define DEBUG
//#define HW_TX_MODE
using IQAPI_C_shap;
using RTKModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
//using static RTL8822_Wifi_Auto_Test_Tool.EVTReport;

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public class TestTask
    {
        //public static readonly log4net.ILog log = LogManager.GetLogger(typeof(TestTask));

        private const IQmeasure.IQAPI_PORT_ENUM port = IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT;
        private const byte IndexCryUpperLimit = 0x7f; // MSB reserved
        private const byte IndexCryLowerLimit = 0x00;
        private const byte TxPowerUpperLimit = 0x7f; // 0 ~ 127
        private const byte TxPowerLowerLimit = 0x00;

        private static int testTimeout = 10000; // default analyze data timeout
        private const byte defaultIndexCry = 0x5A; // crystal calibration value

        private static readonly string saveMapDir = Application.StartupPath + "\\Map";
        private static readonly string sampleMapPath = Application.StartupPath + "\\Sample\\sample.map";
        private static EfuseMap efuseMap;

        private static byte indexCryOk;

        private static bool powerResult = false;
        private static bool evmResult = false;
        private static bool freqErrResult = false;
        private static bool leakageResult = false;
        private static bool flatnessResult = false;

        private static string powerFormat;
        private static string evmFormat;
        private static string freqErrFormat;
        private static string leakageFormat;
        private static string flatnessFormat;
        private static string maskFormat;
        private static string therFormat;
        private static string gainFormat;

        private static string titleFormat;
        private static string valueFormat;
        private static string rangeFormat;

        private static void InitAnalyzeValueResult(bool result = false)
        {
            powerResult = result;
            evmResult = result;
            freqErrResult = result;
            leakageResult = result;
            flatnessResult = result;
        }

        private static void InitAnalyzeValueFormat(WIFI protocol)
        {
            powerFormat = "   {0,-15}:{1,10:0.00} dBm ({2},{3})";
            freqErrFormat = "   {0,-15}:{1,10:0.00} ppm ({2},{3})";
            flatnessFormat = "   {0,-15}:{1,10:0.00} dB (,{2})";
            therFormat = "   {0,-15}:{1,10:x2}";
            gainFormat = "   {0,-15}:{1,10:x2}";
            switch (protocol)
            {
                case WIFI.B:
                    // Gain|MPower|EVM|FreqErr|Thermal|Flatness
                    titleFormat = "{0,15}|{1,15}|{2,15}|{3,15}|{4,15}|{5,15}";
                    valueFormat = "{0,15}|{1,15:0.00}|{2,15:0.00}|{3,15:0.00}|{4,15}|{5,15:0.00}";
                    rangeFormat = "{0,15}|{1,15}|{2,15}|{3,15}|{4,15}|{5,15}";
                    evmFormat = "   {0,-15}:{1,10:0.00} %   (,{2})";
                    break;
                case WIFI.AG:
                case WIFI.N:
                case WIFI.AC:
                default:
                    // Gain|MPower|EVM|FreqErr|Thermal|Flatness|Lo_Leakage
                    titleFormat = "{0,15}|{1,15}|{2,15}|{3,15}|{4,15}|{5,15}|{6,15}";
                    valueFormat = "{0,15}|{1,15:0.00}|{2,15:0.00}|{3,15:0.00}|{4,15}|{5,15:0.00}|{6,15:0.00}";
                    rangeFormat = "{0,15}|{1,15}|{2,15}|{3,15}|{4,15}|{5,15}|{6,15}";
                    evmFormat = "   {0,-15}:{1,10:0.00} dB  (,{2})";
                    leakageFormat = "   {0,-15}:{1,10:0.00} dB  (,{2})";
                    break;
            }
        }

        private static void CalcInterpolation(int startEfusePos, int endEfusePos)
        {
            int v1 = -1, v2 = -1, calibPos1 = -1, calibPos2 = -1, step = 1;
            for (int i = startEfusePos; i <= endEfusePos; i++)
            {
                byte v = efuseMap.ReadMap(i);
                if (v != 0xFF)
                {
                    if (v1 == -1)
                    {
                        calibPos1 = i;
                        v1 = v;
                        step = 1;
                    }
                    else if (v2 == -1)
                    {
                        calibPos2 = i;
                        v2 = v;
                        break;
                    }
                }
                else
                    step++;
            }

            if (v1 == -1 || v2 == -1)
                throw new Exception("pls, make sure calibration of two group at least should be done!");

            int diff = (v2 - v1) / step;

            // set data to start group
            byte startEfusePosData = efuseMap.ReadMap(startEfusePos);
            if (startEfusePosData == 0xFF)
            {
                startEfusePosData = (byte)(efuseMap.ReadMap(calibPos1) - diff * (calibPos1 - startEfusePos));
                efuseMap.WriteMap(startEfusePos, startEfusePosData);
            }

            for (int i = startEfusePos; i < calibPos1; i++)
            {
                byte v = efuseMap.ReadMap(i);
                if (v == 0xFF)
                    efuseMap.WriteMap(i, (byte)(startEfusePosData + diff * (i - startEfusePos)));
            }

            for (int i = calibPos1; i < calibPos2; i++)
            {
                byte v = efuseMap.ReadMap(i);
                if (v == 0xFF)
                    efuseMap.WriteMap(i, (byte)(v1 + diff * (i - calibPos1)));
            }

            for (int i = calibPos2; i <= endEfusePos; i++)
            {
                byte v = efuseMap.ReadMap(i);
                if (v == 0xFF)
                    efuseMap.WriteMap(i, (byte)(v2 + diff * (i - calibPos2)));
            }
        }

        public static TestResult Init(TestStatus testStatus, object obj = null)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".Init DUT...";
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);

                if (SysConfig.connectionInterface.StartsWith("COM"))
                {
                    if (SysConfig.platform == "Bananapi")
                    {
                        Regex userLoginRegex = new Regex(@"\w+\s*login\s*:\s*");
                        Regex rootPasswdRegex = new Regex(@"password\s*for\s*pi\s*:\s*");
                        Regex userHeaderRegex = new Regex(@"\w+\s*@\s*\w+\s*:\s*\S+\s*[#$]{1}");
                        string returnString = "";
                        if(!mainForm.rtwCommand.WaitFor("\n", userHeaderRegex, ref returnString, 2000, 1))
                        {
                            mainForm.rtwCommand.WaitFor("root", "Password:", 2000); // user
                            mainForm.rtwCommand.WaitFor("bananapi", userHeaderRegex, ref returnString, 2000); // password
                        } 
                    }
                }

                if (!mainForm.rtwCommand.Init(SysConfig.drivFile, SysConfig.drivDir, SysConfig.powerLimit, SysConfig.powerByRate))
                {
                    msg = "Init fail!";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }

                if (!mainForm.rtwCommand.StartMp())
                {
                    msg = "Start MP fail!";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }

                try
                {
                    // init
                    efuseMap = new EfuseMap(EfuseMap.WLMAPSIZE);
                    efuseMap.LoadMap(sampleMapPath);

                    // to produce test report
                    //txTestList = new List<TxTest>();
                    //rxTestList = new List<RxTest>();
                }
                catch (Exception e)
                {
                    mainForm.testLog.WriteLine(e.Message);
                    mainForm.PCMessageWrite(e.Message, true);
                    return TestResult.TEST_FAILURE;
                }

                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            return TestResult.TEST_SUCCESS;
        }

        //public static TestResult Reinit(TestStatus testStatus, Rtw rtw)
        //{
        //    Form1 mainForm = Form1.GetMainForm();

        //    string msg;

        //    if (testStatus == TestStatus.TEST_INIT)
        //    {
        //        msg = (mainForm.testNum + 1) + ".Reinit DUT...";
        //        mainForm.testLog.WriteLine(msg + "\r\n");
        //        mainForm.PCMessageWrite(msg + "\r\n", true);

        //        //msg = "IP:" + Form1.adbIp;
        //        //if (!mainForm.adb.isConnected)
        //        //{
        //        //    msg += "\r\nDUT isn't connected!";
        //        //    mainForm.testLog.WriteLine(msg);
        //        //    mainForm.PCMessageWrite(msg, true);
        //        //    return TestResult.TEST_FAILURE;
        //        //}

        //        //msg += "\r\nDUT is connected!\r\n";
        //        //mainForm.testLog.WriteLine(msg);
        //        //mainForm.PCMessageWrite(msg, true);

        //        if (RtwCommand.Init(SysConfig.drivFile, SysConfig.drivDir))
        //        {
        //            return TestResult.TEST_SUCCESS;
        //        }

        //        msg = "Send command fail!";
        //        mainForm.testLog.WriteLine(msg);
        //        mainForm.PCMessageWrite(msg, true);
        //        return TestResult.TEST_FAILURE;
        //    }

        //    if (testStatus == TestStatus.TEST_END)
        //    {
        //        return TestResult.TEST_SUCCESS;
        //    }

        //    return TestResult.TEST_SUCCESS;
        //}

        public static TestResult InitTester(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".Init Tester...";
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);

                try
                {
                    // init IQxel
                    IQxel.init();
                    msg = IQxel.getIQxelString();
                    mainForm.testLog.Write(msg);
                    mainForm.PCMessageWrite(msg);
                    // connect IQxel
                    IQxel.connectTester();
                    msg = IQxel.getIQxelString();
                    mainForm.testLog.Write(msg);
                    mainForm.PCMessageWrite(msg);

                    if (!IQxel.bIsConnectTester)
                    {
                        msg = "Tester isn't connected!\r\n";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                        return TestResult.TEST_FAILURE;
                    }

                    if (!IQxel.obtainControl())
                    {
                        msg = "Not obtain control of Tester!\r\n";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                        return TestResult.TEST_FAILURE;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                mainForm.testLog.WriteLine("");
                mainForm.PCMessageWrite("", true);

                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult PreHeating(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;
            RtwTx tx = (RtwTx)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".Pre-Heating...";
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);

                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            try
            {
                /*
                 * 2.4G channel 7 40M
                 * record 5 to 10 mins in Packet Rx mode
                 */
                int therRx =  mainForm.rtwCommand.GetThermalValue();
#if HW_TX_MODE
                if (mainForm.rtwCommand.StartHwTxCommand(tx))
#else
                if (mainForm.rtwCommand.StartSwTxCommand(tx, false)
                 && mainForm.rtwCommand.SendTxPowerCommand(tx.antPath, 0x64, 0x64))
#endif
                {
                    DateTime preheatingTime = DateTime.Now;
                    int therHeating;
                    while (true)
                    {
                        Thread.Sleep(1000);
                        therHeating = mainForm.rtwCommand.GetThermalValue();
                        if (therHeating >= SysConfig.targetThermal)
                        {
                            //msg = "Reach to desired thermal: 0x" + therHeating.ToString("X2") + "\r\n";
                            msg = "TherRx: 0x" + therRx.ToString("X2") + ", TherHeating: 0x" + therHeating.ToString("X2") + "\r\n";
                            mainForm.testLog.WriteLine(msg);
                            mainForm.PCMessageWrite(msg, true);

                            // wait for defined last time
                            if (SysConfig.thermalWaitingTime > 0)
                                Thread.Sleep(SysConfig.thermalWaitingTime * 1000);
                            break;
                        }
                        //int diff = therHeating - therRx;
                        //if (therHeating >= therRx && diff >= 5)
                        //    break;
                        //else if (diff > 3)
                        //{
                        //    msg = "Pls cold down DUT to Thermal Rx:" + therRx + "!";
                        //    mainForm.testLog.WriteLine(msg);
                        //    mainForm.PCMessageWrite(msg, true);
                        //    return TestResult.TEST_FAILURE;
                        //}
                    }
#if HW_TX_MODE
                    mainForm.rtwCommand.StopHwTxCommand();
#else
                    mainForm.rtwCommand.StopSwTxCommand();
#endif
                    return TestResult.TEST_SUCCESS;
                }

                msg = "Send Tx command fail!";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
                
            }
        }

        public static TestResult CrystalCalib(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;
            CrystalCalibPlan crystalCalib = (CrystalCalibPlan)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".CRYSTAL_CALIBRATION Frequency:" + crystalCalib.freq + " (limit : " + crystalCalib.crystalCriterion.freqErrLower + " ~ " + crystalCalib.crystalCriterion.freqErrUpper + " ppm)";
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);

                try
                {
                    //double cableloss = mainForm.GetCableLoss(tx.freq, 0);
                    IQxel.setIQxelVsa(port, crystalCalib.freq, 0, (crystalCalib.rateID == RATE_ID.R_11M) ? 3 : 10, 0); // VSA

                    if (
#if HW_TX_MODE
                        mainForm.rtwCommand.StartHwTxCommand(crystalCalib)
#else
                        mainForm.rtwCommand.StartSwTxCommand(crystalCalib, false)
#endif
                     && mainForm.rtwCommand.IndexCryNextCommand(defaultIndexCry))
                    {
                        Thread.Sleep(1000);
                        return TestResult.TEST_SUCCESS;
                    }

                    msg = "Send Tx command fail!";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
#if HW_TX_MODE
                    mainForm.rtwCommand.StopHwTxCommand();
#else
                    mainForm.rtwCommand.StopSwTxCommand();
#endif
                    return TestResult.TEST_FAILURE;
                }
                catch (Exception ex)
                {
#if HW_TX_MODE
                    mainForm.rtwCommand.StopHwTxCommand();
#else
                    mainForm.rtwCommand.StopSwTxCommand();
#endif
                    mainForm.testLog.WriteLine(ex.Message);
                    mainForm.PCMessageWrite(ex.Message, true);
                    return TestResult.TEST_FAILURE;
                }
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            // analyze WIFI data
            msg = "Tuning frequency offset...";
            mainForm.testLog.WriteLine(msg);
            mainForm.PCMessageWrite(msg, true);

            int tryCountIfCrystalOffsetOk = 0;
            try
            {
                DateTime startCaptureTime = DateTime.Now;
                double timeDiff = 0;
                testTimeout = 10000;

                byte indexCryNext = defaultIndexCry;

                while ((timeDiff = DateTime.Now.Subtract(startCaptureTime).TotalMilliseconds) < testTimeout)
                {
                    Thread.Sleep(100);
                    try
                    {
                        // Perform data capture
                        IQxel.vsaDataCapture();
                        // Perform analysis
                        IQxel.analyze80211n();
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message + ", retry...";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
#if HW_TX_MODE
                        mainForm.rtwCommand.StopHwTxCommand();
#else
                        mainForm.rtwCommand.StopSwTxCommand();
#endif
                        if (
#if HW_TX_MODE
                           mainForm.rtwCommand.StartHwTxCommand(crystalCalib)
#else
                           mainForm.rtwCommand.StartSwTxCommand(crystalCalib, false)
#endif
                        && mainForm.rtwCommand.IndexCryNextCommand(defaultIndexCry))
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        msg = "Send Tx command fail!";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                        return TestResult.TEST_FAILURE;
                    }

                    // get scalar from IQxel
                    double mCrystalOffset = IQxel.getScalarMeasurement("symClockErrorPpm");
                    Console.WriteLine(mCrystalOffset);
                    // frequency offset between -2 ~ 2 ppm ??
                    if (mCrystalOffset > crystalCalib.crystalCriterion.freqErrUpper || mCrystalOffset < crystalCalib.crystalCriterion.freqErrLower)
                    {
                        indexCryNext = (byte)Math.Round(indexCryNext - (mCrystalOffset * crystalCalib.freq / 2500));

                        if (indexCryNext <= IndexCryUpperLimit && indexCryNext >= IndexCryLowerLimit)
                            mainForm.rtwCommand.IndexCryNextCommand(indexCryNext);
                        else
                        {
                            msg = "Index Crystal: 0x" + indexCryNext.ToString("X2") + " is out of range!";
                            mainForm.testLog.WriteLine(msg);
                            mainForm.PCMessageWrite(msg, true);
                            return TestResult.TEST_FAILURE;
                        }
                    }
                    else
                    {
                        if (tryCountIfCrystalOffsetOk < 10)
                        {
                            tryCountIfCrystalOffsetOk++;
                            continue;
                        }

                        msg = "Crystal Offset: " + mCrystalOffset + " ppm";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg);
                        break;
                    }

                    // reset tryCount
                    tryCountIfCrystalOffsetOk = 0;
                }

                if (timeDiff >= testTimeout)
                {
                    msg = "timeout.";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }

                indexCryOk = indexCryNext;

                // write to corresponding position of memory
                int p1 = 0xB9, p2 = 0x110, p3 = 0x111;
                efuseMap.WriteMap(p1, indexCryOk);
                efuseMap.WriteMap(p2, indexCryOk);
                efuseMap.WriteMap(p3, indexCryOk);

                msg = ", Index Crystal: 0x" + indexCryOk.ToString("X2") + ", Efuse: 0x" + p1.ToString("X2") + " 0x" + p2.ToString("X2") + " 0x" + p3.ToString("X2") + "\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
#if HW_TX_MODE
                mainForm.rtwCommand.StopHwTxCommand();
#else
                mainForm.rtwCommand.StopSwTxCommand();
#endif
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult TxIndexCalib(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;
            TxCalibPlan txCalib = (TxCalibPlan)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".TX_CALIBRATION Frequency:" + txCalib.freq + " Bandwidth:" + Wifi.bwDic[txCalib.bw] + " Tx Mode:" + Wifi.rateIdDic[txCalib.rateID] + " ANT:" + Wifi.antPathDic[txCalib.antPath].ToUpper() + " Target Power:" + txCalib.calibCriterion.targetPower + "dBm\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);

                try
                {
                    double cableloss = SysConfig.GetCableLoss(txCalib.freq, (int)txCalib.antPath);
                    // p_to_Avg_diff = 10 (802.11ag,n,ac), = 3 (802.11b)
                    IQxel.setIQxelVsa(port, txCalib.freq, (int)txCalib.calibCriterion.targetPower, (txCalib.rateID == RATE_ID.R_11M) ? 3 : 10, cableloss); // VSA

                    if (
#if HW_TX_MODE
                        mainForm.rtwCommand.StartHwTxCommand(txCalib)
#else
                        mainForm.rtwCommand.StartSwTxCommand(txCalib, false)
#endif
                     && mainForm.rtwCommand.IndexCryNextCommand(indexCryOk)
                     && mainForm.rtwCommand.SendTxPowerCommand(txCalib.antPath, txCalib.defaultPower[(int)txCalib.antPath]))
                    {
                        Thread.Sleep(1000);
                        return TestResult.TEST_SUCCESS;
                    }

                    msg = "Send Tx command fail!";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
#if HW_TX_MODE
                    mainForm.rtwCommand.StopHwTxCommand();
#else
                    mainForm.rtwCommand.StopSwTxCommand();
#endif
                    return TestResult.TEST_FAILURE;
                }
                catch (Exception ex)
                {
#if HW_TX_MODE
                    mainForm.rtwCommand.StopHwTxCommand();
#else
                    mainForm.rtwCommand.StopSwTxCommand();
#endif
                    mainForm.testLog.WriteLine(ex.Message);
                    mainForm.PCMessageWrite(ex.Message, true);
                    return TestResult.TEST_FAILURE;
                }
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            // analyze WIFI data
            try
            {
                DateTime startCaptureTime = DateTime.Now;
                double timeDiff = 0;
                testTimeout = 20000;

                bool result = true;
                bool powerResult = true;
                bool freqErrResult = true;
                bool evmResult = true;
                double freqErr = 0.0;
                double mPower = 0.0;
                double evm = 0.0;
                string strPowerResult = "";
                string strFreqErrResult = "";
                string strEvmResult = "";
                string powerFormat = "";
                string freqErrFormat = "";
                string evmFormat = "";

                int tryCount = 0;

                double cableloss = SysConfig.GetCableLoss(txCalib.freq, (int)txCalib.antPath);
                byte curTxPower = txCalib.defaultPower[(int)txCalib.antPath];

                while ((timeDiff = DateTime.Now.Subtract(startCaptureTime).TotalMilliseconds) < testTimeout)
                {
                    Thread.Sleep(100);

                    freqErr = 0.0;
                    mPower = 0.0;
                    evm = 0.0;

                    try
                    {
                        // Perform data capture
                        IQxel.vsaDataCapture();

                        // Perform analysis
                        if (txCalib.rateID == RATE_ID.R_11M)
                        {
                            IQxel.analyze80211b();
                            //mPower = IQxel.getScalarMeasurement("rmsPower") + cableloss;
                            freqErr = IQxel.getScalarMeasurement("clockErr");
                            evm = Math.Pow(10, IQxel.getScalarMeasurement("evmAll") / 20) * 100;
                            IQxel.analyzePower();
                            mPower = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm") + cableloss;

                            powerFormat = "{0,10}:{1,10:0.00} dBm ({2},{3})";
                            freqErrFormat = "{0,10}:{1,10:0.00} ppm ({2},{3})";
                            evmFormat = "{0,10}:{1,10:0.00} %   (,{2})";
                        }
                        else if (txCalib.rateID == RATE_ID.HTMCS7)
                        {
                            IQxel.analyze80211n();
                            //mPower = IQxel.getScalarMeasurement("rxRmsPowerDb") + cableloss;
                            freqErr = IQxel.getScalarMeasurement("symClockErrorPpm");
                            evm = IQxel.getScalarMeasurement("evmAvgAll");
                            IQxel.analyzePower();
                            mPower = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm") + cableloss;

                            powerFormat = "{0,10}:{1,10:0.00} dBm ({2},{3})";
                            freqErrFormat = "{0,10}:{1,10:0.00} ppm ({2},{3})";
                            evmFormat = "{0,10}:{1,10:0.00} dB  (,{2})";
                        }
                        else
                        {
                            msg = "Not support this rate ID for testing tx calibration!";
                            mainForm.testLog.WriteLine(msg);
                            mainForm.PCMessageWrite(msg, true);
                            return TestResult.TEST_FAILURE;
                        }
                    }
                    catch (Exception ex)
                    {
                        mainForm.testLog.WriteLine(ex.Message + ", retry...");
                        mainForm.PCMessageWrite(ex.Message + ", retry...", true);
#if HW_TX_MODE
                        mainForm.rtwCommand.StopHwTxCommand();
#else
                        mainForm.rtwCommand.StopSwTxCommand();
#endif
                        if (
#if HW_TX_MODE
                            mainForm.rtwCommand.StartHwTxCommand(txCalib)
#else
                            mainForm.rtwCommand.StartSwTxCommand(txCalib, false)
#endif
                         && mainForm.rtwCommand.IndexCryNextCommand(indexCryOk)
                         && mainForm.rtwCommand.SendTxPowerCommand(txCalib.antPath, curTxPower))
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        msg = "Send Tx command fail!";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                        return TestResult.TEST_FAILURE;
                    }

                    powerResult = true;
                    freqErrResult = true;
                    evmResult = true;

                    if (mPower < txCalib.calibCriterion.powerLower || mPower > txCalib.calibCriterion.powerUpper)
                    {
                        powerResult = false;
                        strPowerResult = "--> FAIL";
                    }
                    else
                    {
                        strPowerResult = "--> PASS";
                    }

                    if (freqErr < txCalib.calibCriterion.freqErrLower || freqErr > txCalib.calibCriterion.freqErrUpper)
                    {
                        freqErrResult = false;
                        strFreqErrResult = "--> FAIL";
                    }
                    else
                    {
                        strFreqErrResult = "--> PASS";
                    }

                    if (txCalib.calibCriterion.evm < evm)
                    {
                        evmResult = false;
                        strEvmResult = "--> FAIL";
                    }
                    else
                    {
                        strEvmResult = "--> PASS";
                    }

                    if (!(curTxPower <= TxPowerUpperLimit && curTxPower >= TxPowerLowerLimit))
                    {
                        msg = "Tx index power is over range!";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                        return TestResult.TEST_FAILURE;
                    }

                    result = powerResult && freqErrResult && evmResult;
                    if (result)
                    {
                        tryCount++;
                        if (tryCount >= 3)
                            break;
                        continue;
                    }
                    else
                    {
                        if (!freqErrResult)
                            break;
                        if (!powerResult)
                            curTxPower += (byte)Math.Round((txCalib.calibCriterion.targetPower - mPower) * 4);
                        else if (!evmResult)
                            curTxPower -= 1;
                        tryCount = 0;
                    }

                    mainForm.rtwCommand.SendTxPowerCommand(txCalib.antPath, curTxPower);
                    //Thread.Sleep(1000);
                }

                string strPower = string.Format(powerFormat, "MPower", mPower, txCalib.calibCriterion.powerLower, txCalib.calibCriterion.powerUpper);
                string strFreqErr = string.Format(freqErrFormat, "Freq Err", freqErr, txCalib.calibCriterion.freqErrLower, txCalib.calibCriterion.freqErrUpper);
                string strEvm = string.Format(evmFormat, "EVM", evm, txCalib.calibCriterion.evm);

                strPower = string.Format("{0}{1," + (60 - strPower.Length) + "}", strPower, strPowerResult);
                strFreqErr = string.Format("{0}{1," + (60 - strFreqErr.Length) + "}", strFreqErr, strFreqErrResult);
                strEvm = string.Format("{0}{1," + (60 - strEvm.Length) + "}", strEvm, strEvmResult);

                int calibTher = mainForm.rtwCommand.GetThermalValue();

                mainForm.testLog.WriteLine(strPower);
                mainForm.testLog.WriteLine(strFreqErr);
                mainForm.testLog.WriteLine(strEvm);
                mainForm.testLog.WriteLine("Termal: " + calibTher);
                mainForm.testLog.WriteLine("");
                mainForm.PCMessageWrite(strPower, true);
                mainForm.PCMessageWrite(strFreqErr, true);
                mainForm.PCMessageWrite(strEvm, true);
                mainForm.PCMessageWrite("Termal: " + calibTher);
                mainForm.PCMessageWrite("", true);

                if (timeDiff >= testTimeout)
                {
                    msg = "timeout.";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }

                if (!result)
                {
                    return TestResult.TEST_FAILURE;
                }

                byte txPowerOk = curTxPower;
                int writeEfusePos = EfuseMap.GetPosition(txCalib.rateID, txCalib.bw, txCalib.antPath, txCalib.freq);

                // write to corresponding position of memory
                efuseMap.WriteMap(writeEfusePos, txPowerOk);

                msg = "Tx Index: 0x" + txPowerOk.ToString("X2") + ", Efuse: 0x" + writeEfusePos.ToString("X2") + "\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
#if HW_TX_MODE
                mainForm.rtwCommand.StopHwTxCommand();
#else
                mainForm.rtwCommand.StopSwTxCommand();
#endif
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult InterpolationOfTxIndex(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".Calc tx power...";
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);

                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            try
            {
                // 2.4G band CCK 11M
                // ant a
                int startEfusePos = 0x10, endEfusePos = 0x15;
                CalcInterpolation(startEfusePos, endEfusePos);
                // ant b
                startEfusePos = 0x3A;
                endEfusePos = 0x3F;
                CalcInterpolation(startEfusePos, endEfusePos);

                // 2.4G band HTMCS7 40M
                // ant a
                startEfusePos = 0x16;
                endEfusePos = 0x1A;
                CalcInterpolation(startEfusePos, endEfusePos);
                // ant b
                startEfusePos = 0x40;
                endEfusePos = 0x44;
                CalcInterpolation(startEfusePos, endEfusePos);

                // 5G band 3
                // ant a
                startEfusePos = 0x26;
                endEfusePos = 0x29;
                CalcInterpolation(startEfusePos, endEfusePos);
                startEfusePos = 0x29;
                endEfusePos = 0x2B;
                CalcInterpolation(startEfusePos, endEfusePos);
                // ant b
                startEfusePos = 0x50;
                endEfusePos = 0x53;
                CalcInterpolation(startEfusePos, endEfusePos);
                startEfusePos = 0x53;
                endEfusePos = 0x55;
                CalcInterpolation(startEfusePos, endEfusePos);

                // 5G band 4
                // ant a
                startEfusePos = 0x2C;
                endEfusePos = 0x2F;
                CalcInterpolation(startEfusePos, endEfusePos);
                // ant b
                startEfusePos = 0x56;
                endEfusePos = 0x59;
                CalcInterpolation(startEfusePos, endEfusePos);

                return TestResult.TEST_SUCCESS;
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message + "\r\n");
                mainForm.PCMessageWrite(ex.Message + "\r\n", true);
            }

            return TestResult.TEST_FAILURE;
        }

        public static TestResult WriteTherValue(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".Write thermal value...";
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);

                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            if (rtw is RtwTx)
            {
                RtwTx tx = (RtwTx)rtw;
                // get thermal
                int thermal = mainForm.rtwCommand.GetThermalValue(tx.antPath);
                msg = "Thermal value: " + thermal;

                if (thermal == -1)
                {
                    msg = "Get thermal fail!";
                    mainForm.testLog.WriteLine(msg + "\r\n");
                    mainForm.PCMessageWrite(msg + "\r\n", true);
                    return TestResult.TEST_FAILURE;
                }

                int pos;
                // write memory
                if (tx.antPath == ANT_PATH.PATH_A)
                    pos = 0xD0;
                else if (tx.antPath == ANT_PATH.PATH_B)
                    pos = 0xD1;
                else
                {
                    msg = "Write error position of map";
                    mainForm.testLog.WriteLine(msg + "\r\n");
                    mainForm.PCMessageWrite(msg + "\r\n", true);
                    return TestResult.TEST_FAILURE;
                }

                efuseMap.WriteMap(pos, (byte)thermal);
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult WriteMap(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".Write to map...";
                mainForm.testLog.WriteLine(msg + "\r\n");
                mainForm.PCMessageWrite(msg + "\r\n", true);

                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            if (!Directory.Exists(saveMapDir))
                Directory.CreateDirectory(saveMapDir);

            // write all data to local fake map
            efuseMap.Save(saveMapDir + "\\" + mainForm.sn + ".map");

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult TxVerifyAnalyzeByWifib(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg = "";
            TxVerifyPlan tx = (TxVerifyPlan)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".TX_VERIFICATION_11B Frequency:" + tx.freq + " Bandwidth:" + Wifi.bwDic[tx.bw] + " TxMode:" + Wifi.rateIdDic[tx.rateID] + " ANT:" + Wifi.antPathDic[tx.antPath].ToUpper() + " TxPower:" + tx.verifyCriterion.targetPower + "dBm\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
                IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, tx.freq, (int)tx.verifyCriterion.targetPower, 10, cableloss); // VSA

                if (!
#if HW_TX_MODE
                    mainForm.rtwCommand.StartHwTxCommand(tx))
#else
                    mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs))
#endif
                {

                }
                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            // analyze WIFI data

            try
            {
                DateTime startCaptureTime = DateTime.Now;
                double timeDiff = 0;
                if(SysConfig.platform == "ADB")
                    testTimeout = 5000;
                else
                    testTimeout = 20000;

                InitAnalyzeValueFormat(WIFI.B);
                InitAnalyzeValueResult();

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);

                int captureCount = 1;
                while(captureCount > 0 && (timeDiff = DateTime.Now.Subtract(startCaptureTime).TotalMilliseconds) < testTimeout)
                {
                    Thread.Sleep(1000);
                    double mPower = 0.0;
                    double evm = 0.0;
                    double freqErr = 0.0;
                    double flatness = 0.0;
                    double mask = 0.0;

                    try
                    {
                        // Perform data capture
                        IQxel.vsaDataCapture();
                        // Perform analysis
                        IQxel.analyze80211b();
                        evm = Math.Pow(10, IQxel.getScalarMeasurement("evmAll") / 20) * 100;
                        freqErr = IQxel.getScalarMeasurement("clockErr");
                        flatness = IQxel.getVectorMeasurement("loLeakageDb");
                        IQxel.analyzePower();
                        mPower = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm") + cableloss;
                    }
                    catch (Exception ex)
                    {
                        mainForm.testLog.WriteLine(ex.Message + ", retry...");
                        mainForm.PCMessageWrite(ex.Message + ", retry...", true);
                        if (SysConfig.platform != "ADB")
                        {
                            mainForm.rtwCommand.Init(SysConfig.drivFile, SysConfig.drivDir, SysConfig.powerLimit, SysConfig.powerByRate);
                            if (!(mainForm.rtwCommand.StartMp()
                               && mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs)))
                            {

                            }
                        }  
                        continue;
                    }

                    InitAnalyzeValueResult(true);

                    if (mPower < tx.verifyCriterion.powerLower || mPower > tx.verifyCriterion.powerUpper)
                        powerResult = false;
                    if (evm > tx.verifyCriterion.evm)
                        evmResult = false;
                    if (freqErr < tx.verifyCriterion.freqErrLower || freqErr > tx.verifyCriterion.freqErrUpper)
                        freqErrResult = false;
                    if (flatness >= tx.verifyCriterion.flatness)
                        flatnessResult = false;

                    bool result = powerResult && evmResult && freqErrResult && flatnessResult;
                    string strPower = string.Format(powerFormat, "MPower", mPower, tx.verifyCriterion.powerLower, tx.verifyCriterion.powerUpper);
                    string strEvm = string.Format(evmFormat, "EVM", evm, tx.verifyCriterion.evm);
                    string strFreqErr = string.Format(freqErrFormat, "Freq Err", freqErr, tx.verifyCriterion.freqErrLower, tx.verifyCriterion.freqErrUpper);
                    string strFlatness = string.Format(flatnessFormat, "Flatness", flatness, tx.verifyCriterion.flatness);

                    int thermal = mainForm.rtwCommand.GetThermalValue();
                    string strTher = string.Format(therFormat, "Thermal", thermal);

                    int[] txGain = null;
                    switch (tx.antPath)
                    {
                        case ANT_PATH.PATH_A:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_A);
                            break;
                        case ANT_PATH.PATH_B:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_B);
                            break;
                    }

                    string strGain = string.Format(gainFormat, "Gain", txGain != null ? txGain[0] : -1);

                    msg = string.Format(titleFormat, "Gain", "MPower(dBm)", "EVM(%)", "FreqErr(ppm)", "Thermal", "Flatness(dB)");
                    msg += "\r\n" + string.Format(valueFormat, txGain != null ? "0x" + txGain[0].ToString("X2") : "", mPower, evm, freqErr, "0x" + thermal.ToString("X2"), flatness);
                    msg += "\r\n" + string.Format(rangeFormat, "", "(" + tx.verifyCriterion.powerLower + "," + tx.verifyCriterion.powerUpper + ")", "(," + tx.verifyCriterion.evm + ")", "(" + tx.verifyCriterion.freqErrLower + "," + tx.verifyCriterion.freqErrUpper + ")", "", "(," + tx.verifyCriterion.flatness + ")");
                    msg += "\r\n";
                    mainForm.testLog.WriteLine(msg);

                    msg = strPower + "\r\n" + strEvm + "\r\n" + strFreqErr + "\r\n" + strFlatness + "\r\n" + strTher + "\r\n" + strGain + "\r\n";
                    mainForm.PCMessageWrite(msg, true);

                    if (!result)
                    {
                        msg = "";
                        if (!powerResult)
                            msg = "Power Fail";
                        if (!evmResult)
                            msg += ", EVM Fail";
                        if (!freqErrResult)
                            msg += ", Freq Err Fail";
                        if (!flatnessResult)
                            msg += ", Flatness Fail";

                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                    }
                    captureCount--;
                }

                if (timeDiff >= testTimeout)
                {
                    msg = "timeout.";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
#if HW_TX_MODE
                mainForm.rtwCommand.StopHwTxCommand();
#else
                mainForm.rtwCommand.StopSwTxCommand();
#endif
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult TxVerifyAnalyzeByWifiAg(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg = "";
            TxVerifyPlan tx = (TxVerifyPlan)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".TX_VERIFICATION_11AG Frequency:" + tx.freq + " Bandwidth:" + Wifi.bwDic[tx.bw] + " TxMode:" + Wifi.rateIdDic[tx.rateID] + " ANT:" + Wifi.antPathDic[tx.antPath].ToUpper() + " TxPower:" + tx.verifyCriterion.targetPower + "dBm\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
                IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, tx.freq, (int)tx.verifyCriterion.targetPower, 10, cableloss); // VSA

                if (!
#if HW_TX_MODE
                    mainForm.rtwCommand.StartHwTxCommand(tx))
#else
                    mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs))
#endif
                {

                }
                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            // analyze WIFI data

            try
            {
                DateTime startCaptureTime = DateTime.Now;
                double timeDiff = 0;
                if (SysConfig.platform == "ADB")
                    testTimeout = 5000;
                else
                    testTimeout = 20000;

                InitAnalyzeValueFormat(WIFI.AG);
                InitAnalyzeValueResult();

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);

                int captureCount = 1;
                while (captureCount > 0 && (timeDiff = DateTime.Now.Subtract(startCaptureTime).TotalMilliseconds) < testTimeout)
                {
                    Thread.Sleep(1000);

                    double mPower = 0.0;
                    double evm = 0.0;
                    double freqErr = 0.0;
                    double leakage = 0.0;
                    double flatness = 0.0;
                    double mask = 0.0;

                    try
                    {
                        // Perform data capture
                        IQxel.vsaDataCapture();
                        // Perform analysis
                        IQxel.analyze80211ag();
                        evm = IQxel.getScalarMeasurement("evmAll");
                        freqErr = IQxel.getScalarMeasurement("clockErr");
                        leakage = IQxel.getScalarMeasurement("dcLeakageDbc");
                        flatness = IQxel.getSpectrumFlatness11ag();
                        IQxel.analyzePower();
                        mPower = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm") + cableloss;
                    }
                    catch (Exception ex)
                    {
                        mainForm.testLog.WriteLine(ex.Message + ", retry...");
                        mainForm.PCMessageWrite(ex.Message + ", retry...", true);
                        if (SysConfig.platform != "ADB")
                        {
                            mainForm.rtwCommand.Init(SysConfig.drivFile, SysConfig.drivDir, SysConfig.powerLimit, SysConfig.powerByRate);
                            if (!(mainForm.rtwCommand.StartMp()
                               && mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs)))
                            {

                            }
                        }   
                        continue;
                    }

                    InitAnalyzeValueResult(true);

                    if (mPower < tx.verifyCriterion.powerLower || mPower > tx.verifyCriterion.powerUpper)
                        powerResult = false;
                    if (evm > tx.verifyCriterion.evm)
                        evmResult = false;
                    if (freqErr < tx.verifyCriterion.freqErrLower || freqErr > tx.verifyCriterion.freqErrUpper)
                        freqErrResult = false;
                    if (leakage > tx.verifyCriterion.leakage)
                        leakageResult = false;
                    if (flatness > tx.verifyCriterion.flatness)
                        flatnessResult = false;

                    bool result = powerResult && evmResult && freqErrResult && leakageResult && flatnessResult;
                    string strPower = string.Format(powerFormat, "MPower", mPower, tx.verifyCriterion.powerLower, tx.verifyCriterion.powerUpper);
                    string strEvm = string.Format(evmFormat, "EVM", evm, tx.verifyCriterion.evm);
                    string strFreqErr = string.Format(freqErrFormat, "Freq Err", freqErr, tx.verifyCriterion.freqErrLower, tx.verifyCriterion.freqErrUpper);
                    string strLeakage = string.Format(leakageFormat, "LO Leakage", leakage, tx.verifyCriterion.leakage);
                    string strFlatness = string.Format(flatnessFormat, "Flatness", flatness, tx.verifyCriterion.flatness);

                    int thermal = mainForm.rtwCommand.GetThermalValue();
                    string strTher = string.Format(therFormat, "Thermal", thermal);

                    int[] txGain = null;
                    switch (tx.antPath)
                    {
                        case ANT_PATH.PATH_A:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_A);
                            break;
                        case ANT_PATH.PATH_B:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_B);
                            break;
                    }

                    string strGain = string.Format(gainFormat, "Gain", txGain != null ? txGain[0] : -1);

                    msg = string.Format(titleFormat, "Gain", "MPower(dBm)", "EVM(dB)", "FreqErr(ppm)", "Thermal", "Flatness(dB)", "Lo_Leakage(dB)");
                    msg += "\r\n" + string.Format(valueFormat, txGain != null ? "0x" + txGain[0].ToString("X2") : "", mPower, evm, freqErr, "0x" + thermal.ToString("X2"), flatness, leakage);
                    msg += "\r\n" + string.Format(rangeFormat, "", "(" + tx.verifyCriterion.powerLower + "," + tx.verifyCriterion.powerUpper + ")", "(," + tx.verifyCriterion.evm + ")", "(" + tx.verifyCriterion.freqErrLower + "," + tx.verifyCriterion.freqErrUpper + ")", "", "(," + tx.verifyCriterion.flatness + ")", "(," + tx.verifyCriterion.leakage + ")");
                    msg += "\r\n";
                    mainForm.testLog.WriteLine(msg);

                    msg = strPower + "\r\n" + strEvm + "\r\n" + strFreqErr + "\r\n" + strLeakage + "\r\n" + strFlatness + "\r\n" + strTher + "\r\n" + strGain + "\r\n";
                    mainForm.PCMessageWrite(msg, true);

                    if (!result)
                    {
                        msg = "";
                        if (!powerResult)
                            msg = "Power Fail";
                        if (!evmResult)
                            msg += ", EVM Fail";
                        if (!freqErrResult)
                            msg += ", Freq Err Fail";
                        if (!leakageResult)
                            msg += ", LO Leakage Fail";
                        if (!flatnessResult)
                            msg += ", Flatness Fail";

                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                    }
                    captureCount--;
                }

                if (timeDiff >= testTimeout)
                {
                    msg = "timeout.";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
#if HW_TX_MODE
                mainForm.rtwCommand.StopHwTxCommand();
#else
                mainForm.rtwCommand.StopSwTxCommand();
#endif
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult TxVerifyAnalyzeByWifiN(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg = "";
            TxVerifyPlan tx = (TxVerifyPlan)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".TX_VERIFICATION_11N Frequency:" + tx.freq + " Bandwidth:" + Wifi.bwDic[tx.bw] + " TxMode:" + Wifi.rateIdDic[tx.rateID] + " ANT:" + Wifi.antPathDic[tx.antPath].ToUpper() + " TxPower:" + tx.verifyCriterion.targetPower + "dBm\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
                IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, tx.freq, (int)tx.verifyCriterion.targetPower, 10, cableloss); // VSA

                if (!
#if HW_TX_MODE
                    mainForm.rtwCommand.StartHwTxCommand(tx))
#else
                    mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs))
#endif
                {

                }
                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            // analyze WIFI data

            try
            {
                DateTime startCaptureTime = DateTime.Now;
                double timeDiff = 0;
                if (SysConfig.platform == "ADB")
                    testTimeout = 5000;
                else
                    testTimeout = 20000;

                InitAnalyzeValueFormat(WIFI.N);
                InitAnalyzeValueResult();

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);

                int captureCount = 1;
                while (captureCount > 0 && (timeDiff = DateTime.Now.Subtract(startCaptureTime).TotalMilliseconds) < testTimeout)
                {
                    Thread.Sleep(1000);

                    double mPower = 0.0;
                    double evm = 0.0;
                    double freqErr = 0.0;
                    double leakage = 0.0;
                    double flatness = 0.0;
                    double mask = 0.0;

                    try
                    {
                        // Perform data capture
                        IQxel.vsaDataCapture();
                        // Perform analysis
                        IQxel.analyze80211n();
                        evm = IQxel.getScalarMeasurement("evmAvgAll");
                        freqErr = IQxel.getScalarMeasurement("symClockErrorPpm");
                        leakage = IQxel.getScalarMeasurement("dcLeakageDbc");
                        switch (tx.bw)
                        {
                            case BW.B_20MHZ:
                                flatness = IQxel.getSpectrumFlatness11n(20);
                                break;
                            case BW.B_40MHZ:
                            default:
                                flatness = IQxel.getSpectrumFlatness11n(40);
                                break;
                        }
                        IQxel.analyzePower();
                        mPower = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm") + cableloss;
                    }
                    catch (Exception ex)
                    {
                        mainForm.testLog.WriteLine(ex.Message + ", retry...");
                        mainForm.PCMessageWrite(ex.Message + ", retry...", true);
                        if(SysConfig.platform != "ADB")
                        {
                            mainForm.rtwCommand.Init(SysConfig.drivFile, SysConfig.drivDir, SysConfig.powerLimit, SysConfig.powerByRate);
                            if (!(mainForm.rtwCommand.StartMp()
                               && mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs)))
                            {

                            }
                        }
                        continue;
                    }

                    InitAnalyzeValueResult(true);

                    if (mPower < tx.verifyCriterion.powerLower || mPower > tx.verifyCriterion.powerUpper)
                        powerResult = false;
                    if (evm > tx.verifyCriterion.evm)
                        evmResult = false;
                    if (freqErr < tx.verifyCriterion.freqErrLower || freqErr > tx.verifyCriterion.freqErrUpper)
                        freqErrResult = false;
                    if (leakage > tx.verifyCriterion.leakage)
                        leakageResult = false;
                    if (flatness > tx.verifyCriterion.flatness)
                        flatnessResult = false;

                    bool result = powerResult && evmResult && freqErrResult && leakageResult && flatnessResult;
                    string strPower = string.Format(powerFormat, "MPower", mPower, tx.verifyCriterion.powerLower, tx.verifyCriterion.powerUpper);
                    string strEvm = string.Format(evmFormat, "EVM", evm, tx.verifyCriterion.evm);
                    string strFreqErr = string.Format(freqErrFormat, "Freq Err", freqErr, tx.verifyCriterion.freqErrLower, tx.verifyCriterion.freqErrUpper);
                    string strLeakage = string.Format(leakageFormat, "LO Leakage", leakage, tx.verifyCriterion.leakage);
                    string strFlatness = string.Format(flatnessFormat, "Flatness", flatness, tx.verifyCriterion.flatness);

                    int thermal = mainForm.rtwCommand.GetThermalValue();
                    string strTher = string.Format(therFormat, "Thermal", thermal);

                    int[] txGain = null;
                    switch (tx.antPath)
                    {
                        case ANT_PATH.PATH_A:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_A);
                            break;
                        case ANT_PATH.PATH_B:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_B);
                            break;
                    }

                    string strGain = string.Format(gainFormat, "Gain", txGain != null ? txGain[0] : -1);

                    msg = string.Format(titleFormat, "Gain", "MPower(dBm)", "EVM(dB)", "FreqErr(ppm)", "Thermal", "Flatness(dB)", "Lo_Leakage(dB)");
                    msg += "\r\n" + string.Format(valueFormat, txGain != null ? "0x" + txGain[0].ToString("X2") : "", mPower, evm, freqErr, "0x" + thermal.ToString("X2"), flatness, leakage);
                    msg += "\r\n" + string.Format(rangeFormat, "", "(" + tx.verifyCriterion.powerLower + "," + tx.verifyCriterion.powerUpper + ")", "(," + tx.verifyCriterion.evm + ")", "(" + tx.verifyCriterion.freqErrLower + "," + tx.verifyCriterion.freqErrUpper + ")", "", "(," + tx.verifyCriterion.flatness + ")", "(," + tx.verifyCriterion.leakage + ")");
                    msg += "\r\n";
                    mainForm.testLog.WriteLine(msg);

                    msg = strPower + "\r\n" + strEvm + "\r\n" + strFreqErr + "\r\n" + strLeakage + "\r\n" + strFlatness + "\r\n" + strTher + "\r\n" + strGain + "\r\n";
                    mainForm.PCMessageWrite(msg, true);

                    if (!result)
                    {
                        msg = "";
                        if (!powerResult)
                            msg = "Power Fail";
                        if (!evmResult)
                            msg += ", EVM Fail";
                        if (!freqErrResult)
                            msg += ", Freq Err Fail";
                        if (!leakageResult)
                            msg += ", LO Leakage Fail";
                        if (!flatnessResult)
                            msg += ", Flatness Fail";

                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                    }
                    captureCount--;
                }

                if (timeDiff >= testTimeout)
                {
                    msg = "timeout.";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
#if HW_TX_MODE
                mainForm.rtwCommand.StopHwTxCommand();
#else
                mainForm.rtwCommand.StopSwTxCommand();
#endif
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult TxVerifyAnalyzeByWifiAc(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg = "";
            TxVerifyPlan tx = (TxVerifyPlan)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".TX_VERIFICATION_11AC Frequency:" + tx.freq + " Bandwidth:" + Wifi.bwDic[tx.bw] + " TxMode:" + Wifi.rateIdDic[tx.rateID] + " ANT:" + Wifi.antPathDic[tx.antPath].ToUpper() + " TxPower:" + tx.verifyCriterion.targetPower + "dBm\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
                IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, tx.freq, (int)tx.verifyCriterion.targetPower, 10, cableloss); // VSA

                if (!
#if HW_TX_MODE
                    mainForm.rtwCommand.StartHwTxCommand(tx))
#else
                    mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs))
#endif
                {

                }
                return TestResult.TEST_SUCCESS;
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            // analyze WIFI data

            try
            {
                DateTime startCaptureTime = DateTime.Now;
                double timeDiff = 0;
                if (SysConfig.platform == "ADB")
                    testTimeout = 5000;
                else
                    testTimeout = 20000;

                InitAnalyzeValueFormat(WIFI.AC);
                InitAnalyzeValueResult();

                double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);

                int captureCount = 1;
                while(captureCount > 0 && (timeDiff = DateTime.Now.Subtract(startCaptureTime).TotalMilliseconds) < testTimeout)
                {
                    Thread.Sleep(1000);

                    double mPower = 0.0;
                    double evm = 0.0;
                    double freqErr = 0.0;
                    double leakage = 0.0;
                    double flatness = 0.0;
                    double mask = 0.0;

                    try
                    {
                        // Perform data capture
                        IQxel.vsaDataCapture();
                        // Perform analysis
                        IQxel.analyze80211ac();
                        evm = IQxel.getScalarMeasurement("evmAvgAll");
                        freqErr = IQxel.getScalarMeasurement("symClockErrorPpm");
                        leakage = IQxel.getScalarMeasurement("dcLeakageDbc");
                        switch (tx.bw)
                        {
                            case BW.B_20MHZ:
                                flatness = IQxel.getSpectrumFlatness11ac(20);
                                break;
                            case BW.B_40MHZ:
                                flatness = IQxel.getSpectrumFlatness11ac(40);
                                break;
                            case BW.B_80MHZ:
                            default:
                                flatness = IQxel.getSpectrumFlatness11ac(80);
                                break;
                        }
                        IQxel.analyzePower();
                        mPower = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm") + cableloss;
                    }
                    catch (Exception ex)
                    {
                        mainForm.testLog.WriteLine(ex.Message + ", retry...");
                        mainForm.PCMessageWrite(ex.Message + ", retry...", true);
                        if (SysConfig.platform != "ADB")
                        {
                            mainForm.rtwCommand.Init(SysConfig.drivFile, SysConfig.drivDir, SysConfig.powerLimit, SysConfig.powerByRate);
                            if (!(mainForm.rtwCommand.StartMp()
                               && mainForm.rtwCommand.StartSwTxCommand(tx, SysConfig.powerTracking, SysConfig.delayForCaptureMs)))
                            {

                            }
                        }
                        continue;
                    }

                    InitAnalyzeValueResult(true);

                    if (mPower < tx.verifyCriterion.powerLower || mPower > tx.verifyCriterion.powerUpper)
                        powerResult = false;
                    if (evm > tx.verifyCriterion.evm)
                        evmResult = false;
                    if (freqErr < tx.verifyCriterion.freqErrLower || freqErr > tx.verifyCriterion.freqErrUpper)
                        freqErrResult = false;
                    if (leakage > tx.verifyCriterion.leakage)
                        leakageResult = false;
                    if (flatness > tx.verifyCriterion.flatness)
                        flatnessResult = false;

                    bool result = powerResult && evmResult && freqErrResult && leakageResult && flatnessResult;
                    string strPower = string.Format(powerFormat, "MPower", mPower, tx.verifyCriterion.powerLower, tx.verifyCriterion.powerUpper);
                    string strEvm = string.Format(evmFormat, "EVM", evm, tx.verifyCriterion.evm);
                    string strFreqErr = string.Format(freqErrFormat, "Freq Err", freqErr, tx.verifyCriterion.freqErrLower, tx.verifyCriterion.freqErrUpper);
                    string strLeakage = string.Format(leakageFormat, "LO Leakage", leakage, tx.verifyCriterion.leakage);
                    string strFlatness = string.Format(flatnessFormat, "Flatness", flatness, tx.verifyCriterion.flatness);

                    int thermal = mainForm.rtwCommand.GetThermalValue();
                    string strTher = string.Format(therFormat, "Thermal", thermal);

                    int[] txGain = null;
                    switch (tx.antPath)
                    {
                        case ANT_PATH.PATH_A:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_A);
                            break;
                        case ANT_PATH.PATH_B:
                            txGain = mainForm.rtwCommand.GetTxPower(ANT_PATH.PATH_B);
                            break;
                    }

                    string strGain = string.Format(gainFormat, "Gain", txGain != null ? txGain[0] : -1);

                    msg = string.Format(titleFormat, "Gain", "MPower(dBm)", "EVM(dB)", "FreqErr(ppm)", "Thermal", "Flatness(dB)", "Lo_Leakage(dB)");
                    msg += "\r\n" + string.Format(valueFormat, txGain != null ? "0x" + txGain[0].ToString("X2") : "", mPower, evm, freqErr, "0x" + thermal.ToString("X2"), flatness, leakage);
                    msg += "\r\n" + string.Format(rangeFormat, "", "(" + tx.verifyCriterion.powerLower + "," + tx.verifyCriterion.powerUpper + ")", "(," + tx.verifyCriterion.evm + ")", "(" + tx.verifyCriterion.freqErrLower + "," + tx.verifyCriterion.freqErrUpper + ")", "", "(," + tx.verifyCriterion.flatness + ")", "(," + tx.verifyCriterion.leakage + ")");
                    msg += "\r\n";
                    mainForm.testLog.WriteLine(msg);

                    msg = strPower + "\r\n" + strEvm + "\r\n" + strFreqErr + "\r\n" + strLeakage + "\r\n" + strFlatness + "\r\n" + strTher + "\r\n" + strGain + "\r\n";
                    mainForm.PCMessageWrite(msg, true);

                    if (!result)
                    {
                        msg = "";
                        if (!powerResult)
                            msg = "Power Fail";
                        if (!evmResult)
                            msg += ", EVM Fail";
                        if (!freqErrResult)
                            msg += ", Freq Err Fail";
                        if (!leakageResult)
                            msg += ", LO Leakage Fail";
                        if (!flatnessResult)
                            msg += ", Flatness Fail";

                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                    }
                    captureCount--;
                }

                if (timeDiff >= testTimeout)
                {
                    msg = "timeout.";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    return TestResult.TEST_FAILURE;
                }
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
#if HW_TX_MODE
                mainForm.rtwCommand.StopHwTxCommand();
#else
                mainForm.rtwCommand.StopSwTxCommand();
#endif
            }

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult RxVerifyMP(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;
            RxVerifyPlan rx = (RxVerifyPlan)rtw;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".RX_VERIFICATION Frequency:" + rx.freq + " Bandwidth:" + Wifi.bwDic[rx.bw] + " RxMode:" + Wifi.rateIdDic[rx.rateID] + " ANT:" + Wifi.antPathDic[rx.antPath].ToUpper() + " RxPower:" + rx.verifyCriterion.rxPower + "dB\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);

                try
                {
                    double vsgPowerLevel = rx.verifyCriterion.rxPower;

                    double cableloss = SysConfig.GetCableLoss(rx.freq, (int)rx.antPath);
                    IQxel.setIQxelVsg(port, rx.freq, cableloss + vsgPowerLevel); // VSG

                    string strVsgFile = rx.streamFile; //need to modify for various rates

                    IQxel.setVsgModulation(strVsgFile);
                    IQxel.enableVsgRf(1);
                    //send 1 dummy frame
                    IQxel.setFrameCount(1);

                    if (mainForm.rtwCommand.StartMp()
                     && mainForm.rtwCommand.StartRxCommand(rx))
                    {
                        //Thread.Sleep(1000); //must delay 1 sec before IQ sending packet frames  
                        return TestResult.TEST_SUCCESS;
                    }

                    msg = "Send command fail!";
                    mainForm.testLog.WriteLine(msg);
                    mainForm.PCMessageWrite(msg, true);
                    mainForm.rtwCommand.StopMp();
                    return TestResult.TEST_FAILURE;
                }
                catch (Exception ex)
                {
                    mainForm.rtwCommand.StopMp();
                    mainForm.testLog.WriteLine(ex.Message);
                    mainForm.PCMessageWrite(ex.Message, true);
                    return TestResult.TEST_FAILURE;
                }
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            int retryCount = 0;

            try
            {
                while (true)
                {
                    IQxel.setFrameCount(1000); //start IQxel TX to send 1000 packets
                    IQxel.waitTxDone(1500);

                    //get rx info from DUT
                    int okCount = mainForm.rtwCommand.GetRxPacketCount();
                    double per = 1000 - okCount;
                    per = (per * 100) / 1000;

                    string perFormat = "   {0,-15}:{1,10:0.00} %  (,{2})";
                    string strPer = string.Format(perFormat, "Per", per, rx.verifyCriterion.per);
                    mainForm.testLog.WriteLine(strPer);
                    mainForm.PCMessageWrite(strPer, true);

                    if (per > rx.verifyCriterion.per)
                    {
                        retryCount++;
                        if (retryCount < 3)
                        {
                            if (mainForm.rtwCommand.ResetRxStat())
                                continue;
                            msg = "Send command fail!";
                            mainForm.testLog.WriteLine(msg);
                            mainForm.PCMessageWrite(msg, true);
                            return TestResult.TEST_FAILURE;
                        }

                        msg = "PER FAIL";
                        mainForm.testLog.WriteLine(msg);
                        mainForm.PCMessageWrite(msg, true);
                        return TestResult.TEST_FAILURE;
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
                mainForm.rtwCommand.StopMp();
            }

            mainForm.testLog.WriteLine("");
            mainForm.PCMessageWrite("", true);

            return TestResult.TEST_SUCCESS;
        }

        public static TestResult RxVerify(TestStatus testStatus, Rtw rtw)
        {
            Form1 mainForm = Form1.GetMainForm();

            string msg;
            RxVerifyPlan rx = (RxVerifyPlan)rtw;

            double cableloss;

            if (testStatus == TestStatus.TEST_INIT)
            {
                msg = (mainForm.testNum + 1) + ".RX_VERIFICATION Frequency:" + rx.freq + " Bandwidth:" + Wifi.bwDic[rx.bw] + " RxMode:" + Wifi.rateIdDic[rx.rateID] + " ANT:" + Wifi.antPathDic[rx.antPath].ToUpper() + "\r\n";
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);

                try
                {
                    // preliminary value
                    double vsgPowerLevel = rx.verifyCriterion.rxPower;

                    cableloss = SysConfig.GetCableLoss(rx.freq, (int)rx.antPath);
                    IQxel.setIQxelVsg(port, rx.freq, cableloss + vsgPowerLevel); // VSG

                    string strVsgFile = rx.streamFile; //need to modify for various rates

                    IQxel.setVsgModulation(strVsgFile);
                    IQxel.enableVsgRf(1);
                    //send 1 dummy frame
                    IQxel.setFrameCount(1);

                    if (!(mainForm.rtwCommand.StartMp()
                       && mainForm.rtwCommand.StartRxCommand(rx)))
                    {
                       
                    }
                    return TestResult.TEST_SUCCESS;
                }
                catch (Exception ex)
                {
                    mainForm.rtwCommand.StopMp();
                    mainForm.testLog.WriteLine(ex.Message);
                    mainForm.PCMessageWrite(ex.Message, true);
                    return TestResult.TEST_FAILURE;
                }
            }

            if (testStatus == TestStatus.TEST_END)
            {
                return TestResult.TEST_SUCCESS;
            }

            bool isReinit = false;
            bool isCloseToRxPower = false;
            bool isLowestRxPower = false;
            double targetRxPower;
            double rxPower = targetRxPower = rx.verifyCriterion.rxPower;
            cableloss = SysConfig.GetCableLoss(rx.freq, (int)rx.antPath);

            try
            {
                while (true)
                {
                    Thread.Sleep(1000); //must delay before IQ sending packet frames
                    IQxel.setFrameCount(1000); //start IQxel TX to send 1000 packets
                    switch (rx.rateID)
                    {
                        case RATE_ID.R_1M:
                            IQxel.waitTxDone(12000);
                            break;
                        case RATE_ID.R_2M:
                            IQxel.waitTxDone(6000);
                            break;
                        case RATE_ID.R_6M:
                        case RATE_ID.R_5_5M:
                            IQxel.waitTxDone(4000);
                            break;
                        case RATE_ID.VHT1MCS0:
                            IQxel.waitTxDone(3000);
                            break;
                        default:
                            IQxel.waitTxDone(1500);
                            break;
                    }

                    //get rx info from DUT
                    int okCount = mainForm.rtwCommand.GetRxPacketCount();
                    if(okCount >= 0)
                    {
                        double per = 1000 - okCount;
                        per = (per * 100) / 1000;

                        string perFormat = "   {0,-15}:{1,10:0.00} %  (,{2})";
                        string strPer = string.Format(perFormat, "Per", per, rxPower);
                        mainForm.testLog.WriteLine(strPer);
                        mainForm.PCMessageWrite(strPer, true);

                        if (!isReinit && rxPower >= targetRxPower + 20 && per > 99) // if rxPower is in a distance with targetRxPower, reinit the module
                        {
                            mainForm.rtwCommand.Init(SysConfig.drivFile, SysConfig.drivDir, SysConfig.powerLimit, SysConfig.powerByRate);
                            if (!(mainForm.rtwCommand.StartMp()
                               && mainForm.rtwCommand.StartRxCommand(rx)))
                            {

                            }
                            isReinit = true;
                            continue;
                        }
                        else
                        {
                            isReinit = false;
                            if (per > rx.verifyCriterion.per)
                            {
                                if (!isCloseToRxPower)
                                {
                                    if (per >= 90)
                                        rxPower += 10;
                                    else if (per >= 50)
                                        rxPower += 5;
                                    else if (per >= 30)
                                        rxPower += 2;
                                    else if (per >= 20)
                                        rxPower += 1;
                                    else if (per >= 10)
                                        rxPower += 0.5;
                                    else
                                        rxPower += 0.5;
                                }
                                else
                                {
                                    rxPower += 0.5;
                                    isLowestRxPower = true;
                                }
                            }
                            else
                            {
                                if (!isLowestRxPower)
                                {
                                    if (!isCloseToRxPower)
                                    {
                                        if (per <= 10)
                                        {
                                            rxPower -= 0.5;
                                            isCloseToRxPower = true;
                                        }
                                    }
                                    else
                                        rxPower -= 0.5;
                                }
                                else
                                {
                                    msg = "Channel:" + Wifi.ChannelMapping(rx.freq) + " Frequency:" + rx.freq + " Per:" + per + "%" + " RxPower:" + rxPower;
                                    mainForm.testLog.WriteLine(msg);
                                    //EVTReport.AddToRxTestList(rx.rateID, rx.bw, rx.antPath, Wifi.ChannelMapping(rx.freq), rx.freq, rxPower, per);
                                    break;
                                }
                            }

                            // if rxPower is too high, stop testing
                            if (rxPower > -40)
                            {
                                //msg = "Channel:" + Wifi.ChannelMapping(rx.freq) + " Frequency:" + rx.freq + " Per:100% RxPower:0\r\nFinal RxPower:0";
                                //mainForm.testLog.WriteLine(msg);
                                msg = "RxPower is too high!!";
                                mainForm.testLog.WriteLine(msg);
                                mainForm.PCMessageWrite(msg, true);
                                break;
                            }
                            IQxel.setIQxelVsg(port, rx.freq, cableloss + rxPower); // VSG
                        }  
                    }
                    mainForm.rtwCommand.ResetRxStat();
                }

                msg = "Final RxPower:" + rxPower;
                mainForm.testLog.WriteLine(msg);
                mainForm.PCMessageWrite(msg, true);
            }
            catch (Exception ex)
            {
                mainForm.testLog.WriteLine(ex.Message);
                mainForm.PCMessageWrite(ex.Message, true);
                return TestResult.TEST_FAILURE;
            }
            finally
            {
                mainForm.rtwCommand.StopMp();
            }

            mainForm.testLog.WriteLine("");
            mainForm.PCMessageWrite("", true);

            return TestResult.TEST_SUCCESS;
        }




































        //        public static TestResult TxVerify(TestStatus testStatus, Rtw rtw)
        //        {
        //            Form1 mainForm = Form1.GetMainForm();

        //            string msg;
        //            RtwTx tx = (RtwTx)rtw;

        //            if (testStatus == TestStatus.TEST_INIT)
        //            {
        //                msg = (mainForm.testNum + 1) + ".TX_VERIFIVATION Frequency: " + tx.freq + " Bandwidth:" + bwDic[tx.bw] + " Tx Mode:" + rateIdDic[tx.rateID] + " ANT:" + antPathDic[tx.antPath].ToUpper() + " Tx Power: " + TargetPower + "dBm\r\n";
        //                mainForm.testLog.WriteLine(msg + "\r\n");
        //                mainForm.PCMessageWrite(msg + "\r\n", true);

        //                if (StartMp()
        //                 && StartHwTxCommand(tx))
        //                {
        //                    Thread.Sleep(1000);
        //                    return TestResult.TEST_SUCCESS;
        //                }

        //                msg = "Send command fail!";
        //#if DEBUG
        //                log.Warn(msg);
        //#endif
        //                mainForm.testLog.WriteLine(msg);
        //                mainForm.PCMessageWrite(msg, true);
        //                StopHwTxCommand();
        //                return TestResult.TEST_FAILURE;
        //            }

        //            if (testStatus == TestStatus.TEST_END)
        //            {
        //                return TestResult.TEST_SUCCESS;
        //            }

        //            // analyze WIFI data

        //            try
        //            {
        //                DateTime startCaptureTime = DateTime.Now;
        //                double timeDiff = 0;
        //                testTimeout = 10000;

        //                int retryCount = 3;

        //                int evmLimit = GetStandardEvmLimit(tx.rateID);

        //                string strPowerResult;
        //                string strEvmResult;
        //                string strFreqErrResult;
        //                string strLeakageResult;
        //                string strMaskResult;
        //                string powerFormat;
        //                string evmFormat;
        //                string freqErrFormat;
        //                string leakageFormat;
        //                string maskFormat;

        //                double cableloss = mainForm.GetCableLoss(tx.freq, 0);
        //                byte curTxPower = defaultTxPower;

        //                while ((timeDiff = DateTime.Now.Subtract(startCaptureTime).TotalMilliseconds) < testTimeout || retryCount >= 0)
        //                {
        //                    Thread.Sleep(100);

        //                    double mPower = 0.0;
        //                    double evm = 0.0;
        //                    double freqErr = 0.0;
        //                    double leakage = 0.0;
        //                    double mask = 0.0;

        //                    try
        //                    {
        //                        // Perform data capture
        //                        IQxel.vsaDataCapture();

        //                        // Perform analysis
        //                        if (tx.rateID == RATE_ID.R_11M)
        //                        {
        //                            IQxel.analyze80211b();
        //                            mPower = IQxel.getScalarMeasurement("rmsPower") + cableloss;
        //                            evm = Math.Pow(10, IQxel.getScalarMeasurement("evmAll") / 20) * 100;
        //                            freqErr = IQxel.getScalarMeasurement("clockErr");
        //                            //leakage = IQxel.getScalarMeasurement("loLeakageDb");
        //                            powerFormat = "{0,10}:{1,10:#.00} dBm {2,15}";
        //                            evmFormat = "{0,10}:{1,10:#.00} %   {2,15}";
        //                            freqErrFormat = "{0,10}:{1,10:#.00} ppm {2,15}";
        //                            //leakageFormat = "{0,10}:{1,10:#.00} dB  {2,15}";
        //                        }
        //                        else if (tx.rateID == RATE_ID.R_54M)
        //                        {
        //                            IQxel.analyze80211ag();
        //                            mPower = IQxel.getScalarMeasurement("rmsPower") + cableloss;
        //                            evm = IQxel.getScalarMeasurement("evmAll");
        //                            freqErr = IQxel.getScalarMeasurement("clockErr");
        //                            leakage = IQxel.getScalarMeasurement("dcLeakageDbc");
        //                            powerFormat = "{0,10}:{1,10:#.00} dBm {2,15}";
        //                            evmFormat = "{0,10}:{1,10:#.00} dB  {2,15}";
        //                            freqErrFormat = "{0,10}:{1,10:#.00} ppm {2,15}";
        //                            leakageFormat = "{0,10}:{1,10:#.00} dB  {2,15}";
        //                        }
        //                        else if (tx.rateID == RATE_ID.HTMCS7)
        //                        {
        //                            IQxel.analyze80211n();
        //                            mPower = IQxel.getScalarMeasurement("rmsPower") + cableloss;
        //                            evm = IQxel.getScalarMeasurement("evmAvgAll");
        //                            freqErr = IQxel.getScalarMeasurement("symClockErrorPpm");
        //                            leakage = IQxel.getScalarMeasurement("dcLeakageDbc");
        //                            powerFormat = "{0,10}:{1,10:#.00} dBm {2,15}";
        //                            evmFormat = "{0,10}:{1,10:#.00} dB  {2,15}";
        //                            freqErrFormat = "{0,10}:{1,10:#.00} ppm {2,15}";
        //                            leakageFormat = "{0,10}:{1,10:#.00} dB  {2,15}";
        //                        }
        //                        else
        //                        {
        //                            msg = "Not support this rate ID for testing tx calibration!";
        //#if DEBUG
        //                            log.Warn(msg);
        //#endif
        //                            mainForm.testLog.WriteLine(msg);
        //                            mainForm.PCMessageWrite(msg, true);
        //                            return TestResult.TEST_FAILURE;
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //#if DEBUG
        //                        log.Warn(ex.Message);
        //#endif
        //                        mainForm.testLog.WriteLine(ex.Message + ", retry...");
        //                        mainForm.PCMessageWrite(ex.Message + ", retry...", true);

        //                        if (StartMp()
        //                         && StartHwTxCommand(tx))
        //                        {
        //                            Thread.Sleep(1000);
        //                            return TestResult.TEST_SUCCESS;
        //                        }

        //                        msg = "Send command fail!";
        //#if DEBUG
        //                        log.Warn(msg);
        //#endif
        //                        mainForm.testLog.WriteLine(msg);
        //                        mainForm.PCMessageWrite(msg, true);
        //                        StopHwTxCommand();
        //                        return TestResult.TEST_FAILURE;
        //                    }

        //                    bool powerResult = true;
        //                    bool evmResult = true;

        //                    if (mPower < TxVerifyPowerLowerLimit || mPower > TxVerifyPowerUpperLimit)
        //                    {
        //                        powerResult = false;
        //                        strPowerResult = "--> FAIL";
        //                    }
        //                    else
        //                    {
        //                        strPowerResult = "--> PASS";
        //                    }


        //                    if (evmLimit < evm)
        //                    {
        //                        evmResult = false;
        //                        strEvmResult = "--> FAIL";
        //                    }
        //                    else
        //                    {
        //                        strEvmResult = "--> PASS";
        //                    }

        //                    if (curTxPower > TxPowerUpperLimit || curTxPower < TxPowerLowerLimit)
        //                    {
        //                        msg = "Tx index power is over range!";
        //                        mainForm.testLog.WriteLine(msg);
        //                        mainForm.PCMessageWrite(msg, true);
        //                        return TestResult.TEST_FAILURE;
        //                    }

        //                    string strPower = string.Format(powerFormat, "MPower", mPower, strPowerResult);
        //                    string strEvm = string.Format(evmFormat, "EVM", evm, strEvmResult);

        //                    mainForm.testLog.WriteLine(strPower);
        //                    mainForm.testLog.WriteLine(strEvm);
        //                    mainForm.PCMessageWrite(strPower, true);
        //                    mainForm.PCMessageWrite(strEvm, true);

        //                    if (powerResult && evmResult)
        //                        break;

        //                    msg = "\r\nRetry...\r\n";
        //                    mainForm.testLog.WriteLine(msg);
        //                    mainForm.PCMessageWrite(msg, true);
        //                    SendTxPowerCommand(tx.antPath, curTxPower);
        //                }

        //                if (timeDiff >= testTimeout)
        //                {
        //                    msg = "timeout.";
        //                    mainForm.testLog.WriteLine(msg);
        //                    mainForm.PCMessageWrite(msg, true);
        //                    return TestResult.TEST_FAILURE;
        //                }

        //                byte txPowerOk = curTxPower;
        //                int writeEfusePos = EfuseMap.GetPosition(tx.rateID, tx.bw, tx.antPath, tx.freq);

        //                // write to corresponding position of memory
        //                efuseMap.WriteMap(writeEfusePos, txPowerOk);

        //                msg = "\r\nTxPower: 0x" + txPowerOk.ToString("X2") + ", Efuse: 0x" + writeEfusePos.ToString("X2") + "\r\n";
        //                mainForm.testLog.WriteLine(msg);
        //                mainForm.PCMessageWrite(msg, true);
        //            }
        //            catch (Exception ex)
        //            {
        //#if DEBUG
        //                log.Error(ex.Message);
        //#endif
        //                mainForm.testLog.WriteLine(ex.Message);
        //                mainForm.PCMessageWrite(ex.Message, true);
        //                return TestResult.TEST_FAILURE;
        //            }
        //            finally
        //            {
        //                StopHwTxCommand();
        //            }


        //            return TestResult.TEST_SUCCESS;
        //        }
    }
}
