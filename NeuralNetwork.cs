using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using System;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using TMPro;
using UnityEditor.Overlays;
using Unity.VisualScripting.FullSerializer;
using static UnityEngine.Rendering.DebugUI;
using UnityEditor.Rendering;

class Layer
{
    public List<List<float>> weightsArray;
    public List<float> biasesArray;
    public List<float> nodesArray;

    public int n_nodes;
    public int n_inputs;

    public Layer(int n_inputs, int n_nodes)
    {
        Recalculate(n_inputs, n_nodes);
    }

    public void Recalculate(int n_inputs, int n_nodes)
    {
        this.n_nodes = n_nodes;
        this.n_inputs = n_inputs;

        weightsArray = new List<List<float>>();
        biasesArray = new List<float>();
        nodesArray = new List<float>();

        for (int i = 0; i < n_nodes; i++)
        {
            nodesArray.Add(0);
            biasesArray.Add(0);

            setUpWeights(i);
        }
    }

    public void setUpWeights(int node)
    {
        weightsArray.Add(new List<float>());

        for (int j = 0; j < n_inputs; j++)
        {
            

            float val = Random.Range(NeuralNetwork.w_min, NeuralNetwork.w_max);
            weightsArray[node].Add(val);
        }
    }

    public void forwardPass(float[] inputsArray)
    {

        nodesArray = new List<float>();

        for (int i = 0; i < n_nodes; i++)
        {
            nodesArray.Add(0);

            // sum of weights * inputs
            for (int j = 0; j < n_inputs; j++)
            {
                nodesArray[i] += weightsArray[i][j] * inputsArray[j];
            }

            // add the bias
            nodesArray[i] += biasesArray[i];
        }
    }

    public void Activation(string Func = "Tanh")
    {
        if (Func == "ReLU")
        {
            for (int i = 0; i < n_nodes; i++)
            {
                if (nodesArray[i] < 0)
                {
                    nodesArray[i] = 0;
                }
            }
        }
        else if (Func == "Tanh")
        {
            for (int i = 0; i < n_nodes; i++)
            {
                nodesArray[i] = math.tanh(nodesArray[i]);
            }
        }
        else if (Func == "Sigmoid")
        {
            for (int i = 0; i < n_nodes; i++)
            {
                nodesArray[i] = 1.0f / (1.0f + (float)math.exp(-nodesArray[i]));
            }
        }
        else if (Func == "Sin")
        {
            for (int i = 0; i < n_nodes; i++)
            {
                nodesArray[i] = math.sin(nodesArray[i]);
            }
        }
    }

}


public class NeuralNetwork
{

    private GameObject input_Node = Resources.Load<GameObject>("Input Node");
    private GameObject output_Node = Resources.Load<GameObject>("Output Node");
    private GameObject hidden_Node = Resources.Load<GameObject>("Hidden Node");
    private GameObject connection = Resources.Load<GameObject>("Connection");
    private GameObject background = GameObject.Find("Background");



    public int id;
    private Layer[] layers;
    private Transform[][] layout;
    private int[] layer_structure;
    private Transform container;
    private bool rendered;

    public static float w_min = 0f;
    public static float w_max = 0.5f;

    public NeuralNetwork(int[] layer_structure, int generation, int generation_id)
    {
        this.id = generation_id;
        this.layer_structure = layer_structure;
        this.rendered = false;

        container = new GameObject("Generation " + (generation + 1) + " NN " + id.ToString()).transform;
        container.SetParent(GameObject.Find("NN_Container").transform);
        

        layers = new Layer[layer_structure.Length - 1]; // Maxmimum number of layers (Not including inputs)
        for (int i = 1; i < layer_structure.Length; i++)
        {
            layers[i - 1] = new Layer(layer_structure[i - 1], layer_structure[i]);
        }
    }


    private float round2(float i)
    {
        return (math.floor(i * 100) / 100);
    }

    private void SetText(Transform go, string text)
    {
        GameObject label = go.Find("Canvas").Find("Text (TMP)").gameObject;
        label.GetComponent<TextMeshProUGUI>().text = text;
    }

    private Color GetConnectionColor(float weight)
    {
        float normalizedWeight = Mathf.InverseLerp(-w_max, w_max, weight);

        Color newColor = Color.Lerp(Color.red, Color.green, normalizedWeight);

        return newColor;
    }

