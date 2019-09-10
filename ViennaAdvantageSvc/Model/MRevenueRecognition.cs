/********************************************************
 * Project Name   : VAdvantage
 * Class Name     : MRevenueRecognition
 * Purpose        : Revenue Recognition Model
 * Class Used     : X_C_RevenueRecognition
 * Chronological    Development
 * Raghunandan      19-Jan-2010
  ******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using VAdvantage.DataBase;
//using java.io;
//using System.IO;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace ViennaAdvantage.Model
{
    /// <summary>
    /// Revenue Recognition Model
    /// </summary>
    public class MRevenueRecognition : X_C_RevenueRecognition
    {
        private static VLogger _log = VLogger.GetVLogger(typeof(MINT15RevenueService).FullName);
        /// <summary>
        /// Standard Constructor
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="C_RevenueRecognition_ID"></param>
        /// <param name="trxName"></param>
        public MRevenueRecognition(Ctx ctx, int C_RevenueRecognition_ID, Trx trxName)
            : base(ctx, C_RevenueRecognition_ID, trxName)
        {

        }

        /// <summary>
        /// Load Constructor
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="rs"></param>
        /// <param name="trxName"></param>
        public MRevenueRecognition(Ctx ctx, DataRow idr, Trx trxName)
            : base(ctx, idr, trxName)
        {

        }

        public static MRevenueRecognition[] GetRecognitions(Ctx ctx, Trx trx)
        {
            List<MRevenueRecognition> list = new List<MRevenueRecognition>();
            string sql = "Select * From C_RevenueRecognition Where AD_Client_ID=" + ctx.GetAD_Client_ID();

            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                idr = DB.ExecuteReader(sql, null, null);
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new MRevenueRecognition(ctx, dr, trx));
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

            MRevenueRecognition[] retValue = new MRevenueRecognition[list.Count];
            retValue = list.ToArray();
            return retValue;
        }

        private static MINT15RevenueService[] GetServices(MRevenueRecognition MRevenueRecognition)
        {
            List<MINT15RevenueService> list = new List<MINT15RevenueService>();
            String sql = "SELECT * FROM INT15_RevenueService WHERE C_RevenueRecognition_ID=" + MRevenueRecognition.GetC_RevenueRecognition_ID();
            DataSet ds = new DataSet();
            try
            {
                ds = DB.ExecuteDataset(sql, null, MRevenueRecognition.Get_TrxName());
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    MINT15RevenueService il = new MINT15RevenueService(MRevenueRecognition.GetCtx(), dr, MRevenueRecognition.Get_TrxName());
                    list.Add(il);
                }
                ds = null;
            }
            catch (Exception e)
            {
                // log.Log(Level.SEVERE, "getServices", e);
            }

            MINT15RevenueService[] lines = new MINT15RevenueService[list.Count];
            lines = list.ToArray();
            return lines;
        }
        public static string CreateRevenueRecognitionPlan(int C_InvoiceLine_ID, int C_RevenueRecognition_ID, MInvoice Invoice)
        {
            try
            {
                MRevenueRecognitionRun revenueRecognitionRun = null;
                DateTime? RecognizationDate = null;
                MRevenueRecognition revenueRecognition = new MRevenueRecognition(Invoice.GetCtx(), C_RevenueRecognition_ID, Invoice.Get_Trx());
                int defaultAccSchemaOrg_ID = GetDefaultActSchema(Invoice.GetAD_Org_ID());
                if (defaultAccSchemaOrg_ID <= 0)
                {
                    _log.Log(Level.SEVERE, "Default Schema not found for the oraganization");
                    return "Default Schema not found for the oraganization";
                }
                MINT15AccountingSchemaOrg accountingSchemaOrg = new MINT15AccountingSchemaOrg(Invoice.GetCtx(), defaultAccSchemaOrg_ID, Invoice.Get_Trx());

                MRevenueRecognitionPlan revenueRecognitionPlan = new MRevenueRecognitionPlan(Invoice.GetCtx(), 0, Invoice.Get_Trx());
                MInvoiceLine invoiceLine = new MInvoiceLine(Invoice.GetCtx(), C_InvoiceLine_ID, Invoice.Get_Trx());
                MInvoice invoice = new MInvoice(Invoice.GetCtx(), invoiceLine.GetC_Invoice_ID(), Invoice.Get_Trx());

                string sql = "Select INT15_StartDate From C_InvoiceLine Where C_InvoiceLine_ID=" + invoiceLine.GetC_InvoiceLine_ID();
                RecognizationDate = Util.GetValueOfDateTime(DB.ExecuteScalar(sql));
                if (RecognizationDate == null)
                {
                    RecognizationDate = invoice.GetDateInvoiced();
                }

                revenueRecognitionPlan.SetRecognitionPlan(invoiceLine, invoice, C_RevenueRecognition_ID);
                revenueRecognitionPlan.SetC_AcctSchema_ID(accountingSchemaOrg.GetC_AcctSchema_ID());
                revenueRecognitionPlan.SetRecognizedAmt(0);
                if (revenueRecognition.GetINT15_RecognizeType() == "R")
                {
                    revenueRecognitionPlan.SetUnEarnedRevenue_Acct(RecognitionCombination(accountingSchemaOrg, "UR", invoiceLine));
                    revenueRecognitionPlan.SetP_Revenue_Acct(RecognitionCombination(accountingSchemaOrg, "TR", invoiceLine));
                }
                else
                {
                    revenueRecognitionPlan.SetINT15_PrepaidExpense(RecognitionCombination(accountingSchemaOrg, "DE", invoiceLine));
                    revenueRecognitionPlan.SetINT15_ProductExpense(RecognitionCombination(accountingSchemaOrg, "PE", invoiceLine));
                }
                revenueRecognitionPlan.Save();
                if (!revenueRecognition.IsTimeBased())
                {
                    MINT15RevenueService[] revenueService = GetServices(revenueRecognition);
                    for (int i = 0; i < revenueService.Length; i++)
                    {
                        MINT15RevenueService revenueserviceline = revenueService[i];
                        revenueRecognitionRun = new MRevenueRecognitionRun(Invoice.GetCtx(), 0, Invoice.Get_Trx());
                        revenueRecognitionRun.SetRecognitionRun(revenueRecognition, revenueserviceline, revenueRecognitionPlan);
                        Decimal recognizedAmt = (revenueRecognitionPlan.GetTotalAmt() * revenueserviceline.GetINT15_Percentage()) / 100;
                        revenueRecognitionRun.SetRecognizedAmt(recognizedAmt);
                        revenueRecognitionRun.SetINT15_RevenueService_ID(revenueserviceline.GetINT15_RevenueService_ID());

                        revenueRecognitionRun.Save();
                    }
                }
                else
                {
                    if (revenueRecognition.GetRecognitionFrequency() == "M")
                    {
                        //Decimal recognizedAmt = revenueRecognitionPlan.GetTotalAmt() / revenueRecognition.GetNoMonths();
                        double totaldays = (RecognizationDate.Value.AddMonths(revenueRecognition.GetNoMonths()) - RecognizationDate.Value.Date).TotalDays;
                        decimal perdayAmt = revenueRecognitionPlan.GetTotalAmt() / Convert.ToDecimal(totaldays);
                        decimal recognizedAmt = 0;
                        DateTime? lastdate = null;
                        int days = 0;
                        for (int i = 0; i < revenueRecognition.GetNoMonths() + 1; i++)
                        {
                            if (i == 0)
                            {
                                if (RecognizationDate.Value.Month == 12)
                                {
                                    lastdate = new DateTime(RecognizationDate.Value.Year, RecognizationDate.Value.Month, 1).AddMonths(1).AddDays(-1);
                                }
                                else
                                {
                                    lastdate = new DateTime(RecognizationDate.Value.Year, RecognizationDate.Value.Month + 1, 1).AddDays(-1);
                                }
                                days = Util.GetValueOfInt((lastdate.Value.Date - RecognizationDate.Value.Date).TotalDays);
                                days += 1;
                            }
                            else if (i == (revenueRecognition.GetNoMonths()))
                            {
                                DateTime EndDate = RecognizationDate.Value.AddMonths(i);
                                var startDate = new DateTime(EndDate.Year, EndDate.Month, 1);
                                days = Util.GetValueOfInt((EndDate.Date - startDate.Date).TotalDays);
                            }
                            else
                            {
                                DateTime startdate = RecognizationDate.Value.AddMonths(i);
                                days = DateTime.DaysInMonth(startdate.Year, startdate.Month);
                            }
                            recognizedAmt = Convert.ToDecimal(days) * perdayAmt;
                            revenueRecognitionRun = new MRevenueRecognitionRun(Invoice.GetCtx(), 0, Invoice.Get_Trx());
                            revenueRecognitionRun.SetRecognitionRun(revenueRecognition, null, revenueRecognitionPlan);
                            revenueRecognitionRun.SetRecognizedAmt(recognizedAmt);
                            revenueRecognitionRun.SetINT15_RecognitionDate(RecognizationDate.Value.AddMonths(i));
                            revenueRecognitionRun.Save();
                            recognizedAmt = 0;
                        }
                    }
                    else if (revenueRecognition.GetRecognitionFrequency() == "D")
                    {
                        Decimal recognizedAmt = revenueRecognitionPlan.GetTotalAmt() / revenueRecognition.GetNoMonths();
                        int days = 0;
                        for (int i = 0; i < revenueRecognition.GetNoMonths(); i++)
                        {
                            revenueRecognitionRun = new MRevenueRecognitionRun(Invoice.GetCtx(), 0, Invoice.Get_Trx());
                            revenueRecognitionRun.SetRecognitionRun(revenueRecognition, null, revenueRecognitionPlan);
                            revenueRecognitionRun.SetRecognizedAmt(recognizedAmt);
                            revenueRecognitionRun.SetINT15_RecognitionDate(RecognizationDate.Value.AddDays(days));
                            days += 1;
                            revenueRecognitionRun.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return "Revenue Recognition Created";
        }

        public static int GetDefaultActSchema(int AD_Org_ID)
        {
            string sql = "Select INT15_AccountingSchemaOrg_ID From INT15_AccountingSchemaOrg Where AD_Org_ID=" + AD_Org_ID;
            int C_AcctschemaOrg_ID = Util.GetValueOfInt(DB.ExecuteScalar(sql));
            return C_AcctschemaOrg_ID;
        }

        public static int RecognitionCombination(MINT15AccountingSchemaOrg accountingSchemaOrg, string recognitionType, MInvoiceLine InvoiceLine)
        {
            string sql = "";
            if (InvoiceLine.GetM_Product_ID() > 0)
            {
                sql = "Select pacct.c_validcombination_id From FRPT_Product_Acct pacct INNER JOIN FRPT_AcctDefault fad on fad.FRPT_AcctDefault_ID = pacct.FRPT_AcctDefault_ID " +
                           "  where fad.INT15_RecognizeType = '" + recognitionType + "' and pacct.c_acctschema_id = " + accountingSchemaOrg.GetC_AcctSchema_ID() + " And pacct.M_PRoduct_ID=" + InvoiceLine.GetM_Product_ID();
            }
            else if (InvoiceLine.GetC_Charge_ID() > 0)
            {
                sql = "Select pacct.c_validcombination_id From FRPT_Charge_Acct pacct INNER JOIN FRPT_AcctDefault fad on fad.FRPT_AcctDefault_ID = pacct.FRPT_AcctDefault_ID " +
                           "  where fad.INT15_RecognizeType = '" + recognitionType + "' and pacct.c_acctschema_id = " + accountingSchemaOrg.GetC_AcctSchema_ID() + " And pacct.C_Charge_ID=" + InvoiceLine.GetC_Charge_ID();
            }
            else
            {
                sql = "select ad.c_validcombination_id from INT15_AcctSchema_Default ad inner join FRPT_AcctDefault fad on fad.FRPT_AcctDefault_ID = ad.FRPT_AcctDefault_ID" +
                    " where fad.INT15_RecognizeType = '" + recognitionType + "' and ad.c_acctschema_id = " + accountingSchemaOrg.GetC_AcctSchema_ID();
            }
            int ValidCombination_ID = Util.GetValueOfInt(DB.ExecuteScalar(sql));
            return ValidCombination_ID;
        }
    }
}
