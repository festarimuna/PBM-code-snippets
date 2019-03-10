using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine.UI;

// OVERVIEW

// Gives an example of how I handle using databases for the game
// Tightly coupled with many other classes. This is something I need to improve

// Contains classes related to information stored in databases
// Contains public methods other classes call to change databases e.g. add items to inventory
// Contains various private helper methods for making (secure) changes to databases
// Reads, writes, XML data files
// Handles setting up new game, saving game, loading game

// Attached to XMLManager gameObject

public class XmlManager : MonoBehaviour
{
    public static XmlManager xmlManagerIns;

    // private NeetFreek classes
    private AssetManager assetManager;
    private ControllerPC controllerPC;
    private HotBarManager hotbarManager;
    private ItemDisplay itemDisplay;
    private Journal journal;
    private LevelManager levelManager;
    private NoteDisplay noteDisplay;
    private PanelAbilities panelAbilities;
    private PopupPanel popupPanel;
    private SheetPC sheetPC;
    private Spawner spawner;
    private TextManager textManager;

    // private Unity classes
    private XElement rootXML;

    // databases
    private AbilitiesDatabase abilitiesDB;
    private InventoryDatabase inventoryDB;
    private ItemDatabase itemDB;
    private KeyDatabase keyDB;
    private QuestsDatabase notesDB;
    private PickedUpDatabase pickedUpDB;
    private PlayerDatabase progressDB;
    private RewardsDatabase rewardsDB;
    private TriggeredMessagesDatabase triggeredMessagesDB;
    private VisitedDatabase visitedDB;

    // new quest Observer Pattern delegates
    public delegate void OnNewQuest(Quest note);
    public event OnNewQuest NotifyOnNewQuestObservers;

    // public properties
    public bool IsLoadGame
    {
        get
        {
            return isLoadGame;
        }
    }
    public bool LoadingGame
    {
        get
        {
            return loadingGame;
        }
    }

    // private fields
    private bool loadingGame;
    private bool isLoadGame;
    private static readonly int sizeMaxInventory = 24;


    // Setup and Unity methods
    private void Awake()
    {
        EnsureSingleton();
        SetClassReferences(); // references set in Awake, XmlManager.cs uses them before most classes start functioning
        CreateDatabases();

        // if in menuMainStart
        if (!sheetPC.inGame)
        {
            SetNewOrLoadGame();
        } 
    }

    private void EnsureSingleton()
    {
        if (xmlManagerIns != null)
        {
            Destroy(gameObject);
        }
        else
        {
            xmlManagerIns = this;
            DontDestroyOnLoad(transform.gameObject);
        }
    }

    private void SetClassReferences()
    {
        assetManager = GameObject.Find("AssetManager").GetComponent<AssetManager>();
        controllerPC = GameObject.Find("Player").GetComponent<ControllerPC>();
        itemDisplay = GameObject.Find("inventory").GetComponent<ItemDisplay>();
        journal = GameObject.Find("journal").GetComponent<Journal>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        noteDisplay = GameObject.Find("journal").GetComponent<NoteDisplay>();
        panelAbilities = GameObject.Find("abilities").GetComponent<PanelAbilities>();
        sheetPC = GameObject.Find("Player").GetComponent<SheetPC>();
        spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        textManager = GameObject.Find("TextManager").GetComponent<TextManager>();
        hotbarManager = GameObject.Find("hotbars").GetComponent<HotBarManager>();
    }

    private void CreateDatabases()
    {
        progressDB = new PlayerDatabase();
        rewardsDB = new RewardsDatabase();
        visitedDB = new VisitedDatabase();
    }

    private void SetNewOrLoadGame()
    {
        bool loadRequested = levelManager.loadSaveGameRequested; // check if player loading game

        if (loadRequested)
        {
            levelManager.loadSaveGameRequested = false;
            LoadGame();
        } else
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        // setup databases
        LoadAbilitiesDB();
        LoadItemsDB();
        LoadKeyDB();

        // reset certain game states for new game
        progressDB.savePosition = "";
        ResetAbilities();
        ResetTriggeredMessages();
        ResetKeys();
        ResetVisited();
        triggeredMessagesDB.list.Clear();

        loadingGame = false;
        isLoadGame = false;

        sheetPC.SetupNewGame();
    }