    public void NetRender(bool clear = false, bool destroy = false, bool reset = false)
    {
        if (destroy)
        {
            Debug.Log("Don't pmo, " + id);
            Debug.Log("... why art error?");
            GameObject.Destroy(container.gameObject);
            rendered = false;
        }
        if (clear)
        {
            for (int i = 0; i < container.childCount; i++)
            {
                GameObject.Destroy(container.GetChild(i).gameObject);
            }
            rendered = false;
        }
        if (reset)
        {
            this.container.localScale = Vector3.one;
            this.container.position = Vector3.zero;
        }

        if (destroy || clear || reset) return;

        rendered = true;

        int inputLength = layer_structure[0];


        layout = new Transform[layers.Length+1][];
        layout[0] = new Transform[inputLength];
        
        float left_x = (background.transform.position.x - background.transform.localScale.x / 2);
        float x_offset = background.transform.localScale.x / (layers.Length + 2);

        float top_y = (background.transform.position.y + background.transform.localScale.y / 2);
            

        for (int j = 0; j < inputLength; j++)
        {
            
            layout[0][j] = GameObject.Instantiate(input_Node, new Vector3(left_x+x_offset,top_y - background.transform.localScale.y*(j+1) / (inputLength + 1), 0), Quaternion.identity, container).transform;
        }

        for (int i = 0; i < layers.Length; i++)
        {
            List<float> nodes = layers[i].nodesArray;

 
            float y_offset = background.transform.localScale.y / (nodes.Count + 1);

            layout[i + 1] = new Transform[nodes.Count];

            for (int j = 0; j < nodes.Count; j++) 
            {

                if (i == layers.Length-1)
                {
                    layout[i+1][j] = GameObject.Instantiate(output_Node, new Vector3(left_x + x_offset * (i + 2), top_y - y_offset * (j + 1), 0), Quaternion.identity, container).transform;
                } else
                {
                    layout[i+1][j] = GameObject.Instantiate(hidden_Node, new Vector3(left_x + x_offset * (i + 2), top_y - y_offset * (j + 1), 0), Quaternion.identity, container).transform;
                }

                SetText(layout[i+1][j], round2(nodes[j]).ToString());
            }
        }

        // For each layer (including the input layer)
        for (int i = 0; i < layout.Length-1; i++)
        {
            // For each node in layer i
            for (int j = 0; j < layout[i].Length; j++)
            {
                // For each node in the next layer
                for (int k=0; k < layout[i+1].Length; k++)
                {
                    Transform node1 = layout[i][j];
                    Transform node2 = layout[i + 1][k];

                    float midpoint_x = (node1.position.x + node2.position.x) / 2;
                    float midpoint_y = (node1.position.y + node2.position.y) / 2;

                    float y = node2.position.y - node1.position.y;
                    float x = node2.position.x - node1.position.x;

                    Transform con = GameObject.Instantiate(connection,
                        new Vector3(midpoint_x, midpoint_y, 0),
                        Quaternion.Euler(0, 0, MathF.Atan2(y,x)*180/MathF.PI),
                        node1
                    ).transform;

                    con.localScale = new Vector3(Vector3.Distance(node1.position, node2.position), con.localScale.y, 1);

                    SpriteRenderer spriteRenderer = con.GetComponent<SpriteRenderer>();

                    float weight = layers[i].weightsArray[k][j];

                    spriteRenderer.color = GetConnectionColor(weight);
                    SetText(con, round2(weight).ToString());
                    
                }
            }
        }
    }


    // Brain: Takes inputs, returns outputs
    public float[] Brain(float[] inputs)
    {
   

        for (int i=0; i<inputs.Length; i++)
        {
            if (!rendered) break;
            SetText(layout[0][i], round2(inputs[i]).ToString());
        }

        for (int i = 0; i < layers.Length; i++)
        {
          
            if (layers[i] == null) continue;
           
            if (i == 0)
            {
                layers[i].forwardPass(inputs);
            }
            else
            {
                
                layers[i].forwardPass(layers[i - 1].nodesArray.ToArray());
                
            }


            if (i != layers.Length - 1)
            {
                layers[i].Activation();
            }

            for (int j = 0; j < layers[i].nodesArray.Count; j++)
            {
                if (!rendered) break;
                SetText(layout[i + 1][j], round2(layers[i].nodesArray[j]).ToString());
            }
        }

        return (layers[layers.Length - 1].nodesArray.ToArray());
    }

    public void Move(float right, float up)
    {
        this.container.position = new Vector3(right, up, 0);
    }

    public void Resize(float factor)
    {
        this.container.localScale *= factor;
    }

    public NeuralNetwork Mutate(int generation, int generation_id)
    {
        NeuralNetwork copy = new NeuralNetwork(this.layer_structure, generation, generation_id);

        copy.container = GameObject.Instantiate(
            this.container, 
            GameObject.Find("Background").transform
        ).transform;

        copy.container.name = "Generation " + (generation + 1) + " NN " + copy.id.ToString();

        for (int layer_num = 0; layer_num < this.layer_structure.Length; layer_num++)
        {
            Layer og_layer = this.layers[layer_num];
            Layer copy_layer = copy.layers[layer_num];

            copy_layer.biasesArray = new List<float>(og_layer.biasesArray);
            copy_layer.weightsArray = og_layer.weightsArray
            .Select(innerList => new List<float>(innerList))
            .ToList();

           for (int i = 0; i < copy_layer.n_nodes;)
           {
                for (int j = 0; j < copy_layer.n_inputs;)
                {
                   if (Random.Range(0,100) < 3)
                    {
                        copy_layer.weightsArray[i][j] = Random.Range(w_min, w_max);
                    }
                }
           }
        }

        return copy;
    }

    
}
