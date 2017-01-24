using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class TestScript : MonoBehaviour {

	[DllImport ("libc.dylib")]
	private static extern int getpid ();

	void Start () {
		Debug.Log ("Started");
	}

	public void OnMyClick() {
		Debug.Log ("Onclick " + getpid() + " here");
	}
}