    // save, load handler routines
    private void SaveGame()
    {
        SaveAbilities();
        SavePlayerData();

        SaveAbilitiesDB();
        SaveInventoryDB();
        SaveItemsDB();
        SaveKeyDB();
        SaveQuestsDB();
        SavePickedUpDB();
        SaveVisitedDB();
        SaveTriggeredMessagesDB();
        SaveProgressDB();
    }

    private void LoadGame()
    {
        isLoadGame = true;
        loadingGame = true;

        LoadProgressDB();
        LoadAbilitiesDB();
        LoadKeyDB();
        LoadItemsDB();
        LoadInventoryDB();
        LoadQuestsDB();
        LoadPickedUpDB();
        LoadTriggeredMessagesDB();
        LoadRewardsDB();
        LoadVisitedDB();

        LoadPlayerData();
        LoadInventoryPanel();
        LoadQuests();
    }


    // save
    private void SavePlayerData()
    {
        // player state
        progressDB.playerName = sheetPC.PlayerName;

        progressDB.gold = sheetPC.Gold;

        progressDB.playerPosition = controllerPC.FetchPlayerPos();
        progressDB.currentScene = levelManager.ReturnSceneName();
        progressDB.savePosition = sheetPC.savePosition;

        // stats
        progressDB.agility = sheetPC.Agility;
        progressDB.magic = sheetPC.Magic;
        progressDB.strength = sheetPC.Strength;

        progressDB.maxHealth = sheetPC.MaxHealth;
        progressDB.healthChange = sheetPC.HealthChange;

        progressDB.armour = sheetPC.Armour;

        // secondary stats
        progressDB.attackSpeed = sheetPC.AttackSpeed;

        progressDB.critRating = sheetPC.CritRating;
        progressDB.critMultiplier = sheetPC.CritMultiplier;

        progressDB.baseAttackDamage = sheetPC.BaseDamage;
        progressDB.damageChangeEquipment = sheetPC.DamageChangeEquipment;
        progressDB.damageChangeStrength = sheetPC.DamageChangeStrength;

        // ability points and skill points
        progressDB.ap = sheetPC.AP;
        progressDB.sp = sheetPC.SP;

        // experience
        progressDB.levelReqBase = sheetPC.LevelReqBase;
        progressDB.xpLevel = sheetPC.XpLevel;
        progressDB.xpCurrent = sheetPC.XpCurrent;
        progressDB.xpLeftToLevel = sheetPC.XpLeftToLevel;
        progressDB.xpReqThisLevel = sheetPC.XpReqThisLevel;

        // equipment
        progressDB.equippedAmulet = sheetPC.equippedAmulet;
        progressDB.equippedBracer1 = sheetPC.equippedBracer1;
        progressDB.equippedBracer2 = sheetPC.equippedBracer2;
        progressDB.equippedRing1 = sheetPC.equippedRing1;
        progressDB.equippedRing2 = sheetPC.equippedRing2;
        progressDB.equippedWeapon = sheetPC.equippedWeapon;
    }

    private void SaveAbilities()
    {
        SaveAbilitiesPanel();
        SaveAbilitiesHotbar();
    }


    // ability save subroutines
    private void SaveAbilitiesPanel()
    {
        abilitiesDB.abilities.Clear();

        foreach (Abilities ability in panelAbilities.abilities)
        {
            abilitiesDB.abilities.Add(ability);
        }
    }

    private void SaveAbilitiesHotbar()
    {
        for (int i = 0; i < abilitiesDB.hotbarAbilities.Length; i++)
        {
            abilitiesDB.hotbarAbilities[i] = hotbarManager.hotbarAbilities[i];
        }
    }


