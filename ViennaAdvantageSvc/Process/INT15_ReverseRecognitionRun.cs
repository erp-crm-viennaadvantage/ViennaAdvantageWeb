using System;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Process
{
    class INT15_ReverseRecognitionRun : SvrProcess
    {
        String _DocType = null;
        DateTime? _RecognitionDate = null;
        int _RevenueRecognition_ID = 0;
        VAdvantage.Model.MJournal journal = null;
        VAdvantage.Model.MJournalLine journalLine = null;
        int C_Period_ID = 0;
        int _AcctSchema_ID = 0;
        int _Currency_ID = 0;
        string DocNo = null;
        DateTime? _recognizeDate = null;
        int C_InvoiceLine_ID = 0;

        protected override string DoIt()
        {
            try
            {
                MRevenueRecognition mRevenueRecognition = new MRevenueRecognition(GetCtx(), _RevenueRecognition_ID, Get_Trx());
                MRevenueRecognitionPlan[] revenueRecognitionPlans = MRevenueRecognitionPlan.GetRecognitionPlans(mRevenueRecognition, C_InvoiceLine_ID);
                for (int i = 0; i < revenueRecognitionPlans.Length; i++)
                {
                    MRevenueRecognitionPlan revenueRecognitionPlan = revenueRecognitionPlans[i];
                    VAdvantage.Model.MInvoiceLine invoiceLine = new VAdvantage.Model.MInvoiceLine(GetCtx(), revenueRecognitionPlan.GetC_InvoiceLine_ID(), Get_Trx());
                    MInvoice invoice = new MInvoice(GetCtx(), invoiceLine.GetC_Invoice_ID(), Get_Trx());
                    MRevenueRecognitionRun[] mRevenueRecognitionRuns = MRevenueRecognitionRun.GetRecognitionRuns(revenueRecognitionPlan, _RecognitionDate, false);
                    if (mRevenueRecognitionRuns.Length > 0)
                    {
                        if (_DocType == "GL")
                        {
                            if (revenueRecognitionPlan.GetC_AcctSchema_ID() != _AcctSchema_ID || revenueRecognitionPlan.GetC_Currency_ID() != _Currency_ID)
                            {
                                if (journal != null && journal.CompleteIt() == "CO")
                                {

                                    journal.SetProcessed(true);
                                    journal.SetDocStatus("CO");
                                    journal.SetDocAction("CL");
                                    journal.Save(Get_Trx());
                                    if (DocNo == null)
                                    {
                                        DocNo = journal.GetDocumentNo();
                                    }
                                    else
                                    {
                                        DocNo += "," + journal.GetDocumentNo();

                                    }
                                }
                                journal = new VAdvantage.Model.MJournal(GetCtx(), 0, Get_Trx());
                                journal = CreateJournalHDR(mRevenueRecognition, revenueRecognitionPlan);
                                journal.Save();
                                _AcctSchema_ID = journal.GetC_AcctSchema_ID();
                                _Currency_ID = journal.GetC_Currency_ID();

                                decimal totalAmt = 0;
                                for (int j = 0; j < mRevenueRecognitionRuns.Length; j++)
                                {
                                    MRevenueRecognitionRun revenueRecognitionRun = mRevenueRecognitionRuns[j];
                                    totalAmt += revenueRecognitionRun.GetRecognizedAmt();
                                    revenueRecognitionRun.SetGL_Journal_ID(journal.GetGL_Journal_ID());
                                    revenueRecognitionRun.Save();
                                }

                                revenueRecognitionPlan.SetRecognizedAmt(totalAmt + revenueRecognitionPlan.GetRecognizedAmt());
                                revenueRecognitionPlan.Save();

                                for (int k = 0; k < 2; k++)
                                {
                                    journalLine = new VAdvantage.Model.MJournalLine(journal);
                                    int combination_ID = 0;
                                    if (k == 0)
                                    {
                                        if (mRevenueRecognition.GetINT15_RecognizeType() == "R")
                                        {
                                            combination_ID = revenueRecognitionPlan.GetUnEarnedRevenue_Acct();
                                            //journalLine.SetC_ValidCombination_ID(combination_ID);
                                        }
                                        else
                                        {
                                            combination_ID = revenueRecognitionPlan.GetINT15_ProductExpense();
                                            //journalLine.SetC_ValidCombination_ID(combination_ID);
                                        }
                                        journalLine.SetAmtAcctDr(totalAmt);
                                        journalLine.SetAmtSourceDr(totalAmt);
                                        journalLine.SetAmtSourceCr(0);
                                        journalLine.SetAmtAcctCr(0);
                                    }
                                    else
                                    {
                                        if (mRevenueRecognition.GetINT15_RecognizeType() == "R")
                                        {
                                            combination_ID = revenueRecognitionPlan.GetP_Revenue_Acct();
                                            //journalLine.SetC_ValidCombination_ID(combination_ID);
                                        }
                                        else
                                        {
                                            combination_ID = revenueRecognitionPlan.GetINT15_PrepaidExpense();
                                            //journalLine.SetC_ValidCombination_ID(combination_ID);
                                        }
                                        journalLine.SetAmtAcctDr(0);
                                        journalLine.SetAmtSourceDr(0);
                                        journalLine.SetAmtSourceCr(totalAmt);
                                        journalLine.SetAmtAcctCr(totalAmt);
                                    }
                                    int account_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select Account_ID From C_ValidCombination Where C_ValidCombination_ID=" + combination_ID));
                                    VAdvantage.Model.MElementValue elementValue = new VAdvantage.Model.MElementValue(GetCtx(), account_ID, Get_Trx());
                                    try
                                    {
                                        journalLine.Set_ValueNoCheck("Account_ID", account_ID);
                                        journalLine.Set_ValueNoCheck("C_BPartner_ID", invoice.GetC_BPartner_ID());
                                        journalLine.Set_ValueNoCheck("M_Product_ID", invoiceLine.GetM_Product_ID());
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    journalLine.Save();
                                }
                            }
                        }
                    }
                }
                if (journal != null && journal.GetDocStatus() != "CO")
                {
                    if (journal.CompleteIt() == "CO")
                    {
                        journal.SetProcessed(true);
                        journal.SetDocStatus("CO");
                        journal.SetDocAction("CL");
                        journal.Save(Get_Trx());
                        if (DocNo == null)
                        {
                            DocNo = journal.GetDocumentNo();
                        }
                        else
                        {
                            DocNo += "," + journal.GetDocumentNo();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(Level.SEVERE, "GL Journal not allocated due to " + ex);
                return ex.ToString();
            }
            return Msg.GetMsg(GetCtx(), "INT15_GLJournalCreated = " + DocNo);
        }

        public VAdvantage.Model.MJournal CreateJournalHDR(MRevenueRecognition revenueRecognition, MRevenueRecognitionPlan revenueRecognitionPlan)
        {
            journal.SetClientOrg(revenueRecognitionPlan.GetAD_Client_ID(), revenueRecognitionPlan.GetAD_Org_ID());
            journal.SetC_AcctSchema_ID(revenueRecognitionPlan.GetC_AcctSchema_ID());
            journal.SetDescription("Revenue Recognition Run");
            journal.SetPostingType("A");
            int C_Doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select C_Doctype_ID From C_Doctype Where DocBaseType='GLJ'"));
            journal.SetC_DocType_ID(C_Doctype_ID);
            int GL_Category_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select GL_Category_ID From GL_Category Where CategoryType='M' Order by GL_Category_ID desc"));
            journal.SetGL_Category_ID(GL_Category_ID);
            journal.SetDateDoc(DateTime.Now);
            journal.SetDateAcct(DateTime.Now);

            DateTime? Today = DateTime.Now;

            C_Period_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select C_Period_ID From C_Period pr INNER JOIN ad_clientinfo cl ON cl.ad_client_id = " + GetAD_Client_ID() + " INNER JOIN c_year yr ON (yr.c_year_id = pr.c_year_id" +
                " AND cl.c_calendar_id=yr.c_calendar_id) Where " + GlobalVariable.TO_DATE(Today, true) + " between StartDate and EndDate"));

            journal.SetC_Period_ID(C_Period_ID);
            journal.SetC_Currency_ID(revenueRecognitionPlan.GetC_Currency_ID());
            int C_ConversionType_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select C_ConversionType_ID From C_ConversionType where IsDefault='Y'"));
            journal.SetC_ConversionType_ID(C_ConversionType_ID);
            journal.SetTotalCr(revenueRecognitionPlan.GetTotalAmt());
            journal.SetTotalDr(revenueRecognitionPlan.GetTotalAmt());
            journal.SetDocStatus("DR");
            journal.SetDocAction("CO");

            return journal;
        }


        protected override void Prepare()
        {
            ProcessInfoParameter[] para = GetParameter();
            for (int i = 0; i < para.Length; i++)
            {
                String name = para[i].GetParameterName();
                if (para[i].GetParameter() == null)
                {
                    ;
                }
                else if (name.Equals("INT15_DocType"))
                {
                    _DocType = Util.GetValueOfString(para[i].GetParameter());
                }
                else if (name.Equals("C_RevenueRecognition_ID"))
                {
                    _RevenueRecognition_ID = Util.GetValueOfInt(para[i].GetParameter());
                }
                else if (name.Equals("C_InvoiceLine_ID"))
                {
                    C_InvoiceLine_ID = Util.GetValueOfInt(para[i].GetParameter());
                }
            }
        }
    }
}
