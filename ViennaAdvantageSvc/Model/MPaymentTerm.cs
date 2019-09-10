/********************************************************
 * Module Name    : 
 * Purpose        : 
 * Class Used     : X_C_PaymentTerm
 * Chronological Development
 * Veena Pandey     22-June-2009
 ******************************************************/

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Data;
using VAdvantage.Classes;
using VAdvantage.Utility;
using VAdvantage.DataBase;
using VAdvantage.Common;
using VAdvantage.Logging;

namespace ViennaAdvantage.Model
{
    public class MPaymentTerm : X_C_PaymentTerm
    {
        /** 100									*/
        private const Decimal HUNDRED = 100.0M;

        /**	Payment Schedule children			*/
        private MPaySchedule[] _schedule;

        /**
	     * 	Standard Constructor
	     *	@param ctx context
	     *	@param C_PaymentTerm_ID id
	     *	@param trxName transaction
	     */
        public MPaymentTerm(Ctx ctx, int C_PaymentTerm_ID, Trx trxName)
            : base(ctx, C_PaymentTerm_ID, trxName)
        {
            if (C_PaymentTerm_ID == 0)
            {
                SetAfterDelivery(false);
                SetNetDays(0);
                SetDiscount(Env.ZERO);
                SetDiscount2(Env.ZERO);
                SetDiscountDays(0);
                SetDiscountDays2(0);
                SetGraceDays(0);
                SetIsDueFixed(false);
                SetIsValid(false);
            }
        }

        /**
         * 	Load Constructor
         *	@param ctx context
         *	@param dr result set
         *	@param trxName transaction
         */
        public MPaymentTerm(Ctx ctx, DataRow dr, Trx trxName)
            : base(ctx, dr, trxName)
        {
        }

        /**
	     * 	Get Payment Schedule
	     * 	@param requery if true re-query
	     *	@return array of schedule
	     */
        public MPaySchedule[] GetSchedule(bool requery)
        {
            if (_schedule != null && !requery)
                return _schedule;
            String sql = "SELECT * FROM C_PaySchedule WHERE C_PaymentTerm_ID=" + GetC_PaymentTerm_ID() + " ORDER BY NetDays";
            List<MPaySchedule> list = new List<MPaySchedule>();
            try
            {
                DataSet ds = DB.ExecuteDataset(sql, null, Get_TrxName());
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        MPaySchedule ps = new MPaySchedule(GetCtx(), dr, Get_TrxName());
                        ps.SetParent(this);
                        list.Add(ps);
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, "GetSchedule", e);
            }

            _schedule = new MPaySchedule[list.Count];
            _schedule = list.ToArray();
            return _schedule;
        }

        /**
         * 	Validate Payment Term & Schedule
         *	@return Validation Message @OK@ or error
         */
        public String Validate()
        {
            GetSchedule(true);
            if (_schedule.Length == 0)
            {
                SetIsValid(true);
                return "@OK@";
            }
            if (_schedule.Length == 1)
            {
                SetIsValid(false);
                return "@Invalid@ @Count@ # = 1 (@C_PaySchedule_ID@)";
            }

            //	Add up
            Decimal total = Env.ZERO;
            for (int i = 0; i < _schedule.Length; i++)
            {
                Decimal percent = _schedule[i].GetPercentage();
                // if (percent != null)
                total = Decimal.Add(total, percent);
            }
            bool valid = total.CompareTo(HUNDRED) == 0;
            SetIsValid(valid);
            for (int i = 0; i < _schedule.Length; i++)
            {
                if (_schedule[i].IsValid() != valid)
                {
                    _schedule[i].SetIsValid(valid);
                    _schedule[i].Save();
                }
            }
            String msg = "@OK@";
            if (!valid)
                msg = "@Total@ = " + total + " - @Difference@ = " + Decimal.Subtract(HUNDRED, total);
            return Msg.ParseTranslation(GetCtx(), msg);
        }


        /*************************************************************************
         * 	Apply Payment Term to Invoice -
         *	@param C_Invoice_ID invoice
         *	@return true if payment schedule is valid
         */
        public bool Apply(int C_Invoice_ID)
        {
            MInvoice invoice = new MInvoice(GetCtx(), C_Invoice_ID, Get_TrxName());
            if (invoice == null || invoice.Get_ID() == 0)
            {
                log.Log(Level.SEVERE, "apply - Not valid C_Invoice_ID=" + C_Invoice_ID);
                return false;
            }
            return Apply(invoice);
        }

