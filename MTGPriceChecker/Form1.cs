using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;
using System.IO;

namespace MTGPriceChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string csvfile = Environment.CurrentDirectory + "\\mtgdata.csv";
        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar == '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            //if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            //{
            //    e.Handled = true;
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] addrow = { buyDatePicker.Value.ToString("dd.MM.yyyy"),urlBox.Text, numericUpDown1.Value.ToString(), "" };
            listView1.Items.Add(assetNameBox.Text).SubItems.AddRange(addrow);
            tocsv(listView1, csvfile, true);
        }

        private void parsePriceBtn_Click(object sender, EventArgs e)
        {
            //int i = 0;
            foreach (ListViewItem item in listView1.Items)
            {

                //Console.WriteLine(item.SubItems[2].Text);
                //Get HTML for given Link
                string html = string.Empty;
                string url = item.SubItems[2].Text;
                string lang = "";

                if (url.Contains(".com/de/")){
                    lang = "de";
                }else if (url.Contains(".com/en/")){
                    lang = "en";
                }
                else
                {
                    lang = "unknown";
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "C# console client";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                //Parse HTML for actual Price
                string substr = "";
                if (lang == "de")
                {
                    substr = html.Substring(html.IndexOf(">ab<"), 45);
                }else if (lang == "en")
                {
                    substr = html.Substring(html.IndexOf(">From<"), 45);
                }                
                string lowestPrice = substr.Substring(substr.IndexOf("\">"), 10).Replace(" ","").Replace(".",",");
                
                //Farbe funktioniert noch nicht
                //Double dbllow, dblbuy;
                //Double.TryParse(lowestPrice, out dbllow);
                //Double.TryParse(item.SubItems[3].Text, out dblbuy);
                //item.UseItemStyleForSubItems = false;
                //if (Math.Abs(dbllow) > dblbuy)
                //{
                //    item.SubItems[4].ForeColor = Color.Green;
                //}
                //else
                //{
                //    item.SubItems[4].ForeColor = Color.Red;
                //}

                Regex rgx = new Regex("[^a-zA-Z0-9 ,]");
                item.SubItems[4].Text = rgx.Replace(lowestPrice, "");

                //Console.WriteLine(rgx.Replace(lowestPrice,""));                
                //Console.WriteLine(html);

            }
            tocsv(listView1, csvfile, true);
        }

        public static void tocsv(ListView listView, string filePath, bool includeHidden)
        {
            Console.WriteLine(filePath);
            //make header string
            StringBuilder result = new StringBuilder();
            WriteCSVRow(result, listView.Columns.Count, i => includeHidden || listView.Columns[i].Width > 0, i => listView.Columns[i].Text);

            //export data rows
            foreach (ListViewItem listItem in listView.Items)
                WriteCSVRow(result, listView.Columns.Count, i => includeHidden || listView.Columns[i].Width > 0, i => listItem.SubItems[i].Text);

            if (!File.Exists(filePath))
            {
                File.Create(filePath);
                File.Create(filePath).Close();
            }

            File.WriteAllText(filePath, result.ToString());
        }

        public static void WriteCSVRow(StringBuilder result, int itemsCount, Func<int, bool> isColumnNeeded, Func<int, string> columnValue)
        {
            bool isFirstTime = true;
            for (int i = 0; i < itemsCount; i++)
            {
                if (!isColumnNeeded(i))
                    continue;

                if (!isFirstTime)
                    result.Append(";");
                isFirstTime = false;

                result.Append(String.Format("\"{0}\"", columnValue(i)));
            }
            result.AppendLine();
        }



        private void loadcsv()
        {
            StreamReader sr = new StreamReader(csvfile);
            var lines = File.ReadAllLines(csvfile);
            sr.Close();
            foreach (string line in lines.Skip(1))
            {
                var parts = line.Split(';');               
                ListViewItem lvi = new ListViewItem(parts[0].Replace("\"", ""));
                lvi.SubItems.Add(parts[1].Replace("\"", ""));
                lvi.SubItems.Add(parts[2].Replace("\"", ""));
                lvi.SubItems.Add(parts[3].Replace("\"", ""));
                lvi.SubItems.Add(parts[4].Replace("\"", ""));

                listView1.Items.Add(lvi);
            }
        }

        //private void sortByDate()
        //{
        //    try
        //    {
        //        DateTime dateX = Convert.ToDateTime(listView1.SubItems[1].Text);
        //        DateTime dateY = Convert.ToDateTime(listView1.SubItems[1].Text);
        //        compareResult = ObjectCompare.Compare(dateX, dateY);
        //    }
        //    catch
        //    {
        //        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
        //    }
        //}

        private void Form1_Load(object sender, EventArgs e)
        {
            loadcsv();
            //sortByDate();
        }
    }
}
