using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Random = UnityEngine.Random;
using Transform = UnityEngine.Transform;


public class Entity
{

    private GameObject target = Resources.Load<GameObject>("Target");
    private Transform background = GameObject.Find("AI_bg").transform;

    private NeuralNetwork network;
    private int collected;
    private float closest_dist;
    private float current_dist;
    private float time_not_moving;
    private float ov_time;
    private Vector3[] target_positions;
    public List<float> f_values;


    public GameObject current_target;
    public bool destroyed;


    private float forward;
    private float rotate;

    public Entity(NeuralNetwork NN, GameObject body)
    {
        this.destroyed = false;
        this.network = NN;
        this.collected = 0;
        this.closest_dist = 10000;
        this.time_not_moving = 0;
        this.ov_time = 0;
        this.target_positions = new Vector3[] {new Vector3(1, 1, 0), new Vector3(6, 3, 0), new Vector3(-5, -2)};
        this.f_values = new List<float>();

        GameObject label = body.transform.Find("Square").Find("Canvas").Find("Text (TMP)").gameObject;
        label.GetComponent<TextMeshProUGUI>().text = (network.id).ToString();

        randomize_target();

    }

    public float Fitness()
    {
        if (destroyed) return 0;
        float val = 40 * collected + 40 / (current_dist + 1);
        f_values.Add(val);
        return val;
    }

    public void Reset()
    {
        if (destroyed) return;
        collected = 0;
        closest_dist = 10000;
        time_not_moving = 0;
    
        GameObject.Destroy(current_target);
       
        randomize_target();
    }

    public void Destroy()
    {
        if (destroyed) return;

        destroyed = true;

        GameObject.Destroy(current_target);

        current_target = null;
        network = null;
    }

    public void Update(GameObject Body)
    {
 

        if (destroyed) return;

        if (network == null) return;
        if (current_target == null) {
            collected++;
            if (collected >= target_positions.Length) return;
            randomize_target();
        };


        
        float[] inputs = CollectData(Body.transform);

        float[] output = network.Brain(inputs);
       

        forward = output[0];
        rotate = output[1];

        if (forward < 0)
        {
            time_not_moving += Time.deltaTime;
        }
        ov_time += Time.deltaTime;

        Fitness();

    }

    private float[] CollectData(Transform Body)
    {

        float new_dist = (Body.position - current_target.transform.position).magnitude;
        current_dist = new_dist;
        if (new_dist < closest_dist)
        {
            closest_dist = new_dist;
        }

        float x = current_target.transform.position.x;
        float y = current_target.transform.position.y;

        float my_x = Body.position.x;
        float my_y = Body.position.y;

        Vector3 dir3D = -Body.transform.up.normalized;  // Use up instead of right
        Vector3 directionToTarget3D = (current_target.transform.position - Body.position).normalized;

        // Project both onto XY plane
        Vector2 dir = new Vector2(dir3D.x, dir3D.y);
        Vector2 directionToTarget = new Vector2(directionToTarget3D.x, directionToTarget3D.y);

        float dot = Vector2.Dot(dir, directionToTarget);

        float angle = Mathf.Acos(dot) * 180f/Mathf.PI;

        float[] data = { dot };

        return data;
    }

    private void randomize_target()
    {
        if (destroyed) return;

        current_target = GameObject.Instantiate(target, target_positions[collected], Quaternion.identity);
        current_target.transform.SetParent(background, true);
        
    }

    public float GetVertical()
    {
        if (destroyed) return 0;
        return forward;
    }

    public float GetHorizontal()
    {
        if (destroyed) return 0;
        return rotate;
    }
}