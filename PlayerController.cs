using JetBrains.Annotations;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{


    public float move_speed;
    public float rotate_speed;

    private byte color_mult = 4;
    private Transform arrow;
    private Transform triangle;
    private Vector3 original_size;
    private Vector3 original_position;
    public Entity Entity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
       move_speed = 4f;
       rotate_speed = 300f;

       arrow = transform.GetChild(0);
       triangle = arrow.GetChild(0);
       original_size = arrow.localScale;
       original_position = arrow.localPosition;

       BoxCollider2D collider = GetComponent<BoxCollider2D>();
       collider.isTrigger = true;

    }


    // Update is called once per frame
    void FixedUpdate()
    {

        
        if (Entity == null) return;
        if (Entity.destroyed)
        {
            Destroy(gameObject);
            return;
        }

        Entity.Update(gameObject);

        float forward = 1; // Entity.GetVertical();
        forward = Mathf.Clamp(forward, 0.0f, forward);
        float rotation = -Entity.GetHorizontal();

        
        transform.Translate(0, forward * move_speed * Time.fixedDeltaTime, 0);
        transform.Rotate(0, 0, rotation * rotate_speed * Time.fixedDeltaTime);

        SpriteRenderer arrow_sprite = arrow.GetComponent<SpriteRenderer>();
        SpriteRenderer tri_sprite = triangle.GetComponent<SpriteRenderer>();


        if (forward >= 0.5 )
        {

            arrow.localScale = Vector3.Lerp(arrow.localScale, new Vector3(0.15f, 1.15f, 0f), Time.fixedDeltaTime*color_mult);
            arrow.localPosition = Vector3.Lerp(arrow.localPosition, new Vector3(0, 0.3f, 0), Time.fixedDeltaTime*color_mult);

            if (arrow_sprite.color == Color.green && tri_sprite.color == Color.green) { return; }

            arrow_sprite.color = Color.Lerp(arrow_sprite.color, Color.green, Time.fixedDeltaTime*color_mult);
            tri_sprite.color = Color.Lerp(tri_sprite.color, Color.green, Time.fixedDeltaTime*color_mult);

        } else
        {

            arrow.localScale = Vector3.Lerp(arrow.localScale, original_size, Time.fixedDeltaTime * color_mult);
            arrow.localPosition = Vector3.Lerp(arrow.localPosition, original_position, Time.fixedDeltaTime*color_mult);

            if (arrow_sprite.color == Color.red && tri_sprite.color == Color.red) { return; }

            arrow_sprite.color = Color.Lerp(arrow_sprite.color, Color.red, Time.fixedDeltaTime*color_mult);
            tri_sprite.color = Color.Lerp(tri_sprite.color, Color.red, Time.fixedDeltaTime*color_mult);
        }





    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Entity == null) return;


        if (collision.gameObject == Entity.current_target)
        {
            print("collided!");
            Destroy(collision.gameObject);
            Entity.current_target = null;
        }
    }
}
