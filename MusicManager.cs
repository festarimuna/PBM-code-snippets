using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// OVERVIEW

// Gives an example of how I handle using assets (in this case audio) in-game

// Contains lists of playlists of music clips
// Handles selecting and setting music clip for menuMainStart scene
// Handles selecting and playing playlists of music clips based on currently loaded scene
// Handles music clip fade-in, fade out effects on loading, unloading scenes

// Attached to MusicManager gameObject

public class MusicManager : MonoBehaviour
{
    // private Unity classes
    private static MusicManager musicPlayerIns = null; // for singleton
    private static AudioSource audioSource; // attached to gameObject, assigned in EnsureSingleton();
    private Scene sceneCurrent; // current scene, assigned in OnSceneLoad()
    // music clips, assigned in editor
    public AudioClip track1, track2, track3, track4, track5, track6, track7, track8, track9, track10,
                    track11, track12, track13, track14, track15, track16, track17, track18, track19, track20,
                    track21, track22, track23, track24, track25, track26, track27, track28, track29, track30,
                    track31, track32, track33, track34, track35, track36;
    // music clip playlists
    public List<AudioClip> tracksCivilised, tracksDangerous, tracksGloomy, tracksMenu, tracksSunny, tracksWild;
    private List<AudioClip> currentPlaylist; 

    // private readonly fields
    private static readonly float fadeInterval = 0.1f, fadeInTime = 3f, fadeOutTime = 2f;
    private static readonly int trackInterval = 5;

    // private fields
    private int currentTrack = 0;
    private float trackLength;


    // Setup and Unity methods
    private void Awake()
    {
        EnsureSingleton();
    }

