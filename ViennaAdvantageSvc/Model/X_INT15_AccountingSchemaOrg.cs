namespace ViennaAdvantage.Model{
/** Generated Model - DO NOT CHANGE */
using System;using System.Text;using VAdvantage.DataBase;using VAdvantage.Common;using VAdvantage.Classes;using VAdvantage.Process;using VAdvantage.Model;using VAdvantage.Utility;using System.Data;/** Generated Model for INT15_AccountingSchemaOrg
 *  @author Raghu (Updated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_INT15_AccountingSchemaOrg : PO{public X_INT15_AccountingSchemaOrg (Context ctx, int INT15_AccountingSchemaOrg_ID, Trx trxName) : base (ctx, INT15_AccountingSchemaOrg_ID, trxName){/** if (INT15_AccountingSchemaOrg_ID == 0){SetC_AcctSchema_ID (0);SetC_Currency_ID (0);SetINT15_AccountingSchemaOrg_ID (0);} */
}public X_INT15_AccountingSchemaOrg (Ctx ctx, int INT15_AccountingSchemaOrg_ID, Trx trxName) : base (ctx, INT15_AccountingSchemaOrg_ID, trxName){/** if (INT15_AccountingSchemaOrg_ID == 0){SetC_AcctSchema_ID (0);SetC_Currency_ID (0);SetINT15_AccountingSchemaOrg_ID (0);} */
}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_AccountingSchemaOrg (Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_AccountingSchemaOrg (Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_AccountingSchemaOrg (Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName){}/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_INT15_AccountingSchemaOrg(){ Table_ID = Get_Table_ID(Table_Name); model = new KeyNamePair(Table_ID,Table_Name);}/** Serial Version No */
static long serialVersionUID = 27822918399768L;/** Last Updated Timestamp 10/29/2018 11:34:43 AM */
public static long updatedMS = 1540793082979L;/** AD_Table_ID=1000581 */
public static int Table_ID; // =1000581;
/** TableName=INT15_AccountingSchemaOrg */
public static String Table_Name="INT15_AccountingSchemaOrg";
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
public override String ToString(){StringBuilder sb = new StringBuilder ("X_INT15_AccountingSchemaOrg[").Append(Get_ID()).Append("]");return sb.ToString();}/** Set Accounting Schema.
@param C_AcctSchema_ID Rules for accounting */
public void SetC_AcctSchema_ID (int C_AcctSchema_ID){if (C_AcctSchema_ID < 1) throw new ArgumentException ("C_AcctSchema_ID is mandatory.");Set_Value ("C_AcctSchema_ID", C_AcctSchema_ID);}/** Get Accounting Schema.
@return Rules for accounting */
public int GetC_AcctSchema_ID() {Object ii = Get_Value("C_AcctSchema_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Currency.
@param C_Currency_ID The Currency for this record */
public void SetC_Currency_ID (int C_Currency_ID){if (C_Currency_ID < 1) throw new ArgumentException ("C_Currency_ID is mandatory.");Set_Value ("C_Currency_ID", C_Currency_ID);}/** Get Currency.
@return The Currency for this record */
public int GetC_Currency_ID() {Object ii = Get_Value("C_Currency_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Description.
@param Description Optional short description of the record */
public void SetDescription (String Description){Set_Value ("Description", Description);}/** Get Description.
@return Optional short description of the record */
public String GetDescription() {return (String)Get_Value("Description");}/** Set Export.
@param Export_ID Export */
public void SetExport_ID (String Export_ID){if (Export_ID != null && Export_ID.Length > 50){log.Warning("Length > 50 - truncated");Export_ID = Export_ID.Substring(0,50);}Set_Value ("Export_ID", Export_ID);}/** Get Export.
@return Export */
public String GetExport_ID() {return (String)Get_Value("Export_ID");}/** Set INT15_AccountingSchemaOrg_ID.
@param INT15_AccountingSchemaOrg_ID INT15_AccountingSchemaOrg_ID */
public void SetINT15_AccountingSchemaOrg_ID (int INT15_AccountingSchemaOrg_ID){if (INT15_AccountingSchemaOrg_ID < 1) throw new ArgumentException ("INT15_AccountingSchemaOrg_ID is mandatory.");Set_ValueNoCheck ("INT15_AccountingSchemaOrg_ID", INT15_AccountingSchemaOrg_ID);}/** Get INT15_AccountingSchemaOrg_ID.
@return INT15_AccountingSchemaOrg_ID */
public int GetINT15_AccountingSchemaOrg_ID() {Object ii = Get_Value("INT15_AccountingSchemaOrg_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}}
}