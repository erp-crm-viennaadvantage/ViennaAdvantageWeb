using System;
using System.Collections.Generic;
using System.Data;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Utility;

namespace ViennaAdvantage.Model
{
    public class MRevenueRecognitionRun : X_C_RevenueRecognition_Run
    {
        private static VLogger _log = VLogger.GetVLogger(typeof(MRevenueRecognitionRun).FullName);
        public MRevenueRecognitionRun(Ctx ctx, int C_RevenueRecognition_Run_ID, Trx trxName)
            : base(ctx, C_RevenueRecognition_Run_ID, trxName)
        {

        }
        public MRevenueRecognitionRun(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {

        }

        public static MRevenueRecognitionRun[] GetRecognitionRuns(MRevenueRecognitionPlan revenueRecognitionPlan, DateTime? recognitionDate, bool reverse)
        {
            List<MRevenueRecognitionRun> list = new List<MRevenueRecognitionRun>();
            string sql = "Select * from C_RevenueRecognition_Run Where C_RevenueRecognition_Plan_ID =" + revenueRecognitionPlan.GetC_RevenueRecognition_Plan_ID();
            if (reverse)
            {
                sql += "And INT15_RecognitionDate <=" + GlobalVariable.TO_DATE(recognitionDate, true);
            }
            sql += " And NVL(GL_Journal_ID,0) <= 0";

            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                idr = DB.ExecuteReader(sql, null, revenueRecognitionPlan.Get_Trx());
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new MRevenueRecognitionRun(revenueRecognitionPlan.GetCtx(), dr, revenueRecognitionPlan.Get_Trx()));
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                _log.Log(Level.SEVERE, sql, e);
            }
            finally
            {
                dt = null;
            }

            MRevenueRecognitionRun[] retValue = new MRevenueRecognitionRun[list.Count];
            retValue = list.ToArray();
            return retValue;
        }

        public void SetRecognitionRun(MRevenueRecognition revenueRecognition, MINT15RevenueService revenueService, MRevenueRecognitionPlan revenueRecognitionPlan)
        {
            SetAD_Client_ID(revenueRecognitionPlan.GetAD_Client_ID());
            SetAD_Org_ID(revenueRecognitionPlan.GetAD_Org_ID());
            SetC_RevenueRecognition_Plan_ID(revenueRecognitionPlan.GetC_RevenueRecognition_Plan_ID());
            if (revenueService != null)
            {
                SetINT15_RevenueService_ID(revenueService.GetINT15_RevenueService_ID());
            }
        }
    }
}
