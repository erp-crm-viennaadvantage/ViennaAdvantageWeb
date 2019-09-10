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
using ViennaAdvantage.CMFG.Model;
using VAdvantage.Model;

namespace ViennaAdvantage.Model
{
    public class MVAMFGMWorkOrder : X_VAMFG_M_WorkOrder, DocAction
    {
        private static new VLogger log = VLogger.GetVLogger(typeof(MVAMFGMWorkOrder).FullName);
        private static long serialVersionUID = 1L;
        /**	Process Message 			*/
        private String _processMsg = null;

        private int _countGOM01 = 0;

        public MVAMFGMWorkOrder(Ctx ctx, int VAMFG_M_WorkOrder_ID, Trx trx)
            : base(ctx, VAMFG_M_WorkOrder_ID, trx)
        {
            //super(ctx, M_WorkOrder_ID, trx);

            //  New
            if (VAMFG_M_WorkOrder_ID == 0)
            {
                SetDocStatus(DOCSTATUS_Drafted);
                SetDocAction(DOCACTION_Prepare);
                //
                SetIsApproved(false);
                SetIsSOTrx(true);
                //
                base.SetProcessed(false);

                SetProcessing(false);
                SetPosted(false);
            }
        } // MWorkOrder

        /** Reversal Indicator			*/
        public static String REVERSE_INDICATOR = "^";

        public MVAMFGMWorkOrder(Ctx ctx, DataRow rs, Trx trx)
            : base(ctx, rs, trx)
        {
            //super(ctx, rs, trx);
        }

        private int m_MRP_PlannedOrder_ID = 0;

        public int GetVarMRP_PlannedOrder_ID()
        {
            return m_MRP_PlannedOrder_ID;
        }

        public void SetVarMRP_PlannedOrder_ID(int plannedID)
        {
            m_MRP_PlannedOrder_ID = plannedID;
        }

        /**
         * 	get Work Order using document no
         *	@param documentNo document number
         *	@return workorder
         */
        public static MVAMFGMWorkOrder GetWorkOrder(Ctx ctx, String documentNo, Trx trx)
        {
            MVAMFGMWorkOrder workorder = null;
            String sql = "SELECT * FROM VAMFG_M_WorkOrder WHERE DocumentNo = @param1";
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", documentNo);
                idr = DB.ExecuteReader(sql, param, null);
                dt.Load(idr);
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    workorder = new MVAMFGMWorkOrder(ctx, dt.Rows[i], trx);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                log.Log(Level.SEVERE, sql, e);
            }

            return workorder;
        }	//	MWorkOrder

