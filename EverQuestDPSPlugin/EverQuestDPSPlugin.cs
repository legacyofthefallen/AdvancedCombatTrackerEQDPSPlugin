using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using EverQuestDPS.Enums;
using EverQuestDPS.ParserObjectGenerators;
using EverQuestDPS.Extensions;

/*
* Project: EverQuest DPS Plugin
* Original: EverQuest 2 English DPS Localization plugin developed by EQAditu
* Description: Missing from the arsenal of the plugin based Advanced Combat Tracker to track EverQuest's current combat messages.  Ignores chat as that is displayed in game.
*/

namespace EverQuestDPS
{
    public class EQDPSParser : UserControl, IActPluginV1
    {
        #region Designer generated code (Avoid editing)
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EQDPSParser));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.varianceOff = new System.Windows.Forms.RadioButton();
            this.sampVariance = new System.Windows.Forms.RadioButton();
            this.populVariance = new System.Windows.Forms.RadioButton();
            this.UpDownForPrecision = new System.Windows.Forms.NumericUpDown();
            this.digitsForPrecision = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpDownForPrecision)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.varianceOff);
            this.groupBox1.Controls.Add(this.sampVariance);
            this.groupBox1.Controls.Add(this.populVariance);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            this.groupBox1.MouseHover += new System.EventHandler(this.OnMouseHoverVarianceGroupBox);
            // 
            // varianceOff
            // 
            resources.ApplyResources(this.varianceOff, "varianceOff");
            this.varianceOff.Name = "varianceOff";
            this.varianceOff.TabStop = true;
            this.varianceOff.UseVisualStyleBackColor = true;
            this.varianceOff.CheckedChanged += new System.EventHandler(this.VarianceTypeCheckedChanged);
            this.varianceOff.MouseEnter += new System.EventHandler(this.OnMouseEnterVarianceOff);
            this.varianceOff.MouseLeave += new System.EventHandler(this.OnMouseLeaveButtonArea);
            // 
            // sampVariance
            // 
            resources.ApplyResources(this.sampVariance, "sampVariance");
            this.sampVariance.Name = "sampVariance";
            this.sampVariance.TabStop = true;
            this.sampVariance.UseVisualStyleBackColor = true;
            this.sampVariance.CheckedChanged += new System.EventHandler(this.VarianceTypeCheckedChanged);
            this.sampVariance.MouseEnter += new System.EventHandler(this.OnMouseEnterSampleVariance);
            // 
            // populVariance
            // 
            resources.ApplyResources(this.populVariance, "populVariance");
            this.populVariance.Name = "populVariance";
            this.populVariance.TabStop = true;
            this.populVariance.UseVisualStyleBackColor = true;
            this.populVariance.CheckedChanged += new System.EventHandler(this.VarianceTypeCheckedChanged);
            this.populVariance.MouseEnter += new System.EventHandler(this.OnMouseEnterPoplationVariance);
            this.populVariance.MouseLeave += new System.EventHandler(this.OnMouseLeaveButtonArea);
            // 
            // UpDownForPrecision
            // 
            resources.ApplyResources(this.UpDownForPrecision, "UpDownForPrecision");
            this.UpDownForPrecision.Name = "UpDownForPrecision";
            this.UpDownForPrecision.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.UpDownForPrecision.ValueChanged += new System.EventHandler(this.OnUpDownValueChanged);
            // 
            // digitsForPrecision
            // 
            resources.ApplyResources(this.digitsForPrecision, "digitsForPrecision");
            this.digitsForPrecision.Name = "digitsForPrecision";
            // 
            // EQDPSParser
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.digitsForPrecision);
            this.Controls.Add(this.UpDownForPrecision);
            this.Controls.Add(this.groupBox1);
            this.Name = "EQDPSParser";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpDownForPrecision)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        #endregion

        #region Class Members       
        List<Tuple<Color, Regex, Action<Match>>> beforeLogLineRead;
        List<Tuple<Color, Regex, Action<Match>>> onLogLineRead;
        #region Non-Combat Regex Members
        Regex selfCheck;
        Regex possesive;
        #endregion
        string settingsFile;
        SettingsSerializer xmlSettings;
        readonly string PluginSettingsFileName = $"Config{Path.DirectorySeparatorChar}ACT_EverQuest_English_Parser.config.xml";
        #region UI Class Members
        TreeNode optionsNode = null;
        Label lblStatus;    // The status label that appears in ACT's Plugin tab
        private GroupBox groupBox1;
        private RadioButton varianceOff;
        private RadioButton sampVariance;
        private RadioButton populVariance;
        #endregion
        //NumericUpDown nud;
        private MasterSwing chilled;
        readonly static internal string[] SpecialAttack = new string[] {
                Properties.PluginRegex.CripplingBlow
                , Properties.PluginRegex.WildRampage
                , Properties.PluginRegex.Twincast
                , Properties.PluginRegex.Strikethrough
                , Properties.PluginRegex.Riposte
                , Properties.PluginRegex.Lucky
                , Properties.PluginRegex.Locked
                , Properties.PluginRegex.Flurry
                , Properties.PluginRegex.DoubleBowShot
                , Properties.PluginRegex.FinishingBlow
        };
        private NumericUpDown UpDownForPrecision;
        private Label digitsForPrecision;
        long precisionForDPS;
        static Regex regexSelf = new Regex(@"(you|your|it|her|him|them)(s|sel(f|ves))", RegexOptions.Compiled);
        readonly object precisionObject = new object();
        #endregion

        /// <summary>
        /// Constructor that calls initialize component
        /// </summary>
        public EQDPSParser()
        {
            InitializeComponent();
        }

        #region Plugin Class Interface Methods
        /// <summary>
        /// Called by the ACT program to start the plugin initialization
        /// Calls regex initialization methods and check for update methods
        /// assigns methods to the delegates in ActGlobals class
        /// </summary>
        /// <param name="pluginScreenSpace"></param>
        /// <param name="pluginStatusText"></param>
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            Localization.EQLocalization.EditLocalizations();
            settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, PluginSettingsFileName);
            lblStatus = pluginStatusText;   // Hand the status label's reference to our local var

            //pluginScreenSpace.Controls.Add(this);
            this.Dock = DockStyle.Fill;

            foreach (TreeNode tn in ActGlobals.oFormActMain.OptionsTreeView.Nodes)
            {
                if (tn.Text.Equals("Data Correction"))
                {
                    Action optionsControlSetsAdd = () =>
                    {
                        optionsNode = tn.Nodes.Add($"{Properties.PluginRegex.pluginName}");
                        // Register our user control(this) to our newly create node path.  All controls added to the list will be laid out left to right, top to bottom
                        ActGlobals.oFormActMain.OptionsControlSets.Add($@"Data Correction\{Properties.PluginRegex.pluginName}",
                            new List<Control> { this });
                        Label lblConfig = new Label
                        {
                            AutoSize = true,
                            Text = "Find the applicable options in the Options tab, Data Correction section."
                        };
                        //Image img = new Bitmap(Properties.PluginRegex.logo);

                        //lblConfig.Image = img;
                        lblConfig.ImageAlign = ContentAlignment.MiddleLeft;
                        lblConfig.TextAlign = ContentAlignment.MiddleCenter;

                        pluginScreenSpace.Controls.Add(lblConfig);
                    };

                    if (ActGlobals.oFormActMain.InvokeRequired)
                    {
                        ActGlobals.oFormActMain.Invoke(optionsControlSetsAdd);
                    }
                    else
                    {
                        optionsControlSetsAdd.Invoke();
                    }
                    break;
                }
            }

            xmlSettings = new SettingsSerializer(this); // Create a new settings serializer and pass it this instance
            LoadSettings();
            PopulateRegexNonCombat();
            PopulateRegexCombat();
            SetupEverQuestEnvironment();
            ActGlobals.oFormActMain.LogFileFilter = Properties.PluginRegex.logFilter;
            ActGlobals.oFormActMain.CharacterFileNameRegex = new Regex(Properties.PluginRegex.fileNameForLog, RegexOptions.Compiled);
            ActGlobals.oFormActMain.ZoneChangeRegex = new Regex($@"\[(?:.+)\] {Properties.PluginRegex.zoneChange}", RegexOptions.Compiled);
            ChangePluginStatusLabel($"{Properties.PluginRegex.pluginName} {Properties.PluginRegex.pluginStarted}");
            SetEventsForParsing();
        }

        private void SetEventsForParsing()
        {
            Action SetEvents = new Action(() =>
            {
                ActGlobals.oFormActMain.GetDateTimeFromLog += new FormActMain.DateTimeLogParser(ParseEQTimeStampFromLog);
                ActGlobals.oFormActMain.BeforeLogLineRead += new LogLineEventDelegate(FormActMain_BeforeLogLineRead);
                ActGlobals.oFormActMain.OnLogLineRead += new LogLineEventDelegate(FormActMain_OnLogLineRead);
            });
            if (ActGlobals.oFormActMain.InvokeRequired)
            {
                ActGlobals.oFormActMain.Invoke(SetEvents);
            }
            else
            {
                SetEvents.Invoke();
            }
        }

        /// <summary>
        /// Removes methods from the delegates assigned during initialization
        /// attemps to save the settings and then update the plugin dock with status of the exit
        /// </summary>
        public void DeInitPlugin()
        {
            Action removeOptionsFromMainForm = () =>
            {
                if (!(optionsNode == null))    // If we added our user control to the Options tab, remove it
                {
                    optionsNode.Remove();
                    ActGlobals.oFormActMain.OptionsControlSets.Remove($@"Data Correction\{Properties.PluginRegex.pluginName}");
                }
            };

            Action removeEventsFromFormMain = () =>
            {
                ActGlobals.oFormActMain.GetDateTimeFromLog -= ParseEQTimeStampFromLog;
                ActGlobals.oFormActMain.BeforeLogLineRead -= FormActMain_BeforeLogLineRead;
                ActGlobals.oFormActMain.OnLogLineRead -= FormActMain_OnLogLineRead;
            };

            Action runDeInitActions = () =>
            {
                ActGlobals.oFormActMain.Invoke(removeOptionsFromMainForm);
                ActGlobals.oFormActMain.Invoke(removeEventsFromFormMain);
            };

            if (ActGlobals.oFormActMain.InvokeRequired)
            {
                ActGlobals.oFormActMain.Invoke(runDeInitActions);
            }
            else
            {
                runDeInitActions();
            }
            SaveSettings();
            ChangePluginStatusLabel($"{Properties.PluginRegex.pluginName} {Properties.PluginRegex.pluginExited}");
        }
        #endregion

        #region Settings
        /// <summary>
        /// Loads settings file and attempts to assign values to the controls added in the method
        /// </summary>
        void LoadSettings()
        {
            Action loadSettings = new Action(() =>
            {
                xmlSettings.AddControlSetting(populVariance.Name, populVariance);
                xmlSettings.AddControlSetting(sampVariance.Name, sampVariance);
                xmlSettings.AddControlSetting(varianceOff.Name, varianceOff);
                xmlSettings.AddControlSetting(UpDownForPrecision.Name, UpDownForPrecision);

                if (File.Exists(settingsFile))
                {
                    using (FileStream settingsFileStream = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (XmlTextReader xReader = new XmlTextReader(settingsFileStream))
                    {
                        try
                        {
                            while (xReader.Read())
                            {
                                if (xReader.NodeType == XmlNodeType.Element)
                                {
                                    if (xReader.LocalName == "SettingsSerializer")
                                        xmlSettings.ImportFromXml(xReader);
                                }
                            }
                        }
                        catch (ArgumentNullException ex)
                        {
                            ChangePluginStatusLabel($"Argument Null for {ex.ParamName} with message: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            ChangePluginStatusLabel($"With message: {ex.Message}");
                        }
                    }
                    if (populVariance.Checked)
                        StatisticalProcessors.Variance.varianceCalc = StatisticalProcessors.Variance.populationVariance;
                    else if (sampVariance.Checked)
                        StatisticalProcessors.Variance.varianceCalc = StatisticalProcessors.Variance.sampleVariance;
                    else
                        StatisticalProcessors.Variance.varianceCalc = default;                 
                }
                else
                {
                    ChangePluginStatusLabel($"{settingsFile} does not exist and no settings were loaded, first time loading {Properties.PluginRegex.pluginName}?");
                    SaveSettings();
                    varianceOff.Checked = true;
                    StatisticalProcessors.Variance.varianceCalc = default;
                    VarianceTypeCheckedChanged(this, EventArgs.Empty);
                    OnUpDownValueChanged(this, EventArgs.Empty);
                }
            });

            if (this.InvokeRequired)
                this.Invoke(loadSettings);
            else
                loadSettings.Invoke();
        }

        /// <summary>
        /// Saves the settings file usually called when there is a change in the settings, 
        /// a settings file doesn't exist during LoadSettings method call, 
        /// or during the exit of the plugin
        /// </summary>
        void SaveSettings()
        {
            Action action = new Action(() =>
                {
                    try
                    {
                        using (FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8))
                            {
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
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ActGlobals.oFormActMain.WriteExceptionLog(ex, "Failed to save file in entirety");
                    }
                });
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action.Invoke();
        }
        #endregion
        /// <summary>
        /// Sets up the EverQuest environment with the standard information for the ACT application
        /// </summary>
        private void SetupEverQuestEnvironment()
        {
            CultureInfo usCulture = new CultureInfo(Properties.PluginRegex.cultureSetting);   // This is for SQL syntax; do not change
            //ActGlobals.blockIsHit = true;
            EncounterData.ColumnDefs.Clear();
            //Do not change the SqlDataName while doing localization
            EncounterData.ColumnDefs.Add("EncId", new EncounterData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.EncId; }));
            EncounterData.ColumnDefs.Add("Title", new EncounterData.ColumnDef("Title", true, "VARCHAR(64)", "Title", (Data) => { return Data.Title; }, (Data) => { return Data.Title; }));
            EncounterData.ColumnDefs.Add("StartTime", new EncounterData.ColumnDef("StartTime", true, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : String.Format("{0} {1}", Data.StartTime.ToShortDateString(), Data.StartTime.ToLongTimeString()); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            EncounterData.ColumnDefs.Add("EndTime", new EncounterData.ColumnDef("EndTime", true, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            EncounterData.ColumnDefs.Add("Duration", new EncounterData.ColumnDef("Duration", true, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }));
            EncounterData.ColumnDefs.Add("Damage", new EncounterData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(); }, (Data) => { return Data.Damage.ToString(); }));
            EncounterData.ColumnDefs.Add("EncDPS", new EncounterData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.DPS.ToString(); }, (Data) => { return Data.DPS.ToString(usCulture); }));
            EncounterData.ColumnDefs.Add("Zone", new EncounterData.ColumnDef("Zone", false, "VARCHAR(64)", "Zone", (Data) => { return Data.ZoneName; }, (Data) => { return Data.ZoneName; }));
            EncounterData.ColumnDefs.Add("Kills on", new EncounterData.ColumnDef("Kills", true, "INT", "Kills", (Data) => { return Data.AlliedKills.ToString(); }, (Data) => { return Data.AlliedKills.ToString(); }));
            EncounterData.ColumnDefs.Add("Deaths by", new EncounterData.ColumnDef("Deaths", true, "INT", "Deaths", (Data) => { return Data.AlliedDeaths.ToString(); }, (Data) => { return Data.AlliedDeaths.ToString(); }));

            EncounterData.ExportVariables.Clear();
            EncounterData.ExportVariables.Add("n", new EncounterData.TextExportFormatter("n", "New Line", "Formatting after this element will appear on a new line.", (Data, SelectiveAllies, Extra) => { return "\n"; }));
            EncounterData.ExportVariables.Add("t", new EncounterData.TextExportFormatter("t", "Tab Character", "Formatting after this element will appear in a relative column arrangement.  (The formatting example cannot display this properly)", (Data, SelectiveAllies, Extra) => { return "\t"; }));
            EncounterData.ExportVariables.Add("title", new EncounterData.TextExportFormatter("title", "Encounter Title", "The title of the completed encounter.  This may only be used in Allies formatting.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "title", Extra); }));
            EncounterData.ExportVariables.Add("duration", new EncounterData.TextExportFormatter("duration", "Duration", "The duration of the combatant or the duration of the encounter, displayed as mm:ss", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "duration", Extra); }));
            EncounterData.ExportVariables.Add("DURATION", new EncounterData.TextExportFormatter("DURATION", "Short Duration", "The duration of the combatant or encounter displayed in whole seconds.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DURATION", Extra); }));
            EncounterData.ExportVariables.Add("damage", new EncounterData.TextExportFormatter("damage", "Damage", "The amount of damage from auto-attack, spells, CAs, etc done to other combatants.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damage", Extra); }));
            EncounterData.ExportVariables.Add("damage-m", new EncounterData.TextExportFormatter("damage-m", "Damage M", "Damage divided by 1,000,000 (with two decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damage-m", Extra); }));
            EncounterData.ExportVariables.Add("damage-*", new EncounterData.TextExportFormatter("damage-*", "Damage w/suffix", "Damage divided 1/K/M/B/T/Q (with two decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damage-*", Extra); }));
            EncounterData.ExportVariables.Add("DAMAGE-k", new EncounterData.TextExportFormatter("DAMAGE-k", "Short Damage K", "Damage divided by 1,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DAMAGE-k", Extra); }));
            EncounterData.ExportVariables.Add("DAMAGE-m", new EncounterData.TextExportFormatter("DAMAGE-m", "Short Damage M", "Damage divided by 1,000,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DAMAGE-m", Extra); }));
            EncounterData.ExportVariables.Add("DAMAGE-b", new EncounterData.TextExportFormatter("DAMAGE-b", "Short Damage B", "Damage divided by 1,000,000,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DAMAGE-b", Extra); }));
            EncounterData.ExportVariables.Add("DAMAGE-*", new EncounterData.TextExportFormatter("DAMAGE-*", "Short Damage w/suffix", "Damage divided by 1/K/M/B/T/Q (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DAMAGE-*", Extra); }));
            EncounterData.ExportVariables.Add("dps", new EncounterData.TextExportFormatter("dps", "DPS", "The damage total of the combatant divided by their personal duration, formatted as 12.34", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "dps", Extra); }));
            EncounterData.ExportVariables.Add("dps-*", new EncounterData.TextExportFormatter("dps-*", "DPS w/suffix", "The damage total of the combatant divided by their personal duration, formatted as 12.34K", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "dps-*", Extra); }));
            EncounterData.ExportVariables.Add("DPS", new EncounterData.TextExportFormatter("DPS", "Short DPS", "The damage total of the combatatant divided by their personal duration, formatted as 12", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DPS", Extra); }));
            EncounterData.ExportVariables.Add("encdps", new EncounterData.TextExportFormatter("encdps", "Encounter DPS", "The damage total of the combatant divided by the duration of the encounter, formatted as 12.34 -- This is more commonly used than DPS", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "encdps", Extra); }));
            EncounterData.ExportVariables.Add("encdps-*", new EncounterData.TextExportFormatter("encdps-*", "Encounter DPS w/suffix", "The damage total of the combatant divided by the duration of the encounter, formatted as 12.34 -- This is more commonly used than DPS", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "encdps-*", Extra); }));
            EncounterData.ExportVariables.Add("ENCDPS", new EncounterData.TextExportFormatter("ENCDPS", "Short Encounter DPS", "The damage total of the combatant divided by the duration of the encounter, formatted as 12 -- This is more commonly used than DPS", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "ENCDPS", Extra); }));
            EncounterData.ExportVariables.Add("hits", new EncounterData.TextExportFormatter("hits", "Hits", "The number of attack attempts that produced damage.  IE a spell successfully doing damage.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "hits", Extra); }));
            EncounterData.ExportVariables.Add("crithits", new EncounterData.TextExportFormatter("crithits", "Critical Hit Count", "The number of damaging attacks that were critical.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "crithits", Extra); }));
            EncounterData.ExportVariables.Add("crithit%", new EncounterData.TextExportFormatter("crithit%", "Critical Hit Percentage", "The percentage of damaging attacks that were critical.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "crithit%", Extra); }));
            EncounterData.ExportVariables.Add("misses", new EncounterData.TextExportFormatter("misses", "Misses", "The number of auto-attacks or CAs that produced a miss message.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "misses", Extra); }));
            EncounterData.ExportVariables.Add("hitfailed", new EncounterData.TextExportFormatter("hitfailed", "Other Avoid", "Any type of failed attack that was not a miss.  This includes resists, reflects, blocks, dodging, etc.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "hitfailed", Extra); }));
            EncounterData.ExportVariables.Add("swings", new EncounterData.TextExportFormatter("swings", "Swings (Attacks)", "The number of attack attempts.  This includes any auto-attacks or abilities, also including resisted abilities that do no damage.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "swings", Extra); }));
            EncounterData.ExportVariables.Add("tohit", new EncounterData.TextExportFormatter("tohit", "To Hit %", "The percentage of hits to swings as 12.34", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "tohit", Extra); }));
            EncounterData.ExportVariables.Add("TOHIT", new EncounterData.TextExportFormatter("TOHIT", "Short To Hit %", "The percentage of hits to swings as 12", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "TOHIT", Extra); }));
            EncounterData.ExportVariables.Add("maxhit", new EncounterData.TextExportFormatter("maxhit", "Highest Hit", "The highest single damaging hit formatted as [Combatant-]SkillName-Damage#", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxhit", Extra); }));
            EncounterData.ExportVariables.Add("MAXHIT", new EncounterData.TextExportFormatter("MAXHIT", "Short Highest Hit", "The highest single damaging hit formatted as [Combatant-]Damage#", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHIT", Extra); }));
            EncounterData.ExportVariables.Add("maxhit-*", new EncounterData.TextExportFormatter("maxhit-*", "Highest Hit w/ suffix", "MaxHit divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxhit-*", Extra); }));
            EncounterData.ExportVariables.Add("MAXHIT-*", new EncounterData.TextExportFormatter("MAXHIT-*", "Short Highest Hit w/ suffix", "Short MaxHit divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHIT-*", Extra); }));
            EncounterData.ExportVariables.Add("healed", new EncounterData.TextExportFormatter("healed", "Healed", "The numerical total of all heals, wards or similar sourced from this combatant.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "healed", Extra); }));
            EncounterData.ExportVariables.Add("heals", new EncounterData.TextExportFormatter("heals", "Heal Count", "The total number of heals from this combatant.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "heals", Extra); }));
            EncounterData.ExportVariables.Add("critheals", new EncounterData.TextExportFormatter("critheals", "Critical Heal Count", "The number of heals that were critical.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "critheals", Extra); }));
            EncounterData.ExportVariables.Add("critheal%", new EncounterData.TextExportFormatter("critheal%", "Critical Heal Percentage", "The percentage of heals that were critical.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "critheal%", Extra); }));
            EncounterData.ExportVariables.Add("cures", new EncounterData.TextExportFormatter("cures", "Cure or Dispel Count", "The total number of times the combatant cured or dispelled", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "cures", Extra); }));
            EncounterData.ExportVariables.Add("maxheal", new EncounterData.TextExportFormatter("maxheal", "Highest Heal", "The highest single healing amount formatted as [Combatant-]SkillName-Healing#", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxheal", Extra); }));
            EncounterData.ExportVariables.Add("MAXHEAL", new EncounterData.TextExportFormatter("MAXHEAL", "Short Highest Heal", "The highest single healing amount formatted as [Combatant-]Healing#", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHEAL", Extra); }));
            EncounterData.ExportVariables.Add("maxhealward", new EncounterData.TextExportFormatter("maxhealward", "Highest Heal/Ward", "The highest single healing/warding amount formatted as [Combatant-]SkillName-Healing#", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxhealward", Extra); }));
            EncounterData.ExportVariables.Add("MAXHEALWARD", new EncounterData.TextExportFormatter("MAXHEALWARD", "Short Highest Heal/Ward", "The highest single healing/warding amount formatted as [Combatant-]Healing#", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHEALWARD", Extra); }));
            EncounterData.ExportVariables.Add("maxheal-*", new EncounterData.TextExportFormatter("maxheal-*", "Highest Heal w/ suffix", "Highest Heal divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxheal-*", Extra); }));
            EncounterData.ExportVariables.Add("MAXHEAL-*", new EncounterData.TextExportFormatter("MAXHEAL-*", "Short Highest Heal w/ suffix", "Short Highest Heal divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHEAL-*", Extra); }));
            EncounterData.ExportVariables.Add("maxhealward-*", new EncounterData.TextExportFormatter("maxhealward-*", "Highest Heal/Ward w/ suffix", "Highest Heal/Ward divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxhealward-*", Extra); }));
            EncounterData.ExportVariables.Add("MAXHEALWARD-*", new EncounterData.TextExportFormatter("MAXHEALWARD-*", "Short Highest Heal/Ward w/ suffix", "Short Highest Heal/Ward divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHEALWARD-*", Extra); }));
            EncounterData.ExportVariables.Add("damagetaken", new EncounterData.TextExportFormatter("damagetaken", "Damage Received", "The total amount of damage this combatant received.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damagetaken", Extra); }));
            EncounterData.ExportVariables.Add("damagetaken-*", new EncounterData.TextExportFormatter("damagetaken-*", "Damage Received w/suffix", "Damage Received divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damagetaken-*", Extra); }));
            EncounterData.ExportVariables.Add("healstaken", new EncounterData.TextExportFormatter("healstaken", "Healing Received", "The total amount of healing this combatant received.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "healstaken", Extra); }));
            EncounterData.ExportVariables.Add("healstaken-*", new EncounterData.TextExportFormatter("healstaken-*", "Healing Received w/suffix", "Healing Received divided by 1/K/M/B/T/Q", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "healstaken-*", Extra); }));
            EncounterData.ExportVariables.Add("kills", new EncounterData.TextExportFormatter("kills", "Killing Blows", "The total number of times this character landed a killing blow.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "kills", Extra); }));
            EncounterData.ExportVariables.Add("deaths", new EncounterData.TextExportFormatter("deaths", "Deaths", "The total number of times this character was killed by another.", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "deaths", Extra); }));

            CombatantData.ColumnDefs.Clear();
            CombatantData.ColumnDefs.Add("EncId", new CombatantData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.EncId; }, (Left, Right) => { return 0; }));
            CombatantData.ColumnDefs.Add("Ally", new CombatantData.ColumnDef("Ally", false, "CHAR(1)", "Ally", (Data) => { return Data.Parent.GetAllies().Contains(Data).ToString(); }, (Data) => { return Data.Parent.GetAllies().Contains(Data) ? "T" : "F"; }, (Left, Right) => { return Left.Parent.GetAllies().Contains(Left).CompareTo(Right.Parent.GetAllies().Contains(Right)); }));
            CombatantData.ColumnDefs.Add("Name", new CombatantData.ColumnDef("Name", true, "VARCHAR(64)", "Name", (Data) => { return Data.Name; }, (Data) => { return Data.Name; }, (Left, Right) => { return Left.Name.CompareTo(Right.Name); }));
            CombatantData.ColumnDefs.Add("StartTime", new CombatantData.ColumnDef("StartTime", true, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.StartTime.CompareTo(Right.StartTime); }));
            CombatantData.ColumnDefs.Add("EndTime", new CombatantData.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.EndTime.CompareTo(Right.EndTime); }));
            CombatantData.ColumnDefs.Add("Duration", new CombatantData.ColumnDef("Duration", true, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }, (Left, Right) => { return Left.Duration.CompareTo(Right.Duration); }));
            CombatantData.ColumnDefs.Add("Damage", new CombatantData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            CombatantData.ColumnDefs.Add("Damage%", new CombatantData.ColumnDef("Damage%", true, "VARCHAR(4)", "DamagePerc", (Data) => { return Data.DamagePercent; }, (Data) => { return Data.DamagePercent; }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            CombatantData.ColumnDefs.Add("Kills", new CombatantData.ColumnDef("Kills", false, "INT", "Kills", (Data) => { return Data.Kills.ToString(); }, (Data) => { return Data.Kills.ToString(); }, (Left, Right) => { return Left.Kills.CompareTo(Right.Kills); }));
            CombatantData.ColumnDefs.Add("Healed", new CombatantData.ColumnDef("Healed", false, "BIGINT", "Healed", (Data) => { return Data.Healed.ToString(); }, (Data) => { return Data.Healed.ToString(); }, (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); }));
            CombatantData.ColumnDefs.Add("Healed%", new CombatantData.ColumnDef("Healed%", false, "VARCHAR(4)", "HealedPerc", (Data) => { return Data.HealedPercent; }, (Data) => { return Data.HealedPercent; }, (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); }));
            CombatantData.ColumnDefs.Add("CritHeals", new CombatantData.ColumnDef("CritHeals", false, "INT", "CritHeals", (Data) => { return Data.CritHeals.ToString(); }, (Data) => { return Data.CritHeals.ToString(); }, (Left, Right) => { return Left.CritHeals.CompareTo(Right.CritHeals); }));
            CombatantData.ColumnDefs.Add("Heals", new CombatantData.ColumnDef("Heals", false, "INT", "Heals", (Data) => { return Data.Heals.ToString(); }, (Data) => { return Data.Heals.ToString(); }, (Left, Right) => { return Left.Heals.CompareTo(Right.Heals); }));
            CombatantData.ColumnDefs.Add("Cures", new CombatantData.ColumnDef("Cures", false, "INT", "CureDispels", (Data) => { return Data.CureDispels.ToString(); }, (Data) => { return Data.CureDispels.ToString(); }, (Left, Right) => { return Left.CureDispels.CompareTo(Right.CureDispels); }));
            CombatantData.ColumnDefs.Add("DPS", new CombatantData.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(); }, (Data) => { return Data.DPS.ToString(usCulture); }, (Left, Right) => { return Left.DPS.CompareTo(Right.DPS); }));
            CombatantData.ColumnDefs.Add("EncDPS", new CombatantData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(); }, (Data) => { return Data.EncDPS.ToString(usCulture); }, (Left, Right) => { return Left.EncDPS.CompareTo(Right.EncDPS); }));
            CombatantData.ColumnDefs.Add("Hits", new CombatantData.ColumnDef("Hits", false, "INT", "Hits", (Data) => { return Data.Hits.ToString(); }, (Data) => { return Data.Hits.ToString(); }, (Left, Right) => { return Left.Hits.CompareTo(Right.Hits); }));
            CombatantData.ColumnDefs.Add("CritHits", new CombatantData.ColumnDef("CritHits", false, "INT", "CritHits", (Data) => { return Data.CritHits.ToString(); }, (Data) => { return Data.CritHits.ToString(); }, (Left, Right) => { return Left.CritHits.CompareTo(Right.CritHits); }));
            CombatantData.ColumnDefs.Add("Avoids", new CombatantData.ColumnDef("Avoids", false, "INT", "Blocked", (Data) => { return Data.Blocked.ToString(); }, (Data) => { return Data.Blocked.ToString(); }, (Left, Right) => { return Left.Blocked.CompareTo(Right.Blocked); }));
            CombatantData.ColumnDefs.Add("Misses", new CombatantData.ColumnDef("Misses", false, "INT", "Misses", (Data) => { return Data.Misses.ToString(); }, (Data) => { return Data.Misses.ToString(); }, (Left, Right) => { return Left.Misses.CompareTo(Right.Misses); }));
            CombatantData.ColumnDefs.Add("Swings", new CombatantData.ColumnDef("Swings", false, "INT", "Swings", (Data) => { return Data.Swings.ToString(); }, (Data) => { return Data.Swings.ToString(); }, (Left, Right) => { return Left.Swings.CompareTo(Right.Swings); }));
            CombatantData.ColumnDefs.Add("HealingTaken", new CombatantData.ColumnDef("HealingTaken", false, "BIGINT", "HealsTaken", (Data) => { return Data.HealsTaken.ToString(); }, (Data) => { return Data.HealsTaken.ToString(); }, (Left, Right) => { return Left.HealsTaken.CompareTo(Right.HealsTaken); }));
            CombatantData.ColumnDefs.Add("DamageTaken", new CombatantData.ColumnDef("DamageTaken", true, "BIGINT", "DamageTaken", (Data) => { return Data.DamageTaken.ToString(); }, (Data) => { return Data.DamageTaken.ToString(); }, (Left, Right) => { return Left.DamageTaken.CompareTo(Right.DamageTaken); }));
            CombatantData.ColumnDefs.Add("Deaths", new CombatantData.ColumnDef("Deaths", true, "INT", "Deaths", (Data) => { return Data.Deaths.ToString(); }, (Data) => { return Data.Deaths.ToString(); }, (Left, Right) => { return Left.Deaths.CompareTo(Right.Deaths); }));
            CombatantData.ColumnDefs.Add("ToHit%", new CombatantData.ColumnDef("ToHit%", false, "FLOAT", "ToHit", (Data) => { return Data.ToHit.ToString(); }, (Data) => { return Data.ToHit.ToString(usCulture); }, (Left, Right) => { return Left.ToHit.CompareTo(Right.ToHit); }));
            CombatantData.ColumnDefs.Add("CritDam%", new CombatantData.ColumnDef("CritDam%", false, "VARCHAR(8)", "CritDamPerc", (Data) => { return Data.CritDamPerc.ToString("0'%"); }, (Data) => { return Data.CritDamPerc.ToString("0'%"); }, (Left, Right) => { return Left.CritDamPerc.CompareTo(Right.CritDamPerc); }));
            CombatantData.ColumnDefs.Add("CritHeal%", new CombatantData.ColumnDef("CritHeal%", false, "VARCHAR(8)", "CritHealPerc", (Data) => { return Data.CritHealPerc.ToString("0'%"); }, (Data) => { return Data.CritHealPerc.ToString("0'%"); }, (Left, Right) => { return Left.CritHealPerc.CompareTo(Right.CritHealPerc); }));

            CombatantData.ColumnDefs["Damage"].GetCellForeColor = (Data) => { return Color.DarkRed; };
            CombatantData.ColumnDefs["Damage%"].GetCellForeColor = (Data) => { return Color.DarkRed; };
            CombatantData.ColumnDefs["Healed"].GetCellForeColor = (Data) => { return Color.DarkBlue; };
            CombatantData.ColumnDefs["Healed%"].GetCellForeColor = (Data) => { return Color.DarkBlue; };
            CombatantData.ColumnDefs["DPS"].GetCellForeColor = (Data) => { return Color.DarkRed; };
            CombatantData.ColumnDefs["DamageTaken"].GetCellForeColor = (Data) => { return Color.DarkOrange; };

            CombatantData.OutgoingDamageTypeDataObjects = new Dictionary<string, CombatantData.DamageTypeDef>
        {
            {GenerateCombatDataStringOut(Properties.PluginRegex.AutoAttack), new CombatantData.DamageTypeDef(GenerateCombatDataStringOut(Properties.PluginRegex.AutoAttack), -1, Color.DarkGoldenrod)},
            {GenerateCombatDataStringOut(Properties.PluginRegex.SkillAbility), new CombatantData.DamageTypeDef(GenerateCombatDataStringOut(Properties.PluginRegex.SkillAbility), -1, Color.DarkOrange)},
            {"Outgoing Damage", new CombatantData.DamageTypeDef("Outgoing Damage", 0, Color.Orange)},
            {GenerateCombatDataStringOut(Properties.PluginRegex.DirectDamageSpell), new CombatantData.DamageTypeDef(GenerateCombatDataStringOut(Properties.PluginRegex.DirectDamageSpell), -1, Color.LightCyan) },
            {GenerateCombatDataStringOut(Properties.PluginRegex.Bane), new CombatantData.DamageTypeDef(GenerateCombatDataStringOut(Properties.PluginRegex.Bane), -1, Color.LightGreen) },
                {GenerateCombatDataStringOut(Properties.PluginRegex.DamageShield), new CombatantData.DamageTypeDef(GenerateCombatDataStringOut(Properties.PluginRegex.DamageShield), -1, Color.Brown) },
            {GenerateCombatDataStringOut(Properties.PluginRegex.InstantHealed), new CombatantData.DamageTypeDef(GenerateCombatDataStringOut(Properties.PluginRegex.InstantHealed), 1, Color.Blue)},
            {GenerateCombatDataStringOut(Properties.PluginRegex.HealOverTime), new CombatantData.DamageTypeDef(GenerateCombatDataStringOut(Properties.PluginRegex.HealOverTime), 1, Color.Blue)},
            {"All Outgoing (Ref)", new CombatantData.DamageTypeDef("All Outgoing (Ref)", 0, Color.Black)}
        };
            CombatantData.IncomingDamageTypeDataObjects = new Dictionary<string, CombatantData.DamageTypeDef>
        {
            {"Incoming Damage", new CombatantData.DamageTypeDef("Incoming Damage", -1, Color.Red)},
            {"Incoming NonMelee Damage", new CombatantData.DamageTypeDef("Incoming NonMelee Damage", -1 , Color.DarkRed) },
            {"Direct Damage Spell (Inc)", new CombatantData.DamageTypeDef("Direct Damage Spell (Inc)", -1, Color.LightCyan) },
            {"Damage Shield (Inc)", new CombatantData.DamageTypeDef("Damage Shield (Inc)", -1, Color.Brown) },
            {"Instant Heal (Inc)",new CombatantData.DamageTypeDef("Instant Heal (Inc)", 1, Color.LimeGreen)},
            {"Heal Over Time (Inc)",new CombatantData.DamageTypeDef("Heal Over Time (Inc)", 1, Color.LimeGreen)},
            {"All Incoming (Ref)",new CombatantData.DamageTypeDef("All Incoming (Ref)", 0, Color.Black)}
        };
            CombatantData.SwingTypeToDamageTypeDataLinksOutgoing = new SortedDictionary<int, List<string>>
        {
            {(int)EQSwingType.Melee, new List<string> { "Auto-Attack (Out)" } },
            {(int)EQSwingType.NonMelee, new List<string> { "Skill/Ability (Out)" } },
            {(int)EQSwingType.DirectDamageSpell, new List<string> { "Direct Damage Spell (Out)"} },
                {(int)EQSwingType.Bane, new List<string>{"Bane (Out)"} },
            {(int)EQSwingType.InstantHealing, new List<string> { "Instant Heal (Out)" } },
            {(int)EQSwingType.HealingOverTime, new List<string> { "Heal Over Time (Out)" } },
                {(int)EQSwingType.DamageShield, new List<string> { "Damage Shield (Out)"} },
        };
            CombatantData.SwingTypeToDamageTypeDataLinksIncoming = new SortedDictionary<int, List<string>>
        {
            {(int)EQSwingType.Melee, new List<string> { "Incoming Damage" } },
            {(int)EQSwingType.NonMelee, new List<string> { "Incoming NonMelee Damage", "Incoming Damage" } },
            {(int)EQSwingType.DirectDamageSpell, new List<string> { "Direct Damage Spell (Inc)", "Incoming Damage" } },
            {(int)EQSwingType.InstantHealing, new List<string> { "Instant Heal (Inc)" } },
            {(int)EQSwingType.SpellOverTime, new List<string> {"Damage Over Time Spell (Inc)", "Incoming Damage" } },
            {(int)EQSwingType.HealingOverTime, new List<string> { "Heal Over Time (Inc)" } },
                {(int)EQSwingType.DamageShield, new List<string> { "Damage Shield (Inc)"} },
        };

            CombatantData.DamageSwingTypes = new List<int>()
            {
                (int)EQSwingType.Bane,
                (int)EQSwingType.DamageShield,
                (int)EQSwingType.DirectDamageSpell,
                (int)EQSwingType.SpellOverTime,
                (int)EQSwingType.Melee,
                (int)EQSwingType.NonMelee
            };

            CombatantData.HealingSwingTypes = new List<int>()
            {
                (int)EQSwingType.HealingOverTime,
                (int)EQSwingType.InstantHealing
            };

            CombatantData.ExportVariables.Clear();
            CombatantData.ExportVariables.Add("n", new CombatantData.TextExportFormatter("n", "New Line", "Formatting after this element will appear on a new line.", (Data, Extra) => { return "\n"; }));
            CombatantData.ExportVariables.Add("t", new CombatantData.TextExportFormatter("t", "Tab Character", "Formatting after this element will appear in a relative column arrangement.  (The formatting example cannot display this properly)", (Data, Extra) => { return "\t"; }));
            CombatantData.ExportVariables.Add("name", new CombatantData.TextExportFormatter("name", "Name", "The combatant's name.", (Data, Extra) => { return Data.Name; }));
            CombatantData.ExportVariables.Add("NAME", new CombatantData.TextExportFormatter("NAME", "Short Name", "The combatant's name shortened to a number of characters after a colon, like: \"NAME:5\"", (Data, Extra) => { return NameFormatChange(Data, Int32.Parse(Extra)); }));
            CombatantData.ExportVariables.Add("duration", new CombatantData.TextExportFormatter("duration", "Duration", "The duration of the combatant or the duration of the encounter, displayed as mm:ss", (Data, Extra) => { return CombatantFormatSwitch(Data, "duration", Extra); }));
            CombatantData.ExportVariables.Add("DURATION", new CombatantData.TextExportFormatter("DURATION", "Short Duration", "The duration of the combatant or encounter displayed in whole seconds.", (Data, Extra) => { return CombatantFormatSwitch(Data, "DURATION", Extra); }));
            CombatantData.ExportVariables.Add("damage", new CombatantData.TextExportFormatter("damage", "Damage", "The amount of damage from auto-attack, spells, CAs, etc done to other combatants.", (Data, Extra) => { return CombatantFormatSwitch(Data, "damage", Extra); }));
            CombatantData.ExportVariables.Add("damage%", new CombatantData.TextExportFormatter("damage%", "Damage %", "This value represents the percent share of all damage done by allies in this encounter.", (Data, Extra) => { return CombatantFormatSwitch(Data, "damage%", Extra); }));
            CombatantData.ExportVariables.Add("dps", new CombatantData.TextExportFormatter("dps", "DPS", "The damage total of the combatant divided by their personal duration, formatted as 12.34", (Data, Extra) => { return CombatantFormatSwitch(Data, "dps", Extra); }));
            CombatantData.ExportVariables.Add("DPS", new CombatantData.TextExportFormatter("DPS", "Short DPS", "The damage total of the combatatant divided by their personal duration, formatted as 12K", (Data, Extra) => { return CombatantFormatSwitch(Data, "DPS", Extra); }));
            CombatantData.ExportVariables.Add("PetDPS", new CombatantData.TextExportFormatter("PetDPS", "Pet DPS", "The damage total of the combatant's pets divided by their personal duration", (Data, Extra) => { return CombatantFormatSwitch(Data, "PetDPS", Extra); }));
            CombatantData.ExportVariables.Add("encdps", new CombatantData.TextExportFormatter("encdps", "Encounter DPS", "The damage total of the combatant divided by the duration of the encounter, formatted as 12.34 -- This is more commonly used than DPS", (Data, Extra) => { return CombatantFormatSwitch(Data, "encdps", Extra); }));
            CombatantData.ExportVariables.Add("ENCDPS", new CombatantData.TextExportFormatter("ENCDPS", "Short Encounter DPS", "The damage total of the combatant divided by the duration of the encounter, formatted as 12 -- This is more commonly used than DPS", (Data, Extra) => { return CombatantFormatSwitch(Data, "ENCDPS", Extra); }));
            CombatantData.ExportVariables.Add("encdps-*", new CombatantData.TextExportFormatter("encdps-*", "Encounter DPS", "The damage total of the combatant divided by the duration of the encounter, formatted as 12.34 -- This is more commonly used than DPS", (Data, Extra) => { return CombatantFormatSwitch(Data, "encdps-*", Extra); }));
            CombatantData.ExportVariables.Add("ENCPetDPS", new CombatantData.TextExportFormatter("ENCPetDPS", "Encounter Pet DPS", "Pet DPS for the combatant dividied by the duration of the encounter", (Data, Extra) => { return CombatantFormatSwitch(Data, "ENCPetDPS", Extra); }));
            CombatantData.ExportVariables.Add("hits", new CombatantData.TextExportFormatter("hits", "Hits", "The number of attack attempts that produced damage.  IE a spell successfully doing damage.", (Data, Extra) => { return CombatantFormatSwitch(Data, "hits", Extra); }));
            CombatantData.ExportVariables.Add("crithits", new CombatantData.TextExportFormatter("crithits", "Critical Hit Count", "The number of damaging attacks that were critical.", (Data, Extra) => { return CombatantFormatSwitch(Data, "crithits", Extra); }));
            CombatantData.ExportVariables.Add("crithit%", new CombatantData.TextExportFormatter("crithit%", "Critical Hit Percentage", "The percentage of damaging attacks that were critical.", (Data, Extra) => { return CombatantFormatSwitch(Data, "crithit%", Extra); }));
            //CombatantData.ExportVariables.Add("crittypes", new CombatantData.TextExportFormatter("crittypes", "Critical Types", "Distribution of Critical Types  (Normal|Legendary|Fabled|Mythical)", (Data, Extra) => { return CombatantFormatSwitch(Data, "crittypes", Extra, Extra); }));
            CombatantData.ExportVariables.Add("misses", new CombatantData.TextExportFormatter("misses", "Misses", "The number of auto-attacks or CAs that produced a miss message.", (Data, Extra) => { return CombatantFormatSwitch(Data, "misses", Extra); }));
            CombatantData.ExportVariables.Add("hitfailed", new CombatantData.TextExportFormatter("hitfailed", "Other Avoid", "Any type of failed attack that was not a miss.  This includes resists, reflects, blocks, dodging, etc.", (Data, Extra) => { return CombatantFormatSwitch(Data, "hitfailed", Extra); }));
            CombatantData.ExportVariables.Add("swings", new CombatantData.TextExportFormatter("swings", "Swings (Attacks)", "The number of attack attempts.  This includes any auto-attacks or abilities, also including resisted abilities that do no damage.", (Data, Extra) => { return CombatantFormatSwitch(Data, "swings", Extra); }));
            CombatantData.ExportVariables.Add("tohit", new CombatantData.TextExportFormatter("tohit", "To Hit %", "The percentage of hits to swings as 12.34", (Data, Extra) => { return CombatantFormatSwitch(Data, "tohit", Extra); }));
            CombatantData.ExportVariables.Add("TOHIT", new CombatantData.TextExportFormatter("TOHIT", "Short To Hit %", "The percentage of hits to swings as 12", (Data, Extra) => { return CombatantFormatSwitch(Data, "TOHIT", Extra); }));
            CombatantData.ExportVariables.Add("maxhit", new CombatantData.TextExportFormatter("maxhit", "Highest Hit", "The highest single damaging hit formatted as [Combatant-]SkillName-Damage#", (Data, Extra) => { return CombatantFormatSwitch(Data, "maxhit", Extra); }));
            CombatantData.ExportVariables.Add("MAXHIT", new CombatantData.TextExportFormatter("MAXHIT", "Short Highest Hit", "The highest single damaging hit formatted as [Combatant-]Damage#", (Data, Extra) => { return CombatantFormatSwitch(Data, "MAXHIT", Extra); }));
            CombatantData.ExportVariables.Add("maxhit-*", new CombatantData.TextExportFormatter("maxhit-*", "Highest Hit w/ suffix", "MaxHit divided by 1/K/M/B/T/Q", (Data, Extra) => { return CombatantFormatSwitch(Data, "maxhit-*", Extra); }));
            CombatantData.ExportVariables.Add("MAXHIT-*", new CombatantData.TextExportFormatter("MAXHIT-*", "Short Highest Hit w/ suffix", "Short MaxHit divided by 1/K/M/B/T/Q", (Data, Extra) => { return CombatantFormatSwitch(Data, "MAXHIT-*", Extra); }));
            CombatantData.ExportVariables.Add("healed", new CombatantData.TextExportFormatter("healed", "Healed", "The numerical total of all heals, wards or similar sourced from this combatant.", (Data, Extra) => { return CombatantFormatSwitch(Data, "healed", Extra); }));
            CombatantData.ExportVariables.Add("healed%", new CombatantData.TextExportFormatter("healed%", "Healed %", "This value represents the percent share of all healing done by allies in this encounter.", (Data, Extra) => { return CombatantFormatSwitch(Data, "healed%", Extra); }));
            CombatantData.ExportVariables.Add("enchps", new CombatantData.TextExportFormatter("enchps", "Encounter HPS", "The healing total of the combatant divided by the duration of the encounter, formatted as 12.34", (Data, Extra) => { return CombatantFormatSwitch(Data, "enchps", Extra); }));
            CombatantData.ExportVariables.Add("ENCHPS", new CombatantData.TextExportFormatter("ENCHPS", "Short Encounter HPS", "The healing total of the combatant divided by the duration of the encounter, formatted as 12", (Data, Extra) => { return CombatantFormatSwitch(Data, "ENCHPS", Extra); }));
            CombatantData.ExportVariables.Add("critheals", new CombatantData.TextExportFormatter("critheals", "Critical Heal Count", "The number of heals that were critical.", (Data, Extra) => { return CombatantFormatSwitch(Data, "critheals", Extra); }));
            CombatantData.ExportVariables.Add("critheal%", new CombatantData.TextExportFormatter("critheal%", "Critical Heal Percentage", "The percentage of heals that were critical.", (Data, Extra) => { return CombatantFormatSwitch(Data, "critheal%", Extra); }));
            CombatantData.ExportVariables.Add("heals", new CombatantData.TextExportFormatter("heals", "Heal Count", "The total number of heals from this combatant.", (Data, Extra) => { return CombatantFormatSwitch(Data, "heals", Extra); }));
            CombatantData.ExportVariables.Add("cures", new CombatantData.TextExportFormatter("cures", "Cure or Dispel Count", "The total number of times the combatant cured or dispelled", (Data, Extra) => { return CombatantFormatSwitch(Data, "cures", Extra); }));
            CombatantData.ExportVariables.Add("maxheal", new CombatantData.TextExportFormatter("maxheal", "Highest Heal", "The highest single healing amount formatted as [Combatant-]SkillName-Healing#", (Data, Extra) => { return CombatantFormatSwitch(Data, "maxheal", Extra); }));
            CombatantData.ExportVariables.Add("MAXHEAL", new CombatantData.TextExportFormatter("MAXHEAL", "Short Highest Heal", "The highest single healing amount formatted as [Combatant-]Healing#", (Data, Extra) => { return CombatantFormatSwitch(Data, "MAXHEAL", Extra); }));
            CombatantData.ExportVariables.Add("maxhealward", new CombatantData.TextExportFormatter("maxhealward", "Highest Heal/Ward", "The highest single healing/warding amount formatted as [Combatant-]SkillName-Healing#", (Data, Extra) => { return CombatantFormatSwitch(Data, "maxhealward", Extra); }));
            CombatantData.ExportVariables.Add("MAXHEALWARD", new CombatantData.TextExportFormatter("MAXHEALWARD", "Short Highest Heal/Ward", "The highest single healing/warding amount formatted as [Combatant-]Healing#", (Data, Extra) => { return CombatantFormatSwitch(Data, "MAXHEALWARD", Extra); }));
            CombatantData.ExportVariables.Add("damagetaken", new CombatantData.TextExportFormatter("damagetaken", "Damage Received", "The total amount of damage this combatant received.", (Data, Extra) => { return CombatantFormatSwitch(Data, "damagetaken", Extra); }));
            CombatantData.ExportVariables.Add("damagetaken-*", new CombatantData.TextExportFormatter("damagetaken-*", "Damage Received w/suffix", "Damage Received divided by 1/K/M/B/T/Q", (Data, Extra) => { return CombatantFormatSwitch(Data, "damagetaken-*", Extra); }));
            CombatantData.ExportVariables.Add("healstaken", new CombatantData.TextExportFormatter("healstaken", "Healing Received", "The total amount of healing this combatant received.", (Data, Extra) => { return CombatantFormatSwitch(Data, "healstaken", Extra); }));
            CombatantData.ExportVariables.Add("kills", new CombatantData.TextExportFormatter("kills", "Killing Blows", "The total number of times this character landed a killing blow.", (Data, Extra) => { return CombatantFormatSwitch(Data, "kills", Extra); }));
            CombatantData.ExportVariables.Add("deaths", new CombatantData.TextExportFormatter("deaths", "Deaths", "The total number of times this character was killed by another.", (Data, Extra) => { return CombatantFormatSwitch(Data, "deaths", Extra); }));
            CombatantData.ExportVariables.Add("NAME3", new CombatantData.TextExportFormatter("NAME3", "Name (3 chars)", "The combatant's name, up to 3 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 3); }));
            CombatantData.ExportVariables.Add("NAME4", new CombatantData.TextExportFormatter("NAME4", "Name (4 chars)", "The combatant's name, up to 4 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 4); }));
            CombatantData.ExportVariables.Add("NAME5", new CombatantData.TextExportFormatter("NAME5", "Name (5 chars)", "The combatant's name, up to 5 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 5); }));
            CombatantData.ExportVariables.Add("NAME6", new CombatantData.TextExportFormatter("NAME6", "Name (6 chars)", "The combatant's name, up to 6 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 6); }));
            CombatantData.ExportVariables.Add("NAME7", new CombatantData.TextExportFormatter("NAME7", "Name (7 chars)", "The combatant's name, up to 7 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 7); }));
            CombatantData.ExportVariables.Add("NAME8", new CombatantData.TextExportFormatter("NAME8", "Name (8 chars)", "The combatant's name, up to 8 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 8); }));
            CombatantData.ExportVariables.Add("NAME9", new CombatantData.TextExportFormatter("NAME9", "Name (9 chars)", "The combatant's name, up to 9 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 9); }));
            CombatantData.ExportVariables.Add("NAME10", new CombatantData.TextExportFormatter("NAME10", "Name (10 chars)", "The combatant's name, up to 10 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 10); }));
            CombatantData.ExportVariables.Add("NAME11", new CombatantData.TextExportFormatter("NAME11", "Name (11 chars)", "The combatant's name, up to 11 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 11); }));
            CombatantData.ExportVariables.Add("NAME12", new CombatantData.TextExportFormatter("NAME12", "Name (12 chars)", "The combatant's name, up to 12 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 12); }));
            CombatantData.ExportVariables.Add("NAME13", new CombatantData.TextExportFormatter("NAME13", "Name (13 chars)", "The combatant's name, up to 13 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 13); }));
            CombatantData.ExportVariables.Add("NAME14", new CombatantData.TextExportFormatter("NAME14", "Name (14 chars)", "The combatant's name, up to 14 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 14); }));
            CombatantData.ExportVariables.Add("NAME15", new CombatantData.TextExportFormatter("NAME15", "Name (15 chars)", "The combatant's name, up to 15 characters will be displayed.", (Data, Extra) => { return NameFormatChange(Data, 15); }));

            DamageTypeData.ColumnDefs.Clear();
            DamageTypeData.ColumnDefs.Add("EncId", new DamageTypeData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.Parent.EncId; }));
            DamageTypeData.ColumnDefs.Add("Combatant", new DamageTypeData.ColumnDef("Combatant", false, "VARCHAR(64)", "Combatant", (Data) => { return Data.Parent.Name; }, (Data) => { return Data.Parent.Name; }));
            DamageTypeData.ColumnDefs.Add("Type", new DamageTypeData.ColumnDef("Type", true, "VARCHAR(64)", "Type", (Data) => { return Data.Type; }, (Data) => { return Data.Type; }));
            DamageTypeData.ColumnDefs.Add("StartTime", new DamageTypeData.ColumnDef("StartTime", false, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            DamageTypeData.ColumnDefs.Add("EndTime", new DamageTypeData.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            DamageTypeData.ColumnDefs.Add("Duration", new DamageTypeData.ColumnDef("Duration", false, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }));
            DamageTypeData.ColumnDefs.Add("Damage", new DamageTypeData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(); }, (Data) => { return Data.Damage.ToString(); }));
            DamageTypeData.ColumnDefs.Add("EncDPS", new DamageTypeData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(); }, (Data) => { return Data.EncDPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("CharDPS", new DamageTypeData.ColumnDef("CharDPS", false, "DOUBLE", "CharDPS", (Data) => { return Data.CharDPS.ToString(); }, (Data) => { return Data.CharDPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("DPS", new DamageTypeData.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(); }, (Data) => { return Data.DPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("Average", new DamageTypeData.ColumnDef("Average", true, "DOUBLE", "Average", (Data) => { return Data.Average.ToString(); }, (Data) => { return Data.Average.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Median", new DamageTypeData.ColumnDef("Median", false, "BIGINT", "Median", (Data) => { return Data.Median.ToString(); }, (Data) => { return Data.Median.ToString(); }));
            DamageTypeData.ColumnDefs.Add("MinHit", new DamageTypeData.ColumnDef("MinHit", true, "BIGINT", "MinHit", (Data) => { return Data.Swings == 0 ? String.Empty : Data.MinHit.ToString(); }, (Data) => { return Data.Swings == 0 ? String.Empty : Data.MinHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("MaxHit", new DamageTypeData.ColumnDef("MaxHit", true, "BIGINT", "MaxHit", (Data) => { return Data.Swings == 0 ? String.Empty : Data.MaxHit.ToString(); }, (Data) => { return Data.Swings == 0 ? String.Empty : Data.MaxHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Hits", new DamageTypeData.ColumnDef("Hits", true, "INT", "Hits", (Data) => { return Data.Hits.ToString(); }, (Data) => { return Data.Hits.ToString(); }));
            DamageTypeData.ColumnDefs.Add("CritHits", new DamageTypeData.ColumnDef("CritHits", false, "INT", "CritHits", (Data) => { return Data.CritHits.ToString(); }, (Data) => { return Data.CritHits.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Avoids", new DamageTypeData.ColumnDef("Avoids", false, "INT", "Blocked", (Data) => { return Data.Blocked.ToString(); }, (Data) => { return Data.Blocked.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Misses", new DamageTypeData.ColumnDef("Misses", false, "INT", "Misses", (Data) => { return Data.Misses.ToString(); }, (Data) => { return Data.Misses.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Swings", new DamageTypeData.ColumnDef("Swings", true, "INT", "Swings", (Data) => { return Data.Swings.ToString(); }, (Data) => { return Data.Swings.ToString(); }));
            DamageTypeData.ColumnDefs.Add("ToHit", new DamageTypeData.ColumnDef("ToHit", false, "FLOAT", "ToHit", (Data) => { return Data.ToHit.ToString(); }, (Data) => { return Data.ToHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("AvgDelay", new DamageTypeData.ColumnDef("AvgDelay", false, "FLOAT", "AverageDelay", (Data) => { return Data.AverageDelay.ToString(); }, (Data) => { return Data.AverageDelay.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Crit%", new DamageTypeData.ColumnDef("Crit%", false, "VARCHAR(8)", "CritPerc", (Data) => { return Data.CritPerc.ToString("0'%"); }, (Data) => { return Data.CritPerc.ToString("0'%"); }));

            AttackType.ColumnDefs.Clear();
            AttackType.ColumnDefs.Add("EncId", new AttackType.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.Parent.Parent.EncId; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("Attacker", new AttackType.ColumnDef("Attacker", false, "VARCHAR(64)", "Attacker", (Data) => { return Data.Parent.Outgoing ? Data.Parent.Parent.Name : string.Empty; }, (Data) => { return Data.Parent.Outgoing ? Data.Parent.Parent.Name : string.Empty; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("Victim", new AttackType.ColumnDef("Victim", false, "VARCHAR(64)", "Victim", (Data) => { return Data.Parent.Outgoing ? string.Empty : Data.Parent.Parent.Name; }, (Data) => { return Data.Parent.Outgoing ? string.Empty : Data.Parent.Parent.Name; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("SwingType", new AttackType.ColumnDef("SwingType", false, "TINYINT", "SwingType", GetAttackTypeSwingType, GetAttackTypeSwingType, (Left, Right) => { return GetAttackTypeSwingType(Left).CompareTo(GetAttackTypeSwingType(Right)); }));
            AttackType.ColumnDefs.Add("Type", new AttackType.ColumnDef("Type", true, "VARCHAR(64)", "Type",
            (Data) => Data.GetAttackTypeListWithDnumListExcept(new List<Dnum> { Dnum.Death, Dnum.ThreatPosition }).Type,
            (Data) => Data.GetAttackTypeListWithDnumListExcept(new List<Dnum> { Dnum.Death, Dnum.ThreatPosition }).Type,
            (Left, Right) =>
                Left.GetAttackTypeListWithDnumListExcept(new List<Dnum> { Dnum.Death, Dnum.ThreatPosition }).Type.CompareTo(Right.GetAttackTypeListWithDnumListExcept(new List<Dnum> { Dnum.Death, Dnum.ThreatPosition }).Type)
            ));
            AttackType.ColumnDefs.Add("StartTime", new AttackType.ColumnDef("StartTime", false, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.StartTime.CompareTo(Right.StartTime); }));
            AttackType.ColumnDefs.Add("EndTime", new AttackType.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.EndTime.CompareTo(Right.EndTime); }));
            AttackType.ColumnDefs.Add("Duration", new AttackType.ColumnDef("Duration", false, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }, (Left, Right) => { return Left.Duration.CompareTo(Right.Duration); }));
            AttackType.ColumnDefs.Add("Damage", new AttackType.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            AttackType.ColumnDefs.Add("EncDPS", new AttackType.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(); }, (Data) => { return Data.EncDPS.ToString(usCulture); }, (Left, Right) => { return Left.EncDPS.CompareTo(Right.EncDPS); }));
            AttackType.ColumnDefs.Add("CharDPS", new AttackType.ColumnDef("CharDPS", false, "DOUBLE", "CharDPS", (Data) => { return Data.CharDPS.ToString(); }, (Data) => { return Data.CharDPS.ToString(usCulture); }, (Left, Right) => { return Left.CharDPS.CompareTo(Right.CharDPS); }));
            AttackType.ColumnDefs.Add("DPS", new AttackType.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(); }, (Data) => { return Data.DPS.ToString(usCulture); }, (Left, Right) => { return Left.DPS.CompareTo(Right.DPS); }));
            AttackType.ColumnDefs.Add("Average", new AttackType.ColumnDef("Average", true, "DOUBLE", "Average", (Data) => { return Data.Average.ToString(); }, (Data) => { return Data.Average.ToString(); }, (Left, Right) => { return Left.Average.CompareTo(Right.Average); }));
            AttackType.ColumnDefs.Add("Median", new AttackType.ColumnDef("Median", true, "BIGINT", "Median", (Data) => { return Data.Median.ToString(); }, (Data) => { return Data.Median.ToString(); }, (Left, Right) => { return Left.Median.CompareTo(Right.Median); }));
            AttackType.ColumnDefs.Add("StdDev", new AttackType.ColumnDef("StdDev", true, "DOUBLE", "StdDev", (Data) => { return Math.Sqrt(AttackTypeGetVariance(Data)).ToString(); }, (Data) => { return Math.Sqrt(AttackTypeGetVariance(Data)).ToString(); }, (Left, Right) => { return Math.Sqrt(AttackTypeGetVariance(Left)).CompareTo(Math.Sqrt(AttackTypeGetVariance(Right))); }));
            AttackType.ColumnDefs.Add("Max", new AttackType.ColumnDef("Max", true, "BIGINT", "Max", (Data) => { return Data.MaxHit.ToString(); }, (Data) => { return Data.MaxHit.ToString(); }, (Left, Right) => { return Left.MaxHit.CompareTo(Right.MaxHit); }));
            AttackType.ColumnDefs.Add("Min", new AttackType.ColumnDef("Min", true, "BIGINT", "Min", (Data) => { return Data.MinHit.ToString(); }, (Data) => { return Data.MinHit.ToString(); }, (Left, Right) => { return Left.MinHit.CompareTo(Right.MinHit); }));
            SetupCritPercentage(SpecialAttack);


            MasterSwing.ColumnDefs.Clear();
            MasterSwing.ColumnDefs.Add("EncId", new MasterSwing.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.ParentEncounter.EncId; }, (Left, Right) => { return 0; }));
            MasterSwing.ColumnDefs.Add("Time", new MasterSwing.ColumnDef("Time", true, "TIMESTAMP", "STime", (Data) => { return Data.Time.ToString("T"); }, (Data) => { return Data.Time.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.Time.CompareTo(Right.Time); }));
            MasterSwing.ColumnDefs.Add("RelativeTime", new MasterSwing.ColumnDef("RelativeTime", true, "FLOAT", "RelativeTime", (Data) => { return !(Data.ParentEncounter == null) ? (Data.Time - Data.ParentEncounter.StartTime).ToString("g") : String.Empty; }, (Data) => { return !(Data.ParentEncounter == null) ? (Data.Time - Data.ParentEncounter.StartTime).TotalSeconds.ToString(usCulture) : String.Empty; }, (Left, Right) => { return Left.Time.CompareTo(Right.Time); }));
            MasterSwing.ColumnDefs.Add("Attacker", new MasterSwing.ColumnDef("Attacker", true, "VARCHAR(64)", "Attacker", (Data) => { return Data.Attacker; }, (Data) => { return Data.Attacker; }, (Left, Right) => { return Left.Attacker.CompareTo(Right.Attacker); }));
            MasterSwing.ColumnDefs.Add("SwingType", new MasterSwing.ColumnDef("SwingType", false, "TINYINT", "SwingType", (Data) => { return Data.SwingType.ToString(); }, (Data) => { return Data.SwingType.ToString(); }, (Left, Right) => { return Left.SwingType.CompareTo(Right.SwingType); }));
            MasterSwing.ColumnDefs.Add("AttackType", new MasterSwing.ColumnDef("AttackType", true, "VARCHAR(64)", "AttackType", (Data) => { return Data.AttackType; }, (Data) => { return Data.AttackType; }, (Left, Right) => { return Left.AttackType.CompareTo(Right.AttackType); }));
            MasterSwing.ColumnDefs.Add("DamageType", new MasterSwing.ColumnDef("DamageType", true, "VARCHAR(64)", "DamageType", (Data) => { return Data.DamageType; }, (Data) => { return Data.DamageType; }, (Left, Right) => { return Left.DamageType.CompareTo(Right.DamageType); }));
            MasterSwing.ColumnDefs.Add("Victim", new MasterSwing.ColumnDef("Victim", true, "VARCHAR(64)", "Victim", (Data) => { return Data.Victim; }, (Data) => { return Data.Victim; }, (Left, Right) => { return Left.Victim.CompareTo(Right.Victim); }));

            MasterSwing.ColumnDefs.Add("DamageNum", new MasterSwing.ColumnDef("DamageNum", false, "BIGINT", "DamageNum", (Data) =>
            {
                if (Data.Damage.Number < 0)
                    return String.Empty;
                else
                    return ((long)Data.Damage).ToString();
            }
            ,
            (Data) =>
            {
                if (Data.Damage.Number < 0)
                    return String.Empty;
                else
                    return ((long)Data.Damage).ToString();
            },
            (Left, Right) =>
            {
                return Left.Damage.CompareTo(Right.Damage);
            }
            ));
            MasterSwing.ColumnDefs.Add("Damage", new MasterSwing.ColumnDef("Damage", true, "VARCHAR(128)", "DamageString", (Data) => { return Data.Damage.ToString(); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            MasterSwing.ColumnDefs.Add("Critical", 
                new MasterSwing.ColumnDef("Critical", false, "BOOLEAN", "Critical", 
                (Data) => { return String.Empty; }, 
                (Data) => { return String.Empty.ToString(usCulture)[0].ToString(); }, (Left, Right) => { return Left.Critical.CompareTo(Right.Critical); })
                {
                    GetCellBackColor = (Data) =>
                    {
                        return Data.Critical ? Color.Black : Color.White;
                    }
                });

            MasterSwing.ColumnDefs.Add("Overheal", new MasterSwing.ColumnDef("Overheal", true, "BIGINT", "Overheal", (Data) => { return Data.Tags.ContainsKey("overheal") ? ((long)Data.Tags["overheal"]).ToString() : string.Empty; }, (Data) => { return Data.Tags.ContainsKey("overheal") ? ((long)Data.Tags["overheal"]).ToString() : string.Empty; }, (Left, Right) =>
            {
                return (Left.Tags.ContainsKey("overheal") && Right.Tags.ContainsKey("overheal")) ? ((long)Left.Tags["overheal"]).CompareTo((long)Right.Tags["overheal"]) : 0;
            }));
            MasterSwing.ColumnDefs.Add(Properties.PluginRegex.OutgoingTag, new MasterSwing.ColumnDef(Properties.PluginRegex.OutgoingTag, true, "VARCHAR2(16)", Properties.PluginRegex.OutgoingTag,
                (Data) => { return Data.Tags.ContainsKey(Properties.PluginRegex.OutgoingTag) ? Data.Tags[Properties.PluginRegex.OutgoingTag].ToString() : String.Empty; },
                (Data) => { return Data.Tags.ContainsKey(Properties.PluginRegex.OutgoingTag) ? Data.Tags[Properties.PluginRegex.OutgoingTag].ToString() : String.Empty; }, (Left, Right) =>
                {
                    return (Left.Tags.ContainsKey(Properties.PluginRegex.OutgoingTag) && Right.Tags.ContainsKey(Properties.PluginRegex.OutgoingTag)) ? Left.Tags[Properties.PluginRegex.OutgoingTag].ToString().CompareTo(Right.Tags[Properties.PluginRegex.OutgoingTag].ToString()) : 0;
                })
            );
            MasterSwing.ColumnDefs.Add(Properties.PluginRegex.IncomingTag, new MasterSwing.ColumnDef(Properties.PluginRegex.IncomingTag, true, "VARCHAR2(16)", Properties.PluginRegex.IncomingTag,
                (Data) => { return Data.Tags.ContainsKey(Properties.PluginRegex.IncomingTag) ? Data.Tags[Properties.PluginRegex.IncomingTag].ToString() : String.Empty; },
                (Data) => { return Data.Tags.ContainsKey(Properties.PluginRegex.IncomingTag) ? Data.Tags[Properties.PluginRegex.IncomingTag].ToString() : String.Empty; }, (Left, Right) =>
                {
                    return (Left.Tags.ContainsKey(Properties.PluginRegex.IncomingTag) && Right.Tags.ContainsKey(Properties.PluginRegex.IncomingTag)) ? Left.Tags[Properties.PluginRegex.IncomingTag].ToString().CompareTo(Right.Tags[Properties.PluginRegex.IncomingTag].ToString()) : 0;
                })
            );
            SetupSpecialTypeForMasterSwing();
            foreach (KeyValuePair<string, MasterSwing.ColumnDef> pair in MasterSwing.ColumnDefs)
                pair.Value.GetCellForeColor = (Data) => { return GetSwingTypeColor(Data.SwingType); };

            ActGlobals.oFormActMain.ValidateLists();
            ActGlobals.oFormActMain.ValidateTableSetup();
        }

        private void SetupSpecialTypeForMasterSwing()
        {
            foreach (String s in SpecialAttack)
                MasterSwing.ColumnDefs.Add(s, new MasterSwing.ColumnDef(s, false, "BOOLEAN", s,
                    (Data) =>
                    {
                        return String.Empty;
                    },
                    (Data) =>
                    {
                        return String.Empty;
                    },
                    (Left, Right) =>
                    {
                        return ((Specials)Left.Tags[Properties.PluginRegex.SpecialStringTag]).HasFlag(SpecialParsers.GetSpecialByString(s)).CompareTo(((Specials)Right.Tags[Properties.PluginRegex.SpecialStringTag]).HasFlag(SpecialParsers.GetSpecialByString(s)));
                    })
                { GetCellBackColor = (Data) =>

                    ((Specials)Data.Tags[Properties.PluginRegex.SpecialStringTag]).HasFlag(SpecialParsers.GetSpecialByString(s)) ? Color.Black : Color.White
                 }
                    );
        }

        private string GenerateCombatDataString(String s, bool outgoing)
        {
            return $"{s} ({(outgoing ? Properties.PluginRegex.Out : Properties.PluginRegex.In)})";
        }

        private string GenerateCombatDataStringOut(String s)
        {
            return GenerateCombatDataString(s, true);
        }

        private void SetupCritPercentage(String[] critTypes)
        {
            foreach (String critType in critTypes)
            {
                AttackTypeColumnDefGenerator.GetAttackTypeCritColumnDef(critType, true, "DOUBLE");
                DamageTypeDataColumnDefGenerator.GetDamageTypeDataCritColumnDef(critType, true, "DOUBLE");
                CombatantDataColumnDefGenerator.GetCombatantDataCritColumnDef(critType, true, "DOUBLE");
            }
        }

        /// <summary>
        /// attempts to get attack type and the swing type
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        private string GetAttackTypeSwingType(AttackType Data)
        {
            int? swingType = null;
            List<String> swingTypes = Data.Items.ToList().Select(o => o.AttackType).Distinct().ToList();
            List<MasterSwing> cachedItems = new List<MasterSwing>();
            foreach (MasterSwing s in Data.Items)
            {
                if (swingTypes.Contains(s.SwingType.ToString()) == false)
                    swingTypes.Add(s.SwingType.ToString());
            }
            if (swingTypes.Count == 1)
                swingType = Data.Items.First().SwingType;
            if (!(swingType == null))
                return String.Empty;
            else
                return !(swingType == null) ? String.Empty : swingType.ToString();
        }

        /// <summary>
        /// parses the combatant format switch
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="VarName"></param>
        /// <param name="Extra"></param>
        /// <returns></returns>
        private string CombatantFormatSwitch(CombatantData Data, string VarName, String Extra)
        {
            switch (VarName)
            {
                case "DURATION":
                    return Data.Duration.TotalSeconds.ToString("0");
                case "duration":
                    return Data.DurationS;
                case "maxhit":
                    return Data.GetMaxHit(true, false);
                case "MAXHIT":
                    return Data.GetMaxHit(false, false);
                case "maxhit-*":
                    return Data.GetMaxHit(true, true);
                case "MAXHIT-*":
                    return Data.GetMaxHit(false, true);
                case "maxheal":
                    return Data.GetMaxHeal(true, false, false);
                case "MAXHEAL":
                    return Data.GetMaxHeal(false, false, false);
                case "maxheal-*":
                    return Data.GetMaxHeal(true, false, true);
                case "MAXHEAL-*":
                    return Data.GetMaxHeal(false, false, true);
                case "maxhealward":
                    return Data.GetTag(Properties.PluginRegex.ward).GetMaxHeal(true, UseSuffix: false);
                case "MAXHEALWARD":
                    return Data.GetTag(Properties.PluginRegex.ward).GetMaxHeal(false, UseSuffix: false);
                case "maxhealward-*":
                    return Data.GetMaxHeal(true, true, true);
                case "MAXHEALWARD-*":
                    return Data.GetMaxHeal(false, true, true);
                case "damage":
                    return Data.Damage.ToString();
                case "damage-k":
                    return (Data.Damage / 1000.0).ToString("0.00");
                case "damage-m":
                    return (Data.Damage / 1000000.0).ToString("0.00");
                case "damage-b":
                    return (Data.Damage / 1000000000.0).ToString("0.00");
                case "damage-*":
                    return ActGlobals.oFormActMain.CreateDamageString(Data.Damage, true, true);
                case "DAMAGE-k":
                    return (Data.Damage / 1000.0).ToString("0");
                case "DAMAGE-m":
                    return (Data.Damage / 1000000.0).ToString("0");
                case "DAMAGE-b":
                    return (Data.Damage / 1000000000.0).ToString("0");
                case "DAMAGE-*":
                    return ActGlobals.oFormActMain.CreateDamageString(Data.Damage, true, false);
                case "healed":
                    return Data.Healed.ToString();
                case "healed-*":
                    return ActGlobals.oFormActMain.CreateDamageString(Data.Healed, true, true);
                case "swings":
                    return Data.Swings.ToString();
                case "hits":
                    return Data.Hits.ToString();
                case "crithits":
                    return Data.CritHits.ToString();
                case "critheals":
                    return Data.CritHeals.ToString();
                case "crithit%":
                    return Data.CritDamPerc.ToString("0'%");
                case "critheal%":
                    return Data.CritHealPerc.ToString("0'%");
                case "heals":
                    return Data.Heals.ToString();
                case "cures":
                    return Data.CureDispels.ToString();
                case "misses":
                    return Data.Misses.ToString();
                case "hitfailed":
                    return Data.Blocked.ToString();
                case "TOHIT":
                    return Data.ToHit.ToString("0");
                case "DPS":
                    return Data.DPS.ToString("0");
                case "DPS-k":
                    return (Data.DPS / 1000.0).ToString("0");
                case "DPS-m":
                    return (Data.DPS / 1000000.0).ToString("0");
                case "DPS-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.DPS, true, false);
                case "ENCDPS":
                    return Data.EncDPS.ToString("0");
                case "ENCDPS-k":
                    return (Data.EncDPS / 1000.0).ToString("0");
                case "ENCDPS-m":
                    return (Data.EncDPS / 1000000.0).ToString("0");
                case "ENCDPS-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.EncDPS, true, false);
                case "ENCHPS":
                    return Data.EncHPS.ToString("0");
                case "ENCHPS-k":
                    return (Data.EncHPS / 1000.0).ToString("0");
                case "ENCHPS-m":
                    return (Data.EncHPS / 1000000.0).ToString("0");
                case "ENCHPS-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.EncHPS, true, false);
                case "tohit":
                    lock(precisionObject)
                        return Data.ToHit.ToString($"F{precisionForDPS}");
                case "dps":
                    lock(precisionObject)
                        return Data.DPS.ToString($"F{precisionForDPS}");
                case "dps-k":
                    lock(precisionObject)
                        return (Data.DPS / 1000.0).ToString($"F{precisionForDPS}");
                case "dps-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.DPS, true, true);
                case "encdps":
                    lock(precisionObject)
                       return Data.EncDPS.ToString($"F{precisionForDPS}");
                case "encdps-k":
                    lock(precisionObject)
                       return (Data.EncDPS / 1000.0).ToString($"F{precisionForDPS}");
                case "encdps-m":
                    lock(precisionObject)
                        return (Data.EncDPS / 1000000.0).ToString($"F{precisionForDPS}");
                case "encdps-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.EncDPS, true, true);
                case "enchps":
                    return Data.EncHPS.ToString($"F{precisionForDPS}");
                case "enchps-k":
                    lock(precisionObject)
                        return (Data.EncHPS / 1000.0).ToString($"F{precisionForDPS}");
                case "enchps-m":
                    lock(precisionObject)
                        return (Data.EncHPS / 1000000.0).ToString($"F{precisionForDPS}");
                case "enchps-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.EncHPS, true, true);
                case "healstaken":
                    return Data.HealsTaken.ToString();
                case "healstaken-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.HealsTaken, true, true);
                case "damagetaken":
                    return Data.DamageTaken.ToString();
                case "damagetaken-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)Data.DamageTaken, true, true);
                //case "powerdrain":
                //    return Data.PowerDamage.ToString();
                //case "powerdrain-*":
                //    return ActGlobals.oFormActMain.CreateDamageString((long)Data.PowerDamage, true, true);
                //case "powerheal":
                //    return Data.PowerReplenish.ToString();
                //case "powerheal-*":
                //    return ActGlobals.oFormActMain.CreateDamageString((long)Data.PowerReplenish, true, true);
                case "kills":
                    return Data.Kills.ToString();
                case "deaths":
                    return Data.Deaths.ToString();
                case "damage%":
                    return Data.DamagePercent;
                case "healed%":
                    return Data.HealedPercent;
                //case "threatstr":
                //    return Data.GetThreatStr("Threat (Out)");
                //case "threatdelta":
                //    return Data.GetThreatDelta("Threat (Out)").ToString();
                case "n":
                    return "\n";
                case "t":
                    return "\t";
                default:
                    return VarName;
            }
        }
        /// <summary>
        /// attempts to get the encounter format switch
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="SelectiveAllies"></param>
        /// <param name="VarName"></param>
        /// <param name="Extra"></param>
        /// <returns></returns>
        private string EncounterFormatSwitch(EncounterData Data, List<CombatantData> SelectiveAllies, string VarName, String Extra)
        {
            switch (VarName)
            {
                case "maxheal":
                    return Data.GetMaxHeal(true, false, false);
                case "MAXHEAL":
                    return Data.GetMaxHeal(false, false, false);
                case "maxheal-*":
                    return Data.GetMaxHeal(true, false, true);
                case "MAXHEAL-*":
                    return Data.GetMaxHeal(false, false, true);
                case "maxhealward":
                    return Data.GetMaxHeal(true, true, false);
                case "MAXHEALWARD":
                    return Data.GetMaxHeal(false, true, false);
                case "maxhealward-*":
                    return Data.GetMaxHeal(true, true, true);
                case "MAXHEALWARD-*":
                    return Data.GetMaxHeal(false, true, true);
                case "maxhit":
                    return Data.GetMaxHit(true, false);
                case "MAXHIT":
                    return Data.GetMaxHit(false, false);
                case "maxhit-*":
                    return Data.GetMaxHit(true, true);
                case "MAXHIT-*":
                    return Data.GetMaxHit(false, true);
                case "duration":
                    if (ActGlobals.wallClockDuration)
                    {
                        if (Data.Active)
                        {
                            if (ActGlobals.oFormActMain.LastEstimatedTime > Data.StartTime)
                            {
                                TimeSpan wallDuration = ActGlobals.oFormActMain.LastEstimatedTime - Data.StartTime;
                                if (wallDuration.Hours == 0)
                                    return String.Format("{0:00}:{1:00}", wallDuration.Minutes, wallDuration.Seconds);
                                else
                                    return String.Format("{0:00}:{1:00}:{2:00}", wallDuration.Hours, wallDuration.Minutes, wallDuration.Seconds);
                            }
                            else
                            {
                                return "00:00";
                            }
                        }
                        else
                        {
                            return Data.DurationS;
                        }
                    }
                    else
                        return Data.DurationS;
                case "DURATION":
                    if (ActGlobals.wallClockDuration)
                    {
                        if (Data.Active)
                        {
                            if (ActGlobals.oFormActMain.LastEstimatedTime > Data.StartTime)
                            {
                                TimeSpan wallDuration = ActGlobals.oFormActMain.LastEstimatedTime - Data.StartTime;
                                return ((int)wallDuration.TotalSeconds).ToString("0");
                            }
                            else
                            {
                                return "0";
                            }
                        }
                        else
                        {
                            return ((int)Data.Duration.TotalSeconds).ToString("0");
                        }
                    }
                    else
                        return Data.Duration.TotalSeconds.ToString("0");
                case "damage":
                    return SelectiveAllies.Sum((cd) => cd.Damage).ToString();
                case "damage-m":
                    return (SelectiveAllies.Sum((cd) => cd.Damage) / Math.Pow(1, 7)).ToString("0.00");
                case "damage-b":
                    return (SelectiveAllies.Sum((cd) => cd.Damage) / Math.Pow(1, 10)).ToString("0.00");
                case "damage-*":
                    return ActGlobals.oFormActMain.CreateDamageString(SelectiveAllies.Sum((cd) => cd.Damage), true, true);
                case "DAMAGE-k":
                    return (SelectiveAllies.Sum((cd) => cd.Damage) / Math.Pow(1, 4)).ToString("0");
                case "DAMAGE-m":
                    return (SelectiveAllies.Sum((cd) => cd.Damage) / Math.Pow(1, 7)).ToString("0");
                case "DAMAGE-b":
                    return (SelectiveAllies.Sum((cd) => cd.Damage) / Math.Pow(1, 10)).ToString("0");
                case "DAMAGE-*":
                    return ActGlobals.oFormActMain.CreateDamageString(SelectiveAllies.Sum((cd) => cd.Damage), true, false);
                case "healed":
                    return SelectiveAllies.Sum((cd) => cd.Healed).ToString();
                case "healed-*":
                    return ActGlobals.oFormActMain.CreateDamageString(SelectiveAllies.Sum((cd) => cd.Healed), true, true);
                case "swings":
                    return SelectiveAllies.Sum((cd) => cd.Swings).ToString();
                case "hits":
                    return SelectiveAllies.Sum((cd) => cd.Hits).ToString();
                case "crithits":
                    return SelectiveAllies.Sum((cd) => cd.CritHits).ToString();
                case "crithit%":
                    return (SelectiveAllies.Sum((cd) => cd.CritHits) / SelectiveAllies.Sum((cd) => cd.Hits)).ToString("0'%");
                case "heals":
                    return SelectiveAllies.Sum((cd) => cd.Heals).ToString();
                case "critheals":
                    return SelectiveAllies.Sum((cd) => cd.CritHits).ToString();
                case "critheal%":
                    return (SelectiveAllies.Sum((cd) => cd.CritHeals) / SelectiveAllies.Sum((cd) => cd.Heals)).ToString("0'%");
                case "cures":
                    return SelectiveAllies.Sum((cd) => cd.CureDispels).ToString();
                case "misses":
                    return SelectiveAllies.Sum((cd) => cd.Misses).ToString();
                case "hitfailed":
                    return SelectiveAllies.Sum((cd) => cd.Blocked).ToString();
                case "TOHIT":
                    return (SelectiveAllies.Sum((cd) => cd.ToHit) / SelectiveAllies.Count).ToString("0");
                case "DPS":

                case "ENCDPS":
                    return SelectiveAllies.Sum((cd) => cd.Damage / Data.Duration.TotalSeconds).ToString("0");
                case "DPS-*":

                case "ENCDPS-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)(SelectiveAllies.Sum((cd) => cd.Damage) / Data.Duration.TotalSeconds), true, false);
                case "DPS-k":
                case "ENCDPS-k":
                    return ((SelectiveAllies.Sum((cd) => cd.Damage / Data.Duration.TotalSeconds)) / Math.Pow(1, 4)).ToString("0");
                case "ENCDPS-m":
                    return ((SelectiveAllies.Sum((cd) => cd.Damage) / Data.Duration.TotalSeconds) / Math.Pow(1, 7)).ToString("0");
                case "ENCHPS":
                    return (SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds).ToString("0");
                case "ENCHPS-k":
                    return ((SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds) / Math.Pow(1, 4)).ToString("0");
                case "ENCHPS-m":
                    return ((SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds) / Math.Pow(1, 10)).ToString("0");
                case "ENCHPS-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)(SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds), true, false);
                case "tohit":
                    lock(precisionObject)
                        return (SelectiveAllies.Sum((cd) => cd.ToHit) / SelectiveAllies.Count).ToString($"F{precisionForDPS}");
                case "dps":
                case "encdps":
                    lock(precisionObject)
                        return (SelectiveAllies.Sum(cd => cd.Damage) / Data.Duration.TotalSeconds).ToString($"F{precisionForDPS}");
                case "dps-k":
                case "encdps-k":
                    lock(precisionObject)
                        return ((SelectiveAllies.Sum(cd => cd.Damage) / Data.Duration.TotalSeconds) / Math.Pow(1, 4)).ToString($"F{precisionForDPS}");
                case "encdps-m":
                    lock(precisionObject)
                        return ((SelectiveAllies.Sum(cd => cd.Damage) / Data.Duration.TotalSeconds) / Math.Pow(1, 7)).ToString($"F{precisionForDPS}");
                case "encdps-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)(SelectiveAllies.Sum(cd => cd.Damage) / Data.Duration.TotalSeconds), true, true);
                case "enchps":
                    lock(precisionObject)
                       return (SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds).ToString($"F{precisionForDPS}");
                case "enchps-k":
                    lock(precisionObject)
                        return ((SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds) / Math.Pow(1, 4)).ToString($"F{precisionForDPS}");
                case "enchps-m":
                    lock(precisionObject)
                        return ((SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds) / Math.Pow(1, 7)).ToString($"F{precisionForDPS}");
                case "enchps-*":
                    return ActGlobals.oFormActMain.CreateDamageString((long)(SelectiveAllies.Sum((cd) => cd.Healed) / Data.Duration.TotalSeconds), true, true);
                case "healstaken":
                    return SelectiveAllies.Sum((cd) => cd.HealsTaken).ToString();
                case "healstaken-*":
                    return ActGlobals.oFormActMain.CreateDamageString(SelectiveAllies.Sum((cd) => cd.HealsTaken), true, true);
                case "damagetaken":
                    return SelectiveAllies.Sum((cd) => cd.DamageTaken).ToString();
                case "damagetaken-*":
                    return ActGlobals.oFormActMain.CreateDamageString(SelectiveAllies.Sum((cd) => cd.DamageTaken), true, true);
                case "kills":
                    return SelectiveAllies.Sum(cd => cd.Kills).ToString();
                case "deaths":
                    return SelectiveAllies.Sum(cd => cd.Deaths).ToString();
                case "title":
                    return Data.Title;
                default:
                    return VarName;
            }
        }

        /// <summary>
        /// Builds a regex string with the timestamp and regex provided
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal String RegexString(String regex)
        {
            if (regex == null)
                throw new ArgumentNullException("Missing value for regex");
            else
                return $@"\[(?<{Properties.PluginRegex.dateTimeOfLogLine}>.+)\] {regex}";
        }

        #region Regex Population Methods

        /// <summary>
        /// Populates noncombat style regexes
        /// </summary>
        private void PopulateRegexNonCombat()
        {
            possesive = new Regex(Properties.PluginRegex.petAndPlayerName, RegexOptions.Compiled);
            selfCheck = new Regex(Properties.PluginRegex.selfMatch, RegexOptions.Compiled); 
        }

        /// <summary>
        /// Populates the regex list with combat GenericStrings associated with combat actions in the character log file
        /// </summary>
        private void PopulateRegexCombat()
        {
            onLogLineRead = new List<Tuple<Color, Regex, Action<Match>>>();
            beforeLogLineRead = new List<Tuple<Color, Regex, Action<Match>>>();
            String MeleeAttack = @"(?<attacker>.+) (?<attackType>" + $@"{Properties.PluginRegex.attackTypes}" + @")(|s|es|bed) (?<victim>.+)(\sfor\s)(?<damageAmount>[\d]+) ((?:point)(?:s|)) of damage.(?:\s\((?<" + $@"{ Properties.PluginRegex.DamageSpecial}" + @">.+)\)){0,1}";
            String Evasion = @"(?<attacker>.*) tries to (?<attackType>\S+) (?:(?<victim>(.+)), but \1) (?:(?<evasionType>" + $@"{Properties.PluginRegex.evasionTypes}" + @"))(?:\swith (your|his|hers|its) (shield|staff)){0,1}!(?:[\s][\(](?<evasionSpecial>.+)[\)]){0,1}";
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Clear();
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.Red, new Regex(RegexString(MeleeAttack), RegexOptions.Compiled), (match) =>
            {
                Tuple<String, String> petTypeAndName = GetPetTypeAndPlayerName(ReplaceSelfWithCharacterName(match.Groups["attacker"].Value));
                Tuple<String, String> victimPetTypeAndName = GetPetTypeAndPlayerName(match.Groups["victim"].Value);
                DateTime dateTimeOfLogLine = ParseEQTimeStampFromLog(match.Groups[Properties.PluginRegex.dateTimeOfLogLine].Value);
                
                if (ActGlobals.oFormActMain.SetEncounter(ActGlobals.oFormActMain.LastKnownTime, ReplaceSelfWithCharacterName(petTypeAndName.Item2), ReplaceSelfWithCharacterName(victimPetTypeAndName.Item2)))
                {
                    if (chilled != default)
                    {
                        chilled.Tags.Add(Properties.PluginRegex.OutgoingTag, petTypeAndName.Item1);
                        ActGlobals.oFormActMain.AddCombatAction(chilled);
                        chilled = default;
                    }
                    Dnum damage = new Dnum(Int64.Parse(match.Groups["damageAmount"].Value), "melee");
                    String attackName = match.Groups["attackType"].Value == "frenzies on" ? "frenzy" : match.Groups["attackType"].Value;
                    Dictionary<string, object> tags = new Dictionary<string, object>
                    {
                        [Properties.PluginRegex.OutgoingTag] = petTypeAndName.Item1,
                        [Properties.PluginRegex.IncomingTag] = victimPetTypeAndName.Item1,
                        [Properties.PluginRegex.SpecialStringTag] = SpecialsParse(match.Groups[Properties.PluginRegex.DamageSpecial])
                    };
                    AddMasterSwing(
                            EQSwingType.Melee
                            , match.Groups[Properties.PluginRegex.DamageSpecial].Value.Contains(Properties.PluginRegex.Critical)
                            , damage
                            , attackName
                            , dateTimeOfLogLine
                            , ReplaceSelfWithCharacterName(petTypeAndName.Item2)
                            , "Hitpoints"
                            , ReplaceSelfWithCharacterName(victimPetTypeAndName.Item2)
                            , tags
                            );
                }
            })
              );
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.Plum, new Regex(RegexString(Properties.PluginRegex.MissedMeleeAttack), RegexOptions.Compiled), (match) =>
            {
                CombatMasterSwingAdd(match, EQSwingType.Melee,
                    "special", Dnum.Miss, "attackType", "Hitpoints", default);
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.Red, new Regex(RegexString(Properties.PluginRegex.SpellDamage), RegexOptions.Compiled), (match) =>
            {
                CombatMasterSwingAdd(match,
                        EQSwingType.Spell
                        , "special"
                        , new Dnum(Int64.Parse(match.Groups["damagePoints"].Value), match.Groups["typeOfDamage"].Value)
                        , "attackType"
                        , "Hitpoints", default
                    );
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.DarkBlue, new Regex(RegexString(Properties.PluginRegex.Heal), RegexOptions.Compiled),
                (match) =>
                {
                    Dnum heal = new Dnum(Int64.Parse(match.Groups["pointsOfHealing"].Value), "healing");
                    void tagsAction(Dictionary<string, object> tags)
                    {
                        if (match.Groups["overHealPoints"].Success)
                            tags["overheal"] = Int64.Parse(match.Groups["overHealPoints"].Value) - heal.Number ;
                    }

                    CombatMasterSwingAdd(match,
                        (match.Groups["overTime"].Success ? EQSwingType.HealingOverTime : EQSwingType.InstantHealing),
                        "special",
                        heal,
                        "healingSpellName",
                        "Hitpoints",
                        tagsAction);
                }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.Silver, new Regex(RegexString(Properties.PluginRegex.Unknown), RegexOptions.Compiled), (match) =>
            {
                DateTime dateTimeOfParse = ParseEQTimeStampFromLog(match.Groups[Properties.PluginRegex.dateTimeOfLogLine].Value);
                Tuple<String, String> petTypeAndName = GetPetTypeAndPlayerName(ReplaceSelfWithCharacterName(match.Groups["attacker"].Value));
                Tuple<String, String> victimPetTypeAndName = GetPetTypeAndPlayerName(match.Groups["victim"].Value);
                MasterSwing msUnknown = new MasterSwing(
                            (int)EQSwingType.NonMelee
                            , false
                            , new Dnum(Dnum.Unknown)
                            {
                                DamageString2 = match.Value
                            },
                            dateTimeOfParse,
                            ActGlobals.oFormActMain.GlobalTimeSorter,
                            "Unknown",
                            "Unknown",
                            "Unknown",
                            "Unknown")
                { Tags = new Dictionary<string, object> { { Properties.PluginRegex.OutgoingTag, petTypeAndName.Item1 }, { Properties.PluginRegex.IncomingTag, victimPetTypeAndName.Item1 } } };
                ActGlobals.oFormActMain.AddCombatAction(msUnknown);
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.DeepSkyBlue, new Regex(RegexString(Evasion), RegexOptions.Compiled),
                (match) =>
                {
                    CombatMasterSwingAdd(match,
                               EQSwingType.Melee
                               , "evasionSpecial"
                               , new Dnum(Dnum.Miss, match.Groups["evasionType"].Value)
                               , "attackType"
                               , "Hitpoints", default
                           );

                }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.LightBlue, new Regex(RegexString(Properties.PluginRegex.Banestrike), RegexOptions.Compiled), (match) =>
            {
                CombatMasterSwingAdd(match,
                               EQSwingType.Bane
                               , "special"
                               , new Dnum(Int64.Parse(match.Groups["baneDamage"].Value), "bane")
                               , "typeOfDamage"
                               , "Hitpoints", default
                           );
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.AliceBlue, new Regex(RegexString(Properties.PluginRegex.SpellDamageOverTime), RegexOptions.Compiled),
                (match) =>
                {
                    CombatMasterSwingAdd(match,
                           EQSwingType.Spell
                           , "special"
                           , new Dnum(Int64.Parse(match.Groups["damagePoints"].Value), "spell dot")
                           , "damageEffect"
                           , "Hitpoints", default
                       );

                }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.PaleVioletRed, new Regex(RegexString(Properties.PluginRegex.FocusDamageEffect), RegexOptions.Compiled),
                (match) =>
                {
                    CombatMasterSwingAdd(match,
                                   EQSwingType.Spell
                                   , "special"
                                   , new Dnum(Int64.Parse(match.Groups["damagePoints"].Value), "spell focus")
                                   , "damageEffect"
                                   , "Hitpoints", default
                               );

                }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            beforeLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.DarkOliveGreen, new Regex(RegexString(Properties.PluginRegex.DamageShieldUnknownOrigin), RegexOptions.Compiled),
                (match) =>
                {
                    CombatMasterSwingAdd(match,
                                  EQSwingType.NonMelee
                                  , "special"
                                  , new Dnum(Int64.Parse(match.Groups["damagePoints"].Value), "damage shield")
                                  , "damageShieldResponse"
                                  , "Hitpoints", default
                              );

                }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.SaddleBrown, new Regex(RegexString(Properties.PluginRegex.zoneChange)), (match) =>
            {
                String zoneName = match.Groups["zoneName"].Value;
                ActGlobals.oFormActMain.ChangeZone(zoneName);
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.Tan, new Regex(RegexString(Properties.PluginRegex.spellResist), RegexOptions.Compiled), (match) =>
            {
                CombatMasterSwingAdd(match,
                               EQSwingType.Spell
                               , "special"
                               , new Dnum(Dnum.NoDamage, "spell")
                               , "spellName"
                               , "Hitpoints", default
                           );
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead.Count - 1].Item1);
            onLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.Black, new Regex(RegexString(Properties.PluginRegex.SlainMessages), RegexOptions.Compiled), (match) =>
            {
                ParseDeathMessage(match);
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, onLogLineRead[onLogLineRead
                .Count - 1].Item1);
            beforeLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.ForestGreen
            , new Regex(RegexString(Properties.PluginRegex.DamageShield)
            , RegexOptions.Compiled), (match) =>
            {

                CombatMasterSwingAdd(match, EQSwingType.NonMelee,
                        "special",
                        new Dnum(Int64.Parse(match.Groups["damagePoints"].Value), "damage shield"),
                        "damageShieldType",
                        "Hitpoints", default
                );
            }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, beforeLogLineRead[beforeLogLineRead.Count - 1].Item1);
            beforeLogLineRead.Add(new Tuple<Color, Regex, Action<Match>>(Color.ForestGreen
                , new Regex(RegexString(Properties.PluginRegex.chilledDamageShield)
                , RegexOptions.Compiled)
                , (match) =>
                {
                    DateTime dateTimeOfParse = ParseEQTimeStampFromLog(match.Groups[Properties.PluginRegex.dateTimeOfLogLine].Value);
                    Tuple<String, String> petTypeAndName = GetPetTypeAndPlayerName(ReplaceSelfWithCharacterName(match.Groups["attacker"].Value));
                    Tuple<String, String> victimPetTypeAndName = GetPetTypeAndPlayerName(match.Groups["victim"].Value);
                    Dictionary<string, object> tags = new Dictionary<string, object>
                {
                    { Properties.PluginRegex.OutgoingTag, petTypeAndName.Item1 },
                    { Properties.PluginRegex.IncomingTag, victimPetTypeAndName.Item1 }
                };
                    chilled = new MasterSwing((int)EQSwingType.NonMelee,
                        match.Groups["special"].Success && match.Groups["special"].Value.Contains("Critical"),
                        new Dnum(Int64.Parse(match.Groups["damageAmount"].Value), "damage shield"),
                        dateTimeOfParse,
                        ActGlobals.oFormActMain.GlobalTimeSorter,
                        "chilled",
                        null,
                        "Hitpoints",
                        match.Groups["victim"].Value)
                    { Tags = tags };
                }));
            ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Add(ActGlobals.oFormEncounterLogs.LogTypeToColorMapping.Count, beforeLogLineRead[beforeLogLineRead.Count - 1].Item1);
        }
        #endregion
        /// <summary>
        /// gets the color associated with the type of action in the log file
        /// </summary>
        /// <param name="eqst"></param>
        /// <returns></returns>
        private Color GetSwingTypeColor(int eqst)
        {
            switch ((EQSwingType)eqst)
            {
                case EQSwingType.Melee:
                    return Color.DarkViolet;
                case EQSwingType.NonMelee:
                    return Color.DarkRed;
                case EQSwingType.Healing:
                    return Color.DarkBlue;
                case EQSwingType.Bane:
                    return Color.Honeydew;
                default:
                    return Color.Black;
            }
        }

        #region Parsing Eventers
        /// <summary>
        /// Attemps to parse previous logline if is exists in parse
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="logInfo"></param>
        private void FormActMain_BeforeLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            Tuple<Color, Regex, Action<Match>> tupleFirstOrDefault = beforeLogLineRead.FirstOrDefault((tuple) =>
            {
                return tuple.Item2.Match(logInfo.logLine).Success;
            });
            if (tupleFirstOrDefault != default) tupleFirstOrDefault.Item3(tupleFirstOrDefault.Item2.Match(logInfo.logLine));
        }

        /// <summary>
        /// Attempts to parse log line when it is read
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="logInfo"></param>
        private void FormActMain_OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            Tuple<Color, Regex, Action<Match>> tupleFirstOrDefault = onLogLineRead.FirstOrDefault((tuple) =>
            {
                return tuple.Item2.Match(logInfo.logLine).Success;
            });
            if (tupleFirstOrDefault != default) tupleFirstOrDefault.Item3(tupleFirstOrDefault.Item2.Match(logInfo.logLine));
        }
        #endregion

        private Specials SpecialsParse(Group specials)
        {
            if (specials.Success)
            {
                return 
                    ((specials.Value.Contains(Properties.PluginRegex.CripplingBlow) ? Specials.CripplingBlow : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.WildRampage) ? Specials.WildRampage : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.Twincast) ? Specials.Twincast : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.Strikethrough) ? Specials.Strikethrough : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.Riposte) ? Specials.Riposte : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.Lucky) ? Specials.Lucky : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.Locked) ? Specials.Locked : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.Flurry) ? Specials.Flurry : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.DoubleBowShot) ? Specials.DoubleBowShot : Specials.None) |
                    (specials.Value.Contains(Properties.PluginRegex.FinishingBlow) ? Specials.FinishingBlow : Specials.None));
            }
            else
                return Specials.None;
        }

        #region String Parsing
        private void CombatMasterSwingAdd(Match match, EQSwingType eqst, String specialMatchGroup, Dnum damage, String attackTypeMatchGroup, String typeOfResource, Action<Dictionary<string, object>> tagsAction)
        {
            DateTime dateTimeOfLogLine = ParseEQTimeStampFromLog(match.Groups[Properties.PluginRegex.dateTimeOfLogLine].Value);
            Tuple<String, String> attackerPetTypeAndName = GetPetTypeAndPlayerName(ReplaceSelfWithCharacterName(match.Groups["attacker"].Value));
            Tuple<String, String> victimPetTypeAndName = GetPetTypeAndPlayerName(match.Groups["victim"].Value);
            Dictionary<string, Object> tags = new Dictionary<string, Object>
                    {
                        { Properties.PluginRegex.OutgoingTag, attackerPetTypeAndName.Item1 },
                        { Properties.PluginRegex.IncomingTag, victimPetTypeAndName.Item1 }
                    };
            tags[Properties.PluginRegex.SpecialStringTag] = SpecialsParse(match.Groups["special"]);
            if (tagsAction != default)
                tagsAction(tags);
            if (((eqst & EQSwingType.Healing) == EQSwingType.Healing) && ActGlobals.oFormActMain.InCombat)
            {
                bool critical = match.Groups[specialMatchGroup].Value.Contains(Properties.PluginRegex.Critical);
                String healName = match.Groups[attackTypeMatchGroup].Success ? match.Groups[attackTypeMatchGroup].Value : "unnamed heal";
                String attacker = ReplaceSelfWithCharacterName(attackerPetTypeAndName.Item2);
                    AddMasterSwing(
                    eqst
                    , critical
                    , damage
                    , healName
                    , dateTimeOfLogLine
                    , attacker
                    , typeOfResource
                    , CheckIfSelf(victimPetTypeAndName.Item2) ? attackerPetTypeAndName.Item2 : ReplaceSelfWithCharacterName(victimPetTypeAndName.Item2)
                    , tags);
            }
            else
            {
                if (ActGlobals.oFormActMain.SetEncounter(ActGlobals.oFormActMain.LastKnownTime, ReplaceSelfWithCharacterName(attackerPetTypeAndName.Item2), ReplaceSelfWithCharacterName(victimPetTypeAndName.Item2)))
                {
                    AddMasterSwing(
                     eqst
                    , match.Groups[specialMatchGroup].Value.Contains(Properties.PluginRegex.Critical)
                    , damage
                    , match.Groups[attackTypeMatchGroup].Value
                    , dateTimeOfLogLine
                    , ReplaceSelfWithCharacterName(attackerPetTypeAndName.Item2)
                    , typeOfResource
                    , ReplaceSelfWithCharacterName(victimPetTypeAndName.Item2)
                    , tags);
                }
            }
        }

        private string NameFormatChange(CombatantData Data, int len)
        {
            return Data.Name.Length > len ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
        }

        /// <summary>
        /// Parsess the date and time based on the EverQuest character log time stamp format
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns>DateTime</returns>
        internal DateTime ParseEQTimeStampFromLog(String timeStamp)
        {
            DateTime.TryParseExact(timeStamp, Properties.PluginRegex.eqDateTimeStampFormat, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.AssumeLocal, out DateTime currentEQTimeStamp);
            return currentEQTimeStamp;
        }

        /// <summary>
        /// Examines the parameter for summoned entities and provides such type entity back to the parsing method with entity name and character name
        /// </summary>
        /// <param name="nameToSetTypeTo"></param>
        /// <returns></returns>
        internal Tuple<String, String> GetPetTypeAndPlayerName(String nameToSetTypeTo)
        {
            Match possessiveMatch = possesive.Match(nameToSetTypeTo);
            if (possessiveMatch.Success)
            {
                return new Tuple<String, string>(possessiveMatch.Groups["petName"].Value, possessiveMatch.Groups["playerName"].Value);
            }
            else return new Tuple<String, string>(String.Empty, nameToSetTypeTo);
        }
        /// <summary>
        /// Examines the string if the parsed line was a self type action
        /// </summary>
        /// <param name="nameOfCharacter"></param>
        /// <returns>true if self type action, false otherwise</returns>
        internal static bool CheckIfSelf(String nameOfCharacter)
        {
            Match m = regexSelf.Match(nameOfCharacter);
            return m.Success;
        }

        /// <summary>
        /// Construct Master Swing object
        /// </summary>
        internal static void AddMasterSwing(
            EQSwingType eqst
            , bool criticalAttack
            , Dnum damage
            , String attackName
            , DateTime dateTimeofLogline
            , String attacker
            , String typeOfResource
            , String victim
            , Dictionary<string, Object> tags
            )
        {
            ActGlobals.oFormActMain.AddCombatAction(new MasterSwing(
                (int)eqst
                , criticalAttack
                , damage
                , dateTimeofLogline
                , ActGlobals.oFormActMain.GlobalTimeSorter
                , attackName
                , attacker
                , typeOfResource
                , victim)
            { Tags = tags });
        }

        private void ParseDeathMessage(Match match)
        {
            CombatMasterSwingAdd(match, (int)EQSwingType.None,
                String.Empty
                , Dnum.Death
                , "Killing"
                , "Death"
                , default);
        }

        /// <summary>
        /// returns the charater's name from the log file if there is a match to the persona's listed in the regex used
        /// </summary>
        /// <param name="PersonaString"></param>
        /// <returns></returns>
        private string ReplaceSelfWithCharacterName(string PersonaString)
        {
            bool personaMatch = selfCheck.Match(PersonaString).Success;
            string personaReturn = personaMatch ? ActGlobals.charName : PersonaString;
            return personaReturn;
        }
        #endregion

        #region UI Elements
        #region Control Event Methods
        private void OnUpDownValueChanged(object sender, EventArgs e)
        {
            lock (precisionObject)
                precisionForDPS = ((long)UpDownForPrecision.Value);
        }
        private void OnMouseHoverVarianceGroupBox(object sender, EventArgs e)
        {
            ChangeSetOptionsHelpText("Whether variance is calculated and if so if it is sample or population variance.");
        }

        private void OnMouseLeaveButtonArea(object sender, EventArgs e)
        {
            ChangeSetOptionsHelpText(Properties.PluginRegex.MouseLeave);
        }

        private void OnMouseEnterPoplationVariance(object sender, EventArgs e)
        {
            ChangeSetOptionsHelpText("Select for calculation variance using population method");
        }

        private void OnMouseEnterSampleVariance(object sender, EventArgs e)
        {
            ChangeSetOptionsHelpText("Select for calculation of variance using sample method");
        }

        private void OnMouseEnterVarianceOff(object sender, EventArgs e)
        {
            ChangeSetOptionsHelpText("Select to turn off calculation of variance in plugin");
        }

        private void ChangeSetOptionsHelpText(String text)
        {
            Action changeText = () => ActGlobals.oFormActMain.SetOptionsHelpText(text);

            if (ActGlobals.oFormActMain.InvokeRequired)
            {
                ActGlobals.oFormActMain.Invoke(changeText);
            }
            else
            {
                changeText.Invoke();
            }
        }

        #endregion

        private void ChangeTextInControl(Control control, String text)
        {
            switch (control.InvokeRequired)
            {
                case true:
                    control.Invoke(new Action(() =>
                    {
                        control.Text = text;
                    }));
                    break;
                case false:
                    control.Text = text;
                    break;
                default:
                    break;
            }
        }
        private void ChangePluginStatusLabel(String status)
        {
            ChangeTextInControl(lblStatus, status);
        }

        #endregion

        #region Variance Methods and Lock Object

        readonly Object varianceMethodChangeLockObj = new Object();

        //Variance calculation for attack damage
        /// <summary>
        /// gets the variance of the attack type for display in the ACT application
        /// </summary>
        /// <param name="Data">data from attacktype collection to be parsed for variance type selected</param>
        /// <returns></returns>
        private double AttackTypeGetVariance(AttackType Data)
        {
            if (Data.Swings > 0)
            {
                lock (varianceMethodChangeLockObj)
                    if (StatisticalProcessors.Variance.varianceCalc != default)
                        return StatisticalProcessors.Variance.varianceCalc(Data);
                    else
                        return double.NaN;
            }
            else
            {
                return default;
            }
        }

        private void VarianceTypeCheckedChanged(object sender, EventArgs e)
        {
            lock (varianceMethodChangeLockObj)
            {
                if (sender.Equals(populVariance))
                {
                    ChangePluginStatusLabel("population variance radio button selected");
                    StatisticalProcessors.Variance.varianceCalc = StatisticalProcessors.Variance.populationVariance;
                }
                else if (sender.Equals(sampVariance))
                {
                    ChangePluginStatusLabel("sample variance radio button selected");
                    StatisticalProcessors.Variance.varianceCalc = StatisticalProcessors.Variance.sampleVariance;
                }
                else
                {
                    ChangePluginStatusLabel("off variance radio button selected");
                    StatisticalProcessors.Variance.varianceCalc = default;
                }
            }
        }

        #endregion
    }
}
