using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraToggle : MonoBehaviour {
	public GameObject cam1;
	public GameObject cam2;
	public GameObject cam3;
    public GameObject showControlCanvas;
    public Material Game;
	public Material Leaderboard;
	public Material ShowControl;
	public Material WinnerScreen;
	private MeshRenderer renderTex;

    PlayerControls playerControls;

	private void Awake() {
		playerControls = new PlayerControls();
	}

	void Start() {
		playerControls.Enable();
	}

	// Update is called once per frame
	void Update() {

        bool isKey1Pressed = playerControls.Player.SwitchCam1.ReadValue<float>() > 0.1f;
        bool isKey2Pressed = playerControls.Player.SwitchCam2.ReadValue<float>() > 0.1f;
        bool isKey3Pressed = playerControls.Player.SwitchCam3.ReadValue<float>() > 0.1f;
        bool isKey4Pressed = playerControls.Player.SwitchCam4.ReadValue<float>() > 0.1f;
        bool isKey5Pressed = playerControls.Player.SwitchCam5.ReadValue<float>() > 0.1f;
        bool isKey6Pressed = playerControls.Player.SwitchCam6.ReadValue<float>() > 0.1f;
        bool isKey7Pressed = playerControls.Player.SwitchCam7.ReadValue<float>() > 0.1f;
        bool isKey8Pressed = playerControls.Player.SwitchCam8.ReadValue<float>() > 0.1f;
        bool isKey9Pressed = playerControls.Player.SwitchCam9.ReadValue<float>() > 0.1f;
        bool isKey10Pressed = playerControls.Player.SwitchCam10.ReadValue<float>() > 0.1f;
        bool isKey11Pressed = playerControls.Player.SwitchCam11.ReadValue<float>() > 0.1f;
        bool isKey12Pressed = playerControls.Player.SwitchCam12.ReadValue<float>() > 0.1f;

        if (Input.GetKey(KeyCode.LeftShift))
        {
        
            if (isKey1Pressed) {
			    Material[] newMaterials = new Material[]{Game};
			    renderTex = cam1.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

		    if (isKey2Pressed) {
			    Material[] newMaterials = new Material[] { Leaderboard };
			    renderTex = cam1.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);

            }

		    if (isKey3Pressed) {
			    Material[] newMaterials = new Material[] { ShowControl };
			    renderTex = cam1.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(true);
                Canvas canvasVar;
                canvasVar = showControlCanvas.GetComponent<Canvas>();
                canvasVar.targetDisplay = 0;
            }

		    if (isKey4Pressed) {
			    Material[] newMaterials = new Material[] { WinnerScreen };
			    renderTex = cam1.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

		    if (isKey5Pressed) {
			    Material[] newMaterials = new Material[] { Game };
			    renderTex = cam2.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

		    if (isKey6Pressed) {
			    Material[] newMaterials = new Material[] { Leaderboard };
			    renderTex = cam2.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

		    if (isKey7Pressed) {
			    Material[] newMaterials = new Material[] { ShowControl };
			    renderTex = cam2.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(true);
                Canvas canvasVar;
                canvasVar = showControlCanvas.GetComponent<Canvas>();
                canvasVar.targetDisplay = 1;
            }

		    if (isKey8Pressed) {
			    Material[] newMaterials = new Material[] { WinnerScreen };
			    renderTex = cam2.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

		    if (isKey9Pressed) {
			    Material[] newMaterials = new Material[] { Game };
			    renderTex = cam3.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

		    if (isKey10Pressed) {
			    Material[] newMaterials = new Material[] { Leaderboard };
			    renderTex = cam3.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

		    if (isKey11Pressed) {
			    Material[] newMaterials = new Material[] { ShowControl };
			    renderTex = cam3.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(true);
                Canvas canvasVar;
                canvasVar = showControlCanvas.GetComponent<Canvas>();
                canvasVar.targetDisplay = 2;
            }

		    if (isKey12Pressed) {
			    Material[] newMaterials = new Material[] { WinnerScreen };
			    renderTex = cam3.GetComponent<MeshRenderer>();
			    renderTex.materials = newMaterials;
                showControlCanvas.SetActive(false);
            }

	    }

    }
}
