using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Threading;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;

namespace packingNotification
{
    class Program
    {
        static void Main(string[] args)
        {
            TimerCallback tmCallback = CheckEffectExpiry;
            System.Threading.Timer timer = new System.Threading.Timer(tmCallback, "test", 10000,10000);
            Console.WriteLine("Press any key to exit the sample");
            Console.ReadLine();
            Console.ReadLine();
        }
        static void CheckEffectExpiry(object objectInfo)
        {
            //whenever the timer procs the code here runs 
            //MessageBox.Show("test");
            using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
            {
                conn.Open();
                //here we check the packing log table for any completions or pauses of jobs and then produce a messagebox for it
                string sql = "SELECT top 1 * FROM dbo.packing_notification_log WHERE CAST(date_logged as date) = CAST(GETDATE() as date) AND (message_sent = 0 or message_sent is null) order by id asc";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        //update the message_sent while are here
                        sql = "UPDATE dbo.packing_notification_log SET message_sent = -1 WHERE id = " + dt.Rows[0][0].ToString();
                        using (SqlCommand cmdUpdate = new SqlCommand(sql, conn))
                            cmdUpdate.ExecuteNonQuery();
                        //show the messagebox here 
                        string message = "UPDATE: " + dt.Rows[0][3].ToString() + " has " + dt.Rows[0][4].ToString() + " door #" + dt.Rows[0][1].ToString() + " at "+ Convert.ToDateTime(dt.Rows[0][5]).ToString("HH:mm");
                        MessageBox.Show(message, "!!!", MessageBoxButtons.OK);
                    }
                }
                conn.Close();
            }
        }
    }
}
