using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
//using VAdvantage.Common;
using ViennaAdvantage.Process;
//using System.Windows.Forms;
//using ViennaAdvantage.Model;
using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using VAdvantage.Utility;
using System.Data;
using System.Data.SqlClient;
using VAdvantage.Logging;
using ViennaAdvantage.Model;
using VAdvantage.Model;

namespace ViennaAdvantage.Model
{
    public class MVAMFGMWrkOdrTrnsctionLine : X_VAMFG_M_WrkOdrTrnsctionLine
    {
        private static new VLogger log = VLogger.GetVLogger(typeof(MVAMFGMWrkOdrTrnsctionLine).FullName);
        private static long serialVersionUID = 1L;
        public MVAMFGMWrkOdrTrnsctionLine(Ctx ctx, int VAMFG_M_WrkOdrTrnsctionLine_ID, Trx trx)
            : base(ctx, VAMFG_M_WrkOdrTrnsctionLine_ID, trx)
        {
            //base(ctx, M_WorkOrderTransactionLine_ID, trx);

            MVAMFGMWrkOdrTransaction wot = new MVAMFGMWrkOdrTransaction(ctx, GetVAMFG_M_WrkOdrTransaction_ID(), trx);
            SetClientOrg(wot);

            //  New
            if (VAMFG_M_WrkOdrTrnsctionLine_ID == 0)
            {
                base.SetProcessed(false);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ctx"> ctx</param>
        /// <param name="dr">rs</param>
        /// <param name="trx">trx</param>
        public MVAMFGMWrkOdrTrnsctionLine(Ctx ctx, DataRow dr, Trx trx)
            : base(ctx, dr, trx)
        {
            //base(ctx, rs, trx);
        }

        /// <summary>
        /// Set Product
        /// </summary>
        /// <param name="product">product product</param>
        public void SetProduct(VAdvantage.Model.MProduct product)
        {
            if (product != null)
            {
                SetM_Product_ID(product.GetM_Product_ID());
                SetC_UOM_ID(product.GetC_UOM_ID());
            }
            else
            {
                SetM_Product_ID(0);
                Set_ValueNoCheck("C_UOM_ID", null);
            }
            SetM_AttributeSetInstance_ID(0);
        }	//	setProduct

        /// <summary>
        /// Set M_Product_ID
        ///  </summary>
        ///  <param name="M_Product_ID">M_Product_ID product</param>
        /// <param name="setUOM">setUOM set also UOM</param>
        public void SetM_Product_ID(int M_Product_ID, Boolean setUOM)
        {
            if (setUOM)
                SetProduct(VAdvantage.Model.MProduct.Get(GetCtx(), M_Product_ID));
            else
                base.SetM_Product_ID(M_Product_ID);

            SetM_AttributeSetInstance_ID(0);
        }	//	setM_Product_ID

        /// <summary>
        /// This sets the required (not null) columns besides the ones that are standard across all POs.
        /// </summary>
        ///  <param name="workOrderTransactionID">workOrderTransactionID</param>
        ///  <param name="productID">productID</param>
        /// <param name="uomID"> qty</param>
        /// <param name="qty"></param>
        /// <param name="operationID"></param>
        /// <param name="basisType"></param>
        public void SetRequiredColumns(int workOrderTransactionID, int productID, int AttributeSet_ID, int uomID, Decimal qty, int operationID, String basisType)
        {
            SetVAMFG_M_WrkOdrTransaction_ID(workOrderTransactionID);

            if (uomID > 0)
            {
                SetM_Product_ID(productID);
                // set Attributeset Instance ID also on work order transaction line
                // added by vivek on 16/12/2017 assigned by pradeep
                SetM_AttributeSetInstance_ID(AttributeSet_ID);
                SetC_UOM_ID(uomID);
            }
            else
            {
                SetM_Product_ID(productID, true);
                // set Attributeset Instance ID also on work order transaction line
                // added by vivek on 16/12/2017 assigned by pradeep
                SetM_AttributeSetInstance_ID(AttributeSet_ID);
            }
            //setQtyEntered(qty.setScale(MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()), BigDecimal.ROUND_HALF_UP));
            SetVAMFG_QtyEntered(Decimal.Round((qty), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID())));
            SetVAMFG_M_WorkOrderOperation_ID(operationID);
            SetBasisType(basisType);
            // set BP details while creating component Txn lines  added by vivek on 04/01/2018
            MVAMFGMWrkOdrTransaction wot = new MVAMFGMWrkOdrTransaction(GetCtx(), workOrderTransactionID, Get_TrxName());
            SetC_BPartner_ID(wot.GetC_BPartner_ID());
            SetC_BPartner_Location_ID(wot.GetC_BPartner_Location_ID());
            SetAD_User_ID(wot.GetAD_User_ID());

            VAdvantage.Model.MRole role = VAdvantage.Model.MRole.GetDefault(GetCtx(), false);
            String sql = "SELECT COALESCE(MAX(VAMFG_Line),0)+10 AS DefaultValue FROM VAMFG_M_WrkOdrTrnsctionLine WHERE VAMFG_M_WrkOdrTransaction_ID=@param1";
            sql = role.AddAccessSQL(sql, "VAMFG_M_WrkOdrTrnsctionLine", VAdvantage.Model.MRole.SQL_NOTQUALIFIED, VAdvantage.Model.MRole.SQL_RO);
            //PreparedStatement pstmt = DB.prepareStatement(sql, (Trx) null);
            //ResultSet rs = null;
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", workOrderTransactionID);
                idr = DB.ExecuteReader(sql, param, null);
                if (idr.Read())
                {
                    SetVAMFG_Line(VAdvantage.Utility.Util.GetValueOfInt(idr[0]));
                }
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                //pstmt.setInt(1, workOrderTransactionID);
                //rs = pstmt.executeQuery();

                //if (rs.next())
                //{
                //    setLine(rs.getInt(1));
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
            }


        } // setRequiredColumns	

