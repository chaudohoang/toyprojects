using System;
using System.IO;


namespace Create_filtered_report
{
   
    public class Program
    {
        
        
        public static void Main(string[] args)
        {
            
            PingReport.PingReportGenerator pingReportGenerator = new PingReport.PingReportGenerator();
            pingReportGenerator.CreateReport("report.html", System.Environment.CurrentDirectory);


            Console.ReadKey();
        }
        
    }
}
