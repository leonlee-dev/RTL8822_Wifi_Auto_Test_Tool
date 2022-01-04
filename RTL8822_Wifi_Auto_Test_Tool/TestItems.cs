using RTKModule;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static RTL8822_Wifi_Auto_Test_Tool.TestTask;

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public class TestItems
    {
        public List<Func<TestStatus, Rtw, TestResult>> rtwTestFunctionList { get; private set; }
        public List<Rtw> rtwTestPlanList { get; private set; }
        private static TestItems testItems = new TestItems();

        private TestItems()
        {

        }

        public static TestItems GetInstance()
        {
            if (testItems.rtwTestFunctionList == null)
                testItems.rtwTestFunctionList = new List<Func<TestStatus, Rtw, TestResult>>();
            if (testItems.rtwTestPlanList == null)
                testItems.rtwTestPlanList = new List<Rtw>();
            return testItems;
        }

        private void AddTxVerifyItem(TX_MODE txMode, CH ch, ANT_PATH path, VerifyItem verifyItem, TxCriterion txCriterion)
        {
            RtwTx tx = new RtwTx();
            tx.txMode = txMode;
            tx.freq = (int)ch;
            switch (verifyItem)
            {
                case VerifyItem.MCS0_B20:
                    tx.rateID = RATE_ID.HTMCS0;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS1_B20:
                    tx.rateID = RATE_ID.HTMCS1;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS2_B20:
                    tx.rateID = RATE_ID.HTMCS2;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS3_B20:
                    tx.rateID = RATE_ID.HTMCS3;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS4_B20:
                    tx.rateID = RATE_ID.HTMCS4;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS5_B20:
                    tx.rateID = RATE_ID.HTMCS5;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS6_B20:
                    tx.rateID = RATE_ID.HTMCS6;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS7_B20:
                    tx.rateID = RATE_ID.HTMCS7;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS8_B20:
                    tx.rateID = RATE_ID.HTMCS8;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS9_B20:
                    tx.rateID = RATE_ID.HTMCS9;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS0_B40:
                    tx.rateID = RATE_ID.HTMCS0;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS1_B40:
                    tx.rateID = RATE_ID.HTMCS1;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS2_B40:
                    tx.rateID = RATE_ID.HTMCS2;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS3_B40:
                    tx.rateID = RATE_ID.HTMCS3;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS4_B40:
                    tx.rateID = RATE_ID.HTMCS4;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS5_B40:
                    tx.rateID = RATE_ID.HTMCS5;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS6_B40:
                    tx.rateID = RATE_ID.HTMCS6;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS7_B40:
                    tx.rateID = RATE_ID.HTMCS7;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS8_B40:
                    tx.rateID = RATE_ID.HTMCS8;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS9_B40:
                    tx.rateID = RATE_ID.HTMCS9;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS0_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS0;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS1_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS1;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS2_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS2;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS3_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS3;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS4_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS4;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS5_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS5;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS6_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS6;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS7_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS7;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS8_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS8;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS9_NSS1_B20:
                    tx.rateID = RATE_ID.VHT1MCS9;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS0_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS0;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS1_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS1;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS2_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS2;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS3_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS3;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS4_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS4;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS5_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS5;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS6_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS6;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS7_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS7;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS8_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS8;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS9_NSS1_B40:
                    tx.rateID = RATE_ID.VHT1MCS9;
                    tx.bw = BW.B_40MHZ;
                    break;
                case VerifyItem.MCS0_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS0;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS1_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS1;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS2_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS2;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS3_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS3;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS4_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS4;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS5_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS5;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS6_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS6;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS7_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS7;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS8_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS8;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.MCS9_NSS1_B80:
                    tx.rateID = RATE_ID.VHT1MCS9;
                    tx.bw = BW.B_80MHZ;
                    break;
                case VerifyItem.OFDM_6M:
                    tx.rateID = RATE_ID.R_6M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.OFDM_9M:
                    tx.rateID = RATE_ID.R_9M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.OFDM_12M:
                    tx.rateID = RATE_ID.R_12M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.OFDM_18M:
                    tx.rateID = RATE_ID.R_18M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.OFDM_24M:
                    tx.rateID = RATE_ID.R_24M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.OFDM_36M:
                    tx.rateID = RATE_ID.R_36M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.OFDM_48M:
                    tx.rateID = RATE_ID.R_48M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.OFDM_54M:
                    tx.rateID = RATE_ID.R_54M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.DSSS_1M:
                    tx.rateID = RATE_ID.R_1M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.DSSS_2M:
                    tx.rateID = RATE_ID.R_2M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.CCK_5_5M:
                    tx.rateID = RATE_ID.R_5_5M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.CCK_11M:
                    tx.rateID = RATE_ID.R_11M;
                    tx.bw = BW.B_20MHZ;
                    break;
                case VerifyItem.MCS7_NSS2_B80:
                case VerifyItem.MCS15_B40:
                default:
                    break;
            }
            tx.antPath = path;
            // add test item
            rtwTestPlanList.Add(new TxVerifyPlan(tx, txCriterion));
            // add test function
            switch (verifyItem)
            {
                case VerifyItem.MCS0_B20:
                case VerifyItem.MCS1_B20:
                case VerifyItem.MCS2_B20:
                case VerifyItem.MCS3_B20:
                case VerifyItem.MCS4_B20:
                case VerifyItem.MCS5_B20:
                case VerifyItem.MCS6_B20:
                case VerifyItem.MCS7_B20:
                case VerifyItem.MCS8_B20:
                case VerifyItem.MCS9_B20:
                case VerifyItem.MCS0_B40:
                case VerifyItem.MCS1_B40:
                case VerifyItem.MCS2_B40:
                case VerifyItem.MCS3_B40:
                case VerifyItem.MCS4_B40:
                case VerifyItem.MCS5_B40:
                case VerifyItem.MCS6_B40:
                case VerifyItem.MCS7_B40:
                case VerifyItem.MCS8_B40:
                case VerifyItem.MCS9_B40:
                    rtwTestFunctionList.Add(TxVerifyAnalyzeByWifiN);
                    break;
                case VerifyItem.MCS0_NSS1_B20:
                case VerifyItem.MCS1_NSS1_B20:
                case VerifyItem.MCS2_NSS1_B20:
                case VerifyItem.MCS3_NSS1_B20:
                case VerifyItem.MCS4_NSS1_B20:
                case VerifyItem.MCS5_NSS1_B20:
                case VerifyItem.MCS6_NSS1_B20:
                case VerifyItem.MCS7_NSS1_B20:
                case VerifyItem.MCS8_NSS1_B20:
                case VerifyItem.MCS9_NSS1_B20:
                case VerifyItem.MCS0_NSS1_B40:
                case VerifyItem.MCS1_NSS1_B40:
                case VerifyItem.MCS2_NSS1_B40:
                case VerifyItem.MCS3_NSS1_B40:
                case VerifyItem.MCS4_NSS1_B40:
                case VerifyItem.MCS5_NSS1_B40:
                case VerifyItem.MCS6_NSS1_B40:
                case VerifyItem.MCS7_NSS1_B40:
                case VerifyItem.MCS8_NSS1_B40:
                case VerifyItem.MCS9_NSS1_B40:
                case VerifyItem.MCS0_NSS1_B80:
                case VerifyItem.MCS1_NSS1_B80:
                case VerifyItem.MCS2_NSS1_B80:
                case VerifyItem.MCS3_NSS1_B80:
                case VerifyItem.MCS4_NSS1_B80:
                case VerifyItem.MCS5_NSS1_B80:
                case VerifyItem.MCS6_NSS1_B80:
                case VerifyItem.MCS7_NSS1_B80:
                case VerifyItem.MCS8_NSS1_B80:
                case VerifyItem.MCS9_NSS1_B80:
                    rtwTestFunctionList.Add(TxVerifyAnalyzeByWifiAc);
                    break;
                case VerifyItem.OFDM_6M:
                case VerifyItem.OFDM_9M:
                case VerifyItem.OFDM_12M:
                case VerifyItem.OFDM_18M:
                case VerifyItem.OFDM_24M:
                case VerifyItem.OFDM_36M:
                case VerifyItem.OFDM_48M:
                case VerifyItem.OFDM_54M:
                    rtwTestFunctionList.Add(TxVerifyAnalyzeByWifiAg);
                    break;
                case VerifyItem.DSSS_1M:
                case VerifyItem.DSSS_2M:
                case VerifyItem.CCK_5_5M:
                case VerifyItem.CCK_11M:
                    rtwTestFunctionList.Add(TxVerifyAnalyzeByWifib);
                    break;
                case VerifyItem.MCS7_NSS2_B80:
                case VerifyItem.MCS15_B40:
                default:
                    break;
            }
        }

        private void AddRxVerifyItem(CH ch , ANT_PATH path, VerifyItem verifyItem, RxCriterion rxCriterion)
        {
            RtwRx rx = new RtwRx();
            string streamFile = "";
            rx.freq = (int)ch;
            switch (verifyItem)
            {
                case VerifyItem.MCS0_B20:
                    rx.rateID = RATE_ID.HTMCS0;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS0.iqvsg";
                    break;
                case VerifyItem.MCS1_B20:
                    rx.rateID = RATE_ID.HTMCS1;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS1.iqvsg";
                    break;
                case VerifyItem.MCS2_B20:
                    rx.rateID = RATE_ID.HTMCS2;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS2.iqvsg";
                    break;
                case VerifyItem.MCS3_B20:
                    rx.rateID = RATE_ID.HTMCS3;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS3.iqvsg";
                    break;
                case VerifyItem.MCS4_B20:
                    rx.rateID = RATE_ID.HTMCS4;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS4.iqvsg";
                    break;
                case VerifyItem.MCS5_B20:
                    rx.rateID = RATE_ID.HTMCS5;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS5.iqvsg";
                    break;
                case VerifyItem.MCS6_B20:
                    rx.rateID = RATE_ID.HTMCS6;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS6.iqvsg";
                    break;
                case VerifyItem.MCS7_B20:
                    rx.rateID = RATE_ID.HTMCS7;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS7.iqvsg";
                    break;
                case VerifyItem.MCS8_B20:
                    rx.rateID = RATE_ID.HTMCS8;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS8.iqvsg";
                    break;
                case VerifyItem.MCS9_B20:
                    rx.rateID = RATE_ID.HTMCS9;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT20_MCS9.iqvsg";
                    break;
                case VerifyItem.MCS0_B40:
                    rx.rateID = RATE_ID.HTMCS0;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS0.iqvsg";
                    break;
                case VerifyItem.MCS1_B40:
                    rx.rateID = RATE_ID.HTMCS1;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS1.iqvsg";
                    break;
                case VerifyItem.MCS2_B40:
                    rx.rateID = RATE_ID.HTMCS2;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS2.iqvsg";
                    break;
                case VerifyItem.MCS3_B40:
                    rx.rateID = RATE_ID.HTMCS3;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS3.iqvsg";
                    break;
                case VerifyItem.MCS4_B40:
                    rx.rateID = RATE_ID.HTMCS4;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS4.iqvsg";
                    break;
                case VerifyItem.MCS5_B40:
                    rx.rateID = RATE_ID.HTMCS5;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS5.iqvsg";
                    break;
                case VerifyItem.MCS6_B40:
                    rx.rateID = RATE_ID.HTMCS6;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS6.iqvsg";
                    break;
                case VerifyItem.MCS7_B40:
                    rx.rateID = RATE_ID.HTMCS7;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS7.iqvsg";
                    break;
                case VerifyItem.MCS8_B40:
                    rx.rateID = RATE_ID.HTMCS8;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS8.iqvsg";
                    break;
                case VerifyItem.MCS9_B40:
                    rx.rateID = RATE_ID.HTMCS9;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_HT40_MCS9.iqvsg";
                    break;
                case VerifyItem.MCS0_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS0;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS0.iqvsg";
                    break;
                case VerifyItem.MCS1_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS1;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS1.iqvsg";
                    break;
                case VerifyItem.MCS2_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS2;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS2.iqvsg";
                    break;
                case VerifyItem.MCS3_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS3;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS3.iqvsg";
                    break;
                case VerifyItem.MCS4_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS4;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS4.iqvsg";
                    break;
                case VerifyItem.MCS5_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS5;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS5.iqvsg";
                    break;
                case VerifyItem.MCS6_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS6;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS6.iqvsg";
                    break;
                case VerifyItem.MCS7_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS7;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS7.iqvsg";
                    break;
                case VerifyItem.MCS8_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS8;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS8.iqvsg";
                    break;
                case VerifyItem.MCS9_NSS1_B20:
                    rx.rateID = RATE_ID.VHT1MCS9;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT20_S1_MCS9.iqvsg";
                    break;
                case VerifyItem.MCS0_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS0;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS0.iqvsg";
                    break;
                case VerifyItem.MCS1_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS1;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS1.iqvsg";
                    break;
                case VerifyItem.MCS2_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS2;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS2.iqvsg";
                    break;
                case VerifyItem.MCS3_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS3;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS3.iqvsg";
                    break;
                case VerifyItem.MCS4_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS4;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS4.iqvsg";
                    break;
                case VerifyItem.MCS5_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS5;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS5.iqvsg";
                    break;
                case VerifyItem.MCS6_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS6;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS6.iqvsg";
                    break;
                case VerifyItem.MCS7_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS7;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS7.iqvsg";
                    break;
                case VerifyItem.MCS8_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS8;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS8.iqvsg";
                    break;
                case VerifyItem.MCS9_NSS1_B40:
                    rx.rateID = RATE_ID.VHT1MCS9;
                    rx.bw = BW.B_40MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT40_S1_MCS9.iqvsg";
                    break;
                case VerifyItem.MCS0_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS0;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS0.iqvsg";
                    break;
                case VerifyItem.MCS1_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS1;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS1.iqvsg";
                    break;
                case VerifyItem.MCS2_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS2;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS2.iqvsg";
                    break;
                case VerifyItem.MCS3_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS3;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS3.iqvsg";
                    break;
                case VerifyItem.MCS4_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS4;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS4.iqvsg";
                    break;
                case VerifyItem.MCS5_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS5;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS5.iqvsg";
                    break;
                case VerifyItem.MCS6_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS6;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS6.iqvsg";
                    break;
                case VerifyItem.MCS7_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS7;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS7.iqvsg";
                    break;
                case VerifyItem.MCS8_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS8;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS8.iqvsg";
                    break;
                case VerifyItem.MCS9_NSS1_B80:
                    rx.rateID = RATE_ID.VHT1MCS9;
                    rx.bw = BW.B_80MHZ;
                    streamFile = @".\iqWaveforms\WiFi_11AC_VHT80_S1_MCS9.iqvsg";
                    break;
                case VerifyItem.OFDM_6M:
                    rx.rateID = RATE_ID.R_6M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-6.iqvsg";
                    break;
                case VerifyItem.OFDM_9M:
                    rx.rateID = RATE_ID.R_9M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-9.iqvsg";
                    break;
                case VerifyItem.OFDM_12M:
                    rx.rateID = RATE_ID.R_12M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-12.iqvsg";
                    break;
                case VerifyItem.OFDM_18M:
                    rx.rateID = RATE_ID.R_18M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-18.iqvsg";
                    break;
                case VerifyItem.OFDM_24M:
                    rx.rateID = RATE_ID.R_24M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-24.iqvsg";
                    break;
                case VerifyItem.OFDM_36M:
                    rx.rateID = RATE_ID.R_36M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-36.iqvsg";
                    break;
                case VerifyItem.OFDM_48M:
                    rx.rateID = RATE_ID.R_48M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-48.iqvsg";
                    break;
                case VerifyItem.OFDM_54M:
                    rx.rateID = RATE_ID.R_54M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_OFDM-54.iqvsg";
                    break;
                case VerifyItem.DSSS_1M:
                    rx.rateID = RATE_ID.R_1M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_DSSS-1L.iqvsg";
                    break;
                case VerifyItem.DSSS_2M:
                    rx.rateID = RATE_ID.R_2M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_DSSS-2L.iqvsg";
                    break;
                case VerifyItem.CCK_5_5M:
                    rx.rateID = RATE_ID.R_5_5M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_CCK-5_5L.iqvsg";
                    break;
                case VerifyItem.CCK_11M:
                    rx.rateID = RATE_ID.R_11M;
                    rx.bw = BW.B_20MHZ;
                    streamFile = @".\iqWaveforms\WiFi_CCK-11L.iqvsg";
                    break;
                case VerifyItem.MCS7_NSS2_B80:
                case VerifyItem.MCS15_B40:
                default:
                    break;
            }
            rx.antPath = path;
            // add test item
            rtwTestPlanList.Add(new RxVerifyPlan(rx, streamFile, rxCriterion));
            // add test function
            switch (verifyItem)
            {
                case VerifyItem.MCS0_B20:
                case VerifyItem.MCS1_B20:
                case VerifyItem.MCS2_B20:
                case VerifyItem.MCS3_B20:
                case VerifyItem.MCS4_B20:
                case VerifyItem.MCS5_B20:
                case VerifyItem.MCS6_B20:
                case VerifyItem.MCS7_B20:
                case VerifyItem.MCS8_B20:
                case VerifyItem.MCS9_B20:
                case VerifyItem.MCS0_B40:
                case VerifyItem.MCS1_B40:
                case VerifyItem.MCS2_B40:
                case VerifyItem.MCS3_B40:
                case VerifyItem.MCS4_B40:
                case VerifyItem.MCS5_B40:
                case VerifyItem.MCS6_B40:
                case VerifyItem.MCS7_B40:
                case VerifyItem.MCS8_B40:
                case VerifyItem.MCS9_B40:
                case VerifyItem.MCS0_NSS1_B20:
                case VerifyItem.MCS1_NSS1_B20:
                case VerifyItem.MCS2_NSS1_B20:
                case VerifyItem.MCS3_NSS1_B20:
                case VerifyItem.MCS4_NSS1_B20:
                case VerifyItem.MCS5_NSS1_B20:
                case VerifyItem.MCS6_NSS1_B20:
                case VerifyItem.MCS7_NSS1_B20:
                case VerifyItem.MCS8_NSS1_B20:
                case VerifyItem.MCS9_NSS1_B20:
                case VerifyItem.MCS0_NSS1_B40:
                case VerifyItem.MCS1_NSS1_B40:
                case VerifyItem.MCS2_NSS1_B40:
                case VerifyItem.MCS3_NSS1_B40:
                case VerifyItem.MCS4_NSS1_B40:
                case VerifyItem.MCS5_NSS1_B40:
                case VerifyItem.MCS6_NSS1_B40:
                case VerifyItem.MCS7_NSS1_B40:
                case VerifyItem.MCS8_NSS1_B40:
                case VerifyItem.MCS9_NSS1_B40:
                case VerifyItem.MCS0_NSS1_B80:
                case VerifyItem.MCS1_NSS1_B80:
                case VerifyItem.MCS2_NSS1_B80:
                case VerifyItem.MCS3_NSS1_B80:
                case VerifyItem.MCS4_NSS1_B80:
                case VerifyItem.MCS5_NSS1_B80:
                case VerifyItem.MCS6_NSS1_B80:
                case VerifyItem.MCS7_NSS1_B80:
                case VerifyItem.MCS8_NSS1_B80:
                case VerifyItem.MCS9_NSS1_B80:
                case VerifyItem.OFDM_6M:
                case VerifyItem.OFDM_9M:
                case VerifyItem.OFDM_12M:
                case VerifyItem.OFDM_18M:
                case VerifyItem.OFDM_24M:
                case VerifyItem.OFDM_36M:
                case VerifyItem.OFDM_48M:
                case VerifyItem.OFDM_54M:
                case VerifyItem.DSSS_1M:
                case VerifyItem.DSSS_2M:
                case VerifyItem.CCK_5_5M:
                case VerifyItem.CCK_11M:
                    rtwTestFunctionList.Add(RxVerify);
                    break;
                case VerifyItem.MCS7_NSS2_B80:
                case VerifyItem.MCS15_B40:
                default:
                    break;
            }
        }

        public void AddFunctions()
        {
            if (rtwTestFunctionList.Count > 0) rtwTestFunctionList.Clear();
            if (rtwTestPlanList.Count > 0) rtwTestPlanList.Clear();

            RtwTx tx = new RtwTx();
            RtwRx rx = new RtwRx();

            CrystalCriterion crystalCriterion;
            CalibCriterion calibCriterion;
            TxCriterion txCriterion;
            RxCriterion rxCriterion;

            rtwTestPlanList.Add(new RtwTx());
            rtwTestFunctionList.Add(InitTester);

            rtwTestPlanList.Add(new RtwTx());
            rtwTestFunctionList.Add(Init);

            // ------------------ Pre-heating ------------------ //

            if (SysConfig.testItems.Contains("CALIB") || SysConfig.testItems.Contains("TX") || SysConfig.testItems.Contains("HEAT"))
            {
                rtwTestPlanList.Add(new RtwTx(2442, BW.B_40MHZ, ANT_PATH.PATH_AB, RATE_ID.VHT2MCS7, TX_MODE.PACKET_TX));
                rtwTestFunctionList.Add(PreHeating);
            }

            if (SysConfig.testItems.Contains("CALIB"))
            {
                // ------------------ Crystal Calib ------------------ //

                switch (SysConfig.crystal.crystalItem)
                {
                    case CrystalItem.MCS7_B40:
                        crystalCriterion = SysConfig.crystalCalibCriterion.crystalCriterion;
                        if (crystalCriterion == null)
                        {
                            MessageBox.Show(SysConfig.crystal.crystalItem.ToString() + " " + (int)SysConfig.crystal.ch + " get criterion of crystal calibration fail!");
                            return;
                        }
                        rtwTestPlanList.Add(new CrystalCalibPlan(new RtwTx((int)SysConfig.crystal.ch, BW.B_40MHZ, ANT_PATH.PATH_A, RATE_ID.HTMCS7, TX_MODE.PACKET_TX), crystalCriterion));
                        rtwTestFunctionList.Add(CrystalCalib);
                        break;
                    default:
                        MessageBox.Show("Pls add the one 'HTMCS7-40M' crystal calibration test!");
                        return;
                }

                // ------------------ Tx Index Calib ------------------ //

                // ANTA
                tx.antPath = ANT_PATH.PATH_A;
                tx.txMode = TX_MODE.PACKET_TX;
                for (int i = 0; i < SysConfig.calibitems.Count; i++)
                {
                    // Criterion
                    calibCriterion = SysConfig.GetTxCalibCriterion(SysConfig.calibitems[i].calibItem);
                    if (calibCriterion == null)
                    {
                        MessageBox.Show(SysConfig.calibitems[i].calibItem.ToString() + " " + (int)SysConfig.calibitems[i].ch + " get criterion of tx calibration fail!");
                        return;
                    }
                    // Set Tx variable
                    tx.freq = (int)SysConfig.calibitems[i].ch;
                    switch (SysConfig.calibitems[i].calibItem)
                    {
                        case CalibItem.MCS7_B40:
                            tx.rateID = RATE_ID.HTMCS7;
                            tx.bw = BW.B_40MHZ;
                            break;
                        case CalibItem.CCK_11M:
                            tx.rateID = RATE_ID.R_11M;
                            tx.bw = BW.B_20MHZ;
                            break;
                        default:
                            continue;
                    }
                    rtwTestPlanList.Add(new TxCalibPlan(new byte[] { SysConfig.calibitems[i].defaultPower0, SysConfig.calibitems[i].defaultPower1 }, tx, calibCriterion));
                    rtwTestFunctionList.Add(TxIndexCalib);
                }

                // record thermal A
                rtwTestPlanList.Add(new RtwTx() { antPath = ANT_PATH.PATH_A });
                rtwTestFunctionList.Add(WriteTherValue);

                // ANTB
                tx.antPath = ANT_PATH.PATH_B;
                tx.txMode = TX_MODE.PACKET_TX;
                for (int i = 0; i < SysConfig.calibitems.Count; i++)
                {
                    // Criterion
                    calibCriterion = SysConfig.GetTxCalibCriterion(SysConfig.calibitems[i].calibItem);
                    if (calibCriterion == null)
                    {
                        MessageBox.Show(SysConfig.calibitems[i].calibItem.ToString() + " " + (int)SysConfig.calibitems[i].ch + " get criterion of tx calibration fail!");
                        return;
                    }
                    // Set Tx variable
                    tx.freq = (int)SysConfig.calibitems[i].ch;
                    switch (SysConfig.calibitems[i].calibItem)
                    {
                        case CalibItem.MCS7_B40:
                            tx.rateID = RATE_ID.HTMCS7;
                            tx.bw = BW.B_40MHZ;
                            break;
                        case CalibItem.CCK_11M:
                            tx.rateID = RATE_ID.R_11M;
                            tx.bw = BW.B_20MHZ;
                            break;
                        default:
                            continue;
                    }
                    rtwTestPlanList.Add(new TxCalibPlan(new byte[] { SysConfig.calibitems[i].defaultPower0, SysConfig.calibitems[i].defaultPower1 }, tx, calibCriterion));
                    rtwTestFunctionList.Add(TxIndexCalib);
                }

                // record thermal B
                rtwTestPlanList.Add(new RtwTx() { antPath = ANT_PATH.PATH_B });
                rtwTestFunctionList.Add(WriteTherValue);

                // rest of data populated by interpolation
                rtwTestPlanList.Add(new RtwTx());
                rtwTestFunctionList.Add(InterpolationOfTxIndex);

                rtwTestPlanList.Add(new RtwTx());
                rtwTestFunctionList.Add(WriteMap);
            }

            if(SysConfig.testItems.Contains("TX"))
            {
                // ------------------ Tx Verification ------------------ //
#if true
                // A - A ... B - B ...
                if (SysConfig.antTestOnly.Contains("A"))
                {
                    // ANT A
                    for (int i = 0; i < SysConfig.txItems.Count; i++)
                    {
                        // Criterion
                        txCriterion = SysConfig.GetTxVerifyCriterion(SysConfig.txItems[i].verifyItem, SysConfig.txItems[i].ch);
                        if (txCriterion == null)
                        {
                            MessageBox.Show(SysConfig.txItems[i].verifyItem.ToString() + " " + (int)SysConfig.txItems[i].ch + " get criterion of tx verification fail!");
                            return;
                        }
                        AddTxVerifyItem(TX_MODE.PACKET_TX, SysConfig.txItems[i].ch, ANT_PATH.PATH_A, SysConfig.txItems[i].verifyItem, txCriterion);
                    }
                }

                if (SysConfig.antTestOnly.Contains("B"))
                {
                    // ANT B
                    for (int i = 0; i < SysConfig.txItems.Count; i++)
                    {
                        // Criterion
                        txCriterion = SysConfig.GetTxVerifyCriterion(SysConfig.txItems[i].verifyItem, SysConfig.txItems[i].ch);
                        if (txCriterion == null)
                        {
                            MessageBox.Show(SysConfig.txItems[i].verifyItem.ToString() + " " + (int)SysConfig.txItems[i].ch + " get criterion of tx verification fail!");
                            return;
                        }
                        AddTxVerifyItem(TX_MODE.PACKET_TX, SysConfig.txItems[i].ch, ANT_PATH.PATH_B, SysConfig.txItems[i].verifyItem, txCriterion);
                    }
                }
#else
                // A - B - A - B ...
                for (int i = 0; i < SysConfig.txItems.Count; i++)
                {
                    // Criterion
                    txCriterion = SysConfig.GetTxVerifyCriterion(SysConfig.txItems[i].verifyItem, SysConfig.txItems[i].ch);
                    if (txCriterion == null)
                    {
                        MessageBox.Show(SysConfig.txItems[i].verifyItem.ToString() + " " + (int)SysConfig.txItems[i].ch + " get criterion of tx verification fail!");
                        return;
                    }
                    AddTxVerifyItem(TX_MODE.PACKET_TX, SysConfig.txItems[i].ch, ANT_PATH.PATH_A, SysConfig.txItems[i].verifyItem, txCriterion);
                    AddTxVerifyItem(TX_MODE.PACKET_TX, SysConfig.txItems[i].ch, ANT_PATH.PATH_B, SysConfig.txItems[i].verifyItem, txCriterion);
                }
#endif
            }
            
            if (SysConfig.testItems.Contains("RX"))
            {
                // ------------------ Rx Verification ------------------ //
                // A - A ... B - B ...
                if (SysConfig.antTestOnly.Contains("A"))
                {
                    // ANT A
                    for (int i = 0; i < SysConfig.rxItems.Count; i++)
                    {
                        // Criterion
                        rxCriterion = SysConfig.GetRxVerifyCriterion(SysConfig.rxItems[i].verifyItem, SysConfig.rxItems[i].ch);
                        if (rxCriterion == null)
                        {
                            MessageBox.Show(SysConfig.rxItems[i].verifyItem.ToString() + " " + (int)SysConfig.rxItems[i].ch + " get criterion of rx verification fail!");
                            return;
                        }
                        AddRxVerifyItem(SysConfig.rxItems[i].ch, ANT_PATH.PATH_A, SysConfig.rxItems[i].verifyItem, rxCriterion);
                    }

                }

                if (SysConfig.antTestOnly.Contains("B"))
                {
                    // ANT B
                    for (int i = 0; i < SysConfig.rxItems.Count; i++)
                    {
                        // Criterion
                        rxCriterion = SysConfig.GetRxVerifyCriterion(SysConfig.rxItems[i].verifyItem, SysConfig.rxItems[i].ch);
                        if (rxCriterion == null)
                        {
                            MessageBox.Show(SysConfig.rxItems[i].verifyItem.ToString() + " " + (int)SysConfig.rxItems[i].ch + " get criterion of rx verification fail!");
                            return;
                        }
                        AddRxVerifyItem(SysConfig.rxItems[i].ch, ANT_PATH.PATH_B, SysConfig.rxItems[i].verifyItem, rxCriterion);
                    }
                }
            }
        }
    }
}