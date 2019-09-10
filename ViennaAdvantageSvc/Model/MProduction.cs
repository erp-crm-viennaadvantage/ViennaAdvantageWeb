using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.Utility;

namespace ViennaAdvantage.Model
{
    class MProduction : X_M_Production
    {
        public MProduction(Ctx ctx, int M_Production_ID, Trx trxName)
            : base(ctx, M_Production_ID, trxName)
        {
            /** if (M_Production_ID == 0)
            {
            SetIsCreated (false);
            SetM_Production_ID (0);
            SetMovementDate (DateTime.Now);	// @#Date@
            SetName (null);
            SetPosted (false);
            SetProcessed (false);	// N
            }
             */
        }

        /** Load Constructor 
        @param ctx context
        @param rs result set 
        @param trxName transaction
        */
        public MProduction(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {
        }

        /// <summary>
        /// Before Save
        /// </summary>
        /// <param name="newRecord">new</param>
        /// <returns>true or false</returns>
        protected override bool BeforeSave(bool newRecord)
        {
            if (!CheckProductionExist(true))
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithFutureDate"));
                return false;
            }

            if (!CheckProductionExist(false))
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "GOM01_RecordExistWithBackDate"));
                return false;
            }
            return true;
        }

        private bool CheckProductionExist(bool futureDate)
        {
            string Sql = "Select Count(M_Production_ID) From M_Production  Where AD_Client_ID=" + GetAD_Client_ID() + " AND AD_Org_ID=" + GetAD_Org_ID() + " ";
            if (futureDate)
            {
                Sql += " AND MovementDate > " + GlobalVariable.TO_DATE(GetMovementDate(), true);
            }
            else
            {
                Sql += " AND MovementDate < " + GlobalVariable.TO_DATE(GetMovementDate(), true);
            }
            int Count = Util.GetValueOfInt(DB.ExecuteScalar(Sql, null, Get_TrxName()));
            if (Count > 0)
            {
                return false;
            }
            return true;
        }
    }
}
