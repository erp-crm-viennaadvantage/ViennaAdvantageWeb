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
using ViennaAdvantage.Model;
using VAdvantage.Model;
using ViennaAdvantage.CMFG.Model;

namespace ViennaAdvantage.Model
{
    public class MVAMFGMWorkOrderComponent : X_VAMFG_M_WorkOrderComponent
    {
        private static new VLogger log = VLogger.GetVLogger(typeof(MVAMFGMWorkOrderComponent).FullName);
        private static long serialVersionUID = 1L;
        public MVAMFGMWorkOrderComponent(Ctx ctx, int VAMFG_M_WorkOrderComponent_ID, Trx trx)
            : base(ctx, VAMFG_M_WorkOrderComponent_ID, trx)
        {
            //super(ctx, VAMFG_M_WorkOrderComponent_ID, trx);

            if (VAMFG_M_WorkOrderComponent_ID == 0)
            {
                SetVAMFG_Line(0);
                SetM_AttributeSetInstance_ID(0);
                SetProcessed(false);
                SetVAMFG_QtyRequired(Env.ZERO);
                SetVAMFG_QtyAvailable(Env.ZERO);
                SetVAMFG_QtySpent(Env.ZERO);
                SetVAMFG_QtyAllocated(Env.ZERO);
                SetVAMFG_QtyDedicated(Env.ZERO);
            }
        }

        /// <summary>
        ///  Parent Constructor.
        /// </summary>
        /// <param name="workorderoperation"></param>
        /// <param name="workorder"></param>
        public MVAMFGMWorkOrderComponent(MVAMFGMWorkOrderOperation workorderoperation, MVAMFGMWorkOrder workorder)
            : this(workorderoperation.GetCtx(), 0, workorderoperation.Get_TrxName())
        {
            if (workorderoperation.Get_ID() == 0)
                throw new Exception("Header not saved");
            SetVAMFG_M_WorkOrderOperation_ID(workorderoperation.GetVAMFG_M_WorkOrderOperation_ID());	//	parent
            SetWorkOrder(workorder);
        }	//	MWorkOrderComponent

        /// <summary>
        /// constructor called from BOM Explode
        /// assumes MLocator as null (not specified)
        /// </summary>
        /// <param name="workorder"></param>
        /// <param name="workorderoperation"></param>
        /// <param name="product"></param>
        /// <param name="QtyRequired"></param>
        /// <param name="SupplyType"></param>
        public MVAMFGMWorkOrderComponent(MVAMFGMWorkOrder workorder, MVAMFGMWorkOrderOperation workorderoperation, ViennaAdvantage.Model.MProduct product, Decimal QtyRequired,
                String SupplyType, int M_AttributeSetInstance_ID)
            : this(workorder, workorderoperation, product, QtyRequired, SupplyType, M_AttributeSetInstance_ID, null)
        {

        }

        /// <summary>
        /// Constructor called from BOM Drop
        /// </summary>
        /// <param name="workorder"></param>
        /// <param name="workorderoperation"></param>
        /// <param name="product"></param>
        /// <param name="QtyRequired"></param>
        /// <param name="SupplyType"></param>
        /// <param name="locator"></param>
        public MVAMFGMWorkOrderComponent(ViennaAdvantage.Model.MVAMFGMWorkOrder workorder, MVAMFGMWorkOrderOperation workorderoperation, ViennaAdvantage.Model.MProduct product, Decimal QtyRequired,
                String SupplyType, int M_AttributeSetInstance_ID, VAdvantage.Model.MLocator locator)
            : this(workorderoperation.GetCtx(), 0, workorderoperation.Get_TrxName())
        {

            if (workorderoperation.Get_ID() == 0)
                throw new Exception("Header not saved");
            SetVAMFG_M_WorkOrderOperation_ID(workorderoperation.GetVAMFG_M_WorkOrderOperation_ID());	//	parent
            SetM_Product_ID(product.GetM_Product_ID());
            SetC_UOM_ID(product.GetC_UOM_ID());
            SetVAMFG_QtyRequired(QtyRequired);
            SetVAMFG_SupplyType(SupplyType);
            // Changes done b y Vivek Kumar assigned by Mukesh on 16/11/2017
            // Changes done to save AttributesetInstance at Work Order component
            SetM_AttributeSetInstance_ID(M_AttributeSetInstance_ID);
            if (locator != null)
                SetM_Locator_ID(locator.GetM_Locator_ID());

            SetWorkOrder(workorder);
        }

