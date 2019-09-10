namespace ViennaAdvantage.Model{
/** Generated Model - DO NOT CHANGE */
using System;using System.Text;using VAdvantage.DataBase;using VAdvantage.Common;using VAdvantage.Classes;using VAdvantage.Process;using VAdvantage.Model;using VAdvantage.Utility;using System.Data;/** Generated Model for INT15_RevenueService
 *  @author Raghu (Updated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_INT15_RevenueService : PO{public X_INT15_RevenueService (Context ctx, int INT15_RevenueService_ID, Trx trxName) : base (ctx, INT15_RevenueService_ID, trxName){/** if (INT15_RevenueService_ID == 0){SetC_RevenueRecognition_ID (0);SetINT15_RevenueService_ID (0);} */
}public X_INT15_RevenueService (Ctx ctx, int INT15_RevenueService_ID, Trx trxName) : base (ctx, INT15_RevenueService_ID, trxName){/** if (INT15_RevenueService_ID == 0){SetC_RevenueRecognition_ID (0);SetINT15_RevenueService_ID (0);} */
}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_RevenueService (Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_RevenueService (Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName){}/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_INT15_RevenueService (Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName){}/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_INT15_RevenueService(){ Table_ID = Get_Table_ID(Table_Name); model = new KeyNamePair(Table_ID,Table_Name);}/** Serial Version No */
static long serialVersionUID = 27823004216651L;/** Last Updated Timestamp 10/30/2018 11:24:59 AM */
public static long updatedMS = 1540878899862L;/** AD_Table_ID=1000580 */
public static int Table_ID; // =1000580;
/** TableName=INT15_RevenueService */
public static String Table_Name="INT15_RevenueService";
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
public override String ToString(){StringBuilder sb = new StringBuilder ("X_INT15_RevenueService[").Append(Get_ID()).Append("]");return sb.ToString();}/** Set Revenue Recognition.
@param C_RevenueRecognition_ID Method for recording revenue */
public void SetC_RevenueRecognition_ID (int C_RevenueRecognition_ID){if (C_RevenueRecognition_ID < 1) throw new ArgumentException ("C_RevenueRecognition_ID is mandatory.");Set_ValueNoCheck ("C_RevenueRecognition_ID", C_RevenueRecognition_ID);}/** Get Revenue Recognition.
@return Method for recording revenue */
public int GetC_RevenueRecognition_ID() {Object ii = Get_Value("C_RevenueRecognition_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Description.
@param Description Optional short description of the record */
public void SetDescription (String Description){Set_Value ("Description", Description);}/** Get Description.
@return Optional short description of the record */
public String GetDescription() {return (String)Get_Value("Description");}/** Set Export.
@param Export_ID Export */
public void SetExport_ID (String Export_ID){if (Export_ID != null && Export_ID.Length > 50){log.Warning("Length > 50 - truncated");Export_ID = Export_ID.Substring(0,50);}Set_Value ("Export_ID", Export_ID);}/** Get Export.
@return Export */
public String GetExport_ID() {return (String)Get_Value("Export_ID");}/** Set Percent.
@param INT15_Percentage Percent */
public void SetINT15_Percentage (Decimal? INT15_Percentage){Set_Value ("INT15_Percentage", (Decimal?)INT15_Percentage);}/** Get Percent.
@return Percent */
public Decimal GetINT15_Percentage() {Object bd =Get_Value("INT15_Percentage");if (bd == null) return Env.ZERO;return  Convert.ToDecimal(bd);}/** Set INT15_RevenueService_ID.
@param INT15_RevenueService_ID INT15_RevenueService_ID */
public void SetINT15_RevenueService_ID (int INT15_RevenueService_ID){if (INT15_RevenueService_ID < 1) throw new ArgumentException ("INT15_RevenueService_ID is mandatory.");Set_ValueNoCheck ("INT15_RevenueService_ID", INT15_RevenueService_ID);}/** Get INT15_RevenueService_ID.
@return INT15_RevenueService_ID */
public int GetINT15_RevenueService_ID() {Object ii = Get_Value("INT15_RevenueService_ID");if (ii == null) return 0;return Convert.ToInt32(ii);}/** Set Line.
@param LineNo Line No */
public void SetLineNo (int LineNo){Set_Value ("LineNo", LineNo);}/** Get Line.
@return Line No */
public int GetLineNo() {Object ii = Get_Value("LineNo");if (ii == null) return 0;return Convert.ToInt32(ii);}}
}