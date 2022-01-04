using RTKModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public class EVTReport
    {
        public static List<TxTest> txTestList = null;
        public static List<RxTest> rxTestList = null;

        private static readonly string REPORTDIR = Environment.CurrentDirectory + "\\EVT";
        private static string reportName;

        public enum ReportType
        {
            ByRate,
            ByChannel
        }

        public class TxTest
        {
            public WIFI protocol;
            public RATE_ID rateID;
            public BW bw;
            public ANT_PATH ant;
            public int channel;
            public int frequency;
            public int gain;
            public double outputPower;
            public double evm;
            public double freqOffset;
            public int thermal;
            public double? leakage;
            public double flatness;
        }

        public class RxTest
        {
            public WIFI protocol;
            public RATE_ID rateID;
            public BW bw;
            public ANT_PATH ant;
            public int channel;
            public int frequency;
            public double rxPower;
            public double per;
        }

        public static void AddToTxTestList(RATE_ID rateID, BW bw, ANT_PATH ant, int channel, int frequency, int gain, double outputPower, double evm, double freqOffset, double? leakage, double flatness, int thermal)
        {
            if (txTestList == null)
                return;

            TxTest txTest = new TxTest();
            txTest.protocol = Wifi.GetWifiProtocol(rateID);
            txTest.rateID = rateID;
            txTest.bw = bw;
            txTest.ant = ant;
            txTest.channel = channel;
            txTest.frequency = frequency;
            txTest.gain = gain;
            txTest.outputPower = outputPower;
            txTest.evm = evm;
            txTest.freqOffset = freqOffset;
            txTest.leakage = leakage;
            txTest.flatness = flatness;
            txTest.thermal = thermal;
            txTestList.Add(txTest);
        }

        public static void AddToRxTestList(RATE_ID rateID, BW bw, ANT_PATH ant, int channel, int frequency, double rxPower, double per)
        {
            if (rxTestList == null)
                return;

            RxTest rxTest = new RxTest();
            rxTest.protocol = Wifi.GetWifiProtocol(rateID);
            rxTest.rateID = rateID;
            rxTest.bw = bw;
            rxTest.ant = ant;
            rxTest.channel = channel;
            rxTest.frequency = frequency;
            rxTest.per = per;
            rxTestList.Add(rxTest);
        }

        public static List<TxTest> GetTxTestByRate(RATE_ID rateID, BW bw, ANT_PATH ant, int band)
        {
            return txTestList.FindAll(tx => tx.rateID == rateID && tx.bw == bw && tx.ant == ant && (int)Math.Floor((double)tx.frequency / 1000) == band)
                            .OrderBy(tx => tx.channel)
                            .ToList();
        }

        public static List<TxTest> GetTxTestByChannel(WIFI protocol, CH ch, BW bw, ANT_PATH ant, int band)
        {
            return txTestList.FindAll(tx => Wifi.GetWifiProtocol(tx.rateID) == protocol && tx.channel == (int)ch && tx.bw == bw && tx.ant == ant && (int)Math.Floor((double)tx.frequency / 1000) == band)
                            .OrderByDescending(tx => tx.rateID)
                            .ToList();
        }

        public static List<RxTest> GetRxTestByRate(RATE_ID rateID, BW bw, ANT_PATH ant, int band)
        {
            return rxTestList.FindAll(rx => rx.rateID == rateID && rx.bw == bw && rx.ant == ant && (int)Math.Floor((double)rx.frequency / 1000) == band)
                            .OrderBy(rx => rx.channel)
                            .ToList();
        }

        public static List<RxTest> GetRxTestByChannel(WIFI protocol, CH ch, BW bw, ANT_PATH ant, int band)
        {
            return rxTestList.FindAll(rx => Wifi.GetWifiProtocol(rx.rateID) == protocol && rx.channel == (int)ch && rx.bw == bw && rx.ant == ant && (int)Math.Floor((double)rx.frequency / 1000) == band)
                            .OrderByDescending(rx => rx.rateID)
                            .ToList();
        }

        public static void Produce(string filename)
        {
            reportName = filename;

            log(GetTxTestByRate(RATE_ID.R_1M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_2M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_5_5M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_11M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByRate);

            log(GetTxTestByRate(RATE_ID.R_1M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_2M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_5_5M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_11M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByRate);

            log(GetTxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);

            log(GetTxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);

            log(GetTxTestByRate(RATE_ID.VHT1MCS0, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS1, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS2, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS3, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS4, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS5, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS6, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS7, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS8, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS9, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS0, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS1, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS2, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS3, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS4, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS5, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS6, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS7, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS8, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS9, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS0, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS1, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS2, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS3, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS4, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS5, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS6, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS7, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS8, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS9, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByRate);

            log(GetTxTestByRate(RATE_ID.VHT1MCS0, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS1, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS2, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS3, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS4, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS5, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS6, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS7, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS8, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS9, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS0, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS1, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS2, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS3, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS4, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS5, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS6, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS7, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS8, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS9, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS0, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS1, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS2, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS3, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS4, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS5, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS6, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS7, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS8, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);
            log(GetTxTestByRate(RATE_ID.VHT1MCS9, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByRate);

            log(GetTxTestByChannel(WIFI.B, CH.CH1, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.B, CH.CH7, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.B, CH.CH13, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.B, CH.CH1, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.B, CH.CH7, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.B, CH.CH13, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.AG, CH.CH1, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH7, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH13, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH1, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH7, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH13, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.N, CH.CH1, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH7, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH13, BW.B_20MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH1, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH7, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH13, BW.B_20MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.N, CH.CH1, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH7, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH13, BW.B_40MHZ, ANT_PATH.PATH_A, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH1, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH7, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH13, BW.B_40MHZ, ANT_PATH.PATH_B, 2), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.AC, CH.CH36, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH108, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH132, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH177, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH36, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH108, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH132, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH177, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.AC, CH.CH62, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH102, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH142, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH151, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH62, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH102, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH142, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH151, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.AC, CH.CH42, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH122, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH171, BW.B_80MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH42, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH122, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AC, CH.CH171, BW.B_80MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.AG, CH.CH36, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH108, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH132, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH177, BW.B_20MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH36, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH108, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH132, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.AG, CH.CH177, BW.B_20MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);

            log(GetTxTestByChannel(WIFI.N, CH.CH38, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH102, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH134, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH175, BW.B_40MHZ, ANT_PATH.PATH_A, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH38, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH102, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH134, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);
            log(GetTxTestByChannel(WIFI.N, CH.CH175, BW.B_40MHZ, ANT_PATH.PATH_B, 5), ReportType.ByChannel);

            log(GetRxTestByRate(RATE_ID.R_11M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_5_5M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_2M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_1M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_11M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_5_5M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_2M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_1M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));

            log(GetRxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_B, 2));

            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_B, 2));

            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_A, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_B, 2));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_B, 2));

            log(GetRxTestByRate(RATE_ID.VHT1MCS8, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS7, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS6, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS5, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS4, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS3, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS2, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS1, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS0, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS8, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS7, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS6, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS5, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS4, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS3, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS2, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS1, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS0, BW.B_20MHZ, ANT_PATH.PATH_B, 5));

            log(GetRxTestByRate(RATE_ID.VHT1MCS9, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS8, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS7, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS6, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS5, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS4, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS3, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS2, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS1, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS0, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS9, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS8, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS7, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS6, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS5, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS4, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS3, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS2, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS1, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS0, BW.B_40MHZ, ANT_PATH.PATH_B, 5));

            log(GetRxTestByRate(RATE_ID.VHT1MCS9, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS8, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS7, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS6, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS5, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS4, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS3, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS2, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS1, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS0, BW.B_80MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS9, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS8, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS7, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS6, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS5, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS4, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS3, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS2, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS1, BW.B_80MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.VHT1MCS0, BW.B_80MHZ, ANT_PATH.PATH_B, 5));

            log(GetRxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.R_54M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.R_48M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.R_36M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.R_24M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.R_18M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.R_12M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.R_9M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.R_6M, BW.B_20MHZ, ANT_PATH.PATH_B, 5));

            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_20MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_20MHZ, ANT_PATH.PATH_B, 5));

            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_A, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS7, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS6, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS5, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS4, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS3, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS2, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS1, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
            log(GetRxTestByRate(RATE_ID.HTMCS0, BW.B_40MHZ, ANT_PATH.PATH_B, 5));
        }

        private static void log(List<TxTest> txTests, ReportType type)
        {
            string saveFile = REPORTDIR + "\\" + reportName;
            StringBuilder sb = new StringBuilder();
            if(type == ReportType.ByRate)
            {
                foreach (TxTest txTest in txTests)
                {
                    sb.AppendLine(txTest.channel + "," + txTest.frequency + "," + Wifi.bwDic[txTest.bw] + "," + Wifi.antPathDic[txTest.ant] + "," + Wifi.rateIdDic[txTest.rateID] + "," + txTest.gain + "," + txTest.outputPower + "," + txTest.evm + "," + txTest.freqOffset + "," + "0x" + txTest.thermal.ToString("X2") + "," + txTest.flatness + "," + txTest.leakage);
                }
            }
            else
            {
                foreach (TxTest txTest in txTests)
                {
                    sb.AppendLine(txTest.channel + "," + txTest.frequency + "," + Wifi.bwDic[txTest.bw] + "," + Wifi.antPathDic[txTest.ant] + "," + Wifi.rateIdDic[txTest.rateID] + "," + txTest.gain + "," + txTest.outputPower + "," + txTest.evm + "," + txTest.freqOffset + "," + "0x" + txTest.thermal.ToString("X2") + "," + txTest.flatness + "," + txTest.leakage);
                }
            }

            if (txTests != null)
                sb.AppendLine();

            File.AppendAllText(saveFile, sb.ToString());
        }

        private static void log(List<RxTest> rxTests)
        {
            string saveFile = REPORTDIR + "\\" + reportName;
            StringBuilder sb = new StringBuilder();
            foreach (RxTest rxTest in rxTests)
            {
                sb.AppendLine(Wifi.rateIdDic[rxTest.rateID] + "," + rxTest.channel + "," + rxTest.frequency + "," + Wifi.bwDic[rxTest.bw] + "," + Wifi.antPathDic[rxTest.ant] + "," + rxTest.rxPower + "," + rxTest.per);
            }

            if (rxTests != null)
                sb.AppendLine();

            File.AppendAllText(saveFile, sb.ToString());
        }
    }
}
