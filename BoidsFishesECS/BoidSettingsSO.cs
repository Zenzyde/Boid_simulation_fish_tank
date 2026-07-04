using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BoidSettingsSO : ScriptableObject
{
	public EBoidsBehaviour behavior;
	public float minBehaviourWeight;
	public float maxBehaviourWeight;
	public float minBehaviourRadius;
	public float maxBehaviourRadius;
}