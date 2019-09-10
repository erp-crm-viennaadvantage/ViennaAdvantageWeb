using System;
using System.Data;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Process
{
    public class INT15_RevenueRecognitionRun : SvrProcess
    {
        String _DocType = null;
        DateTime? _RecognitionDate = null;
        int _RevenueRecognition_ID = 0;
        VAdvantage.Model.MJournal journal = null;
        VAdvantage.Model.MJournalLine journalLine = null;
        int _AcctSchema_ID = 0;
        int _Currency_ID = 0;
        int C_Period_ID = 0;
        string DocNo = null;
        DateTime? _RecognizeDate = null;


        protected override string DoIt()
        {
            String msg = "";
            MRevenueRecognition mRevenueRecognition = null;

            if (_RevenueRecognition_ID > 0)
            {
                mRevenueRecognition = new MRevenueRecognition(GetCtx(), _RevenueRecognition_ID, Get_Trx());
                msg = createJournals(mRevenueRecognition);
            }
            else
            {
                MRevenueRecognition[] RevenueRecognitions = MRevenueRecognition.GetRecognitions(GetCtx(), Get_Trx());
                if (RevenueRecognitions != null && RevenueRecognitions.Length > 0)
                {
                    for (int i = 0; i < RevenueRecognitions.Length; i++)
                    {
                        mRevenueRecognition = RevenueRecognitions[i];
                        msg = createJournals(mRevenueRecognition);
                    }
                }
            }
            return msg;
        }

        public string createJournals(MRevenueRecognition mRevenueRecognition)
        {
            try
            {
                MRevenueRecognitionPlan[] revenueRecognitionPlans = MRevenueRecognitionPlan.GetRecognitionPlans(mRevenueRecognition, 0);
                MRevenueRecognitionRun[] mRevenueRecognitionRuns = null;
                for (int i = 0; i < revenueRecognitionPlans.Length; i++)
                {
                    MRevenueRecognitionPlan revenueRecognitionPlan = revenueRecognitionPlans[i];
                    VAdvantage.Model.MInvoiceLine invoiceLine = new VAdvantage.Model.MInvoiceLine(GetCtx(), revenueRecognitionPlan.GetC_InvoiceLine_ID(), Get_Trx());
                    MInvoice invoice = new MInvoice(GetCtx(), invoiceLine.GetC_Invoice_ID(), Get_Trx());

                    mRevenueRecognitionRuns = MRevenueRecognitionRun.GetRecognitionRuns(revenueRecognitionPlan, _RecognitionDate, true);

                    if (mRevenueRecognitionRuns.Length > 0)
                    {
                        for (int j = 0; j < mRevenueRecognitionRuns.Length; j++)
                        {
                            MRevenueRecognitionRun revenueRecognitionRun = mRevenueRecognitionRuns[j];
                            if (_DocType == "GL")
                            {
                                if (revenueRecognitionPlan.GetC_AcctSchema_ID() != _AcctSchema_ID || revenueRecognitionPlan.GetC_Currency_ID() != _Currency_ID || revenueRecognitionRun.GetINT15_RecognitionDate() != _RecognizeDate)
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
                                    journal = CreateJournalHDR(revenueRecognitionPlan, revenueRecognitionRun);

                                    #region Commented Code for Journal Batch
                                    //else if (_DocType == "GB")
                                    //{
                                    //    VAdvantage.Model.MJournalBatch journalBatch = new VAdvantage.Model.MJournalBatch(GetCtx(), 0, Get_Trx());
                                    //    journalBatch.SetClientOrg(revenueRecognitionPlan);
                                    //    journalBatch.SetDescription("Revenue Recognition Run");
                                    //    journalBatch.SetPostingType("A");
                                    //    journalBatch.SetDateDoc(_RecognitionDate);
                                    //    journalBatch.SetDateAcct(_RecognitionDate);
                                    //    journalBatch.SetC_Period_ID(C_Period_ID);
                                    //    int C_Doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select C_Doctype_ID From C_Doctype Where DocBaseType='GLJ'"));
                                    //    journalBatch.SetC_DocType_ID(C_Doctype_ID);
                                    //    journalBatch.SetC_Currency_ID(revenueRecognitionPlan.GetC_Currency_ID());
                                    //    journalBatch.SetTotalCr(revenueRecognitionPlan.GetTotalAmt());
                                    //    journalBatch.SetTotalDr(revenueRecognitionPlan.GetTotalAmt());
                                    //    journalBatch.SetDocStatus("DR");
                                    //    journalBatch.SetDocAction("CO");
                                    //    if (journalBatch.Save())
                                    //    {
                                    //        if (revenueRecognitionPlan.GetC_AcctSchema_ID() != _AcctSchema_ID || revenueRecognitionPlan.GetC_Currency_ID() != _Currency_ID)
                                    //        {
                                    //            journal = new VAdvantage.Model.MJournal(journalBatch);
                                    //            journal = CreateJournalHDR(mRevenueRecognition, revenueRecognitionPlan);
                                    //        }
                                    //    }
                                    //}
                                    #endregion

                                    if (journal.Save())
                                    {
                                        _AcctSchema_ID = journal.GetC_AcctSchema_ID();
                                        _Currency_ID = journal.GetC_Currency_ID();
                                        _RecognizeDate = revenueRecognitionRun.GetINT15_RecognitionDate();
                                    }
                                }
                                for (int k = 0; k < 2; k++)
                                {
                                    journalLine = new VAdvantage.Model.MJournalLine(journal);
                                    journalLine = GenerateJounalLine(journal, invoice, invoiceLine, revenueRecognitionPlan, revenueRecognitionRun, mRevenueRecognition.GetINT15_RecognizeType(), k);
                                    if (journalLine.Save())
                                    {
                                        revenueRecognitionRun.SetGL_Journal_ID(journal.GetGL_Journal_ID());
                                        revenueRecognitionRun.Save();
                                    }
                                }
                                revenueRecognitionPlan.SetRecognizedAmt(revenueRecognitionRun.GetRecognizedAmt() + revenueRecognitionPlan.GetRecognizedAmt());
                                revenueRecognitionPlan.Save();
                            }
                        }
                    }
                }
                if (journal != null && journal.GetDocStatus() != "CO")
                {
                    journal.CompleteIt();
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
            catch (Exception ex)
            {
                log.Log(Level.SEVERE, "GL Journal not allocated due to " + ex);
                return ex.ToString();
            }
            if (DocNo == null)
            {
                DocNo = "0";
            }
            return Msg.GetMsg(GetCtx(), "INT15_GLJournalCreated = " + DocNo);
        }

        public VAdvantage.Model.MJournalLine GenerateJounalLine(VAdvantage.Model.MJournal journal, MInvoice invoice, VAdvantage.Model.MInvoiceLine invoiceLine,
                                            MRevenueRecognitionPlan revenueRecognitionPlan, MRevenueRecognitionRun revenueRecognitionRun, string recognitionType, int k)
        {
            try
            {
                int combination_ID = 0;
                if (k == 0)
                {
                    if (recognitionType == "R")
                    {
                        combination_ID = revenueRecognitionPlan.GetUnEarnedRevenue_Acct();
                        //journalLine.SetC_ValidCombination_ID(combination_ID);
                    }
                    else
                    {
                        combination_ID = revenueRecognitionPlan.GetINT15_ProductExpense();
                        //journalLine.SetC_ValidCombination_ID(combination_ID);
                    }
                    journalLine.SetAmtAcctDr(revenueRecognitionRun.GetRecognizedAmt());
                    journalLine.SetAmtSourceDr(revenueRecognitionRun.GetRecognizedAmt());
                    journalLine.SetAmtSourceCr(0);
                    journalLine.SetAmtAcctCr(0);

                }
                else
                {
                    if (recognitionType == "R")
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
                    journalLine.SetAmtSourceCr(revenueRecognitionRun.GetRecognizedAmt());
                    journalLine.SetAmtAcctCr(revenueRecognitionRun.GetRecognizedAmt());
                }
                int account_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select Account_ID From C_ValidCombination Where C_ValidCombination_ID=" + combination_ID));
                //VAdvantage.Model.MElementValue elementValue = new VAdvantage.Model.MElementValue(GetCtx(), account_ID, Get_Trx());

                journalLine.Set_ValueNoCheck("Account_ID", account_ID);
                journalLine.Set_ValueNoCheck("C_BPartner_ID", invoice.GetC_BPartner_ID());
                journalLine.Set_ValueNoCheck("M_Product_ID", invoiceLine.GetM_Product_ID());
            }
            catch (Exception ex)
            {
                log.SaveError(null, ex);
            }
            return journalLine;
        }

        public VAdvantage.Model.MJournal CreateJournalHDR(MRevenueRecognitionPlan revenueRecognitionPlan, MRevenueRecognitionRun revenurecognitionRun)
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
            journal.SetDateAcct(revenurecognitionRun.GetINT15_RecognitionDate());

            
            C_Period_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select C_Period_ID From C_Period pr INNER JOIN ad_clientinfo cl ON cl.ad_client_id = " + GetAD_Client_ID() + " INNER JOIN c_year yr ON (yr.c_year_id = pr.c_year_id" +
                " AND cl.c_calendar_id=yr.c_calendar_id) Where " + GlobalVariable.TO_DATE(revenurecognitionRun.GetINT15_RecognitionDate(), true) + " between StartDate and EndDate"));

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
                else if (name.Equals("INT15_RecognitionDate"))
                {
                    _RecognitionDate = Util.GetValueOfDateTime(para[i].GetParameter());
                }
                else if (name.Equals("C_RevenueRecognition_ID"))
                {
                    _RevenueRecognition_ID = Util.GetValueOfInt(para[i].GetParameter());
                }
            }
        }
    }
}
