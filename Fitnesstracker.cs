using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.Rendering.Universal;

public class FitnessTracker
{

    private static GameObject Canvas;
    private static int num_wanted;
    private static int per_generation;

    public static void Initialize(GameObject C, int pg, int nw = 5)
    {
        Canvas = C;
        num_wanted = nw;
        per_generation = pg;
    }
    public static void DisplayTop10(Dictionary<int, float?>[] Fitness, GameObject[] entities)
    {
        // Create a list to hold (ID, Fitness) pairs
        List<(int ID, float fitness)> allFitness = new List<(int, float)>();

        for (int generation = 0; generation < Fitness.Length; generation++)
        {
            if (Fitness[generation] is null) continue;

            foreach (var kvp in Fitness[generation])
            {

                int ID = generation * per_generation + kvp.Key;

                if (entities[ID] is null) continue;

                //Debug.Log(ID + "...");
                Entity e = entities[ID].GetComponent<PlayerController>().Entity;

                if (e.f_values.Count <= 0) continue;

                allFitness.Add((ID, e.f_values[e.f_values.Count-1]));
            }
        }

        // Sort by fitness descending and take top 10
        var top10 = allFitness
            .OrderByDescending(pair => pair.fitness)
            .Take(num_wanted)
            .ToList();

        for ( int i = 1; i <= top10.Count; i++)
        {
            var entry = top10[i-1];
            Canvas.transform.Find(i.ToString()).GetComponent<TextMeshProUGUI>().text = "ID: " + entry.ID + ", Fitness: " + entry.fitness;
        }
    }
}
#if false
public static void DisplayTop10(GameObject[] entities)
    {
        // Create a list to hold (ID, Fitness) pairs
        List<(int ID, float fitness)> allFitness = new List<(int, float)>();

        for (int ID = 0; ID < entities.Length; ID++)
        {
            if (!entities[ID]) continue;
            Entity e = go.GetComponent<PlayerController>().Entity;

            foreach (var kvp in Fitness[generation])
            {
                int fitnessIndex = kvp.Key;
                float fitnessValue = kvp.Value;

                int ID = generation * per_generation + fitnessIndex;

                allFitness.Add((ID, fitnessValue));
            }
        }

        // Sort by fitness descending and take top 10
        var top10 = allFitness
            .OrderByDescending(pair => pair.fitness)
            .Take(num_wanted)
            .ToList();

        for (int i = 1; i <= top10.Count; i++)
        {
            var entry = top10[i];
            Canvas.transform.Find(i.ToString()).GetComponent<TextMeshProUGUI>().text = "ID: " + entry.ID + ", Fitness: " + entry.fitness;
        }
    }
#endif