using Kingmaker;
using Kingmaker.BundlesLoading;
//using Kingmaker.Visual.Animation.Kingmaker;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace MyOwlcatModification
{    
        class EquipmentImporter : EditorWindow
        {
            static List<(string NameInBundle, string ObjectName)> EquipmentItems = null;
            static List<(string NameInBundle, string ObjectName)> FilteredEquipmentItems = null;

            public Vector2 scroller;
            public bool fold = true;
            public IObservable<ByRace.RacePostfix> raceFilter;
            public IObservable<ByBodyType.BodyPartsPrefixes> partFilter;

        [MenuItem("Importers/Import Equipment")]
        public static void MenuButton()
        {
            if (!ModAssetImporter.Launched) return;
            if (FilteredEquipmentItems == null)
                SetUp();
            if (EquipmentItems == null)
            {
                Debug.LogError("Failed to create item names list for Importing Equipment button");
                return;
            }
            var wnd = GetWindow<EquipmentImporter>();
            wnd.titleContent = new GUIContent("Equipment importer");
        }

        public class ByBodyType : IComparer<string>
            {
                public static ByBodyType instance = new ByBodyType();
                static string X;
                static string Y;

                public int Compare(string x, string y)
                {
                    long xxVal;
                    long yyVal;

                    if (x.Length < 3 || x[2] != '_')
                        X = "";
                    else X = x.Substring(0, 2);
                    if (y.Length < 3 || y[2] != '_')
                        Y = "";
                    else Y = y.Substring(0, 2);

                    if (Enum.TryParse<BodyPartsPrefixes>(X, out var xVal))
                        xxVal = (long)xVal;
                    else xxVal = 2199023255552+1;
                    if (Enum.TryParse<BodyPartsPrefixes>(Y, out var yVal))
                        yyVal = (long)yVal;
                    else yyVal = 2199023255552 + 1;
                    var result = xxVal.CompareTo(yyVal);
                    return result;
                }

                [Flags]
                public enum BodyPartsPrefixes : long
                {
                    BT = 1L,
                    BB = 2L,
                    BW = 4L,
                    CB = 8L,
                    CF = 16L,
                    CT = 32L,
                    EY = 64L,
                    EC = 128L,
                    FT = 256L,
                    FA = 512L,
                    HN = 1024L,
                    HD = 2048L,
                    HH = 4096L,
                    HR = 8192L,
                    KC = 16384L,
                    LS = 32768L,
                    LL = 65536L,
                    H2 = 131072L,
                    GG = 262144L,
                    RL = 524288L,
                    RR = 1048576L,
                    SK = 2097152L,
                    SP = 4194304L,
                    TL = 8388608L,
                    TB = 16777216L,
                    TS = 33554432L,
                    UA = 67108864L,
                    UL = 134217728L,
                    HT = 268435456L,
                    HB = 536870912L,
                    EA = 1073741824L,
                    NK = 2147483648L,
                    TH = 4294967296L,
                    MA = 8589934592L,
                    DA = 17179869184L,
                    HA = 34359738368L,
                    N1 = 68719476736L,
                    N2 = 137438953472L,
                    NO = 274877906944L,
                    CP = 549755813888L,
                    BP = 1099511627776L,
                    WG = 2199023255552L
                }
            }
            public class ByRace : IComparer<string>
            {
                public static ByRace instance = new ByRace();
                static string X;
                static string Y;

                public int Compare(string x, string y)
                {
                    long xxVal;
                    long yyVal;

                    int xDash = -2;
                    int yDash = -2;

                    try
                    {
                        xDash = x.LastIndexOf('_') +1;
                        yDash = y.LastIndexOf('_') +1;
                        if (xDash < 0)
                            X = "";
                        else X = x.Substring(xDash, x.Length  - xDash);
                        if (yDash < 0)
                            Y = "";
                        else Y = y.Substring(yDash, y.Length - yDash);

                        if (Enum.TryParse<RacePostfix>(X, out var xVal))
                            xxVal = (long)xVal;
                        else xxVal = 50 + 1;
                        if (Enum.TryParse<RacePostfix>(Y, out var yVal))
                            yyVal = (long)yVal;
                        else yyVal = 50 + 1;
                        var result = xxVal.CompareTo(yyVal);
                        return result;
                    }
                    catch (Exception)
                    {
                        Debug.Log($"Failed on strings {x} and {y}. Indices are {xDash} and {yDash}");
                        return -1;
                    }
                }

                [Flags]
                public enum RacePostfix : long
                {
                    Any =  0b1111111111111111111111111111111,
                    HM = 0b1,//Human
                    EL = 0b10, //Elf
                    DW = 0b100, //Dwarf
                    HL = 0b1000, //Halfling
                    HO = 0b10000, //Half-Orc
                    GN = 0b100000, //Gnome
                    HE = 0b1000000, //Half-Elf
                    Goblin = 0b10000000,
                    Aasimar = 0b100000000,
                    TF = 0b1000000000, //Tiefling
                    Oread = 0b10000000000,
                    Dhampir = 0b100000000000,
                    KT = 0b1000000000000, //Kitsune
                    Catfolk = 0b10000000000000,
                    MM = 0b100000000000000, // MongrelMan
                    RIG = 0b10000000000000000000000000000000
                }
            }

            public static void SetUp()
            {
                EquipmentItems = new List<(string NameInBUndle, string ObjectName)>();
                var bundle = BundlesLoadService.Instance.RequestBundle("equipment");
                var names = bundle.GetAllAssetNames();
                foreach (var name in names)
                {
                    var go = bundle.LoadAsset<GameObject>(name);
                    if (go != null)
                        EquipmentItems.Add((name, go.name));
                }
                EquipmentItems = EquipmentItems
                    .OrderBy(pair => pair.ObjectName, ByRace.instance)
                    .OrderBy(pair => pair.ObjectName, ByBodyType.instance)
                    .ToList();
                FilteredEquipmentItems = EquipmentItems;
                bundle.Unload(true);
            }


            void OnGUI()
            {
                fold = Foldout(fold, "Filter", true);
                if (!fold) goto DontDrawFilter;


                ;DontDrawFilter:
                scroller = BeginScrollView(scroller, GUILayout.Width(600), GUILayout.Height(1500));
                //BeginVertical();
                foreach(var names in FilteredEquipmentItems)
                {
                    BeginHorizontal();
                    GUILayout.Label(names.ObjectName);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Load"))
                    ModAssetImporter.DoImportEquipmentEntity(names.NameInBundle);
                    EndHorizontal();
                }
                //EndVertical();
                EndScrollView();

            }
        }
}
