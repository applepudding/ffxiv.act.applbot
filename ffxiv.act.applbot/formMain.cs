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

            /*
            LoadSettings();
            initEncounterPlugin();
            // Create some sort of parsing event handler.  After the "+=" hit TAB twice and the code will be generated for you.
            ActGlobals.oFormActMain.AfterCombatAction += new CombatActionDelegate(encounter_oFormActMain_AfterCombatAction);
            ActGlobals.oFormActMain.OnCombatStart += new CombatToggleEventDelegate(encounter_oFormActMain_OnCombatStart);
            ActGlobals.oFormActMain.OnCombatEnd += new CombatToggleEventDelegate(encounter_oFormActMain_OnCombatEnd);
            ActGlobals.oFormActMain.LogFileChanged += new LogFileChangedDelegate(encounter_oFormActMain_LogFileChanged);
            */
            lblStatus.Text = "Plugin Started";
        }
        public void DeInitPlugin()
        {
            // Unsubscribe from any events you listen to when exiting!
            /*
            ActGlobals.oFormActMain.AfterCombatAction -= encounter_oFormActMain_AfterCombatAction;
            ActGlobals.oFormActMain.OnCombatStart -= encounter_oFormActMain_OnCombatStart;
            ActGlobals.oFormActMain.OnCombatEnd -= encounter_oFormActMain_OnCombatEnd;
            ActGlobals.oFormActMain.LogFileChanged -= encounter_oFormActMain_LogFileChanged;

            SaveSettings();
            disposeEncounterPlugin();
            */
            lblStatus.Text = "Plugin Exited";
        }
        #endregion
        
    }
}
