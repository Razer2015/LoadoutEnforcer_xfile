using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Web;
using System.Data;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Reflection;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents
{
    //Aliases
    using EventType = PRoCon.Core.Events.EventType;
    using CapturableEvent = PRoCon.Core.Events.CapturableEvents;
    using System.CodeDom.Compiler;
    using Microsoft.CSharp;
    using System.Xml.Serialization;
    using System.Linq;
    using System.Windows.Forms;

    public delegate Dictionary<String, String> SelectWeaponsDelegate(String base_path);
    public delegate Dictionary<String, String> SelectAccessoriesDelegate(String base_path);
    public delegate Dictionary<String, String> SelectKititemsDelegate(String base_path);
    public delegate String HTMLDecode(String encoded);

    public enum Spawning
    {
        Allow,
        Disallow,
        DisallowAfterOne
    }

    public enum kits
    {
        ASSAULT,
        ENGINEER,
        SUPPORT,
        RECON,
        NOKIT
    }

    public enum category
    {
        PRIMARY,
        SECONDARY,
        GADGET1,
        GADGET2,
        GRENADE,
        MELEE,
        SPECIALIZATION
    }

    public class compact
    {
        public bool enabled { get; set; }
        public String name { get; set; }
        public List<String> maps { get; set; }
        public List<String> Whitelist { get; set; }
        public Weapons weapons { get; set; }
        public Accessories accessories { get; set; }
        public Kititems kititems { get; set; }

        public compact()
        {
            enabled = true;
            name = "Example Name";
            maps = new List<String>();
            Whitelist = new List<String>();
            weapons = new Weapons();
            accessories = new Accessories();
            kititems = new Kititems();
        }
    }

    public class Weapons
    {
        public List<Status> assault_rifles { get; set; }
        public List<Status> carbines { get; set; }
        public List<Status> dmrs { get; set; }
        public List<Status> handguns { get; set; }
        public List<Status> lmgs { get; set; }
        public List<Status> pdws { get; set; }
        public List<Status> shotguns { get; set; }
        public List<Status> sniper_rifles { get; set; }
        public List<Status> special { get; set; }

        public Weapons()
        {
            assault_rifles = new List<Status>();
            carbines = new List<Status>();
            dmrs = new List<Status>();
            handguns = new List<Status>();
            lmgs = new List<Status>();
            pdws = new List<Status>();
            shotguns = new List<Status>();
            sniper_rifles = new List<Status>();
            special = new List<Status>();
        }
    }

    public class Accessories
    {
        public List<Status> ACCESSORY { get; set; }
        public List<Status> AMMO { get; set; }
        public List<Status> AUXILIARY { get; set; }
        public List<Status> BARREL { get; set; }
        public List<Status> CLOSE_RANGE { get; set; }
        public List<Status> MEDIUM_RANGE { get; set; }
        public List<Status> LONG_RANGE { get; set; }
        public List<Status> UNDERBARREL { get; set; }

        public Accessories()
        {
            ACCESSORY = new List<Status>();
            AMMO = new List<Status>();
            AUXILIARY = new List<Status>();
            BARREL = new List<Status>();
            CLOSE_RANGE = new List<Status>();
            MEDIUM_RANGE = new List<Status>();
            LONG_RANGE = new List<Status>();
            UNDERBARREL = new List<Status>();
        }
    }

    public class Kititems
    {
        public List<Status> FIELD_UPGRADES { get; set; }
        public List<Status> GADGET { get; set; }
        public List<Status> GRENADE { get; set; }

        public Kititems()
        {
            FIELD_UPGRADES = new List<Status>();
            GADGET = new List<Status>();
            GRENADE = new List<Status>();
        }
    }

    public class Replace
    {
        public String name { get; set; }
        public category type { get; set; }
        public Int64 old_key { get; set; }
        public Int64 new_key { get; set; }
    }

    public class Status
    {
        public String Key { get; set; }
        public Spawning status { get; set; }
        // this is required for deserialization if a .ctor with parameters is present.
        public Status() { }
        public Status(String key, Spawning status)
        {
            this.Key = key;
            this.status = status;
        }
    }

    public class Player_Kit
    {
        public String personaID { get; set; }
        public kits kit { get; set; }
        public PRIMARY_WEAPON PRIMARY_WEAPON { get; set; }
        public SIDEARM SIDEARM { get; set; }
        public String GADGET_1 { get; set; }
        public String GADGET_2 { get; set; }
        public String GRENADES { get; set; }
        public String MELEE { get; set; }
        public String FIELD_UPGRADES { get; set; }
        public String UNK_7 { get; set; }
        public String UNK_8 { get; set; }
        public String OUTFIT { get; set; }
        public String PARACHUTE_CAMOUFLAGE { get; set; }
        public String UNK_11 { get; set; }
        public String UNK_12 { get; set; }
        public int VIOLATIONS { get; set; }
        public List<String> VIOLATION_REASONS { get; set; }
        public List<String> NO_SPAWN { get; set; }
        public List<String> ONE_SPAWN { get; set; }

        public UNLOCKS UNLOCKS { get; set; }
        public DateTime LAST_KILL { get; set; }
        public bool LOADOUT_PREVIOUSLY_BAD { get; set; }
        public bool FIRST_SPAWN_NOTED { get; set; }
        public bool ERROR { get; set; }

        public Player_Kit()
        {
            PRIMARY_WEAPON = new PRIMARY_WEAPON();
            SIDEARM = new SIDEARM();
            UNLOCKS = new UNLOCKS();
            VIOLATION_REASONS = new List<String>();
            NO_SPAWN = new List<String>();
            ONE_SPAWN = new List<String>();
            LOADOUT_PREVIOUSLY_BAD = false;
        }
    }

    public class PRIMARY_WEAPON
    {
        public String Key { get; set; }
        public String OPTIC { get; set; }
        public String ACCESSORY { get; set; }
        public String BARREL { get; set; }
        public String UNDERBARREL { get; set; }
        public String PAINT { get; set; }
        public String AMMO { get; set; }
    }

    public class SIDEARM
    {
        public String Key { get; set; }
        public String OPTIC { get; set; }
        public String ACCESSORY { get; set; }
        public String BARREL { get; set; }
        public String PAINT { get; set; }
    }

    public class UNLOCKS
    {
        public int sc_handgrenades { get; set; }
        public bool sc_handgrenades_unlocked { get; set; }
    }

    public class LoadoutEnforcer_xfile : PRoConPluginAPI, IPRoConPluginInterface
    {
        /* Inherited:
            this.PunkbusterPlayerInfoList = new Dictionary<string, CPunkbusterInfo>();
            this.FrostbitePlayerInfoList = new Dictionary<string, CPlayerInfo>();
        */

        private bool fIsEnabled;
        private int fDebugLevel;
        private bool m_DisplayOnKill;
        private int categoryToShow;

        private int m_waitOnPlayerJoin;
        private int m_waitOnLevelLoaded;
        private int m_waitOnSpawn;

        private Int64 m_spawnCount;

        private String base_path;
        private String port;
        private String host;
        private String log_path = String.Empty;
        private int limit_index;
        private List<compact> limits;
        private Dictionary<String, String> compact_weapons;
        private Dictionary<String, String> compact_accessories;
        private Dictionary<String, String> compact_kititems;
        private Dictionary<String, String> rcon_to_weapon;
        private Dictionary<String, Player_Kit> player_kits;
        private List<CPlayerInfo> PlayersList;
        private String currentLevel;
        public string[] SpawningEnum = new String[] { "Spawning allowed", "Spawning not allowed", "One spawn allowed" };
        public string[] CategoryEnum = new String[] { "Weapons", "Accessories", "Kititems" };
        public List<Replace> replaces;

        public LoadoutEnforcer_xfile()
        {
            fIsEnabled = false;
            fDebugLevel = 2;
            categoryToShow = 0;

            m_waitOnPlayerJoin = 30;
            m_waitOnLevelLoaded = 20;
            m_waitOnSpawn = 5;

            m_spawnCount = 0;
            m_DisplayOnKill = false;
            base_path = Reflector.AssemblyDirectory;
            limit_index = 0;
            limits = new List<compact>();
            compact_weapons = new Dictionary<String, String>();
            compact_accessories = new Dictionary<String, String>();
            compact_kititems = new Dictionary<String, String>();
            rcon_to_weapon = new Dictionary<String, String>();
            LoadInfo();
            rcon_to_weapon = getRCONtoWeapon();
            player_kits = new Dictionary<String, Player_Kit>();
            PlayersList = new List<CPlayerInfo>();
        }

        private void LoadInfo()
        {
            if (File.Exists(Path.Combine(base_path, @"LoadoutEnforcer\loadoutenforcer_settings.xml")))
            {
                if (compact_weapons.Count <= 0)
                {
                    compact_weapons = SQLite.GetWeapons;
                }
                if (compact_accessories.Count <= 0)
                {
                    compact_accessories = SQLite.GetAccessories;
                }
                if (compact_kititems.Count <= 0)
                {
                    compact_kititems = SQLite.GetKititems;
                }
                XmlSerializer deserializer = new XmlSerializer(typeof(List<compact>));
                TextReader reader = new StreamReader(Path.Combine(base_path, @"LoadoutEnforcer\loadoutenforcer_settings.xml"));
                object obj = deserializer.Deserialize(reader);
                limits = (List<compact>)obj;
                reader.Close();
            }
            else
            {
                if (!PopulateWeapons())
                    ConsoleWarn("Error in Populating Weapons!");
                if (!PopulateAccessories())
                    ConsoleWarn("Error in Populating Accessories!");
                if (!PopulateKititems())
                    ConsoleWarn("Error in Populating Kititems!");
            }
        }

        public bool PopulateWeapons()
        {
            // Weapons
            if (compact_weapons.Count <= 0)
            {
                compact_weapons = SQLite.GetWeapons;
            }
            if (limits.Count <= 0)
            {
                compact weapons = new compact();
                limits.Add(weapons);
            }
            foreach (KeyValuePair<String, String> pair in compact_weapons)
            {
                #region Weapons
                String[] parameters = pair.Value.Split('|');
                // Assault Rifles
                if (parameters[0].Equals("Assault Rifles"))
                {
                    if (!limits[limit_index].weapons.assault_rifles.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.assault_rifles.Add(new Status(pair.Key, Spawning.Allow));
                    //if (!limits[limit_index].weapons.assault_rifles.ContainsKey(pair.Key))
                    //limits[limit_index].weapons.assault_rifles.Add(pair.Key, Spawning.Allow);
                }
                // Carbines
                else if (parameters[0].Equals("Carbines"))
                {
                    if (!limits[limit_index].weapons.carbines.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.carbines.Add(new Status(pair.Key, Spawning.Allow));
                }
                // DMRs
                else if (parameters[0].Equals("DMRs"))
                {
                    if (!limits[limit_index].weapons.dmrs.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.dmrs.Add(new Status(pair.Key, Spawning.Allow));
                }
                // Handguns
                else if (parameters[0].Equals("Handguns"))
                {
                    if (!limits[limit_index].weapons.handguns.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.handguns.Add(new Status(pair.Key, Spawning.Allow));
                }
                // LMGs
                else if (parameters[0].Equals("LMGs"))
                {
                    if (!limits[limit_index].weapons.lmgs.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.lmgs.Add(new Status(pair.Key, Spawning.Allow));
                }
                // PDWs
                else if (parameters[0].Equals("PDWs"))
                {
                    if (!limits[limit_index].weapons.pdws.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.pdws.Add(new Status(pair.Key, Spawning.Allow));
                }
                // Shotguns
                else if (parameters[0].Equals("Shotguns"))
                {
                    if (!limits[limit_index].weapons.shotguns.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.shotguns.Add(new Status(pair.Key, Spawning.Allow));
                }
                // Sniper Rifles
                else if (parameters[0].Equals("Sniper Rifles"))
                {
                    if (!limits[limit_index].weapons.sniper_rifles.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.sniper_rifles.Add(new Status(pair.Key, Spawning.Allow));
                }
                // Special
                else if (parameters[0].Equals("Special"))
                {
                    if (!limits[limit_index].weapons.special.Any(weapon => weapon.Key == pair.Key))
                        limits[limit_index].weapons.special.Add(new Status(pair.Key, Spawning.Allow));
                } 
                #endregion
            }

            return (true);
        }

        public bool PopulateAccessories()
        {
            // ACCESSORY    // AMMO         // AUXILIARY    // BARREL
            // CLOSE_RANGE  // MEDIUM_RANGE // LONG_RANGE   // UNDERBARREL
            // Accessories
            if (compact_accessories.Count <= 0)
            {
                compact_accessories = SQLite.GetAccessories;
            }
            if (limits.Count <= 0)
            {
                compact weapons = new compact();
                limits.Add(weapons);
            }
            foreach (KeyValuePair<String, String> pair in compact_accessories)
            {
                #region Accessories
                String[] parameters = pair.Value.Split('|');
                if (parameters[0].Equals("ACCESSORY")) // ACCESSORY
                {
                    if (!limits[limit_index].accessories.ACCESSORY.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.ACCESSORY.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("AMMO")) // AMMO
                {
                    if (!limits[limit_index].accessories.AMMO.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.AMMO.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("AUXILIARY")) // AUXILIARY
                {
                    if (!limits[limit_index].accessories.AUXILIARY.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.AUXILIARY.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("BARREL")) // BARREL
                {
                    if (!limits[limit_index].accessories.BARREL.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.BARREL.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("CLOSE RANGE")) // CLOSE_RANGE
                {
                    if (!limits[limit_index].accessories.CLOSE_RANGE.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.CLOSE_RANGE.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("MEDIUM RANGE")) // MEDIUM_RANGE
                {
                    if (!limits[limit_index].accessories.MEDIUM_RANGE.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.MEDIUM_RANGE.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("LONG RANGE")) // LONG_RANGE
                {
                    if (!limits[limit_index].accessories.LONG_RANGE.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.LONG_RANGE.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("UNDERBARREL")) // UNDERBARREL
                {
                    if (!limits[limit_index].accessories.UNDERBARREL.Any(key => key.Key == pair.Key))
                        limits[limit_index].accessories.UNDERBARREL.Add(new Status(pair.Key, Spawning.Allow));
                }
                #endregion
            }

            return (true);
        }

        public bool PopulateKititems()
        {
            // FIELD_UPGRADES   // GADGET  // GRENADE
            // Kititems
            if (compact_kititems.Count <= 0)
            {
                compact_kititems = SQLite.GetKititems;
            }
            if (limits.Count <= 0)
            {
                compact weapons = new compact();
                limits.Add(weapons);
            }
            foreach (KeyValuePair<String, String> pair in compact_kititems)
            {
                #region Kititems
                String[] parameters = pair.Value.Split('|');
                if (parameters[0].Equals("FIELD UPGRADES")) // FIELD_UPGRADES
                {
                    if (!limits[limit_index].kititems.FIELD_UPGRADES.Any(key => key.Key == pair.Key))
                        limits[limit_index].kititems.FIELD_UPGRADES.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("GADGET")) // GADGET
                {
                    if (!limits[limit_index].kititems.GADGET.Any(key => key.Key == pair.Key))
                        limits[limit_index].kititems.GADGET.Add(new Status(pair.Key, Spawning.Allow));
                }
                else if (parameters[0].Equals("GRENADE")) // GRENADE
                {
                    if (!limits[limit_index].kititems.GRENADE.Any(key => key.Key == pair.Key))
                        limits[limit_index].kititems.GRENADE.Add(new Status(pair.Key, Spawning.Allow));
                }
                #endregion
            }

            return (true);
        }

        #region CURRENTLY UNNECESSARY
        public enum MessageType { Warning, Error, Exception, Normal };
        public String FormatMessage(String msg, MessageType type)
        {
            String prefix = "[^bLoadout Enforcer!^n] ";

            if (type.Equals(MessageType.Warning))
                prefix += "^1^bWARNING^0^n: ";
            else if (type.Equals(MessageType.Error))
                prefix += "^1^bERROR^0^n: ";
            else if (type.Equals(MessageType.Exception))
                prefix += "^1^bEXCEPTION^0^n: ";

            return prefix + msg;
        }
        public void LogWrite(String msg)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }
        public void ConsoleWrite(string msg, MessageType type)
        {
            LogWrite(FormatMessage(msg, type));
        }
        public void ConsoleWrite(string msg)
        {
            ConsoleWrite(msg, MessageType.Normal);
        }
        public void ConsoleWarn(String msg)
        {
            ConsoleWrite(msg, MessageType.Warning);
        }
        public void ConsoleError(String msg)
        {
            ConsoleWrite(msg, MessageType.Error);
        }
        public void ConsoleException(String msg)
        {
            ConsoleWrite(msg, MessageType.Exception);
        }
        public void DebugWrite(string msg, int level)
        {
            if (fDebugLevel >= level) ConsoleWrite(msg, MessageType.Normal);
        }

        public void ServerCommand(params String[] args)
        {
            List<string> list = new List<string>();
            list.Add("procon.protected.send");
            list.AddRange(args);
            this.ExecuteCommand(list.ToArray());
        }

        public string GetPluginName()
        {
            return "Loadout Enforcer";
        }
        public string GetPluginVersion()
        {
            return "0.0.0.5";
        }
        public string GetPluginAuthor()
        {
            return "xfileFIN";
        }
        public string GetPluginWebsite()
        {
            return "github.com/Razer2015";
        }
        public string GetPluginDescription()
        {
            return @"
<h2>Description</h2>
<p>Plugin to disallow spawning with certain weapons/attachments in loadout.</p>

<h2>Commands</h2>
<p><b>!myl [optional (admin only): nick], !myloadout</b> Check what is wrong with the loadout</p>
<p><b>!loadoutcheck</b> Check which limits are enabled (Admin only)</p>
<p><b>!violations [optional (admin only): nick]</b> Check how many violations the user has broken</p>
<p><b>!violations</b> Check how many players have broken rules</p>
<p><b>!remviol [nick]</b> Reset the violation count for player</p>

<h2>Settings</h2>
<blockquote>
<h4>Global Settings</h4>
	<p><b>Show OnKill forbidden users</b> Whether to display that the player has been killed for using unallowed weapon/attachment or not</p>
    <p><b>Wait Time OnPlayerJoin [Seconds]</b> How long to wait before checking the loadout when player joins the server</p>
    <p><b>Wait Time OnLevelLoaded [Seconds]</b> How long to wait before checking the loadouts after new level loaded</p>
    <p><b>Wait Time OnSpawn [Seconds]</b> How long to wait before checking the loadout when player spawns</p>
</blockquote>

<blockquote>
<h4>Limit Settings - Limit Global</h4>
	<p><b>Limit count</b> Displays which limit is currently shown and how many are there</p>
	<p><b>Category</b> Which category is currently displayed (note: Accessories takes alot time to load)</p>
	<p><b>Add new limit</b> 'Yes' adds a new limit</p>
	<p><b>Delete limit</b> Change to the limit index which you want to delete</p>
	<p><b>Currently shown limit</b> Change to the limit index which you want to be currently shown</p>
</blockquote>

<blockquote>
<h4>Limit Settings - Limit Specific</h4>
	<p><b>Limit Enabled</b> Whether the limit is enabled or not</p>
	<p><b>Limit Name</b> Limit name</p>
	<p><b>Limit Maps</b> Map names where the limit is present - One at a line, example: MP_Prison (note. No map names equals all)</p>
	<p><b>Limit Whitelist</b> Players who don't have to obey the limits</p>
</blockquote>

<blockquote>
<h4>Limit Settings - Limit Quickset</h4>
	<p><b>Spawning allowed</b> Quickly set multiple items to the 'Spawning allowed' -position. One code at a line, example. 234564305</p>
	<p><b>Spawning not allowed</b> Quickly set multiple items to the 'Spawning not allowed' -position. One code at a line, example. 234564305</p>
	<p><b>One spawn allowed</b> Quickly set multiple items to the 'One spawn allowed' -position. One code at a line, example. 234564305</p>
</blockquote>

<h2>Development</h2>
<p>Developed by xfileFIN</p>
<blockquote>
<h4>TODO:</h4>
	- Nothing... <br/>
</blockquote>

<h3>Changelog</h3>
<blockquote>
<h4>0.0.0.5 (23.3.2016)</h4>
	- fixed error in checking with old codes (not necessarily have all the codes yet)<br/>
	- added @remviol [nick] commands<br/>
</blockquote>

<blockquote>
<h4>0.0.0.4 (19.3.2016)</h4>
	- added @violations & @violations [nick] commands<br/>
    - added checks the current map on first spawn to eliminate wrong map in limit checking<br/>
</blockquote>

<blockquote>
<h4>0.0.0.3 (13.3.2016)</h4>
	- fixed OnLevelLoaded bug which occurs if new player joins before level has loaded<br/>
    - Wait Time OnLevelLoaded [Seconds] is not currently in use because it causes Procon crash for unknown reason<br/>
</blockquote>

<blockquote>
<h4>0.0.0.2 (13.3.2016)</h4>
	- fixed M67 Grenade for player who don't have others unlocked<br/>
</blockquote>

<blockquote>
<h4>0.0.0.1 (13.3.2016)</h4>
	- initial version<br/>
</blockquote>
";
        } 
        #endregion

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("1 Global Settings|Show OnKill forbidden users", m_DisplayOnKill.GetType(), m_DisplayOnKill));
            lstReturn.Add(new CPluginVariable("1 Global Settings|Wait Time OnPlayerJoin [Seconds]", m_waitOnPlayerJoin.GetType(), m_waitOnPlayerJoin));
            lstReturn.Add(new CPluginVariable("1 Global Settings|Wait Time OnLevelLoaded [Seconds]", m_waitOnLevelLoaded.GetType(), m_waitOnLevelLoaded));
            lstReturn.Add(new CPluginVariable("1 Global Settings|Wait Time OnSpawn [Seconds]", m_waitOnSpawn.GetType(), m_waitOnSpawn));

            lstReturn.Add(new CPluginVariable("Debug|Debug level", fDebugLevel.GetType(), fDebugLevel));

            //lstReturn.Add(new CPluginVariable("1 Settings|Debug level", fDebugLevel.GetType(), fDebugLevel));
            //lstReturn.Add(new CPluginVariable("1 Updated|Updated", typeof(string), DateTime.Now.ToString(), true));

            if (limits.Count < 1)
                lstReturn.Add(new CPluginVariable("2.1 Limit Settings|Add new limit", "enum.Add new limit(...|Yes|No)", String.Format("... ({0})", limits.Count)));

            if (limits.Count > 0)
            {
                if (limit_index >= limits.Count)
                    limit_index = 0;

                lstReturn.Add(new CPluginVariable("2.1 Limit Settings - Limit Global|Limit count", typeof(string), String.Format("{0} of {1}", (limit_index + 1), limits.Count), true));
                lstReturn.Add(new CPluginVariable("2.1 Limit Settings - Limit Global|Category", "enum.Category(Weapons|Accessories|Kititems)", CategoryEnum[categoryToShow]));
                lstReturn.Add(new CPluginVariable("2.1 Limit Settings - Limit Global|Add new limit", "enum.Add new limit(...|Yes|No)", String.Format("... ({0})", limits.Count)));
                lstReturn.Add(new CPluginVariable("2.1 Limit Settings - Limit Global|Delete limit", typeof(int), 0));
                lstReturn.Add(new CPluginVariable("2.1 Limit Settings - Limit Global|Currently shown limit", typeof(int), (limit_index + 1)));

                lstReturn.Add(new CPluginVariable("2.2 Limit Settings - Limit Specific|Limit Enabled", typeof(bool), limits[limit_index].enabled));
                lstReturn.Add(new CPluginVariable("2.2 Limit Settings - Limit Specific|Limit Name", typeof(string), limits[limit_index].name));
                lstReturn.Add(new CPluginVariable("2.2 Limit Settings - Limit Specific|Limit Maps", typeof(string[]), limits[limit_index].maps.ToArray()));
                lstReturn.Add(new CPluginVariable("2.2 Limit Settings - Limit Specific|Limit Whitelist", typeof(string[]), limits[limit_index].Whitelist.ToArray()));

                lstReturn.Add(new CPluginVariable("2.3 Limit Settings - Quickset|Spawning allowed", typeof(string[]), null));
                lstReturn.Add(new CPluginVariable("2.3 Limit Settings - Quickset|Spawning not allowed", typeof(string[]), null));
                lstReturn.Add(new CPluginVariable("2.3 Limit Settings - Quickset|One spawn allowed", typeof(string[]), null));

                // 3.0
                #region Weapons
                // Weapons
                if (compact_weapons.Count > 0 && categoryToShow == 0)
                    foreach (KeyValuePair<String, String> pair in compact_weapons)
                    {
                        String[] parameters = pair.Value.Split('|');
                        String enumtext = "";
                        if (parameters.Length == 3)
                            enumtext = String.Join("|", SpawningEnum);
                        else
                            enumtext = String.Format("{0}|{1}", SpawningEnum[0], SpawningEnum[1]);
                        // Assault Rifles
                        if (parameters[0].Equals("Assault Rifles"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.assault_rifles.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.assault_rifles.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.1 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                    //lstReturn.Add(new CPluginVariable(String.Format("3.1 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), typeof(enumBoolYesNo), status.status));
                                }
                        }
                        // Carbines
                        else if (parameters[0].Equals("Carbines"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.carbines.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.carbines.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.2 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        // DMRs
                        else if (parameters[0].Equals("DMRs"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.dmrs.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.dmrs.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.3 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        // Handguns
                        else if (parameters[0].Equals("Handguns"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.handguns.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.handguns.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.4 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        // LMGs
                        else if (parameters[0].Equals("LMGs"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.lmgs.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.lmgs.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.5 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        // PDWs
                        else if (parameters[0].Equals("PDWs"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.pdws.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.pdws.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.6 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        // Shotguns
                        else if (parameters[0].Equals("Shotguns"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.shotguns.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.shotguns.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.7 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        // Sniper Rifles
                        else if (parameters[0].Equals("Sniper Rifles"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.sniper_rifles.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.sniper_rifles.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.8 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        // Special
                        else if (parameters[0].Equals("Special"))
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].weapons.special.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].weapons.special.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("3.9 Weapons - {0}|ALWS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALWS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                    }
                #endregion

                // 4.0
                #region Accessories
                // ACCESSORY    // AMMO         // AUXILIARY    // BARREL
                // CLOSE_RANGE  // MEDIUM_RANGE // LONG_RANGE   // UNDERBARREL
                // Accessories
                if (compact_accessories.Count > 0 && categoryToShow == 1)
                    foreach (KeyValuePair<String, String> pair in compact_accessories)
                    {
                        String[] parameters = pair.Value.Split('|');
                        String enumtext = "";
                        if (parameters.Length == 4)
                            enumtext = String.Join("|", SpawningEnum);
                        else
                            enumtext = String.Format("{0}|{1}", SpawningEnum[0], SpawningEnum[1]);
                        if (parameters[0].Equals("ACCESSORY")) // ACCESSORY
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.ACCESSORY.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.ACCESSORY.Find(x => (x.Key == pair.Key));

                                    lstReturn.Add(new CPluginVariable(String.Format("4.1 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                    //lstReturn.Add(new CPluginVariable(String.Format("4.1 Accessories - {0}|ALAS-{1} | {2} ({3})", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), typeof(enumBoolYesNo), status.status));
                                }
                        }
                        else if (parameters[0].Equals("AMMO")) // AMMO
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.AMMO.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.AMMO.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("4.2 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        else if (parameters[0].Equals("AUXILIARY")) // AUXILIARY
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.AUXILIARY.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.AUXILIARY.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("4.3 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        else if (parameters[0].Equals("BARREL")) // BARREL
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.BARREL.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.BARREL.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("4.4 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        else if (parameters[0].Equals("CLOSE RANGE")) // CLOSE_RANGE
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.CLOSE_RANGE.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.CLOSE_RANGE.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("4.5 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        else if (parameters[0].Equals("MEDIUM RANGE")) // MEDIUM_RANGE
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.MEDIUM_RANGE.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.MEDIUM_RANGE.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("4.6 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        else if (parameters[0].Equals("LONG RANGE")) // LONG_RANGE
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.LONG_RANGE.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.LONG_RANGE.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("4.7 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        else if (parameters[0].Equals("UNDERBARREL")) // UNDERBARREL
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].accessories.UNDERBARREL.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].accessories.UNDERBARREL.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("4.8 Accessories - {0}|ALAS-{1} | {2} [{3}]", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1], parameters[2]), String.Format("enum.ALAS-{0} | {1} [{2}]({3})", pair.Key.PadLeft(10, '0'), parameters[1], parameters[2], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                    }
                #endregion

                // 5.0
                #region Kititems
                // FIELD_UPGRADES   // GADGET  // GRENADE
                // Kititems
                if (compact_kititems.Count > 0 && categoryToShow == 2)
                    foreach (KeyValuePair<String, String> pair in compact_kititems)
                    {
                        String[] parameters = pair.Value.Split('|');
                        String enumtext = "";
                        if (parameters.Length == 3)
                            enumtext = String.Join("|", SpawningEnum);
                        else
                            enumtext = String.Format("{0}|{1}", SpawningEnum[0], SpawningEnum[1]);
                        if (parameters[0].Equals("FIELD UPGRADES")) // FIELD_UPGRADES
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].kititems.FIELD_UPGRADES.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].kititems.FIELD_UPGRADES.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("5.1 Kititems - {0}|ALKS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALKS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                    //lstReturn.Add(new CPluginVariable(String.Format("5.1 Kititems - {0}|ALKS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), typeof(enumBoolYesNo), status.status));
                                }
                        }
                        else if (parameters[0].Equals("GADGET")) // GADGET
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].kititems.GADGET.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].kititems.GADGET.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("5.2 Kititems - {0}|ALKS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALKS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                        else if (parameters[0].Equals("GRENADE")) // GRENADE
                        {
                            if (limits != null && limits.Count > 0)
                                if (limits[limit_index].kititems.GRENADE.Any(weapon => weapon.Key == pair.Key))
                                {
                                    Status status = limits[limit_index].kititems.GRENADE.Find(x => (x.Key == pair.Key));
                                    lstReturn.Add(new CPluginVariable(String.Format("5.3 Kititems - {0}|ALKS-{1} | {2}", parameters[0], pair.Key.PadLeft(10, '0'), parameters[1]), String.Format("enum.ALKS-{0} | {1}({2})", pair.Key.PadLeft(10, '0'), parameters[1], enumtext), SpawningEnum[(int)status.status]));
                                }
                        }
                    }
                #endregion
            }

            return lstReturn;
        }
        public List<CPluginVariable> GetPluginVariables()
        {
            return GetDisplayPluginVariables();
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            Boolean savexml = false;
            if (Regex.Match(strVariable, @"Debug level").Success)
            {
                int tmp = 2;
                int.TryParse(strValue, out tmp);
                fDebugLevel = tmp;
            }
            else if (Regex.Match(strVariable, @"Show OnKill forbidden users").Success)
            {
                bool tmp = false;
                bool.TryParse(strValue, out tmp);
                m_DisplayOnKill = tmp;
            }
            else if (strVariable.Equals("Wait Time OnPlayerJoin [Seconds]"))
            {
                int tmp = 30;
                int.TryParse(strValue, out tmp);
                m_waitOnPlayerJoin = tmp;
            }
            else if (strVariable.Equals("Wait Time OnLevelLoaded [Seconds]"))
            {
                int tmp = 20;
                int.TryParse(strValue, out tmp);
                m_waitOnLevelLoaded = tmp;
            }
            else if (strVariable.Equals("Wait Time OnSpawn [Seconds]"))
            {
                int tmp = 5;
                int.TryParse(strValue, out tmp);
                m_waitOnSpawn = tmp;
            }
            else if (Regex.Match(strVariable, @"Category").Success)
            {
                int tmp = 0;
                tmp = Array.IndexOf(CategoryEnum, strValue);
                if (tmp == -1)
                    tmp = 0;
                categoryToShow = tmp;
            }
            else if (Regex.Match(strVariable, @"Add new limit").Success)
            {
                if(strValue.Equals("Yes"))
                {
                    limits.Add(new compact());
                    limit_index = limits.Count - 1;
                    PopulateWeapons();
                    PopulateAccessories();
                    PopulateKititems();

                    //ExecuteCommand("procon.protected.plugins.setVariable", GetPluginName(), "Delete limit", "...");
                    //ExecuteCommand("procon.protected.plugins.setVariable", GetPluginName(), "Currently shown limit", String.Format("{0} of {1}", (limit_index + 1), limits.Count));

                    savexml = true;
                }
            }
            else if (Regex.Match(strVariable, @"Delete limit").Success)
            {
                int tmp = -1;
                int.TryParse(Regex.Match(strValue, @"\d+").Value, out tmp); tmp--;
                if (tmp >= 0 && limit_index < limits.Count)
                    limits.RemoveAt(tmp);

                limit_index = 0;

                savexml = true;
            }
            else if (Regex.Match(strVariable, @"Currently shown limit").Success)
            {
                int tmp = -1;
                int.TryParse(Regex.Match(strValue, @"\d+").Value, out tmp); tmp--;
                if (tmp >= 0 && limit_index < limits.Count)
                    limit_index = tmp;

                savexml = true;
            }
            else if (Regex.Match(strVariable, @"Limit Enabled").Success)
            {
                bool tmp = true;
                bool.TryParse(strValue, out tmp);
                limits[limit_index].enabled = tmp;

                savexml = true;
            }
            else if (Regex.Match(strVariable, @"Limit Name").Success)
            {
                limits[limit_index].name = strValue;

                savexml = true;
            }
            else if (Regex.Match(strVariable, @"Limit Maps").Success)
            {
                limits[limit_index].maps = new List<string>(CPluginVariable.DecodeStringArray(strValue));

                savexml = true;
            }
            else if (Regex.Match(strVariable, @"Limit Whitelist").Success)
            {
                limits[limit_index].Whitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));

                savexml = true;
            }
            else if (Regex.Match(strVariable, @"Spawning allowed").Success) // 2.3 Limit Settings|Spawning allowed
            {
                List<String> Keys = new List<String>(CPluginVariable.DecodeStringArray(strValue));
                if (SetMultipleKeys(Keys.ToArray(), Spawning.Allow))
                    savexml = true;
            }
            else if (Regex.Match(strVariable, @"Spawning not allowed").Success) // 2.3 Limit Settings|Spawning not allowed
            {
                List<String> Keys = new List<String>(CPluginVariable.DecodeStringArray(strValue));
                if (SetMultipleKeys(Keys.ToArray(), Spawning.Disallow))
                    savexml = true;
            }
            else if (Regex.Match(strVariable, @"One spawn allowed").Success) // 2.3 Limit Settings|One spawn allowed
            {
                List<String> Keys = new List<String>(CPluginVariable.DecodeStringArray(strValue));
                if (SetMultipleKeys(Keys.ToArray(), Spawning.DisallowAfterOne))
                    savexml = true;
            }
            else if (strVariable.StartsWith("ALWS-"))
            {
                // Assault Rifles   // Carbines     // DMRs             // Handguns     // LMGs 
                // PDWs             // Shotguns     // Sniper Rifles    // Special

                #region Weapons
                String[] parameters = strVariable.Split('|');
                String key = parameters[0].TrimStart(new char[] { 'A', 'L', 'W', 'S', '-' }).TrimStart('0').Trim();

                if (limits[limit_index].weapons.assault_rifles.Any(weapon => weapon.Key == key)) // Assault Rifles
                {
                    limits[limit_index].weapons.assault_rifles.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.carbines.Any(weapon => weapon.Key == key)) // Carbines
                {
                    limits[limit_index].weapons.carbines.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.dmrs.Any(weapon => weapon.Key == key)) // DMRs
                {
                    limits[limit_index].weapons.dmrs.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.handguns.Any(weapon => weapon.Key == key)) // Handguns
                {
                    limits[limit_index].weapons.handguns.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.lmgs.Any(weapon => weapon.Key == key)) // LMGs
                {
                    limits[limit_index].weapons.lmgs.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.pdws.Any(weapon => weapon.Key == key)) // PDWs
                {
                    limits[limit_index].weapons.pdws.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.shotguns.Any(weapon => weapon.Key == key)) // Shotguns
                {
                    limits[limit_index].weapons.shotguns.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.sniper_rifles.Any(weapon => weapon.Key == key)) // Sniper Rifles
                {
                    limits[limit_index].weapons.sniper_rifles.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].weapons.special.Any(weapon => weapon.Key == key)) // Special
                {
                    limits[limit_index].weapons.special.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                #endregion

                savexml = true;
            }
            else if (strVariable.StartsWith("ALAS-"))
            {
                // ACCESSORY    // AMMO         // AUXILIARY    // BARREL
                // CLOSE_RANGE  // MEDIUM_RANGE // LONG_RANGE   // UNDERBARREL

                #region Accessories
                String[] parameters = strVariable.Split('|');
                String key = parameters[0].TrimStart(new char[] { 'A', 'L', 'W', 'S', '-' }).TrimStart('0').Trim();

                if (limits[limit_index].accessories.ACCESSORY.Any(param => param.Key == key)) // ACCESSORY
                {
                    limits[limit_index].accessories.ACCESSORY.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].accessories.AMMO.Any(param => param.Key == key)) // AMMO
                {
                    limits[limit_index].accessories.AMMO.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].accessories.AUXILIARY.Any(param => param.Key == key)) // AUXILIARY
                {
                    limits[limit_index].accessories.AUXILIARY.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].accessories.BARREL.Any(param => param.Key == key)) // BARREL
                {
                    limits[limit_index].accessories.BARREL.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].accessories.CLOSE_RANGE.Any(param => param.Key == key)) // CLOSE_RANGE
                {
                    limits[limit_index].accessories.CLOSE_RANGE.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].accessories.MEDIUM_RANGE.Any(param => param.Key == key)) // MEDIUM_RANGE
                {
                    limits[limit_index].accessories.MEDIUM_RANGE.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].accessories.LONG_RANGE.Any(param => param.Key == key)) // LONG_RANGE
                {
                    limits[limit_index].accessories.LONG_RANGE.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].accessories.UNDERBARREL.Any(param => param.Key == key)) // UNDERBARREL
                {
                    limits[limit_index].accessories.UNDERBARREL.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                #endregion

                savexml = true;
            }
            else if (strVariable.StartsWith("ALKS-"))
            {
                // FIELD_UPGRADES   // GADGET  // GRENADE

                #region Kititems
                String[] parameters = strVariable.Split('|');
                String key = parameters[0].TrimStart(new char[] { 'A', 'L', 'K', 'S', '-' }).TrimStart('0').Trim();

                if (limits[limit_index].kititems.FIELD_UPGRADES.Any(param => param.Key == key)) // FIELD_UPGRADES
                {
                    limits[limit_index].kititems.FIELD_UPGRADES.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].kititems.GADGET.Any(param => param.Key == key)) // GADGET
                {
                    limits[limit_index].kititems.GADGET.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                else if (limits[limit_index].kititems.GRENADE.Any(param => param.Key == key)) // GRENADE
                {
                    limits[limit_index].kititems.GRENADE.Find(x => (x.Key == key)).status = (Spawning)Array.IndexOf(SpawningEnum, strValue);
                }
                #endregion

                savexml = true;
            }
            if (savexml)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<compact>));
                using (TextWriter writer = new StreamWriter(Path.Combine(base_path, @"LoadoutEnforcer\loadoutenforcer_settings.xml")))
                {
                    serializer.Serialize(writer, limits);
                }
                savexml = false;
            }
        }
        private bool SetMultipleKeys(String[] Keys, Spawning Status)
        {
            try
            {
                if (Keys.Length == 1 && Keys[0].Equals(""))
                    return (false);

                foreach (String _key in Keys)
                {
                    String key = _key.TrimStart('0').Trim();

                    // Assault Rifles   // Carbines     // DMRs             // Handguns     // LMGs 
                    // PDWs             // Shotguns     // Sniper Rifles    // Special
                    #region Weapons
                    if (limits[limit_index].weapons.assault_rifles.Any(weapon => weapon.Key == key)) // Assault Rifles
                    {
                        if(compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.assault_rifles.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.carbines.Any(weapon => weapon.Key == key)) // Carbines
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.carbines.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.dmrs.Any(weapon => weapon.Key == key)) // DMRs
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.dmrs.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.handguns.Any(weapon => weapon.Key == key)) // Handguns
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.handguns.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.lmgs.Any(weapon => weapon.Key == key)) // LMGs
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.lmgs.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.pdws.Any(weapon => weapon.Key == key)) // PDWs
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.pdws.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.shotguns.Any(weapon => weapon.Key == key)) // Shotguns
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.shotguns.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.sniper_rifles.Any(weapon => weapon.Key == key)) // Sniper Rifles
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.sniper_rifles.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].weapons.special.Any(weapon => weapon.Key == key)) // Special
                    {
                        if (compact_weapons[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].weapons.special.Find(x => (x.Key == key)).status = Status;
                    }
                    #endregion

                    // ACCESSORY    // AMMO         // AUXILIARY    // BARREL
                    // CLOSE_RANGE  // MEDIUM_RANGE // LONG_RANGE   // UNDERBARREL
                    #region Accessories
                    else if (limits[limit_index].accessories.ACCESSORY.Any(param => param.Key == key)) // ACCESSORY
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.ACCESSORY.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].accessories.AMMO.Any(param => param.Key == key)) // AMMO
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.AMMO.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].accessories.AUXILIARY.Any(param => param.Key == key)) // AUXILIARY
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.AUXILIARY.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].accessories.BARREL.Any(param => param.Key == key)) // BARREL
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.BARREL.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].accessories.CLOSE_RANGE.Any(param => param.Key == key)) // CLOSE_RANGE
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.CLOSE_RANGE.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].accessories.MEDIUM_RANGE.Any(param => param.Key == key)) // MEDIUM_RANGE
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.MEDIUM_RANGE.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].accessories.LONG_RANGE.Any(param => param.Key == key)) // LONG_RANGE
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.LONG_RANGE.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].accessories.UNDERBARREL.Any(param => param.Key == key)) // UNDERBARREL
                    {
                        if (compact_accessories[key].Split('|').Length < 4 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].accessories.UNDERBARREL.Find(x => (x.Key == key)).status = Status;
                    }
                    #endregion

                    // FIELD_UPGRADES   // GADGET  // GRENADE
                    #region Kititems
                    else if (limits[limit_index].kititems.FIELD_UPGRADES.Any(param => param.Key == key)) // FIELD_UPGRADES
                    {
                        if (compact_kititems[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].kititems.FIELD_UPGRADES.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].kititems.GADGET.Any(param => param.Key == key)) // GADGET
                    {
                        if (compact_kititems[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].kititems.GADGET.Find(x => (x.Key == key)).status = Status;
                    }
                    else if (limits[limit_index].kititems.GRENADE.Any(param => param.Key == key)) // GRENADE
                    {
                        if (compact_kititems[key].Split('|').Length < 3 && Status == Spawning.DisallowAfterOne)
                            continue;
                        limits[limit_index].kititems.GRENADE.Find(x => (x.Key == key)).status = Status;
                    }
                    #endregion
                }
                return (true);
            }
            catch (Exception e)
            {
                ConsoleError(String.Format("^1{0}", e));
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(String.Empty);
                }
                return (false);
            }
        }

        #region CURRENTLY UNNECESSARY
        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            port = strPort;
            host = strHostName;
            log_path = makeRelativePath(this.GetType().Name + "_" + port + "_" + host + ".log");

            this.RegisterEvents(this.GetType().Name, "OnListPlayers", "OnPlayerJoin", "OnPlayerLeft", "OnPlayerKilled", "OnPlayerSpawned", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnRoundOverPlayers", "OnRoundOver", "OnLoadingLevel", "OnLevelLoaded", "OnCurrentLevel");
        }

        public void OnPluginEnable()
        {
            if (!Reflector.Reflect())
            {
                ConsoleWrite("^1Error compiling the LoadoutEnforcer\\xfileFIN.cs");
                OnPluginDisable();
                return;
            }
            fIsEnabled = true;
            ConsoleWrite("^2Enabled!");
        }

        public void OnPluginDisable()
        {
            fIsEnabled = false;
            PlayersList = null;
            ConsoleWrite("^1Disabled =(");
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            PlayersList = players;
        }

        public override void OnPlayerJoin(string soldierName)
        {
            String solttu = soldierName;
            Thread onjoin = new Thread(new ThreadStart(delegate ()
            {
                try
                {
                    Thread.Sleep(m_waitOnPlayerJoin * 1000);
                    fetchPlayerKit(solttu);
                    checkLimits(solttu, player_kits[solttu], false, false, false);
                    player_kits[solttu].FIRST_SPAWN_NOTED = false;
                }
                catch (Exception e)
                {
                    ConsoleException(e.ToString());
                    using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                    {
                        sw.WriteLine(DateTime.Now.ToString());
                        sw.WriteLine(e.ToString());
                        sw.WriteLine(String.Empty);
                    }
                }
            }));

            onjoin.IsBackground = true;
            onjoin.Name = "OnPlayerJoin";
            onjoin.Start();
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            if (playerInfo == null)
                return;

            if (PlayersList == null)
                return;

            String solttu = playerInfo.SoldierName;
            Player_Kit pKit = player_kits[solttu];

            if (pKit.LOADOUT_PREVIOUSLY_BAD)
            {
                ServerCommand("admin.say", StripModifiers(E(String.Format("/{0} left the server without fixing his loadout!", solttu))), "all");
                Boolean no_file = false;
                if (!File.Exists(log_path))
                    no_file = true;
                using (StreamWriter sw = new StreamWriter(log_path, true))
                {
                    if (no_file)
                        sw.WriteLine("DateTime,Faulty Loadout,NO SPAWN,NO KILL,Grenades Unlocked,M67 Equipped");
                    Boolean m67_nade_equipped = false;
                    if (!pKit.UNLOCKS.sc_handgrenades_unlocked && pKit.GRENADES != "2670747868")
                        m67_nade_equipped = true;

                    sw.WriteLine(String.Format("{5},{0},{1},{2},{3},{4}", solttu, 
                        String.Join(";", pKit.NO_SPAWN.ToArray()), 
                        String.Join(";", pKit.ONE_SPAWN.ToArray()),
                        pKit.UNLOCKS.sc_handgrenades_unlocked.ToString(),
                        m67_nade_equipped.ToString(),
                        DateTime.Now.ToString()
                        ));
                }
            }

            PlayersList.Remove(playerInfo);

            if (player_kits.ContainsKey(solttu))
                player_kits.Remove(solttu);
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            String solttu = kKillerVictimDetails.Killer.SoldierName;
            String dType = kKillerVictimDetails.DamageType;
            checkOnKill(solttu, dType, player_kits[solttu]);
            ServerCommand("listPlayers", "all");
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            String solttu = soldierName;
            GetAvailableMemory();

            if(m_spawnCount < 1)
                ServerCommand("currentLevel");
            m_spawnCount++;

            ServerCommand("listPlayers", "all");

            if (player_kits.ContainsKey(solttu))
                if (player_kits[solttu].LOADOUT_PREVIOUSLY_BAD)
                    ServerCommand("admin.yell", StripModifiers(E("Checking loadout!")), m_waitOnSpawn.ToString(), "player", solttu);

            Thread delayed = new Thread(new ThreadStart(delegate ()
            {
                Thread.Sleep(m_waitOnSpawn * 1000);
                fetchPlayerKit(solttu);
                checkLimits(solttu, player_kits[solttu], true, false, true);
            }));

            delayed.IsBackground = true;
            delayed.Name = "OnSpawn_LoadoutCheck";
            delayed.Start();
        }

        public override void OnGlobalChat(string speaker, string message)
        {
            ingame_Commands(speaker, message);
        }

        public override void OnTeamChat(string speaker, string message, int teamId)
        {
            ingame_Commands(speaker, message);
        }

        public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
            ingame_Commands(speaker, message);
        }

        public override void OnRoundOverPlayers(List<CPlayerInfo> players) { PlayersList = players; }

        public override void OnRoundOver(int winningTeamId)
        {
            m_spawnCount = 0;
        }

        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal) { currentLevel = mapFileName; }

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal)
        {
            //Thread delayed = new Thread(new ThreadStart(delegate ()
            //{
            //Thread.Sleep(m_waitOnLevelLoaded * 1000);
            try
            {
                currentLevel = mapFileName;
                foreach (CPlayerInfo player in PlayersList)
                {
                    if (player_kits.ContainsKey(player.SoldierName))
                        player_kits[player.SoldierName].FIRST_SPAWN_NOTED = false;
                    fetchPlayerKit(player.SoldierName);
                    checkLimits(player.SoldierName, player_kits[player.SoldierName], false, false, false);
                    player_kits[player.SoldierName].FIRST_SPAWN_NOTED = false;
                    if (player_kits[player.SoldierName].LOADOUT_PREVIOUSLY_BAD)
                    {
                        ServerCommand("admin.yell", StripModifiers(E("Please update your loadout for this maps requirements! (Command /myloadout)")), "10", "player", player.SoldierName);
                        ServerCommand("admin.say", StripModifiers(E("[DON'T KILL] = Don't kill with this | [DON'T SPAWN] = Don't spawn with this")), "player", player.SoldierName);
                    }
                }
            }
            catch (Exception e)
            {
                DebugWrite(e.ToString(), 2);
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(String.Empty);
                }
            }
            //}));

            //delayed.IsBackground = true;
            //delayed.Name = "OnLevelLoaded_LoadoutCheck";
            //delayed.Start();
        } // BF3

        public override void OnCurrentLevel(string mapFileName)
        {
            currentLevel = mapFileName;
        }

        #endregion

        private void ingame_Commands(string speaker, string message)
        {
            Char[] Prefixes = new Char[] { '!', '@', '#', '/' };
            Boolean iscommand = false;
            foreach (Char prefix in Prefixes)
                if (message.StartsWith(prefix.ToString()))
                {
                    iscommand = true;
                    break;
                }

            if (!iscommand)
                return;

            Match cmd_loadout_other = Regex.Match(message, @"[!@#/]myl\s+([^\s]+)", RegexOptions.IgnoreCase);
            Match cmd_loadout_self = Regex.Match(message, @"[!@#/]\bmyl[a-zA-Z]*", RegexOptions.IgnoreCase);
            Match cmd_loadouts_enabled = Regex.Match(message, @"[!@#/]loadoutcheck", RegexOptions.IgnoreCase);
            Match cmd_mapName = Regex.Match(message, @"[!@#/]mapname", RegexOptions.IgnoreCase);
            Match cmd_loadout_violations_other = Regex.Match(message, @"[!@#/]violations\s+([^\s]+)", RegexOptions.IgnoreCase);
            Match cmd_loadout_violations_all = Regex.Match(message, @"[!@#/]violations", RegexOptions.IgnoreCase);
            Match cmd_reset_violations = Regex.Match(message, @"[!@#/]remviol\s+([^\s]+)", RegexOptions.IgnoreCase);
            //Match cmd_mapName = Regex.Match(message, @"[!@#/]mapname", RegexOptions.IgnoreCase);

            #region cmd_loadout_other
            if (cmd_loadout_other.Success)
            {
                Boolean permission = false;

                String target = BestPlayerMatch(message, speaker, cmd_loadout_other);
                if (String.IsNullOrEmpty(target))
                    return;

                CPrivileges cpSpeakerPrivs = this.GetAccountPrivileges(speaker);
                if (cpSpeakerPrivs.CanKillPlayers)
                    permission = true;

                if (!permission)
                {
                    ServerCommand("admin.say", "Not enough privileges to issue this command for another user!", "player", speaker);
                    return;
                }
                else
                {
                    fetchPlayerKit(target);
                    checkLimits(target, speaker, player_kits[target], false, true, false);
                }
                return;
            } 
            #endregion
            else if (cmd_loadout_self.Success)
            {
                fetchPlayerKit(speaker);
                checkLimits(speaker, player_kits[speaker], false, true, false);
                return;
            }
            #region cmd_loadouts_enabled
            else if (cmd_loadouts_enabled.Success)
            {
                Boolean permission = false;

                CPrivileges cpSpeakerPrivs = this.GetAccountPrivileges(speaker);
                if (cpSpeakerPrivs.CanIssueLimitedProconPluginCommands)
                    permission = true;

                if (!permission)
                {
                    ServerCommand("admin.say", "Not enough privileges to issue this command!", "player", speaker);
                    return;
                }

                int index = 0;
                foreach (compact limit in limits)
                {
                    if (limit.enabled)
                    {
                        if ((limit.maps.Contains(currentLevel) || ListEmpty(limit.maps)))
                            ServerCommand("admin.say", String.Format("{2}/{1} {0} [ENABLED]", limit.name, limits.Count, (index + 1)), "player", speaker);
                        else
                            ServerCommand("admin.say", String.Format("{2}/{1} {0} [ENABLED] (NOT IN CURRENT MAP)", limit.name, limits.Count, (index + 1)), "player", speaker);
                    }
                    else
                        ServerCommand("admin.say", String.Format("{2}/{1} {0} [DISABLED]", limit.name, limits.Count, (index + 1)), "player", speaker);
                    index++;
                }
                return;
            } 
            #endregion
            else if (cmd_mapName.Success)
            {
                ServerCommand("currentLevel");
                ServerCommand("admin.say", String.Format("Current Map: {0}", currentLevel), "player", speaker);
            }
            #region cmd_loadout_violations_other
            else if (cmd_loadout_violations_other.Success)
            {
                Boolean permission = false;

                String target = BestPlayerMatch(message, speaker, cmd_loadout_violations_other);
                if (String.IsNullOrEmpty(target))
                    return;

                CPrivileges cpAccount = null;
                cpAccount = this.GetAccountPrivileges(speaker);
                if (cpAccount != null && cpAccount.PrivilegesFlags > 0)
                    permission = true;

                if (!permission)
                {
                    ServerCommand("admin.say", "Not enough privileges to issue this command for another user!", "player", speaker);
                    return;
                }
                else
                {
                    if (!player_kits.ContainsKey(target))
                        return;
                    Player_Kit tmp = player_kits[target];
                    ServerCommand("admin.say", String.Format("Violations for {0} is {1}", target, tmp.VIOLATIONS), "player", speaker);
                    for(int i = 0; i < tmp.VIOLATION_REASONS.Count;i++)
                        ServerCommand("admin.say", String.Format("Violation {0}/{1} {2}", (i + 1), tmp.VIOLATION_REASONS.Count, tmp.VIOLATION_REASONS[i]), "player", speaker);
                }
                return;
            }
            #endregion
            #region cmd_loadout_violations_all
            else if (cmd_loadout_violations_all.Success)
            {
                Boolean permission = false;

                CPrivileges cpAccount = null;
                cpAccount = this.GetAccountPrivileges(speaker);
                if (cpAccount != null && cpAccount.PrivilegesFlags > 0)
                    permission = true;

                if (!permission)
                {
                    ServerCommand("admin.say", "Not enough privileges to issue this command for another user!", "player", speaker);
                    return;
                }
                else
                {
                    Thread violations = new Thread(new ThreadStart(delegate ()
                    {
                        Dictionary<String, Player_Kit> tmp = player_kits;
                        foreach (KeyValuePair<string, Player_Kit> kvp in tmp)
                        {
                            if(kvp.Value.VIOLATIONS > 0)
                            {
                                ServerCommand("admin.say", String.Format("Violations for {0} is {1}", kvp.Key, kvp.Value.VIOLATIONS), "player", speaker);
                                Thread.Sleep(2 * 1000);
                            }
                        }
                    }));

                    violations.IsBackground = true;
                    violations.Name = "violation_check";
                    violations.Start();
                }
                return;
            }
            #endregion
            #region cmd_reset_violations
            else if (cmd_reset_violations.Success)
            {
                Boolean permission = false;

                String target = BestPlayerMatch(message, speaker, cmd_reset_violations);
                if (String.IsNullOrEmpty(target))
                    return;

                CPrivileges cpSpeakerPrivs = this.GetAccountPrivileges(speaker);
                if (cpSpeakerPrivs.CanKillPlayers)
                    permission = true;

                if (!permission)
                {
                    ServerCommand("admin.say", "Not enough privileges to issue this command!", "player", speaker);
                    return;
                }
                else
                {
                    if (!player_kits.ContainsKey(target))
                    {
                        ServerCommand("admin.say", String.Format("Didn't find player_kit for {0}", target), "player", speaker);
                        return;
                    }
                    if(player_kits[target].VIOLATIONS > 0)
                    {
                        player_kits[target].VIOLATIONS = 0;
                        player_kits[target].VIOLATION_REASONS = new List<String>();
                        ServerCommand("admin.say", String.Format("Violation count for {0} is now {1}", target, speaker), "player", speaker);
                        ServerCommand("admin.say", String.Format("Your violation count has been reseted by {0}, don't break them anymore!", speaker), "player", target);
                        return;
                    }
                    else
                    {
                        ServerCommand("admin.say", String.Format("Violation count for {0} was already {1}", target, speaker), "player", speaker);
                        return;
                    }
                }
            }
            #endregion
        }

        private Dictionary<String, String> getRCONtoWeapon()
        {
            Dictionary<String, String> dic = new Dictionary<String, String>();

            foreach (KeyValuePair<String, String> kvp in compact_weapons)
            {
                String rcon = getWeaponName(kvp.Key, true);
                if (!String.IsNullOrEmpty(rcon))
                    if(!dic.ContainsKey(rcon))
                        dic.Add(rcon, getWeaponName(kvp.Key, false));
            }
            foreach (KeyValuePair<String, String> kvp in compact_accessories)
            {
                String rcon = getAccessoryName(kvp.Key, true);
                if (!String.IsNullOrEmpty(rcon))
                    if (!dic.ContainsKey(rcon))
                        dic.Add(rcon, getAccessoryName(kvp.Key, false));
            }
            foreach (KeyValuePair<String, String> kvp in compact_kititems)
            {
                String rcon = getKititemsName(kvp.Key, true);
                if (!String.IsNullOrEmpty(rcon))
                    if (!dic.ContainsKey(rcon))
                        dic.Add(rcon, getKititemsName(kvp.Key, false));
            }

            return (dic);
        }

        private void checkOnKill(string soldierName, string DamageType, Player_Kit inventory)
        {
            if (inventory.LAST_KILL != null)
            {
                TimeSpan sincelast = TimeSpan.FromTicks(DateTime.Now.Ticks - inventory.LAST_KILL.Ticks);
                if (sincelast.TotalSeconds < 2) // Count double/multi kills as one
                    return;
            }

            inventory.LAST_KILL = DateTime.Now;
            bool Kill_soldier = false;
            foreach (compact limit in limits)
            {
                if (limit.enabled && (limit.maps.Contains(currentLevel) || ListEmpty(limit.maps)))
                {
                    if (limit.Whitelist.Contains(soldierName))
                        continue;

                    Dictionary<String, Dictionary<String, String>> OneSpawn = getRCONMisuseCodes(limit);
                    // Weapons
                    Dictionary<String, String> OneSpawn_weapons = OneSpawn["weapons"];
                    foreach (KeyValuePair<String, String> kvp in OneSpawn_weapons)
                        if (DamageType == kvp.Key) { Kill_soldier = true; inventory.VIOLATIONS++; }

                    // Accessories
                    //Dictionary<String, String> OneSpawn_accessories = OneSpawn["accessories"];
                    //foreach (KeyValuePair<String, String> kvp in OneSpawn_accessories)
                    //    if (DamageType == kvp.Key) { Kill_soldier = true; inventory.VIOLATIONS++; }

                    // Kititems
                    Dictionary<String, String> OneSpawn_kititems = OneSpawn["kititems"];
                    foreach (KeyValuePair<String, String> kvp in OneSpawn_kititems)
                        if (DamageType == kvp.Key) { Kill_soldier = true; inventory.VIOLATIONS++; }
                }
            }
            if(Kill_soldier)
            {
                /* CONFIGURABLE VARIABLES */
                int warnings = 1; // How many warnings until kicked (Default value that will be used if no match in warning_list)
                KeyValuePair<int, int>[] warning_list = new KeyValuePair<int, int>[] // Players, Warnings
                {
            new KeyValuePair<int, int>(10, 3),
            new KeyValuePair<int, int>(20, 2),
            new KeyValuePair<int, int>(32, 1)
                };
                /* END OF CONFIGURABLE VARIABLES */

                // Process the warning count
                for (int i = 0; i < warning_list.Length; i++)
                    if (PlayersList.Count <= warning_list[i].Key)
                    {
                        warnings = warning_list[i].Value;
                        break;
                    }

                if(inventory.VIOLATIONS > warnings)
                {
                    ServerCommand("admin.kickPlayer", soldierName, "You've been kicked for using weapons/attachments that are not allowed!");
                    ServerCommand("admin.say", StripModifiers(E(String.Format("{0} kicked for ignoring weapon/attachment restrictions!", soldierName))), "all");
                    inventory.VIOLATIONS = 0;
                    return;
                }

                if (rcon_to_weapon == null || rcon_to_weapon.Count < 1)
                    rcon_to_weapon = getRCONtoWeapon();
                String kill_weapon = "UNKNOWN";
                if (rcon_to_weapon.ContainsKey(DamageType))
                    kill_weapon = rcon_to_weapon[DamageType];
                ServerCommand("admin.killPlayer", soldierName);
                ServerCommand("admin.yell", StripModifiers(E(String.Format("You've been killed for using {0}", kill_weapon))), "20", "player", soldierName);
                if (m_DisplayOnKill)
                    ServerCommand("admin.say", StripModifiers(E(String.Format("{0} killed for using {1}", soldierName, kill_weapon))), "all");
                else
                    ServerCommand("admin.say", StripModifiers(E(String.Format("{0} killed for using {1}", soldierName, kill_weapon))), "player", soldierName);
                inventory.VIOLATION_REASONS.Add(kill_weapon);
                checkLimits(soldierName, inventory, false, true, false);
            }
        }

        private void checkLimits(string soldierName, Player_Kit Inventory, bool OnSpawn, bool UserQuery, bool yell_message)
        {
            checkLimits(soldierName, soldierName, Inventory, OnSpawn, UserQuery, yell_message);
        }
        private void checkLimits(string soldierName, string resultSoldierName, Player_Kit Inventory, bool OnSpawn, bool UserQuery, bool yell_message) // Check Inventory and punish if needed
        {
            if (Inventory.ERROR)
                return;

            List<String> toRemoveNoSpawn = new List<String>();
            List<String> toRemoveOneSpawn = new List<String>();
            String yell = null;
            Inventory.UNLOCKS.sc_handgrenades_unlocked = true;

            #region CheckForMatches
            foreach (compact limit in limits)
            {
                if (limit.enabled && (limit.maps.Contains(currentLevel) || ListEmpty(limit.maps)))
                {
                    if (limit.Whitelist.Contains(soldierName))
                        continue;

                    Dictionary<String, Dictionary<String, String>> NoSpawn = getSpawningNotAllowed(limit);
                    Dictionary<String, Dictionary<String, String>> OneSpawn = getOneMisuseAllowed(limit);

                    // Weapons
                    Dictionary<String, String> NoSpawn_weapons = NoSpawn["weapons"];
                    foreach (KeyValuePair<String, String> kvp in NoSpawn_weapons)
                        if (Inventory.PRIMARY_WEAPON.Key == kvp.Key || Inventory.SIDEARM.Key == kvp.Key || Inventory.MELEE == kvp.Key)
                            toRemoveNoSpawn.Add(kvp.Value);
                    Dictionary<String, String> OneSpawn_weapons = OneSpawn["weapons"];
                    foreach (KeyValuePair<String, String> kvp in OneSpawn_weapons)
                        if (Inventory.PRIMARY_WEAPON.Key == kvp.Key || Inventory.SIDEARM.Key == kvp.Key || Inventory.MELEE == kvp.Key)
                            toRemoveOneSpawn.Add(kvp.Value);

                    // Accessories
                    Dictionary<String, String> NoSpawn_accessories = NoSpawn["accessories"];
                    foreach (KeyValuePair<String, String> kvp in NoSpawn_accessories)
                        if (Inventory.PRIMARY_WEAPON.OPTIC == kvp.Key || Inventory.PRIMARY_WEAPON.ACCESSORY == kvp.Key || Inventory.PRIMARY_WEAPON.BARREL == kvp.Key || Inventory.PRIMARY_WEAPON.UNDERBARREL == kvp.Key || Inventory.PRIMARY_WEAPON.PAINT == kvp.Key || Inventory.PRIMARY_WEAPON.AMMO == kvp.Key ||
                            Inventory.SIDEARM.OPTIC == kvp.Key || Inventory.SIDEARM.ACCESSORY == kvp.Key || Inventory.SIDEARM.BARREL == kvp.Key || Inventory.SIDEARM.PAINT == kvp.Key)
                            toRemoveNoSpawn.Add(kvp.Value);
                    Dictionary<String, String> OneSpawn_accessories = OneSpawn["accessories"];
                    foreach (KeyValuePair<String, String> kvp in OneSpawn_accessories)
                        if (Inventory.PRIMARY_WEAPON.OPTIC == kvp.Key || Inventory.PRIMARY_WEAPON.ACCESSORY == kvp.Key || Inventory.PRIMARY_WEAPON.BARREL == kvp.Key || Inventory.PRIMARY_WEAPON.UNDERBARREL == kvp.Key || Inventory.PRIMARY_WEAPON.PAINT == kvp.Key || Inventory.PRIMARY_WEAPON.AMMO == kvp.Key ||
                            Inventory.SIDEARM.OPTIC == kvp.Key || Inventory.SIDEARM.ACCESSORY == kvp.Key || Inventory.SIDEARM.BARREL == kvp.Key || Inventory.SIDEARM.PAINT == kvp.Key)
                            toRemoveOneSpawn.Add(kvp.Value);

                    // Kititems
                    Dictionary<String, String> NoSpawn_kititems = NoSpawn["kititems"];
                    foreach (KeyValuePair<String, String> kvp in NoSpawn_kititems)
                    {
                        if (Inventory.GADGET_1 == kvp.Key || Inventory.GADGET_2 == kvp.Key || Inventory.GRENADES == kvp.Key || Inventory.FIELD_UPGRADES == kvp.Key)
                            toRemoveNoSpawn.Add(kvp.Value);

                        // M67 FRAG         Default                 2670747868
                        // V40 MINI         500 Grenade Score       69312926
                        // RGO IMPACT       1,000 Grenade Score     3767777089
                        // M34 INCENDIARY   1,500 Grenade Score     2842275721
                        // M18 SMOKE        2,000 Grenade Score     3133964300
                        // M84 FLASHBANG    2,500 Grenade Score     1779756455
                        // HAND FLARE       3,000 Grenade Score     2916285594

                        if (kvp.Key == "69312926" && Inventory.UNLOCKS.sc_handgrenades < 500)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveNoSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "3767777089" && Inventory.UNLOCKS.sc_handgrenades < 1000)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveNoSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "2842275721" && Inventory.UNLOCKS.sc_handgrenades < 1500)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveNoSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "3133964300" && Inventory.UNLOCKS.sc_handgrenades < 2000)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveNoSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "1779756455" && Inventory.UNLOCKS.sc_handgrenades < 2500)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveNoSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "2916285594" && Inventory.UNLOCKS.sc_handgrenades < 3000)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveNoSpawn.Remove(kvp.Value);
                        }
                    }

                    if (NoSpawn_kititems != null && NoSpawn_kititems.Count > 0)
                        if (Inventory.GRENADES == "2670747868" && !Inventory.UNLOCKS.sc_handgrenades_unlocked)
                            if (toRemoveOneSpawn.Contains(NoSpawn_kititems["2670747868"]))
                                toRemoveNoSpawn.Remove(NoSpawn_kititems["2670747868"]);

                    Dictionary<String, String> OneSpawn_kititems = OneSpawn["kititems"];
                    foreach (KeyValuePair<String, String> kvp in OneSpawn_kititems)
                    {
                        if (Inventory.GADGET_1 == kvp.Key || Inventory.GADGET_2 == kvp.Key || Inventory.GRENADES == kvp.Key || Inventory.FIELD_UPGRADES == kvp.Key)
                            toRemoveOneSpawn.Add(kvp.Value);

                        // M67 FRAG         Default                 2670747868
                        // V40 MINI         500 Grenade Score       69312926
                        // RGO IMPACT       1,000 Grenade Score     3767777089
                        // M34 INCENDIARY   1,500 Grenade Score     2842275721
                        // M18 SMOKE        2,000 Grenade Score     3133964300
                        // M84 FLASHBANG    2,500 Grenade Score     1779756455
                        // HAND FLARE       3,000 Grenade Score     2916285594
                        if (kvp.Key == "69312926" && Inventory.UNLOCKS.sc_handgrenades < 500)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveOneSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "3767777089" && Inventory.UNLOCKS.sc_handgrenades < 1000)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveOneSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "2842275721" && Inventory.UNLOCKS.sc_handgrenades < 1500)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveOneSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "3133964300" && Inventory.UNLOCKS.sc_handgrenades < 2000)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveOneSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "1779756455" && Inventory.UNLOCKS.sc_handgrenades < 2500)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveOneSpawn.Remove(kvp.Value);
                        }
                        else if (kvp.Key == "2916285594" && Inventory.UNLOCKS.sc_handgrenades < 3000)
                        {
                            Inventory.UNLOCKS.sc_handgrenades_unlocked = false;
                            toRemoveOneSpawn.Remove(kvp.Value);
                        }
                    }

                    if (OneSpawn_kititems != null && OneSpawn_kititems.Count > 0)
                        if (Inventory.GRENADES == "2670747868" && !Inventory.UNLOCKS.sc_handgrenades_unlocked)
                            if (toRemoveOneSpawn.Contains(OneSpawn_kititems["2670747868"]))
                                toRemoveOneSpawn.Remove(OneSpawn_kititems["2670747868"]);
                }
            }
            #endregion

            Inventory.NO_SPAWN = toRemoveNoSpawn;
            Inventory.ONE_SPAWN = toRemoveOneSpawn;

            #region Punish/Print
            if(!Inventory.UNLOCKS.sc_handgrenades_unlocked && Inventory.GRENADES != "2670747868")
                ServerCommand("admin.say", "Please equip M67 FRAG and NEVER use it!", "player", resultSoldierName);
            if (toRemoveNoSpawn.Count > 0 && toRemoveOneSpawn.Count < 1)
            {
                Inventory.LOADOUT_PREVIOUSLY_BAD = true;
                ServerCommand("admin.killPlayer", soldierName);
                //ServerCommand("admin.say", "You're not allowed to SPAWN with the following item(s)!", "player", soldierName);
                for (int i = 0; i < toRemoveNoSpawn.Count; i++)
                    ServerCommand("admin.say", String.Format("[DON'T SPAWN] Remove {0}/{1} - {2}", (i + 1), toRemoveNoSpawn.Count, toRemoveNoSpawn[i]), "player", resultSoldierName);
                if (!yell_message)
                    return;

                yell = String.Format("Please remove, {0} from your loadout!", String.Join(", ", toRemoveNoSpawn.ToArray()));

                Thread delayed = new Thread(new ThreadStart(delegate ()
                {
                    Thread.Sleep(2 * 1000);
                    if (yell.Length < 256)
                        ServerCommand("admin.yell", StripModifiers(E(yell)), "10", "player", resultSoldierName);
                    else
                        ServerCommand("admin.yell", "Please fix your loadout! Use /myloadout to check what is wrong!", "10", "player", resultSoldierName);
                }));

                delayed.IsBackground = true;
                delayed.Name = "yell_delay";
                delayed.Start();
            }
            else if (toRemoveNoSpawn.Count > 0 || toRemoveOneSpawn.Count > 0)
            {
                if (toRemoveNoSpawn.Count > 0 || (toRemoveOneSpawn.Count > 0 && Inventory.VIOLATIONS > 0))
                {
                    Inventory.LOADOUT_PREVIOUSLY_BAD = true;
                    ServerCommand("admin.killPlayer", soldierName);
                    //ServerCommand("admin.say", "You're not allowed to SPAWN with the following item(s)!", "player", soldierName);

                    int violation_count = 0;
                    if (Inventory.VIOLATIONS > 0)
                        violation_count = (toRemoveNoSpawn.Count + toRemoveOneSpawn.Count);
                    else if(Inventory.VIOLATIONS < 1)
                        violation_count = toRemoveNoSpawn.Count;

                    for (int i = 0; i < toRemoveNoSpawn.Count; i++)
                        ServerCommand("admin.say", String.Format("[DON'T SPAWN] Remove {0}/{1} - {2}", (i + 1), violation_count, toRemoveNoSpawn[i]), "player", resultSoldierName);

                    if (toRemoveOneSpawn.Count > 0 && Inventory.VIOLATIONS > 0)
                        for (int i = 0; i < toRemoveOneSpawn.Count; i++)
                            ServerCommand("admin.say", String.Format("[DON'T SPAWN] Remove {0}/{1} - {2}", (i + 1), (toRemoveNoSpawn.Count + toRemoveOneSpawn.Count), toRemoveOneSpawn[i]), "player", resultSoldierName);

                    if (yell_message)
                    {
                        if (toRemoveNoSpawn.Count > 0 && (toRemoveOneSpawn.Count > 0 && Inventory.VIOLATIONS > 0))
                            yell = String.Format("Please remove, {0}, {1} from your loadout!", String.Join(", ", toRemoveNoSpawn.ToArray()), String.Join(", ", toRemoveOneSpawn.ToArray()));
                        else if (toRemoveNoSpawn.Count > 0 && toRemoveOneSpawn.Count < 1)
                            yell = String.Format("Please remove, {0} from your loadout!", String.Join(", ", toRemoveNoSpawn.ToArray()));
                        else if (toRemoveNoSpawn.Count > 0 && (toRemoveOneSpawn.Count > 0 && Inventory.VIOLATIONS < 1))
                            yell = String.Format("Please remove, {0} from your loadout!", String.Join(", ", toRemoveNoSpawn.ToArray()));
                        else if (toRemoveNoSpawn.Count < 1 && (toRemoveOneSpawn.Count > 0 && Inventory.VIOLATIONS > 0))
                            yell = String.Format("Please remove, {0} from your loadout!", String.Join(", ", toRemoveOneSpawn.ToArray()));

                        //System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        //{
                        //    Thread.Sleep(2 * 1000);
                        //    if (yell.Length < 256)
                        //        ServerCommand("admin.yell", StripModifiers(E(yell)), "20", "player", soldierName);
                        //    else
                        //        ServerCommand("admin.yell", "Please fix your loadout! Use !myloadout to check what is wrong!", "20", "player", soldierName);
                        //}, null);

                        Thread delayed = new Thread(new ThreadStart(delegate ()
                        {
                            Thread.Sleep(2 * 1000);
                            if (yell.Length < 256)
                                ServerCommand("admin.yell", StripModifiers(E(yell)), "10", "player", resultSoldierName);
                            else
                                ServerCommand("admin.yell", "Please fix your loadout! Use /myloadout to check what is wrong!", "10", "player", resultSoldierName);
                        }));

                        delayed.IsBackground = true;
                        delayed.Name = "yell_delay";
                        delayed.Start();
                    }
                }
                if(toRemoveOneSpawn.Count > 0 && Inventory.VIOLATIONS < 1 && (!Inventory.FIRST_SPAWN_NOTED || UserQuery))
                {
                    Inventory.FIRST_SPAWN_NOTED = true;
                    //ServerCommand("admin.say", "You are NOT allowed to KILL with the following item(s)!", "player", soldierName);
                    for (int i = 0; i < toRemoveOneSpawn.Count; i++)
                        ServerCommand("admin.say", String.Format("[DON'T KILL] {0}/{1} - {2}", (i + 1), toRemoveOneSpawn.Count, toRemoveOneSpawn[i]), "player", resultSoldierName);
                }
                if (Inventory.LOADOUT_PREVIOUSLY_BAD && toRemoveOneSpawn.Count < 1 && Inventory.VIOLATIONS < 1)
                {
                    Inventory.LOADOUT_PREVIOUSLY_BAD = false;
                    ServerCommand("admin.say", "Thank you for fixing your loadout!", "player", resultSoldierName);
                    ServerCommand("admin.yell", StripModifiers(E("Thank you for fixing your loadout!")), "5", "player", resultSoldierName);
                }
            }
            else if (Inventory.LOADOUT_PREVIOUSLY_BAD)
            {
                Inventory.LOADOUT_PREVIOUSLY_BAD = false;
                ServerCommand("admin.say", "Thank you for fixing your loadout!", "player", resultSoldierName);
                ServerCommand("admin.yell", StripModifiers(E("Thank you for fixing your loadout!")), "5", "player", resultSoldierName);
            }
            else if(!OnSpawn)
            {
                ServerCommand("admin.say", "Your loadout is OK!", "player", resultSoldierName);
            }
            #endregion
        }

        private String getWeaponName(String Key, bool rcon)
        {
            try
            {
                String param;
                if (rcon)
                {
                    compact_weapons.TryGetValue(Key, out param);
                    String[] RCON_Code = param.Split('|');
                    if (RCON_Code.Length == 3)
                        return (RCON_Code[2]);
                    else
                        return (String.Empty);
                }
                if (compact_weapons == null)
                    return ("UnkWeapon");

                compact_weapons.TryGetValue(Key, out param);
                String[] parameters = param.Split('|');
                if (parameters.Length > 1)
                    return (String.Format("{0} ({1})", parameters[1], parameters[0]));
                else
                    return ("UnkKititem");
            }
            catch (Exception e)
            {
                DebugWrite(e.ToString(), 3);
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(String.Empty);
                }
                return (String.Empty);
            }
        }
        private String getAccessoryName(String Key, bool rcon)
        {
            try
            {
                String param;
                if (rcon)
                {
                    compact_accessories.TryGetValue(Key, out param);
                    String[] RCON_Code = param.Split('|');
                    if (RCON_Code.Length == 4)
                        return (RCON_Code[3]);
                    else
                        return (String.Empty);
                }
                if (compact_accessories == null)
                    return ("UnkAccessory");

                compact_accessories.TryGetValue(Key, out param);
                String[] parameters = param.Split('|');
                if (parameters.Length > 1)
                    return (String.Format("{0} ({1})", parameters[1], parameters[0]));
                else
                    return ("UnkKititem");
            }
            catch (Exception e)
            {
                DebugWrite(e.ToString(), 3);
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(String.Empty);
                }
                return (String.Empty);
            }
        }
        private String getKititemsName(String Key, bool rcon)
        {
            try
            {
                String param;
                if (rcon)
                {
                    compact_kititems.TryGetValue(Key, out param);
                    String[] RCON_Code = param.Split('|');
                    if (RCON_Code.Length == 3)
                        return (RCON_Code[2]);
                    else
                        return (String.Empty);
                }
                if (compact_kititems == null)
                    return ("UnkKititem");

                compact_kititems.TryGetValue(Key, out param);
                String[] parameters = param.Split('|');
                if (parameters.Length > 1)
                    return (String.Format("{0} ({1})", parameters[1], parameters[0]));
                else
                    return ("UnkKititem");
            }
            catch (Exception e)
            {
                DebugWrite(e.ToString(), 3);
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(String.Empty);
                }
                return (String.Empty);
            }
        }

        private Dictionary<String, Dictionary<String, String>> getSpawningNotAllowed(compact limit)
        {
            Dictionary<String, String> Weapons = new Dictionary<String, String>();
            Dictionary<String, String> Accessories = new Dictionary<String, String>();
            Dictionary<String, String> Kititems = new Dictionary<String, String>();
            Dictionary<String, Dictionary<String, String>> keys = new Dictionary<String, Dictionary<String, String>>();

            // Weapons
            var weapon_properties = limit.weapons.GetType().GetProperties();
            for (int i = 0; i < weapon_properties.Length; i++)
                foreach (Status status in weapon_properties[i].GetValue(limit.weapons, null) as List<Status>)
                    if (status.status == Spawning.Disallow)
                        Weapons.Add(status.Key, getWeaponName(status.Key, false));

            // Accessories
            var accessory_properties = limit.accessories.GetType().GetProperties();
            for (int i = 0; i < accessory_properties.Length; i++)
                foreach (Status status in accessory_properties[i].GetValue(limit.accessories, null) as List<Status>)
                    if (status.status == Spawning.Disallow)
                        Accessories.Add(status.Key, getAccessoryName(status.Key, false));

            // Kititems
            var kititems_properties = limit.kititems.GetType().GetProperties();
            for (int i = 0; i < kititems_properties.Length; i++)
                foreach (Status status in kititems_properties[i].GetValue(limit.kititems, null) as List<Status>)
                    if (status.status == Spawning.Disallow)
                        Kititems.Add(status.Key, getKititemsName(status.Key, false));

            keys.Add("weapons", Weapons);
            keys.Add("accessories", Accessories);
            keys.Add("kititems", Kititems);

            return (keys);
        }
        private Dictionary<String, Dictionary<String, String>> getOneMisuseAllowed(compact limit)
        {
            Dictionary<String, String> Weapons = new Dictionary<String, String>();
            Dictionary<String, String> Accessories = new Dictionary<String, String>();
            Dictionary<String, String> Kititems = new Dictionary<String, String>();
            Dictionary<String, Dictionary<String, String>> keys = new Dictionary<String, Dictionary<String, String>>();

            // Weapons
            var weapon_properties = limit.weapons.GetType().GetProperties();
            for (int i = 0; i < weapon_properties.Length; i++)
                foreach (Status status in weapon_properties[i].GetValue(limit.weapons, null) as List<Status>)
                    if (status.status == Spawning.DisallowAfterOne)
                        Weapons.Add(status.Key, getWeaponName(status.Key, false));

            // Accessories
            var accessory_properties = limit.accessories.GetType().GetProperties();
            for (int i = 0; i < accessory_properties.Length; i++)
                foreach (Status status in accessory_properties[i].GetValue(limit.accessories, null) as List<Status>)
                    if (status.status == Spawning.DisallowAfterOne)
                        Accessories.Add(status.Key, getAccessoryName(status.Key, false));

            // Kititems
            var kititems_properties = limit.kititems.GetType().GetProperties();
            for (int i = 0; i < kititems_properties.Length; i++)
                foreach (Status status in kititems_properties[i].GetValue(limit.kititems, null) as List<Status>)
                    if (status.status == Spawning.DisallowAfterOne)
                        Kititems.Add(status.Key, getKititemsName(status.Key, false));

            keys.Add("weapons", Weapons);
            keys.Add("accessories", Accessories);
            keys.Add("kititems", Kititems);

            return (keys);
        }
        private Dictionary<String, Dictionary<String, String>> getRCONMisuseCodes(compact limit)
        {
            Dictionary<String, String> Weapons = new Dictionary<String, String>();
            //Dictionary<String, String> Accessories = new Dictionary<String, String>();
            Dictionary<String, String> Kititems = new Dictionary<String, String>();
            Dictionary<String, Dictionary<String, String>> keys = new Dictionary<String, Dictionary<String, String>>();

            // Weapons
            var weapon_properties = limit.weapons.GetType().GetProperties();
            for (int i = 0; i < weapon_properties.Length; i++)
                foreach (Status status in weapon_properties[i].GetValue(limit.weapons, null) as List<Status>)
                    if (status.status == Spawning.DisallowAfterOne)
                        Weapons.Add(getWeaponName(status.Key, true), status.Key);

            // Accessories
            //var accessory_properties = limit.accessories.GetType().GetProperties();
            //for (int i = 0; i < accessory_properties.Length; i++)
            //    foreach (Status status in accessory_properties[i].GetValue(limit.accessories, null) as List<Status>)
            //        if (status.status == Spawning.DisallowAfterOne)
            //            Accessories.Add(getAccessoryName(status.Key, true), status.Key);

            // Kititems
            var kititems_properties = limit.kititems.GetType().GetProperties();
            for (int i = 0; i < kititems_properties.Length; i++)
                foreach (Status status in kititems_properties[i].GetValue(limit.kititems, null) as List<Status>)
                    if (status.status == Spawning.DisallowAfterOne)
                        Kititems.Add(getKititemsName(status.Key, true), status.Key);

            keys.Add("weapons", Weapons);
            //keys.Add("accessories", Accessories);
            keys.Add("kititems", Kititems);

            return (keys);
        }

        private Boolean fetchPlayerKit(string soldierName)
        {
            try
            {
                String solttu = soldierName;
                Hashtable Loadout = null;
                BattlelogClient bclient = new BattlelogClient();
                if (!player_kits.ContainsKey(solttu))
                {
                    player_kits.Add(solttu, new Player_Kit());
                    Loadout = bclient.getStats(solttu);
                }
                else
                    Loadout = bclient.getStats(solttu, player_kits[solttu].personaID);

                if (Loadout == null)
                    return (false);

                player_kits[solttu].personaID = Loadout["personaId"].ToString();
                if (!Loadout.Contains("currentLoadout"))
                {
                    if (Loadout.ContainsKey("error"))
                    {
                        player_kits[solttu].ERROR = true;
                        ConsoleError(String.Format("^1Error: {0} ({1})", Loadout["error"].ToString(), solttu));
                    }
                    return (false);
                }

                if (!Loadout.Contains("playerStats"))
                {
                    if (Loadout.ContainsKey("error"))
                    {
                        player_kits[solttu].ERROR = true;
                        ConsoleError(String.Format("^1Error: {0} ({1})", Loadout["error"].ToString(), solttu));
                    }
                    return (false);
                }
                Hashtable playerStats = (Hashtable)Loadout["playerStats"];
                player_kits[solttu].UNLOCKS.sc_handgrenades = Convert.ToInt32(playerStats["sc_handgrenades"]);

                Hashtable currentLoadout = (Hashtable)Loadout["currentLoadout"];

                if (!currentLoadout.Contains("weapons"))
                    return (false);
                Hashtable weapons = (Hashtable)currentLoadout["weapons"];

                if (!currentLoadout.Contains("selectedKit"))
                    return (false);

                // Currently Selected Kit
                byte selectedKit = Convert.ToByte(currentLoadout["selectedKit"]);
                player_kits[solttu].kit = (kits)selectedKit;

                // Kits
                ArrayList kits = (ArrayList)currentLoadout["kits"];
                object[] _kits = kits.ToArray();
                string[] Kit = ((IEnumerable)kits[selectedKit]).Cast<object>().Select(x => x.ToString()).ToArray();

                // PRIMARY_WEAPON & attachments
                player_kits[solttu].PRIMARY_WEAPON.Key = fixKey(Convert.ToInt64(Kit[0]) , category.PRIMARY).ToString();
                player_kits[solttu].PRIMARY_WEAPON = getPrimaryAttachments(weapons, player_kits[solttu].PRIMARY_WEAPON, solttu);
                // SIDEARM & attachments
                player_kits[solttu].SIDEARM.Key = fixKey(Convert.ToInt64(Kit[1]) , category.SECONDARY).ToString();
                player_kits[solttu].SIDEARM = getSidearmAttachments(weapons, player_kits[solttu].SIDEARM, solttu);
                // GADGET_1
                player_kits[solttu].GADGET_1 = fixKey(Convert.ToInt64(Kit[2]), category.GADGET1).ToString();
                // GADGET_2
                player_kits[solttu].GADGET_2 = fixKey(Convert.ToInt64(Kit[3]), category.GADGET2).ToString();
                // GRENADES
                player_kits[solttu].GRENADES = fixKey(Convert.ToInt64(Kit[4]), category.GRENADE).ToString();
                // MELEE
                player_kits[solttu].MELEE = fixKey(Convert.ToInt64(Kit[5]), category.MELEE).ToString();
                // FIELD_UPGRADES
                player_kits[solttu].FIELD_UPGRADES = fixKey(Convert.ToInt64(Kit[6]), category.SPECIALIZATION).ToString();
                // UNK_7
                //player_kits[soldierName].UNK_7 = Kit[7];
                // UNK_8
                //player_kits[soldierName].UNK_8 = Kit[8];
                // OUTFIT
                player_kits[solttu].OUTFIT = Kit[9];
                // PARACHUTE_CAMOUFLAGE
                player_kits[solttu].PARACHUTE_CAMOUFLAGE = Kit[10];
                // UNK_11
                //player_kits[soldierName].UNK_11 = Kit[11];
                // UNK_12
                //player_kits[soldierName].UNK_12 = Kit[12];
                player_kits[solttu].ERROR = false;

                return (true);
            }
            catch(Exception e)
            {
                DebugWrite(soldierName + " " + e.ToString(), 3);
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(String.Format("{0} {1}", soldierName, e.ToString()));
                    sw.WriteLine(String.Empty);
                }
                return (false);
            }
        }

        private PRIMARY_WEAPON getPrimaryAttachments(Hashtable weapons, PRIMARY_WEAPON primary_weapon, String soldierName)
        {
            try
            {
                ArrayList arraylist = (ArrayList)weapons[primary_weapon.Key];
                string[] weapon_accessories = (string[])arraylist.ToArray(typeof(string));
                if (weapon_accessories.Length > 5)
                {
                    primary_weapon.OPTIC = weapon_accessories[0];
                    primary_weapon.ACCESSORY = weapon_accessories[1];
                    primary_weapon.BARREL = weapon_accessories[2];
                    primary_weapon.UNDERBARREL = weapon_accessories[3];
                    primary_weapon.PAINT = weapon_accessories[4];
                    primary_weapon.AMMO = weapon_accessories[5];
                }
                else
                {
                    primary_weapon.OPTIC = "0";
                    primary_weapon.ACCESSORY = "0";
                    primary_weapon.BARREL = "0";
                    primary_weapon.UNDERBARREL = "0";
                    primary_weapon.PAINT = "0";
                    primary_weapon.AMMO = "0";
                }
                return (primary_weapon);
            }
            catch (Exception e)
            {
                primary_weapon.OPTIC = "0";
                primary_weapon.ACCESSORY = "0";
                primary_weapon.BARREL = "0";
                primary_weapon.UNDERBARREL = "0";
                primary_weapon.PAINT = "0";
                primary_weapon.AMMO = "0";

                DebugWrite("Couldn't find weapon with ID: " + primary_weapon.Key + " Player: " + soldierName, 3);
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine("Weapon key: " + primary_weapon.Key + " Player: " + soldierName);
                    sw.WriteLine(String.Empty);
                }
                return (primary_weapon);
            }
        }
        private SIDEARM getSidearmAttachments(Hashtable weapons, SIDEARM sidearm, String soldierName)
        {
            try
            {
                ArrayList arraylist = (ArrayList)weapons[sidearm.Key];
                string[] weapon_accessories = (string[])arraylist.ToArray(typeof(string));
                if (weapon_accessories.Length > 3)
                {
                    sidearm.OPTIC = weapon_accessories[0];
                    sidearm.ACCESSORY = weapon_accessories[1];
                    sidearm.BARREL = weapon_accessories[2];
                    sidearm.PAINT = weapon_accessories[3];
                }
                else
                {
                    sidearm.OPTIC = "0";
                    sidearm.ACCESSORY = "0";
                    sidearm.BARREL = "0";
                    sidearm.PAINT = "0";
                }
                return (sidearm);
            }
            catch (Exception e)
            {
                sidearm.OPTIC = "0";
                sidearm.ACCESSORY = "0";
                sidearm.BARREL = "0";
                sidearm.PAINT = "0";

                DebugWrite("Couldn't find sidearm with ID: " + sidearm.Key + " Player: " + soldierName, 3);
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine("Sidearm key: " + sidearm.Key + " Player: " + soldierName);
                    sw.WriteLine(String.Empty);
                }
                return (sidearm);
            }
        }

        public String StripModifiers(String text)
        {
            return Regex.Replace(text, @"\^[0-9a-zA-Z]", "");
        }
        public String E(String text) // Escape replacements
        {
            text = Regex.Replace(text, @"\\n", "\n");
            text = Regex.Replace(text, @"\\t", "\t");
            return text;
        }
        public String BestPlayerMatch(String message, String speaker, Match cmd)
        {
            int found = 0;
            String name = cmd.Groups[1].Value;
            CPlayerInfo target = null;
            foreach (CPlayerInfo p in PlayersList)
            {
                if (p == null)
                    continue;

                if (Regex.Match(p.SoldierName, name, RegexOptions.IgnoreCase).Success)
                {
                    ++found;
                    target = p;
                }
            }

            if (found == 0)
            {
                ServerCommand("admin.say", "No such player name matches (" + name + ")", "player", speaker);
                DebugWrite("No such player name matches (" + name + ") " + " --> " + speaker, 2);
                return (String.Empty);
            }
            if (found > 1)
            {
                ServerCommand("admin.say", "Multiple players match the target name (" + name + "), try again!", "player", speaker);
                DebugWrite("Multiple players match the target name (" + name + "), try again! --> " + speaker, 2);
                return (String.Empty);
            }
            else
                return (target.SoldierName);
        }
        private bool ListEmpty(List<string> list)
        {
            if (list.Count < 1)
                return (true);

            foreach (String entry in list)
                if (!String.IsNullOrEmpty(entry))
                    return (false);

            return (true);
        }
        public String makeRelativePath(String file)
        {
            String exe_path = Directory.GetParent(Application.ExecutablePath).FullName;
            String dll_path = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

            String rel_path = dll_path.Replace(exe_path, "");
            rel_path = Path.Combine(rel_path.Trim(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }), file);
            return rel_path;
        }
        public String FixLogFilePath()
        {
            String file = log_path;
            try
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(this.GetType().Name))
                    file = makeRelativePath(this.GetType().Name + "_" + host + "_" + port + ".log");
            }
            catch (Exception e)
            {
                using (StreamWriter sw = new StreamWriter(Path.ChangeExtension(log_path, ".err")))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(String.Empty);
                }
            }
            return file.Trim();
        }

        public Int64 fixKey(Int64 key, category type)
        {
            if(replaces == null)
            {
                if (File.Exists(Path.Combine(base_path, @"LoadoutEnforcer\weapon_replaces.xml")))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(List<Replace>));
                    TextReader reader = new StreamReader(Path.Combine(base_path, @"LoadoutEnforcer\weapon_replaces.xml"));
                    object obj = deserializer.Deserialize(reader);
                    replaces = (List<Replace>)obj;
                    reader.Close();
                }
                else
                    return (key);
            }

            int index = replaces.FindIndex(f => f.old_key == key && f.type == type);
            if (index > -1)
                return (replaces[index].new_key);
            else
                return (key);
        }

        #region DIAGNOSTICS
        public void GetAvailableMemory()
        {
            // Determine the maximum number of generations the system
            // garbage collector currently supports.
            DebugWrite(String.Format("The highest generation is {0}", GC.MaxGeneration), 5);

            // Determine the best available approximation of the number 
            // of bytes currently allocated in managed memory.
            DebugWrite(String.Format("Total Memory: {0}", GC.GetTotalMemory(false)), 5);

            GC.Collect();

            // Determine which generation myGCCol object is stored in.
            DebugWrite(String.Format("Total Memory: {0}", GC.GetTotalMemory(false)), 5);
        }
        #endregion
    } // end LoadoutEnforcer_xfile

    public class SQLite
    {
        public static Dictionary<String, String> GetWeapons
        {
            get
            {
                if (Reflector.Select_Weapons == null)
                    Reflector.Reflect();
                Dictionary<String, String> c_weapons = Reflector.Select_Weapons(Reflector.AssemblyDirectory);
                return (c_weapons);
            }
        }
        public static Dictionary<String, String> GetAccessories
        {
            get
            {
                if (Reflector.Select_Accessories == null)
                    Reflector.Reflect();
                Dictionary<String, String> c_accessories = Reflector.Select_Accessories(Reflector.AssemblyDirectory);
                return (c_accessories);
            }
        }
        public static Dictionary<String, String> GetKititems
        {
            get
            {
                if (Reflector.Select_Kititems == null)
                    Reflector.Reflect();
                Dictionary<String, String> c_kititems = Reflector.Select_Kititems(Reflector.AssemblyDirectory);
                return (c_kititems);
            }
        }
    }

    public class Reflector
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static Dictionary<string, string> weapons;
        public static String encoded;
        public static String decoded;

        public static SelectWeaponsDelegate Select_Weapons;
        public static SelectAccessoriesDelegate Select_Accessories;
        public static SelectKititemsDelegate Select_Kititems;
        public static HTMLDecode htmldecode;

        public static bool Reflect()
        {
            String assembly_path = AssemblyDirectory;
            String cs_path = File.ReadAllText(Path.Combine(AssemblyDirectory, @"LoadoutEnforcer\xfileFIN.cs"));
            Assembly assembly = CompileSource(cs_path);
            if (assembly == null)
                return (false);

            Type xfileFINClass = assembly.GetType("xfileFIN");

            MethodInfo Method1 = xfileFINClass.GetMethod("Select_Weapons");
            Select_Weapons = (SelectWeaponsDelegate)Delegate.CreateDelegate(typeof(SelectWeaponsDelegate), Method1);

            MethodInfo Method2 = xfileFINClass.GetMethod("Select_Accessories");
            Select_Accessories = (SelectAccessoriesDelegate)Delegate.CreateDelegate(typeof(SelectAccessoriesDelegate), Method2);

            MethodInfo Method3 = xfileFINClass.GetMethod("Select_Kititems");
            Select_Kititems = (SelectKititemsDelegate)Delegate.CreateDelegate(typeof(SelectKititemsDelegate), Method3);

            MethodInfo Method4 = xfileFINClass.GetMethod("HTMLDecode");
            htmldecode = (HTMLDecode)Delegate.CreateDelegate(typeof(HTMLDecode), Method4);

            return (true);
        }

        private static Assembly CompileSource(string sourceCode)
        {
            String SystemWeb = Path.Combine(AssemblyDirectory, @"LoadoutEnforcer\System.Web.dll");
            String SystemDataSQLite = Path.Combine(AssemblyDirectory, @"System.Data.SQLite.dll");

            if (!File.Exists(SystemWeb) || !File.Exists(SystemWeb) /*|| !File.Exists(SystemDataSQLiteInteropx64) || !File.Exists(SystemDataSQLiteInteropx86)*/)
                return (null);

            CodeDomProvider cpd = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Web.dll");
            //cp.ReferencedAssemblies.Add(SystemWeb);
            cp.ReferencedAssemblies.Add(SystemDataSQLite);
            cp.GenerateExecutable = false;
            // True - memory generation, false - external file generation
            cp.GenerateInMemory = true;
            // Invoke compilation.
            CompilerResults cr = cpd.CompileAssemblyFromSource(cp, sourceCode);

            if (cr.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in cr.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException(sb.ToString());
            }

            return cr.CompiledAssembly;
        }
    }

    public class BattlelogClient
    {
        WebClient client = null;

        private String fetchWebPage(ref String html_data, String url)
        {
            try
            {
                if (client == null)
                    client = new WebClient();

                html_data = client.DownloadString(url);
                return html_data;

            }
            catch (WebException e)
            {
                if (e.Status.Equals(WebExceptionStatus.Timeout))
                    throw new Exception("HTTP request timed-out");
                else
                    throw;
            }
        }

        public Hashtable getStats(String player)
        {
            try
            {
                if (Reflector.htmldecode == null)
                    Reflector.Reflect();

                /* First fetch the player's main page to get the persona id */
                String result = "";
                fetchWebPage(ref result, "http://battlelog.battlefield.com/bf4/user/" + player);

                string decoded = Reflector.htmldecode(result);

                /* Extract the persona id */
                MatchCollection pid = Regex.Matches(decoded, @"bf4/soldier/" + player + @"/stats/(\d+)(/\w*)?/", RegexOptions.Singleline);

                String personaId = "";

                foreach (Match m in pid)
                {
                    if (m.Success && m.Groups[2].Value.Trim() == "/pc")
                    {
                        personaId = m.Groups[1].Value.Trim();
                    }
                }

                if (personaId == "")
                    throw new Exception("could not find persona-id for ^b" + player);

                return getStats(player, personaId);
            }
            catch (Exception e)
            {
                //Handle exceptions here however you want
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public Hashtable getStats(String player, String personaId)
        {
            try
            {
                /* First fetch the player's main page to get the persona id */
                String result = "";

                fetchWebPage(ref result, "http://battlelog.battlefield.com/bf4/loadout/get/" + player + "/" + personaId + "/1/");

                Hashtable json = (Hashtable)JSON.JsonDecode(result);

                // check we got a valid response
                if (!(json.ContainsKey("type") && json.ContainsKey("message")))
                    throw new Exception("JSON response does not contain \"type\" or \"message\" fields");

                String type = (String)json["type"];
                String message = (String)json["message"];

                /* verify we got a success message */
                if (!(type.StartsWith("success") && message.StartsWith("OK")))
                    throw new Exception("JSON response was type=" + type + ", message=" + message);


                /* verify there is data structure */
                Hashtable data = null;
                if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                    throw new Exception("JSON response was does not contain a data field");

                return data;
            }
            catch (Exception e)
            {
                //Handle exceptions here however you want
                Console.WriteLine(e.ToString());
            }

            return null;
        }
    }

} // end namespace PRoConEvents
