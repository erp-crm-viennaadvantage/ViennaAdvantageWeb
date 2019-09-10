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
using VAdvantage.ProcessEngine;
using ViennaAdvantage.CMFG.Model;
using ViennaAdvantage.CMFG.Util1;
using VAdvantage.Model;

namespace ViennaAdvantage.CMFG.Process
{
    public class GeneratePushLines : SvrProcess
    {
        private int p_M_WorkOrderTransaction_ID = 0;
        private Decimal p_Qty = Decimal.Zero;

        //@Override
        protected override void Prepare()
        {
            p_M_WorkOrderTransaction_ID = GetRecord_ID();

            ProcessInfoParameter[] para = GetParameter();
            foreach (ProcessInfoParameter element in para)
            {
                String name = element.GetParameterName();
                if (element.GetParameter() == null)
                {
                }

                else if (name.Equals("Quantity"))
                {
                    p_Qty = VAdvantage.Utility.Util.GetValueOfDecimal(element.GetParameter());
                }
                else
                    log.Log(Level.SEVERE, "Unknown Parameter: " + name);
            }
        }

        /**
         *  Perform process.
         *  @return Message
         *  @throws Exception
         */
        //@Override
        protected override String DoIt()
        {
            if (0 == p_M_WorkOrderTransaction_ID)
                throw new Exception("@FillMandatory@ @M_WorkOrderTransaction_ID@");
            ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction woTxn = new ViennaAdvantage.Model.MVAMFGMWrkOdrTransaction(GetCtx(), p_M_WorkOrderTransaction_ID, Get_TrxName());
            ViennaAdvantage.Model.MVAMFGMWorkOrder wo = new ViennaAdvantage.Model.MVAMFGMWorkOrder(GetCtx(), woTxn.GetVAMFG_M_WorkOrder_ID(), Get_TrxName());

            VAdvantage.Model.MBOM bom = new VAdvantage.Model.MBOM(GetCtx(), wo.GetM_BOM_ID(), Get_TrxName());
            MBOMProduct[] BOMproducts = MBOMProduct.GetOfBOM(bom);
            for (int i = 0; i < BOMproducts.Length; i++)
            {
                string prodensity = "SELECT nvl(GOM01_DENSITY,0) FROM VAMFG_M_WorkOrder WHERE VAMFG_M_WorkOrder_ID =" + woTxn.GetVAMFG_M_WorkOrder_ID();
                decimal DenQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(prodensity));
                if (DenQty == 0)
                {
                    DenQty = 1;
                }	

                MBOMProduct BOMproduct = BOMproducts[i];
                decimal qtyReqd = (p_Qty * BOMproduct.GetBOMQty()) * DenQty;

                //string qry = "SELECT currentqty FROM M_Transaction WHERE M_Transaction_ID = (SELECT MAX(M_Transaction_ID)   FROM M_Transaction  WHERE movementdate = " +
                //            " (SELECT MAX(movementdate) FROM M_Transaction WHERE movementdate <= " + GlobalVariable.TO_DATE(woTxn.GetVAMFG_DateTrx(), true) + " AND  M_Product_ID = " + BOMproduct.GetM_ProductBOM_ID() + " AND M_Locator_ID = " + woTxn.GetM_Locator_ID() +
                //            " AND M_AttributeSetInstance_ID = " + BOMproduct.GetM_AttributeSetInstance_ID() + ") AND  M_Product_ID = " + BOMproduct.GetM_ProductBOM_ID() + " AND M_Locator_ID = " + woTxn.GetM_Locator_ID() +
                //            " AND M_AttributeSetInstance_ID = " + BOMproduct.GetM_AttributeSetInstance_ID() + ") AND AD_Org_ID = " + woTxn.GetAD_Org_ID() + " AND  M_Product_ID = " + BOMproduct.GetM_ProductBOM_ID() +
                //            " AND M_Locator_ID = " + woTxn.GetM_Locator_ID() + " AND M_AttributeSetInstance_ID = " + BOMproduct.GetM_AttributeSetInstance_ID();
                //decimal CurrentQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(qry));
                //if (CurrentQty < qtyReqd)
                //{
                //    ViennaAdvantage.Model.MProduct product = new ViennaAdvantage.Model.MProduct(GetCtx(), BOMproduct.GetM_ProductBOM_ID(), Get_Trx());
                //    return "Insufficient qty in warehouse for : " + product.GetName();
                //}

                VAdvantage.Model.MStorage st = VAdvantage.Model.MStorage.Get(Env.GetCtx(), woTxn.GetM_Locator_ID(), BOMproduct.GetM_ProductBOM_ID(), BOMproduct.GetM_AttributeSetInstance_ID(), Get_TrxName());
                if (st == null)
                {
                    ViennaAdvantage.Model.MProduct product = new ViennaAdvantage.Model.MProduct(GetCtx(), BOMproduct.GetM_ProductBOM_ID(), Get_Trx());
                    return "Insufficient qty in warehouse for : " + product.GetName();
                }
                decimal CurrentQty = st.GetQtyOnHand();
                if (CurrentQty < qtyReqd)
                {
                    ViennaAdvantage.Model.MProduct product = new ViennaAdvantage.Model.MProduct(GetCtx(), BOMproduct.GetM_ProductBOM_ID(), Get_Trx());
                    return "Insufficient qty in warehouse for : " + product.GetName();
                }
            }

