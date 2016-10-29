using System;
using System.ComponentModel;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.IO;
using System.Reflection;
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

            
            LoadSettings();
            initEncounterPlugin();
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
                startNewFight();
            }
        }

        void encounter_oFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            log("ACT combat end: " + encounterInfo.encounter);
            stopFight();
        }

        void encounter_oFormActMain_AfterCombatAction(bool isImport, CombatActionEventArgs actionInfo)
        {
            if (workerRunning) // implement in new way later PLZ, too resource intensive
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
                        this.grid_players.DataSource = ffxiv_player_list;
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

            //a11s stuff
            xmlSettings.AddControlSetting(txt_a11s_optical_shiva.Name, txt_a11s_optical_shiva);
            xmlSettings.AddControlSetting(txt_a11s_optical_stack.Name, txt_a11s_optical_stack);
            xmlSettings.AddControlSetting(txt_a11s_optical_out.Name, txt_a11s_optical_out);
            xmlSettings.AddControlSetting(txt_a11s_sword_left.Name, txt_a11s_sword_left);
            xmlSettings.AddControlSetting(txt_a11s_sword_right.Name, txt_a11s_sword_right);
            xmlSettings.AddControlSetting(txt_a11s_pauldron.Name, txt_a11s_pauldron);

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
        }

        private void btn_a7s_setPriority_Click(object sender, EventArgs e)
        {
            ffxiv_jobSortOrder = Regex.Split(this.txt_a7s_partyPriority.Text, ",");
            clearPlayerList();
        }

        private void btn_test_Click(object sender, EventArgs e)
        {
            myTTTVServer.varAPIAuth = "oajfd0ajfcafpsd";
            this.txt_simFile.Text = "applbot\\output.txt";
            this.txt_bossName.Text = "Cruise Chaser";
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
                log("Watch started", true);
                workerRunning = true;
                myThread = new Thread(myBackgroudWorker);
                myThread.Start();
                btn_start.Text = "STOP";

                simulationFile = this.txt_simFile.Text;
                this.txt_simFile.Enabled = false;
                this.txt_bossName.Enabled = false;
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
                this.txt_bossName.Enabled = true;
            }
        }

        private void combo_voiceSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            synthesizer.SelectVoice(combo_voiceSelector.SelectedItem.ToString());
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
                synthesizer.SpeakAsync(txt_toSpeak.Text);
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
    }
}
