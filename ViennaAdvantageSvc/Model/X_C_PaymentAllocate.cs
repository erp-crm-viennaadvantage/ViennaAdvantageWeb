namespace ViennaAdvantage.Model{
/** Generated Model - DO NOT CHANGE */
using System;using System.Text;using VAdvantage.DataBase;using VAdvantage.Common;using VAdvantage.Classes;using VAdvantage.Process;using VAdvantage.Model;using VAdvantage.Utility;using System.Data;/** Generated Model for C_PaymentAllocate
 *  @author Raghu (Updated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_C_PaymentAllocate : PO{public X_C_PaymentAllocate (Context ctx, int C_PaymentAllocate_ID, Trx trxName) : base (ctx, C_PaymentAllocate_ID, trxName){/** if (C_PaymentAllocate_ID == 0){SetAmount (0.0);SetC_Invoice_ID (0);SetC_PaymentAllocate_ID (0);SetC_Payment_ID (0);SetDiscountAmt (0.0);SetOverUnderAmt (0.0);SetWriteOffAmt (0.0);} */
}public X_C_PaymentAllocate (Ctx ctx, int C_PaymentAllocate_ID, Trx trxName) : base (ctx, C_PaymentAllocate_ID, trxName){/** if (C_PaymentAllocate_ID == 0){SetAmount (0.0);SetC_Invoice_ID (0);SetC_PaymentAllocate_ID (0);SetC_Payment_ID (0);SetDiscountAmt (0.0);SetOverUnderAmt (0.0);SetWriteOffAmt (0.0);} */
}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_C_PaymentAllocate (Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_C_PaymentAllocate (Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_C_PaymentAllocate (Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName){}/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_C_PaymentAllocate(){ Table_ID = Get_Table_ID(Table_Name); model = new KeyNamePair(Table_ID,Table_Name);}/** Serial Version No */
static long serialVersionUID = 27816973077595L;/** Last Updated Timestamp 8/21/2018 4:06:00 PM */
public static long updatedMS = 1534847760806L;/** AD_Table_ID=812 */
public static int Table_ID; // =812;
/** TableName=C_PaymentAllocate */
public static String Table_Name="C_PaymentAllocate";
protected static KeyNamePair model;protected Decimal accessLevel = new Decimal(1);/** AccessLevel
@return 1 - Org 
*/
protected override int Get_AccessLevel(){return Convert.ToInt32(accessLevel.ToString());}/** Load Meta Data
@param ctx context
@return PO Info
*/
protected override POInfo InitPO (Context ctx){POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);return poi;}/** Load Meta Data
@param ctx context
@return PO Info
*/
protected override POInfo InitPO (Ctx ctx){POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);return poi;}/** Info
@return info
*/
public override String ToString(){StringBuilder sb = new StringBuilder ("X_C_PaymentAllocate[").Append(Get_ID()).Append("]");return sb.ToString();}/** Set Amount.
@param Amount Amount in a defined currency */
public void SetAmount (Decimal? Amount){if (Amount == null) throw new ArgumentException ("Amount is mandatory.");Set_Value ("Amount", (Decimal?)Amount);}/** Get Amount.
@return Amount in a defined currency */
public Decimal GetAmount() {Object bd =Get_Value("Amount");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}/** Set Allocation Line.
@param C_AllocationLine_ID Allocation Line */
public void SetC_AllocationLine_ID (int C_AllocationLine_ID){if (C_AllocationLine_ID <= 0) Set_Value ("C_AllocationLine_ID", null);else
Set_Value ("C_AllocationLine_ID", C_AllocationLine_ID);}/** Get Allocation Line.
@return Allocation Line */
public int GetC_AllocationLine_ID() {Object ii = Get_Value("C_AllocationLine_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Charge.
@param C_Charge_ID Additional document charges */
public void SetC_Charge_ID (int C_Charge_ID){if (C_Charge_ID <= 0) Set_Value ("C_Charge_ID", null);else
Set_Value ("C_Charge_ID", C_Charge_ID);}/** Get Charge.
@return Additional document charges */
public int GetC_Charge_ID() {Object ii = Get_Value("C_Charge_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Invoice Payment Schedule.
@param C_InvoicePaySchedule_ID Invoice Payment Schedule */
public void SetC_InvoicePaySchedule_ID (int C_InvoicePaySchedule_ID){if (C_InvoicePaySchedule_ID <= 0) Set_Value ("C_InvoicePaySchedule_ID", null);else
Set_Value ("C_InvoicePaySchedule_ID", C_InvoicePaySchedule_ID);}/** Get Invoice Payment Schedule.
@return Invoice Payment Schedule */
public int GetC_InvoicePaySchedule_ID() {Object ii = Get_Value("C_InvoicePaySchedule_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Invoice.
@param C_Invoice_ID Invoice Identifier */
public void SetC_Invoice_ID (int C_Invoice_ID){if (C_Invoice_ID < 1) throw new ArgumentException ("C_Invoice_ID is mandatory.");Set_Value ("C_Invoice_ID", C_Invoice_ID);}/** Get Invoice.
@return Invoice Identifier */
public int GetC_Invoice_ID() {Object ii = Get_Value("C_Invoice_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Get Record ID/ColumnName
@return ID/ColumnName pair */
public KeyNamePair GetKeyNamePair() {return new KeyNamePair(Get_ID(), GetC_Invoice_ID().ToString());}/** Set Allocate Payment.
@param C_PaymentAllocate_ID Allocate Payment to Invoices */
public void SetC_PaymentAllocate_ID (int C_PaymentAllocate_ID){if (C_PaymentAllocate_ID < 1) throw new ArgumentException ("C_PaymentAllocate_ID is mandatory.");Set_ValueNoCheck ("C_PaymentAllocate_ID", C_PaymentAllocate_ID);}/** Get Allocate Payment.
@return Allocate Payment to Invoices */
public int GetC_PaymentAllocate_ID() {Object ii = Get_Value("C_PaymentAllocate_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Payment.
@param C_Payment_ID Payment identifier */
public void SetC_Payment_ID (int C_Payment_ID){if (C_Payment_ID < 1) throw new ArgumentException ("C_Payment_ID is mandatory.");Set_ValueNoCheck ("C_Payment_ID", C_Payment_ID);}/** Get Payment.
@return Payment identifier */
public int GetC_Payment_ID() {Object ii = Get_Value("C_Payment_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Tax.
@param C_Tax_ID Tax identifier */
public void SetC_Tax_ID (int C_Tax_ID){if (C_Tax_ID <= 0) Set_Value ("C_Tax_ID", null);else
Set_Value ("C_Tax_ID", C_Tax_ID);}/** Get Tax.
@return Tax identifier */
public int GetC_Tax_ID() {Object ii = Get_Value("C_Tax_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Discount Amount.
@param DiscountAmt Calculated amount of discount */
public void SetDiscountAmt (Decimal? DiscountAmt){if (DiscountAmt == null) throw new ArgumentException ("DiscountAmt is mandatory.");Set_Value ("DiscountAmt", (Decimal?)DiscountAmt);}/** Get Discount Amount.
@return Calculated amount of discount */
public Decimal GetDiscountAmt() {Object bd =Get_Value("DiscountAmt");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}/** Set Export.
@param Export_ID Export */
public void SetExport_ID (String Export_ID){if (Export_ID != null && Export_ID.Length > 50){log.Warning("Length > 50 - truncated");Export_ID = Export_ID.Substring(0,50);}Set_ValueNoCheck ("Export_ID", Export_ID);}/** Get Export.
@return Export */
public String GetExport_ID() {return (String)Get_Value("Export_ID");}
/** INT13_AllocateType AD_Reference_ID=1000226 */
public static int INT13_ALLOCATETYPE_AD_Reference_ID=1000226;/** Charge = C */
public static String INT13_ALLOCATETYPE_Charge = "C";/** Invoice = I */
public static String INT13_ALLOCATETYPE_Invoice = "I";/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsINT13_AllocateTypeValid (String test){return test == null || test.Equals("C") || test.Equals("I");}/** Set Allocate Type.
@param INT13_AllocateType Allocate Type */
public void SetINT13_AllocateType (String INT13_AllocateType){if (!IsINT13_AllocateTypeValid(INT13_AllocateType))
throw new ArgumentException ("INT13_AllocateType Invalid value - " + INT13_AllocateType + " - Reference_ID=1000226 - C - I");if (INT13_AllocateType != null && INT13_AllocateType.Length > 1){log.Warning("Length > 1 - truncated");INT13_AllocateType = INT13_AllocateType.Substring(0,1);}Set_Value ("INT13_AllocateType", INT13_AllocateType);}/** Get Allocate Type.
@return Allocate Type */
public String GetINT13_AllocateType() {return (String)Get_Value("INT13_AllocateType");}/** Set Invoice Amt.
@param InvoiceAmt Invoice Amt */
public void SetInvoiceAmt (Decimal? InvoiceAmt){Set_Value ("InvoiceAmt", (Decimal?)InvoiceAmt);}/** Get Invoice Amt.
@return Invoice Amt */
public Decimal GetInvoiceAmt() {Object bd =Get_Value("InvoiceAmt");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}/** Set Over/Under Payment.
@param OverUnderAmt Over-Payment (unallocated) or Under-Payment (partial payment) Amount */
public void SetOverUnderAmt (Decimal? OverUnderAmt){if (OverUnderAmt == null) throw new ArgumentException ("OverUnderAmt is mandatory.");Set_Value ("OverUnderAmt", (Decimal?)OverUnderAmt);}/** Get Over/Under Payment.
@return Over-Payment (unallocated) or Under-Payment (partial payment) Amount */
public Decimal GetOverUnderAmt() {Object bd =Get_Value("OverUnderAmt");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}/** Set Remaining Amt.
@param RemainingAmt Remaining Amount */
public void SetRemainingAmt (Decimal? RemainingAmt){throw new ArgumentException ("RemainingAmt Is virtual column");}/** Get Remaining Amt.
@return Remaining Amount */
public Decimal GetRemainingAmt() {Object bd =Get_Value("RemainingAmt");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}/** Set Tax Amount.
@param TaxAmount Tax Amount */
public void SetTaxAmount (Decimal? TaxAmount){Set_Value ("TaxAmount", (Decimal?)TaxAmount);}/** Get Tax Amount.
@return Tax Amount */
public Decimal GetTaxAmount() {Object bd =Get_Value("TaxAmount");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}
/** VA009_ExecutionStatus AD_Reference_ID=1000227 */
public static int VA009_EXECUTIONSTATUS_AD_Reference_ID=1000227;/** Awaited = A */
public static String VA009_EXECUTIONSTATUS_Awaited = "A";/** Bounced = B */
public static String VA009_EXECUTIONSTATUS_Bounced = "B";/** Rejected = C */
public static String VA009_EXECUTIONSTATUS_Rejected = "C";/** Dunning = D */
public static String VA009_EXECUTIONSTATUS_Dunning = "D";/** In-Progress = I */
public static String VA009_EXECUTIONSTATUS_In_Progress = "I";/** Assigned To Journal = J */
public static String VA009_EXECUTIONSTATUS_AssignedToJournal = "J";/** Overdue = O */
public static String VA009_EXECUTIONSTATUS_Overdue = "O";/** Received = R */
public static String VA009_EXECUTIONSTATUS_Received = "R";/** Stopped = S */
public static String VA009_EXECUTIONSTATUS_Stopped = "S";/** Write-Off = W */
public static String VA009_EXECUTIONSTATUS_Write_Off = "W";/** Assigned To Batch = Y */
public static String VA009_EXECUTIONSTATUS_AssignedToBatch = "Y";/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsVA009_ExecutionStatusValid (String test){return test == null || test.Equals("A") || test.Equals("B") || test.Equals("C") || test.Equals("D") || test.Equals("I") || test.Equals("J") || test.Equals("O") || test.Equals("R") || test.Equals("S") || test.Equals("W") || test.Equals("Y");}/** Set Payment Execution Status.
@param VA009_ExecutionStatus Payment Execution Status */
public void SetVA009_ExecutionStatus (String VA009_ExecutionStatus){if (!IsVA009_ExecutionStatusValid(VA009_ExecutionStatus))
throw new ArgumentException ("VA009_ExecutionStatus Invalid value - " + VA009_ExecutionStatus + " - Reference_ID=1000227 - A - B - C - D - I - J - O - R - S - W - Y");if (VA009_ExecutionStatus != null && VA009_ExecutionStatus.Length > 1){log.Warning("Length > 1 - truncated");VA009_ExecutionStatus = VA009_ExecutionStatus.Substring(0,1);}Set_Value ("VA009_ExecutionStatus", VA009_ExecutionStatus);}/** Get Payment Execution Status.
@return Payment Execution Status */
public String GetVA009_ExecutionStatus() {return (String)Get_Value("VA009_ExecutionStatus");}/** Set Payment Method.
@param VA009_PaymentMethod_ID The way that a buyer choose to pay the seller of a good or service. */
public void SetVA009_PaymentMethod_ID (int VA009_PaymentMethod_ID){if (VA009_PaymentMethod_ID <= 0) Set_Value ("VA009_PaymentMethod_ID", null);else
Set_Value ("VA009_PaymentMethod_ID", VA009_PaymentMethod_ID);}/** Get Payment Method.
@return The way that a buyer choose to pay the seller of a good or service. */
public int GetVA009_PaymentMethod_ID() {Object ii = Get_Value("VA009_PaymentMethod_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Write-off Amount.
@param WriteOffAmt Amount to write-off */
public void SetWriteOffAmt (Decimal? WriteOffAmt){if (WriteOffAmt == null) throw new ArgumentException ("WriteOffAmt is mandatory.");Set_Value ("WriteOffAmt", (Decimal?)WriteOffAmt);}/** Get Write-off Amount.
@return Amount to write-off */
public Decimal GetWriteOffAmt() {Object bd =Get_Value("WriteOffAmt");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}}
}