using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BoidEntityTypeSO : ScriptableObject
{
	public List<BoidSettingsSO> behaviors;
	public float minSpeed;
	public float maxSpeed;
	public GameObject prefab;
	public string speciesName;
}