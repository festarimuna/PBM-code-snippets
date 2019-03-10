using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// OVERVIEW

// Gives an example of how I handle setting up gameObjects and player interactions with gameObjects in the game

// Handles initial locking, unlocking, opening of containers
// Handles which containers are locked
// Handles dropping items on container opening
// Handles checking for, removing, keys
// Handles which items are in which containers

// Attach to each container gameObject

public class Container : MonoBehaviour
{
    // private NeetFreek classes
    private AmbientSFX ambientSFX;
    private HelperMethods helperMethods;
    private MessageDisplay messageDisplay;
    private Options options;
    private Spawner spawner;
    private Tutorial tutorial;
    private XmlManager xmlManager;

    // private Unity classes
    private Animator animator;

    // private fields
    private bool locked, opened;
    private string loot, nameKey;


    // setup and Unity methods
    void Start ()
    {
        SetClassReferences();        
        SetLoot();        
        SetLockedState();
        HandleSetContainerKeyName();
    }

    private void SetClassReferences()
    {
        ambientSFX = GameObject.Find("AmbientSFXPlayer").GetComponent<AmbientSFX>();
        animator = GetComponent<Animator>();
        helperMethods = GameObject.Find("XMLManager").GetComponent<HelperMethods>();
        messageDisplay = GameObject.Find("messageDisplay").GetComponent<MessageDisplay>();
        options = GameObject.Find("OptionsPanelGame").GetComponent<Options>();
        spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        tutorial = GameObject.Find("HUD").GetComponent<Tutorial>();
        xmlManager = GameObject.Find("XMLManager").GetComponent<XmlManager>();
    }

    private void SetLoot()
    {
        string nameWithoutNumbers = helperMethods.RemoveNumbersFromName(name);
        
        switch (nameWithoutNumbers)
        {
            // chests
            case "Black Chest":
                SetBlackChestLoot(gameObject.name);
                break;
            case "Brown Chest":
                SetBrownChestLoot(gameObject.name);
                break;
            case "Green Chest":
                SetGreenChestLoot(gameObject.name);
                break;
            case "Purple Chest":
                SetPurpleChestLoot(gameObject.name);
                break;
            case "Red Chest":
                SetRedChestLoot(gameObject.name);
                break;
            case "Blue Chest":
                SetBlueChestLoot(gameObject.name);
                break;

            // barrels
            case "Barrel":
                SetBarrelLoot(gameObject.name);
                break;

            default:
                break;
        }
    } // set container loot based on this container's 1) name and 2) scene

    private void SetLockedState()
    {
        string name = gameObject.name;

        if (name.Contains("Brown"))
        {
            name = "Brown";
        } // used to get all brown chests treated the same (e.g. Brown Chest1 -> Brown)

        switch (name)
        {
            case "Brown":
                locked = false; // brown chests are never locked (early game)
                break;

            default:
                locked = true; // all other chests are locked
                break;
        }
    } // brown chests (early game) unlocked, all other containers locked

    private void HandleSetContainerKeyName()
    {
        if (locked)
        {
            nameKey = ReturnKeyName(SubstringToRemove());
        }
    } // handles setting name for this container's key

    private string SubstringToRemove()
    {
        string substringToRemove = "";

        if (gameObject.name.Contains(" Chest"))
        {
            substringToRemove = " Chest";
        }

        return substringToRemove;
    } // remove " Chest" substring, used in setting this container's key's name

    private string ReturnKeyName(string subStrToRemove)
    {
        string strToRemove = subStrToRemove;

        string keyNameFixed =
            helperMethods.RemoveNumbersFromName(helperMethods.RemoveSubstring(gameObject.name, strToRemove)) + "Key";

        return keyNameFixed;
    } // sets name of this container's key


    // public interface 
    public void OnClick()
    {
        string nameContainer = helperMethods.RemoveNumbersFromName(gameObject.name);

        if (!opened)
        {
            // if locked
            if (locked)
            {
                if (ReturnHaveKey())
                {
                    HandleUnlockContainer(nameContainer);
                }
                else
                {
                    HandleContainerLocked(nameContainer);
                }
            }
            // if unlocked
            else
            {
                OpenContainer();
                DropItem();
            }

        HandleTutorial();
        }
    } // handle player click on this container


    // container interaction routines
    private void HandleUnlockContainer(string nameContainer)
    {
        string name = nameContainer;

        messageDisplay.SetMessage(name + " unlocked with a " + helperMethods.AddSpacesToName(nameKey), "");
        UnlockContainer();
        RemoveKey();
        OpenContainer();
        DropItem();
    }

    private void HandleContainerLocked(string nameContainer)
    {
        name = nameContainer;
        ambientSFX.PlaySFX(1, 1, ambientSFX.ReturnSFXClip("LockedContainer"), options.vSFX);
        messageDisplay.SetMessage("The " + name + " is locked", "");
    }


