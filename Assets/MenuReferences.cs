using UnityEngine;
using System.Collections;

public class MenuReferences : MonoBehaviour {

	public Animator homeAnimator;
	public Animator startButton;
	public Animator words;
	public Animator letters;
	public Animator topicAnimator;
	public Animator levelAnimator;
	public Animator playAnimator;
	public Animator cameraAnimator;

	[Range(-1,4)]
	public int Menu;

	public void setMenu(int menu){
		this.Menu = menu;
	}

	private Animator anim;

	void Start(){
		anim = GetComponent<Animator> ();
	}

	void Update(){
		anim.SetInteger ("Menu", Menu);

	}

}
