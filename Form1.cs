using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.IO;

namespace GDLevelGenerator
{
    public partial class Form1 : Form
    {
        string label1basetext = "Status: ";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }

        static string sendPostRequest(string url, string content)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var postData = content;
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Random rnd = new Random();
            label1.Text = label1basetext + "Collecting Levels";
            string getLevels = sendPostRequest("http://www.boomlings.com/database/getGJLevels21.php", "gameVersion=21&binaryVersion=33&gdw=0&type=4&str=&diff=-&len=-&page=0&total=0&uncompleted=0&onlyCompleted=0&featured=1&original=0&twoPlayer=0&coins=0&epic=0&star=1&secret=Wmfd2893gb7");
            string[] getLevelsArray = getLevels.Split('#')[0].Split('|');
            int count = 1;
            int getLevelsCount = getLevelsArray.Length;
            string[] leveldata = new string[1];
            //string[] leveldata = new string[getLevelsCount];
            // foreach (string eachGetLevels in getLevelsArray)
            //{
                string currentLevelID = getLevelsArray[rnd.Next(0,10)].Split(':')[1];
            label1.Text = label1basetext + "Downloading Level ";// + count + "/" + getLevelsCount;
                string downloadLevel = sendPostRequest("http://www.boomlings.com/database/downloadGJLevel22.php", "gameVersion=21&binaryVersion=33&gdw=0&levelID=" + currentLevelID + "&inc=1&extras=0&secret=Wmfd2893gb7");
                string levelString = downloadLevel.Split(':')[7];
            label1.Text = label1basetext + "Decompressing Level ";// + count + "/" + getLevelsCount;
                count--;
                leveldata[count] = sendPostRequest("https://pizzaroot.altervista.org/gzdecode.php", "data=" + levelString);
                count += 2;
            //}
            label1.Text = label1basetext + "Generating Level";
            string generatedLevel = "";
            var firstsettings = new List<string>();
            for (var i = 0; i < leveldata.Length; i++)
            {
                firstsettings.Add(leveldata[i].Split(';')[0]);
            }
            string currentblock = firstsettings[rnd.Next(0, firstsettings.Count)];
            generatedLevel += currentblock;
            int currentblockprop_1 = 0;
            double currentblockX = 0;
            double currentblockY = 0;

            int objects = Convert.ToInt32(textBox1.Text);
            int objectcounts = 0;
            label1.Text = label1basetext + "Generating Level 0/" + objects;
            int cycle50 = 0;
            do
            {
                var nextblock = new List<string>();
                for (int i = 0; i < leveldata.Length; i++)
                {
                    string currentlevel = leveldata[i];
                    string[] blockinlevel = currentlevel.Split(';');
                    for (int j = cycle50; j < blockinlevel.Length - 1; j += 50)
                    {
                        string eachblock = blockinlevel[j];
                        if (!eachblock.Contains("_") && eachblock.Contains(',')) {
                            int eachblockprop_1 = Convert.ToInt32(eachblock.Split(',')[1]);
                            double distanceX = Convert.ToDouble(eachblock.Split(',')[3]) - currentblockX;
                            double distanceY = Convert.ToDouble(eachblock.Split(',')[5]) - currentblockY;

                            if (eachblockprop_1 == currentblockprop_1 && blockinlevel[j + 1] != "")
                            {
                                string besideblock = blockinlevel[j + 1];
                                string[] besideblockarr = besideblock.Split(',');
                                besideblockarr[3] = (Convert.ToDouble(besideblockarr[3]) - distanceX).ToString();
                                besideblockarr[5] = (Convert.ToDouble(besideblockarr[5]) - distanceY).ToString();
                                besideblock = "";
                                foreach (string eachbesideblockprop in besideblockarr)
                                {
                                    besideblock += "," + eachbesideblockprop;
                                }
                                nextblock.Add(besideblock.Substring(1));
                            }
                        } else
                        {
                            if (eachblock == currentblock)
                            {
                                nextblock.Add(blockinlevel[j + 1]);
                            }
                        }
                    }
                }
                if (nextblock.Count > 0)
                {
                    currentblock = nextblock[rnd.Next(0, nextblock.Count)];
                    currentblockprop_1 = Convert.ToInt32(currentblock.Split(',')[1]);
                    currentblockX = Convert.ToDouble(currentblock.Split(',')[3]);
                    currentblockY = Convert.ToDouble(currentblock.Split(',')[5]);
                    generatedLevel += ";" + currentblock;
                } else
                {
                    currentblock = "1,1,2," + currentblockX.ToString() + ",3," + currentblockY.ToString();
                    currentblockprop_1 = Convert.ToInt32(currentblock.Split(',')[1]);
                    currentblockX = Convert.ToDouble(currentblock.Split(',')[3]);
                    currentblockY = Convert.ToDouble(currentblock.Split(',')[5]);
                    generatedLevel += ";" + currentblock;
                }
                objectcounts++;
                cycle50 = (cycle50 + 1) % 50;
                label1.Text = label1basetext + "Generating Level " + objectcounts + "/" + objects;
            } while (objectcounts < objects);

            label1.Text = label1basetext + "Saving Level";
            string LocalCC = File.ReadAllText(Environment.GetEnvironmentVariable("LocalAppData") + @"\GeometryDash\CCLocalLevels.dat");
            using (StreamWriter file = new StreamWriter(Environment.GetEnvironmentVariable("LocalAppData") + @"\GeometryDash\CCLocalLevels.dat"))
            {
                string replacement = "<?xml version=\"1.0\"?><plist version=\"1.0\" gjver=\"2.0\"><dict><k>LLM_01</k><d><k>_isArr</k><t /><k>k_0</k><d><k>kCEK</k><i>4</i><k>k2</k><s>Generated Level " + rnd.Next(0, 1000) + "</s><k>k4</k><s>" + sendPostRequest("http://pizzaroot.altervista.org/gzencode.php", "data=" + generatedLevel) + "</s>";
                var result = new StringBuilder();
                for (int c = 0; c < LocalCC.Length; c++)
                    result.Append((char)(LocalCC[c] ^ 11));
                string decryptedLocalCC = sendPostRequest("http://pizzaroot.altervista.org/gzdecode.php", "data=" + result.ToString());
                string dataToWrite = sendPostRequest("http://pizzaroot.altervista.org/gzencode.php", "data=" + decryptedLocalCC.Replace("<?xml version=\"1.0\"?><plist version=\"1.0\" gjver=\"2.0\"><dict><k>LLM_01</k><d><k>_isArr</k><t /><k>k_0</k><d><k>kCEK</k><i>4</i><k>k2</k><s>replace this</s>", replacement));
                result = new StringBuilder();
                for (int c = 0; c < dataToWrite.Length; c++)
                    result.Append((char)(dataToWrite[c] ^ 11));
                File.Delete(Environment.GetEnvironmentVariable("LocalAppData") + @"\GeometryDash\CCLocalLevels.dat.bak");
                file.Write(result.ToString());
            }
            label1.Text = label1basetext + "Completed";
        }
    }
}
