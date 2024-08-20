using Model;
using Model.Bots;
using Model.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class EvoBotAgent : Agent, IListener
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
        Debug.Log("Evo Bot initialized");
        return bot;
    }

    //Agent override methods
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(new float[] { bot.BrickCount, bot.StrawCount, bot.WoodCount});
        sensor.AddObservation(bot.Hand.Cards.Count);
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        switch (continuousActions[0])
        {
            //place from hand
            case 0:
                break;

            //draw
            case 1:
                break;

            //discard
            case 2:
                break;
        }
    }
}
