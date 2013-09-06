using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MiscUtil.IO;
using MiscUtil.Collections;


namespace Find_Replace
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public struct ReplaseFromTo
        {
            public string from;
            public string to;
        }

        public List<string> IntetrestedIPList = new List<string>();

        public List<string> FilterList = new List<string>();

        public static String ReadLastLine(string path)
        {
            return ReadLastLine(path, Encoding.ASCII, "\n");
        }

        public static String ReadLastLine(string path, Encoding encoding, string newline)
        {
            int charsize = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes(newline);
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                long endpos = stream.Length / charsize;
                for (long pos = charsize * 2; pos < endpos; pos += charsize)
                {
                    stream.Seek(-pos, SeekOrigin.End);
                    stream.Read(buffer, 0, buffer.Length);
                    if (encoding.GetString(buffer) == newline)
                    {
                        buffer = new byte[stream.Length - stream.Position];
                        stream.Read(buffer, 0, buffer.Length);
                        return encoding.GetString(buffer);
                    }
                }
            }
            return null;
        }

        public static string ReturnMounth(int IntMounth)
        {
            switch (IntMounth)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                default:
                    return "";
            }
        }
        
        public static List<ReplaseFromTo> AllDatesToReplase(string [] FirstAndLastast)
        {

            string DatePattern = @"\d{1,2}(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\d{4}";

            try
            {
                Regex Date = new Regex(DatePattern, RegexOptions.Compiled);
                string DateFirst = Date.Match(FirstAndLastast[0]).ToString();
                string DateLast = Date.Match(FirstAndLastast[1]).ToString();
                
                DateTime DateInTrueFormatFirst = Convert.ToDateTime(DateFirst);
                DateTime DateInTrueFormatLast = Convert.ToDateTime(DateLast);
                
                ReplaseFromTo tmp;

                List<ReplaseFromTo> result = new List<ReplaseFromTo>();

                for (; DateInTrueFormatFirst <= DateInTrueFormatLast; DateInTrueFormatFirst = DateInTrueFormatFirst.AddDays(1))
                {
                    tmp.from = "|" + DateInTrueFormatFirst.Day.ToString() + ReturnMounth(DateInTrueFormatFirst.Month) + DateInTrueFormatFirst.Year.ToString() + "|";
                    tmp.to = "|" + DateInTrueFormatFirst.Year.ToString() + "-" + DateInTrueFormatFirst.Month.ToString("00") + "-" + DateInTrueFormatFirst.Day.ToString("00") + " ";
                    result.Add(tmp);
                }
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }
        
        static Int64 CountLinesInFile(string f)
        {
            string LineNumbPattern = @"^\d{1,}";

            try
            {
                Regex LineNumb = new Regex(LineNumbPattern, RegexOptions.Compiled);
                string LineNumbMatch = LineNumb.Match(ReadLastLine(f)).ToString();
                Int64 LineCount = Convert.ToInt64(LineNumbMatch) + 2;
                return LineCount;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return 0;
            }            

        }

        public bool IsGarbage(string line)
        {
            foreach (string pattern in FilterList)
            {
                int counter = 0;
                string[] splitedpattern = pattern.Split(new string[] { " && ", "&&" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splitedpattern.Length; i++ )                
                    if (line.Contains(splitedpattern[i].Trim()))
                        counter++;
                if (counter == splitedpattern.Length)
                    return true; 
            }
            return false;
        }

        public bool IsNeeded(string line)
        {
            foreach (string pattern in IntetrestedIPList)
            {
                int counter = 0;
                string[] splitedpattern = pattern.Split(new string[] { " && ", "&&" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splitedpattern.Length; i++)
                    if (line.Contains(splitedpattern[i].Trim()))
                        counter++;
                if (counter == splitedpattern.Length)
                    return true;
            }
            return false;
        }
        
        public static string [] GetDatesToReplaseInLine(string file)
        {
            string last = ReadLastLine(file).Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            StreamReader reader = new StreamReader(file);
            string first;
            while ((first = reader.ReadLine()).Contains("num|date|time|"))
            {}

            string [] result = new string[2] {first, last};
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo dir = new System.IO.DirectoryInfo(folderBrowserDialog1.SelectedPath);
                FileInfo[] innames = dir.GetFiles("*.txt");
                Int64 current = 0, errorcounter = 0, linecount = 0, i = 0;
                bool progresOK = true;

                foreach (FileInfo infilename in innames)
                    linecount += CountLinesInFile(infilename.FullName);

                progressBar1.Minimum = 0;
                try
                {
                    progressBar1.Maximum = Convert.ToInt32(linecount / 1000);
                }
                catch (Exception ex)
                {
                    progressBar1.Maximum = innames.Length;
                    progresOK = false;
                }

                List<ReplaseFromTo> AllRepl = new List<ReplaseFromTo>();

                System.IO.StreamWriter outfile = new StreamWriter(folderBrowserDialog1.SelectedPath + "\\[Replased_sum_of_files].txt");
                System.IO.StreamReader infile = new System.IO.StreamReader(innames[0].OpenRead());
                string header = infile.ReadLine().Replace("|date|time|", "|date time|");
                outfile.WriteLine(header); 
                infile.Close();
                
                foreach (FileInfo infilename in innames)
                {
                    AllRepl.Clear();
                    AllRepl.AddRange(AllDatesToReplase(GetDatesToReplaseInLine(infilename.FullName)));
                    AllRepl = AllRepl.Distinct().ToList();

                    infile = new System.IO.StreamReader(infilename.OpenRead());
                    
                    infile.ReadLine();                    
                    
                    string line;
                    
                    for (i = 0; (line = infile.ReadLine()) != null; i++)
                    {
                        if (i % 1000 == 0 && progresOK && progressBar1.Value < progressBar1.Maximum)
                        {
                            progressBar1.Increment(1);
                            progressBar1.Update();
                            progressBar1.Refresh();
                            System.Windows.Forms.Application.DoEvents(); 

                            /*progressBar1.Parent.Invoke(new MethodInvoker(delegate { progressBar1.Value = progressBar1.Value; }));
                            progressBar1.Parent.Invoke(new MethodInvoker(delegate
                            {
                                progressBar1.Update();
                                progressBar1.Refresh();
                                System.Windows.Forms.Application.DoEvents(); 
                            }));*/
                            
                        }

                        if (IsGarbage(line) && checkBox1.Checked)
                            continue;

                        if (!IsNeeded(line) && IntetrestedIPList.Count != 0)
                            continue;

                        for (int j = AllRepl.Count - 1; j >= 0; j--)
                        {
                            line = line.Replace(AllRepl[j].from, AllRepl[j].to);
                            if (line.IndexOf(AllRepl[j].to) > 0)
                                break;
                        }

                        line = line.Replace("| |", "||");
                        

                        if (line == "")
                        {
                            errorcounter++;
                            continue;
                        }

                        line = line.Remove(0, line.IndexOf("|"));
                        line = current + line;
                        outfile.WriteLine(line);
                        current++;                        
                    }
                    current++;
                    progressBar1.Increment(1);
                    infile.Close();                    
                }
                outfile.Close();                
                MessageBox.Show("Всего было " + errorcounter.ToString() + " ошибок");                
            }            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.AddExtension = true;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    IntetrestedIPList.AddRange(System.IO.File.ReadAllLines(openFileDialog1.FileName).ToList());
                    IntetrestedIPList = IntetrestedIPList.Distinct().ToList();
                }
                catch (Exception ex)
                {
                    IntetrestedIPList.Clear();
                    MessageBox.Show("Не удалось прочитать интересующие IP\n" + ex);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
                return;
            openFileDialog1.AddExtension = true;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FilterList.AddRange(System.IO.File.ReadAllLines(openFileDialog1.FileName).ToList());
                    FilterList = FilterList.Distinct().ToList();
                }
                catch (Exception ex)
                {
                    FilterList.Clear();
                    MessageBox.Show("Не удалось прочитать фильтры\n" + ex);
                }
            }
        }
    }
}
