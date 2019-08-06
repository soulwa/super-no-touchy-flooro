using UnityEngine;

public class Cursor : MonoBehaviour
{
    public Transform[] buttons;
    private int buttonIndex;

    private bool holdingButton;

    private void Start()
    {
        buttonIndex = 0;
    }

    public void Update()
    {
        if (Input.GetAxisRaw("Vertical") != 0 && !holdingButton)
        {
            holdingButton = true;

            switch ((int)Input.GetAxisRaw("Vertical"))
            {
                case 1:
                    if (buttonIndex == 0)
                    {
                        break;
                    }
                    else
                    {
                        transform.position = new Vector3(transform.position.x, buttons[buttonIndex - 1].position.y);
                        buttonIndex--;
                        break;
                    }
                case -1:
                    if ((buttonIndex + 1) >= buttons.Length)
                    {
                        break;
                    }
                    else
                    {
                        transform.position = new Vector3(transform.position.x, buttons[buttonIndex + 1].position.y);
                        buttonIndex++;
                        break;
                    }
                default:
                    break;
            }
            
        }

        else if (Input.GetAxisRaw("Vertical") == 0 && holdingButton)
        {
            holdingButton = false;
        }
    }
}
