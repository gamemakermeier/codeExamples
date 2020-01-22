using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
public class ActivateOnVisible : MonoBehaviour
{
	public enum TargetingMode
	{
		gameObject,
		meshRenderer,
		script
	}
	public TargetingMode targetingMode;
	public List<MonoBehaviour> targetScripts;
	public List<MeshRenderer> meshRenderers;
	public bool deactivateOnInvisible;
	public enum RepeatMode
	{
		repeat,
		cooldown,
		noRepeat
	}
	public RepeatMode repeatMode;

	public float cooldown;
	private float cdTimer;

	private void OnBecameInvisible()
	{
		if (deactivateOnInvisible)
		{
			ToggleState(false);
		}
	}
	private void OnBecameVisible()
	{
		ToggleState(true);
	}

	private void ToggleState(bool targetState)
	{
		if (repeatMode == RepeatMode.cooldown && Time.time - cdTimer < cooldown)
		{
			return;
		}

		if (targetingMode == TargetingMode.gameObject)
		{
			this.gameObject.SetActive(targetState);
		}

		if (targetingMode == TargetingMode.meshRenderer)
		{
			foreach (MeshRenderer meshRenderer in meshRenderers)
			{
				meshRenderer.enabled = targetState;
			}
		}

		if (targetingMode == TargetingMode.script)
		{
			foreach (MonoBehaviour script in targetScripts)
			{
				script.enabled = targetState;
			}
		}

		cdTimer = Time.time;

		if (repeatMode == RepeatMode.noRepeat)
		{
			this.enabled = targetState;
		}
	}
}
