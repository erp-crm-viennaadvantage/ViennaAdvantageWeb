using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VAdvantage.Logging;
using VAdvantage.Utility;
using VAdvantage.DataBase;
using System.Data.SqlClient;
using System.IO;
using VAdvantage.Process;
using VAdvantage.VOS;
using VAdvantage.ProcessEngine;
using ViennaAdvantage.CMFG.Util1;
using ViennaAdvantage.CMFG.Model;
using VAdvantage.Model;

namespace ViennaAdvantage.Model
{
    public class MVAMFGMWrkOdrTransaction : X_VAMFG_M_WrkOdrTransaction, DocAction
    {
        private static new VLogger log = VLogger.GetVLogger(typeof(MVAMFGMWrkOdrTransaction).FullName);
        private static long serialVersionUID = 1L;

        /** Reversal Indicator			*/
        public static String REVERSE_INDICATOR = "^";

        /**	Work Order Component Transaction Lines					*/
        private MVAMFGMWrkOdrTrnsctionLine[] m_lines = null;

        private StringBuilder warningLog = new StringBuilder();

        private StringBuilder infoLog = new StringBuilder();
        bool storeQtyUpdate = false;
        /**	Process Message 			*/
        private String _processMsg = null;


        private int _countGOM01 = 0;

        public MVAMFGMWrkOdrTransaction(Ctx ctx, int VAMFG_M_WorkOrderTransaction_ID,
            Trx trx)
            : base(ctx, VAMFG_M_WorkOrderTransaction_ID, trx)
        {
            //super(ctx, VAMFG_M_WrkOdrTransaction_ID, trx);

            //  New
            if (VAMFG_M_WorkOrderTransaction_ID == 0)
            {
                SetDocStatus(DOCSTATUS_Drafted);
                SetDocAction(DOCACTION_Prepare);
                SetC_DocType_ID(0);
                //SetVAMFG_DateAcct (new Timestamp (System.currentTimeMillis ()));
                SetVAMFG_DateAcct(DateTime.Now.Date);
                SetIsApproved(false);
                //super.setProcessed(false);
                SetProcessed(false);
                SetProcessing(false);
                SetPosted(false);
            }
        }

