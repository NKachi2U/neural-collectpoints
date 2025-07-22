using System;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEditor.Rendering.CameraUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using Input = UnityEngine.Input;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;
using JetBrains.Annotations;
using TMPro;
using static UnityEngine.Rendering.DebugUI;



public class GameManager : MonoBehaviour
{

    private Vector3 e_spawn;
    private GameObject aiObject;
    private GameObject gen_text;

    private Transform background;
    private Transform ai_background;
    private Transform last_click;

    public int per_generation;
    public int n_generations; // Number of generations I want to generate
    public int survivors; // Number of survivor per round
    public int eval_time;
    
    NeuralNetwork[] neuralNetworks; // An array of neural networks
    GameObject[] entities;

    // layerstructure[0] = number of inputs, layer_structure[n-1] = output layer, anything between are hidden layers
    int[][] layer_structures = { 
        new int[]{ 1, 2 },
        //new int[]{4, 2, 2, 2},
    };

    const float RemovedFitness = 10;
    Dictionary<int, float>[] fitness; // Fitness[Generation][Index] = F_value



    private void Start()
    {
        aiObject = Resources.Load<GameObject>("Player");
        e_spawn = aiObject.transform.position;
        background = GameObject.Find("Background").transform;
        gen_text = GameObject.Find("Main Camera").transform.Find("Generation").Find("Gen").gameObject;

        FitnessTracker.Initialize(gen_text.transform.parent.gameObject, per_generation, 5);

        StartCoroutine(Generate());

    }

    private void Update()
    {
        //FitnessTracker.DisplayTop10(fitness, entities);
    }

    private Vector2 GetNetworkPosition(int index)
    {
        // Define the layout table
        (int rows, int cols)[] layoutTable = new (int, int)[]
        {
        (0, 0),    // 0 networks (optional)
        (1, 1),    // 1 network
        (1, 2),    // 2 networks
        (2, 2),    // 3 networks
        (2, 2),    // 4 networks
        (2, 3),    // 5 networks
        (2, 3),    // 6 networks
        (3, 3),    // 7 networks
        (3, 3),    // 8 networks
        (3, 3)     // 9 networks
        };

        // Determine grid layout
        (int rows, int cols) grid = (1, 1); // Default
        if (per_generation < layoutTable.Length)
        {
            grid = layoutTable[per_generation];
        }
        else
        {
            // Fallback for large counts
            grid.rows = Mathf.CeilToInt(Mathf.Sqrt(per_generation));
            grid.cols = Mathf.CeilToInt((float)per_generation / grid.rows);
        }

        // Background size
        float length = background.localScale.x;
        float width = background.localScale.y;

        // Step size
        float xStep = length / (grid.cols + 1);
        float yStep = width / (grid.rows + 1);

        Debug.Log(xStep);
        

        // Calculate row and column for the index
        int col = index % grid.cols;
        int row = index / grid.cols;

        Debug.Log(col+" "+row);


        // Calculate x and y
        float x = background.position.x; // + xStep * (col + 0.5f);
        float y = background.position.y + width / 2f - yStep * (row + 0.5f);

        return new Vector2(x, y);
    }

