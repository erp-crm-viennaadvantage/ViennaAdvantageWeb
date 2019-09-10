/********************************************************
 * Project Name   : ViennaAdvantage
 * Class Name     : MWorkOrderTxnUtil
 * Purpose        : 
 * Class Used     :
 * Chronological    Development
 * Karan    05-March-2011
  ******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VAdvantage.Logging;
using VAdvantage.Utility;
using ViennaAdvantage.Model;
using VAdvantage.DataBase;
using System.Data.SqlClient;
using ViennaAdvantage.CMFG.Model;
using System.Collections;
using VAdvantage.Model;



namespace ViennaAdvantage.Process
{
    public class MWorkOrderTxnUtil
    {
        private static VLogger log = VLogger.GetVLogger(typeof(MWorkOrderTxnUtil).FullName);
        private bool save = false;
        public MWorkOrderTxnUtil(bool save)
        {
            this.save = save;
        }

        /// <summary>
        /// Generates Resource Txn Lines against WO Txn for quantity earlier estimated and not yet used for all the operations in the Work Order; skips the optional operations
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="M_WorkOrderTransaction_ID"></param>
        /// <param name="Qty"></param>
        /// <param name="trx"></param>
        /// <param name="automatic"></param>
        /// <returns></returns>

        public MVAMFGMWrkOdrRscTxnLine[] GenerateResourceTxnLine(Ctx ctx, int M_WorkOrderTransaction_ID, Decimal Qty,
                Trx trx, bool automatic)
        {

            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, M_WorkOrderTransaction_ID, trx);
            if (wot != null && !(wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ResourceUsage)
                    && (wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_Drafted) || wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_InProgress))))
            {
                log.Severe("Work Order transaction type not correct.");
                return null;
            }

            int OperationFrom = DB.GetSQLValue(trx, "SELECT MIN(VAMFG_SeqNo) FROM VAMFG_M_WorkOrderOperation " +
                    "WHERE VAMFG_M_WorkOrder_ID = " + wot.GetVAMFG_M_WorkOrder_ID() + " AND VAMFG_IsOptional<> 'Y'");
            int OperationTo = DB.GetSQLValue(trx, "SELECT MAX(VAMFG_SeqNo) FROM VAMFG_M_WorkOrderOperation " +
                    "WHERE VAMFG_M_WorkOrder_ID =" + wot.GetVAMFG_M_WorkOrder_ID() + " AND VAMFG_IsOptional <> 'Y'");

            return GenerateResourceTxnLine(ctx, M_WorkOrderTransaction_ID, Qty,
                    new Decimal(OperationFrom), new Decimal(OperationTo), trx, automatic);
        }

        /// <summary>
        /// Generates Resource Transaction Lines against WO Txn between &
        /// including specified operation VAMFG_SeqNo; skips the optional operations unless 
        /// they are either the starting operation sequence or ending operation sequence
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="M_WorkOrderTransaction_ID"></param>
        /// <param name="Qty"></param>
        /// <param name="OperationFrom"></param>
        /// <param name="OperationTo"></param>
        /// <param name="trx"></param>
        /// <param name="automatic"></param>
        /// <returns></returns>
        public MVAMFGMWrkOdrRscTxnLine[] GenerateResourceTxnLine(Ctx ctx, int M_WorkOrderTransaction_ID, Decimal? Qty,
                Decimal? OperationFrom, Decimal? OperationTo, Trx trx, bool automatic)
        {

            if (0 >= M_WorkOrderTransaction_ID)
            {
                log.Severe("No Work Order Transaction ID specified");
                return null;
            }

            if (OperationFrom.Value.CompareTo(OperationTo) > 0)
            {
                log.Severe("Operation Numbers not correct.");
                return null;
            }

            if (Qty != null && Qty.Value.CompareTo(Decimal.Zero) <= 0)
            {
                log.Severe("Number of product assemblies must be positive");
                return null;
            }

            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, M_WorkOrderTransaction_ID, trx);
            if (wot != null && !(wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ResourceUsage)
                    && (wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_Drafted) || wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_InProgress))))
            {
                log.Severe("Work Order transaction type not correct.");
                return null;
            }

            //ArrayList<MVAMFGMWrkOdrRscTxnLine> wortLines = new ArrayList<MVAMFGMWrkOdrRscTxnLine>();
            // List<MVAMFGMWrkOdrRscTxnLine> wortLines=new List<MVAMFGMWrkOdrRscTxnLine>();
            System.Collections.ArrayList wortLines = new System.Collections.ArrayList();

            ViennaAdvantage.Model.MVAMFGMWorkOrder wo = new ViennaAdvantage.Model.MVAMFGMWorkOrder(ctx, wot.GetVAMFG_M_WorkOrder_ID(), trx);
            int resTxnLineSeqNo = DB.GetSQLValue(trx, "SELECT Max(VAMFG_SeqNo) FROM MVAMFGMWrkOdrRscTxnLine WHERE VAMFG_M_WrkOdrTransaction_ID =" + M_WorkOrderTransaction_ID);

            StringBuilder wc = new StringBuilder();
            if (OperationFrom.Value.CompareTo(Decimal.Zero) > 0)
            {
                wc.Append(" VAMFG_SeqNo >= ").Append(OperationFrom);
                if (OperationTo.Value.CompareTo(Decimal.Zero) > 0)
                    wc.Append(" AND VAMFG_SeqNo <= ").Append(OperationTo);
            }
            else if (OperationTo.Value.CompareTo(Decimal.Zero) > 0)
                wc.Append(" VAMFG_SeqNo <= ").Append(OperationTo);

            //	Don't consider the optional operations, but include the "From" and "To" operations even if they are optional
            wc.Append(" AND (VAMFG_IsOptional <> 'Y' ");
            if (OperationFrom.Value.CompareTo(Decimal.Zero) > 0)
                wc.Append(" OR VAMFG_SeqNo = ").Append(OperationFrom);
            if (OperationTo.Value.CompareTo(Decimal.Zero) > 0)
                wc.Append(" OR VAMFG_SeqNo = ").Append(OperationTo);
            wc.Append(" )");

            ViennaAdvantage.CMFG.Model.MVAMFGMWorkOrder worder = new CMFG.Model.MVAMFGMWorkOrder(ctx, wo.GetVAMFG_M_WorkOrder_ID(), trx);
            String whereClause = (wc.Length > 0 ? wc.ToString() : null);
            MVAMFGMWorkOrderOperation[] woos = MVAMFGMWorkOrderOperation.GetOfWorkOrder(worder, whereClause, "VAMFG_SeqNo");

            StringBuilder response = new StringBuilder();

            foreach (MVAMFGMWorkOrderOperation woo in woos)
            {
                MVAMFGMWorkOrderResource[] wors = MVAMFGMWorkOrderResource.GetofWorkOrderOperation(woo, null, null);
                foreach (MVAMFGMWorkOrderResource wor in wors)
                {
                    String chargeType = wor.GetVAMFG_ChargeType();
                    if (!((chargeType.Equals(X_VAMFG_M_WorkOrderResource.VAMFG_CHARGETYPE_Automatic) && automatic)
                            || (chargeType.Equals(X_VAMFG_M_WorkOrderResource.VAMFG_CHARGETYPE_Manual) && !automatic)))
                        continue;
                    //calculated values
                    Decimal resAmt = Decimal.Zero;	//Resource amount to be charged
                    //if product assembly quantity, Qty, is null then derive based on how many have already been charged
                    //if estimated amt as indicated by WorkOrder Resource is already, then don't charge any more
                    if (Qty == null)
                    {
                        Decimal resCharged = wor.GetVAMFG_QtySpent();
                        Decimal resReq = Decimal.Multiply(wo.GetVAMFG_QtyEntered(), (wor.GetVAMFG_QtyRequired()));
                        resAmt = Decimal.Subtract(resReq, (resCharged));
                        if (resAmt.CompareTo(Decimal.Zero) <= 0)
                        {
                            log.Warning("Estimated Resource usage has been already charged.");
                            continue;
                        }
                    }
                    else
                        resAmt = Decimal.Multiply(Qty.Value, (wor.GetVAMFG_QtyRequired()));

                    MVAMFGMWrkOdrRscTxnLine wortl = new MVAMFGMWrkOdrRscTxnLine(ctx, 0, trx);
                    //wortl.SetVAMFG_QtyEntered(resAmt.setScale(MUOM.getPrecision(ctx, wor.getC_UOM_ID()), Decimal.ROUND_HALF_UP));
                    wortl.SetVAMFG_QtyEntered(Decimal.Round((resAmt), VAdvantage.Model.MUOM.GetPrecision(ctx, wor.GetC_UOM_ID()), MidpointRounding.AwayFromZero));
                    // set fields from parent Work Order Transaction
                    wortl.SetVAMFG_M_WrkOdrTransaction_ID(M_WorkOrderTransaction_ID);
                    wortl.SetClientOrg(wot);

                    // set fields from Work Order Resource
                    wortl.Setresourceinfo(wor);

                    //increase the VAMFG_SeqNo for each WOResourceTxnLine
                    resTxnLineSeqNo += 10;
                    wortl.SetVAMFG_SeqNo(resTxnLineSeqNo);

                    wortl.SetIsActive(true);

                    //Add to the return ArrayList
                    wortLines.Add(wortl);

                    ViennaAdvantage.Model.MProduct product = ViennaAdvantage.Model.MProduct.Get(ctx, wortl.GetM_Product_ID());
                    response.Append(product.GetName() + ": ").Append(wortl.GetVAMFG_QtyEntered());
                    if (!wortl.Save())
                    {
                        log.Severe("Could not save resource transaction line.");
                    }
                }
            }

            if (save)
            {
                try
                {
                    if (!VAdvantage.Model.PO.SaveAll(trx, wortLines))
                    {
                        log.Severe("Could not save resource transaction line.");
                        return null;
                    }
                }
                catch { }
            }
            log.SaveInfo("Info", response.ToString());
            // return (MVAMFGMWrkOdrRscTxnLine[])wortLines.ToArray();
            MVAMFGMWrkOdrRscTxnLine[] newObject = null;

            try
            {
                newObject = (MVAMFGMWrkOdrRscTxnLine[])wortLines.ToArray(typeof(MVAMFGMWrkOdrRscTxnLine));
                return newObject;
            }
            catch { }
            return (MVAMFGMWrkOdrRscTxnLine[])wortLines.ToArray();
        }



        /**
         * Creates a Work Order Transaction header for a given Work Order assumes locator is same locator as on Work Order
         * @param ctx
         * @param VAMFG_M_WorkOrder_ID Work Order
         * @param TxnType valid values are CI (Component Issue), CR (Component Return), RU (Resource Usage)
         * @param trx
         * @return MVAMFGMWrkOdrTransaction on success, null otherwise
         */
        public ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction CreateWOTxn(Ctx ctx, int VAMFG_M_WorkOrder_ID, String TxnType, Trx trx)
        {

            ViennaAdvantage.Model.MVAMFGMWorkOrder wo = new ViennaAdvantage.Model.MVAMFGMWorkOrder(ctx, VAMFG_M_WorkOrder_ID, trx);
            if (wo == null || !wo.GetDocStatus().Equals(ViennaAdvantage.Model.MVAMFGMWorkOrder.DOCSTATUS_InProgress))
            {
                log.Severe("Work Order number not valid for transactions.");
                return null;
            }

            log.Info("Getting Default Locator of Work Order Warehouse.");
            MLocator loc = MWarehouse.Get(ctx, wo.GetM_Warehouse_ID()).GetDefaultLocator();

            return createWOTxn(ctx, VAMFG_M_WorkOrder_ID, TxnType, 0, loc.GetM_Locator_ID(), Decimal.Zero, trx);
        }

        /**
         * Creates a Work Order Transaction header for a given Work Order, assumes Quantity as entered in parent Work Order Transaction for child transactions or takes Work Order Quantity
         * @param ctx
         * @param VAMFG_M_WorkOrder_ID Work Order
         * @param TxnType Valid values are CI (Component Issue), CR (Component Return), RU (Resource Usage)
         * @param parent_M_WorkOrderTransaction_ID Parent Work Order Transaction if this is a child transaction
         * @param trx
         * @return MVAMFGMWrkOdrTransaction on success, null otherwise
         */
        public ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction createWOTxn(Ctx ctx, int VAMFG_M_WorkOrder_ID, String TxnType,
                int parent_M_WorkOrderTransaction_ID, Trx trx)
        {

            return createWOTxn(ctx, VAMFG_M_WorkOrder_ID, TxnType, parent_M_WorkOrderTransaction_ID, 0, Decimal.Zero, trx);

        }

        /**
         * Creates a Work Order Transaction header for a given Work Order
         * Transactions handled : Resource Usage (RU), Component Return (CR), Component Issue (CI)
         * @param ctx
         * @param VAMFG_M_WorkOrder_ID Work Order
         * @param TxnType Work Order Transaction Type
         * @param parent_M_WorkOrderTransaction_ID Parent Work Order Transaction
         * @param M_Locator_ID Work Order Transaction Locator
         * @param Qty Work Order Transaction Quantity
         * @param trx
         * @return MVAMFGMWrkOdrTransaction
         */
        public ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction createWOTxn(Ctx ctx, int VAMFG_M_WorkOrder_ID, String TxnType,
                int parent_M_WorkOrderTransaction_ID, int M_Locator_ID, Decimal Qty, Trx trx)
        {
            int _countGOM01 = Convert.ToInt32(DB.ExecuteScalar("SELECT COUNT(AD_ModuleInfo_ID) FROM AD_ModuleInfo WHERE Prefix like 'GOM01_'"));
            if (!(TxnType.Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ResourceUsage)
                    || TxnType.Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder)
                    || TxnType.Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder)))
            {
                log.Severe("Not correct transaction type to generate WO Transaction");
                return null;
            }
            //Check the validity of the Work Order
            ViennaAdvantage.Model.MVAMFGMWorkOrder wo = new ViennaAdvantage.Model.MVAMFGMWorkOrder(ctx, VAMFG_M_WorkOrder_ID, trx);
            if (wo == null || !wo.GetDocStatus().Equals(X_VAMFG_M_WorkOrder.DOCSTATUS_InProgress))
            {
                log.Severe("Work Order number not valid for transactions.");
                return null;
            }

            //	Checking if Quantity can be derived from associated WorkOrder
            if (parent_M_WorkOrderTransaction_ID == 0)
                if (Qty == null || Qty == 0)
                {
                    log.Info("Deriving Quantity from Work Order");
                    Qty = wo.GetVAMFG_QtyEntered();
                }

            //Deriving Qty from parent WO Move Txn
            if (Qty.CompareTo(Decimal.Zero) <= 0)
            {
                ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction parentWOT = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, parent_M_WorkOrderTransaction_ID, trx);
                Qty = parentWOT.GetVAMFG_QtyEntered();
            }

            //Creating new WorkOrder Transaction
            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, 0, trx);
            wot.SetRequiredColumns(VAMFG_M_WorkOrder_ID, M_Locator_ID, X_VAMFG_M_WrkOdrTransaction.VAMFG_WOTXNSOURCE_Generated, TxnType);
            if (parent_M_WorkOrderTransaction_ID > 0)
            {
                wot.SetParentWorkOrderTxn_ID(parent_M_WorkOrderTransaction_ID);
                ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction parentWOT = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, parent_M_WorkOrderTransaction_ID, trx);
                wot.SetC_DocType_ID(parentWOT.GetC_DocType_ID());
                // set the client & org derived from parent work order transaction
                wot.SetClientOrg(parentWOT);
                if (_countGOM01 > 0)
                {
                    wot.SetGOM01_Density(parentWOT.GetGOM01_Density());
                    wot.SetGOM01_Quantity(parentWOT.GetGOM01_Quantity());
                    wot.SetGOM01_ActualQuantity(parentWOT.GetGOM01_ActualQuantity());
                    wot.SetGOM01_ActualDensity(parentWOT.GetGOM01_ActualDensity());
                    wot.SetGOM01_ActualLiter(parentWOT.GetGOM01_ActualLiter());
                }
            }
            else
            {	//	derive the doctype from the WorkOrderClass
                MVAMFGMWorkOrderClass woclass = new MVAMFGMWorkOrderClass(ctx, wo.GetVAMFG_M_WorkOrderClass_ID(), trx);
                wot.SetC_DocType_ID(woclass.GetWOT_DocType_ID());
                // since there is no parent work order transaction
                // set the client & org derived from the associated work order
                wot.SetClientOrg(wo);
            }
            //wot.SetVAMFG_QtyEntered(Qty.setScale(MUOM.getPrecision(ctx, wot.getC_UOM_ID()), Decimal.ROUND_HALF_UP));
            wot.SetVAMFG_QtyEntered(Decimal.Round((Qty), VAdvantage.Model.MUOM.GetPrecision(ctx, wot.GetC_UOM_ID()), MidpointRounding.AwayFromZero));
            if (save)
                if (!wot.Save(trx))
                {
                    log.Severe("Could not save WO Txn.");
                    return null;
                }

            return wot;
        }


        /**
         * 	Get latest open work order transaction for the Work Order or create one
         * @param ctx
         * @param VAMFG_M_WorkOrder_ID Work Order
         * @param TxnType Work Order Transaction Type
         * @param trx
         * @return MVAMFGMWrkOdrTransaction - latest open work order transaction
         */
        public ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction RetrieveWOTxn(Ctx ctx, int VAMFG_M_WorkOrder_ID, String TxnType, Trx trx)
        {
            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = null;
            if (wot == null)
                wot = CreateWOTxn(ctx, VAMFG_M_WorkOrder_ID, TxnType, trx);
            return wot;
        }


        /**
         * api to generate component transaction lines in M_WorkOrderTransactionLine
         * uses OperationFrom as the 1st Operation of WO
         * uses OperationTo as the last
         * @param ctx
         * @param M_WorkOrderTransaction_ID Work Order Transaction header
         * @param Qty Number of Work Order Product Assemblies for which to generate components
         * @param SupplyType Component Supply Type. Valid values are P (Push), O (Operation Pull), A (Assembly Pull)
         * @param trx
         * @return Array of MVAMFGMWrkOdrTrnsctionLine on success, null otherwise
         */
        public ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine[] GenerateComponentTxnLine(Ctx ctx, int M_WorkOrderTransaction_ID,
                Decimal Qty, String SupplyType, Trx trx)
        {

            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, M_WorkOrderTransaction_ID, trx);
            if (!((wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_Drafted) || wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_InProgress))
                    && (wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder)
                            || wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder))))
            {
                log.Severe("Invalid Work Order Txn DocStatus.");
                return null;
            }

            int OperationFrom = DB.GetSQLValue(trx, "SELECT MIN(VAMFG_SeqNo) FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID = " + wot.GetVAMFG_M_WorkOrder_ID() +
                    "AND VAMFG_IsOptional <> 'Y'");
            int OperationTo = DB.GetSQLValue(trx, "SELECT MAX(VAMFG_SeqNo) FROM VAMFG_M_WorkOrderOperation WHERE VAMFG_M_WorkOrder_ID =  " + wot.GetVAMFG_M_WorkOrder_ID() +
                    "AND VAMFG_IsOptional <> 'Y'");

            return GenerateComponentTxnLine(ctx, M_WorkOrderTransaction_ID, Qty,
                    new Decimal(OperationFrom), new Decimal(OperationTo), SupplyType, trx);
        }
        /**
         * api to generate component transaction lines in M_WorkOrderTransactionLine. Skips optional operations unless they are either part of starting operation sequence number or ending operation sequence number
         * uses Default Locator/ highest priority Locator of Warehouse of the WorkOrder
         * @param ctx
         * @param M_WorkOrderTransaction_ID Work Order Transaction header
         * @param Qty Number of Work Order Product Assemblies for which to generate components 
         * @param OperationFrom Starting operation sequence number
         * @param OperationTo Ending operation sequence number
         * @param SupplyType Component supply type. Valid values are P (Push), O (Operation Pull), A (Assembly Pull)
         * @param trx
         * @return MVAMFGMWrkOdrTrnsctionLine
         */
        public ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine[] GenerateComponentTxnLine(Ctx ctx, int M_WorkOrderTransaction_ID,
                Decimal? Qty, Decimal? OperationFrom, Decimal? OperationTo, String SupplyType, Trx trx)
        {

            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, M_WorkOrderTransaction_ID, trx);
            if (!((wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_Drafted) || wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_InProgress))
                    && (wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder)
                            || wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder))))
            {
                log.Severe("Invalid Work Order Txn DocStatus.");
                return null;
            }

            int locatorID = wot.GetM_Locator_ID();
            if (0 == locatorID)
            {
                ViennaAdvantage.Model.MVAMFGMWorkOrder wo = new ViennaAdvantage.Model.MVAMFGMWorkOrder(ctx, wot.GetVAMFG_M_WorkOrder_ID(), trx);
                if (!wo.GetDocStatus().Equals(X_VAMFG_M_WorkOrder.DOCSTATUS_InProgress))
                {
                    log.Severe("Invalid Work Order DocStatus.");
                    return null;
                }
                locatorID = (MWarehouse.Get(ctx, wo.GetM_Warehouse_ID())).GetDefaultM_Locator_ID();
            }

            return GenerateComponentTxnLine(ctx, M_WorkOrderTransaction_ID,
                    Qty, OperationFrom, OperationTo, SupplyType, locatorID, trx);
        }

        //	API to generate component transaction lines in M_WorkOrderTransactionLine. It should be possible to generate component lines 
        //	for all push / pull components in a specific operation and all push / pull components in all operations in a work order
        /**
         * api to generate component transaction lines in M_WorkOrderTransactionLine. Skips Optional operations unless they are either the starting operation sequence or ending operation sequence in the list of operations specified
         * @param ctx
         * @param M_WorkOrderTransaction_ID Work Order Transaction
         * @param Qty Number of Work Order Product Assemblies for which to generate components
         * @param OperationFrom Starting operation sequence number
         * @param OperationTo Ending operation sequence number
         * @param SupplyType Component Supply Type. Valid values are P (Push), O (Operation Pull), A (Assembly Pull)
         * @param M_Locator_ID Supply Locator for the components in case of Push supply type
         * @param trx
         * @return Array of MVAMFGMWrkOdrTrnsctionLine
         */
        public ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine[] GenerateComponentTxnLine(Ctx ctx, int M_WorkOrderTransaction_ID,
                Decimal? Qty, Decimal? OperationFrom, Decimal? OperationTo,
            String SupplyType, int M_Locator_ID, Trx trx)
        {
            int _countGOM01 = 0;
            Tuple<String, String, String> mInfo = null;
            if (Env.HasModulePrefix("GOM01_", out mInfo))
            {
                _countGOM01 = 1;
            }
            if (OperationFrom != null && OperationFrom.Value.CompareTo(OperationTo) > 0)
            {
                log.Severe("Operation Numbers not correct.");
                return null;
            }

            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(ctx, M_WorkOrderTransaction_ID, trx);
            if (0 == M_WorkOrderTransaction_ID || !((wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_Drafted) || wot.GetDocStatus().Equals(X_VAMFG_M_WrkOdrTransaction.DOCSTATUS_InProgress))
                    && (wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder)
                            || wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder))))
            {
                log.Severe("Cannot create component lines against give WO Txn.");
                return null;
            }

            if (Qty != null && Qty.Value.CompareTo(Decimal.Zero) <= 0)
            {
                log.Severe("Number of product assemblies must be positive");
                return null;
            }

            //ArrayList<MVAMFGMWrkOdrTrnsctionLine> wotLines = new ArrayList<MVAMFGMWrkOdrTrnsctionLine>();
            // List<MVAMFGMWrkOdrTrnsctionLine> wotLines = new List<MVAMFGMWrkOdrTrnsctionLine>();
            ArrayList wotLines = new ArrayList();

            int lastMandatoryWOO = DB.GetSQLValue(trx, "SELECT MAX(VAMFG_SeqNo) FROM VAMFG_M_WorkOrderOperation " +
                    "WHERE VAMFG_M_WorkOrder_ID = " + wot.GetVAMFG_M_WorkOrder_ID() + " AND VAMFG_IsOptional<>'Y'");

            //			automatically consider AssemblyPull Supply Type when the OperationTo is greater than the last Work Order Operation. 
            //			This in turn should take care of optional operations.
            bool assemblyPull = lastMandatoryWOO == OperationTo;

            StringBuilder response = new StringBuilder("");

            //	Getting the WorkOrder product assembly quantity
            Decimal woQty = DB.GetSQLValue(trx, "SELECT VAMFG_QtyEntered FROM VAMFG_M_WorkOrder " +
                    "WHERE VAMFG_M_WorkOrder_ID = " + wot.GetVAMFG_M_WorkOrder_ID());

            // Get component requirements based on the operations moved through in this transaction and then make lines
            StringBuilder sqlBuf = new StringBuilder("SELECT woc.M_Product_ID, woc.C_UOM_ID, woc.VAMFG_QtyRequired, " +
                    " woc.VAMFG_SupplyType, woc.M_AttributeSetInstance_ID, woc.M_Locator_ID, woc.BasisType," +
                    " woo.VAMFG_M_WorkOrderOperation_ID, woc.VAMFG_QtyAvailable, woc.VAMFG_QtySpent," +
                    " woc.VAMFG_QtyAllocated, woc.VAMFG_QtyDedicated,woc.isqualitycorrection" +
                    " FROM VAMFG_M_WorkOrderOperation woo INNER JOIN VAMFG_M_WorkOrderComponent woc ON woo.VAMFG_M_WorkOrderOperation_ID = woc.VAMFG_M_WorkOrderOperation_ID" +
                    " WHERE woo.VAMFG_M_WorkOrder_ID = " + wot.GetVAMFG_M_WorkOrder_ID() + " AND woc.VAMFG_QtyRequired != 0 AND (woo.VAMFG_IsOptional <> 'Y' OR ");
            if (OperationFrom != null && OperationFrom.Value.CompareTo(Decimal.Zero) > 0)
            {
                sqlBuf.Append(" woo.VAMFG_SeqNo =" + VAdvantage.Utility.Util.GetValueOfInt(OperationFrom));
                sqlBuf.Append(" OR woo.VAMFG_SeqNo =" + VAdvantage.Utility.Util.GetValueOfInt(OperationTo));
                sqlBuf.Append(")");
            }
            else
            {
                sqlBuf.Append(" woo.VAMFG_SeqNo =0");
                sqlBuf.Append(" OR woo.VAMFG_SeqNo =0");
                sqlBuf.Append(")");

            }
            // Set OperationTo sequence no
            if (OperationTo != null && OperationTo.Value.CompareTo(Decimal.Zero) > 0)
            {
                sqlBuf.Append(" AND ((woo.VAMFG_SeqNo BETWEEN " + VAdvantage.Utility.Util.GetValueOfInt(OperationFrom) + " AND " + VAdvantage.Utility.Util.GetValueOfInt(OperationTo));
                sqlBuf.Append(")");
            }
            else
            {
                sqlBuf.Append(" AND ((woo.VAMFG_SeqNo BETWEEN " + lastMandatoryWOO + " AND 0");
                sqlBuf.Append(")");
            }

            if (assemblyPull) // if assembly pull, then get component lines from all operations except
                sqlBuf.Append(" OR woc.VAMFG_SupplyType = 'A'");
            sqlBuf.Append(" ) ORDER BY woo.VAMFG_SeqNo, woc.M_Product_ID, woc.VAMFG_QtyRequired "); // close the statement and add ORDER BY clause
            String sql = sqlBuf.ToString();
            // SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            try
            {

                //pstmt.setInt(1, wot.GetVAMFG_M_WorkOrder_ID()); // woo.VAMFG_M_WorkOrder_ID
                //param = new SqlParameter[5];
                //param[0] = new SqlParameter("@param1", wot.GetVAMFG_M_WorkOrder_ID());
                //// Set OperationFrom sequence no
                //if (OperationFrom != null && OperationFrom.Value.CompareTo(Decimal.Zero) > 0)
                //{
                //    param[1] = new SqlParameter("@param2", VAdvantage.Utility.Util.GetValueOfInt(OperationFrom));
                //    param[3] = new SqlParameter("@param4", VAdvantage.Utility.Util.GetValueOfInt(OperationFrom));
                //    //pstmt.setInt(2, OperationFrom.intValue());
                //    //pstmt.setInt(4, OperationFrom.intValue());
                //}
                //else
                //{
                //    param[1] = new SqlParameter("@param2", 0);
                //    param[3] = new SqlParameter("@param4", 0);
                //    //pstmt.setInt(2, 0);
                //    //pstmt.setInt(4, 0);
                //}
                //// Set OperationTo sequence no
                //if (OperationTo != null && OperationTo.Value.CompareTo(Decimal.Zero) > 0)
                //{
                //    param[2] = new SqlParameter("@param3", VAdvantage.Utility.Util.GetValueOfInt(OperationTo));
                //    param[4] = new SqlParameter("@param5", VAdvantage.Utility.Util.GetValueOfInt(OperationTo));
                //    //pstmt.setInt(3, OperationTo.intValue());
                //    //pstmt.setInt(5, OperationTo.intValue());
                //}
                //else
                //{
                //    param[2] = new SqlParameter("@param3", lastMandatoryWOO);
                //    param[4] = new SqlParameter("@param5", 0);
                //    //pstmt.setInt(3, lastMandatoryWOO);
                //    //pstmt.setInt(5, 0);
                //}

                //rs = pstmt.executeQuery();
                idr = DB.ExecuteReader(sql.ToString(), null, trx);
                //dt.Load(idr);
                //idr.Close();
                int productID = 0;
                int uomID = 0;
                Decimal qtyEntered = Decimal.Zero;	//	Quantity in the Product Transaction Line
                Decimal QtyInKg = Decimal.Zero;	    //	Quantity in KG at Product Transaction Line
                int asiID = 0;
                int locatorID = 0;

                //			set the component line no to 10 greater than existing
                int compLineNo = DB.GetSQLValue(trx, "SELECT COALESCE(MAX(VAMFG_Line),0)+10 FROM VAMFG_M_WrkOdrTrnsctionLine " +
                        "WHERE VAMFG_M_WrkOdrTransaction_ID =" + M_WorkOrderTransaction_ID);

                bool checkInventory = wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder);

                while (idr.Read())
                {
                    productID = VAdvantage.Utility.Util.GetValueOfInt(idr[0]);
                    uomID = VAdvantage.Utility.Util.GetValueOfInt(idr[1]);
                    Decimal qtyRequired = VAdvantage.Utility.Util.GetValueOfDecimal(idr[2]);
                    String wocSupplyType = VAdvantage.Utility.Util.GetValueOfString(idr[3]);
                    asiID = VAdvantage.Utility.Util.GetValueOfInt(idr[4]);
                    locatorID = VAdvantage.Utility.Util.GetValueOfInt(idr[5]);
                    String basisType = VAdvantage.Utility.Util.GetValueOfString(idr[6]);
                    int wooID = VAdvantage.Utility.Util.GetValueOfInt(idr[7]);
                    string IsQualityCorr = VAdvantage.Utility.Util.GetValueOfString(idr[12]);

                    // 	If qty=0, then no value was passed for Qty
                    //	for Component Issue assume issue needs to be generated for remaining amount
                    //	for Component Return assume return needs to be generated for unused quantity
                    Decimal qtyIssued = VAdvantage.Utility.Util.GetValueOfDecimal(idr[8]);
                    Decimal qtySpent = VAdvantage.Utility.Util.GetValueOfDecimal(idr[9]);
                    Decimal qtyAllocated = VAdvantage.Utility.Util.GetValueOfDecimal(idr[10]);
                    Decimal qtyDedicated = VAdvantage.Utility.Util.GetValueOfDecimal(idr[11]);

                    // continue if component line is quality correction
                    if (IsQualityCorr == "Y")
                    {
                        continue;
                    }

                    if (Qty == null)
                    {
                        if (wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder))
                        {
                            // qtyEntered = (qtyRequired.multiply(woQty)).subtract(qtyIssued).subtract(qtyAllocated).subtract(qtyDedicated);
                            qtyEntered = Decimal.Subtract(Decimal.Subtract(Decimal.Subtract(Decimal.Multiply(qtyRequired, woQty), qtyIssued), qtyAllocated), qtyDedicated);
                        }
                        else	//	automatically assume ComponentReturn txn
                        {
                            qtyEntered = Decimal.Subtract(qtyIssued, qtySpent);
                        }
                    }
                    else
                    {
                        qtyEntered = Decimal.Multiply(qtyRequired, Qty.Value);
                        if (wot.GetVAMFG_WorkOrderTxnType().Equals(X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_ComponentReturnFromWorkOrder)
                            //&& qtyEntered.setScale(MUOM.getPrecision(ctx, uomID), Decimal.ROUND_HALF_UP).compareTo(qtyIssued.subtract(qtySpent)) > 0)
                            && Decimal.Round((qtyEntered), VAdvantage.Model.MUOM.GetPrecision(ctx, uomID), MidpointRounding.AwayFromZero).CompareTo(Decimal.Subtract(qtyIssued, qtySpent)) > 0)
                        {
                            log.Warning("Not enough quantities to return from Work Order");
                            continue;
                        }
                    }

                    //	For Pull type: If quantity requirement has been filled or there are not enough quantities to issue
                    //	then don't generate a line -> goto next line processing
                    //if (qtyEntered.setScale(MUOM.getPrecision(ctx, uomID), Decimal.ROUND_HALF_UP).compareTo(Decimal.Zero) <= 0)
                    //    continue;
                    if (Decimal.Round((qtyEntered), VAdvantage.Model.MUOM.GetPrecision(ctx, uomID), MidpointRounding.AwayFromZero).CompareTo(Decimal.Zero) <= 0)
                        continue;

                    if (SupplyType.Equals(wocSupplyType)
                            || (wocSupplyType.Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_AssemblyPull) && assemblyPull
                                    && !SupplyType.Equals(X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_Push)))
                    {
                        ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine compIssueLine = new ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine(ctx, 0, trx);
                        //	set the client + org derived from the transaction header
                        compIssueLine.SetClientOrg(wot);
                        // Work done to add attributeset Instance ID for finished products
                        compIssueLine.SetRequiredColumns(M_WorkOrderTransaction_ID, productID, asiID, uomID, qtyEntered, wooID, basisType);

                        // Added by Bharat on 26 Dec 2017 to set Business Partner Information on Line
                        compIssueLine.SetC_BPartner_ID(wot.GetC_BPartner_ID());
                        compIssueLine.SetC_BPartner_Location_ID(wot.GetC_BPartner_Location_ID());

                        if (locatorID != 0)	// Implicit assumption : Only Pull components will have a locator populated in the WOC
                        {
                            // compIssueLine.SetM_Locator_ID(locatorID);
                            ((X_VAMFG_M_WrkOdrTrnsctionLine)compIssueLine).SetM_Locator_ID(locatorID);
                        }
                        else
                        {
                            // Check if the locator passed is under the Warehouse of the Work Order
                            // If no, then skip generating transaction line for this component
                            ViennaAdvantage.Model.MVAMFGMWorkOrder wo = new ViennaAdvantage.Model.MVAMFGMWorkOrder(ctx, wot.GetVAMFG_M_WorkOrder_ID(), trx);
                            MLocator loc = new MLocator(ctx, M_Locator_ID, trx);
                            if (loc.GetM_Warehouse_ID() != wo.GetM_Warehouse_ID())
                            {
                                log.Warning("Locator passed is not under the Warehouse of the WorkOrder");
                                continue;
                            }
                            ((X_VAMFG_M_WrkOdrTrnsctionLine)compIssueLine).SetM_Locator_ID(M_Locator_ID);
                        }

                        if (asiID > 0)
                            compIssueLine.SetM_AttributeSetInstance_ID(asiID);
                        compIssueLine.SetVAMFG_Line(compLineNo);
                        compLineNo += 10;

                        // Added by Bharat on 20/12/2016 to Set Density and Liter values for production execution Process of Gulf Oil.

                        if (_countGOM01 > 0)
                        {
                            Decimal density = wot.GetGOM01_Density();
                            QtyInKg = Decimal.Multiply(qtyEntered, density);
                            QtyInKg = Decimal.Round((QtyInKg), VAdvantage.Model.MUOM.GetPrecision(ctx, uomID));
                            Decimal ltrQty = density > 0 ? QtyInKg / density : 0;
                            compIssueLine.SetGOM01_Quantity(QtyInKg);
                            compIssueLine.SetGOM01_ActualQuantity(QtyInKg);
                            compIssueLine.SetGOM01_Density(density);
                            compIssueLine.SetGOM01_Litre(Decimal.Round((ltrQty), VAdvantage.Model.MUOM.GetPrecision(ctx, uomID)));
                            compIssueLine.SetGOM01_FromProcess(true);
                        }
                        // End Bharat


                        wotLines.Add(compIssueLine);

                        VAdvantage.Model.MProduct product = new VAdvantage.Model.MProduct(ctx, productID, trx);
                        if (checkInventory)
                        {
                            if (_countGOM01 > 0)
                            {
                                response.Append(product.GetName() + ": ").Append(VerifyQuantity(product, wot, QtyInKg, asiID)).Append(" ");
                            }
                            else
                            {
                                response.Append(product.GetName() + ": ").Append(VerifyQuantity(product, wot, qtyEntered, asiID)).Append(" ");
                            }
                        }

                        if (compIssueLine.Save())
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, sqlBuf.ToString(), e);
                log.Severe("SQL failure in checking component requirements");
                return null;
            }
            finally
            {
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
            }

            //Not using this here we have save line one by one
            //if (save)
            //{
            //    if (!VAdvantage.Model.PO.SaveAll(trx, wotLines))
            //    {
            //        log.Severe("Could not save component transaction lines.");
            //        return null;
            //    }
            //}
            log.SaveInfo("Info", response.ToString());
            //return wotLines.toArray(new MVAMFGMWrkOdrTrnsctionLine[] { });
            ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine[] newObject = null;

            try
            {
                newObject = (ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine[])wotLines.ToArray(typeof(ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine));

                //newObject = new MVAMFGMWrkOdrTrnsctionLine[] { };
                //newObject = Convert.ChangeType(wotLines.ToArray(), typeof(MVAMFGMWrkOdrTrnsctionLine[]));
                //newObject = lst.ToArray();
                return newObject;
            }
            catch { }
            return (ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine[])wotLines.ToArray();

        }

        //	API to add component transaction lines in M_WorkOrderTransactionLine. Called from MWarehouseTask for WMS integration. 
        /**
         * Api to add component transaction lines.
         * @param ctx
         * @param VAMFG_M_WorkOrder_ID Work Order
         * @param M_WorkOrderComponent_ID Work Order Component
         * @param Qty Number of components to be issued
         * @param M_Locator_ID Supply Locator for the component
         * @param trx
         * @return int M_WorkOrderTransactionLine_ID
         */
        public int AddComponentTxnLine(Ctx ctx, int M_WorkOrderComponent_ID, Decimal Qty, int M_Locator_ID, Trx trx)
        {

            ViennaAdvantage.Model.MVAMFGMWorkOrderComponent woc = new ViennaAdvantage.Model.MVAMFGMWorkOrderComponent(ctx, M_WorkOrderComponent_ID, trx);
            MVAMFGMWorkOrderOperation woo = new MVAMFGMWorkOrderOperation(ctx, woc.GetVAMFG_M_WorkOrderOperation_ID(), trx);
            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot = RetrieveWOTxn(ctx, woo.GetVAMFG_M_WorkOrder_ID(), X_VAMFG_M_WrkOdrTransaction.VAMFG_WORKORDERTXNTYPE_1_ComponentIssueToWorkOrder, trx);
            if (wot == null)
            {
                log.Severe("Cannot create or retrieve WO Txn.");
                return 0;
            }

            ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine wotLine = new ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine(ctx, 0, trx);
            wotLine.SetRequiredColumns(wot.GetVAMFG_M_WrkOdrTransaction_ID(), woc.GetM_Product_ID(), woc.GetM_AttributeSetInstance_ID(), woc.GetC_UOM_ID(), Qty, woc.GetVAMFG_M_WorkOrderOperation_ID(), woc.GetBasisType());
            wotLine.SetC_BPartner_ID(wot.GetC_BPartner_ID());
            wotLine.SetC_BPartner_Location_ID(wot.GetC_BPartner_Location_ID());
            wotLine.SetM_Locator_ID(M_Locator_ID);

            if (save)
            {
                if (!wotLine.Save(trx))
                {
                    log.Severe("Could not save component transaction line.");
                    return 0;
                }
            }

            return wotLine.GetVAMFG_M_WrkOdrTrnsctionLine_ID();
        }


        /**
         * verifyQuantity - checks that the warehouse specified in the work order transaction
         * has sufficient quantity of the product
         * @param product
         * @param wot
         * @param qty
         * @param asiID
         * @return error message if any, else Quantity Available
         */
        private String VerifyQuantity(VAdvantage.Model.MProduct product, ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction wot, Decimal qty, int asiID)
        {
            if (product.IsStocked())
            {
                ViennaAdvantage.Model.MVAMFGMWorkOrder wo = new ViennaAdvantage.Model.MVAMFGMWorkOrder(wot.GetCtx(), wot.GetVAMFG_M_WorkOrder_ID(), wot.Get_TrxName());
                int M_Warehouse_ID = wo.GetM_Warehouse_ID();

                //logic change by Raghu
                //Decimal available = ViennaAdvantage.Model.Storage.GetQtyAvailable
                //(M_Warehouse_ID, product.GetM_Product_ID(), asiID, null);

                // during creation of production execution line, reserverd qty to be checked or not
                Decimal? available = 0.0M;
                if (VAdvantage.Utility.Util.GetValueOfString(wot.GetConsiderReservedQty()) == "N")
                {
                    try
                    {
                        available = MStorage.GetQtyAvailableWithoutReserved
                          (M_Warehouse_ID, product.GetM_Product_ID(), asiID, null);
                    }
                    catch
                    {
                        return Msg.GetMsg(wot.GetCtx(), "PleaseUpdateVAFramework");
                    }
                }
                else
                {
                    available = MStorage.GetQtyAvailable
                   (M_Warehouse_ID, product.GetM_Product_ID(), asiID, null);
                }

                if (available == null)
                    available = Env.ZERO;
                if (Env.Signum(available.Value) == 0)
                {
                    return Msg.GetMsg(wot.GetCtx(), "NoQtyAvailable", "0");
                }
                else if (available.Value.CompareTo(qty) < 0)
                {
                    return Msg.GetMsg(wot.GetCtx(), "InsufficientQtyAvailable", available.ToString());
                }
            }
            return Msg.GetMsg(wot.GetCtx(), "QtyAvailable");
        } // verifyQuantity

    }
}