    // load
    private void LoadPlayerData()
    {
        // player state
        sheetPC.PlayerName = progressDB.playerName;

        sheetPC.Gold = progressDB.gold;

        controllerPC.SetPlayerPos(progressDB.playerPosition);
        sheetPC.currentScene = progressDB.currentScene;
        sheetPC.savePosition = progressDB.savePosition;

        // stats
        sheetPC.Agility = progressDB.agility;
        sheetPC.Magic = progressDB.magic;
        sheetPC.Strength = progressDB.strength;

        sheetPC.MaxHealth = progressDB.maxHealth;
        sheetPC.HealthChange = progressDB.healthChange;
        sheetPC.CurrentHealth = progressDB.maxHealth; // give player full health on load

        sheetPC.Armour = progressDB.armour;

        // secondary stats
        sheetPC.AttackSpeed = progressDB.attackSpeed;

        sheetPC.CritRating = progressDB.critRating;
        sheetPC.CritMultiplier = progressDB.critMultiplier;

        sheetPC.BaseDamage = progressDB.baseAttackDamage;
        sheetPC.DamageChangeEquipment = progressDB.damageChangeEquipment;
        sheetPC.DamageChangeStrength = progressDB.damageChangeStrength;

        // ability points and skill points
        sheetPC.AP = progressDB.ap;
        sheetPC.SP = progressDB.sp;

        // experience
        sheetPC.LevelReqBase = progressDB.levelReqBase;
        sheetPC.XpLevel = progressDB.xpLevel;
        sheetPC.XpCurrent = progressDB.xpCurrent;
        sheetPC.XpLeftToLevel = progressDB.xpLeftToLevel;
        sheetPC.XpReqThisLevel = progressDB.xpReqThisLevel;

        // equipment
        sheetPC.equippedAmulet = progressDB.equippedAmulet;
        sheetPC.equippedBracer1 = progressDB.equippedBracer1;
        sheetPC.equippedBracer2 = progressDB.equippedBracer2;
        sheetPC.equippedRing1 = progressDB.equippedRing1;
        sheetPC.equippedRing2 = progressDB.equippedRing2;
        sheetPC.equippedWeapon = progressDB.equippedWeapon;
    }

    private void LoadInventoryPanel()
    {
        itemDisplay.SetClassReferences();

        bool equippedWeapon = false;
        foreach (Items item in inventoryDB.list)
        {
            // places equipped equipment icons in equipment slots
            if (item.type == "equipment")
            {

                if (sheetPC.equippedWeapon != null && item.name == sheetPC.equippedWeapon.nameItem && !equippedWeapon)
                {
                    equippedWeapon = true;
                    itemDisplay.SetupInvIcon(item, GameObject.Find("WeaponSlot"));
                    spawner.SetClassReferences();
                    spawner.EquipWeapon(item.name);
                }
                // places equipment icons in inv slots
                else
                {
                    itemDisplay.SetupInvIcon(item, GameObject.Find(item.mySlot));
                }
            }

            if (item.type == "inventory")
            {
                itemDisplay.SetupInvIcon(item, GameObject.Find(item.mySlot));
            }
        }
    }

    private void LoadQuests()
    {
        foreach (var note in notesDB.list)
        {
            if (note.archived != "1")
            {
                noteDisplay.AddNewNote(note);
            }
        }

        journal.SetClassReferences();
        journal.AddNoteSlotGameObjects();
        journal.DisplayTopQuestText();
    }    


    // reset 
    private void ResetAbilities()
    {
        foreach (Abilities ability in abilitiesDB.abilities)
        {
            ability.level = 0;
        }
    }

    private void ResetKeys()
    {
        foreach (Key key in keyDB.list)
        {
            key.amount = 0;
        }
    }

    private void ResetVisited()
    {
        visitedDB.list.Clear();
        visitedDB.list.Add("Fairbrook");
    }


