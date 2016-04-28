using UnityEngine;
using System.Collections;

public class Topics : StateMachineBehaviour {

	private Animator homeAnimator;
	private Animator words;
	private Animator letters;
	private Animator topicAnimator;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		MenuReferences r = animator.gameObject.GetComponent<MenuReferences> ();

		homeAnimator = r.homeAnimator;
		words = r.words;
		letters = r.letters;
		topicAnimator = r.topicAnimator;

		words.SetBool ("Show", true);
		letters.SetBool ("Show", true);

		words.SetBool ("Table", false);
		letters.SetBool ("Table", false);

		homeAnimator.SetBool ("Show", true);
		topicAnimator.SetBool ("Show", false);

	}

	//OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		homeAnimator.SetBool ("Show", true);
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {



		if (animator.GetInteger ("Menu") < 1) {
			words.SetBool ("Show", false);
			letters.SetBool ("Show", false);
			words.SetBool ("Table", false);
			letters.SetBool ("Table", false);
		} else {
			homeAnimator.SetBool ("Show", false);
			topicAnimator.SetBool ("Show", true);

			words.SetBool ("Table", true);
			letters.SetBool ("Table", true);

		}

	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
