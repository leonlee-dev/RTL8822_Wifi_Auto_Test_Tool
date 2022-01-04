using RTKModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public class CrystalCalibPlan : RtwTx
    {
        public CrystalCriterion crystalCriterion;

        public CrystalCalibPlan(RtwTx tx, CrystalCriterion crystalCriterion) : base(tx.freq, tx.bw, tx.antPath, tx.rateID, tx.txMode)
        {
            this.crystalCriterion = crystalCriterion;
        }
    }

    public class TxCalibPlan : RtwTx
    {
        public CalibCriterion calibCriterion;
        public byte[] defaultPower;

        public TxCalibPlan(byte[] defaultPower, RtwTx tx, CalibCriterion calibCriterion) : base(tx.freq, tx.bw, tx.antPath, tx.rateID, tx.txMode)
        {
            this.defaultPower = defaultPower;
            this.calibCriterion = calibCriterion;
        }
    }

    public class TxVerifyPlan : RtwTx
    {
        public TxCriterion verifyCriterion;

        public TxVerifyPlan(RtwTx tx, TxCriterion verifyCriterion) : base(tx.freq, tx.bw, tx.antPath, tx.rateID, tx.txMode)
        {
            this.verifyCriterion = verifyCriterion;
        }
    }

    public class RxVerifyPlan : RtwRx
    {
        public RxCriterion verifyCriterion;
        public string streamFile;

        public RxVerifyPlan(RtwRx rx, string streamFile, RxCriterion verifyCriterion) : base(rx.freq, rx.bw, rx.antPath, rx.rateID)
        {
            this.streamFile = streamFile;
            this.verifyCriterion = verifyCriterion;
        }
    }
}
