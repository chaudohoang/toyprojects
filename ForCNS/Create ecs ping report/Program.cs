using System;
using System.IO;

namespace Create_ecs_ping_report
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            if (File.Exists("PingInfoView.exe"))
                if (File.Exists("Pinginfoview_ECS_IP.txt"))
                {
                    process.StartInfo.FileName = "PingInfoView.exe";
                    process.StartInfo.Arguments = "/loadfile Pinginfoview_ECS_IP.txt /shtml temp_ECS_ping_report.html";
                    process.Start();
                    process.WaitForExit();

                    
                    PingReport.PingReportGenerator pingReportGenerator= new PingReport.PingReportGenerator();
                    pingReportGenerator.CreateReport("temp_ECS_ping_report.html", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

                    File.Delete("temp_ECS_ping_report.html");
                    
                    try
                    {
                        File.Copy(pingReportGenerator.ReportFileName, pingReportGenerator.RemoteReportFileName,true);
                        Console.WriteLine("Report file copied to : \n" + pingReportGenerator.RemoteReportFileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Couldn't copy to : \n"+ pingReportGenerator.RemoteReportFileName);
                        
                    }



                }
                else { 

                    Console.WriteLine("Pinginfoview_ECS_IP.txt not found !");
                    
                }
            else {

                Console.WriteLine("PingInfoView.exe not found !");
                
            }

            Console.ReadKey();



        }
    }


}
