using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    // Start is called before the first frame update

    //public CharacterController stickObject;
    public CharacterController scalpelObject;
    public GameObject scalpel;
    public Transform scalpelTransform;
    //private Vector3 startStickPos, startScalpelPos;
    // public Button stickButton, scalpelButton;
    // public GameObject restartButton, infoPanel, exitButton;
    // public GameObject green;

    // public MeshHandle MeshHandle;

    void Start()
    {
        // stickObject = GameObject.FindGameObjectWithTag("Stick").GetComponent<CharacterController>();
        // scalpelObject = GameObject.FindGameObjectWithTag("Scalpel").GetComponent<CharacterController>();
        // stick = GameObject.FindGameObjectWithTag("StickButton");
        // scalpel = GameObject.FindGameObjectWithTag("ScalpelButton");
        // restartButton = GameObject.FindGameObjectWithTag("RestartButton");
        // infoPanel = GameObject.FindGameObjectWithTag("InfoPanel");
        // exitButton = GameObject.FindGameObjectWithTag("ExitButton");
        // green = GameObject.FindGameObjectWithTag("green");

        // chooseToolPanel = GameObject.FindGameObjectWithTag("ChooseToolPanel");

        // stickButton = stick.GetComponent<Button>();
        // scalpelButton = scalpel.GetComponent<Button>();

        // restartButton.GetComponent<Button>().onClick.AddListener(OnStart);
        // exitButton.GetComponent<Button>().onClick.AddListener(ExitApp);

        // startScalpelPos = scalpelTransform.position;
        // startStickPos = stickTransform.position;

        OnStart();

        
    }


    // void Update()
    // {
    // green.SetActive(MeshHandle.green_icon);
    // if (MeshHandle.green_icon)
    // {
    //     green.enabled = true;
    // }
    // else
    // {
    //     green.enabled = false;
    // }

    // if (Input.GetKey(KeyCode.Alpha1))
    // {
    //     SetStickActive();
    // }
    // else if (Input.GetKey(KeyCode.Alpha2))
    // {
    //     SetScalpelActive();
    // }
    // if (Input.GetKey(KeyCode.Alpha2))
    // {
    //     SetScalpelActive();
    // }
    // }


    // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetKey(KeyCode.Alpha1))
    //     {
    //         SetStickActive();
    //     }
    //     else if (Input.GetKey(KeyCode.Alpha2))
    //     {
    //         SetScalpelActive();
    //     }
    // }

    // void SetStickActive()
    // {
    //     stickObject.enabled = true;
    //     scalpelObject.enabled = false;
    //     chooseToolPanel.SetActive(false);
    // }

    // void SetScalpelActive()
    // {
    //     //stickObject.enabled = false;
    //     scalpelObject.enabled = true;
    //     // chooseToolPanel.SetActive(false);
    // }

    void OnStart()
    {
        // green.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);
        scalpelTransform.position = new Vector3(0, 13, 0);
        //chooseToolPanel.SetActive(true);
        // stickTransform.position = stickTransform.position;
        // scalpelTransform.position = scalpelTransform.position;
        // stick.GetComponent<Image>().color = stickButton.colors.normalColor;
        // scalpel.GetComponent<Image>().color = scalpelButton.colors.normalColor;
        //stickObject.enabled = false;
        scalpelObject.enabled = true;
        // when the button is clicked it runs the enclosed function
        // stickButton.onClick.AddListener(SetStickActive);
        // scalpelButton.onClick.AddListener(SetScalpelActive);
    }

     

    // void BeforeStart()
    // {
    //     stick.GetComponent<Image>().color = stickButton.colors.normalColor;
    //     scalpel.GetComponent<Image>().color = scalpelButton.colors.normalColor;
    //     stickObject.enabled = false;
    //     scalpelObject.enabled = false;
    // }

    
}