        /// <summary>
        /// Set Product - Callout
        /// </summary>
        ///  <param name="oldM_Product_ID"> oldM_Product_ID old value</param>
        ///  <param name="newM_Product_ID">newM_Product_ID new value</param>
        /// <param name="windowNo">windowNo window</param>

        public void SetM_Product_ID(String oldM_Product_ID,
            String newM_Product_ID, int windowNo)
        {
            if (newM_Product_ID == null || newM_Product_ID.Length == 0)
            {
                SetM_AttributeSetInstance_ID(0);
                return;
            }
            int M_Product_ID = VAdvantage.Utility.Util.GetValueOfInt(newM_Product_ID);
            base.SetM_Product_ID(M_Product_ID);
            //SetM_Product_ID(M_Product_ID);
            if (M_Product_ID == 0)
            {
                SetM_AttributeSetInstance_ID(0);
                return;
            }

            //	Set Attribute
            int M_AttributeSetInstance_ID = GetCtx().GetContextAsInt(EnvConstants.WINDOW_INFO, EnvConstants.TAB_INFO, "M_AttributeSetInstance_ID");
            if (GetCtx().GetContextAsInt(EnvConstants.WINDOW_INFO, EnvConstants.TAB_INFO, "M_Product_ID") == M_Product_ID
                    && M_AttributeSetInstance_ID != 0)
                SetM_AttributeSetInstance_ID(M_AttributeSetInstance_ID);
            else
                SetM_AttributeSetInstance_ID(0);

            VAdvantage.Model.MProduct product = VAdvantage.Model.MProduct.Get(GetCtx(), M_Product_ID);

            //
            SetC_UOM_ID(product.GetC_UOM_ID());
        }	//	setM_Product_ID

