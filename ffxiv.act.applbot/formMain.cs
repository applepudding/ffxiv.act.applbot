using System;
using System.ComponentModel;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.IO;
using System.Xml;

using System.Linq;
using System.Text.RegularExpressions; //regex
using System.Collections.Generic;


namespace ffxiv.act.applbot
{
    public partial class formMain : UserControl, IActPluginV1
    {
        public formMain()
        {
            InitializeComponent();
        }

        Label lblStatus;    // The status label that appears in ACT's Plugin tab
        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\applbot.config.xml");
        SettingsSerializer xmlSettings;

        #region IActPluginV1 Members
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            lblStatus = pluginStatusText;   // Hand the status label's reference to our local var
            pluginScreenSpace.Controls.Add(this);   // Add this UserControl to the tab ACT provides
            this.Dock = DockStyle.Fill; // Expand the UserControl to fill the tab's client space
            xmlSettings = new SettingsSerializer(this);	// Create a new settings serializer and pass it this instance

            
            
            initEncounterPlugin();
            LoadSettings();
            // Create some sort of parsing event handler.  After the "+=" hit TAB twice and the code will be generated for you.
            ActGlobals.oFormActMain.AfterCombatAction += new CombatActionDelegate(encounter_oFormActMain_AfterCombatAction);
            ActGlobals.oFormActMain.OnCombatStart += new CombatToggleEventDelegate(encounter_oFormActMain_OnCombatStart);
            ActGlobals.oFormActMain.OnCombatEnd += new CombatToggleEventDelegate(encounter_oFormActMain_OnCombatEnd);
            ActGlobals.oFormActMain.LogFileChanged += new LogFileChangedDelegate(encounter_oFormActMain_LogFileChanged);
            
            lblStatus.Text = "Plugin Started";
        }
        public void DeInitPlugin()
        {
            // Unsubscribe from any events you listen to when exiting!
            
            ActGlobals.oFormActMain.AfterCombatAction -= encounter_oFormActMain_AfterCombatAction;
            ActGlobals.oFormActMain.OnCombatStart -= encounter_oFormActMain_OnCombatStart;
            ActGlobals.oFormActMain.OnCombatEnd -= encounter_oFormActMain_OnCombatEnd;
            ActGlobals.oFormActMain.LogFileChanged -= encounter_oFormActMain_LogFileChanged;

            SaveSettings();
            disposeEncounterPlugin();
            
            lblStatus.Text = "Plugin Exited";
        }
        #endregion

        void encounter_oFormActMain_LogFileChanged(bool IsImport, string NewLogFileName)
        {
            log("log file changed event", false, NewLogFileName);
            logFileName_active = NewLogFileName;
        }

        void encounter_oFormActMain_OnCombatStart(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            if (workerRunning)
            {
                if (this.txt_simFile.Text == "")
                {
                    startNewFight();
                }
            }
        }