        private MVAMFGMWorkOrderOperation headerInfo = null;

        /// <summary>
        /// Load Constructor
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="rs"></param>
        /// <param name="trx"></param>
        public MVAMFGMWorkOrderComponent(Ctx ctx, DataRow dr, Trx trx)
            : base(ctx, dr, trx)
        {
            //super(ctx, rs, trx);
        }

        /** Logger								*/
        //private static CLogger	s_log = CLogger.getCLogger (MWorkOrderComponent.class);

        /// <summary>
        /// Get Production BOM Components of Work Order
        /// </summary>
        /// <param name="workorder"></param>
        /// <param name="whereClause"></param>
        /// <param name="orderClause"></param>
        /// <returns></returns>
        public static MVAMFGMWorkOrderComponent[] GetOfWorkOrder(MVAMFGMWorkOrder workorder, String whereClause, String orderClause)
        {
            StringBuilder sqlstmt = new StringBuilder("SELECT * FROM VAMFG_M_WorkOrderComponent WHERE VAMFG_M_WorkOrder_ID=@param1");
            if (whereClause != null)
                sqlstmt.Append("AND ").Append(whereClause);
            if (orderClause != null)
                sqlstmt.Append(" ORDER BY ").Append(orderClause);
            String sql = sqlstmt.ToString();
            //ArrayList<MWorkOrderComponent> list = new ArrayList<MWorkOrderComponent>();
            List<MVAMFGMWorkOrderComponent> list = new List<MVAMFGMWorkOrderComponent>();
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            //PreparedStatement pstmt = DB.prepareStatement (sql, workorder.get_Trx());
            //ResultSet rs = null;
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", workorder.GetVAMFG_M_WorkOrder_ID());
                idr = DB.ExecuteReader(sql, param, workorder.Get_TrxName());
                dt.Load(idr);
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list.Add(new MVAMFGMWorkOrderComponent(workorder.GetCtx(), dt.Rows[i], workorder.Get_TrxName()));
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
                return null;
            }

            MVAMFGMWorkOrderComponent[] retValue = new MVAMFGMWorkOrderComponent[list.Count];
            retValue = list.ToArray();
            return retValue;
        }	//	getOfWorkOrder

