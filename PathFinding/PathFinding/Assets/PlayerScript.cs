using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

    GridManager gm = null;
    Coroutine move_coroutine = null;
        
	// Use this for initialization
	void Start ()
    {
        gm = Camera.main.GetComponent<GridManager>() as GridManager;
        gm.BuildWorld(50, 50);
    }

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Wall" && hit.normal == Vector3.up) {
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.GetComponent<MeshRenderer>().material.color = Color.yellow;
                    wall.tag = "Wall";
                    wall.transform.position = hit.transform.position + Vector3.up;
                    return;
                }
                if (move_coroutine != null) StopCoroutine(move_coroutine);
                move_coroutine = StartCoroutine(gm.Move(gameObject, hit.point));                
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                print(hit.transform.tag);
                if (hit.transform.tag == "Plane")
                {
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.GetComponent<MeshRenderer>().material.color = Color.yellow;
                    wall.tag = "Wall";
                    wall.transform.position = gm.pos2center(hit.point);
                    gm.SetAsWall(wall.transform.position);
                }
            }
        }
    }    
}
