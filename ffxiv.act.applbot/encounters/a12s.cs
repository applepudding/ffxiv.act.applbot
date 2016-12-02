using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace ffxiv.act.applbot
{
    public partial class formMain
    {
        bool a12s_temporalStasis = false;
        int a12s_ts_id = 0;
        int a12s_ts_countdown = 2;
        int a12s_ts_count = 0;
        int a12s_halfGravityCount = 0;

        string a12s_preyTarget = "";
        int a12s_preyCount = 0;
        
        void a12s_setDebuff(string playerName, string debuff, int order)
        {
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if (player.varName == playerName)
                {
                    player.varDebuff = debuff;
                    player.varInt = order;
                    break;
                }
            }
        }
        /*
        string a12s_resolveTemporalStasis()
        {
            string resultPosition = "";

            if (chk_a12s_tscallout.Checked)
            {
                int tempPosition = 1;
                int defamationCount = a12s_countDefamation();
                int sharedSentenceCount = a12s_countSharedSentence();
                string tetherType = "";

                bool tempPosition3Claimed = false;
                bool tempPosition2Claimed = false;
                bool tempPosition5Claimed = false;

                log("resolving stasis");

                //sort partylist by debuff order
                List<ffxiv_player> SortedList = ffxiv_player_list.ToList<ffxiv_player>().OrderBy(o => o.varInt).ToList();
                ffxiv_player_list = new BindingList<ffxiv_player>(SortedList);
                this.grid_players.DataSource = ffxiv_player_list;

                //position all defamation

                foreach (ffxiv_player player in ffxiv_player_list)
                {
                    if (player.varDebuff == "Defamation")
                    {
                        player.varPosition = tempPosition.ToString();
                        tempPosition++;
                    }
                }

                if ((defamationCount > 1) || (sharedSentenceCount == 2))
                {
                    //position shared sentence (same for 2 and 3 defamations)
                    foreach (ffxiv_player player in ffxiv_player_list)
                    {
                        if (player.varDebuff == "Shared Sentence")
                        {
                            //if (player.varClass == "heal")
                            if (tempPosition5Claimed)
                            {
                                tempPosition = 4;
                            }
                            else
                            {
                                tempPosition = 5;
                                tempPosition5Claimed = true;
                            }
                            player.varPosition = tempPosition.ToString();
                        }
                    }

                    //position Restraining Order 
                    foreach (ffxiv_player player in ffxiv_player_list)
                    {
                        if (player.varDebuff == "Restraining Order")
                        {
                            tetherType = "Restraining Order";
                            //if (player.varClass == "tank")
                            if (tempPosition2Claimed)
                            {
                                tempPosition = 5;
                            }
                            else
                            {
                                tempPosition = 2;
                                tempPosition2Claimed = true;
                            }

                            player.varPosition = tempPosition.ToString();
                        }
                    }
                    //position House Arrest    
                    foreach (ffxiv_player player in ffxiv_player_list)
                    {
                        if (player.varDebuff == "House Arrest")
                        {
                            tempPosition = 4;
                            player.varPosition = tempPosition.ToString();
                        }
                    }

                    //position no debuff  
                    foreach (ffxiv_player player in ffxiv_player_list)
                    {
                        if (player.varInt == 0)
                        {
                            if (tetherType == "Restraining Order")
                            {
                                tempPosition = 4;
                            }
                            else
                            {
                                tempPosition = 5;
                            }
                            player.varPosition = tempPosition.ToString();
                        }
                    }
                }
                else
                {
                    //position shared sentence (same for 1 and 0 defamations)
                    foreach (ffxiv_player player in ffxiv_player_list)
                    {
                        if (player.varDebuff == "Shared Sentence")
                        {
                            if (tempPosition3Claimed)
                            {
                                tempPosition = 1;
                            }
                            else
                            {
                                tempPosition = 3;
                                tempPosition3Claimed = true;
                            }

                            player.varPosition = tempPosition.ToString();
                        }
                    }
                    //position House Arrest
                    foreach (ffxiv_player player in ffxiv_player_list)
                    {
                        if (player.varDebuff == "House Arrest")
                        {
                            tempPosition = 3;
                            player.varPosition = tempPosition.ToString();
                        }
                    }
                    //position Restraining Order   
                    tempPosition = 1;
                    foreach (ffxiv_player player in ffxiv_player_list)
                    {
                        if (player.varDebuff == "Restraining Order")
                        {
                            if (tempPosition == 1)
                            {
                                player.varPosition = tempPosition.ToString();
                                tempPosition = 3;
                            }
                            else
                            {
                                player.varPosition = tempPosition.ToString();
                                tempPosition = 1;
                            }
                        }
                    }
                }

                //build output
                string[] temp_pos = new string[] { txt_a12s_pos1.Text, txt_a12s_pos2.Text, txt_a12s_pos3.Text, txt_a12s_pos4.Text, txt_a12s_pos5.Text };
                string[] temp_direction = new string[] { txt_a12s_left.Text, "center", txt_a12s_right.Text };

                foreach (ffxiv_player player in ffxiv_player_list)
                {
                    if ((defamationCount > 1) || (sharedSentenceCount == 2))
                    {
                        resultPosition += "@" + player.varName + ":" + temp_pos[Int32.Parse(player.varPosition) - 1];
                    }
                    else
                    {
                        string resultDirection = "";

                        if ((player.varPosition != "") && (player.varPosition != null) && (player.varDebuff != ""))
                        {
                            log(player.varName, true, player.varDebuff);
                            resultDirection = temp_direction[Int32.Parse(player.varPosition) - 1];
                        }

                        resultPosition += "@" + player.varName + ":" + resultDirection;
                    }
                }
            }
            return resultPosition;
        }
        */
        string a12s_resolveTemporalStasis_2()
        {
            string resultPosition = "";
            int tempPosition = 1;

            if (chk_a12s_tscallout.Checked)
            {

                //build output
                string[] temp_pos = new string[] { txt_a12s_pos1.Text, txt_a12s_pos2.Text, txt_a12s_pos3.Text, txt_a12s_pos4.Text, txt_a12s_pos5.Text };
                string[] temp_direction = new string[] { txt_a12s_left.Text, txt_a12s_right.Text };
                

                //sort partylist by debuff order
                List<ffxiv_player> SortedList = ffxiv_player_list.ToList<ffxiv_player>().OrderBy(o => o.varInt).ToList();
                ffxiv_player_list = new BindingList<ffxiv_player>(SortedList);
                
                switch (a12s_ts_id)
                {
                    case 1:
                        // 3 dps = purple, 2 tanks = near/far, 1 heal 1 dps = shared
                        //===========================================================
                        //sort partylist by name (for consistency)
                        
                        SortedList = ffxiv_player_list.ToList<ffxiv_player>().OrderByDescending(o => o.varName).ToList();
                        SortedList = ffxiv_player_list.ToList<ffxiv_player>().OrderByDescending(o => o.varOI).ToList();
                        ffxiv_player_list = new BindingList<ffxiv_player>(SortedList);
                        

                        //create list of dps
                        List<ffxiv_player> dpsList = new List<ffxiv_player>();                        
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if ((player.varClass == "caster") || (player.varClass == "range") || (player.varClass == "melee"))
                            {
                                dpsList.Add(player);
                            }
                        }
                        

                        //swap floater with Shared Sentence
                        int dpsCount = 0;
                        for (dpsCount = 0; dpsCount <3; dpsCount++)
                        {
                            if (dpsList[dpsCount].varDebuff == "Shared Sentence")
                            {
                                ffxiv_player tempDps = dpsList[dpsCount];
                                dpsList[dpsCount] = dpsList[3];
                                dpsList[3] = tempDps;
                                break;
                            }
                        }
                        

                        //position all DPS
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            dpsCount = 0;
                            foreach (ffxiv_player dpsPlayer in dpsList)
                            {
                                if(player.varName == dpsPlayer.varName)
                                {
                                    if (player.varDebuff == "Defamation")
                                    {
                                        player.varPosition = (dpsCount + 1).ToString();
                                    }
                                    if (player.varDebuff == "Shared Sentence")
                                    {
                                        tempPosition = 4;
                                        player.varPosition = tempPosition.ToString();
                                    }
                                    continue;
                                }
                                dpsCount++;
                            }
                        }
                        //position shared sentence
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if ((player.varDebuff == "Shared Sentence") && (player.varClass == "heal"))
                            {
                                tempPosition = 5;
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        //position Restraining Order
                        tempPosition = 4;
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "Restraining Order")
                            {
                                player.varPosition = tempPosition.ToString();
                                tempPosition = 3;
                            }
                        }
                        //position House Arrest    
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "House Arrest")
                            {
                                tempPosition = 4;
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        //position no debuff  
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varInt == 0)
                            {
                                tempPosition = 5;
                                player.varPosition = tempPosition.ToString();
                            }
                        }

                        break;
                    case 2:
                        // 1 tank 1 heal = purple, 1 tank 1 heal = near/far, 2 dps = near/far, 2 dps = shared
                        //===========================================================

                        bool tempPosition3Claimed = false;

                        //position all defamation
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "Defamation")
                            {

                                tempPosition = (player.varClass == "tank") ? 1 : 2;
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        //position shared sentence
                        tempPosition = 5;
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "Shared Sentence")
                            {
                                player.varPosition = tempPosition.ToString();
                                tempPosition--;
                            }
                        }
                        //position Restraining Order 
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "Restraining Order")
                            {
                                if (player.varClass == "tank")
                                {
                                    tempPosition = 3;
                                    tempPosition3Claimed = !tempPosition3Claimed;
                                }
                                else
                                {
                                    if (player.varClass == "heal")
                                    {
                                        tempPosition = 5;
                                    }
                                    else //DPS
                                    {
                                        tempPosition = (tempPosition3Claimed) ? 4 : 3; // 4 : 3
                                        tempPosition3Claimed = !tempPosition3Claimed;
                                    }
                                }

                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        //position House Arrest   
                        int houseCount = 0; 
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "House Arrest")
                            {                                
                                tempPosition = 4;
                                tempPosition = (houseCount < 2) ? 4 : 5;
                                houseCount++;
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        break;
                    case 3:
                        // 1 tank = purple, 1 tank + 1 dps = near/far, 1 heal + 1 dps = near/far, 2dps = near/far, 1 heal = shared
                        //===========================================================
                        bool tempPosition1Claimed = false;

                        //position defamation
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "Defamation")
                            {

                                tempPosition = 1;
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        //position shared sentence (same for 1 and 0 defamations)
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "Shared Sentence")
                            {
                                tempPosition = 2;
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        //position House Arrest
                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "House Arrest")
                            {
                                tempPosition = 2;
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        //position Restraining Order  
                        int restrainCount = 0;
                        List<ffxiv_player> restrainList = new List<ffxiv_player>();

                        foreach (ffxiv_player player in ffxiv_player_list)
                        {
                            if (player.varDebuff == "Restraining Order")
                            {
                                restrainList.Add(player);

                                restrainCount ++;
                                if (tempPosition1Claimed)
                                {
                                    tempPosition = 2;
                                }
                                else
                                {
                                    tempPosition = 1;
                                }
                                tempPosition1Claimed = !tempPosition1Claimed;

                                if ((restrainCount % 2) == 0) //swap if heal/tank got position 1
                                {
                                    if ((restrainList[restrainCount-2].varClass == "tank") || (restrainList[restrainCount - 2].varClass == "heal"))
                                    {
                                        tempPosition = 1;
                                        restrainList[restrainCount - 2].varPosition = "2";
                                    }
                                }
                                player.varPosition = tempPosition.ToString();
                            }
                        }
                        break;
                }


                //build output string
                
                foreach (ffxiv_player player in ffxiv_player_list)
                {
                    if ((player.varPosition != null) && (player.varPosition != ""))
                    {
                        if (a12s_ts_id < 3)
                        {
                            resultPosition += "@" + player.varName + ":" + temp_pos[Int32.Parse(player.varPosition) - 1];
                        }
                        else
                        {
                            string resultDirection = "";
                            
                            log(player.varName, true, player.varDebuff);
                            resultDirection = temp_direction[Int32.Parse(player.varPosition) - 1];
                            //resultDirection = temp_pos[Int32.Parse(player.varPosition) - 1];

                            resultPosition += "@" + player.varName + ":" + resultDirection;
                        }
                    }
                    
                }
            }
            return resultPosition;
        }
        
        int a12s_countDefamation()
        {
            int defamationCount = 0;
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if (player.varDebuff == "Defamation")
                {
                    defamationCount++;
                }
            }
            return defamationCount;
        }
        int a12s_countSharedSentence()
        {
            int sharedSentenceCount = 0;
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if (player.varDebuff == "Shared Sentence")
                {
                    sharedSentenceCount++;
                }
            }
            return sharedSentenceCount;
        }
        
        void a12s_cleanPlayerListDebuff()
        {
            //clean player list from debuffs
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                player.varInt = 0;
                player.varDebuff = "";
                player.varPosition = "";
            }
        }
    }
}