        /// <summary>
        ///  Get Components of a Work Order Operation
        /// </summary>
        /// <param name="workorderoperation"></param>
        /// <param name="whereClause"></param>
        /// <param name="orderClause"></param>
        /// <returns></returns>
        public static MVAMFGMWorkOrderComponent[] GetOfWorkOrderOperation(MVAMFGMWorkOrderOperation workorderoperation, String whereClause, String orderClause)
        {
            StringBuilder sqlstmt = new StringBuilder("SELECT * FROM VAMFG_M_WorkOrderComponent WHERE VAMFG_M_WorkOrderOperation_ID=@param1 ");
            if (whereClause != null)
                sqlstmt.Append("AND ").Append(whereClause);
            if (orderClause != null)
                sqlstmt.Append(" ORDER BY ").Append(orderClause);
            String sql = sqlstmt.ToString();
            //ArrayList<MWorkOrderComponent> list = new ArrayList<MWorkOrderComponent>();
            List<MVAMFGMWorkOrderComponent> list = new List<MVAMFGMWorkOrderComponent>();
            SqlParameter[] param = null;
            IDataReader idr = null;
            DataTable dt = new DataTable();
            //PreparedStatement pstmt = DB.prepareStatement (sql, workorderoperation.get_Trx());
            //ResultSet rs = null;
            try
            {
                param = new SqlParameter[1];
                param[0] = new SqlParameter("@param1", workorderoperation.GetVAMFG_M_WorkOrderOperation_ID());
                idr = DB.ExecuteReader(sql, param, workorderoperation.Get_TrxName());
                dt.Load(idr);
                if (idr != null)
                {
                    idr.Close();
                    idr = null;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list.Add(new MVAMFGMWorkOrderComponent(workorderoperation.GetCtx(), dt.Rows[i], workorderoperation.Get_TrxName()));
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
                return null;
            }

            MVAMFGMWorkOrderComponent[] retValue = new MVAMFGMWorkOrderComponent[list.Count];
            retValue = list.ToArray();
            return retValue;
        }	//	getOfWorkOrderOperation

        /// <summary>
        /// Set Defaults from WorkOrder parent
        /// </summary>
        /// <param name="workorder"></param>
        private void SetWorkOrder(MVAMFGMWorkOrder workorder)
        {
            SetClientOrg(workorder);
            SetC_BPartner_ID(workorder.GetC_BPartner_ID());
            SetC_BPartner_Location_ID(workorder.GetC_BPartner_Location_ID());
            if (!GetVAMFG_SupplyType().Equals(VAMFG_SUPPLYTYPE_Push) && GetM_Locator_ID() == 0)
                SetM_Locator_ID(VAdvantage.Model.MWarehouse.Get(GetCtx(), workorder.GetM_Warehouse_ID()).GetDefaultM_Locator_ID());
            //	Don't set Activity, etc as they are overwrites
        }	//	setWorkOrder


        /// <summary>
        /// Called before Save for Pre-Save Operation
        /// Set Line number if missing
        ///Check for duplicate component under one operation. 
        /// </summary>
        /// <param name="newRecord"></param>
        /// <returns></returns>
        protected override Boolean BeforeSave(bool newRecord)
        {
            //	Get Line No
            if (GetVAMFG_Line() == 0)
            {
                String sql = "SELECT COALESCE(MAX(VAMFG_Line),0)+10 FROM VAMFG_M_WorkOrderComponent WHERE VAMFG_M_WorkOrderOperation_ID=" + GetVAMFG_M_WorkOrderOperation_ID();
                int ii = DB.GetSQLValue(Get_TrxName(), sql);
                SetVAMFG_Line(ii);
            }

            if (newRecord)
            {
                String sql = "SELECT * from VAMFG_M_WorkOrderComponent WHERE M_Product_ID=@param1 and VAMFG_M_WorkOrderOperation_ID = @param2";
                SqlParameter[] param = null;
                IDataReader idr = null;
                DataTable dt = new DataTable();
                //PreparedStatement pstmt = DB.prepareStatement(sql, null);
                //ResultSet rs = null;
                bool success = true;
                try
                {
                    param = new SqlParameter[2];
                    param[0] = new SqlParameter("@param1", GetM_Product_ID());
                    param[1] = new SqlParameter("@param2", GetVAMFG_M_WorkOrderOperation_ID());
                    idr = DB.ExecuteReader(sql, param, null);
                    if (idr.Read())
                    {
                        // Show error
                        log.SaveError("Error", Msg.GetMsg(GetCtx(), "DuplicateComponent"));
                        success = false;

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

                if (!success)
                    return false;
            }

            if ((GetVAMFG_QtyRequired().CompareTo(Env.ZERO)) < 0)
            {
                log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@QtyRequired@ < 0"));
                return false;
            }

            if (GetVAMFG_Line() < 0)
            {
                log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@Line@ < 0"));
                return false;
            }

            // get current cost from product cost on new record and when product changed
            // currency conversion also required if order has different currency with base currency
            if (newRecord || (Is_ValueChanged("M_Product_ID")))
            {
                if (GetM_Product_ID() > 0)
                {
                    decimal currentcostprice = MCost.GetproductCosts(GetAD_Client_ID(), GetAD_Org_ID(), GetM_Product_ID(), VAdvantage.Utility.Util.GetValueOfInt(GetM_AttributeSetInstance_ID()), Get_TrxName());
                    MVAMFGMWorkOrderOperation woo = new MVAMFGMWorkOrderOperation(GetCtx(), GetVAMFG_M_WorkOrderOperation_ID(), Get_TrxName());
                    MVAMFGMWorkOrder wor = new MVAMFGMWorkOrder(GetCtx(), woo.GetVAMFG_M_WorkOrder_ID(), Get_TrxName());
                    if (wor.GetVAMFG_Description() != null && wor.GetVAMFG_Description().Contains("(->"))
                    {
                        // not to set cuurent cost price on reversal because its already filed during creation of line
                    }
                    else
                    {
                        SetCurrentCostPrice(currentcostprice);
                    }
                }
            }

            if (Is_ValueChanged("VAMFG_QtyRequired"))
            {
                //BigDecimal qtyRequired = getQtyRequired().setScale(MUOM.getPrecision(getCtx(), getC_UOM_ID()), BigDecimal.ROUND_HALF_UP);
                Decimal qtyRequired = Decimal.Round((GetVAMFG_QtyRequired()), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()));
                if (qtyRequired.CompareTo(GetVAMFG_QtyRequired()) != 0)
                {
                    log.Fine("Corrected QtyRequired Scale UOM=" + GetC_UOM_ID()
                            + "; QtyRequired=" + GetVAMFG_QtyRequired() + "->" + qtyRequired);
                    SetVAMFG_QtyRequired(qtyRequired);
                }
            }

            return true;
        } // beforeSave

        /// <summary>
        /// Set Product - Callout
        /// </summary>
        /// <param name="oldM_Product_ID"></param>
        /// <param name="newM_Product_ID"></param>
        /// <param name="windowNo"></param>
        public void SetM_Product_ID(String oldM_Product_ID,
               String newM_Product_ID, int windowNo)
        {
            // If Product is set to null, reset BOM.
            if (newM_Product_ID == null || newM_Product_ID.Length == 0)
            {
                Set_ValueNoCheck("C_UOM_ID", null);
                return;
            }
            int M_Product_ID = VAdvantage.Utility.Util.GetValueOfInt(newM_Product_ID);
            if (M_Product_ID == 0)
            {
                Set_ValueNoCheck("C_UOM_ID", null);
                return;
            }

            // Set UOM from Product
            VAdvantage.Model.MProduct product = new VAdvantage.Model.MProduct(Env.GetCtx(), M_Product_ID, null);
            SetC_UOM_ID(product.GetC_UOM_ID());

        }	//	setM_Product_ID	


        /**
         * @param headerInfo the headerInfo to set
         */
        public void SetHeaderInfo(MVAMFGMWorkOrderOperation headerInfo)
        {
            this.headerInfo = headerInfo;
        }

        /**
         * @return the headerInfo
         */
        public MVAMFGMWorkOrderOperation GetHeaderInfo()
        {
            return headerInfo;
        }

        /// <summary>
        /// QtyEntered callout: Set the quantity according to UOM Precision
        /// </summary>
        /// <param name="oldQtyRequired"></param>
        /// <param name="newQtyRequired"></param>
        /// <param name="windowNo"></param>
        public void SetQtyRequired(String oldQtyRequired,
               String newQtyRequired, int windowNo)
        {

            if (newQtyRequired == null || newQtyRequired.Trim().Length == 0)
                return;

            if (GetC_UOM_ID() == 0)
                return;

            Decimal QtyRequired = Convert.ToDecimal(newQtyRequired);
            //BigDecimal QtyRequired1 = QtyRequired.setScale(
            //        MUOM.getPrecision(getCtx(), getC_UOM_ID()), BigDecimal.ROUND_HALF_UP);
            Decimal QtyRequired1 = Decimal.Round((QtyRequired), VAdvantage.Model.MUOM.GetPrecision(GetCtx(), GetC_UOM_ID()));
            if (QtyRequired.CompareTo(QtyRequired1) != 0)
            {
                log.Fine("Corrected QtyRequired Scale UOM=" + GetC_UOM_ID()
                        + "; QtyRequired=" + QtyRequired + "->" + QtyRequired1);
                QtyRequired = QtyRequired1;
                SetVAMFG_QtyRequired(QtyRequired);
            }
        }
    }

}