    // database read/write (should abstract these to one function, select database via parameter - oops!)
    private void SaveAbilitiesDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(AbilitiesDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/abilities_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, abilitiesDB);
        fileStream.Close();
    }

    private void LoadAbilitiesDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(AbilitiesDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/abilities_data.xml", FileMode.OpenOrCreate);
        abilitiesDB = (AbilitiesDatabase)xmlSerialiser.Deserialize(fileStream);
        fileStream.Close();
    }

    private void SaveInventoryDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(InventoryDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/inventory_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, inventoryDB);
        fileStream.Close();
    }

    private void LoadInventoryDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(InventoryDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/inventory_data.xml", FileMode.OpenOrCreate);
        inventoryDB = (InventoryDatabase)xmlSerialiser.Deserialize(fileStream);
        fileStream.Close();
    }

    private void SaveItemsDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(ItemDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/item_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, itemDB);
        fileStream.Close();
    }

    private void LoadItemsDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(ItemDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/item_data.xml", FileMode.Open);
        itemDB = (ItemDatabase)xmlSerialiser.Deserialize(fileStream);
        fileStream.Close();
    }

    private void SaveKeyDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(KeyDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/key_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, keyDB);
        fileStream.Close();
    }

    private void LoadKeyDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(KeyDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/key_data.xml", FileMode.Open);
        keyDB = (KeyDatabase)xmlSerialiser.Deserialize(fileStream);
        fileStream.Close();
    }

    private void SavePickedUpDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(PickedUpDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/pickedUp_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, pickedUpDB);
        fileStream.Close();
    }

    private void LoadPickedUpDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(PickedUpDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/pickedUp_data.xml", FileMode.Open);
        pickedUpDB = (PickedUpDatabase)xmlSerialiser.Deserialize(fileStream);
        fileStream.Close();
    }

    private void SaveProgressDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(PlayerDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/save_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, progressDB);
        fileStream.Close();
    }

    private void LoadProgressDB()
    {
        PlayerDatabase newSaveDataBase = new PlayerDatabase();
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(PlayerDatabase));

        TextReader txtReader = new StreamReader(Application.dataPath + "/StreamingAssets/XML/save_data.xml");
        newSaveDataBase = (PlayerDatabase)xmlSerialiser.Deserialize(txtReader);
        txtReader.Close();
        progressDB = newSaveDataBase;
    }

    private void SaveQuestsDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(QuestsDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/notes_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, notesDB);
        fileStream.Close();
    }

    private void LoadQuestsDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(QuestsDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/notes_data.xml", FileMode.OpenOrCreate);
        notesDB = (QuestsDatabase)xmlSerialiser.Deserialize(fileStream);
        fileStream.Close();
    }

    private void LoadRewardsDB()
    {
        RewardsDatabase rewardsDataBase = new RewardsDatabase();
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(RewardsDatabase));

        TextReader txtReader = new StreamReader(Application.dataPath + "/StreamingAssets/XML/rewards_data.xml");
        rewardsDataBase = (RewardsDatabase)xmlSerialiser.Deserialize(txtReader);
        txtReader.Close();
        rewardsDB = rewardsDataBase;
    }

    private void SaveTriggeredMessagesDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(TriggeredMessagesDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/triggeredMessages_data.xml", FileMode.Create);
        xmlSerialiser.Serialize(fileStream, triggeredMessagesDB);
        fileStream.Close();
    }

    private void LoadTriggeredMessagesDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(TriggeredMessagesDatabase));
        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/triggeredMessages_data.xml", FileMode.Open);
        triggeredMessagesDB = (TriggeredMessagesDatabase)xmlSerialiser.Deserialize(fileStream);
        fileStream.Close();
    }

    private void SaveVisitedDB()
    {
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(VisitedDatabase));

        FileStream fileStream = new FileStream(Application.dataPath + "/StreamingAssets/XML/visited_data.xml", FileMode.Create); xmlSerialiser.Serialize(fileStream, visitedDB);
        fileStream.Close();

    }

    private void LoadVisitedDB()
    {
        VisitedDatabase visitedDatabase = new VisitedDatabase();
        XmlSerializer xmlSerialiser = new XmlSerializer(typeof(VisitedDatabase));

        TextReader txtReader = new StreamReader(Application.dataPath + "/StreamingAssets/XML/visited_data.xml");
        visitedDatabase = (VisitedDatabase)xmlSerialiser.Deserialize(txtReader);
        txtReader.Close();
        visitedDB = visitedDatabase;
    }


    // ability helper methods  
    public Abilities ReturnAbility(string targetName)
    {
        foreach (Abilities ability in abilitiesDB.abilities)
        {
            if (ability.name == targetName)
            {
                return ability;
            }
        }

        return null;
    }


    // conversation helper methods
    public bool ReturnNPCFunctionButton(string clickedButton, string nameNPC)
    {
        string nameToCompare = "nameQuest=" + "\"" + nameNPC + "\"";
        string s = "";
        rootXML = XElement.Load(Application.dataPath + "/StreamingAssets/XML/text_data.xml");
        foreach (XElement xEle in rootXML.Elements("npc"))
        {
            if (xEle.Attribute("nameQuest").ToString() == nameToCompare)
            {
                s = xEle.Element("functionButton").Value.ToString();
                if (s == clickedButton)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public string ReturnButtonNumberEle(string nameNPCConvo)
    {
        string convoName = nameNPCConvo;
        string number = "";

        foreach (Quest note in notesDB.list)
        {
            if (note.nameQuestNPC == convoName)
            {
                number = note.buttonNumber;
                return number;
            }
        }

        return number;
    }


    // hotbar helper methods
    public void SaveAbilityHotbarSlot(int index, Abilities abilityToAdd)
    {
        abilitiesDB.hotbarAbilities[index] = abilityToAdd;
    }
    // called by HotbarManager.cs
    public void LoadHotbar()
    {
        for (int i = 0; i < abilitiesDB.hotbarAbilities.Length; i++)
        {
            if (abilitiesDB.hotbarAbilities[i] != null)
            {
                string nameAbility = abilitiesDB.hotbarAbilities[i].name;

                GameObject abilityIconGO = GameObject.Find("slotsHotbar").transform.GetChild(i).gameObject;
                abilityIconGO.name = nameAbility;

                GameObject slotGO = abilityIconGO.transform.GetChild(0).gameObject;
                slotGO.name = nameAbility;
                slotGO.GetComponent<RawImage>().texture = assetManager.ReturnIconTexture(nameAbility);
                slotGO.GetComponent<RawImage>().enabled = true;

                switch (nameAbility)
                {
                    case "healingTouch":
                        hotbarManager.ToggleSubscribeHealingTouch(i, true);
                        break;
                    case "heroicMight":
                        hotbarManager.ToggleSubscribeHeroicMight(i, true);
                        break;
                    case "stoneShell":
                        hotbarManager.ToggleSubscribeStoneShell(i, true);
                        break;
                    case "lightningStrikes":
                        hotbarManager.ToggleSubscribeLightningStrikes(i, true);
                        break;
                    default:
                        break;
                }
            }
        }
    }


    // inventory items helper
    public Items ReturnInvItem(string name)
    {
        foreach (Items item in inventoryDB.list)
        {
            if (item.name == name)
            {
                return item;
            }
        }

        return null;
    }

    public void ChangeItemSlotNumber(Items i, string slot)
    {
        foreach (Items item in itemDB.list)
        {
            if (item == i)
            {
                item.mySlot = slot;
            }
        }
    }

    public void AddItemToInventoryPickup(Items item)
    {
        inventoryDB.list.Add(item);
    }

    public void RemoveItemFromInventoryList(Items item)
    {
        Items itemToRemove = null;
        foreach (Items i in inventoryDB.list)
        {
            if (i.name == item.name)
            {
                itemToRemove = i;
            }
        }

        if (itemToRemove != null)
        {
            inventoryDB.list.Remove(itemToRemove);
        }
    }

    public bool ReturnItemInInventory(string name)
    {
        foreach (Items item in inventoryDB.list)
        {
            if (item.name == name)
            {
                return true;
            }
        }

        return false;
    }

    public bool ReturnInvHasSpace()
    {
        int counter = 0;

        foreach (Items item in inventoryDB.list)
        {
            counter++;
        }

        counter -= CountEquippedItems();

        if (counter < sizeMaxInventory)
        {
            return true;
        }

        return false;
    }

    private int CountEquippedItems()
    {
        int i = 0;
        if (sheetPC.equippedAmulet.nameItem != "")
        {
            i++;
        }
        if (sheetPC.equippedBracer1.nameItem != "")
        {
            i++;
        }
        if (sheetPC.equippedBracer2.nameItem != "")
        {
            i++;
        }
        if (sheetPC.equippedRing1.nameItem != "")
        {
            i++;
        }
        if (sheetPC.equippedRing2.nameItem != "")
        {
            i++;
        }
        if (sheetPC.equippedWeapon.nameItem != "")
        {
            i++;
        }
        return i;
    }


    // item pick up helper methods
    public void AddItemToPickedUpList(string name)
    {
        pickedUpDB.list.Add(name);
    }

    public void RemoveItemFromPickedUpList(string name)
    {
        foreach (string item in pickedUpDB.list)
        {
            if (item == name)
            {
                pickedUpDB.list.Remove(item);
                break;
            }
        }
    }

    public bool ReturnIfItemPickedUp(string name)
    {
        foreach (string item in pickedUpDB.list)
        {
            if (item == name)
            {
                return true;
            }
        }
        return false;
    }

    public void DeletePickedUpItemsKeys()
    {
        DeletePickedUpItems();
        DeletePickedUpKeys();
    }

    private void DeletePickedUpKeys()
    {
        foreach (string entry in pickedUpDB.list)
        {
            if (entry.Contains("Key"))
            {
                if (GameObject.Find(entry))
                {
                    Destroy(GameObject.Find(entry));
                }
            }
        }
    }

    private void DeletePickedUpItems()
    {
        if (pickedUpDB.list.Count == 0)
        {
            LoadPickedUpDB();
        }
        foreach (string entry in pickedUpDB.list)
        {
            if (!entry.Contains("Key"))
            {
                if (GameObject.Find(entry))
                {
                    Destroy(GameObject.Find(entry));
                }
            }
        }
    }


    // key helper methods
    public void AddKey(string name)
    {
        foreach (Key key in keyDB.list)
        {
            if (key.name == name)
            {
                key.amount++;
            }
        }
    }

    public int ReturnKeyCount(string name)
    {
        foreach (Key key in keyDB.list)
        {
            if (key.name == name)
            {
                return key.amount;
            }
        }

        return 0;
    }

    public bool ReturnHaveKey(string name)
    {
        foreach (Key key in keyDB.list)
        {
            if (key.name == name && key.amount > 0)
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveKey(string name)
    {
        foreach (Key key in keyDB.list)
        {
            if (key.name == name)
            {
                key.amount--;
            }
        }
    }


    // journal helper methods
    public Quest ReturnQuest(string nameQuest)
    {
        foreach (Quest note in notesDB.list)
        {
            if (note.name == nameQuest)
            {
                return note;
            }
        }

        return null;
    }

    public void AddQuestToList(NewQuestDetails newQuest)
    {
        bool noteAlreadyExists = false;

        Quest quest = new Quest
        {
            name = newQuest.nameQuest,
            archived = "0",
            buttonNumber = newQuest.buttonNumber,
            text = newQuest.textJournal,
            nameQuestNPC = newQuest.nameNPC,
            textQuestNPC = newQuest.textNPC
        };

        if (quest.killTarget != "")
        {
            quest.killTarget = newQuest.killTarget;
            quest.killsAmount = newQuest.killedAmount;
            quest.killReq = newQuest.killsRequired;
        }
        if (quest.targetTalkNPC != "")
        {
            quest.targetTalkNPC = newQuest.targetTalkNPC;
        }

        foreach (Quest n in notesDB.list)
        {
            if (n.name == quest.name)
            {
                noteAlreadyExists = true;
            }
        }

        if (noteAlreadyExists == false)
        {
            notesDB.list.Add(quest);
            NotifyOnNewQuestObservers(quest);
            
            textManager.haveNewNote = true;
            textManager.note = quest;
        }
    }

    public bool ReturnQuestArchived(string nameQuest)
    {
        foreach (Quest note in notesDB.list)
        {
            if (note.name == nameQuest)
            {
                if (note.archived == "1")
                {
                    return true;
                }
            }
        }

        // for cases when Journal.ReturnQuestName(string nameNPC) returns default NPC nameQuest instead of quest nameQuest;
        foreach (Quest note in notesDB.list)
        {
            if (note.nameQuestNPC == nameQuest)
            {
                if (note.archived == "1")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void ArchiveQuest(string nameQuest)
    {
        foreach (Quest note in notesDB.list)
        {
            if (note.name == nameQuest)
            {
                note.archived = "1";
            }
        }
    } // archived quests == completed quests


    // lore helper methods
    public string ReturnLoreDataText(string topic)
    {
        string nameToCompare = "topic=" + "\"" + topic + "\"";
        rootXML = XElement.Load(Application.dataPath + "/StreamingAssets/XML/lore_data.xml");
        string s = "";
        string t;

        foreach (XElement xEle in rootXML.Elements("textJournal"))
        {
            t = xEle.Attribute("topic").ToString();
            if (t == nameToCompare)
            {
                s = xEle.Value.ToString();
            }
        }

        return s;
    }


    // player position helper methods
    public string ReturnSavePos()
    {
        return progressDB.savePosition;
    }


    // triggered messages helper methods
    public void AddTriggeredMessage(string name)
    {
        triggeredMessagesDB.list.Add(name);
    }

    public void ResetTriggeredMessages()
    {
        triggeredMessagesDB.list.Clear();
    }

    public bool ReturnIfTriggeredMessage(string name)
    {
        foreach (string message in triggeredMessagesDB.list)
        {
            if (name == message)
            {
                return true;
            }
        }

        return false;
    }

    public bool ReturnIfTriggeredMessagesEmpty()
    {
        if (triggeredMessagesDB.list.Count == 0)
        {
            return true;
        }

        return false;
    }


    // visited places helper methods
    public bool ReturnHaveVisitedBefore(string namePlace)
    {
        foreach (string place in visitedDB.list)
        {
            if (place == namePlace)
            {
                return true;
            }
        }

        return false;
    }

    public void AddPlaceToVisited(string namePlace)
    {
        visitedDB.list.Add(namePlace);
    }
}


// abilities database
[System.Serializable]
[XmlRoot("AbilitiesDatabase")]
public class AbilitiesDatabase
{
    [XmlArray("AbilitiesDatabase")]
    public List<Abilities> abilities = new List<Abilities>();

    public Abilities[] hotbarAbilities = new Abilities[10];
}

// inventory database
[System.Serializable]
[XmlRoot("InventoryDatabase")]
public class InventoryDatabase
{
    [XmlArray("InventoryDatabase")]
    public List<Items> list = new List<Items>();
}

// items database
[System.Serializable]
[XmlRoot("ItemDatabase")]
public class ItemDatabase
{
    public List<Items> list = new List<Items>();
}

// keys database
[System.Serializable]
[XmlRoot("KeyDatabase")]
public class KeyDatabase
{
    public List<Key> list = new List<Key>();
}

// pickedUp database
[System.Serializable]
[XmlRoot("PickedUpDatabase")]
public class PickedUpDatabase
{
    [XmlArray("PickedUpDatabase")]
    public List<String> list = new List<String>();
}

// progress database
[System.Serializable]
[XmlRoot("PlayerDatabase")]
public class PlayerDatabase
{
    public string playerName, currentScene, savePosition;

    public Items equippedWeapon, equippedRing1, equippedRing2, equippedAmulet, equippedBracer1, equippedBracer2;

    public float attackDamage, attackSpeed, baseAttackDamage, critRating, damageChangeEquipment, damageChangeStrength, day,
        gameSecond, gameMinute, gameHour, gameDay, healthChange, hour, minute, maxHealth, second;

    public int agility, ap, sp, gold, magic, strength, armour, cooldownModifier, critMultiplier,
        xpLevel, xpCurrent, xpLeftToLevel, xpReqThisLevel, levelReqBase;

    public Vector3 playerPosition;
}

// quest database
[System.Serializable]
[XmlRoot("QuestsDatabase")]
public class QuestsDatabase
{
    [XmlArray("QuestsDatabase")]
    public List<Quest> list = new List<Quest>();
}

// rewards database
[System.Serializable]
[XmlRoot("RewardsDatabase")]
public class RewardsDatabase
{
    public int gold, xp;
}

// triggered messages database
[System.Serializable]
[XmlRoot("TriggeredMessagesDatabase")]
public class TriggeredMessagesDatabase
{
    [XmlArray("TriggeredMessagesDatabase")]
    public List<String> list = new List<String>();
}

// visited database
[System.Serializable]
[XmlRoot("VisitedDatabase")]
public class VisitedDatabase
{
    [XmlArray("VisitedDatabase")]
    public List<String> list = new List<String>();
}


// items attributes
[System.Serializable]
public class Items
{
    public string name;
    public int value;
    public string type;
    public string subType;
    public string description;
    public int bonusDamage;
    public int bonusHealth;
    public int bonusArmour;
    public int bonusAgility;
    public int bonusMagic;
    public int bonusStrength;
    public int attackSpeed;
    public string mySlot;
    public int mySlotIndex;
    public bool stackable;
}

// abilities attributes
[System.Serializable]
public class Abilities
{
    public string name;
    public int level;
    public string description;
    public float cooldown;
    public float duration;
}

// keys attributes
[System.Serializable]
public class Key
{
    public int amount;
    public string name;
}

// new quest attributes
public class NewQuestDetails
{
    public string nameQuest;
    public string buttonNumber;
    public string textJournal;
    public string textNPC;
    public string nameNPC;
    public string killTarget;
    public string killedAmount;
    public string killsRequired;
    public string targetTalkNPC;
}

// quest attributes
[System.Serializable]
public class Quest
{
    public string buttonNumber;
    public string name;
    public string text;
    public string nameQuestNPC;
    public string textQuestNPC;
    public string killTarget;
    public string killsAmount;
    public string killReq;
    public string archived;
    public string targetTalkNPC;
}