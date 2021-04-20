using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Threading;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;

namespace packingNotification
{

    class Program
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        static void Main(string[] args)
        {
            Console.Title = "Packing Notification";
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
            TimerCallback tmCallback = CheckEffectExpiry;
            System.Threading.Timer timer = new System.Threading.Timer(tmCallback, "test", 10000, 10000);
            Console.WriteLine("Packing Notifications...");
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
        }
        static void CheckEffectExpiry(object objectInfo)
        {
            Console.Title = "Packing Notification - Last Update: " + DateTime.Now.ToString("HH:mm:ss");
            if (staticVariables.counter == 10)
            {
                staticVariables.counter = 0;
                staticVariables.messages.Clear();
            }
            //whenever the timer procs the code here runs 
            //MessageBox.Show("test");
            using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
            {
                conn.Open();
                //here we check the packing log table for any completions or pauses of jobs and then produce a messagebox for it
                string sql = "SELECT top 1 * FROM dbo.packing_notification_log WHERE CAST(date_logged as date) = CAST(GETDATE() as date) AND (message_sent = 0 or message_sent = 1 or  message_sent is null) order by id asc";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        //show the messagebox here 
                        string message = "UPDATE: " + dt.Rows[0][3].ToString() + " has marked door #" + dt.Rows[0][1].ToString() + " as " + dt.Rows[0][4].ToString() + " at " + Convert.ToDateTime(dt.Rows[0][5]).ToString("HH:mm:ss");                      
                        if (staticVariables.messages.Contains(message))
                        {
                            sql = "UPDATE dbo.packing_notification_log SET message_sent = 2 WHERE id = " + dt.Rows[0][0].ToString();
                            using (SqlCommand cmdUpdate = new SqlCommand(sql, conn))
                                cmdUpdate.ExecuteNonQuery();
                            return;
                        }
                        Console.WriteLine(message);
                        staticVariables.messages.Add(message);
                        //update the message_sent while are here
                        if (dt.Rows[0][6].ToString() == null || string.IsNullOrWhiteSpace(dt.Rows[0][6].ToString())) //if its null then its the first time ANY of the active apps are reading it, mark as 1 so the other app can read it too!
                            sql = "UPDATE dbo.packing_notification_log SET message_sent = 1 WHERE id = " + dt.Rows[0][0].ToString();
                        else if (dt.Rows[0][6].ToString() == "1")
                            sql = "UPDATE dbo.packing_notification_log SET message_sent = 2 WHERE id = " + dt.Rows[0][0].ToString(); //if its 1 then the second app is reading this and  we mark it as 2 and it stops showing up all together

                        using (SqlCommand cmdUpdate = new SqlCommand(sql, conn))
                            cmdUpdate.ExecuteNonQuery();
                        MessageBox.Show(message, "Packing Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        //^^^^ MessageBoxOptions.DefaultDesktopOnly gives the messagebox the highest prio from what i could find online. brief testing seems to confirm this
                    }
                }
                conn.Close();
            }
        }
    }
}