            if (p_Qty == 0)
            {
                //MVAMFGMWorkOrder wo = new MVAMFGMWorkOrder(GetCtx(), woTxn.GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
                // p_Qty = wo.GetVAMFG_QtyEntered().subtract(wo.GetVAMFG_QtyAssembled());
                string prdOrdQry = "SELECT SUM(wkt.VAMFG_QtyEntered) AS ProdOrder FROM VAMFG_M_WrkOdrTransaction wkt WHERE wkt.VAMFG_WorkOrderTxnType ='CI' AND wkt.M_Product_ID = "
                                    + woTxn.GetM_Product_ID() + " AND wkt.VAMFG_M_Workorder_ID = " + woTxn.GetVAMFG_M_WorkOrder_ID() + " AND wkt.DocStatus ='CO'";

                Decimal ProdOrdQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(prdOrdQry, null, Get_TrxName()));
                p_Qty = Decimal.Subtract(wo.GetVAMFG_QtyEntered(), (ProdOrdQty));
                //p_Qty = Decimal.Subtract(wo.GetVAMFG_QtyEntered(), (wo.GetVAMFG_QtyAssembled()));

                //log.Info ("@Quantity@ = " + wo.GetVAMFG_QtyEntered().subtract(wo.GetVAMFG_QtyAssembled().add(wo.GetVAMFG_QtyScrapped())));
                log.Info("@Quantity@ = " + Decimal.Subtract(wo.GetVAMFG_QtyEntered(), Decimal.Add(wo.GetVAMFG_QtyAssembled(), (wo.GetVAMFG_QtyScrapped()))));
            }

            //woTxn.SetVAMFG_QtyEntered(p_Qty.setScale(MUOM.GetPrecision(GetCtx(), woTxn.GetC_UOM_ID()), Decimal.ROUND_HALF_UP));
            woTxn.SetVAMFG_QtyEntered(Decimal.Round((p_Qty), VAdvantage.Model.MUOM.GetPrecision(woTxn.GetCtx(), woTxn.GetC_UOM_ID()), MidpointRounding.AwayFromZero));
            // Added by Bharat on 20/12/2016 to Set Density and Liter values for production execution Process of Gulf Oil.
            Tuple<String, String, String> mInfo = null;
            if (Env.HasModulePrefix("GOM01_", out mInfo))
            {
                woTxn.SetGOM01_Density(wo.GetGOM01_Density());
                Decimal qtyKg = Decimal.Multiply(wo.GetGOM01_Density(), woTxn.GetVAMFG_QtyEntered());
                woTxn.SetGOM01_Quantity(Decimal.Round((qtyKg), MUOM.GetPrecision(woTxn.GetCtx(), woTxn.GetC_UOM_ID()), MidpointRounding.AwayFromZero));
            }
            woTxn.Save();

            ViennaAdvantage.Process.MWorkOrderTxnUtil prodTxnLines = new ViennaAdvantage.Process.MWorkOrderTxnUtil(true);
            // Done by Bharat on 24 Jan 2018 to delete lines as when process runs multiple times it creates duplicate lines.
            int no = DB.ExecuteQuery("DELETE FROM VAMFG_M_WrkOdrTrnsctionLine WHERE VAMFG_M_WrkOdrTransaction_ID = " + p_M_WorkOrderTransaction_ID, null, Get_TrxName());

            ViennaAdvantage.Model.MVAMFGMWrkOdrTrnsctionLine[] wotlines = prodTxnLines.GenerateComponentTxnLine(GetCtx(), p_M_WorkOrderTransaction_ID, p_Qty,
                      X_VAMFG_M_WorkOrderComponent.VAMFG_SUPPLYTYPE_Push, Get_TrxName());

            if (wotlines != null && wotlines.Length > 0)
                return "Generated " + wotlines.Length + " line(s) for component(s): " + VLogger.RetrieveInfo().GetName();
            else
                return "Generated 0 lines for components.";

        }
    }
}