        /// <summary>
        /// 	Set (default) Locator based on qty.
        /// 	
        /// 	</summary>
        /// 	<param name="Qty">Qty quantity</param>
        public void SetM_Locator_ID(Decimal Qty)
        {
            //	Locator established
            if (GetM_Locator_ID() != 0)
                return;
            //	No Product
            if (GetM_Product_ID() == 0)
            {
                Set_ValueNoCheck("M_Locator_ID", null);
                return;
            }

            VAdvantage.Model.MOrg org = new VAdvantage.Model.MOrg(GetCtx(), GetAD_Org_ID(), Get_TrxName());

            //	Get existing Location
            int M_Locator_ID = VAdvantage.Model.MStorage.GetM_Locator_ID(org.GetM_Warehouse_ID(),
                    GetM_Product_ID(), GetM_AttributeSetInstance_ID(),
                    Qty, Get_TrxName());
            //	Get default Location
            if (M_Locator_ID == 0)
            {
                VAdvantage.Model.MProduct product = VAdvantage.Model.MProduct.Get(GetCtx(), GetM_Product_ID());
                M_Locator_ID = VAdvantage.Model.MProductLocator.GetFirstM_Locator_ID(product, org.GetM_Warehouse_ID());
                if (M_Locator_ID == 0)
                {
                    VAdvantage.Model.MWarehouse wh = VAdvantage.Model.MWarehouse.Get(GetCtx(), org.GetM_Warehouse_ID());
                    M_Locator_ID = wh.GetDefaultM_Locator_ID();
                }
            }
            base.SetM_Locator_ID(M_Locator_ID);
        }	//	setM_Locator_ID

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
        ///</summary>
        /// <returns> true of it can be deleted</returns>
        protected override Boolean BeforeDelete()
        {
            MVAMFGMWrkOdrTransaction workOrderTxn = new MVAMFGMWrkOdrTransaction(GetCtx(), GetVAMFG_M_WrkOdrTransaction_ID(), Get_TrxName());
            if (workOrderTxn.IsProcessed())
                return false;

            if (IsProcessed())
                return false;

            return true;
        }	//	beforeDelete

