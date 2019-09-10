using System.Data;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Utility;

namespace ViennaAdvantage.Model
{
    public class MINT15RevenueService : X_INT15_RevenueService
    {
        //	Static Logger	
        private static VLogger _log = VLogger.GetVLogger(typeof(MINT15RevenueService).FullName);
        public MINT15RevenueService(Ctx ctx, int INT15_RevenueService_ID, Trx trxName) : base(ctx, INT15_RevenueService_ID, trxName)
        {

        }


        public MINT15RevenueService(Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName)
        {

        }

    }
}
