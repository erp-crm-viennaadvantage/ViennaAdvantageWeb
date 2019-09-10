using System;
using System.Data;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Utility;
//using ViennaAdvantage.Model;

namespace ViennaAdvantage.Model
{
    public class MINT15AccountingSchemaOrg : X_INT15_AccountingSchemaOrg
    {
        //	Static Logger	
        private static VLogger _log = VLogger.GetVLogger(typeof(MINT15AccountingSchemaOrg).FullName);
        /// <summary>
        /// Standard Constructor
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="INT15_AccountingSchemaOrg_ID"></param>
        /// <param name="trxName"></param>
        public MINT15AccountingSchemaOrg(Ctx ctx, int INT15_AccountingSchemaOrg_ID, Trx trxName) : base(ctx, INT15_AccountingSchemaOrg_ID, trxName)
        {

        }

        /// <summary>
        /// Load Constructor
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="rs"></param>
        /// <param name="trxName"></param>
        public MINT15AccountingSchemaOrg(Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName)
        {

        }

        protected override bool BeforeSave(bool newRecord)
        {
            if (newRecord || Is_ValueChanged("C_AcctSchema_ID") || Is_ValueChanged("AD_Org_ID"))
            {
                string sql = "Select Count(INT15_AccountingSchemaOrg_ID) From INT15_AccountingSchemaOrg Where AD_Org_ID=" + GetAD_Org_ID() + " And C_AcctSchema_ID=" + GetC_AcctSchema_ID();
                int count = Util.GetValueOfInt(DB.ExecuteScalar(sql));
                if (count >= 1)
                {
                    log.SaveError("Error", Msg.GetMsg(GetCtx(), "INT15_RecordExistAlready"));
                    return false;
                }
            }
            return true;
        }                
    }
}