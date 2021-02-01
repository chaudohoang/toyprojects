using System;
using System.IO;

namespace PingReport
{
    public class PingReportGenerator
    {
        public string ReportFileName { get; set; }
        public string RemoteReportFileName { get; set; }

        public void CreateReport(string rawReportFileName, string saveLocation)
        {

            if (File.Exists(rawReportFileName))
            {

                string checkdate = DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string checktime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string text = File.ReadAllText(rawReportFileName);

                text = text.Replace("<title>Pings List</title>", "<title>ECS PC Ping Report</title>");

                text = text.Replace("<h3>Pings List</h3>", @"<h3>ECS PC Ping Report</h3>
<h3>Time : " + checktime + @"</h3>
<h4><i>Created by Do Hoang Chau (dohoangchau@lgdpartner.com)</i></h4>");

                text = text.Replace("<br><h4>Created by using <a href=\"http://www.nirsoft.net/\" target=\"newwin\">PingInfoView</a></h4>", string.Empty);

                text = text.Replace("<table", "<table id=\"result\"");

                text = text.Replace("</table>", @"</table>
<script src=""tablefilter_all_min.js""></script>            
<script type = ""text/javascript"" language = ""javascript"">
var table12_Props = {

    col_0: ""none"",
    col_1: ""none"",
    col_2: ""select"",
    col_3: ""none"",
    col_4: ""none"",
    display_all_text: ""[ Show all ] "",
    sort_select: true

};
var tf12 = setFilterGrid(""result"", table12_Props);
</script> ");
                string reportFileName = saveLocation + "\\" + checkdate + "_ECS_ping_report.html";
                File.WriteAllText(reportFileName, text);


                this.ReportFileName = reportFileName;

                string remoteReportFileName = "\\\\vhnasscip.lgdisplay.com\\lgdisplay_vh_cd_169$\\00.New IT Team\\10. Daily check list results\\01. Daily Report\\04. ECS" + "\\" + checkdate + "_ECS_ping_report.html";
                this.RemoteReportFileName = remoteReportFileName;

                Console.WriteLine("Report file created : \n" + reportFileName);


            }

            else
            {
                Console.WriteLine(rawReportFileName + " not found !");

            }

        }
    }
}