        /// <summary>
        ///Copy Lines From other work order
        /// </summary>
        /// <param name="otherWorkOrder"></param>
        /// <returns></returns>
        public int CopyLinesFrom(MVAMFGMWorkOrder otherWorkOrder)
        {
            if (IsProcessed() || IsPosted() || otherWorkOrder == null)
                return 0;
            int count = 0;
            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder otherWorkOrder1 = new ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder(GetCtx(), otherWorkOrder.GetVAMFG_M_WorkOrder_ID(), null);
            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder thisRecord = new ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), null);
            MVAMFGMWorkOrderOperation[] fromOperations = MVAMFGMWorkOrderOperation.GetOfWorkOrder(otherWorkOrder1, null, null);
            foreach (MVAMFGMWorkOrderOperation fromOperation in fromOperations)
            {
                MVAMFGMWorkOrderOperation operation = new MVAMFGMWorkOrderOperation(GetCtx(), 0, Get_TrxName());
                CopyValues(fromOperation, operation, fromOperation.GetAD_Client_ID(), fromOperation.GetAD_Org_ID());
                operation.SetVAMFG_M_WorkOrder_ID(GetVAMFG_M_WorkOrder_ID());
                operation.setHeaderInfo(thisRecord);
                operation.Set_ValueNoCheck("VAMFG_M_WorkOrderOperation_ID", I_ZERO);

                if (operation.Save())
                    count++;

                // Copy Work Order Operation Component Lines
                MVAMFGMWorkOrderComponent[] fromComponents = MVAMFGMWorkOrderComponent.GetOfWorkOrderOperation(fromOperation, null, null);
                int linecount = 0;
                foreach (MVAMFGMWorkOrderComponent fromComponent in fromComponents)
                {
                    MVAMFGMWorkOrderComponent component = new MVAMFGMWorkOrderComponent(GetCtx(), 0, Get_TrxName());
                    CopyValues(fromComponent, component, fromComponent.GetAD_Client_ID(), fromComponent.GetAD_Org_ID());
                    component.SetVAMFG_M_WorkOrderOperation_ID(operation.GetVAMFG_M_WorkOrderOperation_ID());
                    component.SetHeaderInfo(operation);
                    component.Set_ValueNoCheck("VAMFG_M_WorkOrderComponent_ID", I_ZERO);	// new

                    if (component.Save())
                        linecount++;
                }
                if (fromComponents.Length != linecount)
                    log.Log(Level.SEVERE, "Component Line difference - From=" + fromComponents.Length + " <> Saved=" + linecount);

                // Copy Work Order Operation Resource Lines
                MVAMFGMWorkOrderResource[] fromResources = MVAMFGMWorkOrderResource.GetofWorkOrderOperation(fromOperation, null, null);
                linecount = 0;
                foreach (MVAMFGMWorkOrderResource fromResource in fromResources)
                {
                    MVAMFGMWorkOrderResource resource = new MVAMFGMWorkOrderResource(GetCtx(), 0, Get_TrxName());
                    CopyValues(fromResource, resource, fromResource.GetAD_Client_ID(), fromResource.GetAD_Org_ID());
                    resource.SetVAMFG_M_WorkOrderOperation_ID(operation.GetVAMFG_M_WorkOrderOperation_ID());
                    resource.SetHeaderInfo(operation);
                    resource.Set_ValueNoCheck("VAMFG_M_WorkOrderResource_ID", I_ZERO);

                    if (resource.Save())
                        linecount++;
                }
                if (fromResources.Length != linecount)
                    log.Severe("Resource Line difference - From = " + fromResources.Length + " <> Saved = " + linecount);
            }

            if (fromOperations.Length != count)
                log.Severe("Operation Line difference - From = " + fromOperations.Length + " <> Saved = " + count);

            return count;
        }	//	copyLinesFrom



        /**	Process Message 			*/
        private String m_processMsg = null;
        /** Call Prepared flag            */
        private bool m_callPrepared = false;

        /**
         * 	Approve Document
         * 	@return true if success 
         */
        public bool ApproveIt()
        {
            log.Info("approveIt - " + ToString());
            SetIsApproved(true);
            return true;
        }

        /**
         * 	Called before Save for Pre-Save Operation
         * 	@param newRecord new record
         *	@return true if record can be saved
         */

        protected override Boolean BeforeSave(bool newRecord)
        {


            //	Client/Org Check
            if (GetAD_Org_ID() == 0)
            {
                int context_AD_Org_ID = GetCtx().GetAD_Org_ID();
                if (context_AD_Org_ID != 0)
                {
                    SetAD_Org_ID(context_AD_Org_ID);
                    log.Warning("Changed Org to Context=" + context_AD_Org_ID);
                }
                else
                {
                    log.SaveError("Error", Msg.Translate(GetCtx(), "Org0NotAllowed"));
                    return false;
                }
            }
            if (GetAD_Client_ID() == 0)
            {
                //			m_processMsg = "AD_Client_ID = 0";
                return false;
            }

            if (newRecord)
            {
                VAdvantage.Model.MClient client = VAdvantage.Model.MClient.Get(GetCtx(), GetAD_Client_ID());
                VAdvantage.Model.MAcctSchema acctSchema = null;
                acctSchema = new VAdvantage.Model.MAcctSchema(Env.GetCtx(), client.GetAcctSchemaID(), null);

                if (!X_M_Cost.COSTINGMETHOD_StandardCosting.Equals(acctSchema.GetCostingMethod()))
                {
                    log.SaveWarning(Msg.GetMsg(GetCtx(), "NoStandardCosting"), "");
                }
            }

            // Product Check
            if (GetM_Product_ID() == 0)
                return false;

            // Prevent saving the WorkOrder if both dates are provided, before the user  prepares the document
            if (GetDocStatus().Equals(X_VAMFG_M_WorkOrder.DOCSTATUS_Drafted) && !m_callPrepared)
                if (GetVAMFG_DateScheduleFrom() != null && GetVAMFG_DateScheduleTo() != null)
                {
                    log.SaveError("Error", Msg.Translate(GetCtx(), "Enter either ScheduledDateFrom or ScheduledDateTo(Not Both, Scheduler will calculate the other date)"));
                    return false;
                }

            // Check dates
            if (GetVAMFG_DateActualFrom() != null && GetVAMFG_DateActualTo() != null)
            {
                //if (getDateActualTo().before(getDateActualFrom()))
                if (GetVAMFG_DateActualTo().Value.ToLocalTime() < (GetVAMFG_DateActualFrom().Value.ToLocalTime()))
                {
                    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@VAMFG_DateActualFrom@ > @VAMFG_DateActualTo@"));
                    return false;
                }
            }

            if (GetVAMFG_QtyEntered().CompareTo(Env.ZERO) < 0)
            {
                log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@VAMFG_QtyEntered@ < 0"));
                return false;
            }

            if (MLocator.Get(GetCtx(), GetM_Locator_ID()).GetM_Warehouse_ID() != GetM_Warehouse_ID())
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "WarehouseLocatorMismatch"));
                return false;
            }

            // get current cost from product cost on new record and when product changed
            // currency conversion also required if order has different currency with base currency
            if (newRecord || (Is_ValueChanged("M_Product_ID")))
            {
                if (GetM_Product_ID() > 0)
                {
                    decimal currentcostprice = MCost.GetproductCosts(GetAD_Client_ID(), GetAD_Org_ID(), GetM_Product_ID(), 0, Get_TrxName());
                    if (GetVAMFG_Description() != null && GetVAMFG_Description().Contains("(->"))
                    {
                        // not to set cuurent cost price on reversal because its already filed during creation of line
                    }
                    else
                    {
                        SetCurrentCostPrice(currentcostprice);
                    }
                }
            }

            if (Is_ValueChanged("VAMFG_QtyEntered"))
            {
                // BigDecimal qtyEntered = getQtyEntered().setScale(MUOM.getPrecision(getCtx(), getC_UOM_ID()), BigDecimal.ROUND_HALF_UP);
                Decimal qtyEntered = Decimal.Round((GetVAMFG_QtyEntered()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()));
                if (qtyEntered.CompareTo(GetVAMFG_QtyEntered()) != 0)
                {
                    log.Fine("Corrected QtyEntered Scale UOM=" + GetC_UOM_ID()
                            + "; QtyEntered =" + GetVAMFG_QtyEntered() + "->" + qtyEntered);
                    SetVAMFG_QtyEntered(qtyEntered);
                }
            }
            // if routing's attribute and work order's attribute does not match done by vivek on 09/01/2017 assigned by pradeep
            MVAMFGMRouting routing = new MVAMFGMRouting(GetCtx(), GetVAMFG_M_Routing_ID(), null);
            if (routing.GetM_AttributeSetInstance_ID() != GetM_AttributeSetInstance_ID())
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "VAMFG_RoutingMismatch"));
                return false;
            }

            MProduct product = new MProduct(GetCtx(), GetM_Product_ID(), Get_TrxName());
            Set_Value("GOM01_MaxDensity", product.Get_Value("GOM01_MaxDensity"));
            Set_Value("GOM01_MinDensity", product.Get_Value("GOM01_MinDensity"));

            if (GetGOM01_Density() < Util.GetValueOfDecimal(Get_Value("GOM01_MinDensity")) || GetGOM01_Density() > Util.GetValueOfDecimal(product.Get_Value("GOM01_MaxDensity")))
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_DensityRange"));
                return false;
            }


            return true;
            /** Prevents saving
            log.saveError("Error", Msg.parseTranslation(getCtx(), "@C_Currency_ID@ = @C_Currency_ID@"));
            log.saveError("FillMandatory", Msg.getElement(getCtx(), "PriceEntered"));
            /** Issues message
            log.saveWarning(AD_Message, message);
            log.saveInfo (AD_Message, message);
             **/
        }	//	beforeSave

        /**
         * 	Before Delete
         *	@return true of it can be deleted
         */

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

        /**
         * 	get Reversal WorkOrder using reversal document no in description
         *	@return workorder 
         */
        public MVAMFGMWorkOrder GetReversal()
        {
            String description = GetVAMFG_Description();
            if (description == null || description.Length == 0)
                return null;
            String s = description;
            int pos1 = 0;
            pos1 = s.IndexOf("<-)");
            if (pos1 == -1)
                return null;

            int pos2 = s.LastIndexOf("(", pos1);
            if (pos2 == -1)
                return null;
            String workorderDocNo = s.Substring(pos2 + 1, pos1);

            MVAMFGMWorkOrder reversal = GetWorkOrder(GetCtx(), workorderDocNo, Get_TrxName());
            return reversal;
        }

        /**
         * 	Set Processed.
         * 	Propagate to Lines/Taxes
         *	@param processed processed
         */

        public new void SetProcessed(bool processed)
        {
            base.SetProcessed(processed);

            if (Get_ID() == 0)
                return;
            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder wo1 = new CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            MVAMFGMWorkOrderOperation[] operations = MVAMFGMWorkOrderOperation.GetOfWorkOrder(wo1, null, "VAMFG_SeqNo");
            int numComponent = 0, numResource = 0;

            foreach (MVAMFGMWorkOrderOperation operation in operations)
            {
                operation.SetProcessed(true);
                String sql = "SET Processed='Y' WHERE VAMFG_M_WorkOrderOperation_ID =" + operation.Get_ID();
                numComponent += DB.ExecuteQuery("UPDATE VAMFG_M_WorkOrderComponent " + sql, null, Get_TrxName());
                numResource += DB.ExecuteQuery("UPDATE VAMFG_M_WorkOrderResource " + sql, null, Get_TrxName());
                operation.Save();
            }
            log.Fine(processed + " - Components = " + numComponent + ", Operations = " + operations.Length
                    + ", Resources = " + numResource);
        }	//	setProcessed


        public bool CloseIt()
        {

            //	Reverse any associated & incomplete Warehouse Tasks
            if (!ReverseTasks(true))
            {
                m_processMsg = Msg.GetMsg(GetCtx(), "CannotReverseTasks");
                return false;
            }
            log.Info(ToString());
            SetProcessed(true);
            SetDocAction(DOCACTION_None);
            return true;
        }	//	closeIt

        public String CompleteIt()
        {
            //	Std Period open?
            _countGOM01 = Convert.ToInt32(DB.ExecuteScalar("SELECT COUNT(AD_ModuleInfo_ID) FROM AD_ModuleInfo WHERE Prefix like 'GOM01_'"));
            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder wo1 = new CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            MVAMFGMWorkOrderOperation[] woos = MVAMFGMWorkOrderOperation.GetOfWorkOrder(wo1, null, " VAMFG_SeqNo DESC ");

            // check that enough components have been issued to complete the Work Order
            foreach (MVAMFGMWorkOrderOperation woo in woos)
            {

                MVAMFGMWorkOrderComponent[] wocs = MVAMFGMWorkOrderComponent.GetOfWorkOrderOperation(woo, null, null);
                foreach (MVAMFGMWorkOrderComponent woc in wocs)
                {
                    if (woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_AssemblyPull) || woc.GetVAMFG_SupplyType().Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_OperationPull))
                        continue; // no need to check, should be correct and the exact amount by default
                    // Added by Bharat for Gulf Oil
                    if (_countGOM01 == 0)
                    {
                        if (woc.GetVAMFG_QtySpent().CompareTo(woc.GetVAMFG_QtyAvailable()) > 0)
                        {
                            m_processMsg = "@NotEnoughQty@";
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                }

                // No check for Resource Usage, since
                // 1. Automatic Resources are automatically charged during work order movements
                // 2. Manual Resources are charged directly by the User.
            }

            // For Repair/Refurbish Work Orders check that Product Assemblies issued equals Quantity Assembled
            if (!GetVAMFG_WOType().Equals(VAMFG_WOTYPE_Standard))
            {
                String sql = "SELECT Sum(VAMFG_QtyAvailable) FROM VAMFG_M_WorkOrderComponent WHERE VAMFG_M_WorkOrderOperation_ID IN " +
                "(SELECT VAMFG_M_WorkOrderOperation_ID FROM VAMFG_M_WorkOrder WHERE VAMFG_M_WorkOrder_ID = @param1) " +
                "AND M_Product_ID = @param2";
                Decimal Qty = Decimal.Zero;
                SqlParameter[] param = null;
                IDataReader idr = null;
                DataTable dt = new DataTable();
                //PreparedStatement pstmt = DB.prepareStatement(sql, get_TrxName());
                //ResultSet rs = null;
                try
                {
                    param = new SqlParameter[2];
                    param[0] = new SqlParameter("@param1", GetVAMFG_M_WorkOrder_ID());
                    param[1] = new SqlParameter("@param2", GetM_Product_ID());
                    idr = DB.ExecuteReader(sql.ToString(), param, null);
                    if (idr.Read())
                    {
                        Qty = VAdvantage.Utility.Util.GetValueOfDecimal(idr[0]);
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
                    //e.printStackTrace();
                    // e.StackTrace.ToString();
                }
                // Added by Bharat for Gulf Oil
                if (_countGOM01 == 0)
                {
                    if (Qty.CompareTo(Decimal.Subtract(GetVAMFG_QtyAssembled(), (GetVAMFG_QtyAvailable()))) < 0)
                    {
                        m_processMsg = "@NotEnoughQty@";
                        log.Severe("Enough Product Assemblies have not been issued to complete Work Order - " + GetVAMFG_M_WorkOrder_ID()
                                + "; Required - " + Qty + ", Issued - " + Decimal.Subtract(GetVAMFG_QtyAssembled(), (GetVAMFG_QtyAvailable())));
                        return DocActionConstants.STATUS_Invalid;
                    }
                }
            }


            // Check that at least 1 WOTxn exist
            //and
            // Check all WOTxns are either Completed, Closed, Voided or Reversed
            // Don't check for Reversal Documents
            if (!GetDocumentNo().EndsWith("^"))
            {
                MVAMFGMWrkOdrTransaction[] WOTxns = MVAMFGMWrkOdrTransaction.GetOfWorkOrder(this, null, null);
                if (WOTxns == null || 0 == WOTxns.Length)
                {
                    m_processMsg = "@NoWODetails@";
                    return DocActionConstants.STATUS_Invalid;
                }
                else if (WOTxns != null)
                {
                    log.Fine("WOTxns #" + WOTxns.Length);
                    String status;
                    foreach (MVAMFGMWrkOdrTransaction element in WOTxns)
                    {
                        status = element.GetDocStatus();
                        if (DOCSTATUS_Reversed.Equals(status) ||
                                DOCSTATUS_Voided.Equals(status) ||
                                DOCSTATUS_Closed.Equals(status) ||
                                DOCSTATUS_Completed.Equals(status))
                            continue;  // transaction is already voided/reversed/completed
                        else
                        {
                            m_processMsg = "@WOTxnsNotCompleted@";
                            return DocActionConstants.STATUS_Invalid;
                        }
                    }
                }
            }
            SetDocAction(DOCACTION_Close);
            return DocActionConstants.STATUS_Completed;
        }	//	completeIt



        public FileInfo CreatePDF()
        {
            //try
            //{
            //    File temp = File.createTempFile(Get_TableName() + Get_ID() + "_", ".pdf");
            //    return CreatePDF(temp);
            //}
            //catch (Exception e)
            //{
            //    log.Severe("Could not create PDF - " + e.Message + ", for Work Order - " + GetM_WorkOrder_ID());
            //}
            return null;
        }	//	createPDF

        /**
         * 	Create PDF file
         *	@param file output file
         *	@return file if success
         */
        public FileInfo CreatePDF(FileInfo file)
        {
            //	ReportEngine re = ReportEngine.get (getCtx(), ReportEngine.INVOICE, getC_Invoice_ID());
            //	if (re == null)
            return null;
            //	return re.getPDF(file);
        }	//	createPDF

        /**
         * 	get Document Approval Amount
         *	@return amount
         */
        public Decimal GetApprovalAmt()
        {
            //return null;
            return Decimal.Zero;
        }	//	getApprovalAmt

        public int GetC_Currency_ID()
        {
            return 0;
        }	//	getC_Currency_ID

        /**
         * 	get Document Owner (Responsible)
         *	@return AD_User_ID
         */
        public int GetDoc_User_ID()
        {
            if (GetSalesRep_ID() != 0)
                return GetSalesRep_ID();
            else
                return GetCreatedBy();
        }	//	getDoc_User_ID

        /**
         * 	get Document Info
         *	@return document info (untranslated)
         */
        public String GetDocumentInfo()
        {
            VAdvantage.Model.MDocType dt = VAdvantage.Model.MDocType.Get(GetCtx(), GetC_DocType_ID());
            return dt.GetName() + " " + GetDocumentNo();
        }	//	getDocumentInfo

        /**
         * 	get Process Message
         *	@return clear text error message
         */
        public String GetProcessMsg()
        {
            return m_processMsg;
        }	//	getProcessMsg

        /**
         * 	get Summary
         *	@return Summary of Document
         */
        public String GetSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetDocumentNo());
            sb.Append(": ");

            VAdvantage.Model.MProduct product = new VAdvantage.Model.MProduct(GetCtx(), GetM_Product_ID(), null);
            sb.Append(product.GetName());

            if (GetVAMFG_Description() != null && GetVAMFG_Description().Length > 0)
                sb.Append(" - ").Append(GetVAMFG_Description());
            return sb.ToString();
        }	//	getSummary

        /**
         * 	Invalidate Document
         * 	@return true if success ok 
         */
        public bool InvalidateIt()
        {
            log.Info(ToString());
            SetDocAction(DOCACTION_Prepare);
            return true;
        }	//	invalidateIt

        /**
         *	Prepare Document
         * 	@return new status (In Progress or Invalid) 
         */
        public String PrepareIt()
        {
            String docStatus = GetDocStatus();

            MDocType dt = MDocType.Get(GetCtx(), GetC_DocType_ID());

            if (!MPeriod.IsOpen(GetCtx(), GetVAMFG_DateAcct(), dt.GetDocBaseType()))
            {
                _processMsg = "@PeriodClosed@";
                return DocActionVariables.STATUS_INVALID;
            }

            //	since all transactions against WO are related to a Operation
            //	checking for existence of at least 1 WO Operation
            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder wo1 = new CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
            MVAMFGMWorkOrderOperation[] woos = MVAMFGMWorkOrderOperation.GetOfWorkOrder(wo1, null, "VAMFG_SeqNo");
            if (woos == null || 0 == woos.Length)
            {
                m_processMsg = "@NoWOOperations@";
                return docStatus;
            }

            // Simplify WO Txn design by preventing the first and the final operation from being optional
            if (woos[woos.Length - 1].IsVAMFG_IsOptional() || woos[0].IsVAMFG_IsOptional())
            {
                m_processMsg = "@LastOperation@";
                return docStatus;
            }

            // isSOTrx was intentionally set to true earlier so that query 
            // on Sales Order will run without user intervention
            SetIsSOTrx(false);

            // Prevent updates to the WO after it is prepared
            SetProcessed(true);

            //set the Quantity Queued in 1st operation as Work Order Quantity
            if (woos.Length > 0 &&
                    (GetDocStatus().Equals(DocActionConstants.STATUS_Drafted) || GetDocStatus().Equals(DocActionConstants.STATUS_Invalid)))
            {
                Decimal qty = Decimal.Zero;
                foreach (MVAMFGMWorkOrderOperation woo in woos)
                    //qty = qty.add(woo.GetVAMFG_QtyAssembled().add(woo.GetVAMFG_QtyAssembled()).add(woo.GetVAMFG_QtyQueued()).add(woo.GetVAMFG_QtyRun()));
                    qty = Decimal.Add(qty, Decimal.Add(woo.GetVAMFG_QtyAssembled(), Decimal.Add((woo.GetVAMFG_QtyAssembled()), Decimal.Add((woo.GetVAMFG_QtyQueued()), (woo.GetVAMFG_QtyRun())))));
                qty = Decimal.Add(qty, (GetVAMFG_QtyAssembled()));
                if (qty.CompareTo(Decimal.Zero) == 0)
                {
                    // Commented by Bharat on 11 Jan 2018 as discussed with Pradeep.
                    //woos[0].SetVAMFG_QtyQueued(GetVAMFG_QtyEntered());   
                    //woos[0].Save();
                }
            }

            // Run Scheduler	
            // If both the dates (DateScheduleFrom, DateScheduleTo) are not provided then Scheduler will not run
            // If both the dates are provided then it throws an error
            // If either of the dates are provided, then Scheduler runs and calculates the other date 
            // Run only in drafted/invalid status
            if ((GetVAMFG_DateScheduleFrom() != null || GetVAMFG_DateScheduleTo() != null) &&
                    (GetDocStatus().Equals(DocActionConstants.STATUS_Drafted) || GetDocStatus().Equals(DocActionConstants.STATUS_Invalid)))
            {
                bool getparameters = true;

                // Changes done by Bharat on 26 Dec 2017 as the Process was not Available in DataBase
                //int AD_Process_ID = VAdvantage.Model.MProcess.GetIDByValue(GetCtx(), "VAMFG_M_WorkOrderScheduler");

                int AD_Process_ID = VAdvantage.Model.MProcess.GetIDByValue(GetCtx(), "VAMFG_WorkOrderSchedular");
                MPInstance instance = new MPInstance(GetCtx(), AD_Process_ID, 0);
                if (!instance.Save())
                {
                    m_processMsg = "@RunWOScheduler@";
                    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "Error while retrieving the WorkOrderScheduler instance"));
                }
                else
                {
                    // Change Process Code written here instead of calling Process
                    //ProcessInfo pi = new ProcessInfo("", AD_Process_ID);
                    //pi.SetAD_PInstance_ID(instance.GetAD_PInstance_ID());
                    //pi.SetAD_Client_ID(GetAD_Client_ID());

                    ////      Add Parameters
                    //MPInstancePara para1 = new MPInstancePara(instance, 10);
                    //para1.setParameter("VAMFG_M_WorkOrder_ID", this.GetVAMFG_M_WorkOrder_ID());
                    //if (!para1.Save())
                    //{
                    //    getparameters = false;
                    //}

                    //MPInstancePara para2 = new MPInstancePara(instance, 20);
                    //String dateScheduledFrom = null;
                    //if (GetVAMFG_DateScheduleFrom() != null)
                    //    dateScheduledFrom = GetVAMFG_DateScheduleFrom().ToString();
                    //para2.setParameter("DateScheduleFrom", dateScheduledFrom);
                    //if (!para2.Save())
                    //{
                    //    getparameters = false;
                    //}

                    //MPInstancePara para3 = new MPInstancePara(instance, 30);
                    //String dateScheduledTo = null;
                    //if (GetVAMFG_DateScheduleTo() != null)
                    //    dateScheduledTo = GetVAMFG_DateScheduleTo().ToString();
                    //para3.setParameter("DateScheduleTo", dateScheduledTo);
                    //if (!para3.Save())
                    //{
                    //    getparameters = false;
                    //}

                    //if (!getparameters)
                    //{
                    //    m_processMsg = "@RunWOScheduler@";
                    //    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "While saving the parameters"));
                    //}
                    //else
                    //{
                    //    //ProcessCtl worker = new ProcessCtl(this, pi, null);
                    //    //worker.Run();    

                    //    ProcessCtl woSchedule;
                    //    woSchedule = new ProcessCtl(null, pi, null);  // batch start                               
                    //    woSchedule.Run();		// Parallel start of scheduler. Use Run() for sequential.	
                    //    //get results
                    //    if (pi.IsError())
                    //    {
                    //        //does not hit this code if start() is used for the process.
                    //        //log gets generated but the user will not see the message on the UI.
                    //        //if run() is used, the message is shown in UI.
                    //        //set the message from Process Info as the process message.
                    //        m_processMsg = pi.GetSummary();
                    //    }
                    //}


                    DateTime? dateScheduledFrom = null;
                    if (GetVAMFG_DateScheduleFrom() != null)
                        dateScheduledFrom = GetVAMFG_DateScheduleFrom();

                    DateTime? dateScheduledTo = null;
                    if (GetVAMFG_DateScheduleTo() != null)
                        dateScheduledTo = GetVAMFG_DateScheduleTo();

                    if (this.GetVAMFG_M_WorkOrder_ID() == 0)
                    {
                        log.Warning(Msg.ParseTranslation(GetCtx(), "@FillMandatory@ @M_WorkOrder_ID@"));
                        throw new Exception(Msg.ParseTranslation(GetCtx(), "@FillMandatory@ @M_WorkOrder_ID@"));
                    }

                    if (dateScheduledFrom == null && dateScheduledTo == null)
                    {
                        log.Warning(Msg.ParseTranslation(GetCtx(), "@FillMandatory@ - Enter either @DateScheduleTo@ or @DateScheduleFrom@"));
                        throw new Exception(Msg.ParseTranslation(GetCtx(), "@FillMandatory@ - Enter either @DateScheduleTo@ or @DateScheduleFrom@"));
                    }
                    if (!SchedulerSetDate(this.GetVAMFG_M_WorkOrder_ID(), dateScheduledFrom, dateScheduledTo))
                    {
                        log.Warning(Msg.ParseTranslation(GetCtx(), "@RunWOScheduler@"));
                        throw new Exception(Msg.ParseTranslation(GetCtx(), "@RunWOScheduler@"));
                    }
                    //return null;
                }
            }

            if (!DOCACTION_Complete.Equals(GetDocAction()))
                SetDocAction(DOCACTION_Complete);
            return DocActionConstants.STATUS_InProgress;
        }

        /** 
         * 	Re-activate
         * 	@return true if success 
         */
        public bool ReActivateIt()
        {
            log.Info(ToString());

            //	Std Period open?
            m_processMsg = DocumentEngine.IsPeriodOpen(this);
            if (m_processMsg != null)
                return false;

            SetDocAction(DOCACTION_Complete);

            return true;
        }	//	reActivateIt

        /**
         * 	Reject Approval
         * 	@return true if success 
         */
        public bool RejectIt()
        {
            log.Info("rejectIt - " + ToString());
            SetIsApproved(false);
            return true;
        }

        /**
         * 	Reverse Accrual - none
         * 	@return true if success 
         */
        public bool ReverseAccrualIt()
        {
            log.Info(ToString());
            return false;
        }

        /**
         * 	Reverse Correction
         * 	@return true if success 
         */
        public bool ReverseCorrectIt()
        {
            log.Info(ToString());
            return false;
        }

        /**
         * 	String Representation
         *	@return info
         */

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("MVAMFGMWorkOrder[")
            .Append(Get_ID()).Append("-").Append(GetDocumentNo())
            .Append(",C_DocType_ID=").Append(GetC_DocType_ID())
            .Append(", M_Product_ID=").Append(GetM_Product_ID())
            .Append(", VAMFG_QtyEntered=").Append(GetVAMFG_QtyEntered())
            .Append("]");
            return sb.ToString();
        }	//	ToString

        /**
         * 	Unlock Document.
         * 	@return true if success 
         */
        public bool UnlockIt()
        {
            log.Info(ToString());
            SetProcessing(false);
            return true;
        }	//	unlockIt

        public bool VoidIt()
        {
            log.Info(ToString());
            if (DOCSTATUS_Closed.Equals(GetDocStatus())
                    || DOCSTATUS_Voided.Equals(GetDocStatus()))
            {
                m_processMsg = "Document Closed: " + GetDocStatus();
                SetDocAction(DOCACTION_None);
                return false;
            }

            // Commented As No warehouse task Tables are there in this module

            //	Reverse any associated & incomplete Warehouse Tasks
            //if (!ReverseTasks(true))
            //{
            //    m_processMsg = Msg.GetMsg(GetCtx(), "CannotReverseTasks");
            //    return false;
            //}

            //	Not Processed
            if (DOCSTATUS_Drafted.Equals(GetDocStatus())
                    || DOCSTATUS_Invalid.Equals(GetDocStatus())
                    || DOCSTATUS_Approved.Equals(GetDocStatus())
                    || DOCSTATUS_NotApproved.Equals(GetDocStatus()))
            {
                AddDescription(Msg.GetMsg(GetCtx(), "Voided") + " (" + GetVAMFG_QtyEntered() + ")");
                SetVAMFG_QtyEntered(Env.ZERO);
                SetVAMFG_QtyAvailable(Env.ZERO);
                SetVAMFG_QtyAssembled(Env.ZERO);

                MVAMFGMWrkOdrTransaction[] WOTxns = MVAMFGMWrkOdrTransaction.GetOfWorkOrder(this, " ParentWorkOrderTxn_ID IS NULL ", " VAMFG_M_WrkOdrTransaction_ID DESC ");
                if (WOTxns != null)
                {
                    log.Fine("WOTxns #" + WOTxns.Length);
                    for (int i = 0; i < WOTxns.Length; i++)
                    {
                        if (DOCSTATUS_Reversed.Equals(WOTxns[i].GetDocStatus()) ||
                                DOCSTATUS_Voided.Equals(WOTxns[i].GetDocStatus()))
                            continue;  // transaction is already voided/reversed
                        WOTxns[i].Set_TrxName(Get_TrxName());
                        WOTxns[i].SetDocAction(DocActionConstants.ACTION_Void);
                        if (!DocumentEngine.ProcessIt(WOTxns[i], DocActionConstants.ACTION_Void))
                        {
                            m_processMsg = "WOTxn Void erorr: " + WOTxns[i].GetProcessMsg();
                            return false;
                        }
                        if (!WOTxns[i].Save(Get_TrxName()))
                        {
                            m_processMsg = "Could not save work order transaction void";
                            return false;
                        }
                    }
                }

            }
            else
            {
                //	Std Period open?
                DateTime? dateAcct = GetVAMFG_DateAcct();
                m_processMsg = DocumentEngine.IsPeriodOpen(this);
                if (m_processMsg != null)
                {
                    log.Log(Level.SEVERE, m_processMsg);
                    return false;
                }

                //	Create Reversal
                MVAMFGMWorkOrder reversal = new MVAMFGMWorkOrder(GetCtx(), 0, Get_TrxName());
                CopyValues(this, reversal);
                reversal.SetClientOrg(this);
                reversal.SetC_Order_ID(0);
                reversal.SetVAMFG_DateAcct(dateAcct);
                //
                reversal.SetDocumentNo(GetDocumentNo() + REVERSE_INDICATOR);	//	indicate reversals
                reversal.SetDocStatus(DOCSTATUS_InProgress); // make this wo InProgress so that the reversal txns can proceed
                reversal.SetDocAction(DOCACTION_Complete);
                //
                reversal.SetIsApproved(true);
                reversal.SetProcessing(false);
                reversal.SetProcessed(false);
                reversal.SetPosted(false);
                reversal.SetVAMFG_Description(GetVAMFG_Description());
                reversal.AddDescription("(->" + GetDocumentNo() + ")");
                if (!reversal.Save(Get_TrxName()))
                {
                    m_processMsg = "Could not save work order reversal";
                    return false;
                }
                reversal.CopyLinesFrom(this);
                if (!reversal.Save(Get_TrxName()))
                {
                    m_processMsg = "Could not save work order detail reversal ";
                    return false;
                }

                /****************** Commented Bcoz It do not allow to Reverse Transactions
                // Save reverse status and reversal document no in description to database 
                // so that workordertxn reversal can access these
                SetDocStatus(DOCSTATUS_Reversed);
                AddDescription("(" + reversal.GetDocumentNo() + "<-)");
                Save(Get_TrxName());
                 * *************************/


                MVAMFGMWrkOdrTransaction[] WOTxns = MVAMFGMWrkOdrTransaction.GetOfWorkOrder(this, " ParentWorkOrderTxn_ID IS NULL ", " VAMFG_M_WrkOdrTransaction_ID DESC ");
                if (WOTxns != null)
                {
                    log.Fine("WOTxns #" + WOTxns.Length);
                    for (int i = 0; i < WOTxns.Length; i++)
                    {
                        if (DOCSTATUS_Reversed.Equals(WOTxns[i].GetDocStatus()) ||
                                DOCSTATUS_Voided.Equals(WOTxns[i].GetDocStatus()))
                            continue;  // transaction is already voided/reversed
                        WOTxns[i].Set_TrxName(Get_TrxName());
                        WOTxns[i].SetDocAction(DocActionConstants.ACTION_Void);
                        if (!DocumentEngine.ProcessIt(WOTxns[i], DocActionConstants.ACTION_Void))
                        {
                            m_processMsg = "WOTxn Reversal error: " + WOTxns[i].GetProcessMsg();
                            return false;
                        }
                        if (!WOTxns[i].Save(Get_TrxName()))
                        {
                            m_processMsg = "Could not save work order transaction reversal";
                            return false;
                        }
                    }
                }

                // Save reverse status and reversal document no in description to database 
                // so that workordertxn reversal can access these
                SetDocStatus(DOCSTATUS_Reversed);
                AddDescription("(" + reversal.GetDocumentNo() + "<-)");
                Save(Get_TrxName());

                //	Post Reversal
                if (!DocumentEngine.ProcessIt(reversal, DocActionConstants.ACTION_Complete))
                {
                    m_processMsg = "WO Reversal error: " + reversal.GetProcessMsg();
                    return false;
                }
                DocumentEngine.ProcessIt(reversal, DocActionConstants.ACTION_Close);
                reversal.SetDocStatus(DOCSTATUS_Reversed);
                reversal.SetDocAction(DOCACTION_None);
                reversal.Save(Get_TrxName());
            }

            SetProcessed(true);
            SetDocAction(DOCACTION_None);
            return true;
        }	//	voidIt

        /**
         * 	Add to Description
         *	@param description text
         */
        public void AddDescription(String description)
        {
            String desc = GetVAMFG_Description();
            if (desc == null)
                SetVAMFG_Description(description);
            else
                SetVAMFG_Description(desc + " | " + description);
        }	//	addDescription

        /**
         * Set Warehouse - Callout : Clears the Locator field
         * @param oldM_Warehouse_ID
         * @param newM_Warehouse_ID
         * @param windowNo
         * @throws Exception
         */
        public void SetM_Warehouse_ID(String oldM_Warehouse_ID,
                 String newM_Warehouse_ID, int windowNo)
        {

            if (newM_Warehouse_ID == null || 0 == newM_Warehouse_ID.Trim().Length)
            {
                Set_ValueNoCheck("M_Locator_ID", null);
                return;
            }

            int M_Warehouse_ID = VAdvantage.Utility.Util.GetValueOfInt(newM_Warehouse_ID);

            if (0 == M_Warehouse_ID)
                Set_ValueNoCheck("M_Locator_ID", null);
            else
                SetM_Locator_ID(VAdvantage.Model.MWarehouse.Get(GetCtx(), GetM_Warehouse_ID()).GetDefaultM_Locator_ID());

            return;
        }

        /**
         * 	Set Org - Callout
         *	@param oldAD_Org_ID old org
         *	@param newAD_Org_ID new org
         *	@param windowNo window no
         */
        public void SetAD_Org_ID(String oldAD_Org_ID,
                  String newAD_Org_ID, int windowNo)
        {
            if (newAD_Org_ID == null || newAD_Org_ID.Length == 0)
            {
                Set_ValueNoCheck("M_Warehouse_ID", null);
                Set_ValueNoCheck("VAMFG_M_WorkOrderClass_ID", null);
                return;
            }

            int AD_Org_ID = VAdvantage.Utility.Util.GetValueOfInt(newAD_Org_ID);

            if (GetM_Warehouse_ID() != 0)
            {
                VAdvantage.Model.MWarehouse warehouse = VAdvantage.Model.MWarehouse.Get(GetCtx(), GetM_Warehouse_ID());
                if (warehouse.GetAD_Org_ID() != AD_Org_ID)
                    Set_ValueNoCheck("M_Warehouse_ID", null);
            }

            Set_ValueNoCheck("VAMFG_M_WorkOrderClass_ID", null);
            DefaultWorkOrderClass(GetAD_Client_ID(), AD_Org_ID, GetVAMFG_WOType(), windowNo);

        }	//	setAD_Org_ID

        /**
         * 	Set WOSource - Callout
         *	@param oldWOSource old WOSource
         *	@param newWOSource new WOSource
         *	@param windowNo window no
         */
        public void SetWOSource(String oldWOSource,
                   String newWOSource, int windowNo)
        {
            if (newWOSource == null || newWOSource.Length == 0)
            {
                // if WOSource is null, set order and orderline to zero
                SetC_Order_ID(0);
                SetC_OrderLine_ID(0);
            }
        }	//	setWOSource

        /**
         * 	Set WOType - Callout
         *	@param oldWOType
         *	@param newWOType
         *	@param windowNo window no
         */
        public void SetWOType(String oldWOType,
                   String newWOType, int windowNo)
        {
            if (newWOType == null || newWOType.Length == 0)
            {
                // if WOType is null, set the Work Order Class to null as well.
                Set_ValueNoCheck("VAMFG_M_WorkOrderClass_ID", null);
                return;
            }

            Set_ValueNoCheck("VAMFG_M_WorkOrderClass_ID", null);
            DefaultWorkOrderClass(GetAD_Client_ID(), GetAD_Org_ID(), newWOType, windowNo);


        }	//	setWOType


        private void DefaultWorkOrderClass(int AD_Client_ID, int AD_Org_ID, String woType, int windowNo)
        {
            // Check for the existence of a default work order class.
            String sql = "SELECT VAMFG_M_WorkOrderClass_ID FROM VAMFG_M_WorkOrderClass WHERE" +
            " AD_Org_ID IN (0, @param1) AND AD_Client_ID = @param2" + // 1..2
            " AND VAMFG_WOTYPE = @param3 AND VAMFG_ISDEFAULT = 'Y' AND IsActive = 'Y' ORDER BY AD_Org_ID DESC"; // 3
            //PreparedStatement pstmt = DB.prepareStatement(sql, get_TrxName());
            //ResultSet rs = null;
            SqlParameter[] param = null;
            IDataReader idr = null;

            try
            {

                param = new SqlParameter[3];
                param[0] = new SqlParameter("@param1", AD_Org_ID);
                param[1] = new SqlParameter("@param2", AD_Client_ID);
                param[2] = new SqlParameter("@param3", woType);
                idr = DB.ExecuteReader(sql, param, null);
                if (idr.Read())
                {
                    SetVAMFG_M_WorkOrderClass_ID(VAdvantage.Utility.Util.GetValueOfInt(idr[0]));
                    SetM_WorkOrderClass_ID(null, VAdvantage.Utility.Util.GetValueOfString(idr[0]), windowNo);
                }
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                //pstmt.setInt(1, AD_Org_ID);
                //pstmt.setInt(2, AD_Client_ID);
                //pstmt.setString(3, woType);
                //rs = pstmt.executeQuery();

                //if (rs.next())
                //{
                //    setM_WorkOrderClass_ID(rs.getInt(1));
                //    setM_WorkOrderClass_ID(null, String.valueOf(rs.getInt(1)), windowNo);
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

                log.Log(Level.SEVERE, sql, e);

            }
            // defaultWorkOrderClass
        }
        /**
         * 	Set M_WorkOrderClass_ID - Callout
         *	@param oldM_WorkOrderClass_ID
         *	@param newM_WorkOrderClass_ID
         *	@param windowNo window no
         */
        public void SetM_WorkOrderClass_ID(String oldM_WorkOrderClass_ID,
                    String newM_WorkOrderClass_ID, int windowNo)
        {
            if (newM_WorkOrderClass_ID == null || newM_WorkOrderClass_ID.Length == 0)
            {
                Set_ValueNoCheck("C_DocType_ID", null);
                return;
            }
            int M_WorkOrderClass_ID = VAdvantage.Utility.Util.GetValueOfInt(newM_WorkOrderClass_ID);
            if (M_WorkOrderClass_ID == 0)
            {
                Set_ValueNoCheck("C_DocType_ID", null);
                return;
            }

            // Populate the Document Type
            MVAMFGMWorkOrderClass woc = new MVAMFGMWorkOrderClass(GetCtx(), M_WorkOrderClass_ID, Get_TrxName());
            SetC_DocType_ID(VAdvantage.Utility.Util.GetValueOfString(GetC_DocType_ID()), VAdvantage.Utility.Util.GetValueOfString(woc.GetWO_DocType_ID()), windowNo);
        }	//	setM_WorkOrderClass_ID

        /**
         * 	Set Order - Callout
         *	@param oldC_Order_ID old Order
         *	@param newC_Order_ID new Order
         *	@param windowNo window no
         */
        public void SetC_Order_ID(String oldC_Order_ID,
                   String newC_Order_ID, int windowNo)
        {
            // If Order is changed, reset order line to zero
            SetC_OrderLine_ID(0);
            Set_ValueNoCheck("M_Product_ID", null);
            Set_ValueNoCheck("M_BOM_ID", null);

            if (newC_Order_ID == null || newC_Order_ID.Length == 0)
                return;
            int C_Order_ID = VAdvantage.Utility.Util.GetValueOfInt(newC_Order_ID);
            if (C_Order_ID == 0)
                return;
            // If Order is populated, default BP and BP location.
            VAdvantage.Model.MOrder order = new VAdvantage.Model.MOrder(GetCtx(), C_Order_ID, null);
            SetC_BPartner_ID(order.GetC_BPartner_ID());
            SetC_BPartner_Location_ID(order.GetC_BPartner_Location_ID());
            SetAD_User_ID(order.GetAD_User_ID());
            SetVAMFG_PriorityRule(order.GetPriorityRule());
            SetC_Project_ID(order.GetC_Project_ID());
            SetC_Campaign_ID(order.GetC_Campaign_ID());
            SetC_Activity_ID(order.GetC_Activity_ID());
            SetAD_OrgTrx_ID(order.GetAD_OrgTrx_ID());
            SetVAMFG_User1_ID(order.GetUser1_ID());
            SetVAMFG_User2_ID(order.GetUser2_ID());
        }	//	setC_Order_ID	

        /**
         * 	Set Order Line - Callout
         *	@param oldC_OrderLine_ID old OrderLine
         *	@param newC_OrderLine_ID new OrderLine
         *	@param windowNo window no
         */
        public void SetC_OrderLine_ID(String oldC_OrderLine_ID,
                   String newC_OrderLine_ID, int windowNo)
        {
            Set_ValueNoCheck("M_Product_ID", null);
            Set_ValueNoCheck("M_BOM_ID", null);

            if (newC_OrderLine_ID == null || newC_OrderLine_ID.Length == 0)
                return;
            int C_OrderLine_ID = VAdvantage.Utility.Util.GetValueOfInt(newC_OrderLine_ID);
            if (C_OrderLine_ID == 0)
                return;
            // If Order Line is populated, default product and qty.
            VAdvantage.Model.MOrderLine line = new VAdvantage.Model.MOrderLine(GetCtx(), C_OrderLine_ID, null);
            SetM_Product_ID(line.GetM_Product_ID());
            SetM_Product_ID(line.GetM_Product_ID());
            SetM_Product_ID(null, VAdvantage.Utility.Util.GetValueOfString(line.GetM_Product_ID()), windowNo);
            SetVAMFG_QtyEntered(line.GetQtyEntered());
        }	//	setC_OrderLine_ID	

        /**
         * 	Set Product - Callout
         *	@param oldM_Product_ID old Order
         *	@param newM_Product_ID new Order
         *	@param windowNo window no
         */
        public void SetM_Product_ID(String oldM_Product_ID,
                    String newM_Product_ID, int windowNo)
        {
            // If Product is set to null, reset BOM.
            if (newM_Product_ID == null || newM_Product_ID.Length == 0)
            {
                Set_ValueNoCheck("M_BOM_ID", null);
                Set_ValueNoCheck("C_UOM_ID", null);
                Set_ValueNoCheck("VAMFG_M_Routing_ID", null);
                return;
            }
            int M_Product_ID = VAdvantage.Utility.Util.GetValueOfInt(newM_Product_ID);
            if (M_Product_ID == 0)
            {
                Set_ValueNoCheck("M_BOM_ID", null);
                Set_ValueNoCheck("C_UOM_ID", null);
                Set_ValueNoCheck("VAMFG_M_Routing_ID", null);
                return;
            }
            // If Product is populated, default current active, master BOM.
            String restriction = "BOMType='" + VAdvantage.Model.X_M_BOM.BOMTYPE_CurrentActive + "' AND BOMUse='" + VAdvantage.Model.X_M_BOM.BOMUSE_Manufacturing
            + "' AND IsActive = 'Y'";
            VAdvantage.Model.MBOM[] boms = VAdvantage.Model.MBOM.GetOfProduct(GetCtx(), M_Product_ID, null, restriction);
            if (boms.Length != 0)
            {
                VAdvantage.Model.MBOM bom = boms[0];
                SetM_BOM_ID(bom.GetM_BOM_ID());
            }
            else
                Set_ValueNoCheck("M_BOM_ID", null);

            // If Product is populated, default the Default Routing.
            MVAMFGMRouting routing = null;
            if (GetM_Warehouse_ID() != 0)
                routing = MVAMFGMRouting.GetDefaultRouting(GetCtx(), M_Product_ID, GetM_Warehouse_ID());
            if (routing != null)
                SetVAMFG_M_Routing_ID(routing.GetVAMFG_M_Routing_ID());
            else
                Set_ValueNoCheck("VAMFG_M_Routing_ID", null);

            // Set UOM from Product
            VAdvantage.Model.MProduct product = new VAdvantage.Model.MProduct(Env.GetCtx(), M_Product_ID, null);
            SetC_UOM_ID(product.GetC_UOM_ID());

        }	//	setM_Product_ID	

        /**
         * 	Set Document Type.
         * 	Sets DocumentNo
         * 	@param oldC_DocType_ID old ID
         * 	@param newC_DocType_ID new ID
         * 	@param windowNo window
         */
        public void SetC_DocType_ID(String oldC_DocType_ID,
                String newC_DocType_ID, int windowNo)
        {
            if (VAdvantage.Utility.Util.IsEmpty(newC_DocType_ID))
                return;
            int C_DocType_ID = VAdvantage.Utility.Util.GetValueOfInt(newC_DocType_ID);
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
                oldDocType_ID = VAdvantage.Utility.Util.GetValueOfInt(oldC_DocType_ID);

            String sql = "SELECT d.DocBaseType, d.IsDocNoControlled,"
                + " s.CurrentNext, s.CurrentNextSys, s.AD_Sequence_ID "
                + "FROM C_DocType d"
                + " LEFT OUTER JOIN AD_Sequence s ON (d.DocNoSequence_ID=s.AD_Sequence_ID)"
                + "WHERE C_DocType_ID=@param1";		//	1
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            try
            {
                int AD_Sequence_ID = 0;

                //	get old AD_SeqNo for comparison
                if (!newDocNo && oldDocType_ID != 0)
                {
                    param = new SqlParameter[1];
                    param[0] = new SqlParameter("@param1", oldDocType_ID);
                    idr = DB.ExecuteReader(sql.ToString(), param, null);
                    if (idr.Read())
                    {
                        AD_Sequence_ID = VAdvantage.Utility.Util.GetValueOfInt(idr[4]);
                    }
                    if (idr != null)
                    {
                        idr.Close();
                        idr = null;
                    }

                }


                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", C_DocType_ID);
                idr = DB.ExecuteReader(sql.ToString(), param, null);
                if (idr.Read())
                {
                    SetC_DocType_ID(C_DocType_ID);
                    //p_changeVO.setContext(getCtx(), windowNo, "C_DocTypeTarget_ID", C_DocType_ID);
                    p_ctx.SetContext(windowNo, GetCtx().ToString(), "C_DocTypeTarget_ID" + C_DocType_ID);


                    if (VAdvantage.Utility.Util.GetValueOfString(idr[1]).Equals("Y"))
                    {
                        if (!newDocNo && AD_Sequence_ID != VAdvantage.Utility.Util.GetValueOfInt(idr[5]))
                            newDocNo = true;
                        if (newDocNo)
                            if (Ini.IsPropertyBool(Ini.P_VIENNASYS)
                                 && Env.GetCtx().GetAD_Client_ID() < 1000000)
                                SetDocumentNo("<" + VAdvantage.Utility.Util.GetValueOfString(idr[3]) + ">");
                            else if (VAdvantage.Utility.Util.GetValueOfString(idr[2]) != null)
                                SetDocumentNo("<" + VAdvantage.Utility.Util.GetValueOfString(idr[2]) + ">");
                    }

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

        /**
         * 	Set Business Partner
         *  Sets location and contact
         *	@param oldC_BPartner_ID old BP
         *	@param newC_BPartner_ID new BP
         *	@param windowNo window no
         */
        public void SetC_BPartner_ID(String oldC_BPartner_ID,
                    String newC_BPartner_ID, int windowNo)
        {
            if (newC_BPartner_ID == null || newC_BPartner_ID.Length == 0)
            {
                Set_ValueNoCheck("C_BPartner_Location_ID", null);
                Set_ValueNoCheck("AD_User_ID", null);
                SetC_Project_ID(0);
                return;
            }
            int BPartner_ID = VAdvantage.Utility.Util.GetValueOfInt(newC_BPartner_ID);
            if (BPartner_ID == 0)
            {
                Set_ValueNoCheck("C_BPartner_Location_ID", null);
                Set_ValueNoCheck("AD_User_ID", null);
                SetC_Project_ID(0);
                return;
            }

            int Location_ID = 0;
            int User_ID = 0;

            String sql = "SELECT c.AD_User_ID, loc.C_BPartner_Location_ID AS Location_ID "
                + "FROM C_BPartner p"
                + " LEFT OUTER JOIN C_BPartner_Location loc ON (p.C_BPartner_ID=loc.C_BPartner_ID AND loc.IsActive='Y')"
                + " LEFT OUTER JOIN AD_User c ON (p.C_BPartner_ID=c.C_BPartner_ID) "
                + "WHERE p.C_BPartner_ID=@param1 AND p.IsActive='Y'"		//	#1
                + " ORDER BY loc.Name ASC ";

            //PreparedStatement pstmt = DB.prepareStatement(sql, (Trx)null);
            //ResultSet rs = null;
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", BPartner_ID);
                if (idr.Read())
                {
                    Location_ID = VAdvantage.Utility.Util.GetValueOfInt(idr["Location_ID"]);
                    User_ID = VAdvantage.Utility.Util.GetValueOfInt(idr["AD_User_ID"]);
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
                log.Log(Level.SEVERE, "bPartnerBill");
            }

            int orderID = this.GetC_Order_ID();
            if (orderID != null && orderID != 0)
            {
                VAdvantage.Model.MOrder order = new VAdvantage.Model.MOrder(Env.GetCtx(), orderID, null);
                int bpartnerID = order.GetC_BPartner_ID();
                if (BPartner_ID == bpartnerID)
                {
                    Location_ID = order.GetC_BPartner_Location_ID();
                    User_ID = order.GetAD_User_ID();
                }
            }


            //	overwritten by InfoBP selection - works only if InfoWindow
            //	was used otherwise creates error (uses last value, may belong to differnt BP)
            if (GetCtx().GetContextAsInt(EnvConstants.WINDOW_INFO, EnvConstants.TAB_INFO, "C_BPartner_ID") == BPartner_ID)
            {
                String loc = GetCtx().GetContext(EnvConstants.WINDOW_INFO, EnvConstants.TAB_INFO, "C_BPartner_Location_ID");
                if (loc.Length > 0)
                    Location_ID = VAdvantage.Utility.Util.GetValueOfInt(loc);
                String cont = GetCtx().GetContext(EnvConstants.WINDOW_INFO, EnvConstants.TAB_INFO, "AD_User_ID");
                if (cont.Length > 0)
                    User_ID = VAdvantage.Utility.Util.GetValueOfInt(cont);
            }

            SetC_BPartner_Location_ID(Location_ID);
            SetAD_User_ID(User_ID);

            SetC_Project_ID(0);
        }	//	setC_BPartner_ID

        /**
         * Set the Sales Representative_ID derived from Supervisor_ID
         * If Supervisor_ID corresponds to 'SuperUser' then don't change the value of SalesRep_ID
         * @param oldSupervisor_ID
         * @param newSupervisor_ID
         * @param windowNo
         * @throws Exception
         */
        public void SetSupervisor_ID(String oldSupervisor_ID,
                   String newSupervisor_ID, int windowNo)
        {
            if (newSupervisor_ID == null || newSupervisor_ID.Trim().Length == 0)
                return;

            if (0 == VAdvantage.Utility.Util.GetValueOfInt(newSupervisor_ID))
                return;

            if (0 == GetSalesRep_ID())
                SetSalesRep_ID(VAdvantage.Utility.Util.GetValueOfInt(newSupervisor_ID));
        }

        public void SetIsPrepared(bool prepare)
        {
            m_callPrepared = prepare;

        }

        public void SetQtyEntered(String oldQtyEntered,
                   String newQtyEntered, int windowNo)
        {

            if (newQtyEntered == null || newQtyEntered.Trim().Length == 0)
                return;

            if (GetC_UOM_ID() == 0)
                return;

            Decimal QtyEntered = Convert.ToDecimal(newQtyEntered);
            // Decimal QtyEntered1 = QtyEntered.setScale(
            //        MUOM.getPrecision(getCtx(), getC_UOM_ID()), BigDecimal.ROUND_HALF_UP);
            Decimal QtyEntered1 = Decimal.Round((QtyEntered), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()));
            if (QtyEntered.CompareTo(QtyEntered1) != 0)
            {
                log.Fine("Corrected QtyEntered Scale UOM=" + GetC_UOM_ID()
                        + "; QtyEntered=" + QtyEntered + "->" + QtyEntered1);
                QtyEntered = QtyEntered1;
                SetVAMFG_QtyEntered(QtyEntered);
            }
        }


        public void SetProcessMsg(String processMsg)
        {
            m_processMsg = processMsg;
        }


        public String GetDocBaseType()
        {
            return VAdvantage.Model.MDocBaseType.DOCBASETYPE_WORKORDER;
        }

        //@override
        public DateTime? GetDocumentDate()
        {
            // TODO Auto-generated method stub
            return GetVAMFG_DateAcct();
        }

        /// <summary>
        /// Reverse any associated Warehouse Tasks
        /// </summary>
        /// <param name="onlyIncomplete">onlyIncomplete - true if only tasks with DocStatus 'IP','DR',</param>
        /// <returns>Return true if get method</returns>
        private Boolean ReverseTasks(bool onlyIncomplete)
        {
            //Class<?>[] parameterTypes = new Class[]{X_M_WorkOrder.class, boolean.class};
            Type[] parameterTypes = new Type[] { typeof(X_VAMFG_M_WorkOrder), typeof(Boolean) };
            Object[] args = new Object[] { this, onlyIncomplete };

            try
            {
                // Class<?> c = Class.forName("org.compiere.cwms.util.MWarehouseTaskUtil");

                // Commented by Bharat on 30 Oct 2017 as discussed with Mukesh Sir
                // it will be uncommented whenever the WMS classes are available.

                //Type c = Type.GetType("ViennaAdvantage.CWMS.Util.MWarehouseTaskUtil");
                //if (c == null)
                //{
                //    return false;
                //}

                //System.Reflection.MethodInfo m = c.GetMethod("ReverseWorkOrderTasks", parameterTypes);
                //return (Boolean)m.Invoke(null, args);
                return true;
            }
            catch (Exception e)
            {
                log.Warning("Error reversing Warehouse Tasks:" + e.ToString());
            }
            return false;

        }




        #region DocAction Members
        public Env.QueryParams GetLineOrgsQueryInfo()
        {
            return new VAdvantage.Utility.Env.QueryParams("SELECT DISTINCT AD_Org_ID FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = @param1",
                    new Object[] { GetVAMFG_M_WorkOrder_ID() });
        }

        #endregion



        #region DocAction Members


        public Trx Get_Trx()
        {
            return Trx.Get(Get_TrxName().GetTrxName());
        }

        #endregion


        //public bool ProcessIt(string action)
        //{
        //    return true;
        //}


        public bool ProcessIt(string action)
        {
            _processMsg = null;
            DocumentEngine engine = new DocumentEngine(this, GetDocStatus());
            return engine.ProcessIt(action, GetDocAction());
        }

        /**
        * @param workOrderID
        * @param DateScheduledFrom
        * @param DateScheduleTo
        * @return true if success
        */
        //Sets the fields  ScheduledFrom and ScheduledTo dates in WorkOrderOperations and WorkOrder windows
        private Boolean SchedulerSetDate(int workOrderID, DateTime? DateScheduledFrom, DateTime? DateScheduleTo)
        {
            // ScheduleDateFrom of an Operation
            DateTime? StartDate = null;

            // ScheduleDateTo of an Operation
            DateTime? EndDate = null;

            // Consider SetupTime while calculating the TotalTimeConsumed for an operation
            Boolean includeSetupTime = true;
            // WorkOrder Quantity
            Decimal quantity = Decimal.Zero;
            //Include Optional Operation Time 
            Boolean includeOptionalOperationTime = false;
            // If Date Scheduled from is provided, then it is ForwardScheduling
            Boolean isForwardScheduling = true;
            MVAMFGMWorkOrder workOrder = new MVAMFGMWorkOrder(GetCtx(), workOrderID, Get_TrxName());

            // set IsPrepared flag to true
            workOrder.SetIsPrepared(true);
            quantity = workOrder.GetVAMFG_QtyEntered();
            if (DateScheduledFrom != null)
            {
                StartDate = DateScheduledFrom.Value.ToLocalTime();
                isForwardScheduling = true;
                ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder wo1 = new CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
                MVAMFGMWorkOrderOperation[] woos = MVAMFGMWorkOrderOperation.GetOfWorkOrder(wo1, null, "VAMFG_SeqNo");
                foreach (MVAMFGMWorkOrderOperation woo in woos)
                {
                    woo.setIsProcessCalled(true);
                    VAdvantage.Model.MUOM uom = new VAdvantage.Model.MUOM(this.GetCtx(), woo.GetC_UOM_ID(), null);

                    if (!(uom.IsDay() || uom.IsHour() || uom.IsMinute()))
                    {
                        log.Warning("UOM should either be a Day or Hour or Minute");
                        return false;
                    }
                    // Start date of an operation is the End date of the previous operation 
                    woo.SetVAMFG_DateScheduleFrom(StartDate);
                    EndDate = ViennaAdvantage.CMFG.Util.MWorkOrderSchedulerUtil.ScheduleDate(GetCtx(), woo, quantity, StartDate,
                            includeOptionalOperationTime, includeSetupTime, isForwardScheduling);
                    //EndDate = StartDate;
                    woo.SetVAMFG_DateScheduleTo(EndDate);
                    woo.Save();
                    if (EndDate != null)
                        StartDate = EndDate;
                }
                workOrder.SetVAMFG_DateScheduleTo(StartDate);// sets the WorkOrder DateScheduleTo field to the EndDate(DateScheduleTo) of last Operation.
                workOrder.Save();
            }

            else if (DateScheduleTo != null)
            {
                EndDate = DateScheduleTo.Value.ToLocalTime();
                isForwardScheduling = false;
                ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder wo1 = new CMFG.Model.MVAMFGMWorkOrder(GetCtx(), GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
                MVAMFGMWorkOrderOperation[] woos = MVAMFGMWorkOrderOperation.GetOfWorkOrder(wo1, null, "VAMFG_SeqNo DESC");
                foreach (MVAMFGMWorkOrderOperation woo in woos)
                {
                    woo.setIsProcessCalled(true);
                    VAdvantage.Model.MUOM uom = new VAdvantage.Model.MUOM(this.GetCtx(), woo.GetC_UOM_ID(), null);
                    if (!(uom.IsDay() || uom.IsHour() || uom.IsMinute()))
                    {
                        log.Warning("UOM should either be a Day or Hour or Minute");
                        return false;
                    }
                    // End date of an operation is the Start date of the next operation			
                    woo.SetVAMFG_DateScheduleTo(EndDate);
                    StartDate = ViennaAdvantage.CMFG.Util.MWorkOrderSchedulerUtil.ScheduleDate(GetCtx(), woo, quantity, EndDate,
                            includeOptionalOperationTime, includeSetupTime, isForwardScheduling);
                    woo.SetVAMFG_DateScheduleFrom(StartDate);
                    woo.Save();
                    if (StartDate != null)
                        EndDate = StartDate;
                }
                workOrder.SetVAMFG_DateScheduleFrom(EndDate); //sets the WorkOrder DateScheduleFrom field to the StartDate(DateScheduleFrom) of First Operation.
                workOrder.Save();
            }
            return true;
        }// Set Dates	
    }
}