        /**
         * 	Apply Payment Term to Invoice
         *	@param invoice invoice
         *	@return true if payment schedule is valid
         */
        public bool Apply(MInvoice invoice)
        {
            if (invoice == null || invoice.Get_ID() == 0)
            {
                log.Log(Level.SEVERE, "No valid invoice - " + invoice);
                return false;
            }

            //Added By Vivek on 17/5/2016 fro Advance Payment    
            GetSchedule(true);
            if (invoice.GetC_Order_ID() != 0)
            {
                return ApplyNoSchedule(invoice);
            }
            else
                return ApplySchedule(invoice);
            //End Vivek

            //if (!IsValid())
            //    return ApplyNoSchedule(invoice);
            ////
            //GetSchedule(true);
            //if (_schedule.Length <= 1)
            //    return ApplyNoSchedule(invoice);
            //else	//	only if valid
            //    return ApplySchedule(invoice);
        }

        /**
         * 	Apply Payment Term without schedule to Invoice
         *	@param invoice invoice
         *	@return false as no payment schedule
         */

        #region Advance Payment by Vivek on 16/06/2016

        private bool ApplyNoSchedule(MInvoice invoice)
        {
            int _CountVA009 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA009_'  AND IsActive = 'Y'"));
            if (_CountVA009 > 0)
            {
                DeleteInvoicePaySchedule(invoice.GetC_Invoice_ID(), invoice.Get_TrxName());
                StringBuilder _sql = new StringBuilder();
                MInvoicePaySchedule schedule = null;
                MPaymentTerm payterm = new MPaymentTerm(GetCtx(), invoice.GetC_PaymentTerm_ID(), invoice.Get_TrxName());

                Decimal remainder = invoice.GetGrandTotal();
                int BaseCurrency = 0;

                if (Util.GetValueOfInt(DB.ExecuteScalar("Select Count(*) From C_PaySchedule Where C_PaymentTerm_ID=" + invoice.GetC_PaymentTerm_ID())) > 1)
                {

                    for (int i = 0; i < _schedule.Length; i++)
                    {
                        MPaySchedule _sch = new MPaySchedule(GetCtx(), _schedule[i].GetC_PaySchedule_ID(), Get_TrxName());
                        #region IsAdvance true on Payment Schedule
                        if (_sch.IsVA009_Advance())
                        {
                            #region Copy Order Schedules
                            String sql = "SELECT * FROM VA009_OrderPaySchedule WHERE C_Order_ID=" + invoice.GetC_Order_ID() + " AND VA009_IsPaid='Y' ORDER BY Created";
                            DataSet _ds = new DataSet();

                            _ds = DB.ExecuteDataset(sql, null);
                            if (_ds.Tables[0].Rows.Count > 0 && _ds.Tables[0].Rows != null)
                            {
                                for (int j = 0; j < _ds.Tables[0].Rows.Count; j++)
                                {
                                    schedule = new MInvoicePaySchedule(GetCtx(), 0, invoice.Get_TrxName());
                                    Boolean _isPaid = false;
                                    if (Util.GetValueOfString(_ds.Tables[0].Rows[j]["VA009_IsPaid"]) == "Y")
                                    {
                                        _isPaid = true;
                                    }
                                    schedule.SetAD_Client_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["AD_Client_ID"]));
                                    schedule.SetAD_Org_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["AD_Org_ID"]));
                                    schedule.SetC_Invoice_ID(invoice.GetC_Invoice_ID());
                                    schedule.SetC_DocType_ID(invoice.GetC_DocType_ID());
                                    schedule.SetC_PaymentTerm_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["C_PaymentTerm_ID"]));
                                    schedule.SetVA009_GrandTotal(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["VA009_GrandTotal"]));
                                    schedule.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["VA009_PaymentMethod_ID"]));
                                    schedule.SetDueDate(invoice.GetDateInvoiced());
                                    schedule.SetDueAmt(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["DueAmt"]));
                                    schedule.SetDiscountDate(Util.GetValueOfDateTime(_ds.Tables[0].Rows[j]["DiscountDate"]));
                                    schedule.SetDiscountAmt(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["DiscountAmt"]));
                                    schedule.SetVA009_IsPaid(_isPaid);
                                    schedule.SetC_Payment_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["C_Payment_ID"]));

                                    schedule.SetDiscountDays2(Util.GetValueOfDateTime(_ds.Tables[0].Rows[j]["DiscountDays2"]));
                                    schedule.SetDiscount2(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["Discount2"]));

                                    schedule.SetVA009_PlannedDueDate(Util.GetValueOfDateTime(_ds.Tables[0].Rows[j]["VA009_PlannedDueDate"]));

                                    schedule.SetVA009_OpenAmnt(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["VA009_OpenAmnt"]));


                                    schedule.SetC_Currency_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["C_Currency_ID"]));
                                    schedule.SetVA009_BseCurrncy(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["VA009_bseCurrncy"]));
                                    schedule.SetVA009_OpnAmntInvce(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["VA009_OpnAmntInvce"]));
                                    schedule.SetC_BPartner_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[j]["C_Bpartner_ID"]));

                                    schedule.SetVA009_FollowupDate(Util.GetValueOfDateTime(_ds.Tables[0].Rows[j]["VA009_FollowUpDate"]));
                                    schedule.SetVA009_PaymentMode(Util.GetValueOfString(_ds.Tables[0].Rows[j]["va009_paymentmode"]));
                                    schedule.SetVA009_PaymentType(Util.GetValueOfString(_ds.Tables[0].Rows[j]["va009_paymenttype"]));
                                    schedule.SetVA009_PaymentTrigger(Util.GetValueOfString(_ds.Tables[0].Rows[j]["va009_paymenttrigger"]));
                                    schedule.SetVA009_ExecutionStatus(Util.GetValueOfString(_ds.Tables[0].Rows[j]["VA009_ExecutionStatus"]));
                                    schedule.SetProcessed(true);
                                    if (!schedule.Save(invoice.Get_TrxName()))
                                    {
                                        return false;
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region IsAdvance false on Payment Schedule
                        else
                        {
                            schedule = new MInvoicePaySchedule(invoice, _schedule[i]);

                            schedule.SetVA009_ExecutionStatus("A");
                            schedule.SetAD_Client_ID(invoice.GetAD_Client_ID());
                            schedule.SetAD_Org_ID(invoice.GetAD_Org_ID());
                            schedule.SetC_Invoice_ID(invoice.GetC_Invoice_ID());
                            schedule.SetC_DocType_ID(invoice.GetC_DocType_ID());

                            MOrder _Order = new MOrder(GetCtx(), invoice.GetC_Order_ID(), Get_TrxName());
                            schedule.SetVA009_PaymentMethod_ID(invoice.GetVA009_PaymentMethod_ID());
                            schedule.SetC_PaymentTerm_ID(invoice.GetC_PaymentTerm_ID());
                            schedule.SetVA009_GrandTotal(invoice.GetGrandTotal());

                            MPaymentTerm paytrm = new MPaymentTerm(GetCtx(), invoice.GetC_PaymentTerm_ID(), Get_TrxName());
                            int _graceDay = paytrm.GetGraceDays();
                            //DateTime? _followUpDay = GetDueDate(invoice);
                            schedule.SetVA009_FollowupDate(schedule.GetDueDate().Value.AddDays(_graceDay));
                            //schedule.SetVA009_PlannedDueDate(GetDueDate(invoice));
                            schedule.SetVA009_PlannedDueDate(schedule.GetDueDate());
                            //schedule.SetDueDate(GetDueDate(invoice));

                            //change by amit 25-11-2015
                            StringBuilder _sqlBsCrrncy = new StringBuilder();
                            _sql.Clear();
                            _sqlBsCrrncy.Append(@"SELECT UNIQUE asch.C_Currency_ID FROM c_acctschema asch INNER JOIN ad_clientinfo ci ON ci.c_acctschema1_id = asch.c_acctschema_id
                                 INNER JOIN ad_client c ON c.ad_client_id = ci.ad_client_id INNER JOIN c_invoice i ON c.ad_client_id    = i.ad_client_id
                                 WHERE i.ad_client_id = " + invoice.GetAD_Client_ID());
                            BaseCurrency = Util.GetValueOfInt(DB.ExecuteScalar(_sqlBsCrrncy.ToString(), null, null));

                            if (BaseCurrency != invoice.GetC_Currency_ID())
                            {
                                _sqlBsCrrncy.Clear();
                                _sqlBsCrrncy.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE AD_Client_ID = " + invoice.GetAD_Client_ID() + " AND  c_currency_id  = " + invoice.GetC_Currency_ID() +
                                              " AND c_currency_to_id = " + BaseCurrency + " AND " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                                decimal multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sqlBsCrrncy.ToString(), null, null));
                                if (multiplyRate == 0)
                                {
                                    _sqlBsCrrncy.Clear();
                                    _sqlBsCrrncy.Append(@"SELECT dividerate FROM c_conversion_rate WHERE AD_Client_ID = " + invoice.GetAD_Client_ID() + " AND c_currency_id  = " + BaseCurrency +
                                                  " AND c_currency_to_id = " + invoice.GetC_Currency_ID() + " AND " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                                    multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sqlBsCrrncy.ToString(), null, null));
                                }
                                schedule.SetVA009_OpenAmnt(schedule.GetDueAmt() * multiplyRate);
                            }
                            else
                            {
                                schedule.SetVA009_OpenAmnt(schedule.GetDueAmt());
                            }

                            schedule.SetC_Currency_ID(invoice.GetC_Currency_ID());
                            schedule.SetVA009_BseCurrncy(BaseCurrency);
                            schedule.SetVA009_OpnAmntInvce(schedule.GetDueAmt());
                            schedule.SetC_BPartner_ID(invoice.GetC_BPartner_ID());
                            //end

                            string _sqlPaymentMthd = "Select va009_paymentmode, va009_paymenttype, va009_paymenttrigger  From va009_paymentmethod where va009_paymentmethod_ID=" + invoice.GetVA009_PaymentMethod_ID() + "   AND IsActive = 'Y' AND AD_Client_ID = " + invoice.GetAD_Client_ID();
                            DataSet dsPayMthd = new DataSet();
                            dsPayMthd = DB.ExecuteDataset(_sqlPaymentMthd);
                            if (dsPayMthd.Tables != null && dsPayMthd.Tables.Count > 0 && dsPayMthd.Tables[0].Rows.Count > 0)
                            {
                                for (int j = 0; j < dsPayMthd.Tables[0].Rows.Count; j++)
                                {
                                    schedule.SetVA009_PaymentMode(Util.GetValueOfString(dsPayMthd.Tables[0].Rows[j]["va009_paymentmode"]));
                                    schedule.SetVA009_PaymentType(Util.GetValueOfString(dsPayMthd.Tables[0].Rows[j]["va009_paymenttype"]));
                                    schedule.SetVA009_PaymentTrigger(Util.GetValueOfString(dsPayMthd.Tables[0].Rows[j]["va009_paymenttrigger"]));
                                    schedule.SetVA009_ExecutionStatus("A");
                                }
                            }
                            schedule.SetProcessed(true);
                            if (!schedule.Save(invoice.Get_TrxName()))
                            {
                                return false;
                            }
                        }
                    }
                        #endregion

                    if (invoice.GetC_PaymentTerm_ID() != GetC_PaymentTerm_ID())
                        invoice.SetC_PaymentTerm_ID(GetC_PaymentTerm_ID());
                    return invoice.ValidatePaySchedule();

                }
                else if (Util.GetValueOfInt(DB.ExecuteScalar("Select Count(*) From C_PaySchedule Where C_PaymentTerm_ID=" + invoice.GetC_PaymentTerm_ID())) <= 1)
                {

                    schedule = new MInvoicePaySchedule(GetCtx(), 0, Get_TrxName());

                    schedule.SetAD_Client_ID(invoice.GetAD_Client_ID());
                    schedule.SetAD_Org_ID(invoice.GetAD_Org_ID());
                    schedule.SetC_Invoice_ID(invoice.GetC_Invoice_ID());
                    schedule.SetC_DocType_ID(invoice.GetC_DocType_ID());
                    schedule.SetC_PaymentTerm_ID(invoice.GetC_PaymentTerm_ID());
                    schedule.SetVA009_GrandTotal(invoice.GetGrandTotal());
                    schedule.SetVA009_PaymentMethod_ID(invoice.GetVA009_PaymentMethod_ID());
                    schedule.SetDueDate(GetDueDate(invoice));
                    schedule.SetDueAmt(invoice.GetGrandTotal());
                    schedule.SetDiscountDate(invoice.GetDateInvoiced().Value.AddDays(Util.GetValueOfInt(payterm.GetDiscountDays())));
                    schedule.SetDiscountAmt((Util.GetValueOfDecimal((invoice.GetGrandTotal() * payterm.GetDiscount()) / 100)));

                    schedule.SetDiscountDays2(invoice.GetDateInvoiced().Value.AddDays(Util.GetValueOfInt(payterm.GetDiscountDays2())));
                    schedule.SetDiscount2((Util.GetValueOfDecimal((invoice.GetGrandTotal() * payterm.GetDiscount2()) / 100)));

                    schedule.SetVA009_PlannedDueDate(GetDueDate(invoice));

                    _sql.Clear();
                    _sql.Append(@"SELECT UNIQUE asch.C_Currency_ID FROM c_acctschema asch INNER JOIN ad_clientinfo ci ON ci.c_acctschema1_id = asch.c_acctschema_id
                                 INNER JOIN ad_client c ON c.ad_client_id = ci.ad_client_id INNER JOIN c_invoice i ON c.ad_client_id    = i.ad_client_id
                                 WHERE i.ad_client_id = " + invoice.GetAD_Client_ID());
                    BaseCurrency = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));

                    if (BaseCurrency != invoice.GetC_Currency_ID())
                    {
                        // change by amit
                        _sql.Clear();
                        _sql.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE AD_Client_ID = " + GetCtx().GetAD_Client_ID() + " AND c_currency_id  = " + invoice.GetC_Currency_ID() +
                                      " AND c_currency_to_id = " + BaseCurrency + " AND  " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                        decimal multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                        if (multiplyRate == 0)
                        {
                            _sql.Clear();
                            _sql.Append(@"SELECT dividerate FROM c_conversion_rate WHERE AD_Client_ID = " + GetCtx().GetAD_Client_ID() + " AND c_currency_id  = " + BaseCurrency +
                                          " AND c_currency_to_id = " + invoice.GetC_Currency_ID() + " AND  " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                            multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                        }
                        schedule.SetVA009_OpenAmnt(invoice.GetGrandTotal() * multiplyRate);
                    }
                    else
                    {
                        schedule.SetVA009_OpenAmnt(invoice.GetGrandTotal());
                    }

                    schedule.SetC_Currency_ID(invoice.GetC_Currency_ID());
                    schedule.SetVA009_BseCurrncy(BaseCurrency);
                    schedule.SetVA009_OpnAmntInvce(invoice.GetGrandTotal());
                    schedule.SetC_BPartner_ID(invoice.GetC_BPartner_ID());

                    MOrder _Order = new MOrder(GetCtx(), invoice.GetC_Order_ID(), Get_TrxName());
                    //schedule.SetVA009_PaymentMethod_ID(_Order.GetVA009_PaymentMethod_ID());
                    //schedule.SetC_PaymentTerm_ID(_Order.GetC_PaymentTerm_ID());

                    int _graceDay = payterm.GetGraceDays();
                    DateTime? _followUpDay = GetDueDate(invoice);
                    schedule.SetVA009_FollowupDate(_followUpDay.Value.AddDays(_graceDay));
                    _sql.Clear();
                    _sql.Append("Select va009_paymentmode, va009_paymenttype, va009_paymenttrigger  From va009_paymentmethod where va009_paymentmethod_ID=" + invoice.GetVA009_PaymentMethod_ID() + "   AND IsActive = 'Y' AND AD_Client_ID = " + invoice.GetAD_Client_ID());
                    DataSet ds = new DataSet();
                    ds = DB.ExecuteDataset(_sql.ToString());
                    if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            schedule.SetVA009_PaymentMode(Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymentmode"]));
                            schedule.SetVA009_PaymentType(Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymenttype"]));
                            schedule.SetVA009_PaymentTrigger(Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymenttrigger"]));
                            schedule.SetVA009_ExecutionStatus("A");
                        }
                    }
                    if (!schedule.Save())
                    {
                        return false;
                    }
                    if (invoice.GetC_PaymentTerm_ID() != GetC_PaymentTerm_ID())
                        invoice.SetC_PaymentTerm_ID(GetC_PaymentTerm_ID());
                    return invoice.ValidatePaySchedule();
                }
            }
            return true;
        }

        #endregion

        private DateTime? GetDueDate(MInvoice invoice)
        {
            MPaymentTerm payterm = new MPaymentTerm(GetCtx(), invoice.GetC_PaymentTerm_ID(), Get_TrxName());
            String _sql = "Select PAYMENTTERMDUEDATE (C_PaymentTerm_ID, DATEINVOICED) as DueDate from C_invoice where  C_invoice_ID=" + invoice.GetC_Invoice_ID();
            DateTime? _dueDate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sql.ToString(), null, Get_TrxName()));
            if (_dueDate == Util.GetValueOfDateTime("1/1/0001 12:00:00 AM"))
                _dueDate = DateTime.Now;
            return _dueDate;
        }

        /**
         * 	Apply Payment Term with schedule to Invoice
         *	@param invoice invoice
         *	@return true if payment schedule is valid
         */

        #region Advance Payment by Vivek on 16/06/2016

        private bool ApplySchedule(MInvoice invoice)
        {
            //	Create Schedule
            DeleteInvoicePaySchedule(invoice.GetC_Invoice_ID(), invoice.Get_TrxName());
            Decimal remainder = invoice.GetGrandTotal();
            MPaymentTerm payterm = new MPaymentTerm(GetCtx(), invoice.GetC_PaymentTerm_ID(), invoice.Get_TrxName());
            StringBuilder _sql = new StringBuilder();
            int BaseCurrency = 0;
            MInvoicePaySchedule schedule = null;
            int _CountVA009 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA009_'  AND IsActive = 'Y'"));
            if (_CountVA009 > 0)
            {
                if (Util.GetValueOfInt(DB.ExecuteScalar("Select Count(*) From C_PaySchedule Where C_PaymentTerm_ID=" + invoice.GetC_PaymentTerm_ID())) <= 1)
                {

                    schedule = new MInvoicePaySchedule(GetCtx(), 0, Get_TrxName());

                    schedule.SetAD_Client_ID(invoice.GetAD_Client_ID());
                    schedule.SetAD_Org_ID(invoice.GetAD_Org_ID());
                    schedule.SetC_Invoice_ID(invoice.GetC_Invoice_ID());
                    schedule.SetC_DocType_ID(invoice.GetC_DocType_ID());
                    schedule.SetC_PaymentTerm_ID(invoice.GetC_PaymentTerm_ID());
                    schedule.SetVA009_GrandTotal(invoice.GetGrandTotal());
                    schedule.SetVA009_PaymentMethod_ID(invoice.GetVA009_PaymentMethod_ID());
                    schedule.SetDueDate(GetDueDate(invoice));
                    schedule.SetDueAmt(invoice.GetGrandTotal());
                    schedule.SetDiscountDate(invoice.GetDateInvoiced().Value.AddDays(Util.GetValueOfInt(payterm.GetDiscountDays())));
                    schedule.SetDiscountAmt((Util.GetValueOfDecimal((invoice.GetGrandTotal() * payterm.GetDiscount()) / 100)));

                    schedule.SetDiscountDays2(invoice.GetDateInvoiced().Value.AddDays(Util.GetValueOfInt(payterm.GetDiscountDays2())));
                    schedule.SetDiscount2((Util.GetValueOfDecimal((invoice.GetGrandTotal() * payterm.GetDiscount2()) / 100)));

                    schedule.SetVA009_PlannedDueDate(GetDueDate(invoice));

                    _sql.Clear();
                    _sql.Append(@"SELECT UNIQUE asch.C_Currency_ID FROM c_acctschema asch INNER JOIN ad_clientinfo ci ON ci.c_acctschema1_id = asch.c_acctschema_id
                                 INNER JOIN ad_client c ON c.ad_client_id = ci.ad_client_id INNER JOIN c_invoice i ON c.ad_client_id    = i.ad_client_id
                                 WHERE i.ad_client_id = " + invoice.GetAD_Client_ID());
                    BaseCurrency = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));

                    if (BaseCurrency != invoice.GetC_Currency_ID())
                    {
                        // change by amit
                        _sql.Clear();
                        _sql.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE AD_Client_ID = " + GetCtx().GetAD_Client_ID() + " AND c_currency_id  = " + invoice.GetC_Currency_ID() +
                                      " AND c_currency_to_id = " + BaseCurrency + " AND  " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                        decimal multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                        if (multiplyRate == 0)
                        {
                            _sql.Clear();
                            _sql.Append(@"SELECT dividerate FROM c_conversion_rate WHERE AD_Client_ID = " + GetCtx().GetAD_Client_ID() + " AND c_currency_id  = " + BaseCurrency +
                                          " AND c_currency_to_id = " + invoice.GetC_Currency_ID() + " AND  " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                            multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                        }
                        schedule.SetVA009_OpenAmnt(invoice.GetGrandTotal() * multiplyRate);
                    }
                    else
                    {
                        schedule.SetVA009_OpenAmnt(invoice.GetGrandTotal());
                    }

                    schedule.SetC_Currency_ID(invoice.GetC_Currency_ID());
                    schedule.SetVA009_BseCurrncy(BaseCurrency);
                    schedule.SetVA009_OpnAmntInvce(invoice.GetGrandTotal());
                    schedule.SetC_BPartner_ID(invoice.GetC_BPartner_ID());

                    MOrder _Order = new MOrder(GetCtx(), invoice.GetC_Order_ID(), Get_TrxName());
                    //schedule.SetVA009_PaymentMethod_ID(_Order.GetVA009_PaymentMethod_ID());
                    //schedule.SetC_PaymentTerm_ID(_Order.GetC_PaymentTerm_ID());

                    int _graceDay = payterm.GetGraceDays();
                    DateTime? _followUpDay = GetDueDate(invoice);
                    schedule.SetVA009_FollowupDate(_followUpDay.Value.AddDays(_graceDay));
                    _sql.Clear();
                    _sql.Append("Select va009_paymentmode, va009_paymenttype, va009_paymenttrigger  From va009_paymentmethod where va009_paymentmethod_ID=" + invoice.GetVA009_PaymentMethod_ID() + "   AND IsActive = 'Y' AND AD_Client_ID = " + invoice.GetAD_Client_ID());
                    DataSet ds = new DataSet();
                    ds = DB.ExecuteDataset(_sql.ToString());
                    if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            schedule.SetVA009_PaymentMode(Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymentmode"]));
                            schedule.SetVA009_PaymentType(Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymenttype"]));
                            schedule.SetVA009_PaymentTrigger(Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymenttrigger"]));
                            schedule.SetVA009_ExecutionStatus("A");
                        }
                    }
                    if (!schedule.Save())
                    {
                        return false;
                    }
                    if (invoice.GetC_PaymentTerm_ID() != GetC_PaymentTerm_ID())
                        invoice.SetC_PaymentTerm_ID(GetC_PaymentTerm_ID());
                    return invoice.ValidatePaySchedule();
                }
                else
                {
                    for (int i = 0; i < _schedule.Length; i++)
                    {
                        schedule = new MInvoicePaySchedule(invoice, _schedule[i]);

                        schedule.SetVA009_ExecutionStatus("A");
                        schedule.SetAD_Client_ID(invoice.GetAD_Client_ID());
                        schedule.SetAD_Org_ID(invoice.GetAD_Org_ID());
                        schedule.SetC_Invoice_ID(invoice.GetC_Invoice_ID());
                        schedule.SetC_DocType_ID(invoice.GetC_DocType_ID());

                        MOrder _Order = new MOrder(GetCtx(), invoice.GetC_Order_ID(), Get_TrxName());
                        schedule.SetVA009_PaymentMethod_ID(invoice.GetVA009_PaymentMethod_ID());
                        schedule.SetC_PaymentTerm_ID(invoice.GetC_PaymentTerm_ID());
                        schedule.SetVA009_GrandTotal(invoice.GetGrandTotal());

                        MPaymentTerm paytrm = new MPaymentTerm(GetCtx(), invoice.GetC_PaymentTerm_ID(), Get_TrxName());
                        int _graceDay = paytrm.GetGraceDays();
                        //DateTime? _followUpDay = GetDueDate(invoice);
                        schedule.SetVA009_FollowupDate(schedule.GetDueDate().Value.AddDays(_graceDay));
                        //schedule.SetVA009_PlannedDueDate(GetDueDate(invoice));
                        schedule.SetVA009_PlannedDueDate(schedule.GetDueDate());
                        //schedule.SetDueDate(GetDueDate(invoice));

                        //change by amit 25-11-2015
                        StringBuilder _sqlBsCrrncy = new StringBuilder();
                        _sql.Clear();
                        _sqlBsCrrncy.Append(@"SELECT UNIQUE asch.C_Currency_ID FROM c_acctschema asch INNER JOIN ad_clientinfo ci ON ci.c_acctschema1_id = asch.c_acctschema_id
                                 INNER JOIN ad_client c ON c.ad_client_id = ci.ad_client_id INNER JOIN c_invoice i ON c.ad_client_id    = i.ad_client_id
                                 WHERE i.ad_client_id = " + invoice.GetAD_Client_ID());
                        BaseCurrency = Util.GetValueOfInt(DB.ExecuteScalar(_sqlBsCrrncy.ToString(), null, null));


                        if (BaseCurrency != invoice.GetC_Currency_ID())
                        {
                            // change by amit
                            _sql.Clear();
                            _sql.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE AD_Client_ID = " + GetCtx().GetAD_Client_ID() + " AND c_currency_id  = " + invoice.GetC_Currency_ID() +
                                          " AND c_currency_to_id = " + BaseCurrency + " AND  " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                            decimal multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                            if (multiplyRate == 0)
                            {
                                _sql.Clear();
                                _sql.Append(@"SELECT dividerate FROM c_conversion_rate WHERE AD_Client_ID = " + GetCtx().GetAD_Client_ID() + " AND c_currency_id  = " + BaseCurrency +
                                              " AND c_currency_to_id = " + invoice.GetC_Currency_ID() + " AND  " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                                multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                            }
                            schedule.SetVA009_OpenAmnt(invoice.GetGrandTotal() * multiplyRate);
                        }
                        else
                        {
                            schedule.SetVA009_OpenAmnt(invoice.GetGrandTotal());
                        }

                        schedule.SetC_Currency_ID(invoice.GetC_Currency_ID());
                        schedule.SetVA009_BseCurrncy(BaseCurrency);
                        schedule.SetVA009_OpnAmntInvce(invoice.GetGrandTotal());
                        schedule.SetC_BPartner_ID(invoice.GetC_BPartner_ID());

                        //schedule.SetVA009_PaymentMethod_ID(_Order.GetVA009_PaymentMethod_ID());
                        //schedule.SetC_PaymentTerm_ID(_Order.GetC_PaymentTerm_ID());

                        _sql.Clear();
                        _sql.Append("Select va009_paymentmode, va009_paymenttype, va009_paymenttrigger  From va009_paymentmethod where va009_paymentmethod_ID=" + invoice.GetVA009_PaymentMethod_ID() + "   AND IsActive = 'Y' AND AD_Client_ID = " + invoice.GetAD_Client_ID());
                        DataSet ds = new DataSet();
                        ds = DB.ExecuteDataset(_sql.ToString());
                        if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                            {
                                schedule.SetVA009_PaymentMode(Util.GetValueOfString(ds.Tables[0].Rows[j]["va009_paymentmode"]));
                                schedule.SetVA009_PaymentType(Util.GetValueOfString(ds.Tables[0].Rows[j]["va009_paymenttype"]));
                                schedule.SetVA009_PaymentTrigger(Util.GetValueOfString(ds.Tables[0].Rows[j]["va009_paymenttrigger"]));
                                schedule.SetVA009_ExecutionStatus("A");
                            }
                        }
                        if (!schedule.Save())
                        {
                            return false;
                        }
                        remainder = Decimal.Subtract(remainder, schedule.GetDueAmt());
                    }
                    if (remainder.CompareTo(Env.ZERO) != 0 && schedule != null)
                    {
                        schedule.SetDueAmt(Decimal.Add(schedule.GetDueAmt(), remainder));
                        schedule.Save(invoice.Get_TrxName());
                        log.Fine("Remainder=" + remainder + " - " + schedule);
                    }
                    if (invoice.GetC_PaymentTerm_ID() != GetC_PaymentTerm_ID())
                        invoice.SetC_PaymentTerm_ID(GetC_PaymentTerm_ID());
                    return invoice.ValidatePaySchedule();
                }
            }
            return true;
        }

        #endregion
        /**
         * 	Delete existing Invoice Payment Schedule
         *	@param C_Invoice_ID id
         *	@param trxName transaction
         */
        private void DeleteInvoicePaySchedule(int C_Invoice_ID, Trx trxName)
        {
            String sql = "DELETE FROM C_InvoicePaySchedule WHERE C_Invoice_ID=" + C_Invoice_ID;
            int no = DB.ExecuteQuery(sql, null, trxName);
            log.Fine("C_Invoice_ID=" + C_Invoice_ID + " - #" + no);
        }


        /**************************************************************************
         * 	String Representation
         *	@return info
         */
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("MPaymentTerm[");
            sb.Append(Get_ID()).Append("-").Append(GetName())
                .Append(",Valid=").Append(IsValid())
                .Append("]");
            return sb.ToString();
        }

        /**
         * 	Before Save
         *	@param newRecord new
         *	@return true
         */
        protected override bool BeforeSave(bool newRecord)
        {
            if (IsDueFixed())
            {
                int dd = GetFixMonthDay();
                if (dd < 1 || dd > 31)
                {
                    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@Invalid@ @FixMonthDay@"));
                    return false;
                }
                dd = GetFixMonthCutoff();
                if (dd < 1 || dd > 31)
                {
                    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@Invalid@ @FixMonthCutoff@"));
                    return false;
                }
            }

            if (!newRecord || !IsValid())
                Validate();
            return true;
        }
    }
}
