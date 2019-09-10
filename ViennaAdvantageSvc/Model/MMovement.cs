/********************************************************
 * Module Name    : 
 * Purpose        : Inventory Movement Model
 * Class Used     : X_M_Movement, DocAction(Interface)
 * Chronological Development
 * Veena         26-Oct-2009
 ******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Common;
using VAdvantage.Process;
using VAdvantage.DataBase;
using VAdvantage.Utility;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using VAdvantage.Logging;
using VAdvantage.Model;

namespace ViennaAdvantage.Model
{
    /// <summary>
    /// Inventory Movement Model
    /// </summary>
    public class MMovement : X_M_Movement, DocAction
    {
        /**	Lines						*/
        private MMovementLine[] _lines = null;
        /** Confirmations				*/
        private MMovementConfirm[] _confirms = null;
        /**	Process Message 			*/
        private String _processMsg = null;
        /**	Just Prepared Flag			*/
        private Boolean _justPrepared = false;

        private string query = "";
        private bool isGetFromStorage = false;
        MAsset ast = null;
        private bool isAsset = false;

        private decimal totalMovementQty = 0;
        private VAdvantage.Model.MProduct productMovement = null;
        private MAcctSchema acctSchema = null;
        private MProductCategory productCategory = null;
        private string costingLevel = "";
        private DataSet dsAcctSchema = null;
        private int toOrg = 0;
        private MCost cost = null;
        private Decimal costPerUnit = 0;

        VAdvantage.Model.MProduct product1 = null;
        decimal currentCostPrice = 0;
        string conversionNotFoundInOut = "";
        string conversionNotFoundMovement = "";
        string conversionNotFoundMovement1 = "";
        VAdvantage.Model.MMovementLine baseMovementLine = null;

        /// <summary>
        /// Standard Constructor
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="M_Movement_ID">id</param>
        /// <param name="trxName">transaction</param>
        public MMovement(Ctx ctx, int M_Movement_ID, Trx trxName)
            : base(ctx, M_Movement_ID, trxName)
        {
            if (M_Movement_ID == 0)
            {
                //	SetC_DocType_ID (0);
                SetDocAction(DOCACTION_Complete);	// CO
                SetDocStatus(DOCSTATUS_Drafted);	// DR
                SetIsApproved(false);
                SetIsInTransit(false);
                SetMovementDate(new DateTime(CommonFunctions.CurrentTimeMillis()));	// @#Date@
                SetPosted(false);
                base.SetProcessed(false);
            }
        }

        /// <summary>
        /// Load Constructor
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="dr">data row</param>
        /// <param name="trxName">transation</param>
        public MMovement(Ctx ctx, DataRow dr, Trx trxName)
            : base(ctx, dr, trxName)
        {
        }

        /// <summary>
        /// Get Lines
        /// </summary>
        /// <param name="requery">requery</param>
        /// <returns>array of lines</returns>
        public MMovementLine[] GetLines(Boolean requery)
        {
            if (_lines != null && !requery)
                return _lines;
            //
            List<MMovementLine> list = new List<MMovementLine>();
            String sql = "SELECT * FROM M_MovementLine WHERE M_Movement_ID=@moveid ORDER BY Line";
            try
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@moveid", GetM_Movement_ID());

                DataSet ds = DB.ExecuteDataset(sql, param, Get_TrxName());
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new MMovementLine(GetCtx(), dr, Get_TrxName()));
                }
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, "GetLines", e);
            }

            _lines = new MMovementLine[list.Count];
            _lines = list.ToArray();
            return _lines;
        }

        /// <summary>
        /// Get Confirmations
        /// </summary>
        /// <param name="requery">requery</param>
        /// <returns>array of confirmations</returns>
        public MMovementConfirm[] GetConfirmations(Boolean requery)
        {
            if (_confirms != null && !requery)
                return _confirms;

            List<MMovementConfirm> list = new List<MMovementConfirm>();
            String sql = "SELECT * FROM M_MovementConfirm WHERE M_Movement_ID=@moveid";
            try
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@moveid", GetM_Movement_ID());

                DataSet ds = DB.ExecuteDataset(sql, param, Get_TrxName());
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new MMovementConfirm(GetCtx(), dr, Get_TrxName()));
                }
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, "GetConfirmations", e);
            }

            _confirms = new MMovementConfirm[list.Count];
            _confirms = list.ToArray();
            return _confirms;
        }

        /// <summary>
        /// Add to Description
        /// </summary>
        /// <param name="description">text</param>
        public void AddDescription(String description)
        {
            String desc = GetDescription();
            if (desc == null)
                SetDescription(description);
            else
                SetDescription(desc + " | " + description);
        }

        /// <summary>
        /// Get Document Info
        /// </summary>
        /// <returns>document info (untranslated)</returns>
        public String GetDocumentInfo()
        {
            MDocType dt = MDocType.Get(GetCtx(), GetC_DocType_ID());
            return dt.GetName() + " " + GetDocumentNo();
        }

        /// <summary>
        /// Create PDF
        /// </summary>
        /// <returns>File or null</returns>
        public FileInfo CreatePDF()
        {
            try
            {
                string fileName = Get_TableName() + Get_ID() + "_" + CommonFunctions.GenerateRandomNo()
                                    + ".txt"; //.pdf
                string filePath = Path.GetTempPath() + fileName;

                FileInfo temp = new FileInfo(filePath);
                if (!temp.Exists)
                {
                    return CreatePDF(temp);
                }
            }
            catch (Exception e)
            {
                log.Severe("Could not create PDF - " + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Create PDF file
        /// </summary>
        /// <param name="file">output file</param>
        /// <returns>file if success</returns>
        public FileInfo CreatePDF(FileInfo file)
        {
            //	ReportEngine re = ReportEngine.Get (GetCtx(), ReportEngine.INVOICE, GetC_Invoice_ID());
            //	if (re == null)
            return null;
            //	return re.GetPDF(file);
        }

        /// <summary>
        /// Before Save
        /// </summary>
        /// <param name="newRecord">new</param>
        /// <returns>true if success</returns>
        protected override Boolean BeforeSave(Boolean newRecord)
        {
            if (!CheckMovementExist(true))
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithFutureDate"));
                return false;
            }

            //if (!CheckMovementExist(false))
            //{
            //    log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithBackDate"));
            //    return false;
            //}

            if (GetC_DocType_ID() == 0)
            {
                MDocType[] types = MDocType.GetOfDocBaseType(GetCtx(), MDocBaseType.DOCBASETYPE_MATERIALMOVEMENT);
                if (types.Length > 0)	//	Get first
                    SetC_DocType_ID(types[0].GetC_DocType_ID());
                else
                {
                    log.SaveError("Error", Msg.ParseTranslation(GetCtx(), "@NotFound@ @C_DocType_ID@"));
                    return false;
                }
            }
            return true;
        }

        private bool CheckMovementExist(bool futureDate)
        {
            MDocType Doctype = new MDocType(GetCtx(), GetC_DocType_ID(), Get_TrxName());
            string Decanting = Util.GetValueOfBool(Doctype.Get_Value("IsDecanting")) ? "Y" : "N";
            string IsnternalUse = Util.GetValueOfBool(Doctype.Get_Value("IsInternalUse")) ? "Y" : "N";

            string Sql = @"Select Count(M_Movement_ID) From M_Movement Mov Inner Join C_Doctype doc on doc.C_Doctype_ID=Mov.C_Doctype_ID Where doc.DocBaseType='MMM' AND Mov.AD_Client_ID=" + GetAD_Client_ID() + " AND Mov.AD_Org_ID=" + GetAD_Org_ID() + " ";
            if (futureDate)
            {
                Sql += " AND Mov.DocStatus IN ('CO','CL','IP','DR') AND Mov.MovementDate > " + GlobalVariable.TO_DATE(GetMovementDate(), true);
            }
            else
            {
                Sql += " AND Mov.DocStatus IN ('IP') AND Mov.MovementDate < " + GlobalVariable.TO_DATE(GetMovementDate(), true);
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
            MDocType Doctype = new MDocType(GetCtx(), GetC_DocType_ID(), Get_TrxName());
            string Decanting = Util.GetValueOfBool(Doctype.Get_Value("IsDecanting")) ? "Y" : "N";
            string IsnternalUse = Util.GetValueOfBool(Doctype.Get_Value("IsInternalUse")) ? "Y" : "N";

            string Sql = @"Select Count(M_Movement_ID) From M_Movement Mov Inner Join C_Doctype doc on doc.C_Doctype_ID=Mov.C_Doctype_ID Where doc.DocBaseType='MMM' AND Mov.AD_Client_ID=" + GetAD_Client_ID() + " AND Mov.AD_Org_ID=" + GetAD_Org_ID() + "AND Mov.DocStatus IN ('IP') AND Mov.MovementDate < sysdate AND Mov.M_Movement_ID=" +GetM_Movement_ID();
            int cnt = Util.GetValueOfInt(DB.ExecuteScalar(Sql, null, Get_TrxName()));
            if (cnt > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set Processed.
        ///	Propergate to Lines/Taxes
        /// </summary>
        /// <param name="processed">processed</param>
        public void SetProcessed(Boolean processed)
        {
            base.SetProcessed(processed);
            if (Get_ID() == 0)
                return;
            String sql = "UPDATE M_MovementLine SET Processed='"
                + (processed ? "Y" : "N")
                + "' WHERE M_Movement_ID=" + GetM_Movement_ID();
            int noLine = DB.ExecuteQuery(sql, null, Get_TrxName());
            _lines = null;
            log.Fine("Processed=" + processed + " - Lines=" + noLine);
        }

        /// <summary>
        /// Process document
        /// </summary>
        /// <param name="processAction">document action</param>
        /// <returns>true if performed</returns>
        public Boolean ProcessIt(String processAction)
        {
            _processMsg = null;
            DocumentEngine engine = new DocumentEngine(this, GetDocStatus());
            return engine.ProcessIt(processAction, GetDocAction());
        }

        /// <summary>
        /// Unlock Document.
        /// </summary>
        /// <returns>true if success</returns>
        public Boolean UnlockIt()
        {
            log.Info(ToString());
            SetProcessing(false);
            return true;
        }

        /// <summary>
        /// Invalidate Document
        /// </summary>
        /// <returns>true if success</returns>
        public Boolean InvalidateIt()
        {
            log.Info(ToString());
            SetDocAction(DOCACTION_Prepare);
            return true;
        }

        /// <summary>
        /// Prepare Document
        /// </summary>
        /// <returns>new status (In Progress or Invalid)</returns>
        public String PrepareIt()
        {
            log.Info(ToString());
            _processMsg = ModelValidationEngine.Get().FireDocValidate(this, ModalValidatorVariables.DOCTIMING_BEFORE_PREPARE);
            if (_processMsg != null)
                return DocActionVariables.STATUS_INVALID;
            MDocType dt = MDocType.Get(GetCtx(), GetC_DocType_ID());

            //	Std Period open?
            if (!MPeriod.IsOpen(GetCtx(), GetMovementDate(), dt.GetDocBaseType()))
            {
                _processMsg = "@PeriodClosed@";
                return DocActionVariables.STATUS_INVALID;
            }

            // is Non Business Day?
            if (MNonBusinessDay.IsNonBusinessDay(GetCtx(), GetMovementDate()))
            {
                _processMsg = VAdvantage.Common.Common.NONBUSINESSDAY;
                return DocActionVariables.STATUS_INVALID;
            }

            if (!CheckMovementExist(true))
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
            //if (!CheckMovementExist(false))
            //{
            //    _processMsg = Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithBackDate");                
            //    return DocActionVariables.STATUS_INVALID;
            //}

            MMovementLine[] lines = GetLines(false);
            if (lines.Length == 0)
            {
                _processMsg = "@NoLines@";
                return DocActionVariables.STATUS_INVALID;
            }
            //	Add up Amounts

            /* nnayak - Bug 1750251 : check material policy and update storage
               at the line level in completeIt()*/
            //checkMaterialPolicy();

            //	Confirmation
            if (GetDescription() != null)
            {
                if (GetDescription().Substring(0, 3) != "{->")
                {
                    if (dt.IsInTransit())
                        CreateConfirmation();
                }
            }
            else
            {
                if (dt.IsInTransit())
                    CreateConfirmation();
            }

            _justPrepared = true;
            if (!DOCACTION_Complete.Equals(GetDocAction()))
                SetDocAction(DOCACTION_Complete);
            return DocActionVariables.STATUS_INPROGRESS;
        }

        /// <summary>
        /// Create Movement Confirmation
        /// </summary>
        private void CreateConfirmation()
        {
            MMovementConfirm[] confirmations = GetConfirmations(false);
            if (confirmations.Length > 0)
                return;

            //	Create Confirmation
            MMovementConfirm.Create(this, false);
        }

        /// <summary>
        /// Approve Document
        /// </summary>
        /// <returns>true if success</returns>
        public Boolean ApproveIt()
        {
            log.Info(ToString());
            SetIsApproved(true);
            return true;
        }

        /// <summary>
        /// Reject Approval
        /// </summary>
        /// <returns>true if success</returns>
        public Boolean RejectIt()
        {
            log.Info(ToString());
            SetIsApproved(false);
            return true;
        }

        /// <summary>
        /// Complete Document
        /// </summary>
        /// <returns>new status (Complete, In Progress, Invalid, Waiting ..)</returns>
        public String CompleteIt()
        {
            //By Sukhwinder on 22 Dec, 2017
            #region[Prevent from completing, If on hand quantity of Product not available as per qty entered at line and Disallow negative is true at Warehouse. By Sukhwinder on 22 Dec, 2017. Only if DTD001 Module Installed.]
            if (Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM AD_ModuleInfo WHERE Prefix = 'DTD001_'")) > 0)
            {
                string sql = "";
                sql = "SELECT ISDISALLOWNEGATIVEINV FROM M_Warehouse WHERE M_Warehouse_ID = " + Util.GetValueOfInt(GetDTD001_MWarehouseSource_ID());
                string disallow = Util.GetValueOfString(DB.ExecuteScalar(sql, null, Get_TrxName()));

                if (disallow.ToUpper() == "Y")
                {
                    int[] movementLine = MInOutLine.GetAllIDs("M_MovementLine", "M_Movement_ID = " + GetM_Movement_ID(), Get_TrxName());
                    int m_locator_id = 0;
                    int m_product_id = 0;
                    StringBuilder products = new StringBuilder();
                    StringBuilder locators = new StringBuilder();
                    bool check = false;
                    for (int i = 0; i < movementLine.Length; i++)
                    {
                        MMovementLine mmLine = new MMovementLine(Env.GetCtx(), movementLine[i], Get_TrxName());
                        //MInOutLine iol = new MInOutLine(Env.GetCtx(), movementLine[i], Get_TrxName());
                        m_locator_id = Util.GetValueOfInt(mmLine.GetM_Locator_ID());
                        m_product_id = Util.GetValueOfInt(mmLine.GetM_Product_ID());


                        sql = "SELECT M_AttributeSet_ID FROM M_Product WHERE M_Product_ID = " + m_product_id;
                        int m_attribute_ID = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, Get_TrxName()));
                        if (m_attribute_ID == 0)
                        {
                            sql = "SELECT SUM(QtyOnHand) FROM M_Storage WHERE M_Locator_ID = " + m_locator_id + " AND M_Product_ID = " + m_product_id;
                            int qty = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, Get_TrxName()));
                            int qtyToMove = Util.GetValueOfInt(mmLine.GetMovementQty());
                            if (qty < qtyToMove)
                            {
                                check = true;
                                products.Append(m_product_id + ", ");
                                locators.Append(m_locator_id + ", ");
                                continue;
                            }
                        }
                        else
                        {
                            sql = "SELECT SUM(QtyOnHand) FROM M_Storage WHERE M_Locator_ID = " + m_locator_id + " AND M_Product_ID = " + m_product_id + " AND M_AttributeSetInstance_ID = " + mmLine.GetM_AttributeSetInstance_ID();
                            int qty = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, Get_TrxName()));
                            int qtyToMove = Util.GetValueOfInt(mmLine.GetMovementQty());
                            if (qty < qtyToMove)
                            {
                                check = true;
                                products.Append(m_product_id + ",");
                                locators.Append(m_locator_id + ",");
                                continue;
                            }
                        }
                    }
                    if (check)
                    {
                        sql = "SELECT SUBSTR (SYS_CONNECT_BY_PATH (value , ', '), 2) CSV FROM (SELECT value , ROW_NUMBER () OVER (ORDER BY value ) rn, COUNT (*) over () CNT FROM "
                             + " (SELECT DISTINCT value FROM m_locator WHERE M_Locator_ID IN(" + locators.ToString().Trim().Trim(',') + "))) WHERE rn = cnt START WITH RN = 1 CONNECT BY rn = PRIOR rn + 1";
                        string loc = Util.GetValueOfString(DB.ExecuteScalar(sql, null, Get_TrxName()));

                        sql = "SELECT SUBSTR (SYS_CONNECT_BY_PATH (Name , ', '), 2) CSV FROM (SELECT Name , ROW_NUMBER () OVER (ORDER BY Name ) rn, COUNT (*) over () CNT FROM "
                            + " M_Product WHERE M_Product_ID IN (" + products.ToString().Trim().Trim(',') + ") ) WHERE rn = cnt START WITH RN = 1 CONNECT BY rn = PRIOR rn + 1";
                        string prod = Util.GetValueOfString(DB.ExecuteScalar(sql, null, Get_TrxName()));

                        _processMsg = Msg.GetMsg(Env.GetCtx(), "InsufficientQuantityFor: ") + prod + Msg.GetMsg(Env.GetCtx(), "OnLocators: ") + loc;
                        return DocActionVariables.STATUS_DRAFTED;
                    }
                }
            }
            #endregion
            //

            //	Re-Check
            if (!_justPrepared)
            {
                String status = PrepareIt();
                if (!DocActionVariables.STATUS_INPROGRESS.Equals(status))
                    return status;
            }
            // check column name new 12 jan 0 vikas
            int _count = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT Count(*) FROM AD_Column WHERE columnname = 'DTD001_SourceReserve' "));

            //	Outstanding (not processed) Incoming Confirmations ?
            MMovementConfirm[] confirmations = GetConfirmations(true);
            for (int i = 0; i < confirmations.Length; i++)
            {
                MMovementConfirm confirm = confirmations[i];
                if (!confirm.IsProcessed())
                {
                    _processMsg = "Open: @M_MovementConfir_ID@ - "
                        + confirm.GetDocumentNo();
                    return DocActionVariables.STATUS_INPROGRESS;
                }
            }

            //	Implicit Approval
            if (!IsApproved())
                ApproveIt();
            log.Info(ToString());

            // for checking - costing calculate on completion or not
            // IsCostImmediate = true - calculate cost on completion
            MClient client = MClient.Get(GetCtx(), GetAD_Client_ID());

            //Amit - 12-9-2015
            // Calculate qty for charge implementation against this movement
            query = "SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='GOM01_'";
            int countGulfoil = Util.GetValueOfInt(DB.ExecuteScalar(query, null, null));
            if (countGulfoil > 0 && GetC_Charge_ID() > 0)
            {
                query = "SELECT NVL(SUM(movementqty), 0) FROM m_movementline WHERE isactive = 'Y' AND gom01_iscostconsider = 'Y' AND M_Movement_ID =  " + GetM_Movement_ID();
                totalMovementQty = Util.GetValueOfDecimal(DB.ExecuteScalar(query, null, Get_Trx()));

                query = "SELECT AD_Org_ID FROM M_Warehouse WHERE IsActive = 'Y' AND M_WareHouse_ID = " + GetM_Warehouse_ID();
                toOrg = Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx()));
            }
            //end

            query = "SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA203_'";
            int countKarminati = Util.GetValueOfInt(DB.ExecuteScalar(query, null, null));
            //
            MMovementLine[] lines = GetLines(false);
            for (int i = 0; i < lines.Length; i++)
            {
                MMovementLine line = lines[i];

                /* nnayak - Bug 1750251 : If you have multiple lines for the same product
                in the same Sales Order, or if the generate shipment process was generating
                multiple shipments for the same product in the same run, the first layer 
                was Getting consumed by all the shipments. As a result, the first layer had
                negative Inventory even though there were other positive layers. */
                CheckMaterialPolicy(line);

                MTransaction trxFrom = null;
                if (line.GetM_AttributeSetInstance_ID() == 0)
                {
                    MMovementLineMA[] mas = MMovementLineMA.Get(GetCtx(),
                        line.GetM_MovementLine_ID(), Get_TrxName());
                    for (int j = 0; j < mas.Length; j++)
                    {
                        MMovementLineMA ma = mas[j];
                        //
                        MStorage storageFrom = MStorage.Get(GetCtx(), line.GetM_Locator_ID(),
                            line.GetM_Product_ID(), ma.GetM_AttributeSetInstance_ID(), Get_TrxName());
                        if (storageFrom == null)
                            storageFrom = MStorage.GetCreate(GetCtx(), line.GetM_Locator_ID(),
                                line.GetM_Product_ID(), ma.GetM_AttributeSetInstance_ID(), Get_TrxName());
                        //
                        MStorage storageTo = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(),
                            line.GetM_Product_ID(), ma.GetM_AttributeSetInstance_ID(), Get_TrxName());
                        if (storageTo == null)
                            storageTo = MStorage.GetCreate(GetCtx(), line.GetM_LocatorTo_ID(),
                                line.GetM_Product_ID(), ma.GetM_AttributeSetInstance_ID(), Get_TrxName());
                        //
                        storageFrom.SetQtyOnHand(Decimal.Subtract(storageFrom.GetQtyOnHand(), ma.GetMovementQty()));
                        if (line.GetMovementQty() > 0 && line.GetM_RequisitionLine_ID() > 0)
                        {
                            storageFrom.SetQtyReserved(Decimal.Subtract(storageFrom.GetQtyReserved(), line.GetMovementQty()));
                            // new 12 jan 1 vikas
                            if (_count > 0)
                            {
                                storageFrom.SetDTD001_SourceReserve(Decimal.Subtract(storageFrom.GetDTD001_SourceReserve(), line.GetMovementQty()));
                            }
                        }
                        if (!storageFrom.Save(Get_TrxName()))
                        {
                            _processMsg = "Storage From not updated (MA)";
                            return DocActionVariables.STATUS_INVALID;
                        }
                        //
                        storageTo.SetQtyOnHand(Decimal.Add(storageTo.GetQtyOnHand(), ma.GetMovementQty()));
                        if (!storageTo.Save(Get_TrxName()))
                        {
                            _processMsg = "Storage To not updated (MA)";
                            return DocActionVariables.STATUS_INVALID;
                        }

                        // Done to Update Current Qty at Transaction
                        Decimal? trxQty = 0;
                        MProduct pro = new MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
                        int attribSet_ID = pro.GetM_AttributeSet_ID();
                        isGetFromStorage = false;
                        if (attribSet_ID > 0)
                        {
                            query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID()
                                    + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + " AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                            if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                            {
                                trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), true, line.GetM_Locator_ID());
                                isGetFromStorage = true;
                            }
                        }
                        else
                        {
                            query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID()
                                     + " AND M_AttributeSetInstance_ID = 0  AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                            if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                            {
                                trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), false, line.GetM_Locator_ID());
                                isGetFromStorage = true;
                            }
                        }
                        if (!isGetFromStorage)
                        {
                            trxQty = GetProductQtyFromStorage(line, line.GetM_Locator_ID());
                        }
                        // Done to Update Current Qty at Transaction

                        //
                        MLocator locatorFrm = new MLocator(GetCtx(), line.GetM_Locator_ID(), Get_TrxName());
                        trxFrom = new MTransaction(GetCtx(), locatorFrm.GetAD_Org_ID(),
                            MTransaction.MOVEMENTTYPE_MovementFrom,
                            line.GetM_Locator_ID(), line.GetM_Product_ID(), ma.GetM_AttributeSetInstance_ID(),
                            Decimal.Negate(ma.GetMovementQty()), GetMovementDate(), Get_TrxName());
                        trxFrom.SetM_MovementLine_ID(line.GetM_MovementLine_ID());
                        trxFrom.SetCurrentQty(trxQty + Decimal.Negate(ma.GetMovementQty()));
                        if (!trxFrom.Save())
                        {
                            _processMsg = "Transaction From not inserted (MA)";
                            return DocActionVariables.STATUS_INVALID;
                        }

                        //Update Transaction for Current Quantity
                        UpdateTransaction(line, trxFrom, trxQty.Value + Decimal.Negate(ma.GetMovementQty()), line.GetM_Locator_ID());
                        //UpdateCurrentRecord(line, trxFrom, Decimal.Negate(ma.GetMovementQty()), line.GetM_Locator_ID());
                        /*************************************************************************************************/
                        Tuple<String, String, String> mInfo = null;
                        if (Env.HasModulePrefix("DTD001_", out mInfo))
                        {
                            if (line.GetM_RequisitionLine_ID() > 0)
                            {
                                decimal reverseRequisitionQty = 0;
                                MRequisitionLine reqLine = new MRequisitionLine(GetCtx(), line.GetM_RequisitionLine_ID(), Get_Trx());
                                MRequisition req = new MRequisition(GetCtx(), reqLine.GetM_Requisition_ID(), null);
                                if (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()) >= line.GetMovementQty())
                                {
                                    reverseRequisitionQty = line.GetMovementQty();
                                }
                                else if (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()) < line.GetMovementQty())
                                {
                                    reverseRequisitionQty = (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()));
                                }
                                reqLine.SetDTD001_DeliveredQty(Decimal.Add(reqLine.GetDTD001_DeliveredQty(), line.GetMovementQty()));
                                //mohit
                                if (line.GetMovementQty() > 0)
                                {
                                    reqLine.SetDTD001_ReservedQty(Decimal.Subtract(reqLine.GetDTD001_ReservedQty(), line.GetMovementQty()));
                                }
                                reqLine.Save();

                                //Amit 9-feb-2015
                                if (Util.GetValueOfString(req.GetDocStatus()) != "CL" && countKarminati > 0)
                                {
                                    MStorage newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), reqLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                    if (newsg == null)
                                    {
                                        newsg = MStorage.GetCreate(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), reqLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                    }
                                    newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), reverseRequisitionQty));
                                    if (!newsg.Save())
                                    {
                                        _processMsg = "Storage Not Updated";
                                        return DocActionVariables.STATUS_INVALID;
                                    }
                                }
                                else if (Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                {
                                    MStorage newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), 0, Get_Trx());
                                    //if (line.GetMovementQty() > 0)
                                    //{
                                    newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), reverseRequisitionQty));
                                    //if (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()) >= line.GetMovementQty())
                                    //{
                                    //    newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), line.GetMovementQty()));
                                    //}
                                    //else if (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()) < line.GetMovementQty())
                                    //{
                                    //    newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()))));
                                    //}
                                    //}
                                    if (!newsg.Save())
                                    {
                                        _processMsg = "Storage Not Updated";
                                        return DocActionVariables.STATUS_INVALID;
                                    }
                                }
                            }
                            string sql = "SELECT DTD001_ISCONSUMABLE FROM M_Product WHERE M_Product_ID=" + line.GetM_Product_ID();
                            if (Util.GetValueOfString(DB.ExecuteScalar(sql)) == "N")
                            {

                                sql = "SELECT pcat.A_Asset_Group_ID FROM M_Product prd INNER JOIN M_Product_Category pcat ON prd.M_Product_Category_ID=pcat.M_Product_Category_ID WHERE prd.M_Product_ID=" + line.GetM_Product_ID();
                                if (Util.GetValueOfInt(DB.ExecuteScalar(sql)) > 0)
                                {
                                    isAsset = true;
                                }
                                else
                                {
                                    isAsset = false;
                                }
                            }

                            else
                            {
                                isAsset = false;
                            }

                            if (isAsset == true)
                            {
                                DataSet DSReq = null;
                                if (line.GetM_RequisitionLine_ID() > 0)
                                {
                                    string NEWStr = "SELECT req.c_bpartner_id FROM m_requisitionline rqln INNER JOIN m_requisition req  ON req.m_requisition_id  = rqln.m_requisition_id  WHERE rqln.m_requisitionline_id=" + line.GetM_RequisitionLine_ID();
                                    DSReq = DB.ExecuteDataset(NEWStr, null, null);
                                }
                                if (line.GetA_Asset_ID() > 0)
                                {
                                    ast = new MAsset(GetCtx(), line.GetA_Asset_ID(), Get_Trx());
                                    Tuple<String, String, String> aInfo = null;
                                    if (Env.HasModulePrefix("VAFAM_", out aInfo))
                                    {
                                        MVAFAMAssetHistory aHist = new MVAFAMAssetHistory(GetCtx(), 0, Get_Trx());
                                        ast.CopyTo(aHist);
                                        aHist.SetA_Asset_ID(line.GetA_Asset_ID());
                                        if (!aHist.Save() && !ast.Save())
                                        {
                                            _processMsg = "Asset History Not Updated";
                                            return DocActionVariables.STATUS_INVALID;
                                        }
                                    }
                                    ast.SetC_BPartner_ID(line.GetC_BPartner_ID());
                                    if (DSReq != null)
                                    {
                                        if (DSReq.Tables[0].Rows.Count > 0)
                                        {
                                            ast.SetC_BPartner_ID(Util.GetValueOfInt(DSReq.Tables[0].Rows[0]["c_bpartner_id"]));
                                        }
                                    }

                                    ast.SetM_Locator_ID(line.GetM_LocatorTo_ID());
                                    ast.Save();
                                }
                                else
                                {
                                    _processMsg = "Asset Not Selected For Movement Line";
                                    return DocActionVariables.STATUS_INVALID;
                                }
                            }
                        }

                        // Done to Update Current Qty at Transaction Decimal? trxQty = 0;
                        pro = new MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
                        attribSet_ID = pro.GetM_AttributeSet_ID();
                        isGetFromStorage = false;
                        if (attribSet_ID > 0)
                        {
                            query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_LocatorTo_ID()
                                    + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + " AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                            if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                            {
                                trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), true, line.GetM_LocatorTo_ID());
                                isGetFromStorage = true;
                            }
                        }
                        else
                        {
                            query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_LocatorTo_ID()
                                     + " AND M_AttributeSetInstance_ID = 0  AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                            if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                            {
                                trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), false, line.GetM_LocatorTo_ID());
                                isGetFromStorage = true;
                            }
                        }
                        if (!isGetFromStorage)
                        {
                            trxQty = GetProductQtyFromStorage(line, line.GetM_LocatorTo_ID());
                        }
                        // Done to Update Current Qty at Transaction
                        //
                        MLocator locatorTo = new MLocator(GetCtx(), line.GetM_LocatorTo_ID(), Get_TrxName());
                        MTransaction trxTo = new MTransaction(GetCtx(), locatorTo.GetAD_Org_ID(),
                            MTransaction.MOVEMENTTYPE_MovementTo,
                            line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), ma.GetM_AttributeSetInstance_ID(),
                            ma.GetMovementQty(), GetMovementDate(), Get_TrxName());
                        trxTo.SetM_MovementLine_ID(line.GetM_MovementLine_ID());
                        trxTo.SetCurrentQty(trxQty.Value + ma.GetMovementQty());
                        if (!trxTo.Save())
                        {
                            _processMsg = "Transaction To not inserted (MA)";
                            return DocActionVariables.STATUS_INVALID;
                        }

                        //Update Transaction for Current Quantity
                        UpdateTransaction(line, trxTo, trxQty.Value + ma.GetMovementQty(), line.GetM_LocatorTo_ID());
                        //UpdateCurrentRecord(line, trxTo, ma.GetMovementQty(), line.GetM_LocatorTo_ID());
                    }
                }
                //	Fallback - We have ASI
                if (trxFrom == null)
                {

                    MRequisitionLine reqLine = null;
                    MRequisition req = null;
                    decimal reverseRequisitionQty = 0;
                    if (line.GetM_RequisitionLine_ID() > 0)
                    {
                        reqLine = new MRequisitionLine(GetCtx(), line.GetM_RequisitionLine_ID(), Get_Trx());
                        req = new MRequisition(GetCtx(), reqLine.GetM_Requisition_ID(), null);
                        if (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()) >= line.GetMovementQty())
                        {
                            reverseRequisitionQty = line.GetMovementQty();
                        }
                        else if (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()) < line.GetMovementQty())
                        {
                            reverseRequisitionQty = (Decimal.Subtract(reqLine.GetQty(), reqLine.GetDTD001_DeliveredQty()));
                        }
                        Tuple<String, String, String> aInfo = null;
                        if (Env.HasModulePrefix("DTD001_", out aInfo))
                        {
                            reqLine.SetDTD001_DeliveredQty(Decimal.Add(reqLine.GetDTD001_DeliveredQty(), line.GetMovementQty()));
                            if (line.GetMovementQty() > 0 && line.GetM_RequisitionLine_ID() > 0)
                            {
                                reqLine.SetDTD001_ReservedQty(Decimal.Subtract(reqLine.GetDTD001_ReservedQty(), line.GetMovementQty()));
                            }
                            reqLine.Save();
                        }
                    }
                    MStorage storageFrom = MStorage.Get(GetCtx(), line.GetM_Locator_ID(),
                        line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_TrxName());
                    if (storageFrom == null)
                        storageFrom = MStorage.GetCreate(GetCtx(), line.GetM_Locator_ID(),
                            line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_TrxName());
                    if (line.GetMovementQty() > 0 && line.GetM_RequisitionLine_ID() > 0)
                    {
                        storageFrom.SetQtyReserved(Decimal.Subtract(storageFrom.GetQtyReserved(), line.GetMovementQty()));
                    }
                    //  
                    MStorage storageTo = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(),
                        line.GetM_Product_ID(), line.GetM_AttributeSetInstanceTo_ID(), Get_TrxName());
                    if (storageTo == null)
                        storageTo = MStorage.GetCreate(GetCtx(), line.GetM_LocatorTo_ID(),
                            line.GetM_Product_ID(), line.GetM_AttributeSetInstanceTo_ID(), Get_TrxName());
                    //
                    //Update product Qty at storage and Checks Product have Attribute Set Or Not.
                    MProduct newproduct = new MProduct(GetCtx(), line.GetM_Product_ID(), Get_Trx());
                    if (countKarminati > 0 && line.GetM_RequisitionLine_ID() > 0)
                    {
                        if ((newproduct.GetM_AttributeSet_ID() != null) && (newproduct.GetM_AttributeSet_ID() != 0))
                        {
                            MStorage newsg = null;
                            if (reqLine != null)
                            {
                                newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), reqLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                if (newsg == null)
                                {
                                    newsg = MStorage.GetCreate(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), reqLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                }
                            }
                            else
                            {
                                newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_Trx());
                                if (newsg == null)
                                {
                                    newsg = MStorage.GetCreate(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_Trx());
                                }
                            }
                            if (newsg != null && req != null)
                            {
                                Tuple<String, String, String> aInfo = null;
                                if (Env.HasModulePrefix("DTD001_", out aInfo))
                                {
                                    if (newsg.GetDTD001_QtyReserved() != null && Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                    {
                                        if (line.GetM_RequisitionLine_ID() > 0)
                                        {
                                            newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), reverseRequisitionQty));
                                        }
                                        if (!newsg.Save(Get_Trx()))
                                        {
                                            _processMsg = "Storage Not Updated";
                                            return DocActionVariables.STATUS_INVALID;
                                        }
                                    }
                                    else if (Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                    {
                                        if (line.GetM_RequisitionLine_ID() > 0)
                                        {
                                            newsg.SetDTD001_QtyReserved(Decimal.Subtract(0, reverseRequisitionQty));
                                        }
                                        if (!newsg.Save(Get_Trx()))
                                        {
                                            _processMsg = "Storage Not Updated";
                                            return DocActionVariables.STATUS_INVALID;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MStorage newsg = null;
                            if (reqLine != null)
                            {
                                newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), reqLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                if (newsg == null)
                                {
                                    newsg = MStorage.GetCreate(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), reqLine.GetM_AttributeSetInstance_ID(), Get_Trx());
                                }
                            }
                            else
                            {
                                newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_Trx());
                                if (newsg == null)
                                {
                                    newsg = MStorage.GetCreate(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_Trx());
                                }
                            }
                            if (newsg != null && req != null)
                            {
                                Tuple<String, String, String> aInfo = null;
                                if (Env.HasModulePrefix("DTD001_", out aInfo))
                                {
                                    if (newsg.GetDTD001_QtyReserved() != null && Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                    {
                                        if (line.GetM_RequisitionLine_ID() > 0)
                                        {
                                            newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), reverseRequisitionQty));
                                        }
                                        if (!newsg.Save(Get_Trx()))
                                        {
                                            _processMsg = "Storage Not Updated";
                                            return DocActionVariables.STATUS_INVALID;
                                        }
                                    }
                                    else if (Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                    {
                                        if (line.GetM_RequisitionLine_ID() > 0)
                                        {
                                            newsg.SetDTD001_QtyReserved(Decimal.Subtract(0, reverseRequisitionQty));
                                        }
                                        if (!newsg.Save(Get_Trx()))
                                        {
                                            _processMsg = "Storage Not Updated";
                                            return DocActionVariables.STATUS_INVALID;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if ((newproduct.GetM_AttributeSet_ID() != null) && (newproduct.GetM_AttributeSet_ID() != 0))
                    {
                        MStorage newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_Trx());
                        if (newsg != null && req != null)
                        {
                            Tuple<String, String, String> aInfo = null;
                            if (Env.HasModulePrefix("DTD001_", out aInfo))
                            {
                                if (newsg.GetDTD001_QtyReserved() != null && Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                {
                                    if (line.GetM_RequisitionLine_ID() > 0)
                                    {
                                        newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), reverseRequisitionQty));
                                    }
                                    if (!newsg.Save(Get_Trx()))
                                    {
                                        _processMsg = "Storage Not Updated";
                                        return DocActionVariables.STATUS_INVALID;
                                    }
                                }
                                else if (Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                {
                                    if (line.GetM_RequisitionLine_ID() > 0)
                                    {
                                        newsg.SetDTD001_QtyReserved(Decimal.Subtract(0, reverseRequisitionQty));
                                    }
                                    if (!newsg.Save(Get_Trx()))
                                    {
                                        _processMsg = "Storage Not Updated";
                                        return DocActionVariables.STATUS_INVALID;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MStorage newsg = MStorage.Get(GetCtx(), line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), true, Get_TrxName());
                        if (newsg != null && req != null)
                        {
                            Tuple<String, String, String> aInfo = null;
                            if (Env.HasModulePrefix("DTD001_", out aInfo))
                            {
                                if (newsg.GetDTD001_QtyReserved() != null && Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                {
                                    if (line.GetM_RequisitionLine_ID() > 0)
                                    {
                                        newsg.SetDTD001_QtyReserved(Decimal.Subtract(newsg.GetDTD001_QtyReserved(), reverseRequisitionQty));
                                    }
                                    if (!newsg.Save(Get_Trx()))
                                    {
                                        _processMsg = "Storage Not Updated";
                                        return DocActionVariables.STATUS_INVALID;
                                    }
                                }
                                else if (Util.GetValueOfString(req.GetDocStatus()) != "CL")
                                {
                                    if (line.GetM_RequisitionLine_ID() > 0)
                                    {
                                        newsg.SetDTD001_QtyReserved(Decimal.Subtract(0, reverseRequisitionQty));
                                    }
                                    if (!newsg.Save(Get_Trx()))
                                    {
                                        _processMsg = "Storage Not Updated";
                                        return DocActionVariables.STATUS_INVALID;
                                    }
                                }
                            }
                        }
                    }
                    storageFrom.SetQtyOnHand(Decimal.Subtract(storageFrom.GetQtyOnHand(), line.GetMovementQty()));
                    if (!storageFrom.Save(Get_TrxName()))
                    {
                        _processMsg = "Storage From not updated";
                        return DocActionVariables.STATUS_INVALID;
                    }
                    //
                    storageTo.SetQtyOnHand(Decimal.Add(storageTo.GetQtyOnHand(), line.GetMovementQty()));
                    if (!storageTo.Save(Get_TrxName()))
                    {
                        _processMsg = "Storage To not updated";
                        return DocActionVariables.STATUS_INVALID;
                    }
                    /***************************************************/
                    Tuple<String, String, String> iInfo = null;
                    if (Env.HasModulePrefix("DTD001_", out iInfo))
                    {
                        string sql = "SELECT DTD001_ISCONSUMABLE FROM M_Product WHERE M_Product_ID=" + line.GetM_Product_ID();
                        if (Util.GetValueOfString(DB.ExecuteScalar(sql)) != "Y")
                        {
                            sql = "SELECT pcat.A_Asset_Group_ID FROM M_Product prd INNER JOIN M_Product_Category pcat ON prd.M_Product_Category_ID=pcat.M_Product_Category_ID WHERE prd.M_Product_ID=" + line.GetM_Product_ID();
                            if (Util.GetValueOfInt(DB.ExecuteScalar(sql)) > 0)
                            {
                                isAsset = true;
                            }
                            else
                            {
                                isAsset = false;
                            }
                        }

                        else
                        {
                            isAsset = false;
                        }

                        if (isAsset == true)
                        {
                            if (line.GetA_Asset_ID() > 0)
                            {
                                ast = new MAsset(GetCtx(), line.GetA_Asset_ID(), Get_Trx());
                                Tuple<String, String, String> aInfo = null;
                                if (Env.HasModulePrefix("VAFAM_", out aInfo))
                                {
                                    MVAFAMAssetHistory aHist = new MVAFAMAssetHistory(GetCtx(), 0, Get_Trx());
                                    ast.CopyTo(aHist);
                                    aHist.SetA_Asset_ID(line.GetA_Asset_ID());
                                    if (!aHist.Save() && !ast.Save())
                                    {
                                        _processMsg = "Asset History Not Updated";
                                        return DocActionVariables.STATUS_INVALID;
                                    }
                                }
                                ast.SetC_BPartner_ID(line.GetC_BPartner_ID());
                                ast.SetM_Locator_ID(line.GetM_LocatorTo_ID());
                                ast.Save();
                            }
                            else
                            {
                                _processMsg = "Asset Not Selected For Movement Line";
                                return DocActionVariables.STATUS_INVALID;
                            }
                        }

                    }
                    /********************************************************/


                    // Done to Update Current Qty at Transaction
                    Decimal? trxQty = 0;
                    MProduct pro = new MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
                    int attribSet_ID = pro.GetM_AttributeSet_ID();
                    isGetFromStorage = false;
                    if (attribSet_ID > 0)
                    {
                        query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID()
                                + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + " AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                        if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                        {
                            trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), true, line.GetM_Locator_ID());
                            isGetFromStorage = true;
                        }
                    }
                    else
                    {
                        query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID()
                                      + " AND M_AttributeSetInstance_ID = 0  AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                        if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                        {
                            trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), false, line.GetM_Locator_ID());
                            isGetFromStorage = true;
                        }
                    }
                    if (!isGetFromStorage)
                    {
                        trxQty = GetProductQtyFromStorage(line, line.GetM_Locator_ID());
                    }
                    // Done to Update Current Qty at Transaction
                    //
                    trxFrom = new MTransaction(GetCtx(), line.GetAD_Org_ID(),
                        MTransaction.MOVEMENTTYPE_MovementFrom,
                        line.GetM_Locator_ID(), line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(),
                        Decimal.Negate(line.GetMovementQty()), GetMovementDate(), Get_TrxName());
                    trxFrom.SetM_MovementLine_ID(line.GetM_MovementLine_ID());
                    trxFrom.SetCurrentQty(trxQty + Decimal.Negate(line.GetMovementQty()));
                    if (!trxFrom.Save())
                    {
                        _processMsg = "Transaction From not inserted";
                        return DocActionVariables.STATUS_INVALID;
                    }

                    //Update Transaction for Current Quantity
                    UpdateTransaction(line, trxFrom, trxQty.Value + Decimal.Negate(line.GetMovementQty()), line.GetM_Locator_ID());
                    //UpdateCurrentRecord(line, trxFrom, Decimal.Negate(line.GetMovementQty()), line.GetM_Locator_ID());

                    // Done to Update Current Qty at Transaction
                    pro = new MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
                    attribSet_ID = pro.GetM_AttributeSet_ID();
                    isGetFromStorage = false;
                    if (attribSet_ID > 0)
                    {
                        query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_LocatorTo_ID()
                                + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + " AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                        if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                        {
                            trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), true, line.GetM_LocatorTo_ID());
                            isGetFromStorage = true;
                        }
                    }
                    else
                    {
                        query = @"SELECT COUNT(*)   FROM m_transaction
                                    WHERE IsActive = 'Y' AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_LocatorTo_ID()
                                      + " AND M_AttributeSetInstance_ID = 0  AND movementdate <= " + GlobalVariable.TO_DATE(GetMovementDate(), true);
                        if (Util.GetValueOfInt(DB.ExecuteScalar(query, null, Get_Trx())) > 0)
                        {
                            trxQty = GetProductQtyFromTransaction(line, GetMovementDate(), false, line.GetM_LocatorTo_ID());
                            isGetFromStorage = true;
                        }
                    }
                    if (!isGetFromStorage)
                    {
                        trxQty = GetProductQtyFromStorage(line, line.GetM_LocatorTo_ID());
                    }
                    // Done to Update Current Qty at Transaction
                    //
                    MTransaction trxTo = new MTransaction(GetCtx(), line.GetAD_Org_ID(),
                        MTransaction.MOVEMENTTYPE_MovementTo,
                        line.GetM_LocatorTo_ID(), line.GetM_Product_ID(), line.GetM_AttributeSetInstanceTo_ID(),
                        line.GetMovementQty(), GetMovementDate(), Get_TrxName());
                    trxTo.SetM_MovementLine_ID(line.GetM_MovementLine_ID());
                    trxTo.SetCurrentQty(trxQty + line.GetMovementQty());
                    if (!trxTo.Save())
                    {
                        _processMsg = "Transaction To not inserted";
                        return DocActionVariables.STATUS_INVALID;
                    }

                    //Update Transaction for Current Quantity
                    UpdateTransaction(line, trxTo, trxQty.Value + line.GetMovementQty(), line.GetM_LocatorTo_ID());
                    //UpdateCurrentRecord(line, trxTo, line.GetMovementQty(), line.GetM_LocatorTo_ID());
                }	//	Fallback



                // Enhanced by Amit for Cost Queue 10-12-2015
                if (client.IsCostImmediate())
                {
                    #region Costing Calculation

                    #region get price from m_cost (Current Cost Price)
                    if (GetDescription() != null && GetDescription().Contains("{->"))
                    {
                        // do not update current cost price during reversal, this time reverse doc contain same amount which are on original document
                    }
                    else
                    {
                        // get price from m_cost (Current Cost Price)
                        currentCostPrice = 0;
                        currentCostPrice = MCost.GetproductCosts(line.GetAD_Client_ID(), line.GetAD_Org_ID(),
                            line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_Trx());
                        line.SetCurrentCostPrice(currentCostPrice);
                        if (!line.Save(Get_Trx()))
                        {
                            ValueNamePair pp = VLogger.RetrieveError();
                            log.Info("Error found for Movement Line for this Line ID = " + line.GetM_MovementLine_ID() +
                                       " Error Name is " + pp.GetName() + " And Error Type is " + pp.GetType());
                            Get_Trx().Rollback();
                        }
                    }
                    #endregion

                    query = "SELECT AD_Org_ID FROM M_Warehouse WHERE IsActive = 'Y' AND M_Warehouse_ID = " + GetM_Warehouse_ID();
                    if (GetAD_Org_ID() != Util.GetValueOfInt(DB.ExecuteScalar(query, null, null)))
                    {
                        baseMovementLine = new VAdvantage.Model.MMovementLine(GetCtx(), line.GetM_MovementLine_ID(), Get_Trx());
                        product1 = new VAdvantage.Model.MProduct(GetCtx(), line.GetM_Product_ID(), null);
                        if (product1.GetProductType() == "I") // for Item Type product
                        {
                            if (!MCostQueue.CreateProductCostsDetails(GetCtx(), GetAD_Client_ID(), GetAD_Org_ID(), product1, line.GetM_AttributeSetInstance_ID(),
                              "Inventory Move", null, null, baseMovementLine, null, null, 0, line.GetMovementQty(), Get_TrxName(), out conversionNotFoundInOut, optionalstr: "window"))
                            {
                                if (!conversionNotFoundMovement1.Contains(conversionNotFoundMovement))
                                {
                                    conversionNotFoundMovement1 += conversionNotFoundMovement + " , ";
                                }
                                _processMsg = "Could not create Product Costs";
                                //return DocActionVariables.STATUS_INVALID;
                            }
                            else
                            {
                                line.SetIsCostImmediate(true);
                                line.Save();
                            }
                        }
                    }
                    #endregion
                }

                // Enhanced by Amit for Cost 12-09-2016
                if (countGulfoil > 0 && line.IsGOM01_IsCostConsider())
                {
                    if (!CalculateCostAgainstCharge(this, totalMovementQty, line, toOrg))
                    {
                        _processMsg = "Cost against charge not updated on product Costs";
                        Get_Trx().Rollback();
                        return DocActionVariables.STATUS_INVALID;
                    }
                }
                //End

            }	//	for all lines
            //	User Validation
            String valid = ModelValidationEngine.Get().FireDocValidate(this, ModalValidatorVariables.DOCTIMING_AFTER_COMPLETE);
            if (valid != null)
            {
                _processMsg = valid;
                return DocActionVariables.STATUS_INVALID;
            }

            //
            SetProcessed(true);
            SetDocAction(DOCACTION_Close);
            return DocActionVariables.STATUS_COMPLETED;
        }

        private bool CalculateCostAgainstCharge(MMovement movement, Decimal qty, MMovementLine movementLine, int toOrg)
        {
            try
            {
                productMovement = new VAdvantage.Model.MProduct(GetCtx(), movementLine.GetM_Product_ID(), null);
                productCategory = new MProductCategory(GetCtx(), productMovement.GetM_Product_Category_ID(), null);
                query = "SELECT C_AcctSchema_ID FROM C_AcctSchema WHERE IsActive = 'Y'";
                query = MRole.GetDefault(GetCtx()).AddAccessSQL(query, "C_AcctSchema", true, false);
                dsAcctSchema = DB.ExecuteDataset(query.ToString(), null, null);
                if (dsAcctSchema != null && dsAcctSchema.Tables.Count > 0 && dsAcctSchema.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsAcctSchema.Tables[0].Rows.Count; i++)
                    {
                        acctSchema = new MAcctSchema(GetCtx(), Util.GetValueOfInt(dsAcctSchema.Tables[0].Rows[i]["C_AcctSchema_ID"]), Get_Trx());
                        if (!string.IsNullOrEmpty(productCategory.GetCostingLevel()))
                        {
                            costingLevel = productCategory.GetCostingLevel();
                        }
                        else
                        {
                            costingLevel = acctSchema.GetCostingLevel();
                        }
                        // when costing level = Organization
                        if (costingLevel == "O")
                        {
                            //calculate Per unit Cost of Charge
                            if (qty == 0)
                            {
                                return true;
                            }
                            costPerUnit = Decimal.Round(Decimal.Divide(movement.GetGOM01_ChargeAmt(), qty), acctSchema.GetCostingPrecision());

                            // Currency Conversion if Required
                            if (movement.GetC_Currency_ID() != acctSchema.GetC_Currency_ID())
                            {
                                costPerUnit = MConversionRate.Convert(GetCtx(), costPerUnit, movement.GetC_Currency_ID(), acctSchema.GetC_Currency_ID(),
                                                                 movement.GetMovementDate(), movement.GetC_ConversionType_ID(), GetAD_Client_ID(), GetAD_Org_ID());
                            }

                            //getting cost element of Material type and whose costing method cannot be null
                            MCostElement[] ces = MCostElement.GetCostingMethods(this);
                            try
                            {
                                for (int j = 0; j < ces.Length; j++)
                                {
                                    MCostElement ce = ces[j];
                                    if (!UpdateCost(acctSchema, productMovement, ce, GetAD_Org_ID(), toOrg, movementLine, costPerUnit))
                                    {
                                        return false;
                                    }
                                }
                                // calculate Cost combination
                                if (!CalculateCostCombination(acctSchema, productMovement, movementLine.GetM_AttributeSetInstance_ID(), toOrg))
                                {
                                    return false;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Info("During Inventory Movement Completion, Exception Occure for this line " + movementLine.GetLine() + " Exception Type = " + ex);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        // Updating / Inserting -  Accumulation cost on Product Costs
        private bool UpdateCost(MAcctSchema mas, VAdvantage.Model.MProduct product, MCostElement ce, int fromOrgID, int toOrgID, MMovementLine movementLine, Decimal costPerUnit)
        {
            try
            {
                cost = MCost.Get(product, 0, mas, toOrgID, ce.GetM_CostElement_ID());
                //handle reverse case
                if (GetDescription() != null && GetDescription().Contains("{->"))
                {
                    cost.SetCumulatedAmt(Decimal.Add(cost.GetCumulatedAmt(), Decimal.Negate(Decimal.Multiply(costPerUnit, movementLine.GetMovementQty()))));
                }
                else
                {
                    cost.SetCumulatedAmt(Decimal.Add(cost.GetCumulatedAmt(), Decimal.Multiply(costPerUnit, movementLine.GetMovementQty())));
                }

                // if cost element ia Average PO or Average Invoice then need to calculate Current Cost price
                if (ce.IsAveragePO() || ce.IsAverageInvoice())
                {
                    if (Env.Signum(cost.GetCumulatedQty()) != 0)
                    {
                        cost.SetCurrentCostPrice(Decimal.Round(Decimal.Divide(cost.GetCumulatedAmt(), cost.GetCumulatedQty()), acctSchema.GetCostingPrecision()));
                    }
                    else
                    {
                        cost.SetCurrentCostPrice(0);
                    }
                }
                else if (ce.IsWeightedAverageCost() || ce.IsWeightedAveragePO())
                {
                    cost.SetCurrentCostPrice(Decimal.Round(Decimal.Add(cost.GetCurrentCostPrice(), costPerUnit), acctSchema.GetCostingPrecision()));
                }
                if (!cost.Save())
                {
                    log.Info("During Inventory Movement Completion, cost not updated for this cost Element ID = " + ce.GetName() + " And movement Line no = " + movementLine.GetLine());
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Info("During Inventory Movement Completion, Exception Occure for this line " + movementLine.GetLine() + " Exception Type = " + ex);
                return false;
            }
            return true;
        }

        // Calculate Cost for Cost Combination
        private bool CalculateCostCombination(MAcctSchema acctSchema, VAdvantage.Model.MProduct product, int M_ASI_ID, int AD_Org_ID)
        {
            try
            {
                string sql;
                // Get Org based on Costing Level
                dynamic pc = null;
                String cl = null;
                MCostElement ce = null;
                MCost costCombination = null;
                MCost cost = null;

                if (product != null)
                {
                    pc = MProductCategory.Get(product.GetCtx(), product.GetM_Product_Category_ID());
                    if (pc != null)
                    {
                        cl = pc.GetCostingLevel();
                    }
                }
                if (cl == null)
                {
                    cl = acctSchema.GetCostingLevel();
                }

                if (cl == "C" || cl == "B")
                {
                    AD_Org_ID = 0;
                }
                if (cl != "B")
                {
                    M_ASI_ID = 0;
                }

                sql = @"SELECT ce.M_CostElement_ID ,  ce.Name ,  cel.lineno ,  cel.m_ref_costelement
                            FROM M_CostElement ce INNER JOIN m_costelementline cel ON ce.M_CostElement_ID = cel.M_CostElement_ID "
                          + "WHERE ce.AD_Client_ID=" + GetAD_Client_ID()
                          + " AND ce.IsActive='Y' AND ce.CostElementType='C' AND cel.IsActive='Y' ORDER BY ce.M_CostElement_ID";
                DataSet ds = new DataSet();
                ds = DB.ExecuteDataset(sql, null, null);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        costCombination = MCost.Get(product, M_ASI_ID, acctSchema, AD_Org_ID, Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_CostElement_ID"]));
                        costCombination.SetCurrentCostPrice(0);
                        costCombination.SetCurrentQty(0);
                        costCombination.SetCumulatedAmt(0);
                        costCombination.SetCumulatedQty(0);
                        costCombination.Save();
                    }
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        // created object of Cost elemnt for checking iscalculated = true/ false
                        ce = MCostElement.Get(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_ref_costelement"]));

                        costCombination = MCost.Get(product, M_ASI_ID, acctSchema, AD_Org_ID, Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_CostElement_ID"]));
                        cost = MCost.Get(product, M_ASI_ID, acctSchema, AD_Org_ID, Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_ref_costelement"]));
                        costCombination.SetCurrentCostPrice(Decimal.Add(costCombination.GetCurrentCostPrice(), cost.GetCurrentCostPrice()));
                        costCombination.SetCumulatedAmt(Decimal.Add(costCombination.GetCumulatedAmt(), cost.GetCumulatedAmt()));
                        // if calculated = true then we added qty else not and costing method is Standard Costing
                        if (ce.IsCalculated() || ce.GetCostingMethod() == MCostElement.COSTINGMETHOD_StandardCosting)
                        {
                            costCombination.SetCurrentQty(Decimal.Add(costCombination.GetCurrentQty(), cost.GetCurrentQty()));
                            costCombination.SetCumulatedQty(Decimal.Add(costCombination.GetCumulatedQty(), cost.GetCumulatedQty()));
                        }
                        costCombination.Save();
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }



        private void updateCostQueue(VAdvantage.Model.MProduct product, int M_ASI_ID, MAcctSchema mas,
          int Org_ID, MCostElement ce, decimal movementQty)
        {
            //MCostQueue[] cQueue = MCostQueue.GetQueue(product1, sLine.GetM_AttributeSetInstance_ID(), acctSchema, GetAD_Org_ID(), costElement, null);
            MCostQueue[] cQueue = MCostQueue.GetQueue(product, M_ASI_ID, mas, Org_ID, ce, null);
            if (cQueue != null && cQueue.Length > 0)
            {
                Decimal qty = movementQty;
                bool value = false;
                for (int cq = 0; cq < cQueue.Length; cq++)
                {
                    MCostQueue queue = cQueue[cq];
                    if (queue.GetCurrentQty() < 0) continue;
                    if (queue.GetCurrentQty() > qty)
                    {
                        value = true;
                    }
                    else
                    {
                        value = false;
                    }
                    qty = MCostQueue.Quantity(queue.GetCurrentQty(), qty);
                    //if (cq == cQueue.Length - 1 && qty < 0) // last record
                    //{
                    //    queue.SetCurrentQty(qty);
                    //    if (!queue.Save())
                    //    {
                    //        ValueNamePair pp = VLogger.RetrieveError();
                    //        log.Info("Cost Queue not updated for  <===> " + product.GetM_Product_ID() + " Error Type is : " + pp.GetName());
                    //    }
                    //}
                    if (qty <= 0)
                    {
                        queue.Delete(true);
                        qty = Decimal.Negate(qty);
                    }
                    else
                    {
                        queue.SetCurrentQty(qty);
                        if (!queue.Save())
                        {
                            ValueNamePair pp = VLogger.RetrieveError();
                            log.Info("Cost Queue not updated for  <===> " + product.GetM_Product_ID() + " Error Type is : " + pp.GetName());
                        }
                    }
                    if (value)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="trxFrom"></param>
        /// <param name="qtyMove"></param>
        private void UpdateTransaction(MMovementLine line, MTransaction trxFrom, decimal qtyMove, int loc_ID)
        {
            MProduct pro = new MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
            int attribSet_ID = pro.GetM_AttributeSet_ID();
            string sql = "";
            DataSet ds = new DataSet();
            MTransaction trx = null;
            MInventoryLine inventoryLine = null;
            MInventory inventory = null;

            try
            {
                if (attribSet_ID > 0)
                {
                    //sql = "UPDATE M_Transaction SET CurrentQty = MovementQty + " + qtyMove + " WHERE movementdate >= " + GlobalVariable.TO_DATE(trxFrom.GetMovementDate(), true) + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID
                    //     + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID();
                    sql = @"SELECT M_AttributeSetInstance_ID ,  M_Locator_ID ,  M_Product_ID ,  movementqty ,  currentqty ,  movementdate ,  TO_CHAR(Created, 'DD-MON-YY HH24:MI:SS') , m_transaction_id , MovementType , M_InventoryLine_ID
                              FROM m_transaction WHERE movementdate >= " + GlobalVariable.TO_DATE(trxFrom.GetMovementDate().Value.AddDays(1), true)
                              + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID()
                              + " ORDER BY movementdate ASC , m_transaction_id ASC ,  created ASC";
                }
                else
                {
                    //sql = "UPDATE M_Transaction SET CurrentQty = MovementQty + " + qtyMove + " WHERE movementdate >= " + GlobalVariable.TO_DATE(trxFrom.GetMovementDate(), true) + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID
                    //       + " AND M_AttributeSetInstance_ID =  0 ";
                    sql = @"SELECT M_AttributeSetInstance_ID ,  M_Locator_ID ,  M_Product_ID ,  movementqty ,  currentqty ,  movementdate ,  TO_CHAR(Created, 'DD-MON-YY HH24:MI:SS') , m_transaction_id , MovementType , M_InventoryLine_ID
                              FROM m_transaction WHERE movementdate >= " + GlobalVariable.TO_DATE(trxFrom.GetMovementDate().Value.AddDays(1), true)
                             + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID + " AND M_AttributeSetInstance_ID = 0 "
                             + " ORDER BY movementdate ASC , m_transaction_id ASC ,  created ASC";
                }

                //int countUpd = Util.GetValueOfInt(DB.ExecuteQuery(sql, null, Get_TrxName()));
                ds = DB.ExecuteDataset(sql, null, Get_TrxName());
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        int i = 0;
                        for (i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (Util.GetValueOfString(ds.Tables[0].Rows[i]["MovementType"]) == "I+" &&
                                    Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_InventoryLine_ID"]) > 0)
                            {
                                inventoryLine = new MInventoryLine(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_InventoryLine_ID"]), Get_TrxName());
                                inventory = new MInventory(GetCtx(), Util.GetValueOfInt(inventoryLine.GetM_Inventory_ID()), null);
                                if (!inventory.IsInternalUse())
                                {
                                    //break;
                                    inventoryLine.SetQtyBook(qtyMove);
                                    inventoryLine.SetOpeningStock(qtyMove);
                                    inventoryLine.SetDifferenceQty(Decimal.Subtract(qtyMove, Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["currentqty"])));
                                    if (!inventoryLine.Save())
                                    {
                                        log.Info("Quantity Book and Quantity Differenec Not Updated at Inventory Line Tab <===> " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_InventoryLine_ID"]));
                                    }

                                    trx = new MTransaction(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]), Get_TrxName());
                                    trx.SetMovementQty(Decimal.Negate(Decimal.Subtract(qtyMove, Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["currentqty"]))));
                                    if (!trx.Save())
                                    {
                                        log.Info("Movement Quantity Not Updated at Transaction Tab for this ID" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]));
                                    }
                                    else
                                    {
                                        qtyMove = trx.GetCurrentQty();
                                    }
                                    if (i == ds.Tables[0].Rows.Count - 1)
                                    {
                                        MStorage storage = MStorage.Get(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                                                   Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                        if (storage == null)
                                        {
                                            storage = MStorage.GetCreate(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                                                     Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                        }
                                        if (storage.GetQtyOnHand() != qtyMove)
                                        {
                                            storage.SetQtyOnHand(qtyMove);
                                            storage.Save();
                                        }
                                    }
                                    continue;
                                }
                            }
                            trx = new MTransaction(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]), Get_TrxName());
                            trx.SetCurrentQty(qtyMove + trx.GetMovementQty());
                            if (!trx.Save())
                            {
                                log.Info("Current Quantity Not Updated at Transaction Tab for this ID" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["m_transaction_id"]));
                            }
                            else
                            {
                                qtyMove = trx.GetCurrentQty();
                            }
                            if (i == ds.Tables[0].Rows.Count - 1)
                            {
                                MStorage storage = MStorage.Get(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                                           Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                if (storage == null)
                                {
                                    storage = MStorage.GetCreate(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Locator_ID"]),
                                                             Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_Product_ID"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["M_AttributeSetInstance_ID"]), Get_TrxName());
                                }
                                if (storage.GetQtyOnHand() != qtyMove)
                                {
                                    storage.SetQtyOnHand(qtyMove);
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

        private void UpdateCurrentRecord(MMovementLine line, MTransaction trxM, decimal qtyDiffer, int loc_ID)
        {
            MProduct pro = new MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
            int attribSet_ID = pro.GetM_AttributeSet_ID();
            string sql = "";

            try
            {
                if (attribSet_ID > 0)
                {
                    sql = @"SELECT Count(*) from M_Transaction  WHERE MovementDate > " + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID;
                    int count = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, null));
                    if (count > 0)
                    {
                        sql = @"SELECT count(*)  FROM m_transaction tr  WHERE tr.movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + @" and
                     tr.m_product_id =" + line.GetM_Product_ID() + "  and tr.m_locator_ID=" + loc_ID + @" and tr.movementdate in (select max(movementdate) from m_transaction where
                     movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " and m_product_id =" + line.GetM_Product_ID() + "  and m_locator_ID=" + loc_ID + " )order by m_transaction_id desc";
                        int recordcount = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, null));
                        if (recordcount > 0)
                        {
                            sql = @"SELECT tr.currentqty  FROM m_transaction tr  WHERE tr.movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + @" and
                     tr.m_product_id =" + line.GetM_Product_ID() + "  and tr.m_locator_ID=" + loc_ID + @" and tr.movementdate in (select max(movementdate) from m_transaction where
                     movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " and m_product_id =" + line.GetM_Product_ID() + " and m_locator_ID=" + loc_ID + ") order by m_transaction_id desc";

                            Decimal? quantity = Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, null));
                            trxM.SetCurrentQty(Util.GetValueOfDecimal(Decimal.Add(Util.GetValueOfDecimal(quantity), Util.GetValueOfDecimal(qtyDiffer))));
                            if (!trxM.Save())
                            {

                            }
                        }
                        else
                        {
                            trxM.SetCurrentQty(qtyDiffer);
                            if (!trxM.Save())
                            {

                            }
                        }
                        //trxM.SetCurrentQty(

                    }

                    //sql = "UPDATE M_Transaction SET CurrentQty = CurrentQty + " + qtyDiffer + " WHERE MovementDate > " + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID()
                    //     + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID();
                }
                else
                {
                    sql = @"SELECT Count(*) from M_Transaction  WHERE MovementDate > " + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID;
                    int count = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, null));
                    if (count > 0)
                    {
                        sql = @"SELECT count(*)  FROM m_transaction tr  WHERE tr.movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + @" and
                     tr.m_product_id =" + line.GetM_Product_ID() + "  and tr.m_locator_ID=" + loc_ID + @" and tr.movementdate in (select max(movementdate) from m_transaction where
                     movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " and m_product_id =" + line.GetM_Product_ID() + "  and m_locator_ID=" + loc_ID + " )order by m_transaction_id desc";
                        int recordcount = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, null));
                        if (recordcount > 0)
                        {
                            sql = @"SELECT tr.currentqty  FROM m_transaction tr  WHERE tr.movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + @" and
                     tr.m_product_id =" + line.GetM_Product_ID() + "  and tr.m_locator_ID=" + loc_ID + @" and tr.movementdate in (select max(movementdate) from m_transaction where
                     movementdate<=" + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " and m_product_id =" + line.GetM_Product_ID() + " and m_locator_ID=" + loc_ID + ") order by m_transaction_id desc";

                            Decimal? quantity = Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, null));
                            trxM.SetCurrentQty(Util.GetValueOfDecimal(Decimal.Add(Util.GetValueOfDecimal(quantity), Util.GetValueOfDecimal(qtyDiffer))));
                            if (!trxM.Save())
                            {

                            }
                        }
                        else
                        {
                            trxM.SetCurrentQty(qtyDiffer);
                            if (!trxM.Save())
                            {

                            }
                        }
                        //trxM.SetCurrentQty(

                    }
                    //sql = "UPDATE M_Transaction SET CurrentQty = CurrentQty + " + qtyDiffer + " WHERE MovementDate > " + GlobalVariable.TO_DATE(trxM.GetMovementDate(), true) + " AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + line.GetM_Locator_ID();
                }

                // int countUpd = Util.GetValueOfInt(DB.ExecuteQuery(sql, null, Get_TrxName()));
            }
            catch
            {
                log.Info("Current Quantity Not Updated at Transaction Tab");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private decimal? GetProductQtyFromStorage(MMovementLine line, int loc_ID)
        {
            return 0;
            //MProduct pro = new MProduct(Env.GetCtx(), line.GetM_Product_ID(), Get_TrxName());
            //int attribSet_ID = pro.GetM_AttributeSet_ID();
            //string sql = "";

            //if (attribSet_ID > 0)
            //{
            //    sql = @"SELECT SUM(qtyonhand) FROM M_Storage WHERE M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID
            //         + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID();
            //}
            //else
            //{
            //    sql = @"SELECT SUM(qtyonhand) FROM M_Storage WHERE M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + loc_ID;
            //}
            //return Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, Get_TrxName()));
        }

        /// <summary>
        /// Get Latest Current Quantity based on movementdate
        /// </summary>
        /// <param name="line"></param>
        /// <param name="movementDate"></param>
        /// <param name="isAttribute"></param>
        /// <returns></returns>
        private decimal? GetProductQtyFromTransaction(MMovementLine line, DateTime? movementDate, bool isAttribute, int locatorId)
        {
            decimal result = 0;
            string sql = "";

            if (isAttribute && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM m_transaction WHERE movementdate = " + GlobalVariable.TO_DATE(movementDate, true) + @" 
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID(), null, Get_Trx())) > 0)
            {
                sql = @"SELECT currentqty FROM m_transaction WHERE m_transaction_id  =
                        (SELECT MAX(m_transaction_id)   FROM m_transaction
                          WHERE movementdate =     (SELECT MAX(movementdate) FROM m_transaction WHERE movementdate <= " + GlobalVariable.TO_DATE(movementDate, true) + @" 
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + @")
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + @")
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID();
                result = Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, Get_TrxName()));
            }
            else if (isAttribute && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM m_transaction WHERE movementdate < " + GlobalVariable.TO_DATE(movementDate, true) + @" 
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID(), null, Get_Trx())) > 0)
            {
                sql = @"SELECT currentqty FROM m_transaction WHERE m_transaction_id =
                        (SELECT MAX(m_transaction_id)   FROM m_transaction
                          WHERE movementdate =     (SELECT MAX(movementdate) FROM m_transaction WHERE movementdate < " + GlobalVariable.TO_DATE(movementDate, true) + @" 
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + @")
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID() + @")
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = " + line.GetM_AttributeSetInstance_ID();
                result = Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, Get_TrxName()));
            }
            else if (!isAttribute && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM m_transaction WHERE movementdate = " + GlobalVariable.TO_DATE(movementDate, true) + @" 
                          AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = 0 ", null, Get_Trx())) > 0)
            {
                sql = @"SELECT currentqty FROM m_transaction WHERE m_transaction_id =
                        (SELECT MAX(m_transaction_id)   FROM m_transaction
                          WHERE movementdate =     (SELECT MAX(movementdate) FROM m_transaction WHERE movementdate <= " + GlobalVariable.TO_DATE(movementDate, true) + @"
                          AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + "   AND ( M_AttributeSetInstance_ID = 0 OR M_AttributeSetInstance_ID IS NULL ) " + @")
                          AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + "   AND ( M_AttributeSetInstance_ID = 0 OR M_AttributeSetInstance_ID IS NULL ) " + @")
                          AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + "   AND ( M_AttributeSetInstance_ID = 0 OR M_AttributeSetInstance_ID IS NULL ) ";
                result = Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, Get_TrxName()));
            }
            else if (!isAttribute && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM m_transaction WHERE movementdate < " + GlobalVariable.TO_DATE(movementDate, true) + @" 
                           AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + " AND M_AttributeSetInstance_ID = 0 ", null, Get_Trx())) > 0)
            {
                sql = @"SELECT currentqty FROM m_transaction WHERE m_transaction_id =
                        (SELECT MAX(m_transaction_id)   FROM m_transaction
                          WHERE movementdate =     (SELECT MAX(movementdate) FROM m_transaction WHERE movementdate < " + GlobalVariable.TO_DATE(movementDate, true) + @"
                          AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + "   AND ( M_AttributeSetInstance_ID = 0 OR M_AttributeSetInstance_ID IS NULL ) " + @")
                          AND  M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + "   AND ( M_AttributeSetInstance_ID = 0 OR M_AttributeSetInstance_ID IS NULL ) " + @")
                          AND M_Product_ID = " + line.GetM_Product_ID() + " AND M_Locator_ID = " + locatorId + "   AND ( M_AttributeSetInstance_ID = 0 OR M_AttributeSetInstance_ID IS NULL ) ";
                result = Util.GetValueOfDecimal(DB.ExecuteScalar(sql, null, Get_TrxName()));
            }
            return result;
        }

        /// <summary>
        /// Check Material Policy
        /// </summary>
        private void CheckMaterialPolicy()
        {
            int no = MMovementLineMA.DeleteMovementMA(GetM_Movement_ID(), Get_TrxName());
            if (no > 0)
                log.Config("Delete old #" + no);
            MMovementLine[] lines = GetLines(false);

            MClient client = MClient.Get(GetCtx());

            //	Check Lines
            for (int i = 0; i < lines.Length; i++)
            {
                MMovementLine line = lines[i];

                Boolean needSave = false;

                //	Attribute Set Instance
                if (line.GetM_AttributeSetInstance_ID() == 0)
                {
                    MProduct product = MProduct.Get(GetCtx(), line.GetM_Product_ID());
                    MProductCategory pc = MProductCategory.Get(GetCtx(), product.GetM_Product_Category_ID());
                    String MMPolicy = pc.GetMMPolicy();
                    if (MMPolicy == null || MMPolicy.Length == 0)
                        MMPolicy = client.GetMMPolicy();
                    //
                    MStorage[] storages = MStorage.GetAllWithASI(GetCtx(),
                        line.GetM_Product_ID(), line.GetM_Locator_ID(),
                        MClient.MMPOLICY_FiFo.Equals(MMPolicy), Get_TrxName());
                    Decimal qtyToDeliver = line.GetMovementQty();
                    for (int ii = 0; ii < storages.Length; ii++)
                    {
                        MStorage storage = storages[ii];
                        if (ii == 0)
                        {
                            if (storage.GetQtyOnHand().CompareTo(qtyToDeliver) >= 0)
                            {
                                line.SetM_AttributeSetInstance_ID(storage.GetM_AttributeSetInstance_ID());
                                needSave = true;
                                log.Config("Direct - " + line);
                                qtyToDeliver = Env.ZERO;
                            }
                            else
                            {
                                log.Config("Split - " + line);
                                MMovementLineMA ma = new MMovementLineMA(line,
                                    storage.GetM_AttributeSetInstance_ID(),
                                    storage.GetQtyOnHand());
                                if (!ma.Save())
                                    ;
                                qtyToDeliver = Decimal.Subtract(qtyToDeliver, storage.GetQtyOnHand());
                                log.Fine("#" + ii + ": " + ma + ", QtyToDeliver=" + qtyToDeliver);
                            }
                        }
                        else	//	 create Addl material allocation
                        {
                            MMovementLineMA ma = new MMovementLineMA(line,
                                storage.GetM_AttributeSetInstance_ID(),
                                qtyToDeliver);
                            if (storage.GetQtyOnHand().CompareTo(qtyToDeliver) >= 0)
                                qtyToDeliver = Env.ZERO;
                            else
                            {
                                ma.SetMovementQty(storage.GetQtyOnHand());
                                qtyToDeliver = Decimal.Subtract(qtyToDeliver, storage.GetQtyOnHand());
                            }
                            if (!ma.Save())
                                ;
                            log.Fine("#" + ii + ": " + ma + ", QtyToDeliver=" + qtyToDeliver);
                        }
                        if (Env.Signum(qtyToDeliver) == 0)
                            break;
                    }	//	 for all storages

                    //	No AttributeSetInstance found for remainder
                    if (Env.Signum(qtyToDeliver) != 0)
                    {
                        MMovementLineMA ma = new MMovementLineMA(line,
                            0, qtyToDeliver);
                        if (!ma.Save())
                            ;
                        log.Fine("##: " + ma);
                    }
                }	//	attributeSetInstance

                if (needSave && !line.Save())
                    log.Severe("NOT saved " + line);
            }	//	for all lines

        }

        /// <summary>
        /// Check Material Policy
        /// </summary>
        /// <param name="line">movement line</param>
        private void CheckMaterialPolicy(MMovementLine line)
        {
            int no = MMovementLineMA.DeleteMovementLineMA(line.GetM_MovementLine_ID(), Get_TrxName());
            if (no > 0)
                log.Config("Delete old #" + no);

            MClient client = MClient.Get(GetCtx());
            Boolean needSave = false;

            //	Attribute Set Instance
            if (line.GetM_AttributeSetInstance_ID() == 0)
            {
                MProduct product = MProduct.Get(GetCtx(), line.GetM_Product_ID());
                MProductCategory pc = MProductCategory.Get(GetCtx(), product.GetM_Product_Category_ID());
                String MMPolicy = pc.GetMMPolicy();
                if (MMPolicy == null || MMPolicy.Length == 0)
                    MMPolicy = client.GetMMPolicy();
                //
                MStorage[] storages = MStorage.GetAllWithASI(GetCtx(),
                    line.GetM_Product_ID(), line.GetM_Locator_ID(),
                    MClient.MMPOLICY_FiFo.Equals(MMPolicy), Get_TrxName());
                Decimal qtyToDeliver = line.GetMovementQty();
                for (int ii = 0; ii < storages.Length; ii++)
                {
                    MStorage storage = storages[ii];
                    if (ii == 0)
                    {
                        if (storage.GetQtyOnHand().CompareTo(qtyToDeliver) >= 0)
                        {
                            line.SetM_AttributeSetInstance_ID(storage.GetM_AttributeSetInstance_ID());
                            needSave = true;
                            log.Config("Direct - " + line);
                            qtyToDeliver = Env.ZERO;
                        }
                        else
                        {
                            log.Config("Split - " + line);
                            MMovementLineMA ma = new MMovementLineMA(line,
                                storage.GetM_AttributeSetInstance_ID(),
                                storage.GetQtyOnHand());
                            if (!ma.Save())
                                ;
                            qtyToDeliver = Decimal.Subtract(qtyToDeliver, storage.GetQtyOnHand());
                            log.Fine("#" + ii + ": " + ma + ", QtyToDeliver=" + qtyToDeliver);
                        }
                    }
                    else	//	 create Addl material allocation
                    {
                        MMovementLineMA ma = new MMovementLineMA(line,
                            storage.GetM_AttributeSetInstance_ID(),
                            qtyToDeliver);
                        if (storage.GetQtyOnHand().CompareTo(qtyToDeliver) >= 0)
                            qtyToDeliver = Env.ZERO;
                        else
                        {
                            ma.SetMovementQty(storage.GetQtyOnHand());
                            qtyToDeliver = Decimal.Subtract(qtyToDeliver, storage.GetQtyOnHand());
                        }
                        if (!ma.Save())
                            ;
                        log.Fine("#" + ii + ": " + ma + ", QtyToDeliver=" + qtyToDeliver);
                    }
                    if (Env.Signum(qtyToDeliver) == 0)
                        break;
                }	//	 for all storages

                //	No AttributeSetInstance found for remainder
                if (Env.Signum(qtyToDeliver) != 0)
                {
                    MMovementLineMA ma = new MMovementLineMA(line,
                        0, qtyToDeliver);
                    if (!ma.Save())
                        ;
                    log.Fine("##: " + ma);
                }
            }	//	attributeSetInstance


            if (needSave && !line.Save())
                log.Severe("NOT saved " + line);

        }

        /// <summary>
        /// Void Document.
        /// </summary>
        /// <returns>true if success</returns>
        public Boolean VoidIt()
        {
            log.Info(ToString());
            if (DOCSTATUS_Closed.Equals(GetDocStatus())
                || DOCSTATUS_Reversed.Equals(GetDocStatus())
                || DOCSTATUS_Voided.Equals(GetDocStatus()))
            {
                _processMsg = "Document Closed: " + GetDocStatus();
                return false;
            }

            //	Not Processed
            if (DOCSTATUS_Drafted.Equals(GetDocStatus())
                || DOCSTATUS_Invalid.Equals(GetDocStatus())
                || DOCSTATUS_InProgress.Equals(GetDocStatus())
                || DOCSTATUS_Approved.Equals(GetDocStatus())
                || DOCSTATUS_NotApproved.Equals(GetDocStatus()))
            {
                //	Set lines to 0
                MMovementLine[] lines = GetLines(false);
                for (int i = 0; i < lines.Length; i++)
                {
                    MMovementLine line = lines[i];
                    Decimal old = line.GetMovementQty();
                    if (old.CompareTo(Env.ZERO) != 0)
                    {
                        line.SetMovementQty(Env.ZERO);
                        line.AddDescription("Void (" + old + ")");
                        line.Save(Get_TrxName());
                    }
                    //Amit 13-nov-2014
                    if (line.GetM_RequisitionLine_ID() > 0)
                    {
                        MRequisitionLine requisitionLine = new MRequisitionLine(GetCtx(), line.GetM_RequisitionLine_ID(), Get_Trx());
                        requisitionLine.SetDTD001_ReservedQty(Decimal.Subtract(requisitionLine.GetDTD001_ReservedQty(), old));
                        requisitionLine.Save(Get_Trx());

                        MStorage storageFrom = MStorage.Get(GetCtx(), line.GetM_Locator_ID(),
                            line.GetM_Product_ID(), line.GetM_AttributeSetInstance_ID(), Get_Trx());
                        if (storageFrom == null)
                            storageFrom = MStorage.GetCreate(GetCtx(), line.GetM_Locator_ID(),
                                line.GetM_Product_ID(), 0, Get_Trx());
                        storageFrom.SetQtyReserved(Decimal.Subtract(storageFrom.GetQtyReserved(), old));
                        storageFrom.Save(Get_Trx());
                    }
                    //Amit
                }
            }
            else
            {
                return ReverseCorrectIt();
            }

            SetProcessed(true);
            SetDocAction(DOCACTION_None);
            return true;
        }

        /// <summary>
        /// Close Document.
        /// </summary>
        /// <returns>true if success</returns>
        public Boolean CloseIt()
        {
            log.Info(ToString());

            //	Close Not delivered Qty
            SetDocAction(DOCACTION_None);
            return true;
        }

        /// <summary>
        /// Reverse Correction
        /// </summary>
        /// <returns>false</returns>
        public Boolean ReverseCorrectIt()
        {
            log.Info(ToString());
            MDocType dt = MDocType.Get(GetCtx(), GetC_DocType_ID());
            if (!MPeriod.IsOpen(GetCtx(), GetMovementDate(), dt.GetDocBaseType()))
            {
                _processMsg = "@PeriodClosed@";
                return false;
            }

            // is Non Business Day?
            if (MNonBusinessDay.IsNonBusinessDay(GetCtx(), GetMovementDate()))
            {
                _processMsg = VAdvantage.Common.Common.NONBUSINESSDAY;
                return false;
            }

            if (!CheckMovementExist(true))
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithFutureDate"));
                return false;
            }

            //if (!CheckMovementExist(false))
            //{
            //    log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithBackDate"));
            //    return false;
            //}


            //	Deep Copy
            MMovement reversal = new MMovement(GetCtx(), 0, Get_TrxName());
            CopyValues(this, reversal, GetAD_Client_ID(), GetAD_Org_ID());
            reversal.SetDocStatus(DOCSTATUS_Drafted);
            reversal.SetDocAction(DOCACTION_Complete);
            reversal.SetIsApproved(false);
            reversal.SetIsInTransit(false);
            reversal.SetPosted(false);
            reversal.SetProcessed(false);
            reversal.AddDescription("{->" + GetDocumentNo() + ")");
            if (!reversal.Save())
            {
                _processMsg = "Could not create Movement Reversal";
                return false;
            }

            //	Reverse Line Qty
            MMovementLine[] oLines = GetLines(true);
            for (int i = 0; i < oLines.Length; i++)
            {
                MMovementLine oLine = oLines[i];
                MMovementLine rLine = new MMovementLine(GetCtx(), 0, Get_TrxName());
                CopyValues(oLine, rLine, oLine.GetAD_Client_ID(), oLine.GetAD_Org_ID());
                rLine.SetM_Movement_ID(reversal.GetM_Movement_ID());
                //
                rLine.SetMovementQty(Decimal.Negate(rLine.GetMovementQty()));
                rLine.SetTargetQty(Env.ZERO);
                rLine.SetScrappedQty(Env.ZERO);
                rLine.SetConfirmedQty(Env.ZERO);
                rLine.SetProcessed(false);
                if (!rLine.Save())
                {
                    _processMsg = "Could not create Movement Reversal Line";
                    return false;
                }
            }
            //
            if (!reversal.ProcessIt(DocActionVariables.ACTION_COMPLETE))
            {
                _processMsg = "Reversal ERROR: " + reversal.GetProcessMsg();
                return false;
            }
            MMovementLine[] mlines = GetLines(true);
            for (int i = 0; i < mlines.Length; i++)
            {
                MMovementLine mline = mlines[i];
                if (mline.GetA_Asset_ID() > 0)
                {
                    ast = new MAsset(GetCtx(), mline.GetA_Asset_ID(), Get_Trx());
                    Tuple<String, String, String> aInfo = null;
                    if (Env.HasModulePrefix("VAFAM_", out aInfo))
                    {
                        MVAFAMAssetHistory aHist = new MVAFAMAssetHistory(GetCtx(), 0, Get_Trx());
                        ast.CopyTo(aHist);
                        aHist.SetA_Asset_ID(mline.GetA_Asset_ID());
                        if (!aHist.Save())
                        {
                            _processMsg = "Asset History Not Updated";
                            return false;
                        }
                    }
                    ast.SetM_Locator_ID(mline.GetM_Locator_ID());
                    ast.Save();
                }
            }
            reversal.CloseIt();
            reversal.SetDocStatus(DOCSTATUS_Reversed);
            reversal.SetDocAction(DOCACTION_None);
            reversal.Save();
            _processMsg = reversal.GetDocumentNo();

            //	Update Reversed (this)
            AddDescription("(" + reversal.GetDocumentNo() + "<-)");
            SetProcessed(true);
            SetDocStatus(DOCSTATUS_Reversed);	//	may come from void
            SetDocAction(DOCACTION_None);

            return true;
        }

        /// <summary>
        /// Reverse Accrual - none
        /// </summary>
        /// <returns>false</returns>
        public Boolean ReverseAccrualIt()
        {
            log.Info(ToString());
            return false;
        }

        /// <summary>
        /// Re-activate
        /// </summary>
        /// <returns>false</returns>
        public Boolean ReActivateIt()
        {
            log.Info(ToString());
            return false;
        }

        /// <summary>
        /// Get Summary
        /// </summary>
        /// <returns>Summary of Document</returns>
        public String GetSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetDocumentNo());
            //	: Total Lines = 123.00 (#1)
            sb.Append(": ")
                .Append(Msg.Translate(GetCtx(), "ApprovalAmt")).Append("=").Append(GetApprovalAmt())
                .Append(" (#").Append(GetLines(false).Length).Append(")");
            //	 - Description
            if (GetDescription() != null && GetDescription().Length > 0)
                sb.Append(" - ").Append(GetDescription());
            return sb.ToString();
        }

        /// <summary>
        /// String Representation
        /// </summary>
        /// <returns>info</returns>
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("MMovement[");
            sb.Append(Get_ID())
                .Append("-").Append(GetDocumentNo())
                .Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Get Process Message
        /// </summary>
        /// <returns>clear text error message</returns>
        public String GetProcessMsg()
        {
            return _processMsg;
        }

        /// <summary>
        /// Get Document Owner (Responsible)
        /// </summary>
        /// <returns>AD_User_ID</returns>
        public int GetDoc_User_ID()
        {
            return GetCreatedBy();
        }

        /// <summary>
        /// Get Document Currency
        /// </summary>
        /// <returns>C_Currency_ID</returns>
        //public int GetC_Currency_ID()
        //{
        //    //	MPriceList pl = MPriceList.Get(GetCtx(), GetM_PriceList_ID());
        //    //	return pl.GetC_Currency_ID();
        //    return 0;
        //}

        #region DocAction Members


        public Env.QueryParams GetLineOrgsQueryInfo()
        {
            return null;
        }

        public DateTime? GetDocumentDate()
        {
            return null;
        }

        public string GetDocBaseType()
        {
            return null;
        }



        public void SetProcessMsg(string processMsg)
        {

        }
        #endregion

    }
}