        /// <summary>
        /// Is used to save record pn production execution line
        /// </summary>
        /// <param name="newRecord">newRecord</param>
        /// <returns>TRUE if success</returns>
        protected override Boolean BeforeSave(bool newRecord)
        {
            if (GetVAMFG_Line() < 0)
            {
                log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@Line@ < 0"));
                return false;
            }

            MVAMFGMWrkOdrTransaction wot = new MVAMFGMWrkOdrTransaction(GetCtx(), GetVAMFG_M_WrkOdrTransaction_ID(), Get_TrxName());
            string _sql = "Select Sum(GOM01_ActualQuantity) as QtyEntered From VAMFG_M_WrkOdrTrnsctionLine Where VAMFG_M_WrkOdrTransaction_ID=" + GetVAMFG_M_WrkOdrTransaction_ID() + " AND M_Product_ID=" + GetM_Product_ID() + ""
                        + " AND M_AttributeSetInstance_ID=" + GetM_AttributeSetInstance_ID();
            decimal Qty = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql));
            if (Qty > 0)
            {
                Qty = Qty + GetGOM01_ActualQuantity();
                string qry = "SELECT currentqty FROM M_Transaction WHERE M_Transaction_ID = (SELECT MAX(M_Transaction_ID)   FROM M_Transaction  WHERE movementdate = " +
                            " (SELECT MAX(movementdate) FROM M_Transaction WHERE movementdate <= " + GlobalVariable.TO_DATE(wot.GetVAMFG_DateTrx(), true) + " AND  M_Product_ID = " + GetM_Product_ID() + " AND M_Locator_ID = " + GetM_Locator_ID() +
                            " AND M_AttributeSetInstance_ID = " + GetM_AttributeSetInstance_ID() + ") AND  M_Product_ID = " + GetM_Product_ID() + " AND M_Locator_ID = " + GetM_Locator_ID() +
                            " AND M_AttributeSetInstance_ID = " + GetM_AttributeSetInstance_ID() + ") AND AD_Org_ID = " + GetAD_Org_ID() + " AND  M_Product_ID = " + GetM_Product_ID() +
                            " AND M_Locator_ID = " + GetM_Locator_ID() + " AND M_AttributeSetInstance_ID = " + GetM_AttributeSetInstance_ID();
                decimal CurrentQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(qry));
                if (CurrentQty < Qty)
                {
                    log.SaveError("Error", Msg.Translate(GetCtx(), "Insufficient qty in warehouse for the product"));
                    return false;
                }
            }

            // get current cost from product cost on new record and when product changed
            // currency conversion also required if order has different currency with base currency
            if ((newRecord || (Is_ValueChanged("M_Product_ID"))) && GetM_Product_ID() > 0)
            {
                decimal currentcostprice = MCost.GetproductCosts(GetAD_Client_ID(), GetAD_Org_ID(), GetM_Product_ID(), VAdvantage.Utility.Util.GetValueOfInt(GetM_AttributeSetInstance_ID()), Get_TrxName());
                decimal currentQty = GetCurrentQtyFromMCost(GetAD_Client_ID(), GetAD_Org_ID(), GetM_Product_ID(), VAdvantage.Utility.Util.GetValueOfInt(GetM_AttributeSetInstance_ID()), Get_TrxName());
                bool isGomel = Env.IsModuleInstalled("GOM01_");
                //MVAMFGMWrkOdrTransaction wot = new MVAMFGMWrkOdrTransaction(GetCtx(), GetVAMFG_M_WrkOdrTransaction_ID(), Get_TrxName());
                if (wot.GetVAMFG_Description() != null && wot.GetVAMFG_Description().Contains("{->"))
                {
                    // not to set cuurent cost price on reversal because its already filed during creation of line
                }
                else if (isGomel && Get_ColumnIndex("GOM01_ActualQuantity") > 0 && GetGOM01_ActualQuantity() <= currentQty)
                {
                    SetCurrentCostPrice(currentcostprice);
                }
                else if (!isGomel) // if gomel module is not available, then update currenct cost price on save
                {
                    SetCurrentCostPrice(currentcostprice);
                }
            }

            if (Is_ValueChanged("VAMFG_QtyEntered"))
            {
                //BigDecimal qtyEntered = GetVAMFG_QtyEntered().setScale(MUOM.getPrecision(getCtx(), getC_UOM_ID()), BigDecimal.ROUND_HALF_UP);
                Decimal qtyEntered = Decimal.Round((GetVAMFG_QtyEntered()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()));
                if (qtyEntered.CompareTo(GetVAMFG_QtyEntered()) != 0)
                {
                    log.Fine("Corrected QtyEntered Scale UOM=" + GetC_UOM_ID()
                            + "; QtyEntered =" + GetVAMFG_QtyEntered() + "->" + qtyEntered);
                    SetVAMFG_QtyEntered(qtyEntered);
                }
            }

            return true;
        }

        /// <summary>
        /// Get current qty from product cost against Primary Accounting Schema 
        /// pick Costing Method either from Product Category or From Accounting Schema
        /// pick Costing Level either from Product Category or From Accounting Schema 
        /// </summary>
        /// <param name="client_Id">client ID</param>
        /// <param name="org_Id">org ID</param>
        /// <param name="product_id">product ID</param>
        /// <param name="M_ASI_Id">AttributeSetInsatnce ID</param>
        /// <param name="trxName">Transaction</param>
        /// <returns>currentt qty aginst defined costing method or costing level</returns>
        public decimal GetCurrentQtyFromMCost(int client_Id, int org_Id, int product_id, int M_ASI_Id, Trx trxName)
        {
            Decimal currenQty = 0;
            try
            {
                string sql = @"SELECT ROUND(AVG(CST.CURRENTQTY),4 )   FROM M_PRODUCT P   INNER JOIN M_COST CST   ON P.M_PRODUCT_ID=CST.M_PRODUCT_ID
                               LEFT JOIN M_PRODUCT_CATEGORY PC   ON P.M_PRODUCT_CATEGORY_ID=PC.M_PRODUCT_CATEGORY_ID  
                               INNER JOIN C_ACCTSCHEMA ACC   ON CST.C_ACCTSCHEMA_ID=ACC.C_ACCTSCHEMA_ID
                               INNER JOIN M_COSTELEMENT CE  ON CST.M_COSTELEMENT_ID=CE.M_COSTELEMENT_ID
                              WHERE (( CASE WHEN PC.COSTINGMETHOD IS NOT NULL  THEN PC.COSTINGMETHOD
                                            ELSE ACC.COSTINGMETHOD  END) = CE.COSTINGMETHOD )
                              AND ((   CASE WHEN PC.COSTINGMETHOD IS NOT NULL  AND PC.COSTINGMETHOD   = 'C'  THEN PC.M_costelement_id                                           
                                            WHEN PC.COSTINGMETHOD IS NOT NULL  THEN (SELECT M_CostElement_ID FROM M_costelement 
                                             WHERE COSTINGMETHOD = pc.COSTINGMETHOD AND ad_client_id    = " + client_Id + @" )
                                            WHEN ACC.COSTINGMETHOD IS NOT NULL AND ACC.COSTINGMETHOD   = 'C' THEN ACC.M_costelement_id ELSE
                                             (SELECT M_CostElement_ID FROM M_costelement WHERE COSTINGMETHOD = acc.COSTINGMETHOD 
                                             AND ad_client_id    = " + client_Id + @" ) END) = ce.M_COSTELEMENT_id)
                             AND ((    CASE WHEN PC.COSTINGLEVEL IS NOT NULL  AND PC.COSTINGLEVEL  IN  ('A' ,  'O')  THEN " + org_Id + @"
                                            WHEN ACC.COSTINGLEVEL IS NOT NULL AND ACC.COSTINGLEVEL   IN  ('A' ,  'O' ) THEN " + org_Id + @"
                                            ELSE 0  END) = CST.AD_Org_ID)
                            AND ((     CASE WHEN PC.COSTINGLEVEL IS NOT NULL  AND PC.COSTINGLEVEL   IN  ('A' ,   'B' ) THEN " + M_ASI_Id + @"
                                            WHEN ACC.COSTINGLEVEL IS NOT NULL AND ACC.COSTINGLEVEL   IN  ('A' ,   'B' ) THEN " + M_ASI_Id + @"
                                            ELSE 0   END) = NVL(CST.M_AttributeSetInstance_ID , 0))
                            AND P.M_PRODUCT_ID      =" + product_id + @"
                            AND CST.C_ACCTSCHEMA_ID = (SELECT c_acctschema1_id FROM ad_clientinfo WHERE ad_client_id = " + client_Id + " )";
                currenQty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, trxName));
            }
            catch
            {
                throw new ArgumentException("Error in getting currentQty from GetProductCosts");
            }
            return currenQty;
        }

        protected override Boolean AfterSave(bool newRecord, bool success)
        {
            Tuple<String, String, String> mInfo = null;
            if (Env.HasModulePrefix("GOM01_", out mInfo))
            {
                string qry = "SELECT SUM(GOM01_ActualQuantity) FROM VAMFG_M_WrkOdrTrnsctionLine WHERE VAMFG_M_WrkOdrTransaction_ID = " + GetVAMFG_M_WrkOdrTransaction_ID();
                decimal qty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(qry, null, Get_TrxName()));
                MVAMFGMWrkOdrTransaction wrk = new MVAMFGMWrkOdrTransaction(GetCtx(), GetVAMFG_M_WrkOdrTransaction_ID(), Get_TrxName());
                wrk.SetGOM01_ActualQuantity(qty);
                if (!wrk.Save())
                {
                    //Error
                    log.SaveError("Error", Msg.Translate(GetCtx(), "Error while saving Production Execution"));
                    return false;
                }
            }
            return true;
        }

        protected override Boolean AfterDelete(bool success)
        {
            Tuple<String, String, String> mInfo = null;
            if (Env.HasModulePrefix("GOM01_", out mInfo))
            {
                string qry = "SELECT SUM(GOM01_ActualQuantity) FROM VAMFG_M_WrkOdrTrnsctionLine WHERE VAMFG_M_WrkOdrTransaction_ID = " + GetVAMFG_M_WrkOdrTransaction_ID();
                decimal qty = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(qry, null, Get_TrxName()));
                MVAMFGMWrkOdrTransaction wrk = new MVAMFGMWrkOdrTransaction(GetCtx(), GetVAMFG_M_WrkOdrTransaction_ID(), Get_TrxName());
                wrk.SetGOM01_ActualQuantity(qty);
                if (!wrk.Save())
                {
                    //Error
                    log.SaveError("Error", Msg.Translate(GetCtx(), "Error while saving Production Execution"));
                    return false;
                }
            }
            return true;
        }

        //@UICallout
        public void setQtyEntered(String oldQtyEntered,
            String newQtyEntered, int windowNo)
        {

            if (newQtyEntered == null || newQtyEntered.Trim().Length == 0)
                return;

            if (GetC_UOM_ID() == 0)
                return;

            Decimal QtyEntered = VAdvantage.Utility.Util.GetValueOfDecimal(newQtyEntered);
            //BigDecimal QtyEntered1 = QtyEntered.setScale(
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
    }

}