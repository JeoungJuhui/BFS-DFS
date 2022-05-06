using UnityEngine;
using System.Collections;

using System.Collections.Generic;


public class PlayerScript : MonoBehaviour {

    static public GridManager gm = null;
    Coroutine move_coroutine = null;
    Coroutine replay_coroutine = null;

    Dictionary<KeyCode, Command> actions;
    static public List<Command> OldCommands = new List<Command>();
    static public List<GameObject> LastCube = new List<GameObject>();
    static public List<Ray> Rays = new List<Ray>();
    static public List<Vector3> lastPos = new List<Vector3>();


    static public Vector3 startPos;


    public abstract class Command
    {
        public abstract void Execute(GameObject actor, Ray ray);

        public virtual void Undo(GameObject actor) { }

        public virtual void Do(GameObject actor, Ray ray) { }
        public void Push()
        {
            PlayerScript.OldCommands.Add(this);
        }
    }

    public class MakeBox : Command
    {
        public override void Execute(GameObject actor, Ray ray)
        {
            Do(actor, ray);
            Push();
            

        }

        public override void Do(GameObject actor, Ray ray)
        {
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
                    PlayerScript.LastCube.Add(wall);
                    PlayerScript.Rays.Add(ray);
                    gm.SetAsWall(wall.transform.position);

                }
            }
        }

        public override void Undo(GameObject actor)
        {

            var ray = Rays[Rays.Count - 1];

            RaycastHit hit;


            if (Physics.Raycast(ray, out hit))
            {
                print(hit.transform.tag);
                if (hit.transform.tag == "Wall")
                {
                    Destroy(hit.transform.gameObject);
                    gm.SetAsPlain(hit.transform.position);

                    Rays.RemoveAt(Rays.Count - 1);
                }
            }


        }

    }

    public class PlayerMove:Command
    {
        public override void Execute(GameObject actor, Ray ray)
        {
            Do(actor, ray);
            Push();
        }
        public override void Do(GameObject actor, Ray ray) 
        {
            Debug.Log("MoveTo");
        }


        public override void Undo(GameObject actor) 
        {
            var ray = Rays[Rays.Count - 1];
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                print(hit.transform.tag);
                if (hit.transform.tag == "Plane")
                {
                    PlayerScript.Rays.Add(ray);
                    Push();

                }

            }

            else if (hit.transform.tag == "Wall")
            {
                Destroy(hit.transform.gameObject);
                gm.SetAsPlain(hit.transform.position);

                Rays.RemoveAt(Rays.Count - 1);
            }


        }

    }
    

    public class UndoCommand : Command
    {
        public override void Execute(GameObject actor, Ray ray)
        {
            if (OldCommands.Count == 0) return;
            var command = OldCommands[OldCommands.Count - 1];
            command.Undo(actor);
            OldCommands.RemoveAt(OldCommands.Count - 1);
        }
    }

    public class ReplayCommand : Command
    {
        public override void Execute(GameObject actor, Ray ray)
        {
            Vector3 startPos1 = PlayerScript.startPos;
            actor.transform.position = startPos1;

            RaycastHit hit;

            foreach (var rays in Rays)
            {
                if (Physics.Raycast(rays, out hit))
                {
                    if (hit.transform.tag == "Wall")
                    {
                        Destroy(hit.transform.gameObject);
                        gm.SetAsPlain(hit.transform.position);
                    }
                }
            }


            PlayerScript.IsReplay = true;
            PlayerScript.CurrentCount = 0;
            PlayerScript.RaysCount = 0;


        }


    }

    static public int CurrentCount = 0;
    static public int RaysCount = 0;

    static public bool IsReplay = false;

    //코루틴으로 작성 필요.
    
    IEnumerator Replay()
    {
        Debug.Log(CurrentCount);

        if (CurrentCount == OldCommands.Count)
        {

            IsReplay = false;
            yield return null;
        }


        var command = OldCommands[CurrentCount];
        var ray = Rays[RaysCount];

 
        Debug.Log("Replay" + command);
        command.Do(gameObject, ray);
        CurrentCount++;
        RaysCount++;
    }

    void MoveCheck(Ray ray, RaycastHit hit)
    {

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag == "Wall" && hit.normal == Vector3.up)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.GetComponent<MeshRenderer>().material.color = Color.yellow;
                wall.tag = "Wall";
                wall.transform.position = hit.transform.position + Vector3.up;
                return;
            }
            if (move_coroutine != null)
            {
                lastPos.Add(gameObject.transform.position);
                StopCoroutine(move_coroutine);
            }
            move_coroutine = StartCoroutine(gm.Move(gameObject, hit.point));
        }
    }

    // Use this for initialization
    void Start ()
    {
        gm = Camera.main.GetComponent<GridManager>() as GridManager;
        gm.BuildWorld(50, 50);

        actions = new Dictionary<KeyCode, Command>()
        {
            {KeyCode.Mouse1, new MakeBox() },
            {KeyCode.Mouse0, new PlayerMove() },
            {KeyCode.U, new UndoCommand() },
            {KeyCode.R, new ReplayCommand() }
        };

        PlayerScript.startPos = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update ()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        foreach (var action in actions)
        {
            if (Input.GetKeyDown(action.Key)&& Physics.Raycast(ray, out hit))
            {
                if (action.Key == KeyCode.Mouse0)
                {
                    MoveCheck(ray, hit);
                }

                action.Value.Execute(gameObject, ray);
            }
        }
        if (replay_coroutine!=null) StopCoroutine(replay_coroutine);
        if(IsReplay) replay_coroutine = StartCoroutine(Replay());
    
    }    
}
