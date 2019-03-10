using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// OVERVIEW

// Gives an example of how I handle using UI in-game

// Handles toggling on, off of death message presented to player on death
// Handles retrieval of a random death message from XML data
// Handles adding nameQuest of enemy that killed player, nameQuest of current scene, to death message

// Attach to deathMessage gameObject (child of HUD gameObject)

public class DeathMessage : MonoBehaviour
{
    // private NeetFreek classes
    private HelperMethods helperMethods;
    private ManagerCursor managerCursor;
    private XmlManager xmlManager;

    // private Unity classes
    private static readonly Vector3 minimised = new Vector3(0, 0, 0), maximised = new Vector3(1, 1, 1);
    
    // private fields
    private string nameEnemy; // nameQuest of enemy which killed player
    private Text messageDeath; // textJournal message presented to player in deathPanel on player death


    // Setup and Unity methods
    private void Start ()
    {
        SetClassReferences();
        TogglePanel(false);
	}

    private void SetClassReferences()
    {
        helperMethods = GameObject.Find("XMLManager").GetComponent<HelperMethods>();
        managerCursor = GameObject.Find("Cam").GetComponent<ManagerCursor>();
        messageDeath = GameObject.Find("DeathMessage").GetComponent<Text>();
        xmlManager = GameObject.Find("XMLManager").GetComponent<XmlManager>();
    }


    // called by SheetPC on player death
    public void TogglePanel(bool display) // true == on, false == off
    {
        if (display)
        {
            managerCursor.ToggleCursor(true); // re-enable cursor on player death (turned off by screen fade)
            SetDeathMessage();
            gameObject.transform.localScale = maximised;
        }
        if (!display)
        {
            ClearDeathMessage();
            gameObject.transform.localScale = minimised;
        }
    }
    // called by SheetPC on player death
    public void ReceiveEnemyName(string name)
    {
        nameEnemy = helperMethods.RemoveNumbersFromNameEnd(name);
    }


    // Handle setting death message
    private void SetDeathMessage()
    {
        string message = ReturnFlavourText();

        message = InsertEnemyName(message);
        message = InsertAreaName(message);
        messageDeath.text = message;
    } // set messageDeath.textJournal, consisting of 1) message, 2) enemy that killed player, 3) current scene nameQuest

    private string ReturnFlavourText()
    {
        int rand = UnityEngine.Random.Range(1, 10);

        string text = xmlManager.ReturnLoreDataText("Death"+ rand.ToString());
        return text;
    } // return one of 10 possible death messages from XML database

    private string InsertEnemyName(string flavourText)
    {
        string text = flavourText;
        if (flavourText.Contains("killer"))
        {
            text = text.Replace("killer", nameEnemy);
        }
        return text;
    } // replace instances of "killer" in message from data with nameQuest of enemy that killed player

    private string InsertAreaName(string flavourText)
    {
        string text = flavourText;
        if (flavourText.Contains("location"))
        {
            text = text.Replace("location", helperMethods.AddSpacesToName(SceneManager.GetActiveScene().name));
        }
        return text;
    } // replace instances of "location" in message from data with nameQuest of current scene


    // Handle clearing death message
    private void ClearDeathMessage()
    {
        messageDeath.text = "";
    }
}