    private void EnsureSingleton()
    {
        
        if (musicPlayerIns != null)
        {
            Destroy(gameObject);
        }
        else
        {
            musicPlayerIns = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SetupPlaylists();
        SetupMainMenuMusic();
    }

    private void SetupMainMenuMusic()
    {
        float volMusic = PlayerPrefs.GetFloat("volMusic", 0);
        audioSource.clip = tracksMenu[4];
        audioSource.volume = volMusic;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void SetupPlaylists()
    {
        tracksCivilised = new List<AudioClip>
        {
            track1,
            track9,
            track10,
            track13,
            track19,
            track33
        };

        tracksDangerous = new List<AudioClip>
        {
            track1,
            track2,
            track11,
            track12,
            track14,
            track16,
            track18,
            track23,
            track27,
            track28,
            track30,
            track32,
            track34,
            track35
        };

        tracksGloomy = new List<AudioClip>
        {
            track1,
            track7,
            track14,
            track19,
            track23,
            track24,
            track26,
            track28,
            track30,
            track31,
            track35
        };

        tracksMenu = new List<AudioClip>
        {
            track1,
            track2,
            track4,
            track17,
            track36
        };

        tracksSunny = new List<AudioClip>
        {
            track1,
            track3,
            track4,
            track5,
            track6,
            track8,
            track9,
            track10,
            track15,
            track20,
            track21,
            track22,
            track25,
            track33
        };

        tracksWild = new List<AudioClip>
        {
            track1,
            track5,
            track7,
            track9,
            track12,
            track15,
            track16,
            track19,
            track21,
            track26,
            track28,
            track29,
            track31
        };
    } // assign tracks to playlists


    // called by EntranceManager.cs and LevelLoader.cs before loading new scene in-game
    public void ReadyFadeOut()
    {
        StartCoroutine(FadeOut());
    }


    // music playlist methods
    private void SelectPlaylistFromScene(string nameScene)
    {
        switch (nameScene)
        {
            case "Menu_IntroText":
                ReadyTrack(track2);
                break; // exception; selects only a track (for playing menuMainStart music clip)
            case "AbandonedHouseWybar":
                ReadyPlaylist(tracksDangerous);
                break;
            case "AbandonedShackUmbrage":
                ReadyPlaylist(tracksGloomy);
                break;
            case "CottageBarnabus":
                ReadyPlaylist(tracksCivilised);
                break;
            case "CottageFairbrookForest":
                ReadyPlaylist(tracksDangerous);
                break;
            case "Fairbrook":
                ReadyPlaylist(tracksSunny);
                break;
            case "FairbrookAdvGuild1":
                ReadyPlaylist(tracksCivilised);
                break;
            case "FairbrookAdvGuild2":
                ReadyPlaylist(tracksDangerous);
                break;
            case "FairbrookForest":
                ReadyPlaylist(tracksWild);
                break;
            case "FarrowglenOutskirts":
                ReadyPlaylist(tracksSunny);
                break;
            case "FarrowglenVillageN":
                ReadyPlaylist(tracksCivilised);
                break;
            case "FarrowglenVillageS":
                ReadyPlaylist(tracksCivilised);
                break;
            case "GinsbergVillage":
                ReadyPlaylist(tracksGloomy);
                break;
            case "GoldsunFarmstead":
                ReadyPlaylist(tracksSunny);
                break;
            case "HemlockGate":
                ReadyPlaylist(tracksWild);
                break;
            case "MayorsOffice":
                ReadyPlaylist(tracksGloomy);
                break;
            case "ProtectorateOfficeFarrowglen":
                ReadyPlaylist(tracksCivilised);
                break;
            case "ThePits1":
                ReadyPlaylist(tracksDangerous);
                break;
            case "ThePits2":
                ReadyPlaylist(tracksGloomy);
                break;
            case "UmbrageGrotto":
                ReadyPlaylist(tracksGloomy);
                break;
            case "Woodsman'sCottage":
                ReadyPlaylist(tracksDangerous);
                break;
            case "WybarForest":
                ReadyPlaylist(tracksDangerous);
                break;
            default:
                break;
        }
    }

    private void ReadyPlaylist(List<AudioClip> playlist)
    {
        if (this == musicPlayerIns)
        {
            currentTrack = 0;
            ShufflePlaylist(playlist);
            currentPlaylist = playlist;
            if (playlist.Count > 0)
            {
                ReadyTrack(playlist[currentTrack]);
            }
            Invoke("ContinuePlaylist", trackLength + 1);
        }
    }

    private void ShufflePlaylist(List<AudioClip> playList)
    {
        int counterDecrement = playList.Count;
        while (counterDecrement > 1)
        {
            counterDecrement--;
            int randomTrackIndex = Random.Range(0, counterDecrement + 1);
            AudioClip clip = playList[randomTrackIndex];

            playList[randomTrackIndex] = playList[counterDecrement];
            playList[counterDecrement] = clip;
        }
    } // Slightly modified Fisher-Yates shuffle algorithm
         
    private void ContinuePlaylist()
    {        
        if (this == musicPlayerIns)
        {
            currentTrack++;
            // if playlist ended
            if (currentTrack + 1 > currentPlaylist.Count)
            {
                ReadyPlaylist(currentPlaylist);
            }            
            else
            {
                ReadyTrack(currentPlaylist[currentTrack]);
                Invoke("ContinuePlaylist", trackLength + trackInterval);
            }
        }
    } // Invoked from ReadyPlaylist after duration of selected music clip


    // music clip methods
    private void ReadyTrack(AudioClip track)
    {
        if (this == musicPlayerIns)
        {
            audioSource.Pause(); // in case already playing song
            audioSource.clip = track;
            trackLength = track.length;
            StartCoroutine(FadeIn());

        }
    } // pause previous clip, handle new clip to play

    private void PauseTrack()
    {
        audioSource.Pause();
    }


    // fade methods
    private IEnumerator FadeIn()
    {
        // get music volume
        float volMusic = PlayerPrefs.GetFloat("volMusic"); // get music volume from playerPrefs

        audioSource.UnPause();
        audioSource.loop = false; // default true for menuMainStart set false
        audioSource.volume = 0f;
        audioSource.Play();
        float f = 0f;

        while (audioSource.volume < volMusic)
        {
            audioSource.volume = f / fadeInTime;
            yield return new WaitForSeconds(fadeInterval);
            f += fadeInterval;
        }
    }

    private IEnumerator FadeOut()
    {
        float fadeDuration = fadeOutTime;
        float f = 1f;

        while (audioSource.volume > 0f)
        {
            audioSource.volume = (fadeDuration / fadeOutTime);
            yield return new WaitForSeconds(fadeInterval);
            f -= fadeInterval;
            fadeDuration -= fadeInterval;
        }
    }


    // helper methods
    private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        sceneCurrent = scene;
        SelectPlaylistFromScene(scene.name);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        PauseTrack();
    }
}