        void encounter_oFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            if (this.txt_simFile.Text =="")
            {
                log("ACT combat end: " + encounterInfo.encounter);
                stopFight();
            }
        }

        void encounter_oFormActMain_AfterCombatAction(bool isImport, CombatActionEventArgs actionInfo)
        {
            if (workerRunning) // implement in new way later PLZ, too resource intensive
            {
                if (!quickMode)
                {
                    #region add players to list, sort it, and get boss name
                    if (ffxiv_player_list.Count != int.Parse(txt_partySize.Text)) //partySize
                    {
                        Match m;

                        if (isNewPC(actionInfo.attacker))
                        {
                            int jobID = 0;
                            foreach (string actionList in ffxiv_jobSkillList)
                            {
                                string toCheckActionName = "#" + actionInfo.theAttackType.ToLower() + "#";
                                m = Regex.Match(actionList, toCheckActionName);
                                if (m.Success)
                                {
                                    ffxiv_player_list.Add(new ffxiv_player() { varName = actionInfo.attacker, varJob = ffxiv_jobList[jobID], varClass = ffxiv_classList[jobID], varOrder = ffxiv_jobSortOrder[jobID] });
                                    log("adding " + actionInfo.attacker + " to player list", false, actionInfo.combatAction.ToString());
                                    break;
                                }
                                jobID++;
                            }
                        }

                    }
                    else
                    {
                        if (!partySorted)
                        {
                            log("fixing player names");
                            foreach (ffxiv_player p in ffxiv_player_list)
                            {
                                if ((p.varName == "YOU") && (this.txt_you.Text != ""))
                                {
                                    p.varName = this.txt_you.Text;
                                }
                            }
                            log("sorting party");
                            List<ffxiv_player> SortedList = ffxiv_player_list.ToList<ffxiv_player>().OrderBy(o => o.varOrder).ToList();
                            ffxiv_player_list = new BindingList<ffxiv_player>(SortedList);
                            partySorted = true;
                            grid_players.Refresh();
                            //this.grid_players.DataSource = ffxiv_player_list;
                        }

                        if (currentFight == "")
                        {
                            if ((isNewPC(actionInfo.victim)) && (actionInfo.victim != "YOU"))
                            {
                                currentFight = actionInfo.victim;

                                //------------------------------ temp
                                //a6s bandaid fix
                                if ((currentFight == "Vortexer") || (currentFight == "Brawler") || (currentFight == "Blaster") || (currentFight == "Swindler"))
                                {
                                    currentFight = "Machinery Bay 70";
                                }
                                this.group_currentFight.Text = currentFight;
                                log("--- Fight Name: " + currentFight, true);
                            }
                        }
                    }
                    #endregion
                }
            }
            //throw new NotImplementedException();
        }

        bool isNewPC(string pcName)
        {
            foreach (ffxiv_player p in ffxiv_player_list)
            {
                if (p.varName == pcName)
                {
                    return false;
                }
            }
            return true;
        }

        #region SETTINGS (save and load)
        void LoadSettings()
        {
            xmlSettings.AddControlSetting(txt_simFile.Name, txt_simFile);
            xmlSettings.AddControlSetting(txt_toSpeak.Name, txt_toSpeak);
            xmlSettings.AddControlSetting(trackBar_volumeSlider.Name, trackBar_volumeSlider);
            xmlSettings.AddControlSetting(txt_voiceIndex.Name, txt_voiceIndex);
            xmlSettings.AddControlSetting(txt_you.Name, txt_you);
            xmlSettings.AddControlSetting(txt_speak_abilityUse.Name, txt_speak_abilityUse);
            xmlSettings.AddControlSetting(txt_speak_abilityReady.Name, txt_speak_abilityReady);
            xmlSettings.AddControlSetting(chk_speakPhase.Name, chk_speakPhase);
            xmlSettings.AddControlSetting(chk_speakEvent.Name, chk_speakEvent);
            xmlSettings.AddControlSetting(chk_showLogs.Name, chk_showLogs);
            xmlSettings.AddControlSetting(chk_showMini.Name, chk_showMini);
            xmlSettings.AddControlSetting(chk_quickMode.Name, chk_quickMode);
            xmlSettings.AddControlSetting(combo_serverName.Name, combo_serverName);

            //a12s stuff
            xmlSettings.AddControlSetting(chk_a12s_tscallout.Name, chk_a12s_tscallout);
            xmlSettings.AddControlSetting(chk_a12s_preycallout.Name, chk_a12s_preycallout);
            xmlSettings.AddControlSetting(txt_a12s_sacRadiant.Name, txt_a12s_sacRadiant);
            xmlSettings.AddControlSetting(txt_a12s_sac.Name, txt_a12s_sac);
            xmlSettings.AddControlSetting(txt_a12s_pos1.Name, txt_a12s_pos1);
            xmlSettings.AddControlSetting(txt_a12s_pos2.Name, txt_a12s_pos2);
            xmlSettings.AddControlSetting(txt_a12s_pos3.Name, txt_a12s_pos3);
            xmlSettings.AddControlSetting(txt_a12s_pos4.Name, txt_a12s_pos4);
            xmlSettings.AddControlSetting(txt_a12s_pos5.Name, txt_a12s_pos5);
            xmlSettings.AddControlSetting(txt_a12s_left.Name, txt_a12s_left);
            xmlSettings.AddControlSetting(txt_a12s_right.Name, txt_a12s_right);

            //a11s stuff
            xmlSettings.AddControlSetting(txt_a11s_optical_shiva.Name, txt_a11s_optical_shiva);
            xmlSettings.AddControlSetting(txt_a11s_optical_stack.Name, txt_a11s_optical_stack);
            xmlSettings.AddControlSetting(txt_a11s_optical_out.Name, txt_a11s_optical_out);
            xmlSettings.AddControlSetting(txt_a11s_sword_left.Name, txt_a11s_sword_left);
            xmlSettings.AddControlSetting(txt_a11s_sword_right.Name, txt_a11s_sword_right); 

            xmlSettings.AddControlSetting(chk_a10s_stopMoving.Name, chk_a10s_stopMoving);

            if (File.Exists(settingsFile))
            {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);
                try
                {
                    while (xReader.Read())
                    {
                        if (xReader.NodeType == XmlNodeType.Element)
                        {
                            if (xReader.LocalName == "SettingsSerializer")
                            {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error loading settings: " + ex.Message;
                }
                xReader.Close();
            }
        }
        void SaveSettings()
        {
            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
            xWriter.Formatting = Formatting.Indented;
            xWriter.Indentation = 1;
            xWriter.IndentChar = '\t';
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
            xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
            xWriter.WriteEndElement();  // </SettingsSerializer>
            xWriter.WriteEndElement();  // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush();    // Flush the file buffer to disk
            xWriter.Close();
        }
        #endregion

        private void chk_showMini_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chk_showMini.Checked == true)
            {
                uxForm.Show();
            }
            else
            {
                uxForm.Hide();
            }
        }

        private void chk_showLogs_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chk_showLogs.Checked == false)
            {
                this.splitContainer1.Panel2Collapsed = true;
                this.splitContainer1.Panel2.Hide();
            }
            else
            {
                this.splitContainer1.Panel2Collapsed = false;
                this.splitContainer1.Panel2.Show();
            }
        }

        private void btn_reloadPlayers_Click(object sender, EventArgs e)
        {
            loadPlayersFromFile(playerFile);
            grid_players.Refresh();
        }

        private void btn_a7s_setPriority_Click(object sender, EventArgs e)
        {
            ffxiv_jobSortOrder = Regex.Split(this.txt_a7s_partyPriority.Text, ",");
            clearPlayerList();
        }

        private void btn_test_Click(object sender, EventArgs e)
        {
            myTTTVServer.varAPIAuth = "oaj45fd0ajf44cafp881123sd";
            
            //test new form UX    
            //test xml
        }

        private void btn_clearPlayerList_Click(object sender, EventArgs e)
        {
            clearPlayerList();
        }

        private void list_log_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender != list_log) return;

            if (e.Control && e.KeyCode == Keys.C)
            {
                if (this.list_log.SelectedItems.Count == 1)
                {
                    string toCopyPaste = this.list_log.SelectedItems[0].SubItems[this.list_log.SelectedItems[0].SubItems.Count - 1].Text;
                    if (toCopyPaste != "")
                    {
                        Clipboard.SetText(toCopyPaste);
                    }
                }

            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            if (!workerRunning)
            {
                btn_start.Text = "STOP";
                simulationFile = this.txt_simFile.Text;
                workerRunning = true;
                myThread = new Thread(myBackgroudWorker);
                myThread.Start();
                this.txt_simFile.Enabled = false;
                this.chk_quickMode.Enabled = false;
                log("Watch started", true);
            }
            else
            {
                if (txt_simFile.Text != "")
                {
                    stopFight();
                }
                log("Watch stopped", true);
                workerRunning = false;
                myThread.Abort();
                btn_start.Text = "START";
                delayedMessage_t = 0;
                delayedMessage_m = "";
                stopWatch.Stop();
                stopWatch.Reset();
                fightTimer.Enabled = false;

                this.txt_simFile.Enabled = true;
                this.chk_quickMode.Enabled = true;
            }
        }

        private void combo_voiceSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo_voiceSelector.SelectedIndex != combo_voiceSelector.Items.Count-1)
            {
                synthesizer.SelectVoice(combo_voiceSelector.SelectedItem.ToString());
                
            }
            txt_voiceIndex.Text = combo_voiceSelector.SelectedIndex.ToString();
            log("Changing voice", false, combo_voiceSelector.SelectedItem.ToString());
        }

        private void trackBar_volumeSlider_Scroll(object sender, EventArgs e)
        {
            synthesizer.Volume = this.trackBar_volumeSlider.Value;
        }

        private void btn_toSpeak_Click(object sender, EventArgs e)
        {
            if (txt_toSpeak.Text != "")
            {
                botspeak(txt_toSpeak.Text);
            }
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.txt_simFile.Text = this.openFileDialog1.FileName;
        }

        private void btn_openServer_Click(object sender, EventArgs e)
        {
            if ((this.btn_openServer.Text == "Connect") && (this.combo_serverName.Text != ""))
            {
                this.btn_openServer.Text = "Connecting...";
                this.btn_openServer.Enabled = false;
                this.combo_serverName.Enabled = false;
                requestChannel();
                //broadcastChannel = 1;
                //broadcastServer = true;
            }
            else
            {
                broadcastServer = false;
                broadcastChannel = 0; //not needed
                this.combo_serverName.Enabled = true;
                this.btn_openServer.Text = "Connect";
                this.lbl_broadcastChannel.Text = "-";
                this.txt_broadcastURL.Text = "";
            }
        }

        private void chk_quickMode_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chk_quickMode.Checked == true)
            {
                this.lbl_partySize.Visible = false;
                this.txt_partySize.Visible = false;
                this.lbl_you.Visible = false;
                this.txt_you.Visible = false;
                this.splitContainer2.Panel1Collapsed = true;
                this.splitContainer2.Panel1.Hide();
                quickMode = true;
            }
            else
            {
                this.lbl_partySize.Visible = true;
                this.txt_partySize.Visible = true;
                this.lbl_you.Visible = true;
                this.txt_you.Visible = true;
                this.splitContainer2.Panel1Collapsed = false;
                this.splitContainer2.Panel1.Show();
                quickMode = false;
            }
        }

        private void txt_voiceIndex_TextChanged(object sender, EventArgs e)
        {
            this.combo_voiceSelector.SelectedIndex = Int32.Parse(this.txt_voiceIndex.Text);
        }

        private void combo_xml_fightFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.list_fight.Clear();
            if (this.combo_xml_fightFile.Text != "")
            {
                string xmlFilePath = xmlFolderPath + this.combo_xml_fightFile.Text;
                log("Test Loading XML", true, xmlFilePath);
                loadFightXml(xmlFilePath);
                log("XML Loaded", true, this.list_fight.Items.Count.ToString());

                this.list_fight.Columns.Add("Phase #");
                this.list_fight.Columns.Add("Details", 200);
                this.list_fight.Columns.Add("Timing");
                this.list_fight.Columns.Add("Speak");
                this.list_fight.Columns.Add("Trigger", 200);
                this.list_fight.Columns.Add("--", -1);
            }            
        }

        private void combo_serverName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
