using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace ViennaAdvantage.Process
{
    class INT15_UpdateRFQResponse : SvrProcess
    {
        string filename = "";
        string _message = "";
        DataSet dsExcel, ds = null;

        protected override string DoIt()
        {
            string extension = filename;
            string path = HostingEnvironment.ApplicationPhysicalPath;
            if (filename.Contains("_FileCtrl"))
            {
                path = path + "TempDownload//" + filename;
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path);
                    if (files != null && files.Length > 0)
                    {
                        filename = "//" + Path.GetFileName(files[0]);
                    }
                }
                else
                {
                    return Msg.GetMsg(GetCtx(), "PathNotExist");

                }
            }

            int ind = filename.LastIndexOf(".");
            extension = filename.Substring(ind, filename.Length - ind);

            if (extension.ToUpper() == ".XLSX" || extension.ToUpper() == ".CSV")
            {
                try
                {
                    // Reading excel into dataset
                     dsExcel = ImportExcelXLS(path + filename, false);
                    if (dsExcel != null && dsExcel.Tables[0].Rows.Count > 0)
                    {
                        string sql = @"SELECT rsl.c_rfqresponseline_id,  rsqty.C_RfQResponseLineQty_ID,  CASE WHEN rfl.int11_productcode IS NOT NULL
                                    THEN rfl.int11_productcode ELSE pro.value END AS productCode FROM C_RfQResponseLine rsl INNER JOIN 
                                    C_RfQResponseLineQty rsqty ON (rsqty.c_rfqresponseline_id = rsl.c_rfqresponseline_id) INNER JOIN C_RfQLine rfl
                                    ON (rsl.C_RfQLine_ID =rfl.C_RfQLine_ID) INNER JOIN M_product pro ON (rfl.M_product_ID       =pro.m_product_id)
                                    WHERE rsl.C_RfQResponse_ID =" + GetRecord_ID() + "";
                        // Get response lines for checking product code.
                        ds = DB.ExecuteDataset(sql);
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                DataRow[] dr = ds.Tables[0].Select(" productCode='" + dsExcel.Tables[0].Rows[i]["Product Code"] + "'");
                                if (dr.Length > 0)
                                {
                                    if (Util.GetValueOfInt(dr[0]["C_RfQResponseLineQty_ID"]) > 0)
                                    {
                                        MRfQResponseLineQty ResLineQty = new MRfQResponseLineQty(GetCtx(), Util.GetValueOfInt(dr[0]["C_RfQResponseLineQty_ID"]), null);
                                        ResLineQty.SetPrice(Util.GetValueOfDecimal(dsExcel.Tables[0].Rows[i]["Price"]));
                                        if (ResLineQty.Save())
                                        {

                                        }
                                    }
                                }
                            }
                            _message = Msg.GetMsg(GetCtx(), "INT15_ResponseUpdated");
                        }
                        else
                        {

                            _message = Msg.GetMsg(GetCtx(), "INT15_NoRespLine");
                        }
                    }
                    else
                    {
                        _message = Msg.GetMsg(GetCtx(), "ExcelSheetNotInProperFormat");
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }

            }

            return _message;
        }



        public DataSet ImportExcelXLS(string FileName, bool hasHeaders)
        {

            string HDR = hasHeaders ? "Yes" : "No";
            string strConn;
            var connString = "";

            if (FileName.Substring(FileName.LastIndexOf('.')).ToLower() == ".xlsx")
            {
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            }
            else
            {
                connString = string.Format(
    @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
    Path.GetDirectoryName(FileName));
                strConn = "";
            }

            DataSet output = new DataSet();

            try
            {
                if (FileName.Substring(FileName.LastIndexOf('.')).ToLower() == ".xlsx")
                {
                    using (OleDbConnection conn = new OleDbConnection(strConn))
                    {
                        conn.Open();

                        DataTable schemaTable = conn.GetOleDbSchemaTable(
                            OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                        foreach (DataRow schemaRow in schemaTable.Rows)
                        {
                            string sheet = schemaRow["TABLE_NAME"].ToString();

                            if (!sheet.EndsWith("_"))
                            {
                                try
                                {
                                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                                    cmd.CommandType = CommandType.Text;

                                    DataTable outputTable = new DataTable(sheet);
                                    output.Tables.Add(outputTable);
                                    new OleDbDataAdapter(cmd).Fill(outputTable);
                                }
                                catch
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (OleDbConnection conn = new OleDbConnection(connString))
                    {
                        conn.Open();
                        DataTable schemaTable = conn.GetOleDbSchemaTable(
                            OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                        foreach (DataRow schemaRow in schemaTable.Rows)
                        {
                            string sheet = schemaRow["TABLE_NAME"].ToString();

                            if (!sheet.EndsWith("_"))
                            {
                                try
                                {
                                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                                    cmd.CommandType = CommandType.Text;
                                    DataTable outputTable = new DataTable(sheet);
                                    output.Tables.Add(outputTable);
                                    new OleDbDataAdapter(cmd).Fill(outputTable);
                                }
                                catch
                                {

                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _message = ex.Message;
                return output;
            }
            return output;
        }


        protected override void Prepare()
        {
            ProcessInfoParameter[] para = GetParameter();
            for (int i = 0; i < para.Length; i++)
            {
                string name = para[i].GetParameterName();
                if (para[i].GetParameter() == null)
                {
                    ;
                }
                else if (name.Equals("FileType"))
                {
                    filename = para[i].GetInfo();
                }
                else
                {
                    log.Log(Level.SEVERE, "Unknown Parameter: " + name);
                }
            }
        }
    }
}
