using IQAPI_C_shap;
using RTKModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", ConfigFileExtension = "config", Watch = true)]

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public partial class Form1 : Form
    {
        public delegate void BtnTextUpdateHandler(string text);
        public delegate void RichTextbox1AppendMsgHandler(string msg, bool newLine);
        public delegate void RichTextbox2AppendMsgHandler(string msg, bool newLine);
        public delegate void TimerHandler(bool enable);

        //public readonly log4net.ILog log = LogManager.GetLogger(typeof(Form1));
        public readonly string adbPath = Application.StartupPath + "\\platform-tools\\adb.exe";
        public readonly string testLogDirPath = Application.StartupPath + "\\Test_Log";
        public readonly string adbLogDirPath = Application.StartupPath + "\\Adb_Log";

        public const string dateFormat = "yyyyMMdd";
        public const string datetimeFormat = "yyyyMMdd_HHmmss";

        public RTKModule.ILog testLog;
        public RTKModule.ILog adbLog;
        public RTKModule.ILog adbDebugLog;

        public string sn;

        public RtwCommand rtwCommand;
        public RtwProxyProcessor rtwProxyProcessor;
        public Adb adb;
        public ComPort comPort;
         
        public Semaphore bgWorker1Sem = new Semaphore(0, 1);
        public TestItems testItems;
        public TestResult finalTestResult = TestResult.TEST_FAILURE;
        public TestStatus testState = TestStatus.TEST_IDLE;
        public DateTime dtStartTest;
        public int totalTestNum;
        public int testNum;

        public int timeCounter;

        private static Form1 mainForm;
        private readonly string swVersion;

        private bool startTesting = false;

        public static Form1 GetMainForm()
        {
            return mainForm;
        }

        public Form1()
        {
            InitializeComponent();

            swVersion = typeof(Form1).Assembly.FullName.Split(',')[0].Trim();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mainForm = this;
            mainForm.Text = swVersion;

            try
            {
                SysConfig.LoadConfig();
                SysConfig.LoadWifiSet();
                SysConfig.LoadCableLoss();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            // function test added by user
            testItems = TestItems.GetInstance();
            testItems.AddFunctions();
            totalTestNum = testItems.rtwTestFunctionList.Count;

            // background task
            backgroundWorker1.WorkerReportsProgress = true; // enable progress handler
            backgroundWorker1.WorkerSupportsCancellation = true; // enable cancellation handler
            backgroundWorker1.RunWorkerAsync(); // test

            textBox1.Text = DateTime.Now.ToString(dateFormat);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                backgroundWorker1.CancelAsync();
                try
                {
                    bgWorker1Sem.Release(1);
                }
                catch (Exception) { }

                while (backgroundWorker1.IsBusy)
                    Application.DoEvents();
            }
            catch (Exception) { }

            InterfaceClose();

            if (IQxel.bIsConnectTester)
            {
                IQxel.releaseControl();
                IQxel.closeTester();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!startTesting)
            {
                sn = textBox1.Text;
                try
                {
                    if (!File.Exists(adbPath))
                    {
                        MessageBox.Show("There is no adb tool in path:" + adbPath);
                        return;
                    }

                    if (CheckSn(sn))
                    {
                        richTextBox1.Clear();
                        richTextBox2.Clear();

                        dtStartTest = DateTime.Now;

                        // init log
                        testLog = new Log();
                        adbLog = new Log();

                        testLog.WriteLine("Board S/N: " + sn);
                        testLog.WriteLine("Start Test Time: " + dtStartTest.ToString("yyyy.MM.dd.HH.mm.ss"));
                        testLog.WriteLine("=========== START TEST ===========");

                        if (SysConfig.platform == "1319" && SysConfig.connectionInterface == "ADB")
                        {
                            adb = new Adb(adbPath);
                            adb.ReceiveAdbMessageEvent += new Adb.ReceiveAdbMessageEventHandler(ProcessReceive);
                            adb.ExitAdbEvent += new Adb.ExitAdbEventHandler(ProcessExit);
                            adb.OpenAdbShell();
                        }
                        else if ((SysConfig.platform == "1319" || SysConfig.platform == "Bananapi" || SysConfig.platform == "Raspi")
                                && SysConfig.connectionInterface.StartsWith("COM"))
                        {
                            int baudRate = 115200;
                            if (SysConfig.platform == "1319")
                                baudRate = 460800;
                            comPort = new ComPort(SysConfig.connectionInterface, baudRate, 8, StopBits.One, Parity.None);
                            comPort.ReceiveSerialMessageEvent += ProcessSerialReceive;
                            comPort.Open();
                        }
                        else
                        {
                            MessageBox.Show("Pls check the platform or interface connection!");
                            return;
                        }

                        RtwLogHandledInterceptor rtwLogHandledInterceptor = new RtwLogHandledInterceptor(adbLog);
                        // get proxy processor instance
                        if (SysConfig.connectionInterface == "ADB")
                            rtwProxyProcessor = RtwProxyCreator.CreatProxy(adb, rtwLogHandledInterceptor);
                        else if (SysConfig.connectionInterface.StartsWith("COM"))
                            rtwProxyProcessor = RtwProxyCreator.CreatProxy(comPort, rtwLogHandledInterceptor);

                        // set proxy processor
                        rtwCommand = new RtwCommand(rtwProxyProcessor);

                        // reset test num
                        testNum = 0;

                        if (!backgroundWorker1.IsBusy)
                        {
                            backgroundWorker1.RunWorkerAsync();
                        }

                        try
                        {
                            // start test
                            bgWorker1Sem.Release(1);
                        }
                        catch (Exception) { }

                        TimerControl(true);
                        startTesting = true;
                        UIStartBtnUpdate("Stop");

                        textBox1.Focus();
                    }
                }
                catch (Exception ex)
                {
                    ReceiveMessageWrite(ex.Message, true);
                }
            }
            else
            {
                backgroundWorker1.CancelAsync();
                try
                {
                    bgWorker1Sem.Release(1);
                }
                catch (Exception) { }

                while (backgroundWorker1.IsBusy)
                    Application.DoEvents();

                InterfaceClose();

                IQxel.releaseControl();
                IQxel.closeTester();

                TimerControl(false);
                startTesting = false;
                UIStartBtnUpdate("Start");
            }
        }

        private void ProcessReceive(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (rtwProxyProcessor != null)
                    rtwProxyProcessor.Receive(e.Data);
                ReceiveMessageWrite(e.Data, true);
            }
        }

        private void ProcessSerialReceive(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = sender as SerialPort;
            string data = sp.ReadExisting();
            if (string.IsNullOrEmpty(data))
                return;

            if (rtwProxyProcessor != null)
                rtwProxyProcessor.Receive(data);
            ReceiveMessageWrite(data, true);
        }

        private void ProcessExit(object sender, EventArgs e)
        {

        }

        private bool CheckSn(string sn)
        {
            if (string.IsNullOrEmpty(sn))
                throw new Exception("Pls type the DUT SN!");

            return true;
        }

        private void testDone()
        {
            string msg = "";
            string result;

            if (finalTestResult == TestResult.TEST_SUCCESS)
            {
                msg = "Test Done!! PASS!!";
                testLog.WriteLine(msg);
                PCMessageWrite(msg, true);
                result = "PASS";
            }
            else
            {
                msg = "Test Done!! FAIL!!";
                testLog.WriteLine(msg);
                PCMessageWrite(msg, true);
                result = "FAIL";
            }

            testLog.WriteLine("============ END TEST ============");
            testLog.WriteLine("End Test Time: " + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss"));
            testLog.WriteLine("Total Test: " + timeCounter + " s");

            // save log
            string testLogDir = testLogDirPath + "\\" + dtStartTest.ToString(dateFormat);
            if (!Directory.Exists(testLogDir))
                Directory.CreateDirectory(testLogDir);
            testLog.Save(testLogDirPath + "\\" + dtStartTest.ToString(dateFormat) + "\\" + result + "_" + sn + "_" + dtStartTest.ToString(datetimeFormat) + ".txt");

            string adbLogDir = adbLogDirPath + "\\" + dtStartTest.ToString(dateFormat);
            if (!Directory.Exists(adbLogDir))
                Directory.CreateDirectory(adbLogDir);
            adbLog.Save(adbLogDirPath + "\\" + dtStartTest.ToString(dateFormat) + "\\" + result + "_" + sn + "_" + dtStartTest.ToString(datetimeFormat) + ".txt");

            // EVT report
            //EVTReport.Produce("EVT_" + sn + "_" + dtStartTest.ToString(datetimeFormat) + ".csv");

            InterfaceClose();

            if (IQxel.bIsConnectTester)
            {
                IQxel.releaseControl();
                IQxel.closeTester();
            }

            TimerControl(false);
            startTesting = false;
            UIStartBtnUpdate("Start");
        }

        private void continueTest()
        {
            try
            {
                bgWorker1Sem.Release(1);
            }
            catch (Exception) { }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = e.ProgressPercentage.ToString() + "%";
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PCMessageWrite("Done.", true);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                bgWorker1Sem.WaitOne();
                if (backgroundWorker1.CancellationPending)
                {
                    return;
                }

                // default test variable
                testState = TestStatus.TEST_INIT;

                // init each test items
                finalTestResult = testItems.rtwTestFunctionList[testNum](testState, testItems.rtwTestPlanList[testNum]);
                if (finalTestResult == TestResult.TEST_FAILURE)
                {
                    // got failed TEST_INIT init state
                    testState = TestStatus.TEST_END; // Test failed and mark as test failure
                    testItems.rtwTestFunctionList[testNum](testState, null); // to handle test failure
                    testDone();
                    continue;
                }

                testState = TestStatus.TEST_START;

                // start test
                finalTestResult = testItems.rtwTestFunctionList[testNum](testState, testItems.rtwTestPlanList[testNum]);
                if (finalTestResult == TestResult.TEST_SUCCESS || finalTestResult == TestResult.TEST_FAILURE)
                {
                    testState = TestStatus.TEST_END;
                    testItems.rtwTestFunctionList[testNum](testState, null);
                }
                else
                {
                    testState = TestStatus.TEST_END; // Test failed and mark as test failure
                    testItems.rtwTestFunctionList[testNum](testState, null); // to handle test failure
                    testDone();
                    continue;
                }

                testNum++;
                backgroundWorker1.ReportProgress(testNum * 100 / totalTestNum);
                if (testNum < totalTestNum) // test end ??
                {
                    continueTest();
                    continue;
                }
                else
                {
                    testDone();
                }
            }
        }

        public void UIStartBtnUpdate(string text)
        {
            if (this.InvokeRequired)
            {
                BtnTextUpdateHandler handler = new BtnTextUpdateHandler(UIStartBtnUpdate);
                this.Invoke(handler, text);
                return;
            }

            button1.Text = text;
        }

        public void ReceiveMessageWrite(string msg, bool newLine = false)
        {
            if (this.InvokeRequired)
            {
                RichTextbox1AppendMsgHandler handler = new RichTextbox1AppendMsgHandler(ReceiveMessageWrite);
                this.BeginInvoke(handler, msg, newLine); // async
                return;
            }

            richTextBox1.AppendText(newLine ? msg + "\r\n" : msg);
            richTextBox1.SelectionStart = richTextBox1.TextLength - 1;
            richTextBox1.ScrollToCaret();
        }

        public void PCMessageWrite(string msg, bool newLine = false)
        {
            if (this.InvokeRequired)
            {
                RichTextbox2AppendMsgHandler handler = new RichTextbox2AppendMsgHandler(PCMessageWrite);
                this.Invoke(handler, msg, newLine);
                return;
            }

            if (string.IsNullOrEmpty(msg) && newLine == false)
                return;

            richTextBox2.AppendText(newLine ? msg + "\r\n" : msg);
            richTextBox2.SelectionStart = richTextBox2.TextLength - 1;
            richTextBox2.ScrollToCaret();
        }

        public void TimerControl(bool enable)
        {
            if (this.InvokeRequired)
            {
                TimerHandler handler = new TimerHandler(TimerControl);
                this.Invoke(handler, enable);
                return;
            }

            if (enable)
                timeCounter = 0;

            timer1.Enabled = enable;
        }


        //private void button2_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (!IQxel.bIsConnectTester)
        //        {
        //            return;
        //        }

        //        if (!IQxel.obtainControl())
        //        {
        //            return;
        //        }

        //        richTextBox1.Clear();
        //        richTextBox2.Clear();

        //        RtwTx tx = new RtwTx();
        //        tx.freq = 2412;
        //        tx.bw = BW.B_20MHZ;
        //        tx.antPath = ANT_PATH.PATH_A;
        //        tx.rateID = RATE_ID.R_11M;
        //        tx.txMode = TX_MODE.PACKET_TX;

        //        double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
        //        IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, 2412, 16, 3, cableloss); // VSA

        //        RtwCommand.StartMp();
        //        //mainForm.adb.SendRtwCommand("rtwpriv wlan0 1 0 a 11M 1");
        //        RtwCommand.StartHwTxCommand(tx);
        //        RtwCommand.IndexCryNextCommand(80);
        //        RtwCommand.SendTxPowerCommand(tx.antPath, 63);

        //        // Perform data capture
        //        IQxel.vsaDataCapture();

        //        IQxel.analyze80211b();
        //        double mPower = IQxel.getScalarMeasurement("rmsPower") + 12;
        //        double evm = Math.Pow(10, IQxel.getScalarMeasurement("evmAll") / 20) * 100;

        //        string powerFormat = "{0,10}:{1,10} dBm {2,15}";
        //        string evmFormat = "{0,10}:{1,10} % {2,15}";

        //        PCMessageWrite(string.Format(powerFormat, "MPower", mPower, " --> Test"), true);
        //        PCMessageWrite(string.Format(evmFormat, "EVM", evm, " --> Test"), true);
        //    }
        //    catch (Exception ex)
        //    {
        //        PCMessageWrite(ex.Message, true);
        //    }
        //    finally
        //    {
        //        //IQxel.closeTester();
        //    }
        //}

        //private void button5_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (!IQxel.bIsConnectTester)
        //        {
        //            return;
        //        }

        //        if (!IQxel.obtainControl())
        //        {
        //            return;
        //        }

        //        //richTextBox1.Clear();
        //        //richTextBox2.Clear();

        //        RtwTx tx = new RtwTx();
        //        tx.freq = 5190;
        //        tx.bw = BW.B_40MHZ;
        //        tx.antPath = ANT_PATH.PATH_B;
        //        tx.rateID = RATE_ID.HTMCS7;
        //        tx.txMode = TX_MODE.PACKET_TX;

        //        double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
        //        IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, 5190, 16, 10, cableloss); // VSA
        //                                                                                                      //IQxel.setVsaTrigger(6, 0, -25, 0, 6, -5, 1500);
        //        RtwCommand.Init();
        //        RtwCommand.StartMp();
        //        //mainForm.adb.SendRtwCommand("rtwpriv wlan0 1 0 a 11M 1");
        //        RtwCommand.StartHwTxCommand(tx);
        //        RtwCommand.IndexCryNextCommand(80);
        //        RtwCommand.SendTxPowerCommand(tx.antPath, 102);

        //        Thread.Sleep(1000);

        //        IQxel.setAgc();

        //        // Perform data capture
        //        IQxel.vsaDataCapture();

        //        IQxel.analyze80211ac();
        //        double mPower = IQxel.getScalarMeasurement("avgPower");
        //        double evm = IQxel.getScalarMeasurement("evmAvgAll");

        //        string powerFormat = "{0,10}:{1,10} dBm {2,15}";
        //        string evmFormat = "{0,10}:{1,10} % {2,15}";

        //        PCMessageWrite(string.Format(powerFormat, "MPower", mPower, " --> Test"), true);
        //        PCMessageWrite(string.Format(evmFormat, "EVM", evm, " --> Test"), true);
        //    }
        //    catch (Exception ex)
        //    {
        //        PCMessageWrite(ex.Message, true);
        //    }
        //    finally
        //    {
        //        //IQxel.closeTester();
        //    }
        //}

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel4.Text = (++timeCounter).ToString() + " s";
        }

        public void InterfaceClose()
        {
            if (SysConfig.connectionInterface == "ADB")
            {
                // close adb.exe
                if (adb != null)
                {
                    adb.Close();
                    adb = null;
                }
            }
            else if (SysConfig.connectionInterface.StartsWith("COM"))
            {
                if (comPort != null)
                {
                    comPort.Close();
                    comPort = null;
                }
            }
        }

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (!IQxel.bIsConnectTester)
        //        {
        //            return;
        //        }

        //        if (!IQxel.obtainControl())
        //        {
        //            return;
        //        }

        //        RtwTx tx = new RtwTx();
        //        tx.freq = 2412;
        //        tx.bw = BW.B_20MHZ;
        //        tx.antPath = ANT_PATH.PATH_A;
        //        tx.rateID = RATE_ID.R_54M;
        //        tx.txMode = TX_MODE.PACKET_TX;

        //        double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
        //        IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, 2412, 16, 10, cableloss); // VSA
        //        //IQxel.setVsaTrigger(6, 0, -25, 0, 6, -5, 1500);
        //        RtwCommand.Init();
        //        RtwCommand.StartMp();
        //        RtwCommand.StartHwTxCommand(tx);
        //        //RTWTest.IndexCryNextCommand(80);
        //        //RTWTest.SendTxPowerCommand(tx.antPath, 100);

        //        Thread.Sleep(1000);

        //        IQxel.setAgc();

        //        // Perform data capture
        //        IQxel.vsaDataCapture();

        //        IQxel.analyze80211ag();
        //        double mPower0 = IQxel.getScalarMeasurement("rmsPowerNoGap");
        //        double evm = IQxel.getScalarMeasurement("evmAll");
        //        double freqErr = IQxel.getScalarMeasurement("clockErr");
        //        double leakage = IQxel.getScalarMeasurement("dcLeakageDbc");

        //        IQmeasure.LP_AnalyzePower(0.0, 0.0);
        //        double mPower1 = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm");

        //        string powerFormat = "{0,10}:{1,10} dBm {2,15}";
        //        string evmFormat = "{0,10}:{1,10} % {2,15}";
        //        string freqErrFormat = "{0,10}:{1,10:0.00} ppm {2,15}";
        //        string leakageFormat = "{0,10}:{1,10:0.00} dB  {2,15}";
        //        string maskFormat;

        //        PCMessageWrite(string.Format(powerFormat, "MPower0", mPower0, " --> Test"), true);
        //        PCMessageWrite(string.Format(powerFormat, "MPower1", mPower1, " --> Test"), true);
        //        PCMessageWrite(string.Format(evmFormat, "EVM", evm, " --> Test"), true);
        //        PCMessageWrite(string.Format(freqErrFormat, "Freq Err", freqErr, " --> Test"), true);
        //        PCMessageWrite(string.Format(leakageFormat, "LO Leakage", leakage, " --> Test"), true);
        //    }
        //    catch (Exception ex)
        //    {
        //        PCMessageWrite(ex.Message, true);
        //    }
        //    finally
        //    {
        //        //IQxel.closeTester();
        //    }
        //}

        //private void button7_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (!IQxel.bIsConnectTester)
        //        {
        //            return;
        //        }

        //        if (!IQxel.obtainControl())
        //        {
        //            return;
        //        }

        //        RtwTx tx = new RtwTx();
        //        tx.freq = 2457;
        //        tx.bw = BW.B_40MHZ;
        //        tx.antPath = ANT_PATH.PATH_A;
        //        tx.rateID = RATE_ID.HTMCS7;
        //        tx.txMode = TX_MODE.PACKET_TX;

        //        double cableloss = SysConfig.GetCableLoss(tx.freq, (int)tx.antPath);
        //        IQxel.setIQxelVsa(IQAPI_C_shap.IQmeasure.IQAPI_PORT_ENUM.PORT_LEFT, 2457, 16, 10, cableloss); // VSA
        //        //IQxel.setVsaTrigger(6, 0, -25, 0, 6, -5, 1500);
        //        RtwCommand.Init();
        //        RtwCommand.StartMp();
        //        RtwCommand.StartHwTxCommand(tx);
        //        //RTWTest.IndexCryNextCommand(80);
        //        //RTWTest.SendTxPowerCommand(tx.antPath, 100);

        //        Thread.Sleep(1000);

        //        IQxel.setAgc();

        //        // Perform data capture
        //        IQxel.vsaDataCapture();

        //        IQxel.analyze80211n();
        //        double mPower0 = IQxel.getScalarMeasurement("rmsPowerNoGap");
        //        double evm = IQxel.getScalarMeasurement("evmAvgAll");
        //        double freqErr = IQxel.getScalarMeasurement("symClockErrorPpm");
        //        double leakage = IQxel.getScalarMeasurement("dcLeakageDbc");

        //        IQmeasure.LP_AnalyzePower(0.0, 0.0);
        //        double mPower1 = IQxel.getScalarMeasurement("P_av_no_gap_all_dBm");

        //        string powerFormat = "{0,10}:{1,10} dBm {2,15}";
        //        string evmFormat = "{0,10}:{1,10} % {2,15}";
        //        string freqErrFormat = "{0,10}:{1,10:0.00} ppm {2,15}";
        //        string leakageFormat = "{0,10}:{1,10:0.00} dB  {2,15}";
        //        string maskFormat;

        //        PCMessageWrite(string.Format(powerFormat, "MPower0", mPower0, " --> Test"), true);
        //        PCMessageWrite(string.Format(powerFormat, "MPower1", mPower1, " --> Test"), true);
        //        PCMessageWrite(string.Format(evmFormat, "EVM", evm, " --> Test"), true);
        //        PCMessageWrite(string.Format(freqErrFormat, "Freq Err", freqErr, " --> Test"), true);
        //        PCMessageWrite(string.Format(leakageFormat, "LO Leakage", leakage, " --> Test"), true);
        //    }
        //    catch (Exception ex)
        //    {
        //        PCMessageWrite(ex.Message, true);
        //    }
        //    finally
        //    {
        //        //IQxel.closeTester();
        //    }
        //}
    }
}