    // key helper methods
    private bool ReturnHaveKey()
    {
        if (xmlManager.ReturnHaveKey(nameKey))
        {
            return true;
        }
        return false;
    }

    private void RemoveKey()
    {
        xmlManager.RemoveKey(nameKey);
    }
    

    // container functionality 
    private void UnlockContainer()
    {
        locked = false;
    }

    private void OpenContainer()
    {
        ambientSFX.PlaySFX(1, 1, ambientSFX.ReturnSFXClip("OpenContainer1"), options.vSFX);
        AnimateOpen();
        opened = true;
    }

    private void DropItem()
    {
        if (!xmlManager.ReturnItemInInventory(loot))
        {
            spawner.LootDrop(loot, gameObject);
        }
    }


    // set container loot
    private void SetBlackChestLoot(string chestName)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "FarrowglenForest":
                loot = "SpikedLeatherBracer";
                break;
            case "FarrowglenOutskirts":
                loot = "SteelBracer";
                break;
            case "ThePits2":
                loot = "StuddedSteelBracer";
                break;
            case "FarrowglenVillageN":
                loot = "PlatedSteelBracer";
                break;
            case "ProtectorateOfficeFarrowglen":
                loot = "FortitudeGraspBracer";
                break;
            case "WybarForest":
                loot = "BrilliantGuardBracer";
                break;
            case "GinsbergVillage":
                loot = "VengeanceWillBracer";
                break;

            default:
                break;
        }
    }

    private void SetBlueChestLoot(string chestName)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Fairbrook":
                loot = "LeatherBracer";
                break;
            case "Woodsman'sCottage":
                loot = "BeastGlowChoker";
                break;
            case "FairbrookForest":
                loot = "GraceCharmAmulet";
                break;
            case "ThePits1":
                loot = "PortentRing";
                break;
            case "AbandonedHouseWybar":
                loot = "Daevon'sWill";
                break;

            default:
                break;
        }
    }

    private void SetBrownChestLoot(string chestName)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Fairbrook":
                switch (chestName)
                {
                    case "Brown Chest1":
                        loot = "ShortSword";
                        break;
                    case "Brown Chest2":
                        loot = "Mallet";
                        break;
                }
                break;
            case "UmbrageGrotto":
                loot = "Dagger";
                break;

            default:
                break;
        }
    }

    private void SetGreenChestLoot(string chestName)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "FairbrookForest":
                loot = "ThornLoop";
                break;
            case "ThePits1":
                loot = "CruelCharm";
                break;
            case "ThePits2":
                loot = "Winston'sAmulet";
                break;
            case "HemlockGate":
                loot = "FlairAmulet";
                break;
            case "WybarForest":
                loot = "Soldier'sGritAmulet";
                break;
            case "GinsbergVillage":
                loot = "QuickTouchChoker";
                break;

            default:
                break;
        }
    }

    private void SetPurpleChestLoot(string chestName)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "FairbrookForest":
                if (chestName.Contains("1"))
                {
                    loot = "Hunter'sRing";
                }
                if (chestName.Contains("2"))
                {
                    loot = "HearthPendant"; 
                }
                break;
            case "FarrowglenOutskirts":
                loot = "SufferStrikeClasp";
                break;
            case "FarrowglenVillageS":
                loot = "MagicBand";
                break;
            case "ThePits1":
                loot = "StaunchGuardRing";
                break;
            case "MayorsOffice":
                loot = "SolidRing";
                break;

            default:
                break;
        }
    }

    private void SetRedChestLoot(string chestName)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Fairbrook":
                loot = "BearMightBand";
                break;
            case "UmbrageGrotto":
                loot = "Guard'sHeartAmulet";
                break;
            case "CottageFairbrookForest":
                loot = "BeastGlowChoker";
                break;
            case "FairbrookAdvGuild2":
                loot = "PinchRipChoker";
                break;
            case "FarrowglenVillageS":
                loot = "Defender'sTorc";
                break;

            default:
                break;
        }
    }

    private void SetBarrelLoot(string chestName)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "UmbrageGrotto":
                loot = "DeterminationBand";
                break;
            
            default:
                break;
        }
    }


    // animation helper methods
    private void AnimateOpen()
    {
        if (gameObject.name.Contains("Chest"))
        {
            animator.SetBool("isOpened", true);
        }
    }


    // tutorial methods
    private void HandleTutorial()
    {
        if (loot == "ShortSword")
        {
            StartCoroutine(Tutorial());
        }
    } // displays tutorial popup for picking up items if player opens the first encountered chest

    private IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(0.6f);

        tutorial.HandleTutorialPopup("Pickup");
    }
}