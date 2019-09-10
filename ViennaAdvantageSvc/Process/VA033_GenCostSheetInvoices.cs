/********************************************************
 * Project Name   : Landed Cost Creation
 * Class Name     : VA033_GenCostSheetInvoices
 * Purpose        : Create Invoices 
 * Class Used     : ProcessEngine.SvrProcess
 * Chronological    Development
 * Vikas          : 23-Aug-2016
  ******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Common;
using VAdvantage.Process;
using VAdvantage.Model;
using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using VAdvantage.Utility;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using VAdvantage.ProcessEngine;
//using ViennaAdvantage.Model;
using VAdvantage.Logging;


namespace ViennaAdvantage.Process
{
    public class VA033_GenCostSheetInvoices : SvrProcess
    {
        string _docstatus = "CO";
        protected override string DoIt()
        {
            #region Commented
            //string _sql = "";
            //int _count =  0;
            //string _invoiceNo = null;
            //string _invDocStatus = null;
            //MInOut ship = null;
            //MInvoice invoice = null;
            //List<int> c_bpID = null;
            //int _CountVA009 = 0;
            //int _CountVA026 = 0;
            //int _cTaxID = 0;
            //int _InvComplete = 0;
            //int _InvNotComplete = 0;
            //int _InvoiceDocStatus = 0;

            //// GET COST SHEET ID
            //#region Check Conditions

            ////Commented by Manjot
            // //_sql = " SELECT  VA033_CostSheet_ID FROM M_inout WHERE  ISACTIVE='Y' AND  M_inout_ID=" + GetRecord_ID();
            // //_CostSheet_ID = Util.GetValueOfInt(DB.ExecuteScalar(_sql, null, null));
            ////End

            ////Added By Manjot
            //ship = new MInOut(GetCtx(), GetRecord_ID(), null);

            //// check cost Sheet Invoice Setting
            // _sql = " SELECT  count(*) FROM VA033_CostSheet WHERE  ISACTIVE='Y' AND VA033_InvoiceSetting='A' AND VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID();
            // _count = Util.GetValueOfInt(DB.ExecuteScalar(_sql, null, null));
            // if (_count == 0)
            // {
            //     return Msg.GetMsg(Env.GetCtx(), "VA033_CostSheetInvSetting");
            // }


            //// check Number of Type Invoices On Cost Sheet
            // _sql = " SELECT  count(*) FROM VA033_CostSheetLine WHERE VA033_Type=1 AND ISACTIVE='Y' AND  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID();
            //_count = Util.GetValueOfInt(DB.ExecuteScalar(_sql, null, null));
            //if (_count > 1)
            //{
            //    return Msg.GetMsg(Env.GetCtx(), "VA033_TypeInvoiceFoundMultiple");
            //}
            //if (_count <= 0)
            //{
            //    return Msg.GetMsg(Env.GetCtx(), "VA033_TypeInvoiceLineNotFound");
            //}


            //// check Invoice Documnet No Already Exist or Not
            //_sql = null;
            //_count = 0;
            //_sql = @" SELECT csl.va033_invoiceno FROM va033_costsheetline csl INNER JOIN c_invoice inv on (csl.va033_invoiceno=inv.documentno) WHERE inv.issotrx='N' "
            //      + " AND inv.isreturntrx='N' and inv.isactive='Y' and csl.isactive='Y' and csl.va033_costsheet_id=" + ship.GetVA033_CostSheet_ID();

            //_count = Util.GetValueOfInt(DB.ExecuteScalar(_sql, null, null));
            //if (_count > 0)
            //{
            //    return Msg.GetMsg(Env.GetCtx(), "VA033_InvNumberAlreadyExist" + _count);
            //}

            ////Check Date or Invoice no. combination unique or not  when Type Landed
            //_sql = null;
            //_count = 0;
            //_sql = @"SELECT count(*) as total FROM VA033_CostSheetLine WHERE VA033_Type=2 and  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID()
            //                + "group by VA033_InvoiceNo, va033_invdate HAVING count(*) >1";
            //_count = Util.GetValueOfInt(DB.ExecuteScalar(_sql, null, null));
            //if (_count > 1)
            //{
            //    return Msg.GetMsg(Env.GetCtx(), "VA033_InvnoAndDateUnique" );
            //}

            //#endregion

            ////Create Invoice for Type "Invoice" 
            //#region Invoice Type Creation
            //_sql = null;
            //_count = 0;
            //_sql = " SELECT  VA033_InvoiceNo, C_Tax_ID FROM VA033_CostSheetLine WHERE VA033_Type=1 AND ISACTIVE='Y' AND  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID();
            ////added by Manjot For Set Tax
            //   DataSet ds = DB.ExecuteDataset(_sql, null, null);
            //   if (ds != null && ds.Tables[0].Rows.Count > 0)
            //   {
            //       _invoiceNo = Util.GetValueOfString(ds.Tables[0].Rows[0][0]);
            //       _cTaxID = Util.GetValueOfInt(ds.Tables[0].Rows[0][1]);
            //   }

            //if (_invoiceNo != null)
            //{
            //    //ship = new MInOut(GetCtx(), GetRecord_ID(), null); //Commented by Manjot
            //    invoice = new MInvoice(ship, null);
            //    //-------------Column Added by Anuj----------------------
            //    _CountVA009 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA009_'  AND IsActive = 'Y'"));
            //    if (_CountVA009 > 0)
            //    {
            //        int _PaymentMethod_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select VA009_PaymentMethod_ID From C_Order Where C_Order_ID=" + ship.GetC_Order_ID()));
            //        if (_PaymentMethod_ID > 0)
            //        {
            //            invoice.SetVA009_PaymentMethod_ID(_PaymentMethod_ID);
            //        }
            //    }
            //    //-------------Column Added by Anuj----------------------
            //    // added by Amit 26-may-2016
            //    _CountVA026 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA026_'  AND IsActive = 'Y'"));
            //    if (_CountVA026 > 0)
            //    {
            //        MOrder order = new MOrder(GetCtx(), ship.GetC_Order_ID(), Get_TrxName());
            //        if (order != null && order.GetC_Order_ID() > 0)
            //        {
            //            invoice.SetVA026_LCDetail_ID(order.GetVA026_LCDetail_ID());
            //        }
            //    }
            //    //end
            //    if (ship.IsReturnTrx())
            //    {
            //        invoice.SetC_DocTypeTarget_ID(ship.IsSOTrx() ? MDocBaseType.DOCBASETYPE_ARCREDITMEMO : MDocBaseType.DOCBASETYPE_APCREDITMEMO);
            //    }
            //    if (_invoiceNo != null && _invoiceNo.Length > 0)
            //    {
            //        invoice.SetDocumentNo(_invoiceNo);
            //    }
            //    invoice.SetSalesRep_ID(GetCtx().GetAD_User_ID());
            //    if (!invoice.Save())
            //    {
            //        throw new ArgumentException("Cannot save Invoice");
            //    }

            //    MInOutLine[] shipLines = ship.GetLines(false);
            //    for (int i = 0; i < shipLines.Length; i++)
            //    {
            //        MInOutLine sLine = shipLines[i];
            //        MInvoiceLine line = new MInvoiceLine(invoice);
            //        line.SetShipLine(sLine);
            //        line.SetQtyEntered(sLine.GetQtyEntered());
            //        line.SetQtyInvoiced(sLine.GetMovementQty());

            //        if (_cTaxID > 0)
            //            line.SetC_Tax_ID(_cTaxID);

            //        if (line.GetC_Tax_ID() > 0) {
            //            line.SetC_Tax_ID(0);
            //        }
            //        if (!line.Save())
            //        {
            //            throw new ArgumentException("Cannot save Invoice Line");
            //            log.Info(" Cost Sheet Invoice Line Not Save ");
            //        }
            //    }
            //    _cTaxID = 0;
            //    if (_docstatus == "CO")
            //    {
            //        invoice.SetDocStatus("CO");
            //        _invDocStatus = invoice.CompleteIt();
            //        log.Info(" Cost Sheet Invoice Completion DocStatus=" + _invDocStatus + " and Invoice ID=" + invoice.GetC_Invoice_ID());
            //        if (_invDocStatus == "CO")
            //        {
            //            invoice.SetDocStatus("CO");
            //            invoice.Save();
            //            _InvComplete++;
            //        }
            //        else
            //        {
            //            invoice.SetDocStatus(_invDocStatus);
            //            invoice.Save();
            //            _InvNotComplete++;

            //        }
            //    }
            //    else { invoice.SetDocStatus("CO"); invoice.CompleteIt(); }
            //}// if 1
            //#endregion

            ////Landed Cost Sheet Invoices Creation
            //#region Landed Cost Sheet Invoices Creation
            //_sql = null;
            //c_bpID = new List<int>();
            //int _LcSave = 0;
            //int _LcNotSave = 0;
            //DataSet _dsCostLine = null;
            //// count no. of Invoices Create for landed Cost sheet
            //_sql = @"SELECT count(*) as total, C_BPartner_ID,C_BPartner_Location_ID,VA033_InvoiceNo , C_Tax_ID  FROM VA033_CostSheetLine WHERE VA033_Type=2 and  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID()
            //          + " group by C_BPartner_ID,C_BPartner_Location_ID,VA033_InvoiceNo,C_Tax_ID order by C_BPartner_ID";
            //DataSet _dsLandedCost = DB.ExecuteDataset(_sql, null, null);
            //if (_dsLandedCost.Tables.Count > 0)
            //{
            //    if (_dsLandedCost.Tables[0].Rows.Count > 0)
            //    {
            //        for (int i = 0; i < _dsLandedCost.Tables[0].Rows.Count; i++)
            //        {
            //            int _cb_ID = 0;
            //            int _cbLc_ID = 0;
            //            string _invNo = null;
            //            int _invLineTotal = 0;
            //            int _pl_ID = 0;
            //            int _paymentterm = 0;
            //            decimal _amt = 0;

            //            _invLineTotal = Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["total"]);
            //            _cb_ID = Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["C_BPartner_ID"]);
            //            _cbLc_ID = Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["C_BPartner_Location_ID"]);
            //            _invNo = Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"]);
            //            //Added By Manjot to Get Tax
            //            _cTaxID = Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["C_Tax_ID"]);

            //            #region Check BPartner
            //            if (!c_bpID.Contains(_cb_ID))
            //            {
            //                c_bpID.Add(_cb_ID);
            //                invoice = new MInvoice(ship, null);

            //                _CountVA009 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA009_'  AND IsActive = 'Y'"));
            //                if (_CountVA009 > 0)
            //                {
            //                    int _PaymentMethod_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select VA009_PaymentMethod_ID From C_Order Where C_Order_ID=" + ship.GetC_Order_ID()));
            //                    if (_PaymentMethod_ID > 0)
            //                    {
            //                        invoice.SetVA009_PaymentMethod_ID(_PaymentMethod_ID);
            //                    }

            //                }
            //                //-------------Column Added by Anuj----------------------
            //                // added by Amit 26-may-2016
            //                _CountVA026 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA026_'  AND IsActive = 'Y'"));
            //                if (_CountVA026 > 0)
            //                {
            //                    MOrder order = new MOrder(GetCtx(), ship.GetC_Order_ID(), Get_Trx());
            //                    if (order != null && order.GetC_Order_ID() > 0)
            //                    {
            //                        invoice.SetVA026_LCDetail_ID(order.GetVA026_LCDetail_ID());
            //                    }
            //                }
            //                //end
            //                if (ship.IsReturnTrx())
            //                {
            //                    invoice.SetC_DocTypeTarget_ID(ship.IsSOTrx() ? MDocBaseType.DOCBASETYPE_ARCREDITMEMO : MDocBaseType.DOCBASETYPE_APCREDITMEMO);
            //                }

            //                if (_invNo != null && _invNo.Length > 0)
            //                {
            //                    invoice.SetDocumentNo(_invNo);
            //                }

            //                invoice.SetC_BPartner_ID(_cb_ID);
            //                invoice.SetC_BPartner_Location_ID(_cbLc_ID);
            //                invoice.SetAD_User_ID(GetCtx().GetAD_User_ID());
            //                //Price List
            //                _pl_ID = Util.GetValueOfInt(DB.ExecuteScalar(" Select po_pricelist_id from c_bpartner where  IsActive = 'Y' AND C_BPartner_ID=" + _cb_ID));
            //                if (_pl_ID > 0)
            //                {
            //                    invoice.SetM_PriceList_ID(_pl_ID);
            //                }
            //                else
            //                {
            //                    _pl_ID = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT m_pricelist_ID FROM  m_pricelist WHERE isdefault='Y' AND issopricelist='N' AND IsActive = 'Y' "));
            //                    if (_pl_ID > 0)
            //                    {
            //                        invoice.SetM_PriceList_ID(_pl_ID);
            //                    }
            //                    else
            //                    {
            //                        log.Info(" Cost Sheet Price List Not Found");
            //                    }
            //                }

            //                //payment term
            //                _paymentterm = Util.GetValueOfInt(DB.ExecuteScalar(" Select c_paymentterm_id from c_bpartner where  IsActive = 'Y' AND C_BPartner_ID=" + _cb_ID));
            //                if (_paymentterm > 0)
            //                {
            //                    invoice.SetM_PriceList_ID(_paymentterm);
            //                }
            //                else
            //                {
            //                    _paymentterm = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT c_paymentterm_id FROM  c_paymentterm WHERE isdefault='Y' AND IsActive = 'Y' AND ad_client_id=" + GetCtx().GetAD_Client_ID()));
            //                    if (_paymentterm > 0)
            //                    {
            //                        invoice.SetM_PriceList_ID(_paymentterm);
            //                    }
            //                    else
            //                    {
            //                        log.Info(" Cost Sheet Payment Term Not Found ");
            //                    }
            //                }
            //                //Date
            //                _sql = null;
            //                _sql = @" SELECT VA033_InvDate,M_Product_ID,C_Charge_ID,M_CostElement_ID,VA033_LandedCostDisbtn from va033_costsheetline WHERE  VA033_Type=2 AND  C_BPartner_ID=" + _cb_ID + " AND C_BPartner_Location_ID=" + _cbLc_ID + " AND  VA033_InvoiceNo='" + _invNo + "' AND  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID();
            //                _dsCostLine = DB.ExecuteDataset(_sql, null, Get_TrxName());
            //                if (_dsCostLine.Tables.Count > 0)
            //                {
            //                    if (_dsCostLine.Tables[0].Rows.Count > 0)
            //                    {
            //                        invoice.SetDateInvoiced(Util.GetValueOfDateTime(_dsCostLine.Tables[0].Rows[0]["VA033_InvDate"]));
            //                        invoice.SetDateAcct(Util.GetValueOfDateTime(_dsCostLine.Tables[0].Rows[0]["VA033_InvDate"]));
            //                    }
            //                }
            //                //Amt
            //                _sql = null;
            //                _sql = " SELECT Sum(VA033_InvAmt) from va033_costsheetline WHERE  VA033_Type=2 AND  C_BPartner_ID=" + _cb_ID + " AND C_BPartner_Location_ID=" + _cbLc_ID + " AND  VA033_InvoiceNo='" + _invNo + "' AND  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID();
            //                _amt = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql, null, Get_TrxName()));
            //                invoice.SetGrandTotal(_amt);
            //                invoice.SetTotalLines(_amt);
            //                if (!invoice.Save())
            //                {
            //                    throw new ArgumentException("Cannot save Invoice");
            //                }
            //            }
            //            #endregion

            //            MInvoiceLine line = null;
            //            MLandedCost lc = null;
            //            MLandedCost _lc = null;
            //            if (_invLineTotal == 1)
            //            {
            //                line = new MInvoiceLine(invoice);
            //                line.SetQtyEntered(1);
            //                line.SetM_Product_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[0]["M_Product_ID"]));
            //                line.SetC_Charge_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[0]["C_Charge_ID"]));
            //                line.SetPrice(_amt);
            //                line.SetPriceActual(_amt);
            //                line.SetLineNetAmt(_amt);
            //                //Added by Manjot to set Tax ID
            //                if (_cTaxID > 0)
            //                    line.SetC_Tax_ID(_cTaxID);

            //                if (!line.Save())
            //                {
            //                    throw new ArgumentException("Cannot save Invoice Line");
            //                    log.Info(" Cost Sheet Invoice Line Not Save ");
            //                }
            //                // 4th Tab 
            //                lc = new MLandedCost(GetCtx(), 0, Get_TrxName());
            //                lc.SetAD_Client_ID(invoice.GetAD_Client_ID());
            //                lc.SetAD_Org_ID(invoice.GetAD_Org_ID());
            //                lc.SetC_InvoiceLine_ID(line.GetC_InvoiceLine_ID());
            //                lc.SetLandedCostDistribution(Util.GetValueOfString(_dsCostLine.Tables[0].Rows[0]["VA033_LandedCostDisbtn"]));
            //                lc.SetM_CostElement_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[0]["M_CostElement_ID"]));
            //                lc.SetM_InOut_ID(GetRecord_ID());
            //                if (!lc.Save())
            //                {
            //                    log.Info(" Landed Cost Not Save ");
            //                }
            //                else
            //                {

            //                    _lc = new MLandedCost(GetCtx(), lc.GetC_LandedCost_ID(), Get_TrxName());
            //                    log.Info(_lc.ToString());
            //                    if (_lc.Get_ID() == 0)
            //                    {
            //                        throw new Exception("@NotFound@: @C_LandedCost_ID@ - " + lc.GetC_LandedCost_ID());
            //                    }

            //                    String error = _lc.AllocateCosts();
            //                    if (error == null || error.Length == 0)
            //                    {
            //                        _LcSave++;
            //                        if (_docstatus == "CO")
            //                        {
            //                            //invoice.SetDocStatus("CO");
            //                            //_invDocStatus = invoice.CompleteIt();
            //                            log.Info(" Cost Sheet Invoice Completion DocStatus=" + _invDocStatus + " and Invoice ID=" + invoice.GetC_Invoice_ID());
            //                            if (_invDocStatus == "CO")
            //                            {
            //                                invoice.SetDocStatus(_invDocStatus);
            //                                invoice.Save();
            //                                _InvComplete++;
            //                            }
            //                            else
            //                            {
            //                                invoice.SetDocStatus(_invDocStatus);
            //                                invoice.Save();
            //                                _InvNotComplete++;
            //                            }
            //                        }
            //                        // return "@OK@";
            //                    }
            //                    else
            //                    {
            //                        _LcNotSave++;
            //                    }
            //                    //
            //                }
            //            }
            //            if (_invLineTotal > 1)
            //            {

            //                i = 0;
            //                for (i = 0; i < _dsCostLine.Tables[0].Rows.Count; i++)
            //                {
            //                    line = new MInvoiceLine(invoice);
            //                    line.SetQtyEntered(1);
            //                    line.SetM_Product_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[i]["M_Product_ID"]));
            //                    line.SetC_Charge_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[i]["C_Charge_ID"]));
            //                    line.SetPrice(_amt);
            //                    line.SetPriceActual(_amt);
            //                    line.SetLineNetAmt(_amt);

            //                    //Added by Manjot to set Tax ID
            //                    if (_cTaxID > 0)
            //                        line.SetC_Tax_ID(_cTaxID);

            //                    if (!line.Save())
            //                    {
            //                        throw new ArgumentException("Cannot save Invoice Line");
            //                        log.Info(" Cost Sheet Invoice Line Not Save ");
            //                    }
            //                    // 4th Tab 
            //                    lc = new MLandedCost(GetCtx(), 0, Get_TrxName());
            //                    lc.SetAD_Client_ID(invoice.GetAD_Client_ID());
            //                    lc.SetAD_Org_ID(invoice.GetAD_Org_ID());
            //                    lc.SetC_InvoiceLine_ID(line.GetC_InvoiceLine_ID());
            //                    lc.SetLandedCostDistribution(Util.GetValueOfString(_dsCostLine.Tables[0].Rows[0]["VA033_LandedCostDisbtn"]));
            //                    lc.SetM_CostElement_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[0]["M_CostElement_ID"]));
            //                    lc.SetM_InOut_ID(GetRecord_ID());
            //                    if (!lc.Save())
            //                    {
            //                        log.Info(" Landed Cost Not Save ");
            //                    }
            //                    else
            //                    {
            //                        if (_invDocStatus == "CO")
            //                        { 
            //                            invoice.SetDocStatus(_invDocStatus);
            //                             invoice.Save();
            //                            _InvComplete++;
            //                        }
            //                        else
            //                        {
            //                             invoice.SetDocStatus(_invDocStatus);
            //                             invoice.Save();
            //                            _InvNotComplete++;
            //                        }
            //                        _lc = new MLandedCost(GetCtx(), lc.GetC_LandedCost_ID(), Get_TrxName());
            //                        log.Info(_lc.ToString());
            //                        if (_lc.Get_ID() == 0)
            //                        {
            //                            throw new Exception("@NotFound@: @C_LandedCost_ID@ - " + lc.GetC_LandedCost_ID());
            //                        }

            //                        String error = _lc.AllocateCosts();
            //                        if (error == null || error.Length == 0)
            //                        {
            //                            _LcSave++;
            //                            if (_docstatus == "CO")
            //                            {
            //                                //invoice.SetDocStatus("CO");
            //                                //_invDocStatus = invoice.CompleteIt();
            //                                log.Info(" Cost Sheet Invoice Completion DocStatus=" + _invDocStatus + " and Invoice ID=" + invoice.GetC_Invoice_ID());
            //                                if (_invDocStatus == "CO")
            //                                {
            //                                     invoice.SetDocStatus(_invDocStatus);
            //                                     invoice.Save();
            //                                    _InvComplete++;
            //                                }
            //                                else
            //                                {
            //                                     invoice.SetDocStatus(_invDocStatus);
            //                                     invoice.Save();
            //                                    _InvNotComplete++;
            //                                }
            //                            }
            //                            // return "@OK@";
            //                        }
            //                        else
            //                        {
            //                            _LcNotSave++;
            //                        }
            //                    }
            //                }
            //            }
            //            //landed cost done
            //            if (_docstatus == "CO")
            //            {
            //                if (_InvNotComplete > 0)
            //                {
            //                    if (_LcNotSave > 0)
            //                    {
            //                        return Msg.GetMsg(Env.GetCtx(), "VA033_SomeInoviceNotGen");
            //                    }
            //                    return Msg.GetMsg(Env.GetCtx(), "VA033_SomeInoviceNotComplete");
            //                }
            //                else if (_LcNotSave > 0)
            //                {
            //                    return Msg.GetMsg(Env.GetCtx(), "VA033_SomeInoviceNotComplete");
            //                }
            //                else if (_InvNotComplete == 0 && _LcNotSave == 0)
            //                {
            //                    return Msg.GetMsg(Env.GetCtx(), "VA033_InvoiceGenerated");
            //                }
            //            }
            //            else
            //            {
            //                if (_LcNotSave > 0)
            //                {
            //                    return Msg.GetMsg(Env.GetCtx(), "VA033_SomeInoviceNotGen");
            //                }
            //                else {
            //                    invoice.SetDocStatus("CO");
            //                    invoice.CompleteIt();
            //                }
            //            }
            //           // return _invDocStatus;
            //        }
            //        //return Msg.GetMsg(Env.GetCtx(), "VA033_InvoiceGenerated");
            //    }
            //}
            //    // When Landed Cost Not Found
            //else
            //{
            //    _sql = null;
            //    if (_dsLandedCost != null)
            //        _dsLandedCost.Dispose();
            //    if (_docstatus == "CO")
            //    {
            //        if (_InvComplete > 1)
            //        {
            //            return Msg.GetMsg(Env.GetCtx(), "VA033_InvoiceGenerated");
            //        }
            //    }
            //    else
            //    {
            //        if (_InvNotComplete > 1)
            //            return Msg.GetMsg(Env.GetCtx(), "VA033_InvoiceNotGenerated"); 
            //    }
            //}
            //#endregion

            //return "";
            #endregion
            return DOItNew();
        }

        public string DOItNew()
        {
            #region Variables

            StringBuilder _sql = new StringBuilder();
            MInOut ship = null;
            int _count = 0;
            MInvoice invoice = null;
            List<string> c_bpInvNum = null;
            string _invoiceNo = null;
            decimal _CS_InvAmt = 0;
            DateTime? TrxDate = null;
            int _cTaxID = 0;
            int _CountVA009 = 0;
            int _CountVA026 = 0;
            int C_Currency_ID = 0;
            string _invDocStatus = null;
            string msg = null;
            //int _InvComplete = 0;
            //int _InvNotComplete = 0;
            #endregion

            //Added By Manjot
            ship = new MInOut(GetCtx(), GetRecord_ID(), Get_Trx());

            // work only for Purchase side
            if (!(!ship.IsSOTrx() && !ship.IsReturnTrx()))
            {
                return "";
            }

            #region Checking Conditions
            // check cost Sheet Invoice Setting
            _sql.Clear();
            _sql.Append(" SELECT  count(*) FROM VA033_CostSheet WHERE  ISACTIVE='Y' AND VA033_InvoiceSetting='A' AND VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID());
            _count = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));
            if (_count == 0)
            {
                return Msg.GetMsg(Env.GetCtx(), "VA033_CostSheetInvSetting");
            }
            //_sql.Clear();
            //_sql.Append(" SELECT  C_Currency_ID FROM VA033_CostSheet WHERE  ISACTIVE='Y' AND VA033_InvoiceSetting='A' AND VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID());
            //C_Currency_ID = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));

            // check Number of Type Invoices On Cost Sheet
            _sql.Clear();
            _sql.Append(" SELECT  count(*) FROM VA033_CostSheetLine WHERE VA033_Type=1 AND ISACTIVE='Y' AND  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID());
            _count = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));
            if (_count > 1)
            {
                return Msg.GetMsg(Env.GetCtx(), "VA033_TypeInvoiceFoundMultiple");
            }
            if (_count <= 0)
            {
                return Msg.GetMsg(Env.GetCtx(), "VA033_TypeInvoiceLineNotFound");
            }

            // check Invoice Documnet No Already Exist or Not
            _sql.Clear();
            _count = 0;
            _sql.Append(@" SELECT Count(csl.va033_invoiceno) FROM va033_costsheetline csl INNER JOIN c_invoice inv on (csl.va033_invoiceno=inv.documentno) WHERE inv.issotrx='N' "
                  + " AND inv.isreturntrx='N' and inv.isactive='Y' and csl.isactive='Y' and inv.docstatus NOT in ('VO','RE') and csl.va033_costsheet_id=" + ship.GetVA033_CostSheet_ID());

            _count = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));
            if (_count > 0)
            {
                return Msg.GetMsg(Env.GetCtx(), "VA033_InvNumberAlreadyExist" + _count);
            }

            //Check Date or Invoice no. combination unique or not  when Type Landed
            _sql.Clear();
            _count = 0;
            //_sql.Append(@"SELECT count(*) as total FROM VA033_CostSheetLine WHERE VA033_Type=2 and  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID()
            //                + "group by VA033_InvoiceNo, va033_invdate HAVING count(*) >1");
            //_count = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));
            //if (_count > 1)
            //{
            //    return Msg.GetMsg(Env.GetCtx(), "VA033_InvnoAndDateUnique");
            //}

            _sql.Clear();
            #endregion

            #region Create Invoice for Type "Invoice"
            _sql.Clear();
            _count = 0;
            _sql.Append(" SELECT  csl.VA033_InvoiceNo, csl.C_Tax_ID, csl.VA033_InvAmt,cs.VA033_TrxDate FROM VA033_CostSheetLine csl Inner join VA033_CostSheet cs ON cs.VA033_CostSheet_ID=csl.VA033_CostSheet_ID  "
                                + " WHERE csl.VA033_Type=1 AND csl.ISACTIVE='Y' AND  csl.VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID());
            //added by Manjot For Set Tax
            DataSet ds = DB.ExecuteDataset(_sql.ToString(), null, null);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                _invoiceNo = Util.GetValueOfString(ds.Tables[0].Rows[0][0]);
                _cTaxID = Util.GetValueOfInt(ds.Tables[0].Rows[0][1]);
                _CS_InvAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[0][2]);
                TrxDate = Util.GetValueOfDateTime(ds.Tables[0].Rows[0][3]);
            }
            if (_invoiceNo != null)
            {
                //ship = new MInOut(GetCtx(), GetRecord_ID(), null); //Commented by Manjot
                invoice = new MInvoice(GetCtx(), 0, Get_Trx());
                invoice.SetClientOrg(ship.GetAD_Client_ID(), ship.GetAD_Org_ID());
                invoice.SetShipment(ship);	//	set base settings
                invoice.SetC_DocTypeTarget_ID();
                //invoice.SetDateAcct(DateTime.Now);
                _sql.Clear();
                _sql.Append(@" SELECT VA033_InvDate from va033_costsheetline WHERE  VA033_Type=1  AND  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID());
                DateTime? dt = Util.GetValueOfDateTime(DB.ExecuteScalar(_sql.ToString(), null, null));
                if (dt != null)
                {
                    invoice.SetDateInvoiced(dt);
                    invoice.SetDateAcct(dt);
                }
                else
                {
                    invoice.SetDateInvoiced(TrxDate);
                    invoice.SetDateAcct(TrxDate);
                }

                //invoice.SetDateAcct(TrxDate);
                invoice.SetSalesRep_ID(ship.GetSalesRep_ID());
                invoice.SetAD_User_ID(ship.GetAD_User_ID());


                //-------------Column Added by Anuj----------------------
                _CountVA009 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA009_'  AND IsActive = 'Y'"));
                if (_CountVA009 > 0)
                {
                    int _PaymentMethod_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select VA009_PaymentMethod_ID From C_Order Where C_Order_ID=" + ship.GetC_Order_ID()));
                    if (_PaymentMethod_ID > 0)
                    {
                        invoice.SetVA009_PaymentMethod_ID(_PaymentMethod_ID);
                    }
                }

                MOrder order = new MOrder(GetCtx(), ship.GetC_Order_ID(), Get_Trx());

                //-------------Column Added by Anuj----------------------
                // added by Amit 26-may-2016
                _CountVA026 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA026_'  AND IsActive = 'Y'"));
                if (_CountVA026 > 0)
                {
                    if (order != null && order.GetC_Order_ID() > 0)
                    {
                        invoice.SetVA026_LCDetail_ID(order.GetVA026_LCDetail_ID());
                    }
                }
                //end
                if (ship.IsReturnTrx())
                {
                    invoice.SetC_DocTypeTarget_ID(ship.IsSOTrx() ? MDocBaseType.DOCBASETYPE_ARCREDITMEMO : MDocBaseType.DOCBASETYPE_APCREDITMEMO);
                }

                int c_doctype_id = GetC_DocTypeTarget_ID(ship);
                if (c_doctype_id > 0)
                {
                    invoice.SetC_DocTypeTarget_ID(c_doctype_id);
                }
                else
                {
                    log.Info(Msg.GetMsg(Env.GetCtx(), "GOM01_InvDoctypenotfound"));
                    return Msg.GetMsg(Env.GetCtx(), "GOM01_InvDoctypenotfound");
                }

                if (_invoiceNo != null && _invoiceNo.Length > 0)
                {
                    invoice.SetDocumentNo(_invoiceNo);
                }
                invoice.SetSalesRep_ID(GetCtx().GetAD_User_ID());
                if (!invoice.Save(Get_Trx()))
                {
                    Get_Trx().Rollback();
                    log.Info(" Cost Sheet Invoice Not Saved ");
                    return "Cannot save Invoice";
                }
                decimal Total_MR_Value = 0, PO_Price = 0, prc = 0;
                Total_MR_Value = Util.GetValueOfDecimal(DB.ExecuteScalar(@"SELECT SUM(IL.QtyEntered*cl.PriceEntered)as TotalPrc FROM 
                                                                           M_INOUTLINE IL INNER JOIN C_ORDERLINE CL ON cl.C_ORDERLINE_ID
                                                                           =il.C_ORDERLINE_ID WHERE IL.M_INOUT_ID=" + ship.GetM_InOut_ID(), null, Get_Trx()));
                MInOutLine[] shipLines = ship.GetLines(false);
                for (int i = 0; i < shipLines.Length; i++)
                {
                    MInOutLine sLine = shipLines[i];
                    MInvoiceLine line = new MInvoiceLine(invoice);
                    line.SetShipLine(sLine);
                    line.SetQtyEntered(sLine.GetQtyEntered());
                    line.SetQtyInvoiced(sLine.GetQtyEntered());
                    PO_Price = Util.GetValueOfDecimal(DB.ExecuteScalar("SELECT PriceEntered FROM c_orderline WHERE C_ORDERLINE_ID= " + sLine.GetC_OrderLine_ID(), null, null));

                    prc = CalculateEachPrice(_CS_InvAmt, Total_MR_Value, PO_Price, sLine.GetQtyEntered());
                    if (prc > 0)
                    {
                        line.SetPriceActual(prc);
                        line.SetPriceList(prc);
                        line.SetPriceEntered(prc);
                        line.SetPriceLimit(prc);
                    }

                    if (_cTaxID > 0)
                        line.SetC_Tax_ID(_cTaxID);

                    if (!line.Save(Get_Trx()))
                    {
                        log.Info(" Cost Sheet Invoice Line Not Saved ");
                        return "Cannot save Invoice Line";

                    }
                }
                invoice.Save(Get_Trx());
                if (_docstatus == "CO")
                {
                    if (invoice.ProcessIt("CO"))
                    {
                        invoice.SetDocStatus("CO");
                        invoice.SetDocAction("CL");
                    }
                }
                else
                {
                    if (invoice.ProcessIt("PR"))
                    {
                        invoice.SetDocStatus("DR");
                        invoice.SetDocAction("PR");
                    }
                }
                if (!invoice.Save(Get_Trx()))
                { }
                Get_Trx().Commit();
                //Get_Trx().Close();
                log.Info(" Cost Sheet Invoice Completion DocStatus=" + _invDocStatus + " and Invoice ID=" + invoice.GetC_Invoice_ID());
                _cTaxID = 0;
                if (String.IsNullOrEmpty(msg))
                {
                    msg = Msg.GetMsg(GetCtx(), "GOM01_CSInvoiceGen") + " = " + invoice.GetDocumentNo();
                }
                else
                {
                    msg = msg + "," + invoice.GetDocumentNo();
                }
            }
            #endregion

            #region Landed Cost Sheet Invoices Creation
            _sql.Clear();
            c_bpInvNum = new List<string>();
            invoice = null;
            ship = null;
            DataSet _dsCostLine = null;
            ship = new MInOut(GetCtx(), GetRecord_ID(), Get_Trx());
            _sql.Append(@"SELECT   C_BPartner_ID,C_BPartner_Location_ID,  VA033_InvoiceNo  FROM VA033_CostSheetLine WHERE VA033_Type      =2 AND VA033_CostSheet_ID= " + ship.GetVA033_CostSheet_ID()
                            + "GROUP BY C_BPartner_ID,   VA033_InvoiceNo,  C_BPartner_Location_ID ORDER BY C_BPartner_ID");

            DataSet _dsLandedCost = DB.ExecuteDataset(_sql.ToString(), null, null);
            if (_dsLandedCost.Tables.Count > 0 && _dsLandedCost.Tables[0].Rows.Count > 0)
            {
                int _pl_ID = 0;
                int _paymenttermID = 0;
                MBPartner cbPartner = null;
                for (int i = 0; i < _dsLandedCost.Tables[0].Rows.Count; i++)
                {
                    cbPartner = new MBPartner(GetCtx(), Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["C_BPartner_ID"]), null);

                    if (!c_bpInvNum.Contains(Util.GetValueOfString(cbPartner.GetC_BPartner_ID()) + Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"])))
                    {
                        c_bpInvNum.Add(Util.GetValueOfString(cbPartner.GetC_BPartner_ID()) + Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"]));

                        #region Invoice Header creation

                        invoice = new MInvoice(GetCtx(), 0, Get_Trx());


                        invoice.SetClientOrg(ship);
                        invoice.SetShipment(ship);	//	set base settings
                        //
                        invoice.SetC_DocTypeTarget_ID();
                        //invoice.SetDateAcct(DateTime.Now);
                        invoice.SetDateAcct(TrxDate);
                        invoice.SetSalesRep_ID(ship.GetSalesRep_ID());
                        invoice.SetAD_User_ID(ship.GetAD_User_ID());
                        //if (C_Currency_ID > 0)
                        //    invoice.SetC_Currency_ID(C_Currency_ID);

                        MOrder order = new MOrder(GetCtx(), ship.GetC_Order_ID(), null);

                        _CountVA009 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA009_'  AND IsActive = 'Y'"));
                        if (_CountVA009 > 0)
                            invoice.SetVA009_PaymentMethod_ID(order.GetVA009_PaymentMethod_ID());

                        //-------------Column Added by Anuj----------------------
                        // added by Amit 26-may-2016
                        _CountVA026 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA026_'  AND IsActive = 'Y'", null, null));
                        if (_CountVA026 > 0)
                        {
                            if (order != null && order.GetC_Order_ID() > 0)
                                invoice.SetVA026_LCDetail_ID(order.GetVA026_LCDetail_ID());
                        }
                        //end
                        if (ship.IsReturnTrx())
                            invoice.SetC_DocTypeTarget_ID(ship.IsSOTrx() ? MDocBaseType.DOCBASETYPE_ARCREDITMEMO : MDocBaseType.DOCBASETYPE_APCREDITMEMO);

                        int c_doctype_id = GetC_DocTypeTarget_ID(ship);
                        if (c_doctype_id > 0)
                        {
                            invoice.SetC_DocTypeTarget_ID(c_doctype_id);
                        }
                        else
                        {
                            log.Info(Msg.GetMsg(Env.GetCtx(), "GOM01_InvDoctypenotfound"));
                            return Msg.GetMsg(Env.GetCtx(), "GOM01_InvDoctypenotfound");
                        }

                        if (Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"]) != null && Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"]).Length > 0)
                            invoice.SetDocumentNo(Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"]));

                        invoice.SetC_BPartner_ID(cbPartner.GetC_BPartner_ID());
                        invoice.SetC_BPartner_Location_ID(Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["C_BPartner_Location_ID"]));
                        invoice.SetAD_User_ID(GetCtx().GetAD_User_ID());

                        //Price List
                        _pl_ID = Util.GetValueOfInt(DB.ExecuteScalar(" Select po_pricelist_id from c_bpartner where  IsActive = 'Y' AND C_BPartner_ID=" + cbPartner.GetC_BPartner_ID(), null, null));
                        if (_pl_ID > 0)
                            invoice.SetM_PriceList_ID(_pl_ID);
                        else
                        {
                            _pl_ID = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT m_pricelist_ID FROM  m_pricelist WHERE isdefault='Y' AND issopricelist='N' AND IsActive = 'Y' ", null, null));
                            if (_pl_ID > 0)
                                invoice.SetM_PriceList_ID(_pl_ID);
                            else
                                log.Info(" Cost Sheet Price List Not Found");
                        }

                        //payment term
                        _paymenttermID = Util.GetValueOfInt(DB.ExecuteScalar(" Select c_paymentterm_id from c_bpartner where  IsActive = 'Y' AND C_BPartner_ID=" + cbPartner.GetC_BPartner_ID(), null, null));

                        if (_paymenttermID > 0)
                            invoice.SetM_PriceList_ID(_paymenttermID);
                        else
                        {
                            _paymenttermID = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT c_paymentterm_id FROM  c_paymentterm WHERE isdefault='Y' AND IsActive = 'Y' AND ad_client_id=" + GetCtx().GetAD_Client_ID(), null, null));
                            if (_paymenttermID > 0)
                                invoice.SetM_PriceList_ID(_paymenttermID);
                            else
                                log.Info(" Cost Sheet Payment Term Not Found ");
                        }

                        //Date
                        _sql.Clear();
                        _sql.Append(@" SELECT VA033_InvDate from va033_costsheetline WHERE  VA033_Type=2 AND 
                                        C_BPartner_ID=" + cbPartner.GetC_BPartner_ID() + " AND C_BPartner_Location_ID=" + Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["C_BPartner_Location_ID"]) + " AND "
                                        + "VA033_InvoiceNo='" + Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"]) + "' AND  VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID());
                        DateTime? dt = Util.GetValueOfDateTime(DB.ExecuteScalar(_sql.ToString(), null, null));
                        if (dt != null)
                        {
                            invoice.SetDateInvoiced(dt);
                            invoice.SetDateAcct(dt);
                        }
                        else
                        {
                            invoice.SetDateInvoiced(TrxDate);
                            invoice.SetDateAcct(TrxDate);
                        }

                        invoice.SetPOReference("");
                        invoice.SetC_Order_ID(0);

                        if (!invoice.Save(Get_Trx()))
                        {
                            return "Cannot save Invoice";
                        }

                        #endregion
                    }

                    if (invoice != null)
                    {
                        _sql.Clear();
                        _sql.Append(@" SELECT M_Product_ID,VA033_InvAmt,C_Charge_ID,M_CostElement_ID,VA033_LandedCostDisbtn,C_Tax_ID from va033_costsheetline WHERE VA033_Type=2 
                                        AND VA033_InvoiceNo='" + Util.GetValueOfString(_dsLandedCost.Tables[0].Rows[i]["VA033_InvoiceNo"])
                                        + "' AND C_BPartner_ID=" + Util.GetValueOfInt(_dsLandedCost.Tables[0].Rows[i]["C_BPartner_ID"])
                                        + " AND VA033_CostSheet_ID=" + ship.GetVA033_CostSheet_ID());

                        _dsCostLine = DB.ExecuteDataset(_sql.ToString(), null, null);
                        MInvoiceLine line = null;
                        MLandedCost lc = null;
                        MLandedCostAllocation LcAlloc = null;
                        MInOutLine inLine = null;
                        decimal Amt = 0, MR_TQty = 0, TTlAmt = 0;
                        if (_dsCostLine.Tables.Count > 0)
                        {
                            if (_dsCostLine.Tables[0].Rows.Count > 0)
                            {
                                for (int j = 0; j < _dsCostLine.Tables[0].Rows.Count; j++)
                                {
                                    line = new MInvoiceLine(invoice);
                                    line.SetQtyEntered(1);
                                    line.SetM_Product_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[j]["M_Product_ID"]));
                                    line.SetC_Charge_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[j]["C_Charge_ID"]));
                                    line.SetPrice(Util.GetValueOfDecimal(_dsCostLine.Tables[0].Rows[j]["VA033_InvAmt"]));
                                    line.SetPriceActual(Util.GetValueOfDecimal(_dsCostLine.Tables[0].Rows[j]["VA033_InvAmt"]));
                                    line.SetLineNetAmt(Util.GetValueOfDecimal(_dsCostLine.Tables[0].Rows[j]["VA033_InvAmt"]));
                                    _cTaxID = Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[j]["C_Tax_ID"]);
                                    //Added by Manjot to set Tax ID
                                    if (_cTaxID > 0)
                                        line.SetC_Tax_ID(_cTaxID);
                                    if (!line.Save(Get_Trx()))
                                    {
                                        log.Info(" Cost Sheet Invoice Line Not Saved ");
                                        return "Cost Sheet Invoice Line Not Saved";
                                    }
                                    lc = new MLandedCost(GetCtx(), 0, Get_Trx());
                                    lc.SetAD_Client_ID(invoice.GetAD_Client_ID());
                                    lc.SetAD_Org_ID(invoice.GetAD_Org_ID());
                                    lc.SetC_InvoiceLine_ID(line.GetC_InvoiceLine_ID());
                                    lc.SetLandedCostDistribution(Util.GetValueOfString(_dsCostLine.Tables[0].Rows[j]["VA033_LandedCostDisbtn"]));
                                    lc.SetM_CostElement_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[j]["M_CostElement_ID"]));
                                    lc.SetM_InOut_ID(GetRecord_ID());
                                    if (!lc.Save(Get_Trx()))
                                    {
                                        log.Info(" Landed Cost Not Saved ");
                                    }

                                    #region If Landed Cost Distribution Method Is Costs

                                    if (Util.GetValueOfString(_dsCostLine.Tables[0].Rows[j]["VA033_LandedCostDisbtn"]) == "C")
                                    {
                                        int[] IDsLine = MInOutLine.GetAllIDs("M_InOutLine", "M_Inout_ID= " + GetRecord_ID(), null);
                                        if (IDsLine.Length > 0)
                                        {
                                            MR_TQty = Util.GetValueOfDecimal(DB.ExecuteScalar(@"SELECT SUM(IL.QtyEntered)as TotalQty FROM 
                                                                           M_INOUTLINE IL WHERE IL.M_INOUT_ID=" + ship.GetM_InOut_ID()));
                                            #region Get Total Base Ammount
                                            TTlAmt = 0;
                                            Decimal orderAmount = 0;
                                            for (int l = 0; l < IDsLine.Length; l++)
                                            {
                                                inLine = new MInOutLine(GetCtx(), IDsLine[l], Get_Trx());
                                                Amt = MCost.GetproductCosts(invoice.GetAD_Client_ID(), invoice.GetAD_Org_ID(), inLine.GetM_Product_ID(), inLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                                if (inLine.GetCurrentCostPrice() > 0)
                                                {
                                                    TTlAmt += (inLine.GetMovementQty() * inLine.GetCurrentCostPrice());
                                                }
                                                else if (Amt > 0)
                                                {
                                                    TTlAmt += (inLine.GetMovementQty() * Amt);
                                                }
                                                else
                                                {
                                                    MOrderLine ol = new MOrderLine(GetCtx(), inLine.GetC_OrderLine_ID(), Get_Trx());
                                                    MOrder order = new MOrder(GetCtx(), ol.GetC_Order_ID(), Get_Trx());
                                                    orderAmount = (inLine.GetMovementQty() * ol.GetPriceEntered());
                                                    if (order.GetC_Currency_ID() != invoice.GetC_Currency_ID())
                                                    {
                                                        orderAmount = MConversionRate.Convert(GetCtx(), orderAmount, order.GetC_Currency_ID(), invoice.GetC_Currency_ID(),
                                                                             invoice.GetDateAcct(), invoice.GetC_ConversionType_ID(), invoice.GetAD_Client_ID(), invoice.GetAD_Org_ID());
                                                    }
                                                    TTlAmt += orderAmount;
                                                }

                                            }
                                            #endregion

                                            for (int k = 0; k < IDsLine.Length; k++)
                                            {
                                                inLine = new MInOutLine(GetCtx(), IDsLine[k], null);
                                                if (inLine != null)
                                                {
                                                    if (inLine.GetM_Product_ID() > 0)
                                                    {
                                                        LcAlloc = new MLandedCostAllocation(GetCtx(), 0, Get_Trx());
                                                        LcAlloc.SetAD_Client_ID(invoice.GetAD_Client_ID());
                                                        LcAlloc.SetAD_Org_ID(invoice.GetAD_Org_ID());
                                                        LcAlloc.SetC_InvoiceLine_ID(line.GetC_InvoiceLine_ID());
                                                        LcAlloc.SetM_Product_ID(inLine.GetM_Product_ID());
                                                        LcAlloc.SetM_CostElement_ID(Util.GetValueOfInt(_dsCostLine.Tables[0].Rows[j]["M_CostElement_ID"]));
                                                        Amt = MCost.GetproductCosts(invoice.GetAD_Client_ID(), invoice.GetAD_Org_ID(), inLine.GetM_Product_ID(), inLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                                        if (inLine.GetCurrentCostPrice() > 0)
                                                        {
                                                            Amt = inLine.GetCurrentCostPrice();
                                                            Amt = CalculateLandedCost(Util.GetValueOfDecimal(_dsCostLine.Tables[0].Rows[j]["VA033_InvAmt"]), TTlAmt, inLine.GetMovementQty(), Amt);
                                                            LcAlloc.SetAmt(Math.Round(inLine.GetMovementQty() * Amt, 9));
                                                            LcAlloc.SetQty(inLine.GetMovementQty());
                                                            LcAlloc.SetBase(inLine.GetMovementQty());
                                                        }
                                                        else if (Amt > 0)
                                                        {
                                                            Amt = CalculateLandedCost(Util.GetValueOfDecimal(_dsCostLine.Tables[0].Rows[j]["VA033_InvAmt"]), TTlAmt, inLine.GetMovementQty(), Amt);
                                                            LcAlloc.SetAmt(Math.Round(inLine.GetMovementQty() * Amt, 9));
                                                            LcAlloc.SetQty(inLine.GetMovementQty());
                                                            LcAlloc.SetBase(inLine.GetMovementQty());
                                                        }
                                                        else
                                                        {
                                                            MOrderLine ol = new MOrderLine(GetCtx(), inLine.GetC_OrderLine_ID(), null);
                                                            MOrder order = new MOrder(GetCtx(), ol.GetC_Order_ID(), null);
                                                            Amt = ol.GetPriceEntered();
                                                            if (order.GetC_Currency_ID() != invoice.GetC_Currency_ID())
                                                            {
                                                                Amt = MConversionRate.Convert(GetCtx(), Amt, order.GetC_Currency_ID(), invoice.GetC_Currency_ID(),
                                                                                     invoice.GetDateAcct(), invoice.GetC_ConversionType_ID(), invoice.GetAD_Client_ID(), invoice.GetAD_Org_ID());
                                                            }
                                                            Amt = CalculateLandedCost(Util.GetValueOfDecimal(_dsCostLine.Tables[0].Rows[j]["VA033_InvAmt"]), TTlAmt, inLine.GetMovementQty(), Amt);
                                                            LcAlloc.SetAmt(Math.Round(inLine.GetMovementQty() * Amt, 9));
                                                            LcAlloc.SetQty(inLine.GetMovementQty());
                                                            LcAlloc.SetBase(inLine.GetMovementQty());
                                                        }

                                                        if (!LcAlloc.Save(Get_Trx()))
                                                        {
                                                            log.Info(" Landed Cost Allocation Not Saved ");
                                                        }
                                                    }
                                                }
                                            }
                                            TTlAmt = 0;
                                        }
                                    }

                                    #endregion
                                }
                            }
                        }
                        invoice.Save(Get_Trx());
                        Get_Trx().Commit();
                        if (_docstatus == "CO")
                        {
                            if (invoice.ProcessIt("CO"))
                            {
                                invoice.SetDocStatus("CO");
                                invoice.SetDocAction("CL");
                            }
                        }
                        else
                        {
                            if (invoice.ProcessIt("PR"))
                            {
                                invoice.SetDocStatus("DR");
                                invoice.SetDocAction("PR");
                            }
                        }
                        if (!invoice.Save(Get_Trx()))
                        {

                        }
                        Get_Trx().Commit();
                        Get_Trx().Close();
                        if (String.IsNullOrEmpty(msg))
                        {
                            msg = Msg.GetMsg(GetCtx(), "GOM01_CSInvoiceGen") + " = " + invoice.GetDocumentNo();
                        }
                        else
                        {
                            msg = msg + "," + invoice.GetDocumentNo();
                        }
                    }
                    else
                    { return Msg.GetMsg(Env.GetCtx(), "VA033_InvoiceNotGenerated"); }
                }
            }
            #endregion

            return msg;
        }

        public decimal CalculateEachPrice(decimal CS_InvAmt, decimal MR_Total, decimal PO_Price, decimal MR_Qty)
        {
            decimal UnitPrice = 0;
            if (CS_InvAmt > 0 && MR_Total > 0 && PO_Price > 0 && MR_Qty > 0)
            {
                decimal ValueDiff = CS_InvAmt - MR_Total;
                decimal TotalPrice = ValueDiff / MR_Total * (MR_Qty * PO_Price);
                decimal GrossPrice = TotalPrice + (MR_Qty * PO_Price);
                UnitPrice = GrossPrice / MR_Qty;
            }

            return Math.Round(UnitPrice, 9);
        }

        public decimal CalculateLandedCost(decimal FreightAmt, decimal TotalBaseValue, decimal MR_Qty, decimal Amt)
        {

            decimal UnitPrice = 0;
            decimal FreightDistribution = 0;
            if (FreightAmt > 0 && TotalBaseValue > 0 && MR_Qty > 0 && Amt > 0)
            {
                decimal ActPrc = MR_Qty * Amt;
                FreightDistribution = (FreightAmt * ActPrc) / TotalBaseValue;
                UnitPrice = FreightDistribution / MR_Qty;
            }
            return UnitPrice;
        }

        /// <summary>
        /// Set Target Document Type
        /// </summary>
        //<param name="DocBaseType">doc type MDocBaseType.DOCBASETYPE_</param>
        public int GetC_DocTypeTarget_ID(MInOut ship)
        {
            MDocType dt = new MDocType(GetCtx(), ship.GetC_DocType_ID(), Get_Trx());
            int C_DocType_ID = dt.GetC_DocTypeInvoice_ID();
            //String sql = "SELECT C_DocType_ID FROM C_DocType "
            //   + "WHERE AD_Client_ID=" + ord.GetAD_Client_ID() + " AND DocBaseType='API'"
            //   + " AND AD_Org_ID=" + ord.GetAD_Org_ID() + " AND IsActive='Y' AND IsExpenseInvoice = 'N' "
            //   + "ORDER BY C_DocType_ID DESC ,   IsDefault DESC";
            //string sql = "Select C_DocTypeInvoice_ID From C_Doctype Where C_Doctype_ID=" + ship.GetC_DocType_ID();
            //int C_DocType_ID = Util.GetValueOfInt(DB.ExecuteScalar(sql));
            return C_DocType_ID;
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

                else if (name.Equals("VA033_DocStatus"))
                {
                    _docstatus = (String)para[i].GetParameter();
                }

                else
                {
                    //log.log(Level.SEVERE, "Unknown Parameter: " + name);
                }
            }

        }
    }
}