        /// <summary>
        /// Load Constructor
        ///</summary>
        ///<param name="ctx">ctx</param>
        ///<param name="dr">rs</param>
        /// <param name="trx">trx</param>
        public MVAMFGMWrkOdrTransaction(Ctx ctx, DataRow dr, Trx trx)
            : base(ctx, dr, trx)
        {
            //super(ctx, rs, trx);
        }
        /// <summary>
        /// Get Transactions of Work Order
        /// </summary>
        /// <param name="workorder">workorder</param>
        /// <param name="whereClause">whereClause</param>
        ///  <param name="orderClause">orderClause</param>
        /// <returns>Array of MWorkOrderTransactions</returns>
        public static MVAMFGMWrkOdrTransaction[] GetOfWorkOrder(MVAMFGMWorkOrder workorder, String whereClause, String orderClause)
        {
            StringBuilder sqlstmt = new StringBuilder("SELECT * FROM VAMFG_M_WrkOdrTransaction WHERE VAMFG_M_WorkOrder_ID=@parma1 ");
            if (whereClause != null)
                sqlstmt.Append("AND ").Append(whereClause);
            if (orderClause != null)
                sqlstmt.Append("ORDER BY ").Append(orderClause);
            String sql = sqlstmt.ToString();
            //ArrayList<MVAMFGMWrkOdrTransaction> list = new ArrayList<MVAMFGMWrkOdrTransaction>();
            //PreparedStatement pstmt = DB.prepareStatement (sql, workorder.get_Trx());
            //ResultSet rs = null;
            List<MVAMFGMWrkOdrTransaction> list = new List<MVAMFGMWrkOdrTransaction>();
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@parma1", workorder.GetVAMFG_M_WorkOrder_ID());
                idr = DB.ExecuteReader(sql, param, workorder.Get_TrxName());
                dt.Load(idr);
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list.Add(new MVAMFGMWrkOdrTransaction(workorder.GetCtx(), dt.Rows[i], workorder.Get_TrxName()));
                }

            }
            //    catch 
            catch
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                log.Log(Level.SEVERE, sql);
                return null;
            }


            MVAMFGMWrkOdrTransaction[] retValue = new MVAMFGMWrkOdrTransaction[list.Count];
            retValue = list.ToArray();
            return retValue;
        }	//	getOfWorkOrder

        /// <summary>
        /// Set Work Order ID - Callout
        /// This sets the product ID and the UOM for our work order transaction header.
        /// Also copies the Financial Accounting columns from the Work Order </summary>
        /// <param name="oldM_WorkOrder_ID">oldM_WorkOrder_ID old WO</param>
        /// <param name="newM_WorkOrder_ID">newM_WorkOrder_ID new WO</param>
        /// <param name="windowNo">windowNo window no </param>
        public void SetM_WorkOrder_ID(String oldM_WorkOrder_ID,
           String newM_WorkOrder_ID, int windowNo)
        {
            if (newM_WorkOrder_ID == null || newM_WorkOrder_ID.Length == 0)
                return;
            int VAMFG_M_WorkOrder_ID = VAdvantage.Utility.Util.GetValueOfInt(newM_WorkOrder_ID);
            if (VAMFG_M_WorkOrder_ID == 0)
                return;

            MVAMFGMWorkOrder workOrder = new MVAMFGMWorkOrder(GetCtx(), VAMFG_M_WorkOrder_ID, Get_TrxName());
            SetM_Product_ID(workOrder.GetM_Product_ID());
            SetC_UOM_ID(workOrder.GetC_UOM_ID());
            SetM_Locator_ID(workOrder.GetM_Locator_ID());

            SetC_BPartner_ID(workOrder.GetC_BPartner_ID());
            SetC_BPartner_Location_ID(workOrder.GetC_BPartner_Location_ID());
            SetAD_User_ID(workOrder.GetAD_User_ID());
            SetC_Project_ID(workOrder.GetC_Project_ID());
            SetC_Campaign_ID(workOrder.GetC_Campaign_ID());
            SetC_Activity_ID(workOrder.GetC_Activity_ID());
            SetUser1_ID(workOrder.GetVAMFG_User1_ID());
            SetUser2_ID(workOrder.GetVAMFG_User2_ID());

            MVAMFGMWorkOrderClass woc = new MVAMFGMWorkOrderClass(GetCtx(), workOrder.GetVAMFG_M_WorkOrderClass_ID(), Get_TrxName());
            SetC_DocType_ID(VAdvantage.Utility.Util.GetValueOfString(workOrder.GetC_DocType_ID()), VAdvantage.Utility.Util.GetValueOfString(woc.GetWOT_DocType_ID()), windowNo);

            if (GetVAMFG_WorkOrderTxnType() != null && GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
            {
                String sql = "SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID =" + VAMFG_M_WorkOrder_ID + " ORDER BY VAMFG_SeqNo";
                int VAMFG_M_WorkOrderOperation_ID = DB.GetSQLValue(Get_TrxName(), sql);
                SetOperationFrom_ID(VAMFG_M_WorkOrderOperation_ID);
            }
        }

        /// <summary>
        /// Set Work Order Txn Type - Callout
        /// </summary>
        /// For a work order txn type of 'WM' (work order move):
        /// If the routing is one step only, this sets the Operation From and Operation To, 
        /// as well as the Complete checkbox. Otherwise, this doesn't do anything
        /// <param name="oldWorkOrderTxnType">oldWorkOrderTxnType</param>
        /// <param name="newWorkOrderTxnType">newWorkOrderTxnType</param>
        /// <param name="windowNo">windowNo</param>

        public void SetWorkOrderTxnType(String oldWorkOrderTxnType,
        String newWorkOrderTxnType, int windowNo)
        {
            if (newWorkOrderTxnType == null || newWorkOrderTxnType.Length == 0)
                return;

            // set OperationFromID to help populate OperationTo
            if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
            {
                int operationID = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = @param1 ORDER BY VAMFG_SeqNo", GetVAMFG_M_WorkOrder_ID());
                SetOperationFrom_ID(operationID);
            }

            return;
        }

        /// <summary>
        /// Set WOComplete - UI Callout
        /// updates the OperationFrom_ID with the last mandatory operation
        /// updates the optional check-box against OperationFrom_ID
        /// updates StepTo as 'To Move'
        /// </summary>
        ///  <param name="oldWOComplete">ldWOComplete</param>
        /// <param name="newWOComplete">newWOComplete</param>
        ///<param name="windowNo">windowNo</param>

        public void SetWOComplete(String oldWOComplete,
            String newWOComplete, int windowNo)
        {

            //if(newWOComplete.isEmpty() || newWOComplete.Equals("N"))
            if (newWOComplete == null || newWOComplete.Equals("N"))
                return;

            MVAMFGMWorkOrderOperation lastMandatoryWOO = null;
            String sql = "SELECT * FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = @param1 AND VAMFG_IsOptional <> 'Y' ORDER BY VAMFG_SeqNo DESC";
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            //PreparedStatement pstmt = DB.prepareStatement(sql, get_Trx());
            //ResultSet rs = null;
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", GetVAMFG_M_WorkOrder_ID());
                idr = DB.ExecuteReader(sql, param, Get_TrxName());
                dt.Load(idr);
                idr.Close();
                if (dt.Rows.Count > 0)
                {
                    lastMandatoryWOO = new MVAMFGMWorkOrderOperation(GetCtx(), dt.Rows[0], Get_TrxName());
                }
                else
                {
                    SetVAMFG_IsOptionalTo(false);
                    return;
                }


            }
            //catch (SqlException e)
            catch
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                log.Log(Level.SEVERE, sql);
            }

            SetOperationTo_ID(lastMandatoryWOO.GetVAMFG_M_WorkOrderOperation_ID());
            if (lastMandatoryWOO.IsVAMFG_IsOptional())
            {
                SetVAMFG_IsOptionalTo(true);
            }
            else
            {
                SetVAMFG_IsOptionalTo(false);
            }
            SetVAMFG_StepTo(VAMFG_STEPTO_Finish);
            return;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldStepFrom">oldStepFrom</param>
        /// <param name="newStepFrom">newStepFrom</param>
        /// <param name="windowNo">windowNo</param>
        public void SetStepFrom(String oldStepFrom, String newStepFrom, int windowNo)
        {

            if (GetOperationFrom_ID() <= 0)
                return;
            if (newStepFrom == null)
                return;
            if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress))
            {
                MVAMFGMWorkOrderOperation opFrom = new MVAMFGMWorkOrderOperation(GetCtx(), GetOperationFrom_ID(), Get_TrxName());
                if (newStepFrom.Equals(VAMFG_STEPFROM_Waiting)) // Queue
                {
                    SetVAMFG_QtyEntered(opFrom.GetVAMFG_QtyQueued());
                    return;
                }
                else if (newStepFrom.Equals(VAMFG_STEPFROM_Process)) //Run
                {
                    SetVAMFG_QtyEntered(opFrom.GetVAMFG_QtyRun());
                    return;
                }
                else if (newStepFrom.Equals(VAMFG_STEPTO_Finish)) // Move
                {
                    SetVAMFG_QtyEntered(opFrom.GetVAMFG_QtyAssembled());
                    return;
                }
                else if (newStepFrom.Equals(VAMFG_STEPFROM_Scrap)) //Scrap
                {
                    //p_changeVO.addError(Msg.GetMsg(GetCtx(), "Error", "Cannot select scrap intra-operation step as starting operation."));
                    log.SaveError("StartingOperation", Msg.GetMsg(GetCtx(), "Error", "Cannot select scrap intra-operation step as starting operation."));
                    return;
                }
            }
        }

        /// <summary>
        /// Set OperationFrom_ID - Callout
        /// </summary>
        /// <param name="oldOperationFrom_ID">oldOperationFrom_ID</param>
        ///<param name="newOperationFrom_ID">newOperationFrom_ID</param>
        /// <param name="windowNo"> windowNo</param>


        public void SetOperationFrom_ID(String oldOperationFrom_ID,
            String newOperationFrom_ID, int windowNo)
        {

            if (newOperationFrom_ID == null || newOperationFrom_ID.Trim().Length == 0)
                return;

            int OperationFrom_ID = VAdvantage.Utility.Util.GetValueOfInt(newOperationFrom_ID);
            MVAMFGMWorkOrderOperation woo = new MVAMFGMWorkOrderOperation(GetCtx(), OperationFrom_ID, Get_TrxName());
            if (woo.IsVAMFG_IsOptional())
                SetVAMFG_IsOptionalFrom(true);
            else
                SetVAMFG_IsOptionalFrom(false);
            return;
        }


        public void SetVAMFG_QtyEntered(String oldQtyEntered,
            String newQtyEntered, int windowNo)
        {

            if (newQtyEntered == null || newQtyEntered.Trim().Length == 0)
                return;

            if (GetC_UOM_ID() == 0)
                return;

            Decimal QtyEntered = Convert.ToDecimal(newQtyEntered);
            Decimal QtyEntered1 = Decimal.Round((QtyEntered), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()));
            //BigDecimal QtyEntered1 = QtyEntered.setScale(
            //        MUOM.getPrecision(getCtx(), getC_UOM_ID()), BigDecimal.ROUND_HALF_UP);
            if (QtyEntered.CompareTo(QtyEntered1) != 0)
            {
                log.Fine("Corrected QtyEntered Scale UOM=" + GetC_UOM_ID()
                        + "; QtyEntered=" + QtyEntered + "->" + QtyEntered1);
                QtyEntered = QtyEntered1;
                SetVAMFG_QtyEntered(QtyEntered);
            }
        }

        /// <summary>
        /// Set OperationTo_ID - Callout
        ///</summary>
        ///<param name="oldOperationTo_ID">oldOperationTo_ID</param>
        ///<param name="newOperationTo_ID">newOperationTo_ID</param>
        /// <param name="windowNo">windowNo</param>
        public void SetOperationTo_ID(String oldOperationTo_ID,
            String newOperationTo_ID, int windowNo)
        {

            if (newOperationTo_ID == null || newOperationTo_ID.Trim().Length == 0)
                return;

            int OperationTo_ID = VAdvantage.Utility.Util.GetValueOfInt(newOperationTo_ID);
            MVAMFGMWorkOrderOperation woo = new MVAMFGMWorkOrderOperation(GetCtx(), OperationTo_ID, Get_TrxName());
            if (woo.IsVAMFG_IsOptional())
                SetVAMFG_IsOptionalTo(true);
            else
                SetVAMFG_IsOptionalTo(false);

            return;
        }

        /// <summary>
        /// Set Document Type.
        /// Sets DocumentNo
        /// </summary>
        ///  <param name="oldC_DocType_ID">oldC_DocType_ID old ID</param>
        ///  <param name="newC_DocType_ID">newC_DocType_ID new ID</param>
        ///  <param name="windowNo">windowNo window</param>
        public void SetC_DocType_ID(String oldC_DocType_ID,
                String newC_DocType_ID, int windowNo)
        {
            if (VAdvantage.Utility.Util.IsEmpty(newC_DocType_ID))
                return;
            int C_DocType_ID = Convert.ToInt32(newC_DocType_ID);
            if (C_DocType_ID == 0)
                return;

            //	Re-Create new DocNo, if there is a doc number already
            //	and the existing source used a different Sequence number
            String oldDocNo = GetDocumentNo();
            Boolean newDocNo = (oldDocNo == null);
            if (!newDocNo && oldDocNo.StartsWith("<") && oldDocNo.EndsWith(">"))
                newDocNo = true;
            int oldDocType_ID = GetC_DocType_ID();
            if (oldDocType_ID == 0 && !VAdvantage.Utility.Util.IsEmpty(oldC_DocType_ID))
                oldDocType_ID = Convert.ToInt32(oldC_DocType_ID);

            String sql = "SELECT d.DocBaseType, d.IsDocNoControlled,"
                + " s.CurrentNext, s.CurrentNextSys, s.AD_Sequence_ID "
                + "FROM C_DocType d"
                + " LEFT OUTER JOIN AD_Sequence s ON (d.DocNoSequence_ID=s.AD_Sequence_ID)"
                + "WHERE C_DocType_ID=@param1";		//	1
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            // PreparedStatement pstmt = DB.prepareStatement(sql, (Trx)null);
            //                pstmt.setInt(1, oldDocType_ID);
            //                ResultSet rs = pstmt.executeQuery();

            try
            {
                int AD_Sequence_ID = 0;

                //	Get old AD_SeqNo for comparison
                if (!newDocNo && oldDocType_ID != 0)
                {
                    param = new SqlParameter[1];
                    param[0] = new SqlParameter("@param1", oldDocType_ID);
                    idr = DB.ExecuteReader(sql, param, null);
                    if (idr.Read())
                    {
                        AD_Sequence_ID = VAdvantage.Utility.Util.GetValueOfInt(idr[4]);
                    }
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                    //    PreparedStatement pstmt = DB.prepareStatement(sql,  null);
                    //    pstmt.setInt(1, oldDocType_ID);
                    //    ResultSet rs = pstmt.executeQuery();
                    //    if (rs.next())
                    //        AD_Sequence_ID = rs.getInt(5);
                    //    rs.close();
                    //    pstmt.close();
                }

                //PreparedStatement pstmt = DB.prepareStatement(sql, null);
                //pstmt.setInt(1, C_DocType_ID);
                //ResultSet rs = pstmt.executeQuery();
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", C_DocType_ID);
                idr = DB.ExecuteReader(sql, param, null);
                if (idr.Read())
                {
                    SetC_DocType_ID(C_DocType_ID);

                    //p_changeVO.setContext(GetCtx(), windowNo, "C_DocTypeTarget_ID", C_DocType_ID);
                    p_ctx.SetContext(windowNo, GetCtx().ToString(), "C_DocTypeTarget_ID" + C_DocType_ID);
                    //	DocumentNo
                    if (VAdvantage.Utility.Util.GetValueOfString(idr[1]).Equals("Y"))
                    {
                        if (!newDocNo && AD_Sequence_ID != VAdvantage.Utility.Util.GetValueOfInt(idr[5]))
                            newDocNo = true;
                        if (newDocNo)
                            if (Ini.IsPropertyBool(Ini.P_VIENNASYS)
                                    && Env.GetCtx().GetAD_Client_ID() < 1000000)
                                SetDocumentNo("<" + VAdvantage.Utility.Util.GetValueOfString(idr[3]) + ">");
                            else
                                SetDocumentNo("<" + VAdvantage.Utility.Util.GetValueOfString(idr[2]) + ">");
                    }


                    //if (rs.getString(2).Equals("Y"))			//	IsDocNoControlled
                    //{
                    //    if (!newDocNo && AD_Sequence_ID != rs.getInt(6))
                    //        newDocNo = true;
                    //    if (newDocNo)
                    //        if (Ini.isPropertyBool(Ini.P_COMPIERESYS) 
                    //                && Env.getCtx().getAD_Client_ID() < 1000000)
                    //            setDocumentNo("<" + rs.getString(4) + ">");
                    //        else
                    //            setDocumentNo("<" + rs.getString(3) + ">");
                    //}

                }
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
            }
            catch
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                log.Log(Level.SEVERE, sql);
            }
        }	//	setC_DocType_ID

        //@Override
        public void SetM_WorkOrder_ID(int workOrderID)
        {
            base.SetVAMFG_M_WorkOrder_ID(workOrderID);
            MVAMFGMWorkOrder workOrder = new MVAMFGMWorkOrder(GetCtx(), workOrderID, Get_TrxName());
            SetM_Product_ID(workOrder.GetM_Product_ID());
            SetC_UOM_ID(workOrder.GetC_UOM_ID());
        }

        private void SetReversalM_WorkOrder_ID(int workOrderID)
        {
            base.SetVAMFG_M_WorkOrder_ID(workOrderID);
        }

        /// <summary>
        /// This sets the required (not null) columns besides the ones that are standard across all POs.
        /// </summary>
        /// <param name="workOrderID">workOrderID</param>
        /// <param name="locatorID">woTxnSource</param>
        /// <param name="woTxnSource">woTxnType</param>
        /// <param name="woTxnType"></param>
        public void SetRequiredColumns(int workOrderID, int locatorID, String woTxnSource, String woTxnType)
        {
            SetM_WorkOrder_ID(workOrderID);
            SetM_Locator_ID(locatorID);
            SetVAMFG_WOTxnSource(woTxnSource);
            SetVAMFG_WorkOrderTxnType(woTxnType);
            //SetVAMFG_DateTrx(DateTime.Now.Date;);
            SetVAMFG_DateTrx(DateTime.Now.ToLocalTime());

        } // setRequiredColumns

        /// <summary>
        /// Get Document Info
        /// </summary>
        /// <returns>document info (untranslated)</returns>
        public String GetDocumentInfo()
        {
            VAdvantage.Model.MDocType dt = VAdvantage.Model.MDocType.Get(GetCtx(), GetC_DocType_ID());
            return dt.GetName() + " " + GetDocumentNo();
        }	//	getDocumentInfo



        /// <summary>
        /// processInvComponentLine takes in a line and processes the inventory handling as well as
        /// updating the work order quantity for the component.
        /// </summary>
        /// <param name="woTxnLine">oTxnLine - The Work Order Transaction Line to process</param>
        ///  <param name="txnType"> txnType - The transaction type on the Work Order Transaction Header</param>
        ///  <returns>true if successfully processed components</returns>
        private Boolean ProcessInvComponentLine(MVAMFGMWrkOdrTrnsctionLine woTxnLine, String txnType)
        {

            Decimal newQty = Decimal.Zero;
            Decimal density = Decimal.Zero;
            Decimal Litre = Decimal.Zero;

            if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder) || txnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
            {
                //Added by Amit - 01-10-2016
                if (_countGOM01 > 0)
                {
                    density = woTxnLine.GetGOM01_Density();
                    Litre = woTxnLine.GetGOM01_Litre();
                    newQty = woTxnLine.GetGOM01_ActualQuantity();
                }
                else
                {
                    newQty = woTxnLine.GetVAMFG_QtyEntered();
                }
                //end
            }
            else if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder) || txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
            {
                newQty = Decimal.Negate(woTxnLine.GetVAMFG_QtyEntered());
            }
            else
            {
                //This is not a valid transaction type for inv component processing
                m_processMsg = GetVAMFG_M_WrkOdrTransaction_ID() + " - Invalid transaction type " + txnType + " for inventory component processing";
                return false;
            }

            MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            ViennaAdvantage.Model.MVAMFGMWorkOrder wo1 = new ViennaAdvantage.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            MVAMFGMWorkOrderOperation woo = null;
            // Adjust available qty on the assembly
            if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore)
                    || txnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
            {
                woo = new MVAMFGMWorkOrderOperation(GetCtx(), GetOperationFrom_ID(), Get_TrxName());
                if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
                {

                    Decimal woAssyQty = wo.GetVAMFG_QtyAvailable();
                    if (woAssyQty.CompareTo(woTxnLine.GetVAMFG_QtyEntered()) < 0)
                    {
                        m_processMsg = "No of assemblies - " + woAssyQty + ", not enough to issue from the work order - " + wo.GetVAMFG_M_WorkOrder_ID();
                        return false;
                    }

                    // Changed Code Here Test
                    // woo.SetVAMFG_QtyAssembled(Decimal.Add(newQty, (Decimal.Round((woo.GetVAMFG_QtyAssembled()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), wo.GetC_UOM_ID())))));

                    woo.SetVAMFG_QtyAssembled(Decimal.Add(newQty, (Decimal.Round((woo.GetVAMFG_QtyAssembled()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), wo.GetC_UOM_ID())))));
                }
                else	// Implicit assumption : AssemblyReturnFromInventory
                    //woo.SetVAMFG_QtyQueued(newQty.add(woo.GetVAMFG_QtyQueued()).setScale(MUOM.GetPrecision(GetCtx(),wo.GetC_UOM_ID())));
                    // ****************** Commented As it was updating Qty Queued

                    woo.SetVAMFG_QtyQueued(Decimal.Add(newQty, (Decimal.Round((woo.GetVAMFG_QtyQueued()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), wo.GetC_UOM_ID())))));

                if (!woo.Save(Get_TrxName()))
                {
                    return false;
                }
                UpdateAssemblyQty(newQty, true, false);
            }

            // Adjust the available quantity on M_WorkOrderComponent.
            // If there are multiple lines in M_WorkOrderComponent that match this product ID,
            // QtyAvailable on a specific product in WorkOrderComponent should represent the total qty available
            // across all operations and not just a specific component.
            // So, it's actually ok to update all lines like this.
            //
            // If that is the definition, then we must be sure to only sum up quantities for unique products
            // when reporting on the total quantity of parts available in a work order.
            //
            // The deplete in generateMoveComponentIssue works similarly.

            else if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder)
                    || txnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder))
            {
                String sql = "SELECT VAMFG_M_WorkOrderComponent_ID FROM VAMFG_M_WorkOrderComponent" +
                " WHERE VAMFG_M_WorkOrderOperation_ID = @param1 and M_Product_ID = @param2";
                VAdvantage.Model.MRole role = VAdvantage.Model.MRole.GetDefault(GetCtx(), false);
                sql = role.AddAccessSQL(sql, "VAMFG_M_WorkOrderComponent", VAdvantage.Model.MRole.SQL_NOTQUALIFIED, VAdvantage.Model.MRole.SQL_RW);

                int woCompID = 0;
                int componentsUpdated = 0;
                SqlParameter[] param = null;
                IDataReader idr = null;
                //PreparedStatement pstmt = DB.prepareStatement(sql,get_Trx());
                //ResultSet rs = null;
                Boolean success = true;
                MVAMFGMWorkOrderComponent woComp = null;
                try
                {
                    param = new SqlParameter[2];
                    param[0] = new SqlParameter("@param1", woTxnLine.GetVAMFG_M_WorkOrderOperation_ID());
                    param[1] = new SqlParameter("@param2", woTxnLine.GetM_Product_ID());
                    //pstmt.setInt(1, woTxnLine.GetVAMFG_M_WorkOrderOperation_ID());
                    //pstmt.setInt(2, woTxnLine.getM_Product_ID());
                    ////pstmt.setInt(3, woTxnLine.getM_AttributeSetInstance_ID());
                    ////pstmt.setInt(4, woTxnLine.getM_AttributeSetInstance_ID());

                    //rs = pstmt.executeQuery();
                    idr = DB.ExecuteReader(sql.ToString(), param, Get_TrxName());


                    while (idr.Read())
                    {
                        woCompID = VAdvantage.Utility.Util.GetValueOfInt(idr[0]);
                        woComp = new MVAMFGMWorkOrderComponent(GetCtx(), woCompID, Get_TrxName());
                        Decimal woCompQty = Decimal.Subtract(woComp.GetVAMFG_QtyAvailable(), (woComp.GetVAMFG_QtySpent()));	//QtyIssued - QtyUsed
                        if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder) && woCompQty.CompareTo(woTxnLine.GetVAMFG_QtyEntered()) < 0)
                        {
                            m_processMsg = "No of components - " + woCompQty + ", in work order component line - " +
                            woComp.GetVAMFG_M_WorkOrderComponent_ID() + ", not enough to allow component return from the work order - " + wo.GetVAMFG_M_WorkOrder_ID();
                            success = false;
                            break;
                        }
                    }
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                    if (woCompID > 0)
                    {
                        //woComp.SetVAMFG_QtyAvailable(woComp.GetVAMFG_QtyAvailable().add(newQty).setScale(MUOM.GetPrecision(GetCtx(), woComp.getCUOM_ID())));
                        woComp.SetVAMFG_QtyAvailable(Decimal.Add(woComp.GetVAMFG_QtyAvailable(), Decimal.Round(((newQty)), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), woComp.GetC_UOM_ID()))));
                        //Added by Amit - 01-10-2016
                        if (_countGOM01 > 0)
                        {
                            //woComp.SetGOM01_Density(Decimal.Add(woComp.GetGOM01_Density(), density));
                            //woComp.SetGOM01_Litre(Decimal.Add(woComp.GetGOM01_Litre(), Litre));                                                   
                        }
                        //end

                        if (!woComp.Save(Get_TrxName()))
                        {
                            success = false;
                            //break;
                        }
                        Decimal overissue = 0;
                        if (_countGOM01 > 0)
                        {
                            overissue = Decimal.Subtract(woComp.GetVAMFG_QtyAvailable(), woComp.GetGOM01_Quantity());
                        }
                        else
                        {
                            overissue = Decimal.Subtract(woComp.GetVAMFG_QtyAvailable(), Decimal.Multiply(wo.GetVAMFG_QtyEntered(), (woComp.GetVAMFG_QtyRequired())));
                        }
                        if (overissue.CompareTo(Decimal.Zero) > 0)
                        {
                            warningLog.Append("\nOverissue of " + overissue + " for component " + VAdvantage.Model.MProduct.Get(GetCtx(), woComp.GetM_Product_ID()) + ", M_WorkOrderComponent_ID: " + woComp.GetVAMFG_M_WorkOrderComponent_ID());
                        }

                        componentsUpdated++;
                        //	Keep the Work Order Operation for Actual Date update later.
                        woo = new MVAMFGMWorkOrderOperation(GetCtx(), woComp.GetVAMFG_M_WorkOrderOperation_ID(), Get_TrxName());
                    }
                }
                catch
                {
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                    log.Log(Level.SEVERE, sql);
                    return false;
                }

                if (!success)
                    return false;

                if (componentsUpdated == 0)
                {
                    // For a component issue, if nothing was updated, that means this is a component that is new to the work order.// In that case, add an MVAMFGMWorkOrderComponent and then try updating again.
                    if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder))
                    {

                        woo = new MVAMFGMWorkOrderOperation(GetCtx(), woTxnLine.GetVAMFG_M_WorkOrderOperation_ID(), Get_TrxName());
                        ViennaAdvantage.Model.MProduct prod = new ViennaAdvantage.Model.MProduct(GetCtx(), woTxnLine.GetM_Product_ID(), Get_TrxName());
                        VAdvantage.Model.MLocator loc = new VAdvantage.Model.MLocator(GetCtx(), GetM_Locator_ID(), Get_TrxName());
                        woComp = new MVAMFGMWorkOrderComponent(wo1, woo, prod, Decimal.Zero, X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_Push, 0, loc);
                        //woComp.SetVAMFG_QtyAvailable(newQty.setScale(MUOM.GetPrecision(GetCtx(), woComp.GetC_UOM_ID()), BigDecimal.ROUND_HALF_UP));
                        woComp.SetVAMFG_QtyAvailable(Decimal.Round((newQty), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), woComp.GetC_UOM_ID())));
                        //Added by Amit - 01-10-2016
                        if (_countGOM01 > 0)
                        {
                            woComp.SetGOM01_Density(density);
                            woComp.SetGOM01_Litre(Litre);
                            woComp.SetGOM01_Quantity(Decimal.Round((newQty), MUOM.GetPrecision(GetCtx(), woComp.GetC_UOM_ID())));
                            Decimal unitQty = Decimal.Divide(newQty, wo.GetGOM01_Quantity());
                            woComp.SetVAMFG_QtyRequired(Decimal.Round(unitQty, MUOM.GetPrecision(GetCtx(), woComp.GetC_UOM_ID())));
                            // set quality correction value of new component added
                            woComp.Set_Value("IsQualityCorrection", Util.GetValueOfBool(woTxnLine.Get_Value("IsQualityCorrection")));
                        }
                        //end
                        woComp.SetProcessed(true);
                        if (!woComp.Save(Get_TrxName()))
                        {
                            return false;
                        }
                    }
                    else
                    { // txnType.Equals(COMPONENTRETURN)
                        // should never reach here -- there is a validation on the item list for component return lines
                        m_processMsg = "Not enough quantity to return from the work order.";
                        return false;
                    }
                }
            }


            if (!ProcessInventory(woTxnLine, txnType))
                return false;

            //	Update the Actual Dates on the Work Order Operation only if the process is successful
            if (woo != null)
            {
                DateTime? trxDate = GetVAMFG_DateTrx().Value.ToLocalTime();
                //change by Amit - 30-Sep-2016
                if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress) && !IsVAMFG_WOComplete() && _countGOM01 > 0)
                {
                    if (woo.GetVAMFG_DateActualFrom() == null)
                        woo.SetVAMFG_DateActualFrom(GetGOM01_OperationStartDate());
                    if (woo.GetVAMFG_DateActualTo() == null)
                        woo.SetVAMFG_DateActualTo(GetGOM01_OperationEndDate());
                }
                else
                {
                    //Timestamp trxDate = GetVAMFG_DateTrx();
                    if (trxDate == null)
                        //trxDate = new Timestamp(System.currentTimeMillis());
                        trxDate = DateTime.Now.Date;

                    // Code Commented by Bharat on 08 Jan 2018 as issue given by Pradeep to not update Operation in case of Transfer Assembly to Store

                    //if (woo.GetVAMFG_DateActualFrom() == null || woo.GetVAMFG_DateActualFrom().Value.ToLocalTime() > (trxDate))
                    //    woo.SetVAMFG_DateActualFrom(trxDate);
                    //if (woo.GetVAMFG_DateActualTo() == null || woo.GetVAMFG_DateActualTo().Value.ToLocalTime() < (trxDate))
                    //    woo.SetVAMFG_DateActualTo(trxDate);
                }
                woo.Save(Get_TrxName());
            }

            return true;


        } // generateInvComponentTxn

        private Boolean ProcessInventory(MVAMFGMWrkOdrTrnsctionLine woTxnLine, String txnType)
        {

            CheckMaterialPolicy(woTxnLine);

            VAdvantage.Model.MProduct product = VAdvantage.Model.MProduct.Get(GetCtx(), GetM_Product_ID());

            if (!product.IsStocked())
            {
                log.Info("Product " + product + " is not stocked. Not creating MTransaction");
                SetProcessed(true);
                return Save(Get_TrxName());
            }

            Boolean workOrderIn = false;
            if (txnType.Equals(VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder) || txnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
            {
                workOrderIn = true;
            }


            if (woTxnLine.GetM_AttributeSetInstance_ID() == 0)
            {
                MVAMFGMWrkOdrTxnLineMA[] mas = MVAMFGMWrkOdrTxnLineMA.Get(GetCtx(), woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID(), Get_TrxName());

                foreach (MVAMFGMWrkOdrTxnLineMA ma in mas)
                {
                    Decimal qtyMA = workOrderIn ? Decimal.Negate(ma.GetVAMFG_MovementQty()) : ma.GetVAMFG_MovementQty();

                    //Decimal movementQty = Decimal.Zero;
                    //if (_countGOM01 > 0)
                    //{
                    //    movementQty = workOrderIn ? Decimal.Negate(woTxnLine.GetGOM01_ActualQuantity()) : woTxnLine.GetGOM01_ActualQuantity();
                    //}
                    //else
                    //{
                    //    movementQty = workOrderIn ? Decimal.Negate(woTxnLine.GetVAMFG_QtyEntered()) : woTxnLine.GetVAMFG_QtyEntered();
                    //}

                    if (!ProcessMTrxAndStorage(woTxnLine, workOrderIn, 0, qtyMA))
                    {
                        log.Severe("Error processing MA");
                        return false;
                    }
                }

                return true;
            }
            else
            { // woTxnLine M_AttributesetInstance_ID != 0

                Decimal movementQty = Decimal.Zero;
                if (_countGOM01 > 0)
                {
                    movementQty = workOrderIn ? Decimal.Negate(woTxnLine.GetGOM01_ActualQuantity()) : GetGOM01_ActualLiter();
                }
                else
                {
                    movementQty = workOrderIn ? Decimal.Negate(woTxnLine.GetVAMFG_QtyEntered()) : woTxnLine.GetVAMFG_QtyEntered();
                }
                if (!(txnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory)
                    || txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore)))
                    return ProcessMTrxAndStorage(woTxnLine, workOrderIn, woTxnLine.GetM_AttributeSetInstance_ID(), movementQty);
                else
                    return true;
            }
        } // processInventory

        /// <summary>
        /// Check Material Policy
        /// Sets line ASI
        /// </summary>
        /// <param name="line"></param>
        private void CheckMaterialPolicy(MVAMFGMWrkOdrTrnsctionLine line)
        {
            int no = 0;
            String txnType = GetVAMFG_WorkOrderTxnType();
            ViennaAdvantage.CMFG.Model.MVAMFGMWrkOdrTrnsctionLine Lines = new CMFG.Model.MVAMFGMWrkOdrTrnsctionLine(GetCtx(), line.GetVAMFG_M_WrkOdrTrnsctionLine_ID(), Get_TrxName());
            if (!txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
            {
                no = MVAMFGMWrkOdrTxnLineMA.DeleteWorkOrderTransactionLineMA(line.GetVAMFG_M_WrkOdrTrnsctionLine_ID(), Get_TrxName());
            }
            if (no > 0)
                log.Config("Delete old #" + no);


            // toInvTrx indicates whether or not this is a line that represents product movement into inventory
            //Boolean toInvTrx = txnType.Equals(WORKORDERTXNTYPE_ComponentReturnFromWorkOrder)
            //|| txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore);

            Boolean toInvTrx = txnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder);
            VAdvantage.Model.MClient client = VAdvantage.Model.MClient.Get(GetCtx());
            Boolean needSave = false;
            VAdvantage.Model.MProduct product = new VAdvantage.Model.MProduct(GetCtx(), line.GetM_Product_ID(), Get_TrxName());

            //	Need to have Location
            if (product != null && line.GetM_Locator_ID() == 0)
            {
                // this will get a default locator using the default warehouse for the org
                line.SetM_Locator_ID(toInvTrx ? Env.ZERO : line.GetVAMFG_QtyEntered());
                needSave = true;
            }

            //	Attribute Set Instance
            if (product != null && line.GetM_AttributeSetInstance_ID() == 0)
            {
                if (toInvTrx)
                {
                    VAdvantage.Model.MAttributeSetInstance asi = new VAdvantage.Model.MAttributeSetInstance(GetCtx(), 0, Get_TrxName());
                    asi.SetClientOrg(GetAD_Client_ID(), 0);
                    asi.SetM_AttributeSet_ID(product.GetM_AttributeSet_ID());
                    if (asi.Save(Get_TrxName()))
                    {
                        line.SetM_AttributeSetInstance_ID(asi.GetM_AttributeSetInstance_ID());
                        log.Config("New ASI=" + line);
                        needSave = true;
                    }
                }
                else	//	to Work Order Trx
                {
                    VAdvantage.Model.MProductCategory pc = VAdvantage.Model.MProductCategory.Get(GetCtx(), product.GetM_Product_Category_ID());
                    String MMPolicy = pc.GetMMPolicy();
                    if (MMPolicy == null || MMPolicy.Length == 0)
                        MMPolicy = client.GetMMPolicy();
                    //
                    Decimal qtyToDeliver = line.GetVAMFG_QtyEntered();
                    if (_countGOM01 > 0 && !(txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore) || txnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder)))
                    {
                        qtyToDeliver = line.GetGOM01_ActualQuantity();
                    }

                    MVAMFGMWrkOdrTxnLineMA[] mwot = MVAMFGMWrkOdrTxnLineMA.Get(GetCtx(), line.GetVAMFG_M_WrkOdrTrnsctionLine_ID(), Get_TrxName());
                    if (mwot.Length == 0)
                    {
                        // get storage record where ASI is null
                        VAdvantage.Model.MStorage[] storages = VAdvantage.Model.MStorage.GetAllWithASI(GetCtx(),
                           line.GetM_Product_ID(), line.GetM_Locator_ID(),
                           VAdvantage.Model.X_AD_Client.MMPOLICY_FiFo.Equals(MMPolicy), Get_TrxName());

                        for (int ii = 0; ii < storages.Length; ii++)
                        {
                            //Storage.Record storage = storages[ii];
                            VAdvantage.Model.MStorage storage = storages[ii];
                            // Decimal qtyAvailable = storage.getQtyOnHand().subtract(storage.getQtyDedicated()).subtract(storage.getQtyAllocated());
                            Decimal qtyAvailable = 0.0M;
                            if (VAdvantage.Utility.Util.GetValueOfString(GetConsiderReservedQty()) == "Y")
                            {
                                qtyAvailable = Decimal.Subtract(storage.GetQtyOnHand(), storage.GetQtyReserved());
                            }
                            else
                            {
                                qtyAvailable = storage.GetQtyOnHand();
                            }
                            if (qtyAvailable.CompareTo(Env.ZERO) <= 0)
                            {
                                continue;
                            }
                            MVAMFGMWrkOdrTxnLineMA ma = new MVAMFGMWrkOdrTxnLineMA(Lines, storage.GetM_AttributeSetInstance_ID(), qtyToDeliver);

                            if (qtyAvailable.CompareTo(qtyToDeliver) >= 0)
                            {
                                qtyToDeliver = Env.ZERO;
                            }
                            else
                            {
                                ma.SetVAMFG_MovementQty(qtyAvailable);
                                qtyToDeliver = Decimal.Subtract(qtyToDeliver, qtyAvailable);
                            }
                            if (!ma.Save(Get_TrxName()))
                            {
                                log.Fine("failed to save");
                            }
                            Get_TrxName().Commit();
                            log.Fine("#" + ii + ": " + ma + ", QtyToDeliver=" + qtyToDeliver);
                            //}
                            if (Env.Signum(qtyToDeliver) == 0)
                                break;
                        }	//	 for all storages

                        //	No AttributeSetInstance found for remainder
                        if (Env.Signum(qtyToDeliver) != 0)
                        {
                            MVAMFGMWrkOdrTxnLineMA ma = new MVAMFGMWrkOdrTxnLineMA(Lines, line.GetM_AttributeSetInstance_ID(), qtyToDeliver);
                            ma.SetVAMFG_M_WrkOdrTrnsctionLine_ID(line.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
                            if (!ma.Save(Get_TrxName()))
                                log.Fine("failed to save");
                            // neede to commit to save attribute data on attribute tab of execution window done by Vivek on 09/01/2018
                            Get_TrxName().Commit();
                            log.Fine("##: " + ma);
                        }
                    }
                }
            }
            else if (product != null && line.GetM_AttributeSetInstance_ID() > 0)
            {
                VAdvantage.Model.MProductCategory pc = VAdvantage.Model.MProductCategory.Get(GetCtx(), product.GetM_Product_Category_ID());
                String MMPolicy = pc.GetMMPolicy();
                if (MMPolicy == null || MMPolicy.Length == 0)
                    MMPolicy = client.GetMMPolicy();
                //
                Decimal qtyToDeliver = line.GetVAMFG_QtyEntered();
                if (_countGOM01 > 0 && !(txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore) || txnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder)))
                {
                    qtyToDeliver = line.GetGOM01_ActualQuantity();
                }

                MVAMFGMWrkOdrTxnLineMA[] mwot = MVAMFGMWrkOdrTxnLineMA.Get(GetCtx(), line.GetVAMFG_M_WrkOdrTrnsctionLine_ID(), Get_TrxName());
                if (mwot.Length == 0)
                {
                    VAdvantage.Model.MStorage[] storages = VAdvantage.Model.MStorage.GetAllWithASI(GetCtx(),
                       line.GetM_Product_ID(), line.GetM_Locator_ID(),
                       VAdvantage.Model.X_AD_Client.MMPOLICY_FiFo.Equals(MMPolicy), Get_TrxName());

                    for (int ii = 0; ii < storages.Length; ii++)
                    {
                        //Storage.Record storage = storages[ii];
                        VAdvantage.Model.MStorage storage = storages[ii];
                        // Decimal qtyAvailable = storage.getQtyOnHand().subtract(storage.getQtyDedicated()).subtract(storage.getQtyAllocated());
                        Decimal qtyAvailable = 0.0M;
                        if (VAdvantage.Utility.Util.GetValueOfString(GetConsiderReservedQty()) == "Y")
                        {
                            qtyAvailable = Decimal.Subtract(storage.GetQtyOnHand(), storage.GetQtyReserved());
                        }
                        else
                        {
                            qtyAvailable = storage.GetQtyOnHand();
                        }
                        if (qtyAvailable.CompareTo(Env.ZERO) <= 0)
                        {
                            continue;
                        }
                        MVAMFGMWrkOdrTxnLineMA ma = new MVAMFGMWrkOdrTxnLineMA(Lines, storage.GetM_AttributeSetInstance_ID(), qtyToDeliver);

                        if (qtyAvailable.CompareTo(qtyToDeliver) >= 0)
                        {
                            qtyToDeliver = Env.ZERO;
                        }
                        else
                        {
                            ma.SetVAMFG_MovementQty(qtyAvailable);
                            qtyToDeliver = Decimal.Subtract(qtyToDeliver, qtyAvailable);
                        }
                        if (!ma.Save(Get_TrxName()))
                        {
                            log.Fine("failed to save");
                        }
                        Get_TrxName().Commit();
                        log.Fine("#" + ii + ": " + ma + ", QtyToDeliver=" + qtyToDeliver);
                        //}
                        if (Env.Signum(qtyToDeliver) == 0)
                            break;
                    }	//	 for all storages

                    //	No AttributeSetInstance found for remainder
                    if (Env.Signum(qtyToDeliver) != 0)
                    {
                        MVAMFGMWrkOdrTxnLineMA ma = new MVAMFGMWrkOdrTxnLineMA(Lines, line.GetM_AttributeSetInstance_ID(), qtyToDeliver);
                        ma.SetVAMFG_M_WrkOdrTrnsctionLine_ID(line.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
                        if (!ma.Save(Get_TrxName()))
                            log.Fine("failed to save");
                        // neede to commit to save attribute data on attribute tab of execution window done by Vivek on 09/01/2018
                        Get_TrxName().Commit();
                        log.Fine("##: " + ma);
                    }
                }

            }

            if (needSave && !line.Save(Get_TrxName()))
                log.Severe("NOT saved " + line);
        }	//	checkMaterialPolicy


        private Boolean ProcessMTrxAndStorage(MVAMFGMWrkOdrTrnsctionLine woTxnLine, Boolean workOrderIn,
                int ASI_ID, Decimal movementQty)
        {
            MTransaction mTrx = new MTransaction(GetCtx(), GetAD_Org_ID(),
                workOrderIn ? X_M_Transaction.MOVEMENTTYPE_WorkOrder_ : X_M_Transaction.MOVEMENTTYPE_WorkOrderPlus,
                            woTxnLine.GetM_Locator_ID(), woTxnLine.GetM_Product_ID(), ASI_ID,
                            movementQty, GetVAMFG_DateTrx().Value.ToLocalTime(), Get_TrxName());
            //  mTrx.SetM_WorkOrderTransactionLine_ID(woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
            mTrx.SetVAMFG_M_WrkOdrTrnsctionLine_ID(woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
            if (IsReversal())
            {
                mTrx.SetMovementType(X_M_Transaction.MOVEMENTTYPE_WorkOrderPlus);
            }
            MLocator loc = MLocator.Get(GetCtx(), woTxnLine.GetM_Locator_ID());


            // ****************  Check for DisAllow Negative Inventory at warehouse

            if (workOrderIn)
            {
                string sqlDis = "select IsDisallowNegativeInv from m_warehouse where m_warehouse_id = " + loc.GetM_Warehouse_ID();
                string IsDisallowNegative = VAdvantage.Utility.Util.GetValueOfString(DB.ExecuteScalar(sqlDis, null, Get_TrxName()));

                if (IsDisallowNegative == "Y")
                {
                    VAdvantage.Model.MStorage st = VAdvantage.Model.MStorage.Get(Env.GetCtx(), loc.GetM_Locator_ID(), woTxnLine.GetM_Product_ID(), ASI_ID, Get_TrxName());
                    if (st == null)         // Added by Bharat on 20 Dec 2017 as giving Object Reference error when Product not available on Warehouse.
                    {
                        return false;
                    }

                    Decimal qty = st.GetQtyOnHand();

                    // check how many qty available on VAMFG_M_WrkOdrTxnLineMA against same WO Transaction line
                    // if qty on MA is less than qty on WO tansaction Line then no need to complete the process
                    ////                    Decimal qtyMA = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(@"SELECT SUM(vamfg_movementqty) FROM VAMFG_M_WrkOdrTxnLineMA
                    ////                     WHERE IsActive = 'Y' AND VAMFG_M_WrkOdrTrnsctionLine_ID = " + woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID()));

                    // check qty on WO Transaction line
                    //String txnType = GetVAMFG_WorkOrderTxnType();
                    //Decimal qtyToDeliver = woTxnLine.GetVAMFG_QtyEntered();
                    //if (_countGOM01 > 0 && !(txnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore) || txnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder)))
                    //{
                    //    qtyToDeliver = woTxnLine.GetGOM01_ActualQuantity();
                    //}
                    if (Decimal.Add(qty, movementQty) >= 0)
                    {

                    }
                    else
                    {
                        return false;
                    }
                }

            }
            //if (Storage.AddQtys(GetCtx(), loc.GetM_Warehouse_ID(), woTxnLine.GetM_Locator_ID(),
            //        woTxnLine.GetM_Product_ID(), ASI_ID, 0,
            //        movementQty, Decimal.Zero, Decimal.Zero, Get_TrxName()))
            if (VAdvantage.Model.MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), woTxnLine.GetM_Locator_ID(),
                    woTxnLine.GetM_Product_ID(), ASI_ID, 0,
                    movementQty, Decimal.Zero, Decimal.Zero, Get_TrxName()))
            {
                storeQtyUpdate = true;

                // Change To Add Entry of WorkOrder and WorkOrderTransaction IDs in Product's Transaction Tab
                //mTrx.SetM_WorkOrderTransactionLine_ID(woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
                //mTrx.SetM_WorkOrderTransaction_ID(woTxnLine.GetVAMFG_M_WrkOdrTransaction_ID());
                mTrx.SetVAMFG_M_WrkOdrTrnsctionLine_ID(woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
                mTrx.SetVAMFG_M_WrkOdrTransaction_ID(woTxnLine.GetVAMFG_M_WrkOdrTransaction_ID());


                string sql = "select VAMFG_M_WorkOrder_ID from VAMFG_M_WrkOdrTransaction where VAMFG_M_WrkOdrTransaction_ID = " + woTxnLine.GetVAMFG_M_WrkOdrTransaction_ID();
                int VAMFG_M_WorkOrder_ID = VAdvantage.Utility.Util.GetValueOfInt(DB.ExecuteScalar(sql, null, Get_TrxName()));

                // mTrx.SetM_WorkOrder_ID(VAMFG_M_WorkOrder_ID);
                mTrx.SetVAMFG_M_WorkOrder_ID(VAMFG_M_WorkOrder_ID);
                // Added by Bharat on 18 August 2017 to update Current Quantity on M_Transaction.
                sql = @"SELECT SUM(t.CurrentQty) keep (dense_rank last ORDER BY t.MovementDate, t.M_Transaction_ID) AS CurrentQty FROM m_transaction t 
                            INNER JOIN M_Locator l ON t.M_Locator_ID = l.M_Locator_ID WHERE t.MovementDate <= " + GlobalVariable.TO_DATE(GetVAMFG_DateTrx(), true) +
                            " AND t.AD_Client_ID = " + GetAD_Client_ID() + " AND l.AD_Org_ID = " + GetAD_Org_ID() + " AND t.M_Locator_ID = " + woTxnLine.GetM_Locator_ID() +
                            " AND t.M_Product_ID = " + woTxnLine.GetM_Product_ID() + " AND NVL(t.M_AttributeSetInstance_ID,0) = " + ASI_ID;
                Decimal trxQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, Get_TrxName()));
                mTrx.SetCurrentQty(trxQty + movementQty);
                if (mTrx.Save(Get_TrxName()))
                {
                    Decimal currentQty = movementQty + trxQty;
                    UpdateTransaction(woTxnLine, mTrx, currentQty);
                    SetProcessed(true);
                    if (Save(Get_TrxName()))
                        return true;
                    else
                    {
                        log.Log(Level.SEVERE, "VAMFG_M_WrkOdrTrnsctionLine " + woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID() + ": processInventory: Work Order Transaction not saved");
                    }
                }
                else
                {
                    log.Log(Level.SEVERE, "VAMFG_M_WrkOdrTrnsctionLine " + woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID() + "processInventory: MTransaction not saved");
                }
            }
            else
            {
                VAdvantage.Model.ValueNamePair pp = VLogger.RetrieveError();
                if (pp != null)
                {
                    VAdvantage.Model.MProduct pro = new VAdvantage.Model.MProduct(Env.GetCtx(), woTxnLine.GetM_Product_ID(), null);
                    m_processMsg = pp.GetName();
                    log.Log(Level.SEVERE, "VAMFG_M_WrkOdrTrnsctionLine " + woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID() + " processInventory: MStorage not updated" + pp.GetName() +
                    " or NotEnoughQty against product:" + pro.GetName());
                }
                else
                {
                    log.Log(Level.SEVERE, "VAMFG_M_WrkOdrTrnsctionLine " + woTxnLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID() + "processInventory: MStorage not updated");
                }
            }
            return false;
        }

        /// <summary>
        /// Added by Bharata to Update Transaction Tab to set Current Qty
        /// </summary>
        /// <param name="line"></param>
        /// <param name="trx"></param>
        /// <param name="qtyDiff"></param>
        private void UpdateTransaction(MVAMFGMWrkOdrTrnsctionLine line, MTransaction trxM, decimal qtyDiffer)
        {
            VAdvantage.Model.MProduct pro = new VAdvantage.Model.MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
            MTransaction trx = null;
            int attribSet_ID = pro.GetM_AttributeSet_ID();
            string sql = "";
            DataSet ds = new DataSet();
            try
            {
                if (attribSet_ID > 0)
                {
                    sql = @"SELECT M_AttributeSetInstance_ID ,  M_Locator_ID ,  M_Product_ID ,  movementqty ,  currentqty ,  movementdate ,  TO_CHAR(Created, 'DD-MON-YY HH24:MI:SS') , m_transaction_id , MovementType , M_InventoryLine_ID
                              FROM m_transaction WHERE movementdate >= " + GlobalVariable.TO_DATE(trxM.GetMovementDate().Value.AddDays(1), true)
                               + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID() + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID()
                               + " ORDER BY movementdate ASC , m_transaction_id ASC , created ASC";
                }
                else
                {
                    sql = @"SELECT M_AttributeSetInstance_ID ,  M_Locator_ID ,  M_Product_ID ,  movementqty ,  currentqty ,  movementdate ,  TO_CHAR(Created, 'DD-MON-YY HH24:MI:SS') , m_transaction_id , MovementType , M_InventoryLine_ID
                              FROM m_transaction WHERE movementdate >= " + GlobalVariable.TO_DATE(trxM.GetMovementDate().Value.AddDays(1), true)
                               + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID() + " AND M_AttributeSetInstance_ID = 0 "
                               + " ORDER BY movementdate ASC , m_transaction_id ASC , created ASC";
                }

                ds = DB.ExecuteDataset(sql, null, Get_TrxName());
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        int i = 0;
                        for (i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            trx = new MTransaction(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]), Get_TrxName());
                            trx.SetCurrentQty(qtyDiffer + trx.GetMovementQty());
                            if (!trx.Save())
                            {
                                log.Info("Current Quantity Not Updated at Transaction Tab for this ID" + VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]));
                            }
                            else
                            {
                                qtyDiffer = trx.GetCurrentQty();
                            }
                            if (i == ds.Tables[0].Rows.Count - 1)
                            {
                                MStorage storage = MStorage.Get(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                if (storage == null)
                                {
                                    storage = MStorage.GetCreate(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                    VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                }
                                if (storage.GetQtyOnHand() != qtyDiffer)
                                {
                                    storage.SetQtyOnHand(qtyDiffer);
                                    storage.Save();
                                }
                            }
                        }
                    }
                }
                ds.Dispose();
            }
            catch
            {
                if (ds != null)
                {
                    ds.Dispose();
                }
                log.Info("Current Quantity Not Updated at Transaction Tab");
            }
        }

        /// <summary>
        /// Added by Bharata to Update Transaction Tab to set Current Qty
        /// </summary>
        /// <param name="line"></param>
        /// <param name="trx"></param>
        /// <param name="qtyDiff"></param>
        private void UpdateTransaction(MVAMFGMWrkOdrTransaction line, MTransaction trxM, decimal qtyDiffer)
        {
            MTransaction trx = null;
            string sql = "";
            DataSet ds = new DataSet();
            try
            {
                sql = @"SELECT M_AttributeSetInstance_ID ,  M_Locator_ID ,  M_Product_ID ,  movementqty ,  currentqty ,  movementdate ,  TO_CHAR(Created, 'DD-MON-YY HH24:MI:SS') , m_transaction_id , MovementType , M_InventoryLine_ID
                              FROM m_transaction WHERE movementdate >= " + GlobalVariable.TO_DATE(trxM.GetMovementDate().Value.AddDays(1), true)
                           + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID() + " AND M_AttributeSetInstance_ID = 0 "
                     + " ORDER BY movementdate ASC , m_transaction_id ASC , created ASC";

                ds = DB.ExecuteDataset(sql, null, Get_TrxName());
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        int i = 0;
                        for (i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            trx = new MTransaction(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]), Get_TrxName());
                            trx.SetCurrentQty(qtyDiffer + trx.GetMovementQty());
                            if (!trx.Save())
                            {
                                log.Info("Current Quantity Not Updated at Transaction Tab for this ID" + VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]));
                            }
                            else
                            {
                                qtyDiffer = trx.GetCurrentQty();
                            }
                            if (i == ds.Tables[0].Rows.Count - 1)
                            {
                                MStorage storage = MStorage.Get(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                if (storage == null)
                                {
                                    storage = MStorage.GetCreate(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                    VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                }
                                if (storage.GetQtyOnHand() != qtyDiffer)
                                {
                                    storage.SetQtyOnHand(qtyDiffer);
                                    storage.Save();
                                }
                            }
                        }
                    }
                }
                ds.Dispose();
            }
            catch
            {
                if (ds != null)
                {
                    ds.Dispose();
                }
                log.Info("Current Quantity Not Updated at Transaction Tab");
            }
        }

        private int m_nextLineNo = 10;

        /// <summary>
        ///  generateMoveComponentIssueTxn creates the following as necessary:
        ///  1. component issue txn on work order side
        ///  2. for each component, the component issue lines on wo side
        ///  3. component consumption/deplete lines on the move txn
        /// 
        ///  This completes the component issue txn as well.
        ///  </summary>
        ///  <param name="opFrom"></param>
        ///  <param name="opTo"></param>
        ///   <returns> a Boolean: true for success, false for fail</returns>


        private Boolean GenerateMoveComponentIssueTxn(int opFrom, int opTo)
        {

            int count = 0;
            MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder wo1 = new CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            String whereClause = " VAMFG_SeqNo BETWEEN " + opFrom + " AND " + opTo;
            MVAMFGMWorkOrderOperation[] woos = MVAMFGMWorkOrderOperation.GetOfWorkOrder(wo1, whereClause, "VAMFG_SeqNo");
            //ArrayList<MVAMFGMWorkOrderComponent> wocs = new ArrayList<MVAMFGMWorkOrderComponent>();
            List<MVAMFGMWorkOrderComponent> wocs = new List<MVAMFGMWorkOrderComponent>();
            foreach (MVAMFGMWorkOrderOperation woo in woos)
            {
                if (woo.IsVAMFG_IsOptional())
                    continue;
                foreach (MVAMFGMWorkOrderComponent woc in MVAMFGMWorkOrderComponent.GetOfWorkOrderOperation(woo, null, null))
                {
                    wocs.Add(woc);
                    if (woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_AssemblyPull) ||

    woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_OperationPull))
                        count++;
                }
            }
            //	Even if opTo is optional, process it since it is explicitly passed
            if (woos[woos.Length - 1].IsVAMFG_IsOptional())
                foreach (MVAMFGMWorkOrderComponent woc in MVAMFGMWorkOrderComponent.GetOfWorkOrderOperation(woos[woos.Length - 1], " VAMFG_QtyRequired != 0 ", null))
                {
                    wocs.Add(woc);
                    if (woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_AssemblyPull) ||

    woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_OperationPull))
                        count++;
                }
            //	Even if opFrom is optional, process it since it is explicitly passed
            if (woos[0].IsVAMFG_IsOptional())
                foreach (MVAMFGMWorkOrderComponent woc in MVAMFGMWorkOrderComponent.GetOfWorkOrderOperation(woos[0], " VAMFG_QtyRequired != 0 ", null))
                {
                    wocs.Add(woc);
                    if (woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_AssemblyPull) ||

    woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_OperationPull))
                        count++;
                }

            int depLineNo = 10;

            //	If components were found then call MWorkOrderTxnUtil to generate apt txn lines
            //	Additionally do the corresponding depletion lines also
            //	Deplete the issued components from the WO by increasing QtySpent
            if (count > 0)
            {
                MWorkOrderTxnUtil txnUtil = new MWorkOrderTxnUtil(true);	//	saving the generated header & lines
                ViennaAdvantage.CMFG.Model.MVAMFGMWrkOdrTransaction woComponentIssue = txnUtil.createWOTxn(GetCtx(), GetVAMFG_M_WorkOrder_ID(), VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder,
                     GetVAMFG_M_WrkOdrTransaction_ID(), GetM_Locator_ID(), GetVAMFG_QtyEntered(), Get_TrxName());

                if (woComponentIssue == null)
                {
                    VAdvantage.Model.ValueNamePair pp = VLogger.RetrieveError();
                    m_processMsg = pp.GetName();
                    log.Severe("Error " + m_processMsg + ", Could not create component issue transaction for pull components of Work Order - " + GetVAMFG_M_WorkOrder_ID());
                    return false;
                }

                // set the txnDate & acctDate from the parent
                woComponentIssue.SetVAMFG_DateTrx(GetVAMFG_DateTrx().Value.ToLocalTime());
                woComponentIssue.SetVAMFG_DateAcct(GetVAMFG_DateAcct());

                txnUtil.GenerateComponentTxnLine(GetCtx(), woComponentIssue.GetVAMFG_M_WrkOdrTransaction_ID(),
                        woComponentIssue.GetVAMFG_QtyEntered(), new Decimal(opFrom), new Decimal(opTo),
                        X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_OperationPull, Get_TrxName());

                // Process and complete the created component issue.
                if (!DocumentEngine.ProcessIt(woComponentIssue, DocActionConstants.ACTION_Complete))
                {
                    m_processMsg = woComponentIssue.GetProcessMsg();
                    log.Severe("Could not complete component issue : " + m_processMsg + ", for Work Order - " + GetVAMFG_M_WorkOrder_ID());
                    return false;
                }
                if (!woComponentIssue.Save(Get_TrxName()))
                {
                    m_processMsg = "Could not save component issue transaction lines for Work Order - " + GetVAMFG_M_WorkOrder_ID();
                    return false;
                }
            }

            //	Generate deplete lines for all Work Order Components
            foreach (MVAMFGMWorkOrderComponent woc in wocs)
            {
                MVAMFGMWrkOdrTrnsctionLine compDepleteLine = new MVAMFGMWrkOdrTrnsctionLine(GetCtx(), 0, Get_TrxName());
                Decimal reqQty = Decimal.Multiply(woc.GetVAMFG_QtyRequired(), (GetVAMFG_QtyEntered()));
                if (_countGOM01 > 0)
                {
                    reqQty = Decimal.Multiply(woc.GetVAMFG_QtyRequired(), (GetGOM01_ActualQuantity()));
                    compDepleteLine.SetGOM01_ActualQuantity(Decimal.Round(Decimal.Negate(reqQty), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID())));
                }
                compDepleteLine.SetClientOrg(this);
                //	don't generate depletion line if quantity is equal to zero
                if (Decimal.Round((reqQty), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), woc.GetC_UOM_ID())).CompareTo(Decimal.Zero) <= 0)
                    continue;
                compDepleteLine.SetRequiredColumns(GetVAMFG_M_WrkOdrTransaction_ID(), woc.GetM_Product_ID(), woc.GetM_AttributeSetInstance_ID(),
                        woc.GetC_UOM_ID(), Decimal.Negate(reqQty), woc.GetVAMFG_M_WorkOrderOperation_ID(), X_VAMFG_M_WorkOrderComponent.VAMFG_BASISTYPE_PerItem);

                // deplete lines occur in the work order itself and do not need an inventory locator
                if (woc.GetM_AttributeSetInstance_ID() > 0)
                    compDepleteLine.SetM_AttributeSetInstance_ID(woc.GetM_AttributeSetInstance_ID());
                compDepleteLine.SetVAMFG_Line(depLineNo);
                //	message to indicate that enough push components have not been issued to the work order
                if (X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_Push.Equals(woc.GetVAMFG_SupplyType()) && woc.GetVAMFG_QtyAvailable().CompareTo(reqQty) < 0)
                    infoLog.Append("\nEnough " + VAdvantage.Model.MProduct.Get(GetCtx(), woc.GetM_Product_ID())
                            + " have not been issued to the work order - " + wo.GetVAMFG_M_WorkOrder_ID()
                            + ". Issued: " + woc.GetVAMFG_QtyAvailable() + " Required: " + reqQty);

                depLineNo += 10;
                if (!compDepleteLine.Save(Get_TrxName()))
                {
                    m_processMsg = "Could not save deplete lines for Work Order Move";
                    return false;
                }

                Decimal additionalQty = Decimal.Multiply(woc.GetVAMFG_QtyRequired(), (GetVAMFG_QtyEntered()));
                if (_countGOM01 > 0)
                {
                    additionalQty = Decimal.Multiply(woc.GetVAMFG_QtyRequired(), (GetGOM01_ActualQuantity()));
                }
                if (additionalQty.CompareTo(Decimal.Zero) == 0)
                    if (woc.GetM_Product_ID() == wo.GetM_Product_ID())
                        additionalQty = GetVAMFG_QtyEntered();

                //woc.setQtySpent(woc.GetVAMFG_QtySpent().add(additionalQty).setScale(MUOM.getPrecision(getCtx(),woc.getC_UOM_ID()), BigDecimal.ROUND_HALF_UP));
                woc.SetVAMFG_QtySpent(Decimal.Add((woc.GetVAMFG_QtySpent()), Decimal.Round((additionalQty), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), woc.GetC_UOM_ID()))));
                if (!woc.Save(Get_TrxName()))
                {
                    m_processMsg = "Could not update Component Usage";
                    return false;
                }

                // Set the correct DateActualFrom/DateActualTo for the operations
                MVAMFGMWorkOrderOperation woo = new MVAMFGMWorkOrderOperation(GetCtx(), woc.GetVAMFG_M_WorkOrderOperation_ID(), Get_TrxName());
                //Timestamp trxDate = GetVAMFG_DateTrx();
                DateTime? trxDate = GetVAMFG_DateTrx().Value.ToLocalTime();
                //Added by Amit - 30-9-2016
                if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress) && !IsVAMFG_WOComplete() && _countGOM01 > 0)
                {
                    // no need to update from date and to date
                    //if (woo.GetVAMFG_DateActualFrom() == null)
                    //    woo.SetVAMFG_DateActualFrom(GetGOM01_OperationStartDate());
                    //if (woo.GetVAMFG_DateActualTo() == null)
                    //    woo.SetVAMFG_DateActualTo(GetGOM01_OperationEndDate());
                }
                else
                {
                    if (trxDate == null)
                        //trxDate = new Timestamp(System.currentTimeMillis());
                        trxDate = DateTime.Now.Date;
                    if (woo.GetVAMFG_DateActualFrom() == null || woo.GetVAMFG_DateActualFrom().Value.ToLocalTime() > (trxDate))
                        woo.SetVAMFG_DateActualFrom(trxDate);
                    if (woo.GetVAMFG_DateActualTo() == null || woo.GetVAMFG_DateActualTo().Value.ToLocalTime() < (trxDate))
                        woo.SetVAMFG_DateActualTo(trxDate);
                }
                if (!woo.Save(Get_TrxName()))
                {
                    m_processMsg = "Could not update dates.";
                    return false;
                }
            }

            m_nextLineNo = depLineNo;	//	In case of assembly also getting generated

            return true;
        } // generateMoveComponentIssueTxn

        private Boolean UpdateAssemblyQty(Decimal qty, Boolean updateAvailable, Boolean updateAssembled)
        {
            MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            if (updateAvailable)
            {
                //wo.SetVAMFG_QtyAvailable(wo.GetVAMFG_QtyAvailable().add(qty).setScale(MUOM.GetPrecision(GetCtx(),wo.GetC_UOM_ID())));
                wo.SetVAMFG_QtyAvailable(Decimal.Round(Decimal.Add(wo.GetVAMFG_QtyAvailable(), qty), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), wo.GetC_UOM_ID())));
                if (wo.GetVAMFG_QtyAvailable().CompareTo(Decimal.Zero) < 0)
                {
                    log.SaveError("Error", "Could not update Quantity Available on Work Order - " +

    GetVAMFG_M_WorkOrder_ID() + "; Quantity Available must be greater than zero.");
                    return false;
                }
            }

            //if(updateAssembled)
            //{wo.SetVAMFG_QtyAssembled(wo.GetVAMFG_QtyAssembled().add(qty).setScale(MUOM.GetPrecision(GetCtx(),wo.GetC_UOM_ID()),MidpointRounding.AwayFromZero));

            if (updateAssembled)
            {
                wo.SetVAMFG_QtyAssembled(Decimal.Round(Decimal.Add(wo.GetVAMFG_QtyAssembled(), qty), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), wo.GetC_UOM_ID())));
                // wo.SetVAMFG_QtyAssembled(wo.GetVAMFG_QtyAssembled().add(qty).setScale(MUOM.GetPrecision(GetCtx(), wo.GetC_UOM_ID())));
            }
            if (!wo.Save(Get_TrxName()))
            {
                log.SaveError("NotSaved", "Could not save Work Order");
                return false;
            }
            return true;

        }

        /// <summary>
        ///  makeAssemblyCreationLine creates the assembly creation line for this work order transaction
        ///   </summary>
        ///   <param name="assemblyID">assemblyID</param>
        ///   <param name="uomID">uomID</param>
        ///   <param name="operationID">operationID</param>
        /// <returns>true if success</returns>
        private Boolean MakeAssemblyCreationLine(int assemblyID, int AttributeSet_ID, int uomID, int operationID)
        {
            MVAMFGMWrkOdrTrnsctionLine mAssyCreationLine = new MVAMFGMWrkOdrTrnsctionLine(GetCtx(), 0, Get_TrxName());
            mAssyCreationLine.SetClientOrg(this);
            // set Attributeset Instance ID also 
            // added by vivek on 16/12/2017 assigned by pradeep
            mAssyCreationLine.SetRequiredColumns(GetVAMFG_M_WrkOdrTransaction_ID(), assemblyID, AttributeSet_ID, uomID, GetVAMFG_QtyEntered(), operationID, X_VAMFG_M_WorkOrderComponent.VAMFG_BASISTYPE_PerItem);
            // assembly creation line occurs in the work order itself and does not need an inventory locator
            mAssyCreationLine.SetVAMFG_Line(m_nextLineNo);
            m_nextLineNo += 10;
            mAssyCreationLine.Save(Get_TrxName());
            decimal qty = 0;
            if (_countGOM01 > 0)
            {
                mAssyCreationLine.SetGOM01_Quantity(GetGOM01_Quantity());
                mAssyCreationLine.SetGOM01_ActualQuantity(GetGOM01_ActualQuantity());
                qty = GetGOM01_ActualLiter();
            }
            else
            {
                qty = GetVAMFG_QtyEntered();
            }
            if (!UpdateAssemblyQty(qty, true, true))
            {
                log.SaveError("NotUpdated", "Could not update assembly quantity");
                return false;
            }
            return true;
        } // generateAssemblyCreationLine


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyID">assemblyID</param>
        /// <param name="uomID">uomID</param>
        /// <param name="operationID">operationID</param>
        /// <returns> true if successfully generated Assembly Issue Transaction</returns>
        private Boolean GenerateAssemblyIssueTxn(int assemblyID, int AttributeSet_ID, int uomID, int operationID)
        {
            // Generate WO transaction assembly completion/delivery
            MVAMFGMWrkOdrTransaction woAssyIssue = new MVAMFGMWrkOdrTransaction(GetCtx(), 0, Get_TrxName());
            woAssyIssue.SetClientOrg(this);
            woAssyIssue.SetC_DocType_ID(GetC_DocType_ID());
            woAssyIssue.SetRequiredColumns(GetVAMFG_M_WorkOrder_ID(), GetM_Locator_ID(), VAMFG_WOTXNSOURCE_Generated,

    VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore);
            woAssyIssue.SetParentWorkOrderTxn_ID(GetVAMFG_M_WrkOdrTransaction_ID());
            //woAssyIssue.SetVAMFG_QtyEntered(GetVAMFG_QtyEntered().setScale(MUOM.GetPrecision(GetCtx(), GetC_UOM_ID())));
            woAssyIssue.SetVAMFG_QtyEntered(Decimal.Round(GetVAMFG_QtyEntered(), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID())));
            if (_countGOM01 > 0)
            {
                woAssyIssue.SetGOM01_Density(GetGOM01_Density());
                woAssyIssue.SetGOM01_Quantity(GetGOM01_Quantity());
                woAssyIssue.SetGOM01_ActualQuantity(GetGOM01_ActualQuantity());
                woAssyIssue.SetGOM01_ActualDensity(GetGOM01_ActualDensity());
                woAssyIssue.SetGOM01_ActualLiter(GetGOM01_ActualLiter());
            }
            // set the txnDate & acctDate from the parent
            woAssyIssue.SetVAMFG_DateTrx(GetVAMFG_DateTrx().Value.ToLocalTime());
            woAssyIssue.SetVAMFG_DateAcct(GetVAMFG_DateAcct());
            woAssyIssue.Save(Get_TrxName());

            // *************** Changed
            // Operation From not found in new Step
            woAssyIssue.SetOperationFrom_ID(operationID);

            DocumentEngine.ProcessIt(woAssyIssue, DocActionConstants.ACTION_Complete);
            String errMsg = woAssyIssue.GetDocStatus();
            if (errMsg.Equals(DocActionConstants.STATUS_Invalid))
            {
                return false;
            }
            if (woAssyIssue.Save(Get_TrxName()))
            {
                //VAdvantage.Classes.ShowMessage.Info("AssemblyCompletionToInventory Transaction No: " + woAssyIssue.GetDocumentNo(), null, "Before Complete create Attributes", "");
                return true;
            }
            return false;
        } // generateAssemblyIssueTxn

        private void GenerateAssemblyTxnLine(int woTxnID, int assemblyID, int AttributeSet_ID, int uomID, Decimal qty, int locator)
        {
            MVAMFGMWrkOdrTransaction woAssyIssue = new MVAMFGMWrkOdrTransaction(GetCtx(), woTxnID, Get_TrxName());
            MVAMFGMWrkOdrTrnsctionLine assyDeliveryLine = new MVAMFGMWrkOdrTrnsctionLine(GetCtx(), 0, Get_TrxName());
            assyDeliveryLine.SetClientOrg(woAssyIssue);
            //	Implicit assumption : OperationFrom is populated with the operation to which the assembly is being issued.
            // set Attributeset Instance ID also 
            // added by vivek on 16/12/2017 assigned by pradeep
            assyDeliveryLine.SetRequiredColumns(woTxnID, assemblyID, AttributeSet_ID, uomID, qty, woAssyIssue.GetOperationFrom_ID(), X_VAMFG_M_WorkOrderComponent.VAMFG_BASISTYPE_PerItem);

            ((X_VAMFG_M_WrkOdrTrnsctionLine)assyDeliveryLine).SetM_Locator_ID(locator);
            assyDeliveryLine.SetVAMFG_Line(10);
            if (_countGOM01 > 0)
            {
                assyDeliveryLine.SetVAMFG_QtyEntered(GetGOM01_ActualLiter());
                assyDeliveryLine.SetGOM01_ActualQuantity(woAssyIssue.GetGOM01_ActualQuantity());
            }
            assyDeliveryLine.Save(Get_TrxName());
        }

        /// <summary>
        /// Get Lines of Work Order Transaction
        /// </summary>
        /// <param name="whereClause">whereClause where clause or null (starting with AND)</param>
        ///  <param name="orderClause">orderClause order clause</param>
        /// <returns>array of lines</returns>
        public MVAMFGMWrkOdrTrnsctionLine[] GetLines(String whereClause, String orderClause)
        {
            //ArrayList<MVAMFGMWrkOdrTrnsctionLine> list = new ArrayList<MVAMFGMWrkOdrTrnsctionLine> ();
            List<MVAMFGMWrkOdrTrnsctionLine> list = new List<MVAMFGMWrkOdrTrnsctionLine>();
            StringBuilder sql = new StringBuilder("SELECT * FROM VAMFG_M_WrkOdrTrnsctionLine WHERE VAMFG_M_WrkOdrTransaction_ID=@param1 ");

            if (whereClause != null)
                sql.Append(whereClause);
            if (orderClause != null)
                sql.Append(" ").Append(orderClause);
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            //PreparedStatement pstmt = DB.prepareStatement(sql.toString(), Get_TrxName());
            //ResultSet rs = null;
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", GetVAMFG_M_WrkOdrTransaction_ID());
                idr = DB.ExecuteReader(sql.ToString(), param, Get_TrxName());
                dt.Load(idr);
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MVAMFGMWrkOdrTrnsctionLine ol = new MVAMFGMWrkOdrTrnsctionLine(GetCtx(), dt.Rows[i], Get_TrxName());
                    list.Add(ol);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                log.Log(Level.SEVERE, sql.ToString(), e);
            }

            //
            MVAMFGMWrkOdrTrnsctionLine[] lines = new MVAMFGMWrkOdrTrnsctionLine[list.Count];
            lines = list.ToArray();
            return lines;
        }	//	getLines

        //DataSet countAttribute = null;
        public String CompleteIt()
        {
            _countGOM01 = Convert.ToInt32(DB.ExecuteScalar("SELECT COUNT(AD_ModuleInfo_ID) FROM AD_ModuleInfo WHERE Prefix like 'GOM01_'"));

            DataSet countAttribute = null;
            Trx trx = null;
            trx = Trx.Get(Get_TrxName().GetTrxName(), true);
            // If we put it in a process, we need to revisit the auto processing of the inventory transaction.
            // That cannot happen until we do complete this. Instead, we should only save the inventory transaction.
            // save first so that we have an ID to put in the automatically created lines
            Save(Get_TrxName());

            String woTxnType = GetVAMFG_WorkOrderTxnType();

            if (m_processMsg == null)
                m_processMsg = "";

            MVAMFGMWorkOrder workOrder = new MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());

            int assemblyID = workOrder.GetM_Product_ID();
            int uomID = workOrder.GetC_UOM_ID();
            int AttributeSet_ID = workOrder.GetM_AttributeSetInstance_ID();

            String sql;
            VAdvantage.Model.MRole role = VAdvantage.Model.MRole.GetDefault(GetCtx(), false);

            // This does the pulls, depletes, and assembly creation if necessary
            if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress))
            {
                if (GetVAMFG_QtyEntered().CompareTo(Decimal.Zero) == 0)
                {
                    m_processMsg = "you must enter a nonzero quantity for a move transaction.";

                    //Get_TrxName().rollback();
                    trx.Rollback();
                    return DocActionConstants.STATUS_Invalid;
                }

                //ArrayList<Integer> operations = new ArrayList<Integer>();
                List<int> operations = new List<int>();

                StringBuilder sqlBuf = new StringBuilder("SELECT woo.VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation woo WHERE woo.VAMFG_M_WorkOrder_ID = @param1");

                String stepFrom = GetVAMFG_StepFrom();
                if (stepFrom.Equals(VAMFG_STEPFROM_Scrap)
                    )
                {
                    m_processMsg = "cannot initiate move transaction from scrap step";
                    //Get_TrxName().rollback();
                    trx.Rollback();
                    return DocActionConstants.STATUS_Invalid;
                }
                String stepTo = GetVAMFG_StepTo();
                MVAMFGMWorkOrderOperation fromOp = new MVAMFGMWorkOrderOperation(GetCtx(), GetOperationFrom_ID(), Get_TrxName());
                MVAMFGMWorkOrderOperation toOp = new MVAMFGMWorkOrderOperation(GetCtx(), GetOperationTo_ID(), Get_TrxName());

                //	If the stepFrom is "To Move" then the required components/resources have been issued/charged
                if (stepFrom.Equals(VAMFG_STEPFROM_Finish)) // To Move
                {
                    sqlBuf.Append(" AND VAMFG_SeqNo > @param2 ");
                }
                else
                {
                    sqlBuf.Append(" AND VAMFG_SeqNo >= @param2 ");
                }

                // If the stepTo is "To Move" - generate component/resources issue/charge
                // If the stepTo is "Scrap" and the stepFrom is not "To Move" - generate component/resources issue/charge
                if (fromOp.GetVAMFG_SeqNo() < toOp.GetVAMFG_SeqNo())
                {
                    if (stepTo.Equals(VAMFG_STEPTO_Finish))
                    {
                        sqlBuf.Append(" AND VAMFG_SeqNo <= @param3 ");
                    }
                    else
                    {
                        sqlBuf.Append(" AND VAMFG_SeqNo < @param3 ");
                    }
                }
                else if (fromOp.GetVAMFG_SeqNo() == toOp.GetVAMFG_SeqNo())
                {
                    if ((stepTo.Equals(VAMFG_STEPTO_Scrap) && (stepFrom.Equals(VAMFG_STEPFROM_Waiting) ||
    stepFrom.Equals(VAMFG_STEPFROM_Finish))) || stepTo.Equals(VAMFG_STEPTO_Process))
                    {
                        sqlBuf.Append(" AND VAMFG_SeqNo < @param3 ");
                    }
                    else
                    {
                        sqlBuf.Append(" AND VAMFG_SeqNo <= @param3 ");
                    }
                }
                sqlBuf.Append(" AND (VAMFG_IsOptional <> 'Y' OR VAMFG_SeqNo = @param4 OR VAMFG_SeqNo = @param5) ORDER BY VAMFG_SeqNo ");

                sql = sqlBuf.ToString();
                sql = role.AddAccessSQL(sql, "VAMFG_M_WorkOrderOperation", VAdvantage.Model.MRole.SQL_NOTQUALIFIED, VAdvantage.Model.MRole.SQL_RO);
                //PreparedStatement pstmt = DB.prepareStatement(sql, Get_TrxName());
                //ResultSet rs = null;
                SqlParameter[] param = null;
                IDataReader idr = null;
                DataTable dt = new DataTable();
                try
                {

                    param = new SqlParameter[5];
                    param[0] = new SqlParameter("@param1", GetVAMFG_M_WorkOrder_ID());
                    param[1] = new SqlParameter("@param2", fromOp.GetVAMFG_SeqNo());
                    param[2] = new SqlParameter("@param3", toOp.GetVAMFG_SeqNo());
                    param[3] = new SqlParameter("@param4", fromOp.GetVAMFG_SeqNo());
                    param[4] = new SqlParameter("@param5", toOp.GetVAMFG_SeqNo());
                    idr = DB.ExecuteReader(sql, param, Get_TrxName());
                    while (idr.Read())
                    {
                        operations.Add(VAdvantage.Utility.Util.GetValueOfInt(idr[0]));
                    }
                }
                catch
                {
                    log.Log(Level.SEVERE, sql);
                }
                finally
                {
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                }

                MVAMFGMWorkOrderOperation firstOp = null, lastOp = null;
                if (operations.Count > 0)
                {
                    firstOp = new MVAMFGMWorkOrderOperation(GetCtx(), operations[0], Get_TrxName());
                    lastOp = new MVAMFGMWorkOrderOperation(GetCtx(), operations[operations.Count - 1], Get_TrxName());
                }

                int lastMandatoryOp = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation " +
                        "WHERE VAMFG_M_WorkOrder_ID = @param1 AND VAMFG_IsOptional <> 'Y' ORDER BY VAMFG_SeqNo DESC", GetVAMFG_M_WorkOrder_ID());


                // Move the sub-assembly quantities from "OperationFrom/StepFrom" to "OperationTo/StepTo"
                if (stepFrom.Equals(VAMFG_STEPFROM_Waiting))
                {
                    //change By Amit - 03-Oct-2016
                    if (!IsVAMFG_WOComplete() && _countGOM01 > 0)
                    {
                        if (fromOp.GetVAMFG_DateActualFrom() == null)
                            fromOp.SetVAMFG_DateActualFrom(GetGOM01_OperationStartDate());
                    }
                    //end
                    if (_countGOM01 == 0)
                    {
                        fromOp.SetVAMFG_QtyQueued(Decimal.Subtract(fromOp.GetVAMFG_QtyQueued(), (GetVAMFG_QtyEntered())));
                        if (fromOp.GetVAMFG_QtyQueued().CompareTo(Decimal.Zero) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    else
                    {
                        fromOp.SetVAMFG_QtyQueued(Decimal.Subtract(fromOp.GetVAMFG_QtyQueued(), (GetGOM01_ActualLiter())));
                    }
                }
                else if (stepFrom.Equals(VAMFG_STEPFROM_Process))
                {
                    if (_countGOM01 == 0)
                    {
                        fromOp.SetVAMFG_QtyRun(Decimal.Subtract(fromOp.GetVAMFG_QtyRun(), (GetVAMFG_QtyEntered())));
                        if (fromOp.GetVAMFG_QtyRun().CompareTo(Decimal.Zero) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    else
                    {
                        fromOp.SetVAMFG_QtyRun(Decimal.Subtract(fromOp.GetVAMFG_QtyRun(), (GetGOM01_ActualLiter())));
                    }
                }
                else if (stepFrom.Equals(VAMFG_STEPFROM_Finish))
                {
                    if (_countGOM01 == 0)
                    {
                        fromOp.SetVAMFG_QtyAssembled(Decimal.Subtract(fromOp.GetVAMFG_QtyAssembled(), (GetVAMFG_QtyEntered())));
                        if (fromOp.GetVAMFG_QtyAssembled().CompareTo(Decimal.Zero) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    else
                    {
                        fromOp.SetVAMFG_QtyAssembled(Decimal.Subtract(fromOp.GetVAMFG_QtyAssembled(), (GetGOM01_ActualLiter())));
                    }
                }
                fromOp.Save(Get_TrxName());

                if (stepTo.Equals(VAMFG_STEPTO_Waiting)) //Queue
                {
                    if (_countGOM01 == 0)
                    {
                        toOp.SetVAMFG_QtyQueued(Decimal.Add(toOp.GetVAMFG_QtyQueued(), (GetVAMFG_QtyEntered())));
                        if (toOp.GetVAMFG_QtyQueued().CompareTo(Decimal.Zero) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    else
                    {
                        toOp.SetVAMFG_QtyQueued(Decimal.Add(toOp.GetVAMFG_QtyQueued(), (GetGOM01_ActualLiter())));
                    }
                }
                else if (stepTo.Equals(VAMFG_STEPTO_Process))
                {
                    if (_countGOM01 == 0)
                    {
                        toOp.SetVAMFG_QtyRun(Decimal.Add(toOp.GetVAMFG_QtyRun(), (GetVAMFG_QtyEntered())));
                        if (toOp.GetVAMFG_QtyRun().CompareTo(Decimal.Zero) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            //Get_TrxName().rollback();
                            trx.Rollback();

                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    else
                    {
                        toOp.SetVAMFG_QtyRun(Decimal.Add(toOp.GetVAMFG_QtyRun(), (GetGOM01_ActualLiter())));
                    }
                }
                else if (stepTo.Equals(VAMFG_STEPTO_Finish))
                {
                    //Added by Amit - 10-Oct-2016
                    if (!IsVAMFG_WOComplete() && _countGOM01 > 0)
                    {
                        if (toOp.GetVAMFG_DateActualTo() == null)
                            toOp.SetVAMFG_DateActualTo(GetGOM01_OperationEndDate());
                    }
                    //end
                    if (_countGOM01 == 0)
                    {
                        toOp.SetVAMFG_QtyAssembled(Decimal.Add(toOp.GetVAMFG_QtyAssembled(), (GetVAMFG_QtyEntered())));
                        if (toOp.GetVAMFG_QtyAssembled().CompareTo(Decimal.Zero) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    else
                    {
                        toOp.SetVAMFG_QtyAssembled(Decimal.Add(toOp.GetVAMFG_QtyAssembled(), (GetGOM01_ActualLiter())));
                    }
                }
                else if (stepTo.Equals(VAMFG_STEPTO_Scrap))
                {
                    if (_countGOM01 == 0)
                    {
                        toOp.SetVAMFG_QtyScrapped(Decimal.Add(toOp.GetVAMFG_QtyScrapped(), (GetVAMFG_QtyEntered())));
                        if (toOp.GetVAMFG_QtyScrapped().CompareTo(Decimal.Zero) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                        workOrder.SetVAMFG_QtyScrapped(Decimal.Add(workOrder.GetVAMFG_QtyScrapped(), (GetVAMFG_QtyEntered())));
                        if (stepFrom.Equals(VAMFG_STEPFROM_Finish) && toOp.GetVAMFG_M_WorkOrderOperation_ID() == lastMandatoryOp)
                            UpdateAssemblyQty(Decimal.Negate(GetVAMFG_QtyEntered()), true, true);
                    }
                    else
                    {
                        toOp.SetVAMFG_QtyScrapped(Decimal.Add(toOp.GetVAMFG_QtyScrapped(), (GetGOM01_ActualLiter())));
                        workOrder.SetVAMFG_QtyScrapped(Decimal.Add(workOrder.GetVAMFG_QtyScrapped(), (GetGOM01_ActualLiter())));
                        if (stepFrom.Equals(VAMFG_STEPFROM_Finish) && toOp.GetVAMFG_M_WorkOrderOperation_ID() == lastMandatoryOp)
                            UpdateAssemblyQty(Decimal.Negate(GetGOM01_ActualLiter()), true, true);
                    }
                    workOrder.Save(Get_TrxName());
                }
                toOp.Save(Get_TrxName());

                if (!IsReversal())
                {
                    if (operations.Count > 0)
                    {
                        if (GenerateMoveComponentIssueTxn(firstOp.GetVAMFG_SeqNo(), lastOp.GetVAMFG_SeqNo()))
                        {
                            ///// Test Here This Logic
                            //if (!GenerateMoveResourceUsageTxn(firstOp.GetVAMFG_SeqNo(), lastOp.GetVAMFG_SeqNo()))
                            //{
                            //    //Get_TrxName().rollback();
                            //    trx.Rollback();
                            //    return DocActionConstants.STATUS_Invalid;
                            //}


                            // if the "operation to" is the last mandatory operation in the WorkOrder and "step to" is To Move
                            // we should generate the assembly creation line
                            if (lastMandatoryOp <= GetOperationTo_ID() && stepTo.Equals(VAMFG_STEPTO_Finish))
                            {
                                // set Attributeset Instance ID 
                                // added by vivek on 16/12/2017 assigned by pradeep
                                if (!MakeAssemblyCreationLine(assemblyID, AttributeSet_ID, uomID, GetOperationTo_ID()))
                                {
                                    log.Severe("Could not generate assembly line for creating final assembly of Work Order - " + GetVAMFG_M_WorkOrder_ID());
                                    //Get_TrxName().rollback();
                                    trx.Rollback();
                                    return DocActionConstants.STATUS_Invalid;
                                }

                                // if "Complete this Assembly" is checked we should generate the assembly issue transaction
                                if (IsVAMFG_WOComplete())
                                {
                                    // set Attributeset Instance ID 
                                    // added by vivek on 16/12/2017 assigned by pradeep
                                    if (!GenerateAssemblyIssueTxn(assemblyID, AttributeSet_ID, uomID, GetOperationTo_ID()))
                                    {
                                        m_processMsg = "Could not complete Assembly Completion to Inventory Transaction for Work Order - " + GetVAMFG_M_WorkOrder_ID();
                                        log.Severe(m_processMsg);
                                        //Get_TrxName().rollback();
                                        trx.Rollback();
                                        return DocActionConstants.STATUS_Invalid;
                                    }

                                    //New check to updateAvailable qantity
                                    if (workOrder.GetVAMFG_QtyAvailable() != 0)
                                    {
                                        workOrder.SetVAMFG_QtyAvailable(Decimal.Subtract(workOrder.GetVAMFG_QtyAvailable(), GetVAMFG_QtyEntered()));
                                    }
                                }
                            }
                        }
                        else
                        {
                            // generateMove failed
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }

                        if (!GenerateMoveResourceUsageTxn(firstOp.GetVAMFG_SeqNo(), lastOp.GetVAMFG_SeqNo()))
                        {
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }

                }
                else
                {
                    // This is a reversal for a MOVE type transaction.
                    // We need to reverse the depletion lines, the completion line, and the VAMFG_qtyassembled for each operation here,
                    // which is usually done in generateMoveComponentIssueTxn, makeAssemblyCreationLine,and generateAssemblyIssueTxn

                    // For a final operation move:
                    // Disallow the reversal if there is insufficient assembled quantity in the work order to back out.
                    // This can happen if the user has done a manual move (no complete checkbox selected) and separate
                    // manual assembly issue, and then tried to reverse just the move.

                    if (_countGOM01 > 0)
                    {
                        if (lastMandatoryOp == GetOperationTo_ID() && VAMFG_STEPTO_Finish.Equals(stepTo) && workOrder.GetVAMFG_QtyAvailable().CompareTo(Math.Abs(GetGOM01_ActualLiter())) < 0)
                        {
                            m_processMsg = "Not enough assembly quantity available in the work order to reverse.";
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    else
                    {
                        if (lastMandatoryOp == GetOperationTo_ID() && VAMFG_STEPTO_Finish.Equals(stepTo) && workOrder.GetVAMFG_QtyAvailable().CompareTo(Math.Abs(GetVAMFG_QtyEntered())) < 0)
                        //workOrder.GetVAMFG_QtyAvailable().CompareTo(Decimal.Ceiling( GetVAMFG_QtyEntered())) < 0)
                        {
                            m_processMsg = "Not enough assembly quantity available in the work order to reverse.";
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }

                    // Reverse the depletion of component qty
                    // We use QtySpent + QtyEntered from the woTxnLine because the depletion line on a move is negative
                    // Need to copy ASI from existing parent transaction
                    MVAMFGMWrkOdrTrnsctionLine[] wotLines = GetLines(null, "ORDER BY M_Product_ID, M_Locator_ID, M_AttributeSetInstance_ID ");
                    foreach (MVAMFGMWrkOdrTrnsctionLine wotl in wotLines)
                    {
                        MVAMFGMWorkOrderOperation woo = new MVAMFGMWorkOrderOperation(GetCtx(), wotl.GetVAMFG_M_WorkOrderOperation_ID(), Get_TrxName());
                        MVAMFGMWorkOrderComponent[] woc = MVAMFGMWorkOrderComponent.GetOfWorkOrderOperation(woo, " M_Product_ID = " + wotl.GetM_Product_ID(), null);
                        if (woc != null && woc.Length > 0)
                        {
                            //woc[0].SetQtySpent(woc[0].GetVAMFG_QtySpent().subtract(wotl.GetVAMFG_QtyEntered()).setScale(MUOM.GetPrecision(GetCtx(), woc[0].GetC_UOM_ID())));
                            if (_countGOM01 > 0)
                            {
                                woc[0].SetVAMFG_QtySpent(Decimal.Subtract(woc[0].GetVAMFG_QtySpent(), Decimal.Round((wotl.GetGOM01_ActualQuantity()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), woc[0].GetC_UOM_ID()))));
                            }
                            else
                            {
                                woc[0].SetVAMFG_QtySpent(Decimal.Subtract(woc[0].GetVAMFG_QtySpent(), Decimal.Round((wotl.GetVAMFG_QtyEntered()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), woc[0].GetC_UOM_ID()))));
                            }
                            if (!woc[0].Save(Get_TrxName()))
                            {
                                m_processMsg = "Error in reversing component usage";
                                return DocActionConstants.STATUS_Invalid;
                            }
                        }
                    }

                    // also reverse the qty created of the assembly item
                    if (lastMandatoryOp == GetOperationTo_ID() && VAMFG_STEPTO_Finish.Equals(stepTo))
                    {
                        if (_countGOM01 > 0)
                        {
                            UpdateAssemblyQty(GetGOM01_ActualLiter(), true, true);
                        }
                        else
                        {
                            UpdateAssemblyQty(GetVAMFG_QtyEntered(), true, true);
                        }
                    }

                    // if we are reversing a move, then we must not have any Actual To date defined in work order. Unset it
                    workOrder.SetVAMFG_DateActualTo(null);
                    workOrder.Save();
                }

                if (stepFrom.Equals(VAMFG_STEPFROM_Waiting))
                {
                    DateTime? trxnDate = GetVAMFG_DateTrx().Value.ToLocalTime();
                    if (trxnDate == null)
                        //trxDate = new Timestamp(System.currentTimeMillis());
                        trxnDate = DateTime.Now.Date;
                    if (fromOp.GetVAMFG_DateActualFrom() == null || fromOp.GetVAMFG_DateActualFrom().Value.ToLocalTime() > (trxnDate))
                        fromOp.SetVAMFG_DateActualFrom(trxnDate);
                    fromOp.Save(Get_TrxName());
                }

                SetProcessed(true);
            }

            if ((woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory)
                    || woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore)) && !IsReversal())
            {
                /*************************************************************/
                if (_countGOM01 > 0)
                {
                    // update Current cost price for Transfer Assembly to store as well as for Assembly Return form Inventory
                    // calculation process is : 
                    // get Actual qty in KG from Component Transaction Line
                    // get cost from Component line where transaction type is "CI" (Component Issue to Work Order)
                    // then divide the calculated value with Actual Qty in Kg fromProduction Execution Header
                    // after that  we multiple density with  (sum of (qty * cost of each line) / Actual Qty in Kg from Production Execution Header)
                    sql = @"SELECT ROUND( wot.GOM01_ActualDensity * (SUM(wotl.GOM01_ActualQuantity * wotl.CurrentCostPrice) / wot.GOM01_ActualQuantity) , 10) as Currenctcost
                             FROM VAMFG_M_WrkOdrTransaction wot
                             INNER JOIN VAMFG_M_WorkOrder wo ON wo.VAMFG_M_WorkOrder_ID = wot.VAMFG_M_WorkOrder_ID
                             INNER JOIN VAMFG_M_WrkOdrTrnsctionLine wotl ON wot.VAMFG_M_WrkOdrTransaction_ID = wotl.VAMFG_M_WrkOdrTransaction_ID
                           WHERE wotl.IsActive = 'Y' AND wot.VAMFG_M_WorkOrder_ID = " + GetVAMFG_M_WorkOrder_ID() +
                           @" AND wot.VAMFG_WorkOrderTxnType = 'CI' " +
                           " AND wot.GOM01_BatchNo = '" + GetGOM01_BatchNo() + @"' GROUP BY wot.GOM01_ActualQuantity ,  wot.GOM01_ActualDensity";
                    decimal currentcostprice = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, Get_TrxName()));
                    currentcostprice = Decimal.Round(currentcostprice, 10);
                    SetCurrentCostPrice(currentcostprice);
                    if (!Save(Get_TrxName()))
                    {
                        m_processMsg = "Error in saving Work Order Transaction - " + GetVAMFG_M_WrkOdrTransaction_ID();
                        trx.Rollback();
                        return DocActionConstants.STATUS_Invalid;
                    }
                }

                //DataSet countAttribute = null;
                VAdvantage.Model.MProduct product = VAdvantage.Model.MProduct.Get(Env.GetContext(), assemblyID);

                // If AttributeSet is Attached with the Product else execution of the else part 
                // 03-Jan-2012
                // Table ID changed done by Vivek on 21/12/2017 
                sql = "select * from VAMFG_M_WrkOdrTxnLineMA where VAMFG_M_WrkOdrTrnsctionLine_ID= (SELECT VAMFG_M_WrkOdrTrnsctionLine_ID from VAMFG_M_WrkOdrTrnsctionLine tnl where tnl.VAMFG_M_WrkOdrTransaction_ID=" + GetVAMFG_M_WrkOdrTransaction_ID() + ")";
                countAttribute = VAdvantage.DataBase.DB.ExecuteDataset(sql, null, Get_TrxName());
                if (product.GetM_AttributeSet_ID() != 0)
                {
                    int countLines = 1;
                    MAttributeSet ast = new MAttributeSet(GetCtx(), product.GetM_AttributeSet_ID(), Get_TrxName());
                    if (ast.IsSerNo())
                    {
                        countLines = VAdvantage.Utility.Util.GetValueOfInt(GetVAMFG_QtyEntered());
                    }
                    if (countLines != countAttribute.Tables[0].Rows.Count)
                    {
                        log.SaveError("NotMatch" + ":" + "QtyEntered" + ":" + GetVAMFG_QtyEntered() + " & " + "AttributesLinesCount" + ":" + countAttribute, "");
                        // Show msg when there is no data on attribute tab of execution window added by VIvek on 04/01/2018
                        m_processMsg = Msg.GetMsg(GetCtx(), "VAMFG_GenerateAttributes");
                        return DocActionConstants.STATUS_InProgress;
                    }

                    /*************************************************************/
                    m_lines = GetLines(null, "ORDER BY M_Product_ID");
                    if (m_lines.Length == 0)
                    {
                        m_processMsg = "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": no lines to process";
                        //Get_TrxName().rollback();
                        trx.Rollback();
                        return DocActionConstants.STATUS_Invalid;
                    }


                    ////Create new lines in WorkorderTransaction Tab
                    //GenerateAssemblyTxnLine(GetVAMFG_M_WrkOdrTransaction_ID(), assemblyID, uomID,
                    //        GetVAMFG_QtyEntered(), GetM_Locator_ID());
                    #region code commented by Vivek on 09/01/2018 as storage lines were not creating for attributes
                    //loop through the lines, and generate the corresponding inventory component issues.
                    for (int i = 0; i < m_lines.Length; i++)
                    {
                        if (!ProcessInvComponentLine(m_lines[i], woTxnType))
                        {
                            m_processMsg = m_processMsg + "- WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": error processing inv component lines";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    #endregion
                    VAdvantage.Model.MLocator loc = VAdvantage.Model.MLocator.Get(GetCtx(), GetM_Locator_ID());
                    //Update MStorage with M_Locator_id,InstanceAttribute_id,M_product_ID
                    if (!storeQtyUpdate)
                    {
                        // attributes storage updated while completing execution when trx type is component issue to work order
                        for (int i = 0; i < countAttribute.Tables[0].Rows.Count; i++)
                        {
                            int att = 0;
                            if (countAttribute != null)
                            {
                                att = VAdvantage.Utility.Util.GetValueOfInt(countAttribute.Tables[0].Rows[i]["m_attributesetinstance_id"]);
                            }

                            if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
                            {
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                     GetM_Product_ID(), att, att, VAdvantage.Utility.Util.GetValueOfDecimal(countAttribute.Tables[0].Rows[i]["VAMFG_MOVEMENTQTY"]), Decimal.Zero, Decimal.Zero, Get_TrxName());
                            }
                            else if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                            {
                                //     MStorage.AddQtys(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                //GetM_Product_ID(), att, att, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                                //Changed by Pratap
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(), GetM_Product_ID(), att, att, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                            }
                        }
                    }
                }
                else
                {
                    m_lines = GetLines(null, "ORDER BY M_Product_ID");
                    if (m_lines.Length != 0)
                    {
                        foreach (MVAMFGMWrkOdrTrnsctionLine line in m_lines)
                        {
                            if (!line.Delete(true, Get_TrxName()))
                            {
                                m_processMsg = "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": problem deleting existing lines (assembly returns and issues generate their own lines)";
                                //Get_TrxName().rollback();
                                trx.Rollback();
                                return DocActionConstants.STATUS_Invalid;
                            }
                        }
                    }

                    GenerateAssemblyTxnLine(GetVAMFG_M_WrkOdrTransaction_ID(), assemblyID, AttributeSet_ID, uomID, GetVAMFG_QtyEntered(), GetM_Locator_ID());
                    if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                    {
                        // Commented Bcoz no effect was found on updating Store Assembly when Transfer to store 
                        // that's why commented on return too. Under Testing
                        //workOrder.SetVAMFG_QtyAssembled(Decimal.Round(Decimal.Subtract(workOrder.GetVAMFG_QtyAssembled(), GetVAMFG_QtyEntered()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), workOrder.GetC_UOM_ID())));
                    }

                    /*********************************************/
                    m_lines = GetLines(null, "ORDER BY M_Product_ID");
                    for (int i = 0; i < m_lines.Length; i++)
                    {
                        if (!ProcessInvComponentLine(m_lines[i], woTxnType))
                        {
                            m_processMsg = m_processMsg + "- WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": error processing inv component lines";
                            //Get_TrxName().rollback();
                            trx.Rollback();
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                    VAdvantage.Model.MLocator loc = VAdvantage.Model.MLocator.Get(GetCtx(), GetM_Locator_ID());
                    //Update MStorage with M_Locator_id,InstanceAttribute_id,M_product_ID
                    if (!storeQtyUpdate)
                    {
                        for (int i = 0; i < countAttribute.Tables[0].Rows.Count; i++)
                        {
                            int att = 0;
                            if (countAttribute != null)
                            {
                                att = VAdvantage.Utility.Util.GetValueOfInt(countAttribute.Tables[0].Rows[i]["m_attributesetinstance_id"]);
                            }


                            if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
                            {
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                    GetM_Product_ID(), att, att, VAdvantage.Utility.Util.GetValueOfDecimal(countAttribute.Tables[0].Rows[i]["VAMFG_MOVEMENTQTY"]), Decimal.Zero, Decimal.Zero, Get_TrxName());
                            }
                            else if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                            {
                                //       MStorage.AddQtys(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                //GetM_Product_ID(), att, att, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                                //Changed by Pratap
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                      GetM_Product_ID(), att, att, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                            }
                        }
                    }


                    /*********************************************/
                }

                if (_countGOM01 > 0)
                {
                    if (!CalculateFinishedGoodCost(GetCtx(), GetAD_Client_ID(), GetAD_Org_ID(), GetM_Product_ID(), GetM_AttributeSetInstance_ID(),
                        GetVAMFG_M_WorkOrder_ID(), GetGOM01_BatchNo(), GetGOM01_ActualLiter(), Get_TrxName()))
                    {
                        if (string.IsNullOrEmpty(m_processMsg))
                        {
                            m_processMsg = "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": cost of finished good not calculated";
                        }
                        else
                        {
                            m_processMsg += "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": cost of finished good not calculated";
                        }
                        trx.Rollback();
                        return DocActionConstants.STATUS_Invalid;
                    }
                }
            }

            // This is for transactions that need a corresponding inventory side transaction.
            // It will generate those for all existing lines (whether manually entered or generated).
            if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder) ||
                    woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder))
            {
                m_lines = GetLines(null, "ORDER BY VAMFG_Line");

                if (m_lines.Length == 0)
                {
                    m_processMsg = "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": no lines to process";
                    //Get_TrxName().rollback();
                    trx.Rollback();
                    return DocActionConstants.STATUS_Invalid;
                }

                //Added by santosh on 23/05/2019 so system will not create Transactions for Negative Quantity in MTrnsaction
                //for (int i = 0; i < m_lines.Length; i++)
                //{
                //    MVAMFGMWrkOdrTrnsctionLine WtxnLine = new MVAMFGMWrkOdrTrnsctionLine(GetCtx(), Util.GetValueOfInt(m_lines[i].GetVAMFG_M_WrkOdrTrnsctionLine_ID()), Get_TrxName());
                //    string qry = "SELECT currentqty FROM M_Transaction WHERE M_Transaction_ID = (SELECT MAX(M_Transaction_ID) FROM M_Transaction WHERE movementdate = " +
                //    " (SELECT MAX(movementdate) FROM M_Transaction WHERE movementdate <= " + GlobalVariable.TO_DATE(GetVAMFG_DateTrx(), true) + " AND M_Product_ID = " + WtxnLine.GetM_Product_ID() + " AND M_Locator_ID = " + WtxnLine.GetM_Locator_ID() +
                //    " AND M_AttributeSetInstance_ID = " + WtxnLine.GetM_AttributeSetInstance_ID() + ") AND M_Product_ID = " + WtxnLine.GetM_Product_ID() + " AND M_Locator_ID = " + WtxnLine.GetM_Locator_ID() +
                //    " AND M_AttributeSetInstance_ID = " + WtxnLine.GetM_AttributeSetInstance_ID() + ") AND AD_Org_ID = " + WtxnLine.GetAD_Org_ID() + " AND M_Product_ID = " + WtxnLine.GetM_Product_ID() +
                //    " AND M_Locator_ID = " + WtxnLine.GetM_Locator_ID() + " AND M_AttributeSetInstance_ID = " + WtxnLine.GetM_AttributeSetInstance_ID();
                //    decimal CurrentQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(qry));
                //    if (CurrentQty < Util.GetValueOfDecimal(m_lines[i].GetGOM01_ActualQuantity()))
                //    {
                //        ViennaAdvantage.Model.MProduct product = new ViennaAdvantage.Model.MProduct(GetCtx(), WtxnLine.GetM_Product_ID(), Get_TrxName());
                //        m_processMsg = Msg.GetMsg(GetCtx(), "VAMFG_NotEnoughQty") + product.GetName();
                //        trx.Rollback();
                //        return DocActionConstants.STATUS_Invalid;
                //    }
                //}

                for (int i = 0; i < m_lines.Length; i++)
                {
                    MVAMFGMWrkOdrTrnsctionLine WtxnLine = new MVAMFGMWrkOdrTrnsctionLine(GetCtx(), Util.GetValueOfInt(m_lines[i].GetVAMFG_M_WrkOdrTrnsctionLine_ID()), Get_TrxName());
                    VAdvantage.Model.MStorage st = VAdvantage.Model.MStorage.Get(Env.GetCtx(), WtxnLine.GetM_Locator_ID(), WtxnLine.GetM_Product_ID(), WtxnLine.GetM_AttributeSetInstance_ID(), Get_TrxName());
                    if (st == null)
                    {
                        ViennaAdvantage.Model.MProduct product = new ViennaAdvantage.Model.MProduct(GetCtx(), WtxnLine.GetM_Product_ID(), Get_TrxName());
                        m_processMsg = Msg.GetMsg(GetCtx(), "VAMFG_NotEnoughQty") + product.GetName();
                        trx.Rollback();
                        return DocActionConstants.STATUS_Invalid;
                    }

                    Decimal qty = st.GetQtyOnHand();
                    if (qty < Util.GetValueOfDecimal(m_lines[i].GetGOM01_ActualQuantity()))
                    {
                        ViennaAdvantage.Model.MProduct product = new ViennaAdvantage.Model.MProduct(GetCtx(), WtxnLine.GetM_Product_ID(), Get_TrxName());
                        m_processMsg = Msg.GetMsg(GetCtx(), "VAMFG_NotEnoughQty") + product.GetName();
                        trx.Rollback();
                        return DocActionConstants.STATUS_Invalid;
                    }
                }
                //Added by santosh on 23/05/2019 so system will not create Transactions for Negative Quantity in MTrnsaction

                // for checking - costing calculate on completion or not
                // IsCostImmediate = true - calculate cost on completion
                MClient client = MClient.Get(GetCtx(), GetAD_Client_ID());

                VAdvantage.Model.MProduct pro = null;
                string conversionNotFoundProductionExecution = null;
                //string conversionNotFoundProductionExecution1 = null;
                // loop through the lines, and generate the corresponding inventory component issues.
                int qtyCount = 0;
                for (int i = 0; i < m_lines.Length; i++)
                {
                    if (!ProcessInvComponentLine(m_lines[i], woTxnType))
                    {
                        pro = new VAdvantage.Model.MProduct(Env.GetCtx(), m_lines[i].GetM_Product_ID(), Get_TrxName());
                        if (qtyCount == 0)
                        {
                            m_processMsg = Msg.GetMsg(GetCtx(), "VAMFG_NotEnoughQty") + pro.GetName();
                        }
                        else
                        {
                            m_processMsg = m_processMsg + ", " + pro.GetName();
                        }
                        qtyCount++;
                        //Get_TrxName().rollback();
                        //bool ch = trx.Rollback();

                        //return DocActionConstants.STATUS_Invalid;
                    }
                }

                // JID_1131:  if qty not avaialble into warehouse for respective products -- then give message of all component whose qty not avaialble.
                if (!String.IsNullOrEmpty(m_processMsg))
                {
                    bool ch = trx.Rollback();
                    return DocActionConstants.STATUS_Invalid;
                }

                for (int i = 0; i < m_lines.Length; i++)
                {
                    // Calculate costing on completion if cost immediate = true on Tenant and set iscostimmediate on line as True
                    if (client.IsCostImmediate())
                    {
                        pro = new VAdvantage.Model.MProduct(Env.GetCtx(), m_lines[i].GetM_Product_ID(), Get_TrxName());

                        if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder))
                        {
                            if (!MCostQueue.CreateProductCostsDetails(GetCtx(), m_lines[i].GetAD_Client_ID(), m_lines[i].GetAD_Org_ID(), pro, 0,
                                  "Production Execution", null, null, null, null, m_lines[i], 0,
                                  _countGOM01 > 0 ? Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal(m_lines[i].GetGOM01_ActualQuantity())) :
                                Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal(m_lines[i].GetVAMFG_QtyEntered())), Get_TrxName(), out conversionNotFoundProductionExecution, optionalstr: "window"))
                            {
                                //if (!conversionNotFoundProductionExecution1.Contains(conversionNotFoundProductionExecution))
                                //{
                                //    conversionNotFoundProductionExecution1 += conversionNotFoundProductionExecution + " , ";
                                //}
                                log.Info("Cost not Calculated for Production Execution for this Line ID = " + VAdvantage.Utility.Util.GetValueOfInt(m_lines[i].GetVAMFG_M_WrkOdrTrnsctionLine_ID()));

                                m_lines[i].SetIsCostImmediate(false);
                                m_lines[i].Save(Get_TrxName());
                            }
                            else
                            {
                                m_lines[i].SetIsCostImmediate(true);
                                m_lines[i].Save(Get_TrxName());
                            }
                        }
                        else if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder))
                        {
                            if (!MCostQueue.CreateProductCostsDetails(GetCtx(), m_lines[i].GetAD_Client_ID(), m_lines[i].GetAD_Org_ID(), pro, 0,
                                "Production Execution", null, null, null, null, m_lines[i], 0,
                                _countGOM01 > 0 ? VAdvantage.Utility.Util.GetValueOfDecimal(m_lines[i].GetGOM01_ActualQuantity()) :
                                VAdvantage.Utility.Util.GetValueOfDecimal(m_lines[i].GetVAMFG_QtyEntered()), Get_TrxName(), out conversionNotFoundProductionExecution, optionalstr: "window"))
                            {
                                //if (!conversionNotFoundProductionExecution1.Contains(conversionNotFoundProductionExecution))
                                //{
                                //    conversionNotFoundProductionExecution1 += conversionNotFoundProductionExecution + " , ";
                                //}
                                log.Info("Cost not Calculated for Production Execution for this Line ID = " + VAdvantage.Utility.Util.GetValueOfInt(m_lines[i].GetVAMFG_M_WrkOdrTrnsctionLine_ID()));

                                m_lines[i].SetIsCostImmediate(false);
                                m_lines[i].Save(Get_TrxName());
                            }
                            else
                            {
                                m_lines[i].SetIsCostImmediate(true);
                                m_lines[i].Save(Get_TrxName());
                            }
                        }
                    }
                }

                // Added by Bharat on  11 Jan 2018 to set Queue qty on Opeartion as asked by Pradeep.
                MVAMFGMWorkOrderOperation wop = new MVAMFGMWorkOrderOperation(GetCtx(), m_lines[0].GetVAMFG_M_WorkOrderOperation_ID(), Get_TrxName());
                if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder))
                {
                    wop.SetVAMFG_QtyQueued(Decimal.Add(wop.GetVAMFG_QtyQueued(), GetVAMFG_QtyEntered()));
                }
                else
                {
                    wop.SetVAMFG_QtyQueued(Decimal.Subtract(wop.GetVAMFG_QtyQueued(), GetVAMFG_QtyEntered()));
                }
                if (!wop.Save())
                {

                }

                // Calcuale Cost of Finished Good - (Process Manufacturing)
                //if (_countGOM01 > 0)
                //{
                //    if (!CalculateFinishedGoodCost(GetCtx(), GetAD_Client_ID(), GetAD_Org_ID(), GetM_Product_ID(), GetM_AttributeSetInstance_ID(),
                //        GetVAMFG_M_WorkOrder_ID(), GetGOM01_BatchNo(), GetGOM01_ActualLiter(), Get_TrxName()))
                //    {
                //        if (string.IsNullOrEmpty(m_processMsg))
                //        {
                //            m_processMsg = "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": cost of finished good not calculated";
                //        }
                //        else
                //        {
                //            m_processMsg += "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": cost of finished good not calculated";
                //        }
                //        trx.Rollback();
                //        return DocActionConstants.STATUS_Invalid;
                //    }
                //}
            }

            if (woTxnType.Equals(VAMFG_WORKORDERTXNTYPE_ResourceUsage))
            {
                MVAMFGMWrkOdrRscTxnLine[] wortLines = GetResourceTxnLines(null, null);

                if (wortLines.Length == 0)
                {
                    m_processMsg = "WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": no resource transaction lines to process";
                    //Get_TrxName().rollback();
                    trx.Rollback();
                    return DocActionConstants.STATUS_Invalid;
                }

                //	loop through lines and update resource consumption on work order resource lines.
                foreach (MVAMFGMWrkOdrRscTxnLine wortl in wortLines)
                    if (!ProcessResTxnLine(wortl))
                    {
                        m_processMsg = m_processMsg + "- WorkOrderTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() + ": error processing resource transaction lines";
                        //Get_TrxName().rollback();
                        trx.Rollback();
                        return DocActionConstants.STATUS_Invalid;
                    }
            }

            // Update Work Order Dates
            // Implicit Assumption : this trx is successful to have reached this point
            //Timestamp trxDate = GetVAMFG_DateTrx();
            DateTime? trxDate = GetVAMFG_DateTrx().Value.ToLocalTime();
            if (trxDate == null)
                //trxDate = new Timestamp (System.currentTimeMillis());
                trxDate = DateTime.Now.Date;
            if (workOrder.GetVAMFG_DateActualFrom() == null || workOrder.GetVAMFG_DateActualFrom().Value.ToLocalTime() > (trxDate))
                workOrder.SetVAMFG_DateActualFrom(trxDate);
            if (workOrder.GetVAMFG_DateActualTo() == null || workOrder.GetVAMFG_DateActualTo().Value.ToLocalTime() < (trxDate))
                workOrder.SetVAMFG_DateActualTo(trxDate);
            if (!workOrder.Save(Get_TrxName()))
            {
                m_processMsg = "Error in saving work order " + VLogger.RetrieveError();
                //Get_TrxName().rollback();
                trx.Rollback();
                return DocActionConstants.STATUS_Invalid;
            }

            SetProcessed(true);
            SetDocAction(DOCACTION_Close);
            if (!Save(Get_TrxName()))
            {
                m_processMsg = "Error in saving Work Order Transaction - " + GetVAMFG_M_WrkOdrTransaction_ID();
                //Get_TrxName().rollback();
                trx.Rollback();
                return DocActionConstants.STATUS_Invalid;
            }

            if (warningLog.Length > 0)
                log.Warning(warningLog.ToString());
            if (infoLog.Length > 0)
                log.Info(infoLog.ToString());
            return DocActionConstants.STATUS_Completed;
        }

        public bool CalculateFinishedGoodCost(Ctx ctx, int AD_Client_ID, int AD_Org_ID, int M_Product_ID, int M_AttributeSetInstance_ID,
                                               int VAMFG_M_WorkOrder_ID, string BatchNo, Decimal FinishedGoodQty, Trx trxName)
        {
            try
            {
                string costingLevel = null;
                int cqAD_Org_ID = AD_Org_ID;

                // get Primary Accouting Schema
                int c_AcctSchema_ID = VAdvantage.Utility.Util.GetValueOfInt(DB.ExecuteScalar("SELECT c_acctschema1_id FROM ad_clientinfo WHERE IsActive = 'Y' AND ad_client_id = " + AD_Client_ID, null, null));

                VAdvantage.Model.MProduct product = new VAdvantage.Model.MProduct(ctx, M_Product_ID, trxName);
                MAcctSchema acctSchema = new MAcctSchema(ctx, c_AcctSchema_ID, trxName);

                // get Costing Level either from Product category else from Accounting Schema
                costingLevel = VAdvantage.Utility.Util.GetValueOfString(DB.ExecuteScalar(@"SELECT CostingLevel FROM M_Product_Category WHERE M_Product_Category_ID = " + product.GetM_Product_Category_ID(), null, null));
                if (string.IsNullOrEmpty(costingLevel))
                {
                    costingLevel = acctSchema.GetCostingLevel();
                }

                // set Organization and AttributeSetInstance for product costs
                if (costingLevel == "C" || costingLevel == "B")
                {
                    AD_Org_ID = 0;
                }
                if (costingLevel != "B")
                {
                    M_AttributeSetInstance_ID = 0;
                }

                String sql = @"SELECT ROUND(wot.GOM01_ActualDensity * (SUM(wotl.GOM01_ActualQuantity * wotl.CurrentCostPrice) / wot.GOM01_ActualQuantity) , 10) as Currenctcost
                             FROM VAMFG_M_WrkOdrTransaction wot
                             INNER JOIN VAMFG_M_WorkOrder wo ON wo.VAMFG_M_WorkOrder_ID = wot.VAMFG_M_WorkOrder_ID
                             INNER JOIN VAMFG_M_WrkOdrTrnsctionLine wotl ON wot.VAMFG_M_WrkOdrTransaction_ID = wotl.VAMFG_M_WrkOdrTransaction_ID
                           WHERE wotl.IsActive = 'Y' AND wot.VAMFG_M_WorkOrder_ID = " + VAMFG_M_WorkOrder_ID +
                          @" AND wot.VAMFG_WorkOrderTxnType = 'CI' " +
                          " AND wot.GOM01_BatchNo = '" + BatchNo + @"' GROUP BY wot.GOM01_ActualQuantity ,  wot.GOM01_ActualDensity";
                decimal currentcostprice = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, trxName));
                currentcostprice = Decimal.Round(currentcostprice, 10);

                // create Cost Detail
                MCostDetail cd = new MCostDetail(acctSchema, AD_Org_ID, M_Product_ID, M_AttributeSetInstance_ID, 0, Decimal.Round(Decimal.Multiply(currentcostprice, FinishedGoodQty), 10), FinishedGoodQty, "", trxName);
                cd.Set_Value("VAMFG_M_WorkOrder_ID", VAMFG_M_WorkOrder_ID);
                cd.SetProcessed(true);
                if (!cd.Save(trxName))
                {
                    ValueNamePair pp = VLogger.RetrieveError();
                    if (pp.GetName() != null)
                        log.Severe("Error in saving Cost detail - " + GetVAMFG_M_WrkOdrTransaction_ID() + ", " + pp.GetName());
                    return false;
                }

                // Create Cost queue
                if (FinishedGoodQty > 0)
                {
                    if (!CreateCostQueue(ctx, AD_Client_ID, cqAD_Org_ID, M_Product_ID, M_AttributeSetInstance_ID, FinishedGoodQty, c_AcctSchema_ID, acctSchema.GetM_CostType_ID(), currentcostprice, trxName))
                    {
                        ValueNamePair pp = VLogger.RetrieveError();
                        if (pp.GetName() != null)
                            log.Severe("Error in saving Cost queue - " + GetVAMFG_M_WrkOdrTransaction_ID() + ", " + pp.GetName());
                        return false;
                    }
                }
                else
                {
                    MCostElement Lifo = VAdvantage.Model.MCostElement.GetMaterialCostElement(ctx, VAdvantage.Model.X_C_AcctSchema.COSTINGMETHOD_Lifo);
                    MCostElement Fifo = VAdvantage.Model.MCostElement.GetMaterialCostElement(ctx, VAdvantage.Model.X_C_AcctSchema.COSTINGMETHOD_Fifo);
                    updateCostQueue(product, M_AttributeSetInstance_ID, acctSchema, cqAD_Org_ID, Lifo, Decimal.Negate(FinishedGoodQty));
                    updateCostQueue(product, M_AttributeSetInstance_ID, acctSchema, cqAD_Org_ID, Fifo, Decimal.Negate(FinishedGoodQty));
                }

                // need to update Product Cost for all costing method 
                MCostElement[] ces = GetCostingMethods(ctx, AD_Client_ID, trxName);
                MCost updateMCost = null;
                int precision = acctSchema.GetCostingPrecision();

                for (int i = 0; i < ces.Length; i++)
                {
                    updateMCost = null;
                    MCostElement ce = ces[i];
                    updateMCost = MCost.Get(product, M_AttributeSetInstance_ID, acctSchema, AD_Org_ID, ce.GetM_CostElement_ID());
                    updateMCost.SetCurrentQty(Decimal.Add(updateMCost.GetCurrentQty(), FinishedGoodQty));
                    updateMCost.SetCumulatedAmt(Decimal.Add(updateMCost.GetCumulatedAmt(), Decimal.Round(Decimal.Multiply(currentcostprice, FinishedGoodQty), precision)));
                    updateMCost.SetCumulatedQty(Decimal.Add(updateMCost.GetCumulatedQty(), FinishedGoodQty));

                    if (ce.IsStandardCosting())
                    {
                        if (Env.Signum(GetCurrentCostPrice()) == 0)
                        {
                            updateMCost.SetCurrentCostPrice(currentcostprice);
                        }
                    }
                    else if (ce.IsAverageInvoice() || ce.IsAveragePO())
                    {
                        if (Env.Signum(updateMCost.GetCumulatedQty()) != 0)
                        {
                            updateMCost.SetCurrentCostPrice(Decimal.Round(Decimal.Divide(updateMCost.GetCumulatedAmt(), updateMCost.GetCumulatedQty()), precision, MidpointRounding.AwayFromZero));
                        }
                        else
                        {
                            updateMCost.SetCurrentCostPrice(0);
                        }
                    }
                    else if (ce.IsLifo() || ce.IsFifo())
                    {
                        decimal totalPrice = 0;
                        decimal totalQty = 0;
                        MCostQueue[] cQueue = MCostQueue.GetQueue(product, M_AttributeSetInstance_ID, acctSchema, AD_Org_ID, ce, trxName);
                        if (cQueue != null && cQueue.Length > 0)
                        {
                            for (int j = 0; j < cQueue.Length; j++)
                            {
                                totalPrice += Decimal.Multiply(cQueue[j].GetCurrentCostPrice(), cQueue[j].GetCurrentQty());
                                totalQty += cQueue[j].GetCurrentQty();
                            }
                            updateMCost.SetCurrentCostPrice(Decimal.Round((totalPrice / totalQty), precision));
                        }
                        else if (cQueue.Length == 0)
                        {
                            updateMCost.SetCurrentCostPrice(0);
                        }
                    }
                    else if (ce.IsWeightedAverageCost())
                    {
                        var fromcost = 0.0M;
                        if (updateMCost.GetCurrentQty() != 0)
                            fromcost = Decimal.Divide(Decimal.Add(Decimal.Multiply(updateMCost.GetCurrentCostPrice(), Decimal.Subtract(updateMCost.GetCurrentQty(), FinishedGoodQty)), Decimal.Multiply(currentcostprice, FinishedGoodQty)), updateMCost.GetCurrentQty());
                        updateMCost.SetCurrentCostPrice(Decimal.Round(fromcost, precision));
                    }
                    else if (ce.IsWeightedAveragePO())
                    {
                        var fromcost = 0.0M;
                        if (updateMCost.GetCurrentQty() != 0)
                            fromcost = Decimal.Divide(Decimal.Add(Decimal.Multiply(updateMCost.GetCurrentCostPrice(), Decimal.Subtract(updateMCost.GetCurrentQty(), FinishedGoodQty)), Decimal.Multiply(currentcostprice, FinishedGoodQty)), updateMCost.GetCurrentQty());
                        updateMCost.SetCurrentCostPrice(Decimal.Round(fromcost, precision));
                    }

                    if (ce.IsLifo() || ce.IsFifo() || ce.IsAverageInvoice() || ce.IsLastInvoice() || ce.IsAveragePO() || ce.IsLastPOPrice() || ce.IsWeightedAverageCost() || ce.IsWeightedAveragePO() || ce.IsStandardCosting())
                    {
                        if (!CreateCostElementDetail(GetCtx(), GetAD_Client_ID(), AD_Org_ID, product, M_AttributeSetInstance_ID,
                                                        acctSchema, ce.GetM_CostElement_ID(), "Manufacturing", cd, (updateMCost.GetCurrentCostPrice() * cd.GetQty()), cd.GetQty()))
                        {
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp.GetName() != null)
                                log.Severe("Error in saving Cost element detail - " + GetVAMFG_M_WrkOdrTransaction_ID() + ", " + pp.GetName());
                            return false;
                        }
                    }

                    if (!updateMCost.Save(trxName))
                    {
                        ValueNamePair pp = VLogger.RetrieveError();
                        if (pp.GetName() != null)
                            log.Severe("Error in saving product Cost - " + GetVAMFG_M_WrkOdrTransaction_ID() + ", " + pp.GetName());
                        return false;
                    }
                }

                // Create Cost Combination 
                CreateCostForCombination(cd, acctSchema, product, M_AttributeSetInstance_ID, AD_Org_ID, "Cost Rollup");

            }
            catch
            {
                ValueNamePair pp = VLogger.RetrieveError();
                if (pp.GetName() != null)
                    log.Severe("Error in saving Cost calculation - " + GetVAMFG_M_WrkOdrTransaction_ID() + ", " + pp.GetName());
                return false;
            }
            return true;
        }

        // Create Cost Queue
        private bool CreateCostQueue(Ctx ctx, int AD_Client_ID, int AD_Org_ID, int M_Product_ID, int M_AttributeSetInstance_ID, Decimal FinishedGoodQty,
                                     int acctSchema_ID, int costType_ID, Decimal currentCost, Trx trxName)
        {
            try
            {
                MCostElement Lifo = VAdvantage.Model.MCostElement.GetMaterialCostElement(ctx, VAdvantage.Model.X_C_AcctSchema.COSTINGMETHOD_Lifo);
                MCostElement Fifo = VAdvantage.Model.MCostElement.GetMaterialCostElement(ctx, VAdvantage.Model.X_C_AcctSchema.COSTINGMETHOD_Fifo);
                MCostQueue FQueue = new MCostQueue(GetCtx(), 0, trxName);
                FQueue.SetAD_Org_ID(AD_Org_ID);
                FQueue.SetC_AcctSchema_ID(acctSchema_ID);
                FQueue.SetM_CostType_ID(costType_ID);
                FQueue.SetM_CostElement_ID(Fifo.GetM_CostElement_ID());
                FQueue.SetM_Product_ID(M_Product_ID);
                FQueue.SetM_AttributeSetInstance_ID(M_AttributeSetInstance_ID);
                FQueue.SetCurrentCostPrice(currentCost);
                FQueue.SetCurrentQty(FinishedGoodQty);
                FQueue.SetQueueDate(System.DateTime.Now);
                if (FQueue.Save(trxName))
                {
                    MCostQueue LQueue = new MCostQueue(GetCtx(), 0, trxName);
                    LQueue.SetAD_Org_ID(AD_Org_ID);
                    LQueue.SetC_AcctSchema_ID(acctSchema_ID);
                    LQueue.SetM_CostType_ID(costType_ID);
                    LQueue.SetM_CostElement_ID(Lifo.GetM_CostElement_ID());
                    LQueue.SetM_Product_ID(M_Product_ID);
                    LQueue.SetM_AttributeSetInstance_ID(M_AttributeSetInstance_ID);
                    LQueue.SetCurrentCostPrice(currentCost);
                    LQueue.SetCurrentQty(FinishedGoodQty);
                    LQueue.SetQueueDate(System.DateTime.Now);
                    if (LQueue.Save(trxName))
                    {
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
            return true;
        }

        //  update qty on Cost Queue
        private void updateCostQueue(VAdvantage.Model.MProduct product, int M_ASI_ID, MAcctSchema mas,
        int Org_ID, MCostElement ce, decimal movementQty)
        {
            Decimal qty = movementQty;
            #region Org Specific
            MCostQueue[] cQueue = MCostQueue.GetQueue(product, M_ASI_ID, mas, Org_ID, ce, null);
            if (cQueue != null && cQueue.Length > 0)
            {
                bool value = false;
                for (int cq = 0; cq < cQueue.Length; cq++)
                {
                    MCostQueue queue = cQueue[cq];
                    if (queue.GetCurrentQty() < 0) continue;
                    if (queue.GetCurrentQty() >= qty)
                    {
                        value = true;
                    }
                    else
                    {
                        value = false;
                    }
                    qty = MCostQueue.Quantity(queue.GetCurrentQty(), qty);
                    if (qty <= 0)
                    {
                        queue.Delete(true);
                        qty = Decimal.Negate(qty);
                    }
                    else
                    {
                        queue.SetCurrentQty(qty);
                        qty = 0;
                        if (!queue.Save())
                        {
                            //ValueNamePair pp = VLogger.RetrieveError();
                            //log.Severe("Cost Queue not updated by updateCostQueue for product  <===> " + product.GetM_Product_ID() + " Error Type is : " + pp.GetName());
                        }
                    }
                    if (value)
                    {
                        break;
                    }
                }
            }
            #endregion
        }

        private Decimal Quantity(Decimal cQueueQty, Decimal qty)
        {
            Decimal quantity = 0;
            quantity = Decimal.Subtract(cQueueQty, qty);
            return quantity;
        }

        // get All costing Method of material type
        public static MCostElement[] GetCostingMethods(Ctx ctx, int clientID, Trx trxName)
        {
            List<MCostElement> list = new List<MCostElement>();
            String sql = "SELECT * FROM M_CostElement "
                + "WHERE AD_Client_ID=@Client_ID"
                + " AND IsActive='Y' AND CostElementType='M' AND CostingMethod IS NOT NULL";
            DataTable dt = null;
            try
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@Client_ID", clientID);
                DataSet ds = DB.ExecuteDataset(sql, param, trxName);
                dt = new DataTable();
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new MCostElement(ctx, dr, trxName));
                }
            }
            catch (Exception e)
            {
                //log.Log(Level.SEVERE, sql, e);
            }
            finally
            {
                dt = null;
            }

            MCostElement[] retValue = new MCostElement[list.Count];
            retValue = list.ToArray();
            return retValue;
        }

        // Create cost element Detail
        public bool CreateCostElementDetail(Ctx ctx, int AD_Client_ID, int AD_Org_ID, VAdvantage.Model.MProduct Product, int M_ASI_ID,
                                                  MAcctSchema mas, int M_costElement_ID, string windowName, MCostDetail cd, decimal amt, decimal qty)
        {
            try
            {
                MCostElementDetail ced = new MCostElementDetail(ctx, 0, null);
                ced.SetAD_Client_ID(AD_Client_ID);
                ced.SetAD_Org_ID(AD_Org_ID);
                ced.SetC_AcctSchema_ID(mas.GetC_AcctSchema_ID());
                ced.SetM_CostElement_ID(M_costElement_ID);
                ced.SetM_Product_ID(Product.GetM_Product_ID());
                ced.SetM_AttributeSetInstance_ID(M_ASI_ID);
                ced.SetQty(qty);
                ced.SetAmt(amt);
                ced.SetIsSOTrx(cd.IsSOTrx());
                ced.Set_Value("VAMFG_M_WorkOrder_ID", VAdvantage.Utility.Util.GetValueOfInt(cd.Get_Value("VAMFG_M_WorkOrder_ID")));
                if (!ced.Save())
                {
                    ValueNamePair pp = VLogger.RetrieveError();
                    log.Info("Error Occured during costing " + pp.ToString());
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Info("Error Occured during costing " + ex.ToString());
                return false;
            }
            return true;
        }

        public bool CreateCostForCombination(MCostDetail cd, MAcctSchema acctSchema, VAdvantage.Model.MProduct product, int M_ASI_ID, int cq_AD_Org_ID, string windowName, string optionalStrcc = "process")
        {
            string sql;
            int AD_Org_ID = 0;
            // Get Org based on Costing Level
            dynamic pc = null;
            String cl = null;
            MCostElement ce = null;
            string costingMethod = null;
            int costElementId1 = 0;

            if (product != null)
            {
                pc = MProductCategory.Get(product.GetCtx(), product.GetM_Product_Category_ID());
                if (pc != null)
                {
                    cl = pc.GetCostingLevel();
                    costingMethod = pc.GetCostingMethod();
                    if (costingMethod == "C")
                    {
                        costElementId1 = pc.GetM_CostElement_ID();
                    }
                }
            }
            if (cl == null)
            {
                cl = acctSchema.GetCostingLevel();
                costingMethod = acctSchema.GetCostingMethod();
                if (costingMethod == "C")
                {
                    costElementId1 = acctSchema.GetM_CostElement_ID();
                }
            }
            if (cl == "C" || cl == "B")
            {
                AD_Org_ID = 0;
            }
            else
            {
                AD_Org_ID = cd.GetAD_Org_ID();
            }
            if (cl != "B")
            {
                M_ASI_ID = 0;
            }

            // when we complete a record, and costing method is not any combination, then no need to calculate
            if (optionalStrcc == "window" && costingMethod != "C")
            {
                return true;
            }

            // Get Cost element of Cost Combination type
            sql = @"SELECT ce.M_CostElement_ID ,  ce.Name ,  cel.lineno ,  cel.m_ref_costelement
                            FROM M_CostElement ce INNER JOIN m_costelementline cel ON ce.M_CostElement_ID = cel.M_CostElement_ID "
                          + "WHERE ce.AD_Client_ID=" + GetAD_Client_ID()
                          + " AND ce.IsActive='Y' AND ce.CostElementType='C' AND cel.IsActive='Y' ";
            if (optionalStrcc == "window" && costingMethod == "C")
            {
                sql += " AND ce.M_CostElement_ID = " + costElementId1;
            }
            sql += "ORDER BY ce.M_CostElement_ID";
            DataSet ds = new DataSet();
            ds = DB.ExecuteDataset(sql, null, null);
            try
            {
                MCost costCombination = null;
                MCost cost = null;
                int costElementId = 0;

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        costCombination = MCost.Get(product, M_ASI_ID, acctSchema, AD_Org_ID, VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_CostElement_ID"]));
                        costCombination.SetCurrentCostPrice(0);
                        costCombination.SetCurrentQty(0);
                        costCombination.SetCumulatedAmt(0);
                        costCombination.SetCumulatedQty(0);
                        costCombination.Save();
                    }
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        //change 3-5-2016
                        if (i == 0)
                        {
                            costElementId = VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_CostElement_ID"]);
                        }
                        if (costElementId != VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_CostElement_ID"]))
                        {
                            if (windowName == "Inventory Move" && (cl == "C" || cl == "B"))
                            {
                                // do not create Cost combination entry in case of ineventory move and costing level is client or Batch/lot
                            }
                            else if (windowName == "LandedCostAllocation" && (cl == "C" || cl == "B"))
                            {
                                MCostElementDetail.CreateCostElementDetail(GetCtx(), GetAD_Client_ID(), 0, product, M_ASI_ID,
                                              acctSchema, costElementId, "Cost Comination", cd, (costCombination.GetCurrentCostPrice() * cd.GetQty()), cd.GetQty());
                            }
                            else
                            {
                                MCostElementDetail.CreateCostElementDetail(GetCtx(), GetAD_Client_ID(), GetAD_Org_ID(), product, M_ASI_ID,
                                               acctSchema, costElementId, "Cost Comination", cd, (costCombination.GetCurrentCostPrice() * cd.GetQty()), cd.GetQty());
                            }
                        }
                        costElementId = VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_CostElement_ID"]);
                        //end 

                        // created object of Cost elemnt for checking iscalculated = true/ false
                        ce = MCostElement.Get(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_ref_costelement"]));

                        costCombination = MCost.Get(product, M_ASI_ID, acctSchema, AD_Org_ID, VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_CostElement_ID"]));
                        cost = MCost.Get(product, M_ASI_ID, acctSchema, AD_Org_ID, VAdvantage.Utility.Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_ref_costelement"]));
                        costCombination.SetCurrentCostPrice(Decimal.Add(costCombination.GetCurrentCostPrice(), cost.GetCurrentCostPrice()));
                        costCombination.SetCumulatedAmt(Decimal.Add(costCombination.GetCumulatedAmt(), cost.GetCumulatedAmt()));
                        // if calculated = true then we added qty else not and costing method is Standard Costing
                        if (ce.IsCalculated() || ce.GetCostingMethod() == MCostElement.COSTINGMETHOD_StandardCosting)
                        {
                            costCombination.SetCurrentQty(Decimal.Add(costCombination.GetCurrentQty(), cost.GetCurrentQty()));
                            costCombination.SetCumulatedQty(Decimal.Add(costCombination.GetCumulatedQty(), cost.GetCumulatedQty()));
                        }
                        costCombination.Save();
                        //change 3-5-2016
                        if (i == ds.Tables[0].Rows.Count - 1)
                        {
                            if (windowName == "Inventory Move" && (cl == "C" || cl == "B"))
                            {
                                // do not create Cost combination entry in case of ineventory move and costing level is client or Batch/lot
                            }
                            else if (windowName == "LandedCostAllocation" && (cl == "C" || cl == "B"))
                            {
                                CreateCostElementDetail(GetCtx(), GetAD_Client_ID(), 0, product, M_ASI_ID,
                                              acctSchema, costElementId, "Cost Comination", cd, (costCombination.GetCurrentCostPrice() * cd.GetQty()), cd.GetQty());
                            }
                            else
                            {
                                CreateCostElementDetail(GetCtx(), GetAD_Client_ID(), GetAD_Org_ID(), product, M_ASI_ID,
                                           acctSchema, costElementId, "Cost Comination", cd, (costCombination.GetCurrentCostPrice() * cd.GetQty()), cd.GetQty());
                            }
                        }
                        //end
                    }
                }
            }
            catch
            {
                //_log.Info("Error occured during CreateCostForCombination.");
                return false;
            }
            return true;
        }

        private Boolean GenerateMoveResourceUsageTxn(int opFrom, int opTo)
        {

            int count = 0;
            MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder wo1 = new CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            String whereClause = " VAMFG_SeqNo BETWEEN " + opFrom + " AND " + opTo;
            MVAMFGMWorkOrderOperation[] woos = MVAMFGMWorkOrderOperation.GetOfWorkOrder(wo1, whereClause, "VAMFG_SeqNo");
            //ArrayList<MVAMFGMWorkOrderResource> wors = new ArrayList<MVAMFGMWorkOrderResource>();
            List<MVAMFGMWorkOrderResource> wors = new List<MVAMFGMWorkOrderResource>();
            foreach (MVAMFGMWorkOrderOperation woo in woos)
            {
                if (woo.IsVAMFG_IsOptional())
                    continue;
                foreach (MVAMFGMWorkOrderResource wor in MVAMFGMWorkOrderResource.GetofWorkOrderOperation(woo, null, null))
                {
                    wors.Add(wor);
                    if (wor.GetVAMFG_ChargeType().Equals(X_VAMFG_M_WorkOrderResource.VAMFG_CHARGETYPE_Automatic))
                        count++;
                }
            }
            //	Even if opTo is optional, process it since it is explicitly passed
            if (woos[woos.Length - 1].IsVAMFG_IsOptional())
                foreach (MVAMFGMWorkOrderResource wor in MVAMFGMWorkOrderResource.GetofWorkOrderOperation(woos[woos.Length - 1], " VAMFG_QtyRequired != 0 ", null))
                {
                    wors.Add(wor);
                    if (wor.GetVAMFG_ChargeType().Equals(X_VAMFG_M_WorkOrderResource.VAMFG_CHARGETYPE_Automatic))
                        count++;
                }
            //	Even if opFrom is optional, process it since it is explicitly passed
            if (woos[0].IsVAMFG_IsOptional())
                foreach (MVAMFGMWorkOrderResource wor in MVAMFGMWorkOrderResource.GetofWorkOrderOperation(woos[0], " VAMFG_QtyRequired != 0 ", null))
                {
                    wors.Add(wor);
                    if (wor.GetVAMFG_ChargeType().Equals(X_VAMFG_M_WorkOrderResource.VAMFG_CHARGETYPE_Automatic))
                        count++;
                }

            if (count > 0)
            {
                MWorkOrderTxnUtil wotUtil = new MWorkOrderTxnUtil(true);
                ViennaAdvantage.CMFG.Model.MVAMFGMWrkOdrTransaction wot = wotUtil.createWOTxn(GetCtx(), GetVAMFG_M_WorkOrder_ID(),
    VAMFG_WORKORDERTXNTYPE_ResourceUsage,
                        GetVAMFG_M_WrkOdrTransaction_ID(), Get_TrxName());

                if (wot == null)
                {
                    VAdvantage.Model.ValueNamePair pp = VLogger.RetrieveError();
                    m_processMsg = pp.GetName();
                    log.Severe("Could not create Resource Usage Txn during Work Order Move : " + m_processMsg + ", for Work Order - " + GetVAMFG_M_WorkOrder_ID());
                }

                // set the txnDate & acctDate from the parent
                wot.SetVAMFG_DateTrx(GetVAMFG_DateTrx().Value.ToLocalTime());
                wot.SetVAMFG_DateAcct(GetVAMFG_DateAcct());

                int no = wotUtil.GenerateResourceTxnLine(GetCtx(), wot.GetVAMFG_M_WrkOdrTransaction_ID(), GetVAMFG_QtyEntered(),
                        new Decimal(opFrom), new Decimal(opTo), Get_TrxName(), true).Length;

                if (no > 0)
                {
                    if (!DocumentEngine.ProcessIt(wot, DocActionConstants.ACTION_Complete))
                    {
                        m_processMsg = wot.GetProcessMsg();
                        log.Severe("Could not complete resource charging : " + m_processMsg);
                        return false;
                    }
                    if (!wot.Save(Get_TrxName()))
                    {
                        VAdvantage.Model.ValueNamePair pp = VLogger.RetrieveError();
                        m_processMsg = pp.GetName();
                        log.Severe("Could not save Txn during Move Resource Usage.");
                        return false;
                    }
                }
            }
            return true;
        }

        private Boolean ProcessResTxnLine(MVAMFGMWrkOdrRscTxnLine wortl)
        {
            //	Implicit assumption : WOTxn type is RU -- no checking !
            MVAMFGMWorkOrderOperation woo = new MVAMFGMWorkOrderOperation(wortl.GetCtx(), wortl.GetVAMFG_M_WorkOrderOperation_ID(), wortl.Get_TrxName());
            String whereClause = " M_Product_ID = " + wortl.GetM_Product_ID();
            MVAMFGMWorkOrderResource[] wor = MVAMFGMWorkOrderResource.GetofWorkOrderOperation(woo, whereClause, "VAMFG_SeqNo");
            //Timestamp trxDate = GetVAMFG_DateTrx();
            DateTime? trxDate = GetVAMFG_DateTrx().Value.ToLocalTime();
            if (trxDate == null)
                //trxDate = new Timestamp (System.currentTimeMillis());
                trxDate = DateTime.Now.Date;
            if (wor == null || wor.Length == 0)
            {
                //	Production Resource does not exist in Work Order, so don't update Work Order 
                warningLog.Append("\nProduction Resource - " + VAdvantage.Model.MProduct.Get(GetCtx(), wortl.GetM_Product_ID()) + " not in Work Order Operation - " + woo.GetVAMFG_M_WorkOrderOperation_ID());
            }
            else
            {
                //	Implicit assumption : there is only 1 line in WOResource per Production Resource


                //wor[0].SetQtySpent(wor[0].GetVAMFG_QtySpent().add(wortl.GetVAMFG_QtyEntered()).setScale(MUOM.GetPrecision(GetCtx(), wor[0].GetC_UOM_ID()),MidpointRounding.AwayFromZero));
                wor[0].SetVAMFG_QtySpent(Decimal.Add(wor[0].GetVAMFG_QtySpent(), Decimal.Round((wortl.GetVAMFG_QtyEntered()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), wor[0].GetC_UOM_ID()))));
                // Also update the resource dates
                if (_countGOM01 > 0)
                {
                    if (wor[0].GetVAMFG_DateActualFrom() == null || wor[0].GetVAMFG_DateActualFrom() > (trxDate))
                        wor[0].SetVAMFG_DateActualFrom(trxDate);
                }
                else
                {
                    if (wor[0].GetVAMFG_DateActualFrom() == null || wor[0].GetVAMFG_DateActualFrom() > (woo.GetVAMFG_DateActualFrom()))
                        wor[0].SetVAMFG_DateActualFrom(woo.GetVAMFG_DateActualFrom().Value.ToLocalTime());
                }
                if (wor[0].GetVAMFG_DateActualTo() == null || wor[0].GetVAMFG_DateActualTo().Value.ToLocalTime() < (trxDate))
                    wor[0].SetVAMFG_DateActualTo(trxDate);
                wor[0].Save(Get_TrxName());

                MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), woo.GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
                Decimal overissue = Decimal.Subtract(wor[0].GetVAMFG_QtySpent(), Decimal.Multiply(wor[0].GetVAMFG_QtyRequired(), wo.GetVAMFG_QtyEntered()));
                if (overissue.CompareTo(Decimal.Zero) > 0)
                    warningLog.Append("\nOverissue of " + overissue + " for Production Resource " + VAdvantage.Model.MProduct.Get(GetCtx(), wor[0].GetM_Product_ID()));
            }
            wortl.SetProcessed(true);
            if (!wortl.Save(Get_TrxName()))
            {
                m_processMsg = "Could not save Resource Transaction Line - " + wortl.GetVAMFG_M_WrkOdrRscTxnLine_ID();
                return false;
            }

            // Update Work Order Operation Dates only if the transaction is successful
            //change by Amit - 30-Sep-2016
            if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress) && !IsVAMFG_WOComplete() && _countGOM01 > 0)
            {
                //if (woo.GetVAMFG_DateActualFrom() == null)
                //    woo.SetVAMFG_DateActualFrom(GetGOM01_OperationStartDate());
                //if (woo.GetVAMFG_DateActualTo() == null)
                //    woo.SetVAMFG_DateActualTo(GetGOM01_OperationEndDate());
            }
            else
            {
                if (woo.GetVAMFG_DateActualFrom() == null || woo.GetVAMFG_DateActualFrom().Value.ToLocalTime() > (trxDate))
                    woo.SetVAMFG_DateActualFrom(trxDate);
                if (woo.GetVAMFG_DateActualTo() == null || woo.GetVAMFG_DateActualTo().Value.ToLocalTime() < (trxDate))
                    woo.SetVAMFG_DateActualTo(trxDate);
            }
            if (!woo.Save(Get_TrxName()))
            {
                m_processMsg = "Could not update Dates for Work Order Operation - " + woo.GetVAMFG_M_WorkOrderOperation_ID();
                return false;
            }
            return true;
        }

        public MVAMFGMWrkOdrRscTxnLine[] GetResourceTxnLines(String whereClause, String orderClause)
        {
            //ArrayList<MVAMFGMWrkOdrRscTxnLine> list = new ArrayList<MVAMFGMWrkOdrRscTxnLine> ();
            List<MVAMFGMWrkOdrRscTxnLine> list = new List<MVAMFGMWrkOdrRscTxnLine>();

            StringBuilder sql = new StringBuilder("SELECT * FROM VAMFG_M_WrkOdrRscTxnLine WHERE VAMFG_M_WrkOdrTransaction_ID=@param1 ");

            if (whereClause != null)
                sql.Append(whereClause);
            if (orderClause != null)
                sql.Append(" ").Append(orderClause);
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            //PreparedStatement pstmt = DB.prepareStatement(sql.toString(), Get_TrxName());
            //ResultSet rs = null;
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", GetVAMFG_M_WrkOdrTransaction_ID());
                idr = DB.ExecuteReader(sql.ToString(), param, Get_TrxName());
                dt.Load(idr);
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MVAMFGMWrkOdrRscTxnLine ol = new MVAMFGMWrkOdrRscTxnLine(GetCtx(), dt.Rows[i], Get_TrxName());
                    list.Add(ol);
                }
                //            pstmt.setInt(1, GetVAMFG_M_WrkOdrTransaction_ID());
                //            rs = pstmt.executeQuery();
                //            while (rs.next())
                //            {
                //                MVAMFGMWrkOdrRscTxnLine ol = new MVAMFGMWrkOdrRscTxnLine(getCtx(), rs, 

                //Get_TrxName());
                //                list.add(ol);
                //            }
                //            rs.close();
                //            pstmt.close();
                //            pstmt = null;
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                log.Log(Level.SEVERE, sql.ToString(), e);
            }

            //
            MVAMFGMWrkOdrRscTxnLine[] lines = new MVAMFGMWrkOdrRscTxnLine[list.Count];
            lines = list.ToArray();
            return lines;
        }

        /// <summary>
        /// Add to Description
        ///</summary>
        /// <param name="description">description text</param>
        public void AddDescription(String description)
        {
            String desc = GetVAMFG_Description();
            if (desc == null)
                SetVAMFG_Description(description);
            else
                SetVAMFG_Description(desc + " | " + description);
        }	//	addDescription

        /// <summary>
        /// Before Delete
        ///  </summary>
        /// <returns>true of it can be deleted</returns>

        protected override Boolean BeforeDelete()
        {
            if (IsProcessed())
                return false;

            if (DocActionConstants.STATUS_InProgress.Equals(GetDocStatus()))
            {
                log.SaveError("Prepared", "Prepared", false);
                return false;
            }

            return true;
        }	//	beforeDelete

        /// <summary>
        /// Create PDF
        ///  </summary>
        /// <returns>File or null</returns>
        public FileInfo CreatePDF()
        {
            //try
            //{
            //    File temp = File.createTempFile(Get_TableName() + Get_ID() + "_", ".pdf");
            //    return createPDF(temp);
            //}
            //catch (Exception e)
            //{
            //    log.Severe("Could not create PDF - " + e.Message);
            //}
            return null;
        }	//	getPDF

        /// <summary>
        /// Create PDF file
        ///  </summary>
        ///  <param name="file">file output file</param>
        /// <returns>file if success</returns>
        public FileInfo CreatePDF(FileInfo file)
        {
            return null;
        }	//	createPDF

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#approveIt()
         */
        public Boolean ApproveIt()
        {
            return true;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#closeIt()
         */
        public Boolean CloseIt()
        {
            SetProcessing(false);
            SetDocAction(DOCACTION_None);
            return true;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#getApprovalAmt()
         */
        public Decimal GetApprovalAmt()
        {
            return Decimal.Zero;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#getC_Currency_ID()
         */
        public int GetC_Currency_ID()
        {
            return 0;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#getDoc_User_ID()
         */
        public int GetDoc_User_ID()
        {
            return 0;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#getProcessMsg()
         */
        public String GetProcessMsg()
        {
            return m_processMsg;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#getSummary()
         */
        public String GetSummary()
        {
            return null;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#invalidateIt()
         */
        public Boolean InvalidateIt()
        {
            return false;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#prepareIt()
         */
        public String PrepareIt()
        {
            // Work order prepared?
            MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            if (!wo.GetDocStatus().Equals(X_VAMFG_M_WorkOrder.DOCSTATUS_InProgress))
            {
                m_processMsg = "Work Order - " + wo.GetVAMFG_M_WorkOrder_ID() + ", not in progress.";
                return DocActionConstants.STATUS_Invalid;
            }

            MDocType dt = MDocType.Get(GetCtx(), GetC_DocType_ID());

            if (!MPeriod.IsOpen(GetCtx(), GetVAMFG_DateAcct(), dt.GetDocBaseType()))
            {
                _processMsg = "@PeriodClosed@";
                return DocActionVariables.STATUS_INVALID;
            }
            if (!CheckWorkTransactionExist(true))
            {
                _processMsg = Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithFutureDate");
                return DocActionVariables.STATUS_INVALID;
            }
            // Checkmovdate
            if (!Checkmovdate(true))
            {
                _processMsg = Msg.GetMsg(GetCtx(), "Movement date is not Current date Cannot Complete Transaction");
                return DocActionVariables.STATUS_INVALID;
            }
            // Checkmovdate

            if (!DOCACTION_Complete.Equals(GetDocAction()))
                SetDocAction(DOCACTION_Complete);
            return DocActionConstants.STATUS_InProgress;
        }

        /// <summary>
        /// 	Called before Save for Pre-Save Operation
        /// 	</summary>
        /// 	<param name="newRecord">newRecord new record</param>
        /// <returns>true if record can be saved</returns>

        protected override Boolean BeforeSave(bool newRecord)
        {

            //New logic
            //Complete WOT before add new WOT.
            //24-04-2012
            _countGOM01 = Convert.ToInt32(DB.ExecuteScalar("SELECT COUNT(AD_ModuleInfo_ID) FROM AD_ModuleInfo WHERE Prefix like 'GOM01_'"));
            IDataReader idrNew = null;
            try
            {
                string sql = " SELECT docstatus ,  VAMFG_WORKORDERTXNTYPE,  VAMFG_M_WrkOdrTransaction_ID" +
                             " FROM VAMFG_M_WrkOdrTransaction  WHERE VAMFG_M_WorkOrder_ID= " + GetVAMFG_M_WorkOrder_ID() +
                             " AND VAMFG_WORKORDERTXNTYPE  ='CI' and isactive='Y'" +
                             " and docstatus <>'VO' and docstatus <>'RE'";

                idrNew = DB.ExecuteReader(sql);
                if (idrNew.Read())
                {
                    if (VAdvantage.Utility.Util.GetValueOfString(idrNew["VAMFG_WORKORDERTXNTYPE"]) == "CI")
                    {
                        if (VAdvantage.Utility.Util.GetValueOfInt(idrNew["VAMFG_M_WrkOdrTransaction_ID"]) != GetVAMFG_M_WrkOdrTransaction_ID())
                        {
                            if (VAdvantage.Utility.Util.GetValueOfString(idrNew["docstatus"]) != "CO")
                            {
                                m_processMsg = "@ComponentIssueToWorkOrder@";
                                log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "CompleteComponentIssueToWorkOrder"));
                                idrNew.Close();
                                return false;
                            }
                        }
                    }
                }
                idrNew.Close();
            }
            catch
            {
                if (idrNew != null)
                {
                    idrNew.Close();
                    idrNew = null;
                }

            }

            if (!CheckWorkTransactionExist(true))
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithFutureDate"));
                return false;
            }

            //if (!CheckWorkTransactionExist(false))
            //{
            //    log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithBackDate"));
            //    return false;
            //}

            // Added by Bharat on 21/03/2017
            if (_countGOM01 > 0)
            {
                if (GetVAMFG_Description() != null && GetVAMFG_Description().Contains("{->"))
                {

                }
                else
                {
                    //string sql = " SELECT Count(VAMFG_M_WrkOdrTransaction_ID) FROM VAMFG_M_WrkOdrTransaction WHERE IsActive = 'Y' AND GOM01_BatchNo = '" + GetGOM01_BatchNo() +
                    //"' AND VAMFG_M_WrkOdrTransaction_ID != " + Get_ID() + " AND VAMFG_M_WorkOrder_ID != " + GetVAMFG_M_WorkOrder_ID();
                    if (GetVAMFG_WorkOrderTxnType() == "CI")
                    {
                        string sql = @" SELECT Count(VAMFG_M_WrkOdrTransaction_ID) FROM VAMFG_M_WrkOdrTransaction WHERE IsActive = 'Y' AND GOM01_BatchNo = '" + GetGOM01_BatchNo() + @"'  AND vamfg_description Not like '%->%' 
                         AND AD_Client_ID=" + GetAD_Client_ID() + " AND VAMFG_M_WrkOdrTransaction_ID != " + Get_ID() + " AND AD_Org_ID=" + GetAD_Org_ID() + " AND VAMFG_WorkOrderTxnType='" + GetVAMFG_WorkOrderTxnType() + "'";
                        int batchCount = VAdvantage.Utility.Util.GetValueOfInt(DB.ExecuteScalar(sql, null, Get_TrxName()));
                        if (batchCount > 0)
                        {
                            log.SaveError("Error", "Batch Number already used for another Production Execution");
                            return false;
                        }

                        MProduct product = new MProduct(GetCtx(), GetM_Product_ID(), Get_TrxName());

                        if (GetGOM01_ActualDensity() < Util.GetValueOfDecimal(Get_Value("GOM01_MinDensity")) || GetGOM01_ActualDensity() > Util.GetValueOfDecimal(product.Get_Value("GOM01_MaxDensity")))
                        {
                            log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_DensityRange"));
                            return false;
                        }
                    }
                }
            }
            // End Bharat

            if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress) && Is_ValueChanged("VAMFG_QtyEntered"))
            {
                MVAMFGMWorkOrderOperation opFrom = new MVAMFGMWorkOrderOperation(GetCtx(), GetOperationFrom_ID(), Get_TrxName());
                String stepFrom = GetVAMFG_StepFrom();
                Tuple<String, String, String> mInfo = null;
                if (Env.HasModulePrefix("GOM01_", out mInfo))
                {
                    if (stepFrom.Equals(VAMFG_STEPFROM_Scrap))
                    {
                        m_processMsg = "@NotEnoughQty@";
                        log.SaveError("Error", "Quantities scrapped cannot be moved out");
                        return false;
                    }
                }
                else
                {
                    if (stepFrom.Equals(VAMFG_STEPFROM_Waiting))
                    {
                        if (opFrom.GetVAMFG_QtyQueued().CompareTo(GetVAMFG_QtyEntered()) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@QtyEntered@ > @QtyQueued@"));
                            return false;
                        }
                    }
                    else if (stepFrom.Equals(VAMFG_STEPFROM_Process))
                    {
                        if (opFrom.GetVAMFG_QtyRun().CompareTo(GetVAMFG_QtyEntered()) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@QtyEntered@ > @QtyRun@"));
                            return false;
                        }
                    }
                    else if (stepFrom.Equals(VAMFG_STEPTO_Finish))
                    {
                        if (opFrom.GetVAMFG_QtyAssembled().CompareTo(GetVAMFG_QtyEntered()) < 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@QtyEntered@ > @VAMFG_qtyassembled@"));
                            return false;
                        }
                    }
                    else if (stepFrom.Equals(VAMFG_STEPFROM_Scrap))
                    {
                        m_processMsg = "@NotEnoughQty@";
                        log.SaveError("Error", "Quantities scrapped cannot be moved out");
                        return false;
                    }
                }
            }

            if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
            {
                if (GetOperationTo_ID() <= 0)
                {
                    //				m_processMsg = "@NoOperation@";
                    log.SaveError("Error", "NoOperation");
                    return false;
                }
                MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
                if (GetVAMFG_QtyEntered().CompareTo(Decimal.Subtract(wo.GetVAMFG_QtyAssembled(), (wo.GetVAMFG_QtyAvailable()))) > 0 && Is_ValueChanged("VAMFG_QtyEntered"))
                {
                    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@NotEnoughQty@"));
                    return false;
                }
                SetOperationFrom_ID(GetOperationTo_ID());
            }

            if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore) && Is_ValueChanged("VAMFG_QtyEntered"))
            {
                int operationID = 0;
                Decimal VAMFG_qtyassembled = Decimal.Zero;
                StringBuilder sql = new StringBuilder("SELECT VAMFG_M_WorkOrderOperation_ID, VAMFG_QtyAssembled FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = @param1 ORDER BY VAMFG_SeqNo DESC");

                //PreparedStatement pstmt = DB.prepareStatement(sql.toString(), Get_TrxName());
                //ResultSet rs = null;
                SqlParameter[] param = null;
                IDataReader idr = null;
                DataTable dt = new DataTable();
                try
                {
                    param = new SqlParameter[1];
                    param[0] = new SqlParameter("@param1", GetVAMFG_M_WorkOrder_ID());
                    idr = DB.ExecuteReader(sql.ToString(), param, Get_TrxName());
                    if (idr.Read())
                    {
                        operationID = VAdvantage.Utility.Util.GetValueOfInt(idr[0]);
                        VAMFG_qtyassembled = VAdvantage.Utility.Util.GetValueOfDecimal(idr[1]);
                    }
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                    //pstmt.setInt(1, GetVAMFG_M_WorkOrder_ID());
                    //rs = pstmt.executeQuery();
                    //if (rs.next())
                    //{
                    //    operationID = rs.getInt(1);
                    //    VAMFG_qtyassembled = rs.getBigDecimal(2);
                    //}
                    //rs.close();
                    //pstmt.close();
                    //pstmt = null;
                }
                catch (Exception e)
                {
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                    log.Log(Level.SEVERE, sql.ToString(), e);
                }

                if (VAMFG_qtyassembled.CompareTo(_countGOM01 > 0 ? GetGOM01_ActualLiter() : GetVAMFG_QtyEntered()) < 0)
                {
                    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@NotEnoughQty@"));
                    return false;
                }
                SetOperationFrom_ID(operationID);
                SetOperationTo_ID(operationID);
            }

            // get current cost from product cost on new record and when product changed
            // currency conversion also required if order has different currency with base currency
            if ((newRecord || (Is_ValueChanged("M_Product_ID"))) && GetM_Product_ID() > 0)
            {
                decimal currentcostprice = MCost.GetproductCosts(GetAD_Client_ID(), GetAD_Org_ID(), GetM_Product_ID(), 0, Get_TrxName());
                if (GetVAMFG_Description() != null && GetVAMFG_Description().Contains("{->"))
                {
                    // not to set cuurent cost price on reversal because its already filed during creation of line
                }
                else
                {
                    SetCurrentCostPrice(currentcostprice);
                }
            }

            if (Is_ValueChanged("VAMFG_QtyEntered"))
            {
                //BigDecimal qtyEntered = GetVAMFG_QtyEntered().setScale(MUOM.getPrecision(getCtx(), getC_UOM_ID()),BigDecimal.ROUND_HALF_UP);
                Decimal qtyEntered = Decimal.Round((GetVAMFG_QtyEntered()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()));
                if (qtyEntered.CompareTo(GetVAMFG_QtyEntered()) != 0)
                {
                    log.Fine("Corrected QtyEntered Scale UOM=" + GetC_UOM_ID()
                            + "; QtyEntered =" + GetVAMFG_QtyEntered() + "->" + qtyEntered);
                    SetVAMFG_QtyEntered(qtyEntered);
                }
            }

            if (_countGOM01 > 0 && Is_ValueChanged("GOM01_ActualQuantity"))
            {
                if (GetGOM01_ActualDensity() > 0 && GetGOM01_ActualQuantity() > 0)
                {
                    Decimal literQty = Decimal.Divide(GetGOM01_ActualQuantity(), GetGOM01_ActualDensity());
                    SetGOM01_ActualLiter(Decimal.Round((literQty), MUOM.GetPrecision(GetCtx(), GetC_UOM_ID())));
                }
            }

            return true;
        }

        private bool CheckWorkTransactionExist(bool futureDate)
        {
            string Sql = @"Select Count(VAMFG_M_WrkOdrTransaction_ID) From VAMFG_M_WrkOdrTransaction  Where AD_Client_ID=" + GetAD_Client_ID() + " AND AD_Org_ID=" + GetAD_Org_ID() + " ";
            if (futureDate)
            {
                Sql += " AND DocStatus IN ('CO','CL','IP','DR') AND VAMFG_DateTrx > " + GlobalVariable.TO_DATE(GetVAMFG_DateTrx(), true);
            }
            else
            {
                Sql += " AND Mov.DocStatus IN ('IP') AND VAMFG_DateTrx < " + GlobalVariable.TO_DATE(GetVAMFG_DateTrx(), true);
            }
            int Count = Util.GetValueOfInt(DB.ExecuteScalar(Sql, null, Get_TrxName()));
            if (Count > 0)
            {
                return false;
            }
            return true;
        }
        private bool Checkmovdate(bool futureDate)
        {

            string Sql = @"Select Count(VAMFG_M_WrkOdrTransaction_ID) From VAMFG_M_WrkOdrTransaction  Where AD_Client_ID=" + GetAD_Client_ID() + " AND AD_Org_ID=" + GetAD_Org_ID() + "AND Mov.DocStatus IN ('IP') AND VAMFG_DateTrx < sysdate  AND VAMFG_M_WrkOdrTransaction_ID = " +GetVAMFG_M_WrkOdrTransaction_ID();
            int cnt = Util.GetValueOfInt(DB.ExecuteScalar(Sql, null, Get_TrxName()));
            if (cnt > 0)
            {
                return false;
            }
            return true;
        }

        /**	Process Message 			*/
        private String m_processMsg = null;

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#reActivateIt()
         */
        public Boolean ReActivateIt()
        {
            return false;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#rejectIt()
         */
        public Boolean RejectIt()
        {
            return false;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#reverseAccrualIt()
         */
        public Boolean ReverseAccrualIt()
        {
            return false;
        }

        /// <summary>
        /// Copy Lines From other Work Order Transaction
        /// </summary>
        ///  <param name="otherWOTxn">otherWOTxn</param>
        /// <returns>number of lines copied</returns>
        public int CopyLinesFrom(MVAMFGMWrkOdrTransaction otherWOTxn)
        {
            if (IsProcessed() || IsPosted() || otherWOTxn == null)
                return 0;
            MVAMFGMWrkOdrTrnsctionLine[] fromLines = otherWOTxn.GetLines(null, "ORDER BY VAMFG_M_WrkOdrTrnsctionLine_ID");
            int count = 0;
            foreach (MVAMFGMWrkOdrTrnsctionLine fromLine in fromLines)
            {
                MVAMFGMWrkOdrTrnsctionLine line = new MVAMFGMWrkOdrTrnsctionLine(GetCtx(), 0, Get_TrxName());
                line.Set_TrxName(Get_TrxName());
                VAdvantage.Model.PO.CopyValues(fromLine, line, fromLine.GetAD_Client_ID(), fromLine.GetAD_Org_ID());
                line.SetVAMFG_M_WrkOdrTransaction_ID(GetVAMFG_M_WrkOdrTransaction_ID());
                line.Set_ValueNoCheck("VAMFG_M_WrkOdrTrnsctionLine_ID", I_ZERO);	//	new
                line.SetProcessed(false);
                if (line.Get_ColumnIndex("ReversalDoc_ID") > 0)
                {
                    //line.SetReversalDoc_ID(fromLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
                    line.Set_Value("ReversalDoc_ID", fromLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID());
                }
                if (line.Save(Get_TrxName()))
                    count++;
                //	Cross Link
            }
            if (fromLines.Length != count)
                log.Log(Level.SEVERE, "Line difference - From=" + fromLines.Length + " <> Saved=" + count);
            return count;
        }	//	copyLinesFrom

        /// <summary>
        /// Copy Resource Transaction Lines from other Work Order Transaction
        ///</summary>
        ///<param name="otherWOTxn">otherWOTxn</param>
        /// <returns>number of lines copied</returns>
        public int CopyResLinesFrom(MVAMFGMWrkOdrTransaction otherWOTxn)
        {

            if (IsProcessed() || IsPosted() || otherWOTxn == null)
                return 0;
            MVAMFGMWrkOdrRscTxnLine[] fromResLines = otherWOTxn.GetResourceTxnLines(null, "ORDER BY VAMFG_M_WrkOdrRscTxnLine_ID");
            int count = 0;
            foreach (MVAMFGMWrkOdrRscTxnLine fromResLine in fromResLines)
            {
                MVAMFGMWrkOdrRscTxnLine resLine = new MVAMFGMWrkOdrRscTxnLine(GetCtx(), 0, Get_TrxName());
                resLine.Set_TrxName(Get_TrxName());
                VAdvantage.Model.PO.CopyValues(fromResLine, resLine, fromResLine.GetAD_Client_ID(),

    fromResLine.GetAD_Org_ID());
                resLine.SetVAMFG_M_WrkOdrTransaction_ID(GetVAMFG_M_WrkOdrTransaction_ID());
                resLine.Set_ValueNoCheck("VAMFG_M_WrkOdrRscTxnLine_ID", I_ZERO);	// new
                resLine.SetProcessed(false);

                if (resLine.Save(Get_TrxName()))
                    count++;
            }
            if (fromResLines.Length != count)
                log.Severe("Resource Line difference - From = " + fromResLines.Length + " <> Saved = " +

    count);
            return count;
        }

        /** Reversal Flag		*/
        private Boolean m_reversal = false;
        private int m_parentReversal = 0;

        /// <summary>
        /// Set Reversal
        /// </summary>
        ///  <param name="reversal"> reversal reversal</param>
        /// <param name="woTxnID">woTxnID work order transaction ID to be reversed</param>
        private void SetReversal(Boolean reversal, int woTxnID)
        {
            m_reversal = reversal;
        }	//	setReversal

        /**
         * 	Set Parent Reversal
         *	@param parentReversal work order transaction ID of the parent reversal txn
         */
        private void SetParentReversal(int parentReversal)
        {
            m_parentReversal = parentReversal;
        }

        /// <summary>
        /// Is Reversal
        /// </summary>
        /// <returns> reversal</returns>
        private Boolean IsReversal()
        {
            return m_reversal;
        }	//	isReversal

        private Boolean m_forceReverse = false;

        private void SetForceReverse(Boolean force)
        {
            m_forceReverse = force;
        }

        private Boolean IsForceReverse()
        {
            return m_forceReverse;
        }

        /// <summary>
        /// Set Processed.
        /// Propagate to Lines
        /// </summary>
        ///  <param name="processed">processed processed</param>
        public void SetProcessed(Boolean processed)
        {
            base.SetProcessed(processed);
            if (Get_ID() == 0)
                return;
            String set = "SET Processed='"
                + (processed ? "Y" : "N")
                + "' WHERE VAMFG_M_WrkOdrTransaction_ID=" + GetVAMFG_M_WrkOdrTransaction_ID();
            int noLine = DB.ExecuteQuery("UPDATE VAMFG_M_WrkOdrTrnsctionLine " + set, null, Get_TrxName());

            log.Fine(processed + " - Lines=" + noLine);
        }	//	setProcessed

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#reverseCorrectIt()
         */
        public Boolean ReverseCorrectIt()
        {
            _countGOM01 = Convert.ToInt32(DB.ExecuteScalar("SELECT COUNT(*) FROM AD_ModuleInfo WHERE Prefix like 'GOM01_'"));

            Trx trx = null;
            trx = Trx.Get(Get_TrxName().GetTrxName(), true);
            if (GetParentWorkOrderTxn_ID() != 0 && !IsForceReverse())
            {
                m_processMsg = "Cannot reverse a child transaction without reversing the parent transaction - " + GetParentWorkOrderTxn_ID();
                return false;
            }

            log.Info(ToString());

            if (!CheckWorkTransactionExist(true))
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithFutureDate"));
                return false;
            }

            //if (!CheckWorkTransactionExist(false))
            //{
            //    log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithBackDate"));
            //    return false;
            //}

            m_processMsg = DocumentEngine.IsPeriodOpen(this);
            if (m_processMsg != null)
                return false;

            //	Deep Copy
            MVAMFGMWrkOdrTransaction reversal = new MVAMFGMWrkOdrTransaction(GetCtx(), 0, Get_TrxName());
            CopyValues(this, reversal, GetAD_Client_ID(), GetAD_Org_ID());
            reversal.Set_ValueNoCheck("DocumentNo", null);
            reversal.SetDocStatus(DOCSTATUS_Drafted);
            reversal.SetDocAction(DOCACTION_Complete);
            reversal.SetIsApproved(false);
            reversal.SetPosted(false);
            reversal.SetDocumentNo(GetDocumentNo() + REVERSE_INDICATOR);	//	indicate reversals
            reversal.AddDescription("{->" + GetDocumentNo() + ")");
            reversal.SetVAMFG_QtyEntered(Decimal.Negate(GetVAMFG_QtyEntered()));
            if (_countGOM01 > 0)
            {
                reversal.SetGOM01_ActualQuantity(Decimal.Negate(GetGOM01_ActualQuantity()));
                reversal.SetGOM01_Quantity(Decimal.Negate(GetGOM01_Quantity()));
                reversal.SetGOM01_ActualLiter(Decimal.Negate(GetGOM01_ActualLiter()));
            }
            reversal.SetVAMFG_DateTrx(DateTime.Now.ToLocalTime());
            //reversal.SetVAMFG_DateTrx(new DateTime(DateTime.Now.ToShortTimeString()));
            reversal.SetProcessed(false);
            if (m_parentReversal != 0)
            {
                reversal.SetParentWorkOrderTxn_ID(m_parentReversal);
            }

            // Update parent transaction reversals to point to the reversed work order if any
            MVAMFGMWorkOrder workorder = new MVAMFGMWorkOrder(GetCtx(), reversal.GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            if (DOCSTATUS_Reversed.Equals(workorder.GetDocStatus()))
            {
                MVAMFGMWorkOrder WOReversal = workorder.GetReversal();
                if (WOReversal != null)
                {
                    reversal.SetReversalM_WorkOrder_ID(WOReversal.GetVAMFG_M_WorkOrder_ID());
                    // also set the operations to point to the reversed document id
                    if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress)
                            ||

    GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore)
                            ||

    GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                    {
                        int fromSeqNo = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_SeqNo FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrderOperation_ID = @param1", reversal.GetOperationFrom_ID());
                        int reversalFromOpID = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = " + reversal.GetVAMFG_M_WorkOrder_ID() + " AND VAMFG_SeqNo = " + fromSeqNo);
                        reversal.SetOperationFrom_ID(reversalFromOpID);



                        if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore) ||

                        GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                            reversal.SetParentReversal(reversalFromOpID);
                        else
                        {
                            int toSeqNo = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_SeqNo FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrderOperation_ID = @param1", reversal.GetOperationTo_ID());
                            int reversalToOpID = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = " + reversal.GetVAMFG_M_WorkOrder_ID() + " AND VAMFG_SeqNo = " + toSeqNo);
                            reversal.SetOperationTo_ID(reversalToOpID);
                        }
                    }
                }
            }



            if (!reversal.Save(Get_TrxName()))
            {
                m_processMsg = "Could not create Reversal for Work Order Transaction - " +

               GetVAMFG_M_WrkOdrTransaction_ID();
                return false;
            }
            string sqlSec = "select * from VAMFG_M_WrkOdrTxnLineMA where VAMFG_M_WrkOdrTrnsctionLine_ID=" +
                 " (SELECT VAMFG_M_WrkOdrTrnsctionLine_ID from VAMFG_M_WrkOdrTrnsctionLine tnl where " +
                 " tnl.VAMFG_M_WrkOdrTransaction_ID=" + GetVAMFG_M_WrkOdrTransaction_ID() + ")";

            DataSet countAttribute = VAdvantage.DataBase.DB.ExecuteDataset(sqlSec, null, null);
            //Only update Wo When Wot Is reversed
            //Date 03-Jan-2012
            if (reversal.GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
            {
                if (_countGOM01 > 0)
                {
                    workorder.SetVAMFG_QtyAvailable(Decimal.Negate(reversal.GetGOM01_ActualLiter()));
                }
                else
                {
                    workorder.SetVAMFG_QtyAvailable(Decimal.Negate(reversal.GetVAMFG_QtyEntered()));
                }
                if (workorder.Save(Get_TrxName()))
                {
                    //update last opration
                    String sql1 = "Update VAMFG_M_WorkOrderOperation set VAMFG_qtyassembled =" + workorder.GetVAMFG_QtyAvailable() + " where VAMFG_M_WorkOrderOperation_ID = " +
                        "(select max(VAMFG_M_WorkOrderOperation_ID) from VAMFG_M_WorkOrderOperation where VAMFG_M_WorkOrder_ID=" + workorder.GetVAMFG_M_WorkOrder_ID() + ")";
                    int res = DB.ExecuteQuery(sql1, null, Get_TrxName());


                    // Update Storage on reversing Work Order 
                    // 03-Jan-2012
                    VAdvantage.Model.MLocator loc = VAdvantage.Model.MLocator.Get(GetCtx(), GetM_Locator_ID());
                    VAdvantage.Model.MProduct prod = new VAdvantage.Model.MProduct(Env.GetCtx(), reversal.GetM_Product_ID(), Get_TrxName());
                    if (prod.GetM_AttributeSet_ID() != 0)
                    {

                        sql1 = "select * from VAMFG_M_WrkOdrTxnLineMA where VAMFG_M_WrkOdrTrnsctionLine_ID= (SELECT VAMFG_M_WrkOdrTrnsctionLine_ID from VAMFG_M_WrkOdrTrnsctionLine tnl where tnl.VAMFG_M_WrkOdrTransaction_ID=" + GetVAMFG_M_WrkOdrTransaction_ID() + ")";
                        DataSet count = VAdvantage.DataBase.DB.ExecuteDataset(sql1, null, null);

                        for (int i = 0; i < count.Tables[0].Rows.Count; i++)
                        {
                            int attribute = 0;
                            if (countAttribute != null)
                            {
                                attribute = VAdvantage.Utility.Util.GetValueOfInt(countAttribute.Tables[0].Rows[i]["m_attributesetinstance_id"]);
                            }
                            VAdvantage.Model.MStorage store = VAdvantage.Model.MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), attribute, Get_TrxName());
                            if (reversal.GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
                            {
                                //MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                //    GetM_Product_ID(), attribute, attribute, Decimal.Subtract(store.GetQtyOnHand(), Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal(countAttribute.Tables[0].Rows[i]["movementQty"]))), Decimal.Zero, Decimal.Zero, Get_TrxName());
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                   GetM_Product_ID(), attribute, attribute, Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal
                                   (countAttribute.Tables[0].Rows[i]["VAMFG_MOVEMENTQTY"])), Decimal.Zero, Decimal.Zero, Get_TrxName());

                                // check disallow negative inventoy or not 
                                MWarehouse warehouse = MWarehouse.Get(GetCtx(), loc.GetM_Warehouse_ID());
                                if (warehouse.IsDisallowNegativeInv())
                                {
                                    MStorage storage = MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), attribute, Get_TrxName());
                                    //if (storage != null && storage.GetQtyOnHand() < 0)
                                    //{
                                    //    Get_TrxName().Rollback();
                                    //    m_processMsg = Msg.GetMsg(GetCtx(), "NotEnoughQtyInStock") + " , " + prod.GetName();
                                    //    return false;
                                    //}
                                    if (storage != null && storage.GetQtyOnHand() < Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal
                                                                                            (countAttribute.Tables[0].Rows[i]["VAMFG_MOVEMENTQTY"])))
                                    {
                                        Get_TrxName().Rollback();
                                        m_processMsg = Msg.GetMsg(GetCtx(), "NotEnoughQtyInStock") + " , " + prod.GetName();
                                        return false;
                                    }
                                }
                            }
                            else if (reversal.GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                            {
                                //       MStorage.AddQtys(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                //GetM_Product_ID(), attribute, attribute, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                                //Changed by Pratap
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(), GetM_Product_ID(), attribute, attribute, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                            }

                            // added by Bharat on 25 Sep 2017 to set reverse entry on M_Transaction.

                            MTransaction mTrx = new MTransaction(GetCtx(), GetAD_Org_ID(), X_M_Transaction.MOVEMENTTYPE_WorkOrder_,
                            loc.GetM_Locator_ID(), GetM_Product_ID(), attribute, Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal
                                   (countAttribute.Tables[0].Rows[i]["VAMFG_MOVEMENTQTY"])), reversal.GetVAMFG_DateTrx().Value.ToLocalTime(), Get_TrxName());

                            mTrx.SetVAMFG_M_WrkOdrTrnsctionLine_ID(VAdvantage.Utility.Util.GetValueOfInt(countAttribute.Tables[0].Rows[i]["VAMFG_M_WrkOdrTrnsctionLine_ID"]));
                            mTrx.SetVAMFG_M_WrkOdrTransaction_ID(GetVAMFG_M_WrkOdrTransaction_ID());
                            mTrx.SetVAMFG_M_WorkOrder_ID(reversal.GetVAMFG_M_WorkOrder_ID());

                            // Added by Bharat on 25 Sep 2017 to update Current Quantity on M_Transaction.

                            string qry = @"SELECT SUM(t.CurrentQty) keep (dense_rank last ORDER BY t.MovementDate, t.M_Transaction_ID) AS CurrentQty FROM m_transaction t 
                                                        INNER JOIN M_Locator l ON t.M_Locator_ID = l.M_Locator_ID WHERE t.MovementDate <= " + GlobalVariable.TO_DATE(reversal.GetVAMFG_DateTrx(), true) +
                                        " AND t.AD_Client_ID = " + GetAD_Client_ID() + " AND l.AD_Org_ID = " + GetAD_Org_ID() + " AND t.M_Locator_ID = " + loc.GetM_Locator_ID() +
                                        " AND t.M_Product_ID = " + GetM_Product_ID() + " AND NVL(t.M_AttributeSetInstance_ID,0) = " + attribute;
                            Decimal trxQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(qry, null, Get_TrxName()));
                            mTrx.SetCurrentQty(trxQty + Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal(countAttribute.Tables[0].Rows[i]["VAMFG_MOVEMENTQTY"])));
                            if (!mTrx.Save(Get_TrxName()))
                            {
                                log.Log(Level.SEVERE, "VAMFG_M_WrkOdrTrnsctionLine " + VAdvantage.Utility.Util.GetValueOfInt(countAttribute.Tables[0].Rows[i]["VAMFG_M_WrkOdrTrnsctionLine_ID"]) +
                                "processInventory: MTransaction not saved");
                                return false;
                            }
                            Decimal currentQty = trxQty + (_countGOM01 > 0 ? reversal.GetGOM01_ActualLiter() : reversal.GetVAMFG_QtyEntered());
                            UpdateTransaction(reversal, mTrx, currentQty);
                        }
                    }
                    else
                    {
                        VAdvantage.Model.MStorage store = VAdvantage.Model.MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), 0, Get_TrxName());
                        MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                            GetM_Product_ID(), 0, 0, _countGOM01 > 0 ? reversal.GetGOM01_ActualLiter() : reversal.GetVAMFG_QtyEntered(),
                            Decimal.Zero, Decimal.Zero, Get_TrxName());

                        // check disallow negative inventoy or not 
                        MWarehouse warehouse = MWarehouse.Get(GetCtx(), loc.GetM_Warehouse_ID());
                        if (warehouse.IsDisallowNegativeInv())
                        {
                            MStorage storage = MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), 0, Get_TrxName());
                            if (storage != null && storage.GetQtyOnHand() < 0)
                            {
                                Get_TrxName().Rollback();
                                m_processMsg = Msg.GetMsg(GetCtx(), "NotEnoughQtyInStock") + " , " + prod.GetName();
                                return false;
                            }
                            //if (storage != null && storage.GetQtyOnHand() < GetGOM01_ActualLiter())
                            //{
                            //    Get_TrxName().Rollback();
                            //    m_processMsg = Msg.GetMsg(GetCtx(), "NotEnoughQtyInStock") + " , " + prod.GetName();
                            //    return false;
                            //}
                        }

                        // added by Bharat on 25 Sep 2017 to set reverse entry on M_Transaction.

                        MTransaction mTrx = new MTransaction(GetCtx(), GetAD_Org_ID(), X_M_Transaction.MOVEMENTTYPE_WorkOrder_,
                        loc.GetM_Locator_ID(), GetM_Product_ID(), 0, _countGOM01 > 0 ? reversal.GetGOM01_ActualLiter() : reversal.GetVAMFG_QtyEntered(),
                        reversal.GetVAMFG_DateTrx().Value.ToLocalTime(), Get_TrxName());

                        mTrx.SetVAMFG_M_WrkOdrTrnsctionLine_ID(VAdvantage.Utility.Util.GetValueOfInt(countAttribute.Tables[0].Rows[0]["VAMFG_M_WrkOdrTrnsctionLine_ID"]));
                        mTrx.SetVAMFG_M_WrkOdrTransaction_ID(GetVAMFG_M_WrkOdrTransaction_ID());
                        mTrx.SetVAMFG_M_WorkOrder_ID(reversal.GetVAMFG_M_WorkOrder_ID());

                        // Added by Bharat on 25 Sep 2017 to update Current Quantity on M_Transaction.

                        string qry = @"SELECT SUM(t.CurrentQty) keep (dense_rank last ORDER BY t.MovementDate, t.M_Transaction_ID) AS CurrentQty FROM m_transaction t 
                                                    INNER JOIN M_Locator l ON t.M_Locator_ID = l.M_Locator_ID WHERE t.MovementDate <= " + GlobalVariable.TO_DATE(reversal.GetVAMFG_DateTrx(), true) +
                                    " AND t.AD_Client_ID = " + GetAD_Client_ID() + " AND l.AD_Org_ID = " + GetAD_Org_ID() + " AND t.M_Locator_ID = " + loc.GetM_Locator_ID() +
                                    " AND t.M_Product_ID = " + GetM_Product_ID() + " AND NVL(t.M_AttributeSetInstance_ID,0) = " + 0;
                        Decimal trxQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(qry, null, Get_TrxName()));
                        mTrx.SetCurrentQty(trxQty + (_countGOM01 > 0 ? reversal.GetGOM01_ActualLiter() : reversal.GetVAMFG_QtyEntered()));
                        if (!mTrx.Save(Get_TrxName()))
                        {
                            log.Log(Level.SEVERE, "VAMFG_M_WrkOdrTransaction " + GetVAMFG_M_WrkOdrTransaction_ID() +
                            "processInventory: MTransaction not saved");
                            return false;
                        }
                        Decimal currentQty = trxQty + (_countGOM01 > 0 ? reversal.GetGOM01_ActualLiter() : reversal.GetVAMFG_QtyEntered());
                        UpdateTransaction(reversal, mTrx, currentQty);
                    }
                }
            }
            // Added Code To be Tested
            else if (reversal.GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
            {
                workorder.SetVAMFG_QtyAvailable(Decimal.Subtract(workorder.GetVAMFG_QtyAvailable(), Decimal.Negate(reversal.GetVAMFG_QtyEntered())));
                if (workorder.Save(Get_TrxName()))
                {
                    // Update Storage on reversing Work Order 
                    // 03-Jan-2012
                    VAdvantage.Model.MLocator loc = VAdvantage.Model.MLocator.Get(GetCtx(), GetM_Locator_ID());
                    VAdvantage.Model.MProduct prod = new VAdvantage.Model.MProduct(Env.GetCtx(), reversal.GetM_Product_ID(), Get_TrxName());
                    if (prod.GetM_AttributeSet_ID() != 0)
                    {

                        string sql1 = "select * from VAMFG_M_WrkOdrTxnLineMA where VAMFG_M_WrkOdrTrnsctionLine_ID= (SELECT VAMFG_M_WrkOdrTrnsctionLine_ID from VAMFG_M_WrkOdrTrnsctionLine tnl where tnl.VAMFG_M_WrkOdrTransaction_ID=" + GetVAMFG_M_WrkOdrTransaction_ID() + ")";
                        DataSet count = VAdvantage.DataBase.DB.ExecuteDataset(sql1, null, null);

                        for (int i = 0; i < count.Tables[0].Rows.Count; i++)
                        {
                            int attribute = 0;
                            if (countAttribute != null)
                            {
                                attribute = VAdvantage.Utility.Util.GetValueOfInt(countAttribute.Tables[0].Rows[i]["m_attributesetinstance_id"]);
                            }
                            VAdvantage.Model.MStorage store = VAdvantage.Model.MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), attribute, Get_TrxName());
                            if (reversal.GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore))
                            {
                                //MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                //    GetM_Product_ID(), attribute, attribute, Decimal.Subtract(store.GetQtyOnHand(), Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal(countAttribute.Tables[0].Rows[i]["movementQty"]))), Decimal.Zero, Decimal.Zero, Get_TrxName());
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                   GetM_Product_ID(), attribute, attribute, Decimal.Negate(VAdvantage.Utility.Util.GetValueOfDecimal
                                   (countAttribute.Tables[0].Rows[i]["VAMFG_MOVEMENTQTY"])), Decimal.Zero, Decimal.Zero, Get_TrxName());

                                // check disallow negative inventoy or not 
                                MWarehouse warehouse = MWarehouse.Get(GetCtx(), loc.GetM_Warehouse_ID());
                                if (warehouse.IsDisallowNegativeInv())
                                {
                                    MStorage storage = MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), attribute, Get_TrxName());
                                    if (storage != null && storage.GetQtyOnHand() < 0)
                                    {
                                        Get_TrxName().Rollback();
                                        m_processMsg = Msg.GetMsg(GetCtx(), "NotEnoughQtyInStock") + " , " + prod.GetName();
                                        return false;
                                    }
                                }
                            }
                            else if (reversal.GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                            {
                                //       MStorage.AddQtys(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                                //GetM_Product_ID(), attribute, attribute, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                                //Changed by Pratap
                                MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                      GetM_Product_ID(), attribute, attribute, Decimal.Zero, Decimal.Zero, Decimal.Zero, Get_TrxName());
                            }
                        }
                    }
                    else
                    {
                        VAdvantage.Model.MStorage store = VAdvantage.Model.MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), 0, Get_TrxName());
                        MStorage.Add(GetCtx(), loc.GetM_Warehouse_ID(), loc.GetM_Locator_ID(),
                          GetM_Product_ID(), 0, 0, _countGOM01 > 0 ? Decimal.Negate(reversal.GetGOM01_ActualLiter()) : Decimal.Negate(reversal.GetVAMFG_QtyEntered()),
                          Decimal.Zero, Decimal.Zero, Get_TrxName());

                        // check disallow negative inventoy or not 
                        MWarehouse warehouse = MWarehouse.Get(GetCtx(), loc.GetM_Warehouse_ID());
                        if (warehouse.IsDisallowNegativeInv())
                        {
                            MStorage storage = MStorage.Get(GetCtx(), loc.GetM_Locator_ID(), GetM_Product_ID(), 0, Get_TrxName());
                            if (storage != null && storage.GetQtyOnHand() < 0)
                            {
                                Get_TrxName().Rollback();
                                m_processMsg = Msg.GetMsg(GetCtx(), "NotEnoughQtyInStock") + " , " + prod.GetName();
                                return false;
                            }
                        }
                    }
                }
            }

            reversal.SetReversal(true, GetVAMFG_M_WrkOdrTransaction_ID());

            // Reverse the child transactions
            String sql = "SELECT VAMFG_M_WrkOdrTransaction_ID FROM VAMFG_M_WrkOdrTransaction" +
            " WHERE ParentWorkOrderTxn_ID = @param1";
            VAdvantage.Model.MRole role = VAdvantage.Model.MRole.GetDefault(GetCtx(), false);
            sql = role.AddAccessSQL(sql, "VAMFG_M_WrkOdrTransaction", VAdvantage.Model.MRole.SQL_NOTQUALIFIED, VAdvantage.Model.MRole.SQL_RO);
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            //PreparedStatement pstmt = DB.prepareStatement(sql, Get_TrxName());
            //ResultSet rs = null;
            Boolean success = true;
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", GetVAMFG_M_WrkOdrTransaction_ID());
                idr = DB.ExecuteReader(sql.ToString(), param, Get_TrxName());
                dt.Load(idr);
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MVAMFGMWrkOdrTransaction childToReverse = new MVAMFGMWrkOdrTransaction(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(dt.Rows[i]["VAMFG_M_WrkOdrTransaction_ID"]), Get_TrxName());
                    childToReverse.SetForceReverse(true); // bypass the usual "can't reverse a child transaction" rule
                    childToReverse.SetParentReversal(reversal.GetVAMFG_M_WrkOdrTransaction_ID());
                    if (!DocumentEngine.ProcessIt(childToReverse, DocActionConstants.ACTION_Reverse_Correct))
                    {
                        m_processMsg = "Child Reversal ERROR: " + childToReverse.GetProcessMsg();
                        //Get_TrxName().rollback();
                        trx.Rollback();
                        success = false;
                        break;
                    }
                    childToReverse.Save(Get_TrxName());
                }
                //pstmt.setInt(1, GetVAMFG_M_WrkOdrTransaction_ID());
                //rs = pstmt.executeQuery();

                //while(rs.next()){
                //    MVAMFGMWrkOdrTransaction childToReverse = new MVAMFGMWrkOdrTransaction(getCtx(), rs.getInt(1), Get_TrxName());
                //    childToReverse.setForceReverse(true); // bypass the usual "can't reverse a child transaction" rule
                //    childToReverse.setParentReversal(reversal.GetVAMFG_M_WrkOdrTransaction_ID());
                //    if(!DocumentEngine.processIt(childToReverse, DocActionConstants.ACTION_Reverse_Correct))
                //    {
                //        m_processMsg = "Child Reversal ERROR: " + childToReverse.getProcessMsg();
                //        Get_TrxName().rollback();
                //        success = false;
                //        break;
                //    }
                //    childToReverse.save(Get_TrxName());
                //}
                //rs.close();
                //pstmt.close();
            }
            catch
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                log.Log(Level.SEVERE, sql);
                m_processMsg = "Could not reverse child transactions of Work Order Transaction " + GetVAMFG_M_WorkOrder_ID();
                return false;
            }

            if (!success)
                return false;

            // Copy Lines
            if (VAMFG_WORKORDERTXNTYPE_ResourceUsage.Equals(GetVAMFG_WorkOrderTxnType()))
            {
                reversal.CopyResLinesFrom(this);

                MVAMFGMWrkOdrRscTxnLine[] rResLines = reversal.GetResourceTxnLines(null, "ORDER BY M_WorkOrderResourceTxnLine_ID");
                foreach (MVAMFGMWrkOdrRscTxnLine rResLine in rResLines)
                {
                    //				MVAMFGMWrkOdrRscTxnLine rResLine = rResLines[i];
                    rResLine.SetVAMFG_QtyEntered(Decimal.Negate(rResLine.GetVAMFG_QtyEntered()));

                    int VAMFG_SeqNo = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_SeqNo FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrderOperation_ID = @param1", rResLine.GetVAMFG_M_WorkOrderOperation_ID());
                    int reversalOpID = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID =" + reversal.GetVAMFG_M_WorkOrder_ID() + " AND VAMFG_SeqNo =" + VAMFG_SeqNo);
                    rResLine.SetVAMFG_M_WorkOrderOperation_ID(reversalOpID);

                    if (!rResLine.Save(Get_TrxName()))
                    {
                        m_processMsg = "Could not save reversal line for resource transaction - " + GetVAMFG_M_WrkOdrTransaction_ID();
                        return false;
                    }
                }
            }

            else
            {
                MVAMFGMWrkOdrTransaction txn = new MVAMFGMWrkOdrTransaction(GetCtx(), GetVAMFG_M_WrkOdrTransaction_ID(), Get_TrxName());
                reversal.CopyLinesFrom(txn); // copy lines uses "ORDER BY VAMFG_M_WrkOdrTrnsctionLine_ID"

                //	Reverse Line Qty
                MVAMFGMWrkOdrTrnsctionLine[] rLines = reversal.GetLines(null, "ORDER BY VAMFG_M_WrkOdrTrnsctionLine_ID");
                for (int i = 0; i < rLines.Length; i++)
                {
                    MVAMFGMWrkOdrTrnsctionLine rLine = rLines[i];
                    ViennaAdvantage.CMFG.Model.MVAMFGMWrkOdrTrnsctionLine txnline = new CMFG.Model.MVAMFGMWrkOdrTrnsctionLine(GetCtx(), rLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID(), Get_TrxName());
                    rLine.SetVAMFG_QtyEntered(Decimal.Negate(rLine.GetVAMFG_QtyEntered()));

                    // added By Amit - 01-10-2016
                    if (_countGOM01 > 0)
                    {
                        rLine.SetGOM01_Density(Decimal.Negate(rLine.GetGOM01_Density()));
                        rLine.SetGOM01_Litre(Decimal.Negate(rLine.GetGOM01_Litre()));
                        rLine.SetGOM01_Quantity(Decimal.Negate(rLine.GetGOM01_Quantity()));
                        rLine.SetGOM01_ActualQuantity(Decimal.Negate(rLine.GetGOM01_ActualQuantity()));
                    }

                    int VAMFG_SeqNo = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_SeqNo FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrderOperation_ID = @param1", rLine.GetVAMFG_M_WorkOrderOperation_ID());
                    int reversalOpID = DB.GetSQLValue(Get_TrxName(), "SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = " + reversal.GetVAMFG_M_WorkOrder_ID() + " AND VAMFG_SeqNo =" + VAMFG_SeqNo);
                    rLine.SetVAMFG_M_WorkOrderOperation_ID(reversalOpID);

                    if (!rLine.Save(Get_TrxName()))
                    {
                        m_processMsg = "Could not save reversal line for component transaction - " + GetVAMFG_M_WrkOdrTransaction_ID();
                        return false;
                    }

                    //	We need to copy MA
                    if (rLine.GetM_AttributeSetInstance_ID() == 0)
                    {
                        m_lines = GetLines(null, "ORDER BY M_Product_ID");
                        MVAMFGMWrkOdrTxnLineMA[] mas = MVAMFGMWrkOdrTxnLineMA.Get(GetCtx(), m_lines[i].GetVAMFG_M_WrkOdrTrnsctionLine_ID(), Get_TrxName());
                        foreach (MVAMFGMWrkOdrTxnLineMA element in mas)
                        {
                            MVAMFGMWrkOdrTxnLineMA ma = new MVAMFGMWrkOdrTxnLineMA(txnline, element.GetM_AttributeSetInstance_ID(), Decimal.Negate(element.GetVAMFG_MovementQty()));
                            if (!ma.Save())
                                ;
                        }
                    }
                }
            }

            //
            if (!DocumentEngine.ProcessIt(reversal, DocActionConstants.ACTION_Complete))
            {
                m_processMsg = "Reversal ERROR: " + reversal.GetProcessMsg();
                //Get_TrxName().rollback();

                return false;
            }
            DocumentEngine.ProcessIt(reversal, DocActionConstants.ACTION_Close);
            reversal.SetDocStatus(DOCSTATUS_Reversed);
            reversal.SetDocAction(DOCACTION_None);
            reversal.Save();
            m_processMsg = reversal.GetDocumentNo();

            //	Update Reversed (this)
            AddDescription("(" + reversal.GetDocumentNo() + "<-)");
            SetProcessed(true);
            SetDocStatus(DOCSTATUS_Reversed);	//	may come from void
            SetDocAction(DOCACTION_None);

            return true;
        }


        /// <summary>
        /// String Representation
        /// </summary>
        /// <returns> info</returns>
        public String ToString()
        {
            StringBuilder sb = new StringBuilder("MVAMFGMWrkOdrTransaction[").Append(Get_ID()).Append("-").Append(GetDocumentNo())
            .Append(",C_DocType_ID=").Append(GetC_DocType_ID())
            .Append(", M_Product_ID=").Append(GetM_Product_ID())
            .Append(", VAMFG_WORKORDERTXNTYPE=").Append(GetVAMFG_WorkOrderTxnType())
            .Append(", QtyEntered=").Append(GetVAMFG_QtyEntered())
            .Append("]");
            return sb.ToString();
        }	//	toString

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#unlockIt()
         */
        public Boolean UnlockIt()
        {
            return false;
        }

        private Boolean m_forceVoid = false;

        private void SetForceVoid(Boolean force)
        {
            m_forceVoid = force;
        }

        private Boolean IsForceVoid()
        {
            return m_forceVoid;
        }

        /* (non-Javadoc)
         * @see org.compiere.process.DocAction#voidIt()
         */
        public Boolean VoidIt()
        {
            log.Info(ToString());

            if (GetParentWorkOrderTxn_ID() != 0 && !IsForceVoid())
            {
                m_processMsg = "Cannot void a child transaction - " + GetVAMFG_M_WrkOdrTransaction_ID()
                + ", without voiding the parent transaction - " + GetParentWorkOrderTxn_ID();
                return false;
            }

            if (DOCSTATUS_Closed.Equals(GetDocStatus())
                    || DOCSTATUS_Reversed.Equals(GetDocStatus())
                    || DOCSTATUS_Voided.Equals(GetDocStatus()))
            {
                m_processMsg = "Document Closed: " + GetDocStatus();
                return false;
            }

            //	Not Processed
            if (DOCSTATUS_Drafted.Equals(GetDocStatus())
                    || DOCSTATUS_Invalid.Equals(GetDocStatus())
                    || DOCSTATUS_InProgress.Equals(GetDocStatus())
                    || DOCSTATUS_Approved.Equals(GetDocStatus())
                    || DOCSTATUS_NotApproved.Equals(GetDocStatus()))
            {
                // Void the child transactions
                String sql = "SELECT VAMFG_M_WrkOdrTransaction_ID FROM VAMFG_M_WrkOdrTransaction" +
                " WHERE ParentWorkOrderTxn_ID = @param1";
                VAdvantage.Model.MRole role = VAdvantage.Model.MRole.GetDefault(GetCtx(), false);
                sql = role.AddAccessSQL(sql, "VAMFG_M_WrkOdrTransaction", VAdvantage.Model.MRole.SQL_NOTQUALIFIED, VAdvantage.Model.MRole.SQL_RO);
                //PreparedStatement pstmt = DB.prepareStatement(sql, Get_TrxName());
                //ResultSet rs = null;
                SqlParameter[] param = null;
                IDataReader idr = null;
                DataTable dt = new DataTable();
                try
                {
                    param = new SqlParameter[1];
                    param[0] = new SqlParameter("@param1", GetVAMFG_M_WrkOdrTransaction_ID());
                    idr = DB.ExecuteReader(sql.ToString(), param, Get_TrxName());
                    dt.Load(idr);
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        MVAMFGMWrkOdrTransaction childToVoid = new MVAMFGMWrkOdrTransaction(GetCtx(), VAdvantage.Utility.Util.GetValueOfInt(dt.Rows[i]["VAMFG_M_WrkOdrTransaction_ID"]), Get_TrxName());
                        childToVoid.SetForceVoid(true); // bypass the usual "can't reverse a child transaction" rule
                        DocumentEngine.ProcessIt(childToVoid, DocActionConstants.ACTION_Void);
                        childToVoid.Save();
                    }
                    //pstmt.setInt(1, );
                    //rs = pstmt.executeQuery();

                    //while(rs.next()){
                    //    MVAMFGMWrkOdrTransaction childToVoid = new MVAMFGMWrkOdrTransaction(getCtx(), rs.getInt(1), Get_TrxName());
                    //    childToVoid.setForceVoid(true); // bypass the usual "can't reverse a child transaction" rule
                    //    DocumentEngine.processIt(childToVoid, DocActionConstants.ACTION_Void);
                    //    childToVoid.save();
                    //}

                    //rs.close();
                    //pstmt.close();
                }
                catch
                {
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }
                    log.Log(Level.SEVERE, sql);
                    m_processMsg = "Could not void child transactions of Work Order Transaction " + GetVAMFG_M_WorkOrder_ID();
                    return false;
                }


                //	Set lines to 0
                m_lines = GetLines(null, null);
                foreach (MVAMFGMWrkOdrTrnsctionLine line in m_lines)
                {
                    Decimal old = line.GetVAMFG_QtyEntered();
                    if (Env.Signum(old) != 0)
                    {
                        line.SetVAMFG_QtyEntered(Env.ZERO);
                        line.AddDescription("Void (" + old + ")");
                        line.Save(Get_TrxName());
                    }
                }

                // get resource transaction lines
                MVAMFGMWrkOdrRscTxnLine[] lines = GetResourceTxnLines(null, null);
                foreach (MVAMFGMWrkOdrRscTxnLine line in lines)
                {
                    Decimal old = line.GetVAMFG_QtyEntered();
                    if (Env.Signum(old) != 0)
                    {
                        line.SetVAMFG_QtyEntered(Env.ZERO);
                        line.AddDescription("Void (" + old + ")");
                        line.Save(Get_TrxName());
                    }
                }

                if (GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress))
                {
                    Decimal old = GetVAMFG_QtyEntered();
                    if (Env.Signum(old) != 0)
                    {
                        SetVAMFG_QtyEntered(Env.ZERO);
                        AddDescription("Void (" + old + ")");
                    }
                }

            }
            else
            {
                return DocumentEngine.ProcessIt(this, DocActionConstants.ACTION_Reverse_Correct);
            }

            SetProcessed(true);
            SetDocAction(DOCACTION_None);
            return true;
        }	//	voidIt

        //@Override
        public void SetProcessMsg(String processMsg)
        {
            m_processMsg = processMsg;
        }

        //@Override
        public String GetDocBaseType()
        {
            VAdvantage.Model.MDocType dt = VAdvantage.Model.MDocType.Get(GetCtx(), GetC_DocType_ID());
            return dt.GetDocBaseType();
        }

        // @Override
        public DateTime? GetDocumentDate()
        {
            return GetVAMFG_DateTrx();
        }


        public VAdvantage.Utility.Env.QueryParams GetLineOrgsQueryInfo()
        {
            if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_ResourceUsage))
            {
                return new VAdvantage.Utility.Env.QueryParams(
                        "SELECT DISTINCT AD_Org_ID FROM VAMFG_M_WrkOdrRscTxnLine WHERE VAMFG_M_WrkOdrTransaction_ID = @param1",
                        new Object[] { GetVAMFG_M_WrkOdrTransaction_ID() });
            }
            else if (GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_2_WorkOrderInProgress) // Work Order Move
                    || GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_3_TransferAssemblyToStore) // Assembly completion to Inventory
                    || GetVAMFG_WorkOrderTxnType().Equals(VAMFG_WORKORDERTXNTYPE_AssemblyReturnFromInventory))
                return null;
            else
            {
                return new VAdvantage.Utility.Env.QueryParams(
                        "SELECT DISTINCT AD_Org_ID FROM VAMFG_M_WrkOdrTrnsctionLine WHERE VAMFG_M_WrkOdrTransaction_ID=@param1 ",
                        new Object[] { GetVAMFG_M_WrkOdrTransaction_ID() });
            }
        }


        #region DocAction Members


        //public Env.QueryParams GetLineOrgsQueryInfo()
        //{
        //}

        #endregion

        #region DocAction Members


        public Trx Get_Trx()
        {
            return Trx.Get(Get_TrxName().GetTrxName());
        }

        #endregion


        public bool ProcessIt(string action)
        {
            _processMsg = null;
            DocumentEngine engine = new DocumentEngine(this, GetDocStatus());
            return engine.ProcessIt(action, GetDocAction());
        }
    }
}