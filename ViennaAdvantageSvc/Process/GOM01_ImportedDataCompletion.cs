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
    public class GOM01_ImportedDataCompletion : SvrProcess
    {

        private static VLogger _log = VLogger.GetVLogger(typeof(GOM01_ImportedDataCompletion).FullName);
        StringBuilder sql = new StringBuilder();
        DataSet dsInOut = new DataSet();
        DataSet dsRecord = new DataSet();
        DataSet dsInventory = new DataSet();
        DataSet dsMovement = new DataSet();
        DataSet dsPayment = new DataSet();
        DataSet dsCashJournal = new DataSet();
        DataSet dsGLJournal = new DataSet();
        DataRow[] dataRow = null;
        ViennaAdvantage.Model.MInOut inout = null;
        MPayment payment = null;
        MCash cash = null;
        MJournal journal = null;
        DateTime? minDateRecord;
        StringBuilder notCompletedRecord = new StringBuilder();
        StringBuilder message = new StringBuilder();
        StringBuilder errorMessage = new StringBuilder();

        protected override void Prepare()
        {
            ;
        }

        protected override string DoIt()
        {
            try
            {
                _log.Info("Start Imported Data completion for GulfOil : " + System.DateTime.Now);

                #region complete Order Record
                sql.Clear();
                sql.Append("SELECT * FROM C_Order WHERE IsActive = 'Y'   AND DocStatus NOT IN ('CO' , 'CL') ORDER BY dateacct");
                dsRecord = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                dataRow = dsRecord.Tables[0].Select("IsActive = 'Y' ", "dateacct");
                if (dataRow != null && dataRow.Length > 0)
                {
                    ViennaAdvantage.Model.MOrder order = null;

                    message.Clear();
                    notCompletedRecord.Clear();
                    message.Append("Records Of C_Order ");

                    for (int i = 0; i < dataRow.Length; i++)
                    {
                        try
                        {

                            order = new ViennaAdvantage.Model.MOrder(GetCtx(), Util.GetValueOfInt(dataRow[i]["C_Order_ID"]), Get_Trx());
                            order.CompleteIt();
                            if (order.GetDocAction() == "CL")
                            {
                                order.SetDocStatus("CO");
                                order.SetDocAction("CL");
                                if (!order.Save(Get_Trx()))
                                {
                                    notCompletedRecord.Append(order.GetDocumentNo() + " ,");
                                    Get_Trx().Rollback();
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    _log.Info("Error found for saving C_Order Record ID = " + order.GetDocumentNo() +
                                               " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                }
                                else
                                {
                                    Get_Trx().Commit();
                                }
                            }
                            else
                            {
                                notCompletedRecord.Append(order.GetDocumentNo() + " ,");
                                Get_Trx().Rollback();
                                _log.Info("Order not completed for this Record ID = " + order.GetDocumentNo());
                            }
                        }
                        catch { }
                    }

                    if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                    {
                        errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                    }
                }
                #endregion

                #region complete Invoice Record
                sql.Clear();
                sql.Append("SELECT * FROM C_Invoice WHERE IsActive = 'Y'   AND DocStatus NOT IN ('CO' , 'CL') ORDER BY dateacct");
                dsRecord = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                dataRow = dsRecord.Tables[0].Select("IsActive = 'Y' ", "dateacct");
                if (dataRow != null && dataRow.Length > 0)
                {
                    ViennaAdvantage.Model.MInvoice invoice = null;

                    message.Clear();
                    notCompletedRecord.Clear();
                    message.Append("Records Of C_Invoice ");

                    for (int i = 0; i < dataRow.Length; i++)
                    {
                        try
                        {
                            invoice = new ViennaAdvantage.Model.MInvoice(GetCtx(), Util.GetValueOfInt(dataRow[i]["C_Invoice_ID"]), Get_Trx());
                            invoice.CompleteIt();
                            if (invoice.GetDocAction() == "CL")
                            {
                                invoice.SetDocStatus("CO");
                                invoice.SetDocAction("CL");
                                if (!invoice.Save(Get_Trx()))
                                {
                                    notCompletedRecord.Append(invoice.GetDocumentNo() + " ,");
                                    Get_Trx().Rollback();
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    _log.Info("Error found for saving C_Invoice Record ID = " + invoice.GetDocumentNo() +
                                               " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                }
                                else
                                {
                                    Get_Trx().Commit();
                                }
                            }
                            else
                            {
                                notCompletedRecord.Append(invoice.GetDocumentNo() + " ,");
                                Get_Trx().Rollback();
                                _log.Info("Invoice not completed for this Record ID = " + invoice.GetDocumentNo() + " Message- " + invoice.GetProcessMsg());
                            }

                        }
                        catch { }
                    }

                    if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                    {
                        errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                    }
                }
                #endregion

                sql.Clear();
                sql.Append("SELECT * FROM M_InOut WHERE IsActive = 'Y'  AND DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ORDER BY movementdate");
                dsInOut = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                sql.Clear();
                sql.Append("SELECT * FROM M_Inventory WHERE IsActive = 'Y' AND DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ORDER BY movementdate");
                dsInventory = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                sql.Clear();
                sql.Append("SELECT * FROM M_Movement WHERE IsActive = 'Y' AND  DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ORDER BY movementdate");
                dsMovement = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                sql.Clear();
                sql.Append("SELECT * FROM C_Payment WHERE IsActive = 'Y' AND  DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ORDER BY dateacct");
                dsPayment = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                sql.Clear();
                sql.Append("SELECT * FROM C_Cash WHERE IsActive = 'Y' AND  DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ORDER BY dateacct");
                dsCashJournal = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                sql.Clear();
                sql.Append("SELECT * FROM GL_Journal  WHERE IsActive = 'Y' AND  DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ORDER BY dateacct");
                dsGLJournal = DB.ExecuteDataset(sql.ToString(), null, Get_Trx());

                // min date record from the transaction window
                minDateRecord = SerachMinDate();

                int diff = (DateTime.Now - minDateRecord.Value).Days;

                for (int days = 0; days <= diff; days++)
                {
                    if (days != 0)
                    {
                        minDateRecord = minDateRecord.Value.AddDays(1);
                    }

                    try
                    {

                        #region Physical Inventory
                        dataRow = dsInventory.Tables[0].Select("isinternaluse = 'N' AND  movementdate = '" + minDateRecord + "'", "M_Inventory_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            MInventory inventory = null;

                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of Physical Inventory ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    inventory = new MInventory(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_Inventory_ID"]), Get_Trx());
                                    inventory.CompleteIt();
                                    if (inventory.GetDocAction() == "CL")
                                    {
                                        inventory.SetDocStatus("CO");
                                        inventory.SetDocAction("CL");
                                        if (!inventory.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(inventory.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving Physical Inventory Record ID = " + inventory.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(inventory.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Physical Inventory not completed for this Record ID = " + inventory.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }

                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region Internal use inventory
                        dataRow = dsInventory.Tables[0].Select("isinternaluse = 'Y' AND  movementdate = '" + minDateRecord + "'", "M_Inventory_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            MInventory inventory = null;

                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of Internal use inventory ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    inventory = new MInventory(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_Inventory_ID"]), Get_Trx());
                                    inventory.CompleteIt();
                                    if (inventory.GetDocAction() == "CL")
                                    {
                                        inventory.SetDocStatus("CO");
                                        inventory.SetDocAction("CL");
                                        if (!inventory.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(inventory.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving Physical Inventory Record ID = " + inventory.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(inventory.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Physical Inventory not completed for this Record ID = " + inventory.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region complete material receipt
                        dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'N' AND IsReturnTrx = 'N'  AND DocStatus NOT IN ('CO' , 'CL') AND  DateAcct = '" + minDateRecord + "'", "M_InOut_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {

                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of material receipt ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    inout = new ViennaAdvantage.Model.MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                    inout.CompleteIt();
                                    if (inout.GetDocAction() == "CL")
                                    {
                                        inout.SetDocStatus("CO");
                                        inout.SetDocAction("CL");
                                        if (!inout.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Order Record ID = " + inout.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Material Receipt not completed for this Record ID = " + inout.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region complete Movement Record
                        dataRow = dsMovement.Tables[0].Select("movementdate = '" + minDateRecord + "'", "M_Movement_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            ViennaAdvantage.Model.MMovement movement = null;

                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of M_Movement ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    movement = new ViennaAdvantage.Model.MMovement(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_Movement_ID"]), Get_Trx());
                                    movement.CompleteIt();
                                    if (movement.GetDocAction() == "CL")
                                    {
                                        movement.SetDocStatus("CO");
                                        movement.SetDocAction("CL");
                                        if (!movement.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(movement.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Invoice Record ID = " + movement.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(movement.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Movement not completed for this Record ID = " + movement.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region complete shipment
                        dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'Y' AND IsReturnTrx = 'N' AND  DateAcct = '" + minDateRecord + "'", "M_Inout_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of shipment ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    inout = new ViennaAdvantage.Model.MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                    inout.CompleteIt();
                                    if (inout.GetDocAction() == "CL")
                                    {
                                        inout.SetDocStatus("CO");
                                        inout.SetDocAction("CL");
                                        if (!inout.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Order Record ID = " + inout.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Shipment not completed for this Record ID = " + inout.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region complete Customer Return
                        dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'Y' AND IsReturnTrx = 'Y'  AND  DateAcct = '" + minDateRecord + "'", "M_Inout_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of Customer Return ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    inout = new ViennaAdvantage.Model.MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                    inout.CompleteIt();
                                    if (inout.GetDocAction() == "CL")
                                    {
                                        inout.SetDocStatus("CO");
                                        inout.SetDocAction("CL");
                                        if (!inout.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Order Record ID = " + inout.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Customer return not completed for this Record ID = " + inout.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region complete Return to Vendor
                        dataRow = dsInOut.Tables[0].Select("IsSoTrx = 'N' AND IsReturnTrx = 'Y'  AND  DateAcct = '" + minDateRecord + "'", "M_InOut_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of Return to Vendor ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    inout = new ViennaAdvantage.Model.MInOut(GetCtx(), Util.GetValueOfInt(dataRow[i]["M_InOut_ID"]), Get_Trx());
                                    inout.CompleteIt();
                                    if (inout.GetDocAction() == "CL")
                                    {
                                        inout.SetDocStatus("CO");
                                        inout.SetDocAction("CL");
                                        if (!inout.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Order Record ID = " + inout.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(inout.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Return to Vendor not completed for this Record ID = " + inout.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region Complete Payment
                        dataRow = dsPayment.Tables[0].Select("DateAcct = '" + minDateRecord + "'", "C_Payment_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of Payment ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    payment = new MPayment(GetCtx(), Util.GetValueOfInt(dataRow[i]["C_Payment_ID"]), Get_Trx());
                                    payment.CompleteIt();
                                    if (payment.GetDocAction() == "CL")
                                    {
                                        payment.SetDocStatus("CO");
                                        payment.SetDocAction("CL");
                                        if (!payment.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(payment.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Payment Record ID = " + payment.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(payment.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Payment not completed for this Record ID = " + payment.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region Complete Cash Journal
                        dataRow = dsCashJournal.Tables[0].Select("DateAcct = '" + minDateRecord + "'", "C_Cash_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of Cash Journal ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    cash = new MCash(GetCtx(), Util.GetValueOfInt(dataRow[i]["C_Cash_ID"]), Get_Trx());
                                    cash.CompleteIt();
                                    if (cash.GetDocAction() == "CL")
                                    {
                                        cash.SetDocStatus("CO");
                                        cash.SetDocAction("CL");
                                        if (!cash.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(cash.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Cash Record ID = " + cash.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(cash.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Cash Journal not completed for this Record ID = " + cash.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                        #region Complete GL Journal
                        dataRow = dsGLJournal.Tables[0].Select("DateAcct = '" + minDateRecord + "'", "GL_Journal_ID");
                        if (dataRow != null && dataRow.Length > 0)
                        {
                            message.Clear();
                            notCompletedRecord.Clear();
                            message.Append("Records Of GL Journal ");

                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                try
                                {
                                    journal = new MJournal(GetCtx(), Util.GetValueOfInt(dataRow[i]["GL_Journal_ID"]), Get_Trx());
                                    journal.CompleteIt();
                                    if (journal.GetDocAction() == "CL")
                                    {
                                        journal.SetDocStatus("CO");
                                        journal.SetDocAction("CL");
                                        if (!journal.Save(Get_Trx()))
                                        {
                                            notCompletedRecord.Append(journal.GetDocumentNo() + " ,");
                                            Get_Trx().Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            _log.Info("Error found for saving C_Cash Record ID = " + journal.GetDocumentNo() +
                                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                                        }
                                        else
                                        {
                                            Get_Trx().Commit();
                                        }
                                    }
                                    else
                                    {
                                        notCompletedRecord.Append(journal.GetDocumentNo() + " ,");
                                        Get_Trx().Rollback();
                                        _log.Info("Cash Journal not completed for this Record ID = " + journal.GetDocumentNo());
                                    }
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(notCompletedRecord.ToString()))
                            {
                                errorMessage.Append(message.ToString() + " : " + notCompletedRecord.ToString());
                            }
                        }
                        #endregion

                    }
                    catch { }
                }
                _log.Info(" End Imported Data completion for GulfOil : " + System.DateTime.Now);
            }
            catch (Exception ex)
            {
                _log.Info("Error Occured during completion of record by using  ImportedDataCompletion Process - " + ex.ToString());
                return Msg.GetMsg(GetCtx(), "NotCompleted");
            }
            if (!string.IsNullOrEmpty(errorMessage.ToString()))
            {
                return "Not Completed Record : " + errorMessage.ToString();
            }
            else
            {
                return Msg.GetMsg(GetCtx(), "SucessfullyCompleted");
            }
        }

        public DateTime? SerachMinDate()
        {
            DateTime? minDate;
            DateTime? tempDate;
            try
            {
                sql.Clear();
                sql.Append("SELECT Min(MovementDate) FROM m_Inventory WHERE isactive = 'Y' AND docstatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ");
                minDate = Util.GetValueOfDateTime(DB.ExecuteScalar(sql.ToString(), null, Get_Trx()));

                sql.Clear();
                sql.Append(@"SELECT Min(MovementDate) FROM m_movement WHERE isactive = 'Y' AND DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ");
                tempDate = Util.GetValueOfDateTime(DB.ExecuteScalar(sql.ToString(), null, Get_Trx()));
                if (minDate == null || (Util.GetValueOfDateTime(minDate) > Util.GetValueOfDateTime(tempDate) && tempDate != null))
                {
                    minDate = tempDate;
                }

                sql.Clear();
                sql.Append(@"SELECT Min(DateAcct) FROM m_inout WHERE isactive = 'Y' AND DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ");
                tempDate = Util.GetValueOfDateTime
                    (DB.ExecuteScalar(sql.ToString(), null, Get_Trx()));
                if (minDate == null || (Util.GetValueOfDateTime(minDate) > Util.GetValueOfDateTime(tempDate) && tempDate != null))
                {
                    minDate = tempDate;
                }

                sql.Clear();
                sql.Append(@"SELECT Min(DateAcct) FROM C_Payment WHERE isactive = 'Y' AND DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ");
                tempDate = Util.GetValueOfDateTime
                    (DB.ExecuteScalar(sql.ToString(), null, Get_Trx()));
                if (minDate == null || (Util.GetValueOfDateTime(minDate) > Util.GetValueOfDateTime(tempDate) && tempDate != null))
                {
                    minDate = tempDate;
                }

                sql.Clear();
                sql.Append(@"SELECT Min(DateAcct) FROM C_Cash WHERE isactive = 'Y' AND DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ");
                tempDate = Util.GetValueOfDateTime
                    (DB.ExecuteScalar(sql.ToString(), null, Get_Trx()));
                if (minDate == null || (Util.GetValueOfDateTime(minDate) > Util.GetValueOfDateTime(tempDate) && tempDate != null))
                {
                    minDate = tempDate;
                }

                sql.Clear();
                sql.Append(@"SELECT Min(DateAcct) FROM GL_Journal WHERE isactive = 'Y' AND DocStatus NOT IN ('CO' , 'CL' , 'RE' , 'VO') ");
                tempDate = Util.GetValueOfDateTime
                    (DB.ExecuteScalar(sql.ToString(), null, Get_Trx()));
                if (minDate == null || (Util.GetValueOfDateTime(minDate) > Util.GetValueOfDateTime(tempDate) && tempDate != null))
                {
                    minDate = tempDate;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return minDate;
        }
    }
}
