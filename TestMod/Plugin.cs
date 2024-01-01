using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;

namespace TestMod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("LethalNetworkAPI")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger = base.Logger;
        
        // Plugin startup logic
        Logger.LogDebug($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        Instance = this;

        Harmony.CreateAndPatchAll(typeof(Test));

        Message = new LethalNetworkMessage<int>("customMessage");

        Message.OnClientReceived += Receive;

        CustomIntVariable.Value = 7;
        
        Logger.LogDebug(new ulong());

        Test.Init();
    }

    private static void Receive(int data)
    {
        Logger.LogInfo($"Player position: {data}");

        CustomIntVariable.Value = CustomIntVariable.Value switch
        {
            > 5 => 5,
            < -5 => -5,
            _ => CustomIntVariable.Value
        };
    } 

    public static Plugin Instance;
    public new static ManualLogSource Logger;
    
    [LethalNetworkProtected]
    public static LethalNetworkMessage<int> Message;

    private static readonly LethalNetworkVariable<int> CustomIntVariable = new("customGuid");
}

[HarmonyPatch]
public class Test
{
    [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
    private static void TestPrint()
    {
        Plugin.Logger.LogDebug(NetworkManager.Singleton.ConnectedClientsIds.Join());
        
        if (!NetworkManager.Singleton.IsHost) return;
            
        Plugin.Message.SendAllClients(8, true);

        vbool.Variable.Value = true;
        vchar.Variable.Value = 't';
        vchar.Variable.Value = 'x';
        vsbyte.Variable.Value = 5;
        vbyte.Variable.Value = 5;
        vushort.Variable.Value = 5;
        vfloat.Variable.Value = .5f;
        vstring.Variable.Value = "test";
        vColor.Variable.Value = Color.green;
        vVector3.Variable.Value = Vector3.one;
    }

    public static void Init()
    {
        vbool = new CustomNetworkVariable<bool>();
        vchar = new CustomNetworkVariable<char>();
        vsbyte = new CustomNetworkVariable<sbyte>();
        vbyte = new CustomNetworkVariable<byte>();
        vshort = new CustomNetworkVariable<short>();
        vushort = new CustomNetworkVariable<ushort>();
        vint = new CustomNetworkVariable<int>();
        vuint = new CustomNetworkVariable<uint>();
        vlong = new CustomNetworkVariable<long>();
        vulong = new CustomNetworkVariable<ulong>();
        vfloat = new CustomNetworkVariable<float>();
        vdouble = new CustomNetworkVariable<double>();
        vstring = new CustomNetworkVariable<string>();
        vColor = new CustomNetworkVariable<Color>();
        vColor32 = new CustomNetworkVariable<Color32>();
        vVector2 = new CustomNetworkVariable<Vector2>();
        vVector3 = new CustomNetworkVariable<Vector3>();
        vVector4 = new CustomNetworkVariable<Vector4>();
        vQuaternion = new CustomNetworkVariable<Quaternion>();
        vRay = new CustomNetworkVariable<Ray>();
        vRay2D = new CustomNetworkVariable<Ray2D>();
    }

    static CustomNetworkVariable<bool> vbool;
    static CustomNetworkVariable<char> vchar;
    static CustomNetworkVariable<sbyte> vsbyte;
    static CustomNetworkVariable<byte> vbyte;
    static CustomNetworkVariable<short> vshort;
    static CustomNetworkVariable<ushort> vushort;
    static CustomNetworkVariable<int> vint;
    static CustomNetworkVariable<uint> vuint;
    static CustomNetworkVariable<long> vlong;
    static CustomNetworkVariable<ulong> vulong;
    static CustomNetworkVariable<float> vfloat;
    static CustomNetworkVariable<double> vdouble;
    static CustomNetworkVariable<string> vstring;
    static CustomNetworkVariable<Color> vColor;
    static CustomNetworkVariable<Color32> vColor32;
    static CustomNetworkVariable<Vector2> vVector2;
    static CustomNetworkVariable<Vector3> vVector3;
    static CustomNetworkVariable<Vector4> vVector4;
    static CustomNetworkVariable<Quaternion> vQuaternion;
    static CustomNetworkVariable<Ray> vRay;
    static CustomNetworkVariable<Ray2D> vRay2D;
}

public class CustomNetworkVariable<T>
{
    public CustomNetworkVariable()
    {
        Variable = new LethalNetworkVariable<T>("custom" + typeof(T).FullName);

        Variable.OnValueChanged += LogOnChanged;
    }

    private static void LogOnChanged(T data)
    {
        Plugin.Logger.LogDebug(data);
    }

    [LethalNetworkProtected]
    public LethalNetworkVariable<T> Variable { get; }
}
