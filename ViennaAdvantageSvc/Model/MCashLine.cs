﻿/********************************************************
 * Project Name   : VAdvantage
 * Class Name     : MCashLine
 * Purpose        : Cash Line Model
 * Class Used     : X_C_CashLine
 * Chronological    Development
 * Raghunandan     23-Jun-2009
  ******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Common;
using VAdvantage.Process;
using VAdvantage.ProcessEngine;
using System.Windows.Forms;
using VAdvantage.Model;
using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using VAdvantage.Utility;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using VAdvantage.Logging;


namespace ViennaAdvantage.Model
{
    public class MCashLine : X_C_CashLine
    {
        #region variables
        // Parent				
        private MCash _parent = null;
        // Cash Book			
        private MCashBook _cashBook = null;
        // Bank Account			
        private MBankAccount _bankAccount = null;
        // Invoice				
        private MInvoice _invoice = null;
        // old Vlaues
        decimal old_sdAmt = 0, old_ebAmt = 0, new_sdAmt = 0, new_ebAmt = 0;

        Tuple<String, String, String> mInfo = null;

        #endregion


        /* Standard Constructor
         * @param ctx context
	     *	@param C_CashLine_ID id
	     *	@param trxName transaction
	    */
        public MCashLine(Ctx ctx, int C_CashLine_ID, Trx trxName)
            : base(ctx, C_CashLine_ID, trxName)
        {
            if (C_CashLine_ID == 0)
            {
                //	setLine (0);
                //	setCashType (CASHTYPE_GeneralExpense);
                SetAmount(Env.ZERO);
                SetDiscountAmt(Env.ZERO);
                SetWriteOffAmt(Env.ZERO);
                // Added by Amit 31-7-2015 VAMRP
                //if (Env.HasModulePrefix("VAMRP_", out mInfo))
                //{
                //    SetRETURNLOANAMOUNT(Env.ZERO);
                //}
                //End 
                SetIsGenerated(false);
            }
        }

        /**
         * 	Load Cosntructor
         *	@param ctx context
         *	@param dr result set
         *	@param trxName transaction
         */
        public MCashLine(Ctx ctx, DataRow dr, Trx trxName)
            : base(ctx, dr, trxName)
        {
        }

        /**
         * 	Parent Cosntructor
         *	@param cash parent
         */
        public MCashLine(MCash cash)
            : this(cash.GetCtx(), 0, cash.Get_TrxName())
        {
            SetClientOrg(cash);
            SetC_Cash_ID(cash.GetC_Cash_ID());
            _parent = cash;
            _cashBook = _parent.GetCashBook();
        }

        /**
         * 	Add to Description
         *	@param description text
         */
        public void AddDescription(String description)
        {
            String desc = GetDescription();
            if (desc == null)
                SetDescription(description);
            else
                SetDescription(desc + " | " + description);
        }

        /**
         * 	Set Invoice - no discount
         *	@param invoice invoice
         */
        public void SetInvoice(MInvoice invoice)
        {
            SetC_Invoice_ID(invoice.GetC_Invoice_ID());
            SetCashType(CASHTYPE_Invoice);
            SetC_BPartner_ID(invoice.GetC_BPartner_ID());
            SetC_Currency_ID(invoice.GetC_Currency_ID());
            //	Amount
            MDocType dt = MDocType.Get(GetCtx(), invoice.GetC_DocType_ID());
            Decimal amt = invoice.GetGrandTotal();
            if (MDocBaseType.DOCBASETYPE_APINVOICE.Equals(dt.GetDocBaseType())
                || MDocBaseType.DOCBASETYPE_ARCREDITMEMO.Equals(dt.GetDocBaseType()))
                amt = Decimal.Negate(amt);
            SetAmount(amt);
            //
            SetDiscountAmt(Env.ZERO);
            SetWriteOffAmt(Env.ZERO);
            SetIsGenerated(true);
            _invoice = invoice;
        }

        public void CreateCashLine(MInvoice invoice, int C_InvoicePaySchedule_ID, decimal amt)
        {
            SetC_Invoice_ID(invoice.GetC_Invoice_ID());
            SetC_InvoicePaySchedule_ID(C_InvoicePaySchedule_ID);
            SetCashType(CASHTYPE_Invoice);
            SetC_BPartner_ID(invoice.GetC_BPartner_ID());
            SetC_Currency_ID(invoice.GetC_Currency_ID());
            //	Amount
            MDocType dt = MDocType.Get(GetCtx(), invoice.GetC_DocType_ID());
            if (MDocBaseType.DOCBASETYPE_APINVOICE.Equals(dt.GetDocBaseType())
                || MDocBaseType.DOCBASETYPE_ARCREDITMEMO.Equals(dt.GetDocBaseType()))
            {
                amt = Decimal.Negate(amt);
                SetVSS_PAYMENTTYPE("P");
            }
            else
            {
                SetVSS_PAYMENTTYPE("R");
            }
            SetAmount(amt);
            //
            SetDiscountAmt(Env.ZERO);
            SetWriteOffAmt(Env.ZERO);
            SetIsGenerated(true);
            _invoice = invoice;
        }

        /**
         * 	Set Invoice - Callout
         *	@param oldC_Invoice_ID old BP
         *	@param newC_Invoice_ID new BP
         *	@param windowNo window no
         */
        //@UICallout 
        public void SetC_Invoice_ID(String oldC_Invoice_ID, String newC_Invoice_ID, int windowNo)
        {
            if (newC_Invoice_ID == null || newC_Invoice_ID.Length == 0)
                return;
            int C_Invoice_ID = int.Parse(newC_Invoice_ID);
            if (C_Invoice_ID == 0)
                return;

            //  Date
            DateTime ts = new DateTime(GetCtx().GetContextAsTime(windowNo, "DateAcct"));     //  from C_Cash
            String sql = "SELECT C_BPartner_ID, C_Currency_ID,"		//	1..2
                + "invoiceOpen(C_Invoice_ID, 0), IsSOTrx, "			//	3..4
                + "paymentTermDiscount(invoiceOpen(C_Invoice_ID, 0),C_Currency_ID,C_PaymentTerm_ID,DateInvoiced," + DB.TO_DATE(ts, true) + ") "
                + "FROM C_Invoice WHERE C_Invoice_ID=" + C_Invoice_ID;
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
                    SetC_Currency_ID(Convert.ToInt32(dr[1]));//.getInt(2));
                    Decimal PayAmt = Convert.ToDecimal(dr[2]);//.getBigDecimal(3);
                    Decimal DiscountAmt = Convert.ToDecimal(dr[4]);
                    bool isSOTrx = "Y".Equals(dr[3].ToString());
                    if (!isSOTrx)
                    {
                        PayAmt = Decimal.Negate(PayAmt);
                        DiscountAmt = Decimal.Negate(DiscountAmt);
                    }
                    //
                    SetAmount(Decimal.Subtract(PayAmt, DiscountAmt));
                    SetDiscountAmt(DiscountAmt);
                    SetWriteOffAmt(Env.ZERO);
                    //p_changeVO.setContext(getCtx(), windowNo, "InvTotalAmt", PayAmt);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                log.Log(Level.SEVERE, sql, e);
            }
            finally { dt = null; }
        }

        /**
         * 	Set Order - no discount
         *	@param order order
         *	@param trxName transaction
         */
        public void SetOrder(MOrder order, Trx trxName)
        {
            SetCashType(CASHTYPE_Invoice);
            SetC_Currency_ID(order.GetC_Currency_ID());
            //	Amount
            Decimal amt = order.GetGrandTotal();
            SetAmount(amt);
            SetDiscountAmt(Env.ZERO);
            SetWriteOffAmt(Env.ZERO);
            SetIsGenerated(true);
            //
            if (MOrder.DOCSTATUS_WaitingPayment.Equals(order.GetDocStatus()))
            {
                Save(trxName);
                order.SetC_CashLine_ID(GetC_CashLine_ID());
                //order.ProcessIt(MOrder.ACTION_WaitComplete);
                order.ProcessIt(DocActionVariables.ACTION_WAITCOMPLETE);
                order.Save(trxName);
                //	Set Invoice
                MInvoice[] invoices = order.GetInvoices(true);
                int length = invoices.Length;
                if (length > 0)		//	get last invoice
                {
                    _invoice = invoices[length - 1];
                    SetC_Invoice_ID(_invoice.GetC_Invoice_ID());
                }
            }
        }

        /**
         * 	Set Amount - Callout
         *	@param oldAmount old value
         *	@param newAmount new value
         *	@param windowNo window
         *	@throws Exception
         */
        //@UICallout 
        public void SetAmount(String oldAmount, String newAmount, int windowNo)
        {
            if (newAmount == null || newAmount.Length == 0)
                return;
            Decimal Amount = Convert.ToDecimal(newAmount);
            base.SetAmount(Amount);
            SetAmt(windowNo, "Amount");
        }

        // Added by Amit 31-7-2015 VAMRP
        /// <summary>
        /// Return Amount
        /// </summary>
        /// <param name="RETURNLOANAMOUNT"></param>
        //public void SetRETURNLOANAMOUNT(Decimal? RETURNLOANAMOUNT)
        //{
        //    base.SetRETURNLOANAMOUNT(RETURNLOANAMOUNT == null ? Env.ZERO : (Decimal)RETURNLOANAMOUNT);
        //}

        //public void SetLOAN(bool LOAN)
        //{
        //    base.SetLOAN(LOAN == null ? true : LOAN);
        //}
        // End Amit

        /**
         * 	Set WriteOffAmt - Callout
         *	@param oldWriteOffAmt old value
         *	@param newWriteOffAmt new value
         *	@param windowNo window
         *	@throws Exception
         */
        //@UICallout
        public void SetWriteOffAmt(String oldWriteOffAmt, String newWriteOffAmt, int windowNo)
        {
            if (newWriteOffAmt == null || newWriteOffAmt.Length == 0)
                return;
            Decimal WriteOffAmt = Convert.ToDecimal(newWriteOffAmt);
            base.SetWriteOffAmt(WriteOffAmt);
            SetAmt(windowNo, "WriteOffAmt");
        }

        /**
         * 	Set DiscountAmt - Callout
         *	@param oldDiscountAmt old value
         *	@param newDiscountAmt new value
         *	@param windowNo window
         *	@throws Exception
         */
        //@UICallout
        public void SetDiscountAmt(String oldDiscountAmt, String newDiscountAmt, int windowNo)
        {
            if (newDiscountAmt == null || newDiscountAmt.Length == 0)
                return;
            Decimal DiscountAmt = Convert.ToDecimal(newDiscountAmt);
            base.SetDiscountAmt(DiscountAmt);
            SetAmt(windowNo, "DiscountAmt");
        }

        /**
         * 	Set Amount or WriteOffAmt for Invoices
         *	@param windowNo window
         *	@param columnName source column
         */
        private void SetAmt(int windowNo, String columnName)
        {
            //  Needs to be Invoice
            if (!CASHTYPE_Invoice.Equals(GetCashType()))
                return;
            //  Check, if InvTotalAmt exists
            String total = GetCtx().GetContext(windowNo, "InvTotalAmt");
            if (total == null || total.Length == 0)
                return;
            Decimal InvTotalAmt = Convert.ToDecimal(total);

            Decimal PayAmt = GetAmount();
            Decimal DiscountAmt = GetDiscountAmt();
            Decimal WriteOffAmt = GetWriteOffAmt();
            log.Fine(columnName + " - Invoice=" + InvTotalAmt
                + " - Amount=" + PayAmt + ", Discount=" + DiscountAmt + ", WriteOff=" + WriteOffAmt);

            //  Amount - calculate write off
            if (columnName.Equals("Amount"))
            {
                WriteOffAmt = Decimal.Subtract(InvTotalAmt, Decimal.Subtract(PayAmt, DiscountAmt));
                SetWriteOffAmt(WriteOffAmt);
            }
            else    //  calculate PayAmt
            {
                PayAmt = Decimal.Subtract(InvTotalAmt, Decimal.Subtract(DiscountAmt, WriteOffAmt));
                SetAmount(PayAmt);
            }
        }

        /**
         * 	Get Statement Date from header 
         *	@return date
         */
        public DateTime? GetStatementDate()
        {
            return GetParent().GetStatementDate();
        }

        /**
         * 	Create Line Reversal
         *	@return new reversed CashLine
         */
        public MCashLine CreateReversal()
        {
            MCash parent = GetParent();
            if (parent.IsProcessed())
            {	//	saved
                parent = MCash.Get(GetCtx(), parent.GetAD_Org_ID(),
                    parent.GetStatementDate(), parent.GetC_Currency_ID(), Get_TrxName());
            }
            //
            MCashLine reversal = new MCashLine(parent);
            reversal.SetClientOrg(this);
            reversal.SetC_BankAccount_ID(GetC_BankAccount_ID());
            reversal.SetC_Charge_ID(GetC_Charge_ID());
            reversal.SetC_Currency_ID(GetC_Currency_ID());
            reversal.SetC_Invoice_ID(GetC_Invoice_ID());
            reversal.SetCashType(GetCashType());
            reversal.SetDescription(GetDescription());
            reversal.SetIsGenerated(true);
            //
            reversal.SetAmount(Decimal.Negate(GetAmount()));
            //if (GetDiscountAmt() == null)
            ////    SetDiscountAmt(Env.ZERO);
            //else
            reversal.SetDiscountAmt(Decimal.Negate(GetDiscountAmt()));
            //if (GetWriteOffAmt() == null)
            //    SetWriteOffAmt(Env.ZERO);
            //else
            reversal.SetWriteOffAmt(Decimal.Negate(GetWriteOffAmt()));
            reversal.AddDescription("(" + GetLine() + ")");
            return reversal;
        }


        /**
         * 	Get Cash (parent)
         *	@return cash
         */
        public MCash GetParent()
        {
            if (_parent == null)
                _parent = new MCash(GetCtx(), GetC_Cash_ID(), Get_TrxName());
            return _parent;
        }

        /**
         * 	Get CashBook
         *	@return cash book
         */
        public MCashBook GetCashBook()
        {
            if (_cashBook == null)
                _cashBook = MCashBook.Get(GetCtx(), GetParent().GetC_CashBook_ID());
            return _cashBook;
        }

        /**
         * 	Get Bank Account
         *	@return bank account
         */
        public MBankAccount GetBankAccount()
        {
            if (_bankAccount == null && GetC_BankAccount_ID() != 0)
                _bankAccount = MBankAccount.Get(GetCtx(), GetC_BankAccount_ID());
            return _bankAccount;
        }

        /**
         * 	Get Invoice
         *	@return invoice
         */
        public MInvoice GetInvoice()
        {
            if (_invoice == null && GetC_Invoice_ID() != 0)
                _invoice = MInvoice.Get(GetCtx(), GetC_Invoice_ID());
            return _invoice;
        }

        /**
         * 	Before Delete
         *	@return true/false
         */
        protected override bool BeforeDelete()
        {
            //	Cannot Delete generated Invoices
            Boolean? generated = (Boolean?)Get_ValueOld("IsGenerated");
            if (generated != null && generated.Value)
            {
                if (Get_ValueOld("C_Invoice_ID") != null)
                {
                    log.Warning("Cannot delete line with generated Invoice");
                    return false;
                }
            }
            return true;
        }

        /**
         * 	After Delete
         *	@param success
         *	@return true/false
         */
        protected override bool AfterDelete(bool success)
        {
            if (!success)
                return success;
            return UpdateCbAndLine();
            //return UpdateHeader();
        }



        /**
         * 	Before Save
         *	@param newRecord
         *	@return true/false
         */
        protected override bool BeforeSave(bool newRecord)
        {
            // Added by Amit 1-8-2015 VAMRP
            //if (Env.HasModulePrefix("VAMRP_", out mInfo))
            //{
            //    //for kc
            //    //charge
            //    if (GetCashType() == "C")
            //    {
            //        SetC_Invoice_ID(0);
            //        SetDiscountAmt(0);
            //        SetWriteOffAmt(0);
            //        SetC_BankAccount_ID(0);
            //    }
            //    //invoice
            //    if (GetCashType() == "I")
            //    {
            //        SetC_BPartner_ID(0);
            //        SetC_Charge_ID(0);
            //        SetC_BankAccount_ID(0);
            //    }
            //    //bank a/c transfer
            //    if (GetCashType() == "T")
            //    {
            //        SetC_Invoice_ID(0);
            //        SetDiscountAmt(0);
            //        SetWriteOffAmt(0);
            //        SetC_BPartner_ID(0);
            //        SetC_Charge_ID(0);
            //    }
            //    //genral expense
            //    if (GetCashType() == "E")
            //    {
            //        SetC_Invoice_ID(0);
            //        SetDiscountAmt(0);
            //        SetWriteOffAmt(0);
            //        SetC_BPartner_ID(0);
            //        SetC_Charge_ID(0);
            //        SetC_BankAccount_ID(0);
            //    }
            //    //genral receipt
            //    if (GetCashType() == "R")
            //    {
            //        SetC_Invoice_ID(0);
            //        SetDiscountAmt(0);
            //        SetWriteOffAmt(0);
            //        SetC_BPartner_ID(0);
            //        SetC_Charge_ID(0);
            //        SetC_BankAccount_ID(0);
            //    }
            //    //differennce
            //    if (GetCashType() == "D")
            //    {
            //        SetC_Invoice_ID(0);
            //        SetDiscountAmt(0);
            //        SetWriteOffAmt(0);
            //        SetC_BPartner_ID(0);
            //        SetC_Charge_ID(0);
            //        SetC_BankAccount_ID(0);
            //    }
            //}
            // End

            //	Cannot change generated Invoices
            if (Is_ValueChanged("C_Invoice_ID"))
            {
                Object generated = Get_ValueOld("IsGenerated");
                if (generated != null && ((Boolean)generated))
                {
                    log.Warning("Cannot change line with generated Invoice");
                    return false;
                }
            }

            // during saving a new record, system will check same invoice schedule reference exist on same cash line or not
            if (newRecord && GetCashType() == CASHTYPE_Invoice && GetC_InvoicePaySchedule_ID() > 0)
            {
                if (Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM C_CashLine WHERE C_Cash_ID = " + GetC_Cash_ID() +
                          @" AND IsActive = 'Y' AND C_InvoicePaySchedule_ID = " + GetC_InvoicePaySchedule_ID(), null, Get_Trx())) > 0)
                {
                    log.SaveError("Error", Msg.GetMsg(GetCtx(), "VIS_NotSaveDuplicateRecord"));
                    return false;
                }
            }

            //	Verify CashType
            if (CASHTYPE_Invoice.Equals(GetCashType()) && GetC_Invoice_ID() == 0)
                SetCashType(CASHTYPE_GeneralExpense);
            if (CASHTYPE_BankAccountTransfer.Equals(GetCashType()) && GetC_BankAccount_ID() == 0)
                SetCashType(CASHTYPE_GeneralExpense);
            if (CASHTYPE_Charge.Equals(GetCashType()) && GetC_Charge_ID() == 0)
                SetCashType(CASHTYPE_GeneralExpense);

            bool verify = newRecord
                || Is_ValueChanged("CashType")
                || Is_ValueChanged("C_Invoice_ID")
                || Is_ValueChanged("C_BankAccount_ID");
            if (verify)
            {
                //	Verify Currency
                if (CASHTYPE_BankAccountTransfer.Equals(GetCashType()))
                    SetC_Currency_ID(GetBankAccount().GetC_Currency_ID());
                else if (CASHTYPE_Invoice.Equals(GetCashType()))
                {
                    // Added by Amit 1-8-2015 VAMRP
                    if (Env.HasModulePrefix("VAMRP_", out mInfo))
                    {
                       // SetC_Currency_ID(GetInvoice().GetC_Currency_ID());
                    }
                    else
                    {
                        // Commented To Get the Invoice Open Amount Right in case of Diff Currencies
                        // SetC_Currency_ID(GetInvoice().GetC_Currency_ID());
                    }
                    //end
                }
                else	//	Cash 
                    SetC_Currency_ID(GetCashBook().GetC_Currency_ID());

                //	Set Organization
                if (CASHTYPE_BankAccountTransfer.Equals(GetCashType()))
                    SetAD_Org_ID(GetBankAccount().GetAD_Org_ID());
                //	Cash Book
                else if (CASHTYPE_Invoice.Equals(GetCashType()))
                    SetAD_Org_ID(GetCashBook().GetAD_Org_ID());
                //	otherwise (charge) - leave it
                //	Enforce Org
                if (GetAD_Org_ID() == 0)
                    SetAD_Org_ID(GetParent().GetAD_Org_ID());
            }

            /**	General fix of Currency 
            UPDATE C_CashLine cl SET C_Currency_ID = (SELECT C_Currency_ID FROM C_Invoice i WHERE i.C_Invoice_ID=cl.C_Invoice_ID) WHERE C_Currency_ID IS NULL AND C_Invoice_ID IS NOT NULL;
            UPDATE C_CashLine cl SET C_Currency_ID = (SELECT C_Currency_ID FROM C_BankAccount b WHERE b.C_BankAccount_ID=cl.C_BankAccount_ID) WHERE C_Currency_ID IS NULL AND C_BankAccount_ID IS NOT NULL;
            UPDATE C_CashLine cl SET C_Currency_ID = (SELECT b.C_Currency_ID FROM C_Cash c, C_CashBook b WHERE c.C_Cash_ID=cl.C_Cash_ID AND c.C_CashBook_ID=b.C_CashBook_ID) WHERE C_Currency_ID IS NULL;
            **/

            //	Get Line No
            if (GetLine() == 0)
            {
                String sql = "SELECT COALESCE(MAX(Line),0)+10 FROM C_CashLine WHERE C_Cash_ID=@param1";
                int ii = DB.GetSQLValue(Get_TrxName(), sql, GetC_Cash_ID());
                SetLine(ii);
            }

            // Added by Amit 1-8-2015 VAMRP
            //if (Env.HasModulePrefix("VAMRP_", out mInfo))
            //{
            //    if (GetVSS_RECEIPTNO() == null || GetVSS_RECEIPTNO() == "")
            //    {
            //        MOrg mo = new MOrg(GetCtx(), GetAD_Org_ID(), Get_TrxName());
            //        String org_name = mo.GetName();
            //        //modified by ashish.bisht on 04-feb-10
            //        String paymenttype = GetVSS_PAYMENTTYPE();
            //        String test_name = "DocNo_" + org_name + "_" + paymenttype;

            //        int[] s = MSequence.GetAllIDs("AD_Sequence", "Name= '" + test_name + "'", Get_TrxName());

            //        if (s != null && s.Length != 0)
            //        {
            //            MSequence sqq = new MSequence(GetCtx(), s[0], Get_TrxName());
            //            String ss = sqq.GetName();

            //            if (ss.Equals(test_name))
            //            {
            //                int inc = sqq.GetIncrementNo();
            //                String pre = sqq.GetPrefix();
            //                String suff = sqq.GetSuffix();

            //                int curr = sqq.GetCurrentNext();
            //                curr = curr + inc;
            //                sqq.SetCurrentNext(curr);
            //                sqq.Save();
            //                String StrCurr = "" + curr;

            //                if (pre == null && suff == null)
            //                {
            //                    SetVSS_RECEIPTNO(StrCurr);
            //                }
            //                if (pre != null && suff == null)
            //                {
            //                    SetVSS_RECEIPTNO(pre + StrCurr);
            //                }
            //                if (pre == null && suff != null)
            //                {
            //                    SetVSS_RECEIPTNO(StrCurr + suff);
            //                }

            //                if (pre != null && suff != null)
            //                {
            //                    SetVSS_RECEIPTNO(pre + StrCurr + suff);
            //                }
            //            }
            //        }
            //    }
            //}
            //End
            return true;
        }

        /**
       * 	After Save
       *	@param newRecord
       *	@param success
       *	@return success
       */
        protected override bool AfterSave(bool newRecord, bool success)
        {
            if (!success)
                return success;

            return UpdateCbAndLine();
        }

        private bool UpdateCbAndLine()
        {
            // Update Cash Journal
            if (!UpdateHeader())
            {
                log.Warning("Cannot update cash journal.");
                return false;
            }

            // Update Cashbook and CashbookLine
            MCash parent = GetParent();
            MCashBook cashbook = new MCashBook(GetCtx(), parent.GetC_CashBook_ID(), Get_TrxName());
            if (cashbook.GetCompletedBalance() == 0)
            {
                cashbook.SetCompletedBalance(parent.GetBeginningBalance());
            }
            cashbook.SetRunningBalance(Decimal.Add(Decimal.Subtract(cashbook.GetRunningBalance(), old_ebAmt), new_ebAmt));
            //if (cashbook.GetRunningBalance() == 0)
            //{
            //    cashbook.SetRunningBalance
            //        (Decimal.Add(Decimal.Add(Decimal.Subtract(cashbook.GetRunningBalance(), old_ebAmt), new_ebAmt),cashbook.GetCompletedBalance()));
            //}
            //else
            //{
            //    cashbook.SetRunningBalance(Decimal.Add(Decimal.Subtract(cashbook.GetRunningBalance(), old_ebAmt), new_ebAmt));
            //}

            if (!cashbook.Save())
            {
                log.Warning("Cannot update running balance.");
                return false;
            }

            DataTable dtCashbookLine;
            int C_CASHBOOKLINE_ID = 0;

            string sql = "SELECT C_CASHBOOKLINE_ID FROM C_CASHBOOKLINE WHERE C_CASHBOOK_ID="
                            + cashbook.GetC_CashBook_ID() + " AND DATEACCT="
                            + DB.TO_DATE(parent.GetDateAcct()) + " AND AD_ORG_ID=" + GetAD_Org_ID();

            dtCashbookLine = DB.ExecuteDataset(sql, null, null).Tables[0];

            if (dtCashbookLine.Rows.Count > 0)
            {
                C_CASHBOOKLINE_ID = Util.GetValueOfInt(dtCashbookLine.Rows[0]
                    .ItemArray[0]);
            }

            MCashbookLine cashbookLine = new MCashbookLine(GetCtx(), C_CASHBOOKLINE_ID, Get_TrxName());

            if (C_CASHBOOKLINE_ID == 0)
            {
                cashbookLine.SetC_CashBook_ID(cashbook.GetC_CashBook_ID());
                cashbookLine.SetAD_Org_ID(GetAD_Org_ID());
                cashbookLine.SetAD_Client_ID(GetAD_Client_ID());
                cashbookLine.SetEndingBalance
                    (Decimal.Add(Decimal.Add(Decimal.Subtract(cashbookLine.GetEndingBalance(), old_ebAmt), new_ebAmt), cashbook.GetCompletedBalance()));
            }
            else
            {
                cashbookLine.SetEndingBalance(Decimal.Add(Decimal.Subtract(cashbookLine.GetEndingBalance(), old_ebAmt), new_ebAmt));
            }
            cashbookLine.SetDateAcct(parent.GetDateAcct());
            cashbookLine.SetStatementDifference(Decimal.Add(Decimal.Subtract(cashbookLine.GetStatementDifference(), old_sdAmt), new_sdAmt));


            if (!cashbookLine.Save())
            {
                log.Warning("Cannot create/update cashbook line.");
                return false;
            }

            return true;
        }

        /**
         * 	Update Cash Header.
         * 	Statement Difference, Ending Balance
         *	@return true if success
         */
        private bool UpdateHeader()
        {
            /* jz re-write this SQL because SQL Server doesn't like it
            String sql = "UPDATE C_Cash c"
                + " SET StatementDifference="
                    + "(SELECT COALESCE(SUM(currencyConvert(cl.Amount, cl.C_Currency_ID, cb.C_Currency_ID, c.DateAcct, ";
                    //jz null  //TODO check if 0 is OK with application logic
                    //+ DB.NULL("S", Types.INTEGER)   DB2 function wouldn't take null value for int parameter
            if (DB.isDB2())
                sql += "0";
            else
                sql += "NULL";
            sql += ", c.AD_Client_ID, c.AD_Org_ID)),0) "
                    + "FROM C_CashLine cl, C_CashBook cb "
                    + "WHERE cb.C_CashBook_ID=c.C_CashBook_ID"
                    + " AND cl.C_Cash_ID=c.C_Cash_ID) "
                + "WHERE C_Cash_ID=" + getC_Cash_ID();
            int no = DB.executeUpdate(sql, get_TrxName());
            if (no != 1)
                //log.warning("Difference #" + no);
                */
            String sql = "SELECT COALESCE(SUM(currencyConvert(cl.Amount, cl.C_Currency_ID, cb.C_Currency_ID, c.DateAcct, 0"
                        + ", c.AD_Client_ID, c.AD_Org_ID)),0) "
                    + "FROM C_CashLine cl, C_CashBook cb, C_Cash c "
                    + "WHERE cb.C_CashBook_ID=c.C_CashBook_ID"
                    + " AND cl.C_Cash_ID=c.C_Cash_ID AND "
                    + "c.C_Cash_ID=" + GetC_Cash_ID();
            DataTable dt = null;
            IDataReader idr = DB.ExecuteReader(sql, null, Get_TrxName());
            Decimal sum = Env.ZERO;
            try
            {
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    sum = Convert.ToDecimal(dr[0]);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                log.Severe(e.Message.ToString());
                return false;
            }
            finally { dt = null; }


            // Statement difference and ending balance before update in cash journal.
            DataTable dtOldValues = GetCurrentAmounts();

            if (dtOldValues.Rows.Count > 0)
            {
                old_ebAmt = Util.GetValueOfDecimal(dtOldValues.Rows[0].ItemArray[0]);
                old_sdAmt = Util.GetValueOfDecimal(dtOldValues.Rows[0].ItemArray[1]);
            }

            //	Ending Balance
            sql = "UPDATE C_Cash"
                + " SET EndingBalance = BeginningBalance + @sum ,"
                + " StatementDifference=@sum"
                + " WHERE C_Cash_ID=" + GetC_Cash_ID();

            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@sum", sum);
            //DataSet ds = DB.ExecuteDataset(sql, param);

            int no = DB.ExecuteQuery(sql, param, Get_TrxName());
            if (no != 1)
            {
                log.Warning("Balance #" + no);
            }

            // Statement difference and ending balance after update in cash journal.
            DataTable dtNewValues = GetCurrentAmounts();

            if (dtOldValues.Rows.Count > 0)
            {
                new_ebAmt = Util.GetValueOfDecimal(dtNewValues.Rows[0].ItemArray[0]);
                new_sdAmt = Util.GetValueOfDecimal(dtNewValues.Rows[0].ItemArray[1]);
            }

            return no == 1;
        }

        private DataTable GetCurrentAmounts()
        {
            string sql = "SELECT ENDINGBALANCE,STATEMENTDIFFERENCE FROM C_CASH "
                    + "WHERE C_Cash_ID=" + GetC_Cash_ID();

            return DB.ExecuteDataset(sql, null, Get_TrxName()).Tables[0];
        }

        /**
        * 	Set Invoice - no discount
        *	@param invoice invoice
        */
        public void SetInvoiceMultiCurrency(MInvoice invoice, Decimal Amt, int C_Currency_ID)
        {
            SetC_Invoice_ID(invoice.GetC_Invoice_ID());
            SetCashType(CASHTYPE_Invoice);
            SetC_Currency_ID(C_Currency_ID);
            //	Amount
            MDocType dt = MDocType.Get(GetCtx(), invoice.GetC_DocType_ID());
            Decimal amt = Amt;
            if (MDocBaseType.DOCBASETYPE_APINVOICE.Equals(dt.GetDocBaseType())
                || MDocBaseType.DOCBASETYPE_ARCREDITMEMO.Equals(dt.GetDocBaseType()))
                amt = Decimal.Negate(amt);
            SetAmount(amt);
            //
            SetDiscountAmt(Env.ZERO);
            SetWriteOffAmt(Env.ZERO);
            SetIsGenerated(true);
            _invoice = invoice;
        }

    }
}
