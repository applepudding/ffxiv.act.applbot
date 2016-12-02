using System;
using System.Net;
using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;
using System.IO;
using System.Xml;
using Advanced_Combat_Tracker;

using System.Linq;
using System.Threading; //worker stuff
using System.Speech.Synthesis; //speech stuff
using System.Text.RegularExpressions; //regex
using System.Diagnostics; //stopwatch
using System.Collections.Generic;

namespace ffxiv.act.applbot
{
    public partial class formMain
    {
        string logFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Advanced Combat Tracker\\FFXIVLogs\\";
        string pluginFolderName = "applbot";
        string playerFile = "applbot\\players.ini";
        string saveLogFile = "applbot\\log.txt";
        string xmlFolderPath = "applbot\\";
        string logFileName_active;
        string simulationFile = "";
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        bool quickMode;

        //background worker stuff
        Stopwatch stopWatch = new Stopwatch();
        private static System.Timers.Timer fightTimer;
        public Thread myThread;

        public bool workerRunning = false;
        private int threadSleepLength = 200;
        public int countdown = 0;
        public string delayedMessage_m = "";
        public int delayedMessage_t = 0;

        public int sleep_t = 0;
        public int flow_offset = 0;
        public string flow_last;

        public string errorMsg;

        /// <summary>
        /// later fix pls
        formMini uxForm = new formMini();
        int current_lv_index = 0;
        ListViewItem current_lvi = new ListViewItem();
        string current_phaseChange_trigger = "";
        int current_phaseChange_offset = 0;
        int currentRepeatPhaseLvi = -1;
        //int current_phase_repeat_index = 0;
        //bool current_phase_repeat = false;

        /// </summary>

        int temp_number1 = 0;
        string temp_1 = "";
        string temp_2 = "";
        int currentPhase = 0;
        string currentFight = "";

        //tttvserver myTTTVServer;

        BindingList<ffxiv_player> ffxiv_player_list = new BindingList<ffxiv_player>();

        List<string> ffxiv_jobSkillList = new List<string>();
        string[] ffxiv_jobSortOrder;
        string[] ffxiv_jobList;
        string[] ffxiv_classList;
        bool partySorted = false;

        bool broadcastServer = false;
        int broadcastChannel = 0;


        void botspeak(string inputString)
        {
            if (!InvokeRequired)
            {
                string toSpeak = inputString;
                //handle personalized speak @ and : symbol
                if (inputString.Contains("@"))
                {
                    toSpeak = "";
                    string[] mainElements = Regex.Split(inputString, "@");
                    foreach (string ele in mainElements)
                    {
                        if (ele.Contains(txt_you.Text))
                        {
                            string[] subElements = Regex.Split(ele, ":");
                            toSpeak = subElements[1];
                            continue;
                        }
                    }
                }


                //speak
                if (combo_voiceSelector.SelectedIndex != combo_voiceSelector.Items.Count-1)
                {
                    synthesizer.SpeakAsync(toSpeak);
                }
                else
                {
                    ActGlobals.oFormActMain.PlayTtsMethod(toSpeak);
                }
                    
                broadcast(inputString, "");
            }
            else
            {
                Invoke(new Action<string>(botspeak), inputString);
            }

        }

