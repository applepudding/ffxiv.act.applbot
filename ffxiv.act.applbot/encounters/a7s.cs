namespace ffxiv.act.applbot
{
    partial class formMain
    {
        public string getClass(string playerName)
        {
            string className = "";
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if (player.varName == playerName)
                {
                    className = player.varClass;
                    break;
                }
            }
            return className;
        }

        public string a7s_grabPlayerByClass(string className)
        {
            string fullName = "";
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if (player.varClass == className)
                {
                    fullName = player.varName;
                    break;
                }
            }
            return fullName;
        }

        public string a7s_getOtherSameClass(string playerName)
        {
            string classname = "";
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if (player.varName == playerName)
                {
                    classname = player.varClass;
                    break;
                }
            }
            string fullName = "";
            foreach (ffxiv_player player in ffxiv_player_list)
            {
                if ((player.varClass == classname) && (player.varName != playerName))
                {
                    fullName = player.varName;
                    break;
                }
            }
            return fullName;
        }
    }
}
