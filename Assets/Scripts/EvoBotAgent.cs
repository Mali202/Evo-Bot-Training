using Model;
using Model.Bots;
using Model.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class EvoBotAgent : MonoBehaviour, IListener
{
    EvoBot bot;
    private void Awake()
    {
    }

    public void Broadcast(Trigger trigger)
    {
        Debug.Log("started");
    }

    public EvoBot InitializeBot(bool isWolf, string Name, int playerNumber)
    {
        bot = new EvoBot(isWolf, Name, playerNumber);
        return bot;
    }
}
