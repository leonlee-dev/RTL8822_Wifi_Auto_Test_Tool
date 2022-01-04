using RTKModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public enum TestResult
    {
        TEST_SUCCESS = 0,
        TEST_FAILURE = 1,
    }

    public enum TestStatus
    {
        TEST_IDLE = 0,
        TEST_INIT = 1,
        TEST_START = 2,
        TEST_BEFORE_RETRY = 3,
        TEST_END = 99
    }

    public enum CrystalItem
    {
        MCS7_B40
    }

    public enum CalibItem
    {
        CCK_11M,
        MCS7_B40
    }

    public enum VerifyItem
    {
        DSSS_1M,
        DSSS_2M,
        CCK_5_5M,
        CCK_11M,
        OFDM_6M,
        OFDM_9M,
        OFDM_12M,
        OFDM_18M,
        OFDM_24M,
        OFDM_36M,
        OFDM_48M,
        OFDM_54M,
        MCS0_B20,
        MCS1_B20,
        MCS2_B20,
        MCS3_B20,
        MCS4_B20,
        MCS5_B20,
        MCS6_B20,
        MCS7_B20,
        MCS8_B20,
        MCS9_B20,
        MCS0_B40,
        MCS1_B40,
        MCS2_B40,
        MCS3_B40,
        MCS4_B40,
        MCS5_B40,
        MCS6_B40,
        MCS7_B40,
        MCS8_B40,
        MCS9_B40,
        MCS0_NSS1_B20,
        MCS1_NSS1_B20,
        MCS2_NSS1_B20,
        MCS3_NSS1_B20,
        MCS4_NSS1_B20,
        MCS5_NSS1_B20,
        MCS6_NSS1_B20,
        MCS7_NSS1_B20,
        MCS8_NSS1_B20,
        MCS9_NSS1_B20,
        MCS0_NSS1_B40,
        MCS1_NSS1_B40,
        MCS2_NSS1_B40,
        MCS3_NSS1_B40,
        MCS4_NSS1_B40,
        MCS5_NSS1_B40,
        MCS6_NSS1_B40,
        MCS7_NSS1_B40,
        MCS8_NSS1_B40,
        MCS9_NSS1_B40,
        MCS0_NSS1_B80,
        MCS1_NSS1_B80,
        MCS2_NSS1_B80,
        MCS3_NSS1_B80,
        MCS4_NSS1_B80,
        MCS5_NSS1_B80,
        MCS6_NSS1_B80,
        MCS7_NSS1_B80,
        MCS8_NSS1_B80,
        MCS9_NSS1_B80,
        MCS7_NSS2_B80, 
        MCS9_NSS2_B80,
        MCS15_B40
    }

    public class TestMap
    {
        public readonly static Dictionary<string, VerifyItem> testItemVersusVerifyItemDic = new Dictionary<string, VerifyItem>()
        {
            { "OFDM-54M", VerifyItem.OFDM_54M },
            { "CCK-11M" , VerifyItem.CCK_11M },
            { "MCS0-B20", VerifyItem.MCS0_B20 },
            { "MCS1-B20", VerifyItem.MCS1_B20 },
            { "MCS2-B20", VerifyItem.MCS2_B20 },
            { "MCS3-B20", VerifyItem.MCS3_B20 },
            { "MCS4-B20", VerifyItem.MCS4_B20 },
            { "MCS5-B20", VerifyItem.MCS5_B20 },
            { "MCS6-B20", VerifyItem.MCS6_B20 },
            { "MCS7-B20", VerifyItem.MCS7_B20 },
            { "MCS8-B20", VerifyItem.MCS8_B20 },
            { "MCS9-B20", VerifyItem.MCS9_B20 },
            { "MCS0-B40", VerifyItem.MCS0_B40 },
            { "MCS1-B40", VerifyItem.MCS1_B40 },
            { "MCS2-B40", VerifyItem.MCS2_B40 },
            { "MCS3-B40", VerifyItem.MCS3_B40 },
            { "MCS4-B40", VerifyItem.MCS4_B40 },
            { "MCS5-B40", VerifyItem.MCS5_B40 },
            { "MCS6-B40", VerifyItem.MCS6_B40 },
            { "MCS7-B40", VerifyItem.MCS7_B40 },
            { "MCS8-B40", VerifyItem.MCS8_B40 },
            { "MCS9-B40", VerifyItem.MCS9_B40 },
            { "MCS0-NSS1-B20", VerifyItem.MCS0_NSS1_B20 },
            { "MCS1-NSS1-B20", VerifyItem.MCS1_NSS1_B20 },
            { "MCS2-NSS1-B20", VerifyItem.MCS2_NSS1_B20 },
            { "MCS3-NSS1-B20", VerifyItem.MCS3_NSS1_B20 },
            { "MCS4-NSS1-B20", VerifyItem.MCS4_NSS1_B20 },
            { "MCS5-NSS1-B20", VerifyItem.MCS5_NSS1_B20 },
            { "MCS6-NSS1-B20", VerifyItem.MCS6_NSS1_B20 },
            { "MCS7-NSS1-B20", VerifyItem.MCS7_NSS1_B20 },
            { "MCS8-NSS1-B20", VerifyItem.MCS8_NSS1_B20 },
            { "MCS9-NSS1-B20", VerifyItem.MCS9_NSS1_B20 },
            { "MCS0-NSS1-B40", VerifyItem.MCS0_NSS1_B40 },
            { "MCS1-NSS1-B40", VerifyItem.MCS1_NSS1_B40 },
            { "MCS2-NSS1-B40", VerifyItem.MCS2_NSS1_B40 },
            { "MCS3-NSS1-B40", VerifyItem.MCS3_NSS1_B40 },
            { "MCS4-NSS1-B40", VerifyItem.MCS4_NSS1_B40 },
            { "MCS5-NSS1-B40", VerifyItem.MCS5_NSS1_B40 },
            { "MCS6-NSS1-B40", VerifyItem.MCS6_NSS1_B40 },
            { "MCS7-NSS1-B40", VerifyItem.MCS7_NSS1_B40 },
            { "MCS8-NSS1-B40", VerifyItem.MCS8_NSS1_B40 },
            { "MCS9-NSS1-B40", VerifyItem.MCS9_NSS1_B40 },
            { "MCS0-NSS1-B80", VerifyItem.MCS0_NSS1_B80 },
            { "MCS1-NSS1-B80", VerifyItem.MCS1_NSS1_B80 },
            { "MCS2-NSS1-B80", VerifyItem.MCS2_NSS1_B80 },
            { "MCS3-NSS1-B80", VerifyItem.MCS3_NSS1_B80 },
            { "MCS4-NSS1-B80", VerifyItem.MCS4_NSS1_B80 },
            { "MCS5-NSS1-B80", VerifyItem.MCS5_NSS1_B80 },
            { "MCS6-NSS1-B80", VerifyItem.MCS6_NSS1_B80 },
            { "MCS7-NSS1-B80", VerifyItem.MCS7_NSS1_B80 },
            { "MCS8-NSS1-B80", VerifyItem.MCS8_NSS1_B80 },
            { "MCS9-NSS1-B80", VerifyItem.MCS9_NSS1_B80 },
            { "MCS7-NSS2-B80", VerifyItem.MCS7_NSS2_B80 },
            { "MCS9-NSS2-B80", VerifyItem.MCS9_NSS2_B80 },
            { "MCS15-B40", VerifyItem.MCS15_B40 },
        };
    }

    //public enum WIFI_MODE
    //{
    //    CCK,
    //    W_11AG,
    //    W_11N,
    //    W_11AC
    //}
}