        void broadcast(string m, string e)
        {
            if (!InvokeRequired)
            {
                if ((broadcastChannel > 0) && broadcastServer && ((m != "") || (e != ""))) 
                { 
                    //repair single quotes '
                    if (m.Contains("'"))
                    {
                        m = m.Replace("'", "''");
                    }
                    string URI = "http://" + this.combo_serverName.Text + "/applbot/api/up.php";
                    string myParameters = "c=" + broadcastChannel.ToString();
                    myParameters += (m != "") ? "&m=" + Uri.UnescapeDataString(m) : "" ;
                    myParameters += (e != "") ? "&e=" + Uri.UnescapeDataString(e) : "" ;
                    log("broadcasting", true, myParameters);
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        string HtmlResult = wc.UploadString(URI, myParameters);
                    }
                }
            }
            else
            {
                Invoke(new Action<string, string>(broadcast), m, e);
            }

        }
        void clearPlayerList()
        {
            if (!InvokeRequired)
            {
                log("clearing player list", true);
                ffxiv_player_list.Clear();
                
                this.grid_players.DataSource = ffxiv_player_list; //this may crash
                partySorted = false;
            }
            else
            {
                Invoke(new Action(clearPlayerList));
            }

        }
        private void loadPlayersFromFile(string idFileName)
        {
            log("loading players", true, idFileName);

            if (File.Exists(idFileName))
            {
                clearPlayerList();
                StreamReader objReader = new StreamReader(idFileName);
                while (objReader.Peek() != -1)
                {
                    string textLine = objReader.ReadLine();
                    switch (textLine)
                    {
                        case "[playerList]":
                            while (objReader.Peek() != -1)
                            {
                                string variable_names = objReader.ReadLine();
                                string variable_nicknames = objReader.ReadLine();
                                string variable_job = objReader.ReadLine();
                                string variable_class = objReader.ReadLine();
                                //log(variable_nicknames+" - "+ variable_nicknames + " - "+ variable_class + " - " + variable_names);
                                ffxiv_player_list.Add(new ffxiv_player() { varName = variable_names, varClass = variable_class, varNickname = variable_nicknames, varJob = variable_job });
                            }
                            break;
                        default:
                            break;
                    }
                }
                objReader.Close();
                //this.grid_players.DataSource = ffxiv_player_list;
            }
        }
        #region init and dispose plugin
        void initEncounterPlugin()
        {
            if (!InvokeRequired)
            {
                this.list_log.Columns.Add("Local Time", -1);
                this.list_log.Columns.Add("StopWatch", -2, HorizontalAlignment.Right);
                this.list_log.Columns.Add("Event", 200);
                this.list_log.Columns.Add("Details", -1);

                logFileName_active = getLatestFile(logFolder);
                log("Log File", false, logFileName_active);
                synthesizer.Volume = this.trackBar_volumeSlider.Value;  // 0...100     

                //init server
                //myTTTVServer = new tttvserver();

                //get installed voice
                foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    combo_voiceSelector.Items.Add(info.Name);
                }
                combo_voiceSelector.SelectedIndex = int.Parse(txt_voiceIndex.Text);
                //add ACT synch speak engine
                combo_voiceSelector.Items.Add("ACT synch mode (Not Reccomended)");

                //set fight timer
                fightTimer = new System.Timers.Timer(1000); // Create a timer with a 1 second interval.
                fightTimer.Elapsed += OnTimedEvent; // Hook up the Elapsed event for the timer. 
                fightTimer.AutoReset = true;
                fightTimer.Enabled = false;

                #region create joblist
                ffxiv_jobList = new string[] { "whm", "ast", "sch", "war", "drk", "pld", "smn", "blm", "mch", "brd", "nin", "drg", "mnk" };
                ffxiv_jobSortOrder = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m" };
                ffxiv_classList = new string[] { "heal", "heal", "heal", "tank", "tank", "tank", "caster", "caster", "range", "range", "melee", "melee", "melee" };
                ffxiv_jobSkillList.Add("#cure#, #cure ii#, #regen#, #stone iii#, #medica ii#"); //whm
                ffxiv_jobSkillList.Add("#helios#, #benefic ii#, #aspected benefic#, #combust ii#, #essential dignity#"); //ast
                ffxiv_jobSkillList.Add("#physick#, #adloquium#, #succor#, #lustrate#, #indomitability#, #broil#, #sacred soil#"); //sch
                ffxiv_jobSkillList.Add("#heavy swing#, #maim#, #skull Sunder#, #berserk#, #tomahawk#, #deliverance#"); //war
                ffxiv_jobSkillList.Add("#hard slash#, #unmend#, #plunge#"); //drk
                ffxiv_jobSkillList.Add("#fast blade#, #shield lob#"); //pld
                ffxiv_jobSkillList.Add("#fester#, #painflare#, #tri-disaster#, #deathflare#, #dreadwyrm trance#"); //smn
                ffxiv_jobSkillList.Add("#fire ii#, #fire iii#"); //blm
                ffxiv_jobSkillList.Add("#split shot#"); //mch
                ffxiv_jobSkillList.Add("#heavy shot#, #windbite#, #straight shot#, #venomous bite#"); //brd
                ffxiv_jobSkillList.Add("#spinning edge#"); //nin
                ffxiv_jobSkillList.Add("#heavy thrust#, #true thrust#"); //drg
                ffxiv_jobSkillList.Add("#bootshine#, #true strike#, #snap punch#, #twin snakes#"); //mnk
                #endregion

                #region get xml files list
                string[] files = Directory.EnumerateFiles(pluginFolderName, "*.xml", SearchOption.AllDirectories).Select(Path.GetFileName).ToArray();//   System.IO.Directory.GetFiles("ffxiv.encounter\\xml");
                this.combo_xml_fightFile.Items.AddRange(files);
                #endregion

                #region Auto Update Stuff
                //log(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString());
                #endregion
                this.grid_players.DataSource = ffxiv_player_list;
            }
            else
            {
                Invoke(new Action(initEncounterPlugin));
            }
        }
        
        void requestChannel()
        {
            try
            {
                string URI = "http://" + this.combo_serverName.Text + "/applbot/api/up.php";
                string myParameters = "r=1";
                log("requesting channel", true);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string HtmlResult = wc.UploadString(URI, myParameters);
                    log("Connected", true, HtmlResult);
                    this.btn_openServer.Text = "Disconnect";
                    this.btn_openServer.Enabled = true;
                    this.combo_serverName.Enabled = false;
                    this.lbl_broadcastChannel.Text = HtmlResult;
                    this.txt_broadcastURL.Text = "http://" + this.combo_serverName.Text + "/applbot/" + HtmlResult;
                    broadcastChannel = Int32.Parse(HtmlResult);
                    broadcastServer = true;
                }
            }
            catch (Exception e)
            {
                log("error", true, e.Message);
                this.btn_openServer.Text = "Connect";
                this.btn_openServer.Enabled = true;
                this.combo_serverName.Enabled = true;
                this.txt_broadcastURL.Text = "";
                this.lbl_broadcastChannel.Text = "-";
            }
        }
        /*
        private bool checkUpdate(string webServer)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webServer + "up.php");
                request.Timeout = 10000;
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();
                if (appVersion != responseFromServer)
                {

                    log("New Update Available, Restating!");

                    DownloadFile("http://xaxaxaxaxaxa/t/tminiupdater.exe", getFolder() + "tminiupdater.exe");

                    ProcessStartInfo processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = getFolder() + "tminiupdater.exe";
                    Process.Start(processStartInfo);
                    Application.Exit();
                }
                return true;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return false;
            }
        }
        
        public static void DownloadFile(String url, String destination)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000; // 10 seconds            
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var fileStream = File.Open(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var MaxBytesToRead = 10 * 1024;
                        var buffer = new Byte[MaxBytesToRead];
                        var totalBytesRead = 0;
                        var bytesRead = responseStream.Read(buffer, 0, MaxBytesToRead);

                        while (bytesRead > 0)
                        {
                            totalBytesRead += bytesRead;
                            fileStream.Write(buffer, 0, bytesRead);
                            bytesRead = responseStream.Read(buffer, 0, MaxBytesToRead);
                        }
                    }
                }
            }
        }
        */
        void disposeEncounterPlugin()
        {
            uxForm.Close();
            if (workerRunning)
            {
                myThread.Abort();
            }
            if (fightTimer != null)
            {
                fightTimer.Stop();
                fightTimer.Dispose();
                fightTimer = null;
            }
        } //dispose stuff
        #endregion

        public void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (!InvokeRequired)
            {
                decreaseTimerEvent();

                // Format and display the TimeSpan value.
                string textElapsedTime = String.Format("{0:00}:{1:00}", stopWatch.Elapsed.Hours * 60 + stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds);
                this.lbl_fightDuration.Text = textElapsedTime;
                //countdown handler
                if (countdown > 0)
                {
                    if (countdown < Int32.Parse(txt_a6s_swapCountdown.Text) + 1)
                    {
                        log("---countdown: " + countdown.ToString());
                        string toSpeak = string.Format("{0}", countdown);
                        botspeak(toSpeak);
                    }
                    countdown--;
                }
                //delayed message handler
                if (delayedMessage_t > 0)
                {
                    delayedMessage_t--;
                    if (delayedMessage_t == 0)
                    {
                        botspeak(delayedMessage_m);
                        log("---delayed Message: " + delayedMessage_m);
                        delayedMessage_m = "";
                    }
                }
                if (sleep_t > 0)
                {
                    sleep_t--;
                }
                if (flow_offset > 0)
                {
                    flow_offset--;
                }
                if (a12s_temporalStasis) // stop temporal stasis after x seconds
                {
                    if(a12s_ts_countdown > 0)
                    {
                        a12s_ts_countdown--;
                    }
                    else
                    {
                        log("Stop resolving TS", true);
                        a12s_ts_id++;
                        a12s_temporalStasis = false;
                        a12s_ts_count = 0;
                        a12s_cleanPlayerListDebuff();
                    }
                }
            }
            else
            {
                Invoke(new Action<Object, System.Timers.ElapsedEventArgs>(OnTimedEvent), source, e);
            }
        }

        void log(string value, bool textColor = false, string details = "")
        {
            if (!InvokeRequired)
            {
                //string result = DateTime.Now + ": " + value;
                ListViewItem lvi = new ListViewItem();
                ListViewItem.ListViewSubItem lvsi1 = new ListViewItem.ListViewSubItem();
                ListViewItem.ListViewSubItem lvsi2 = new ListViewItem.ListViewSubItem();

                lvi.Text = DateTime.Now.ToString();

                #region add stopwatch elapsed time if > 0s
                TimeSpan ts = stopWatch.Elapsed;
                if (ts.TotalSeconds > 0)
                {
                    string elapsedTime = String.Format("{0:00}:{1:00}", (ts.Hours * 60) + ts.Minutes, ts.Seconds);
                    lvsi1.Text = elapsedTime;
                }
                else
                {
                    lvsi1.Text = "";
                }
                lvi.SubItems.Add(lvsi1);
                #endregion

                #region add the text value
                lvsi2.Text = value;
                if (textColor)
                {
                    lvsi2.ForeColor = Color.DarkOrange;
                }
                lvi.SubItems.Add(lvsi2);
                #endregion

                #region add the details if exist
                lvi.SubItems.Add(details);
                #endregion

                lvi.UseItemStyleForSubItems = false;


                list_log.Items.Insert(0, lvi);
                while (list_log.Items.Count > 500)
                {
                    list_log.Items.RemoveAt(list_log.Items.Count - 1);
                }

                if (this.chk_saveLog.Checked)
                {
                    StreamWriter objWriter = new StreamWriter(saveLogFile, true);
                    objWriter.WriteLine(lvi.Text + " " + lvsi1.Text + " " + lvsi2.Text + "-----" + details);
                    objWriter.Close();
                }
            }
            else
            {
                Invoke(new Action<string, bool, string>(log), value, textColor, details);
            }
        }

        public string getNickname(string playerName)
        {
            string nickname = "";
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if (player.varName == playerName)
                {
                    nickname = player.varNickname;
                    break;
                }
            }
            if (string.IsNullOrEmpty(nickname)) //cant find player in player list, so use first name
            {
                string[] subname = Regex.Split(playerName, " ");
                nickname = subname[0];
            }
            return nickname;
        }
        
        string getLatestFile(string folderName)
        {
            var directory = new DirectoryInfo(folderName);
            var myFile = (from f in directory.GetFiles()
                          orderby f.LastWriteTime descending
                          select f).First();

            return myFile.FullName;
        }

        void startNewFight()
        {
            if (!InvokeRequired)
            {
                this.list_fight.Clear();
                if (this.combo_xml_fightFile.Text != "")
                {
                    string xmlFilePath = xmlFolderPath + this.combo_xml_fightFile.Text;
                    log("Loading XML", true, xmlFilePath);
                    loadFightXml(xmlFilePath);
                    log("XML Loaded", true, this.list_fight.Items.Count.ToString());

                    this.list_fight.Columns.Add("Phase #");
                    this.list_fight.Columns.Add("Details", 200);
                    this.list_fight.Columns.Add("Timing");
                    this.list_fight.Columns.Add("Speak");
                    this.list_fight.Columns.Add("Trigger", 200);
                    this.list_fight.Columns.Add("--", -1);
                }
                current_lv_index = 0;
                current_lvi = new ListViewItem();
                current_phaseChange_trigger = "";
                current_phaseChange_offset = 0;

                a12s_ts_id = 0;
                a12s_temporalStasis = false;
                a12s_preyTarget = "";
                a12s_preyCount = 0;
                a12s_halfGravityCount = 0;
                currentRepeatPhaseLvi = -1;

                countdown = 0;
                temp_number1 = 0;

                currentPhase = 1;
                stopWatch.Reset();
                stopWatch.Start();
                temp_1 = "";
                temp_2 = "";
                fightTimer.Enabled = true;
                log("--- Fight Started", true, currentFight);                
            }
            else
            {
                Invoke(new Action(startNewFight));
            }
        }
        void stopFight()
        {
            if (!InvokeRequired)
            {
                this.group_currentFight.Text = "";
                string toSpeak = string.Format("{0} Stopped", currentFight);
                log("---" + toSpeak, true);

                countdown = 0;
                currentFight = "";
                delayedMessage_t = 0;
                delayedMessage_m = "";
                stopWatch.Stop();
                stopWatch.Reset();
                fightTimer.Enabled = false;
            }
            else
            {
                Invoke(new Action(stopFight));
            }
        }


        public void updateCurrent_lvi(int inputIndex)
        {
            if (!InvokeRequired)
            {
                if (this.list_fight.Items.Count + 1 > inputIndex)
                {
                    if (this.list_fight.Items[inputIndex] != current_lvi)
                    {
                        current_lvi = this.list_fight.Items[inputIndex];
                        string temp_currentEvent = this.list_fight.Items[inputIndex].SubItems[1].Text;
                        string temp_nextEvent = "";
                        for (int i = 1; (inputIndex + i < this.list_fight.Items.Count) && (i < 3); i++)
                        {
                            temp_nextEvent += (temp_nextEvent != "") ? ", " : "";
                            if (this.list_fight.Items[inputIndex + i].SubItems[0].Text != "")
                            {
                                temp_nextEvent += this.list_fight.Items[inputIndex + i].SubItems[0].Text;
                            }
                            else
                            {
                                temp_nextEvent += this.list_fight.Items[inputIndex + i].SubItems[1].Text + " (" + this.list_fight.Items[inputIndex + i].SubItems[2].Tag.ToString() + ")";
                            }
                        }
                        miniUX_update(temp_currentEvent, temp_nextEvent, this.list_fight.Items[inputIndex].SubItems[2].Text );
                    }
                }
                else
                {
                    current_lvi = new ListViewItem();
                }
            }
            else
            {
                Invoke(new Action<int>(updateCurrent_lvi), inputIndex);
            }
        }
        public void next_lvi()
        {
            if (!InvokeRequired)
            {

                if (this.list_fight.Items.Count > current_lv_index + 1)
                {
                    current_lv_index++;
                    updateCurrent_lvi(current_lv_index);

                    if (list_fight.Items[current_lv_index].SubItems[0].Text == "")
                    {
                        list_fight.Items[current_lv_index].SubItems[2].Text = list_fight.Items[current_lv_index].SubItems[2].Tag.ToString();
                    }
                    else
                    {
                        if (currentRepeatPhaseLvi >= 0)
                        {
                            current_lv_index = currentRepeatPhaseLvi;
                            updateCurrent_lvi(current_lv_index);
                        }
                    }

                }
                else
                {
                    if (currentRepeatPhaseLvi >= 0)
                    {
                        current_lv_index = currentRepeatPhaseLvi;
                        updateCurrent_lvi(current_lv_index);
                    }
                }
            }
            else
            {
                Invoke(new Action(next_lvi));
            }
        }
        public void nextPhase()
        {
            if (!InvokeRequired)
            {
                for (int i = current_lv_index; i < list_fight.Items.Count - 1; i++)
                {
                    if (list_fight.Items[i].Text != "") //if line is phase indicator
                    {

                        current_lv_index = i;
                        updateCurrent_lvi(current_lv_index);
                        break;
                    }
                    else
                    {
                        log("skipping " + i);
                    }
                }
            }
            else
            {
                Invoke(new Action(nextPhase));
            }
        }

        public void highlightEvent()
        {
            if (!InvokeRequired)
            {
                this.list_fight.Items[current_lv_index].Selected = true;
                //this.list_fight.Items[current_lv_index].BackColor = Color.Green;
                this.list_fight.Items[current_lv_index].EnsureVisible();


            }
            else
            {
                Invoke(new Action(highlightEvent));
            }
        }
        public void completeEvent()
        {
            if (!InvokeRequired)
            {
                if (this.list_fight.Items[current_lv_index].Text == "")
                {
                    this.list_fight.Items[current_lv_index].SubItems[2].Text = "\u2713";
                }
            }
            else
            {
                Invoke(new Action(completeEvent));
            }
        }
        public void decreaseTimerEvent()
        {
            if (!InvokeRequired)
            {
                if (this.list_fight.Items[current_lv_index].Text == "")
                {
                    if (Int32.Parse(this.list_fight.Items[current_lv_index].SubItems[2].Text) > 0)
                    {
                        int x = Int32.Parse(this.list_fight.Items[current_lv_index].SubItems[2].Text) - 1;
                        this.list_fight.Items[current_lv_index].SubItems[2].Text = x.ToString();
                        uxForm.uxlbl_eventCountdown.Text = x.ToString(); // this will update the mini ux countdown timer
                    }
                }
            }
            else
            {
                Invoke(new Action(decreaseTimerEvent));
            }
        }
        public void miniUX_update(string ux_currentEvent, string ux_nextEvent, string timing)
        {
            if (!InvokeRequired)
            {
                uxForm.uxlbl_eventCurrent.Text = ux_currentEvent;
                uxForm.uxlbl_eventNext.Text = ux_nextEvent;
                broadcast("", ux_currentEvent + ":" + timing + "@" + ux_nextEvent);
            }
            else
            {
                Invoke(new Action<string, string, string>(miniUX_update), ux_currentEvent, ux_nextEvent, timing);
            }
        }
        public void startCountdown(int c)
        {
            if (!InvokeRequired)
            {
                countdown = c;
            }
            else
            {
                Invoke(new Action<int>(startCountdown), c);
            }
        }

        public void loadFightXml(string xmlFilePath)
        {
            if (!InvokeRequired)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlFilePath);

                    // temporary replace later that support multiple bosses in 1 area (ex: a6s blaster+brawler+swindler+vortexer)
                    XmlNode xml_encounterNode = doc.DocumentElement.SelectSingleNode("/encounter");
                    //if (quickMode)
                    {
                        currentFight = (xml_encounterNode.Attributes["name"] == null) ? "" : xml_encounterNode.Attributes["name"].Value;
                    }
                    /////////////////////////////////////////////////////////////

                    XmlNodeList xml_phaseNodes = doc.DocumentElement.SelectNodes("/encounter/encounter-phase");

                    int lineCount = 0;////

                    foreach (XmlNode phaseNode in xml_phaseNodes)
                    {
                        string phaseName = phaseNode.Attributes["id"].Value;  //.SelectSingleNode("encounter-phase-name").InnerText;
                        string phaseDetail = (phaseNode.Attributes["detail"] == null) ? "" : phaseNode.Attributes["detail"].Value; //.SelectSingleNode("encounter-phase-detail").InnerText;
                        string phaseRepeat = (phaseNode.Attributes["repeat"] == null) ? "" : "\u27F3";  //(phaseNode.SelectSingleNode("encounter-phase-repeat").InnerText == "true") ? "\u27F3" : "";
                        string phaseChangeTrigger = (phaseNode.Attributes["skip-trigger"] == null) ? "" : phaseNode.Attributes["skip-trigger"].Value; //.SelectSingleNode("encounter-phase-changeTrigger").InnerText;
                        string phaseChangeOffset = (phaseNode.Attributes["skip-offset"] == null) ? "0" : phaseNode.Attributes["skip-offset"].Value;//phaseNode.SelectSingleNode("encounter-phase-changeTriggerOffset").InnerText;
                        string phaseChangeSpeak = (phaseNode.Attributes["speak"] == null) ? "" : phaseNode.Attributes["speak"].Value;//phaseNode.SelectSingleNode("encounter-phase-changeSpeak").InnerText;

                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = phaseName;
                        lvi.ForeColor = Color.Gray;
                        lvi.SubItems.Add(phaseDetail);

                        ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
                        lvsi.Text = phaseRepeat;
                        
                        lvi.SubItems.Add(lvsi);

                        lvsi = new ListViewItem.ListViewSubItem();
                        lvsi.Text = phaseChangeSpeak;
                        lvsi.ForeColor = Color.Gray;
                        lvi.SubItems.Add(lvsi);

                        lvsi = new ListViewItem.ListViewSubItem();
                        lvsi.Text = phaseChangeTrigger;
                        lvsi.Tag = phaseChangeOffset;
                        lvsi.ForeColor = Color.Gray;
                        lvi.SubItems.Add(lvsi);

                        lvi.SubItems.Add(lineCount.ToString());////
                        lineCount++;

                        lvi.UseItemStyleForSubItems = false;
                        this.list_fight.Items.Add(lvi);

                        XmlNodeList xml_eventNodes = phaseNode.SelectSingleNode("encounter-phase-eventFlow").SelectNodes("encounter-event");

                        foreach (XmlNode eventNode in xml_eventNodes)
                        {
                            string eventDetail = (eventNode.Attributes["detail"] == null) ? "" : eventNode.Attributes["detail"].Value; //eventNode.SelectSingleNode("encounter-event-detail").InnerText;
                            string eventCountdown = eventNode.Attributes["countdown"].Value; //eventNode.SelectSingleNode("encounter-event-countdown").InnerText;
                            string eventTrigger = (eventNode.Attributes["trigger"] == null) ? "" : eventNode.Attributes["trigger"].Value; //eventNode.SelectSingleNode("encounter-event-trigger").InnerText;
                            string eventSpeak = (eventNode.Attributes["speak"] == null) ? "" : eventNode.Attributes["speak"].Value;//eventNode.SelectSingleNode("encounter-event-speak").InnerText;
                            string eventOffset = (eventNode.Attributes["offset"] == null) ? "0" : eventNode.Attributes["offset"].Value;

                            lvi = new ListViewItem();
                            lvi.Text = "";
                            lvi.ForeColor = Color.Gray;
                            lvi.SubItems.Add(eventDetail);

                            lvsi = new ListViewItem.ListViewSubItem();
                            lvsi.Text = eventCountdown;
                            lvsi.Tag = eventCountdown;
                            lvi.SubItems.Add(lvsi);

                            lvsi = new ListViewItem.ListViewSubItem();
                            lvsi.Text = eventSpeak;
                            lvi.SubItems.Add(lvsi);

                            lvsi = new ListViewItem.ListViewSubItem();
                            lvsi.Text = eventTrigger;
                            lvsi.Tag = eventOffset;
                            lvi.SubItems.Add(lvsi);

                            lvi.SubItems.Add(lineCount.ToString());////
                            lineCount++;

                            lvi.UseItemStyleForSubItems = true;
                            this.list_fight.Items.Add(lvi);
                        }
                    }
                }
                catch (Exception e)
                {
                    log("error", true, e.Message);
                }                
            }
            else
            {
                Invoke(new Action<string>(loadFightXml), xmlFilePath);
            }
        }
    }
    public class ffxiv_player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged()
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(""));
        }
        private string _varClass;
        private string _varName;
        private string _varNickname;
        private string _varJob;
        private string _varOrder;
        private string _varDebuff;
        private string _varPosition;
        private int _varInt;
        private int _varOI;

        [Browsable(false)]
        public string varOrder
        {
            get { return _varOrder; }
            set { _varOrder = value; }
        }
        //[Browsable(false)]
        public int varOI
        {
            get { return _varOI; }
            set { _varOI = value; }
        }
        //[Browsable(false)]
        public string varPosition
        {
            get { return _varPosition; }
            set { _varPosition = value; }
        }
        //[Browsable(false)]
        public string varDebuff
        {
            get { return _varDebuff; }
            set { _varDebuff = value; }
        }
        //[Browsable(false)]
        public int varInt
        {
            get { return _varInt; }
            set { _varInt = value; }
        }
        public string varClass
        {
            get { return _varClass; }
            set { _varClass = value; OnPropertyChanged(); }
        }
        public string varJob
        {
            get { return _varJob; }
            set { _varJob = value; OnPropertyChanged(); }
        }
        public string varName
        {
            get { return _varName; }
            set { _varName = value; OnPropertyChanged(); }
        }
        public string varNickname
        {
            get { return _varNickname; }
            set { _varNickname = value; OnPropertyChanged(); }
        }
    }
    /*
    public class tttvserver : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged()
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(""));
        }
        private string _varChannel;
        private string _varLatency;
        private string _varAPIAuth;
        private string _varMsg;

        public string varChannel
        {
            get { return _varChannel; }
            set { _varChannel = value; OnPropertyChanged(); }
        }
        public string varLatency
        {
            get { return _varLatency; }
            set { _varLatency = value; OnPropertyChanged(); }
        }
        public string varAPIAuth
        {
            get { return _varAPIAuth; }
            set { _varAPIAuth = value; OnPropertyChanged(); }
        }
        public string varMsg
        {
            get { return _varMsg; }
            set { _varMsg = value; OnPropertyChanged(); }
        }
    }*/
}
