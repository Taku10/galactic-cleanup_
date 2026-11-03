using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DemoManager : MonoBehaviour {
	public TextMesh text_fx_name;
	public GameObject[] fx_prefabs;
	public int index_fx = 0;
	private Ray ray;
	private RaycastHit2D ray_cast_hit;

	void Start () 
	{
		text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
	}

	void Update () 
	{
		var mouse = Mouse.current;
		var keyboard = Keyboard.current;

		if (mouse != null && mouse.leftButton.wasPressedThisFrame) 
		{
			ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
			ray_cast_hit = Physics2D.Raycast(new Vector2(ray.origin.x, ray.origin.y), new Vector2(0,0));
			if (ray_cast_hit) 
			{
				switch(ray_cast_hit.transform.name){
				case "Wallpaper":
					Instantiate(fx_prefabs[ index_fx ], new Vector3(ray.origin.x, ray.origin.y, 0), Quaternion.identity);
					break;
				case "UI-arrow-right":
					ray_cast_hit.transform.SendMessage("Go");
					index_fx++;
					if(index_fx >= fx_prefabs.Length)
						index_fx = 0;
					text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
					break;
				case "UI-arrow-left":
					ray_cast_hit.transform.SendMessage("Go");
					index_fx--;
					if(index_fx <= -1)
						index_fx = fx_prefabs.Length - 1;
					text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
					break;
				case "Instructions":
					Destroy(ray_cast_hit.transform.gameObject);
					break;
				}
			}
		}
		//Change-FX keyboard..	
		if (keyboard != null && (keyboard.zKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)){
			GameObject.Find("UI-arrow-left").SendMessage("Go");
			index_fx--;
			if(index_fx <= -1)
				index_fx = fx_prefabs.Length - 1;
			text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;	
		}

		if (keyboard != null && (keyboard.xKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)){
			GameObject.Find("UI-arrow-right").SendMessage("Go");
			index_fx++;
			if(index_fx >= fx_prefabs.Length)
				index_fx = 0;
			text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
		}

		if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame){
			Instantiate(fx_prefabs[ index_fx ], new Vector3(0, 0, 0), Quaternion.identity);	
		}
	}

}
