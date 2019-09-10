/********************************************************
    * Project Name   : VAdvantage
    * Class Name     : ImportedDataCompletion
    * Purpose        : Complete Records
    * Class Used     : ProcessEngine.SvrProcess
    * Chronological    Development
    * Amit Bansal     24-May-2016
******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace ViennaAdvantage.Process
{
    public class INT15_ImportedDataCompletion : SvrProcess
    {

        private static VLogger _log = VLogger.GetVLogger(typeof(INT15_ImportedDataCompletion).FullName);
        StringBuilder sql = new StringBuilder();
        DataSet dsInOut = new DataSet();
        DataSet dsRecord = new DataSet();
        DataRow[] dataRow = null;
        MInOut inout = null;
        int C_DocType_ID = 0;
        string windowName = "";

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
                else if (name.Equals("C_DocType_ID"))
                {
                    C_DocType_ID = Util.GetValueOfInt(para[i].GetParameter());
                }
                else if (name.Equals("INT15_WindowName"))
                {
                    windowName = (String)para[i].GetParameter();
                }
            }
        }

        protected override string DoIt()
        {
            try
            {
                sql.Clear();
                sql.Append("SELECT * FROM M_InOut WHERE IsActive = 'Y' AND DocStatus IN ('DR') ");
                if (C_DocType_ID > 0)
                {
                    sql.Append(" AND C_DocType_ID = " + C_DocType_ID);
                }
                sql.Append(" ORDER BY movementdate");
                dsInOut = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                #region complete Order Record
                if (windowName == "OR")
                {
                    sql.Clear();
                    sql.Append("SELECT * FROM C_Order WHERE IsActive = 'Y' AND DocStatus IN ('DR') ");
                    if (C_DocType_ID > 0)
                    {
                        sql.Append(" AND C_DocType_ID = " + C_DocType_ID);
                    }
                    sql.Append(" ORDER BY dateacct");
                    dsRecord = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                    dataRow = dsRecord.Tables[0].Select("DocStatus = 'DR' ", "dateacct");
                    if (dataRow != null && dataRow.Length > 0)
                    {
                        MOrder order = null;
                        for (int i = 0; i < dataRow.Length; i++)
                        {
                            try
                            {
                                order = new MOrder(GetCtx(), Util.GetValueOfInt(dataRow[i]["C_Order_ID"]), Get_Trx());
                                order.CompleteIt();
                                if (order.GetDocAction() == "CL")
                                {
                                    order.SetDocStatus("CO");
                                    order.SetDocAction("CL");
                                    if (!order.Save(Get_Trx()))
                                    {
                                        Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        _log.Info("Error found for saving C_Order Record ID = " + order.GetC_Order_ID() +
                                                   " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                    }
                                    else
                                    {
                                        Get_Trx().Commit();
                                    }
                                }
                                else
                                {
                                    _log.Info("Order not completed for this Record ID = " + order.GetC_Order_ID());
                                }
                            }
                            catch { }
                        }
                    }
                }
                #endregion

                #region complete Invoice Record
                if (windowName == "IN")
                {
                    sql.Clear();
                    sql.Append("SELECT * FROM C_Invoice WHERE IsActive = 'Y' AND DocStatus IN ('DR') ");
                    if (C_DocType_ID > 0)
                    {
                        sql.Append(" AND C_DocType_ID = " + C_DocType_ID);
                    }
                    sql.Append(" ORDER BY dateacct");
                    dsRecord = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());
                    dataRow = dsRecord.Tables[0].Select("DocStatus = 'DR' ", "dateacct");
                    if (dataRow != null && dataRow.Length > 0)
                    {
                        MInvoice invoice = null;
                        for (int i = 0; i < dataRow.Length; i++)
                        {
                            try
                            {
                                invoice = new MInvoice(GetCtx(), Util.GetValueOfInt(dataRow[i]["C_Invoice_ID"]), Get_Trx());
                                invoice.CompleteIt();
                                if (invoice.GetDocAction() == "CL")
                                {
                                    invoice.SetDocStatus("CO");
                                    invoice.SetDocAction("CL");
                                    if (!invoice.Save(Get_Trx()))
                                    {
                                        Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        _log.Info("Error found for saving C_Invoice Record ID = " + invoice.GetC_Invoice_ID() +
                                                   " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                    }
                                    else
                                    {
                                        Get_Trx().Commit();
                                    }
                                }
                                else
                                {
                                    _log.Info("Invoice not completed for this Record ID = " + invoice.GetC_Invoice_ID());
                                }

                            }
                            catch { }
                        }
                    }
                }
                #endregion

                #region complete material receipt
                if (windowName == "MR")
                {
                    dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'N' AND IsReturnTrx = 'N' AND DocStatus = 'DR' ", "dateacct");
                    if (dataRow != null && dataRow.Length > 0)
                    {
                        for (int i = 0; i < dataRow.Length; i++)
                        {
                            try
                            {
                                inout = new MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                inout.CompleteIt();
                                if (inout.GetDocAction() == "CL")
                                {
                                    inout.SetDocStatus("CO");
                                    inout.SetDocAction("CL");
                                    if (!inout.Save(Get_Trx()))
                                    {
                                        Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        _log.Info("Error found for saving C_Order Record ID = " + inout.GetM_InOut_ID() +
                                                   " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                    }
                                    else
                                    {
                                        Get_Trx().Commit();
                                    }
                                }
                                else
                                {
                                    _log.Info("Material Receipt not completed for this Record ID = " + inout.GetM_InOut_ID());
                                }
                            }
                            catch { }
                        }
                    }
                }
                #endregion

                #region complete Movement Record
                if (windowName == "IM")
                {
                    sql.Clear();
                    sql.Append("SELECT * FROM M_Movement WHERE IsActive = 'Y' AND DocStatus IN ('DR') ");
                    if (C_DocType_ID > 0)
                    {
                        sql.Append(" AND C_DocType_ID = " + C_DocType_ID);
                    }
                    sql.Append(" ORDER BY movementdate");
                    dsRecord = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                    dataRow = dsRecord.Tables[0].Select("DocStatus = 'DR' ", "movementdate");
                    if (dataRow != null && dataRow.Length > 0)
                    {
                        MMovement movement = null;
                        for (int i = 0; i < dataRow.Length; i++)
                        {
                            try
                            {
                                movement = new MMovement(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_Movement_ID"]), Get_Trx());
                                movement.CompleteIt();
                                if (movement.GetDocAction() == "CL")
                                {
                                    movement.SetDocStatus("CO");
                                    movement.SetDocAction("CL");
                                    if (!movement.Save(Get_Trx()))
                                    {
                                        Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        _log.Info("Error found for saving C_Invoice Record ID = " + movement.GetM_Movement_ID() +
                                                   " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                    }
                                    else
                                    {
                                        Get_Trx().Commit();
                                    }
                                }
                                else
                                {
                                    _log.Info("Movement not completed for this Record ID = " + movement.GetM_Movement_ID());
                                }
                            }
                            catch { }
                        }
                    }
                }
                #endregion

                #region complete shipment
                if (windowName == "SH")
                {
                    dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'Y' AND IsReturnTrx = 'N' AND DocStatus = 'DR' ", "dateacct");
                    if (dataRow != null && dataRow.Length > 0)
                    {
                        for (int i = 0; i < dataRow.Length; i++)
                        {
                            try
                            {
                                inout = new MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                inout.CompleteIt();
                                if (inout.GetDocAction() == "CL")
                                {
                                    inout.SetDocStatus("CO");
                                    inout.SetDocAction("CL");
                                    if (!inout.Save(Get_Trx()))
                                    {
                                        Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        _log.Info("Error found for saving C_Order Record ID = " + inout.GetM_InOut_ID() +
                                                   " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                    }
                                    else
                                    {
                                        Get_Trx().Commit();
                                    }
                                }
                                else
                                {
                                    _log.Info("Shipment not completed for this Record ID = " + inout.GetM_InOut_ID());
                                }
                            }
                            catch { }
                        }
                    }
                }
                #endregion

                #region complete Customer Return
                if (windowName == "CR")
                {
                    dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'Y' AND IsReturnTrx = 'Y' AND DocStatus = 'DR' ", "dateacct");
                    if (dataRow != null && dataRow.Length > 0)
                    {
                        for (int i = 0; i < dataRow.Length; i++)
                        {
                            try
                            {
                                inout = new MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                inout.CompleteIt();
                                if (inout.GetDocAction() == "CL")
                                {
                                    inout.SetDocStatus("CO");
                                    inout.SetDocAction("CL");
                                    if (!inout.Save(Get_Trx()))
                                    {
                                        Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        _log.Info("Error found for saving C_Order Record ID = " + inout.GetM_InOut_ID() +
                                                   " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                    }
                                    else
                                    {
                                        Get_Trx().Commit();
                                    }
                                }
                                else
                                {
                                    _log.Info("Customer return not completed for this Record ID = " + inout.GetM_InOut_ID());
                                }
                            }
                            catch { }
                        }
                    }
                }
                #endregion

                #region complete Return to Vendor
                if (windowName == "VR")
                {
                    dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'N' AND IsReturnTrx = 'Y' AND DocStatus = 'DR' ", "dateacct");
                    if (dataRow != null && dataRow.Length > 0)
                    {
                        for (int i = 0; i < dataRow.Length; i++)
                        {
                            try
                            {
                                inout = new MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                inout.CompleteIt();
                                if (inout.GetDocAction() == "CL")
                                {
                                    inout.SetDocStatus("CO");
                                    inout.SetDocAction("CL");
                                    if (!inout.Save(Get_Trx()))
                                    {
                                        Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        _log.Info("Error found for saving C_Order Record ID = " + inout.GetM_InOut_ID() +
                                                   " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                    }
                                    else
                                    {
                                        Get_Trx().Commit();
                                    }
                                }
                                else
                                {
                                    _log.Info("Return to Vendor not completed for this Record ID = " + inout.GetM_InOut_ID());
                                }
                            }
                            catch { }
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                _log.Info("Error Occured during completion of record by using  ImportedDataCompletion Process - " + ex.ToString());
                return Msg.GetMsg(GetCtx(), "NotCompleted");
            }
            return Msg.GetMsg(GetCtx(), "SucessfullyCompleted");
        }
    }
}
