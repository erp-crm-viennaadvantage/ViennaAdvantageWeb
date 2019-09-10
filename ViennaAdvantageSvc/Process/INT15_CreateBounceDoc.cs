using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.Model;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace ViennaAdvantage.Process
{
   public class INT15_CreateBounceDoc : SvrProcess
    {
        string reviseddoc = string.Empty;
        string _message = "";        
        public const String REVERSE_INDICATOR = "^";
        private DateTime? datetrx;
        string msg = "Error while creating Bounce Doc";

        protected override string DoIt()
        {
            ViennaAdvantage.Model.MPayment payment = new ViennaAdvantage.Model.MPayment(GetCtx(), GetRecord_ID(), Get_Trx());
            ViennaAdvantage.Model.MPayment newpayment = new ViennaAdvantage.Model.MPayment(GetCtx(), 0, Get_Trx());


            string paybounce = "select INT15_ISBOUNCED from c_payment WHERE C_PAYMENT_ID=" + payment.GetC_Payment_ID();
            string paybnc = Util.GetValueOfString(DB.ExecuteScalar(paybounce));

            if (paybnc == "Y")
            {
                //  _log.Info("Material Receipt not completed for this Record ID = " + payment.GetDocumentNo());
                return Msg.GetMsg(GetCtx(), "Document Already Marked As Bounced ");
            }


            if (payment.IsReconciled())
            {
                return Msg.GetMsg(GetCtx(), "Document Already Reconciled ");
            }

            if (payment.IsAllocated())
            {
                return Msg.GetMsg(GetCtx(), "Document Already Allocated,Delete allocation and Try again ");
            }



            string paymenttype = "select VA009_PaymentBaseType from VA009_PaymentMethod WHERE VA009_PaymentMethod_ID=" + payment.GetVA009_PaymentMethod_ID();
            string paymethod = Util.GetValueOfString(DB.ExecuteScalar(paymenttype));


            if (paymethod == "S" && payment.GetDocStatus() == "CO")
            {
                newpayment.SetAD_Client_ID(payment.GetAD_Client_ID());
                newpayment.SetAD_Org_ID(payment.GetAD_Org_ID());
                newpayment.SetC_DocType_ID(payment.GetC_DocType_ID());
                newpayment.SetDateTrx(payment.GetDateTrx());
                newpayment.SetDateAcct(payment.GetDateTrx());
                newpayment.SetDescription(payment.GetDescription());
                newpayment.SetC_BankAccount_ID(payment.GetC_BankAccount_ID());
                newpayment.SetC_BPartner_ID(payment.GetC_BPartner_ID());
                newpayment.SetC_BPartner_Location_ID(payment.GetC_BPartner_Location_ID());
                newpayment.SetC_Invoice_ID(payment.GetC_Invoice_ID());
                newpayment.SetC_InvoicePaySchedule_ID(payment.GetC_InvoicePaySchedule_ID());
                newpayment.SetC_Charge_ID(payment.GetC_Charge_ID());
                newpayment.SetC_Tax_ID(payment.GetC_Tax_ID());
                newpayment.SetC_Currency_ID(payment.GetC_Currency_ID());
                newpayment.SetC_ConversionType_ID(payment.GetC_ConversionType_ID());
                newpayment.SetRoutingNo(payment.GetRoutingNo());
                newpayment.SetAccountNo(payment.GetAccountNo());
                newpayment.SetPayAmt(-1 * payment.GetPayAmt());
                newpayment.SetOverUnderAmt(payment.GetOverUnderAmt());
                newpayment.SetDiscountAmt(payment.GetDiscountAmt());
                newpayment.SetWriteOffAmt(payment.GetWriteOffAmt());
                newpayment.SetVA009_PaymentMethod_ID(payment.GetVA009_PaymentMethod_ID());
                newpayment.SetDRAFTDATE(payment.GetDRAFTDATE());
                newpayment.SetVA009_ExecutionStatus(payment.GetVA009_ExecutionStatus());
                newpayment.SetPOReference(payment.GetPOReference());
                newpayment.SetCheckNo(payment.GetCheckNo() + "111");

                newpayment.Set_Value("VA034_DepositSlipNo", payment.Get_Value("VA034_DepositSlipNo"));
                //newpayment.SetDateTrx(DateTime.Now);
                //newpayment.SetDateAcct(DateTime.Now);
                //newpayment.SetCheckDate(payment.GetCheckDate());
                //newpayment.SetDRAFTDATE(payment.GetDRAFTDATE());
                newpayment.Set_Value("VA034_DepositSlipNo", (payment.Get_Value("VA034_DepositSlipNo")));

                newpayment.SetDateTrx(DateTime.Now);
                newpayment.SetDateAcct(DateTime.Now);
                newpayment.SetCheckDate(datetrx);
                newpayment.SetDRAFTDATE(datetrx);

                newpayment.SetVA009_ExecutionStatus("B");
                newpayment.SetINT15_IsBounced(true);


                reviseddoc = payment.GetDocumentNo();

                // Set revised Document no 
                if (reviseddoc.Contains("->"))
                {
                    _message = Msg.GetMsg(GetCtx(), "INT15_Documentstatus");
                }
                else
                {
                    reviseddoc = reviseddoc + "->";
                }
                newpayment.AddDescription("{->" + payment.GetDocumentNo() + ")" + " Bounced Payment");
                newpayment.SetDocumentNo(reviseddoc);
                newpayment.Save(Get_Trx());
                if (newpayment.ProcessIt("CO"))
                {
                    newpayment.SetDocStatus("CO");
                    newpayment.SetDocAction("CL");
                    newpayment.Save(Get_Trx());
                }
                payment.Set_Value("INT15_IsBounced", true);
                payment.Save();
                if (!payment.Save())
                {
                    log.SaveError("Error While Creating Bounce Doc", "");
                    return msg;
                }
                return Msg.GetMsg(GetCtx(), "") + reviseddoc;
            }
           return Msg.GetMsg(GetCtx(), "");

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
                else if (name.Equals("datetrx"))
                {
                    
                    datetrx =  Util.GetValueOfDateTime(para[i].GetParameter());
                }
                else
                {
                    log.Log(VAdvantage.Logging.Level.SEVERE, "prepare - Unknown Parameter: " + name);
                }
            }
        }
    }

}
