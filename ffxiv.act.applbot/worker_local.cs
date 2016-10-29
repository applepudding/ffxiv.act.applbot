using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace ffxiv.act.applbot
{
    partial class formMain
    {
        public void myBackgroudWorker()
        {
            Match m;
            string pattern = "";
            string fileName = logFileName_active;

            if (File.Exists(simulationFile)) fileName = simulationFile; //check if simulation mode

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    log("using log file", false, fileName);

                    while (!sr.EndOfStream) sr.ReadLine(); //read file till the end when first running

                    while (workerRunning)
                    {
                        while (sr.EndOfStream) Thread.Sleep(threadSleepLength); //sleeps if no input
                        string resultLine = sr.ReadLine();

                        pattern = " has begun.";
                        if (resultLine.Contains(pattern))
                        {
                            clearPlayerList();
                        }
                        pattern = "=SIMSTART=";
                        if (resultLine.Contains(pattern))
                        {
                            if (this.txt_bossName.Text != "")
                            {
                                startNewFight();
                                currentFight = this.txt_bossName.Text; //TEMPORARY VERY UNSAFE, REMOVE ASAP------------------------------                                                                        
                            }
                        }
                        pattern = "=SIMSTOP=";
                        if (resultLine.Contains(pattern))
                        {
                            stopFight();
                        }

                        if (sleep_t == 0)
                        {
                            //fight flow temp stuff
                            if (current_lv_index < this.list_fight.Items.Count)
                            {
                                //if (this.combo_xml_fightFile.Text != "")
                                {
                                    updateCurrent_lvi(current_lv_index);
                                    if (current_lvi.SubItems.Count > 0) //check if empty
                                    {
                                        if (current_phaseChange_trigger != "") //detect if read line is phase change
                                        {
                                            if (stopWatch.Elapsed.TotalSeconds > current_phaseChange_offset)
                                            {
                                                m = Regex.Match(resultLine, current_phaseChange_trigger);
                                                if (m.Success)
                                                {
                                                    log("force change phase trigger: " + current_lv_index.ToString(), false, resultLine);
                                                    nextPhase();
                                                    list_fight.Items[current_lv_index].Selected = true;
                                                    highlightEvent();
                                                }
                                            }
                                        }

                                        if (current_lvi.Text != "") //currentline is phase indicator
                                        {
                                            current_phaseChange_offset = Int32.Parse(current_lvi.SubItems[2].Tag.ToString());
                                            current_phaseChange_trigger = current_lvi.SubItems[4].Text;

                                            if (chk_speakPhase.Checked)
                                            {
                                                string toSpeak = current_lvi.SubItems[3].Text;
                                                synthesizer.SpeakAsync(toSpeak);
                                            }


                                            next_lvi();
                                            list_fight.Items[current_lv_index].Selected = true;

                                            log("phase change line to: " + current_lv_index.ToString());
                                            highlightEvent();
                                        }
                                        if (current_lvi.Text == "") //currentline is fight event
                                        {
                                            pattern = current_lvi.SubItems[4].Text;
                                            if (pattern != "")
                                            {
                                                m = Regex.Match(resultLine, pattern);
                                                if (m.Success)
                                                {
                                                    completeEvent();
                                                    log("event trigger: " + current_lv_index.ToString(), false, resultLine);
                                                    if (chk_speakEvent.Checked)
                                                    {
                                                        string toSpeak = current_lvi.SubItems[3].Text;
                                                        synthesizer.SpeakAsync(toSpeak);
                                                    }
                                                    next_lvi();
                                                    highlightEvent();
                                                }
                                            }
                                            if ((pattern == "") && (Int32.Parse(current_lvi.SubItems[2].Text) == 0)) //currentline is fight event with no trigger
                                            {
                                                completeEvent();
                                                log("event elapsed: " + current_lv_index.ToString());
                                                if (chk_speakEvent.Checked)
                                                {
                                                    string toSpeak = current_lvi.SubItems[3].Text;
                                                    synthesizer.SpeakAsync(toSpeak);
                                                }
                                                next_lvi();
                                                highlightEvent();
                                            }
                                        }
                                    }
                                }
                            }
                            //------------------------------remove later

                            #region debug boss activity
                            if (this.txt_bossName.Text != "")
                            {
                                string[] bossNames = Regex.Split(this.txt_bossName.Text, "#");
                                foreach (string bossName in bossNames)
                                {
                                    pattern = bossName + " uses ";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string[] mainElements = Regex.Split(resultLine, pattern);
                                        string toSpeak = string.Format(this.txt_speak_abilityUse.Text + " {0}", mainElements[1]);
                                        if (this.chk_speakAbilityUse.Checked) synthesizer.SpeakAsync(toSpeak);
                                        log("Uses " + mainElements[1], false, resultLine);
                                        break;
                                    }

                                    pattern = this.txt_bossName.Text + " readies ";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string[] mainElements = Regex.Split(resultLine, pattern);
                                        string toSpeak = string.Format(this.txt_speak_abilityReady.Text + " {0}", mainElements[1]);
                                        if (this.chk_speakAbilityReady.Checked) synthesizer.SpeakAsync(toSpeak);
                                        log("Readies " + mainElements[1], false, resultLine);
                                        break;
                                    }

                                    pattern = this.txt_bossName.Text + " begins casting ";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string[] mainElements = Regex.Split(resultLine, pattern);
                                        string toSpeak = string.Format(this.txt_speak_abilityReady.Text + " {0}", mainElements[1]);
                                        if (this.chk_speakAbilityReady.Checked) synthesizer.SpeakAsync(toSpeak);
                                        log("Begins Casting " + mainElements[1], false, resultLine);
                                        break;
                                    }

                                    pattern = this.txt_bossName.Text + " casts ";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string[] mainElements = Regex.Split(resultLine, pattern);
                                        string toSpeak = string.Format(this.txt_speak_abilityReady.Text + " {0}", mainElements[1]);
                                        if (this.chk_speakAbilityReady.Checked) synthesizer.SpeakAsync(toSpeak);
                                        log("Casts " + mainElements[1], false, resultLine);
                                        break;
                                    }
                                }

                            }
                            #endregion

                            switch (currentFight)
                            {

                                #region A11S
                                case "Cruise Chaser":
                                    pattern = "Armored Pauldron.0.3c.aa0.2436.0";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string toSpeak = "Pauldron";

                                        synthesizer.SpeakAsync(toSpeak);
                                        log(toSpeak);
                                        sleep_t = 12;
                                    }
                                    break;
                                #endregion

                                #region A9S
                                //NOT WORKING YET
                                /*
                                case "Faust Z":
                                    pattern = "The Faust Z uses Kaltstrahl";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        if (temp_number3 <= 0)
                                        {
                                            temp_number3 = 3;
                                            if (temp_number1 > 0)
                                            {
                                                if (temp_number2 < 2)
                                                {
                                                    delayedMessage_m = "aoe";
                                                    delayedMessage_t = 2;
                                                }
                                                else
                                                {
                                                    delayedMessage_m = "aoe";
                                                    delayedMessage_t = 5;
                                                }
                                            }
                                        }
                                        if (temp_number3 > 0)
                                        {
                                            delayedMessage_m = temp_number3.ToString();
                                            delayedMessage_t = 5;
                                            temp_number3--;
                                        }
                                        temp_number1++;
                                    }
                                    pattern = "The Faust Z uses Panzerschreck";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        temp_number2++;
                                        if ((temp_number2 % 2) != 0)
                                        {
                                            delayedMessage_m = temp_number3.ToString();
                                            if (temp_number2 == 1)
                                            {
                                                delayedMessage_t = 5;
                                            }
                                            else
                                            {
                                                delayedMessage_t = 2;
                                            }                                            
                                            temp_number3--;
                                        }
                                    }
                                    break;
                                    */
                                #endregion

                                #region A5S
                                case "Ratfinx Twinkledinks":
                                    pattern = "41b.Prey.0.00.E0000000..[0-9A-F]{8}.";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string[] mainElements = Regex.Split(resultLine, pattern);
                                        string[] subElements = Regex.Split(mainElements[1], "\\|");
                                        string castTarget = getNickname(subElements[0]);
                                        if (temp_1 == "")
                                        {
                                            if (temp_2 == "")
                                            {
                                                temp_1 = subElements[0];
                                                string toSpeak = string.Format("Prey {0}", castTarget);
                                                if (chk_a5s_callDoublePrey.Checked)
                                                {
                                                    synthesizer.SpeakAsync(toSpeak);
                                                }

                                                log(toSpeak, false, resultLine);
                                            }
                                            else
                                            {
                                                if (temp_2 == subElements[0])
                                                {
                                                    temp_2 = "";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (temp_1 == subElements[0])
                                            {
                                                temp_1 = "";
                                            }
                                            else
                                            {
                                                if (temp_2 != subElements[0])
                                                {
                                                    temp_2 = subElements[0];
                                                    string toSpeak = string.Format("Second Prey {0}", castTarget);
                                                    if (chk_a5s_callDoublePrey.Checked)
                                                    {
                                                        synthesizer.SpeakAsync(toSpeak);
                                                    }
                                                    log(toSpeak, false, resultLine);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                #endregion

                                #region ADS TEST
                                case "The Meteor fissure":
                                    pattern = "High Voltage.1044A387.";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        startCountdown(7);
                                        string[] mainElements = Regex.Split(resultLine, pattern);
                                        string[] subElements = Regex.Split(mainElements[1], "\\|");
                                        string castTarget = getNickname(subElements[0]);
                                        //string toSpeak = string.Format("High Voltage on {0}", castTarget[0]);
                                        string toSpeak = string.Format("High Voltage on {0}", castTarget);

                                        //delay message test
                                        //delayedMessage_m = string.Format("Stack on tank {0}", castTarget[0]);
                                        delayedMessage_m = string.Format("Stack on tank {0}", castTarget);
                                        delayedMessage_t = 10;

                                        synthesizer.SpeakAsync(toSpeak);
                                        log(toSpeak);
                                    }
                                    break;
                                #endregion

                                #region A7S

                                case "Quickthinx Allthoughts": //A7S

                                    //flame MARKER
                                    pattern = "(?<char>[a-zA-Z']*) [a-zA-Z']{1,}.[0-9A-F]{4}.[0-9A-F]{4}.0019.[0-9A-F]{4}.[0-9A-F]{4}.[0-9A-F]{4}."; //27|2016-09-04T19:44:45.8420000-07:00|1044A387|Apple Pudding|0000|E3AF|0019|0000|0000|0000|
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string[] mainElements = Regex.Split(m.Value, "\\|");
                                        string castTarget = mainElements[0];
                                        string toSpeak = "flame " + getNickname(castTarget);
                                        log(toSpeak, false, resultLine);
                                        if (this.chk_a7s_flameCallout.Checked) synthesizer.SpeakAsync(toSpeak);
                                    }

                                    //Beam MARKER
                                    pattern = "(?<char>[a-zA-Z']*) [a-zA-Z']{1,}.[0-9A-F]{4}.[0-9A-F]{4}.0018.[0-9A-F]{4}.[0-9A-F]{4}.[0-9A-F]{4}.";
                                    m = Regex.Match(resultLine, pattern);
                                    if (m.Success)
                                    {
                                        string[] mainElements = Regex.Split(m.Value, "\\|");
                                        string castTarget = mainElements[0];
                                        string toSpeak = "beam " + getNickname(castTarget);
                                        log(toSpeak, false, resultLine);
                                        if (this.chk_a7s_beamCallout.Checked) synthesizer.SpeakAsync(toSpeak);
                                    }

                                    switch (currentPhase)
                                    {
                                        case 1:
                                            pattern = "Bomb.16FA.Explosion..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                currentPhase = 2;
                                                log("--- Phase " + currentPhase, true, resultLine);
                                                sleep_t = 3;
                                            }
                                            break;
                                        case 2:
                                            pattern = "Bomb.16FA.Explosion..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                string[] mainElements = Regex.Split(resultLine, pattern);
                                                string[] subElements = Regex.Split(mainElements[1], "\\|");
                                                string castTarget = subElements[0];
                                                string toLog = string.Format("---Bomb on {0}, {1}", getClass(subElements[0]), subElements[0]);
                                                log(toLog, false, resultLine);

                                                if (getClass(subElements[0]) == "heal")
                                                {
                                                    //string toSpeak = string.Format("tank prey and healer tether");
                                                    string toSpeak = string.Format("{0} prey and {1} tether", getNickname(a7s_grabPlayerByClass("tank")), getNickname(a7s_getOtherSameClass(castTarget)));
                                                    synthesizer.SpeakAsync(toSpeak);
                                                    log(toSpeak);
                                                }
                                                else if (getClass(subElements[0]) == "melee")
                                                {
                                                    //string toSpeak = string.Format("caster prey and tank tether"); //MELEE tether
                                                    string toSpeak = string.Format("{0} prey and {1} tether", getNickname(a7s_grabPlayerByClass("caster")), getNickname(a7s_grabPlayerByClass(this.combo_a7s_melee1jail.Text)));
                                                    synthesizer.SpeakAsync(toSpeak);
                                                    log(toSpeak);
                                                }
                                                sleep_t = 3; //sleep 3s
                                            }
                                            pattern = "Shanoa hurry-scuttles to laugh at uplander doom";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                currentPhase = 3;
                                                log("--- Phase " + currentPhase, true, resultLine);
                                                //synthesizer.SpeakAsync("Phase " + currentPhase);
                                            }
                                            break;
                                        case 3:
                                            pattern = "Bomb.16FA.Explosion..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                string[] mainElements = Regex.Split(resultLine, pattern);
                                                string[] subElements = Regex.Split(mainElements[1], "\\|");
                                                string castTarget = subElements[0];
                                                string toLog = string.Format("---Bomb on {0}, {1}", getClass(subElements[0]), subElements[0]);
                                                log(toLog, false, resultLine);
                                                if (getClass(subElements[0]) == "heal")
                                                {
                                                    //string toSpeak = string.Format("healer prey and caster tether");
                                                    string toSpeak = string.Format("{0} prey and {1} tether", getNickname(a7s_getOtherSameClass(castTarget)), getNickname(a7s_grabPlayerByClass("caster")));
                                                    synthesizer.SpeakAsync(toSpeak);
                                                    log(toSpeak);
                                                }
                                                else if ((getClass(subElements[0]) == "caster") || (getClass(subElements[0]) == "range"))
                                                {
                                                    //string toSpeak = string.Format("melee prey and tank tether");
                                                    string toSpeak = string.Format("{0} prey and {1} tether", getNickname(a7s_grabPlayerByClass("melee")), getNickname(a7s_grabPlayerByClass("tank")));
                                                    synthesizer.SpeakAsync(toSpeak);
                                                    log(toSpeak);
                                                }
                                                sleep_t = 3; //sleep 3s
                                                temp_number1++;
                                            }
                                            pattern = "Quickthinx Allthoughts readies Sizzlebeam";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                if (temp_number1 > 1)
                                                {
                                                    currentPhase = 4;
                                                    log("--- Phase " + currentPhase + ": Superbeam", true, resultLine);
                                                    synthesizer.SpeakAsync("Super Beam Phase");
                                                    temp_number1 = 0;
                                                }
                                            }
                                            pattern = "Quickthinx Allthoughts readies Uplander Doom";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                if (temp_number1 > 1)
                                                {
                                                    currentPhase = 4;
                                                    log("--- Phase " + currentPhase + ": Superbeam", true, resultLine);
                                                    synthesizer.SpeakAsync("Super Beam Phase");
                                                    temp_number1 = 0;
                                                }
                                            }
                                            break;
                                        case 4: // SUPER BEAM
                                            pattern = "Quickthinx Allthoughts readies Sizzlebeam";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                delayedMessage_m = "bombs on everyone";
                                                delayedMessage_t = 4;
                                            }
                                            pattern = "Quickthinx Allthoughts readies Uplander Doom";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                currentPhase = 5;
                                                log("--- Phase " + currentPhase + ": Merry Go Round", true, resultLine);
                                                synthesizer.SpeakAsync("Merry go round Phase");
                                            }
                                            break;
                                        case 5: //MERRY GO ROUND
                                            pattern = "Shanoa readies Undying Affection";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                currentPhase = 6;
                                                log("--- Phase " + currentPhase, true, resultLine);
                                            }
                                            break;
                                        case 6: //2nd cat phase
                                            pattern = "Shanoa readies Undying Affection";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                currentPhase = 7;
                                                log("--- Phase " + currentPhase + ": Double Sizzlespark", true, resultLine);
                                                temp_number1 = 0;
                                            }
                                            break;
                                        case 7:
                                            pattern = "Quickthinx Allthoughts uses Sizzlespark";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                temp_number1++;
                                                if (temp_number1 > 1)
                                                {
                                                    currentPhase = 8;
                                                    log("--- Phase " + currentPhase + ": Final Jail Sets", true, resultLine);
                                                    synthesizer.SpeakAsync("Final Jail Phase");
                                                }
                                            }
                                            break;
                                        case 8: //Last jail set
                                            pattern = "Bomb.16FA.Explosion..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                string[] mainElements = Regex.Split(resultLine, pattern);
                                                string[] subElements = Regex.Split(mainElements[1], "\\|");
                                                string castTarget = subElements[0];
                                                string toLog = string.Format("---Bomb on {0}, {1}", getClass(subElements[0]), subElements[0]);
                                                log(toLog, false, resultLine);

                                                if (getClass(subElements[0]) == "heal")
                                                {
                                                    //string toSpeak = string.Format("melee prey and healer tether"); //melee prey
                                                    string toSpeak = string.Format("{0} tether", getNickname(a7s_getOtherSameClass(castTarget)));
                                                    synthesizer.SpeakAsync(toSpeak);
                                                    log(toSpeak);
                                                }
                                                if (getClass(subElements[0]) == "melee")
                                                {
                                                    //string toSpeak = string.Format("melee prey and healer tether"); //melee prey
                                                    string toSpeak = string.Format("{0} prey", getNickname(a7s_grabPlayerByClass(this.combo_a7s_melee3jail.Text))); //getNickname(a7s_grabPlayerByClass("tank"))
                                                    synthesizer.SpeakAsync(toSpeak);
                                                    log(toSpeak);
                                                }
                                            }

                                            pattern = "(?<char>[a-zA-Z']*) [a-zA-Z']{1,}.[0-9A-F]{4}.[0-9A-F]{4}.0018.[0-9A-F]{4}.[0-9A-F]{4}.[0-9A-F]{4}.";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                string[] mainElements = Regex.Split(m.Value, "\\|");
                                                string castTarget = mainElements[0];
                                                if (getClass(castTarget) != "heal")
                                                {
                                                    //string toSpeak = string.Format("tank prey and caster tether"); 
                                                    string toSpeak = string.Format("{0} prey and {1} tether", getNickname(a7s_grabPlayerByClass("tank")), getNickname(a7s_grabPlayerByClass("caster"))); // a7s_getOtherSameClass(castTarget)
                                                    synthesizer.SpeakAsync(toSpeak);
                                                    log(toSpeak, false, resultLine);
                                                }
                                            }
                                            break;
                                    }
                                    break;
                                #endregion

                                #region A6S
                                case "Machinery Bay 70": //A6S vortexer
                                    switch (currentPhase)
                                    {
                                        case 1:
                                            pattern = "Compressed Water.21.00..........Vortexer..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                startCountdown(20);
                                                string[] mainElements = Regex.Split(resultLine, pattern);
                                                string[] subElements = Regex.Split(mainElements[1], "\\|");
                                                string castTarget = getNickname(subElements[0]);
                                                string toSpeak = string.Format("first Water {0}", castTarget);
                                                synthesizer.SpeakAsync(toSpeak);
                                                log(toSpeak, false, resultLine);

                                                temp_1 = castTarget;

                                            }
                                            pattern = "Compressed Lightning.21.00..........Vortexer..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                string[] mainElements = Regex.Split(resultLine, pattern);
                                                string[] subElements = Regex.Split(mainElements[1], "\\|");
                                                string castTarget = getNickname(subElements[0]);
                                                string toSpeak = string.Format("first Lightning {0}", castTarget);
                                                synthesizer.SpeakAsync(toSpeak);
                                                log(toSpeak, false, resultLine);
                                            }
                                            pattern = "Vortexer readies Fire Beam";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                currentPhase = 2;
                                                log("--- Phase " + currentPhase, true, resultLine);
                                            }
                                            break;
                                        case 2:
                                            pattern = "Compressed Water.21.00..........Vortexer..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                startCountdown(20);
                                                string[] mainElements = Regex.Split(resultLine, pattern);
                                                string[] subElements = Regex.Split(mainElements[1], "\\|");
                                                string castTarget = getNickname(subElements[0]);
                                                string toSpeak = string.Format("second Water {0}", castTarget);
                                                synthesizer.SpeakAsync(toSpeak);
                                                log(toSpeak, false, resultLine);

                                                temp_2 = castTarget;
                                                delayedMessage_m = string.Format("water 1 was {0}", temp_1);
                                                //delayedMessage_m = string.Format("water 1 was on {0}", temp_1);
                                                delayedMessage_t = 7;
                                                currentPhase = 3;
                                            }
                                            break;
                                        case 3:
                                            pattern = "Compressed Water.21.00..........Vortexer..........";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                string[] mainElements = Regex.Split(resultLine, pattern);
                                                string[] subElements = Regex.Split(mainElements[1], "\\|");
                                                string castTarget = getNickname(subElements[0]);
                                                string toSpeak = string.Format("third Water {0}", castTarget);
                                                synthesizer.SpeakAsync(toSpeak);
                                                log(toSpeak, false, resultLine);
                                                string lightningTaker = temp_2;
                                                string awayFromEveryone = temp_1;
                                                if (combo_a6s_lastLightning.Text == "water 1")
                                                {
                                                    lightningTaker = temp_1;
                                                    awayFromEveryone = temp_2;
                                                }
                                                delayedMessage_m = string.Format("stack on tank {0} and run away {1}", temp_1, temp_2);
                                                //delayedMessage_m = string.Format("water 1 was on {0}", temp_1);
                                                delayedMessage_t = 7;
                                                currentPhase = 4;
                                            }
                                            break;
                                        case 4:
                                            pattern = "Vortexer readies Ultra Flash";
                                            m = Regex.Match(resultLine, pattern);
                                            if (m.Success)
                                            {
                                                currentPhase = 1;
                                                log("--- Phase " + currentPhase, true, resultLine);
                                            }
                                            break;
                                    }
                                    break;
                                    #endregion

                            }
                        }
                    }
                }
            }
        }
    }
}
