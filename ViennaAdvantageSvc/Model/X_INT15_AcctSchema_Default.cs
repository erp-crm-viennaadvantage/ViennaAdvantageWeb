namespace ViennaAdvantage.Model{
/** Generated Model - DO NOT CHANGE */
using System;using System.Text;using VAdvantage.DataBase;using VAdvantage.Common;using VAdvantage.Classes;using VAdvantage.Process;using VAdvantage.Model;using VAdvantage.Utility;using System.Data;/** Generated Model for INT15_AcctSchema_Default
 *  @author Raghu (Updated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_INT15_AcctSchema_Default : PO{public X_INT15_AcctSchema_Default (Context ctx, int INT15_AcctSchema_Default_ID, Trx trxName) : base (ctx, INT15_AcctSchema_Default_ID, trxName){/** if (INT15_AcctSchema_Default_ID == 0){SetC_AcctSchema_ID (0);SetINT15_AcctSchema_Default_ID (0);} */
}public X_INT15_AcctSchema_Default (Ctx ctx, int INT15_AcctSchema_Default_ID, Trx trxName) : base (ctx, INT15_AcctSchema_Default_ID, trxName){/** if (INT15_AcctSchema_Default_ID == 0){SetC_AcctSchema_ID (0);SetINT15_AcctSchema_Default_ID (0);} */
}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_AcctSchema_Default (Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_AcctSchema_Default (Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_AcctSchema_Default (Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName){}/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_INT15_AcctSchema_Default(){ Table_ID = Get_Table_ID(Table_Name); model = new KeyNamePair(Table_ID,Table_Name);}/** Serial Version No */
static long serialVersionUID = 27823021364134L;/** Last Updated Timestamp 10/30/2018 4:10:47 PM */
public static long updatedMS = 1540896047345L;/** AD_Table_ID=1000582 */
public static int Table_ID; // =1000582;
/** TableName=INT15_AcctSchema_Default */
public static String Table_Name="INT15_AcctSchema_Default";
protected static KeyNamePair model;protected Decimal accessLevel = new Decimal(3);/** AccessLevel
@return 3 - Client - Org 
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
public override String ToString(){StringBuilder sb = new StringBuilder ("X_INT15_AcctSchema_Default[").Append(Get_ID()).Append("]");return sb.ToString();}/** Set Accounting Schema.
@param C_AcctSchema_ID Rules for accounting */
public void SetC_AcctSchema_ID (int C_AcctSchema_ID){if (C_AcctSchema_ID < 1) throw new ArgumentException ("C_AcctSchema_ID is mandatory.");Set_ValueNoCheck ("C_AcctSchema_ID", C_AcctSchema_ID);}/** Get Accounting Schema.
@return Rules for accounting */
public int GetC_AcctSchema_ID() {Object ii = Get_Value("C_AcctSchema_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Combination.
@param C_ValidCombination_ID Valid Account Combination */
public void SetC_ValidCombination_ID (int C_ValidCombination_ID){if (C_ValidCombination_ID <= 0) Set_Value ("C_ValidCombination_ID", null);else
Set_Value ("C_ValidCombination_ID", C_ValidCombination_ID);}/** Get Combination.
@return Valid Account Combination */
public int GetC_ValidCombination_ID() {Object ii = Get_Value("C_ValidCombination_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Export.
@param Export_ID Export */
public void SetExport_ID (String Export_ID){if (Export_ID != null && Export_ID.Length > 50){log.Warning("Length > 50 - truncated");Export_ID = Export_ID.Substring(0,50);}Set_Value ("Export_ID", Export_ID);}/** Get Export.
@return Export */
public String GetExport_ID() {return (String)Get_Value("Export_ID");}/** Set Accounting Default.
@param FRPT_AcctDefault_ID Accounting Default */
public void SetFRPT_AcctDefault_ID (int FRPT_AcctDefault_ID){if (FRPT_AcctDefault_ID <= 0) Set_Value ("FRPT_AcctDefault_ID", null);else
Set_Value ("FRPT_AcctDefault_ID", FRPT_AcctDefault_ID);}/** Get Accounting Default.
@return Accounting Default */
public int GetFRPT_AcctDefault_ID() {Object ii = Get_Value("FRPT_AcctDefault_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set INT15_AcctSchema_Default_ID.
@param INT15_AcctSchema_Default_ID INT15_AcctSchema_Default_ID */
public void SetINT15_AcctSchema_Default_ID (int INT15_AcctSchema_Default_ID){if (INT15_AcctSchema_Default_ID < 1) throw new ArgumentException ("INT15_AcctSchema_Default_ID is mandatory.");Set_ValueNoCheck ("INT15_AcctSchema_Default_ID", INT15_AcctSchema_Default_ID);}/** Get INT15_AcctSchema_Default_ID.
@return INT15_AcctSchema_Default_ID */
public int GetINT15_AcctSchema_Default_ID() {Object ii = Get_Value("INT15_AcctSchema_Default_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Sequence.
@param SeqNo Method of ordering elements; lowest number comes first */
public void SetSeqNo (int SeqNo){Set_Value ("SeqNo", SeqNo);}/** Get Sequence.
@return Method of ordering elements; lowest number comes first */
public int GetSeqNo() {Object ii = Get_Value("SeqNo");if (ii == null) return 0;return Convert.ToInt32(ii);}}
}