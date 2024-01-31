using KModkit;
using ModuleSprintExtras;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModuleSprintScript : MonoBehaviour
{
    // Unity objects
    // First, KModkit objects
    public KMBombInfo BombInfo;
    public KMBombModule ThisModule;
    public KMAudio BombAudio;
    public KMModSettings ModuleSettings;
    public class Settings
    {
        public int TimePerModule;
        public string note;
    }

    // Second, Selectables (technically a KModkit script but whatever)
    public KMSelectable StartButton, NextButton;
    public KMSelectable ColorblindButton;
    public KMSelectable[] WireSelectables; // Wires
    public KMSelectable ButtonSelectable; // Button
    public KMSelectable MorseLightSelectable, MorseDotSelectable, MorseDashSelectable, MorseNextSelectable, MorseSubmitSelectable, MorseClearSelectable; // Morse
    public KMSelectable[] SimonSelectables; // Simon
    public KMSelectable[] MazeSelectable; // Maze, 0 = up, 1 = left, 2 = right, 3 = down.
    public KMSelectable[] PasswordUpSelectables, PasswordDownSelectables; public KMSelectable PasswordSubmitSelectable; // Passwords

    // Third, General unity objects
    public GameObject TimerObject; public MeshRenderer TimerRender; Coroutine TimerHandler;
    public GameObject TopShutter, BottomShutter;
    public GameObject ForceTopShutter, ForceBottomShutter;
    public GameObject InteriorPlatform;
    public Renderer[] SolveCounters;
    public Renderer MiniSolve_Wires, MiniSolve_Button, MiniSolve_Morse, MiniSolve_Simon, MiniSolve_Maze, MiniSolve_Password;
    public Texture[] ColorblindTextures;
    public Light MiniSolve_Wires_Light, MiniSolve_Button_Light, MiniSolve_Morse_Light, MiniSolve_Simon_Light, MiniSolve_Maze_Light, MiniSolve_Password_Light;
    public Texture SolveLight_Lit, SolveLight_Unlit;
    public List<GameObject> AllModules;
    public List<GameObject> ModuleOrder = new List<GameObject>();

    // Lastly, my animation handler
    private AnimationHandler Animations = new AnimationHandler();

    // Module ID
    static int moduleIdCounter = 1;
    int ModuleID;

    // Global variables
    int CurrentStage = 0;
    /// <summary>
    /// The time per module, in seconds
    /// </summary>
    int Timer = 150;
    const float TimerSize = 0.08f;
    bool ModuleStarted;
    bool[] ModulesSolveState = new bool[4]
    {
        false, false, false, false
    };
    enum ColorEnum
    {
        RED, BLUE, YELLOW, GREEN, WHITE, BLACK
    };
    Color32[] AllColors = new Color32[]
    {
        new Color32(255, 0, 0, 0), // Red
        new Color32(0, 0, 255, 0), // Blue
        new Color32(255, 255, 0, 0), // Yellow
        new Color32(0, 255, 0, 0), // Green
        new Color32(255, 255, 255, 0), // White
        new Color32(50, 50, 50, 0) // Black
    };

    // Module Specific
    // Wires
    public GameObject[] AllWireObjects;
    public GameObject[] AllCutWireObjects;
    public Renderer[] AllCutWireUppers, AllCutWireLowers;
    public Renderer[] AllWireRenders;
    public TextMesh[] AllBottomLetters;
    public TextMesh[] AllTopLetters;

    class Wire
    {
        public ColorEnum Color;
        public int BaseConnection;
        public int EndConnection;
        public bool MustCut;
        public bool IsCut;
    }
    List<Wire> AllWires = new List<Wire>();
    /// <summary>
    /// Note: To get data, use WiresSizeRot[X][Y, Z], where: <para>X is the current wire</para> <para>Y is the end connection of the wire</para> <para>and Z is 0 for its size or 1 for its rotation</para>
    /// </summary>
    /// 
    List<Vector3[,]> WiresSizeRot = new List<Vector3[,]>()
    {
        new Vector3[,] // Wire 1
        {
            { new Vector3(0.001626383f,0.02476735f,0.00125f), new Vector3(90,0,2.56f) }, // 1 to 1
            { new Vector3(0.001626383f,0.0255f,0.00125f), new Vector3(90,0,16.12f) }, // 1 to 2
            { new Vector3(0.001626383f,0.0275f,0.00125f), new Vector3(90,0,26.8f) }, // 1 to 3
            { new Vector3(0.001626383f,0.0305f,0.00125f), new Vector3(90,0,36.6f) }, // 1 to 4
            { new Vector3(0.001626383f,0.034f,0.00125f), new Vector3(90,0,43.6f) }, // 1 to 5
            { new Vector3(0.001626383f,0.038f,0.00125f), new Vector3(90,0,50.11f) }, // 1 to 6
        },
        new Vector3[,] // Wire 2
        {
            { new Vector3(0.001622951f,0.02477001f,0.00125f), new Vector3(90,0,-10.75f) }, // 2 to 1
            { new Vector3(0.001622951f,0.02477001f,0.00125f), new Vector3(90,0,2.64f) }, // 2 to 2
            { new Vector3(0.001622951f,0.02525f,0.00125f), new Vector3(90,0,15.05f) }, // 2 to 3
            { new Vector3(0.001622951f,0.0275f,0.00125f), new Vector3(90,0,27) }, // 2 to 4
            { new Vector3(0.001622951f,0.03025f,0.00125f), new Vector3(90,0,35.6f) }, // 2 to 5
            { new Vector3(0.001622951f,0.034f,0.00125f), new Vector3(90,0,43.7f) }, // 2 to 6
        },
        new Vector3[,] // Wire 3
        {
            { new Vector3(0.001622951f,0.027f,0.00125f), new Vector3(90,0,-25.1f) }, // 3 to 1
            { new Vector3(0.001622951f,0.025f,0.00125f), new Vector3(90,0,-12.91f) }, // 3 to 2
            { new Vector3(0.001622951f,0.02477002f,0.00125f), new Vector3(90,0,-0.38f) }, // 3 to 3
            { new Vector3(0.001622951f,0.025f,0.00125f), new Vector3(90,0,13.2f) }, // 3 to 4
            { new Vector3(0.001622951f,0.027f,0.00125f), new Vector3(90,0,24) }, // 3 to 5
            { new Vector3(0.001622951f,0.0295f,0.00125f), new Vector3(90,0,34.2f) }, // 3 to 6
        },
        new Vector3[,] // Wire 4
        {
            { new Vector3(0.001622951f,0.03025f,0.00125f), new Vector3(90,0,-35.6f) }, // 4 to 1
            { new Vector3(0.001622951f,0.02725f,0.00125f), new Vector3(90,0,-25.5f) }, // 4 to 2
            { new Vector3(0.001622951f,0.02525f,0.00125f), new Vector3(90,0,-14.35f) }, // 4 to 3
            { new Vector3(0.001622951f,0.02477002f,0.00125f), new Vector3(90,0,-0.95f) }, // 4 to 4
            { new Vector3(0.001622951f,0.02477002f,0.00125f), new Vector3(90,0,10.8f) }, // 4 to 5
            { new Vector3(0.001622951f,0.02675f,0.00125f), new Vector3(90,0,23.23f) }, // 4 to 6
        },
        new Vector3[,] // Wire 5
        {
            { new Vector3(0.001622951f,0.03375f,0.00125f), new Vector3(90,0,-43.7f) }, // 5 to 1
            { new Vector3(0.001622951f,0.03f,0.00125f), new Vector3(90,0,-35.7f) }, // 5 to 2
            { new Vector3(0.001622951f,0.02725f,0.00125f), new Vector3(90,0,-26.3f) }, // 5 to 3
            { new Vector3(0.001622951f,0.0255f,0.00125f), new Vector3(90,0,-14.2f) }, // 5 to 4
            { new Vector3(0.001622951f,0.02477f,0.00125f), new Vector3(90,0,-2.6f) }, // 5 to 5
            { new Vector3(0.001622951f,0.025f,0.00125f), new Vector3(90,0,11) }, // 5 to 6
        },
        new Vector3[,] // Wire 6
        {
            // Size, then rotation
            { new Vector3(0.001622951f,0.0375f,0.00125f), new Vector3(90,0,-49) }, // 2 to 1
            { new Vector3(0.001622951f,0.03325f,0.00125f), new Vector3(90,0,-42.4f) }, // 2 to 2
            { new Vector3(0.001622951f,0.03f,0.00125f), new Vector3(90,0,-34.7f) }, // 2 to 3
            { new Vector3(0.001622951f,0.027f,0.00125f), new Vector3(90,0,-24.5f) }, // 2 to 4
            { new Vector3(0.001622951f,0.02525f,0.00125f), new Vector3(90,0,-13.75f) }, // 2 to 5
            { new Vector3(0.001622951f,0.02477001f,0.00125f), new Vector3(90,0,-0.39f) }, // 2 to 6
        }
    };
    string[,] WireTable = new string[6,6]
    {
        /*  RED     */ { "C", "1", "D", "S", "U", "B" },
        /*  BLUE    */ { "U", "B", "C", "3,6", "S", "D" },
        /*  YELLOW  */ { "B", "C", "S", "D", "2", "U" },
        /*  GREEN   */ { "D", "2,3,4", "U", "S", "B", "C" },
        /*  WHITE   */ { "S", "D", "C", "U", "B", "5" },
        /*  BLACK   */ { "1,2,3", "S", "B", "C", "D", "U" },
    };
    string WireConnectionLetters;

    // Button
    class Button
    {
        public ColorEnum Color;
        public string Text;
        public bool MustHold;
        public int ReleaseTime;
    }
    Button TheButton;
    string[] PossibleButtonText = new string[]
    {
        "PRESS", "HOLD", "DETONATE", "ABORT"
    };
    public GameObject ButtonObject;
    public TextMesh ButtonText;
    public Renderer ColorStripObject;
    public TextMesh LEDStripColorblind;
    Coroutine HoldCheck, HoldAnim;
    bool HoldingButton;
    ColorEnum LEDColor;
    int[,] HoldDownTable = new int[2,5]
    {
        // Had to flip these two from how the manual views it because if it blinks that is index 1, because int Blinking = 1
        /*LIT*/     { 5, 6, 4, 1, 0 },
        /*BLINK*/   { 3, 9, 8, 2, 7 }
    };

    // Morse
    public Renderer MorseButtonRender;
    public Texture Morse_Lit, Morse_Unlit;
    public Light MorseLight;
    public TextMesh MorseTextMesh, MorseCodeMesh;
    Coroutine MorseAnim;
    Dictionary<char, string> CharToMorse = new Dictionary<char, string>()
    {
        { 'A', ".-/" },
        { 'B', "-.../" },
        { 'C', "-.-./" },
        { 'D', "-../" },
        { 'E', "./" },
        { 'F', "..-./" },
        { 'G', "--./" },
        { 'H', "..../" },
        { 'I', "../" },
        { 'J', ".---/" },
        { 'K', "-.-/" },
        { 'L', ".-../" },
        { 'M', "--/" },
        { 'N', "-./" },
        { 'O', "---/" },
        { 'P', ".--./" },
        { 'Q', "--.-/" },
        { 'R', ".-./" },
        { 'S', ".../" },
        { 'T', "-/" },
        { 'U', "..-/" },
        { 'V', "...-/" },
        { 'W', ".--/" },
        { 'X', "-..-/" },
        { 'Y', "-.--/" },
        { 'Z', "--../" },
        { '1', ".----/" },
        { '2', "..---/" },
        { '3', "...--/" },
        { '4', "....-/" },
        { '5', "...../" },
        { '6', "-..../" },
        { '7', "--.../" },
        { '8', "---../" },
        { '9', "----./" },
        { '0', "-----/" },
    };
    enum PossibleMorseWords
    {
        DETONATE, STRIKE, TIME, NUMBER, VOWEL, DVI, PARALLEL, PS2, RJ45, SERIAL, STEREORCA, AC, COMPONENTVIDEO, HDMI, COMPOSITEVIDEO, VGA, USB, PCMCIA
    };
    bool MessageIncoming = false;
    string EnteredMorse = "", EnteredText = "";
    string GeneratedMorse = "";
    PossibleMorseWords GeneratedText;

    // Simon
    public Light[] SimonLights;
    public GameObject SimonColorblinds;
    protected enum SimonColor // Needs to be protected because it is used in a protected function's parameter
    {
        RED, BLUE, YELLOW, GREEN
    }
    enum SimonLightType
    {
        Blinking, Flickering, Lit
    }
    class SimonClass
    {
        public SimonColor SimonGeneratedColor;
        public SimonLightType SimonGeneratedLight;
        public SimonColor SimonHeldButton;
        public string SimonColorCycle;
        public string CorrectOrder;
        public string BrokenButtons;
        public int ButtonsPressed = 0;
    }
    SimonClass Simon = new SimonClass();
    string[,] SimonTable = new string[,]
    {
        //               Red     Blue    Yellow  Green
        /*Blinking*/    {"rbyg", "rygb", "rgby", "bygr" },
        /*Flashing*/    {"bgry", "bryg", "yrgb", "ygbr" },
        /*Lit Up*/      {"ybrg", "gybr", "gbry", "gryb" }
    };
    Coroutine SimonCheck, SimonAnim;
    bool HoldingSimon = false;
    SimonColor ColorPressed;

    // Maze
    enum MazeShape
    {
        STAR, DIAMOND, CIRCLE, SQUARE
    }
    MazeShape GeneratedMazeShape;
    public GameObject Pawn;
    public Renderer[] MarkingRenders = new Renderer[2];
    public Texture[] MarkingTextures = new Texture[4];
    public GameObject[] Markings = new GameObject[2];
    string[,] Maze;
    int MazeID;
    int CoordX, CoordY;
    Vector3[,] MazeLocations = new Vector3[,]
    {
        { new Vector3(-2.678f, 0.01f, 2.629f), new Vector3(-1.321f, 0.01f, 2.629f), new Vector3(0.018f, 0.01f, 2.629f), new Vector3(1.342f, 0.01f, 2.629f), new Vector3(2.708f, 0.01f, 2.629f) },
        { new Vector3(-2.678f, 0.01f, 1.337f), new Vector3(-1.321f, 0.01f, 1.337f), new Vector3(0.018f, 0.01f, 1.337f), new Vector3(1.342f, 0.01f, 1.337f), new Vector3(2.708f, 0.01f, 1.337f) },
        { new Vector3(-2.678f, 0.01f, -0.008f), new Vector3(-1.321f, 0.01f, -0.008f), new Vector3(0.018f, 0.01f, -0.008f), new Vector3(1.342f, 0.01f, -0.008f), new Vector3(2.708f, 0.01f, -0.008f) },
        { new Vector3(-2.678f, 0.01f, -1.267f), new Vector3(-1.321f, 0.01f, -1.267f), new Vector3(0.018f, 0.01f, -1.267f), new Vector3(1.342f, 0.01f, -1.267f), new Vector3(2.708f, 0.01f, -1.267f) },
        { new Vector3(-2.678f, 0.01f, -2.599f), new Vector3(-1.321f, 0.01f, -2.599f), new Vector3(0.018f, 0.01f, -2.599f), new Vector3(1.342f, 0.01f, -2.599f), new Vector3(2.708f, 0.01f, -2.599f) }
    };

    // Password
    public TextMesh[] PassCharTextMesh;
    char[] PasswordStartingCharacters = new char[6];
    char[] PasswordCorrectCharacters = new char[6];
    char[] PasswordCurrentCharacters = new char[6];
    string AllPossibleLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    Dictionary<char, char> FakePigpenConvertTable = new Dictionary<char, char>()
    {
        {'A','I'},
        {'B','H'},
        {'C','G'},
        {'D','F'},
        {'E','E'},
        {'F','D'},
        {'G','C'},
        {'H','B'},
        {'I','A'},
        {'J','R'},
        {'K','Q'},
        {'L','P'},
        {'M','O'},
        {'N','N'},
        {'O','M'},
        {'P','L'},
        {'Q','K'},
        {'R','J'},
        {'S','V'},
        {'T','U'},
        {'U','T'},
        {'V','S'},
        {'W','Z'},
        {'X','Y'},
        {'Y','X'},
        {'Z','W'},
    };

    //Unity script start
    void Awake()
    {
        ModuleID = moduleIdCounter++;
    }

    void Start()
    {
        // Set selectables
        StartButton.OnInteract = StartButton_Press;
        ColorblindButton.OnInteract = ColorblindButton_Click;
        ToggleModuleSelectables(true);

        foreach (GameObject module in AllModules)
        {
            module.SetActive(false);
        }
        InteriorPlatform.SetActive(false);

        GetTimePerModule();
    }

    void ToggleModuleSelectables(bool EnableSelectables)
    {
        if (EnableSelectables)
        {
            // Wires
            WireSelectables[0].OnInteract = delegate { return Wire_Click(0); };
            WireSelectables[1].OnInteract = delegate { return Wire_Click(1); };
            WireSelectables[2].OnInteract = delegate { return Wire_Click(2); };
            WireSelectables[3].OnInteract = delegate { return Wire_Click(3); };
            WireSelectables[4].OnInteract = delegate { return Wire_Click(4); };
            WireSelectables[5].OnInteract = delegate { return Wire_Click(5); };

            // Button
            ButtonSelectable.OnInteract = Button_Press;
            ButtonSelectable.OnInteractEnded = Button_Release;

            // Morse
            MorseLightSelectable.OnInteract = MorseLight_Press;
            MorseDotSelectable.OnInteract = delegate { return MorseChar_Press(false); };
            MorseDashSelectable.OnInteract = delegate { return MorseChar_Press(true); };
            MorseNextSelectable.OnInteract = MorseNext_Press;
            MorseClearSelectable.OnInteract = MorseClear_Press;
            MorseSubmitSelectable.OnInteract = MorseSubmit_Press;

            // Simon
            SimonSelectables[0].OnInteract = delegate { return Simon_Press(0); };
            SimonSelectables[1].OnInteract = delegate { return Simon_Press((SimonColor)1); };
            SimonSelectables[2].OnInteract = delegate { return Simon_Press((SimonColor)2); };
            SimonSelectables[3].OnInteract = delegate { return Simon_Press((SimonColor)3); };

            SimonSelectables[0].OnInteractEnded = delegate { Simon_Release(Simon.SimonHeldButton); };
            SimonSelectables[1].OnInteractEnded = delegate { Simon_Release(Simon.SimonHeldButton); };
            SimonSelectables[2].OnInteractEnded = delegate { Simon_Release(Simon.SimonHeldButton); };
            SimonSelectables[3].OnInteractEnded = delegate { Simon_Release(Simon.SimonHeldButton); };

            // Maze
            MazeSelectable[0].OnInteract = delegate { return MazeArrow_Press("U"); };
            MazeSelectable[1].OnInteract = delegate { return MazeArrow_Press("L"); };
            MazeSelectable[2].OnInteract = delegate { return MazeArrow_Press("R"); };
            MazeSelectable[3].OnInteract = delegate { return MazeArrow_Press("D"); };

            // Passwords
            PasswordUpSelectables[0].OnInteract = delegate { return PasswordUp_Press(0); };
            PasswordUpSelectables[1].OnInteract = delegate { return PasswordUp_Press(1); };
            PasswordUpSelectables[2].OnInteract = delegate { return PasswordUp_Press(2); };
            PasswordUpSelectables[3].OnInteract = delegate { return PasswordUp_Press(3); };
            PasswordUpSelectables[4].OnInteract = delegate { return PasswordUp_Press(4); };
            PasswordUpSelectables[5].OnInteract = delegate { return PasswordUp_Press(5); };

            PasswordDownSelectables[0].OnInteract = delegate { return PasswordDown_Press(0); };
            PasswordDownSelectables[1].OnInteract = delegate { return PasswordDown_Press(1); };
            PasswordDownSelectables[2].OnInteract = delegate { return PasswordDown_Press(2); };
            PasswordDownSelectables[3].OnInteract = delegate { return PasswordDown_Press(3); };
            PasswordDownSelectables[4].OnInteract = delegate { return PasswordDown_Press(4); };
            PasswordDownSelectables[5].OnInteract = delegate { return PasswordDown_Press(5); };

            PasswordSubmitSelectable.OnInteract = PasswordSolve_Press;
        }
        else
        {
            // Wires
            WireSelectables[0].OnInteract = delegate { return EmptyButton_Click(); };
            WireSelectables[1].OnInteract = delegate { return EmptyButton_Click(); };
            WireSelectables[2].OnInteract = delegate { return EmptyButton_Click(); };
            WireSelectables[3].OnInteract = delegate { return EmptyButton_Click(); };
            WireSelectables[4].OnInteract = delegate { return EmptyButton_Click(); };
            WireSelectables[5].OnInteract = delegate { return EmptyButton_Click(); };

            // Button
            ButtonSelectable.OnInteract = EmptyButton_Click;
            ButtonSelectable.OnInteractEnded = EmptyButton_Release;

            // Morse
            MorseLightSelectable.OnInteract = EmptyButton_Click;
            MorseDotSelectable.OnInteract = delegate { return EmptyButton_Click(); };
            MorseDashSelectable.OnInteract = delegate { return EmptyButton_Click(); };
            MorseNextSelectable.OnInteract = EmptyButton_Click;
            MorseClearSelectable.OnInteract = EmptyButton_Click;
            MorseSubmitSelectable.OnInteract = EmptyButton_Click;

            // Simon
            SimonSelectables[0].OnInteract = delegate { return EmptyButton_Click(); };
            SimonSelectables[1].OnInteract = delegate { return EmptyButton_Click(); };
            SimonSelectables[2].OnInteract = delegate { return EmptyButton_Click(); };
            SimonSelectables[3].OnInteract = delegate { return EmptyButton_Click(); };

            SimonSelectables[0].OnInteractEnded = delegate { EmptyButton_Release(); };
            SimonSelectables[1].OnInteractEnded = delegate { EmptyButton_Release(); };
            SimonSelectables[2].OnInteractEnded = delegate { EmptyButton_Release(); };
            SimonSelectables[3].OnInteractEnded = delegate { EmptyButton_Release(); };

            // Maze
            MazeSelectable[0].OnInteract = delegate { return EmptyButton_Click(); };
            MazeSelectable[1].OnInteract = delegate { return EmptyButton_Click(); };
            MazeSelectable[2].OnInteract = delegate { return EmptyButton_Click(); };
            MazeSelectable[3].OnInteract = delegate { return EmptyButton_Click(); };

            // Passwords
            PasswordUpSelectables[0].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordUpSelectables[1].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordUpSelectables[2].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordUpSelectables[3].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordUpSelectables[4].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordUpSelectables[5].OnInteract = delegate { return EmptyButton_Click(); };

            PasswordDownSelectables[0].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordDownSelectables[1].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordDownSelectables[2].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordDownSelectables[3].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordDownSelectables[4].OnInteract = delegate { return EmptyButton_Click(); };
            PasswordDownSelectables[5].OnInteract = delegate { return EmptyButton_Click(); };

            PasswordSubmitSelectable.OnInteract = EmptyButton_Click;
        }
    }

    void GetTimePerModule()
    {
        try
        {
            Settings settings = JsonConvert.DeserializeObject<Settings>(ModuleSettings.Settings);
            if (settings != null)
            {
                if (settings.TimePerModule < 30)
                {
                    Debug.LogFormat("(Module Sprint #{0}): Time settings was lower than 30 seconds, setting time to 30", ModuleID);
                    Timer = 30;
                }
                else if (settings.TimePerModule > 240)
                {
                    Debug.LogFormat("(Module Sprint #{0}): Time settings was lower than 2 minutes, setting time to 240", ModuleID);
                    Timer = 240;
                }
                else
                {
                    Debug.LogFormat("(Module Sprint #{0}): Setting time to {1} seconds", ModuleID, settings.TimePerModule);
                    Timer = settings.TimePerModule;
                }
            }
            else
            {
                Debug.LogFormat("(Module Sprint #{0}): Could not find settings. Setting time to default (150 seconds)", ModuleID);
                Timer = 150;
            }
        }
        catch (JsonReaderException)
        {
            Debug.LogFormat("(Module Sprint #{0}): An error occurred while trying to read the JSON. Setting time to default (150 seconds)", ModuleID);
            Timer = 150;
        }

        GetModuleOrder();
    }

    void GetModuleOrder()
    {
        // First of all, get the order of all modules.
        for (int T = 0; T < 4; T++)
        {
            GameObject Module = AllModules[Random.Range(0, AllModules.Count)];
            ModuleOrder.Add(Module);
            AllModules.Remove(Module);
        }

        // Then print the order
        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Module order: {2}, {3}, {4}, {5}", ModuleID, CurrentStage + 1, ModuleOrder[0].name.Replace("Module",""), ModuleOrder[1].name.Replace("Module", ""), ModuleOrder[2].name.Replace("Module", ""), ModuleOrder[3].name.Replace("Module", ""));

        GenerateModules();
    }

    void GenerateModules()
    {
        // Generate the rules of each
        foreach (GameObject Module in ModuleOrder)
        {
            switch (Module.name)
            {
                case "WiresModule":
                    {
                        // Generate all wires
                        for (int T = 0; T < 6; T++)
                        {
                            AllWires.Add(new Wire()
                            {
                                BaseConnection = T + 1,
                                EndConnection = Random.Range(0, 6),
                                Color = (ColorEnum)Random.Range(0, 6)
                            });
                        }

                        // Set colors and generate the connections
                        for (int T = 0; T < 6; T++)
                        {
                            AllWireRenders[T].material.color = AllColors[(int)AllWires[T].Color];
                            AllCutWireUppers[T].material.color = AllColors[(int)AllWires[T].Color];
                            AllCutWireLowers[T].material.color = AllColors[(int)AllWires[T].Color];

                            int EndConnection = AllWires[T].EndConnection;

                            AllWireObjects[T].transform.localScale = WiresSizeRot[T][EndConnection, 0];
                            AllWireObjects[T].transform.localEulerAngles = WiresSizeRot[T][EndConnection, 1];
                        }

                        // Generate the bottom letters.
                        WireConnectionLetters = "";
                        for (int T = 0; T < 6; T++)
                        {
                            int index = Random.Range(0, "ABCDEF".Length);
                            AllBottomLetters[T].text = "ABCDEF"[index].ToString();
                            WireConnectionLetters += "ABCDEF"[index].ToString();
                        }
                        break;
                    }
                case "ButtonModule":
                    {
                        // Generate the button itself
                        TheButton = new Button()
                        {
                            Color = (ColorEnum)Random.Range(0, 6),
                            Text = PossibleButtonText[Random.Range(0, PossibleButtonText.Length)]
                        };

                        ButtonObject.GetComponent<Renderer>().material.color = AllColors[(int)TheButton.Color];
                        ButtonText.text = TheButton.Text;
                        if (TheButton.Color == ColorEnum.YELLOW || TheButton.Color == ColorEnum.WHITE || TheButton.Color == ColorEnum.GREEN)
                        {
                            ButtonText.color = Color.black;
                        }
                        else
                        {
                            ButtonText.color = Color.white;
                        }
                        break;
                    }
                case "MorseModule":
                    {
                        // Generate the plain word
                        GeneratedText = (PossibleMorseWords)Random.Range(0, 17);

                        // Generate the morse code
                        foreach (char Letter in GeneratedText.ToString())
                        {
                            GeneratedMorse += CharToMorse[Letter];
                        }
                        break;
                    }
                case "SimonModule":
                    {
                        // Generate the color and light type
                        Simon.SimonGeneratedColor = (SimonColor)Random.Range(0, 4);
                        Simon.SimonGeneratedLight = (SimonLightType)Random.Range(0, 3);
                        break;
                    }
                case "MazeModule":
                    {
                        // Generate the maze
                        int MazeGenerator = Random.Range(0, 4);
                        GeneratedMazeShape = (MazeShape)Random.Range(0, 4);

                        // Generate maze walls and options
                        // U = up, D = down, L = left, R = right
                        // * = star, d = diamond, c = circle, s = square
                        switch (MazeGenerator)
                        {
                            case 0: // Maze 1
                                {
                                    MazeID = 1;
                                    Markings[0].gameObject.transform.localPosition = MazeLocations[1, 1];
                                    Markings[1].gameObject.transform.localPosition = MazeLocations[1, 3];
                                    Maze = new string[,]
                                    {
                                    { "R,D","L,D","U*,D","R,D","L,D" },
                                    { "U,D","U,R","U,L","U","U,D" },
                                    { "Ld,U,R","L,D","R","R,L,D","U,D,L,Rc" },
                                    { "D","U,D","R,D","U,L","U,D" },
                                    { "U,R","U,L,R","U,L,Ds","R","U,L" }
                                    };
                                    break;
                                }
                            case 1: // Maze 2
                                {
                                    MazeID = 2;
                                    Markings[0].transform.localPosition = MazeLocations[2, 4];
                                    Markings[1].transform.localPosition = MazeLocations[4, 2];
                                    Maze = new string[,]
                                    {
                                    { "R,D","L,R","Uc,L","D","D" },
                                    { "U,R","L,D","R","U,L,D","U,D" },
                                    { "L*,D","U,D","R,D","U,L","U,D,Rs" },
                                    { "U,D","U,R","U,L,D","D","U,D" },
                                    { "U,R","L,R","U,L,R,Dd","U,L,R","U,L" }
                                    };
                                    break;
                                }
                            case 2: // Maze 3
                                {
                                    MazeID = 3;
                                    Markings[0].transform.localPosition = MazeLocations[1, 3];
                                    Markings[1].transform.localPosition = MazeLocations[3, 1];
                                    Maze = new string[,]
                                    {
                                    { "R,D","L,D","Us,D","R","L,D" },
                                    { "U,D","U","U,R","L,R","U,L,D" },
                                    { "Lc,U,D","R,D","L,R","L,R","U,L,Rd" },
                                    { "U,R,D","U,L","R","R,L","L,D" },
                                    { "U,R","L,R","L,R,D*","L,R","U,L" }
                                    };
                                    break;
                                }
                            case 3: // Maze 4
                                {
                                    MazeID = 4;
                                    Markings[0].transform.localPosition = MazeLocations[1, 1];
                                    Markings[1].transform.localPosition = MazeLocations[4, 2];
                                    Maze = new string[,]
                                    {
                                    { "R,D","L","Ud,D","R,D","L,D" },
                                    { "U,R,D","L,D","U,D","U","U,D" },
                                    { "Ls,U,D","U,R","U,L","R,D","U,D,L,R*" },
                                    { "U,D","R,D","L,D","U,D","U,D" },
                                    { "U,R","U,L","U,R,Dc","U,L","U" }
                                    };
                                    break;
                                }
                        }
                        MarkingRenders[0].material.mainTexture = MarkingTextures[(int)GeneratedMazeShape];
                        MarkingRenders[1].material.mainTexture = MarkingTextures[(int)GeneratedMazeShape];

                        break;
                    }
                case "PasswordModule":
                    {
                        // Generate letters
                        for (int T = 0; T < 6; T++)
                        {
                            int LetterGenerator = Random.Range(0, 26);
                            PasswordStartingCharacters[T] = AllPossibleLetters[LetterGenerator];
                            PassCharTextMesh[T].text = PasswordStartingCharacters[T].ToString();

                            PassCharTextMesh[T].color = new Color32(0, 255, 0, 255);
                        }
                        Array.Copy(PasswordStartingCharacters, PasswordCurrentCharacters, 6);
                        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Initial letters are: {2}{3}{4}{5}{6}{7}.", ModuleID, CurrentStage + 1, PasswordCurrentCharacters[0], PasswordCurrentCharacters[1], PasswordCurrentCharacters[2], PasswordCurrentCharacters[3], PasswordCurrentCharacters[4], PasswordCurrentCharacters[5]);
                        break;
                    }
            }
        }
    }

    // General
    void HandleStage(int Stage)
    {
        if (Stage == 0)
        {
            // Good luck!
            ModuleStarted = true;
            StartButton.OnInteract = EmptyButton_Click;
            NextButton.OnInteract = NextButton_Click;

            ToggleModuleSelectables(true);

            HandleAnimation("Open", null, ModuleOrder[0]);
        }
        else if (Stage == 4)
        {
            // A winner is you!
            StopCoroutine(TimerHandler);
            SolveMainModule();
            return;
        }
        else
        {
            HandleAnimation("Cycle", ModuleOrder[Stage - 1], ModuleOrder[Stage]);
            StopCoroutine(TimerHandler);
        }

        // Start the timer!
        TimerHandler = StartCoroutine(Timer_Tick(Timer, TimerSize));

        // Handle solve condition per module
        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Module is {2}", ModuleID, CurrentStage + 1, ModuleOrder[Stage].name.Replace("Module", ""));
        switch (ModuleOrder[Stage].name)
        {
            case "WiresModule":
                {
                    int TEMP = 0; // Only for logging
                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Wire bases: {2}", ModuleID, CurrentStage + 1, WireConnectionLetters);
                    foreach (Wire wire in AllWires)
                    {
                        TEMP++;
                        // Used for the WireTable lookup table. For explanation:
                        // (int)wire.Color gets the current row as index to get the row of the table
                        // "ABCDEF".IndexOf(WireConnectionLetters[wire.EndConnection]) takes the letter that the wire is currently connected to, and rolls it past the table columns (A-F) to get the column of the table
                        string WireAnswer = WireTable[(int)wire.Color, "ABCDEF".IndexOf(WireConnectionLetters[wire.EndConnection])];
                        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Wire {2} color = {3}, End = {4}, Connect Letter: {5}", ModuleID, CurrentStage + 1, TEMP, wire.Color, wire.EndConnection, WireConnectionLetters[wire.EndConnection]);
                        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Wire {2} [{3},{4}] = {5}", ModuleID, CurrentStage + 1, TEMP, (int)wire.Color, "ABCDEF".IndexOf(WireConnectionLetters[wire.EndConnection]), WireAnswer);
                        switch (WireAnswer)
                        {
                            case "C": // Always cut
                                {
                                    wire.MustCut = true;
                                    break;
                                }
                            case "D": // Never cut
                                {
                                    // I know this does nothing as MustCut is already false but I still feel like adding it because why not
                                    break;
                                }
                            case "B": // Cut if more than 3 batteries
                                {
                                    if (BombInfo.GetBatteryCount() > 3)
                                    {
                                        wire.MustCut = true;
                                    }
                                    break;
                                }
                            case "U": // Cut if this is the only wire with given color
                                {
                                    wire.MustCut = true;
                                    foreach (Wire RerunWires in AllWires)
                                    {
                                        if (RerunWires.Color == wire.Color && RerunWires != wire)
                                        {
                                            wire.MustCut = false;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            case "S": // Cut if serial has a vowel
                                {
                                    if (BombInfo.GetSerialNumberLetters().Any("AEIOU".Contains))
                                    {
                                        wire.MustCut = true;
                                    }
                                    break;
                                }
                            default: // Default only used for port rule
                                {
                                    var ValidBases = WireAnswer.Split(',');
                                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Connected base {2}, allowed {3}", ModuleID, CurrentStage + 1, wire.BaseConnection, WireAnswer);
                                    foreach (string Base in ValidBases)
                                    {
                                        if (wire.BaseConnection.ToString() == Base)
                                        {
                                            wire.MustCut = true;
                                        }
                                    }
                                    break;
                                }
                        }
                    }

                    bool NoWiresToCut = true;
                    foreach (Wire wire in AllWires)
                    {
                        if (wire.MustCut)
                        {
                            NoWiresToCut = false;
                            Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Wire {2} must be cut", ModuleID, CurrentStage + 1, AllWires.IndexOf(wire) + 1);
                        }
                    }

                    if (NoWiresToCut)
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) No wires to cut according to rules, so cut all of them!", ModuleID, CurrentStage + 1);
                        foreach (Wire wire in AllWires)
                        {
                            wire.MustCut = true;
                        }
                    }
                    break;
                }
            case "ButtonModule":
                {
                    if (TheButton.Color == ColorEnum.RED && TheButton.Text == "DETONATE")
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Color is red and button says \"DETONATE\"", ModuleID, CurrentStage + 1);
                        TheButton.MustHold = true;
                    }
                    else if (TheButton.Text == "HOLD" && BombInfo.GetSerialNumberLetters().Any("HOLD".Contains))
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Text is \"HOLD\" and serial contains HOLD", ModuleID, CurrentStage + 1);
                        TheButton.MustHold = false;
                    }
                    else if (BombInfo.GetBatteryCount() > 3 && BombInfo.IsIndicatorOff(Indicator.FRK))
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) More than 3 batteries and FRK is off", ModuleID, CurrentStage + 1);
                        TheButton.MustHold = true;
                    }
                    else if (TheButton.Color == ColorEnum.WHITE && BombInfo.GetOnIndicators().Count() > 0)
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Button is white and at least one lit indicator", ModuleID, CurrentStage + 1);
                        TheButton.MustHold = false;
                    }
                    else if (TheButton.Text == "PRESS")
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Button only says \"PRESS\"", ModuleID, CurrentStage + 1);
                        TheButton.MustHold = true;
                    }
                    else
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) None of the rules apply", ModuleID, CurrentStage + 1);
                        TheButton.MustHold = false;
                    }

                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) The button should be {2}", ModuleID, CurrentStage + 1, TheButton.MustHold ? "held" : "pressed");
                    break;
                }
            case "MorseModule":
                {
                    // Since the check takes place after pressing the button, instead of when the module info is generated, there's very little to do here...
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Morse message generated is {2}", ModuleID, CurrentStage + 1, GeneratedText);
                    MorseAnim = StartCoroutine(MorseAnimation(GeneratedMorse));
                    break;
                }
            case "SimonModule":
                {
                    // First, print out the generated module data
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Light is {2} and is {3}", ModuleID, CurrentStage + 1, Simon.SimonGeneratedColor.ToString(), Simon.SimonGeneratedLight.ToString());

                    // Then, generate the expected cycle
                    Simon.SimonColorCycle = SimonTable[(int)Simon.SimonGeneratedLight, (int)Simon.SimonGeneratedColor];
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Base color cycle is {2}", ModuleID, CurrentStage + 1, Simon.SimonColorCycle);

                    // And lastly the broken buttons
                    bool VennDiagram2 = Simon.SimonGeneratedLight == SimonLightType.Flickering, 
                         VennDiagram3 = BombInfo.GetPortCount() < 5,
                         VennDiagram1 = BombInfo.GetBatteryCount() < 5,
                         VennDiagram4 = BombInfo.GetOnIndicators().Count() == 0 && BombInfo.GetOffIndicators().Count() > 0;

                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) {2}{3}{4}{5}", ModuleID, CurrentStage + 1, VennDiagram1, VennDiagram2, VennDiagram3, VennDiagram4);

                    if (VennDiagram1 && VennDiagram2 && VennDiagram3 && VennDiagram4)
                    {
                        Simon.BrokenButtons = "RBYG";
                    }
                    else if (VennDiagram1 && VennDiagram2 && VennDiagram3)
                    {
                        Simon.BrokenButtons = "BGY";
                    }
                    else if (VennDiagram2 && VennDiagram3 && VennDiagram4)
                    {
                        Simon.BrokenButtons = "RBG";
                    }
                    else if (VennDiagram1 && VennDiagram3 && VennDiagram4)
                    {
                        Simon.BrokenButtons = "B";
                    }
                    else if (VennDiagram1 && VennDiagram2 && VennDiagram4)
                    {
                        Simon.BrokenButtons = "G";
                    }
                    else if (VennDiagram1 && VennDiagram2)
                    {
                        Simon.BrokenButtons = "RG";
                    }
                    else if (VennDiagram1 && VennDiagram3)
                    {
                        Simon.BrokenButtons = "RYG";
                    }
                    else if (VennDiagram1 && VennDiagram4)
                    {
                        Simon.BrokenButtons = "BG";
                    }
                    else if (VennDiagram2 && VennDiagram3)
                    {
                        Simon.BrokenButtons = "RY";
                    }
                    else if (VennDiagram2 && VennDiagram4)
                    {
                        Simon.BrokenButtons = "RBY";
                    }
                    else if (VennDiagram3 && VennDiagram4)
                    {
                        Simon.BrokenButtons = "BY";
                    }
                    else if (VennDiagram1)
                    {
                        Simon.BrokenButtons = "R";
                    }
                    else if (VennDiagram2)
                    {
                        Simon.BrokenButtons = "B";
                    }
                    else if (VennDiagram3)
                    {
                        Simon.BrokenButtons = "G";
                    }
                    else if (VennDiagram4)
                    {
                        Simon.BrokenButtons = "Y";
                    }
                    else // None
                    {
                        Simon.BrokenButtons = "";
                    }

                    if (Simon.BrokenButtons == "")
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) No broken buttons", ModuleID, CurrentStage + 1);
                    }
                    else
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Broken buttons are {2}", ModuleID, CurrentStage + 1, Simon.BrokenButtons);
                    }
                   

                    // Convert broken buttons in the correct sequence to upper case
                    foreach (var Letter in Simon.SimonColorCycle)
                    {
                        string Character = Letter.ToString();
                        if (Simon.BrokenButtons.Contains(Character.ToUpper()))
                        {
                            Simon.CorrectOrder += char.ToUpper(Letter);
                        }
                        else
                        {
                            Simon.CorrectOrder += Letter;
                        }
                    }
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) So the final color cycle is {2} (cased letters must be held)", ModuleID, CurrentStage + 1, Simon.CorrectOrder);


                    // Also the module is visible now, so start the animation
                    SimonAnim = StartCoroutine(SimonFlashingAnimation(Simon.SimonGeneratedColor, Simon.SimonGeneratedLight));
                    break;
                }
            case "MazeModule":
                {
                    // Generate location and determine exit.
                    CoordX = Random.Range(0, 5);
                    CoordY = Random.Range(0, 5);
                    Pawn.transform.localPosition = MazeLocations[CoordY, CoordX];

                    string ExitDirection = "";
                    switch (MazeID)
                    {
                        case 1:
                            {
                                Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Maze ID 1 (top left): Navigate to maze exit {2}", ModuleID, CurrentStage + 1, GeneratedMazeShape.ToString());
                                switch (GeneratedMazeShape)
                                {
                                    case MazeShape.STAR:
                                        ExitDirection = "NORTH";
                                        break;
                                    case MazeShape.DIAMOND:
                                        ExitDirection = "WEST";
                                        break;
                                    case MazeShape.CIRCLE:
                                        ExitDirection = "EAST";
                                        break;
                                    case MazeShape.SQUARE:
                                        ExitDirection = "SOUTH";
                                        break;
                                }
                                break;
                            }
                        case 2:
                            {
                                Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Maze ID 2 (top right): Navigate to maze exit {2}", ModuleID, CurrentStage + 1, GeneratedMazeShape.ToString());
                                switch (GeneratedMazeShape)
                                {
                                    case MazeShape.STAR:
                                        ExitDirection = "WEST";
                                        break;
                                    case MazeShape.DIAMOND:
                                        ExitDirection = "SOUTH";
                                        break;
                                    case MazeShape.CIRCLE:
                                        ExitDirection = "NORTH";
                                        break;
                                    case MazeShape.SQUARE:
                                        ExitDirection = "EAST";
                                        break;
                                }
                                break;
                            }
                        case 3:
                            {
                                Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Maze ID 3 (bottom left): Navigate to maze exit {2}", ModuleID, CurrentStage + 1, GeneratedMazeShape.ToString());
                                switch (GeneratedMazeShape)
                                {
                                    case MazeShape.STAR:
                                        ExitDirection = "SOUTH";
                                        break;
                                    case MazeShape.DIAMOND:
                                        ExitDirection = "EAST";
                                        break;
                                    case MazeShape.CIRCLE:
                                        ExitDirection = "WEST";
                                        break;
                                    case MazeShape.SQUARE:
                                        ExitDirection = "NORTH";
                                        break;
                                }
                                break;
                            }
                        case 4:
                            {
                                Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Maze ID 4 (bottom right): Navigate to maze exit {2}", ModuleID, CurrentStage + 1, GeneratedMazeShape.ToString());
                                switch (GeneratedMazeShape)
                                {
                                    case MazeShape.STAR:
                                        ExitDirection = "EAST";
                                        break;
                                    case MazeShape.DIAMOND:
                                        ExitDirection = "NORTH";
                                        break;
                                    case MazeShape.CIRCLE:
                                        ExitDirection = "SOUTH";
                                        break;
                                    case MazeShape.SQUARE:
                                        ExitDirection = "WEST";
                                        break;
                                }
                                break;
                            }
                    }
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Maze exit is {2}", ModuleID, CurrentStage + 1, ExitDirection);

                    break;
                }
            case "PasswordModule":
                {
                    // Generate correct letters through ciphers
                    // Column 1: Ceasar
                    int CipherShift = BombInfo.GetSerialNumberNumbers().First();
                    if (Regex.IsMatch(PasswordStartingCharacters[0].ToString(), "[A-J]"))
                    {
                        PasswordCorrectCharacters[0] = (char)(((PasswordStartingCharacters[0] - 'A' + CipherShift) % 26) + 'A');
                        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 1st letter is in A-J, so shift by {2} to get {3}", ModuleID, CurrentStage + 1, CipherShift, PasswordCorrectCharacters[0]);
                    }
                    else
                    {
                        PasswordCorrectCharacters[0] = (char)(((PasswordStartingCharacters[0] - 'A' - CipherShift) % 26) + 'A');
                        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 1st letter is outside of A-J, so shift by -{2} to get {3}", ModuleID, CurrentStage + 1, CipherShift, PasswordCorrectCharacters[0]);
                    }

                    // Column 2: Pigpen except not really
                    PasswordCorrectCharacters[1] = FakePigpenConvertTable[PasswordStartingCharacters[1]];
                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 2nd letter is {2}->{3}", ModuleID, CurrentStage + 1, PasswordStartingCharacters[1], PasswordCorrectCharacters[1]);

                    // Column 3: Pangram Cipher
                    string Pangram = "THEQUICKBROWNFOXJUMPSOVERTHELAZYDOG";
                    int LetterIndex1 = (PasswordStartingCharacters[2] - 'A' + 1) * BombInfo.GetSerialNumberNumbers().Last() % 34;
                    PasswordCorrectCharacters[2] = Pangram[LetterIndex1];
                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 3rd letter is {2} (Index = {3})", ModuleID, CurrentStage + 1, PasswordCorrectCharacters[2], LetterIndex1);

                    // Column 4: Ceasar again
                    if (Regex.IsMatch(PasswordStartingCharacters[3].ToString(), "[A-J]"))
                    {
                        PasswordCorrectCharacters[3] = (char)(((PasswordStartingCharacters[3] - 'A' + CipherShift) % 26) + 'A');
                        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 4th letter is in A-J, so shift by {2} to get {3}", ModuleID, CurrentStage + 1, CipherShift, PasswordCorrectCharacters[3]);
                    }
                    else
                    {
                        PasswordCorrectCharacters[3] = (char)(((PasswordStartingCharacters[3] - 'A' - CipherShift) % 26) + 'A');
                        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 4th letter is outside of A-J, so shift by -{2} to get {3}", ModuleID, CurrentStage + 1, CipherShift, PasswordCorrectCharacters[3]);
                    }

                    // Column 5: Pigpen but still not really
                    PasswordCorrectCharacters[4] = FakePigpenConvertTable[PasswordStartingCharacters[4]];
                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 5th letter is {2}->{3}", ModuleID, CurrentStage + 1, PasswordStartingCharacters[4], PasswordCorrectCharacters[4]);

                    // column 6: Pangram again
                    int LetterIndex2 = (PasswordStartingCharacters[5] - 'A' + 1) * BombInfo.GetSerialNumberNumbers().Last() % 34;
                    PasswordCorrectCharacters[5] = Pangram[LetterIndex2];
                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) 6th letter is {2} (Index = {3})", ModuleID, CurrentStage + 1, PasswordCorrectCharacters[5], LetterIndex2);

                    // Logging
                    string FinalSolutionLogging = "";
                    foreach (char Letter in PasswordCorrectCharacters)
                    {
                        FinalSolutionLogging += Letter;
                    }
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Final letters in order: {2}", ModuleID, CurrentStage + 1, FinalSolutionLogging);

                    break;
                }
        }

        // Handle colorblind, overwrite generated text with colorblind ones.
        HandleColorblindUpdate();
    }

    // Wires
    protected bool Wire_Click(int WireIndex)
    {
        if (!AllWires[WireIndex].IsCut)
        {
            BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, transform);
            AllWires[WireIndex].IsCut = true;

            WireSelectables[WireIndex].AddInteractionPunch(0.5f);
            Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Cutting wire {2}...", ModuleID, CurrentStage + 1, WireIndex + 1);
            WireSelectables[WireIndex].gameObject.SetActive(false);
            AllCutWireObjects[WireIndex].SetActive(true);

            if (AllWires[WireIndex].MustCut)
            {
                CheckWiresSolved();
            }
            else
            {
                ResetModule(false, "Cutting wire " + (WireIndex + 1) + " was wrong");
            }
        }

        return false;
    }

    void CheckWiresSolved()
    {
        // Check for wires that aren't cut while they should be for a solve
        bool UncutWire = false;
        int Index = 0;
        foreach (Wire wire in AllWires)
        {
            Index++;
            if (wire.MustCut && !wire.IsCut)
            {
                Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Wire {2} still must be cut", ModuleID, CurrentStage + 1, Index);
                UncutWire = true;
            }
        }

        // If there are no uncut wires, solve the mini module
        if (!UncutWire)
        {
            SolveMiniModule("Wires");
        }
    }

    // Button
    protected bool Button_Press()
    {
        ButtonSelectable.AddInteractionPunch(1);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);

        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Button is being pressed...", ModuleID, CurrentStage + 1);

        HoldCheck = StartCoroutine(CountHoldDuration());
        return false;
    }

    void Button_Release()
    {
        ButtonSelectable.AddInteractionPunch(0.5f);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
        StopCoroutine(HoldCheck);
        ColorStripObject.material.color = new Color32(25, 25, 25, 0);
        LEDStripColorblind.gameObject.SetActive(false);

        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Button released. It was {2} (desired: {3})", ModuleID, CurrentStage + 1, HoldingButton? "held":"pressed", TheButton.MustHold ? "held" : "pressed");
        
        if (HoldingButton && TheButton.MustHold) // Holding the button while it should be held
        {
            StopCoroutine(HoldAnim);

            int SecondsTimer = (int)BombInfo.GetTime() % 10;
            Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Button was released at {2} (desired: {3})", ModuleID, CurrentStage + 1, SecondsTimer, TheButton.ReleaseTime);
            if (TheButton.ReleaseTime == SecondsTimer)
            {
                SolveMiniModule("Button");
            }
            else
            {
                ResetModule(false, "Released the button at the incorrect time");
            }
        }
        else if (!HoldingButton && !TheButton.MustHold) // Pressing the button while it should be pressed
        {
            SolveMiniModule("Button");
        }
        else if (HoldingButton && !TheButton.MustHold) // Holding the button while it should be pressed
        {
            StopCoroutine(HoldAnim);
            ResetModule(false, "Held the button while it should have been pressed");
        }
        else // Pressing the button while it should be held
        {
            ResetModule(false, "Pressed the button while it should have been held");
        }
        HoldingButton = false;
        return;
    }

    void GenerateHoldRules()
    {
        // Generate the color itself
        LEDColor = (ColorEnum)Random.Range(0, 5);
        Color32 Color = AllColors[(int)LEDColor];
        int Blinking = Random.Range(0, 2);

        // Update colorblind view for LED strip
        HandleColorblindUpdate();

        // Then generate the actual release timer by using the table
        int ReleaseTimer = HoldDownTable[Blinking, (int)LEDColor];
        TheButton.ReleaseTime = ReleaseTimer;
        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Button color is {2} and is {3}, so release at {4}", ModuleID, CurrentStage + 1, LEDColor.ToString(), Blinking==1? "blinking":"lit up", ReleaseTimer);
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) [{2}, {3}]", ModuleID, CurrentStage + 1, Blinking, (int)LEDColor);

        if (Blinking == 1)
        {
            HoldAnim = StartCoroutine(HandleHoldAnimation(Color, false));
        }
        else
        {
            HoldAnim = StartCoroutine(HandleHoldAnimation(Color, false));
        }
    }

    IEnumerator CountHoldDuration()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        HoldingButton = true;
        GenerateHoldRules();
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Button is being held...", ModuleID, CurrentStage + 1);
    }

    IEnumerator HandleHoldAnimation(Color32 Color, bool AlwaysLit)
    {
        while (true)
        {
            if (AlwaysLit)
            {
                ColorStripObject.material.color = Color;
                yield return new WaitForSecondsRealtime(999f);
            }
            else
            {
                ColorStripObject.material.color = Color;
                yield return new WaitForSecondsRealtime(0.5f);
                ColorStripObject.material.color = new Color32(25, 25, 25, 0); // Gray
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }
    }

    // Morse
    protected bool MorseLight_Press()
    {
        ButtonSelectable.AddInteractionPunch(0.5f);
        if (!MessageIncoming)
        {
            Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Resignalling message...", ModuleID, CurrentStage + 1);
            MorseAnim = StartCoroutine(MorseAnimation(GeneratedMorse));
        }
        else
        {
            Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Can't resignal the message yet because the message is not completely signalled yet.", ModuleID, CurrentStage + 1);
        }
        return false;
    }

    protected bool MorseChar_Press(bool IsDash)
    {
        ButtonSelectable.AddInteractionPunch(0.5f);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (EnteredText.Length < 12)
        {
            if (EnteredMorse.Length < 5)
            {
                if (IsDash) EnteredMorse += "-";
                else EnteredMorse += ".";
            }
            MorseCodeMesh.text = EnteredMorse + "|";
        }
        return false;
    }

    protected bool MorseNext_Press()
    {
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Pressed next character button...", ModuleID, CurrentStage + 1);
        ButtonSelectable.AddInteractionPunch(0.5f);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (EnteredMorse.Length > 0)
        {
            Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Entered Morse: {2}", ModuleID, CurrentStage + 1, EnteredMorse);
            char FoundCharacter = '\0';
            foreach (KeyValuePair<char, string> Character in CharToMorse)
            {
                if (Character.Value.Replace("/", "") == EnteredMorse)
                {
                    FoundCharacter = Character.Key;
                    Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Added character '{2}' to string", ModuleID, CurrentStage + 1, FoundCharacter);

                    EnteredMorse = "";
                    EnteredText += FoundCharacter;
                    
                    MorseCodeMesh.text = EnteredMorse + "|";
                    MorseTextMesh.text = EnteredText;
                    if (EnteredText.Length < 12)
                    {
                        MorseTextMesh.text += "|";
                    }

                    break;
                }
            }
            
            if (FoundCharacter == '\0')
            {
                Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Could not find character with entered morse \"{2}\"", ModuleID, CurrentStage + 1, EnteredMorse);
            }
        }
        return false;
    }

    protected bool MorseClear_Press()
    {
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Pressed reset button. Clearing input...", ModuleID, CurrentStage + 1);
        ButtonSelectable.AddInteractionPunch(1);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        EnteredMorse = "";
        EnteredText = "";

        MorseCodeMesh.text = EnteredMorse + "|";
        MorseTextMesh.text = EnteredText + "|";

        return false;
    }

    protected bool MorseSubmit_Press()
    {
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Pressed submit button...", ModuleID, CurrentStage + 1);
        ButtonSelectable.AddInteractionPunch(1);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        CheckMorseSubmit();
        return false;
    }

    void CheckMorseSubmit()
    {
        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Received word: \"{2}\"", ModuleID, CurrentStage + 1, EnteredText);

        switch (GeneratedText) // If the submission is correct, it will reach the return, after which it will not reach all the way at the bottom after the switch block.
        {
            case PossibleMorseWords.DETONATE:
                {
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Expected: \"CANCEL\" ", ModuleID, CurrentStage + 1);
                    if (EnteredText == "CANCEL")
                    {
                        SolveMiniModule("Morse");
                        return;
                    }
                    break;
                }
            case PossibleMorseWords.STRIKE:
                {
                    string[] NumberToWord = new string[]
                    {
                        "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN"
                    };
                    int StrikeCount = BombInfo.GetStrikes();

                    if (StrikeCount > 10)
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Expected: \"TOOMANY\" ", ModuleID, CurrentStage + 1);
                        if (EnteredText == "TOOMANY")
                        {
                            SolveMiniModule("Morse");
                            return;
                        }
                    }
                    else
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Expected: \"{2}\" ", ModuleID, CurrentStage + 1, NumberToWord[StrikeCount]);
                        if (EnteredText == NumberToWord[StrikeCount])
                        {
                            SolveMiniModule("Morse");
                            return;
                        }
                    }
                    break;
                }
            case PossibleMorseWords.TIME:
                {
                    string SecondsLeft = ((int)BombInfo.GetTime() % 60).ToString();
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Expected: \"{2}\" ", ModuleID, CurrentStage + 1, SecondsLeft);
                    if (EnteredText == SecondsLeft.ToString())
                    {
                        SolveMiniModule("Morse");
                        return;
                    }
                    break;
                }
            case PossibleMorseWords.NUMBER:
                {
                    string SerialNumberString = BombInfo.GetSerialNumberNumbers().First().ToString() + BombInfo.GetSerialNumberNumbers().Last().ToString();
                    Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Expected: \"{2}\" ", ModuleID, CurrentStage + 1, SerialNumberString);
                    if (EnteredText == SerialNumberString.ToString())
                    {
                        SolveMiniModule("Morse");
                        return;
                    }
                    break;
                }
            case PossibleMorseWords.VOWEL:
                {
                    string VowelLetters = "";
                    foreach (char Letter in BombInfo.GetSerialNumberLetters())
                    {
                        if ("AEIOU".IndexOf(Letter) != -1) // -1 means IndexOf didn't find the character
                        {
                            VowelLetters += Letter;
                        }
                    }

                    if (VowelLetters.Length == 0)
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Expected: \"NONE\" ", ModuleID, CurrentStage + 1);
                        if (EnteredText == "NONE")
                        {
                            SolveMiniModule("Morse");
                            return;
                        }
                    }
                    else if (VowelLetters.Length > 0)
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Expected: \"{2}\" ", ModuleID, CurrentStage +1, VowelLetters);
                        if (EnteredText == VowelLetters)
                        {
                            SolveMiniModule("Morse");
                        }
                        return;
                    }

                    break;
                }
            default:
                {
                    bool PortFound = false;
                    foreach (string Port in BombInfo.GetPorts())
                    {
                        if (GeneratedText.ToString() == Port.ToUpper())
                        {
                            PortFound = true;
                        }
                    }

                    if (PortFound)
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Port found, so expected: \"YES\" ", ModuleID, CurrentStage+1);
                        if (EnteredText == "YES")
                        {
                            SolveMiniModule("Morse");
                            return;
                        }
                    }
                    else if (!PortFound)
                    {
                        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Port not found, so expected: \"NO\" ", ModuleID, CurrentStage+1);
                        if (EnteredText == "NO")
                        {
                            SolveMiniModule("Morse");
                            return;
                        }
                    }

                    break;
                }
        }

        // If this point is reached, then the submission was incorrect. Hand the strike here
        ResetModule(false, "Entered morse string did not match expected string");
    }

    IEnumerator MorseAnimation(string MorseText)
    {
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Morse message incoming. String: {2}", ModuleID, CurrentStage + 1, MorseText);
        MessageIncoming = true;
        yield return new WaitForSecondsRealtime(2);
        foreach (char Character in MorseText)
        {
            // Toggle light
            MorseButtonRender.material.mainTexture = Morse_Lit;
            MorseLight.gameObject.SetActive(true);
            switch (Character)
            {
                case '.':
                    {
                        yield return new WaitForSecondsRealtime(0.25f);
                        break;
                    }
                case '-':
                    {
                        yield return new WaitForSecondsRealtime(1);
                        break;
                    }
            }
            MorseButtonRender.material.mainTexture = Morse_Unlit;
            MorseLight.gameObject.SetActive(false);

            // Delay between characters
            if (Character == '/')
            {
                yield return new WaitForSecondsRealtime(1.5f);
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
        MessageIncoming = false;
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) End of message", ModuleID, CurrentStage + 1);
        yield return new WaitForSecondsRealtime(100); // Can't have the coroutine timeout because we need to end it when the module is solved.
    }

    // Simon
    IEnumerator SimonFlashingAnimation(SimonColor Color, SimonLightType Type)
    {
        while (true)
        {
            switch (Type)
            {
                case SimonLightType.Blinking:
                    {
                        for (int T = 0; T < 4; T++)
                        {
                            SimonLights[(int)Color].gameObject.SetActive(true);
                            yield return new WaitForSecondsRealtime(0.25f);
                            SimonLights[(int)Color].gameObject.SetActive(false);
                            yield return new WaitForSecondsRealtime(0.25f);
                        }
                        break;
                    }
                case SimonLightType.Flickering:
                    {
                        SimonLights[(int)Color].gameObject.SetActive(true);
                        for (int T = 0; T < 2; T++)
                        {
                            SimonLights[(int)Color].intensity = 30;
                            yield return new WaitForSecondsRealtime(0.05f);
                            SimonLights[(int)Color].intensity = 10;
                            yield return new WaitForSecondsRealtime(0.2f);
                            SimonLights[(int)Color].intensity = 30;
                            yield return new WaitForSecondsRealtime(0.05f);
                            SimonLights[(int)Color].intensity = 10;
                            yield return new WaitForSecondsRealtime(0.05f);
                            SimonLights[(int)Color].intensity = 30;
                            yield return new WaitForSecondsRealtime(0.2f);
                            SimonLights[(int)Color].intensity = 10;
                            yield return new WaitForSecondsRealtime(0.05f);
                            SimonLights[(int)Color].intensity = 30;
                            yield return new WaitForSecondsRealtime(0.05f);
                            SimonLights[(int)Color].intensity = 10;
                            yield return new WaitForSecondsRealtime(0.2f);
                            SimonLights[(int)Color].intensity = 30;
                            yield return new WaitForSecondsRealtime(0.05f);
                        }
                        SimonLights[(int)Color].gameObject.SetActive(false);
                        break;
                    }
                case SimonLightType.Lit:
                    {
                        SimonLights[(int)Color].gameObject.SetActive(true);
                        yield return new WaitForSecondsRealtime(2);
                        SimonLights[(int)Color].gameObject.SetActive(false);
                        break;
                    }
            }
            yield return new WaitForSecondsRealtime(1.75f);
        }
    }

    protected bool Simon_Press(SimonColor Color)
    {
        SimonSelectables[0].AddInteractionPunch(1);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);

        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Simon button is being pressed...", ModuleID, CurrentStage + 1);

        // Funny sound effects
        switch (Color)
        {
            case SimonColor.RED:
                BombAudio.PlaySoundAtTransform("SimonBeep1", transform);
                break;
            case SimonColor.BLUE:
                BombAudio.PlaySoundAtTransform("SimonBeep2", transform);
                break;
            case SimonColor.YELLOW:
                BombAudio.PlaySoundAtTransform("SimonBeep3", transform);
                break;
            case SimonColor.GREEN:
                BombAudio.PlaySoundAtTransform("SimonBeep4", transform);
                break;
        }

        SimonCheck = StartCoroutine(CountSimonHold());
        Simon.SimonHeldButton = Color;
        return false;
    }

    void Simon_Release(SimonColor Color)
    {
        ButtonSelectable.AddInteractionPunch(0.5f);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
        StopCoroutine(SimonCheck);

        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) {2} Simon button released...", ModuleID, CurrentStage + 1, Color);
        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) {2} the {3} button (desired: {4} {5})...", ModuleID, CurrentStage + 1, HoldingSimon? "Held":"Pressed", Color, char.IsUpper(Simon.CorrectOrder[Simon.ButtonsPressed])? "Hold":"Press", Simon.CorrectOrder[Simon.ButtonsPressed]);

        
        if (Simon.BrokenButtons.Contains(Color.ToString()[0]) && !HoldingSimon)
        {
            // Broken button was only pressed so it doesn't do anything
            Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) That button is broken, so pressing it has no effect", ModuleID, CurrentStage + 1);
            return;
        }
        else
        {
            if (Simon.CorrectOrder[Simon.ButtonsPressed] == Color.ToString().ToUpper()[0] && HoldingSimon)
            {
                Simon.ButtonsPressed++;
            }
            else if (Simon.CorrectOrder[Simon.ButtonsPressed] == Color.ToString().ToLower()[0] && !HoldingSimon)
            {
                Simon.ButtonsPressed++;
            }
            else
            {
                ResetModule(false, "Pressed the wrong color in the sequence");
            }

            // If the 4th button is pressed without strike, module solves.
            if (Simon.ButtonsPressed == 4)
            {
                SolveMiniModule("Simon");
            }
        }

        HoldingSimon = false;
    }

    IEnumerator CountSimonHold()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        SimonSelectables[0].AddInteractionPunch(0.5f);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        HoldingSimon = true;
        Debug.LogFormat("(Module Sprint #{0}): (Stage {1}) Simon button is being held...", ModuleID, CurrentStage + 1);
    }

    // Maze
    protected bool MazeArrow_Press(string Direction)
    {
        MazeSelectable[0].AddInteractionPunch(0.5f);
        string[] PossibleMoves = Maze[CoordY, CoordX].Split(',');

        if (PossibleMoves.Any(Direction.Contains)) // Standard Moves
        {
            switch (Direction)
            {
                case "U":
                    {
                        CoordY--;
                        break;
                    }
                case "D":
                    {
                        CoordY++;
                        break;
                    }
                case "L":
                    {
                        CoordX--;
                        break;
                    }
                case "R":
                    {
                        CoordX++;
                        break;
                    }
            }
        }
        else if (Maze[CoordY, CoordX].Any("*dsc".Contains) && PossibleMoves.Where(x => x.Length == 2).ToArray()[0][0].ToString() == Direction)
        {
            // Exits
            if (Maze[CoordY, CoordX].Contains("*") && GeneratedMazeShape == MazeShape.STAR)
            {
                SolveMiniModule("Maze");
                StartCoroutine(MazeSolveAnim(Direction));
            }
            else if (Maze[CoordY, CoordX].Contains("d") && GeneratedMazeShape == MazeShape.DIAMOND)
            {
                SolveMiniModule("Maze");
                StartCoroutine(MazeSolveAnim(Direction));
            }
            else if (Maze[CoordY, CoordX].Contains("s") && GeneratedMazeShape == MazeShape.SQUARE)
            {
                SolveMiniModule("Maze");
                StartCoroutine(MazeSolveAnim(Direction));
            }
            else if (Maze[CoordY, CoordX].Contains("c") && GeneratedMazeShape == MazeShape.CIRCLE)
            {
                SolveMiniModule("Maze");
                StartCoroutine(MazeSolveAnim(Direction));
            }
            else // Strike
            {
                ResetModule(false, "Tried to exit through the wrong exit");
            }
        }
        else // Strike
        {
            ResetModule(false, "Moved over a wall");
        }
        Pawn.transform.localPosition = MazeLocations[CoordY, CoordX];

        return false;
    }

    IEnumerator MazeSolveAnim(string Direction)
    {
        float Movement;
        switch (Direction)
        {
            case "U":
                {
                    Movement = 2.629f;
                    for (int T = 0; T < 20; T++)
                    {
                        Movement += 0.0866f;
                        Pawn.transform.localPosition = new Vector3(0.018f, -0.01f, Movement);
                        yield return new WaitForSecondsRealtime(0.02f);
                    }
                    Pawn.transform.localPosition = new Vector3(0.018f, -0.01f, 4.361f);
                    break;
                }
            case "D":
                {
                    Movement = -2.599f;
                    for (int T = 0; T < 20; T++)
                    {
                        Movement += -0.0866f;
                        Pawn.transform.localPosition = new Vector3(0.018f, -0.01f, Movement);
                        yield return new WaitForSecondsRealtime(0.02f);
                    }
                    Pawn.transform.localPosition = new Vector3(0.018f, -0.01f, -4.331f);
                    break;
                }
            case "L":
                {
                    Movement = -2.678f;
                    for (int T = 0; T < 20; T++)
                    {
                        Movement += -0.0852f;
                        Pawn.transform.localPosition = new Vector3(Movement, -0.01f, -0.008f);
                        yield return new WaitForSecondsRealtime(0.02f);
                    }
                    Pawn.transform.localPosition = new Vector3(-4.382f, -0.01f, -0.008f);
                    
                    break;
                }
            case "R":
                {
                    Movement = 2.708f;
                    for (int T = 0; T < 20; T++)
                    {
                        Movement += 0.0852f;
                        Pawn.transform.localPosition = new Vector3(Movement, -0.01f, -0.008f);
                        yield return new WaitForSecondsRealtime(0.02f);
                    }
                    Pawn.transform.localPosition = new Vector3(4.412f, -0.01f, -0.008f);
                    
                    break;
                }
        }
        Pawn.SetActive(false);
    }

    // Passwords
    protected bool PasswordUp_Press(int ButtonID)
    {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (PasswordCurrentCharacters[ButtonID] == 'A')
        {
            PasswordCurrentCharacters[ButtonID] = 'Z';
        }
        else
        {
            PasswordCurrentCharacters[ButtonID]--;
        }

        PassCharTextMesh[ButtonID].text = PasswordCurrentCharacters[ButtonID].ToString();

        if (PasswordCurrentCharacters[ButtonID] == PasswordStartingCharacters[ButtonID])
        {
            PassCharTextMesh[ButtonID].color = new Color32(0, 255, 0, 255);
        }
        else
        {
            PassCharTextMesh[ButtonID].color = new Color32(0, 69, 0, 255);
        }
        return false;
    }
    protected bool PasswordDown_Press(int ButtonID)
    {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (PasswordCurrentCharacters[ButtonID] == 'Z')
        {
            PasswordCurrentCharacters[ButtonID] = 'A';
        }
        else
        {
            PasswordCurrentCharacters[ButtonID]++;
        }

        PassCharTextMesh[ButtonID].text = PasswordCurrentCharacters[ButtonID].ToString();

        if (PasswordCurrentCharacters[ButtonID] == PasswordStartingCharacters[ButtonID])
        {
            PassCharTextMesh[ButtonID].color = new Color32(0, 255, 0, 255);
        }
        else
        {
            PassCharTextMesh[ButtonID].color = new Color32(0, 69, 0, 255);
        }
        return false;
    }

    protected bool PasswordSolve_Press()
    {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        for (int T = 0; T < 6; T++)
        {
            if (PasswordCurrentCharacters[T] != PasswordCorrectCharacters[T])
            {
                // Answer T is not correct
                ResetModule(false, "Letter " + T.ToString() + " is incorrect");
                return false;
            }
        }

        // If this is reached, it means there are no issues and the code is correct.
        SolveMiniModule("Password");
        return false;
    }


    // General methods
    IEnumerator Timer_Tick(int Timer, float StartingSize)
    {
        TimerRender.material.color = new Color32(0, 255, 0, 0);

        float Size = StartingSize;
        float SizeDelta = Size / Timer;

        float LerpDelta = 1 / (float)Timer;
        float Lerp = LerpDelta;

        int Minutes = Timer / 60; // One of the few times I'm happy with C truncating integers
        int Seconds = Timer % 60;

        for (int T = 0; T < Timer; T++)
        {
            // Timer
            TimerObject.transform.localScale = new Vector3(0.0254929f, 0.005373799f, Size);
            TimerRender.material.color = Color.Lerp(new Color32(0, 255, 0, 0), new Color32(255, 0, 0, 0), Lerp);
            Size -= SizeDelta;
            Lerp += LerpDelta;

            // Set countdown time
            Seconds--;
            if (Seconds == -1)
            {
                Minutes--;
                Seconds = 59;
            }

            yield return new WaitForSecondsRealtime(1);
        }
        ResetModule(false, "Timer ran out");
    }

    void HandleAnimation(string Command, GameObject ModuleToDisable, GameObject ModuleToEnable)
    {
        switch (Command)
        {
            case "Open":
                StartCoroutine(Animations.HandleShutterAnimation("Open", TopShutter, BottomShutter));
                StartCoroutine(Animations.HandlePlatformAnimation("Open", InteriorPlatform, ModuleToDisable, ModuleToEnable));
                break;
            case "Close":
                StartCoroutine(Animations.HandleShutterAnimation("Close", TopShutter, BottomShutter));
                StartCoroutine(Animations.HandlePlatformAnimation("Close", InteriorPlatform, ModuleToDisable, ModuleToEnable));
                break;
            case "Cycle":
                StartCoroutine(Animations.HandleShutterAnimation("Cycle", TopShutter, BottomShutter));
                StartCoroutine(Animations.HandlePlatformAnimation("Cycle", InteriorPlatform, ModuleToDisable, ModuleToEnable));
                break;
        }
    }

    void SolveMainModule()
    {
        ThisModule.HandlePass();
        NextButton.OnInteract = EmptyButton_Click;
        StartButton.OnInteract = EmptyButton_Click;
        HandleAnimation("Close", ModuleOrder[CurrentStage - 1], null);

        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
    }

    void SolveMiniModule(string Type)
    {
        ModulesSolveState[CurrentStage] = true;
        Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) {2} module solved! Next button enabled.", ModuleID, CurrentStage + 1, Type);
        switch (Type)
        {
            case "Wires":
                {
                    MiniSolve_Wires.material.mainTexture = SolveLight_Lit;
                    MiniSolve_Wires_Light.gameObject.SetActive(true);
                    break;
                }
            case "Button":
                {
                    MiniSolve_Button.material.mainTexture = SolveLight_Lit;
                    MiniSolve_Button_Light.gameObject.SetActive(true);
                    break;
                }
            case "Morse":
                {
                    StopCoroutine(MorseAnim);
                    MiniSolve_Morse.material.mainTexture = SolveLight_Lit;
                    MiniSolve_Morse_Light.gameObject.SetActive(true);
                    MorseButtonRender.material.mainTexture = Morse_Unlit;
                    MorseLight.gameObject.SetActive(false);
                    break;
                }
            case "Simon":
                {
                    StopCoroutine(SimonAnim);
                    foreach (Light light in SimonLights)
                    {
                        light.gameObject.SetActive(true);
                        light.intensity = 30;
                    }
                    MiniSolve_Simon.material.mainTexture = SolveLight_Lit;
                    MiniSolve_Simon_Light.gameObject.SetActive(true);
                    BombAudio.PlaySoundAtTransform("FunnyVictorySound", transform);
                    break;
                }
            case "Maze":
                {
                    MiniSolve_Maze.material.mainTexture = SolveLight_Lit;
                    MiniSolve_Maze_Light.gameObject.SetActive(true);
                    break;
                }
            case "Password":
                {
                    MiniSolve_Password.material.mainTexture = SolveLight_Lit;
                    MiniSolve_Password_Light.gameObject.SetActive(true);
                    break;
                }
        }

    }

    void ResetModule(bool Votesolve, string Reason)
    {
        if (!Votesolve)
        {
            Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Strike! {2}. Resetting...", ModuleID, CurrentStage + 1, Reason);
            ThisModule.HandleStrike();
            HandleAnimation("Close", ModuleOrder[CurrentStage], null);
            ToggleModuleSelectables(false);
        }
        else
        {
            if (ModuleStarted)
            {
                HandleAnimation("Close", ModuleOrder[CurrentStage], null);
            }
        }

        NextButton.OnInteract = EmptyButton_Click;
        

        StartCoroutine(DelayRegenerate(true));

        // Reset general
        CurrentStage = 0;

        for (int T = 0; T < 3; T++)
        {
            SolveCounters[T].material.color = Color.grey;
        }

        for (int T = 0; T < 4; T++)
        {
            ModulesSolveState[T] = false;
        }

        // Timer
        if (TimerHandler != null)
        {
            StopCoroutine(TimerHandler);
        }
        TimerRender.material.color = new Color32(100, 100, 100, 0);
        TimerObject.transform.localScale = new Vector3(0.0254929f, 0.005373799f, TimerSize);

        // Wires
        MiniSolve_Wires.material.mainTexture = SolveLight_Unlit;
        MiniSolve_Wires_Light.gameObject.SetActive(false);
        foreach (KMSelectable Wire in WireSelectables)
        {
            Wire.gameObject.SetActive(true);
        }
        foreach (GameObject Wire in AllCutWireObjects)
        {
            Wire.SetActive(false);
        }
        AllWires.Clear();

        // Button
        MiniSolve_Button.material.mainTexture = SolveLight_Unlit;
        MiniSolve_Button_Light.gameObject.SetActive(false);
        
        // Morse
        MiniSolve_Morse.material.mainTexture = SolveLight_Unlit;
        MiniSolve_Morse_Light.gameObject.SetActive(false);
        
        EnteredMorse = "";
        EnteredText = "";
        MorseCodeMesh.text = EnteredMorse + "|";
        MorseTextMesh.text = EnteredText + "|";
        GeneratedMorse = "";

        // Simon
        MiniSolve_Simon.material.mainTexture = SolveLight_Unlit;
        MiniSolve_Simon_Light.gameObject.SetActive(false);
        Simon = new SimonClass();
        try
        {
            StopCoroutine(SimonAnim);
        }
        catch (NullReferenceException)
        {
        }
        foreach (Light light in SimonLights)
        {
            light.gameObject.SetActive(false);
            light.intensity = 30;
        }

        // Maze
        MiniSolve_Maze.material.mainTexture = SolveLight_Unlit;
        MiniSolve_Maze_Light.gameObject.SetActive(false);
        Pawn.SetActive(true);

        // Password
        MiniSolve_Password.material.mainTexture = SolveLight_Unlit;
        MiniSolve_Password_Light.gameObject.SetActive(false);

        // Stop module
        ModuleStarted = false;
    }

    IEnumerator DelayRegenerate(bool Reset)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        GenerateModules();
        if (Reset)
        {
            StartButton.OnInteract = StartButton_Press;
        }
        yield return null;
    }

    protected bool StartButton_Press()
    {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        StartButton.AddInteractionPunch(1);
        HandleStage(0);
        return false;
    }

    protected bool NextButton_Click()
    {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        StartButton.AddInteractionPunch(1);
        if (ModulesSolveState[CurrentStage])
        {
            BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
            SolveCounters[CurrentStage].material.color = Color.green;
            CurrentStage++;
            Debug.LogFormat("[Module Sprint #{0}]: Moving on to stage {1}", ModuleID, CurrentStage + 1);
            HandleStage(CurrentStage);
        }
        else
        {
            Debug.LogFormat("[Module Sprint #{0}]: (Stage {1}) Module not disarmed yet! Can't move on to next module!", ModuleID, CurrentStage + 1);
        }
        return false;
    }

    protected bool EmptyButton_Click()
    {
        return false;
    }

    protected void EmptyButton_Release()
    {
        return;
    }

    // Colorblind
    private bool ColorblindEnabled = false;
    protected bool ColorblindButton_Click()
    {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        ColorblindEnabled = !ColorblindEnabled;
        ColorblindButton.gameObject.GetComponent<MeshRenderer>().material.mainTexture = ColorblindEnabled ? ColorblindTextures[1] : ColorblindTextures[0];
        if (ModuleStarted)
        {
            HandleColorblindUpdate();
        }
        return false;
    }

    void HandleColorblindUpdate()
    {
        switch (ModuleOrder[CurrentStage].name)
        {
            case "WiresModule":
                {
                    for (int T = 0; T < AllWires.Count; T++)
                    {
                        if (ColorblindEnabled)
                        {
                            if (AllWires[T].Color == ColorEnum.BLACK)
                            {
                                AllTopLetters[T].text = "X";
                                continue;
                            }
                            AllTopLetters[T].text = AllWires[T].Color.ToString()[0].ToString();
                        }
                        else
                        {
                            AllTopLetters[T].text = "123456"[T].ToString();
                        }
                    }
                    break;
                }
            case "ButtonModule":
                {
                    // Colorblind for the LED strip
                    if (ColorblindEnabled)
                    {
                        ButtonText.text = TheButton.Color.ToString() + "\n" + TheButton.Text;
                        if (TheButton.Color == ColorEnum.YELLOW || TheButton.Color == ColorEnum.WHITE || TheButton.Color == ColorEnum.GREEN)
                        {
                            ButtonText.color = Color.black;
                        }
                        else
                        {
                            ButtonText.color = Color.white;
                        }
                        if (HoldingButton)
                        {
                            LEDStripColorblind.gameObject.SetActive(true);
                            LEDStripColorblind.text = LEDColor.ToString()[0].ToString();
                            if (LEDColor == ColorEnum.YELLOW || LEDColor == ColorEnum.WHITE || LEDColor == ColorEnum.GREEN)
                            {
                                LEDStripColorblind.color = Color.black;
                            }
                            else
                            {
                                LEDStripColorblind.color = Color.white;
                            }
                        }
                    }
                    else
                    {
                        ButtonText.text = TheButton.Text;
                        if (HoldingButton)
                        {
                            LEDStripColorblind.gameObject.SetActive(false);
                        }
                    }
                    break;
                }
            case "SimonModule":
                {
                    SimonColorblinds.SetActive(ColorblindEnabled);
                    break;
                }
        }
    }

    // Twitch Plays
    private readonly string TwitchHelpMessage = "!{0} [Module] [Command] [Arguments]. Modules are: wires, button, morse, simon, maze, password. Type !{0} start to start the module, and !{0} next to go to the next module once the current one is solved. For wires: !{0} wires cut [123456] to cut wires 1 to 6. For button: !{0} button [press] to press and immediately release the button, [hold] to hold down the button, and [release] [9] to release at 9 seconds. For morse: !{0} morse [enter] [string] to enter the text \"string\" (Not case-sensitive), [clear] to remove the text, [flash] to reflash the morse message, and [submit] to submit the message. Alternatively, you can use [submitat] [XX] to submit when the timer reaches XX seconds. For simon: !{0} simon [press] [rbyg] to press, in order, red blue yellow green (Lower case means press, upper case means hold). For maze: !{0} maze [move] [udlr] to move, in order, up down left right (Can chain these to make a series of moves). For password: !{0} [enter] [ABCDEF] to enter \"ABCDEF\" (not case-sensetive).";
    private int TPBombTimer;

    IEnumerator ProcessTwitchCommand(string Command)
    {
        string[] CommandArgs = Command.Trim().Split(' ');
        // Make first 2 args lower invariant, but not the rest (important for simon)
        if (CommandArgs.Length > 0)
        {
            CommandArgs[0] = CommandArgs[0].ToLowerInvariant();
        }
        if (CommandArgs.Length > 1)
        {
            CommandArgs[1] = CommandArgs[1].ToLowerInvariant();
        }

        if (CommandArgs[0] == "start")
        {
            yield return null;
            StartButton.OnInteract();
            yield break;
        }
        else if (!ModuleStarted)
        {
            yield return "sendtochaterror The module hasn't been started yet.";
        }
        else
        {
            switch (CommandArgs[0])
            {
                // Wires
                case "wires":
                    {
                        if (ModuleOrder[CurrentStage].name == "WiresModule")
                        {
                            if (CommandArgs.Length > 2 && CommandArgs[1] == "cut")
                            {
                                List<KMSelectable> WiresToCut = new List<KMSelectable>();
                                foreach (char wireToAdd in CommandArgs[2])
                                {
                                    if (Regex.IsMatch(wireToAdd.ToString(), @"[1-6]"))
                                    {
                                        int WireID = int.Parse(wireToAdd.ToString());
                                        WiresToCut.Add(WireSelectables[WireID - 1]);
                                    }
                                    else
                                    {
                                        yield return "sendtochaterror \"" + wireToAdd + "\" is not a valid wire. Please only include numbers 1 to 6";
                                        WiresToCut.Clear();
                                        break;
                                    }
                                }

                                foreach (KMSelectable wireToCut in WiresToCut)
                                {
                                    yield return null;
                                    wireToCut.OnInteract();
                                }
                            }
                            else if (CommandArgs.Length > 2)
                            {
                                yield return "sendtochaterror Wires does not recognize the command \"" + CommandArgs[1] + "\".";
                            }
                            else
                            {
                                yield return "sendtochaterror No further commands given for Wires.";
                            }
                        }
                        else
                        {
                            yield return "sendtochaterror The active module is not Wires.";
                        }
                        break;
                    }
                // Button
                case "button":
                    {
                        if (ModuleOrder[CurrentStage].name == "ButtonModule")
                        {
                            if (CommandArgs.Length > 1)
                            {
                                switch (CommandArgs[1])
                                {
                                    case "press":
                                    case "tap":
                                        {
                                            ButtonSelectable.OnInteract();
                                            ButtonSelectable.OnInteractEnded();
                                            break;
                                        }
                                    case "hold":
                                        {
                                            if (!HoldingButton)
                                            {
                                                ButtonSelectable.OnInteract();
                                            }
                                            else
                                            {
                                                yield return "sendtochaterror The button is already being held.";
                                            }
                                            break;
                                        }
                                    case "release":
                                        {
                                            if (HoldingButton && CommandArgs.Length > 2)
                                            {
                                                int ReleaseTime;
                                                bool ReleaseParsed = int.TryParse(CommandArgs[2], out ReleaseTime);

                                                if (ReleaseParsed && ReleaseTime < 10 && ReleaseTime >= 0)
                                                {
                                                    while ((int)BombInfo.GetTime() % 10 != ReleaseTime)
                                                    {
                                                        yield return "trycancel Button release was cancelled.";
                                                    }
                                                    ButtonSelectable.OnInteractEnded();
                                                }
                                                else if (!ReleaseParsed)
                                                {
                                                    yield return "sendtochaterror \"" + CommandArgs[2] + "\" is not a valid release time. Please enter a value between 0 and 9.";
                                                }
                                                else if (ReleaseTime < 0 || ReleaseTime >= 10)
                                                {
                                                    yield return "sendtochaterror I can't release at " + ReleaseTime + ". Please enter a value between 0 and 9.";
                                                }
                                            }
                                            else if (!HoldingButton)
                                            {
                                                yield return "sendtochaterror The button is not being held.";
                                            }
                                            else if (CommandArgs.Length <= 2)
                                            {
                                                yield return "sendtochaterror No release time given. Please enter a value between 0 and 9.";
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            yield return "sendtochaterror Button does not recognize the command \"" + CommandArgs[1] + "\".";
                                            break;
                                        }
                                }

                            }
                            else
                            {
                                yield return "sendtochaterror No further commands given for Button.";
                            }
                        }
                        else
                        {
                            yield return "sendtochaterror The active module is not Button.";
                        }
                        break;
                    }
                // Morse
                case "morse":
                    {
                        if (ModuleOrder[CurrentStage].name == "MorseModule")
                        {
                            if (CommandArgs.Length > 1)
                            {
                                switch (CommandArgs[1])
                                {
                                    case "flash":
                                        {
                                            if (!MessageIncoming)
                                            {
                                                yield return null;
                                                MorseLightSelectable.OnInteract();
                                            }
                                            else
                                            {
                                                yield return "sendtochaterror The message is still being received.";
                                            }
                                            break;
                                        }
                                    case "enter":
                                        {
                                            if (CommandArgs.Length > 2)
                                            {
                                                string MorseActions = ""; // Just a general string for inputs. '/' means the next button
                                                foreach (char Character in CommandArgs[2])
                                                {
                                                    char CharacterUpper = char.ToUpper(Character);
                                                    if (CharToMorse.ContainsKey(CharacterUpper))
                                                    {
                                                        MorseActions += CharToMorse[CharacterUpper];
                                                        MorseActions += "/";
                                                    }
                                                    else
                                                    {
                                                        MorseActions = "";
                                                        yield return "sendtochaterror The character \"" + Character + "\" is not recognized.";
                                                        break;
                                                    }
                                                }
                                                Debug.LogWarning(MorseActions);
                                                foreach (char Action in MorseActions)
                                                {
                                                    switch (Action)
                                                    {
                                                        case '.':
                                                            {
                                                                yield return null;
                                                                MorseDotSelectable.OnInteract();
                                                                break;
                                                            }
                                                        case '-':
                                                            {
                                                                yield return null;
                                                                MorseDashSelectable.OnInteract();
                                                                break;
                                                            }
                                                        case '/':
                                                            {
                                                                yield return null;
                                                                MorseNextSelectable.OnInteract();
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    case "clear":
                                        {
                                            yield return null;
                                            MorseClearSelectable.OnInteract();
                                            break;
                                        }
                                    case "submit":
                                        {
                                            yield return null;
                                            MorseSubmitSelectable.OnInteract();
                                            break;
                                        }
                                    case "submitat":
                                        {
                                            int SubmitTime;
                                            if (CommandArgs.Length == 3 && int.TryParse(CommandArgs[2], out SubmitTime))
                                            {
                                                if (SubmitTime < 60 && SubmitTime >= 0)
                                                {
                                                    Coroutine GetTime = StartCoroutine(TwitchPlaysGetTime());
                                                    while (SubmitTime != TPBombTimer)
                                                    {
                                                        yield return "trycancel";
                                                    }
                                                    MorseSubmitSelectable.OnInteract();
                                                    StopCoroutine(GetTime);
                                                }
                                                else
                                                {
                                                    yield return "sendtochaterror The release time should be somewhere between 0 and 59. " +  SubmitTime + " is not valid.";
                                                }
                                            }
                                            else
                                            {
                                                yield return "sendtochaterror No valid release time given";
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            yield return "sendtochaterror Morse does not recognize the command \"" + CommandArgs[1] + "\".";
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                yield return "sendtochaterror No further commands given for Morse.";
                            }
                        }
                        else
                        {
                            yield return "sendtochaterror The active module is not Morse.";
                        }
                        break;
                    }
                // Simon
                case "simon":
                    {
                        if (ModuleOrder[CurrentStage].name == "SimonModule")
                        {
                            if (CommandArgs.Length > 1 && CommandArgs[1] == "press")
                            {
                                if (CommandArgs.Length > 2)
                                {
                                    Dictionary<KMSelectable, bool> Inputs = new Dictionary<KMSelectable, bool>(); // Bool for holding
                                    foreach (char Input in CommandArgs[2])
                                    {
                                        switch (Input)
                                        {
                                            case 'r':
                                                {
                                                    Inputs.Add(SimonSelectables[0], false);
                                                    break;
                                                }
                                            case 'b':
                                                {
                                                    Inputs.Add(SimonSelectables[1], false);
                                                    break;
                                                }
                                            case 'y':
                                                {
                                                    Inputs.Add(SimonSelectables[2], false);
                                                    break;
                                                }
                                            case 'g':
                                                {
                                                    Inputs.Add(SimonSelectables[3], false);
                                                    break;
                                                }
                                            case 'R':
                                                {
                                                    Inputs.Add(SimonSelectables[0], true);
                                                    break;
                                                }
                                            case 'B':
                                                {
                                                    Inputs.Add(SimonSelectables[1], true);
                                                    break;
                                                }
                                            case 'Y':
                                                {
                                                    Inputs.Add(SimonSelectables[2], true);
                                                    break;
                                                }
                                            case 'G':
                                                {
                                                    Inputs.Add(SimonSelectables[3], true);
                                                    break;
                                                }
                                            default:
                                                {
                                                    yield return "sendtochaterror \"" + Input + "\" is not a valid color. Valid colors are (R)ed, (B)lue, (Y)ellow or (G)reen";
                                                    Inputs.Clear();
                                                    break;
                                                }
                                        }
                                    }

                                    foreach (KeyValuePair<KMSelectable, bool> Action in Inputs)
                                    {
                                        yield return null;
                                        if (Action.Value) // Hold
                                        {
                                            Action.Key.OnInteract();
                                            yield return new WaitForSeconds(1);
                                            Action.Key.OnInteractEnded();
                                        }
                                        else // Press
                                        {
                                            Action.Key.OnInteract();
                                            Action.Key.OnInteractEnded();
                                        }
                                        
                                    }
                                }
                            }
                            else if (CommandArgs.Length > 1)
                            {
                                yield return "sendtochaterror Simon does not recognize the command \"" + CommandArgs[1] + "\".";
                            }
                            else
                            {
                                yield return "sendtochaterror No further commands given for Simon.";
                            }
                        }
                        else
                        {
                            yield return "sendtochaterror The active module is not Simon.";
                        }
                        break;
                    }
                case "maze":
                    {
                        if (ModuleOrder[CurrentStage].name == "MazeModule")
                        {
                            if (CommandArgs.Length > 1 && CommandArgs[1] == "move")
                            {
                                if (CommandArgs.Length > 2)
                                {
                                    List<KMSelectable> Movements = new List<KMSelectable>();
                                    foreach (char move in CommandArgs[2].ToLowerInvariant())
                                    {
                                        if (move == 'u')
                                        {
                                            Movements.Add(MazeSelectable[0]);
                                        }
                                        else if (move == 'l')
                                        {
                                            Movements.Add(MazeSelectable[1]);
                                        }
                                        else if (move == 'r')
                                        {
                                            Movements.Add(MazeSelectable[2]);
                                        }
                                        else if (move == 'd')
                                        {
                                            Movements.Add(MazeSelectable[3]);
                                        }
                                        else
                                        {
                                            yield return "sendtochaterror \"" + move + "\" is not a valid move. Valid movements are (U)p, (L)eft, (R)ight or (D)own";
                                            Movements.Clear();
                                            break;
                                        }
                                    }

                                    foreach (KMSelectable Action in Movements)
                                    {
                                        yield return null;
                                        Action.OnInteract();
                                    }
                                }
                                else
                                {
                                    yield return "sendtochaterror No movements given for Maze.";
                                }
                            }
                            else if (CommandArgs.Length > 1)
                            {
                                yield return "sendtochaterror Maze does not recognize the command \"" + CommandArgs[1] + "\".";
                            }
                            else
                            {
                                yield return "sendtochaterror No further commands given for Maze.";
                            }
                        }
                        else
                        {
                            yield return "sendtochaterror The active module is not Maze.";
                        }
                        break;
                    }
                case "password":
                    {
                        if (ModuleOrder[CurrentStage].name == "PasswordModule")
                        {
                            if (CommandArgs.Length > 1)
                            {
                                if (CommandArgs[1] == "enter")
                                {
                                    if (CommandArgs.Length > 2 && CommandArgs[2].Length == 6 && CommandArgs[2].All(char.IsLetter))
                                    {
                                        string Line = CommandArgs[2].ToUpper();
                                        for (int T = 0; T < 6; T++)
                                        {
                                            while (PasswordCurrentCharacters[T] != Line[T])
                                            {
                                                yield return null;
                                                PasswordDownSelectables[T].OnInteract();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        yield return "sendtochaterror Please enter a valid 6-letter string.";
                                    }
                                }
                                else if (CommandArgs[1] == "submit")
                                {
                                    yield return null;
                                    PasswordSubmitSelectable.OnInteract();
                                }
                                else if (CommandArgs.Length > 1)
                                {
                                    yield return "sendtochaterror Password does not recognize the command \"" + CommandArgs[1] + "\".";
                                }
                                else
                                {
                                    yield return "sendtochaterror No further commands given for Password.";
                                }
                            }
                            else
                            {
                                yield return "sendtochaterror No further valid commands given for Password.";
                            }
                        }
                        else
                        {
                            yield return "sendtochaterror The active module is not Password.";
                        }
                        break;
                    }
                case "cancel":
                    {
                        // Suppress cancel command
                        break;
                    }
                case "next":
                    {
                        yield return null;
                        NextButton.OnInteract();
                        break;
                    }
                default:
                    {
                        yield return "sendtochaterror I don't recognize the module \"" + CommandArgs[0] + "\".";
                        break;
                    }
            }
        }
        yield return null;
    }

    IEnumerator TwitchPlaysGetTime()
    {
        while (true)
        {
            TPBombTimer = ((int)BombInfo.GetTime()) % 60;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat("[Module Sprint #{0}]: You dirty cheater...", ModuleID);
        BombAudio.PlaySoundAtTransform("AutosolveBuzzer", transform);
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        ResetModule(true, null);
        StartCoroutine(Animations.HandleForceSolveAnimation(ForceTopShutter, ForceBottomShutter));
        ThisModule.HandlePass();
        yield return null;
    }
}
