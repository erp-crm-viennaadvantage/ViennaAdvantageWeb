using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace ViennaAdvantage.Process
{
    class AssetWindowSticker : SvrProcess
    {
        private String _Asset_ID = "0";
        private int _Asset_ID_int = 0;
        string[] str;
        int[] IDs;


        protected override void Prepare()
        {
            ProcessInfoParameter[] para = GetParameter();
            for (int i = 0; i < para.Length; i++)
            {
                String name = para[i].GetParameterName();
                if (para[i].GetParameter() == null)
                {
                    ;
                }
                else if (name.Equals("A_Asset_ID"))
                {
                    _Asset_ID = (String)para[i].GetParameter();

                    IDs = Array.ConvertAll(_Asset_ID.Split(','), int.Parse);
                }
            }        
        }
        protected override string DoIt()
        {
            StringBuilder _sql = new StringBuilder();
            int count = 0;

            try
            {
                count = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM VAFAM_AssetWindowSticker"));
                if (count > 0)
                {
                    DB.ExecuteQuery("DELETE FROM VAFAM_AssetWindowSticker");
                }
                foreach (int st in IDs)
                {
                    count = 0;
                    _sql.Append(@" INSERT INTO VAFAM_AssetWindowSticker(a_asset_id,vafam_searchkey ,vafam_assetgroup,vafam_productname,vafam_serialno)");
                    _sql.Append(@" SELECT " + GetRecord_ID() + @" AS A_asset_id,
                          ast.VALUE           AS searchkey ,
                          agp.name assetgpname ,
                          mp.name   AS productname ,
                          ast.serno AS serialno
                        FROM A_ASSET ast
                        JOIN a_asset_group agp
                        ON(agp.a_asset_group_id=ast.a_asset_group_id)
                        LEFT OUTER JOIN m_product mp
                        ON(mp.m_product_id=ast.m_product_id)
                       WHERE a_asset_id = " + st);
                    count = DB.ExecuteQuery(_sql.ToString());

                    _sql.Clear();
                }                     
            }
            catch
            {

            }
            return "";

        }
    }
}
