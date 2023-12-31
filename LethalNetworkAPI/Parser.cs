using System;
using System.Globalization;
using System.Linq;
using LethalNetworkAPI.Networking;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace LethalNetworkAPI;

internal static class Parser
{
    internal static void SendServerMessageOfValidType(string guid, object data)
    {
        switch (data)
        {
            case bool[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case char[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case sbyte[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case byte[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case short[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case ushort[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case int[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case uint[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case long[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case ulong[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case float[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case double[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case string[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Color[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Color32[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Vector2[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Vector3[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Vector4[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Quaternion[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Ray[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            case Ray2D[] x:
                NetworkHandler.Instance.MessageServerRpc(guid, x);
                break;
            default:
                Plugin.Logger.LogError($"Unable to send data of type {data.GetType()}");
                return;
        }
    }
    
    internal static void SendClientMessageOfValidType(string guid, object data, ClientRpcParams clientRpcParams = default)
    {
        switch (data)
        {
            case bool[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case char[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case sbyte[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case byte[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case short[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case ushort[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case int[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case uint[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case long[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case ulong[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case float[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case double[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case string[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Color[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Color32[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Vector2[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Vector3[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Vector4[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Quaternion[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Ray[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            case Ray2D[] x:
                NetworkHandler.Instance.MessageClientRpc(guid, x, clientRpcParams);
                break;
            default:
                Plugin.Logger.LogError($"Unable to send data of type {data.GetType()}");
                return;
        }
    }
}