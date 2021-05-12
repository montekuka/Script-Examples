using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModelSupply : MonoBehaviour {
	public GameObject[] carModels;
	public GameObject[] pedestrianPrefabs;
	public Color[] skinColors;
	public Color[] eyeColors;

	public static ModelSupply Instance { get; private set; }

	private void Awake() {
		if (Instance != null && Instance != this) { Destroy(this.gameObject); } else { Instance = this; }
	}

	public GameObject GetRandomPedestrian(Vector3 spawnPosition, Quaternion spawnRotation, Transform poolTransform) {
		var randomModel = Instantiate(pedestrianPrefabs[Random.Range(0, 2)], spawnPosition, spawnRotation, poolTransform);
		var charCustom  = randomModel.transform.GetChild(0).GetComponent<CharacterCustomization>();
		RandomizePedestrian(ref charCustom);
		return randomModel;
	}

	private void RandomizePedestrian(ref CharacterCustomization charCustom) {
		charCustom.SetHairByIndex(Random.Range(0,                                                  charCustom.hairPresets.Count));
		charCustom.SetBeardByIndex(Random.Range(0,                                                 charCustom.beardPresets.Count));
		charCustom.SetElementByIndex(CharacterCustomization.ClothesPartType.Shirt, Random.Range(0, charCustom.shirtsPresets.Count));
		var pantsRange = Random.Range(0,                                                           charCustom.pantsPresets.Count);

		charCustom.SetElementByIndex(CharacterCustomization.ClothesPartType.Pants,     pantsRange);
		charCustom.SetElementByIndex(CharacterCustomization.ClothesPartType.Shoes,     Random.Range(0, charCustom.shoesPresets.Count));
		charCustom.SetElementByIndex(CharacterCustomization.ClothesPartType.Hat,       Random.Range(0, charCustom.hatsPresets.Count));
		charCustom.SetElementByIndex(CharacterCustomization.ClothesPartType.Accessory, Random.Range(0, charCustom.accessoryPresets.Count));
		charCustom.SetBodyColor(CharacterCustomization.BodyColorPart.Skin, skinColors[Random.Range(0, skinColors.Length)]);
		charCustom.SetBodyColor(CharacterCustomization.BodyColorPart.Eye,  eyeColors[Random.Range(0,  eyeColors.Length)]);
		charCustom.SetBodyColor(CharacterCustomization.BodyColorPart.Hair, Color.black);
		charCustom.SetHeadSize(Random.Range(-0.25f, 0.25f));
		charCustom.SetHeadOffset(Random.Range(0f,   1f));
		charCustom.SetHeight(Random.Range(-0.1f,    0.1f));
		//charCustom.SetBodyShape(CharacterCustomization.BodyShapeType.Fat,        Random.Range(0f, 1f));
		//charCustom.SetBodyShape(CharacterCustomization.BodyShapeType.Muscles,    Random.Range(0f, 1f));
		//charCustom.SetBodyShape(CharacterCustomization.BodyShapeType.Thin,       Random.Range(0f, 1f));
		//charCustom.SetBodyShape(CharacterCustomization.BodyShapeType.BreastSize, Random.Range(0f, 1f), new string[] {"Chest", "Stomach", "Head"}, new CharacterCustomization.ClothesPartType[] {CharacterCustomization.ClothesPartType.Shirt});
		//c/harCustom.RecalculateBodyShapes();
		//charCustom.RecalculateLOD();
	}
}