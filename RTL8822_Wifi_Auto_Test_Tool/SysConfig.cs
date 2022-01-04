using RTKModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public struct Cableloss
    {
        public int freq;
        public double chain1;
        public double chain2;
        public double chain3;
        public double chain4;
    }

    public class CrystalTest
    {
        public CrystalItem crystalItem;
        public CH ch;
    }

    public class CalibTest
    {
        public CalibItem calibItem;
        public CH ch;
        public byte defaultPower0; // default output power level
        public byte defaultPower1; // default output power level
    }

    public class VerifyTest
    {
        public VerifyItem verifyItem;
        public CH ch;
    }

    public class CrystalCriterion
    {
        public int freqErrUpper;
        public int freqErrLower;
    }

    public class CalibCriterion
    {
        public double targetPower;
        public double powerUpper;
        public double powerLower;
        public int freqErrUpper;
        public int freqErrLower;
        public int evm;
    }

    public class TxCriterion
    {
        public double targetPower;
        public double powerUpper;
        public double powerLower;
        public int evm;
        public int freqErrUpper;
        public int freqErrLower;
        public int leakage;
        public double flatness;
        public double mask;
    }

    public class RxCriterion
    {
        public double rxPower;
        public int per;
    }

    public class CrystalCalibCriterion
    {
        public CrystalItem crystalItem;
        public CrystalCriterion crystalCriterion;
    }

    public class TxCalibCriterion
    {
        public CalibItem calibItem;
        public CalibCriterion calibCriterion;
    }

    public class TxVerifyCriterion
    {
        public VerifyItem verifyItem;
        public int band;
        public TxCriterion verifyCriterion;
    }

    public class RxVerifyCriterion
    {
        public VerifyItem verifyItem;
        public int band;
        public RxCriterion verifyCriterion;
    }

    public class SysConfig
    {
        public readonly static string configPath = Application.StartupPath + "\\Setup\\config.txt";
        public readonly static string wifiPath = Application.StartupPath + "\\Setup\\wifi_plan.txt";
        public readonly static string cablelossPath = Application.StartupPath + "\\Setup\\iq_atten.txt";

        
        public static string platform;
        public static string connectionInterface;
        public static string drivFile;
        public static string drivDir;
        public static bool powerTracking;
        public static int delayForCaptureMs;
        public static int targetThermal;
        public static int thermalWaitingTime;
        public static bool powerByRate;
        public static bool powerLimit;
        public static string antTestOnly;

        public static Cableloss[] cableloss;

        public static List<string> testItems = new List<string>();

        public static CrystalTest crystal = new CrystalTest();
        public static List<CalibTest> calibitems = new List<CalibTest>();
        public static List<VerifyTest> txItems = new List<VerifyTest>();
        public static List<VerifyTest> rxItems = new List<VerifyTest>();

        public static CrystalCalibCriterion crystalCalibCriterion = new CrystalCalibCriterion();
        public static List<TxCalibCriterion> txCalibCritera = new List<TxCalibCriterion>();
        public static List<TxVerifyCriterion> txVerifyCritera = new List<TxVerifyCriterion>();
        public static List<RxVerifyCriterion> rxVerifyCritera = new List<RxVerifyCriterion>();

        public static CalibCriterion GetTxCalibCriterion(CalibItem calibItem)
        {
            for (int i = 0; i < txCalibCritera.Count; i++)
            {
                if (txCalibCritera[i].calibItem == calibItem)
                {
                    return txCalibCritera[i].calibCriterion;
                }
            }
            return null;
        }

        public static TxCriterion GetTxVerifyCriterion(VerifyItem verifyItem, CH ch)
        {
            for (int i = 0; i < txVerifyCritera.Count; i++)
            {
                if (txVerifyCritera[i].verifyItem == verifyItem)
                {
                    int band = (int)Math.Round((double)ch / 1000);
                    band = (band > 5) ? 5: band;
                    if(txVerifyCritera[i].band == band)
                        return txVerifyCritera[i].verifyCriterion;
                }
            }
            return null;
        }

        public static RxCriterion GetRxVerifyCriterion(VerifyItem verifyItem, CH ch)
        {
            for (int i = 0; i < rxVerifyCritera.Count; i++)
            {
                if (rxVerifyCritera[i].verifyItem == verifyItem)
                {
                    int band = (int)Math.Round((double)ch / 1000);
                    band = (band > 5) ? 5 : band;
                    if (rxVerifyCritera[i].band == band)
                        return rxVerifyCritera[i].verifyCriterion;
                }
            }
            return null;
        }

        public static double GetCableLoss(int center_freq, int ant)
        {
            if (SysConfig.cableloss == null)
                return -99999;

            double loss = 0.0;
            int len = SysConfig.cableloss.Length;
            for (int i = 0; i < len; i++)
            {
                if (center_freq == SysConfig.cableloss[i].freq || (i + 1 != len && SysConfig.cableloss[i].freq < center_freq && SysConfig.cableloss[i + 1].freq > center_freq) || i + 1 == len)
                {
                    switch (ant)
                    {
                        case 0:
                            loss = SysConfig.cableloss[i].chain1;
                            break;
                        case 1:
                            loss = SysConfig.cableloss[i].chain2;
                            break;
                        case 2:
                            loss = SysConfig.cableloss[i].chain3;
                            break;
                        case 3:
                            loss = SysConfig.cableloss[i].chain4;
                            break;
                        default:
                            loss = SysConfig.cableloss[i].chain1;
                            break;
                    }
                    break;
                }
            }
            return loss;
        }

        public static void LoadWifiSet()
        {
            using (StreamReader sr = new StreamReader(wifiPath))
            {
                PeekableStreamReader psr = new PeekableStreamReader(sr);

                while(!psr.EndOfStream)
                {
                    string line = psr.ReadLine();
                    if(line != null)
                    {
                        if (line.Contains("**CRYSTAL**"))
                        {
                            try
                            {
                                while ((line = psr.PeekLine()) != null && !line.StartsWith("**"))
                                {
                                    psr.ReadLine();
                                    if (string.IsNullOrEmpty(line.Trim()))
                                        continue;

                                    if (line.Contains("*MCS7-B40"))
                                        continue;
                                    else if (line.StartsWith("*"))
                                        continue;

                                    string value = line.Split('=')[1];
                                    string testItem = value.Substring(1, value.Length - 2).Split(',')[0];
                                    crystal.crystalItem = CrystalItem.MCS7_B40;
                                    crystal.ch = Wifi.chDic.First(c => c.Value == testItem).Key;
                                }
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show("Crystal setting error: " + ex.Message);
                            }
                        }
                        else if (line.Contains("**CALIB**"))
                        {
                            try
                            {
                                int calibItem = 0;
                                while ((line = psr.PeekLine()) != null && !line.StartsWith("**"))
                                {
                                    psr.ReadLine();
                                    if (string.IsNullOrEmpty(line.Trim()))
                                        continue;

                                    if (line.Contains("*MCS7-B40"))
                                    {
                                        calibItem = 1;
                                        continue;
                                    }
                                    else if (line.Contains("*CCK-11M"))
                                    {
                                        calibItem = 2;
                                        continue;
                                    }
                                    else if (line.StartsWith("*"))
                                        continue;

                                    string[] values = line.Split('=')[1].Split(';');
                                    string[] testItems = values[0].Substring(1, values[0].Length - 2).Split(',');
                                    string[] power0 = values[1].Substring(1, values[1].Length - 2).Split(',');
                                    string[] power1 = values[2].Substring(1, values[2].Length - 2).Split(',');
                                    for (int i = 0; i < testItems.Length; i++)
                                    {
                                        CH ch = Wifi.chDic.First(c => c.Value == testItems[i]).Key;

                                        byte defaultPower0 = byte.Parse(power0[i].Substring(2), System.Globalization.NumberStyles.HexNumber);
                                        byte defaultPower1 = byte.Parse(power1[i].Substring(2), System.Globalization.NumberStyles.HexNumber);
                                        if (calibItem == 1)
                                            calibitems.Add(new CalibTest() { calibItem = CalibItem.MCS7_B40, ch = ch , defaultPower0 = defaultPower0, defaultPower1 = defaultPower1 });
                                        else if (calibItem == 2)
                                            calibitems.Add(new CalibTest() { calibItem = CalibItem.CCK_11M, ch = ch, defaultPower0 = defaultPower0, defaultPower1 = defaultPower1 });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Calibration setting error: " + ex.Message);
                            }
                        }
                        else if (line.Contains("**TX_VERIFY**"))
                        {
                            try
                            {
                                int verifyItem = 0;
                                while ((line = psr.PeekLine()) != null && !line.StartsWith("**"))
                                {
                                    psr.ReadLine();
                                    if (string.IsNullOrEmpty(line.Trim()))
                                        continue;

                                    if (line.Contains("*MCS0-B20"))
                                    {
                                        verifyItem = 1;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-B20"))
                                    {
                                        verifyItem = 2;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-B20"))
                                    {
                                        verifyItem = 3;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-B20"))
                                    {
                                        verifyItem = 4;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-B20"))
                                    {
                                        verifyItem = 5;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-B20"))
                                    {
                                        verifyItem = 6;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-B20"))
                                    {
                                        verifyItem = 7;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-B20"))
                                    {
                                        verifyItem = 8;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-B20"))
                                    {
                                        verifyItem = 9;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-B20"))
                                    {
                                        verifyItem = 10;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-B40"))
                                    {
                                        verifyItem = 11;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-B40"))
                                    {
                                        verifyItem = 12;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-B40"))
                                    {
                                        verifyItem = 13;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-B40"))
                                    {
                                        verifyItem = 14;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-B40"))
                                    {
                                        verifyItem = 15;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-B40"))
                                    {
                                        verifyItem = 16;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-B40"))
                                    {
                                        verifyItem = 17;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-B40"))
                                    {
                                        verifyItem = 18;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-B40"))
                                    {
                                        verifyItem = 19;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-B40"))
                                    {
                                        verifyItem = 20;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-NSS1-B20"))
                                    {
                                        verifyItem = 21;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-NSS1-B20"))
                                    {
                                        verifyItem = 22;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-NSS1-B20"))
                                    {
                                        verifyItem = 23;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-NSS1-B20"))
                                    {
                                        verifyItem = 24;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-NSS1-B20"))
                                    {
                                        verifyItem = 25;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-NSS1-B20"))
                                    {
                                        verifyItem = 26;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-NSS1-B20"))
                                    {
                                        verifyItem = 27;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-NSS1-B20"))
                                    {
                                        verifyItem = 28;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-NSS1-B20"))
                                    {
                                        verifyItem = 29;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-NSS1-B20"))
                                    {
                                        verifyItem = 30;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-NSS1-B40"))
                                    {
                                        verifyItem = 31;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-NSS1-B40"))
                                    {
                                        verifyItem = 32;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-NSS1-B40"))
                                    {
                                        verifyItem = 33;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-NSS1-B40"))
                                    {
                                        verifyItem = 34;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-NSS1-B40"))
                                    {
                                        verifyItem = 35;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-NSS1-B40"))
                                    {
                                        verifyItem = 36;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-NSS1-B40"))
                                    {
                                        verifyItem = 37;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-NSS1-B40"))
                                    {
                                        verifyItem = 38;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-NSS1-B40"))
                                    {
                                        verifyItem = 39;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-NSS1-B40"))
                                    {
                                        verifyItem = 40;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-NSS1-B80"))
                                    {
                                        verifyItem = 41;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-NSS1-B80"))
                                    {
                                        verifyItem = 42;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-NSS1-B80"))
                                    {
                                        verifyItem = 43;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-NSS1-B80"))
                                    {
                                        verifyItem = 44;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-NSS1-B80"))
                                    {
                                        verifyItem = 45;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-NSS1-B80"))
                                    {
                                        verifyItem = 46;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-NSS1-B80"))
                                    {
                                        verifyItem = 47;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-NSS1-B80"))
                                    {
                                        verifyItem = 48;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-NSS1-B80"))
                                    {
                                        verifyItem = 49;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-NSS1-B80"))
                                    {
                                        verifyItem = 50;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-6M"))
                                    {
                                        verifyItem = 51;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-9M"))
                                    {
                                        verifyItem = 52;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-12M"))
                                    {
                                        verifyItem = 53;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-18M"))
                                    {
                                        verifyItem = 54;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-24M"))
                                    {
                                        verifyItem = 55;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-36M"))
                                    {
                                        verifyItem = 56;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-48M"))
                                    {
                                        verifyItem = 57;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-54M"))
                                    {
                                        verifyItem = 58;
                                        continue;
                                    }
                                    else if (line.Contains("*DSSS-1M"))
                                    {
                                        verifyItem = 59;
                                        continue;
                                    }
                                    else if (line.Contains("*DSSS-2M"))
                                    {
                                        verifyItem = 60;
                                        continue;
                                    }
                                    else if (line.Contains("*CCK-5_5M"))
                                    {
                                        verifyItem = 61;
                                        continue;
                                    }
                                    else if (line.Contains("*CCK-11M"))
                                    {
                                        verifyItem = 62;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-NSS2-B80"))
                                    {
                                        verifyItem = 63;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS15-B40"))
                                    {
                                        verifyItem = 64;
                                        continue;
                                    }
                                    else if (line.StartsWith("*"))
                                        continue;
  
                                    string value = line.Split('=')[1];
                                    string[] testItems = value.Substring(1, value.Length - 2).Split(',');

                                    for (int i = 0; i < testItems.Length; i++)
                                    {
                                        CH ch = Wifi.chDic.First(c => c.Value == testItems[i]).Key;

                                        if (verifyItem == 1)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_B20, ch = ch });
                                        else if (verifyItem == 2)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_B20, ch = ch });
                                        else if (verifyItem == 3)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_B20, ch = ch });
                                        else if (verifyItem == 4)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_B20, ch = ch });
                                        else if (verifyItem == 5)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_B20, ch = ch });
                                        else if (verifyItem == 6)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_B20, ch = ch });
                                        else if (verifyItem == 7)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_B20, ch = ch });
                                        else if (verifyItem == 8)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_B20, ch = ch });
                                        else if (verifyItem == 9)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_B20, ch = ch });
                                        else if (verifyItem == 10)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_B20, ch = ch });
                                        else if (verifyItem == 11)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_B40, ch = ch });
                                        else if (verifyItem == 12)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_B40, ch = ch });
                                        else if (verifyItem == 13)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_B40, ch = ch });
                                        else if (verifyItem == 14)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_B40, ch = ch });
                                        else if (verifyItem == 15)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_B40, ch = ch });
                                        else if (verifyItem == 16)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_B40, ch = ch });
                                        else if (verifyItem == 17)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_B40, ch = ch });
                                        else if (verifyItem == 18)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_B40, ch = ch });
                                        else if (verifyItem == 19)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_B40, ch = ch });
                                        else if (verifyItem == 20)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_B40, ch = ch });
                                        else if (verifyItem == 21)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_NSS1_B20, ch = ch });
                                        else if (verifyItem == 22)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_NSS1_B20, ch = ch });
                                        else if (verifyItem == 23)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_NSS1_B20, ch = ch });
                                        else if (verifyItem == 24)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_NSS1_B20, ch = ch });
                                        else if (verifyItem == 25)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_NSS1_B20, ch = ch });
                                        else if (verifyItem == 26)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_NSS1_B20, ch = ch });
                                        else if (verifyItem == 27)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_NSS1_B20, ch = ch });
                                        else if (verifyItem == 28)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_NSS1_B20, ch = ch });
                                        else if (verifyItem == 29)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_NSS1_B20, ch = ch });
                                        else if (verifyItem == 30)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_NSS1_B20, ch = ch });
                                        else if (verifyItem == 31)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_NSS1_B40, ch = ch });
                                        else if (verifyItem == 32)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_NSS1_B40, ch = ch });
                                        else if (verifyItem == 33)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_NSS1_B40, ch = ch });
                                        else if (verifyItem == 34)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_NSS1_B40, ch = ch });
                                        else if (verifyItem == 35)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_NSS1_B40, ch = ch });
                                        else if (verifyItem == 36)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_NSS1_B40, ch = ch });
                                        else if (verifyItem == 37)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_NSS1_B40, ch = ch });
                                        else if (verifyItem == 38)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_NSS1_B40, ch = ch });
                                        else if (verifyItem == 39)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_NSS1_B40, ch = ch });
                                        else if (verifyItem == 40)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_NSS1_B40, ch = ch });
                                        else if (verifyItem == 41)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_NSS1_B80, ch = ch });
                                        else if (verifyItem == 42)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_NSS1_B80, ch = ch });
                                        else if (verifyItem == 43)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_NSS1_B80, ch = ch });
                                        else if (verifyItem == 44)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_NSS1_B80, ch = ch });
                                        else if (verifyItem == 45)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_NSS1_B80, ch = ch });
                                        else if (verifyItem == 46)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_NSS1_B80, ch = ch });
                                        else if (verifyItem == 47)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_NSS1_B80, ch = ch });
                                        else if (verifyItem == 48)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_NSS1_B80, ch = ch });
                                        else if (verifyItem == 49)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_NSS1_B80, ch = ch });
                                        else if (verifyItem == 50)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_NSS1_B80, ch = ch });
                                        else if (verifyItem == 51)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_6M, ch = ch });
                                        else if (verifyItem == 52)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_9M, ch = ch });
                                        else if (verifyItem == 53)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_12M, ch = ch });
                                        else if (verifyItem == 54)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_18M, ch = ch });
                                        else if (verifyItem == 55)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_24M, ch = ch });
                                        else if (verifyItem == 56)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_36M, ch = ch });
                                        else if (verifyItem == 57)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_48M, ch = ch });
                                        else if (verifyItem == 58)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_54M, ch = ch });
                                        else if (verifyItem == 59)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.DSSS_1M, ch = ch });
                                        else if (verifyItem == 60)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.DSSS_2M, ch = ch });
                                        else if (verifyItem == 61)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.CCK_5_5M, ch = ch });
                                        else if (verifyItem == 62)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.CCK_11M, ch = ch });
                                        else if (verifyItem == 63)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_NSS2_B80, ch = ch });
                                        else if (verifyItem == 64)
                                            txItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS15_B40, ch = ch });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Tx verification setting error: " + ex.Message);
                            }
                        }
                        else if (line.Contains("**RX_VERIFY**"))
                        {
                            try
                            {
                                int verifyItem = 0;
                                while ((line = psr.PeekLine()) != null && !line.StartsWith("**"))
                                {
                                    psr.ReadLine();
                                    if (string.IsNullOrEmpty(line.Trim()))
                                        continue;

                                    if (line.Contains("*MCS0-B20"))
                                    {
                                        verifyItem = 1;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-B20"))
                                    {
                                        verifyItem = 2;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-B20"))
                                    {
                                        verifyItem = 3;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-B20"))
                                    {
                                        verifyItem = 4;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-B20"))
                                    {
                                        verifyItem = 5;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-B20"))
                                    {
                                        verifyItem = 6;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-B20"))
                                    {
                                        verifyItem = 7;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-B20"))
                                    {
                                        verifyItem = 8;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-B20"))
                                    {
                                        verifyItem = 9;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-B20"))
                                    {
                                        verifyItem = 10;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-B40"))
                                    {
                                        verifyItem = 11;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-B40"))
                                    {
                                        verifyItem = 12;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-B40"))
                                    {
                                        verifyItem = 13;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-B40"))
                                    {
                                        verifyItem = 14;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-B40"))
                                    {
                                        verifyItem = 15;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-B40"))
                                    {
                                        verifyItem = 16;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-B40"))
                                    {
                                        verifyItem = 17;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-B40"))
                                    {
                                        verifyItem = 18;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-B40"))
                                    {
                                        verifyItem = 19;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-B40"))
                                    {
                                        verifyItem = 20;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-NSS1-B20"))
                                    {
                                        verifyItem = 21;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-NSS1-B20"))
                                    {
                                        verifyItem = 22;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-NSS1-B20"))
                                    {
                                        verifyItem = 23;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-NSS1-B20"))
                                    {
                                        verifyItem = 24;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-NSS1-B20"))
                                    {
                                        verifyItem = 25;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-NSS1-B20"))
                                    {
                                        verifyItem = 26;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-NSS1-B20"))
                                    {
                                        verifyItem = 27;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-NSS1-B20"))
                                    {
                                        verifyItem = 28;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-NSS1-B20"))
                                    {
                                        verifyItem = 29;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-NSS1-B20"))
                                    {
                                        verifyItem = 30;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-NSS1-B40"))
                                    {
                                        verifyItem = 31;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-NSS1-B40"))
                                    {
                                        verifyItem = 32;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-NSS1-B40"))
                                    {
                                        verifyItem = 33;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-NSS1-B40"))
                                    {
                                        verifyItem = 34;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-NSS1-B40"))
                                    {
                                        verifyItem = 35;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-NSS1-B40"))
                                    {
                                        verifyItem = 36;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-NSS1-B40"))
                                    {
                                        verifyItem = 37;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-NSS1-B40"))
                                    {
                                        verifyItem = 38;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-NSS1-B40"))
                                    {
                                        verifyItem = 39;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-NSS1-B40"))
                                    {
                                        verifyItem = 40;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS0-NSS1-B80"))
                                    {
                                        verifyItem = 41;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS1-NSS1-B80"))
                                    {
                                        verifyItem = 42;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS2-NSS1-B80"))
                                    {
                                        verifyItem = 43;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS3-NSS1-B80"))
                                    {
                                        verifyItem = 44;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS4-NSS1-B80"))
                                    {
                                        verifyItem = 45;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS5-NSS1-B80"))
                                    {
                                        verifyItem = 46;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS6-NSS1-B80"))
                                    {
                                        verifyItem = 47;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS7-NSS1-B80"))
                                    {
                                        verifyItem = 48;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS8-NSS1-B80"))
                                    {
                                        verifyItem = 49;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-NSS1-B80"))
                                    {
                                        verifyItem = 50;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-6M"))
                                    {
                                        verifyItem = 51;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-9M"))
                                    {
                                        verifyItem = 52;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-12M"))
                                    {
                                        verifyItem = 53;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-18M"))
                                    {
                                        verifyItem = 54;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-24M"))
                                    {
                                        verifyItem = 55;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-36M"))
                                    {
                                        verifyItem = 56;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-48M"))
                                    {
                                        verifyItem = 57;
                                        continue;
                                    }
                                    else if (line.Contains("*OFDM-54M"))
                                    {
                                        verifyItem = 58;
                                        continue;
                                    }
                                    else if (line.Contains("*DSSS-1M"))
                                    {
                                        verifyItem = 59;
                                        continue;
                                    }
                                    else if (line.Contains("*DSSS-2M"))
                                    {
                                        verifyItem = 60;
                                        continue;
                                    }
                                    else if (line.Contains("*CCK-5_5M"))
                                    {
                                        verifyItem = 61;
                                        continue;
                                    }
                                    else if (line.Contains("*CCK-11M"))
                                    {
                                        verifyItem = 62;
                                        continue;
                                    }
                                    else if (line.Contains("*MCS9-NSS2-B80"))
                                    {
                                        verifyItem = 63;
                                        continue;
                                    }
                                    else if (line.StartsWith("*"))
                                        continue;

                                    string value = line.Split('=')[1];
                                    string[] testItems = value.Substring(1, value.Length - 2).Split(',');

                                    for (int i = 0; i < testItems.Length; i++)
                                    {
                                        CH ch = Wifi.chDic.First(c => c.Value == testItems[i]).Key;

                                        if (verifyItem == 1)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_B20, ch = ch });
                                        else if (verifyItem == 2)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_B20, ch = ch });
                                        else if (verifyItem == 3)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_B20, ch = ch });
                                        else if (verifyItem == 4)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_B20, ch = ch });
                                        else if (verifyItem == 5)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_B20, ch = ch });
                                        else if (verifyItem == 6)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_B20, ch = ch });
                                        else if (verifyItem == 7)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_B20, ch = ch });
                                        else if (verifyItem == 8)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_B20, ch = ch });
                                        else if (verifyItem == 9)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_B20, ch = ch });
                                        else if (verifyItem == 10)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_B20, ch = ch });
                                        else if (verifyItem == 11)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_B40, ch = ch });
                                        else if (verifyItem == 12)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_B40, ch = ch });
                                        else if (verifyItem == 13)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_B40, ch = ch });
                                        else if (verifyItem == 14)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_B40, ch = ch });
                                        else if (verifyItem == 15)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_B40, ch = ch });
                                        else if (verifyItem == 16)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_B40, ch = ch });
                                        else if (verifyItem == 17)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_B40, ch = ch });
                                        else if (verifyItem == 18)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_B40, ch = ch });
                                        else if (verifyItem == 19)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_B40, ch = ch });
                                        else if (verifyItem == 20)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_B40, ch = ch });
                                        else if (verifyItem == 21)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_NSS1_B20, ch = ch });
                                        else if (verifyItem == 22)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_NSS1_B20, ch = ch });
                                        else if (verifyItem == 23)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_NSS1_B20, ch = ch });
                                        else if (verifyItem == 24)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_NSS1_B20, ch = ch });
                                        else if (verifyItem == 25)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_NSS1_B20, ch = ch });
                                        else if (verifyItem == 26)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_NSS1_B20, ch = ch });
                                        else if (verifyItem == 27)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_NSS1_B20, ch = ch });
                                        else if (verifyItem == 28)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_NSS1_B20, ch = ch });
                                        else if (verifyItem == 29)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_NSS1_B20, ch = ch });
                                        else if (verifyItem == 30)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_NSS1_B20, ch = ch });
                                        else if (verifyItem == 31)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_NSS1_B40, ch = ch });
                                        else if (verifyItem == 32)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_NSS1_B40, ch = ch });
                                        else if (verifyItem == 33)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_NSS1_B40, ch = ch });
                                        else if (verifyItem == 34)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_NSS1_B40, ch = ch });
                                        else if (verifyItem == 35)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_NSS1_B40, ch = ch });
                                        else if (verifyItem == 36)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_NSS1_B40, ch = ch });
                                        else if (verifyItem == 37)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_NSS1_B40, ch = ch });
                                        else if (verifyItem == 38)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_NSS1_B40, ch = ch });
                                        else if (verifyItem == 39)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_NSS1_B40, ch = ch });
                                        else if (verifyItem == 40)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_NSS1_B40, ch = ch });
                                        else if (verifyItem == 41)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS0_NSS1_B80, ch = ch });
                                        else if (verifyItem == 42)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS1_NSS1_B80, ch = ch });
                                        else if (verifyItem == 43)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS2_NSS1_B80, ch = ch });
                                        else if (verifyItem == 44)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS3_NSS1_B80, ch = ch });
                                        else if (verifyItem == 45)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS4_NSS1_B80, ch = ch });
                                        else if (verifyItem == 46)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS5_NSS1_B80, ch = ch });
                                        else if (verifyItem == 47)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS6_NSS1_B80, ch = ch });
                                        else if (verifyItem == 48)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS7_NSS1_B80, ch = ch });
                                        else if (verifyItem == 49)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS8_NSS1_B80, ch = ch });
                                        else if (verifyItem == 50)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_NSS1_B80, ch = ch });
                                        else if (verifyItem == 51)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_6M, ch = ch });
                                        else if (verifyItem == 52)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_9M, ch = ch });
                                        else if (verifyItem == 53)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_12M, ch = ch });
                                        else if (verifyItem == 54)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_18M, ch = ch });
                                        else if (verifyItem == 55)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_24M, ch = ch });
                                        else if (verifyItem == 56)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_36M, ch = ch });
                                        else if (verifyItem == 57)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_48M, ch = ch });
                                        else if (verifyItem == 58)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.OFDM_54M, ch = ch });
                                        else if (verifyItem == 59)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.DSSS_1M, ch = ch });
                                        else if (verifyItem == 60)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.DSSS_2M, ch = ch });
                                        else if (verifyItem == 61)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.CCK_5_5M, ch = ch });
                                        else if (verifyItem == 62)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.CCK_11M, ch = ch });
                                        else if (verifyItem == 63)
                                            rxItems.Add(new VerifyTest() { verifyItem = VerifyItem.MCS9_NSS2_B80, ch = ch });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Rx verification setting error: " + ex.Message);
                            }
                        }
                        else if (line.Contains("**CRITERION**"))
                        {
                            try
                            {
                                while ((line = psr.ReadLine()) != null)
                                {
                                    if (string.IsNullOrEmpty(line.Trim()))
                                        continue;

                                    if (line.Contains("**CRYSTAL**"))
                                    {
                                        int criterion = 0;
                                        CrystalCriterion crystalCriterion = new CrystalCriterion();
                                        while ((line = psr.PeekLine()) != null && !line.StartsWith("**"))
                                        {
                                            psr.ReadLine();
                                            if (string.IsNullOrEmpty(line.Trim()))
                                                continue;

                                            if (line.Contains("*MCS7-B40"))
                                            {
                                                criterion = 1;
                                                continue;
                                            }
                                            else if (line.StartsWith("*"))
                                                continue;

                                            string value = line.Split('=')[1].Trim();

                                            if (string.IsNullOrEmpty(value))
                                                continue;

                                            string[] limits = value.Split(',');

                                            if (limits == null)
                                                continue;

                                            if (line.Contains("FREQ_ERR"))
                                            {
                                                int upper = int.Parse(limits[0]);
                                                int lower = int.Parse(limits[1]);
                                                crystalCriterion.freqErrUpper = upper;
                                                crystalCriterion.freqErrLower = lower;
                                            }
                                        }

                                        if (criterion == 1)
                                        {
                                            crystalCalibCriterion.crystalItem = CrystalItem.MCS7_B40;
                                            crystalCalibCriterion.crystalCriterion = crystalCriterion;
                                        }  
                                    }
                                    else if (line.Contains("**2G_BAND_CALIB**")
                                          || line.Contains("**5G_BAND_CALIB**"))
                                    {
                                        int criterion = 0;
                                        CalibCriterion calibCriterion = new CalibCriterion();
                                        while ((line = psr.PeekLine()) != null && !line.StartsWith("**"))
                                        {
                                            psr.ReadLine();
                                            if (string.IsNullOrEmpty(line.Trim()))
                                                continue;

                                            if (line.Contains("*MCS7-B40"))
                                            {
                                                criterion = 1;
                                                continue;
                                            } 
                                            if (line.Contains("*CCK-11M"))
                                            {
                                                criterion = 2;
                                                continue;
                                            }
                                            else if (line.StartsWith("*"))
                                                continue;

                                            string value = line.Split('=')[1].Trim();

                                            if (string.IsNullOrEmpty(value))
                                                continue;

                                            string[] limits = value.Split(',');

                                            if (limits == null)
                                                continue;

                                            if (line.Contains("POWER"))
                                            {
                                                double target = double.Parse(limits[0]);
                                                double upper = double.Parse(limits[1]);
                                                double lower = double.Parse(limits[2]);
                                                calibCriterion.targetPower = target;
                                                calibCriterion.powerUpper = target + upper;
                                                calibCriterion.powerLower = target + lower;
                                            }
                                            else if (line.Contains("FREQ_ERR"))
                                            {
                                                int upper = int.Parse(limits[0]);
                                                int lower = int.Parse(limits[1]);
                                                calibCriterion.freqErrUpper = upper;
                                                calibCriterion.freqErrLower = lower;
                                            }
                                            else if (line.Contains("EVM"))
                                            {
                                                calibCriterion.evm = int.Parse(limits[0]);
                                            }
                                        }

                                        if (criterion == 1)
                                            txCalibCritera.Add(new TxCalibCriterion() { calibItem = CalibItem.MCS7_B40, calibCriterion = calibCriterion });
                                        else if(criterion == 2)
                                            txCalibCritera.Add(new TxCalibCriterion() { calibItem = CalibItem.CCK_11M, calibCriterion = calibCriterion });
                                    }             
                                    else if (line.Contains("**2G_BAND_VERIFY**")
                                          || line.Contains("**5G_BAND_VERIFY**"))
                                    {
                                        int band = 0;
                                        int criterion = 0;

                                        if (line.Contains("2G"))
                                            band = 2;
                                        else if (line.Contains("5G"))
                                            band = 5;

                                        //TxCriterion verifyCriterion;

                                        line = psr.ReadLine();
                                        while (line != null && !line.StartsWith("**"))
                                        {
                                            line = psr.ReadLine();
                                            if (string.IsNullOrEmpty(line.Trim()))
                                                continue;

                                            //// 11N MCS0-MCS7 BW20/40
                                            //for (int bw = 20; bw <= 40; bw *= 2)
                                            //{
                                            //    for (int mcs = 0; mcs < 10; mcs++)
                                            //    {
                                            //        string rateWithBw = "MCS" + mcs + "-B" + bw;
                                            //        if (line.Contains("*" + rateWithBw))
                                            //        {
                                            //            verifyCriterion = new TxCriterion();
                                            //            while ((line = psr.PeekLine()) != null && !line.StartsWith("*"))
                                            //            {
                                            //                psr.ReadLine();
                                            //                if (string.IsNullOrEmpty(line.Trim()))
                                            //                    continue;

                                            //                string value = line.Split('=')[1].Trim();

                                            //                if (string.IsNullOrEmpty(value))
                                            //                    continue;

                                            //                string[] limits = value.Split(',');

                                            //                if (limits == null)
                                            //                    continue;

                                            //                if (line.Contains("POWER"))
                                            //                {
                                            //                    double target = double.Parse(limits[0]);
                                            //                    double upper = double.Parse(limits[1]);
                                            //                    double lower = double.Parse(limits[2]);
                                            //                    verifyCriterion.targetPower = target;
                                            //                    verifyCriterion.powerUpper = target + upper;
                                            //                    verifyCriterion.powerLower = target + lower;
                                            //                }
                                            //                else if (line.Contains("EVM"))
                                            //                {
                                            //                    verifyCriterion.evm = int.Parse(limits[0]);
                                            //                }
                                            //                else if (line.Contains("FREQ_ERR"))
                                            //                {
                                            //                    int upper = int.Parse(limits[0]);
                                            //                    int lower = int.Parse(limits[1]);
                                            //                    verifyCriterion.freqErrUpper = upper;
                                            //                    verifyCriterion.freqErrLower = lower;
                                            //                }
                                            //                else if (line.Contains("LEAKAGE"))
                                            //                {
                                            //                    verifyCriterion.leakage = int.Parse(limits[0]);
                                            //                }
                                            //                else if (line.Contains("MASK"))
                                            //                {
                                            //                    verifyCriterion.mask = int.Parse(limits[0]);
                                            //                }
                                            //            }
                                            //            txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = TestMap.testItemVersusVerifyItemDic[rateWithBw], band = band, verifyCriterion = verifyCriterion });
                                            //        }
                                            //    }
                                            //}

                                            //verifyCriterion = new TxCriterion();
                                            //while ((line = psr.PeekLine()) != null && !line.StartsWith("*"))
                                            //{
                                            //    psr.ReadLine();
                                            //    if (string.IsNullOrEmpty(line.Trim()))
                                            //        continue;

                                            //    string value = line.Split('=')[1].Trim();

                                            //    if (string.IsNullOrEmpty(value))
                                            //        continue;

                                            //    string[] limits = value.Split(',');

                                            //    if (limits == null)
                                            //        continue;

                                            //    if (line.Contains("POWER"))
                                            //    {
                                            //        double target = double.Parse(limits[0]);
                                            //        double upper = double.Parse(limits[1]);
                                            //        double lower = double.Parse(limits[2]);
                                            //        verifyCriterion.targetPower = target;
                                            //        verifyCriterion.powerUpper = target + upper;
                                            //        verifyCriterion.powerLower = target + lower;
                                            //    }
                                            //    else if (line.Contains("EVM"))
                                            //    {
                                            //        verifyCriterion.evm = int.Parse(limits[0]);
                                            //    }
                                            //    else if (line.Contains("FREQ_ERR"))
                                            //    {
                                            //        int upper = int.Parse(limits[0]);
                                            //        int lower = int.Parse(limits[1]);
                                            //        verifyCriterion.freqErrUpper = upper;
                                            //        verifyCriterion.freqErrLower = lower;
                                            //    }
                                            //    else if (line.Contains("LEAKAGE"))
                                            //    {
                                            //        verifyCriterion.leakage = int.Parse(limits[0]);
                                            //    }
                                            //    else if (line.Contains("MASK"))
                                            //    {
                                            //        verifyCriterion.mask = int.Parse(limits[0]);
                                            //    }
                                            //}

                                            //if (line.Contains("OFDM-54M"))
                                            //    txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_54M, band = band, verifyCriterion = verifyCriterion });
                                            //else if (line.Contains("CCK-11M"))
                                            //    txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.CCK_11M, band = band, verifyCriterion = verifyCriterion });


                                            if (line.Contains("*MCS0-B20"))
                                                criterion = 1;
                                            else if (line.Contains("*MCS1-B20"))
                                                criterion = 2;
                                            else if (line.Contains("*MCS2-B20"))
                                                criterion = 3;
                                            else if (line.Contains("*MCS3-B20"))
                                                criterion = 4;
                                            else if (line.Contains("*MCS4-B20"))
                                                criterion = 5;
                                            else if (line.Contains("*MCS5-B20"))
                                                criterion = 6;
                                            else if (line.Contains("*MCS6-B20"))
                                                criterion = 7;
                                            else if (line.Contains("*MCS7-B20"))
                                                criterion = 8;
                                            else if (line.Contains("*MCS8-B20"))
                                                criterion = 9;
                                            else if (line.Contains("*MCS9-B20"))
                                                criterion = 10;
                                            else if (line.Contains("*MCS0-B40"))
                                                criterion = 11;
                                            else if (line.Contains("*MCS1-B40"))
                                                criterion = 12;
                                            else if (line.Contains("*MCS2-B40"))
                                                criterion = 13;
                                            else if (line.Contains("*MCS3-B40"))
                                                criterion = 14;
                                            else if (line.Contains("*MCS4-B40"))
                                                criterion = 15;
                                            else if (line.Contains("*MCS5-B40"))
                                                criterion = 16;
                                            else if (line.Contains("*MCS6-B40"))
                                                criterion = 17;
                                            else if (line.Contains("*MCS7-B40"))
                                                criterion = 18;
                                            else if (line.Contains("*MCS8-B40"))
                                                criterion = 19;
                                            else if (line.Contains("*MCS9-B40"))
                                                criterion = 20;
                                            else if (line.Contains("*MCS0-NSS1-B20"))
                                                criterion = 21;
                                            else if (line.Contains("*MCS1-NSS1-B20"))
                                                criterion = 22;
                                            else if (line.Contains("*MCS2-NSS1-B20"))
                                                criterion = 23;
                                            else if (line.Contains("*MCS3-NSS1-B20"))
                                                criterion = 24;
                                            else if (line.Contains("*MCS4-NSS1-B20"))
                                                criterion = 25;
                                            else if (line.Contains("*MCS5-NSS1-B20"))
                                                criterion = 26;
                                            else if (line.Contains("*MCS6-NSS1-B20"))
                                                criterion = 27;
                                            else if (line.Contains("*MCS7-NSS1-B20"))
                                                criterion = 28;
                                            else if (line.Contains("*MCS8-NSS1-B20"))
                                                criterion = 29;
                                            else if (line.Contains("*MCS9-NSS1-B20"))
                                                criterion = 30;
                                            else if (line.Contains("*MCS0-NSS1-B40"))
                                                criterion = 31;
                                            else if (line.Contains("*MCS1-NSS1-B40"))
                                                criterion = 32;
                                            else if (line.Contains("*MCS2-NSS1-B40"))
                                                criterion = 33;
                                            else if (line.Contains("*MCS3-NSS1-B40"))
                                                criterion = 34;
                                            else if (line.Contains("*MCS4-NSS1-B40"))
                                                criterion = 35;
                                            else if (line.Contains("*MCS5-NSS1-B40"))
                                                criterion = 36;
                                            else if (line.Contains("*MCS6-NSS1-B40"))
                                                criterion = 37;
                                            else if (line.Contains("*MCS7-NSS1-B40"))
                                                criterion = 38;
                                            else if (line.Contains("*MCS8-NSS1-B40"))
                                                criterion = 39;
                                            else if (line.Contains("*MCS9-NSS1-B40"))
                                                criterion = 40;
                                            else if (line.Contains("*MCS0-NSS1-B80"))
                                                criterion = 41;
                                            else if (line.Contains("*MCS1-NSS1-B80"))
                                                criterion = 42;
                                            else if (line.Contains("*MCS2-NSS1-B80"))
                                                criterion = 43;
                                            else if (line.Contains("*MCS3-NSS1-B80"))
                                                criterion = 44;
                                            else if (line.Contains("*MCS4-NSS1-B80"))
                                                criterion = 45;
                                            else if (line.Contains("*MCS5-NSS1-B80"))
                                                criterion = 46;
                                            else if (line.Contains("*MCS6-NSS1-B80"))
                                                criterion = 47;
                                            else if (line.Contains("*MCS7-NSS1-B80"))
                                                criterion = 48;
                                            else if (line.Contains("*MCS8-NSS1-B80"))
                                                criterion = 49;
                                            else if (line.Contains("*MCS9-NSS1-B80"))
                                                criterion = 50;
                                            else if (line.Contains("*OFDM-6M"))
                                                criterion = 51;
                                            else if (line.Contains("*OFDM-9M"))
                                                criterion = 52;
                                            else if (line.Contains("*OFDM-12M"))
                                                criterion = 53;
                                            else if (line.Contains("*OFDM-18M"))
                                                criterion = 54;
                                            else if (line.Contains("*OFDM-24M"))
                                                criterion = 55;
                                            else if (line.Contains("*OFDM-36M"))
                                                criterion = 56;
                                            else if (line.Contains("*OFDM-48M"))
                                                criterion = 57;
                                            else if (line.Contains("*OFDM-54M"))
                                                criterion = 58;
                                            else if (line.Contains("*DSSS-1M"))
                                                criterion = 59;
                                            else if (line.Contains("*DSSS-2M"))
                                                criterion = 60;
                                            else if (line.Contains("*CCK-5_5M"))
                                                criterion = 61;
                                            else if (line.Contains("*CCK-11M"))
                                                criterion = 62;
                                            else if (line.Contains("*MCS15-B40"))
                                                criterion = 63;
                                            else if (line.Contains("*MCS7-NSS2-B80"))
                                                criterion = 64;
 
                                            TxCriterion verifyCriterion = new TxCriterion();
                                            while ((line = psr.PeekLine()) != null && !line.StartsWith("*"))
                                            {
                                                psr.ReadLine();
                                                if (string.IsNullOrEmpty(line.Trim()))
                                                    continue;

                                                string value = line.Split('=')[1].Trim();

                                                if (string.IsNullOrEmpty(value))
                                                    continue;

                                                string[] limits = value.Split(',');

                                                if (limits == null)
                                                    continue;

                                                if (line.Contains("POWER"))
                                                {
                                                    double target = double.Parse(limits[0]);
                                                    double upper = double.Parse(limits[1]);
                                                    double lower = double.Parse(limits[2]);
                                                    verifyCriterion.targetPower = target;
                                                    verifyCriterion.powerUpper = target + upper;
                                                    verifyCriterion.powerLower = target + lower;
                                                }
                                                else if (line.Contains("EVM"))
                                                {
                                                    verifyCriterion.evm = int.Parse(limits[0]);
                                                }
                                                else if (line.Contains("FREQ_ERR"))
                                                {
                                                    int upper = int.Parse(limits[0]);
                                                    int lower = int.Parse(limits[1]);
                                                    verifyCriterion.freqErrUpper = upper;
                                                    verifyCriterion.freqErrLower = lower;
                                                }
                                                else if (line.Contains("LEAKAGE"))
                                                {
                                                    verifyCriterion.leakage = int.Parse(limits[0]);
                                                }
                                                else if (line.Contains("FLATNESS"))
                                                {
                                                    verifyCriterion.flatness = double.Parse(limits[0]);
                                                }
                                                else if (line.Contains("MASK"))
                                                {
                                                    verifyCriterion.mask = int.Parse(limits[0]);
                                                }
                                            }

                                            if (criterion == 1)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS0_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 2)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 3)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS2_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 4)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS3_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 5)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS4_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 6)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS5_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 7)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS6_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 8)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS7_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 9)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS8_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 10)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS9_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 11)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS0_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 12)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 13)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS2_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 14)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS3_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 15)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS4_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 16)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS5_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 17)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS6_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 18)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS7_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 19)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS8_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 20)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS9_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 21)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS0_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 22)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS1_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 23)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS2_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 24)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS3_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 25)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS4_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 26)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS5_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 27)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS6_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 28)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS7_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 29)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS8_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 30)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS9_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 31)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS0_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 32)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS1_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 33)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS2_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 34)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS3_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 35)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS4_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 36)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS5_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 37)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS6_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 38)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS7_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 39)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS8_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 40)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS9_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 41)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS0_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 42)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS1_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 43)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS2_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 44)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS3_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 45)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS4_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 46)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS5_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 47)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS6_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 48)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS7_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 49)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS8_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 50)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS9_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 51)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_6M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 52)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_9M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 53)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_12M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 54)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_18M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 55)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_24M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 56)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_36M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 57)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_48M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 58)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.OFDM_54M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 59)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.DSSS_1M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 60)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.DSSS_2M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 61)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.CCK_5_5M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 62)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.CCK_11M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 63)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS15_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 64)
                                                txVerifyCritera.Add(new TxVerifyCriterion() { verifyItem = VerifyItem.MCS7_NSS2_B80, band = band, verifyCriterion = verifyCriterion });
                                        }
                                    }
                                    else if (line.Contains("**2G_BAND_RX_VERIFY**")
                                          || line.Contains("**5G_BAND_RX_VERIFY**"))
                                    {
                                        int band = 0;
                                        int criterion = 0;

                                        if (line.Contains("2G"))
                                            band = 2;
                                        else if (line.Contains("5G"))
                                            band = 5;

                                        line = psr.ReadLine();
                                        while (line != null && !line.StartsWith("**"))
                                        {
                                            line = psr.ReadLine();
                                            if (string.IsNullOrEmpty(line.Trim()))
                                                continue;

                                            if (line.Contains("*MCS0-B20"))
                                                criterion = 1;
                                            else if (line.Contains("*MCS1-B20"))
                                                criterion = 2;
                                            else if (line.Contains("*MCS2-B20"))
                                                criterion = 3;
                                            else if (line.Contains("*MCS3-B20"))
                                                criterion = 4;
                                            else if (line.Contains("*MCS4-B20"))
                                                criterion = 5;
                                            else if (line.Contains("*MCS5-B20"))
                                                criterion = 6;
                                            else if (line.Contains("*MCS6-B20"))
                                                criterion = 7;
                                            else if (line.Contains("*MCS7-B20"))
                                                criterion = 8;
                                            else if (line.Contains("*MCS8-B20"))
                                                criterion = 9;
                                            else if (line.Contains("*MCS9-B20"))
                                                criterion = 10;
                                            else if (line.Contains("*MCS0-B40"))
                                                criterion = 11;
                                            else if (line.Contains("*MCS1-B40"))
                                                criterion = 12;
                                            else if (line.Contains("*MCS2-B40"))
                                                criterion = 13;
                                            else if (line.Contains("*MCS3-B40"))
                                                criterion = 14;
                                            else if (line.Contains("*MCS4-B40"))
                                                criterion = 15;
                                            else if (line.Contains("*MCS5-B40"))
                                                criterion = 16;
                                            else if (line.Contains("*MCS6-B40"))
                                                criterion = 17;
                                            else if (line.Contains("*MCS7-B40"))
                                                criterion = 18;
                                            else if (line.Contains("*MCS8-B40"))
                                                criterion = 19;
                                            else if (line.Contains("*MCS9-B40"))
                                                criterion = 20;
                                            else if (line.Contains("*MCS0-NSS1-B20"))
                                                criterion = 21;
                                            else if (line.Contains("*MCS1-NSS1-B20"))
                                                criterion = 22;
                                            else if (line.Contains("*MCS2-NSS1-B20"))
                                                criterion = 23;
                                            else if (line.Contains("*MCS3-NSS1-B20"))
                                                criterion = 24;
                                            else if (line.Contains("*MCS4-NSS1-B20"))
                                                criterion = 25;
                                            else if (line.Contains("*MCS5-NSS1-B20"))
                                                criterion = 26;
                                            else if (line.Contains("*MCS6-NSS1-B20"))
                                                criterion = 27;
                                            else if (line.Contains("*MCS7-NSS1-B20"))
                                                criterion = 28;
                                            else if (line.Contains("*MCS8-NSS1-B20"))
                                                criterion = 29;
                                            else if (line.Contains("*MCS9-NSS1-B20"))
                                                criterion = 30;
                                            else if (line.Contains("*MCS0-NSS1-B40"))
                                                criterion = 31;
                                            else if (line.Contains("*MCS1-NSS1-B40"))
                                                criterion = 32;
                                            else if (line.Contains("*MCS2-NSS1-B40"))
                                                criterion = 33;
                                            else if (line.Contains("*MCS3-NSS1-B40"))
                                                criterion = 34;
                                            else if (line.Contains("*MCS4-NSS1-B40"))
                                                criterion = 35;
                                            else if (line.Contains("*MCS5-NSS1-B40"))
                                                criterion = 36;
                                            else if (line.Contains("*MCS6-NSS1-B40"))
                                                criterion = 37;
                                            else if (line.Contains("*MCS7-NSS1-B40"))
                                                criterion = 38;
                                            else if (line.Contains("*MCS8-NSS1-B40"))
                                                criterion = 39;
                                            else if (line.Contains("*MCS9-NSS1-B40"))
                                                criterion = 40;
                                            else if (line.Contains("*MCS0-NSS1-B80"))
                                                criterion = 41;
                                            else if (line.Contains("*MCS1-NSS1-B80"))
                                                criterion = 42;
                                            else if (line.Contains("*MCS2-NSS1-B80"))
                                                criterion = 43;
                                            else if (line.Contains("*MCS3-NSS1-B80"))
                                                criterion = 44;
                                            else if (line.Contains("*MCS4-NSS1-B80"))
                                                criterion = 45;
                                            else if (line.Contains("*MCS5-NSS1-B80"))
                                                criterion = 46;
                                            else if (line.Contains("*MCS6-NSS1-B80"))
                                                criterion = 47;
                                            else if (line.Contains("*MCS7-NSS1-B80"))
                                                criterion = 48;
                                            else if (line.Contains("*MCS8-NSS1-B80"))
                                                criterion = 49;
                                            else if (line.Contains("*MCS9-NSS1-B80"))
                                                criterion = 50;
                                            else if (line.Contains("*OFDM-6M"))
                                                criterion = 51;
                                            else if (line.Contains("*OFDM-9M"))
                                                criterion = 52;
                                            else if (line.Contains("*OFDM-12M"))
                                                criterion = 53;
                                            else if (line.Contains("*OFDM-18M"))
                                                criterion = 54;
                                            else if (line.Contains("*OFDM-24M"))
                                                criterion = 55;
                                            else if (line.Contains("*OFDM-36M"))
                                                criterion = 56;
                                            else if (line.Contains("*OFDM-48M"))
                                                criterion = 57;
                                            else if (line.Contains("*OFDM-54M"))
                                                criterion = 58;
                                            else if (line.Contains("*DSSS-1M"))
                                                criterion = 59;
                                            else if (line.Contains("*DSSS-2M"))
                                                criterion = 60;
                                            else if (line.Contains("*CCK-5_5M"))
                                                criterion = 61;
                                            else if (line.Contains("*CCK-11M"))
                                                criterion = 62;
                                            else if (line.Contains("*MCS9-NSS2-B80"))
                                                criterion = 63;

                                            RxCriterion verifyCriterion = new RxCriterion();
                                            while ((line = psr.PeekLine()) != null && !line.StartsWith("*"))
                                            {
                                                psr.ReadLine();
                                                if (string.IsNullOrEmpty(line.Trim()))
                                                    continue;

                                                string value = line.Split('=')[1].Trim();

                                                if (string.IsNullOrEmpty(value))
                                                    continue;

                                                string[] limits = value.Split(',');

                                                if (limits == null)
                                                    continue;

                                                if (line.Contains("RX_POWER"))
                                                {
                                                    verifyCriterion.rxPower = double.Parse(limits[0]);
                                                }
                                                else if (line.Contains("PER"))
                                                {
                                                    verifyCriterion.per = int.Parse(limits[0]);
                                                }
                                            }

                                            if (criterion == 1)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS0_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 2)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 3)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS2_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 4)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS3_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 5)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS4_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 6)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS5_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 7)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS6_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 8)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS7_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 9)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS8_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 10)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS9_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 11)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS0_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 12)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 13)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS2_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 14)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS3_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 15)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS4_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 16)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS5_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 17)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS6_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 18)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS7_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 19)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS8_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 20)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS9_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 21)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS0_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 22)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS1_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 23)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS2_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 24)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS3_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 25)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS4_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 26)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS5_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 27)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS6_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 28)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS7_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 29)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS8_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 30)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS9_NSS1_B20, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 31)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS0_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 32)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS1_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 33)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS2_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 34)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS3_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 35)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS4_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 36)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS5_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 37)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS6_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 38)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS7_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 39)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS8_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 40)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS9_NSS1_B40, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 41)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS0_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 42)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS1_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 43)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS2_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 44)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS3_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 45)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS4_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 46)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS5_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 47)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS6_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 48)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS7_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 49)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS8_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 50)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS9_NSS1_B80, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 51)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_6M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 52)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_9M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 53)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_12M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 54)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_18M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 55)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_24M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 56)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_36M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 57)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_48M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 58)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.OFDM_54M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 59)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.DSSS_1M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 60)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.DSSS_2M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 61)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.CCK_5_5M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 62)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.CCK_11M, band = band, verifyCriterion = verifyCriterion });
                                            else if (criterion == 63)
                                                rxVerifyCritera.Add(new RxVerifyCriterion() { verifyItem = VerifyItem.MCS9_NSS2_B80, band = band, verifyCriterion = verifyCriterion });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Criteria setting error: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        public static void LoadConfig()
        {
            try
            {
                using (StreamReader sr = new StreamReader(configPath))
                {
                    while (!sr.EndOfStream)
                    {
                        string content = sr.ReadLine();

                        if (string.IsNullOrEmpty(content))
                            continue;

                        content = content.Trim();
                        if (content.StartsWith("TEST_ITEM"))
                        {
                            string[] items = content.Split('=')[1].Split(',');
                            for (int i = 0; i < items.Length; i++)
                                items[i] = items[i].ToUpper();
                            testItems.AddRange(items);
                        }
                        else if (content.StartsWith("PLATFORM"))
                        {
                            platform = content.Split('=')[1].Trim();
                        }
                        else if (content.StartsWith("INTERFACE"))
                        {
                            connectionInterface = content.Split('=')[1].Trim();
                        }
                        else if (content.StartsWith("KO_FILE"))
                        {
                            drivFile = content.Split('=')[1].Trim();
                        }
                        else if (content.StartsWith("KO_DIR"))
                        {
                            drivDir = content.Split('=')[1].Trim();
                        }
                        else if (content.StartsWith("POWER_TRACKING"))
                        {
                            string strPowerTracking = content.Split('=')[1].ToUpper().Trim();
                            if (strPowerTracking == "ON")
                                powerTracking = true;
                            else if (strPowerTracking == "OFF")
                                powerTracking = false;
                            else
                                powerTracking = false;
                        }
                        else if (content.StartsWith("DELAY_FOR_CAPTURE"))
                        {
                            delayForCaptureMs = int.Parse(content.Split('=')[1].Trim());
                        }
                        else if (content.StartsWith("TARGET_THERMAL"))
                        {
                            targetThermal = int.Parse(content.Split('=')[1].Trim());
                        }
                        else if (content.StartsWith("THERMAL_WAITING"))
                        {
                            thermalWaitingTime = int.Parse(content.Split('=')[1].Trim());
                        }
                        else if (content.StartsWith("POWER_BY_RATE"))
                        {
                            powerByRate = int.Parse(content.Split('=')[1].Trim()) == 0 ? false : true;
                        }
                        else if (content.StartsWith("POWER_LIMIT"))
                        {
                            powerLimit = int.Parse(content.Split('=')[1].Trim()) == 0 ? false : true;
                        }
                        else if (content.StartsWith("ANT_TEST_ONLY"))
                        {
                            antTestOnly = content.Split('=')[1].ToUpper().Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Criteria setting error: " + ex.Message);
            }
        }

        public static void LoadCableLoss()
        {
            string line = "";
            using (StreamReader iq_atten_sr = new StreamReader(cablelossPath))
            {
                PeekableStreamReader psr = new PeekableStreamReader(iq_atten_sr);
                double IQ_FIXED_ATTEN_2_4_CHAIN1 = 0.0;
                double IQ_FIXED_ATTEN_2_4_CHAIN2 = 0.0;
                double IQ_FIXED_ATTEN_2_4_CHAIN3 = 0.0;
                double IQ_FIXED_ATTEN_2_4_CHAIN4 = 0.0;
                double IQ_FIXED_ATTEN_5_CHAIN1 = 0.0;
                double IQ_FIXED_ATTEN_5_CHAIN2 = 0.0;
                double IQ_FIXED_ATTEN_5_CHAIN3 = 0.0;
                double IQ_FIXED_ATTEN_5_CHAIN4 = 0.0;
                double iq_fixed_attend = 0.0;
                while (!psr.EndOfStream)
                {
                    if ((line = psr.ReadLine()) != null)
                    {
                        if (line.Trim().StartsWith("IQ_FIXED_ATTEN"))
                        {
                            iq_fixed_attend = double.Parse(line.Split('=')[1].Split('\t')[0].Trim());
                        }
                        if (line.Contains("IQ_FIXED_ATTEN_2_4_CHAIN1"))
                        {
                            IQ_FIXED_ATTEN_2_4_CHAIN1 = iq_fixed_attend;
                        }
                        else if (line.Contains("IQ_FIXED_ATTEN_2_4_CHAIN2"))
                        {
                            IQ_FIXED_ATTEN_2_4_CHAIN2 = iq_fixed_attend;
                        }
                        else if (line.Contains("IQ_FIXED_ATTEN_2_4_CHAIN3"))
                        {
                            IQ_FIXED_ATTEN_2_4_CHAIN3 = iq_fixed_attend;
                        }
                        else if (line.Contains("IQ_FIXED_ATTEN_2_4_CHAIN4"))
                        {
                            IQ_FIXED_ATTEN_2_4_CHAIN4 = iq_fixed_attend;
                        }
                        else if (line.Contains("IQ_FIXED_ATTEN_5_CHAIN1"))
                        {
                            IQ_FIXED_ATTEN_5_CHAIN1 = iq_fixed_attend;
                        }
                        else if (line.Contains("IQ_FIXED_ATTEN_5_CHAIN2"))
                        {
                            IQ_FIXED_ATTEN_5_CHAIN2 = iq_fixed_attend;
                        }
                        else if (line.Contains("IQ_FIXED_ATTEN_5_CHAIN3"))
                        {
                            IQ_FIXED_ATTEN_5_CHAIN3 = iq_fixed_attend;
                        }
                        else if (line.Contains("IQ_FIXED_ATTEN_5_CHAIN4"))
                        {
                            IQ_FIXED_ATTEN_5_CHAIN4 = iq_fixed_attend;
                            break;
                        }
                    }
                }

                int i = 0;
                Cableloss[] cablelossbuffer = new Cableloss[1000];
                while (!psr.EndOfStream)
                {
                    if ((line = psr.ReadLine()) != null)
                    {
                        if (line.Contains("IQ_ATTEN_2_4_BEGIN"))
                        {
                            while ((line = psr.ReadLine()) != null)
                            {
                                if (line.Trim() != "")
                                {
                                    string[] freq_depend_atten_value = line.Split('\t');
                                    cablelossbuffer[i].freq = int.Parse(freq_depend_atten_value[0].Trim());
                                    cablelossbuffer[i].chain1 = IQ_FIXED_ATTEN_2_4_CHAIN1 + double.Parse(freq_depend_atten_value[1].Trim());
                                    cablelossbuffer[i].chain2 = IQ_FIXED_ATTEN_2_4_CHAIN2 + double.Parse(freq_depend_atten_value[2].Trim());
                                    cablelossbuffer[i].chain3 = IQ_FIXED_ATTEN_2_4_CHAIN3 + double.Parse(freq_depend_atten_value[3].Trim());
                                    cablelossbuffer[i].chain4 = IQ_FIXED_ATTEN_2_4_CHAIN4 + double.Parse(freq_depend_atten_value[4].Trim());
                                    i++;
                                }
                                if ((line = psr.PeekLine()) != null && line.Contains("IQ_ATTEN_2_4_END"))
                                {
                                    psr.ReadLine();
                                    break;
                                }
                            }
                        }
                        else if (line.Contains("IQ_ATTEN_5_BEGIN"))
                        {
                            while ((line = psr.ReadLine()) != null)
                            {
                                if (line.Trim() != "")
                                {
                                    string[] freq_depend_atten_value = line.Split('\t');
                                    cablelossbuffer[i].freq = int.Parse(freq_depend_atten_value[0].Trim());
                                    cablelossbuffer[i].chain1 = IQ_FIXED_ATTEN_5_CHAIN1 + double.Parse(freq_depend_atten_value[1].Trim());
                                    cablelossbuffer[i].chain2 = IQ_FIXED_ATTEN_5_CHAIN2 + double.Parse(freq_depend_atten_value[2].Trim());
                                    cablelossbuffer[i].chain3 = IQ_FIXED_ATTEN_5_CHAIN3 + double.Parse(freq_depend_atten_value[3].Trim());
                                    cablelossbuffer[i].chain4 = IQ_FIXED_ATTEN_5_CHAIN4 + double.Parse(freq_depend_atten_value[4].Trim());
                                    i++;
                                }
                                if ((line = psr.PeekLine()) != null && line.Contains("IQ_ATTEN_5_END"))
                                {
                                    psr.ReadLine();
                                    break;
                                }
                            }
                        }
                    }
                }
                cableloss = new Cableloss[i];
                for (int j = 0; j < i; j++)
                {
                    cableloss[j] = cablelossbuffer[j];
                }
            }
        }
    }
}