    private IEnumerator Generate()
    {

        neuralNetworks = new NeuralNetwork[n_generations * per_generation]; // 9 neural networks per generation
        fitness = new Dictionary<int, float>[n_generations];
        entities = new GameObject[n_generations * per_generation];

        for (int generation = 0; generation < n_generations; generation++)
        {

            gen_text.GetComponent<TextMeshProUGUI>().text = "Generation " + (generation + 1);

            fitness[generation] = new Dictionary<int, float>();
            
            //previous gen stuff
            NeuralNetwork[] top_previous = new NeuralNetwork[survivors];
            int c = 0;
            
            if (generation > 0)
            {
                var previous_gen = fitness[generation - 1];

                for (int n = 0; n < per_generation; n++)
                {

                    int fitness_index = n;
                    if (previous_gen.ContainsKey(fitness_index) && previous_gen[fitness_index] != RemovedFitness)
                    {

                        fitness[generation].Add(fitness_index, previous_gen[fitness_index]);

                        int generation_index = generation * per_generation + fitness_index;
                        int prev_index = (generation - 1) * per_generation + fitness_index;

                        neuralNetworks[generation_index] = neuralNetworks[prev_index];

                        entities[generation_index] = entities[prev_index];
                        entities[generation_index].GetComponent<PlayerController>().Entity.Reset();

                        top_previous[c++] = neuralNetworks[generation_index];
                    }
                }
            }



            for (int n = 0; n < per_generation; n++)
            {
                int fitness_index = n;
                int generation_index = generation * per_generation + fitness_index;


                NeuralNetwork NN;
                PlayerController component;

                if (!(fitness[generation].ContainsKey(fitness_index) && fitness[generation][fitness_index] != 10)) {

                    fitness[generation].Add(fitness_index, 0);

                    int[] layer_structure = layer_structures[Random.Range(0, layer_structures.Length)];



                    void Display()
                    {

                        NN.NetRender();
                        //NN.Resize(0.9f - 0.15f * layer_structure.Length);
                        NN.Resize((1.3f / 3) * math.sqrt(background.localScale.x / 20));


                        Vector2 pos = GetNetworkPosition(n);
                        

                        NN.Move(pos.x, pos.y);

                    }

                    if (generation > 0)
                    {
                        int chosen = Random.Range(0, survivors);
                        if (chosen >= survivors)
                        {
                            NN = new NeuralNetwork(layer_structure, generation, generation_index);
                        }
                        else
                        {
                          
                            NN = top_previous[chosen].Mutate(generation, generation_index);
                        }
                        NN.NetRender(clear: true, reset: true);
                    }
                    else
                    {
                        NN = new NeuralNetwork(layer_structure, generation, generation_index);
                    }

                    GameObject ai = Instantiate(aiObject, e_spawn, Quaternion.identity, ai_background);
                    ai.name = "AI " + generation_index;

                    component = ai.GetComponent<PlayerController>();


                    component.Entity = new Entity(NN, ai);

                    entities[generation_index] = ai;

                    Display();
                }
                else
                {
                    NN = neuralNetworks[generation_index];
                    component = entities[generation_index].GetComponent<PlayerController>();

                    fitness[generation][fitness_index] = 0;
                }

                neuralNetworks[generation_index] = NN;


                entities[generation_index].transform.position = e_spawn;
                entities[generation_index].transform.rotation = Quaternion.identity;

                yield return new WaitForSeconds(0.02f);
            }

            Debug.Log("bruh.");

            yield return new WaitForSeconds(eval_time);

            Debug.Log("Error. Show yourself!");

            for (int n = 0; n < per_generation; n++)
            {
                int fitness_index = n;

                float f_value = Random.Range(1, 10); //component.Entity.f_values[component.Entity.f_values.Count-1];
                fitness[generation][fitness_index] = f_value;

            }


            Debug.Log("Original Fitness: " + string.Join(", ",
                fitness[generation].Select(kvp => $"{kvp.Key}: {kvp.Value}")
            ));


            for (int i = 0; i < per_generation-survivors; i++)
            {

                int index = fitness[generation]
                    .Where(kvp => kvp.Value != 10)
                    .Aggregate((l, r) => l.Value < r.Value ? l : r).Key; // Compares to find the lowest kvp value
                int generation_index = generation * per_generation + index;

                Debug.Log($"Trying to remove key {index}, type: {index.GetType()}");

                //fitness[generation][index] = 10;

                Debug.Log("Fitness: " + string.Join(", ",
                fitness[generation].Select(kvp => $"{kvp.Key}: {kvp.Value}")
            ));
                

                var e = entities[generation_index].GetComponent<PlayerController>().Entity;
                if (e != null) {
                    e.Destroy();
                }

                //neuralNetworks[generation_index].NetRender(destroy:true);
                
            }

            Debug.Log("yikes... loop completed... ts mad scary gng.");

        }

        yield return 1;

    }

}
