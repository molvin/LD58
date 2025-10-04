using System.Collections;
using UnityEngine;

public class Shoebox : MonoBehaviour
{
    // TODO: do lerping in animation instead, if needed

    public enum State
    {
        None,
        Closed,
        Opening,
        Open,
        Closing
    }
    public State CurrentState;

    public float OpenTime, CloseTime;
    public Transform Root, OpenPoint, ClosedPoint;

    public BoxCollider OpenCollider;

    private float startMoveTime;

    private void Update()
    {
        if(CurrentState == State.Closed)
        {
            ClosedState();
        }
        else if (CurrentState == State.Opening)
        {
            OpeningState();
        }
        else if (CurrentState == State.Open)
        {
            OpenState();
        }
        else if (CurrentState == State.Closing)
        {
            ClosingState();
        }
    }

    private bool HoveringBox()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return OpenCollider.Raycast(ray, out RaycastHit _, 1000.0f);
    }

    private void ClosedState()
    {
        if(HoveringBox() && Input.GetMouseButtonDown(0))
        {
            CurrentState = State.Opening;
            startMoveTime = Time.time;
        }
    }

    private void OpeningState()
    {
        bool done = LerpBox(OpenTime, ClosedPoint.position, OpenPoint.position);

        if(done)
        {
            CurrentState = State.Open;
        }
    }

    private void OpenState()
    {
        if (HoveringBox() && Input.GetMouseButtonDown(1))
        {
            CurrentState = State.Closing;
            startMoveTime = Time.time;
        }
    }

    private void ClosingState()
    {
        bool done = LerpBox(CloseTime, OpenPoint.position, ClosedPoint.position);

        if (done)
        {
            CurrentState = State.Closed;
        }
    }

    private bool LerpBox(float duration, Vector3 start, Vector3 end)
    {
        float t = Time.time - startMoveTime;
        Vector3 pos = Vector3.Lerp(start, end, t / duration);
        Root.transform.position = pos;

        if (t > duration)
        {
            Root.transform.position = end;
            return true;
        }
        return false;
    }
}
