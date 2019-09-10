namespace ViennaAdvantage.Model
{
    /** Generated Model - DO NOT CHANGE */
    using System;
    using System.Text;
    using VAdvantage.DataBase;
    using VAdvantage.Common;
    using VAdvantage.Classes;
    using VAdvantage.Process;
    using VAdvantage.Model;
    using VAdvantage.Utility;
    using System.Data;/** Generated Model for VAMFG_M_WrkOdrTrnsctionLine
 *  @author Raghu (Updated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
    public class X_VAMFG_M_WrkOdrTrnsctionLine : PO
    {
        public X_VAMFG_M_WrkOdrTrnsctionLine(Context ctx, int VAMFG_M_WrkOdrTrnsctionLine_ID, Trx trxName)
            : base(ctx, VAMFG_M_WrkOdrTrnsctionLine_ID, trxName)
        {/** if (VAMFG_M_WrkOdrTrnsctionLine_ID == 0){SetBasisType (null);// I
SetC_UOM_ID (0);SetM_Product_ID (0);SetProcessed (false);// N
SetVAMFG_Line (0);// @SQL=SELECT COALESCE(MAX(VAMFG_Line),0)+10 AS DefaultValue FROM M_WrkOdrTrnsactionLine WHERE M_WrkOdrTransaction_ID=@M_WrkOdrTransaction_ID@
SetVAMFG_M_WorkOrderOperation_ID (0);SetVAMFG_M_WrkOdrTransaction_ID (0);SetVAMFG_M_WrkOdrTrnsctionLine_ID (0);SetVAMFG_QtyEntered (0.0);} */
        }
        public X_VAMFG_M_WrkOdrTrnsctionLine(Ctx ctx, int VAMFG_M_WrkOdrTrnsctionLine_ID, Trx trxName)
            : base(ctx, VAMFG_M_WrkOdrTrnsctionLine_ID, trxName)
        {/** if (VAMFG_M_WrkOdrTrnsctionLine_ID == 0){SetBasisType (null);// I
SetC_UOM_ID (0);SetM_Product_ID (0);SetProcessed (false);// N
SetVAMFG_Line (0);// @SQL=SELECT COALESCE(MAX(VAMFG_Line),0)+10 AS DefaultValue FROM M_WrkOdrTrnsactionLine WHERE M_WrkOdrTransaction_ID=@M_WrkOdrTransaction_ID@
SetVAMFG_M_WorkOrderOperation_ID (0);SetVAMFG_M_WrkOdrTransaction_ID (0);SetVAMFG_M_WrkOdrTrnsctionLine_ID (0);SetVAMFG_QtyEntered (0.0);} */
        }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VAMFG_M_WrkOdrTrnsctionLine(Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) { }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VAMFG_M_WrkOdrTrnsctionLine(Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) { }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VAMFG_M_WrkOdrTrnsctionLine(Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName) { }/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
        static X_VAMFG_M_WrkOdrTrnsctionLine() { Table_ID = Get_Table_ID(Table_Name); model = new KeyNamePair(Table_ID, Table_Name); }/** Serial Version No */
        static long serialVersionUID = 27836927208535L;/** Last Updated Timestamp 4/9/2019 2:54:52 PM */
        public static long updatedMS = 1554801891746L;/** AD_Table_ID=1000731 */
        public static int Table_ID; // =1000731;
        /** TableName=VAMFG_M_WrkOdrTrnsctionLine */
        public static String Table_Name = "VAMFG_M_WrkOdrTrnsctionLine";
        protected static KeyNamePair model; protected Decimal accessLevel = new Decimal(1);/** AccessLevel
@return 1 - Org 
*/
        protected override int Get_AccessLevel() { return Convert.ToInt32(accessLevel.ToString()); }/** Load Meta Data
@param ctx context
@return PO Info
*/
        protected override POInfo InitPO(Context ctx) { POInfo poi = POInfo.GetPOInfo(ctx, Table_ID); return poi; }/** Load Meta Data
@param ctx context
@return PO Info
*/
        protected override POInfo InitPO(Ctx ctx) { POInfo poi = POInfo.GetPOInfo(ctx, Table_ID); return poi; }/** Info
@return info
*/
        public override String ToString() { StringBuilder sb = new StringBuilder("X_VAMFG_M_WrkOdrTrnsctionLine[").Append(Get_ID()).Append("]"); return sb.ToString(); }
        /** AD_OrgTrx_ID AD_Reference_ID=1000306 */
        public static int AD_ORGTRX_ID_AD_Reference_ID = 1000306;/** Set Trx Organization.
@param AD_OrgTrx_ID Performing or initiating organization */
        public void SetAD_OrgTrx_ID(int AD_OrgTrx_ID)
        {
            if (AD_OrgTrx_ID <= 0) Set_Value("AD_OrgTrx_ID", null);
            else
                Set_Value("AD_OrgTrx_ID", AD_OrgTrx_ID);
        }/** Get Trx Organization.
@return Performing or initiating organization */
        public int GetAD_OrgTrx_ID() { Object ii = Get_Value("AD_OrgTrx_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set User/Contact.
@param AD_User_ID User within the system - Internal or Customer/Prospect Contact. */
        public void SetAD_User_ID(int AD_User_ID)
        {
            if (AD_User_ID <= 0) Set_Value("AD_User_ID", null);
            else
                Set_Value("AD_User_ID", AD_User_ID);
        }/** Get User/Contact.
@return User within the system - Internal or Customer/Prospect Contact. */
        public int GetAD_User_ID() { Object ii = Get_Value("AD_User_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
        /** BasisType AD_Reference_ID=1000295 */
        public static int BASISTYPE_AD_Reference_ID = 1000295;/** Per Batch = B */
        public static String BASISTYPE_PerBatch = "B";/** Per Item = I */
        public static String BASISTYPE_PerItem = "I";/** Is test a valid value.
@param test testvalue
@returns true if valid **/
        public bool IsBasisTypeValid(String test) { return test.Equals("B") || test.Equals("I"); }/** Set Cost Basis Type.
@param BasisType Indicates the option to consume and charge materials and resources */
        public void SetBasisType(String BasisType)
        {
            if (BasisType == null) throw new ArgumentException("BasisType is mandatory"); if (!IsBasisTypeValid(BasisType))
                throw new ArgumentException("BasisType Invalid value - " + BasisType + " - Reference_ID=1000295 - B - I"); if (BasisType.Length > 1) { log.Warning("Length > 1 - truncated"); BasisType = BasisType.Substring(0, 1); } Set_Value("BasisType", BasisType);
        }/** Get Cost Basis Type.
@return Indicates the option to consume and charge materials and resources */
        public String GetBasisType() { return (String)Get_Value("BasisType"); }/** Set Activity.
@param C_Activity_ID Business Activity */
        public void SetC_Activity_ID(int C_Activity_ID)
        {
            if (C_Activity_ID <= 0) Set_Value("C_Activity_ID", null);
            else
                Set_Value("C_Activity_ID", C_Activity_ID);
        }/** Get Activity.
@return Business Activity */
        public int GetC_Activity_ID() { Object ii = Get_Value("C_Activity_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Business Partner.
@param C_BPartner_ID Identifies a Customer/Prospect */
        public void SetC_BPartner_ID(int C_BPartner_ID)
        {
            if (C_BPartner_ID <= 0) Set_Value("C_BPartner_ID", null);
            else
                Set_Value("C_BPartner_ID", C_BPartner_ID);
        }/** Get Business Partner.
@return Identifies a Customer/Prospect */
        public int GetC_BPartner_ID() { Object ii = Get_Value("C_BPartner_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Location.
@param C_BPartner_Location_ID Identifies the address for this Account/Prospect. */
        public void SetC_BPartner_Location_ID(int C_BPartner_Location_ID)
        {
            if (C_BPartner_Location_ID <= 0) Set_Value("C_BPartner_Location_ID", null);
            else
                Set_Value("C_BPartner_Location_ID", C_BPartner_Location_ID);
        }/** Get Location.
@return Identifies the address for this Account/Prospect. */
        public int GetC_BPartner_Location_ID() { Object ii = Get_Value("C_BPartner_Location_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Campaign.
@param C_Campaign_ID Marketing Campaign */
        public void SetC_Campaign_ID(int C_Campaign_ID)
        {
            if (C_Campaign_ID <= 0) Set_Value("C_Campaign_ID", null);
            else
                Set_Value("C_Campaign_ID", C_Campaign_ID);
        }/** Get Campaign.
@return Marketing Campaign */
        public int GetC_Campaign_ID() { Object ii = Get_Value("C_Campaign_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Opportunity.
@param C_Project_ID Business Opportunity */
        public void SetC_Project_ID(int C_Project_ID)
        {
            if (C_Project_ID <= 0) Set_Value("C_Project_ID", null);
            else
                Set_Value("C_Project_ID", C_Project_ID);
        }/** Get Opportunity.
@return Business Opportunity */
        public int GetC_Project_ID() { Object ii = Get_Value("C_Project_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set UOM.
@param C_UOM_ID Unit of Measure */
        public void SetC_UOM_ID(int C_UOM_ID) { if (C_UOM_ID < 1) throw new ArgumentException("C_UOM_ID is mandatory."); Set_Value("C_UOM_ID", C_UOM_ID); }/** Get UOM.
@return Unit of Measure */
        public int GetC_UOM_ID() { Object ii = Get_Value("C_UOM_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Current Cost.
@param CurrentCostPrice The currently used cost price */
        public void SetCurrentCostPrice(Decimal? CurrentCostPrice) { Set_Value("CurrentCostPrice", (Decimal?)CurrentCostPrice); }/** Get Current Cost.
@return The currently used cost price */
        public Decimal GetCurrentCostPrice() { Object bd = Get_Value("CurrentCostPrice"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set Export.
@param Export_ID Export */
        public void SetExport_ID(String Export_ID) { if (Export_ID != null && Export_ID.Length > 50) { log.Warning("Length > 50 - truncated"); Export_ID = Export_ID.Substring(0, 50); } Set_Value("Export_ID", Export_ID); }/** Get Export.
@return Export */
        public String GetExport_ID() { return (String)Get_Value("Export_ID"); }/** Set Actual Quantity (In KG).
@param GOM01_ActualQuantity Actual Quantity (In KG) */
        public void SetGOM01_ActualQuantity(Decimal? GOM01_ActualQuantity) { Set_Value("GOM01_ActualQuantity", (Decimal?)GOM01_ActualQuantity); }/** Get Actual Quantity (In KG).
@return Actual Quantity (In KG) */
        public Decimal GetGOM01_ActualQuantity() { Object bd = Get_Value("GOM01_ActualQuantity"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set Density.
@param GOM01_Density Density */
        public void SetGOM01_Density(Decimal? GOM01_Density) { Set_Value("GOM01_Density", (Decimal?)GOM01_Density); }/** Get Density.
@return Density */
        public Decimal GetGOM01_Density() { Object bd = Get_Value("GOM01_Density"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set From Process.
@param GOM01_FromProcess identifies that the record created from process or manual. */
        public void SetGOM01_FromProcess(Boolean GOM01_FromProcess) { Set_Value("GOM01_FromProcess", GOM01_FromProcess); }/** Get From Process.
@return identifies that the record created from process or manual. */
        public Boolean IsGOM01_FromProcess() { Object oo = Get_Value("GOM01_FromProcess"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Litres.
@param GOM01_Litre Litres */
        public void SetGOM01_Litre(Decimal? GOM01_Litre) { Set_Value("GOM01_Litre", (Decimal?)GOM01_Litre); }/** Get Litres.
@return Litres */
        public Decimal GetGOM01_Litre() { Object bd = Get_Value("GOM01_Litre"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set Quantity.
@param GOM01_Quantity Quantity */
        public void SetGOM01_Quantity(Decimal? GOM01_Quantity) { Set_Value("GOM01_Quantity", (Decimal?)GOM01_Quantity); }/** Get Quantity.
@return Quantity */
        public Decimal GetGOM01_Quantity() { Object bd = Get_Value("GOM01_Quantity"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set Cost Calculated.
@param IsCostCalculated Cost Calculated */
        public void SetIsCostCalculated(Boolean IsCostCalculated) { Set_Value("IsCostCalculated", IsCostCalculated); }/** Get Cost Calculated.
@return Cost Calculated */
        public Boolean IsCostCalculated() { Object oo = Get_Value("IsCostCalculated"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Cost Immediately.
@param IsCostImmediate Update Costs immediately for testing */
        public void SetIsCostImmediate(Boolean IsCostImmediate) { Set_Value("IsCostImmediate", IsCostImmediate); }/** Get Cost Immediately.
@return Update Costs immediately for testing */
        public Boolean IsCostImmediate() { Object oo = Get_Value("IsCostImmediate"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Quality Correction.
@param IsQualityCorrection Quality Correction */
        public void SetIsQualityCorrection(Boolean IsQualityCorrection) { Set_Value("IsQualityCorrection", IsQualityCorrection); }/** Get Quality Correction.
@return Quality Correction */
        public Boolean IsQualityCorrection() { Object oo = Get_Value("IsQualityCorrection"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Reversed Cost Calculated.
@param IsReversedCostCalculated Reversed Cost Calculated */
        public void SetIsReversedCostCalculated(Boolean IsReversedCostCalculated) { Set_Value("IsReversedCostCalculated", IsReversedCostCalculated); }/** Get Reversed Cost Calculated.
@return Reversed Cost Calculated */
        public Boolean IsReversedCostCalculated() { Object oo = Get_Value("IsReversedCostCalculated"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Attribute Set Instance.
@param M_AttributeSetInstance_ID Product Attribute Set Instance */
        public void SetM_AttributeSetInstance_ID(int M_AttributeSetInstance_ID)
        {
            if (M_AttributeSetInstance_ID <= 0) Set_Value("M_AttributeSetInstance_ID", null);
            else
                Set_Value("M_AttributeSetInstance_ID", M_AttributeSetInstance_ID);
        }/** Get Attribute Set Instance.
@return Product Attribute Set Instance */
        public int GetM_AttributeSetInstance_ID() { Object ii = Get_Value("M_AttributeSetInstance_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
        /** M_Locator_ID AD_Reference_ID=1000294 */
        public static int M_LOCATOR_ID_AD_Reference_ID = 1000294;/** Set Locator.
@param M_Locator_ID Warehouse Locator */
        public void SetM_Locator_ID(int M_Locator_ID)
        {
            if (M_Locator_ID <= 0) Set_Value("M_Locator_ID", null);
            else
                Set_Value("M_Locator_ID", M_Locator_ID);
        }/** Get Locator.
@return Warehouse Locator */
        public int GetM_Locator_ID() { Object ii = Get_Value("M_Locator_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Product.
@param M_Product_ID Product, Service, Item */
        public void SetM_Product_ID(int M_Product_ID) { if (M_Product_ID < 1) throw new ArgumentException("M_Product_ID is mandatory."); Set_Value("M_Product_ID", M_Product_ID); }/** Get Product.
@return Product, Service, Item */
        public int GetM_Product_ID() { Object ii = Get_Value("M_Product_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Processed.
@param Processed The document has been processed */
        public void SetProcessed(Boolean Processed) { Set_Value("Processed", Processed); }/** Get Processed.
@return The document has been processed */
        public Boolean IsProcessed() { Object oo = Get_Value("Processed"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Product Name.
@param ProductName Name of the Product */
        public void SetProductName(String ProductName) { throw new ArgumentException("ProductName Is virtual column"); }/** Get Product Name.
@return Name of the Product */
        public String GetProductName() { return (String)Get_Value("ProductName"); }/** Set Product Key.
@param ProductValue Key of the Product */
        public void SetProductValue(String ProductValue) { throw new ArgumentException("ProductValue Is virtual column"); }/** Get Product Key.
@return Key of the Product */
        public String GetProductValue() { return (String)Get_Value("ProductValue"); }/** Set Description.
@param VAMFG_Description Optional short description of the record */
        public void SetVAMFG_Description(String VAMFG_Description) { if (VAMFG_Description != null && VAMFG_Description.Length > 255) { log.Warning("Length > 255 - truncated"); VAMFG_Description = VAMFG_Description.Substring(0, 255); } Set_Value("VAMFG_Description", VAMFG_Description); }/** Get Description.
@return Optional short description of the record */
        public String GetVAMFG_Description() { return (String)Get_Value("VAMFG_Description"); }/** Set Comment.
@param VAMFG_Help Comment Help for hint */
        public void SetVAMFG_Help(String VAMFG_Help) { if (VAMFG_Help != null && VAMFG_Help.Length > 2000) { log.Warning("Length > 2000 - truncated"); VAMFG_Help = VAMFG_Help.Substring(0, 2000); } Set_Value("VAMFG_Help", VAMFG_Help); }/** Get Comment.
@return Comment Help for hint */
        public String GetVAMFG_Help() { return (String)Get_Value("VAMFG_Help"); }/** Set Line No.
@param VAMFG_Line Unique line for this document */
        public void SetVAMFG_Line(int VAMFG_Line) { Set_Value("VAMFG_Line", VAMFG_Line); }/** Get Line No.
@return Unique line for this document */
        public int GetVAMFG_Line() { Object ii = Get_Value("VAMFG_Line"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Operation.
@param VAMFG_M_WorkOrderOperation_ID Production routing operation on a work order */
        public void SetVAMFG_M_WorkOrderOperation_ID(int VAMFG_M_WorkOrderOperation_ID) { if (VAMFG_M_WorkOrderOperation_ID < 1) throw new ArgumentException("VAMFG_M_WorkOrderOperation_ID is mandatory."); Set_Value("VAMFG_M_WorkOrderOperation_ID", VAMFG_M_WorkOrderOperation_ID); }/** Get Operation.
@return Production routing operation on a work order */
        public int GetVAMFG_M_WorkOrderOperation_ID() { Object ii = Get_Value("VAMFG_M_WorkOrderOperation_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Production Execution.
@param VAMFG_M_WrkOdrTransaction_ID Production Execution */
        public void SetVAMFG_M_WrkOdrTransaction_ID(int VAMFG_M_WrkOdrTransaction_ID) { if (VAMFG_M_WrkOdrTransaction_ID < 1) throw new ArgumentException("VAMFG_M_WrkOdrTransaction_ID is mandatory."); Set_ValueNoCheck("VAMFG_M_WrkOdrTransaction_ID", VAMFG_M_WrkOdrTransaction_ID); }/** Get Production Execution.
@return Production Execution */
        public int GetVAMFG_M_WrkOdrTransaction_ID() { Object ii = Get_Value("VAMFG_M_WrkOdrTransaction_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Get Record ID/ColumnName
@return ID/ColumnName pair */
        public KeyNamePair GetKeyNamePair() { return new KeyNamePair(Get_ID(), GetVAMFG_M_WrkOdrTransaction_ID().ToString()); }/** Set VAMFG_M_WrkOdrTrnsctionLine_ID.
@param VAMFG_M_WrkOdrTrnsctionLine_ID VAMFG_M_WrkOdrTrnsctionLine_ID */
        public void SetVAMFG_M_WrkOdrTrnsctionLine_ID(int VAMFG_M_WrkOdrTrnsctionLine_ID) { if (VAMFG_M_WrkOdrTrnsctionLine_ID < 1) throw new ArgumentException("VAMFG_M_WrkOdrTrnsctionLine_ID is mandatory."); Set_ValueNoCheck("VAMFG_M_WrkOdrTrnsctionLine_ID", VAMFG_M_WrkOdrTrnsctionLine_ID); }/** Get VAMFG_M_WrkOdrTrnsctionLine_ID.
@return VAMFG_M_WrkOdrTrnsctionLine_ID */
        public int GetVAMFG_M_WrkOdrTrnsctionLine_ID() { Object ii = Get_Value("VAMFG_M_WrkOdrTrnsctionLine_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Quantity.
@param VAMFG_QtyEntered The Quantity Entered is based on the selected UoM */
        public void SetVAMFG_QtyEntered(Decimal? VAMFG_QtyEntered) { if (VAMFG_QtyEntered == null) throw new ArgumentException("VAMFG_QtyEntered is mandatory."); Set_Value("VAMFG_QtyEntered", (Decimal?)VAMFG_QtyEntered); }/** Get Quantity.
@return The Quantity Entered is based on the selected UoM */
        public Decimal GetVAMFG_QtyEntered() { Object bd = Get_Value("VAMFG_QtyEntered"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }
        /** VAMFG_User1_ID AD_Reference_ID=1000307 */
        public static int VAMFG_USER1_ID_AD_Reference_ID = 1000307;/** Set User List 1.
@param VAMFG_User1_ID User defined list element #1 */
        public void SetVAMFG_User1_ID(int VAMFG_User1_ID)
        {
            if (VAMFG_User1_ID <= 0) Set_Value("VAMFG_User1_ID", null);
            else
                Set_Value("VAMFG_User1_ID", VAMFG_User1_ID);
        }/** Get User List 1.
@return User defined list element #1 */
        public int GetVAMFG_User1_ID() { Object ii = Get_Value("VAMFG_User1_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
        /** VAMFG_User2_ID AD_Reference_ID=1000308 */
        public static int VAMFG_USER2_ID_AD_Reference_ID = 1000308;/** Set User List 2.
@param VAMFG_User2_ID User defined list element #2 */
        public void SetVAMFG_User2_ID(int VAMFG_User2_ID)
        {
            if (VAMFG_User2_ID <= 0) Set_Value("VAMFG_User2_ID", null);
            else
                Set_Value("VAMFG_User2_ID", VAMFG_User2_ID);
        }/** Get User List 2.
@return User defined list element #2 */
        public int GetVAMFG_User2_ID() { Object ii = Get_Value("VAMFG_User2_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
    }
}